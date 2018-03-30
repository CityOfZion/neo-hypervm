using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types.Collections;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Types
{
    public unsafe class ExecutionEngine : IDisposable
    {
        /// <summary>
        /// Native handle
        /// </summary>
        IntPtr Handle;
        /// <summary>
        /// LastScript loaded
        /// </summary>
        byte[] LastScript;
        /// <summary>
        /// Interop service
        /// </summary>
        public readonly InteropService InteropService;
        /// <summary>
        /// Script table
        /// </summary>
        public readonly IScriptTable ScriptTable;

        /// <summary>
        /// Script containes
        /// </summary>
        public readonly IScriptContainer ScriptContainer;
        /// <summary>
        /// Invocation Stack
        /// </summary>
        public readonly ExecutionContextStack InvocationStack;
        /// <summary>
        /// Evaluation Stack
        /// </summary>
        public readonly StackItemStack EvaluationStack;
        /// <summary>
        /// Alt Stack
        /// </summary>
        public readonly StackItemStack AltStack;
        /// <summary>
        /// Virtual Machine State
        /// </summary>
        public EVMState State => NeoVM.ExecutionEngine_GetState(Handle);

        #region Shortcuts

        public ExecutionContext CurrentContext => InvocationStack.TryPeek(0, out ExecutionContext i) ? i : null;
        public ExecutionContext CallingContext => InvocationStack.TryPeek(1, out ExecutionContext i) ? i : null;
        public ExecutionContext EntryContext => InvocationStack.TryPeek(InvocationStack.Count - 1, out ExecutionContext i) ? i : null;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Arguments</param>
        internal ExecutionEngine(ExecutionEngineArgs e)
        {
            Handle = NeoVM.ExecutionEngine_Create
                (
                new NeoVM.InvokeInteropCallback(InternalInvokeInterop),
                new NeoVM.GetScriptCallback(InternalGetScript),
                new NeoVM.GetMessageCallback(InternalGetMessage),
                out IntPtr invHandle, out IntPtr evHandle, out IntPtr altHandle
                );

            if (Handle == IntPtr.Zero)
                throw (new ExternalException());

            if (e != null)
            {
                InteropService = e.InteropService;
                ScriptTable = e.ScriptTable;
                ScriptContainer = e.ScriptContainer;
            }

            InvocationStack = new ExecutionContextStack(invHandle);
            EvaluationStack = new StackItemStack(this, evHandle);
            AltStack = new StackItemStack(this, altHandle);
        }
        /// <summary>
        /// Get message callback
        /// </summary>
        /// <param name="iteration">Iteration</param>
        /// <param name="output">Message</param>
        int InternalGetMessage(uint iteration, out IntPtr output)
        {
            if (ScriptContainer != null)
            {
                LastScript = ScriptContainer.GetMessage(iteration);

                if (LastScript == null)
                {
                    output = IntPtr.Zero;
                    return 0;
                }

                // Prevent dispose

                fixed (byte* p = LastScript)
                {
                    output = (IntPtr)p;
                }

                return LastScript.Length;
            }

            output = IntPtr.Zero;
            return 0;
        }
        /// <summary>
        /// Get script callback
        /// </summary>
        /// <param name="scriptHash">Hash</param>
        /// <param name="output">Script</param>
        /// <returns>Length of script</returns>
        int InternalGetScript(byte[] scriptHash, out IntPtr output)
        {
            if (ScriptTable == null)
            {
                output = IntPtr.Zero;
                return 0;
            }

            LastScript = ScriptTable.GetScript(scriptHash);

            if (LastScript == null)
            {
                output = IntPtr.Zero;
                return 0;
            }

            // Prevent dispose

            fixed (byte* p = LastScript)
            {
                output = (IntPtr)p;
            }

            return LastScript.Length;
        }
        /// <summary>
        /// Invoke Interop callback
        /// </summary>
        /// <param name="method">Method</param>
        /// <returns>Return Interop result</returns>
        byte InternalInvokeInterop(string method)
        {
            if (InteropService != null && InteropService.Invoke(method, this))
                return 0x01;

            return 0x00;
        }

        #region Load Script

        /// <summary>
        /// Load script
        /// </summary>
        /// <param name="script">Script</param>
        public void LoadScript(byte[] script)
        {
            fixed (byte* p = script)
            {
                NeoVM.ExecutionEngine_LoadScript(Handle, (IntPtr)p, script.Length);
            }
        }
        /// <summary>
        /// Load script
        /// </summary>
        /// <param name="script">Script</param>
        public void LoadPushOnlyScript(byte[] script)
        {
            fixed (byte* p = script)
            {
                NeoVM.ExecutionEngine_LoadPushOnlyScript(Handle, (IntPtr)p, script.Length);
            }
        }

        #endregion

        #region Execution

        /// <summary>
        /// Execute
        /// </summary>
        public EVMState Execute()
        {
            return NeoVM.ExecutionEngine_Execute(Handle);
        }
        /// <summary>
        /// Step Into
        /// </summary>
        public void StepInto()
        {
            NeoVM.ExecutionEngine_StepInto(Handle);
        }
        /// <summary>
        /// Step Out
        /// </summary>
        public void StepOut()
        {
            NeoVM.ExecutionEngine_StepOut(Handle);
        }
        /// <summary>
        /// Step Over
        /// </summary>
        public void StepOver()
        {
            NeoVM.ExecutionEngine_StepOver(Handle);
        }

        #endregion

        #region Create items

        /// <summary>
        /// Convert native pointer to stack item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Return StackItem</returns>
        internal IStackItem ConvertFromNative(IntPtr item)
        {
            if (item == IntPtr.Zero) return null;

            EStackItemType state = NeoVM.StackItem_SerializeDetails(item, out int size);
            if (state == EStackItemType.None) return null;

            byte[] payload;

            if (size > 0)
            {
                payload = new byte[size];
                fixed (byte* p = payload)
                {
                    int s = NeoVM.StackItem_SerializeData(item, (IntPtr)p, size);
                    if (s != size)
                    {
                        // TODO: Try to fix this issue with BigInteger
                        Array.Resize(ref payload, s);
                    }
                }
            }
            else
            {
                payload = null;
            }

            switch (state)
            {
                case EStackItemType.Array: return new ArrayStackItem(this, item, false);
                case EStackItemType.Struct: return new ArrayStackItem(this, item, true);
                case EStackItemType.Map: return new MapStackItem(this, item);
                case EStackItemType.Interop:
                    {
                        // Extract object

                        IntPtr ptr = new IntPtr(BitConverter.ToInt64(payload, 0));
                        return new InteropStackItem(this, item, Marshal.GetObjectForIUnknown(ptr), ptr);
                    }
                case EStackItemType.ByteArray: return new ByteArrayStackItem(this, item, payload ?? (new byte[] { }));
                case EStackItemType.Integer: return new IntegerStackItem(this, item, payload ?? (new byte[] { }));
                case EStackItemType.Bool: return new BooleanStackItem(this, item, payload ?? (new byte[] { }));

                default: throw new ExternalException();
            }
        }

        /// <summary>
        /// Create Array StackItem
        /// </summary>
        public ArrayStackItem CreateArray()
        {
            return new ArrayStackItem(this, false);
        }
        /// <summary>
        /// Create Struct StackItem
        /// </summary>
        public ArrayStackItem CreateStruct()
        {
            return new ArrayStackItem(this, true);
        }
        /// <summary>
        /// Create Array StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public ArrayStackItem CreateArray(IEnumerable<IStackItem> items)
        {
            return new ArrayStackItem(this, items, false);
        }
        /// <summary>
        /// Create Struct StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public ArrayStackItem CreateStruct(IEnumerable<IStackItem> items)
        {
            return new ArrayStackItem(this, items, true);
        }
        /// <summary>
        /// Create ByteArrayStackItem
        /// </summary>
        /// <param name="data">Buffer</param>
        public ByteArrayStackItem CreateByteArray(byte[] data)
        {
            return new ByteArrayStackItem(this, data);
        }
        /// <summary>
        /// Create InteropStackItem
        /// </summary>
        /// <param name="obj">Object</param>
        public InteropStackItem CreateInterop(object obj)
        {
            return new InteropStackItem(this, obj);
        }
        /// <summary>
        /// Create BooleanStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public BooleanStackItem CreateBool(bool value)
        {
            return new BooleanStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public IntegerStackItem CreateInteger(int value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public IntegerStackItem CreateInteger(long value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public IntegerStackItem CreateInteger(BigInteger value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public IntegerStackItem CreateInteger(byte[] value)
        {
            return new IntegerStackItem(this, value);
        }

        #endregion

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero) return;

            if (disposing)
            {
                // dispose managed state (managed objects).
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.
            NeoVM.ExecutionEngine_Free(ref Handle);
        }

        ~ExecutionEngine()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}