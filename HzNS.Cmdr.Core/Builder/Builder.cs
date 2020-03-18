using System.ComponentModel.Design.Serialization;

namespace HzNS.Cmdr.Builder
{
    public interface IBuilder
    {
        IRoot Root();
    }

    public interface IRoot
    {
        IRoot AddCommand(ICommand cmd);
        IRoot AddFlag(IFlag flag);
        
        // Worker Build();
    }
    
    
    public interface ICommand
    {
        
    }
    
    public interface IFlag
    {
        //
    }
    
}