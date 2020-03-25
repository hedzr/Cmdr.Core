namespace HzNS.Cmdr.Base
{
    public class AppInfo : IAppInfo
    {
        public string AppName { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public int AppVersionInt { get; set; } = 0;
        public string Author { get; set; } = "";
        public string Copyright { get; set; } = "";
        public string BuildTimestamp { get; set; } = "";
        public string BuildRelease { get; set; } = "";
        public string BuildVcsVersion { get; set; } = "";
        public string BuildVcsHash { get; set; } = "";
        public string BuildTags { get; set; } = "";
        public string BuildArgs { get; set; } = "";
        public string Builder { get; set; } = "";
    }
}