namespace NeoVM.Interop.Interfaces
{
    public interface IScriptContainer
    {
        byte[] GetMessage(uint iteration);
    }
}