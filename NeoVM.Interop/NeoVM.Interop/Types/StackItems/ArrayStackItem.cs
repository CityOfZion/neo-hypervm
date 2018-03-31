using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeoVM.Interop.Types.StackItems
{
    public class ArrayStackItem : IStackItem, ICollection, IList<IStackItem>, IEquatable<ArrayStackItem>, IEquatable<IStackItem>
    {
        /// <summary>
        /// Count
        /// </summary>
        public int Count => NeoVM.ArrayStackItem_Count(Handle);
        /// <summary>
        /// IsStruct
        /// </summary>
        public bool IsStruct => Type == EStackItemType.Struct;
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="index">Position</param>
        /// <returns>Returns the StackItem element</returns>
        public IStackItem this[int index]
        {
            get
            {
                IntPtr ptr = NeoVM.ArrayStackItem_Get(Handle, index);
                return Engine.ConvertFromNative(ptr);
            }
            set
            {
                Set(index, value);
            }
        }

        /// <summary>
        /// IsReadOnly
        /// </summary>
        public bool IsReadOnly => false;
        /// <summary>
        /// IsSynchronized
        /// </summary>
        bool ICollection.IsSynchronized => false;
        /// <summary>
        /// SyncRoot
        /// </summary>
        object ICollection.SyncRoot => this;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, bool isStruct) :
            this(engine, new List<IStackItem>(), isStruct)
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IEnumerable<IStackItem> data, bool isStruct) :
            base(engine, isStruct ? EStackItemType.Struct : EStackItemType.Array)
        {
            CreateNativeItem();
            foreach (IStackItem s in data) Add(s);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IntPtr handle, bool isStruct) :
            base(engine, isStruct ? EStackItemType.Struct : EStackItemType.Array, handle)
        { }

        #region Write

        public void Add(IStackItem item)
        {
            NeoVM.ArrayStackItem_Add(Handle, item.Handle);
        }

        public void Add(params IStackItem[] items)
        {
            foreach (IStackItem i in items)
                NeoVM.ArrayStackItem_Add(Handle, i.Handle);
        }

        public void Add(IEnumerable<IStackItem> items)
        {
            foreach (IStackItem i in items)
                NeoVM.ArrayStackItem_Add(Handle, i.Handle);
        }

        public void Clear()
        {
            NeoVM.ArrayStackItem_Clear(Handle);
        }

        public int IndexOf(IStackItem item)
        {
            return NeoVM.ArrayStackItem_IndexOf(Handle, item.Handle);
        }

        public void Insert(int index, IStackItem item)
        {
            NeoVM.ArrayStackItem_Insert(Handle, index, item.Handle);
        }

        public void Set(int index, IStackItem item)
        {
            NeoVM.ArrayStackItem_Set(Handle, index, item.Handle);
        }

        public bool Remove(IStackItem item)
        {
            int ix = IndexOf(item);
            if (ix < 0) return false;

            RemoveAt(ix);
            return true;
        }

        public void RemoveAt(int index)
        {
            NeoVM.ArrayStackItem_RemoveAt(Handle, index, 0x01);
        }

        #endregion

        #region Read

        public bool Equals(ArrayStackItem other)
        {
            if (other == null) return false;
            if (other == this) return true;
            if (other.Type != Type) return false;

            return this.SequenceEqual(other);
        }

        public override bool Equals(IStackItem other)
        {
            if (!(other is ArrayStackItem st)) return false;
            if (st.Type != Type) return false;

            return this.SequenceEqual(st);
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns>Returns copy of this object</returns>
        public ArrayStackItem Clone()
        {
            if (Type == EStackItemType.Struct)
            {
                // Struct logic

                List<IStackItem> newArray = new List<IStackItem>(Count);

                foreach (IStackItem it in this)
                {
                    if (it is ArrayStackItem s && it.Type == EStackItemType.Struct)
                        newArray.Add(s.Clone());
                    else
                        newArray.Add(it);
                }
                return new ArrayStackItem(Engine, newArray, true);
            }
            else
            {
                return new ArrayStackItem(Engine, this, false);
            }
        }

        public bool Contains(IStackItem item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(Array array, int index)
        {
            foreach (IStackItem item in this) array.SetValue(item, index++);
        }

        public void CopyTo(IStackItem[] array, int index)
        {
            foreach (IStackItem item in this) array.SetValue(item, index++);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IStackItem> GetEnumerator()
        {
            for (int x = 0, m = Count; x < m; x++)
                yield return this[x];
        }

        #endregion

        protected override byte[] GetNativeByteArray()
        {
            return new byte[] { };
        }
    }
}