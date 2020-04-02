using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace HzNS.Cmdr.Logger.Serilog
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    internal static class LogExtensions
    {
        #region debug helpers: logInfo

        public static void slogInfo<T>(this T @this, string messageTemplate)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    // logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileName",
                    // new ScalarValue(stack.GetFileName())));
                    .Information(messageTemplate);
            }
        }

        public static void slogInfo<T, T0>(this T @this, string messageTemplate,
            T0 property0)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0);
            }
        }

        public static void slogInfo<T, T0, T1>(this T @this, string messageTemplate,
            T0 property0, T1 property1)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0, property1);
            }
        }

        public static void slogInfo<T, T0, T1, T2>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0, property1, property2);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4, T5>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4, property5);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4, T5, T6>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5,
            T6 property6)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4, property5, property6);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4, T5, T6, T7>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5,
            T6 property6, T7 property7)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4, property5, property6, property7);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4, T5, T6, T7, T8>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5,
            T6 property6, T7 property7, T8 property8)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4, property5, property6, property7, property8);
            }
        }

        public static void slogInfo<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5,
            T6 property6, T7 property7, T8 property8, T9 property9)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3,
                        property4, property5, property6, property7, property8, property9);
            }
        }

        #endregion


        private const int skipLevels = 2;
            
        #region debug helpers: logDebug

        public static void slogDebug<T>(this T @this, string messageTemplate)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    // logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileName",
                    // new ScalarValue(stack.GetFileName())));
                    .Debug(messageTemplate);
            }
        }

        public static void slogDebug<T, T0>(this T @this, string messageTemplate,
            T0 property0)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0);
            }
        }

        public static void slogDebug<T, T0, T1>(this T @this, string messageTemplate,
            T0 property0, T1 property1)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1);
            }
        }

        public static void slogDebug<T, T0, T1, T2>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4, T5>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4, property5);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4, T5, T6>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4, property5,
                        property6);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4, T5, T6, T7>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4, property5, property6,
                        property7);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4, T5, T6, T7, T8>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4, property5, property6,
                        property7, property8);
            }
        }

        public static void slogDebug<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8, T9 property9)
            where T : ISerilogLogger
        {
            if (@this.EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", skipLevels)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4, property5, property6,
                        property7, property8, property9);
            }
        }

        #endregion


        #region debug helpers: logWarning

        public static void slogWarning<T>(this T @this, System.Exception exception, string messageTemplate)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate);
        }

        public static void slogWarning<T, T0>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0);
        }

        public static void slogWarning<T, T0, T1>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1);
        }

        public static void slogWarning<T, T0, T1, T2>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2);
        }

        public static void slogWarning<T, T0, T1, T2, T3>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4, T5>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4, property5);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4, T5, T6>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4, T5, T6, T7>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4, T5, T6, T7, T8>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7, property8);
        }

        public static void slogWarning<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this T @this,
            System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8, T9 property9)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Warning(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7, property8, property9);
        }

        #endregion


        #region debug helpers: logError

        public static void slogError<T>(this T @this, System.Exception exception, string messageTemplate)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate);
        }

        public static void slogError<T, T0>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0);
        }

        public static void slogError<T, T0, T1>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1);
        }

        public static void slogError<T, T0, T1, T2>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2);
        }

        public static void slogError<T, T0, T1, T2, T3>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3);
        }

        public static void slogError<T, T0, T1, T2, T3, T4>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4);
        }

        public static void slogError<T, T0, T1, T2, T3, T4, T5>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4, property5);
        }

        public static void slogError<T, T0, T1, T2, T3, T4, T5, T6>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6);
        }

        public static void slogError<T, T0, T1, T2, T3, T4, T5, T6, T7>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7);
        }

        public static void slogError<T, T0, T1, T2, T3, T4, T5, T6, T7, T8>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7, property8);
        }

        public static void slogError<T, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this T @this,
            System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4, T5 property5, T6 property6,
            T7 property7, T8 property8, T9 property9)
            where T : ISerilogLogger
        {
            @this.log.ForContext("SKIP_", 1)
                .Error(exception, messageTemplate, property0, property1, property2, property3, property4, property5,
                    property6, property7, property8, property9);
        }

        #endregion
        
        
        internal static string FormatForException(this string message, System.Exception ex)
        {
            return $"{message}: {(ex != null ? ex.ToString() : "")}";
        }
        
        internal static string FormatForContext(this string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var methodName = memberName;
        
            return $"{fileName}:{sourceLineNumber} [{methodName}] {message}";
        }

    }
}