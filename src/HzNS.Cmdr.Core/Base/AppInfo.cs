using System;
using System.Reflection;
using HzNS.Cmdr.Tool;

namespace HzNS.Cmdr.Base
{
    public class AppInfo : IAppInfo
    {
        public string AppName { get; set; } = "";

        public string AppVersion =>
            string.IsNullOrWhiteSpace(VersionUtil.InformationalVersion)
                ? VersionUtil.InformationalVersion
                : VersionUtil.AssemblyVersion; // GetType().Assembly.GetName().Version.ToString();

        public int AppVersionInt { get; set; } = 0;
        public string Author { get; set; } = "";
        public string Copyright { get; set; } = "";
        public DateTime BuildTimestamp => VersionUtil.BuildTimestamp;
        public DateTime LinkerTimestampUtc => VersionUtil.LinkerTimestampUtc;
        public string BuildRelease { get; set; } = "";
        public string BuildVcsVersion { get; set; } = "";
        public string BuildVcsHash { get; set; } = "";
        public string BuildTags { get; set; } = "";
        public string BuildArgs { get; set; } = "";
        public string Builder { get; set; } = "";
    }
}