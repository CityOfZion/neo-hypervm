using System;
using System.Collections.Generic;
using System.Linq;

namespace NeoSharp.VM.Interop.Tests.Extra
{
    internal class DummyMessageProvider : IMessageProvider
    {
        readonly Dictionary<uint, byte[]> _messages = new Dictionary<uint, byte[]>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messages">Messages</param>
        public DummyMessageProvider(Dictionary<uint, byte[]> messages = null)
        {
            _messages = messages;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iteration">Iteration</param>
        /// <param name="data">Data</param>
        public DummyMessageProvider(uint iteration, byte[] data)
        {
            _messages = new Dictionary<uint, byte[]>();
            _messages.Add(iteration, data);
        }

        /// <summary>
        /// Get message from iteration
        /// </summary>
        /// <param name="iteration">Iteration</param>
        /// <returns>Return the message or NULL</returns>
        public byte[] GetMessage(uint iteration)
        {
            if (_messages != null && _messages.Count > 0)
            {
                if (_messages.TryGetValue(iteration, out byte[] ret))
                    return ret;

                return null;
            }

            return new byte[]
            {
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A
            }
            .Concat(BitConverter.GetBytes(iteration))
            .ToArray();
        }
    }
}