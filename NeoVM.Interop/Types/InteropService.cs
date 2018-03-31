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
            Register("System.ExecutionEngine.GetScriptContainer", GetScriptContainer);
            Register("System.ExecutionEngine.GetExecutingScriptHash", GetExecutingScriptHash);
            Register("System.ExecutionEngine.GetCallingScriptHash", GetCallingScriptHash);
            Register("System.ExecutionEngine.GetEntryScriptHash", GetEntryScriptHash);
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
            if (!Entries.TryGetValue(method, out delHandler func)) return false;
            return func(engine);
        }

        #region Delegates

        static bool GetScriptContainer(ExecutionEngine engine)
        {
            if (engine.ScriptContainer == null)
                return false;

            engine.EvaluationStack.Push(engine.CreateInterop(engine.ScriptContainer));
            return true;
        }

        static bool GetExecutingScriptHash(ExecutionEngine engine)
        {
            engine.EvaluationStack.Push(engine.CreateByteArray(engine.CurrentContext.ScriptHash));
            return true;
        }

        static bool GetCallingScriptHash(ExecutionEngine engine)
        {
            engine.EvaluationStack.Push(engine.CreateByteArray(engine.CallingContext.ScriptHash));
            return true;
        }

        static bool GetEntryScriptHash(ExecutionEngine engine)
        {
            engine.EvaluationStack.Push(engine.CreateByteArray(engine.EntryContext.ScriptHash));
            return true;
        }

        #endregion
    }
}