using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    public class VMOpCodeTest
    {
        /// <summary>
        /// Rand
        /// </summary>
        public readonly Random Rand = new Random();
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
        /// <param name="bi">Integer</param>
        /// <returns>BigIntegerPair</returns>
        protected static IEnumerable<BigInteger> IntSingleIteration(BigInteger bi)
        {
            // Equal
            yield return bi;
            // First less 1
            yield return bi - 1;
            // First add 1
            yield return bi + 1;
            // Last less 1
            yield return bi * bi;
        }
        /// <summary>
        /// Compute distinct pairs
        /// </summary>
        /// <param name="bi">Integer</param>
        /// <returns>BigIntegerPair</returns>
        protected static IEnumerable<BigIntegerPair> IntPairIteration(BigInteger bi)
        {
            // Equal
            yield return new BigIntegerPair(bi, bi);
            // First less 1
            yield return new BigIntegerPair(bi - 1, bi);
            // First add 1
            yield return new BigIntegerPair(bi + 1, bi);
            // Last less 1
            yield return new BigIntegerPair(bi, bi - 1);
            // Last add 1
            yield return new BigIntegerPair(bi, bi + 1);
            // With Zero
            if (bi != BigInteger.Zero && bi + 1 != BigInteger.Zero && bi - 1 != BigInteger.Zero)
                yield return new BigIntegerPair(bi, BigInteger.Zero);
            // With One
            if (bi != BigInteger.One && bi + 1 != BigInteger.One && bi - 1 != BigInteger.One)
                yield return new BigIntegerPair(bi, BigInteger.One);
            // With MinusOne
            if (bi != BigInteger.MinusOne && bi + 1 != BigInteger.MinusOne && bi - 1 != BigInteger.MinusOne)
                yield return new BigIntegerPair(bi, BigInteger.MinusOne);
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

            foreach (BigInteger bi in TestBigIntegers)
                foreach (BigIntegerPair pair in IntPairIteration(bi))
                {
                    using (ScriptBuilder script = new ScriptBuilder())
                    using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                    {
                        // Make the script

                        foreach (BigInteger bb in new BigInteger[] { pair.A, pair.B })
                            script.EmitPush(bb.ToByteArray());

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

            foreach (BigInteger bbi in TestBigIntegers) foreach (BigInteger bi in IntSingleIteration(bbi))
                {
                    using (ScriptBuilder script = new ScriptBuilder())
                    using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                    {
                        // Make the script

                        script.EmitPush(bi.ToByteArray());
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