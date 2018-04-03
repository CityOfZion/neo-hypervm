using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_ARRAY : VMOpCodeTest
    {
        void NEWARRAY_NEWSTRUCT(bool isStruct)
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // With push (-1)

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSHM1,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && arr.IsStruct == isStruct);
                    Assert.IsTrue(arr.Count == 2);
                    Assert.IsTrue(arr[0] is BooleanStackItem b0 && !b0.Value);
                    Assert.IsTrue(arr[1] is BooleanStackItem b1 && !b1.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NEWSTRUCT() { NEWARRAY_NEWSTRUCT(true); }

        [TestMethod]
        public void NEWARRAY() { NEWARRAY_NEWSTRUCT(false); }

        [TestMethod]
        public void NEWMAP()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.NEWMAP,
                       EVMOpCode.RET
                   ))
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

        [TestMethod]
        public void ARRAYSIZE()
        {
            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH3,
                    EVMOpCode.NEWARRAY,
                    EVMOpCode.ARRAYSIZE,

                    EVMOpCode.PUSHBYTES1, 0x00,
                    EVMOpCode.ARRAYSIZE,

                    EVMOpCode.RET
                ))
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
        public void PACK()
        {
            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH5,
                    EVMOpCode.PUSH6,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PACK,
                    EVMOpCode.RET
                ))
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
                    Assert.IsTrue(arr[0] is IntegerStackItem b1 && b1.Value == 0x06);
                    Assert.IsTrue(arr[1] is IntegerStackItem b2 && b2.Value == 0x05);
                }

                // Remove array and test clean

                engine.EvaluationStack.Pop();

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void UNPACK()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH5,
                       EVMOpCode.PUSH6,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PACK,
                       EVMOpCode.UNPACK,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 0x06);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 0x05);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICKITEM_ARRAY()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PUSH3,
                       EVMOpCode.PUSH3,
                       EVMOpCode.PACK,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PICKITEM,
                       EVMOpCode.RET
                   ))
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
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.NEWARRAY,
                       EVMOpCode.DUP,
                       EVMOpCode.PUSH0,
                       EVMOpCode.PUSH5,
                       EVMOpCode.SETITEM,
                       EVMOpCode.RET
                   ))
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
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.NEWSTRUCT,
                       EVMOpCode.DUP,
                       EVMOpCode.PUSH0,
                       EVMOpCode.PUSH5,
                       EVMOpCode.SETITEM,
                       EVMOpCode.RET
                   ))
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
    }
}