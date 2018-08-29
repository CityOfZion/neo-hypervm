using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
using Neo.SmartContract;
using NeoSharp.VM;
using NeoSharp.VM.Interop;

namespace Neo.HyperVM.Benchmarks
{
    // https://benchmarkdotnet.org/articles/configs/exporters.html#plots

    //[CoreJob]
    //[SimpleJob(3, 2, 10, 1000)]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [RPlotExporter, RankColumn(NumeralSystem.Arabic)]
    public abstract class VMBenchmarkBase
    {
        #region Scripts

        protected byte[] _script;

        #endregion

        #region Neo params

        protected VM.ICrypto _crypto;

        #endregion

        #region HyperVM params

        protected ExecutionEngineArgs _args;
        protected NeoVM _HyperVM;

        [Params(nameof(VMBenchmarkBase))]
        public abstract string OpCodes { get; set; }

        #endregion

        [Benchmark]
        public virtual void HyperVM()
        {
            using (var engine = new NeoSharp.VM.Interop.Types.ExecutionEngine(null))
            {
                engine.LoadScript(_script);
                engine.Execute();
            }
        }

        [Benchmark]
        public virtual void NeoVM()
        {
            using (var engine = new VM.ExecutionEngine(null, _crypto, null, null))
            {
                engine.LoadScript(_script, -1);
                engine.Execute();
            }
        }

        [Benchmark]
        public virtual void ApplicationEngine()
        {
            using (var engine = new ApplicationEngine(TriggerType.Application, null, null, Fixed8.Zero, true))
            {
                engine.LoadScript(_script, -1);
                engine.Execute();
            }
        }

        public virtual void Setup()
        {
            if (!NeoSharp.VM.Interop.NeoVM.IsLoaded)
            {
                NeoSharp.VM.Interop.NeoVM.TryLoadLibrary(NeoSharp.VM.Interop.NeoVM.DefaultLibraryName, out var error);
            }

            _HyperVM = new NeoVM();
            _crypto = Cryptography.Crypto.Default;

            _args = new ExecutionEngineArgs()
            {
                Trigger = ETriggerType.Application,
                InteropService = null,
                Logger = null,
                MessageProvider = null,
                ScriptTable = null
            };
        }
    }
}