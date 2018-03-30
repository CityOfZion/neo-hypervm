using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using System;

namespace NeoVM.Interop.Interfaces
{
    public abstract class IStackItem : IEquatable<IStackItem>, IDisposable
    {
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
        /// Is memory released
        /// </summary>
        public bool IsReleased => Handle == IntPtr.Zero;

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
            Handle = NeoVM.StackItem_Create(Type, data, data.Length);
        }
        /// <summary>
        /// Free native resources
        /// </summary>
        protected virtual void Free()
        {
            lock (this)
            {
                if (Handle == IntPtr.Zero) return;

                NeoVM.StackItem_Free(ref Handle);
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed state (managed objects).
            }

            // free unmanaged resources (unmanaged objects) and override a finalizer below. set large fields to null.
            Free();
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