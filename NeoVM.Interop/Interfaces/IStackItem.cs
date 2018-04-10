using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using System;

namespace NeoVM.Interop.Interfaces
{
    unsafe public abstract class IStackItem : IEquatable<IStackItem>, IDisposable
    {
        /// <summary>
        /// Can convert to byte array
        /// </summary>
        public abstract bool CanConvertToByteArray { get; }
        /// <summary>
        /// Engine
        /// </summary>
        internal readonly ExecutionEngine Engine;
        /// <summary>
        /// Native Handle
        /// </summary>
        internal IntPtr Handle;
        /// <summary>
        /// Type
        /// </summary>
        public readonly EStackItemType Type;
        /// <summary>
        /// Is Disposed
        /// </summary>
        public bool IsDisposed => Handle == IntPtr.Zero;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="type">Type</param>
        /// <param name="handle">Handle</param>
        protected IStackItem(ExecutionEngine engine, EStackItemType type, IntPtr handle)
        {
            Type = type;
            Handle = handle;
            Engine = engine;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="type">Type</param>
        protected IStackItem(ExecutionEngine engine, EStackItemType type)
        {
            Type = type;
            Engine = engine;
        }

        /// <summary>
        /// Get native byte array
        /// </summary>
        protected abstract byte[] GetNativeByteArray();

        /// <summary>
        /// Convert to Byte array
        /// </summary>
        public abstract byte[] ToByteArray();

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="other">Other item</param>
        public virtual bool Equals(IStackItem other)
        {
            if (other == null) return false;

            return ReferenceEquals(this, other);
        }

        /// <summary>
        /// Create native item
        /// </summary>
        protected void CreateNativeItem()
        {
            byte[] data = GetNativeByteArray();

            if (data == null)
            {
                Handle = NeoVM.StackItem_Create((byte)Type, IntPtr.Zero, 0);
            }
            else
            {
                fixed (byte* p = data)
                {
                    Handle = NeoVM.StackItem_Create((byte)Type, (IntPtr)p, data.Length);
                }
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            lock (this)
            {
                if (Handle == IntPtr.Zero) return;

                NeoVM.StackItem_Free(ref Handle);
            }
        }

        ~IStackItem()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}