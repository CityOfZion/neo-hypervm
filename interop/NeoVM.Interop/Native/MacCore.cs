using NeoVM.Interop.Enums;

namespace NeoVM.Interop.Native
{
    internal class MacCore : LinuxCore
    {
        public MacCore() : base(EPlatform.Mac, ".so") { }
    }
}