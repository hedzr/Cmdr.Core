using System.Collections.Generic;
using HzNS.Cmdr.Internal;

namespace HzNS.Cmdr.Base
{
    public interface IPostAction
    {
        void PostInvoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}