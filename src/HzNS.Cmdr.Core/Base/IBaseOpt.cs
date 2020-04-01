#nullable enable
using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IBaseOpt
    {
        string Long { get; set; }

        string Short { get; set; }
        string[] Aliases { get; set; }

        string Description { get; set; }
        string DescriptionLong { get; set; }
        string Examples { get; set; }

        string Group { get; set; }

        bool Hidden { get; set; }

        string[] EnvVars { get; set; }

        /// <summary>
        /// Return false will terminate the command-line arguments parsing and exit the application.
        /// Another way is to raise a `ShouldBeStopException` exception in your Action/Pre/PostAction.
        /// </summary>
        Func<IBaseWorker, IBaseOpt, IEnumerable<string>, bool>? PreAction { get; set; }

        Action<IBaseWorker, IBaseOpt, IEnumerable<string>>? PostAction { get; set; }
        Action<IBaseWorker, IBaseOpt, IEnumerable<string>>? Action { get; set; }
        Action<IBaseWorker, IBaseOpt, object?, object?>? OnSet { get; set; }

        /// <summary>
        /// To point to the owner of an option, an ICommand object.
        /// </summary>
        ICommand? Owner { get; set; }

        IRootCommand? Root => FindRoot();

        IRootCommand? FindRoot();
        IFlag? FindFlag(string dottedKey, IBaseOpt? from = null);

        bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null);

        bool Match(string s, bool isLongOpt = false, bool aliasAsLong = true);
        bool Match(string str, int pos, int len, bool isLong = false, bool aliasAsLong = true);

        bool Match(ref string s, string input, int pos, bool isLong = false, bool aliasAsLong = true,
            bool enableCmdrGreedyLongFlag = false, bool incremental = true);

        string ToDottedKey();
        IEnumerable<string> ToKeys();
    }
}