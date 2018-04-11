﻿using System;
using System.Text;

namespace NeoVM.Interop.Helpers
{
    /// <summary>
    /// This helper was created to guarantee deterministic results in different platforms
    ///   https://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs
    /// </summary>
    unsafe public class BitHelper
    {
        /// <summary>
        /// Convert buffer to hex string
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Return hex string</returns>
        public static string ToHexString(byte[] data)
        {
            if (data == null) return "";

            int m = data.Length;
            if (m == 0) return "";

            char[] sb = new char[(m * 2) + 2];

            sb[0] = '0';
            sb[1] = 'x';

            for (int x = 0, y = 2; x < m; x++, y += 2)
            {
                string hex = data[x].ToString("x2");

                sb[y] = hex[0];
                sb[y + 1] = hex[1];
            }

            return new string(sb);
        }
        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        public static byte[] GetBytes(int value)
        {
            byte[] bytes = new byte[4];

            fixed (byte* b = bytes)
                *((int*)b) = value;

            if (!BitConverter.IsLittleEndian)
            {
                // Convert to Little Endian

                Array.Reverse(bytes);
            }

            return bytes;
        }
        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="value">Value</param>
        public static byte[] GetBytes(ushort value)
        {
            byte[] bytes = new byte[2];

            fixed (byte* b = bytes)
                *((ushort*)b) = value;

            if (!BitConverter.IsLittleEndian)
            {
                // Convert to Little Endian

                Array.Reverse(bytes);
            }

            return bytes;
        }
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