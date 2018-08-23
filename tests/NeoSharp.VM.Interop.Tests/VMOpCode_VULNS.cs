using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_VULNS : VMOpCodeTest
    {
        /*
        [TestMethod]
        public void CloneOverflow()
        {
            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH0,
                EVMOpCode.PICKITEM
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                using (var stA = engine.CreateStruct())
                {
                    stA.Add((IStackItem)stA);

                    engine.CurrentContext.EvaluationStack.Push(stA);
                }

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var stA = engine.ResultStack.Pop<IArrayStackItem>())
                {
                    Assert.IsTrue(stA != null);
                    Assert.AreEqual(stA.Count, 1);
                    Assert.AreEqual(stA, stA[0]);
                }

                engine.Clean();
            }
        }
        */
    }
}