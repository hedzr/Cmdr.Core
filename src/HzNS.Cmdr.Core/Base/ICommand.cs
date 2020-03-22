using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface ICommand : IBaseOpt
    {
        List<ICommand> SubCommands { get; set; }
        List<IFlag> Flags { get; set; }

        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }

        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag<T>(IFlag<T> flag);

        bool IsRoot { get; }
        IRootCommand? FindRoot();
        int FindLevel();

        bool IsEqual(string title);
        bool IsEqual(ICommand command);
    }
}