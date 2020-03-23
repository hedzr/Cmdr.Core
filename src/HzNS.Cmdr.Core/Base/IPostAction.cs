using System.Collections.Generic;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Base
{
    public interface IPostAction
    {
        void PostInvoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}