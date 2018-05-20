using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Tests.Crypto;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.Arguments;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMTest : VMOpCodeTest
    {
        [TestMethod]
        public void TestScriptHash()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // Load script

                engine.LoadScript(script);

                // First check

                Assert.AreEqual(1, engine.InvocationStack.Count);

                ExecutionContext context = engine.InvocationStack.Peek(0);

                // Check

                byte[] realHash;
                using (SHA256 sha = SHA256.Create())
                using (RIPEMD160Managed ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(script);
                    realHash = ripe.ComputeHash(realHash);
                }

                Assert.IsTrue(context.ScriptHash.SequenceEqual(realHash));
            }
        }

        [TestMethod]
        public void TestScriptLogic()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NOT,
                EVMOpCode.DROP
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                engine.LoadScript(script);

                engine.StepInto();
                Assert.IsTrue(engine.EvaluationStack.Count == 1);
                Assert.IsTrue(engine.EvaluationStack.Peek<ByteArrayStackItem>(0).Value.Length == 0);
                engine.StepInto();
                Assert.IsTrue(engine.EvaluationStack.Count == 1);
                Assert.IsTrue(engine.EvaluationStack.Peek<BooleanStackItem>(0).Value);
                engine.StepInto();
                Assert.IsTrue(engine.EvaluationStack.Count == 1);
                Assert.IsTrue(!engine.EvaluationStack.Peek<BooleanStackItem>(0).Value);
                engine.Execute();

                Assert.IsTrue(engine.EvaluationStack.Count == 0);
            }
        }

        [TestMethod]
        public void TestInit()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                Assert.AreEqual(EVMState.NONE, engine.State);

                Assert.IsNull(engine.CurrentContext);
                Assert.IsNull(engine.EntryContext);
                Assert.IsNull(engine.CallingContext);

                Assert.AreEqual(Args.InteropService, engine.InteropService);
                Assert.AreEqual(Args.MessageProvider, engine.MessageProvider);
                Assert.AreEqual(Args.ScriptTable, engine.ScriptTable);

                Assert.AreEqual(0, engine.InvocationStack.Count);
                Assert.AreEqual(0, engine.AltStack.Count);
                Assert.AreEqual(0, engine.EvaluationStack.Count);

                Assert.AreEqual(EVMState.HALT, engine.Execute());
            }
        }

        [TestMethod]
        public void TestParallel()
        {
            ExecutionEngineArgs args = new ExecutionEngineArgs()
            {
                ScriptTable = new DummyScriptTable(new byte[] { (byte) EVMOpCode.EQUAL }, new byte[] { (byte)EVMOpCode.EQUAL })
            };

            // 5 Scripts

            List<ExecutionEngine> engines = new List<ExecutionEngine>()
            {
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args),
                NeoVM.CreateEngine(args)
            };

            Parallel.ForEach(engines, (engine) =>
            {
                for (ushort x = 0; x < 1000; x++)
                {
                    // Load script

                    engine.Clean(x);

                    using (ScriptBuilder script = new ScriptBuilder())
                    {
                        script.EmitPush(x);
                        script.EmitPush(x);
                        script.EmitAppCall(new byte[] 
                        {
                            0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                            0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
                        });

                        engine.LoadScript(script);
                    }

                    // Execute

                    Assert.AreEqual(EVMState.HALT, engine.Execute());

                    // Check result

                    Assert.IsTrue(engine.EvaluationStack.Pop<BooleanStackItem>().Value);
                }

                engine.Dispose();
            });
        }

        /// <summary>
        /// This is for test logs
        /// </summary>
        [TestMethod]
        public void TestLogs()
        {
            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH10,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(new ExecutionEngineArgs()
            {
                Logger = new ExecutionEngineLogger(ELogVerbosity.All)
            }))
            {
                StringBuilder stackLog = new StringBuilder();

                engine.Logger.OnExecutionContextChange += (stack, item, index, operation) =>
                {
                    stackLog.AppendLine("EXE:" + operation.ToString() + "[" + index + "]");
                };

                engine.Logger.OnEvaluationStackChange += (stack, item, index, operation) =>
                {
                    stackLog.AppendLine("EVA:" + operation.ToString() + "[" + index + "]");
                };

                engine.Logger.OnAltStackChange += (stack, item, index, operation) =>
                {
                    stackLog.AppendLine("ALT:" + operation.ToString() + "[" + index + "]");
                };

                StringBuilder stackOp = new StringBuilder();

                engine.Logger.OnStepInto += (context) =>
                {
                    stackOp.AppendLine(context.ToString());
                };

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Test

                Assert.AreEqual(stackOp.ToString().Trim().Replace("\r", ""),
@"[0x41663051791e0cf03178508aea217ca495c24891-000000] PUSH10
[0x41663051791e0cf03178508aea217ca495c24891-000001] TOALTSTACK
[0x41663051791e0cf03178508aea217ca495c24891-000002] FROMALTSTACK
[0x41663051791e0cf03178508aea217ca495c24891-000003] RET".Replace("\r", ""));

                Assert.AreEqual(stackLog.ToString().Trim().Replace("\r", ""),
@"EXE:Push[0]
EXE:TryPeek[0]
EVA:Push[0]
EXE:TryPeek[0]
EVA:Pop[0]
ALT:Push[0]
EXE:TryPeek[0]
ALT:Pop[0]
EVA:Push[0]
EXE:TryPeek[0]
EXE:Drop[0]".Replace("\r", ""));

            }
        }

        /// <summary>
        /// This test try to prevent double free
        /// </summary>
        [TestMethod]
        public void TestDoubleFree()
        {
            ExecutionContext context;

            byte[] realHash;
            using (ScriptBuilder script = new ScriptBuilder(EVMOpCode.RET))
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // Load script

                engine.LoadScript(script);

                // Compute hash

                using (SHA256 sha = SHA256.Create())
                using (RIPEMD160Managed ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(script);
                    realHash = ripe.ComputeHash(realHash);
                }

                // Get Context

                context = engine.CurrentContext;

                // Create new array

                using (ArrayStackItem ar = engine.CreateArray())
                {
                    // Create bool item and free

                    using (BooleanStackItem btest = engine.CreateBool(true))
                    {
                        // Apend item to array

                        ar.Add(btest);
                    }

                    // Check

                    Assert.IsTrue(ar[0] is BooleanStackItem b0 && b0.Value);
                }
            }

            // Check

            Assert.AreEqual(context.NextInstruction, EVMOpCode.RET);
            Assert.IsTrue(context.ScriptHash.SequenceEqual(realHash));

            context.Dispose();
        }

        /// <summary>
        /// Test array type
        /// </summary>
        [TestMethod]
        public void TestArrayStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
                for (int x = 0; x < 2; x++)
                {
                    bool isStruct = x == 1;

                    // First test for array, second for Struct

                    using (ArrayStackItem ar = isStruct ? engine.CreateStruct() : engine.CreateArray())
                    {
                        Assert.AreEqual(isStruct, ar.IsStruct);

                        // Create two random integer types

                        foreach (BigInteger bi in TestBigIntegers)
                            using (IntegerStackItem btest = engine.CreateInteger(bi))
                            {
                                // Check contains

                                Assert.IsFalse(ar.Contains(btest));

                                // Check empty

                                Assert.AreEqual(0, ar.Count);
                                // Add and check count

                                ar.Add(btest);

                                Assert.AreEqual(1, ar.Count);

                                // Check item by position

                                using (IntegerStackItem iau = (IntegerStackItem)ar[0])
                                    Assert.AreEqual(iau.Value, btest.Value);

                                // Remove

                                Assert.IsTrue(ar.Remove(btest));
                            }

                        // Clear

                        ar.Clear();
                        Assert.AreEqual(0, ar.Count);

                        // Add bool and check contains

                        using (BooleanStackItem bkill = engine.CreateBool(true))
                        {
                            Assert.AreEqual(ar.IndexOf(bkill), -1);

                            ar.Add(bkill);

                            Assert.AreEqual(ar.IndexOf(ar), -1);

                            Assert.IsTrue(ar.Contains(bkill));
                        }

                        // Create new array

                        ArrayStackItem ar2;

                        {
                            IStackItem[] art = new IStackItem[] { engine.CreateBool(true), engine.CreateBool(false) };
                            ar2 = engine.CreateArray(art);
                            foreach (IStackItem it in art) it.Dispose();
                        }

                        Assert.IsFalse(ar.Contains(ar2));

                        // Replace bool with array

                        ar[0] = ar2;

                        // Check IndexOf

                        Assert.AreEqual(ar.IndexOf(ar2), 0);

                        // Check count

                        Assert.AreEqual(1, ar.Count);

                        // Remove first element

                        ar.RemoveAt(0);

                        // Check count

                        Assert.AreEqual(0, ar.Count);

                        // Add 1,2,3

                        {
                            IStackItem[] art = new IStackItem[] { engine.CreateInteger(1), engine.CreateInteger(2), engine.CreateInteger(3) };
                            ar.Add(art);
                            foreach (IStackItem it in art) it.Dispose();
                        }

                        // Remove 2

                        ar.RemoveAt(1);

                        // Check count

                        Assert.AreEqual(2, ar.Count);

                        // Check values 1 and 3

                        using (IntegerStackItem iau = (IntegerStackItem)ar[0])
                            Assert.AreEqual(iau.Value, 1);

                        using (IntegerStackItem iau = (IntegerStackItem)ar[1])
                            Assert.AreEqual(iau.Value, 3);

                        // Insert bool

                        using (BooleanStackItem inte = engine.CreateBool(true))
                            ar.Insert(1, inte);

                        // Check values

                        using (IntegerStackItem iau = (IntegerStackItem)ar[0])
                            Assert.AreEqual(iau.Value, 1);
                        using (BooleanStackItem iau = (BooleanStackItem)ar[1])
                            Assert.IsTrue(iau.Value);
                        using (IntegerStackItem iau = (IntegerStackItem)ar[2])
                            Assert.AreEqual(iau.Value, 3);
                    }
                }
        }

        /// <summary>
        /// Test array type
        /// </summary>
        [TestMethod]
        public void TestMapStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // First test for array, second for Struct

                using (MapStackItem ar = engine.CreateMap())
                {
                    // Clear

                    Assert.AreEqual(0, ar.Count);
                    ar.Clear();
                    Assert.AreEqual(0, ar.Count);

                    // Test Equal key and value

                    using (BooleanStackItem b = engine.CreateBool(true))
                        ar[b] = b;

                    Assert.AreEqual(1, ar.Count);

                    // Test remove

                    using (BooleanStackItem b = engine.CreateBool(true))
                    {
                        Assert.IsTrue(ar.ContainsKey(b));
                        Assert.IsTrue(ar.Remove(b));
                        Assert.IsFalse(ar.Remove(b));
                        Assert.IsFalse(ar.ContainsKey(b));
                    }

                    // Test equal Set and Get

                    using (BooleanStackItem b = engine.CreateBool(true))
                    using (IntegerStackItem bi = engine.CreateInteger(1))
                        ar[b] = bi;

                    Assert.AreEqual(1, ar.Count);

                    using (BooleanStackItem b = engine.CreateBool(true))
                    using (IntegerStackItem bi = engine.CreateInteger(2))
                        ar[b] = bi;

                    Assert.AreEqual(1, ar.Count);

                    using (BooleanStackItem b = engine.CreateBool(true))
                    using (IStackItem bi = ar[b])
                        Assert.IsTrue(bi is IntegerStackItem ii && ii.Value == 2);

                    // Test get keys

                    using (BooleanStackItem b = engine.CreateBool(false))
                    using (IntegerStackItem bi = engine.CreateInteger(5))
                        ar[b] = bi;

                    Assert.AreEqual(2, ar.Count);

                    List<bool> keys = new List<bool> { true, false };
                    foreach (IStackItem i in ar.Keys)
                    {
                        Assert.IsTrue(i is BooleanStackItem);

                        BooleanStackItem bb = (BooleanStackItem)i;
                        keys.Remove(bb.Value);

                        i.Dispose();
                    }

                    Assert.AreEqual(0, keys.Count);

                    // Test get values

                    List<BigInteger> values = new List<BigInteger>() { 2, 5 };

                    foreach (IStackItem i in ar.Values)
                    {
                        Assert.IsTrue(i is IntegerStackItem);

                        IntegerStackItem bb = (IntegerStackItem)i;
                        values.Remove(bb.Value);

                        i.Dispose();
                    }

                    Assert.AreEqual(0, values.Count);

                    // Test get key/values

                    keys = new List<bool> { true, false };
                    values = new List<BigInteger>() { 2, 5 };

                    foreach (KeyValuePair<IStackItem, IStackItem> i in ar)
                    {
                        Assert.IsTrue(i.Key is BooleanStackItem);
                        Assert.IsTrue(i.Value is IntegerStackItem);

                        BooleanStackItem bb = (BooleanStackItem)i.Key;
                        int ix = keys.IndexOf(bb.Value);

                        keys.RemoveAt(ix);
                        values.RemoveAt(ix);

                        i.Key.Dispose();
                        i.Value.Dispose();
                    }

                    Assert.AreEqual(0, keys.Count);
                    Assert.AreEqual(0, values.Count);
                }
            }
        }

        /// <summary>
        /// Test bool type
        /// </summary>
        [TestMethod]
        public void TestBoolStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                using (BooleanStackItem btrue = engine.CreateBool(true))
                using (BooleanStackItem bfalse = engine.CreateBool(false))
                {
                    Assert.IsTrue(btrue.Value);
                    Assert.IsFalse(bfalse.Value);
                }
            }
        }

        /// <summary>
        /// Test integer type
        /// </summary>
        [TestMethod]
        public void TestIntegerStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                foreach (BigInteger bi in TestBigIntegers)
                    using (IntegerStackItem bnat = engine.CreateInteger(bi))
                        Assert.AreEqual(bnat.Value, bi);
            }
        }

        /// <summary>
        /// Test byte array type
        /// </summary>
        [TestMethod]
        public void TestByteArrayStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                using (ByteArrayStackItem bOneByte = engine.CreateByteArray(new byte[] { 0x00, 0x01 }))
                using (ByteArrayStackItem bTwoBytes = engine.CreateByteArray(new byte[] { 0x00, 0x02 }))
                {
                    Assert.IsTrue(bOneByte.Value.SequenceEqual(new byte[] { 0x00, 0x01 }));
                    Assert.IsTrue(bTwoBytes.Value.SequenceEqual(new byte[] { 0x00, 0x02 }));
                }
            }
        }

        /// <summary>
        /// Test interop type
        /// </summary>
        [TestMethod]
        public void TestInteropStackItem()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                using (DisposableDummy o1 = new DisposableDummy())
                using (DisposableDummy o2 = new DisposableDummy())
                using (InteropStackItem bInterop1 = engine.CreateInterop(o1))
                using (InteropStackItem bInterop2 = engine.CreateInterop(o2))
                {
                    Assert.AreEqual(bInterop1.Value, o1);
                    Assert.AreEqual(bInterop2.Value, o2);

                    Assert.IsFalse(o1.IsDisposed);
                    Assert.IsFalse(o2.IsDisposed);
                }
            }
        }

        [TestMethod]
        public void TestStacksEngine()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // Empty stack

                Assert.AreEqual(engine.AltStack.Count, 0);
                Assert.IsFalse(engine.AltStack.TryPeek(0, out IStackItem obj));

                // Check integer

                foreach (BigInteger bi in TestBigIntegers)
                {
                    using (IntegerStackItem it = engine.CreateInteger(bi))
                        CheckItem(engine, it);
                }

                // Check bool

                using (BooleanStackItem it = engine.CreateBool(true))
                    CheckItem(engine, it);

                using (BooleanStackItem it = engine.CreateBool(false))
                    CheckItem(engine, it);


                // Check ByteArray

                for (int x = 0; x < 100; x++)
                {
                    using (ByteArrayStackItem it = engine.CreateByteArray(new BigInteger(Rand.Next()).ToByteArray()))
                        CheckItem(engine, it);
                }

                // Check Interops

                Dictionary<string, string> dic = new Dictionary<string, string>()
                    {
                        {"key","value" }
                    };

                using (InteropStackItem it = engine.CreateInterop(dic))
                    CheckItem(engine, it);

                // Check disposed object

                {
                    DisposableDummy dis = new DisposableDummy();
                    using (InteropStackItem it = engine.CreateInterop(dis))
                        CheckItem(engine, it);

                    using (InteropStackItem it = engine.CreateInterop(dis))
                        engine.AltStack.Push(it);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                using (InteropStackItem id = engine.AltStack.Pop<InteropStackItem>())
                    Assert.IsTrue(id != null && id.Value is DisposableDummy dd && !dd.IsDisposed);
            }
        }

        void CheckItem(ExecutionEngine engine, IStackItem item)
        {
            int c = engine.AltStack.Count;
            engine.AltStack.Push(item);
            Assert.AreEqual(engine.AltStack.Count, c + 1);

            Assert.IsTrue(engine.AltStack.TryPeek(0, out IStackItem obj));

            // PEEK

            obj.Dispose();
            obj = engine.AltStack.Peek(0);
            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));

            // POP

            obj.Dispose();
            obj = engine.AltStack.Pop();
            Assert.AreEqual(engine.AltStack.Count, c);

            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));
            obj.Dispose();
        }
    }
}