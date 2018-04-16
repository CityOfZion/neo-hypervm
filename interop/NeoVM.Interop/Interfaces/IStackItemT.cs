using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using System;

namespace NeoVM.Interop.Interfaces
{
    public abstract class IStackItem<T> : IStackItem
    {
        /// <summary>
        /// Value
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        /// <param name="type">Type</param>
        protected IStackItem(ExecutionEngine engine, T data, EStackItemType type) : base(engine, type)
        {
            Value = data;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="data">Data</param>
        /// <param name="type">Type</param>
        /// <param name="handle">Handle</param>
        protected IStackItem(ExecutionEngine engine, T data, EStackItemType type, IntPtr handle) : base(engine, type, handle)
        {
            Value = data;
        }
        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return Value?.ToString();
        }
    }
}