using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.IO;
using System.Linq;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_PUSHES : VMOpCodeTest
    {
        [TestMethod]
        public void PUSHM1()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHM1,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, -1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PUSH0()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PUSHBYTES1_TO_PUSHBYTES75()
        {
            for (int x = 0; x < 75; x++)
                using (MemoryStream script = new MemoryStream())
                {
                    // Generate Script

                    byte[] data = new byte[((byte)EVMOpCode.PUSHBYTES1 + x)];
                    for (byte y = 0; y < data.Length; y++) data[y] = y;

                    script.WriteByte((byte)((byte)EVMOpCode.PUSHBYTES1 + x));
                    script.Write(data, 0, data.Length);

                    // Execute

                    using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                    {
                        // Load script

                        engine.LoadScript(script.ToArray());

                        // Execute

                        Assert.AreEqual(EVMState.HALT, engine.Execute());

                        // Check

                        Assert.AreEqual(1, engine.EvaluationStack.Count);

                        Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(data));

                        CheckClean(engine);
                    }

                    // Try error

                    using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                    {
                        // Load wrong Script

                        engine.LoadScript(script.ToArray().Take((int)script.Length - 1).ToArray());

                        // Execute

                        Assert.AreEqual(EVMState.FAULT, engine.Execute());

                        // Check

                        CheckClean(engine, false);
                    }
                }
        }

        [TestMethod]
        public void PUSH1_TO_PUSH16()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH3, (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH5, (byte)EVMOpCode.PUSH6,
                    (byte)EVMOpCode.PUSH7, (byte)EVMOpCode.PUSH8,
                    (byte)EVMOpCode.PUSH9, (byte)EVMOpCode.PUSH10,
                    (byte)EVMOpCode.PUSH11,(byte)EVMOpCode.PUSH12,
                    (byte)EVMOpCode.PUSH13,(byte)EVMOpCode.PUSH14,
                    (byte)EVMOpCode.PUSH15,(byte)EVMOpCode.PUSH16,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                for (int x = 16; x >= 1; x--)
                {
                    Assert.AreEqual(x, engine.EvaluationStack.Count);
                    Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, x);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PUSHDATA1()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHDATA1,
                    0x04,
                    0x01, 0x02, 0x03, 0x04
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[]
                {
                    0x01,0x02,0x03,0x04
                }));

                CheckClean(engine);
            }

            // Try error

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load wrong Script

                script[1]++;
                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void PUSHDATA2()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHDATA2,
                    0x04, 0x00,
                    0x01, 0x02, 0x03, 0x04
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[]
                {
                    0x01,0x02,0x03,0x04
                }));

                CheckClean(engine);
            }

            // Try error

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load wrong Script

                script[1]++;
                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void PUSHDATA4()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHDATA4,
                    0x04, 0x00, 0x00, 0x00,
                    0x01, 0x02, 0x03, 0x04
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[]
                {
                    0x01,0x02,0x03,0x04
                }));

                CheckClean(engine);
            }

            // Try error

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load wrong Script

                script[1]++;
                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }
    }
}