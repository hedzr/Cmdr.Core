#nullable enable
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Base
{
    // ReSharper disable once UnusedType.Global
    public interface IOnSet
    {
        void OnSetHandler(IBaseWorker w, object newValue, object oldValue);
    }
}