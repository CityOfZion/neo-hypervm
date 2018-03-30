using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using NeoVM.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Types.StackItems
{
    public class InteropStackItem : IStackItem<object>, IEquatable<InteropStackItem>
    {
        IntPtr ObjectPointer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal InteropStackItem(ExecutionEngine engine, object data) : base(engine, data, EStackItemType.Interop)
        {
            CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="data">Data</param>
        /// <param name="ptr">Object pointer</param>
        internal InteropStackItem(ExecutionEngine engine, IntPtr handle, object data, IntPtr ptr) :
            base(engine, data, EStackItemType.Interop, handle)
        {
            ObjectPointer = ptr;
        }

        protected override void Free()
        {
            base.Free();

            if (ObjectPointer != IntPtr.Zero)
            {
                Marshal.Release(ObjectPointer);
                ObjectPointer = IntPtr.Zero;
            }
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

        protected override byte[] GetNativeByteArray()
        {
            IntPtr ptr = Marshal.GetIUnknownForObject(Value);
            return BitHelper.GetBytes(ptr.ToInt64());
        }
    }
}