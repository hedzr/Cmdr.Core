namespace HzNS.Cmdr.Base
{
    // ReSharper disable once UnusedType.Global
    public interface IOnSet
    {
        void OnSetHandler(Worker w, object newValue, object oldValue);
    }
}