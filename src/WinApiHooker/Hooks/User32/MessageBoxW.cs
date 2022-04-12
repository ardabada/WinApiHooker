using System;
using System.Runtime.InteropServices;

namespace WinApiHooker.Hooks.User32
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false)]
    public delegate int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);

    public abstract class MessageBoxWHandler : HookHandler<MessageBoxW>, IModuledHook
    {
        public string ModuleName => Modules.User32;

        public string ProcName => "MessageBoxW";
    }
}
