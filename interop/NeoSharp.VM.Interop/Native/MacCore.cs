using System;
using System.Runtime.InteropServices;
using NeoSharp.VM.Interop.Enums;
using NeoSharp.VM.Interop.Interfaces;

namespace NeoSharp.VM.Interop.Native
{
    internal class MacCore : CrossPlatformLibrary
    {
        const string NativeLibrary = "libdl.dylib";

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public MacCore() : base(EPlatform.Mac, ".dylib") { }

        /// <summary>
        /// Protected constructor
        /// </summary>
        /// <param name="platform">Platform</param>
        /// <param name="extension">Extension</param>
        protected MacCore(EPlatform platform, string extension) : base(platform, extension) { }

        #endregion

        #region Mac

        // const int RTLD_LAZY = 1;
        const int RTLD_NOW = 2;
        // const int RTLD_GLOBAL = 8;

        [DllImport(NativeLibrary)]
        private static extern IntPtr dlopen(string fileName, int flags);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int dlclose(IntPtr handle);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr dlerror();

        #endregion

        #region Internals

        protected override bool InternalFreeLibrary()
        {
            return dlclose(_nativeHandle) == 0;
        }

        protected override IntPtr GetProcAddress(string name)
        {
            // clear previous errors if any

            dlerror();

            var res = dlsym(_nativeHandle, name);

            if (res != IntPtr.Zero) return res;

            var errPtr = dlerror();

            if (errPtr != IntPtr.Zero)
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errPtr));

            return IntPtr.Zero;
        }

        protected override bool InternalLoadLibrary(string fileName, out IntPtr handle)
        {
            // clear previous errors if any

            dlerror();

            handle = dlopen(fileName, RTLD_NOW);

            if (handle != IntPtr.Zero) return true;

            var errPtr = dlerror();

            if (errPtr != IntPtr.Zero)
                throw new Exception("dlopen: " + Marshal.PtrToStringAnsi(errPtr));

            return false;
        }

        #endregion
    }
}