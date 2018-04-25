using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;

namespace NeoVM.Interop.Types.StackItems
{
    public class BooleanStackItem : IStackItem<bool>, IEquatable<BooleanStackItem>
    {
        static readonly byte[] TRUE = { 1 };
        static readonly byte[] FALSE = new byte[0];
        static readonly byte[] FALSE_0 = { 0 };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal BooleanStackItem(ExecutionEngine engine, bool data) : base(engine, data, EStackItemType.Bool)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="value">Raw value</param>
        internal BooleanStackItem(ExecutionEngine engine, IntPtr handle, byte[] value) : base(engine, value[0] == NeoVM.TRUE, EStackItemType.Bool, handle) { }

        public bool Equals(BooleanStackItem other)
        {
            return other != null && other.Value == Value;
        }

        public override bool Equals(IStackItem other)
        {
            if (other is BooleanStackItem b)
                return b.Value == Value;

            return false;
        }

        public override bool CanConvertToByteArray => true;
        public override byte[] ToByteArray()
        {
            return Value ? TRUE : FALSE;
        }

        protected override byte[] GetNativeByteArray()
        {
            return Value ? TRUE : FALSE_0;
        }
    }
}