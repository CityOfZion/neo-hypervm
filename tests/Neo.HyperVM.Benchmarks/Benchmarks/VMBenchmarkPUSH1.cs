using BenchmarkDotNet.Attributes;
using Neo.SmartContract;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkPUSH1 : VMBenchmarkBase
    {
        #region Scripts

        byte[] _script;

        #endregion

        [Params("(PUSH1+DROP)*1000")]
        public override string Test { get; set; }

        [GlobalSetup]
        public override void Setup()
        {
            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < 1000; x++)
                {
                    script.Emit(EVMOpCode.PUSH1);
                    script.Emit(EVMOpCode.DROP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }

        [Benchmark]
        public void HyperVM()
        {
            using (var engine = _HyperVM.Create(null))
            {
                engine.LoadScript(_script);
                engine.Execute();
            }
        }

        [Benchmark]
        public void NeoVM()
        {
            using (var engine = new VM.ExecutionEngine(null, _crypto, null, null))
            {
                engine.LoadScript(_script, -1);
                engine.Execute();
            }
        }

        [Benchmark]
        public void NeoApplicationEngine()
        {
            using (var engine = new ApplicationEngine(TriggerType.Application, null, null, Fixed8.Zero, true))
            {
                engine.LoadScript(_script, -1);
                engine.Execute();
            }
        }
    }
}