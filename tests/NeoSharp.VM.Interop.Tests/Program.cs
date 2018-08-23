using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeoSharp.VM.Interop.Tests
{
    class Program
    {
        readonly static Process Current = Process.GetCurrentProcess();

        private static long GetTotalMemory(string name)
        {
            GC.Collect(0, GCCollectionMode.Forced, true);
            GC.Collect(1, GCCollectionMode.Forced, true);
            GC.Collect(2, GCCollectionMode.Forced, true);

            GC.WaitForPendingFinalizers();

            Current.Refresh();
            return Current.WorkingSet64;
        }

        [STAThread]
        static void Main(string[] args)
        {
            //bool ret = GC.TryStartNoGCRegion(1024 * 1024 * 100);
            //GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            // CHECK

            var g = GetTotalMemory("a");
            var g2 = GetTotalMemory("b") - g;

            Console.WriteLine(g2);

            // **************************************************
            if (!NeoVM.TryLoadLibrary("Neo.HyperVM", out string error))
            {
                Console.WriteLine("Library load error. {0}", error);
                return;
            }
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