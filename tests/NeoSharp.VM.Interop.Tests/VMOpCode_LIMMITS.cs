using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_LIMMITS : VMOpCodeTest
    {
        [TestMethod]
        public void MAX_STACK_SIZE()
        {
            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(Args))
            {
                for (int x = 0; x < (2 * 1024); x++)
                {
                    script.EmitPush(1);
                }

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Overflow max

                script.EmitPush(1);

                engine.Clean();
                engine.LoadScript(script);

                Assert.IsFalse(engine.Execute());
            }
        }
    }
}