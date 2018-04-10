namespace NeoVM.Interop.Enums
{
    public enum ELogVerbosity : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// All
        /// </summary>
        All = Operations | ExecutionContextStackChanges | EvaluationStackChanges | AltStackChanges,

        /// <summary>
        /// Enable operations logs
        /// </summary>
        Operations = 1 << 0,

        /// <summary>
        /// ExecutionContextStack changes
        /// </summary>
        ExecutionContextStackChanges = 2 << 0,
        /// <summary>
        /// EvaluationStack changes
        /// </summary>
        EvaluationStackChanges = 3 << 0,
        /// <summary>
        /// AltStack changes
        /// </summary>
        AltStackChanges = 4 << 0,
    }
}