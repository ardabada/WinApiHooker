using System;
using System.Diagnostics;

namespace WinApiHooker
{
    public class DefaultHookFactory : IHookFactory
    {
        private static bool Is32Bit = IntPtr.Size == 4;

        public IHook Create(string moduleName, string procName, IHookBehaviour hook)
        {
            return Create(GetCurrentProcessHandle(), moduleName, procName, hook);
        }

        public IHook Create<THook>(string moduleName, string procName) where THook : IHookBehaviour, new()
        {
            return Create(GetCurrentProcessHandle(), moduleName, procName, new THook());
        }

        public IHook Create(IntPtr processHandle, string moduleName, string procName, IHookBehaviour hook)
        {
            if (!Is32Bit) throw new NotSupportedException("Only x86 is supported");

            return Create(processHandle, moduleName, procName, hook.GetAlteredBehaviour());
        }

        public IHook Create<THook>(IntPtr processHandle, string moduleName, string procName) where THook : IHookBehaviour, new()
        {
            return Create(processHandle, moduleName, procName, new THook());
        }

        public IHook Create(IModuledHook hook)
        {
            return Create(GetCurrentProcessHandle(), hook);
        }

        public IHook Create<THook>() where THook : IModuledHook, new()
        {
            return Create(new THook());
        }

        public IHook Create(IntPtr processHandle, IModuledHook hook)
        {
            return Create(processHandle, hook.ModuleName, hook.ProcName, hook);
        }

        public IHook Create<THook>(IntPtr processHandle) where THook : IModuledHook, new()
        {
            return Create(processHandle, new THook());
        }

        public IHook Create<THook>(string moduleName, string procName, THook hook) where THook : Delegate
        {
            return Create(GetCurrentProcessHandle(), moduleName, procName, hook);
        }

        public IHook Create<THook>(IntPtr processHandle, string moduleName, string procName, THook hook) where THook : Delegate
        {
            if (!Is32Bit) throw new NotSupportedException("Only x86 is supported");

            return new Native32Hook(processHandle, moduleName, procName, hook);
        }

        private IntPtr GetCurrentProcessHandle()
        {
            return Process.GetCurrentProcess().Handle;
        }
    }
}
