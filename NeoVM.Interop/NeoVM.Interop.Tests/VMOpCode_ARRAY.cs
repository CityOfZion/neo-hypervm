using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_ARRAY : VMOpCodeTest
    {
        [TestMethod]
        public void NEWSTRUCT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.NEWSTRUCT,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && arr.IsStruct);
                    Assert.IsTrue(arr.Count == 2);
                    Assert.IsTrue(arr[0] is BooleanStackItem b0 && !b0.Value);
                    Assert.IsTrue(arr[1] is BooleanStackItem b1 && !b1.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ARRAYSIZE()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.NEWARRAY,
                    (byte)EVMOpCode.ARRAYSIZE,

                    (byte)EVMOpCode.PUSHBYTES1,0x00,
                    (byte)EVMOpCode.ARRAYSIZE,

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
        public void NEWARRAY()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.NEWARRAY,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && !arr.IsStruct);
                    Assert.IsTrue(arr.Count == 2);
                    Assert.IsTrue(arr[0] is BooleanStackItem b0 && !b0.Value);
                    Assert.IsTrue(arr[1] is BooleanStackItem b1 && !b1.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PACK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.PUSHBYTES1,0x02,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PACK,

                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Peek<ArrayStackItem>(0))
                {
                    Assert.IsTrue(arr != null);
                    Assert.IsTrue(arr[0] is ByteArrayStackItem b1 && b1.Value[0] == 0x02);
                    Assert.IsTrue(arr[1] is ByteArrayStackItem b2 && b2.Value[0] == 0x01);
                }

                // Remove array and test clean

                engine.EvaluationStack.Pop();

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void UNPACK()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSHBYTES1,0x01,
                    (byte)EVMOpCode.PUSHBYTES1,0x02,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PACK,

                    (byte)EVMOpCode.UNPACK,
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
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x02);
                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value[0] == 0x01);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICKITEM_ARRAY()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PUSH3,
                    (byte)EVMOpCode.PACK,
                    (byte)EVMOpCode.PUSH2,
                    (byte)EVMOpCode.PICKITEM,
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

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SETITEM_ARRAY()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.NEWARRAY,
                    (byte)EVMOpCode.DUP,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.SETITEM,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Count == 1);

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && !arr.IsStruct);
                    Assert.IsTrue(arr.Count == 1);
                    Assert.IsTrue(arr[0] is IntegerStackItem b0 && b0.Value == 5);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SETITEM_STRUCT()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH1,
                    (byte)EVMOpCode.NEWSTRUCT,
                    (byte)EVMOpCode.DUP,
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.PUSH5,
                    (byte)EVMOpCode.SETITEM,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Count == 1);

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && arr.IsStruct);
                    Assert.IsTrue(arr.Count == 1);
                    Assert.IsTrue(arr[0] is IntegerStackItem b0 && b0.Value == 5);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NEWMAP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.NEWMAP,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop() is MapStackItem);

                CheckClean(engine);
            }
        }
    }
}