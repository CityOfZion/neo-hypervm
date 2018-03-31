namespace NeoVM.Interop.Interfaces
{
    public interface IScriptTable
    {
        /// <summary>
        /// Get script of this hash
        /// </summary>
        /// <param name="scriptHash">Script hash</param>
        /// <returns>Script or NULL</returns>
        byte[] GetScript(byte[] scriptHash);
    }
}