using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types.StackItems;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_NUMERIC : VMOpCodeTest
    {
        //[TestMethod]
        public void INC()
        {
            InternalTestBigInteger(EVMOpCode.INC, (engine, a, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a + 1));
            });
        }

        //[TestMethod]
        public void DEC()
        {
            InternalTestBigInteger(EVMOpCode.DEC, (engine, a, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a + 1));
            });
        }

        [TestMethod]
        public void SIGN()
        {
            InternalTestBigInteger(EVMOpCode.SIGN, (engine, a, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, a.Sign);
            });
        }

        [TestMethod]
        public void NEGATE()
        {
            InternalTestBigInteger(EVMOpCode.NEGATE, (engine, a, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, -a);
            });
        }

        [TestMethod]
        public void ABS()
        {
            InternalTestBigInteger(EVMOpCode.ABS, (engine, a, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, BigInteger.Abs(a));
            });
        }

        //[TestMethod]
        public void ADD()
        {
            InternalTestBigInteger(EVMOpCode.ADD, (engine, a, b, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a + b));
            });
        }

        //[TestMethod]
        public void SUB()
        {
            InternalTestBigInteger(EVMOpCode.SUB, (engine, a, b, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a - b));
            });
        }

        //[TestMethod]
        public void MUL()
        {
            InternalTestBigInteger(EVMOpCode.MUL, (engine, a, b, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a * b));
            });
        }

        //[TestMethod]
        public void DIV()
        {
            InternalTestBigInteger(EVMOpCode.DIV, (engine, a, b, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a / b));
            });
        }

        //[TestMethod]
        public void MOD()
        {
            InternalTestBigInteger(EVMOpCode.MOD, (engine, a, b, cancel) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a % b));
            });
        }

        [TestMethod]
        public void SHL()
        {
            InternalTestBigInteger(EVMOpCode.SHL, (engine, bi, shift, cancel) =>
            {
                int ishift;

                try { ishift = (int)shift; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (bi << ishift));
            });
        }

        [TestMethod]
        public void SHR()
        {
            InternalTestBigInteger(EVMOpCode.SHR, (engine, bi, shift, cancel) =>
            {
                int ishift;

                try { ishift = (int)shift; }
                catch
                {
                    Assert.AreEqual(engine.State, EVMState.FAULT);
                    cancel.Cancel = true;
                    return;
                }

                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (bi >> ishift));
            });
        }
    }
}