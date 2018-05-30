﻿using NeoSharp.VM;
using System;
using System.Linq;

namespace NeoVM.Interop.Tests.Extra
{
    internal class DummyMessageProvider : IMessageProvider
    {
        public byte[] GetMessage(uint iteration)
        {
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