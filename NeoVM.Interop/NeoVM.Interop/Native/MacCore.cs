using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace NeoVM.Interop.Native
{
    internal class MacCore : CrossPlatformLibrary
    {
        public MacCore() : base(EPlatform.Mac, ".so") { }

        #region Unix

        // https://stackoverflow.com/questions/13461989/p-invoke-to-dynamically-loaded-library-on-mono
        // http://dimitry-i.blogspot.com.es/2013/01/mononet-how-to-dynamically-load-native.html

        const int RTLD_NOW = 2;

        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(String fileName, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, String symbol);

        [DllImport("libdl.so")]
        private static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
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
            handle = dlopen(fileName, RTLD_NOW);
            return handle != IntPtr.Zero;
        }

        #endregion
    }
}