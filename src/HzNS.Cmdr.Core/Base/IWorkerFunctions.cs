using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IWorkerFunctions
    {
        void ShowVersionsScreen(IBaseWorker w, IList<string> remainArgs);

        void ShowBuildInfoScreen(IBaseWorker w, IList<string> remainArgs);

        void ShowHelpScreen(IBaseWorker w, IList<string> remainArgs);

        void ShowTreeDumpScreenForAllCommands(IBaseWorker w, IList<string> remainArgs);

        public bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag?, int, bool>? flagsWatcher = null);
    }
}