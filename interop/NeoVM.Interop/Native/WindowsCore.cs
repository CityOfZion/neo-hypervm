using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Native
{
    internal class WindowsCore : CrossPlatformLibrary
    {
        const string NativeLibrary = "kernel32.dll";

        public WindowsCore() : base(EPlatform.Windows, ".dll") { }

        #region Windows

        [DllImport(NativeLibrary, EntryPoint = "LoadLibrary")]
        static extern IntPtr _LoadLibrary(string fileName);

        [DllImport(NativeLibrary, EntryPoint = "FreeLibrary")]
        static extern bool _FreeLibrary(IntPtr hModule);

        [DllImport(NativeLibrary, EntryPoint = "GetProcAddress")]
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