using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types.Collections;
using System;

namespace NeoVM.Interop.Types
{
    unsafe public class ExecutionContext : IDisposable
    {
        #region Delegates

        readonly NeoVM.OnStackChangeCallback _InternalOnAltStackChange;
        readonly NeoVM.OnStackChangeCallback _InternalOnEvaluationStackChange;

        #endregion

        /// <summary>
        /// Native handle
        /// </summary>
        IntPtr Handle;
        /// <summary>
        /// ScriptHashLength
        /// </summary>
        public const int ScriptHashLength = 20;
        /// <summary>
        /// Is Disposed
        /// </summary>
        public bool IsDisposed => Handle == IntPtr.Zero;
        /// <summary>
        /// Evaluation Stack
        /// </summary>
        public readonly StackItemStack EvaluationStack;
        /// <summary>
        /// Alt Stack
        /// </summary>
        public readonly StackItemStack AltStack;
        /// <summary>
        /// Engine
        /// </summary>
        public readonly ExecutionEngine Engine;


        byte[] _ScriptHash;

        /// <summary>
        /// Next instruction
        /// </summary>
        public EVMOpCode NextInstruction => (EVMOpCode)NeoVM.ExecutionContext_GetNextInstruction(Handle);
        /// <summary>
        /// Get Instruction pointer
        /// </summary>
        public int InstructionPointer => NeoVM.ExecutionContext_GetInstructionPointer(Handle);
        /// <summary>
        /// Script
        /// </summary>
        public byte[] ScriptHash
        {
            get
            {
                if (_ScriptHash != null)
                    return _ScriptHash;

                _ScriptHash = new byte[ScriptHashLength];

                fixed (byte* p = _ScriptHash)
                {
                    if (NeoVM.ExecutionContext_GetScriptHash(Handle, (IntPtr)p, 0) != ScriptHashLength)
                        throw (new AccessViolationException());
                }

                return _ScriptHash;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal ExecutionContext(ExecutionEngine engine, IntPtr handle)
        {
            Handle = handle;
            NeoVM.ExecutionContext_Claim(Handle, out IntPtr evHandle, out IntPtr altHandle);

            Engine = engine;
            AltStack = new StackItemStack(Engine, altHandle);
            EvaluationStack = new StackItemStack(Engine, evHandle);

            if (engine.Logger == null) return;

            if (engine.Logger.Verbosity.HasFlag(ELogVerbosity.AltStackChanges))
            {
                _InternalOnAltStackChange = new NeoVM.OnStackChangeCallback(InternalOnAltStackChange);
                NeoVM.StackItems_AddLog(altHandle, _InternalOnAltStackChange);
            }

            if (engine.Logger.Verbosity.HasFlag(ELogVerbosity.EvaluationStackChanges))
            {
                _InternalOnEvaluationStackChange = new NeoVM.OnStackChangeCallback(InternalOnEvaluationStackChange);
                NeoVM.StackItems_AddLog(evHandle, _InternalOnEvaluationStackChange);
            }
        }

        /// <summary>
        /// Internal callback for OnAltStackChange
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="operation">Operation</param>
        void InternalOnAltStackChange(IntPtr item, int index, byte operation)
        {
            using (IStackItem it = Engine.ConvertFromNative(item))
                Engine.Logger.RaiseOnAltStackChange(AltStack, it, index, (ELogStackOperation)operation);
        }
        /// <summary>
        /// Internal callback for OnEvaluationStackChange
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="index">Index</param>
        /// <param name="operation">Operation</param>
        void InternalOnEvaluationStackChange(IntPtr item, int index, byte operation)
        {
            using (IStackItem it = Engine.ConvertFromNative(item))
                Engine.Logger.RaiseOnEvaluationStackChange(EvaluationStack, it, index, (ELogStackOperation)operation);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero) return;

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.
            AltStack.Dispose();
            EvaluationStack.Dispose();
            NeoVM.ExecutionContext_Free(ref Handle);
        }

        ~ExecutionContext()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return "[" + BitHelper.ToHexString(ScriptHash) + "-" + InstructionPointer.ToString("x6") + "] " + NextInstruction.ToString();
        }
    }
}