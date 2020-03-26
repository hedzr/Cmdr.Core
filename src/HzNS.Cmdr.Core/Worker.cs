#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Internal;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Enrichers;
using HzNS.Cmdr.Tool.Ext;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using YamlDotNet.RepresentationModel;

namespace HzNS.Cmdr
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public sealed class Worker : WorkerFunctions, IDefaultHandlers, IDefaultMatchers
    {
        public const string FirstUnsortedGroup = "0111.Unsorted";
        public const string SysMgmtGroup = "y099.System";
        public const string SysMiscGroup = "y000.Misc";
        public const string LastUnsortedGroup = "z111.Unsorted";


        private IRootCommand _root;

        public Worker(IRootCommand root)
        {
            _root = root;
            log = Log.Logger;
        }


        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        // ReSharper disable once UnusedMember.Global
        public IRootCommand RootCommand => _root;

        public int ParsedCount { get; set; }
        public bool Parsed { get; set; }
        public ICommand? ParsedCommand { get; set; }
        public IFlag? ParsedFlag { get; set; }
        public string PrimaryConfigDir { get; internal set; } = "";


        /// <summary>
        /// greedy mode: prefer to longest Long option.
        ///
        /// for example, think about there are two options: `--addr` and `--add`, in the
        /// greedy mode `--addr` will be picked for the input `--addr xxx`.
        /// just the opposite, `--add` && `--r` will be split out.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public bool EnableCmdrGreedyLongFlag
        {
            get => DefaultMatchers.EnableCmdrGreedyLongFlag;
            set => DefaultMatchers.EnableCmdrGreedyLongFlag = value;
        }

        public bool EnableCmdrLogTrace
        {
            get => DefaultMatchers.EnableCmdrLogTrace;
            set => DefaultMatchers.EnableCmdrLogTrace = value;
        }

        public bool EnableCmdrLogDebug
        {
            get => DefaultMatchers.EnableCmdrLogDebug;
            set => DefaultMatchers.EnableCmdrLogDebug = value;
        }

        
        public bool EnableAutoBoxingWhenExtracting
        {
            get => Cmdr.Instance.EnableAutoBoxingWhenExtracting;
            set => Cmdr.Instance.EnableAutoBoxingWhenExtracting = value;
        }

        
        public string[] Prefixes
        {
            get => Cmdr.Instance.Store.Prefixes;
            set => Cmdr.Instance.Store.Prefixes = value;
        }
        
        
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public Store CmdrOptionStore => Cmdr.Instance.Store;


        public bool EnableDuplicatedCharThrows { get; set; } = false;
        public bool EnableEmptyLongFieldThrows { get; set; } = false;
        public bool EnableUnknownCommandThrows { get; set; } = false;
        public bool EnableUnknownFlagThrows { get; set; } = false;
        
        public int TabStop { get; set; } = 45;

        public bool AppVerboseMode => CmdrOptionStore.GetAs("verbose", false);
        public bool AppQuietMode => CmdrOptionStore.GetAs("quiet", false);
        public bool AppDebugMode => CmdrOptionStore.GetAs("debug", false);
        public bool AppTraceMode => CmdrOptionStore.GetAs("trace", false);

        public bool EnableExternalConfigFilesLoading { get; set; } = true;
        public bool NoPopulationAfterFirstExternalConfigLocationLoaded { get; set; } = true;

        public string ConfigFileAutoSubDir { get; set; } = "conf.d";

        private readonly string[] configFileSuffixes =
        {
            ".yaml", ".yml", ".json",
        };

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        // ReSharper disable once UnusedMember.Global
        public string[] ConfigFileLocations
        {
            get => _configFileLocations;
            set => _configFileLocations = value;
        }

        private string[] _configFileLocations =
        {
            "./ci/etc/$APPNAME/$APPNAME.yml", // for developer
            "/etc/$APPNAME/$APPNAME.yml", // regular location: /etc/$APPNAME/$APPNAME.yml
            "/usr/local/etc/$APPNAME/$APPNAME.yml", // regular macOS HomeBrew location
            "$HOME/.config/$APPNAME/$APPNAME.yml", // per user: $HOME/.config/$APPNAME/$APPNAME.yml
            "$HOME/.$APPNAME/$APPNAME.yml", // ext location per user
            "$THIS/$APPNAME.yml", // executable directory
            "$APPNAME.yml", // current directory
        };

        // see also RegisterExternalConfigurationsLoader()
        private readonly List<Action<IBaseWorker, IRootCommand>> _externalConfigurationsLoaders =
            new List<Action<IBaseWorker, IRootCommand>>();

        public bool DebuggerAttached => Debugger.IsAttached;


        internal Worker runOnce()
        {
            // NOTE that the logger `log` is not ready yet at this time.
            ColorifyEnabler.Enable();

            DefaultMatchers.EnableCmdrLogTrace = Util.GetEnvValueBool("CMDR_TRACE");
            if (DefaultMatchers.EnableCmdrLogTrace)
                DefaultMatchers.EnableCmdrLogDebug = true;
            DefaultMatchers.EnableCmdrLogDebug = Util.GetEnvValueBool("CMDR_DEBUG");
            CmdrOptionStore.Set("debug", Util.GetEnvValueBool("DEBUG"));
            CmdrOptionStore.Set("trace", Util.GetEnvValueBool("TRACE"));
            CmdrOptionStore.Set("verbose", Util.GetEnvValueBool("VERBOSE"));
            CmdrOptionStore.Set("verbose-level", Util.GetEnvValueInt("VERBOSE_LEVEL", 5));
            CmdrOptionStore.Set("quiet", Util.GetEnvValueBool("QUIET"));

            return this;
        }

        public Worker With(IRootCommand rootCommand)
        {
            _root = rootCommand;
            preloadCommandDefinitions();
            return this;
        }

        // ReSharper disable once UnusedMember.Local
        private void f4()
        {
            log.Information("YES IT IS");
            // B /= A;
            throw new System.Exception("test");
        }

        public void Run(string[] args, Func<int>? postRun = null)
        {
            // Entry.Log.Information("YES IT IS");
            // log.Information("YES IT IS");
            // f4();
            if (_root == null)
                return;

            // ReSharper disable once NotAccessedVariable
            var position = 0;
            try
            {
                position = this.match(_root, args, position, 1);

                if (position < 0)
                {
                    // -1-position
                    // m 0: -1 => 0 (x+1)
                    // m 1: -2 => 1
                    var pos = -(1 + position);
                    if (!this.onCommandMatched(args, pos > 0 ? pos + 1 : pos, "", ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }
                else
                {
                    if (!this.onCommandMatched(args, position, args[position - 1], ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }

                Parsed = true;
            }
            catch (WantHelpScreenException ex)
            {
                // f4();
                // show help screen
                this.logDebug("showing the help screen ...");
                Parsed = true;
                ShowHelpScreen(this, ex.RemainArgs);
            }
            catch (ShouldBeStopException)
            {
                // not an error
                Parsed = true;
            }
            catch (CmdrException ex)
            {
                this.logError(ex, "Error occurs");
            }
            catch (System.Exception ex)
            {
                this.logError(ex, $"args: {args}, position: {position}");
                throw;
            }
            finally
            {
                ShowDebugDumpFragment(this);

                try
                {
                    postRun?.Invoke();
                }
                finally
                {
                    _shouldTerminate.Set();
                    CancelFileWatcher();
                    ColorifyEnabler.Reset();
                }
            }
        }

        public override bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null)
        {
            return walkFor(parent ?? _root, commandsWatcher, flagsWatcher);
        }


        #region logger

        // public Worker(ILogger log)
        // {
        //     this.log = log;
        // }

        // #pragma warning disable 649
        //         internal ILogger _log;
        // #pragma warning restore 649
        //
        // public ILogger Log { get; set; }

        public ILogger log { get; private set; }

        // public ILogger log;
        // public LogWrapper log;

        public Worker UseSerilog(Func<LoggerConfiguration, Logger>? func = null)
        {
            if (func == null)
                log = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.WithCaller()
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (at {Caller} in {SourceFileName}:line {SourceFileLineNumber}){NewLine}{Exception}")
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    //.WriteTo.Console()
                    .CreateLogger();
            else
                log = func.Invoke(new LoggerConfiguration());

            var builder = new ContainerBuilder();
            builder.RegisterLogger(autowireProperties: true);

            // if (propertyFunc == null) throw new ArgumentNullException(nameof(propertyFunc));
            // var propertyName = ((propertyFunc.Body as UnaryExpression)?.Operand as MemberExpression)?.Member.Name;
            // var props = this.GetType().GetProperties();
            // try
            // {
            //     foreach (var p in props)
            //     {
            //         if (p.SetMethod == null) continue;
            //         
            //         object value;
            //         if (p.Name.Equals(propertyName))
            //         {
            //             value = Convert.ChangeType(propertyValue, p.PropertyType);
            //         }
            //         else
            //         {
            //             Type t = p.PropertyType;
            //             value = t.IsValueType ? Activator.CreateInstance(t) : (t.Name.ToLower().Equals("string") ? string.Empty : null);
            //         }
            //         p.SetValue(this, value);
            //     }
            // }
            // catch (Exception)
            // {
            //     throw;
            // }

            return this;
        }

        #endregion


        #region helpers for With(), Xref

        #region xref class and member container

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once ConvertToAutoProperty
        public Dictionary<ICommand, Xref> xrefs => _xrefs;
        private readonly Dictionary<ICommand, Xref> _xrefs = new Dictionary<ICommand, Xref>();

        #endregion

        #region Preloading, Build Xref

        [SuppressMessage("ReSharper", "InvertIf")]
        private void preloadCommandDefinitions()
        {
            BuiltinOptions.InsertAll(_root);
            collectAndBuildXref(_root);
            loadExternalConfigures();
        }

        private void loadExternalConfigures()
        {
            preparePrivateEnvVars();
            loadExternalConfigurationsFromPredefinedLocations(this, _root);
            foreach (var loader in _externalConfigurationsLoaders)
            {
                loader(this, _root);
            }
        }

        [SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
        private void preparePrivateEnvVars()
        {
            string? currDir = Environment.CurrentDirectory;

#if DEBUG
            // the predefined EnvVar 'CURR_DIR' will prevent the debugging source dir searching action:
            if (Environment.GetEnvironmentVariable("CURR_DIR") == null)
            {
                var pos = currDir.IndexOf("/bin/Debug", StringComparison.Ordinal);
                if (pos >= 0) currDir = currDir.Substring(0, pos);
                pos = currDir.IndexOf("/bin/Release", StringComparison.Ordinal);
                if (pos >= 0) currDir = currDir.Substring(0, pos);
                pos = currDir.IndexOf("/bin/", StringComparison.Ordinal);
                if (pos >= 0) currDir = currDir.Substring(0, pos);
                var projDir = currDir;

                string? dir = currDir;
                while (!string.IsNullOrWhiteSpace(dir))
                {
                    if (Directory.GetFiles(dir, "*.sln", SearchOption.AllDirectories).Length > 0)
                    {
                        currDir = dir;
                        break;
                    }

                    dir = Path.GetDirectoryName(dir);
                }

                Environment.SetEnvironmentVariable("PROJ_DIR", projDir);
                Environment.SetEnvironmentVariable("CURR_DIR", currDir);
            }
#else
            Environment.SetEnvironmentVariable("PROJ_DIR", currDir);
            Environment.SetEnvironmentVariable("CURR_DIR", currDir);
#endif

            var exe = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            var exeDir = Path.GetDirectoryName(exe) ?? Path.Join(Environment.CurrentDirectory, "1");
            Environment.SetEnvironmentVariable("THIS", exe);
            Environment.SetEnvironmentVariable("APPNAME", _root.AppInfo.AppName);
            Environment.SetEnvironmentVariable("APPVERSION", _root.AppInfo.AppVersion);
            Environment.SetEnvironmentVariable("TEAM_AUTHOR", _root.AppInfo.Author);
            Environment.SetEnvironmentVariable("EXECUTABLE_DIR", exeDir);

            if (!Util.GetEnvValueBool("CMDR_VERBOSE")) return;

            foreach (DictionaryEntry? e in Environment.GetEnvironmentVariables())
            {
                this.logDebug($"  - ENV[{e?.Key}] = {e?.Value}");
            }
        }

        private void loadExternalConfigurationsFromPredefinedLocations(IBaseWorker w, IRootCommand root)
        {
            if (!EnableExternalConfigFilesLoading) return;

            if (_configFileLocations.Where(location =>
                    loadExternalConfigurationsFrom(location, w, root))
                .Any(location => NoPopulationAfterFirstExternalConfigLocationLoaded))
            {
                // ReSharper disable once RedundantJumpStatement
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="w"></param>
        /// <param name="root"></param>
        /// <param name="overwriteExists"></param>
        /// <returns></returns>
        public bool LoadExternalConfigurationsFile(string filepath, IBaseWorker? w = null, IRootCommand? root = null,
            bool overwriteExists = true)
        {
            return loadExternalConfigurationsFile(filepath, w ?? this, root ?? _root, overwriteExists);
        }

        private bool loadExternalConfigurationsFile(string filepath, IBaseWorker w, IRootCommand root,
            bool overwriteExists)
        {
            if (!File.Exists(filepath)) return false;

            using var input = new StreamReader(filepath);
            this.logDebug($"loading external config file: {filepath}");

            switch (Path.GetExtension(filepath))
            {
                case ".yml":
                case ".yaml":
                {
                    // using var input = new StringReader(yamlString);
                    var yaml = new YamlStream();
                    yaml.Load(input);
                    if (yaml.Documents[0].RootNode is YamlMappingNode mapping)
                    {
                        var usedConfigDir = Path.GetDirectoryName(filepath);
                        Debug.WriteLine($"usedConfigDir: {usedConfigDir}");
                        Environment.SetEnvironmentVariable("CONFIG_DIR", usedConfigDir);
                        Environment.SetEnvironmentVariable("CONF_DIR", usedConfigDir);
                        PrimaryConfigDir = usedConfigDir ?? string.Empty;

                        var ok = mergeMappingNode(mapping, new string[] { }, overwriteExists);
                        return ok;
                    }

                    break;
                }
                case ".json":
                {
                    // dynamic result = JsonConvert.DeserializeObject(input.ReadToEnd());

                    // using var reader = new JsonTextReader(input);
                    // var o2 = (JObject) JToken.ReadFrom(reader);

                    var o = JObject.Parse(input.ReadToEnd());

                    var usedConfigDir = Path.GetDirectoryName(filepath);
                    Debug.WriteLine($"usedConfigDir: {usedConfigDir}");
                    Environment.SetEnvironmentVariable("CONFIG_DIR", usedConfigDir);
                    Environment.SetEnvironmentVariable("CONF_DIR", usedConfigDir);
                    PrimaryConfigDir = usedConfigDir ?? string.Empty;

                    var ok = mergeMappingNode(o, new string[] { }, overwriteExists);
                    return ok;
                }
            }

            return false;
        }


        private bool loadExternalConfigurationsFrom(string location, IBaseWorker w, IRootCommand root)
        {
            var s1 = Regex.Replace(location, @"\$([A-Za-z0-9_]+)", @"%$1%",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var loc = Environment.ExpandEnvironmentVariables(s1).EatEnd(".yml");
            if (loc[0] == '.') loc = Path.Join(Environment.GetEnvironmentVariable("CURR_DIR"), loc);

            // this.logDebug($"loading: {location} | CURR_DIR = {Environment.GetEnvironmentVariable("CURR_DIR")}");
            // this.logDebug($"         loc = {loc}");

            var ok = configFileSuffixes.Select(suffix => string.Concat(loc, suffix)).Any(filepath =>
                loadExternalConfigurationsFile(filepath, w, root, false) &&
                NoPopulationAfterFirstExternalConfigLocationLoaded);

            if (ok && Directory.Exists(Path.Join(PrimaryConfigDir, ConfigFileAutoSubDir)))
            {
                watchThem(Path.Join(PrimaryConfigDir, ConfigFileAutoSubDir));
            }

            return ok;
        }

        #region FilWatcher

        private void watchThem(string dir)
        {
            foreach (var suffix in configFileSuffixes)
            {
                var files = Directory.GetFiles(dir, "*" + suffix, SearchOption.TopDirectoryOnly);
                foreach (var filepath in files)
                {
                    if (File.Exists(filepath))
                    {
                        loadExternalConfigurationsFile(filepath, this, _root, true);
                    }
                }
            }

            _taskWatcher = startWatcher(dir);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        internal async void CancelFileWatcher()
        {
            if (_taskWatcher == null) return;

            // cancel the task
            _tokenSource.Cancel();
            try
            {
                await _taskWatcher;
                _taskWatcher = null;
            }
            catch (OperationCanceledException e)
            {
                // handle the exception 
                this.logError(e, "cancel file watcher failed");
            }
        }

        private Task startWatcher(string dir)
        {
            _shouldTerminate.Reset();
            var cancellableTask = Task.Run(() =>
            {
                // fsw
                RunFileWatcher(dir);

                _shouldTerminate.Wait();
                if (_tokenSource.Token.IsCancellationRequested)
                {
                    // clean up before exiting
                    _tokenSource.Token.ThrowIfCancellationRequested();
                }

                watcher?.Dispose();
                return 0;
            }, _tokenSource.Token);
            return cancellableTask;
        }

        private static readonly ManualResetEventSlim _shouldTerminate = new ManualResetEventSlim(false);
        private static readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private static Task? _taskWatcher;
        private static FileSystemWatcher? watcher;


        // [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void RunFileWatcher(string dir)
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher
            {
                Path = dir,
                NotifyFilter = NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName,
                // Filter = "*.json|*.yml|*.yaml",
                Filters = {"*.json", "*.yml", "*.yaml",},
                // IncludeSubdirectories = false,
            };


            // Watch for changes in LastAccess and LastWrite times, and
            // the renaming of files or directories.

            // Only watch text files.

            // Add event handlers.
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            Console.WriteLine($"FileWatcher running at {dir}");

            // Wait for the user to quit the program.
            // Console.WriteLine("Press 'q' to quit the sample.");
            // while (Console.Read() != 'q') ;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            this.logDebug($"config file renamed: {e}");
            if (loadExternalConfigurationsFile(e.FullPath, this, _root, false))
            {
                this.logDebug($"config file loaded and merged: {e.FullPath}");
            }
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            this.logDebug($"config file deleted: {e}");
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            this.logDebug($"config file renamed: {e}");
            if (loadExternalConfigurationsFile(e.FullPath, this, _root, false))
            {
                this.logDebug($"config file loaded and merged: {e.FullPath}");
            }
        }

        #endregion


        private bool mergeMappingNode(JObject o, IEnumerable<string> keyParts, bool overwriteExists)
        {
            var keyParts1 = keyParts.ToArray();
            foreach (var (key, val) in o)
            {
                var parts = keyParts1.Append(key);

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (val is JObject jo)
                {
                    var ok = mergeMappingNode(jo, parts, overwriteExists);
                    return ok;
                }

                if (val is JArray ja)
                {
                    CmdrOptionStore.SetByKeys(parts, ja);
                    continue;
                }

                // if (val is JArray ja)
                // {
                //     if (val.HasValues)
                //     {
                //         object? a = null;
                //         switch (val.First.Type)
                //         {
                //             case JTokenType.Boolean:
                //                 a = val.Values<bool>();
                //                 break;
                //             case JTokenType.Date:
                //                 a = val.Values<DateTime>();
                //                 break;
                //             case JTokenType.TimeSpan:
                //                 a = val.Values<TimeSpan>();
                //                 break;
                //             case JTokenType.Float:
                //                 a = val.Values<double>();
                //                 break;
                //             case JTokenType.Integer:
                //                 a = ja.ToArray<long>();
                //                 break;
                //             case JTokenType.String:
                //                 a = ja.ToArray<string>();
                //                 break;
                //             case JTokenType.None:
                //             case JTokenType.Undefined:
                //             case JTokenType.Null:
                //                 break;
                //             default:
                //                 a = ja.ToArray<object>();
                //                 break;
                //         }
                //
                //         if (a != null)
                //             CmdrOptionStore.SetByKeys(parts, a);
                //     }
                //
                //     continue;
                // }

                var newVal = val.ToObject<object?>();
                CmdrOptionStore.SetByKeys(parts, newVal);
            }

            return false;
        }

        private bool mergeMappingNode(YamlMappingNode mapping, IEnumerable<string> keyParts, bool overwriteExists)
        {
            var keyParts1 = keyParts.ToArray();
            foreach (var (keyNode, val) in mapping.Children)
            {
                var key = keyNode as YamlScalarNode;
                if (key == null) continue;

                var parts = keyParts1.Append(key.Value);
                switch (val)
                {
                    case YamlMappingNode map:
                        if (AppVerboseMode) this.logDebug($" [YAML] -> {"  ".Repeat(keyParts1.Length)}{key.Value}:");
                        mergeMappingNode(map, parts, overwriteExists);
                        break;
                    case YamlScalarNode scalarNode:
                        if (AppVerboseMode) this.logDebug($" [YAML] -> {"  ".Repeat(keyParts1.Length)}{key.Value} = {scalarNode.Value}");

                        if (overwriteExists)
                            CmdrOptionStore.SetByKeys(parts, scalarNode.Value);
                        else
                        {
                            var enumerable = parts as string[] ?? parts.ToArray();
                            if (!CmdrOptionStore.HasKeys(enumerable))
                                CmdrOptionStore.SetByKeys(enumerable, scalarNode.Value);
                        }

                        break;
                }
            }

            return true;
        }


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void collectAndBuildXref(ICommand command)
        {
            walkFor(command, (owner, cmd, level) =>
            {
                var x = xrefs;
                if (!x.ContainsKey(owner))
                    x.TryAdd(owner, new Xref());
                if (!x.ContainsKey(cmd))
                    x.TryAdd(cmd, new Xref());

                if (cmd.Owner != owner) cmd.Owner = owner;
                if (string.IsNullOrWhiteSpace(cmd.Group))
                    cmd.Group = FirstUnsortedGroup;

                var xx = x[owner];
                xx.TryAddShort(this, cmd);
                xx.TryAddLong(this, cmd);
                xx.TryAddAliases(this, cmd);
                return true;
            }, (owner, flag, level) =>
            {
                var x = xrefs;
                if (!x.ContainsKey(owner))
                    x.TryAdd(owner, new Xref());

                if (flag.Owner != owner) flag.Owner = owner;
                if (string.IsNullOrWhiteSpace(flag.Group) && !string.IsNullOrWhiteSpace(flag.ToggleGroup))
                    flag.Group = flag.ToggleGroup;
                if (string.IsNullOrWhiteSpace(flag.Group))
                    flag.Group = FirstUnsortedGroup;

                var xx = x[owner];
                xx.TryAddShort(this, owner, flag);
                xx.TryAddLong(this, owner, flag);
                xx.TryAddAliases(this, owner, flag);

                // build into Store too:
                // bool exists = CmdrOptionStore.HasKeys(flag.ToKeys());
                var v = flag.getDefaultValue();

                #region loading values from env vars

                applyValueFromEnv(flag, ref v);
                // // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                // // if (v == null)
                // {
                //     if (flag.EnvVars.Length > 0)
                //     {
                //         foreach (var ek in flag.EnvVars)
                //         {
                //             var tv = Util.GetEnvValue<object>(ek);
                //             // ReSharper disable once InvertIf
                //             if (tv != default)
                //             {
                //                 // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                //                 if (v != null)
                //                     v = v is bool ? tv.ToBool() : Convert.ChangeType(tv, v.GetType());
                //                 else
                //                     v = tv;
                //
                //                 break;
                //             }
                //         }
                //     }
                //     else
                //     {
                //         var tv = Util.GetEnvValue<object>(Store.Instance.WrapKeys(flag.ToKeys()));
                //         if (tv != default)
                //         {
                //             // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                //             if (v != null)
                //                 v = v is bool ? tv.ToBool() : Convert.ChangeType(tv, v.GetType());
                //             else
                //                 v = tv;
                //         }
                //     }
                // }

                #endregion

                if (v != null)
                    CmdrOptionStore.SetByKeysInternal(flag.ToKeys(), v);

                return true; // return false to break the walkForFlags' loop.
            });
            this.logDebug("_xrefs was built.");
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private void applyValueFromEnv(IFlag flag, ref object? v)
        {
            if (flag.EnvVars.Length > 0)
            {
                foreach (var ek in flag.EnvVars)
                {
                    var tv = Util.GetEnvValue<object>(ek);
                    // ReSharper disable once InvertIf
                    if (tv != default)
                    {
                        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                        if (v != null)
                            v = v is bool ? tv.ToBool() : Convert.ChangeType(tv, v.GetType());
                        else
                            v = tv;

                        break;
                    }
                }
            }
            else
            {
                var tv = Util.GetEnvValue<object>(Store.Instance.WrapKeys(flag.ToKeys()));
                // ReSharper disable once InvertIf
                if (tv != default)
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (v != null)
                        v = v is bool ? tv.ToBool() : Convert.ChangeType(tv, v.GetType());
                    else
                        v = tv;
                }
            }
        }

        #endregion

        #endregion

        public Worker RegisterExternalConfigurationsLoader(params Action<IBaseWorker, IRootCommand>[] loaders)
        {
            _externalConfigurationsLoaders.AddRange(loaders);
            return this;
        }


        #region helpers for Walk()

        private bool walkFor(ICommand parent,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null,
            int level = 0)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var f in parent.Flags)
                if (flagsWatcher != null && flagsWatcher(parent, f, level) == false)
                    return false;

            if (parent.SubCommands == null) return true;

            foreach (var cmd in parent.SubCommands)
            {
                if (commandsWatcher != null && commandsWatcher.Invoke(parent, cmd, level) == false)
                    return false;
                if (walkFor(cmd, commandsWatcher, flagsWatcher, level + 1) == false)
                    return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        private bool walkForFlags(ICommand parent, Func<ICommand, IFlag, int, bool> watcher1, int level = 0)
        {
            if (parent.Flags.Any(f => watcher1 != null && watcher1(parent, f, level) == false)) return false;

            if (parent.SubCommands == null) return true;

            return parent.SubCommands.All(cmd => walkForFlags(cmd, watcher1, level + 1));
        }

        #endregion

        // #region helpers for Run() - match
        //
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // private int match(ICommand command, string[] args, int position, int level)
        // {
        //     log.Debug("  - match for command: {CommandTitle}", command.backtraceTitles);
        //
        //     var matchedPosition = -1;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     string fragment;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     int pos, len, size;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     int ate;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     object? value;
        //
        //     // ReSharper disable once ForCanBeConvertedToForeach
        //     for (var i = position; i < args.Length; i++)
        //     {
        //         bool ok;
        //         var arg = args[i];
        //         var isOpt = arg.StartsWith("-");
        //         var longOpt = arg.StartsWith("--");
        //
        //         log.Debug("    -> arg {Index}: {Argument}", i, arg);
        //         if (!isOpt)
        //         {
        //             #region matching command
        //
        //             if (command.SubCommands != null && command.SubCommands.Count > 0)
        //             {
        //                 var positionCopy = i + 1;
        //                 foreach (var cmd in command.SubCommands)
        //                 {
        //                     // ReSharper disable once RedundantArgumentDefaultValue
        //                     ok = cmd.Match(arg, false);
        //                     if (!ok) ok = cmd.Match(arg, true);
        //                     if (!ok) continue;
        //
        //                     log.Debug("    ++ command matched: {CommandTitle}", cmd.backtraceTitles);
        //
        //                     ParsedCommand = cmd;
        //                     ParsedFlag = null;
        //
        //                     positionCopy = i + 1;
        //                     if (cmd.SubCommands.Count > 0)
        //                     {
        //                         if (i == arg.Length - 1) throw new WantHelpScreenException();
        //
        //                         var pos1 = match(cmd, args, i + 1, level + 1);
        //                         if (pos1 < 0) matchedPosition = positionCopy;
        //                         positionCopy = pos1;
        //                     }
        //
        //                     // onCommandMatched(args, i + 1, arg, cmd);
        //
        //                     if (matchedPosition < 0 || positionCopy > 0)
        //                         matchedPosition = positionCopy;
        //                     command = cmd;
        //                     break;
        //                 }
        //
        //                 if (matchedPosition < 0)
        //                 {
        //                     log.Debug("level {Level} (cmd can't matched): returning {Position}", level, -position - 1);
        //                     onCommandCannotMatched(args, i, arg, command);
        //                     return -position - 1;
        //                 }
        //
        //                 if (positionCopy < 0 && matchedPosition > 0)
        //                     return positionCopy;
        //             }
        //             else
        //             {
        //                 log.Debug("level {Level} (no sub-cmds): returning {Position}", level, matchedPosition);
        //                 onCommandCannotMatched(args, i, arg, command);
        //                 return matchedPosition;
        //             }
        //
        //             #endregion
        //
        //             continue;
        //         }
        //
        //         // matching for flags of 'command'
        //
        //         var ccc = command;
        //         fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
        //         pos = 0;
        //         len = 1;
        //         size = fragment.Length;
        //         ate = 0;
        //
        //         forEachFragmentParts:
        //         var part = fragment.Substring(pos, len);
        //
        //         log.Debug("    - try finding flags for ccc: {CommandTitle}", ccc.backtraceTitles);
        //         
        //         backtraceAllParentFlags:
        //         ok = false;
        //         foreach (var flg in ccc.Flags)
        //         {
        //             ok = flg.Match(ref part, fragment, pos, longOpt);
        //             if (!ok) continue;
        //
        //             // a flag matched ok, try extracting its value from commandline arguments
        //             (ate, value) = tryExtractingValue(flg, args, i, part, pos);
        //
        //             log.Debug("    ++ flag matched: {SW:l}{flgLong:l} {value}",
        //                 Util.SwitchChar(longOpt), flg.Long, value);
        //
        //             len = part.Length;
        //             ParsedFlag = flg;
        //             onFlagMatched(args, i + 1, part, longOpt, flg);
        //             matchedPosition = i + 1;
        //             break;
        //         }
        //
        //         // ReSharper disable once InvertIf
        //         if (!ok)
        //         {
        //             if (ccc.Owner != null && ccc.Owner != ccc)
        //             {
        //                 ccc = ccc.Owner;
        //                 log.Debug("    - try finding flags for its(ccc) parent: {CommandTitle}", ccc.backtraceTitles);
        //                 goto backtraceAllParentFlags;
        //             }
        //
        //             log.Debug("can't match a flag: {Argument}/part={Part}/fragment={Fragment}.", arg, part, fragment);
        //             onFlagCannotMatched(args, i, part, longOpt, command);
        //         }
        //
        //         if (pos + len < size)
        //         {
        //             pos += len;
        //             len = 1;
        //             log.Debug("    - for next part: {Part}", fragment.Substring(pos, len));
        //             ccc = command;
        //             goto forEachFragmentParts;
        //         }
        //
        //         if (ate > 0)
        //         {
        //             i += ate;
        //         }
        //
        //         // 
        //     }
        //
        //     // ReSharper disable once InvertIf
        //     if (matchedPosition < 0)
        //     {
        //         log.Debug("level {Level}: returning {Position}", level, -position - 1);
        //         return -position - 1;
        //     }
        //
        //     return matchedPosition;
        // }

        // // ReSharper disable once SuggestBaseTypeForParameter
        // private (int ate, object? value) tryExtractingValue(IFlag flg, string[] args, int i, string part, int pos)
        // {
        //     var ate = 0;
        //     object? val = null;
        //     
        //     var remains = args[i].Substring(pos + part.Length);
        //     bool? flipChar = null;
        //     if (remains.Length > 0)
        //     {
        //         flipChar = remains[0] switch
        //         {
        //             '-' => false,
        //             '+' => true,
        //             _ => null
        //         };
        //     }
        //
        //     var dv = flg.getDefaultValue();
        //     switch (dv)
        //     {
        //         case bool _:
        //             val = flipChar ?? true;
        //             break;
        //         
        //         case string _:
        //             ate = 1;
        //             val = args[i + ate];
        //             break;
        //         
        //         case string[] _:
        //             val = true;
        //             break;
        //     }
        //
        //     return (ate, val);
        // }
        //
        // #endregion

        // #region helpers for Run() - match(ed)
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="args"></param>
        // /// <param name="position"></param>
        // /// <param name="arg"></param>
        // /// <param name="cmd"></param>
        // /// <returns>false means no action triggered.</returns>
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // // ReSharper disable once UnusedParameter.Local
        // private bool onCommandMatched(IEnumerable<string> args, int position, string arg, ICommand cmd)
        // {
        //     var remainArgs = args.Where((it, idx) => idx >= position).ToArray();
        //
        //     var root = cmd.FindRoot();
        //     if (root?.PreAction != null && !root.PreAction.Invoke(this, cmd, remainArgs))
        //         throw new ShouldBeStopException();
        //     if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(this, cmd, remainArgs))
        //         throw new ShouldBeStopException();
        //
        //     try
        //     {
        //         log.Debug("---> matched command: {cmd}, remains: {Args}", cmd, string.Join(",", remainArgs));
        //
        //         if (cmd is IAction action)
        //             action.Invoke(this, remainArgs);
        //         else if (cmd.Action != null)
        //             cmd.Action.Invoke(this, cmd, remainArgs);
        //         else
        //             return false;
        //         return true;
        //     }
        //     finally
        //     {
        //         if (root != cmd) cmd.PostAction?.Invoke(this, cmd, remainArgs);
        //         root?.PostAction?.Invoke(this, cmd, remainArgs);
        //     }
        //
        //     // throw new NotImplementedException();
        // }
        //
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // private void onFlagMatched(IEnumerable<string> args, int position, string fragment, in bool longOpt,
        //     IFlag flag)
        // {
        //     var remainArgs = args.Where((it, idx) => idx >= position).ToArray();
        //
        //     if (flag.PreAction != null && !flag.PreAction.Invoke(this, flag, remainArgs))
        //         throw new ShouldBeStopException();
        //
        //     try
        //     {
        //         // ReSharper disable once UnusedVariable
        //         var sw = Util.SwitchChar(longOpt);
        //         log.Debug("  ---> flag matched: {SW:l}{Fragment:l}", sw, fragment);
        //         if (flag.OnSet != null)
        //             flag.OnSet?.Invoke(this, flag, flag.getDefaultValue(), flag.getDefaultValue());
        //         else
        //             defaultOnSet?.Invoke(this, flag, flag.getDefaultValue(), flag.getDefaultValue());
        //
        //         // ReSharper disable once SuspiciousTypeConversion.Global
        //         // ReSharper disable once UseNegatedPatternMatching
        //         var action = flag as IAction;
        //         if (action == null)
        //             flag.Action?.Invoke(this, flag, remainArgs);
        //         else
        //             action.Invoke(this, remainArgs);
        //     }
        //     finally
        //     {
        //         flag.PostAction?.Invoke(this, flag, remainArgs);
        //     }
        // }
        //
        // // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // private Action<Worker, IBaseOpt, object?, object?>? defaultOnSet = (w, flg, oldVal, newVal) =>
        // {
        //     w.log.Debug("");
        // };
        //
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once UnusedParameter.Local
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once SuggestBaseTypeForParameter
        // private void onCommandCannotMatched(string[] args, in int position, string arg, ICommand cmd)
        // {
        //     // throw new NotImplementedException();
        //     errPrint($"- Unknown command(arg): '{args[position]}'. context: '{cmd.backtraceTitles}'.");
        //     suggestCommands(args, position, arg, cmd);
        //     if (EnableUnknownCommandThrows)
        //         throw new UnknownCommandException(false, arg, cmd);
        // }
        //
        // [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // // ReSharper disable once UnusedParameter.Local
        // private void onFlagCannotMatched(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        // {
        //     var sw = Util.SwitchChar(longOpt);
        //     errPrint($"- Unknown flag({sw}{fragment}): '{args[position]}'. context: '{cmd.backtraceTitles}'");
        //     suggestFlags(args, position, fragment, longOpt, cmd);
        //     if (EnableUnknownFlagThrows)
        //         throw new UnknownFlagException(!longOpt, fragment, cmd);
        // }
        //
        // #endregion
        //
        // #region suggestions
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestCommands(string[] args, in int position, string tag, ICommand cmd)
        // {
        //     var xref = _xrefs[cmd];
        //     suggestFor(tag, xref.SubCommandsLongNames);
        //     suggestFor(tag, xref.SubCommandsAliasNames);
        //     suggestFor(tag, xref.SubCommandsShortNames);
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFlags(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        // {
        //     var xref = _xrefs[cmd];
        //     if (longOpt)
        //     {
        //         suggestFor(fragment, xref.FlagsLongNames);
        //         suggestFor(fragment, xref.FlagsAliasNames);
        //     }
        //     else
        //     {
        //         suggestFor(fragment, xref.FlagsShortNames);
        //     }
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFor(string tag, Dictionary<string, ICommand> dataset)
        // {
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFor(string tag, Dictionary<string, IFlag> dataset)
        // {
        // }
        //
        // #endregion
    }
}