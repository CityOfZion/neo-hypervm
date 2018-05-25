using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types.Arguments;
using System;
using System.Collections.Generic;

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
            using (ExecutionContext ctx = engine.CurrentContext)
            using (IStackItem item = engine.CreateInteger((int)engine.Trigger))
                ctx.EvaluationStack.Push(item);

            return true;
        }

        bool NeoRuntimeLog(ExecutionEngine engine)
        {
            using (ExecutionContext ctx = engine.CurrentContext)
            {
                if (ctx == null) return false;

                if (!ctx.EvaluationStack.TryPop(out IStackItem it))
                    return false;

                using (it)
                {
                    if (OnLog == null)
                        return true;

                    // Get string

                    string message = it.ToString();
                    OnLog.Invoke(this, new LogEventArgs(engine.MessageProvider, ctx?.ScriptHash, message ?? ""));
                }
            }

            return true;
        }

        bool NeoRuntimeNotify(ExecutionEngine engine)
        {
            using (ExecutionContext ctx = engine.CurrentContext)
            {
                if (ctx == null) return false;

                if (!ctx.EvaluationStack.TryPop(out IStackItem it))
                    return false;

                using (it)
                {
                    OnNotify?.Invoke(this, new NotifyEventArgs(engine.MessageProvider, ctx?.ScriptHash, it));
                }
            }

            return true;
        }

        static bool GetScriptContainer(ExecutionEngine engine)
        {
            if (engine.MessageProvider == null)
                return false;

            using (ExecutionContext current = engine.CurrentContext)
            using (IStackItem item = engine.CreateInterop(engine.MessageProvider))
                current.EvaluationStack.Push(item);

            return true;
        }

        static bool GetExecutingScriptHash(ExecutionEngine engine)
        {
            using (ExecutionContext ctx = engine.CurrentContext)
            {
                if (ctx == null) return false;

                using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                    ctx.EvaluationStack.Push(item);
            }

            return true;
        }

        static bool GetCallingScriptHash(ExecutionEngine engine)
        {
            using (ExecutionContext ctx = engine.CallingContext)
            {
                if (ctx == null) return false;

                using (ExecutionContext current = engine.CurrentContext)
                using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                    current.EvaluationStack.Push(item);
            }

            return true;
        }

        static bool GetEntryScriptHash(ExecutionEngine engine)
        {
            using (ExecutionContext ctx = engine.EntryContext)
            {
                if (ctx == null) return false;

                using (ExecutionContext current = engine.CurrentContext)
                using (IStackItem item = engine.CreateByteArray(ctx.ScriptHash))
                    current.EvaluationStack.Push(item);
            }

            return true;
        }

        #endregion
    }
}