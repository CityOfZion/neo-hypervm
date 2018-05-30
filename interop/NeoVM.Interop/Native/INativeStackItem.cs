using NeoSharp.VM;
using System;

namespace NeoVM.Interop.Native
{
    public interface INativeStackItem
    {
        /// <summary>
        /// Handle
        /// </summary>
        IntPtr Handle { get; }
        /// <summary>
        /// Type
        /// </summary>
        EStackItemType Type { get; }
        /// <summary>
        /// Get native byte array
        /// </summary>
        byte[] GetNativeByteArray();
    }
}