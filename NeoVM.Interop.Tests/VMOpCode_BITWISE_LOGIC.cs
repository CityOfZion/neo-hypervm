using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types.StackItems;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_BITWISE_LOGIC : VMOpCodeTest
    {
        [TestMethod]
        public void INVERT()
        {
            InternalTestBigInteger(EVMOpCode.INVERT, (engine, a, cancel) =>
            {
                BigInteger res;

                try { res = ~a; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void AND()
        {
            InternalTestBigInteger(EVMOpCode.AND, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a & b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void OR()
        {
            InternalTestBigInteger(EVMOpCode.OR, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a | b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void XOR()
        {
            InternalTestBigInteger(EVMOpCode.XOR, (engine, a, b, cancel) =>
            {
                BigInteger res;

                try { res = (a ^ b); }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, res);
            });
        }

        [TestMethod]
        public void EQUAL()
        {
            // TODO: More equals, conversions, and test all types

            // Equal ByteArrays

            InternalTestBigInteger(EVMOpCode.EQUAL, (engine, a, b, cancel) =>
            {
                bool res;

                try { res = a == b; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<BooleanStackItem>().Value, res);
            });
        }
    }
}