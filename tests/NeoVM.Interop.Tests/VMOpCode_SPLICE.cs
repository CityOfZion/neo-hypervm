using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_SPLICE : VMOpCodeTest
    {
        [TestMethod]
        public void LEFT()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH11,
                EVMOpCode.LEFT,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 11);

                CheckClean(engine, false);
            }

            // Wrong number

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH4);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.LEFT);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 4);

                CheckClean(engine, false);
            }

            // Overflow string

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH11);
                script.Emit(EVMOpCode.LEFT);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

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
        public void RIGHT()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH11,
                EVMOpCode.RIGHT,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 11);

                CheckClean(engine, false);
            }

            // Wrong number

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH4);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.RIGHT);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 4);

                CheckClean(engine, false);
            }

            // Overflow string

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH11);
                script.Emit(EVMOpCode.RIGHT);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

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
        public void SIZE()
        {
            // Wrong type

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.SIZE,
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

            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.SIZE,
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
        public void SUBSTR()
        {
            // Without 3 PUSH

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.SUBSTR);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 3);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 2);

                CheckClean(engine, false);
            }

            // Not num (1)

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.SUBSTR);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Not num (2)

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.SUBSTR);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 1);

                CheckClean(engine, false);
            }

            // Overflow string

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH9);
                script.Emit(EVMOpCode.SUBSTR);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

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
            // Max limit

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.EmitPush(new byte[1024 * 1024]);
                script.Emit(EVMOpCode.CAT, EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // With wrong types

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NEWMAP,
                EVMOpCode.CAT,
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

            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.CAT,
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
    }
}