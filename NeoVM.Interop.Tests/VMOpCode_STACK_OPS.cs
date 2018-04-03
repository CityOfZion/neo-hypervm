using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_STACK_OPS : VMOpCodeTest
    {
        [TestMethod]
        public void XDROP()
        {
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

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);

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
        public void TOALTSTACK()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
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

                Assert.IsTrue(engine.AltStack.Pop<ByteArrayStackItem>().Value.Length == 0);

                CheckClean(engine);
            }
        }

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
        public void WITHIN()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH5,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH7,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH0,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.RET
            ))
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
            InternalTestBigInteger(EVMOpCode.NUMEQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a == b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void MAX()
        {
            InternalTestBigInteger(EVMOpCode.MAX, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = BigInteger.Max(a, b); }
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
        public void MIN()
        {
            InternalTestBigInteger(EVMOpCode.MIN, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = BigInteger.Min(a, b); }
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
        public void GTE()
        {
            InternalTestBigInteger(EVMOpCode.GTE, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a >= b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void LTE()
        {
            InternalTestBigInteger(EVMOpCode.LTE, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a <= b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void NZ()
        {
            InternalTestBigInteger(EVMOpCode.NZ, (engine, a, cancel) =>
            {
                bool res;

                try { res = (a != 0); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void GT()
        {
            InternalTestBigInteger(EVMOpCode.GT, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a > b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void LT()
        {
            InternalTestBigInteger(EVMOpCode.LT, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a < b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void NUMNOTEQUAL()
        {
            InternalTestBigInteger(EVMOpCode.NUMNOTEQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a != b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void BOOLAND()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.BOOLAND,
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
        public void RIGHT()
        {
            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.RIGHT);
                script.Emit(EVMOpCode.RET);

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
            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.LEFT);
                script.Emit(EVMOpCode.RET);

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
            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.SUBSTR);
                script.Emit(EVMOpCode.RET);

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
            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x01 });
                script.EmitPush(new byte[] { 0x02, 0x03 });
                script.Emit(EVMOpCode.CAT);
                script.Emit(EVMOpCode.RET);

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
            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x01 });
                script.Emit(EVMOpCode.SIZE);
                script.EmitPush(new byte[] { 0x02, 0x03 });
                script.Emit(EVMOpCode.SIZE);
                script.Emit(EVMOpCode.RET);

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
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.BOOLOR,
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
        public void NOT()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.NOT
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
                EVMOpCode.NOT,
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
        public void NIP()
        {
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
        public void DEPTH()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
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

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }
        }
    }
}