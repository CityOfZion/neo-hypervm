using System;

namespace NeoVM.Interop.Enums
{
    [Flags]
    public enum ELogVerbosity : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Enable step into logs
        /// </summary>
        StepInto = 1,

        /// <summary>
        /// ExecutionContextStack changes
        /// </summary>
        ExecutionContextStackChanges = 2,
        /// <summary>
        /// EvaluationStack changes
        /// </summary>
        EvaluationStackChanges = 4,
        /// <summary>
        /// AltStack changes
        /// </summary>
        AltStackChanges = 8,
        /// <summary>
        /// ResultStack changes
        /// </summary>
        ResultStackChanges = 16,

        /// <summary>
        /// All
        /// </summary>
        All = StepInto | ExecutionContextStackChanges | EvaluationStackChanges | AltStackChanges| ResultStackChanges,
        /// <summary>
        /// Stack changes
        /// </summary>
        StackChanges = ExecutionContextStackChanges | EvaluationStackChanges | AltStackChanges | ResultStackChanges,
    }
}