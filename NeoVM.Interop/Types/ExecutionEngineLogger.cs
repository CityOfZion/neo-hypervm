using NeoVM.Interop.Delegates;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Interfaces;
using NeoVM.Interop.Types.Collections;

namespace NeoVM.Interop.Types
{
    public class ExecutionEngineLogger
    {
        #region Properties

        /// <summary>
        /// Verbosity
        /// </summary>
        public readonly ELogVerbosity Verbosity;

        #endregion

        #region Events

        /// <summary>
        /// On Operation
        /// </summary>
        public event delOnOperation OnOperation;
        /// <summary>
        /// On ExecutionContextChange
        /// </summary>
        public event delOnExecutionContextStackChange OnExecutionContextChange;
        /// <summary>
        /// On AltStackChange
        /// </summary>
        public event delOnStackItemsStackChange OnAltStackChange;
        /// <summary>
        /// On EvaluationStackChanges
        /// </summary>
        public event delOnStackItemsStackChange OnEvaluationStackChanges;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="verbosity">Verbosity</param>
        public ExecutionEngineLogger(ELogVerbosity verbosity)
        {
            Verbosity = verbosity;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Raise OnOperation
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="opcode">OpCode</param>
        /// <param name="arguments">Argument</param>
        public virtual void RaiseOnOperation(ExecutionContext context, EVMOpCode opcode, byte[] arguments)
        {
            OnOperation?.Invoke(context, opcode, arguments);
        }
        /// <summary>
        /// Raise OnExecutionContextChange
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="item">Item</param>
        /// <param name="operation">Operation</param>
        public virtual void RaiseOnExecutionContextChange(ExecutionContextStack stack, ExecutionContext item, ELogStackOperation operation)
        {
            OnExecutionContextChange?.Invoke(stack, item, operation);
        }
        /// <summary>
        /// Raise OnAltStackChange
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="item">Item</param>
        /// <param name="operation">Operation</param>
        public virtual void RaiseOnAltStackChange(StackItemStack stack, IStackItem item, ELogStackOperation operation)
        {
            OnAltStackChange?.Invoke(stack, item, operation);
        }
        /// <summary>
        /// Raise OnEvaluationStackChanges
        /// </summary>
        /// <param name="stack">Stack</param>
        /// <param name="item">Item</param>
        /// <param name="operation">Operation</param>
        public virtual void RaiseOnEvaluationStackChanges(StackItemStack stack, IStackItem item, ELogStackOperation operation)
        {
            OnEvaluationStackChanges?.Invoke(stack, item, operation);
        }

        #endregion
    }
}