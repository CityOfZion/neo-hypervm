using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NeoSharp.VM.Helpers;
using NeoSharp.VM.Interop.Tests.Helpers;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests.Extra
{
    public class DummyInteropService : InteropService
    {
        /// <summary>
        /// Fake storages
        /// </summary>
        Dictionary<string, DummyStorageContext> Storages = new Dictionary<string, DummyStorageContext>();

        /// <summary>
        /// Constructor
        /// </summary>
        public DummyInteropService() : base()
        {
            Register("Test", TestMethod);

            // Fake storages

            Register("Neo.Runtime.Serialize", Runtime_Serialize);
            Register("Neo.Runtime.Deserialize", Runtime_Deserialize);
            Register("Neo.Storage.GetContext", Storage_GetContext);
            Register("Neo.Storage.Get", Storage_Get);
            Register("Neo.Storage.Put", Storage_Put);
            Register("Neo.Storage.Delete", Storage_Delete);
            Register("Neo.Runtime.CheckWitness", CheckWitness);

            //Register("Neo.Storage.Find", Storage_Find);
            //Register("Neo.Iterator.Next", Iterator_Next);
            //Register("Neo.Iterator.Key", Iterator_Key);
            //Register("Neo.Iterator.Value", Iterator_Value);
        }

        private bool Runtime_Serialize(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out IStackItem it))
                    return false;

                using (it)
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(ms))
                    {
                        try
                        {
                            SerializeStackItem(it, writer);
                        }
                        catch
                        {
                            return false;
                        }

                        writer.Flush();

                        using (var bi = engine.CreateByteArray(ms.ToArray()))
                            context.EvaluationStack.Push(bi);
                    }
                }
            }

            return true;
        }

        private bool Runtime_Deserialize(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out IStackItem it))
                    return false;

                var data = it.ToByteArray();
                it.Dispose();

                using (MemoryStream ms = new MemoryStream(data, false))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    IStackItem item = null;

                    try
                    {
                        item = DeserializeStackItem(engine, reader);
                    }
                    catch
                    {
                        if (item != null) item.Dispose();
                        return false;
                    }

                    context.EvaluationStack.Push(item);
                    if (item != null) item.Dispose();
                }
            }

            return true;
        }

        private IStackItem DeserializeStackItem(IExecutionEngine engine, BinaryReader reader)
        {
            EStackItemType type = (EStackItemType)reader.ReadByte();

            switch (type)
            {
                case EStackItemType.ByteArray:
                    return engine.CreateByteArray(reader.ReadVarBytes());
                case EStackItemType.Bool:
                    return engine.CreateBool(reader.ReadBoolean());
                case EStackItemType.Integer:
                    return engine.CreateInteger(new BigInteger(reader.ReadVarBytes()));
                case EStackItemType.Array:
                case EStackItemType.Struct:
                    {
                        IArrayStackItem array;

                        if (type == EStackItemType.Struct)
                            array = engine.CreateStruct();
                        else
                            array = engine.CreateArray();

                        ulong count = reader.ReadVarInt();
                        while (count-- > 0)
                            array.Add(DeserializeStackItem(engine, reader));

                        return array;
                    }
                case EStackItemType.Map:
                    {
                        IMapStackItem map = engine.CreateMap();

                        ulong count = reader.ReadVarInt();
                        while (count-- > 0)
                        {
                            IStackItem key = DeserializeStackItem(engine, reader);
                            IStackItem value = DeserializeStackItem(engine, reader);

                            map[key] = value;

                            key.Dispose();
                            value.Dispose();
                        }

                        return map;
                    }
                default: throw new FormatException();
            }
        }

        private void SerializeStackItem(IStackItem item, BinaryWriter writer)
        {
            switch (item)
            {
                case ByteArrayStackItem _:
                    {
                        writer.Write((byte)EStackItemType.ByteArray);
                        writer.WriteVarBytes(item.ToByteArray());
                        break;
                    }
                case BooleanStackItem bl:
                    {
                        writer.Write((byte)EStackItemType.Bool);
                        writer.Write(bl.Value);
                        break;
                    }
                case IntegerStackItem _:
                    {
                        writer.Write((byte)EStackItemType.Integer);
                        writer.WriteVarBytes(item.ToByteArray());
                        break;
                    }
                case InteropStackItem _: throw new NotSupportedException();
                case ArrayStackItem array:
                    {
                        if (array.IsStruct)
                            writer.Write((byte)EStackItemType.Struct);
                        else
                            writer.Write((byte)EStackItemType.Array);

                        writer.WriteVarInt(array.Count);

                        foreach (IStackItem subitem in array)
                        {
                            SerializeStackItem(subitem, writer);
                        }

                        break;
                    }
                case MapStackItem map:
                    {
                        writer.Write((byte)EStackItemType.Map);
                        writer.WriteVarInt(map.Count);

                        foreach (var pair in map)
                        {
                            SerializeStackItem(pair.Key, writer);
                            SerializeStackItem(pair.Value, writer);
                        }

                        break;
                    }
            }
        }

        bool CheckWitness(IExecutionEngine engine)
        {
            // Fake CheckWitness

            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out IStackItem it))
                    return false;

                using (it)
                {
                    if (!it.CanConvertToByteArray) return false;

                    using (var itb = engine.CreateBool(true))
                        context.EvaluationStack.Push(itb);
                }
            }

            return true;
        }

        bool Storage_GetContext(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                string id = BitHelper.ToHexString(context.ScriptHash);
                if (!Storages.TryGetValue(id, out DummyStorageContext stContext))
                {
                    stContext = new DummyStorageContext(id, context.ScriptHash);
                    Storages[stContext.Id] = stContext;
                }

                using (var i = engine.CreateInterop(stContext))
                    context.EvaluationStack.Push(i);
            }

            return true;
        }

        bool Storage_Get(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItem inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext)) return false;

                    if (!context.EvaluationStack.TryPop(out IStackItem it))
                        return false;

                    using (it)
                    {
                        if (!it.CanConvertToByteArray) return false;

                        byte[] key = it.ToByteArray();

                        if (stContext.Storage.TryGetValue(BitHelper.ToHexString(key), out byte[] value))
                        {
                            using (var ret = engine.CreateByteArray(value))
                                context.EvaluationStack.Push(ret);
                        }
                        else
                        {
                            using (var ret = engine.CreateByteArray(new byte[] { }))
                                context.EvaluationStack.Push(ret);
                        }
                    }
                }
            }

            return true;
        }

        bool Storage_Delete(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItem inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext))
                        return false;

                    if (!context.EvaluationStack.TryPop(out IStackItem it))
                        return false;

                    using (it)
                    {
                        if (!it.CanConvertToByteArray) return false;
                        byte[] key = it.ToByteArray();
                        stContext.Storage.Remove(BitHelper.ToHexString(key));
                    }
                }
            }

            return true;
        }

        bool Storage_Put(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItem inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext))
                        return false;

                    if (!context.EvaluationStack.TryPop(out IStackItem it))
                        return false;

                    byte[] key;
                    using (it)
                    {
                        if (!it.CanConvertToByteArray) return false;

                        key = it.ToByteArray();
                        if (key.Length > 1024) return false;
                    }

                    if (!context.EvaluationStack.TryPop(out it))
                        return false;

                    byte[] value;
                    using (it)
                    {
                        if (!it.CanConvertToByteArray) return false;

                        value = it.ToByteArray();
                    }

                    stContext.Storage[BitHelper.ToHexString(key)] = value;
                }
            }

            return true;
        }

        bool TestMethod(IExecutionEngine engine)
        {
            using (var context = engine.CurrentContext)
            {
                if (context == null) return false;

                using (var it = engine.CreateInterop(new DisposableDummy()))
                    context.EvaluationStack.Push(it);
            }

            return true;
        }
    }
}