using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Native
{
    internal class WindowsCore : CrossPlatformLibrary
    {
        public WindowsCore() : base(EPlatform.Windows, ".dll") { }

        #region Windows

        [DllImport("kernel32", EntryPoint = "LoadLibrary")]
        static extern IntPtr _LoadLibrary(string fileName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool _FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        static extern IntPtr _GetProcAddress(IntPtr handle, string procedureName);

        #endregion

        #region Internals

        protected override bool InternalFreeLibrary()
        {
            return _FreeLibrary(NativeHandle);
        }

        protected override IntPtr GetProcAddress(string name)
        {
            return _GetProcAddress(NativeHandle, name);
        }

        protected override bool InternalLoadLibrary(string fileName, out IntPtr handle)
        {
            handle = _LoadLibrary(fileName);
            return handle != IntPtr.Zero;
        }

        #endregion
    }
}