using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using System;

namespace NeoVM.Interop.Types
{
    unsafe public class ExecutionContext : IDisposable
    {
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
        /// <param name="handle">Handle</param>
        internal ExecutionContext(IntPtr handle)
        {
            Handle = handle;
            NeoVM.ExecutionContext_Claim(Handle);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (Handle == IntPtr.Zero) return;

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.
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