using NeoVM.Interop.Interfaces;
using System;

namespace NeoVM.Interop.Types.Arguments
{
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Message Provider
        /// </summary>
        public readonly IMessageProvider MessageProvider;
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
        /// <param name="messageProvider">Message Provider</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="message">Message</param>
        public LogEventArgs(IMessageProvider messageProvider, byte[] scriptHash, string message)
        {
            MessageProvider = messageProvider;
            ScriptHash = scriptHash;
            Message = message;
        }
    }
}