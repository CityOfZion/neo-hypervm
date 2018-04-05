using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using System;

namespace NeoVM.Interop.Types
{
    unsafe public class ExecutionContext
    {
        /// <summary>
        /// Native handle
        /// </summary>
        internal readonly IntPtr Handle;
        /// <summary>
        /// ScriptHashLength
        /// </summary>
        public const int ScriptHashLength = 20;

        byte[] _ScriptHash;

        /// <summary>
        /// Next instruction
        /// </summary>
        public EVMOpCode NextInstruction => NeoVM.ExecutionContext_GetNextInstruction(Handle);
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
        /// <param name="handle">Handle</param>
        internal ExecutionContext(IntPtr handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return BitHelper.ToHexString(ScriptHash) + " (" + InstructionPointer.ToString("x6") + ") ";
        }
    }
}