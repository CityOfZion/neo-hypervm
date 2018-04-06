using NeoVM.Interop.Types;

namespace NeoVM.Interop.Tests.Extra
{
    public class DummyInteropService : InteropService
    {
        public DummyInteropService() : base()
        {
            Register("Test", TestMethod);
        }

        bool TestMethod(ExecutionEngine engine)
        {
            engine.EvaluationStack.Push(engine.CreateInterop(new DisposableDummy()));
            return true;
        }
    }
}