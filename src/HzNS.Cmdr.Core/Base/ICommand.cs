using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface ICommand : IBaseOpt
    {
        List<ICommand> SubCommands { get; }
        List<IFlag> Flags { get; }

        List<IFlag> RequiredFlags { get; }
        Dictionary<string, List<IFlag>> ToggleableFlags { get; }

        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }
        string HitTitle { get; }

        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag<T>(IFlag<T> flag, bool required = false);

        bool IsRoot { get; }
        IRootCommand? FindRoot();
        int FindLevel();

        bool IsEqual(string title);
        bool IsEqual(ICommand command);
    }
}