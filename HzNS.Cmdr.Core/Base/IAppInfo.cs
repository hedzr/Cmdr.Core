namespace HzNS.Cmdr.Base
{
    public interface IAppInfo
    {
        string AppName { get; set; }
        string AppVersion { get; set; }
        int AppVersionInt { get; set; }
        string BuildTime { get; set; }
    }
}