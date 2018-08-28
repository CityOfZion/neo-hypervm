using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Extensions;
using NeoSharp.VM.Interop.Native;
using Newtonsoft.Json;

namespace NeoSharp.VM.Interop.Types.StackItems
{
    public class MapStackItem : IMapStackItem, INativeStackItem
    {
        #region Private fields

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
        }

        public override bool CanConvertToByteArray
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return false; }
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

        /// <summary>
        /// Count
        /// </summary>
        public override int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return NeoVM.MapStackItem_Count(_handle); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        internal MapStackItem(ExecutionEngine engine) : this(engine, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="value">Value</param>
        internal MapStackItem(ExecutionEngine engine, Dictionary<IStackItem, IStackItem> value) : base()
        {
            NativeEngine = engine;
            _handle = this.CreateNativeItem();

            if (value == null) return;

            foreach (KeyValuePair<IStackItem, IStackItem> pair in value)
                Set(pair.Key, pair.Value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        internal MapStackItem(ExecutionEngine engine, IntPtr handle) : base()
        {
            NativeEngine = engine;
            _handle = handle;
        }

        #endregion

        #region Write

        public override bool Remove(IStackItem key)
            => NeoVM.MapStackItem_Remove(_handle, key == null ? IntPtr.Zero : ((INativeStackItem)key).Handle) == NeoVM.TRUE;

        public override void Set(KeyValuePair<IStackItem, IStackItem> item) => Set(item.Key, item.Value);

        public override void Set(IStackItem key, IStackItem value) =>

            NeoVM.MapStackItem_Set
                (
                _handle,
                key == null ? IntPtr.Zero : ((INativeStackItem)key).Handle,
                value == null ? IntPtr.Zero : ((INativeStackItem)value).Handle
                );

        public override void Clear() => NeoVM.MapStackItem_Clear(_handle);

        #endregion

        #region Read

        public override bool ContainsKey(IStackItem key)
            => NeoVM.MapStackItem_Get(_handle, key == null ? IntPtr.Zero : ((INativeStackItem)key).Handle) != IntPtr.Zero;

        public override bool TryGetValue(IStackItem key, out IStackItem value)
        {
            var ret = NeoVM.MapStackItem_Get(_handle, key == null ? IntPtr.Zero : ((INativeStackItem)key).Handle);

            if (ret == IntPtr.Zero)
            {
                value = null;
                return false;
            }

            value = NativeEngine.ConvertFromNative(ret);
            return true;
        }

        #endregion

        #region Enumerables

        public override IEnumerable<IStackItem> GetKeys()
        {
            for (int x = 0, c = Count; x < c; x++)
            {
                yield return NativeEngine.ConvertFromNative(NeoVM.MapStackItem_GetKey(_handle, x));
            }
        }

        public override IEnumerable<IStackItem> GetValues()
        {
            for (int x = 0, c = Count; x < c; x++)
            {
                yield return NativeEngine.ConvertFromNative(NeoVM.MapStackItem_GetValue(_handle, x));
            }
        }

        public override IEnumerator<KeyValuePair<IStackItem, IStackItem>> GetEnumerator()
        {
            for (int x = 0, c = Count; x < c; x++)
            {
                var key = NativeEngine.ConvertFromNative(NeoVM.MapStackItem_GetKey(_handle, x));
                var value = NativeEngine.ConvertFromNative(NeoVM.MapStackItem_GetValue(_handle, x));

                yield return new KeyValuePair<IStackItem, IStackItem>(key, value);
            }
        }

        #endregion

        public override byte[] ToByteArray() => throw new NotImplementedException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] GetNativeByteArray() => null;

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (_handle == IntPtr.Zero) return;

            NeoVM.StackItem_Free(ref _handle);
        }

        #endregion
    }
}