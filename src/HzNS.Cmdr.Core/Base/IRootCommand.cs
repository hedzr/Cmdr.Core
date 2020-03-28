#nullable enable
namespace HzNS.Cmdr.Base
{
    public interface IRootCommand : ICommand
    {
        IRootCommand AddAppInfo(IAppInfo appInfo);
        public IAppInfo AppInfo { get; }

        // Worker Build();
    }
}