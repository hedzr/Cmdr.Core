using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Base;

namespace HzNS.Cmdr.Handlers
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public abstract class WorkerFunctions : DefaultHandlers
    {
        public void ShowVersions(Worker w, params string[] remainArgs)
        {
            //
        }
        public void ShowBuildInfo(Worker w, params string[] remainArgs)
        {
            //
        }
        
        public void ShowHelpScreen(ICommand lastParsed, Worker w, params string[] remainArgs)
        {
            //
        }
        
        public void DumpTreeForAllCommands(Worker w, params string[] remainArgs)
        {
            //
        }

    }
}