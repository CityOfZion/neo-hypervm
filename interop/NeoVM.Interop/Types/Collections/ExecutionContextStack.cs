using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Types.Collections
{
    public class ExecutionContextStack : IEnumerable<ExecutionContext>, IDisposable
    {
        /// <summary>
        /// Native handle
        /// </summary>
        private IntPtr Handle;
        /// <summary>
        /// Engine
        /// </summary>
        private readonly ExecutionEngine Engine;

        /// <summary>
        /// Return the number of items in the stack
        /// </summary>
        public int Count => NeoVM.ExecutionContextStack_Count(Handle);
        /// <summary>
        /// Drop object from the stack
        /// </summary>
        /// <param name="count">Number of items to drop</param>
        /// <returns>Return the first element of the stack</returns>
        public int Drop(int count = 0)
        {
            return NeoVM.ExecutionContextStack_Drop(Handle, count);
        }
        /// <summary>
        /// Try to obtain the element at `index` position, without consume them
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="obj">Object</param>
        /// <returns>Return tru eif object exists</returns>
        public bool TryPeek(int index, out ExecutionContext obj)
        {
            if (index < 0)
            {
                obj = null;
                return false;
            }

            IntPtr ptr = NeoVM.ExecutionContextStack_Peek(Handle, index);

            if (ptr == IntPtr.Zero)
            {
                obj = null;
                return false;
            }

            obj = new ExecutionContext(Engine, ptr);
            return true;
        }
        /// <summary>
        /// Obtain the element at `index` position, without consume them
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Return object</returns>
        public ExecutionContext Peek(int index = 0)
        {
            if (!TryPeek(index, out ExecutionContext obj))
                throw new ArgumentOutOfRangeException();

            return obj;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal ExecutionContextStack(ExecutionEngine engine, IntPtr handle)
        {
            Handle = handle;
            Engine = engine;

            if (handle == IntPtr.Zero)
                throw new ExternalException();
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return Count.ToString();
        }

        #region IEnumerable

        public IEnumerator<ExecutionContext> GetEnumerator()
        {
            for (int x = 0, m = Count; x < m; x++)
                yield return Peek(x);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int x = 0, m = Count; x < m; x++)
                yield return Peek(x);
        }

        #endregion

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            Handle = IntPtr.Zero;
        }
    }
}