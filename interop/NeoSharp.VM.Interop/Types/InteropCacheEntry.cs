using System;

namespace NeoSharp.VM.Interop.Types
{
    internal class InteropCacheEntry 
    {
        public object Value;

        public Func<ExecutionEngine, IntPtr, int, object, StackItemBase> Instanciator;
    }
}