using System.Collections.Generic;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Base
{
    public interface IPreAction
    {
        void PreInvoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}