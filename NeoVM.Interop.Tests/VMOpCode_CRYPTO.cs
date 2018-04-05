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
        public void SHA1()
        {
            InternalTestBigInteger(EVMOpCode.SHA1, (engine, a, cancel) =>
            {
                byte[] hash;

                try
                {
                    using (SHA1 sha = System.Security.Cryptography.SHA1.Create())
                    {
                        hash = sha.ComputeHash(a.ToByteArray());
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
            });
        }

        [TestMethod]
        public void SHA256()
        {
            InternalTestBigInteger(EVMOpCode.SHA256, (engine, a, cancel) =>
            {
                byte[] hash;

                try
                {
                    using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                    {
                        hash = sha.ComputeHash(a.ToByteArray());
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
            });
        }

        [TestMethod]
        public void HASH160()
        {
            InternalTestBigInteger(EVMOpCode.HASH160, (engine, a, cancel) =>
            {
                byte[] hash;

                try
                {
                    using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                    using (RIPEMD160Managed ripe = new RIPEMD160Managed())
                    {
                        hash = sha.ComputeHash(a.ToByteArray());
                        hash = ripe.ComputeHash(hash);
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
            });
        }

        [TestMethod]
        public void HASH256()
        {
            InternalTestBigInteger(EVMOpCode.HASH256, (engine, a, cancel) =>
            {
                byte[] hash;

                try
                {
                    using (SHA256 sha = System.Security.Cryptography.SHA256.Create())
                    {
                        hash = sha.ComputeHash(a.ToByteArray());
                        hash = sha.ComputeHash(hash);
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
            });
        }

        [TestMethod]
        public void CHECKSIG()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.CHECKSIG
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x05);

                CheckClean(engine, false);
            }

            // Without get message

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(new ExecutionEngineArgs()))
            {
                // Load script

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                script.Emit(EVMOpCode.CHECKSIG);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Without valid push

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.CHECKSIG
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Real test

            foreach (bool value in new bool[] { true, false, true, true, false , true, true, false, false, false })
            {
                // Get data

                byte[] message = Args.ScriptContainer.GetMessage(0);

                // Create Ecdsa

                byte[] publicKey, signature;

                using (CngKey hkey = CngKey.Create(CngAlgorithm.ECDsaP256, null, new CngKeyCreationParameters()
                {
                    KeyCreationOptions = CngKeyCreationOptions.None,
                    KeyUsage = CngKeyUsages.AllUsages,
                    ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowArchiving | CngExportPolicies.AllowPlaintextArchiving | CngExportPolicies.AllowPlaintextExport,
                    Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                    UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                }))
                using (ECDsaCng sign = new ECDsaCng(hkey)
                {
                    HashAlgorithm = CngAlgorithm.Sha256
                })
                {
                    publicKey = hkey.Export(CngKeyBlobFormat.EccPublicBlob);
                    signature = sign.SignData(message);
                }

                // Fake results

                if (!value)
                {
                    signature[0] = (byte)(signature[0] == 0xFF ? 0x00 : signature[0] + 0x01);
                }

                using (ScriptBuilder script = new ScriptBuilder())
                using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                {
                    // Load script

                    script.EmitPush(signature);
                    script.EmitPush(publicKey);
                    script.Emit(EVMOpCode.CHECKSIG);

                    engine.LoadScript(script);

                    // Execute

                    Assert.AreEqual(EVMState.HALT, engine.Execute());

                    // Check

                    Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, value);

                    CheckClean(engine);
                }
            }
        }

        [TestMethod]
        public void CHECKMULTISIG()
        {
            Assert.IsFalse(true);
        }
    }
}