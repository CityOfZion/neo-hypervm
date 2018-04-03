using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Crypto;
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
    }
}