using NeoVM.Interop.Helpers;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types;
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

        bool CheckWitness(ExecutionEngine engine)
        {
            // Fake CheckWitness

            if (!engine.EvaluationStack.TryPop(out IStackItem it) || !it.CanConvertToByteArray)
                return false;

            using (IStackItem itb = engine.CreateBool(true))
                engine.EvaluationStack.Push(itb);

            return true;
        }

        bool Storage_GetContext(ExecutionEngine engine)
        {
            ExecutionContext cnt = engine.CurrentContext;

            if (cnt == null) return false;

            string id = BitHelper.ToHexString(cnt.ScriptHash);
            if (!Storages.TryGetValue(id, out DummyStorageContext context))
            {
                context = new DummyStorageContext(id, cnt.ScriptHash);
                Storages[context.Id] = context;
            }

            using (IStackItem i = engine.CreateInterop(context))
                engine.EvaluationStack.Push(i);

            return true;
        }

        bool Storage_Get(ExecutionEngine engine)
        {
            if (!engine.EvaluationStack.TryPop(out InteropStackItem inter) ||
                !(inter.Value is DummyStorageContext context))
                return false;

            if (!engine.EvaluationStack.TryPop(out IStackItem it) || !it.CanConvertToByteArray)
                return false;

            byte[] key = it.ToByteArray();

            if (context.Storage.TryGetValue(BitHelper.ToHexString(key), out byte[] value))
            {
                using (IStackItem ret = engine.CreateByteArray(value))
                    engine.EvaluationStack.Push(ret);
            }
            else
            {
                using (IStackItem ret = engine.CreateByteArray(new byte[] { }))
                    engine.EvaluationStack.Push(ret);
            }

            return true;
        }

        bool Storage_Delete(ExecutionEngine engine)
        {
            if (!engine.EvaluationStack.TryPop(out InteropStackItem inter) ||
                !(inter.Value is DummyStorageContext context))
                return false;

            if (!engine.EvaluationStack.TryPop(out IStackItem it) || !it.CanConvertToByteArray)
                return false;

            byte[] key = it.ToByteArray();
            context.Storage.Remove(BitHelper.ToHexString(key));
            return true;
        }

        bool Storage_Put(ExecutionEngine engine)
        {
            if (!engine.EvaluationStack.TryPop(out InteropStackItem inter) ||
                !(inter.Value is DummyStorageContext context))
                return false;

            if (!engine.EvaluationStack.TryPop(out IStackItem it) || !it.CanConvertToByteArray)
                return false;

            byte[] key = it.ToByteArray();
            if (key.Length > 1024) return false;

            if (!engine.EvaluationStack.TryPop(out it) || !it.CanConvertToByteArray)
                return false;

            byte[] value = it.ToByteArray();

            context.Storage[BitHelper.ToHexString(key)] = value;
            return true;
        }

        bool TestMethod(ExecutionEngine engine)
        {
            using (IStackItem it = engine.CreateInterop(new DisposableDummy()))
                engine.EvaluationStack.Push(it);

            return true;
        }
    }
}