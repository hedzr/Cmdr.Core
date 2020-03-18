using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr.Action;
using HzNS.Cmdr.Builder;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr
{
    public class Entry
    {
        private Entry()
        {
        }

        private static Entry _instance;

        // ReSharper disable once InconsistentNaming
        private static readonly object _lock = new object();

        [SuppressMessage("ReSharper", "UseObjectOrCollectionInitializer")]
        [SuppressMessage("ReSharper", "InvertIf")]
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

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public Worker Try(string[] args)
        {
            var worker = new Worker();
            worker.RunOnce();
            return worker.Run(args);
        }

        // // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // // ReSharper disable once MemberCanBePrivate.Global
        // public static ILogger Log { get; set; }
        //
        // public Entry AddLogger(ILogger log)
        // {
        //     Log = log;
        //     return this;
        // }


        public static Worker NewCmdrWorker(string[] args = null)
        {
            return CreateDefaultWorker(args)
                .UseSerilog((configuration) => configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day)
                    .CreateLogger());
        }

        private static Worker CreateDefaultWorker(string[] args)
        {
            var worker = new Worker();
            worker.RunOnce();
            return worker;
        }
    }


    public sealed class Worker
    {
        // public Worker(ILogger log)
        // {
        //     this.log = log;
        // }

        // ReSharper disable once InconsistentNaming
        public ILogger log;

        // ReSharper disable once IdentifierTypo
        public Worker UseSerilog(Func<LoggerConfiguration, Logger> func = null)
        {
            if (func == null)
            {
                log = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    .WriteTo.Console()
                    .CreateLogger();
            }
            else
            {
                log = func.Invoke(new LoggerConfiguration());
            }

            var builder = new ContainerBuilder();
            builder.RegisterLogger(autowireProperties: true);

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

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int ParsedCount { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool Parsed { get; set; }

        //         // ReSharper disable once NotAccessedField.Local
        //         // ReSharper disable once InconsistentNaming
        // #pragma warning disable 649
        //         internal ILogger _log;
        // #pragma warning restore 649
        //
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // public ILogger Log { get; set; }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void RunOnce()
        {
            // no logger
        }

        public Worker With(IRootCommand rootCommand)
        {
            _root = rootCommand;
            return this;
        }

        private IRootCommand _root;

        public Worker Run(string[] args)
        {
            // Entry.Log.Information("YES IT IS");
            log.Information("YES IT IS");
            if (_root == null) return this;

            // ReSharper disable once NotAccessedVariable
            var position = 0;
            position = match(_root, args, position);

            if (position < 0)
            {
                onCommandMatched(args, 0, "", _root);
            }

            return this;
        }


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private int match(ICommand command, string[] args, int position, int level = 1)
        {
            var ok = false;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                var arg = args[i];
                var isOpt = arg.StartsWith("-");
                var longOpt = arg.StartsWith("--");

                if (!isOpt && command.SubCommands != null)
                    foreach (var cmd in command.SubCommands)
                    {
                        ok = cmd.Match(arg, false);
                        if (!ok) continue;

                        onCommandMatched(args, i + 1, arg, cmd);

                        return match(cmd, args, i + 1, level + 1);
                    }

                if (ok) continue;

                // ReSharper disable once InvertIf
                if (isOpt && command.Flags != null)
                    foreach (var flg in command.Flags)
                    {
                        ok = flg.Match(arg, longOpt);
                        if (!ok) continue;

                        onFlagMatched(args, i + 1, arg, flg);
                        break;
                    }

                // if (ok) continue;

                // 
            }

            return -1;
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void onCommandMatched(IEnumerable<string> args, int position, string arg, ICommand cmd)
        {
            if (!(cmd is IAction action)) return;

            var remainArgs = args.Where((it, idx) => idx >= position);
            action.Invoke(this, remainArgs);

            // throw new NotImplementedException();
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void onFlagMatched(IEnumerable<string> args, int position, string arg, IFlag flag)
        {
            if (!(flag.Owner is IAction action)) return;

            var remainArgs = args.Where((it, idx) => idx >= position);
            action.Invoke(this, remainArgs);
        }


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void collect(ICommand command)
        {
            foreach (var cmd in command.SubCommands)
            {
                collect(cmd);
            }

            foreach (var flg in command.Flags)
            {
            }
        }
    }
}