using System.IO;
using HzNS.Cmdr;
using HzNS.Cmdr.Logger.Serilog.Enrichers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace SampleAttrs
{
    class Program
    {
        public static Logger log;

        // static void Main(string[] args)
        // {
        //     Console.WriteLine("Hello World!");
        // }
        // ReSharper disable once ArrangeTypeMemberModifiers
        static int Main(string[] args)
        {
            log = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithCaller()
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message} (at {Caller} in {SourceFileName}:line {SourceFileLineNumber}){NewLine}{Exception}")
                // .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                // .WriteTo.Console()
                .CreateLogger();

            return Cmdr.Compile<SampleAttrApp>(args,
                HzNS.Cmdr.Logger.Serilog.SerilogBuilder.Build((logger) =>
                {
                    // logger.EnableCmdrLogInfo = true;
                    // logger.EnableCmdrLogTrace = true;
                }),
                (w) =>
                {
                    // w.SetLogger();
                });
        }
    }
}