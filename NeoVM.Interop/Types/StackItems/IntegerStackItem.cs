using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Numerics;

namespace NeoVM.Interop.Types.StackItems
{
    public class IntegerStackItem : IStackItem<BigInteger>, IEquatable<IntegerStackItem>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal IntegerStackItem(ExecutionEngine engine, int data) : base(engine, new BigInteger(data), EStackItemType.Integer)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal IntegerStackItem(ExecutionEngine engine, long data) : base(engine, new BigInteger(data), EStackItemType.Integer)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal IntegerStackItem(ExecutionEngine engine, byte[] data) : base(engine, new BigInteger(data), EStackItemType.Integer)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal IntegerStackItem(ExecutionEngine engine, BigInteger data) : base(engine, data, EStackItemType.Integer)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="value">Raw value</param>
        internal IntegerStackItem(ExecutionEngine engine, IntPtr handle, byte[] value) :
            base(engine, new BigInteger(value), EStackItemType.Integer, handle)
        { }

        public bool Equals(IntegerStackItem other)
        {
            return other != null && other.Value.Equals(Value);
        }

        public override bool Equals(IStackItem other)
        {
            if (other is IntegerStackItem b)
                return b.Value.Equals(Value);

            return false;
        }

        protected override byte[] GetNativeByteArray()
        {
            return Value.ToByteArray();
        }
    }
}