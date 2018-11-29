using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using NeoSharp.VM.Interop.Types.Collections;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Types
{
    public unsafe class ExecutionEngine : ExecutionEngineBase
    {
        #region Private fields

        // This delegates are required for native calls, 
        // otherwise is disposed and produce a memory error

        private readonly NeoVM.OnStepIntoCallback _internalOnStepInto;

        private readonly NeoVM.InvokeInteropCallback _internalInvokeInterop;
        private readonly NeoVM.LoadScriptCallback _internalLoadScript;
        private readonly NeoVM.GetMessageCallback _internalGetMessage;

        /// <summary>
        /// Native handle
        /// </summary>
        private IntPtr _handle;

        /// <summary>
        /// Last message
        /// </summary>
        private byte[] _lastMessage;

        /// <summary>
        /// Result stack
        /// </summary>
        private readonly Stack _resultStack;

        /// <summary>
        /// Invocation stack
        /// </summary>
        private readonly StackBase<ExecutionContextBase> _invocationStack;

        /// <summary>
        /// Interop Cache
        /// </summary>
        private readonly List<InteropCacheEntry> _interopCache;
        private readonly List<object> _interopCacheIndex;

        #endregion

        #region Public fields

        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle => _handle;

        /// <summary>
        /// Is Disposed
        /// </summary>
        public override bool IsDisposed => _handle == IntPtr.Zero;

        /// <summary>
        /// Invocation Stack
        /// </summary>
        public override StackBase<ExecutionContextBase> InvocationStack => _invocationStack;

        /// <summary>
        /// Result Stack
        /// </summary>
        public override Stack ResultStack => _resultStack;

        /// <summary>
        /// Virtual Machine State
        /// </summary>
        public override EVMState State => (EVMState)NeoVM.ExecutionEngine_GetState(_handle);

        /// <summary>
        /// Consumed Gas
        /// </summary>
        public override ulong ConsumedGas => NeoVM.ExecutionEngine_GetConsumedGas(_handle);

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Arguments</param>
        public ExecutionEngine(ExecutionEngineArgs e) : base(e)
        {
            _interopCache = new List<InteropCacheEntry>();
            _interopCacheIndex = new List<object>();

            _internalInvokeInterop = new NeoVM.InvokeInteropCallback(InternalInvokeInterop);
            _internalLoadScript = new NeoVM.LoadScriptCallback(InternalLoadScript);
            _internalGetMessage = new NeoVM.GetMessageCallback(InternalGetMessage);

            _handle = NeoVM.ExecutionEngine_Create
                (
                _internalInvokeInterop, _internalLoadScript, _internalGetMessage,
                out IntPtr invHandle, out IntPtr resHandle
                );

            if (_handle == IntPtr.Zero) throw new ExternalException();

            _invocationStack = new ExecutionContextStack(this, invHandle);
            _resultStack = new StackItemStack(this, resHandle);

            if (Logger != null)
            {
                if (Logger.Verbosity.HasFlag(ELogVerbosity.StepInto))
                {
                    _internalOnStepInto = new NeoVM.OnStepIntoCallback(InternalOnStepInto);
                    NeoVM.ExecutionEngine_AddLog(_handle, _internalOnStepInto);
                }
            }
        }

        /// <summary>
        /// Get interop object
        /// </summary>
        /// <param name="objKey">Object key</param>
        /// <returns>Object</returns>
        internal InteropCacheEntry GetInteropObject(int objKey)
        {
            return _interopCache[objKey];
        }

        /// <summary>
        /// Internal callback for OnStepInto
        /// </summary>
        /// <param name="it">Context</param>
        void InternalOnStepInto(IntPtr it)
        {
            Logger.RaiseOnStepInto(new ExecutionContext(this, it));
        }

        /// <summary>
        /// Get message callback
        /// </summary>
        /// <param name="iteration">Iteration</param>
        /// <param name="output">Message</param>
        int InternalGetMessage(uint iteration, out IntPtr output)
        {
            if (MessageProvider != null)
            {
                // TODO: should change this, too dangerous

                var script = MessageProvider.GetMessage(iteration);

                if (script != null && script.Length > 0)
                {
                    // Prevent dispose

                    _lastMessage = script;

                    fixed (byte* p = _lastMessage)
                    {
                        output = (IntPtr)p;
                    }

                    return _lastMessage.Length;
                }
            }

            output = IntPtr.Zero;
            return 0;
        }

        /// <summary>
        /// Load script callback
        /// </summary>
        /// <param name="scriptHash">Hash</param>
        /// <param name="isDynamicInvoke">Is dynamic invoke</param>
        /// <param name="rvcount">RV count</param>
        /// <returns>Return 0x01 if is corrected loaded</returns>
        byte InternalLoadScript(byte[] scriptHash, byte isDynamicInvoke, int rvcount)
        {
            if (ScriptTable == null)
            {
                return NeoVM.FALSE;
            }

            var script = ScriptTable.GetScript(scriptHash, isDynamicInvoke == NeoVM.TRUE);

            if (script == null || script.Length <= 0)
            {
                return NeoVM.FALSE;
            }

            fixed (byte* p = script)
            {
                NeoVM.ExecutionEngine_LoadScript(_handle, (IntPtr)p, script.Length, rvcount);
            }

            return NeoVM.TRUE;
        }

        /// <summary>
        /// Invoke Interop callback
        /// </summary>
        /// <param name="ptr">Method</param>
        /// <param name="size">Size</param>
        /// <returns>Return Interop result</returns>
        byte InternalInvokeInterop(IntPtr ptr, byte size)
        {
            if (InteropService == null)
            {
                return NeoVM.FALSE;
            }

            var method = new byte[size];
            Marshal.Copy(ptr, method, 0, size);

            try
            {
                if (InteropService.Invoke(method, this))
                {
                    return NeoVM.TRUE;
                }
            }
            catch
            {

            }

            return NeoVM.FALSE;
        }

        #region Load Script

        /// <summary>
        /// Load script
        /// </summary>
        /// <param name="script">Script</param>
        /// <returns>Script index in script cache</returns>
        public override int LoadScript(byte[] script)
        {
            fixed (byte* p = script)
            {
                return NeoVM.ExecutionEngine_LoadScript(_handle, (IntPtr)p, script.Length, -1);
            }
        }

        /// <summary>
        /// Load script
        /// </summary>
        /// <param name="scriptIndex">Script Index</param>
        /// <returns>True if is loaded</returns>
        public override bool LoadScript(int scriptIndex)
        {
            return NeoVM.ExecutionEngine_LoadCachedScript(_handle, scriptIndex, -1) == NeoVM.TRUE;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Increase gas
        /// </summary>
        /// <param name="gas">Gas</param>
        public override bool IncreaseGas(ulong gas)
        {
            return NeoVM.ExecutionEngine_IncreaseGas(_handle, gas) == 0x01;
        }

        /// <summary>
        /// Clean Execution engine state
        /// </summary>
        /// <param name="iteration">Iteration</param>
        public override void Clean(uint iteration = 0)
        {
            NeoVM.ExecutionEngine_Clean(_handle, iteration);
        }

        /// <summary>
        /// Execute until
        /// </summary>
        /// <param name="gas">Gas</param>
        public override bool Execute(ulong gas = uint.MaxValue)
        {
            // HALT=TRUE

            return NeoVM.ExecutionEngine_Execute(_handle, gas) == NeoVM.TRUE;
        }

        /// <summary>
        /// Step Into
        /// </summary>
        /// <param name="steps">Steps</param>
        public override void StepInto(int steps = 1)
        {
            for (var x = 0; x < steps; x++)
            {
                NeoVM.ExecutionEngine_StepInto(_handle);
            }
        }

        /// <summary>
        /// Step Out
        /// </summary>
        public override void StepOut()
        {
            NeoVM.ExecutionEngine_StepOut(_handle);
        }

        /// <summary>
        /// Step Over
        /// </summary>
        public override void StepOver()
        {
            NeoVM.ExecutionEngine_StepOver(_handle);
        }

        #endregion

        #region Create items

        /// <summary>
        /// Create Map StackItem
        /// </summary>
        public override MapStackItemBase CreateMap()
        {
            return new MapStackItem(this);
        }

        /// <summary>
        /// Create Array StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public override ArrayStackItemBase CreateArray(IEnumerable<StackItemBase> items = null)
        {
            return new ArrayStackItem(this, items, false);
        }

        /// <summary>
        /// Create Struct StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public override ArrayStackItemBase CreateStruct(IEnumerable<StackItemBase> items = null)
        {
            return new ArrayStackItem(this, items, true);
        }

        /// <summary>
        /// Create ByteArrayStackItem
        /// </summary>
        /// <param name="data">Buffer</param>
        public override ByteArrayStackItemBase CreateByteArray(byte[] data)
        {
            return new ByteArrayStackItem(this, data);
        }

        /// <summary>
        /// Create InteropStackItem
        /// </summary>
        /// <param name="obj">Object</param>
        public override InteropStackItemBase<T> CreateInterop<T>(T obj)
        {
            var objKey = _interopCacheIndex.IndexOf(obj);

            if (objKey == -1)
            {
                objKey = _interopCache.Count;

                _interopCache.Add(new InteropCacheEntry()
                {
                    Value = obj,
                    Instanciator = (engine, ptr, key, value) =>
                        new InteropStackItem<T>(engine, ptr, key, (T)value)
                });

                _interopCacheIndex.Add(obj);
            }

            return new InteropStackItem<T>(this, obj, objKey);
        }

        /// <summary>
        /// Create BooleanStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override BooleanStackItemBase CreateBool(bool value)
        {
            return new BooleanStackItem(this, value);
        }

        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IntegerStackItemBase CreateInteger(int value)
        {
            return new IntegerStackItem(this, value);
        }

        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IntegerStackItemBase CreateInteger(long value)
        {
            return new IntegerStackItem(this, value);
        }

        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IntegerStackItemBase CreateInteger(BigInteger value)
        {
            return new IntegerStackItem(this, value);
        }

        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IntegerStackItemBase CreateInteger(byte[] value)
        {
            return new IntegerStackItem(this, value);
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero) return;

            if (disposing)
            {
                // Clear interop cache

                foreach (var v in _interopCacheIndex)
                {
                    if (v is IDisposable dsp)
                    {
                        dsp.Dispose();
                    }
                }

                _interopCache.Clear();
                _interopCacheIndex.Clear();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.

            NeoVM.ExecutionEngine_Free(ref _handle);
        }

        #endregion
    }
}