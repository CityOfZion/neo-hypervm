using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_EXCEPTIONS : VMOpCodeTest
    {
        [TestMethod]
        public void THROW()
        {
            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.THROW,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void THROWIFNOT()
        {
            // Not throw exception

            byte[] script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.NOT,
                    (byte)EVMOpCode.THROWIFNOT,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());
            }

            // Throw exception

            script = new byte[]
                {
                    (byte)EVMOpCode.PUSH0,
                    (byte)EVMOpCode.THROWIFNOT,
                    (byte)EVMOpCode.RET,
                };

            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                CheckClean(engine, false);
            }
        }
    }
}