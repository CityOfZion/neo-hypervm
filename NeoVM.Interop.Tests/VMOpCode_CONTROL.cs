﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public void APPCALL_AND_TAILCALL(EVMOpCode opcode)
        {
            Assert.IsTrue(opcode == EVMOpCode.APPCALL || opcode == EVMOpCode.TAILCALL);

            byte[] script = new byte[]
            {
                (byte)opcode,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
            };

            // Check without IScriptTable

            using (ExecutionEngine engine = NeoVM.CreateEngine(new ExecutionEngineArgs()))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Check without complete hash

            using (ExecutionEngine engine = NeoVM.CreateEngine(new ExecutionEngineArgs()))
            {
                // Load script

                engine.LoadScript(script.Take(script.Length - 1).ToArray());

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Check script

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x05);

                CheckClean(engine);
            }

            // Check empty hash without push

            script = new byte[]
                {
                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
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

            // Check empty with wrong push

            script = new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1,
                0x01,

                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
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

            // Check empty with wight push

            script = new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1+19,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,

                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                // PUSH

                engine.StepInto();
                Assert.AreEqual(0, engine.AltStack.Count);
                Assert.AreEqual(1, engine.EvaluationStack.Count);
                Assert.AreEqual(1, engine.InvocationStack.Count);

                if (opcode == EVMOpCode.APPCALL)
                {
                    // APP CALL

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(0, engine.EvaluationStack.Count);
                    Assert.AreEqual(2, engine.InvocationStack.Count);

                    // PUSH 0x05

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(1, engine.EvaluationStack.Count);
                    Assert.AreEqual(2, engine.InvocationStack.Count);

                    // RET 1

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(1, engine.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // RET 2

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(1, engine.EvaluationStack.Count);
                    Assert.AreEqual(0, engine.InvocationStack.Count);
                }
                else
                {
                    // TAIL CALL

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(0, engine.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // PUSH 0x05

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(1, engine.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // RET 1

                    engine.StepInto();
                    Assert.AreEqual(0, engine.AltStack.Count);
                    Assert.AreEqual(1, engine.EvaluationStack.Count);
                    Assert.AreEqual(0, engine.InvocationStack.Count);
                }

                Assert.AreEqual(EVMState.HALT, engine.State);

                // Check

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, 0x05);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void APPCALL()
        {
            APPCALL_AND_TAILCALL(EVMOpCode.APPCALL);
        }

        [TestMethod]
        public void TAILCALL()
        {
            APPCALL_AND_TAILCALL(EVMOpCode.TAILCALL);
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