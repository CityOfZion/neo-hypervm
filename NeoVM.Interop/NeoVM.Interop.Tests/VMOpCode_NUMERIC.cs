using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_NUMERIC : VMOpCodeTest
    {
        [TestMethod]
        public void ADD()
        {
            InternalTestBigInteger(EVMOpCode.ADD, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == (a + b));
            });
        }

        [TestMethod]
        public void SUB()
        {
            InternalTestBigInteger(EVMOpCode.SUB, (engine, a, b) =>
            {
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == (a - b));
            });
        }
    }
}