using System;

namespace NeoVM.Interop.Helpers
{
    public class BitHelper
    {
        /// <summary>
        /// Get bytes
        /// </summary>
        /// <param name="data">Data</param>
        public static byte[] GetBytes(long data)
        {
            return BitConverter.GetBytes(data);
        }
    }
}