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
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a & b));
                engine.EvaluationStack.Pop();
            });
        }

        [TestMethod]
        public void OR()
        {
            InternalTestBigInteger(EVMOpCode.OR, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Peek<IntegerStackItem>(0).Value == (a | b));
                engine.EvaluationStack.Pop();
            });
        }
    }
}