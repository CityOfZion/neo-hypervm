using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using NeoVM.Interop.Interfaces;
using System;

namespace NeoVM.Interop.Types.StackItems
{
    public class InteropStackItem : IStackItem<object>, IEquatable<InteropStackItem>
    {
        int ObjKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal InteropStackItem(ExecutionEngine engine, object data) : base(engine, data, EStackItemType.Interop)
        {
            // Search

            ObjKey = engine.InteropCache.IndexOf(data);

            // New

            if (ObjKey == -1)
            {
                ObjKey = engine.InteropCache.Count;
                engine.InteropCache.Add(data);
            }

            // Create native item

            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="objKey">Object key</param>
        internal InteropStackItem(ExecutionEngine engine, IntPtr handle, int objKey) :
            base(engine, engine.InteropCache[objKey], EStackItemType.Interop, handle)
        {
            ObjKey = objKey;
        }

        public bool Equals(InteropStackItem other)
        {
            return other != null && other.Value.Equals(Value);
        }

        public override bool Equals(IStackItem other)
        {
            if (other is InteropStackItem b)
                return b.Value.Equals(Value);

            return false;
        }

        public override bool CanConvertToByteArray => false;
        public override byte[] ToByteArray() { throw new NotImplementedException(); }

        protected override byte[] GetNativeByteArray()
        {
            return BitHelper.GetBytes(ObjKey);
        }
    }
}