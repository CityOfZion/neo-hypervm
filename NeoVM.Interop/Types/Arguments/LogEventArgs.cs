using NeoVM.Interop.Interfaces;
using System;

namespace NeoVM.Interop.Types.Arguments
{
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Script container
        /// </summary>
        public readonly IScriptContainer ScriptContainer;
        /// <summary>
        /// Script Hash
        /// </summary>
        public readonly byte[] ScriptHash;
        /// <summary>
        /// Message
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Script container</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="message">Message</param>
        public LogEventArgs(IScriptContainer container, byte[] scriptHash, string message)
        {
            ScriptContainer = container;
            ScriptHash = scriptHash;
            Message = message;
        }
    }
}