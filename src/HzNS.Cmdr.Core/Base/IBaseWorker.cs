#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Internal;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Base
{
    public interface IBaseWorker : ILoggable, IWorkerFunctions
    {
        int ParsedCount { get; set; }

        /// <summary>
        /// After Run() completed, the Parsed will be set to true if no errors occured.
        /// </summary>
        bool Parsed { get; set; }

        ICommand? ParsedCommand { get; set; }
        IFlag? ParsedFlag { get; set; }
        string[] RemainsArgs { get; set; }

        /// <summary>
        /// greedy mode: prefer to longest Long option.
        ///
        /// for example, think about there are two options: `--addr` and `--add`, in the
        /// greedy mode `--addr` will be picked for the input `--addr xxx`.
        /// just the opposite, `--add` && `--r` will be split out.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        bool EnableCmdrGreedyLongFlag { get; set; }

        bool EnableCmdrLogTrace { get; set; }
        bool EnableCmdrLogDebug { get; set; }
        bool EnableAutoBoxingWhenExtracting { get; set; }

        bool EnableDuplicatedCharThrows { get; set; }
        bool EnableEmptyLongFieldThrows { get; set; }
        bool EnableUnknownCommandThrows { get; set; }
        bool EnableUnknownFlagThrows { get; set; }

        /// <summary>
        /// The shortcut to Cmdr.Instance.Store, a hierarchical configurations holder.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        Store OptionsStore { get; }

        /// <summary>
        /// Store Prefixes can be used for the external config
        /// file, or serializing all options as string.
        /// <br/>
        /// In a yaml file, the prefix `app.ms` start a section
        /// which leads all your flags/options:
        /// <code>
        /// app:
        ///   ms:
        ///     verbose: false
        ///     debug: false
        ///     server:
        ///       port: 1571
        ///       start:
        ///         foreground: false
        /// </code>
        /// 
        /// </summary>
        string[] StorePrefixes { get; set; }

        bool AppVerboseMode { get; }
        bool AppQuietMode { get; }
        bool AppDebugMode { get; }
        bool AppTraceMode { get; }

        /// <summary>
        /// Sort commands/flags by alphabetic order.
        /// </summary>
        bool SortAsc { get; set; }

        /// <summary>
        /// Default tab stop position in help screen.
        /// </summary>
        int TabStop { get; set; }


        /// <summary>
        /// As is
        /// </summary>
        bool EnableExternalConfigFilesLoading { get; set; }

        /// <summary>
        /// After the primary config file found and loaded, cmdr will try loading
        /// from `conf.d`/[ConfigFileAutoSubDir] sub-directory.
        /// <br/>
        /// And [NoPopulationAfterFirstExternalConfigLocationLoaded] will break this action. 
        /// </summary>
        bool NoPopulationAfterFirstExternalConfigLocationLoaded { get; set; }

        /// <summary>
        /// The primary config file folder of $APPNAME.yml, .yaml, .json.
        /// Cmdr.Core will watch its sub-directory `conf.d` and all files in it.
        ///
        /// see also: [ConfigFileAutoSubDir], 
        /// </summary>
        string PrimaryConfigDir { get; }

        /// <summary>
        /// Customizable sub-directory name for configurations, following the [PrimaryConfigDir]
        /// </summary>
        string ConfigFileAutoSubDir { get; set; }

        /// <summary>
        /// Cmdr will search the app primary config file at these locations.
        /// <br/>
        /// Once a valid config file (*.yml,*.yaml,*.json) found, the [PrimaryConfigDir]
        /// will be set.
        /// <br/>
        /// Cmdr would try loading any config files under its [ConfigFileAutoSubDir].
        /// </summary>
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        // ReSharper disable once UnusedMember.Global
        string[] ConfigFileLocations { get; set; }


        // ReSharper disable once CollectionNeverUpdated.Local
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        Dictionary<ICommand, Xref> xrefs { get; }

        IRootCommand RootCommand { get; }

        IFlag? FindFlag(string dottedKey, IBaseOpt? from = null);


        /// <summary>
        /// <code>bool OnDuplicatedCommandChar(IBaseWorker worker, ICommand command,
        ///     bool isShort, string matchingArg)</code>
        /// returning true means the event has been processed.
        /// </summary>
        Func<IBaseWorker, ICommand, bool, string, bool>? OnDuplicatedCommandChar { get; set; }

        /// <summary>
        /// <code>bool OnDuplicatedFlagChar(IBaseWorker worker,
        ///     ICommand command, IFlag flag,
        ///     bool isShort, string matchingArg)</code>
        /// returning true means the event has been processed. 
        /// </summary>
        Func<IBaseWorker, ICommand, IFlag, bool, string, bool>? OnDuplicatedFlagChar { get; set; }


        /// <summary>
        /// <code>bool OnCommandCannotMatched(ICommand parsedCommand, string matchingArg)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBeMadeStatic.Global
        Func<ICommand, string, bool>? OnCommandCannotMatched { get; set; }

        /// <summary>
        /// <code>bool OnCommandCannotMatched(ICommand parsingCommand,
        ///     string fragment, bool isShort, string matchingArg)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBeMadeStatic.Global
        Func<ICommand, string, bool, string, bool>? OnFlagCannotMatched { get; set; }

        /// <summary>
        /// <code>bool OnSuggestingForCommand(object worker,
        ///     Dictionary&lt;string, ICommand&gt; dataset, string token)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBeMadeStatic.Global
        Func<object, Dictionary<string, ICommand>, string, bool>? OnSuggestingForCommand { get; set; }

        /// <summary>
        /// <code>bool OnSuggestingForFlag(object worker,
        ///     Dictionary&lt;string, IFlag&gt; dataset, string token)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBeMadeStatic.Global
        Func<object, Dictionary<string, IFlag>, string, bool>? OnSuggestingForFlag { get; set; }
    }
}