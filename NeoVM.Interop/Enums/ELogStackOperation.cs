namespace NeoVM.Interop.Enums
{
    public enum ELogStackOperation : byte
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Pop item to stack
        /// </summary>
        Pop = 1,
        /// <summary>
        /// Drop item from stack
        /// </summary>
        Drop = 2,
        /// <summary>
        /// Push item to stack
        /// </summary>
        Push = 3,
        /// <summary>
        /// Peek item from stack
        /// </summary>
        Peek = 4,
        /// <summary>
        /// TryPeek item from stack
        /// </summary>
        TryPeek = 5,
        /// <summary>
        /// Remove item from stack
        /// </summary>
        Remove = 6,
        /// <summary>
        /// Insert item to stack
        /// </summary>
        Insert = 7
    }
}