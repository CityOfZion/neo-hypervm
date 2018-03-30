using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Tests.Extra;
using NeoVM.Interop.Types;

namespace NeoVM.Interop.Tests
{
    public class VMOpCodeTest
    {
        /// <summary>
        /// Regular arguments
        /// </summary>
        protected static ExecutionEngineArgs Args = new ExecutionEngineArgs()
        {
            ScriptContainer = new DummyScript(),
            InteropService = new InteropService(),
            ScriptTable = new DummyScriptTable()
        };

        /// <summary>
        /// Check if the engine is clean
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="invocationStack">True for Check invocationStack</param>
        protected void CheckClean(ExecutionEngine engine, bool invocationStack = true)
        {
            Assert.AreEqual(0, engine.EvaluationStack.Count);
            Assert.AreEqual(0, engine.AltStack.Count);
            if (invocationStack) Assert.AreEqual(0, engine.InvocationStack.Count);
        }
    }
}