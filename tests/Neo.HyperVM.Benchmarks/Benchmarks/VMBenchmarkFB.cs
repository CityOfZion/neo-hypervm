using System.Numerics;
using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkFB : VMBenchmarkBase
    {
        [Params("FIBONACCI")]
        public override string OpCodes { get; set; }

        /*
            public class ContractRec : SmartContract
            {
                public static int fib(int f)
                {
                    if (f <= 1) return f;

                    return fib(f - 1) + fib(f - 2);
                }

                public static int Main(int n) // 21
                {
                    return fib(n); // Output is 10946
                }
            }
        */
        [GlobalSetup]
        public override void Setup()
        {
            byte[] avm = 
                {
                    0x51, 0xC5, 0x6B, 0x6C, 0x76, 0x6B, 0x00, 0x52, 0x7A, 0xC4, 0x6C, 0x76,
                    0x6B, 0x00, 0xC3, 0x61, 0xE0, 0x01, 0x01, 0x07, 0x00, 0x61, 0x6C, 0x75,
                    0x66, 0x51, 0xC5, 0x6B, 0x6C, 0x76, 0x6B, 0x00, 0x52, 0x7A, 0xC4, 0x6C,
                    0x76, 0x6B, 0x00, 0xC3, 0x51, 0xA0, 0x63, 0x0C, 0x00, 0x6C, 0x76, 0x6B,
                    0x00, 0xC3, 0x61, 0x6C, 0x75, 0x66, 0x6C, 0x76, 0x6B, 0x00, 0xC3, 0x51,
                    0x94, 0x61, 0xE0, 0x01, 0x01, 0xD9, 0xFF, 0x6C, 0x76, 0x6B, 0x00, 0xC3,
                    0x52, 0x94, 0x61, 0xE0, 0x01, 0x01, 0xCC, 0xFF, 0x93, 0x61, 0x6C, 0x75,
                    0x66
                };

            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < 1; x++)
                {
                    script.EmitPush(new BigInteger(21));
                    script.Emit(avm);
                    script.Emit(EVMOpCode.DROP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}