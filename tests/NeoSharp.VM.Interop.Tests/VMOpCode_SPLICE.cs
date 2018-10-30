using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_SPLICE : VMOpCodeTest
    {
        [TestMethod]
        public void LEFT()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH11,
                EVMOpCode.LEFT,
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

                using (var currentContext = engine.CurrentContext)
                using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(11, it.Value);
                }

                CheckClean(engine, false);
            }

            // Wrong number

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH4);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.LEFT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var currentContext = engine.CurrentContext)
                using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(4, it.Value);
                }

                CheckClean(engine, false);
            }

            // Overflow string

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH11);
                script.Emit(EVMOpCode.LEFT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.LEFT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<ByteArrayStackItem>())
                {
                    Assert.IsTrue(it.Value.SequenceEqual(new byte[] { 0x00, 0x01, 0x02 }));
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void RIGHT()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH11,
                EVMOpCode.RIGHT,
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

                using (var currentContext = engine.CurrentContext)
                using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(11, it.Value);
                }

                CheckClean(engine, false);
            }

            // Wrong number

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH4);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.RIGHT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var currentContext = engine.CurrentContext)
                using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(4, it.Value);
                }

                CheckClean(engine, false);
            }

            // Overflow string

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH11);
                script.Emit(EVMOpCode.RIGHT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.RIGHT);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<ByteArrayStackItem>())
                {
                    Assert.IsTrue(it.Value.SequenceEqual(new byte[] { 0x07, 0x08, 0x09 }));
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SIZE()
        {
            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.SIZE,
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

            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.SIZE,
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

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x01 });
                script.Emit(EVMOpCode.SIZE);
                script.EmitPush(new byte[] { 0x02, 0x03 });
                script.Emit(EVMOpCode.SIZE);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(2, it.Value);
                }

                using (var it = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(1, it.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SUBSTR()
        {
            // Without 3 PUSH

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.SUBSTR);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var currentContext = engine.CurrentContext)
                {
                    using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(3, it.Value);
                    }

                    using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(2, it.Value);
                    }
                }

                CheckClean(engine, false);
            }

            // Not num (1)

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.SUBSTR);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var currentContext = engine.CurrentContext)
                {
                    using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(2, it.Value);
                    }

                    using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(1, it.Value);
                    }
                }

                CheckClean(engine, false);
            }

            // Not num (2)

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.Emit(EVMOpCode.NEWMAP);
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.SUBSTR);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);

                using (var currentContext = engine.CurrentContext)
                using (var it = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(1, it.Value);
                }

                CheckClean(engine, false);
            }

            // Overflow string

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH9);
                script.Emit(EVMOpCode.SUBSTR);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 });
                script.Emit(EVMOpCode.PUSH2);
                script.Emit(EVMOpCode.PUSH3);
                script.Emit(EVMOpCode.SUBSTR);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<ByteArrayStackItem>())
                {
                    Assert.IsTrue(it.Value.SequenceEqual(new byte[] { 0x02, 0x03, 0x04 }));
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void CAT()
        {
            // Max limit

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.Emit(EVMOpCode.PUSH1);
                script.EmitPush(new byte[1024 * 1024]);
                script.Emit(EVMOpCode.CAT, EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // With wrong types

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NEWMAP,
                EVMOpCode.CAT,
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

            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.CAT,
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

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitPush(new byte[] { 0x01 });
                script.EmitPush(new byte[] { 0x02, 0x03 });
                script.Emit(EVMOpCode.CAT);

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var it = engine.ResultStack.Pop<ByteArrayStackItem>())
                {
                    Assert.IsTrue(it.Value.SequenceEqual(new byte[] { 0x01, 0x02, 0x03 }));
                }

                CheckClean(engine);
            }
        }
    }
}