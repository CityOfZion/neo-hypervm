using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;
using System;
using System.Collections.Generic;
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

            new BigInteger(ulong.MaxValue),
            new BigInteger(ulong.MinValue),
            new BigInteger(long.MaxValue),
            new BigInteger(long.MinValue),

            new BigInteger(uint.MaxValue),
            new BigInteger(uint.MinValue),
            new BigInteger(int.MaxValue),
            new BigInteger(int.MinValue),

            new BigInteger(ushort.MaxValue),
            new BigInteger(ushort.MinValue),
            new BigInteger(short.MaxValue),
            new BigInteger(short.MinValue),

            new BigInteger(sbyte.MaxValue),
            new BigInteger(sbyte.MinValue),
            new BigInteger(byte.MaxValue),
            new BigInteger(byte.MinValue),

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
        /// Compute distinct pairs
        /// </summary>
        /// <param name="bi">Integer</param>
        /// <returns>BigIntegerPair</returns>
        protected static IEnumerable<BigIntegerPair> IntIteration(BigInteger bi)
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
        protected void InternalTestBigInteger(EVMOpCode operand, Action<ExecutionEngine, BigInteger, BigInteger> check)
        {
            foreach (BigInteger bi in TestBigIntegers)
                foreach (BigIntegerPair pair in IntIteration(bi))
                {
                    using (MemoryStream script = new MemoryStream())
                    using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                    {
                        // Make the script

                        foreach (BigInteger bb in new BigInteger[] { pair.A, pair.B })
                        {
                            byte[] bba = bb.ToByteArray();

                            script.WriteByte((byte)EVMOpCode.PUSHDATA1);
                            script.WriteByte((byte)bba.Length);
                            script.Write(bba, 0, bba.Length);
                        }

                        script.WriteByte((byte)operand);
                        script.WriteByte((byte)EVMOpCode.RET);

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
                        engine.StepInto();
                        check(engine, pair.A, pair.B);

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
        protected void InternalTestBigInteger(EVMOpCode operand, Action<ExecutionEngine, BigInteger> check)
        {
            foreach (BigInteger bi in TestBigIntegers)
            {
                using (MemoryStream script = new MemoryStream())
                using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
                {
                    // Make the script

                    byte[] bba = bi.ToByteArray();

                    script.WriteByte((byte)EVMOpCode.PUSHDATA1);
                    script.WriteByte((byte)bba.Length);
                    script.Write(bba, 0, bba.Length);

                    script.WriteByte((byte)operand);
                    script.WriteByte((byte)EVMOpCode.RET);

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
                    engine.StepInto();
                    check(engine, bi);

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