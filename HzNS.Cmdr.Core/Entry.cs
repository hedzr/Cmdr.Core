using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HzNS.Cmdr.Builder;
using HzNS.Cmdr.Exception;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr
{
    public class Entry
    {
        public static Worker NewCmdrWorker(IRootCommand root, params Action<Worker>[] opts)
        {
            var worker = CreateDefaultWorker(root, null, opts);
            return worker;
        }

        // ReSharper disable once UnusedParameter.Local
        private static Worker CreateDefaultWorker(IRootCommand root,
            Func<LoggerConfiguration, Logger>? createLoggerFunc = null, params Action<Worker>[] opts)
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
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    .CreateLogger());

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

        private Entry()
        {
        }

        private static Entry _instance = null!;

        // ReSharper disable once InconsistentNaming
        private static readonly object _lock = new object();

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        [SuppressMessage("ReSharper", "InvertIf")]
        // ReSharper disable once UnusedMember.Global
        public static Entry Instance
        {
            get
            {
                // Re|Sharper disable InvertIf
                if (_instance == null)
                    lock (_lock)
                    {
                        if (_instance == null) _instance = new Entry();
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