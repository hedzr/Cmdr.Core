using System.Collections.Generic;

namespace HzNS.Cmdr.Action
{
    public interface IAction
    {
        void Invoke(Worker w, IEnumerable<string> remainsArgs);
    }
    
    public interface IPreAction
    {
        void Invoke(Worker w, IEnumerable<string> remainsArgs);
    }
    
    public interface IPostAction
    {
        void Invoke(Worker w, IEnumerable<string> remainsArgs);
    }
    
    public interface IOnSet
    {
        void Invoke(Worker w, object newValue, object oldValue);
    }
}