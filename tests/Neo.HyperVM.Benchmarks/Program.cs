using System;
using BenchmarkDotNet.Running;

namespace Neo.HyperVM.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var c = new VMBenchmarkPUSH1();
            //c.Setup();

            //for (int x = 0; x < 10000; x++)
            //{
            //    c.HyperVM();
            //}

            //return;

            foreach (var type in new Type[] { typeof(VMBenchmarkNOP), typeof(VMBenchmarkPUSH1) })
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