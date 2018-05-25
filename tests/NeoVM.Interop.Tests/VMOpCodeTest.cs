using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.Arguments;
using NeoVM.Interop.Types.Collections;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    public class VMOpCodeTest
    {
        const bool CalculateNumericalTimes = false;

        [TestInitialize]
        public void TestInitialize()
        {
            Console.WriteLine("Native Library Info");
            Console.WriteLine("  Path: " + NeoVM.LibraryPath);
            Console.WriteLine("  Version: " + NeoVM.LibraryVersion);
            Console.WriteLine("");
        }

        /// <summary>
        /// Rand
        /// </summary>
        public readonly Random Rand = new Random();

        /// <summary>
        /// Test BigInteger array
        /// </summary>
        public readonly BigInteger[] TestBigIntegers = new BigInteger[]
        {
            new BigInteger(long.MinValue)*new BigInteger(long.MinValue)*new BigInteger(long.MinValue),
            new BigInteger(ulong.MaxValue)*new BigInteger(ulong.MaxValue)*new BigInteger(ulong.MaxValue),
            new BigInteger(long.MinValue)*new BigInteger(long.MinValue),
            new BigInteger(ulong.MaxValue)*new BigInteger(ulong.MaxValue),

            new BigInteger(ulong.MaxValue),
            new BigInteger(long.MaxValue),
            new BigInteger(long.MinValue),

            new BigInteger(uint.MaxValue),
            new BigInteger(int.MaxValue),
            new BigInteger(int.MinValue),

            new BigInteger(ushort.MaxValue),
            new BigInteger(short.MaxValue),
            new BigInteger(short.MinValue),

            new BigInteger(sbyte.MaxValue),
            new BigInteger(sbyte.MinValue),
            new BigInteger(byte.MaxValue),

            BigInteger.MinusOne,
            BigInteger.One,
            BigInteger.Zero,
        };
        /// <summary>
        /// Regular arguments
        /// </summary>
        protected readonly ExecutionEngineArgs Args = new ExecutionEngineArgs()
        {
            MessageProvider = new DummyMessageProvider(),
            InteropService = new InteropService(),
            ScriptTable = new DummyScriptTable()
        };

        /// <summary>
        /// Compute distinct BigIntegers
        /// </summary>
        /// <returns>BigIntegerPair</returns>
        protected IEnumerable<BigInteger> IntSingleIteration()
        {
            List<BigInteger> ret = new List<BigInteger>();

            foreach (BigInteger bi in TestBigIntegers)
            {
                // Equal
                ret.Add(bi);
                // First less 1
                ret.Add(bi - 1);
                // First add 1
                ret.Add(bi + 1);
                // Plus self
                ret.Add(bi * 1);
            }

            // Random value
            byte[] data = new byte[Rand.Next(1, 32)];
            Rand.NextBytes(data);

            ret.Add(new BigInteger(data));

            return ret.Distinct().ToArray();
        }
        /// <summary>
        /// Compute distinct pairs
        /// </summary>
        /// <returns>BigIntegerPair</returns>
        protected IEnumerable<BigIntegerPair> IntPairIteration()
        {
            BigInteger[] ar = IntSingleIteration().ToArray();
            List<BigIntegerPair> ret = new List<BigIntegerPair>();

            foreach (BigInteger ba in ar) foreach (BigInteger bb in ar)
                    ret.Add(new BigIntegerPair(ba, bb));

            // Invert all values

            for (int x = 0, m = ret.Count; x < m; x++)
                ret.Add(ret[x].Invert());

            return ret.Distinct().ToArray();
        }
        /// <summary>
        /// Check operand with two BigIntegers
        /// </summary>
        /// <param name="operand">Operand</param>
        /// <param name="check">Check</param>
        protected void InternalTestBigInteger(EVMOpCode operand, Action<ExecutionEngine, BigInteger, BigInteger, CancelEventArgs> check)
        {
            // Test without push

            using (ScriptBuilder script = new ScriptBuilder(operand))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test with wrong type

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                operand
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                if (operand == EVMOpCode.EQUAL)
                {
                    // Equal command don't FAULT here

                    Assert.IsTrue(engine.Execute());
                    Assert.AreEqual(engine.ResultStack.Pop<BooleanStackItem>().Value, false);
                }
                else
                {
                    Assert.IsFalse(engine.Execute());
                }

                // Check

                CheckClean(engine, false);
            }

            Stopwatch sw = new Stopwatch();

            // Test with push

            foreach (BigIntegerPair pair in IntPairIteration())
            {
                using (ScriptBuilder script = new ScriptBuilder())
                using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                {
                    // Make the script

                    foreach (BigInteger bb in new BigInteger[] { pair.A, pair.B })
                        script.EmitPush(bb);

                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script.ToArray());

                    // Execute

                    // PUSH A
                    engine.StepInto();
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);

                    // PUSH B
                    engine.StepInto();
                    Assert.AreEqual(2, engine.CurrentContext.EvaluationStack.Count);

                    // Operand

                    CancelEventArgs cancel = new CancelEventArgs(false);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Restart();
                    }
#pragma warning restore

                    engine.StepInto();
                    engine.StepInto();

                    check(engine, pair.A, pair.B, cancel);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Stop();
                        Console.WriteLine("[" + sw.Elapsed.ToString() + "] " + pair.A + " " + operand.ToString() + " " + pair.B);
                    }
#pragma warning restore

                    if (cancel.Cancel)
                    {
                        CheckClean(engine, false);
                        continue;
                    }

                    // RET

                    Assert.AreEqual(EVMState.HALT, engine.State);

                    // Check

                    CheckClean(engine);
                }
            }

            // Test with dup

            foreach (BigInteger i in IntSingleIteration())
            {
                using (ScriptBuilder script = new ScriptBuilder())
                using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                {
                    // Make the script

                    script.EmitPush(i);
                    script.Emit(EVMOpCode.DUP);
                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script.ToArray());

                    // Execute

                    // PUSH A
                    engine.StepInto();
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);

                    // PUSH B
                    engine.StepInto();
                    Assert.AreEqual(2, engine.CurrentContext.EvaluationStack.Count);

                    // Operand

                    CancelEventArgs cancel = new CancelEventArgs(false);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Restart();
                    }
#pragma warning restore

                    engine.StepInto(2);
                    check(engine, i, i, cancel);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Stop();
                        Console.WriteLine("[" + sw.Elapsed.ToString() + "] " + i + " " + operand.ToString() + " " + i);
                    }
#pragma warning restore

                    if (cancel.Cancel)
                    {
                        CheckClean(engine, false);
                        continue;
                    }

                    // RET
                    engine.StepInto();
                    Assert.AreEqual(EVMState.HALT, engine.State);

                    // Check

                    CheckClean(engine);
                }
            }
        }
        /// <summary>
        /// Check operand with one BigIntegers
        /// </summary>
        /// <param name="operand">Operand</param>
        /// <param name="check">Check</param>
        protected void InternalTestBigInteger(EVMOpCode operand, Action<ExecutionEngine, BigInteger, CancelEventArgs> check)
        {
            // Test without push

            using (ScriptBuilder script = new ScriptBuilder(operand))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test with wrong type

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                operand
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test with push

            Stopwatch sw = new Stopwatch();

            foreach (BigInteger bi in IntSingleIteration())
            {
                using (ScriptBuilder script = new ScriptBuilder())
                using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                {
                    // Make the script

                    script.EmitPush(bi);
                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    // PUSH A
                    engine.StepInto();
                    Assert.AreEqual(1, engine.CurrentContext.EvaluationStack.Count);

                    // Operand

                    CancelEventArgs cancel = new CancelEventArgs(false);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Restart();
                    }
#pragma warning restore

                    engine.StepInto(2);
                    check(engine, bi, cancel);

#pragma warning disable CS0162
                    if (CalculateNumericalTimes)
                    {
                        sw.Stop();
                        Console.WriteLine("[" + sw.Elapsed.ToString() + "] " + bi);
                    }
#pragma warning restore

                    if (cancel.Cancel) continue;

                    // RET
                    engine.StepInto();
                    Assert.AreEqual(EVMState.HALT, engine.State);

                    // Check

                    CheckClean(engine);
                }
            }
        }
        /// <summary>
        /// Check if the engine is clean
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="invocationStack">True for Check invocationStack</param>
        protected void CheckClean(ExecutionEngine engine, bool invocationStack = true)
        {
            Assert.AreEqual(0, engine.ResultStack.Count);

            if (invocationStack)
            {
                Assert.AreEqual(0, engine.InvocationStack.Count);
            }
            else
            {
                if (engine.CurrentContext == null)
                {
                    Assert.AreEqual(EVMState.HALT, engine.State);
                }
                else
                {
                    Assert.AreEqual(0, engine.CurrentContext.EvaluationStack.Count);
                    Assert.AreEqual(0, engine.CurrentContext.AltStack.Count);
                }
            }
        }
        /// <summary>
        /// Check Array (using peek)
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="index">Index</param>
        /// <param name="isStruct">Is struct</param>
        /// <param name="count">Count</param>
        /// <param name="values">Values</param>
        protected void CheckArrayPeek(StackItemStack stack, int index, bool isStruct, params object[] values)
        {
            using (ArrayStackItem arr = stack.Peek<ArrayStackItem>(index))
            {
                Assert.IsTrue(arr != null);
                Assert.AreEqual(isStruct, arr.IsStruct);
                CheckArray(arr, isStruct, values);
            }
        }
        /// <summary>
        /// Check Array (using peek)
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="isStruct">Is struct</param>
        /// <param name="values">Values</param>
        protected void CheckArrayPop(StackItemStack stack, bool isStruct, params object[] values)
        {
            using (ArrayStackItem arr = stack.Pop<ArrayStackItem>())
            {
                Assert.IsTrue(arr != null);
                CheckArray(arr, isStruct, values);
            }
        }
        /// <summary>
        /// Check array
        /// </summary>
        /// <param name="arr">Array</param>
        /// <param name="isStruct">Is struct</param>
        /// <param name="values">Values</param>
        protected void CheckArray(ArrayStackItem arr, bool isStruct, params object[] values)
        {
            Assert.AreEqual(isStruct, arr.IsStruct);
            Assert.IsTrue(arr.Count == values.Length);

            for (int x = values.Length - 1; x >= 0; x--)
            {
                object val = values[x];

                if (val is Int32)
                {
                    Assert.IsTrue(arr[x] is IntegerStackItem);
                    IntegerStackItem i = arr[x] as IntegerStackItem;
                    Assert.AreEqual(i.Value, (int)values[x]);
                }
                else if (val is bool)
                {
                    Assert.IsTrue(arr[x] is BooleanStackItem);
                    BooleanStackItem i = arr[x] as BooleanStackItem;
                    Assert.AreEqual(i.Value, (bool)values[x]);
                }
                else
                {
                    throw (new ArgumentException());
                }
            }
        }
    }
}