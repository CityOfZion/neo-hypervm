using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_ARITHMETIC : VMOpCodeTest
    {
        /// <summary>
        /// Max value for SHL-SHR
        /// </summary>
        const int MAX_SHL_SHR = 65535;
        /// <summary>
        /// Min value for SHL-SHR
        /// </summary>
        const int MIN_SHL_SHR = -MAX_SHL_SHR;
        /// <summary>
        /// Set the max size allowed size for BigInteger
        /// </summary>
        const int MAX_BIGINTEGER_SIZE = 32;

        [TestMethod]
        public void INC()
        {
            InternalTestBigInteger(EVMOpCode.INC, (a) =>
            {
                return a + 1;
            });
        }

        [TestMethod]
        public void DEC()
        {
            InternalTestBigInteger(EVMOpCode.DEC, (a) =>
            {
                return a - 1;
            });
        }

        [TestMethod]
        public void SIGN()
        {
            InternalTestBigInteger(EVMOpCode.SIGN, (a) =>
            {
                return new BigInteger(a.Sign);
            });
        }

        [TestMethod]
        public void NEGATE()
        {
            InternalTestBigInteger(EVMOpCode.NEGATE, (a) =>
            {
                return -a;
            });
        }

        [TestMethod]
        public void ABS()
        {
            InternalTestBigInteger(EVMOpCode.ABS, (a) =>
            {
                return BigInteger.Abs(a);
            });
        }

        [TestMethod]
        public void NOT()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.NOT
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
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(item.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NZ()
        {
            InternalTestBigInteger(EVMOpCode.NZ, (a) =>
            {
                return a != 0;
            });
        }

        [TestMethod]
        public void ADD()
        {
            InternalTestBigInteger(EVMOpCode.ADD, (pair) =>
            {
                BigInteger res = pair.A + pair.B;

                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return res;
            });
        }

        [TestMethod]
        public void SUB()
        {
            InternalTestBigInteger(EVMOpCode.SUB, (pair) =>
            {
                BigInteger res = pair.A - pair.B;

                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return res;
            });
        }

        [TestMethod]
        public void MUL()
        {
            InternalTestBigInteger(EVMOpCode.MUL, (pair) =>
            {
                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.A.ToByteArray().Length + pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return pair.A * pair.B;
            });
        }

        [TestMethod]
        public void DIV()
        {
            InternalTestBigInteger(EVMOpCode.DIV, (pair) =>
            {
                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return pair.A / pair.B;
            });
        }

        [TestMethod]
        public void MOD()
        {
            InternalTestBigInteger(EVMOpCode.MOD, (pair) =>
            {
                BigInteger res = pair.A % pair.B;

                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    pair.B.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                    res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return res;
            });
        }

        [TestMethod]
        public void SHL()
        {
            InternalTestBigInteger(EVMOpCode.SHL, (pair) =>
            {
                BigInteger shift = pair.B;

                if (shift > MAX_SHL_SHR || shift < MIN_SHL_SHR)
                {
                    throw new ArgumentException("Limit exceeded");
                }

                int ishift = (int)shift;

                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                   (pair.A << ishift).ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return pair.A << ishift;
            });
        }

        [TestMethod]
        public void SHR()
        {
            InternalTestBigInteger(EVMOpCode.SHR, (pair) =>
            {
                BigInteger shift = pair.B;

                if (shift > MAX_SHL_SHR || shift < MIN_SHL_SHR)
                {
                    throw new ArgumentException("Limit exceeded");
                }

                int ishift = (int)shift;

                if (pair.A.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                   (pair.A >> ishift).ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                {
                    throw new ArgumentException();
                }

                return pair.A >> ishift;
            });
        }

        [TestMethod]
        public void BOOLAND()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.BOOLAND,
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

                using (var currentContext = engine.CurrentContext)
                using (var item = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, 3);
                }

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.BOOLAND,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(item.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void BOOLOR()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH3,
                EVMOpCode.BOOLOR,
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

                using (var currentContext = engine.CurrentContext)
                using (var item = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, 3);
                }

                CheckClean(engine, false);
            }

            // Real test

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH0,
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.BOOLOR,
                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(item.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NUMEQUAL()
        {
            InternalTestBigInteger(EVMOpCode.NUMEQUAL, (pair) =>
            {
                return pair.A == pair.B;
            });
        }

        [TestMethod]
        public void NUMNOTEQUAL()
        {
            InternalTestBigInteger(EVMOpCode.NUMNOTEQUAL, (pair) =>
            {
                return pair.A != pair.B;
            });
        }

        [TestMethod]
        public void LT()
        {
            InternalTestBigInteger(EVMOpCode.LT, (pair) =>
            {
                return pair.A < pair.B;
            });
        }

        [TestMethod]
        public void GT()
        {
            InternalTestBigInteger(EVMOpCode.GT, (pair) =>
            {
                return pair.A > pair.B;
            });
        }

        [TestMethod]
        public void LTE()
        {
            InternalTestBigInteger(EVMOpCode.LTE, (pair) =>
            {
                return pair.A <= pair.B;
            });
        }

        [TestMethod]
        public void GTE()
        {
            InternalTestBigInteger(EVMOpCode.GTE, (pair) =>
            {
                return pair.A >= pair.B;
            });
        }

        [TestMethod]
        public void MIN()
        {
            InternalTestBigInteger(EVMOpCode.MIN, (pair) =>
            {
                return BigInteger.Min(pair.A, pair.B);
            });
        }

        [TestMethod]
        public void MAX()
        {
            InternalTestBigInteger(EVMOpCode.MAX, (pair) =>
            {
                return BigInteger.Max(pair.A, pair.B);
            });
        }

        [TestMethod]
        public void WITHIN()
        {
            // Without push

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,
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

                using (var currentContext = engine.CurrentContext)
                {
                    using (var item = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(item.Value, 5);
                    }
                    using (var item = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(item.Value, 1);
                    }
                }

                CheckClean(engine, false);
            }

            // Without wrong types

            using (var script = new ScriptBuilder
            (
                EVMOpCode.PUSH1, EVMOpCode.PUSH5, EVMOpCode.NEWMAP,
                EVMOpCode.WITHIN,
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
                EVMOpCode.PUSH1,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH5,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH2,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH7,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.PUSH0,
                EVMOpCode.PUSH1, EVMOpCode.PUSH5,
                EVMOpCode.WITHIN,

                EVMOpCode.RET
            ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(item.Value);
                }
                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(item.Value);
                }
                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(item.Value);
                }
                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(item.Value);
                }
                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsTrue(item.Value);
                }

                CheckClean(engine);
            }
        }
    }
}