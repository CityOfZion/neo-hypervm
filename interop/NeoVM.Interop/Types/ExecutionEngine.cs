﻿using NeoSharp.VM;
using NeoVM.Interop.Extensions;
using NeoVM.Interop.Types.Collections;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Types
{
    public unsafe class ExecutionEngine : IExecutionEngine
    {
        #region Delegates

        readonly NeoVM.OnStepIntoCallback _InternalOnStepInto;
        readonly NeoVM.OnStackChangeCallback _InternalOnExecutionContextChange;
        readonly NeoVM.OnStackChangeCallback _InternalOnResultStackChange;

        readonly NeoVM.InvokeInteropCallback _InternalInvokeInterop;
        readonly NeoVM.LoadScriptCallback _InternalLoadScript;
        readonly NeoVM.GetMessageCallback _InternalGetMessage;

        #endregion

        /// <summary>
        /// Native handle
        /// </summary>
        IntPtr Handle;
        /// <summary>
        /// Last message
        /// </summary>
        byte[] LastMessage;

        readonly IStackItemsStack _ResultStack;
        readonly IStack<IExecutionContext> _InvocationStack;

        /// <summary>
        /// Is Disposed
        /// </summary>
        public override bool IsDisposed => Handle == IntPtr.Zero;

        /// <summary>
        /// Invocation Stack
        /// </summary>
        public override IStack<IExecutionContext> InvocationStack => _InvocationStack;
        /// <summary>
        /// Result Stack
        /// </summary>
        public override IStackItemsStack ResultStack => _ResultStack;
        /// <summary>
        /// Virtual Machine State
        /// </summary>
        public override EVMState State => (EVMState)NeoVM.ExecutionEngine_GetState(Handle);
        /// <summary>
        /// Interop Cache
        /// </summary>
        internal readonly List<object> InteropCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="e">Arguments</param>
        public ExecutionEngine(ExecutionEngineArgs e) : base(e)
        {
            _InternalInvokeInterop = new NeoVM.InvokeInteropCallback(InternalInvokeInterop);
            _InternalLoadScript = new NeoVM.LoadScriptCallback(InternalLoadScript);
            _InternalGetMessage = new NeoVM.GetMessageCallback(InternalGetMessage);
            InteropCache = new List<object>();

            Handle = NeoVM.ExecutionEngine_Create
                (
                _InternalInvokeInterop, _InternalLoadScript, _InternalGetMessage,
                out IntPtr invHandle, out IntPtr resHandle
                );

            if (Handle == IntPtr.Zero)
                throw (new ExternalException());

            _InvocationStack = new ExecutionContextStack(this, invHandle);
            _ResultStack = new StackItemStack(this, resHandle);

            if (Logger != null)
            {
                if (Logger.Verbosity.HasFlag(ELogVerbosity.StepInto))
                {
                    _InternalOnStepInto = new NeoVM.OnStepIntoCallback(InternalOnStepInto);
                    NeoVM.ExecutionEngine_AddLog(Handle, _InternalOnStepInto);
                }

                if (Logger.Verbosity.HasFlag(ELogVerbosity.ExecutionContextStackChanges))
                {
                    _InternalOnExecutionContextChange = new NeoVM.OnStackChangeCallback(InternalOnExecutionContextChange);
                    NeoVM.ExecutionContextStack_AddLog(invHandle, _InternalOnExecutionContextChange);
                }

                if (Logger.Verbosity.HasFlag(ELogVerbosity.ResultStackChanges))
                {
                    _InternalOnResultStackChange = new NeoVM.OnStackChangeCallback(InternalOnResultStackChange);
                    NeoVM.StackItems_AddLog(resHandle, _InternalOnResultStackChange);
                }
            }
        }

        /// <summary>
        /// Internal callback for OnStepInto
        /// </summary>
        /// <param name="it">Context</param>
        void InternalOnStepInto(IntPtr it)
        {
            using (var context = new ExecutionContext(this, it))
                Logger.RaiseOnStepInto(context);
        }
        /// <summary>
        /// Internal callback for OnExecutionContextChange
        /// </summary>
        /// <param name="it">Item</param>
        /// <param name="index">Index</param>
        /// <param name="operation">Operation</param>
        void InternalOnExecutionContextChange(IntPtr it, int index, byte operation)
        {
            using (var context = new ExecutionContext(this, it))
                Logger.RaiseOnExecutionContextChange(InvocationStack, context, index, (ELogStackOperation)operation);
        }
        /// <summary>
        /// Internal callback for OnResultStackChange
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="operation">Operation</param>
        void InternalOnResultStackChange(IntPtr item, int index, byte operation)
        {
            using (var it = this.ConvertFromNative(item))
                Logger.RaiseOnResultStackChange(ResultStack, it, index, (ELogStackOperation)operation);
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

                byte[] script = MessageProvider.GetMessage(iteration);

                if (script != null && script.Length > 0)
                {
                    // Prevent dispose

                    LastMessage = script;

                    fixed (byte* p = LastMessage)
                    {
                        output = (IntPtr)p;
                    }

                    return LastMessage.Length;
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

            byte[] script = ScriptTable.GetScript(scriptHash, isDynamicInvoke == NeoVM.TRUE);

            if (script == null || script.Length <= 0)
            {
                return NeoVM.FALSE;
            }

            fixed (byte* p = script)
            {
                NeoVM.ExecutionEngine_LoadScript(Handle, (IntPtr)p, script.Length, rvcount);
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
                return NeoVM.FALSE;

            string method = Marshal.PtrToStringUTF8(ptr, size);

            try
            {
                if (InteropService.Invoke(method, this))
                    return NeoVM.TRUE;
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
                return NeoVM.ExecutionEngine_LoadScript(Handle, (IntPtr)p, script.Length, -1);
            }
        }

        /// <summary>
        /// Load script
        /// </summary>
        /// <param name="scriptIndex">Script Index</param>
        /// <returns>True if is loaded</returns>
        public override bool LoadScript(int scriptIndex)
        {
            return NeoVM.ExecutionEngine_LoadCachedScript(Handle, scriptIndex, -1) == NeoVM.TRUE;
        }

        #endregion

        #region Execution

        /// <summary>
        /// Clean Execution engine state
        /// </summary>
        /// <param name="iteration">Iteration</param>
        public override void Clean(uint iteration = 0)
        {
            NeoVM.ExecutionEngine_Clean(Handle, iteration);
        }
        /// <summary>
        /// Execute
        /// </summary>
        public override bool Execute()
        {
            return NeoVM.ExecutionEngine_Execute(Handle) == NeoVM.TRUE;
        }
        /// <summary>
        /// Step Into
        /// </summary>
        /// <param name="steps">Steps</param>
        public override void StepInto(int steps = 1)
        {
            for (int x = 0; x < steps; x++)
                NeoVM.ExecutionEngine_StepInto(Handle);
        }
        /// <summary>
        /// Step Out
        /// </summary>
        public override void StepOut()
        {
            NeoVM.ExecutionEngine_StepOut(Handle);
        }
        /// <summary>
        /// Step Over
        /// </summary>
        public override void StepOver()
        {
            NeoVM.ExecutionEngine_StepOver(Handle);
        }

        #endregion

        #region Create items

        /// <summary>
        /// Create Map StackItem
        /// </summary>
        public override IMapStackItem CreateMap()
        {
            return new MapStackItem(this);
        }
        /// <summary>
        /// Create Array StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public override IArrayStackItem CreateArray(IEnumerable<IStackItem> items = null)
        {
            return new ArrayStackItem(this, items, false);
        }
        /// <summary>
        /// Create Struct StackItem
        /// </summary>
        /// <param name="items">Items</param>
        public override IArrayStackItem CreateStruct(IEnumerable<IStackItem> items = null)
        {
            return new ArrayStackItem(this, items, true);
        }
        /// <summary>
        /// Create ByteArrayStackItem
        /// </summary>
        /// <param name="data">Buffer</param>
        public override IByteArrayStackItem CreateByteArray(byte[] data)
        {
            return new ByteArrayStackItem(this, data);
        }
        /// <summary>
        /// Create InteropStackItem
        /// </summary>
        /// <param name="obj">Object</param>
        public override IInteropStackItem CreateInterop(object obj)
        {
            return new InteropStackItem(this, obj);
        }
        /// <summary>
        /// Create BooleanStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IBooleanStackItem CreateBool(bool value)
        {
            return new BooleanStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IIntegerStackItem CreateInteger(int value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IIntegerStackItem CreateInteger(long value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IIntegerStackItem CreateInteger(BigInteger value)
        {
            return new IntegerStackItem(this, value);
        }
        /// <summary>
        /// Create IntegerStackItem
        /// </summary>
        /// <param name="value">Value</param>
        public override IIntegerStackItem CreateInteger(byte[] value)
        {
            return new IntegerStackItem(this, value);
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero) return;

            if (disposing)
            {
                // Clear interop cache

                foreach (var v in InteropCache)
                    if (v is IDisposable dsp)
                        dsp.Dispose();

                InteropCache.Clear();
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.
            ResultStack.Dispose();
            NeoVM.ExecutionEngine_Free(ref Handle);
        }

        #endregion
    }
}