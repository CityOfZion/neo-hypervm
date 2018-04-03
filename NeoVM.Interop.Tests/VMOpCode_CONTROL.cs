using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;
using System.Text;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_CONTROL : VMOpCodeTest
    {
        [TestMethod]
        public void JUMP_JUMPIF_JMPIFNOT()
        {
            byte[] script = new byte[]
                {
                    /*     */ (byte)EVMOpCode.PUSH0,
                    /* ┌─◄ */ (byte)EVMOpCode.JMP,
                    /* │   */ 0x04, 0x00,
                    /* │   */ (byte)EVMOpCode.RET,
                    /* └─► */ (byte)EVMOpCode.NOT,
                    /* ┌─◄ */ (byte)EVMOpCode.JMPIF,
                    /* │   */ 0x04, 0x00,
                    /* │   */ (byte)EVMOpCode.RET,
                    /* └─► */ (byte)EVMOpCode.PUSH5,
                    /*     */ (byte)EVMOpCode.PUSH0,
                    /* ┌─◄ */ (byte)EVMOpCode.JMPIFNOT,
                    /* │   */ 0x04, 0x00,
                    /* │   */ (byte)EVMOpCode.RET,
                    /* └─► */ (byte)EVMOpCode.PUSH6,
                    /*     */ (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 6);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void CALL()
        {
            byte[] script = new byte[]
                {
                    /*     */ (byte)EVMOpCode.PUSH1,
                    /*     */ (byte)EVMOpCode.NOT,
                    /* ┌─◄ */ (byte)EVMOpCode.CALL,
                    /* │   */ 0x05, 0x00,
                    /* │   */ (byte)EVMOpCode.PUSH2,
                    /* │   */ (byte)EVMOpCode.RET,
                    /* └─► */ (byte)EVMOpCode.NOT,
                    /*     */ (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 2);
                Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }

            script = new byte[]
                {
                    /* ┌─◄ */ (byte)EVMOpCode.CALL,
                    /* │   */ 0x07, 0x00,
                    /* │   */ (byte)EVMOpCode.PUSH0,
                    /* x   */ (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void APPCALL()
        {
            byte[] script = new byte[]
                {
                (byte)EVMOpCode.APPCALL,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x05 }));

                CheckClean(engine);
            }

            script = new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1+19,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,

                (byte)EVMOpCode.APPCALL,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x05 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TAILCALL()
        {
            byte[] script = new byte[]
                {
                (byte)EVMOpCode.TAILCALL,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x05 }));

                CheckClean(engine);
            }

            script = new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1+19,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,

                (byte)EVMOpCode.TAILCALL,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual(new byte[] { 0x05 }));

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NOP()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.NOP,(byte)EVMOpCode.NOP,
                    (byte)EVMOpCode.NOP,(byte)EVMOpCode.NOP,
                    (byte)EVMOpCode.NOP,(byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                for (int x = 0; x < 5; x++)
                {
                    Assert.AreEqual(EVMOpCode.NOP, engine.CurrentContext.NextInstruction);
                    engine.StepInto();
                    Assert.AreEqual(EVMState.NONE, engine.State);
                }

                Assert.AreEqual(EVMOpCode.RET, engine.CurrentContext.NextInstruction);
                engine.StepInto();
                Assert.AreEqual(EVMState.HALT, engine.State);

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void RET()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMOpCode.RET, engine.CurrentContext.NextInstruction);
                engine.StepInto();
                Assert.AreEqual(EVMState.HALT, engine.State);

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SYSCALL()
        {
            string strCall = "System.ExecutionEngine.GetScriptContainer";

            byte[] str = Encoding.ASCII.GetBytes(strCall);
            byte[] script =
                new byte[] { (byte)EVMOpCode.SYSCALL }
                .Concat(new byte[] { (byte)str.Length })
                .Concat(str)
                .Concat(new byte[] { (byte)EVMOpCode.RET })
                .ToArray();

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<InteropStackItem>().Value, Args.ScriptContainer);

                CheckClean(engine);
            }

            // Test FAULT (1)

            script[1] += 2;

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test FAULT (2)

            script[1] = 253;

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }
    }
}