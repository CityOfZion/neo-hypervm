using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM;
using NeoVM.Interop.Tests.Crypto;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System.Linq;
using System.Security.Cryptography;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_CONTROL : VMOpCodeTest
    {
        [TestMethod]
        public void NOP()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.NOP, EVMOpCode.NOP,
                EVMOpCode.NOP, EVMOpCode.NOP,
                EVMOpCode.NOP, EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                for (int x = 0; x < 5; x++)
                {
                    Assert.AreEqual(EVMOpCode.NOP, engine.CurrentContext.NextInstruction);
                    engine.StepInto();
                    Assert.AreEqual(EVMState.None, engine.State);
                }

                Assert.AreEqual(EVMOpCode.RET, engine.CurrentContext.NextInstruction);
                engine.StepInto();
                Assert.AreEqual(EVMState.Halt, engine.State);

                // Check

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void JMP()
        {
            // Jump outside (1)

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.JMP,
                /* x   */ 0x04 , 0x00
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Jump outside (2)

            using (var script = new ScriptBuilder(new byte[]
            {
                /* x─◄ */ (byte)EVMOpCode.JMP,
                /*     */ 0xFF , 0xFF
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Without offset

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.JMP,
                /* x   */ 0x04
            }))
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

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte)EVMOpCode.PUSH0,
                /* ┌─◄ */ (byte)EVMOpCode.JMP,
                /* │   */ 0x04, 0x00,
                /* │   */ (byte)EVMOpCode.RET,
                /* └─► */ (byte)EVMOpCode.NOT,
                /*     */ (byte)EVMOpCode.RET,
            }))
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

        void JMPIF_JMPIFNOT(bool isIf)
        {
            // Jump outside (1)

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte) EVMOpCode.PUSH1,
                /* ┌─◄ */ (byte)(isIf? EVMOpCode.JMPIF : EVMOpCode.JMPIFNOT),
                /* x   */ 0x04 , 0x00
            }))
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

            // Jump outside (2)

            using (var script = new ScriptBuilder(new byte[]
            {
                /* x   */ (byte) EVMOpCode.PUSH1,
                /* └─◄ */ (byte)(isIf? EVMOpCode.JMPIF : EVMOpCode.JMPIFNOT),
                /*     */ 0xFE , 0xFF
            }))
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

            // Without offset

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte) EVMOpCode.PUSH1,
                /* ┌─◄ */ (byte)(isIf? EVMOpCode.JMPIF : EVMOpCode.JMPIFNOT),
                /* x   */ 0x04
            }))
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

            // Without push

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)(isIf? EVMOpCode.JMPIF : EVMOpCode.JMPIFNOT),
                /* x   */ 0x04 , 0x00
            }))
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

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte)EVMOpCode.PUSH1,
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
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 6);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 5);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void JMPIF() { JMPIF_JMPIFNOT(true); }

        [TestMethod]
        public void JMPIFNOT() { JMPIF_JMPIFNOT(false); }

        [TestMethod]
        public void CALL_I()
        {
            // Stack isolation error because NOT is isolated

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte)EVMOpCode.PUSH1,
                /*     */ (byte)EVMOpCode.NOT,
                /* ┌─◄ */ (byte)EVMOpCode.CALL_I,
                /* │   */ 0x00, 0x00, 0x05, 0x00,
                /* │   */ (byte)EVMOpCode.PUSH2,
                /* │   */ (byte)EVMOpCode.RET,
                /* └─► */ (byte)EVMOpCode.NOT,
                /*     */ (byte)EVMOpCode.RET,
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Stack isolation OK

            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte)EVMOpCode.PUSH1,
                /*     */ (byte)EVMOpCode.NOT,
                /* ┌─◄ */ (byte)EVMOpCode.CALL_I,
                /* │   */ 0x01, 0x01, 0x05, 0x00,
                /* │   */ (byte)EVMOpCode.PUSH2,
                /* │   */ (byte)EVMOpCode.RET,
                /* └─► */ (byte)EVMOpCode.NOT,
                /*     */ (byte)EVMOpCode.RET,
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 2);
                Assert.IsTrue(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }

            // Error read 1

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.CALL_I
                /* │   */ 
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Error read 2

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.CALL_I,
                /* │   */ 0x00
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Error jump

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.CALL_I,
                /* │   */ 0x00, 0x00, 0x07, 0x00,
                /* │   */ (byte)EVMOpCode.PUSH0,
                /* x   */ (byte)EVMOpCode.RET,
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void CALL()
        {
            using (var script = new ScriptBuilder(new byte[]
            {
                /*     */ (byte)EVMOpCode.PUSH1,
                /*     */ (byte)EVMOpCode.NOT,
                /* ┌─◄ */ (byte)EVMOpCode.CALL,
                /* │   */ 0x05, 0x00,
                /* │   */ (byte)EVMOpCode.PUSH2,
                /* │   */ (byte)EVMOpCode.RET,
                /* └─► */ (byte)EVMOpCode.NOT,
                /*     */ (byte)EVMOpCode.RET,
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 2);
                Assert.IsTrue(engine.ResultStack.Pop<BooleanStackItem>().Value);

                CheckClean(engine);
            }

            using (var script = new ScriptBuilder(new byte[]
            {
                /* ┌─◄ */ (byte)EVMOpCode.CALL,
                /* │   */ 0x07, 0x00,
                /* │   */ (byte)EVMOpCode.PUSH0,
                /* x   */ (byte)EVMOpCode.RET,
            }))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void RET()
        {
            using (var script = new ScriptBuilder(EVMOpCode.RET))
            using (var engine = CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMOpCode.RET, engine.CurrentContext.NextInstruction);
                engine.StepInto();
                Assert.AreEqual(EVMState.Halt, engine.State);

                // Check

                CheckClean(engine);
            }
        }

        void GLOBAL_CALL_EX(EVMOpCode opcode)
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)opcode, 0x00, 0x01
                }
            ))
            {
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }
            }

            // Without pcount

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)opcode, 0x00
                }
            ))
            {
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }
            }

            // Without rvcount

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)opcode
                }
            ))
            {
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }
            }
        }

        [TestMethod]
        public void CALL_ED()
        {
            // Global tests

            GLOBAL_CALL_EX(EVMOpCode.CALL_ED);

            // Check without IScriptTable

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES20,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
                (byte)EVMOpCode.CALL_ED, 0x01, 0x00
                }
            ))
            {
                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without complete hash

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    byte[] raw = script.ToArray().Skip(1).ToArray();
                    raw[0] = (byte)EVMOpCode.PUSHBYTES19;
                    engine.LoadScript(raw);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without pushes

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    byte[] raw = script.ToArray().ToArray();
                    raw[23] = 1;
                    engine.LoadScript(raw);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    Assert.IsTrue(engine.CurrentContext.EvaluationStack.Pop<ByteArrayStackItem>().Value.SequenceEqual
                        (
                        new byte[]
                            {
                            0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                            0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
                            }
                        ));

                    CheckClean(engine, false);
                }

                // Check script

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x06);

                    CheckClean(engine);
                }

                // Check isolated

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    byte[] raw = script.ToArray();
                    raw[22] = 0x00; // no return
                    engine.LoadScript(raw);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    CheckClean(engine);
                }
            }
        }

        [TestMethod]
        public void CALL_E()
        {
            // Global tests

            GLOBAL_CALL_EX(EVMOpCode.CALL_E);

            // Check without IScriptTable

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.CALL_E, 0x01, 0x00,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
                }
            ))
            {
                // Test cache with the real hash

                byte[] msg = Args.ScriptTable.GetScript(script.ToArray().Skip(3).Take(20).ToArray(), false);

                byte[] realHash;
                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(msg);
                    realHash = ripe.ComputeHash(realHash);
                }

                script.Emit(EVMOpCode.CALL_E);
                script.Emit(0x01, 0x00);
                script.Emit(realHash);

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without complete hash

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script.ToArray().Take(22).ToArray());

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check script

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x04);
                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x04);

                    CheckClean(engine);
                }

                // Check isolated

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    byte[] raw = script.ToArray();
                    raw[1] = 0x00; // no return
                    engine.LoadScript(raw);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x04);

                    CheckClean(engine);
                }
            }
        }

        [TestMethod]
        public void CALL_ET()
        {
            // Global tests

            GLOBAL_CALL_EX(EVMOpCode.CALL_ET);

            // Check without IScriptTable

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.CALL_ET, 0x01, 0x00,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
                }
            ))
            {
                // Test cache with the real hash

                byte[] msg = Args.ScriptTable.GetScript(script.ToArray().Skip(3).Take(20).ToArray(), false);

                byte[] realHash;
                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(msg);
                    realHash = ripe.ComputeHash(realHash);
                }

                script.Emit(EVMOpCode.CALL_ET);
                script.Emit(0x01, 0x00);
                script.Emit(realHash);

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without complete hash

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script.ToArray().Take(22).ToArray());

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Without context

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // TODO: More real tests here
            }
        }

        [TestMethod]
        public void CALL_EDT()
        {
            // Global tests

            GLOBAL_CALL_EX(EVMOpCode.CALL_EDT);

            // Check without IScriptTable

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.CALL_EDT, 0x01, 0x00,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
                }
            ))
            {
                // Test cache with the real hash

                byte[] msg = Args.ScriptTable.GetScript(script.ToArray().Skip(3).Take(20).ToArray(), false);

                byte[] realHash;
                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(msg);
                    realHash = ripe.ComputeHash(realHash);
                }

                script.Emit(EVMOpCode.CALL_EDT);
                script.Emit(0x01, 0x00);
                script.Emit(realHash);

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without complete hash

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script.ToArray().Take(22).ToArray());

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Without context

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // TODO: More real tests here
            }
        }

        public void APPCALL_AND_TAILCALL(EVMOpCode opcode)
        {
            Assert.IsTrue(opcode == EVMOpCode.APPCALL || opcode == EVMOpCode.TAILCALL);

            // Check without IScriptTable

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)opcode,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
                }
            ))
            {
                // Test cache with the real hash

                byte[] msg = Args.ScriptTable.GetScript(script.ToArray().Skip(1).Take(20).ToArray(), false);

                byte[] realHash;
                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(msg);
                    realHash = ripe.ComputeHash(realHash);
                }

                script.Emit(opcode);
                script.Emit(realHash);

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check without complete hash

                using (var engine = CreateEngine(new ExecutionEngineArgs()))
                {
                    // Load script

                    engine.LoadScript(script.ToArray().Take((int)script.Length - 1).ToArray());

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Check script

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x04);

                    if (opcode == EVMOpCode.APPCALL)
                        Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x04);

                    CheckClean(engine);
                }
            }

            // Check empty hash without push

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                }
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

            // Check empty with wrong push

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1,
                0x01,

                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                }
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

            // Check empty with with push

            using (var script = new ScriptBuilder
            (
                new byte[]
                {
                (byte)EVMOpCode.PUSHBYTES1+19,
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,

                (byte)opcode,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00
                }
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                // PUSH

                engine.StepInto();
                Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);
                Assert.AreEqual(1, engine.InvocationStack.Count);

                if (opcode == EVMOpCode.APPCALL)
                {
                    // APP CALL

                    engine.StepInto();
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                    Assert.AreEqual(0, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(2, engine.InvocationStack.Count);

                    // PUSH 0x05

                    engine.StepInto();
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(2, engine.InvocationStack.Count);

                    // RET 1

                    engine.StepInto();
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // RET 2

                    engine.StepInto();
                    Assert.AreEqual(1, engine.ResultStack.Count);
                    Assert.AreEqual(0, engine.InvocationStack.Count);
                }
                else
                {
                    // TAIL CALL

                    engine.StepInto();
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                    Assert.AreEqual(0, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // PUSH 0x05

                    engine.StepInto();
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(1, engine.InvocationStack.Count);

                    // RET 1

                    engine.StepInto();
                    Assert.AreEqual(1, engine.ResultStack.Count);
                    Assert.AreEqual(0, engine.InvocationStack.Count);
                }

                Assert.AreEqual(EVMState.Halt, engine.State);

                // Check

                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x06);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void APPCALL()
        {
            APPCALL_AND_TAILCALL(EVMOpCode.APPCALL);
        }

        [TestMethod]
        public void SYSCALL()
        {
            using (var script = new ScriptBuilder())
            {
                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.EmitRET();

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<InteropStackItem>().Value, Args.MessageProvider);

                    CheckClean(engine);
                }

                // Test FAULT (1)

                byte[] badScript = script.ToArray();
                badScript[1] += 2;

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(badScript);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Test FAULT (2)

                badScript[1] = 0xFD;

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(badScript);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Test FAULT (3)

                badScript[1] = 0xFE;

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(badScript);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }

                // Test FAULT (4)

                badScript[1] = 0xFF;

                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(badScript);

                    // Execute

                    Assert.IsFalse(engine.Execute());

                    // Check

                    CheckClean(engine, false);
                }
            }
        }

        [TestMethod]
        public void TAILCALL()
        {
            APPCALL_AND_TAILCALL(EVMOpCode.TAILCALL);
        }
    }
}