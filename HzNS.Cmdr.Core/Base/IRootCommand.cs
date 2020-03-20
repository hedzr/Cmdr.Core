using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    public interface IRootCommand : ICommand
    {
        IRootCommand AddAppInfo(IAppInfo appInfo);

        // Worker Build();
    }
}