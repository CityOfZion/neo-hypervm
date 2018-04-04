using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_STACK : VMOpCodeTest
    {
        [TestMethod]
        public void DUP_FROMALTSTACK()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.DUPFROMALTSTACK,
                EVMOpCode.RET
            ))
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
        public void TOALTSTACK()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.TOALTSTACK,
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
                EVMOpCode.PUSH5,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.AltStack.Pop<IntegerStackItem>().Value, 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void FROMALTSTACK()
        {
            // Without push (on altstack)

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute and Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.EvaluationStack.Count, 0);
                Assert.AreEqual(engine.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.EvaluationStack.Count, 1);
                Assert.AreEqual(engine.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.EvaluationStack.Count, 0);
                Assert.AreEqual(engine.AltStack.Count, 1);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.EvaluationStack.Count, 1);
                Assert.AreEqual(engine.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(EVMState.HALT, engine.State);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XDROP()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.XDROP,
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

            // Overflow drop

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH3,
                EVMOpCode.XDROP,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 3);

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1,
                EVMOpCode.XDROP,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XSWAP()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.XSWAP,
                EVMOpCode.RET
            ))
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
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH4,
                EVMOpCode.PUSH4,
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
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

            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH3,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PUSH1,
                    EVMOpCode.PUSH4,
                    EVMOpCode.PUSH2,
                    EVMOpCode.XTUCK,
                    EVMOpCode.RET
                ))
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
        public void DEPTH()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.DEPTH,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 2);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DROP()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.DROP
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
                EVMOpCode.PUSH0,
                EVMOpCode.DROP,
                EVMOpCode.RET
            ))
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
        public void DUP()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.DUP
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

            // Real Test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.DUP,
                EVMOpCode.RET
            ))
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
        public void NIP()
        {
            // Without two stack items

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.NIP,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 5);

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NIP,
                EVMOpCode.RET
            ))
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
        public void OVER()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NOT,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.OVER,
                EVMOpCode.RET
            ))
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
        public void PICK()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PICK,

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
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ROLL()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH4,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH4,
                EVMOpCode.ROLL,
                EVMOpCode.RET
            ))
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

            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH5,
                    EVMOpCode.PUSH4,
                    EVMOpCode.PUSH3,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PUSH1,
                    EVMOpCode.PUSH2,
                    EVMOpCode.ROLL,
                    EVMOpCode.RET
                ))
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
        public void ROT()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.ROT,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x03);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SWAP()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.SWAP,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TUCK()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.TUCK,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine);
            }
        }
    }
}