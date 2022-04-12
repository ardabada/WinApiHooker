using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinApiHooker
{
    internal class Native32Hook : IHook
    {
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        private const byte JMP = 0xE9;
        private const byte NOP = 0x90;
        private const byte RET = 0xC3;
        private const int SIZE = 6;
        private const int JMP_SIZE = 5;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        private readonly UIntPtr DW_SIZE = (UIntPtr)SIZE;
        private readonly IntPtr oldBytes = Marshal.AllocHGlobal(SIZE);
        private readonly string moduleName;
        private readonly string procName;
        private readonly Delegate hook;
        private readonly IntPtr hProcess;

        private bool isAttached = false;
        private uint oldProtect;
        private uint newProtect;
        private IntPtr originalFunction;

        internal Native32Hook(string moduleName, string procName, Delegate hook)
            : this(Process.GetCurrentProcess().Handle, moduleName, procName, hook)
        { }

        internal Native32Hook(IntPtr hProcess, string moduleName, string procName, Delegate hook)
        {
            this.moduleName = moduleName;
            this.procName = procName;
            this.hook = hook;
            this.hProcess = hProcess;
        }

        public bool Attach()
        {
            try
            {
                if (isAttached) return isAttached;

                IntPtr pOrigMBAddress = GetProcAddress(GetModuleHandle(moduleName), procName);
                if (pOrigMBAddress == IntPtr.Zero)
                {
                    //TODO: check if same process (use LoadLibraryEx? )
                    pOrigMBAddress = GetProcAddress(LoadLibrary(moduleName), procName);
                    if (pOrigMBAddress == IntPtr.Zero) return false;
                }

                this.originalFunction = pOrigMBAddress;
                var hookAddress = Marshal.GetFunctionPointerForDelegate(hook);

                this.isAttached = BeginRedirect(hookAddress);

                return this.isAttached;
            }
            catch (Exception ex)
            {
                throw new Win32Exception(string.Format("Failed to attach hook for \"{0}\" on module \"{1}\". See inner exception.", procName, moduleName), ex);
            }
        }

        public bool Detach()
        {
            try
            {
                if (!isAttached) return false;

                VirtualProtectEx(hProcess, originalFunction, DW_SIZE, newProtect, out oldProtect);

                CopyMemory(originalFunction, oldBytes, SIZE);

                VirtualProtectEx(hProcess, originalFunction, DW_SIZE, oldProtect, out newProtect);

                return true;
            }
            catch (Exception ex)
            {
                throw new Win32Exception(string.Format("Failed to detach hook for \"{0}\" on module \"{1}\". See inner exception.", procName, moduleName), ex);
            }
        }

        private bool BeginRedirect(IntPtr newFunction)
        {
            byte[] tempJmp = { JMP, NOP, NOP, NOP, NOP, RET };

            int jmpSize = newFunction.ToInt32() - originalFunction.ToInt32() - JMP_SIZE;

            VirtualProtectEx(hProcess, originalFunction, DW_SIZE, PAGE_EXECUTE_READWRITE, out oldProtect);

            CopyMemory(oldBytes, originalFunction, SIZE);

            var bytes = BitConverter.GetBytes(jmpSize);

            Array.Copy(bytes, 0, tempJmp, 1, JMP_SIZE - 1);

            Marshal.Copy(tempJmp, 0, originalFunction, SIZE);

            VirtualProtectEx(hProcess, originalFunction, DW_SIZE, oldProtect, out newProtect);

            return true;
        }
    }
}
