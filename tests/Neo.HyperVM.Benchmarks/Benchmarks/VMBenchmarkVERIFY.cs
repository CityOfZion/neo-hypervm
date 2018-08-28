using BenchmarkDotNet.Attributes;
using NeoSharp.VM;

namespace Neo.HyperVM.Benchmarks
{
    public class VMBenchmarkVERIFY : VMBenchmarkBase
    {
        [Params("VERIFY*1K")]
        public override string OpCodes { get; set; }

        [GlobalSetup]
        public override void Setup()
        {
            var message = "00000000ea5029691bd94d9667cb32bf136cbba38cf9eb5978bd1d0bf825a3f8a80be6af157aee574e343ff867f3c470ffeecd77312bed61195ba8f1c6588fd275257f60ef6b0458d6070000a36a49f800ef916159e75d652b5d3827bf04c165bbe9ef95cca4bf55".HexToBytes();
            var signature = "95083c5c98cdacdaf57af61104b68940cd0f7cae59b907ddea7f77ae1c4884348321ab62e65eabd82876e2e5f58f822538633521307be831a260ecab2cc5d16c".HexToBytes();
            var publicKey = "03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a".HexToBytes();

            using (var script = new ScriptBuilder())
            {
                for (int x = 0; x < 1000; x++)
                {
                    // Message

                    script.EmitPush(message);

                    // Signature

                    script.EmitPush(signature);

                    // PublicKey

                    script.EmitPush(publicKey);

                    // Verify

                    script.Emit(EVMOpCode.VERIFY);
                    script.Emit(EVMOpCode.THROWIFNOT);
                }

                _script = script.ToArray();
            }

            base.Setup();
        }
    }
}