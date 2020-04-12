#nullable enable
using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface ICommand : IBaseOpt
    {
        List<ICommand> SubCommands { get; }
        List<IFlag> Flags { get; }

        /// <summary>
        /// In the help screen, Usages line looks like:
        /// <code>
        /// {appName} [Commands] [tail args]|[Options]|[Parent/Global Options]
        /// </code> 
        /// </summary>
        string TailArgs { get; }

        List<IFlag> RequiredFlags { get; }
        Dictionary<string, List<IFlag>> ToggleableFlags { get; }

        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }
        // string HitTitle { get; }

        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag<T>(IFlag<T> flag, bool required = false);
        ICommand AddAction(Action<IBaseWorker, IBaseOpt, IEnumerable<string>> action);

        bool IsRoot { get; }

        // IRootCommand? FindRoot();
        int FindLevel();

        bool IsEqual(string title);
        bool IsEqual(ICommand command);
    }
}