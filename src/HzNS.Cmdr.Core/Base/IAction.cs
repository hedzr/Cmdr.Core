#nullable enable
using System.Collections.Generic;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Base
{
    public interface IAction
    {
        void Invoke(IBaseWorker w, IEnumerable<string> remainsArgs);
    }
}