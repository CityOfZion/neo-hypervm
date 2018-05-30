﻿using NeoSharp.VM;
using NeoVM.Interop.Extensions;
using NeoVM.Interop.Native;
using System;
using System.Collections.Generic;

namespace NeoVM.Interop.Types.StackItems
{
    public class ArrayStackItem : IArrayStackItem, INativeStackItem
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
        /// Count
        /// </summary>
        public override int Count => NeoVM.ArrayStackItem_Count(_handle);
        /// <summary>
        /// Type
        /// </summary>
        public new EStackItemType Type => base.Type;
        /// <summary>
        /// Engine
        /// </summary>
        private readonly new ExecutionEngine Engine;

        /// <summary>
        /// Index
        /// </summary>
        /// <param name="index">Position</param>
        /// <returns>Returns the StackItem element</returns>
        public override IStackItem this[int index]
        {
            get { return Engine.ConvertFromNative(NeoVM.ArrayStackItem_Get(_handle, index)); }
            set { Set(index, value); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, bool isStruct) :
            base(engine, isStruct)
        {
            Engine = engine;
            _handle = this.CreateNativeItem();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IEnumerable<IStackItem> data, bool isStruct) :
            base(engine, isStruct)
        {
            Engine = engine;
            _handle = this.CreateNativeItem();

            if (data != null)
            {
                foreach (var s in data)
                    Add(s);
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="handle">Handle</param>
        /// <param name="isStruct">Is struct?</param>
        internal ArrayStackItem(ExecutionEngine engine, IntPtr handle, bool isStruct) :
            base(engine, isStruct)
        {
            Engine = engine;
            _handle = handle;
        }

        #region Read

        public override int IndexOf(IStackItem item)
        {
            return NeoVM.ArrayStackItem_IndexOf(Handle, ((INativeStackItem)item).Handle);
        }

        #endregion

        #region Write

        public override void Add(IStackItem item)
        {
            NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);
        }

        public override void Add(params IStackItem[] items)
        {
            foreach (var item in items)
                NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);
        }

        public override void Add(IEnumerable<IStackItem> items)
        {
            foreach (var item in items)
                NeoVM.ArrayStackItem_Add(Handle, ((INativeStackItem)item).Handle);
        }

        public override void Clear()
        {
            NeoVM.ArrayStackItem_Clear(Handle);
        }

        public override void Insert(int index, IStackItem item)
        {
            NeoVM.ArrayStackItem_Insert(Handle, ((INativeStackItem)item).Handle, index);
        }

        public override void Set(int index, IStackItem item)
        {
            NeoVM.ArrayStackItem_Set(Handle, ((INativeStackItem)item).Handle, index);
        }

        public override void RemoveAt(int index)
        {
            NeoVM.ArrayStackItem_RemoveAt(Handle, index);
        }

        #endregion

        public byte[] GetNativeByteArray()
        {
            return null;
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