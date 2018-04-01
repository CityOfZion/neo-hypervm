using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_BITWISE_LOGIC : VMOpCodeTest
    {
        [TestMethod]
        public void AND()
        {
            InternalTestBigInteger(EVMOpCode.AND, (engine, a, b) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a & b));
            });
        }

        [TestMethod]
        public void OR()
        {
            InternalTestBigInteger(EVMOpCode.OR, (engine, a, b) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a | b));
            });
        }

        [TestMethod]
        public void XOR()
        {
            InternalTestBigInteger(EVMOpCode.XOR, (engine, a, b) =>
            {
                Assert.AreEqual(engine.EvaluationStack.Pop<IntegerStackItem>().Value, (a ^ b));
            });
        }
    }
}