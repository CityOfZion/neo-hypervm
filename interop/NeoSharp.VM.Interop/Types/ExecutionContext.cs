using System;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Types.Collections;

namespace NeoSharp.VM.Interop.Types
{
    unsafe public class ExecutionContext : IExecutionContext
    {
        #region Private fields

        // This delegates are required for native calls, 
        // otherwise is disposed and produce a memory error

        private byte[] _scriptHash;

        private readonly IStackItemsStack _altStack;
        private readonly IStackItemsStack _evaluationStack;

        /// <summary>
        /// Native handle
        /// </summary>
        private IntPtr _handle;

        #endregion

        #region Public fields

        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _handle; }
        }

        /// <summary>
        /// Is Disposed
        /// </summary>
        public override bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Next instruction
        /// </summary>
        public override EVMOpCode NextInstruction
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (EVMOpCode)NeoVM.ExecutionContext_GetNextInstruction(_handle); }
        }

        /// <summary>
        /// Get Instruction pointer
        /// </summary>
        public override int InstructionPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return NeoVM.ExecutionContext_GetInstructionPointer(_handle); }
        }

        /// <summary>
        /// Script Hash
        /// </summary>
        public override byte[] ScriptHash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_scriptHash != null)
                {
                    return _scriptHash;
                }

                _scriptHash = new byte[ScriptHashLength];

                fixed (byte* p = _scriptHash)
                {
                    if (NeoVM.ExecutionContext_GetScriptHash(_handle, (IntPtr)p, 0) != ScriptHashLength)
                    {
                        throw new AccessViolationException();
                    }
                }

                return _scriptHash;
            }
        }

        /// <summary>
        /// AltStack
        /// </summary>
        public override IStackItemsStack AltStack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _altStack; }
        }

        /// <summary>
        /// EvaluationStack
        /// </summary>
        public override IStackItemsStack EvaluationStack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _evaluationStack; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal ExecutionContext(ExecutionEngine engine, IntPtr handle)
        {
            _handle = handle;
            NeoVM.ExecutionContext_Claim(_handle, out IntPtr evHandle, out IntPtr altHandle);

            _altStack = new StackItemStack(engine, altHandle);
            _evaluationStack = new StackItemStack(engine, evHandle);
        }

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero) return;

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.

            _altStack.Dispose();
            _evaluationStack.Dispose();

            NeoVM.ExecutionContext_Free(ref _handle);
        }

        #endregion
    }
}
