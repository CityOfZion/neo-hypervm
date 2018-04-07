using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
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
        /// <summary>
        /// Rand
        /// </summary>
        public static readonly Random Rand = new Random();

        /// <summary>
        /// Test BigInteger array
        /// </summary>
        public static readonly BigInteger[] TestBigIntegers = new BigInteger[]
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
        protected readonly static ExecutionEngineArgs Args = new ExecutionEngineArgs()
        {
            ScriptContainer = new DummyScript(),
            InteropService = new InteropService(),
            ScriptTable = new DummyScriptTable()
        };

        /// <summary>
        /// Compute distinct BigIntegers
        /// </summary>
        /// <returns>BigIntegerPair</returns>
        protected static IEnumerable<BigInteger> IntSingleIteration()
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
        protected static IEnumerable<BigIntegerPair> IntPairIteration()
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

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

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

                    Assert.AreEqual(EVMState.HALT, engine.Execute());
                    Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, false);
                }
                else
                {
                    Assert.AreEqual(EVMState.FAULT, engine.Execute());
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
                    Assert.AreEqual(1, engine.EvaluationStack.Count);

                    // PUSH B
                    engine.StepInto();
                    Assert.AreEqual(2, engine.EvaluationStack.Count);

                    // Operand

                    CancelEventArgs cancel = new CancelEventArgs(false);
                    sw.Restart();
                    engine.StepInto();
                    check(engine, pair.A, pair.B, cancel);
                    sw.Stop();
                    Console.WriteLine("[" + sw.Elapsed.ToString() + "] " + pair.A + " " + operand.ToString() + " " + pair.B);

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

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

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

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

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

                    engine.LoadScript(script.ToArray());

                    // Execute

                    // PUSH A
                    engine.StepInto();
                    Assert.AreEqual(1, engine.EvaluationStack.Count);

                    // Operand

                    CancelEventArgs cancel = new CancelEventArgs(false);
                    sw.Restart();
                    engine.StepInto();
                    check(engine, bi, cancel);
                    sw.Stop();
                    Console.WriteLine("[" + sw.Elapsed.ToString() + "] " + bi);

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
            Assert.AreEqual(0, engine.EvaluationStack.Count);
            Assert.AreEqual(0, engine.AltStack.Count);
            if (invocationStack) Assert.AreEqual(0, engine.InvocationStack.Count);
        }
    }
}