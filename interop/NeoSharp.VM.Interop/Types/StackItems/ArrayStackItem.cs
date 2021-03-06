﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NeoSharp.VM.Interop.Extensions;
using NeoSharp.VM.Interop.Native;
using Newtonsoft.Json;

namespace NeoSharp.VM.Interop.Types.StackItems
{
    public class ArrayStackItem : ArrayStackItemBase, INativeStackItem
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
        /// Count
        /// </summary>
        public override int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return NeoVM.ArrayStackItem_Count(_handle); }
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
        /// Index
        /// </summary>
        /// <param name="index">Position</param>
        /// <returns>Returns the StackItem element</returns>
        public override StackItemBase this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return NativeEngine.ConvertFromNative(NeoVM.ArrayStackItem_Get(_handle, index)); }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Set(index, value); }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, bool isStruct) :
            base(isStruct)
        {
            NativeEngine = engine;
            _handle = this.CreateNativeItem();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IEnumerable<StackItemBase> data, bool isStruct) :
            base(isStruct)
        {
            NativeEngine = engine;
            _handle = this.CreateNativeItem();

            if (data != null)
            {
                foreach (var s in data)
                {
                    Add(s);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IntPtr handle, bool isStruct) : base(isStruct)
        {
            NativeEngine = engine;
            _handle = handle;
        }

        #region Read

        public override int IndexOf(StackItemBase item) => NeoVM.ArrayStackItem_IndexOf(Handle, ((INativeStackItem)item).Handle);

        #endregion

        #region Write

        public override void Add(StackItemBase item) => NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);

        public override void Add(params StackItemBase[] items)
        {
            foreach (var item in items)
            {
                NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);
            }
        }

        public override void Add(IEnumerable<StackItemBase> items)
        {
            foreach (var item in items)
            {
                NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);
            }
        }

        public override void Clear() => NeoVM.ArrayStackItem_Clear(Handle);

        public override void Insert(int index, StackItemBase item) => NeoVM.ArrayStackItem_Insert(Handle, ((INativeStackItem)item).Handle, index);

        public override void Set(int index, StackItemBase item) => NeoVM.ArrayStackItem_Set(Handle, ((INativeStackItem)item).Handle, index);

        public override void RemoveAt(int index) => NeoVM.ArrayStackItem_RemoveAt(Handle, index);

        #endregion

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