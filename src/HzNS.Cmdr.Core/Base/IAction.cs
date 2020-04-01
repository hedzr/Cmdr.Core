#nullable enable
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IAction
    {
        void Invoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}