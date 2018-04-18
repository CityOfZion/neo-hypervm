using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Native
{
    internal class UnixCore : CrossPlatformLibrary
    {
        const string NativeLibrary = "libdl.so";

        #region Constructors

        public UnixCore() : base(EPlatform.Unix, ".so") { }
        protected UnixCore(EPlatform platform, string extension) : base(platform, extension) { }

        #endregion

        #region Unix

        // https://stackoverflow.com/questions/13461989/p-invoke-to-dynamically-loaded-library-on-mono
        // http://dimitry-i.blogspot.com.es/2013/01/mononet-how-to-dynamically-load-native.html

        const int RTLD_LAZY = 1;
        const int RTLD_NOW = 2;
        const int RTLD_GLOBAL = 8;

        [DllImport(NativeLibrary)]
        private static extern IntPtr dlopen([MarshalAs(UnmanagedType.LPTStr)] string fileName, int flags);

        [DllImport(NativeLibrary)]
        private static extern IntPtr dlsym(IntPtr handle, [MarshalAs(UnmanagedType.LPTStr)] string symbol);

        [DllImport(NativeLibrary)]
        private static extern int dlclose(IntPtr handle);

        [DllImport(NativeLibrary)]
        private static extern IntPtr dlerror();

        #endregion

        #region Internals

        protected override bool InternalFreeLibrary()
        {
            return dlclose(NativeHandle) == 0;
        }

        protected override IntPtr GetProcAddress(string name)
        {
            // clear previous errors if any

            dlerror();
            IntPtr res = dlsym(NativeHandle, name);
            IntPtr errPtr = dlerror();

            if (errPtr != IntPtr.Zero)
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));

            return res;
        }

        protected override bool InternalLoadLibrary(string fileName, out IntPtr handle)
        {
            // clear previous errors if any
            dlerror();
            handle = dlopen(fileName, RTLD_NOW);

            if (handle != IntPtr.Zero)
                return true;

            IntPtr errPtr = dlerror();

            if (errPtr != IntPtr.Zero)
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));

            return false;
        }

        #endregion
    }
}