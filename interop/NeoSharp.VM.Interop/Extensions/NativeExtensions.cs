using System;
using System.Runtime.InteropServices;
using NeoSharp.VM.Extensions;
using NeoSharp.VM.Interop.Native;
using NeoSharp.VM.Interop.Types;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Extensions
{
    public unsafe static class NativeExtensions
    {
        private static readonly Type InteropType = typeof(InteropStackItem<>);

        /// <summary>
        /// Convert native pointer to stack item
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="item">Item</param>
        /// <returns>Return StackItem</returns>
        public static StackItemBase ConvertFromNative(this ExecutionEngine engine, IntPtr item)
        {
            if (item == IntPtr.Zero) return null;

            var state = (EStackItemType)NeoVM.StackItem_SerializeInfo(item, out int size);
            if (state == EStackItemType.None) return null;

            int read;
            byte[] payload;

            if (size > 0)
            {
                payload = new byte[size];
                fixed (byte* p = payload)
                {
                    read = NeoVM.StackItem_Serialize(item, (IntPtr)p, size);
                }
            }
            else
            {
                read = 0;
                payload = null;
            }

            switch (state)
            {
                case EStackItemType.Array: return new ArrayStackItem(engine, item, false);
                case EStackItemType.Struct: return new ArrayStackItem(engine, item, true);
                case EStackItemType.Map: return new MapStackItem(engine, item);
                case EStackItemType.Interop:
                    {
                        var key = payload.ToInt32(0);
                        var cache = engine.GetInteropObject(key);

                        return cache.Instanciator(engine, item, key, cache.Value);
                    }
                case EStackItemType.ByteArray: return new ByteArrayStackItem(engine, item, payload ?? (new byte[] { }));
                case EStackItemType.Integer:
                    {
                        if (read != size)
                        {
                            // TODO: Try to fix this issue with BigInteger

                            Array.Resize(ref payload, read);
                        }

                        return new IntegerStackItem(engine, item, payload ?? (new byte[] { }));
                    }
                case EStackItemType.Bool:
                    {
                        return new BooleanStackItem(engine, item, payload);
                    }
                default: throw new ExternalException();
            }
        }

        /// <summary>
        /// Create native item
        /// </summary>
        /// <param name="item">Item</param>
        public static IntPtr CreateNativeItem(this INativeStackItem item)
        {
            var data = item.GetNativeByteArray();

            if (data == null || data.Length == 0)
            {
                return NeoVM.StackItem_Create(item.NativeEngine.Handle, (byte)item.Type, IntPtr.Zero, 0);
            }

            fixed (byte* p = data)
            {
                return NeoVM.StackItem_Create(item.NativeEngine.Handle, (byte)item.Type, (IntPtr)p, data.Length);
            }
        }
    }
}