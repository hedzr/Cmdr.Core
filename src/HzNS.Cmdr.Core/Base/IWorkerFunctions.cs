using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IWorkerFunctions
    {
        void ShowVersionsScreen(IBaseWorker w, IEnumerable<string> remainArgs);

        void ShowBuildInfoScreen(IBaseWorker w, IEnumerable<string> remainArgs);

        void ShowHelpScreen(IBaseWorker w, IEnumerable<string> remainArgs);

        void ShowTreeDumpScreenForAllCommands(IBaseWorker w, IEnumerable<string> remainArgs);

        public bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null);
    }
}