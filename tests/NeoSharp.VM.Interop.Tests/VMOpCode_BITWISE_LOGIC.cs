using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Tests.Extra;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_BITWISE_LOGIC : VMOpCodeTest
    {
        [TestMethod]
        public void INVERT()
        {
            InternalTestBigInteger(EVMOpCode.INVERT, (a) =>
            {
                return ~a;
            });
        }

        [TestMethod]
        public void AND()
        {
            InternalTestBigInteger(EVMOpCode.AND, (pair) =>
            {
                return pair.A & pair.B;
            });
        }

        [TestMethod]
        public void OR()
        {
            InternalTestBigInteger(EVMOpCode.OR, (pair) =>
            {
                return pair.A | pair.B;
            });
        }

        [TestMethod]
        public void XOR()
        {
            InternalTestBigInteger(EVMOpCode.XOR, (pair) =>
            {
                return pair.A ^ pair.B;
            });
        }

        [TestMethod]
        public void EQUAL()
        {
            // Interop equal = true, false, false

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                InteropService = new DummyInteropService(),
                MessageProvider = new DummyMessageProvider(),
            }))
            {
                // Load script

                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.Emit(EVMOpCode.EQUAL);

                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.EmitSysCall("Test");
                script.Emit(EVMOpCode.EQUAL);

                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.Emit(EVMOpCode.PUSH11);
                script.Emit(EVMOpCode.EQUAL);

                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                engine.StepInto(3);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                engine.StepInto(3);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                // Check

                engine.StepInto();
                Assert.AreEqual(EVMState.Halt, engine.State);

                CheckClean(engine);
            }

            // Equals map, array and structure (false,false,true)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.NEWMAP,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH2,
                EVMOpCode.NEWSTRUCT,
                EVMOpCode.PUSH2,
                EVMOpCode.NEWSTRUCT,
                EVMOpCode.EQUAL,

                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                engine.StepInto(5);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                engine.StepInto(5);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                engine.StepInto();
                Assert.AreEqual(EVMState.Halt, engine.State);

                // Check

                CheckClean(engine);
            }

            // Equals map, array and structure (true)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.DUP,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                EVMOpCode.DUP,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH2,
                EVMOpCode.NEWSTRUCT,
                EVMOpCode.DUP,
                EVMOpCode.EQUAL,

                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                engine.StepInto(4);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                engine.StepInto(4);
                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                engine.StepInto();
                Assert.AreEqual(EVMState.Halt, engine.State);

                // Check

                CheckClean(engine);
            }

            // Byte array conversions

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSHBYTES1, EVMOpCode.PUSHBYTES1 /*0x01*/,
                EVMOpCode.PUSH0,
                EVMOpCode.INC,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.PUSHBYTES1, EVMOpCode.PUSHBYTES1 /*0x01*/,
                EVMOpCode.EQUAL,

                EVMOpCode.PUSH1,
                EVMOpCode.NOT,
                EVMOpCode.PUSHBYTES1, EVMOpCode.PUSHBYTES1 /*0x00*/,
                EVMOpCode.EQUAL
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                CheckClean(engine);
            }

            // Bool compare = true

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.NOT,
                EVMOpCode.DUP,
                EVMOpCode.EQUAL
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(it.Value);
                }

                CheckClean(engine);
            }

            // Bool compare = false

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.NOT,
                EVMOpCode.PUSH1,
                EVMOpCode.EQUAL
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(it.Value);
                }

                CheckClean(engine);
            }

            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.EQUAL
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var context = engine.CurrentContext)
                using (var it = context.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(0x01, it.Value);
                }

                CheckClean(engine, false);
            }
            
            // Equal ByteArrays

            InternalTestBigInteger(EVMOpCode.EQUAL, (pair) =>
            {
                return pair.A == pair.B;
            });
        }
    }
}