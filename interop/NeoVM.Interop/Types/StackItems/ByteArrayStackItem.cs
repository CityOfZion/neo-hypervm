using NeoSharp.VM;
using NeoVM.Interop.Extensions;
using NeoVM.Interop.Native;
using System;

namespace NeoVM.Interop.Types.StackItems
{
    public class ByteArrayStackItem : IByteArrayStackItem, INativeStackItem
    {
        /// <summary>
        /// Native Handle
        /// </summary>
        IntPtr _handle;
        /// <summary>
        /// Native Handle
        /// </summary>
        public IntPtr Handle => _handle;
        /// <summary>
        /// Is Disposed
        /// </summary>
        public override bool IsDisposed => _handle == IntPtr.Zero;
        /// <summary>
        /// Type
        /// </summary>
        public new EStackItemType Type => base.Type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        internal ByteArrayStackItem(ExecutionEngine engine, byte[] data) : base(engine, data)
        {
            _handle = this.CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="value">Raw value</param>
        internal ByteArrayStackItem(ExecutionEngine engine, IntPtr handle, byte[] value) : base(engine, value)
        {
            _handle = handle;
        }

        public byte[] GetNativeByteArray()
        {
            return Value;
        }

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_handle == IntPtr.Zero) return;

                NeoVM.StackItem_Free(ref _handle);
            }
        }

        #endregion
    }
}