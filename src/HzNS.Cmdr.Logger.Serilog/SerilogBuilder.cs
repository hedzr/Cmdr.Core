#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HzNS.Cmdr.Logger.Serilog.Enrichers;
using Serilog;
using Serilog.Events;

namespace HzNS.Cmdr.Logger.Serilog
{
    public abstract class SerilogBuilder
    {
        public static ILogger Build(params System.Action<ILogger>[] opts)
        {
            return new SerilogWrapper().Init(opts);
        }
    }


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ISerilogLogger
    {
        global::Serilog.ILogger log { get; }
        bool EnableCmdrLogInfo { get; }
        bool EnableCmdrLogTrace { get; }
    }


    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
    public class SerilogWrapper : ILogger, ISerilogLogger
    {
        internal SerilogWrapper()
        {
            log = Log.Logger;
        }

        public ILogger Init(params System.Action<ILogger>[] opts)
        {
            UseSerilog();
            foreach (var opt in opts)
            {
                opt(this);
            }

            return this;
        }


        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public global::Serilog.ILogger log { get; private set; }

        public bool EnableCmdrLogInfo { get; set; }
        public bool EnableCmdrLogTrace { get; set; }

        // public ILogger log;
        // public LogWrapper log;

        protected virtual ILogger UseSerilog(System.Func<LoggerConfiguration, global::Serilog.Core.Logger>? func = null)
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

            // var builder = new ContainerBuilder();
            // builder.RegisterLogger(autowireProperties: true);

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

        #region publics

        public void logInfo(string messageTemplate) => this.slogInfo(messageTemplate);
        
        public void logInfo<T0>(string messageTemplate, T0 p0) => this.slogInfo(messageTemplate, p0);

        public void logInfo<T0, T1>(string messageTemplate, T0 p0, T1 p1) =>
            this.slogInfo(messageTemplate, p0, p0, p1);

        public void logInfo<T0, T1, T2>(string messageTemplate, T0 p0, T1 p1, T2 p2) =>
            this.slogInfo(messageTemplate, p0, p0, p1, p2);

        public void logInfo<T0, T1, T2, T3>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3) =>
            this.slogInfo(messageTemplate, p0, p1, p2, p3);

        public void logInfo<T0, T1, T2, T3, T4>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4) =>
            this.slogInfo(messageTemplate, p0, p1, p2, p3, p4);

        public void logInfo<T0, T1, T2, T3, T4, T5>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5) => this.slogInfo(messageTemplate, p0, p1, p2, p3, p4, p5);

        public void logInfo<T0, T1, T2, T3, T4, T5, T6>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6) => this.slogInfo(messageTemplate, p0, p1, p2, p3, p4, p5, p6);

        public void logInfo<T0, T1, T2, T3, T4, T5, T6, T7>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7) => this.slogInfo(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7);

        public void logInfo<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => this.slogInfo(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8);

        public void logInfo<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
            this.slogInfo(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);

        
        
        public void logDebug(string messageTemplate) => this.slogDebug(messageTemplate);
        
        public void logDebug<T0>(string messageTemplate, T0 p0) => this.slogDebug(messageTemplate, p0);

        public void logDebug<T0, T1>(string messageTemplate, T0 p0, T1 p1) =>
            this.slogDebug(messageTemplate, p0, p0, p1);

        public void logDebug<T0, T1, T2>(string messageTemplate, T0 p0, T1 p1, T2 p2) =>
            this.slogDebug(messageTemplate, p0, p0, p1, p2);

        public void logDebug<T0, T1, T2, T3>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3) =>
            this.slogDebug(messageTemplate, p0, p1, p2, p3);

        public void logDebug<T0, T1, T2, T3, T4>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4) =>
            this.slogDebug(messageTemplate, p0, p1, p2, p3, p4);

        public void logDebug<T0, T1, T2, T3, T4, T5>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5) => this.slogDebug(messageTemplate, p0, p1, p2, p3, p4, p5);

        public void logDebug<T0, T1, T2, T3, T4, T5, T6>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6) => this.slogDebug(messageTemplate, p0, p1, p2, p3, p4, p5, p6);

        public void logDebug<T0, T1, T2, T3, T4, T5, T6, T7>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7) => this.slogDebug(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7);

        public void logDebug<T0, T1, T2, T3, T4, T5, T6, T7, T8>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => this.slogDebug(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8);

        public void logDebug<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
            this.slogDebug(messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);

        
        
        public void logWarning(System.Exception exception, string messageTemplate) => this.slogWarning(exception, messageTemplate);
        
        public void logWarning<T0>(System.Exception exception, string messageTemplate, T0 p0) => this.slogWarning(exception, messageTemplate, p0);

        public void logWarning<T0, T1>(System.Exception exception, string messageTemplate, T0 p0, T1 p1) =>
            this.slogWarning(exception, messageTemplate, p0, p0, p1);

        public void logWarning<T0, T1, T2>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2) =>
            this.slogWarning(exception, messageTemplate, p0, p0, p1, p2);

        public void logWarning<T0, T1, T2, T3>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3) =>
            this.slogWarning(exception, messageTemplate, p0, p1, p2, p3);

        public void logWarning<T0, T1, T2, T3, T4>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4) =>
            this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4);

        public void logWarning<T0, T1, T2, T3, T4, T5>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5) => this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4, p5);

        public void logWarning<T0, T1, T2, T3, T4, T5, T6>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6) => this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6);

        public void logWarning<T0, T1, T2, T3, T4, T5, T6, T7>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7) => this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7);

        public void logWarning<T0, T1, T2, T3, T4, T5, T6, T7, T8>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8);

        public void logWarning<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
            this.slogWarning(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);

        
        
        public void logError(System.Exception exception, string messageTemplate) => this.slogError(exception, messageTemplate);
        
        public void logError<T0>(System.Exception exception, string messageTemplate, T0 p0) => this.slogError(exception, messageTemplate, p0);

        public void logError<T0, T1>(System.Exception exception, string messageTemplate, T0 p0, T1 p1) =>
            this.slogError(exception, messageTemplate, p0, p0, p1);

        public void logError<T0, T1, T2>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2) =>
            this.slogError(exception, messageTemplate, p0, p0, p1, p2);

        public void logError<T0, T1, T2, T3>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3) =>
            this.slogError(exception, messageTemplate, p0, p1, p2, p3);

        public void logError<T0, T1, T2, T3, T4>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4) =>
            this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4);

        public void logError<T0, T1, T2, T3, T4, T5>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5) => this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4, p5);

        public void logError<T0, T1, T2, T3, T4, T5, T6>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6) => this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6);

        public void logError<T0, T1, T2, T3, T4, T5, T6, T7>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3, T4 p4,
            T5 p5, T6 p6, T7 p7) => this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7);

        public void logError<T0, T1, T2, T3, T4, T5, T6, T7, T8>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) => this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8);

        public void logError<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(System.Exception exception, string messageTemplate, T0 p0, T1 p1, T2 p2, T3 p3,
            T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
            this.slogError(exception, messageTemplate, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9);


        #endregion
    }
}