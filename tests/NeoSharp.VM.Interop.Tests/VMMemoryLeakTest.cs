using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Interop.Tests.Extra;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMMemoryLeakTest : VMOpCodeTest
    {
        [TestMethod, Ignore]
        public void TestScriptHash()
        {
            using (var memLeak = new MemoryLeakAssert())
            {
                using (var engine = CreateEngine(Args))
                {

                }
            }
        }
    }
}