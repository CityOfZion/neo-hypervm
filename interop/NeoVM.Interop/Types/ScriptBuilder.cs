﻿using NeoVM.Interop.Enums;
using NeoVM.Interop.Helpers;
using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace NeoVM.Interop.Types
{
    public class ScriptBuilder : IDisposable
    {
        readonly MemoryStream writer;

        /// <summary>
        /// Stream offset
        /// </summary>
        public long Offset => writer.Position;
        /// <summary>
        /// Length
        /// </summary>
        public long Length => writer.Length;

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public ScriptBuilder()
        {
            writer = new MemoryStream();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="opcodes">OpCodes</param>
        public ScriptBuilder(params EVMOpCode[] opcodes) : this()
        {
            Emit(opcodes);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="opcode">OpCode</param>
        /// <param name="rawOpCodes">Raw OpCodes</param>
        public ScriptBuilder(EVMOpCode opcode, byte[] rawOpCodes) : this()
        {
            Emit(opcode);
            Emit(rawOpCodes);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="rawOpCodes">Raw OpCodes</param>
        public ScriptBuilder(byte[] rawOpCodes) : this()
        {
            Emit(rawOpCodes);
        }

        #endregion

        /// <summary>
        /// Free resources
        /// </summary>
        public void Dispose()
        {
            writer.Dispose();
        }
        /// <summary>
        /// Clear buffer
        /// </summary>
        public ScriptBuilder Clear()
        {
            writer.SetLength(0);

            return this;
        }

        public ScriptBuilder Emit(params byte[] raw)
        {
            if (raw != null)
                writer.Write(raw, 0, raw.Length);

            return this;
        }

        public ScriptBuilder Emit(byte[] raw, int index, int length)
        {
            if (length > 0)
                writer.Write(raw, index, length);

            return this;
        }

        public ScriptBuilder Emit(byte opCode)
        {
            writer.WriteByte(opCode);

            return this;
        }

        public ScriptBuilder Emit(params EVMOpCode[] ops)
        {
            foreach (EVMOpCode op in ops)
                writer.WriteByte((byte)op);

            return this;
        }

        public ScriptBuilder Emit(EVMOpCode op, byte[] arg = null)
        {
            writer.WriteByte((byte)op);

            if (arg != null)
                writer.Write(arg, 0, arg.Length);

            return this;
        }

        public ScriptBuilder EmitAppCall(byte[] scriptHash, bool useTailCall = false)
        {
            if (scriptHash.Length != 20)
                throw new ArgumentException();

            return Emit(useTailCall ? EVMOpCode.TAILCALL : EVMOpCode.APPCALL, scriptHash);
        }

        public ScriptBuilder EmitJump(EVMOpCode op, short offset)
        {
            if (op != EVMOpCode.JMP && op != EVMOpCode.JMPIF && op != EVMOpCode.JMPIFNOT && op != EVMOpCode.CALL)
                throw new ArgumentException();

            return Emit(op, BitConverter.GetBytes(offset));
        }

        public ScriptBuilder EmitRET()
        {
            return Emit(EVMOpCode.RET);
        }

        public ScriptBuilder EmitPush(BigInteger number)
        {
            if (number == -1) return Emit(EVMOpCode.PUSHM1);
            if (number == 0) return Emit(EVMOpCode.PUSH0);
            if (number > 0 && number <= 16) return Emit(EVMOpCode.PUSH1 - 1 + (byte)number);

            return EmitPush(number.ToByteArray());
        }

        public ScriptBuilder EmitPush(bool data)
        {
            return Emit(data ? EVMOpCode.PUSH1 : EVMOpCode.PUSH0);
        }

        public ScriptBuilder EmitPush(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException();

            if (data.Length <= (int)EVMOpCode.PUSHBYTES75)
            {
                writer.WriteByte((byte)data.Length);
                writer.Write(data, 0, data.Length);
            }
            else if (data.Length < 0x100)
            {
                Emit(EVMOpCode.PUSHDATA1);
                writer.WriteByte((byte)data.Length);
                writer.Write(data, 0, data.Length);
            }
            else if (data.Length < 0x10000)
            {
                Emit(EVMOpCode.PUSHDATA2);
                writer.Write(BitHelper.GetBytes((ushort)data.Length), 0, 2);
                writer.Write(data, 0, data.Length);
            }
            else// if (data.Length < 0x100000000L)
            {
                Emit(EVMOpCode.PUSHDATA4);
                writer.Write(BitHelper.GetBytes(data.Length), 0, 4);
                writer.Write(data, 0, data.Length);
            }
            return this;
        }

        public ScriptBuilder EmitPush(string data)
        {
            return EmitPush(Encoding.UTF8.GetBytes(data));
        }

        public ScriptBuilder EmitPush(object[] data)
        {
            int size = data.Length;

            for (int x = size - 1; x >= 0; x--)
            {
                switch (data[x])
                {
                    case string v: EmitPush(v); break;
                    case int v: EmitPush(v); break;
                    case long v: EmitPush(v); break;
                    case BigInteger v: EmitPush(v); break;
                    case bool v: EmitPush(v); break;
                    case Byte[] v: EmitPush(v); break;
                    case Object[] v: EmitPush(v); break;

                    default: throw (new ArgumentException());
                }
            }

            EmitPush(size);
            Emit(EVMOpCode.PACK);

            return this;
        }

        public ScriptBuilder EmitMainPush(string operation, object[] pars)
        {
            EmitPush(pars);
            EmitPush(operation);

            return this;
        }

        public ScriptBuilder EmitSysCall(string api)
        {
            if (api == null)
                throw new ArgumentNullException();

            byte[] api_bytes = Encoding.ASCII.GetBytes(api);
            if (api_bytes.Length == 0 || api_bytes.Length > 252)
                throw new ArgumentException();

            byte[] arg = new byte[api_bytes.Length + 1];
            arg[0] = (byte)api_bytes.Length;
            Buffer.BlockCopy(api_bytes, 0, arg, 1, api_bytes.Length);

            return Emit(EVMOpCode.SYSCALL, arg);
        }

        #region Conversions

        /// <summary>
        /// Convert to byte array
        /// </summary>
        public byte[] ToArray()
        {
            return writer.ToArray();
        }

        public static implicit operator byte[] (ScriptBuilder script)
        {
            return script.ToArray();
        }

        #endregion
    }
}