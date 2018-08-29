using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkNOP : VMBenchmarkBase
    {
        [Params("NOP*1K")]
        public override string OpCodes { get; set; }

        [GlobalSetup]
        public override void Setup()
        {
            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < 1000; x++)
                {
                    script.Emit(EVMOpCode.NOP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}