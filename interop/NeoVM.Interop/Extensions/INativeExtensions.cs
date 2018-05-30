﻿using NeoSharp.VM;
using NeoSharp.VM.Helpers;
using NeoVM.Interop.Native;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Extensions
{
    public unsafe static class INativeExtensions
    {
        /// <summary>
        /// Convert native pointer to stack item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>Return StackItem</returns>
        public static IStackItem ConvertFromNative(this ExecutionEngine engine, IntPtr item)
        {
            if (item == IntPtr.Zero) return null;

            EStackItemType state = (EStackItemType)NeoVM.StackItem_SerializeInfo(item, out int size);
            if (state == EStackItemType.None) return null;

            int readed;
            byte[] payload;

            if (size > 0)
            {
                payload = new byte[size];
                fixed (byte* p = payload)
                {
                    readed = NeoVM.StackItem_Serialize(item, (IntPtr)p, size);
                }
            }
            else
            {
                readed = 0;
                payload = null;
            }

            switch (state)
            {
                case EStackItemType.Array: return new ArrayStackItem(engine, item, false);
                case EStackItemType.Struct: return new ArrayStackItem(engine, item, true);
                case EStackItemType.Map: return new MapStackItem(engine, item);
                case EStackItemType.Interop:
                    {
                        // Extract object

                        return new InteropStackItem(engine, item, BitHelper.ToInt32(payload, 0));
                    }
                case EStackItemType.ByteArray: return new ByteArrayStackItem(engine, item, payload ?? (new byte[] { }));
                case EStackItemType.Integer:
                    {
                        if (readed != size)
                        {
                            // TODO: Try to fix this issue with BigInteger
                            Array.Resize(ref payload, readed);
                        }

                        return new IntegerStackItem(engine, item, payload ?? (new byte[] { }));
                    }
                case EStackItemType.Bool: return new BooleanStackItem(engine, item, payload ?? (new byte[] { }));
                default: throw new ExternalException();
            }
        }

        /// <summary>
        /// Create native item
        /// </summary>
        /// <param name="item">Item</param>
        public static IntPtr CreateNativeItem(this INativeStackItem item)
        {
            byte[] data = item.GetNativeByteArray();

            if (data == null)
            {
                return NeoVM.StackItem_Create((byte)item.Type, IntPtr.Zero, 0);
            }
            else
            {
                fixed (byte* p = data)
                {
                    return NeoVM.StackItem_Create((byte)item.Type, (IntPtr)p, data.Length);
                }
            }
        }
    }
}