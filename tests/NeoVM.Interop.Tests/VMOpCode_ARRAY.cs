using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_ARRAY : VMOpCodeTest
    {
        [TestMethod]
        public void ARRAYSIZE()
        {
            // With wrong type

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                // Load script

                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.Emit(EVMOpCode.ARRAYSIZE);
                script.EmitRET();

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Without push

            using (var script = new ScriptBuilder(EVMOpCode.ARRAYSIZE))
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
                EVMOpCode.NEWARRAY,
                EVMOpCode.ARRAYSIZE,

                EVMOpCode.PUSHBYTES1, 0x00,
                EVMOpCode.ARRAYSIZE,

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
        public void PACK()
        {
            // Overflow

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK
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

            // Wrong type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.PACK
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

            using (var script = new ScriptBuilder(EVMOpCode.PACK))
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
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                CheckArrayPop(engine.ResultStack, false, 0x06, 0x05);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void UNPACK()
        {
            // Without array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH10,
                EVMOpCode.UNPACK,
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
                EVMOpCode.UNPACK,
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
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK,
                EVMOpCode.UNPACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 0x02);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 0x06);
                Assert.IsTrue(engine.ResultStack.Pop<IntegerStackItem>().Value == 0x05);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICKITEM()
        {
            // Wrong array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.PICKITEM
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
                EVMOpCode.PICKITEM
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

            // Wrong key type

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH6,
                EVMOpCode.NEWMAP,
                EVMOpCode.PICKITEM,
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
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 6);

                CheckClean(engine, false);
            }

            // Out of bounds

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH2,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.PUSH3,
                    EVMOpCode.PICKITEM,
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

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    // Create array or struct

                    EVMOpCode.PUSH3,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,

                    // Make a copy

                    EVMOpCode.TOALTSTACK,

                    // [0]=1

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH0,
                    EVMOpCode.PUSH1,
                    EVMOpCode.SETITEM,

                    // [1]=2

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH1,
                    EVMOpCode.PUSH2,
                    EVMOpCode.SETITEM,

                    // [2]=3

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PUSH3,
                    EVMOpCode.SETITEM,

                    // Pick

                    EVMOpCode.FROMALTSTACK,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PICKITEM,
                    EVMOpCode.RET
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 3);

                    CheckClean(engine);
                }
        }

        [TestMethod]
        public void SETITEM()
        {
            // Map in key

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.NEWMAP,
                EVMOpCode.PUSH0,
                EVMOpCode.SETITEM
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

            // Without array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.SETITEM
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
                EVMOpCode.SETITEM
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

            // Out of bounds

            foreach (var isStruct in new bool[] { true, false })
            {
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH1,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.PUSH1,
                    EVMOpCode.PUSH5,
                    EVMOpCode.SETITEM,
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
            }

            // Clone test (1)

            foreach (var isStruct in new bool[] { true, false })
            {
                using (var script = new ScriptBuilder
                (
                    // Create new array

                    EVMOpCode.PUSH1,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.DUP,

                    // Init [0]=0x05

                    EVMOpCode.PUSH0,
                    EVMOpCode.PUSH5,
                    EVMOpCode.SETITEM,

                    // Clone

                    EVMOpCode.TOALTSTACK,
                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.DUP,

                    // Set [0]=0x04

                    EVMOpCode.PUSH0,
                    EVMOpCode.PUSH4,
                    EVMOpCode.SETITEM,
                    
                    EVMOpCode.FROMALTSTACK,
                    EVMOpCode.RET
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    engine.StepInto(3);
                    Assert.AreEqual(engine.CurrentContext.AltStack.Count, 0);
                    Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 2);

                    engine.StepInto(3);
                    Assert.AreEqual(engine.CurrentContext.AltStack.Count, 0);
                    Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 1);

                    CheckArrayPeek(engine.CurrentContext.EvaluationStack, 0, isStruct, 0x05);

                    engine.StepInto(3);
                    Assert.AreEqual(engine.CurrentContext.AltStack.Count, 1);
                    Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 2);

                    engine.StepInto(3);
                    Assert.AreEqual(engine.CurrentContext.AltStack.Count, 1);
                    Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 1);

                    engine.StepInto(2);
                    Assert.AreEqual(EVMState.Halt, engine.State);

                    // Check

                    CheckArrayPop(engine.ResultStack, isStruct, 0x04);
                    CheckArrayPop(engine.ResultStack, isStruct, 0x04);

                    CheckClean(engine);
                }
            }

            // Real test

            foreach (var isStruct in new bool[] { true, false })
            {
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH1,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.DUP,
                    EVMOpCode.PUSH0,
                    EVMOpCode.PUSH5,
                    EVMOpCode.SETITEM,
                    EVMOpCode.RET
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    CheckArrayPop(engine.ResultStack, isStruct, 0x05);

                    CheckClean(engine);
                }
            }
        }

        void NEWARRAY_NEWSTRUCT(bool isStruct)
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
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

            // With push (-1)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSHM1,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
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
                EVMOpCode.PUSH2,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                CheckArrayPop(engine.ResultStack, isStruct, false, false);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NEWARRAY() { NEWARRAY_NEWSTRUCT(false); }

        [TestMethod]
        public void NEWSTRUCT() { NEWARRAY_NEWSTRUCT(true); }

        [TestMethod]
        public void NEWMAP()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.NEWMAP,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                Assert.IsTrue(engine.ResultStack.Pop() is MapStackItem);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void APPEND()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.APPEND
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

            // Without array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.APPEND
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

            // Clone test

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    // a = new Array[]{ 0x05 }

                    EVMOpCode.PUSH0,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.DUP,
                    EVMOpCode.PUSH5,
                    EVMOpCode.APPEND,
                    EVMOpCode.TOALTSTACK,

                    // b = new Array[] { }

                    EVMOpCode.PUSH0,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,

                    // b.Append(a)

                    EVMOpCode.DUP,
                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.APPEND,

                    // a.Append(0x06)

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH6,
                    EVMOpCode.APPEND,

                    EVMOpCode.FROMALTSTACK,
                    EVMOpCode.RET
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    engine.StepInto(6);
                    CheckArrayPeek(engine.CurrentContext.AltStack, 0, isStruct, 0x05);
                    engine.StepInto(10);
                    Assert.AreEqual(EVMState.Halt, engine.State);

                    // Check

                    CheckArrayPop(engine.ResultStack, isStruct, 0x05, 0x06);

                    if (isStruct)
                    {
                        using (var ar = engine.ResultStack.Pop<ArrayStackItem>())
                        using (var ar2 = ar[0] as ArrayStackItem)
                        {
                            CheckArray(ar2, isStruct, 0x05);
                        }
                    }
                    else
                    {
                        using (var ar = engine.ResultStack.Pop<ArrayStackItem>())
                        using (var ar2 = ar[0] as ArrayStackItem)
                        {
                            CheckArray(ar2, isStruct, 0x05, 0x06);
                        }
                    }

                    CheckClean(engine);
                }

            // Real test

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH0,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.DUP,
                    EVMOpCode.PUSH5,
                    EVMOpCode.APPEND
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    CheckArrayPop(engine.ResultStack, isStruct, 0x05);

                    CheckClean(engine);
                }
        }

        [TestMethod]
        public void REVERSE()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.REVERSE
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

            // Without Array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH9,
                EVMOpCode.REVERSE
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
                EVMOpCode.PUSH9,
                EVMOpCode.PUSH8,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK,
                EVMOpCode.DUP,
                EVMOpCode.REVERSE
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                CheckArrayPop(engine.ResultStack, false, 0x09, 0x08);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void REMOVE()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.REMOVE
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

            // Without array

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH2,
                EVMOpCode.REMOVE
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

            // Wrong key

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.NEWMAP,
                EVMOpCode.REMOVE
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.InvocationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x02);

                CheckClean(engine, false);
            }

            // Out of bounds

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK,
                EVMOpCode.PUSH2,
                EVMOpCode.REMOVE
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
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH2,
                EVMOpCode.PACK,

                EVMOpCode.TOALTSTACK,
                EVMOpCode.DUPFROMALTSTACK,

                EVMOpCode.PUSH0,
                EVMOpCode.REMOVE,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.UNPACK,
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
                Assert.AreEqual(engine.ResultStack.Pop<IntegerStackItem>().Value, 0x06);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void HASKEY()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.HASKEY
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

            // Wrong type (1)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.NEWMAP,
                EVMOpCode.HASKEY
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Count, 1);
                Assert.AreEqual(engine.CurrentContext.EvaluationStack.Pop<IntegerStackItem>().Value, 0x01);

                CheckClean(engine, false);
            }

            // Wrong type (2)

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1,
                EVMOpCode.HASKEY
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

            // Wrong index (-1)

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH0,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.PUSHM1,
                    EVMOpCode.HASKEY
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

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    EVMOpCode.PUSH1,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.DUP,

                    EVMOpCode.PUSH0,
                    EVMOpCode.HASKEY,
                    EVMOpCode.TOALTSTACK,

                    EVMOpCode.PUSH1,
                    EVMOpCode.HASKEY,
                    EVMOpCode.FROMALTSTACK,
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
                    Assert.IsFalse(engine.ResultStack.Pop<BooleanStackItem>().Value);

                    CheckClean(engine);
                }
        }

        [TestMethod]
        public void KEYS()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.KEYS
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
                EVMOpCode.NEWARRAY,
                EVMOpCode.PUSH0,
                EVMOpCode.KEYS
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
        }

        [TestMethod]
        public void VALUES()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.VALUES
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

            // Wrong item

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                EVMOpCode.VALUES
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

            // Real test - Test clone

            foreach (var isStruct in new bool[] { true, false })
                using (var script = new ScriptBuilder
                (
                    // a= new Array[]{}

                    EVMOpCode.PUSH0,
                    isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                    EVMOpCode.TOALTSTACK,

                    // a.Append(0x01)

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH1,
                    EVMOpCode.APPEND,

                    // a.Append(0x02)

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.PUSH2,
                    EVMOpCode.APPEND,

                    // b=a.ToArray()

                    EVMOpCode.DUPFROMALTSTACK,
                    EVMOpCode.VALUES,
                    EVMOpCode.DUP,

                    // b.RemoveAt(0x00)

                    EVMOpCode.PUSH0,
                    EVMOpCode.REMOVE,
                    EVMOpCode.FROMALTSTACK,

                    EVMOpCode.RET
                ))
                using (var engine = CreateEngine(Args))
                {
                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    Assert.IsTrue(engine.Execute());

                    // Check

                    CheckArrayPop(engine.ResultStack, isStruct, 0x01, 0x02);
                    CheckArrayPop(engine.ResultStack, isStruct, 0x02);

                    CheckClean(engine);
                }
        }
    }
}