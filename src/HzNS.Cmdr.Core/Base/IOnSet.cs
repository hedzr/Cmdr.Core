using HzNS.Cmdr.Internal;

namespace HzNS.Cmdr.Base
{
    // ReSharper disable once UnusedType.Global
    public interface IOnSet
    {
        void OnSetHandler(IBaseWorker w, object newValue, object oldValue);
    }
}