using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Tests.Crypto;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

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
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
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
        public void TestPushOnly()
        {
            using (ScriptBuilder script = new ScriptBuilder(EVMOpCode.SYSCALL))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadPushOnlyScript(script);

                // Call

                Assert.AreEqual(EVMState.FAULT, engine.Execute());
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
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
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
                Assert.AreEqual(Args.ScriptContainer, engine.ScriptContainer);
                Assert.AreEqual(Args.ScriptTable, engine.ScriptTable);

                Assert.AreEqual(0, engine.InvocationStack.Count);
                Assert.AreEqual(0, engine.AltStack.Count);
                Assert.AreEqual(0, engine.EvaluationStack.Count);

                Assert.AreEqual(EVMState.HALT, engine.Execute());
            }
        }

        /// <summary>
        /// This test try to prevent double free
        /// </summary>
        [TestMethod]
        public void TestDoubleFree()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // Create new array

                using (ArrayStackItem ar = engine.CreateArray())

                // Create bool item

                using (BooleanStackItem btest = engine.CreateBool(true))

                    // Apend item to array

                    ar.Add(btest);
            }
        }
        /// <summary>
        /// Test array type
        /// </summary>
        [TestMethod]
        public void TestArrayItem()
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

                                Assert.AreEqual(((IntegerStackItem)ar[0]).Value, btest.Value);

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

                        Assert.AreEqual(((IntegerStackItem)ar[0]).Value, 1);
                        Assert.AreEqual(((IntegerStackItem)ar[1]).Value, 3);

                        // Insert 2 again

                        using (IntegerStackItem inte = engine.CreateInteger(2))
                            ar.Insert(1, inte);

                        // Check values

                        Assert.AreEqual(((IntegerStackItem)ar[0]).Value, 1);
                        Assert.AreEqual(((IntegerStackItem)ar[1]).Value, 2);
                        Assert.AreEqual(((IntegerStackItem)ar[2]).Value, 3);
                    }
                }
        }

        [TestMethod]
        public void TestNativeStackItems()
        {
            using (ExecutionEngine engine = NeoVM.CreateEngine(null))
            {
                // Bool

                using (BooleanStackItem btrue = engine.CreateBool(true))
                using (BooleanStackItem bfalse = engine.CreateBool(false))
                {
                    Assert.IsTrue(btrue.Value);
                    Assert.IsFalse(bfalse.Value);
                }

                // Integer

                foreach (BigInteger bi in TestBigIntegers)
                    using (IntegerStackItem bnat = engine.CreateInteger(bi))
                        Assert.AreEqual(bnat.Value, bi);

                // ByteArray

                using (ByteArrayStackItem bOneByte = engine.CreateByteArray(new byte[] { 0x00, 0x01 }))
                using (ByteArrayStackItem bTwoBytes = engine.CreateByteArray(new byte[] { 0x00, 0x02 }))
                {
                    Assert.IsTrue(bOneByte.Value.SequenceEqual(new byte[] { 0x00, 0x01 }));
                    Assert.IsTrue(bTwoBytes.Value.SequenceEqual(new byte[] { 0x00, 0x02 }));
                }

                // Interop

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

                foreach (BigInteger bi in TestBigIntegers)
                {
                    CheckItem(engine, engine.CreateInteger(bi));
                }

                // Push bool

                CheckItem(engine, engine.CreateBool(true));
                CheckItem(engine, engine.CreateBool(false));

                // Push integer and ByteArray

                for (int x = 0; x < 100; x++)
                {
                    CheckItem(engine, engine.CreateInteger(Rand.Next()));
                    CheckItem(engine, engine.CreateByteArray(new BigInteger(Rand.Next()).ToByteArray()));
                }

                // Interop
                Dictionary<string, string> dic = new Dictionary<string, string>()
                    {
                        {"key","value" }
                    };

                CheckItem(engine, engine.CreateInterop(dic));

                // Check disposed object

                {
                    DisposableDummy dis = new DisposableDummy();
                    CheckItem(engine, engine.CreateInterop(dis));
                    engine.AltStack.Push(engine.CreateInterop(dis));
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();

                InteropStackItem id = engine.AltStack.Pop<InteropStackItem>();
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

            obj = engine.AltStack.Peek(0);
            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));

            // POP

            obj = engine.AltStack.Pop();
            Assert.AreEqual(engine.AltStack.Count, c);

            Assert.IsNotNull(obj);

            Assert.AreEqual(obj.Type, item.Type);
            Assert.IsTrue(obj.Equals(item));
        }
    }
}