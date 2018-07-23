using System;

namespace NeoSharp.VM.Interop.Tests.Extra
{
    internal class DisposableDummy : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() { IsDisposed = true; }
    }
}