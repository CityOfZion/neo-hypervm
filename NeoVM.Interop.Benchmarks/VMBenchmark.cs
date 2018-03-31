using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using NeoVM.Interop.Tests;

namespace NeoVM.Interop.Benchmarks
{
    [/*ClrJob(isBaseline: true),*/ CoreJob/*, MonoJob*/]
    [RPlotExporter/*, RankColumn*/]
    public class VMBenchmark
    {
        //[Params(1000, 10000)]
        //public int N;

        VMOpCode_CONTROL vm;

        [GlobalSetup]
        public void Setup()
        {
            vm = new VMOpCode_CONTROL();
        }

        [Benchmark]
        public void RET()
        {
            vm.RET();
        }
    }
}