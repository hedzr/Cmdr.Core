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

        /// <summary>
        /// Fast references for internal usages
        /// </summary>
        List<IFlag> RequiredFlags { get; }
        /// <summary>
        /// Fast references for internal usages
        /// </summary>
        Dictionary<string, List<IFlag>> ToggleableFlags { get; }

        /// <summary>
        /// for a command/flag, sample returns is "tags mode sub1 sub2".
        /// </summary>
        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }
        
        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag<T>(IFlag<T> flag, bool required = false);
        ICommand AddAction(Action<IBaseWorker, IBaseOpt, IEnumerable<string>> action);

        bool IsRoot { get; }

        int FindLevel();

        bool IsEqual(string title);
        bool IsEqual(ICommand command);
    }
}