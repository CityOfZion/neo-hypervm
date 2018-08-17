using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Tests.Extra;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    public class VMOpCodeTest
    {
        private const bool CalculateNumericalTimes = false;
        private IVMFactory _VMFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            Assert.IsTrue(NeoVM.IsLoaded || NeoVM.TryLoadLibrary(NeoVM.DefaultLibraryName, out var error));

            _VMFactory = new NeoVM();

            //Console.WriteLine("Native Library Info");
            //Console.WriteLine("  Path: " + NeoVM.LibraryPath);
            //Console.WriteLine("  Version: " + NeoVM.LibraryVersion);
            //Console.WriteLine("");
        }

        /// <summary>
        /// Create new Engine
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Return new engine</returns>
        public IExecutionEngine CreateEngine(ExecutionEngineArgs args)
        {
            return _VMFactory.Create(args);
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
            var ret = new List<BigInteger>();

            foreach (var bi in TestBigIntegers)
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
            var data = new byte[Rand.Next(1, 32)];
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
            var ar = IntSingleIteration().ToArray();
            var ret = new List<BigIntegerPair>();

            foreach (var ba in ar) foreach (var bb in ar)
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
        protected void InternalTestBigInteger(EVMOpCode operand, Action<IExecutionEngine, BigInteger, BigInteger, CancelEventArgs> check)
        {
            // Test without push

            using (var script = new ScriptBuilder(operand))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test with wrong type

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                operand
                ))
            using (var engine = CreateEngine(Args))
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

            foreach (var pair in IntPairIteration())
            {
                using (var script = new ScriptBuilder())
                using (var engine = CreateEngine(Args))
                {
                    // Make the script

                    foreach (BigInteger bb in new BigInteger[] { pair.A, pair.B })
                        script.EmitPush(bb);

                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script.ToArray());

                    // Execute

                    using (var currentContext = engine.CurrentContext)
                    {
                        // PUSH A
                        engine.StepInto();

                        Assert.AreEqual(1, currentContext.EvaluationStack.Count);

                        // PUSH B
                        engine.StepInto();
                        Assert.AreEqual(2, currentContext.EvaluationStack.Count);
                    }

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

                    Assert.AreEqual(EVMState.Halt, engine.State);

                    // Check

                    CheckClean(engine);
                }
            }

            // Test with dup

            foreach (var i in IntSingleIteration())
            {
                using (var script = new ScriptBuilder())
                using (var engine = CreateEngine(Args))
                {
                    // Make the script

                    script.EmitPush(i);
                    script.Emit(EVMOpCode.DUP);
                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script.ToArray());

                    // Execute

                    using (var currentContext = engine.CurrentContext)
                    {
                        // PUSH A
                        engine.StepInto();
                        Assert.AreEqual(1, currentContext.EvaluationStack.Count);

                        // PUSH B
                        engine.StepInto();
                        Assert.AreEqual(2, currentContext.EvaluationStack.Count);
                    }

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
                    Assert.AreEqual(EVMState.Halt, engine.State);

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
        protected void InternalTestBigInteger(EVMOpCode operand, Action<IExecutionEngine, BigInteger, CancelEventArgs> check)
        {
            // Test without push

            using (var script = new ScriptBuilder(operand))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Test with wrong type

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH1,
                EVMOpCode.NEWARRAY,
                operand
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

            // Test with push

            Stopwatch sw = new Stopwatch();

            foreach (var bi in IntSingleIteration())
            {
                using (var script = new ScriptBuilder())
                using (var engine = CreateEngine(Args))
                {
                    // Make the script

                    script.EmitPush(bi);
                    script.Emit(operand, EVMOpCode.RET);

                    // Load script

                    engine.LoadScript(script);

                    // Execute

                    using (var currentContext = engine.CurrentContext)
                    {
                        // PUSH A
                        engine.StepInto();
                        Assert.AreEqual(1, currentContext.EvaluationStack.Count);
                    }

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
                    Assert.AreEqual(EVMState.Halt, engine.State);

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
        protected void CheckClean(IExecutionEngine engine, bool invocationStack = true)
        {
            Assert.AreEqual(0, engine.ResultStack.Count);

            if (invocationStack)
            {
                Assert.AreEqual(0, engine.InvocationStack.Count);
            }
            else
            {
                var current = engine.CurrentContext;

                if (current == null)
                {
                    Assert.AreEqual(EVMState.Halt, engine.State);
                }
                else
                {
                    Assert.AreEqual(0, current.EvaluationStack.Count);
                    Assert.AreEqual(0, current.AltStack.Count);

                    current.Dispose();
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
        protected void CheckArrayPeek(IStackItemsStack stack, int index, bool isStruct, params object[] values)
        {
            using (var arr = stack.Peek<IArrayStackItem>(index))
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
        protected void CheckArrayPop(IStackItemsStack stack, bool isStruct, params object[] values)
        {
            using (var arr = stack.Pop<IArrayStackItem>())
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
        protected void CheckArray(IArrayStackItem arr, bool isStruct, params object[] values)
        {
            Assert.AreEqual(isStruct, arr.IsStruct);
            Assert.IsTrue(arr.Count == values.Length);

            for (int x = values.Length - 1; x >= 0; x--)
            {
                object val = values[x];

                if (val is Int32)
                {
                    Assert.IsTrue(arr[x] is IIntegerStackItem);
                    var i = arr[x] as IIntegerStackItem;
                    Assert.AreEqual(i.Value, (int)values[x]);
                }
                else if (val is bool)
                {
                    Assert.IsTrue(arr[x] is IBooleanStackItem);
                    var i = arr[x] as IBooleanStackItem;
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