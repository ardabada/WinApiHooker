using System;

namespace WinApiHooker
{
    public interface IHookBehaviour
    {
        Delegate GetAlteredBehaviour();
    }
}
