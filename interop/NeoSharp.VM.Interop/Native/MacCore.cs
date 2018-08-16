using System;
using System.IO;
using NeoSharp.VM.Interop.Enums;

namespace NeoSharp.VM.Interop.Native
{
    internal class MacCore : LinuxCore
    {
        public MacCore() : base(EPlatform.Mac, ".dynlib") { }

        /// <summary>
        /// Try to search first .dynlib then .so
        /// </summary>
        /// <param name="fileName">Filename</param>
        /// <param name="handle">Handle</param>
        /// <returns>Return true if is loaded</returns>
        protected override bool InternalLoadLibrary(string fileName, out IntPtr handle)
        {
            if (!File.Exists(fileName))
            {
                var unixPath = Path.ChangeExtension(fileName, ".so");

                if (File.Exists(unixPath))
                {
                    fileName = unixPath;
                }
            }

            return base.InternalLoadLibrary(fileName, out handle);
        }
    }
}