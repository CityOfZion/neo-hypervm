using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using NeoSharp.VM.Extensions;
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
            Register("UT.Test", TestMethod);

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

        private bool Runtime_Serialize(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out StackItemBase it))
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

        private bool Runtime_Deserialize(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out StackItemBase it))
                    return false;

                var data = it.ToByteArray();
                it.Dispose();

                using (MemoryStream ms = new MemoryStream(data, false))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    StackItemBase item = null;

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

        private StackItemBase DeserializeStackItem(ExecutionEngineBase engine, BinaryReader reader)
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
                        ArrayStackItemBase array;

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
                        var map = engine.CreateMap();

                        ulong count = reader.ReadVarInt();
                        while (count-- > 0)
                        {
                            StackItemBase key = DeserializeStackItem(engine, reader);
                            StackItemBase value = DeserializeStackItem(engine, reader);

                            map[key] = value;

                            key.Dispose();
                            value.Dispose();
                        }

                        return map;
                    }
                default: throw new FormatException();
            }
        }

        private void SerializeStackItem(StackItemBase item, BinaryWriter writer)
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
                case ArrayStackItem array:
                    {
                        if (array.IsStruct)
                            writer.Write((byte)EStackItemType.Struct);
                        else
                            writer.Write((byte)EStackItemType.Array);

                        writer.WriteVarInt(array.Count);

                        foreach (StackItemBase subitem in array)
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
                default: throw new NotSupportedException();
            }
        }

        bool CheckWitness(ExecutionEngineBase engine)
        {
            // Fake CheckWitness

            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out StackItemBase it))
                    return false;

                using (it)
                {
                    if (it.ToByteArray() == null) return false;

                    using (var itb = engine.CreateBool(true))
                        context.EvaluationStack.Push(itb);
                }
            }

            return true;
        }

        bool Storage_GetContext(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                var id = context.ScriptHash.ToHexString();
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

        bool Storage_Get(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItemBase<DummyStorageContext> inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext)) return false;

                    if (!context.EvaluationStack.TryPop(out StackItemBase it))
                        return false;

                    using (it)
                    {
                        var key = it.ToByteArray();
                        if (key == null) return false;

                        if (stContext.Storage.TryGetValue(key.ToHexString(), out byte[] value))
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

        bool Storage_Delete(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItemBase<DummyStorageContext> inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext))
                        return false;

                    if (!context.EvaluationStack.TryPop(out StackItemBase it))
                        return false;

                    using (it)
                    {
                        var key = it.ToByteArray();

                        if (key == null) return false;
                        stContext.Storage.Remove(key.ToHexString());
                    }
                }
            }

            return true;
        }

        bool Storage_Put(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                if (!context.EvaluationStack.TryPop(out InteropStackItemBase<DummyStorageContext> inter))
                    return false;

                using (inter)
                {
                    if (!(inter.Value is DummyStorageContext stContext))
                        return false;

                    if (!context.EvaluationStack.TryPop(out StackItemBase it))
                        return false;

                    byte[] key;
                    using (it)
                    {
                        key = it.ToByteArray();

                        if (key == null) return false;
                        if (key.Length > 1024) return false;
                    }

                    if (!context.EvaluationStack.TryPop(out it))
                        return false;

                    byte[] value;
                    using (it)
                    {
                        value = it.ToByteArray();

                        if (value == null) return false;
                    }

                    stContext.Storage[key.ToHexString()] = value;
                }
            }

            return true;
        }

        bool TestMethod(ExecutionEngineBase engine)
        {
            var context = engine.CurrentContext;
            {
                if (context == null) return false;

                using (var it = engine.CreateInterop(new DisposableDummy()))
                    context.EvaluationStack.Push(it);
            }

            return true;
        }
    }
}