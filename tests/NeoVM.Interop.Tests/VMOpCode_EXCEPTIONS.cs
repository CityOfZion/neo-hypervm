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
            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.THROW
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                engine.StepInto();

                Assert.AreEqual(EVMState.FAULT, engine.State);

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void THROWIFNOT()
        {
            // Not throw exception

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.THROWIFNOT,
                EVMOpCode.RET
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                CheckClean(engine);
            }

            // Throw exception (with PUSH)

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.PUSH0,
                EVMOpCode.THROWIFNOT,
                EVMOpCode.RET
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                CheckClean(engine, false);
            }

            // Throw exception (without PUSH - FAULT)

            using (ScriptBuilder script = new ScriptBuilder
                (
                EVMOpCode.THROWIFNOT
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                CheckClean(engine, false);
            }
        }
    }
}