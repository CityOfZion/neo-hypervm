using System.Numerics;
using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkFACTORIAL : VMBenchmarkBase
    {
        [Params("FACTORIAL")]
        public override string OpCodes { get; set; }

        /*
            public class ContractRec : SmartContract
            {
                public static int fact(int f)
                {
                    if (f <= 1) return 1;

                    return f * fact(f - 1);
                }
                public static int Main(int n) // 56
                {
                    return fact(n); // 710998587804863451854045647463724949736497978881168458687447040000000000000
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
                    0x76, 0x6B, 0x00, 0xC3, 0x51, 0xA0, 0x63, 0x08, 0x00, 0x51, 0x61, 0x6C,
                    0x75, 0x66, 0x6C, 0x76, 0x6B, 0x00, 0xC3, 0x6C, 0x76, 0x6B, 0x00, 0xC3,
                    0x51, 0x94, 0x61, 0xE0, 0x01, 0x01, 0xD8, 0xFF, 0x95, 0x61, 0x6C, 0x75,
                    0x66
                };

            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < 1; x++)
                {
                    script.EmitPush(new BigInteger(56));
                    script.Emit(avm);
                    script.Emit(EVMOpCode.DROP);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}