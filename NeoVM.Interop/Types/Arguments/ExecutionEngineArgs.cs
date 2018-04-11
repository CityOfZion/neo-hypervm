using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;

namespace NeoVM.Interop.Types.Arguments
{
    public class ExecutionEngineArgs
    {
        /// <summary>
        /// Trigger
        /// </summary>
        public ETriggerType Trigger { get; set; } = ETriggerType.Application;
        /// <summary>
        /// Message Provider
        /// </summary>
        public IMessageProvider MessageProvider { get; set; }
        /// <summary>
        /// Interop service
        /// </summary>
        public InteropService InteropService { get; set; }
        /// <summary>
        /// Script table
        /// </summary>
        public IScriptTable ScriptTable { get; set; }
        /// <summary>
        /// Logger
        /// </summary>
        public ExecutionEngineLogger Logger { get; set; }
    }
}