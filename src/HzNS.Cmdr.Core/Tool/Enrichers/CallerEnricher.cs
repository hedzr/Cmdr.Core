using System.Diagnostics;
using System.Linq;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr.Tool.Enrichers
{
    public static class LoggerCallerEnrichmentConfiguration
    {
        public static Serilog.LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }

    public class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;
            while (true)
            {
                var stack = new StackFrame(skip, true);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method != null && method.DeclaringType != null
                                   && method.DeclaringType.Assembly != typeof(Serilog.Log).Assembly)
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
    }
}