using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeoVM.Interop.Benchmarks
{
    class Program
    {
        /// <summary>
        /// Allow debug
        /// </summary>
        public class AllowNonOptimized : ManualConfig
        {
            public AllowNonOptimized()
            {
                // ALLOW NON-OPTIMIZED DLLS
                Add(JitOptimizationsValidator.DontFailOnError);
                // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetLoggers().ToArray());
                // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetExporters().ToArray());
                // manual config has no columns by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray());

                KeepBenchmarkFiles = false;
            }
        }

        static void Main(string[] args)
        {
            // Do job

            var summary = BenchmarkRunner.Run<VMBenchmark>(new AllowNonOptimized());
            Console.WriteLine(summary.ToString());

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key for exit");
                Console.ReadKey();
            }
        }
    }
}