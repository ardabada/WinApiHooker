using System;

namespace WinApiHooker
{
    public interface IHookFactory
    {
        IHook Create(string moduleName, string procName, IHookBehaviour hook);

        IHook Create<THook>(string moduleName, string procName) where THook : IHookBehaviour, new();

        IHook Create(IntPtr processHandle, string moduleName, string procName, IHookBehaviour hook);

        IHook Create<THook>(IntPtr processHandle, string moduleName, string procName) where THook : IHookBehaviour, new();

        IHook Create(IModuledHook hook);

        IHook Create<THook>() where THook : IModuledHook, new();

        IHook Create(IntPtr processHandle, IModuledHook hook);

        IHook Create<THook>(IntPtr processHandle) where THook : IModuledHook, new();

        IHook Create<THook>(string moduleName, string procName, THook hook) where THook : Delegate;

        IHook Create<THook>(IntPtr processHandle, string moduleName, string procName, THook hook) where THook : Delegate;
    }
}
