using System;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Extensions;
using NeoSharp.VM.Interop.Native;
using Newtonsoft.Json;

namespace NeoSharp.VM.Interop.Types.StackItems
{
    public class BooleanStackItem : IBooleanStackItem, INativeStackItem
    {
        #region Private fields

        private static readonly byte[] FALSE_0 = { 0 };

        /// <summary>
        /// Native Handle
        /// </summary>
        private IntPtr _handle;

        #endregion

        #region Public fields

        /// <summary>
        /// Native engine
        /// </summary>
        [JsonIgnore]
        public ExecutionEngine NativeEngine
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set;
        }

        /// <summary>
        /// Native Handle
        /// </summary>
        [JsonIgnore]
        public IntPtr Handle
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _handle; }
        }

        /// <summary>
        /// Is Disposed
        /// </summary>
        [JsonIgnore]
        public override bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _handle == IntPtr.Zero; }
        }

        /// <summary>
        /// Type
        /// </summary>
        public new EStackItemType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return base.Type; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal BooleanStackItem(ExecutionEngine engine, bool data) : base(data)
        {
            NativeEngine = engine;
            _handle = this.CreateNativeItem();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="value">Raw value</param>
        internal BooleanStackItem(ExecutionEngine engine, IntPtr handle, byte[] value) :
            base(value != null && value[0] == NeoVM.TRUE)
        {
            NativeEngine = engine;
            _handle = handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetNativeByteArray() => Value ? TRUE : FALSE_0;

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero) return;

            NeoVM.StackItem_Free(ref _handle);
        }

        #endregion
    }
}