using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Builder
{
    public interface IRootCommand : ICommand
    {
        IRootCommand AddAppInfo(IAppInfo appInfo);

        // Worker Build();
    }

    public interface ICommand : IBaseOpt
    {
        List<ICommand> SubCommands { get; set; }
        List<IBaseFlag> Flags { get; set; }

        // ReSharper disable once InconsistentNaming
        string backtraceTitles { get; }

        ICommand AddCommand(ICommand cmd);
        ICommand AddFlag<T>(IFlag<T> flag);

        IRootCommand? FindRoot();
    }

    public interface IFlag<T> : IBaseFlag // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
        T DefaultValue { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IBaseFlag : IBaseOpt
    {
        string PlaceHolder { get; set; }
        object? getDefaultValue();
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
        Action<Worker, object?, object?>? OnSet { get; set; }

        /// <summary>
        /// To point to the owner of an option, an ICommand object.
        /// </summary>
        ICommand? Owner { get; set; }

        bool Match(string s, bool isLongOpt = false);
    }
    
    public interface IAppInfo
    {
        string AppName { get; set; }
        string AppVersion { get; set; }
        int AppVersionInt { get; set; }
        string BuildTime { get; set; }
    }

}