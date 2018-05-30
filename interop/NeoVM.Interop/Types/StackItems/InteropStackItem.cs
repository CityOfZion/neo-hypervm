using NeoSharp.VM;
using NeoSharp.VM.Helpers;
using NeoVM.Interop.Extensions;
using NeoVM.Interop.Native;
using System;

namespace NeoVM.Interop.Types.StackItems
{
    public class InteropStackItem : IInteropStackItem, INativeStackItem
    {
        readonly int _objKey;

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
        internal InteropStackItem(ExecutionEngine engine, object data) : base(engine, data)
        {
            // Search

            _objKey = engine.InteropCache.IndexOf(data);

            // New

            if (_objKey == -1)
            {
                _objKey = engine.InteropCache.Count;
                engine.InteropCache.Add(data);
            }

            // Create native item

            _handle = this.CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="objKey">Object key</param>
        internal InteropStackItem(ExecutionEngine engine, IntPtr handle, int objKey) :
            base(engine, engine.InteropCache[objKey])
        {
            _objKey = objKey;
            _handle = handle;
        }

        public byte[] GetNativeByteArray()
        {
            return BitHelper.GetBytes(_objKey);
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