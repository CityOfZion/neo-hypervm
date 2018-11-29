using System;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Types.Collections;

namespace NeoSharp.VM.Interop.Types
{
    unsafe public class ExecutionContext : ExecutionContextBase
    {
        #region Private fields

        // This delegates are required for native calls, 
        // otherwise is disposed and produce a memory error

        private byte[] _scriptHash;

        private readonly Stack _altStack;
        private readonly Stack _evaluationStack;

        /// <summary>
        /// Native handle
        /// </summary>
        private readonly IntPtr _handle;

        /// <summary>
        /// Engine
        /// </summary>
        private readonly ExecutionEngine _engine;

        #endregion

        #region Public fields

        /// <summary>
        /// Native handle
        /// </summary>
        public IntPtr Handle => _handle;

        /// <summary>
        /// Next instruction
        /// </summary>
        public override EVMOpCode NextInstruction
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

                return (EVMOpCode)NeoVM.ExecutionContext_GetNextInstruction(_handle);
            }
        }

        /// <summary>
        /// Get Instruction pointer
        /// </summary>
        public override int InstructionPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

                return NeoVM.ExecutionContext_GetInstructionPointer(_handle);
            }
        }

        /// <summary>
        /// Script Hash
        /// </summary>
        public override byte[] ScriptHash
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

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
        public override Stack AltStack
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _altStack; }
        }

        /// <summary>
        /// EvaluationStack
        /// </summary>
        public override Stack EvaluationStack
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
            _engine = engine;

            if (engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

            _handle = handle;

            NeoVM.ExecutionContext_Claim(_handle, out IntPtr evHandle, out IntPtr altHandle);

            _altStack = new StackItemStack(engine, altHandle);
            _evaluationStack = new StackItemStack(engine, evHandle);
        }
    }
}
