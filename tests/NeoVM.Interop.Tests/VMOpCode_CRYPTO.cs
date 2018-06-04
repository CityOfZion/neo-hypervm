using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM;
using NeoSharp.VM.Helpers;
using NeoVM.Interop.Tests.Crypto;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_CRYPTO : VMOpCodeTest
    {
        readonly static byte[] ECDSA_PUBLIC_P256_MAGIC = new byte[] { 0x45, 0x43, 0x53, 0x31, 0x20, 0x0, 0x0, 0x0 };
        readonly static byte[] ECDSA_PRIVATE_P256_MAGIC = new byte[] { 0x45, 0x43, 0x53, 0x32, 0x20, 0x0, 0x0, 0x0 };

        const int ECDSA_PUBLIC_P256_MAGIC_LENGTH = 8;

        /// <summary>
        /// Prepare publicKey for Address
        /// </summary>
        /// <param name="publicKey">PublicKey</param>
        static byte[] PreparePublicKey(byte[] publicKey)
        {
            switch (publicKey.Length)
            {
                case 64: return publicKey;
                case 65:
                    {
                        if (publicKey[0] == 0x04)
                            return publicKey.Skip(1).ToArray();

                        throw new ArgumentException("publicKey");
                    }
                case 72:
                    {
                        return publicKey.Skip(ECDSA_PUBLIC_P256_MAGIC_LENGTH).ToArray();
                    }
            }

            throw new ArgumentException("publicKey");
        }

        [TestMethod]
        public void SHA1()
        {
            InternalTestBigInteger(EVMOpCode.SHA1, (engine, a, cancel) =>
            {
                byte[] hash;

                try
                {
                    using (var sha = System.Security.Cryptography.SHA1.Create())
                    {
                        hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
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
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                    {
                        hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
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
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                    using (var ripe = new RIPEMD160Managed())
                    {
                        hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                        hash = ripe.ComputeHash(hash);
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
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
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                    {
                        hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                        hash = sha.ComputeHash(hash);
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(hash));
            });
        }

        [TestMethod]
        public void CHECKSIG()
        {
            // Without push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.CHECKSIG
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x05);

                CheckClean(engine, false);
            }

            // Without get message

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()))
            {
                // Load script

                // signature

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                // publicKey

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                script.Emit(EVMOpCode.CHECKSIG);

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsFalse(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Real message

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                MessageProvider = new DummyMessageProvider(0, BitHelper.FromHexString(
                        "00000000bf4421c88776c53b43ce1dc45463bfd2028e322fdfb60064be150ed3e36125d418f98ec3ed2c2d1c9427385e7b85d0d1a366e29c4e399693a59718380f8bbad6d6d90358010000004490d0bb7170726c59e75d652b5d3827bf04c165bbe9ef95cca4bf55"))
            }))
            {
                // Load script

                // signature

                script.EmitPush(BitHelper.FromHexString(
                    "4e0ebd369e81093866fe29406dbf6b402c003774541799d08bf9bb0fc6070ec0f6bad908ab95f05fa64e682b485800b3c12102a8596e6c715ec76f4564d5eff3"));

                // publicKey

                script.EmitPush(BitHelper.FromHexString(
                    "ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba5543672835e89a5c1f821d773214881e84618770508ce1ddfd488ae377addf7ca38"));

                script.Emit(EVMOpCode.CHECKSIG);

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsFalse(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Without valid push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.CHECKSIG
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsFalse(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Real test

            foreach (var value in new bool[] { true, false, true, true, false, true, true, false, false, false })
            {
                // Get data

                byte[] message = Args.MessageProvider.GetMessage(0);

                // Create Ecdsa

                byte[] publicKey, signature;

                using (var hkey = CngKey.Create(CngAlgorithm.ECDsaP256, null, new CngKeyCreationParameters()
                {
                    KeyCreationOptions = CngKeyCreationOptions.None,
                    KeyUsage = CngKeyUsages.AllUsages,
                    ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowArchiving | CngExportPolicies.AllowPlaintextArchiving | CngExportPolicies.AllowPlaintextExport,
                    Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                    UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                }))
                using (var sign = new ECDsaCng(hkey)
                {
                    HashAlgorithm = CngAlgorithm.Sha256
                })
                {
                    publicKey = PreparePublicKey(hkey.Export(CngKeyBlobFormat.EccPublicBlob));
                    signature = sign.SignData(message);
                }

                // Fake results

                if (!value)
                {
                    signature[0] = (byte)(signature[0] == 0xFF ? 0x00 : signature[0] + 0x01);
                }

                using (var script = new ScriptBuilder())
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    script.EmitPush(signature);
                    script.EmitPush(publicKey);
                    script.Emit(EVMOpCode.CHECKSIG);

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<BooleanStackItem>().Value, value);

                    CheckClean(engine);
                }
            }
        }

        [TestMethod]
        public void VERIFY()
        {
            // Without push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.VERIFY
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x05);

                CheckClean(engine, false);
            }

            // Without valid push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH6,
                EVMOpCode.VERIFY
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsFalse(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine, false);
            }

            // Real test

            foreach (var value in new bool[] { true, false, true, true, false, true, true, false, false, false })
            {
                // Get data

                byte[] message = Args.MessageProvider.GetMessage(0);

                // Create Ecdsa

                byte[] publicKey, signature;

                using (var hkey = CngKey.Create(CngAlgorithm.ECDsaP256, null, new CngKeyCreationParameters()
                {
                    KeyCreationOptions = CngKeyCreationOptions.None,
                    KeyUsage = CngKeyUsages.AllUsages,
                    ExportPolicy = CngExportPolicies.AllowExport | CngExportPolicies.AllowArchiving | CngExportPolicies.AllowPlaintextArchiving | CngExportPolicies.AllowPlaintextExport,
                    Provider = CngProvider.MicrosoftSoftwareKeyStorageProvider,
                    UIPolicy = new CngUIPolicy(CngUIProtectionLevels.None),
                }))
                using (var sign = new ECDsaCng(hkey)
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

                using (var script = new ScriptBuilder())
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    script.EmitPush(message);
                    script.EmitPush(signature);
                    script.EmitPush(publicKey);
                    script.Emit(EVMOpCode.VERIFY);

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<BooleanStackItem>().Value, value);

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