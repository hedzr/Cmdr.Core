#nullable enable
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IPostAction
    {
        void PostInvoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}