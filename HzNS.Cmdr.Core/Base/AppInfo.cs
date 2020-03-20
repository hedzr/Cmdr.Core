namespace HzNS.Cmdr.Builder
{
    public class AppInfo : IAppInfo
    {
        public string AppName { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public int AppVersionInt { get; set; } = 0;
        public string BuildTime { get; set; } = "";
    }
}