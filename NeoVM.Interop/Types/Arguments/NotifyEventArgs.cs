using NeoVM.Interop.Interfaces;
using System;

namespace NeoVM.Interop.Types.Arguments
{
    public class NotifyEventArgs : EventArgs
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
        /// Stack Item
        /// </summary>
        public readonly IStackItem State;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Script container</param>
        /// <param name="scriptHash">Script hash</param>
        /// <param name="state">State</param>
        public NotifyEventArgs(IScriptContainer container, byte[] scriptHash, IStackItem state)
        {
            ScriptContainer = container;
            ScriptHash = scriptHash;
            State = state;
        }
    }
}