using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using NeoVM.Interop.Interfaces;
using System;
using System.Linq;
using System.Text;

namespace NeoVM.Interop.Types.StackItems
{
    public class ByteArrayStackItem : IStackItem<byte[]>, IEquatable<ByteArrayStackItem>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal ByteArrayStackItem(ExecutionEngine engine, byte[] data) : base(engine, data, EStackItemType.ByteArray)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="value">Raw value</param>
        internal ByteArrayStackItem(ExecutionEngine engine, IntPtr handle, byte[] value) : base(engine, value, EStackItemType.ByteArray, handle) { }

        public bool Equals(ByteArrayStackItem other)
        {
            return other != null && other.Value.SequenceEqual(Value);
        }

        public override bool Equals(IStackItem other)
        {
            if (other is ByteArrayStackItem b)
                return b.Value.SequenceEqual(Value);

            return false;
        }

        public override bool CanConvertToByteArray => true;
        public override byte[] ToByteArray()
        {
            return Value;
        }

        protected override byte[] GetNativeByteArray()
        {
            return Value;
        }

        public override string ToString()
        {
            if (Value == null) return "NULL";

            // Check printable characters

            bool allOk = true;
            foreach (byte c in Value)
                if (c < 32 || c > 126)
                {
                    allOk = false;
                    break;
                }

            return allOk ? "'" + Encoding.ASCII.GetString(Value) + "'" : BitHelper.ToHexString(Value);
        }
    }
}