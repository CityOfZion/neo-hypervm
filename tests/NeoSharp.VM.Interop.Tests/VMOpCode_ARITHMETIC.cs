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
            InternalTestBigInteger(EVMOpCode.INC, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = a + 1; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void DEC()
        {
            InternalTestBigInteger(EVMOpCode.DEC, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = a - 1; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void SIGN()
        {
            InternalTestBigInteger(EVMOpCode.SIGN, (engine, a, cancel) =>
            {
                int res;

                try { res = a.Sign; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void NEGATE()
        {
            InternalTestBigInteger(EVMOpCode.NEGATE, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = -a; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void ABS()
        {
            InternalTestBigInteger(EVMOpCode.ABS, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = BigInteger.Abs(a); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
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
            InternalTestBigInteger(EVMOpCode.NZ, (engine, a, cancel) =>
            {
                bool res;

                try { res = (a != 0); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void ADD()
        {
            InternalTestBigInteger(EVMOpCode.ADD, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try
                {
                    res = (a + b);

                    if (a.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        b.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void SUB()
        {
            InternalTestBigInteger(EVMOpCode.SUB, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try
                {
                    res = (a - b);

                    if (a.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        b.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void MUL()
        {
            InternalTestBigInteger(EVMOpCode.MUL, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try
                {
                    res = (a * b);

                    if (a.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        b.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void DIV()
        {
            InternalTestBigInteger(EVMOpCode.DIV, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a / b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void MOD()
        {
            InternalTestBigInteger(EVMOpCode.MOD, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try
                {
                    res = (a % b);

                    if (a.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        b.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                        res.ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void SHL()
        {
            InternalTestBigInteger(EVMOpCode.SHL, (engine, bi, shift, cancel) =>
            {
                int ishift;

                try
                {
                    if (shift > MAX_SHL_SHR || shift < MIN_SHL_SHR)
                    {
                        throw (new ArgumentException("Limit exceded"));
                    }

                    ishift = (int)shift;

                    if (bi.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                       (bi << ishift).ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }

                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, bi << ishift);
                }
            });
        }

        [TestMethod]
        public void SHR()
        {
            InternalTestBigInteger(EVMOpCode.SHR, (engine, bi, shift, cancel) =>
            {
                int ishift;

                try
                {
                    if (shift > MAX_SHL_SHR || shift < MIN_SHL_SHR)
                    {
                        throw (new ArgumentException("Limit exceded"));
                    }

                    ishift = (int)shift;

                    if (bi.ToByteArray().Length > MAX_BIGINTEGER_SIZE ||
                       (bi >> ishift).ToByteArray().Length > MAX_BIGINTEGER_SIZE)
                    {
                        throw new ArgumentException();
                    }
                }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, bi >> ishift);
                }
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
            InternalTestBigInteger(EVMOpCode.NUMEQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a == b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void NUMNOTEQUAL()
        {
            InternalTestBigInteger(EVMOpCode.NUMNOTEQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a != b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void LT()
        {
            InternalTestBigInteger(EVMOpCode.LT, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a < b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void GT()
        {
            InternalTestBigInteger(EVMOpCode.GT, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a > b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void LTE()
        {
            InternalTestBigInteger(EVMOpCode.LTE, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a <= b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void GTE()
        {
            InternalTestBigInteger(EVMOpCode.GTE, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = (a >= b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void MIN()
        {
            InternalTestBigInteger(EVMOpCode.MIN, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = BigInteger.Min(a, b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
            });
        }

        [TestMethod]
        public void MAX()
        {
            InternalTestBigInteger(EVMOpCode.MAX, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = BigInteger.Max(a, b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.Fault);
                    cancel.Cancel = true;
                    return;
                }

                using (var item = engine.ResultStack.Pop<IntegerStackItem>())
                {
                    Assert.AreEqual(item.Value, res);
                }
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