using System.Linq;

namespace NeoSharp.VM.Interop.Tests.Extra
{
    public class DummyScriptTable : IScriptTable
    {
        public readonly byte[] DynamicRet;
        public readonly byte[] NonDynamicRet;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DummyScriptTable()
        {
            DynamicRet = new byte[] { (byte)EVMOpCode.PUSH6 };
            NonDynamicRet = new byte[] { (byte)EVMOpCode.PUSH4 };
        }

        /// <summary>
        /// Manual constructor
        /// </summary>
        /// <param name="dynamicRet">Dynamic script</param>
        /// <param name="nonDynamicRet">Non dynamic script</param>
        public DummyScriptTable(byte[] dynamicRet, byte[] nonDynamicRet)
        {
            DynamicRet = dynamicRet;
            NonDynamicRet = nonDynamicRet;
        }

        public byte[] GetScript(byte[] scriptHash, bool isDynamicInvoke)
        {
            if (scriptHash.SequenceEqual(new byte[]
            {
                0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,
                0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,
            }))
            {
                return isDynamicInvoke ? DynamicRet : NonDynamicRet;
            }

            return null;
        }
    }
}