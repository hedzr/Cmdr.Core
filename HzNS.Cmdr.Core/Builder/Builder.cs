using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace HzNS.Cmdr.Builder
{
    public interface IBuilder
    {
        IRootCommand Root();
    }

    public interface IRootCommand : ICommand
    {
        IRootCommand AddAppInfo(IAppInfo appInfo);

        // Worker Build();
    }

    public interface IAppInfo
    {
        string AppName { get; set; }
        string AppVersion { get; set; }
        int AppVersionInt { get; set; }
        string BuildTime { get; set; }
    }

    public interface ICommand : IBaseOpt
    {
        List<ICommand> SubCommands { get; set; }
        List<IFlag> Flags { get; set; }

        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag(IFlag flag);
    }

    public interface IFlag : IBaseOpt // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
    }

    public interface IBaseOpt
    {
        string Short { get; set; }
        string Long { get; set; }
        string[] Aliases { get; set; }

        string Description { get; set; }
        string DescriptionLong { get; set; }
        string Examples { get; set; }

        ICommand Owner { get; set; }

        bool Match(string s, bool isLongOpt = false);
    }

    public class AppInfo : IAppInfo
    {
        public string AppName { get; set; }
        public string AppVersion { get; set; }
        public int AppVersionInt { get; set; }
        public string BuildTime { get; set; }
    }
}