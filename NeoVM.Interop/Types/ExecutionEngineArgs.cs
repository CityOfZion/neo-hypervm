using NeoVM.Interop.Interfaces;

namespace NeoVM.Interop.Types
{
    public class ExecutionEngineArgs
    {
        /// <summary>
        /// Script container
        /// </summary>
        public IScriptContainer ScriptContainer { get; set; }
        /// <summary>
        /// Interop service
        /// </summary>
        public InteropService InteropService { get; set; }
        /// <summary>
        /// Script table
        /// </summary>
        public IScriptTable ScriptTable { get; set; }
    }
}