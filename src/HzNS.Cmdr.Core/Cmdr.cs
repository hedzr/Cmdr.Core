#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.CmdrAttrs;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Logger;

namespace HzNS.Cmdr
{
    public class Cmdr
    {
        public static Worker NewWorker(IRootCommand root, params Action<Worker>[] opts)
        {
            var worker = CreateDefaultWorker(root, opts);
            return worker;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Worker CreateDefaultWorker(IRootCommand root,
            // Func<LoggerConfiguration, Logger>? createLoggerFunc = null,
            params Action<Worker>[] opts)
        {
            var worker = new Worker(root);

            // if (createLoggerFunc != null)
            //     worker.UseSerilog(createLoggerFunc);
            // else
            //     worker.UseSerilog(configuration => configuration
            //         .MinimumLevel.Debug()
            //         // .MinimumLevel.Information()
            //         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //         .Enrich.FromLogContext()
            //         .Enrich.WithCaller()
            //         .WriteTo.Console(
            //             outputTemplate:
            //             "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (at {Caller} in {SourceFileName}:line {SourceFileLineNumber}){NewLine}{Exception}")
            //         // .WriteTo.Console()
            //         .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
            //         .CreateLogger());

            // Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            // Serilog.Debugging.SelfLog.Enable(Console.Error);

            worker.runOnce();

            foreach (var opt in opts) opt(worker);

            try
            {
                Instance.Worker = worker;
                // Instance.Logger = worker;
                // worker.logDebug("EnableDuplicatedCharThrows: {EnableDuplicatedCharThrows}",
                //     worker.EnableDuplicatedCharThrows);
                if (root != null)
                    worker.With(root);
            }
            // catch (DuplicationCommandCharException)
            // {
            // }
            // catch (DuplicationFlagCharException)
            // {
            // }
            catch (CmdrException ex)
            {
                worker.log?.logError(ex, "Cmdr Error occurs");
                throw; // don't ignore it
            }

            return worker;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IBaseWorker? Worker { get; internal set; }
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // public IDefaultMatchers? Logger { get; internal set; }

        public Store Store { get; } = Store.Instance;


        /// <summary>
        /// In default, Store.GetAs&lt;T&gt;(key, defaultValue) will extract the entry value as T.
        /// If the entry value has a different data type, Convert.ChangeType will be applied.
        /// But you can disable this act by set EnableAutoBoxingWhenExtracting to false.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public bool EnableAutoBoxingWhenExtracting { get; set; } = true;


        public static int Compile<T>(string[] args, ILogger? log = null, params Action<Worker>[] opts)
        {
            if (log != null)
            {
                Instance.Worker = new Worker(RootCommand.New(new AppInfo()));
                if (Instance.Worker is Worker w)
                {
                    w.EnableCmdrLogInfo = true;
                    w.EnableCmdrLogDebug = true;
                    w.EnableCmdrLogTrace = true;
                    w.SetLogger(log);
                }
            }

            return compile(typeof(T), args, opts);
        }

        private static int compile(Type t, string[] args, params Action<Worker>[] opts)
        {
            RootCommand rootCmd;

            // find app info
            var appInfoAttr = Attribute.GetCustomAttribute(t, typeof(CmdrAppInfo));

            if (appInfoAttr is CmdrAppInfo a)
            {
                rootCmd = RootCommand.New(new AppInfo
                {
                    AppName = a.AppName,
                    AppVersionInt = 0,
                    Author = a.Author,
                    Copyright = a.Copyright,
                    BuildTags = a.Tags,
                });
            }
            else
            {
                rootCmd = RootCommand.New(new AppInfo());
            }

            // loop for commands,

            compileCommands(rootCmd, t, args);

            // loop for flags/options,

            // build, and run!

            NewWorker(rootCmd, opts).Run(args);

            return 0;
        }

        // ReSharper disable once InconsistentNaming
        private static int compileCommands(BaseCommand cmd, Type t, string[] args)
        {
            foreach (var nt in t.GetNestedTypes())
            {
                var attr = (CmdrAttrs.CmdrCommand?) Attribute.GetCustomAttribute(nt,
                    typeof(CmdrAttrs.CmdrCommand));
                if (attr == null) continue;

                var desc = (CmdrAttrs.CmdrDescriptions?) Attribute.GetCustomAttribute(nt,
                    typeof(CmdrAttrs.CmdrDescriptions));
                var group = (CmdrAttrs.CmdrGroup?) Attribute.GetCustomAttribute(nt,
                    typeof(CmdrAttrs.CmdrGroup));
                var hidden = (CmdrAttrs.CmdrHidden?) Attribute.GetCustomAttribute(nt,
                    typeof(CmdrAttrs.CmdrHidden));
                var vars = (CmdrAttrs.CmdrEnvVars?) Attribute.GetCustomAttribute(nt,
                    typeof(CmdrAttrs.CmdrEnvVars));

                var obj = Activator.CreateInstance(nt);
                var action = findForMethods<CmdrAttrs.CmdrAction>(nt);
                var actionPre = findForMethods<CmdrAttrs.CmdrPreAction>(nt);
                var actionPost = findForMethods<CmdrAttrs.CmdrPostAction>(nt);
                var actionOnSet = findForMethods<CmdrAttrs.CmdrOnSetAction>(nt);

                var subCommand = new Command
                {
                    Long = attr.Long,
                    Short = attr.Short,
                    Aliases = attr.Aliases,
                    Description = desc?.Description ?? string.Empty,
                    DescriptionLong = desc?.DescriptionLong ?? string.Empty,
                    Examples = desc?.Examples ?? string.Empty,
                    Group = group?.GroupName ?? string.Empty,
                    Hidden = hidden?.HiddenFlag ?? false,
                    EnvVars = vars?.VariableNames ?? new string[] { },
                    TailArgs = desc?.TailArgs ?? string.Empty
                };
                if (action != null)
                    subCommand.Action = (worker, opt, remainArgs) =>
                        action.Invoke(obj, new object?[] {worker, opt, remainArgs});
#pragma warning disable CS8603,CS8605
                if (actionPre != null)
                    subCommand.PreAction = (worker, opt, remainArgs) =>
                        (bool) actionPre.Invoke(obj, new object?[] {worker, opt, remainArgs});
#pragma warning restore CS8603,CS8605
                if (actionPost != null)
                    subCommand.PostAction = (worker, opt, remainArgs) =>
                        actionPost.Invoke(obj, new object?[] {worker, opt, remainArgs});
                if (actionOnSet != null)
                    subCommand.OnSet = (worker, opt, ov, nv) =>
                        actionOnSet?.Invoke(obj, new object?[] {worker, opt, ov, nv});
                cmd.AddCommand(subCommand);

                Cmdr.Instance.Worker?.log?.logInfo("  - Add Command to {cmd}: {sub}.", cmd, subCommand);
                if (action != null || actionPre != null || actionPost != null || actionOnSet != null)
                {
                    Cmdr.Instance.Worker?.log?.logInfo(
                        "    -> link to {obj}[{objType}].Action/Pre/Post/OnSet ({a},{pre},{post},{onset})",
                        obj, nt, action, actionPre, actionPost, actionOnSet);
                }

                compileCommands(subCommand, nt, args);
            }

            return 0;
        }

        // // ReSharper disable once InconsistentNaming
        // private static T findForMethodsClz<T>(Type nt) where T : class
        // {
        //     return null;
        // }

        // ReSharper disable once InconsistentNaming
        private static MethodInfo? findForMethods<T>(Type nt) where T : System.Attribute
        {
            foreach (var mtd in nt.GetMethods())
            {
#pragma warning disable CS8600
                var attr = (T) Attribute.GetCustomAttribute(mtd, typeof(T));
#pragma warning restore CS8600
                if (attr != null)
                {
                    return mtd;
                }
            }

            return null;
        }

        #region Singleton Pattern

        private Cmdr()
        {
        }

        // ReSharper disable once RedundantDefaultMemberInitializer
        private static Cmdr _instance = null!;

        // ReSharper disable once InconsistentNaming
        private static readonly object _lock = new object();

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        [SuppressMessage("ReSharper", "InvertIf")]
        // ReSharper disable once UnusedMember.Global
        public static Cmdr Instance
        {
            get
            {
                // Re|Sharper disable InvertIf
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new Cmdr();
                    }
                // Re|Sharper restore InvertIf

                return _instance;
            }
        }

        #endregion

        #region unused codes

        // // ReSharper disable once MemberCanBeMadeStatic.Global
        // public Worker Try(string[] args)
        // {
        //     var worker = new Worker();
        //     worker.RunOnce();
        //     return worker.Run(args);
        // }

        // // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // // ReSharper disable once MemberCanBePrivate.Global
        // public static ILogger Log { get; set; }
        //
        // public Entry AddLogger(ILogger log)
        // {
        //     Log = log;
        //     return this;
        // }

        #endregion
    }
}