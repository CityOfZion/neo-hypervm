using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_EXCEPTIONS : VMOpCodeTest
    {
        [TestMethod]
        public void THROW()
        {
            using (var script = new ScriptBuilder
                (
                EVMOpCode.THROW
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                engine.StepInto();

                Assert.AreEqual(EVMState.Fault, engine.State);

                CheckClean(engine, false);
            }
        }

        [TestMethod]
        public void THROWIFNOT()
        {
            // Not throw exception

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH0,
                EVMOpCode.NOT,
                EVMOpCode.THROWIFNOT,
                EVMOpCode.RET
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                CheckClean(engine);
            }

            // Throw exception (with PUSH)

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH0,
                EVMOpCode.THROWIFNOT,
                EVMOpCode.RET
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load Script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                CheckClean(engine, false);
            }

            // Throw exception (without PUSH - FAULT)

            using (var script = new ScriptBuilder
                (
                EVMOpCode.THROWIFNOT
                ))
            using (var engine = CreateEngine(Args))
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