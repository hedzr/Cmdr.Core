using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IPreAction
    {
        void PreInvoke(Worker w, IEnumerable<string> remainsArgs);
    }
}