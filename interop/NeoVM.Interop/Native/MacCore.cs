using NeoVM.Interop.Enums;

namespace NeoVM.Interop.Native
{
    internal class MacCore : UnixCore
    {
        public MacCore() : base(EPlatform.Mac, ".so") { }
    }
}