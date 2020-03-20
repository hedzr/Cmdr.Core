using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IAction
    {
        void Invoke(Worker w, IEnumerable<string> remainsArgs);
    }
}