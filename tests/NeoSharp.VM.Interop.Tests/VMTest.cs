using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Tests.Crypto;
using NeoSharp.VM.Interop.Tests.Extra;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMTest : VMOpCodeTest
    {
        [TestMethod]
        public void TestScriptHash()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(null))
            {
                // Load script

                engine.LoadScript(script);

                // First check

                Assert.AreEqual(1, engine.InvocationStack.Count);

                // Check

                byte[] realHash;
                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(script);
                    realHash = ripe.ComputeHash(realHash);
                }

                using (var context = engine.EntryContext)
                {
                    Assert.IsTrue(context.ScriptHash.SequenceEqual(realHash));
                }
            }
        }

        [TestMethod]
        public void TestScriptLogic()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.NOT,
                EVMOpCode.DROP
            ))
            using (var engine = CreateEngine(null))
            {
                engine.LoadScript(script);

                using (var context = engine.CurrentContext)
                {
                    engine.StepInto();

                    Assert.IsTrue(context.EvaluationStack.Count == 1);

                    using (var it = context.EvaluationStack.Peek<ByteArrayStackItem>(0))
                    {
                        Assert.IsTrue(it.Value.Length == 0);
                    }

                    engine.StepInto();

                    Assert.IsTrue(context.EvaluationStack.Count == 1);

                    using (var it = context.EvaluationStack.Peek<BooleanStackItem>(0))
                    {
                        Assert.IsTrue(it.Value);
                    }

                    engine.StepInto();

                    Assert.IsTrue(context.EvaluationStack.Count == 1);

                    using (var it = context.EvaluationStack.Peek<BooleanStackItem>(0))
                    {
                        Assert.IsFalse(it.Value);
                    }

                    engine.Execute();
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void TestInit()
        {
            using (var engine = CreateEngine(Args))
            {
                Assert.AreEqual(EVMState.None, engine.State);

                Assert.IsNull(engine.CurrentContext);
                Assert.IsNull(engine.EntryContext);
                Assert.IsNull(engine.CallingContext);

                Assert.AreEqual(Args.InteropService, engine.InteropService);
                Assert.AreEqual(Args.MessageProvider, engine.MessageProvider);
                Assert.AreEqual(Args.ScriptTable, engine.ScriptTable);

                Assert.AreEqual(0, engine.InvocationStack.Count);
                Assert.AreEqual(0, engine.ResultStack.Count);

                Assert.IsTrue(engine.Execute());
            }
        }

        [TestMethod]
        public void TestParallel()
        {
            var args = new ExecutionEngineArgs()
            {
                ScriptTable = new DummyScriptTable
                    (
                    new byte[] { (byte)EVMOpCode.EQUAL },
                    new byte[] { (byte)EVMOpCode.EQUAL }
                    )
            };

            // 5 Scripts

            var engines = new List<IExecutionEngine>()
            {
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args),
                CreateEngine(args)
            };

            Parallel.ForEach(engines, (engine) =>
            {
                using (engine)
                {
                    for (ushort x = 0; x < 1000; x++)
                    {
                        // Load script

                        engine.Clean(x);

                        using (var script = new ScriptBuilder())
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

                        Assert.IsTrue(engine.Execute());

                        // Check result

                        using (var it = engine.ResultStack.Pop<BooleanStackItem>())
                        {
                            Assert.IsTrue(it.Value);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// This is for test logs
        /// </summary>
        [TestMethod]
        public void TestLogs()
        {
            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH10,
                EVMOpCode.TOALTSTACK,
                EVMOpCode.FROMALTSTACK,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                Logger = new ExecutionEngineLogger(ELogVerbosity.StepInto)
            }))
            {
                var stackOp = new StringBuilder();

                engine.Logger.OnStepInto += (context) =>
                {
                    stackOp.AppendLine(context.ToString());
                };

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Test

                Assert.AreEqual(stackOp.ToString().Trim().Replace("\r", ""),
@"[0x41663051791e0cf03178508aea217ca495c24891-000000] PUSH10
[0x41663051791e0cf03178508aea217ca495c24891-000001] TOALTSTACK
[0x41663051791e0cf03178508aea217ca495c24891-000002] FROMALTSTACK
[0x41663051791e0cf03178508aea217ca495c24891-000003] RET".Replace("\r", ""));
            }
        }

        /// <summary>
        /// This test try to prevent double free
        /// </summary>
        [TestMethod]
        public void TestFreeEngineBefore()
        {
            IExecutionContext context;
            IStackItem item;

            using (var script = new ScriptBuilder(EVMOpCode.PUSH1))
            using (var engine = CreateEngine(null))
            {
                // Load script

                engine.LoadScript(script);

                // Compute hash

                engine.StepInto();
                context = engine.CurrentContext;

                Assert.IsTrue(engine.Execute());

                item = engine.ResultStack.Peek();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Check

            item.Dispose();
            context.Dispose();
        }

        /// <summary>
        /// This test try to prevent double free
        /// </summary>
        [TestMethod]
        public void TestDoubleFree()
        {
            IExecutionContext context;

            byte[] realHash;
            using (var script = new ScriptBuilder(EVMOpCode.RET))
            using (var engine = CreateEngine(null))
            {
                // Load script

                engine.LoadScript(script);

                // Compute hash

                using (var sha = SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    realHash = sha.ComputeHash(script);
                    realHash = ripe.ComputeHash(realHash);
                }

                // Get Context

                context = engine.CurrentContext;

                // Create new array

                using (var ar = engine.CreateArray())
                {
                    // Create bool item and free

                    using (var btest = engine.CreateBool(true))
                    {
                        // Append item to array

                        ar.Add(btest);
                    }

                    // Check

                    Assert.IsTrue(ar[0] is BooleanStackItem b0 && b0.Value);
                }

                Assert.IsFalse(context.IsDisposed);
                Assert.AreEqual(context.NextInstruction, EVMOpCode.RET);
            }

            // Check

            Assert.IsFalse(context.IsDisposed);
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
            using (var engine = CreateEngine(null))
            {
                for (int x = 0; x < 2; x++)
                {
                    bool isStruct = x == 1;

                    // First test for array, second for Struct

                    using (var ar = isStruct ? engine.CreateStruct() : engine.CreateArray())
                    {
                        Assert.AreEqual(isStruct, ar.IsStruct);

                        // Create two random integer types

                        foreach (var bi in TestBigIntegers)
                            using (var btest = engine.CreateInteger(bi))
                            {
                                // Check contains

                                Assert.IsFalse(ar.Contains(btest));

                                // Check empty

                                Assert.AreEqual(0, ar.Count);

                                // Add and check count

                                ar.Add(btest);

                                Assert.AreEqual(1, ar.Count);

                                // Check item by position

                                using (var iau = (IntegerStackItem)ar[0])
                                    Assert.AreEqual(iau.Value, btest.Value);

                                // Remove

                                Assert.IsTrue(ar.Remove(btest));
                            }

                        // Clear

                        ar.Clear();
                        Assert.AreEqual(0, ar.Count);

                        // Add bool and check contains

                        using (var bkill = engine.CreateBool(true))
                        {
                            Assert.AreEqual(ar.IndexOf(bkill), -1);

                            ar.Add(bkill);

                            Assert.AreEqual(ar.IndexOf(ar), -1);

                            Assert.IsTrue(ar.Contains(bkill));
                        }

                        // Create new array

                        IArrayStackItem ar2;

                        {
                            var art = new IStackItem[] { engine.CreateBool(true), engine.CreateBool(false) };
                            ar2 = engine.CreateArray(art);
                            foreach (var it in art) it.Dispose();
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
                            var art = new IStackItem[]
                            {
                                engine.CreateInteger(1),
                                engine.CreateInteger(2),
                                engine.CreateInteger(3)
                            };

                            ar.Add(art);

                            foreach (var it in art) it.Dispose();
                        }

                        // Remove 2

                        ar.RemoveAt(1);

                        // Check count

                        Assert.AreEqual(2, ar.Count);

                        // Check values 1 and 3

                        using (var iau = (IntegerStackItem)ar[0])
                            Assert.AreEqual(iau.Value, 1);

                        using (var iau = (IntegerStackItem)ar[1])
                            Assert.AreEqual(iau.Value, 3);

                        // Insert bool

                        using (var inte = engine.CreateBool(true))
                            ar.Insert(1, inte);

                        // Check values

                        using (var iau = (IntegerStackItem)ar[0])
                            Assert.AreEqual(iau.Value, 1);
                        using (var iau = (BooleanStackItem)ar[1])
                            Assert.IsTrue(iau.Value);
                        using (var iau = (IntegerStackItem)ar[2])
                            Assert.AreEqual(iau.Value, 3);
                    }
                }
            }
        }

        /// <summary>
        /// Test array type
        /// </summary>
        [TestMethod]
        public void TestMapStackItem()
        {
            using (var engine = CreateEngine(null))
            {
                // First test for array, second for Struct

                using (var ar = engine.CreateMap())
                {
                    // Clear

                    Assert.AreEqual(0, ar.Count);
                    ar.Clear();
                    Assert.AreEqual(0, ar.Count);

                    // Test Equal key and value

                    using (var b = engine.CreateBool(true))
                    {
                        ar[b] = b;
                    }

                    Assert.AreEqual(1, ar.Count);

                    // Test remove

                    using (var b = engine.CreateBool(true))
                    {
                        Assert.IsTrue(ar.ContainsKey(b));
                        Assert.IsTrue(ar.Remove(b));
                        Assert.IsFalse(ar.Remove(b));
                        Assert.IsFalse(ar.ContainsKey(b));
                    }

                    // Test equal Set and Get

                    using (var b = engine.CreateBool(true))
                    using (var bi = engine.CreateInteger(1))
                    {
                        ar[b] = bi;
                    }

                    Assert.AreEqual(1, ar.Count);

                    using (var b = engine.CreateBool(true))
                    using (var bi = engine.CreateInteger(2))
                    {
                        ar[b] = bi;
                    }

                    Assert.AreEqual(1, ar.Count);

                    using (var b = engine.CreateBool(true))
                    using (var bi = ar[b])
                    {
                        Assert.IsTrue(bi is IntegerStackItem ii && ii.Value == 2);
                    }

                    // Test get keys

                    using (var b = engine.CreateBool(false))
                    using (var bi = engine.CreateInteger(5))
                    {
                        ar[b] = bi;
                    }

                    Assert.AreEqual(2, ar.Count);

                    var keys = new List<bool> { true, false };
                    foreach (var i in ar.Keys)
                    {
                        Assert.IsTrue(i is BooleanStackItem);

                        var bb = (BooleanStackItem)i;
                        keys.Remove(bb.Value);

                        i.Dispose();
                    }

                    Assert.AreEqual(0, keys.Count);

                    // Test get values

                    var values = new List<BigInteger>() { 2, 5 };

                    foreach (var i in ar.Values)
                    {
                        Assert.IsTrue(i is IntegerStackItem);

                        var bb = (IntegerStackItem)i;
                        values.Remove(bb.Value);

                        i.Dispose();
                    }

                    Assert.AreEqual(0, values.Count);

                    // Test get key/values

                    keys = new List<bool> { true, false };
                    values = new List<BigInteger>() { 2, 5 };

                    foreach (var i in ar)
                    {
                        Assert.IsTrue(i.Key is BooleanStackItem);
                        Assert.IsTrue(i.Value is IntegerStackItem);

                        var bb = (BooleanStackItem)i.Key;
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
            using (var engine = CreateEngine(null))
            {
                using (var btrue = engine.CreateBool(true))
                using (var bfalse = engine.CreateBool(false))
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
            using (var engine = CreateEngine(null))
            {
                foreach (var bi in TestBigIntegers)
                    using (var bnat = engine.CreateInteger(bi))
                        Assert.AreEqual(bnat.Value, bi);
            }
        }

        /// <summary>
        /// Test byte array type
        /// </summary>
        [TestMethod]
        public void TestByteArrayStackItem()
        {
            using (var engine = CreateEngine(null))
            {
                using (var bOneByte = engine.CreateByteArray(new byte[] { 0x00, 0x01 }))
                using (var bTwoBytes = engine.CreateByteArray(new byte[] { 0x00, 0x02 }))
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
            using (var engine = CreateEngine(null))
            {
                using (var o1 = new DisposableDummy())
                using (var o2 = new DisposableDummy())
                using (var bInterop1 = engine.CreateInterop(o1))
                using (var bInterop2 = engine.CreateInterop(o2))
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
            using (var engine = CreateEngine(null))
            {
                // Empty stack

                Assert.AreEqual(engine.ResultStack.Count, 0);
                Assert.IsFalse(engine.ResultStack.TryPeek(0, out IStackItem obj));

                // Check integer

                foreach (var bi in TestBigIntegers)
                {
                    using (var it = engine.CreateInteger(bi))
                        CheckItem(engine, it);
                }

                // Check bool

                using (var it = engine.CreateBool(true))
                    CheckItem(engine, it);

                using (var it = engine.CreateBool(false))
                    CheckItem(engine, it);

                // Check ByteArray

                for (int x = 0; x < 100; x++)
                {
                    using (var it = engine.CreateByteArray(new BigInteger(Rand.Next()).ToByteArray()))
                        CheckItem(engine, it);
                }

                // Check Interops

                var dic = new Dictionary<string, string>()
                    {
                        {"key","value" }
                    };

                using (var it = engine.CreateInterop(dic))
                    CheckItem(engine, it);

                // Check disposed object

                {
                    var dis = new DisposableDummy();
                    using (var it = engine.CreateInterop(dis))
                        CheckItem(engine, it);

                    using (var it = engine.CreateInterop(dis))
                        engine.ResultStack.Push(it);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                using (var id = engine.ResultStack.Pop<InteropStackItem>())
                {
                    Assert.IsTrue(id != null && id.Value is DisposableDummy dd && !dd.IsDisposed);
                }
            }
        }

        void CheckItem(IExecutionEngine engine, IStackItem item)
        {
            int c = engine.ResultStack.Count;
            engine.ResultStack.Push(item);
            Assert.AreEqual(engine.ResultStack.Count, c + 1);

            Assert.IsTrue(engine.ResultStack.TryPeek(0, out IStackItem obj));

            // PEEK

            obj.Dispose();
            obj = engine.ResultStack.Peek(0);
            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));

            // POP

            obj.Dispose();
            obj = engine.ResultStack.Pop();
            Assert.AreEqual(engine.ResultStack.Count, c);

            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));
            obj.Dispose();
        }
    }
}