using System;
using BenchmarkDotNet.Running;

namespace Neo.HyperVM.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var c = new VMBenchmarkNOP()
            //{
            //    Repetitions = 1000
            //};
            //c.Setup();

            //for (int x = 0; x < 100; x++)
            //{
            //    c.HyperVM();
            //}

            //return;

            foreach (var type in new Type[]
            {
                typeof(VMBenchmarkFACTORIAL),
                typeof(VMBenchmarkFB),
                typeof(VMBenchmarkVERIFY),
                typeof(VMBenchmarkSHA1),
                typeof(VMBenchmarkNOP),
                typeof(VMBenchmarkPUSH0)
            })
            {
                var summary = BenchmarkRunner.Run(type, new AllowNonOptimized());

                //PlainExporter.Default.ExportToLog(s, new ConsoleLogger(null));
                //PlainExporter.Default.ExportToFiles(s, new ConsoleLogger(null));

                Console.WriteLine(type.Name);
                Console.ReadLine();
            }
        }
    }
}