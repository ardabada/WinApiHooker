using System;

namespace WinApiHooker.Hooks
{
    public abstract class HookHandler<T> : IHookBehaviour where T : Delegate
    {
        public Delegate GetAlteredBehaviour()
        {
            return HandleAltered();
        }

        public abstract T HandleAltered();
    }
}
