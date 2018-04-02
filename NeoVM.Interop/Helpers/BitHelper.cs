using System;

namespace NeoVM.Interop.Helpers
{
    /// <summary>
    /// This helper was created to guarantee deterministic results in different platforms
    ///   https://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs
    /// </summary>
    unsafe public class BitHelper
    {
        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        public static byte[] GetBytes(long value)
        {
            byte[] bytes = new byte[8];

            fixed (byte* b = bytes)
                *((long*)b) = value;

            if (!BitConverter.IsLittleEndian)
            {
                // Convert to Little Endian

                Array.Reverse(bytes);
            }

            return bytes;
        }
        /// <summary>
        /// To Int64
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="startIndex">index</param>
        public static long ToInt64(byte[] value, int startIndex)
        {
            if (!BitConverter.IsLittleEndian)
            {
                // Convert to Little Endian

                Array.Reverse(value, startIndex, 8);
            }

            fixed (byte* pbyte = &value[startIndex])
            {
                if (startIndex % 8 == 0)
                { 
                    // data is aligned 
                    return *((long*)pbyte);
                }

                int i1 = (*pbyte) | (*(pbyte + 1) << 8) | (*(pbyte + 2) << 16) | (*(pbyte + 3) << 24);
                int i2 = (*(pbyte + 4)) | (*(pbyte + 5) << 8) | (*(pbyte + 6) << 16) | (*(pbyte + 7) << 24);
                return (uint)i1 | ((long)i2 << 32);
            }
        }
    }
}