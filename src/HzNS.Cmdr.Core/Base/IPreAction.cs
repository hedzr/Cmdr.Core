#nullable enable
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IPreAction
    {
        void PreInvoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}