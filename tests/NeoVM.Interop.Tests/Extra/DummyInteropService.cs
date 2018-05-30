using NeoSharp.VM;
using NeoSharp.VM.Helpers;
using NeoVM.Interop.Types.StackItems;
using System.Collections.Generic;

namespace NeoVM.Interop.Tests.Extra
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