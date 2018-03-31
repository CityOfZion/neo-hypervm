using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_STACK_OPS : VMOpCodeTest
    {
        [TestMethod]
        public void XDROP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.XDROP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ROLL()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.ROLL,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 5);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);

                CheckClean(engine);
            }

            script = new byte[]
                {
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.ROLL,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XSWAP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.XSWAP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XTUCK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.XTUCK,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);

                CheckClean(engine);
            }

            script = new byte[]
                {
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.XTUCK,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TOALTSTACK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.TOALTSTACK,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.AltStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DUP_FROMALTSTACK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.TOALTSTACK,
                    (byte)EVMOpCode.DUPFROMALTSTACK,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.AltStack.Pop<ByteArrayStackItem>().Value.Length == 0);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void WITHIN()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.WITHIN,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.WITHIN,

                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.WITHIN,

                    (byte)EVMOpCode.PUSH7,
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.WITHIN,

                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH1, (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.WITHIN,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NUMEQUAL()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NUMEQUAL,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.NUMEQUAL,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.NUMEQUAL,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void MAX()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.MAX,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.MAX,

                    (byte)EVMOpCode.PUSH4,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.MAX,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 5);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void MIN()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.MIN,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.MIN,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.MIN,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void GTE()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.GTE,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.GTE,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.GTE,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void LTE()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.LTE,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.LTE,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.LTE,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NZ()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NZ,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.NZ,

                    (byte)EVMOpCode.PUSHBYTES1,0x00,
                    (byte)EVMOpCode.NZ,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void GT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.GT,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.GT,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.GT,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void LT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.LT,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.LT,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.LT,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NUMNOTEQUAL()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NUMNOTEQUAL,

                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.NUMNOTEQUAL,

                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.NUMNOTEQUAL,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void BOOLAND()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.BOOLAND,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(!engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PICK,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void RIGHT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+9,
                    0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.RIGHT,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x07, 0x08, 0x09 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void LEFT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+9,
                    0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.LEFT,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x00, 0x01, 0x02 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SUBSTR()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1+9,
                    0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.SUBSTR,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x02, 0x03, 0x04 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void CAT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.PUSHBYTES1+1,0x02,0x03,
                    (byte)EVMOpCode.CAT,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x01, 0x02, 0x03 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SIZE()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.SIZE,
                    (byte)EVMOpCode.PUSHBYTES1+1,0x01,0x02,
                    (byte)EVMOpCode.SIZE,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void BOOLOR()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.BOOLOR,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NOT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TUCK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x02,
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.TUCK,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x02);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x02);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x01);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SWAP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x02,
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.SWAP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x02);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x01);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ROT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x03,
                    (byte)EVMOpCode.PUSHBYTES1,0x02,
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.ROT,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x03);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x01);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x02);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void OVER()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.OVER,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(!engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(!engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NIP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.NIP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DUP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.DUP,
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
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DROP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.DROP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DEPTH()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.DEPTH,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }
    }
}