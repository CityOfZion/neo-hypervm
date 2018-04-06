using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_BITWISE_LOGIC : VMOpCodeTest
    {
        [TestMethod]
        public void INVERT()
        {
            InternalTestBigInteger(EVMOpCode.INVERT, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = ~a; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void AND()
        {
            InternalTestBigInteger(EVMOpCode.AND, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a & b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void OR()
        {
            InternalTestBigInteger(EVMOpCode.OR, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a | b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void XOR()
        {
            InternalTestBigInteger(EVMOpCode.XOR, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a ^ b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void EQUAL()
        {
            // Interop equal = true, false, false

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(new ExecutionEngineArgs()
            {
                InteropService = new DummyInteropService(),
                ScriptContainer = new DummyScript(),
            }
                ))
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

                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(3);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(4);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                // Check

                Assert.AreEqual(EVMState.HALT, engine.State);

                CheckClean(engine);
            }

            // Equals map, array and structure (false)

            using (ScriptBuilder script = new ScriptBuilder
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
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(5);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(6);
                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                Assert.AreEqual(EVMState.HALT, engine.State);

                // Check

                CheckClean(engine);
            }

            // Equals map, array and structure (true)

            using (ScriptBuilder script = new ScriptBuilder
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
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                engine.StepInto(3);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(4);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                engine.StepInto(5);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                Assert.AreEqual(EVMState.HALT, engine.State);

                // Check

                CheckClean(engine);
            }

            // Byte array conversion = true

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSHBYTES1, EVMOpCode.PUSHBYTES1 /*0x01*/,
                EVMOpCode.PUSH0,
                EVMOpCode.INC,
                EVMOpCode.EQUAL
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

            // Bool compare = true

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.NOT,
                EVMOpCode.DUP,
                EVMOpCode.EQUAL
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

            // Bool compare = false

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.NOT,
                EVMOpCode.PUSH1,
                EVMOpCode.EQUAL
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsFalse(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }

            // Without push

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.EQUAL
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine, false);
            }

            // Equal ByteArrays

            InternalTestBigInteger(EVMOpCode.EQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = a == b; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }
    }
}