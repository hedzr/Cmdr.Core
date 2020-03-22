using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;

namespace HzNS.Cmdr.Tool.ExLog
{
    /// <summary>
    /// not work
    /// </summary>
    [Deprecated("all", "not completed")]
    internal static class Ext
    {
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

    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LogWrapper
    {
        private readonly ILogger _log;

        public LogWrapper(ILogger logger)
        {
            _log = logger;
        }

        public void Verbose(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Verbose(
                message
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber)
            );
        }

        public void Verbose(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Verbose(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber)
            );
        }

        public void Debug(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Debug(message
                .FormatForContext(memberName, sourceFilePath, sourceLineNumber)
            );
        }

        public void Debug(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Debug(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber)
            );
        }

        public void Info(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Information(
                message
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber)
            );
        }

        public void Info(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Information(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Warn(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Warning(
                message
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Warn(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Warning(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Error(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Error(
                message
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Error(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Error(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Test([CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var fr = new StackFrame(1, true);
            var mb = fr.GetMethod();
            if (mb != null)
            {
                Console.WriteLine("MethodName: {0}", mb.Name);
                if (mb.DeclaringType != null)
                    Console.WriteLine(mb.DeclaringType.Assembly.GetName());
            }
        }

        public void Error(System.Exception ex,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            _log.Error(
                (ex != null ? ex.ToString() : "")
                .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Fatal(string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            FatalAction();

            _log.Error(message.FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Fatal(string message, System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            FatalAction();

            _log.Error(
                message
                    .FormatForException(ex)
                    .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        public void Fatal(System.Exception ex, [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            FatalAction();

            _log.Error(
                (ex != null ? ex.ToString() : "")
                .FormatForContext(memberName, sourceFilePath, sourceLineNumber));
        }

        private static void FatalAction()
        {
            Environment.ExitCode = -1;
        }
    }
}