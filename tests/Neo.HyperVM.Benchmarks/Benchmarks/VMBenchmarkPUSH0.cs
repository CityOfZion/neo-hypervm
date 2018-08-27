using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkPUSH0 : VMBenchmarkBase
    {
        [Params("PUSH0+DROP")]
        public override string OpCodes { get; set; }

        [GlobalSetup]
        public override void Setup()
        {
            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < Repetitions; x++)
                {
                    script.Emit(EVMOpCode.PUSH0);
                    script.Emit(EVMOpCode.DROP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}