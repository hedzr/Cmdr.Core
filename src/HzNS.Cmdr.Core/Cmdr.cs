using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Tool.Enrichers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr
{
    public class Cmdr
    {
        public static Worker NewWorker(IRootCommand root, params Action<Worker>[] opts)
        {
            var worker = CreateDefaultWorker(root, null, opts);
            return worker;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Worker CreateDefaultWorker(IRootCommand root,
            Func<LoggerConfiguration, Logger>? createLoggerFunc = null,
            params Action<Worker>[] opts)
        {
            var worker = new Worker(root);

            if (createLoggerFunc != null)
                worker.UseSerilog(createLoggerFunc);
            else
                worker.UseSerilog(configuration => configuration
                    .MinimumLevel.Debug()
                    // .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.WithCaller()
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (at {Caller} in {SourceFileName}:line {SourceFileLineNumber}){NewLine}{Exception}")
                    // .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    .CreateLogger());

            // Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            // Serilog.Debugging.SelfLog.Enable(Console.Error);

            worker.runOnce();

            foreach (var opt in opts) opt(worker);

            try
            {
                worker.log.Debug($"EnableDuplicatedCharThrows: {worker.EnableDuplicatedCharThrows}");
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
                worker.log.Error(ex, "Error occurs");
            }

            return worker;
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