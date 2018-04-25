using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NeoVM.Interop.Types.StackItems
{
    public class MapStackItem : IStackItem, IEnumerable<KeyValuePair<IStackItem, IStackItem>>
    {
        public override bool CanConvertToByteArray => false;
        public override byte[] ToByteArray() { throw new NotImplementedException(); }

        public IStackItem this[IStackItem key]
        {
            get
            {
                if (TryGetValue(key, out IStackItem value))
                    return value;

                return null;
            }
            set
            {
                Set(key, value);
            }
        }

        public IEnumerable<IStackItem> Keys => GetKeys();
        public IEnumerable<IStackItem> Values => GetValues();
        public int Count => NeoVM.MapStackItem_Count(Handle);

        #region Constructors

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
            CreateNativeItem();

            if (value == null) return;

            foreach (KeyValuePair<IStackItem, IStackItem> pair in value)
                Set(pair.Key, pair.Value);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal MapStackItem(ExecutionEngine engine, IntPtr handle) : base(engine, EStackItemType.Map, handle) { }

        #endregion

        #region Write

        public bool Remove(IStackItem key)
        {
            return NeoVM.MapStackItem_Remove(Handle, key == null ? IntPtr.Zero : key.Handle) == NeoVM.TRUE;
        }

        public void Set(KeyValuePair<IStackItem, IStackItem> item)
        {
            Set(item.Key, item.Value);
        }

        public void Set(IStackItem key, IStackItem value)
        {
            NeoVM.MapStackItem_Set
                (
                Handle,
                key == null ? IntPtr.Zero : key.Handle,
                value == null ? IntPtr.Zero : value.Handle
                );
        }

        public void Clear()
        {
            NeoVM.MapStackItem_Clear(Handle);
        }

        #endregion

        #region Read

        public bool ContainsKey(IStackItem key)
        {
            return NeoVM.MapStackItem_Get(Handle, key == null ? IntPtr.Zero : key.Handle) != IntPtr.Zero;
        }

        public bool TryGetValue(IStackItem key, out IStackItem value)
        {
            IntPtr ret = NeoVM.MapStackItem_Get(Handle, key == null ? IntPtr.Zero : key.Handle);

            if (ret == IntPtr.Zero)
            {
                value = null;
                return false;
            }

            value = Engine.ConvertFromNative(ret);
            return true;
        }

        #endregion

        #region Enumerables

        public IEnumerable<IStackItem> GetKeys()
        {
            for (int x = 0, c = Count; x < c; x++)
                yield return Engine.ConvertFromNative(NeoVM.MapStackItem_GetKey(Handle, x));
        }

        public IEnumerable<IStackItem> GetValues()
        {
            for (int x = 0, c = Count; x < c; x++)
                yield return Engine.ConvertFromNative(NeoVM.MapStackItem_GetValue(Handle, x));
        }
        
        IEnumerator<KeyValuePair<IStackItem, IStackItem>> IEnumerable<KeyValuePair<IStackItem, IStackItem>>.GetEnumerator()
        {
            for (int x = 0, c = Count; x < c; x++)
            {
                IStackItem key = Engine.ConvertFromNative(NeoVM.MapStackItem_GetKey(Handle, x));
                IStackItem value = Engine.ConvertFromNative(NeoVM.MapStackItem_GetValue(Handle, x));

                yield return new KeyValuePair<IStackItem, IStackItem>(key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int x = 0, c = Count; x < c; x++)
            {
                IStackItem key = Engine.ConvertFromNative(NeoVM.MapStackItem_GetKey(Handle, x));
                IStackItem value = Engine.ConvertFromNative(NeoVM.MapStackItem_GetValue(Handle, x));

                yield return new KeyValuePair<IStackItem, IStackItem>(key, value);
            }
        }

        #endregion

        protected override byte[] GetNativeByteArray()
        {
            return null;
        }
    }
}