#nullable enable
using System.Diagnostics;
using System.Linq;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr.Logger.Serilog.Enrichers
{
    public static class LoggerCallerEnrichmentConfiguration
    {
        public static global::Serilog.LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }

    public class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;

            // var postSkip = 0;
            // if (logEvent.Properties.ContainsKey("SKIP_"))
            // postSkip = int.Parse(logEvent.Properties["SKIP_"].ToString());

            while (true)
            {
                var stack = new StackFrame(skip, true);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method != null
                    && method.DeclaringType != null
                    && method.DeclaringType.Assembly != typeof(global::Serilog.Log).Assembly
                    && IsValid(method.Name))
                {
                    var caller =
                        $"{method.DeclaringType.FullName}.{method.Name}({string.Join(", ", method.GetParameters().Select(pi => pi.ParameterType.FullName))})";
                    // var caller = method.ToString();
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    if (stack.HasSource())
                    {
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileName",
                            new ScalarValue(stack.GetFileName())));
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileLineNumber",
                            new ScalarValue(stack.GetFileLineNumber())));
                        logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileColumnNumber",
                            new ScalarValue(stack.GetFileColumnNumber())));
                    }
                }

                skip++;
            }
        }

        private static bool IsValid(string mn)
        {
            return mn switch
            {
                "logDebug" => false,
                "logWarning" => false,
                "logError" => false,
                _ => true
            };
        }
    }
}