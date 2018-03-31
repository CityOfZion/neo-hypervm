using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Crypto;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;
using System.Security.Cryptography;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_CRYPTO : VMOpCodeTest
    {
        [TestMethod]
        public void HASH160()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+1,0x01,0x02,
                    (byte)EVMOpCode.HASH160,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                byte[] hash;
                using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                using (RIPEMD160Managed ripe = new RIPEMD160Managed())
                {
                    hash = sha.ComputeHash(new byte[] { 0x01, 0x02 });
                    hash = ripe.ComputeHash(hash);
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void HASH256()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+1,0x01,0x02,
                    (byte)EVMOpCode.HASH256,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                byte[] hash;
                using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    hash = sha.ComputeHash(new byte[] { 0x01, 0x02 });
                    hash = sha.ComputeHash(hash);
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SHA1()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+1,0x01,0x02,
                    (byte)EVMOpCode.SHA1,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                byte[] hash;
                using (SHA1 sha = System.Security.Cryptography.SHA1.Create())
                {
                    hash = sha.ComputeHash(new byte[] { 0x01, 0x02 });
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SHA256()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+1,0x01,0x02,
                    (byte)EVMOpCode.SHA256,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                byte[] hash;
                using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                {
                    hash = sha.ComputeHash(new byte[] { 0x01, 0x02 });
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));

                CheckClean(engine);
            }
        }
    }
}