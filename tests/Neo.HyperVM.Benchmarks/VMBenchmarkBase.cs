using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;
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
        #region Neo params

        protected VM.ICrypto _crypto;

        #endregion

        #region HyperVM params

        protected ExecutionEngineArgs _args;
        protected NeoVM _HyperVM;

        [Params(nameof(VMBenchmarkBase))]
        public abstract string Test { get; set; }

        #endregion

        public abstract void HyperVM();

        public abstract void NeoVM();

        public abstract void ApplicationEngine();

        public virtual void Setup()
        {
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