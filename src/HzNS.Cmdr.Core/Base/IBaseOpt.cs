#nullable enable
using System;
using System.Collections.Generic;

namespace HzNS.Cmdr.Base
{
    public interface IBaseOpt
    {
        /// <summary>
        /// Long title for a command/flag/option.
        /// <br/>
        /// It is the part of keyword which
        /// can be extracted from `Option Store`.
        ///
        /// Such as: If there is an top-level boolean option with
        /// Long='verbose', we could extract its value via
        /// <code>worker.OptionStore.GetAs&lt;bool&gt;("verbose")</code>.
        /// <br/>
        /// 
        /// A long flag has two hyphens lead in the command-line. It likes:
        ///
        /// <code>$ app --verbose</code>
        /// 
        /// </summary>
        string Long { get; set; }

        /// <summary>
        /// Short title for a command/flag.
        /// <br/>
        /// A short flag has single hyphen lead in the command-line, just likes:
        ///
        /// <code>$ app -v</code>
        /// 
        /// </summary>
        string Short { get; set; }
        /// <summary>
        /// A command/flag can have many aliases.
        /// <br/>
        /// Aliases of a flag has double hyphens lead in the command-line, like long title.
        /// </summary>
        string[] Aliases { get; set; }

        /// <summary>
        /// Description must be single line string.
        /// <br/>
        /// For the multiple lines descriptions, use [DescriptionLong].
        /// <br/>
        /// The Description will be shown in the list of commands/flags in the help screen.
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Multiple lines, complex, detail descriptions for a command/flag.
        /// <br/>
        /// <code>Optional</code>
        /// </summary>
        string DescriptionLong { get; set; }
        /// <summary>
        /// Example text string for a command/flag.
        /// <br/>
        /// <code>Optional</code>
        /// </summary>
        string Examples { get; set; }

        /// <summary>
        /// Commands/flags can be group by [Group] text string.
        /// <br/>
        /// Separated by period ('.') char, the first part of a Group
        /// string will be striped while it has been showing in the
        /// help screen.
        /// <br/>
        /// The first part of the string, is for sorting internally.
        /// <br/>
        /// Typically, a full Group string can be:
        /// <code>0010.c++</code>,
        /// <code>0030.java</code>,
        /// <code>0050.golang</code>,
        /// ...
        /// <code>9999.Others</code>,
        /// </summary>
        string Group { get; set; }

        /// <summary>
        /// Hidden flag make a command/flag away from help screen.
        /// </summary>
        bool Hidden { get; set; }

        /// <summary>
        /// After the command-line arguments parsed, HitTitle will
        /// be set as same as the input.
        /// </summary>
        string HitTitle { get; }

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