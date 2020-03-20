using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IPostAction
    {
        void PostInvoke(Worker w, IEnumerable<string> remainsArgs);
    }
}