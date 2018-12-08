using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NeoSharp.VM.Interop.Types.Collections
{
    public class ExecutionContextStack : StackBase<ExecutionContextBase>
    {
        /// <summary>
        /// Native handle
        /// </summary>
        private readonly IntPtr _handle;

        /// <summary>
        /// Engine
        /// </summary>
        private readonly ExecutionEngine _engine;

        /// <summary>
        /// Return the number of items in the stack
        /// </summary>
        public override int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

                return NeoVM.ExecutionContextStack_Count(_handle);
            }
        }

        #region Not implemented

        public override int Drop(int count = 0) => throw new NotImplementedException();
        public override ExecutionContextBase Pop() => throw new NotImplementedException();
        public override void Push(ExecutionContextBase item) => throw new NotImplementedException();

        #endregion

        /// <summary>
        /// Try to obtain the element at `index` position, without consume them
        /// -1=Last , -2=Last-1 , -3=Last-2
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="obj">Object</param>
        /// <returns>Return tru eif object exists</returns>
        public override bool TryPeek(int index, out ExecutionContextBase obj)
        {
            if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));

            var ptr = NeoVM.ExecutionContextStack_Peek(_handle, index);

            if (ptr == IntPtr.Zero)
            {
                obj = null;
                return false;
            }

            obj = new ExecutionContext(_engine, ptr);
            return true;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal ExecutionContextStack(ExecutionEngine engine, IntPtr handle)
        {
            _engine = engine;
            _handle = handle;

            if (handle == IntPtr.Zero) throw new ExternalException();
            if (_engine.IsDisposed) throw new ObjectDisposedException(nameof(ExecutionEngine));
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString() => Count.ToString();
    }
}