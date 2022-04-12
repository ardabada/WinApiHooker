namespace WinApiHooker
{
    public interface IModuledHook : IHookBehaviour
    {
        string ModuleName { get; }

        string ProcName { get; }
    }
}
