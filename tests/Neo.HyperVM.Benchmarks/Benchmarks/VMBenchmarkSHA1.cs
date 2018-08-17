using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkSHA1 : VMBenchmarkBase
    {
        [Params("PUSHBYTES1+0x01+SHA1+DROP")]
        public override string OpCodes { get; set; }

        [GlobalSetup]
        public override void Setup()
        {
            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < Repetitions; x++)
                {
                    script.Emit(EVMOpCode.PUSHBYTES1);
                    script.Emit(new byte[] { 0x01 });
                    script.Emit(EVMOpCode.SHA1);
                    script.Emit(EVMOpCode.DROP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}