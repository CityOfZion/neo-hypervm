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

            //Register("Neo.Storage.Find", Storage_Find);
            //Register("Neo.Iterator.Next", Iterator_Next);
            //Register("Neo.Iterator.Key", Iterator_Key);
            //Register("Neo.Iterator.Value", Iterator_Value);
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

            //byte[] key = engine.EvaluationStack.Pop().GetByteArray();

            //StorageItem item = Storages.TryGet(new StorageKey
            //{
            //    ScriptHash = context.ScriptHash,
            //    Key = key
            //});

            //engine.EvaluationStack.Push(item?.Value ?? new byte[0]);
            //return true;

            return true;
        }

        bool Storage_Delete(ExecutionEngine engine)
        {
            if (!engine.EvaluationStack.TryPop(out InteropStackItem inter) ||
                !(inter.Value is DummyStorageContext context))
                return false;

            //    byte[] key = engine.EvaluationStack.Pop().GetByteArray();
            //    storages.Delete(new StorageKey
            //    {
            //        ScriptHash = context.ScriptHash,
            //        Key = key
            //    });
            //    return true;
            //}
            //return false;

            return true;
        }

        bool Storage_Put(ExecutionEngine engine)
        {
            if (!engine.EvaluationStack.TryPop(out InteropStackItem inter) ||
                !(inter.Value is DummyStorageContext context))
                return false;

            //byte[] key = engine.EvaluationStack.Pop().GetByteArray();
            //if (key.Length > 1024) return false;
            //byte[] value = engine.EvaluationStack.Pop().GetByteArray();
            //storages.GetAndChange(new StorageKey
            //{
            //    ScriptHash = context.ScriptHash,
            //    Key = key
            //}, () => new StorageItem()).Value = value;
            //return true;

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