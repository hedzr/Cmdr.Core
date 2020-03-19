using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog;
using Serilog.Events;

namespace HzNS.Cmdr
{
    public class Entry
    {
        #region Singleton Pattern

        private Entry()
        {
        }

        private static Entry _instance;

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
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Entry();
                        }
                    }
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

        public static Worker NewCmdrWorker(string[] args = null)
        {
            return CreateDefaultWorker(args)
                .UseSerilog((configuration) => configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    .CreateLogger());
        }

        // ReSharper disable once UnusedParameter.Local
        private static Worker CreateDefaultWorker(string[] args)
        {
            var worker = new Worker();
            worker.runOnce();
            return worker;
        }
    }
}