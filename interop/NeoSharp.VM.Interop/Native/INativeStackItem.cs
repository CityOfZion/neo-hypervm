using System;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Types;
using Newtonsoft.Json;

namespace NeoSharp.VM.Interop.Native
{
    public interface INativeStackItem
    {
        /// <summary>
        /// Handle
        /// </summary>
        [JsonIgnore]
        IntPtr Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Native engine
        /// </summary>
        [JsonIgnore]
        ExecutionEngine NativeEngine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Type
        /// </summary>
        EStackItemType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Get native byte array
        /// </summary>
        byte[] GetNativeByteArray();
    }
}