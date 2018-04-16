using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoVM.Interop.Types.StackItems
{
    public class MapStackItem : IStackItem, ICollection, IDictionary<IStackItem, IStackItem>
    {
        public override bool CanConvertToByteArray => false;
        public override byte[] ToByteArray() { throw new NotImplementedException(); }

        readonly Dictionary<IStackItem, IStackItem> Value;

        public IStackItem this[IStackItem key]
        {
            get => Value[key];
            set => Value[key] = value;
        }

        public ICollection<IStackItem> Keys => Value.Keys;
        public ICollection<IStackItem> Values => Value.Values;
        public int Count => Value.Count;
        public bool IsReadOnly => false;

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        internal MapStackItem(ExecutionEngine engine) : this(engine, new Dictionary<IStackItem, IStackItem>()) { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="value">Value</param>
        internal MapStackItem(ExecutionEngine engine, Dictionary<IStackItem, IStackItem> value) : base(engine, EStackItemType.Map)
        {
            Value = value;
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal MapStackItem(ExecutionEngine engine, IntPtr handle) : base(engine, EStackItemType.Map, handle)
        {
            Value = new Dictionary<IStackItem, IStackItem>();
        }

        public void Add(IStackItem key, IStackItem value)
        {
            Value.Add(key, value);
        }

        void ICollection<KeyValuePair<IStackItem, IStackItem>>.Add(KeyValuePair<IStackItem, IStackItem> item)
        {
            Value.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Value.Clear();
        }

        bool ICollection<KeyValuePair<IStackItem, IStackItem>>.Contains(KeyValuePair<IStackItem, IStackItem> item)
        {
            return Value.ContainsKey(item.Key);
        }

        public bool ContainsKey(IStackItem key)
        {
            return Value.ContainsKey(key);
        }

        void ICollection<KeyValuePair<IStackItem, IStackItem>>.CopyTo(KeyValuePair<IStackItem, IStackItem>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<IStackItem, IStackItem> item in Value)
                array[arrayIndex++] = item;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (KeyValuePair<IStackItem, IStackItem> item in Value)
                array.SetValue(item, index++);
        }

        IEnumerator<KeyValuePair<IStackItem, IStackItem>> IEnumerable<KeyValuePair<IStackItem, IStackItem>>.GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public bool Remove(IStackItem key)
        {
            return Value.Remove(key);
        }

        bool ICollection<KeyValuePair<IStackItem, IStackItem>>.Remove(KeyValuePair<IStackItem, IStackItem> item)
        {
            return Value.Remove(item.Key);
        }

        public bool TryGetValue(IStackItem key, out IStackItem value)
        {
            return Value.TryGetValue(key, out value);
        }

        protected override byte[] GetNativeByteArray()
        {
            return null;
        }
    }
}