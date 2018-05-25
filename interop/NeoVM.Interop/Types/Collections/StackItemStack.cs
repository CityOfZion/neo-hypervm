using NeoVM.Interop.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Types.Collections
{
    public class StackItemStack : IEnumerable<IStackItem>, IDisposable
    {
        /// <summary>
        /// Execution Engine parent
        /// </summary>
        public readonly ExecutionEngine Engine;
        /// <summary>
        /// Native handle
        /// </summary>
        private IntPtr Handle;

        /// <summary>
        /// Return the number of items in the stack
        /// </summary>
        public int Count => NeoVM.StackItems_Count(Handle);
        /// <summary>
        /// Drop object from the stack
        /// </summary>
        /// <param name="count">Number of items to drop</param>
        /// <returns>Return the first element of the stack</returns>
        public int Drop(int count = 0)
        {
            return NeoVM.StackItems_Drop(Handle, count);
        }
        /// <summary>
        /// Pop object from the stack
        /// </summary>
        /// <param name="free">True for free object</param>
        /// <returns>Return the first element of the stack</returns>
        public IStackItem Pop()
        {
            IntPtr ptr = NeoVM.StackItems_Pop(Handle);

            if (ptr == IntPtr.Zero)
                throw (new IndexOutOfRangeException());

            return Engine.ConvertFromNative(ptr);
        }
        /// <summary>
        /// Push objet to the stack
        /// </summary>
        /// <param name="item">Object</param>
        public void Push(IStackItem item)
        {
            NeoVM.StackItems_Push(Handle, item.Handle);
        }
        /// <summary>
        /// Try to obtain the element at `index` position, without consume them
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="obj">Object</param>
        /// <returns>Return tru eif object exists</returns>
        public bool TryPeek(int index, out IStackItem obj)
        {
            if (index < 0)
            {
                obj = null;
                return false;
            }

            IntPtr ptr = NeoVM.StackItems_Peek(Handle, index);

            if (ptr == IntPtr.Zero)
            {
                obj = null;
                return false;
            }

            obj = Engine.ConvertFromNative(ptr);
            return true;
        }
        /// <summary>
        /// Obtain the element at `index` position, without consume them
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Return object</returns>
        public IStackItem Peek(int index = 0)
        {
            if (!TryPeek(index, out IStackItem obj))
                throw new ArgumentOutOfRangeException();

            return obj;
        }
        /// <summary>
        /// Obtain the element at `index` position, without consume them
        /// </summary>
        /// <param name="index">Index</param>
        /// <typeparam name="TStackItem">Object type</typeparam>
        /// <returns>Return object</returns>
        public TStackItem Peek<TStackItem>(int index = 0) where TStackItem : IStackItem
        {
            if (!TryPeek(index, out IStackItem obj))
                throw new ArgumentOutOfRangeException();

            if (obj is TStackItem ts) return ts;

            throw (new FormatException());
        }
        /// <summary>
        /// Pop object casting to this type
        /// </summary>
        /// <typeparam name="TStackItem">Object type</typeparam>
        /// <returns>Return object</returns>
        public TStackItem Pop<TStackItem>() where TStackItem : IStackItem
        {
            if (Pop() is TStackItem ts) return ts;

            throw (new FormatException());
        }
        /// <summary>
        /// Try Pop object casting to this type
        /// </summary>
        /// <typeparam name="TStackItem">Object type</typeparam>
        /// <param name="item">Item</param>
        /// <returns>Return false if it is something wrong</returns>
        public bool TryPop<TStackItem>(out TStackItem item) where TStackItem : IStackItem
        {
            IntPtr ptr = NeoVM.StackItems_Pop(Handle);

            if (ptr == IntPtr.Zero)
            {
                item = default(TStackItem);
                return false;
            }

            IStackItem ret = Engine.ConvertFromNative(ptr);

            if (ret is TStackItem ts)
            {
                item = (TStackItem)ret;
                return true;
            }

            item = default(TStackItem);
            return false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal StackItemStack(ExecutionEngine engine, IntPtr handle)
        {
            Engine = engine;
            Handle = handle;

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

        public IEnumerator<IStackItem> GetEnumerator()
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