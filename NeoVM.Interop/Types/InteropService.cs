using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types.Arguments;
using NeoVM.Interop.Types.StackItems;
using System;
using System.Collections.Generic;
using System.Text;

namespace NeoVM.Interop.Types
{
    public class InteropService
    {
        /// <summary>
        /// Delegate
        /// </summary>
        /// <param name="engine">Execution engine</param>
        /// <returns>Return false if something wrong</returns>
        public delegate bool delHandler(ExecutionEngine engine);
        /// <summary>
        /// Notify event
        /// </summary>
        public event EventHandler<NotifyEventArgs> OnNotify;
        /// <summary>
        /// Log event
        /// </summary>
        public event EventHandler<LogEventArgs> OnLog;

        /// <summary>
        /// Cache dictionary
        /// </summary>
        SortedDictionary<string, delHandler> Entries = new SortedDictionary<string, delHandler>();

        /// <summary>
        /// Get method
        /// </summary>
        /// <param name="method">Method</param>
        public delHandler this[string method]
        {
            get
            {
                if (!Entries.TryGetValue(method, out delHandler func)) return null;
                return func;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public InteropService()
        {
            Register("Neo.Runtime.GetTrigger", NeoRuntimeGetTrigger);
            Register("Neo.Runtime.Log", NeoRuntimeLog);
            Register("Neo.Runtime.Notify", NeoRuntimeNotify);
            //Register("Neo.Runtime.Serialize", Runtime_Serialize);
            //Register("Neo.Runtime.Deserialize", Runtime_Deserialize);

            Register("System.ExecutionEngine.GetScriptContainer", GetScriptContainer);
            Register("System.ExecutionEngine.GetExecutingScriptHash", GetExecutingScriptHash);
            Register("System.ExecutionEngine.GetCallingScriptHash", GetCallingScriptHash);
            Register("System.ExecutionEngine.GetEntryScriptHash", GetEntryScriptHash);
        }

        /// <summary>
        /// Register method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="synonymous">Synonymous</param>
        /// <param name="handler">Method delegate</param>
        protected void Register(string method, string synonymous, delHandler handler)
        {
            Entries[method] = handler;
            Entries[synonymous] = handler;
        }
        /// <summary>
        /// Register method
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="handler">Method delegate</param>
        protected void Register(string method, delHandler handler)
        {
            Entries[method] = handler;
        }
        /// <summary>
        /// Clear entries
        /// </summary>
        public void Clear()
        {
            Entries.Clear();
        }
        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="method">Method name</param>
        /// <param name="engine">Execution engine</param>
        /// <returns>Return false if something wrong</returns>
        public bool Invoke(string method, ExecutionEngine engine)
        {
            if (!Entries.TryGetValue(method, out delHandler func))
                return false;

            return func(engine);
        }

        #region Delegates

        static bool NeoRuntimeGetTrigger(ExecutionEngine engine)
        {
            using (IStackItem item = engine.CreateInteger((int)engine.Trigger))
                engine.EvaluationStack.Push(item);

            return true;
        }

        bool NeoRuntimeLog(ExecutionEngine engine)
        {
            if (engine.EvaluationStack.Count < 1)
            {
                return false;
            }

            string message = null;
            using (IStackItem it = engine.EvaluationStack.Pop())
            {
                if (OnLog == null)
                    return true;

                // Get string

                switch (it.Type)
                {
                    case EStackItemType.ByteArray:
                        {
                            message = Encoding.UTF8.GetString(((ByteArrayStackItem)it).Value);
                            break;
                        }
                    default:
                        {
                            message = it.ToString();
                            break;
                        }
                }
            }

            ExecutionContext ctx = engine.CurrentContext;
            OnLog.Invoke(this, new LogEventArgs(engine.ScriptContainer, ctx?.ScriptHash, message ?? ""));
            return true;
        }

        bool NeoRuntimeNotify(ExecutionEngine engine)
        {
            if (engine.EvaluationStack.Count < 1)
            {
                return false;
            }

            IStackItem it = engine.EvaluationStack.Pop();

            if (OnNotify == null)
            {
                it.Dispose();
                return true;
            }

            ExecutionContext ctx = engine.CurrentContext;
            OnNotify.Invoke(this, new NotifyEventArgs(engine.ScriptContainer, ctx?.ScriptHash, it));
            return true;
        }

        static bool GetScriptContainer(ExecutionEngine engine)
        {
            if (engine.ScriptContainer == null)
                return false;

            using (IStackItem item = engine.CreateInterop(engine.ScriptContainer))
                engine.EvaluationStack.Push(item);

            return true;
        }

        static bool GetExecutingScriptHash(ExecutionEngine engine)
        {
            ExecutionContext ctx = engine.CurrentContext;
            if (ctx == null) return false;

            using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                engine.EvaluationStack.Push(item);

            return true;
        }

        static bool GetCallingScriptHash(ExecutionEngine engine)
        {
            ExecutionContext ctx = engine.CallingContext;
            if (ctx == null) return false;

            using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                engine.EvaluationStack.Push(item);

            return true;
        }

        static bool GetEntryScriptHash(ExecutionEngine engine)
        {
            ExecutionContext ctx = engine.EntryContext;
            if (ctx == null) return false;

            using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                engine.EvaluationStack.Push(item);

            return true;
        }

        #endregion
    }
}