namespace WinApiHooker
{
    public interface IHook
    {
        bool Attach();

        bool Detach();
    }
}
