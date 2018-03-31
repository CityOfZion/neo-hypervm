namespace NeoVM.Interop.Enums
{
    public enum EVMState : byte
    {
        /// <summary>
        /// Normal state 
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Virtual machine stopped
        /// </summary>
        HALT = 1,
        /// <summary>
        /// Virtual machine execution with errors
        /// </summary>
        FAULT = 2,
    };
}