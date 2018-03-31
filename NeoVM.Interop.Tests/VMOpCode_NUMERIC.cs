using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types.StackItems;
using System.Numerics;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_NUMERIC : VMOpCodeTest
    {
        [TestMethod]
        public void INC()
        {
            InternalTestBigInteger(EVMOpCode.INC, (engine, a) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a + 1));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void DEC()
        {
            InternalTestBigInteger(EVMOpCode.DEC, (engine, a) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a + 1));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void SIGN()
        {
            InternalTestBigInteger(EVMOpCode.SIGN, (engine, a) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == a.Sign);
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void NEGATE()
        {
            InternalTestBigInteger(EVMOpCode.NEGATE, (engine, a) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == -a);
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void ABS()
        {
            InternalTestBigInteger(EVMOpCode.ABS, (engine, a) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == BigInteger.Abs(a));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void ADD()
        {
            InternalTestBigInteger(EVMOpCode.ADD, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a + b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void SUB()
        {
            InternalTestBigInteger(EVMOpCode.SUB, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a - b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void MUL()
        {
            InternalTestBigInteger(EVMOpCode.MUL, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a * b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void DIV()
        {
            InternalTestBigInteger(EVMOpCode.DIV, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a / b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void MOD()
        {
            InternalTestBigInteger(EVMOpCode.MOD, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a % b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void SHL()
        {
            InternalTestBigInteger(EVMOpCode.SHL, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a << (int)b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void SHR()
        {
            InternalTestBigInteger(EVMOpCode.SHR, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a >> (int)b));
                engine.EvaluationStack.Pop();
            });
        }
    }
}