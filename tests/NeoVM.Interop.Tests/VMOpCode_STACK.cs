using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_STACK : VMOpCodeTest
    {
        [TestMethod]
        public void DUPFROMALTSTACK()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.DUPFROMALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.DUPFROMALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.AreEqual(engine.CurrentContext.AltStack.Pop<IntegerStackItem>().Value, 5);

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TOALTSTACK()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.TOALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(2);

                Assert.AreEqual(EVMState.None, engine.State);
                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.AltStack.Pop<IntegerStackItem>().Value, 5);

                Assert.IsTrue(engine.Execute());

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void FROMALTSTACK()
        {
            // Without push (on altstack)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute and Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 0);
                Assert.AreEqual(engine.CurrentContext.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 0);
                Assert.AreEqual(engine.CurrentContext.AltStack.Count, 1);

                engine.StepInto();

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.AltStack.Count, 0);

                engine.StepInto();

                Assert.AreEqual(EVMState.Halt, engine.State);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XDROP()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.XDROP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Overflow drop

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH3,
                EVMOpCode.XDROP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1,
                EVMOpCode.XDROP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XSWAP()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.XSWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Pick outside

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH4,
                EVMOpCode.XSWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.NEWMAP,
                EVMOpCode.XSWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.XSWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void XTUCK()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Pick outside

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH4,
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH4,
                EVMOpCode.PUSH4,
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);

                CheckClean(engine);
            }

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH4,
                EVMOpCode.PUSH2,
                EVMOpCode.XTUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DEPTH()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.DEPTH,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 2);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DROP()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.DROP
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.DROP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void DUP()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.DUP
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real Test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.DUP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.Length == 0);
                Assert.IsTrue(engine.ResultStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NIP()
        {
            // Without two stack items

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.NIP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 5);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NIP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void OVER()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.OVER,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NOT,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.OVER,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(!engine.ResultStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(engine.ResultStack.Pop<BooleanStackItem>().Value);
                Assert.IsTrue(!engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICK()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PICK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Pick outside

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH4,
                EVMOpCode.PICK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.NEWMAP,
                EVMOpCode.PICK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PICK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ROLL()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.ROLL,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.ROLL,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real tests

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH4,
                EVMOpCode.ROLL,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 3);

                CheckClean(engine, false);
            }

            // Real tests

            using (var script = new ScriptBuilder
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
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 5);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);

                CheckClean(engine);
            }

            using (var script = new ScriptBuilder
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
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 3);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 2);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 4);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void ROT()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.ROT,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.ROT,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x03);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x01);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x02);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SWAP()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.SWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.SWAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x02);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TUCK()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.TUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1,
                EVMOpCode.TUCK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x01);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x02);
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine);
            }
        }
    }
}