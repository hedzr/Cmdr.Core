#nullable enable
using System;
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

        IRootCommand FindRoot();

        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }
    }

    public interface IFlag : IBaseOpt // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
        object DefaultValue { get; set; }
        string PlaceHolder { get; set; }
    }

    public interface IBaseOpt
    {
        string Short { get; set; }
        string Long { get; set; }
        string[] Aliases { get; set; }

        string Description { get; set; }
        string DescriptionLong { get; set; }
        string Examples { get; set; }

        Func<Worker, IEnumerable<string>, bool>? PreAction { get; set; }
        Action<Worker, IEnumerable<string>>? PostAction { get; set; }
        Action<Worker, IEnumerable<string>>? Action { get; set; }
        Action<Worker, object, object>? OnSet { get; set; }

        ICommand? Owner { get; set; }

        bool Match(string s, bool isLongOpt = false);
    }

    public class AppInfo : IAppInfo
    {
        public string AppName { get; set; } = "";
        public string AppVersion { get; set; } = "";
        public int AppVersionInt { get; set; } = 0;
        public string BuildTime { get; set; } = "";
    }
}