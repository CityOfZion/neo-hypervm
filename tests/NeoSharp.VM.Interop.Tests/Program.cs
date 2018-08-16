using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeoSharp.VM.Interop.Tests
{
    class Program
    {
        readonly static Process Current = Process.GetCurrentProcess();

        /*

        [SuppressUnmanagedCodeSecurity]
        internal static class MiniDumpNativeMethods
        {
            [DllImport("dbghelp.dll",
                EntryPoint = "MiniDumpWriteDump",
                CallingConvention = CallingConvention.StdCall,
                CharSet = CharSet.Unicode,
                ExactSpelling = true,
                SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MiniDumpWriteDump(
                IntPtr hProcess,
                uint processId,
                SafeHandle hFile,
                MINIDUMP_TYPE dumpType,
                IntPtr expParam,
                IntPtr userStreamParam,
                IntPtr callbackParam);
        }

        [Flags]
        [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "")]
        [SuppressMessage("Microsoft.Naming", "CA1714:FlagsEnumsShouldHavePluralNames", Justification = "")]
        public enum MINIDUMP_TYPE : int
        {
            // From dbghelp.h:
            Normal = 0x00000000,
            WithDataSegs = 0x00000001,
            WithFullMemory = 0x00000002,
            WithHandleData = 0x00000004,
            FilterMemory = 0x00000008,
            ScanMemory = 0x00000010,
            WithUnloadedModules = 0x00000020,
            WithIndirectlyReferencedMemory = 0x00000040,
            FilterModulePaths = 0x00000080,
            WithProcessThreadData = 0x00000100,
            WithPrivateReadWriteMemory = 0x00000200,
            WithoutOptionalData = 0x00000400,
            WithFullMemoryInfo = 0x00000800,
            WithThreadInfo = 0x00001000,
            WithCodeSegs = 0x00002000,
            WithoutAuxiliaryState = 0x00004000,
            WithFullAuxiliaryState = 0x00008000,
            WithPrivateWriteCopyMemory = 0x00010000,
            IgnoreInaccessibleMemory = 0x00020000,
            [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "")]
            ValidTypeFlags = 0x0003ffff,
        };

        */

        private static long GetTotalMemory(string name)
        {
            //var dumpFile = name + ".dmp";
            //var dumpType =
            //    MINIDUMP_TYPE.WithFullMemory |
            //    MINIDUMP_TYPE.WithPrivateReadWriteMemory |
            //    MINIDUMP_TYPE.WithPrivateWriteCopyMemory;

            //if (File.Exists(dumpFile))
            //{
            //    File.Delete(dumpFile);
            //}

            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.Collect(1, GCCollectionMode.Forced, true);
            GC.Collect(2, GCCollectionMode.Forced, true);

            GC.WaitForPendingFinalizers();

            //using (var fs = new FileStream(dumpFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            //{
            //    if (!MiniDumpNativeMethods.MiniDumpWriteDump(Current.Handle,
            //        (uint)Current.Id, fs.SafeFileHandle, dumpType,
            //        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
            //    {
            //        throw new Win32Exception(Marshal.GetLastWin32Error());
            //    }

            //    return new FileInfo(dumpFile).Length;
            //}

            Current.Refresh();
            return Current.WorkingSet64;
            //return GC.GetTotalMemory(true);
        }

        [STAThread]
        static void Main(string[] args)
        {
            bool ret = GC.TryStartNoGCRegion(1024 * 1024 * 100);
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            // CHECK

            var g = GetTotalMemory("a");
            var g2 = GetTotalMemory("b") - g;

            Console.WriteLine(g2);

            // **************************************************

            Console.WriteLine("Native Library Info");
            Console.WriteLine("  Path: " + NeoVM.LibraryPath);
            Console.WriteLine("  Version: " + NeoVM.LibraryVersion);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Memory leak detector");

            var types = typeof(Program).Assembly
                .GetTypes()
                .Where(u => u.GetCustomAttribute<TestClassAttribute>() != null)
                .ToArray();

            foreach (var type in types)
            {
                ExecuteMemoryLeakSearcher(type);
            }
        }

        private static void ExecuteMemoryLeakSearcher(Type type)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Looking for memory leak in: " + type.Name);

            var init = type.GetMethods().Where(u => u.GetCustomAttribute<TestInitializeAttribute>() != null).ToArray();
            var call = type.GetMethods().Where(u => u.GetCustomAttribute<TestMethodAttribute>() != null).ToArray();

            var ob = Activator.CreateInstance(type);

            ExecuteMemoryLeakSearcher(ob, init, call);

            if (ob is IDisposable dsp)
            {
                dsp.Dispose();
            }
        }

        private static void ExecuteMemoryLeakSearcher(object instance, MethodInfo[] inits, MethodInfo[] calls)
        {
            // Execute inits

            foreach (var method in inits)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("     Execute init: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(method.Name);
                Console.ForegroundColor = ConsoleColor.Cyan;

                method.Invoke(instance, null);
            }

            // Execute call

            foreach (var method in calls)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("     Execute call: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(method.Name + " ");
                Console.ForegroundColor = ConsoleColor.Cyan;

                var before = GetTotalMemory("before");

                method.Invoke(instance, null);

                var after = GetTotalMemory("after");
                var result = after - before;
                var color = Console.ForegroundColor;

                if (result != 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result);
                    Console.ForegroundColor = color;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("OK!");
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}