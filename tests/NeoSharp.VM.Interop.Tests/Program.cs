using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Tests.Extra;

namespace NeoSharp.VM.Interop.Tests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Native Library Info");
            Console.WriteLine("  Path: " + NeoVM.LibraryPath);
            Console.WriteLine("  Version: " + NeoVM.LibraryVersion);
            Console.WriteLine("");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Memory leak detector");

            var types = typeof(MemoryLeakAssert).Assembly
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

            var before = ConsoleColor.White;

            using (var leakWhole = new MemoryLeakAssert())
            {
                foreach (var method in inits)
                {
                    Console.Write("     Execute init: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(method.Name);
                    Console.ForegroundColor = before;

                    method.Invoke(instance, null);
                }

                // Execute call

                foreach (var method in calls)
                {
                    Console.Write("     Execute call: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(method.Name + " ");
                    Console.ForegroundColor = before;

                    using (var leakInside = new MemoryLeakAssert())
                    {
                        method.Invoke(instance, null);
                    }
                }
            }
        }
    }
}