#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr.Action;
using HzNS.Cmdr.Builder;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Handlers;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HzNS.Cmdr
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public sealed class Worker : DefaultHandlers
    {
        public Worker(IRootCommand root)
        {
            _root = root;
            log = Log.Logger;
        }

        #region logger

        // public Worker(ILogger log)
        // {
        //     this.log = log;
        // }

        // #pragma warning disable 649
        //         internal ILogger _log;
        // #pragma warning restore 649
        //
        // public ILogger Log { get; set; }

        public ILogger log;

        public Worker UseSerilog(Func<LoggerConfiguration, Logger>? func = null)
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

        #endregion

        public int ParsedCount { get; set; }

        public bool Parsed { get; set; }

        private IRootCommand _root;

        public bool EnableDuplicatedCharThrows { get; set; } = false;
        public bool EnableEmptyLongFieldThrows { get; set; } = false;


        internal Worker runOnce()
        {
            // NOTE that the logger `log` is not ready yet at this time.
            return this;
        }

        public Worker With(IRootCommand rootCommand)
        {
            _root = rootCommand;
            preloadCommandDefinitions();
            return this;
        }

        #region helpers for With()

        #region xref class and member container

        #region Xref class

        [SuppressMessage("ReSharper", "InvertIf")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Xref
        {
            #region properties

            // public ICommand Command { get; set; } = null;
            public Dictionary<string, ICommand> SubCommandsShortNames { get; set; } =
                new Dictionary<string, ICommand>();

            public Dictionary<string, ICommand> SubCommandsLongNames { get; set; } = new Dictionary<string, ICommand>();

            public Dictionary<string, ICommand> SubCommandsAliasNames { get; set; } =
                new Dictionary<string, ICommand>();

            public Dictionary<string, IFlag> FlagsShortNames { get; set; } = new Dictionary<string, IFlag>();
            public Dictionary<string, IFlag> FlagsLongNames { get; set; } = new Dictionary<string, IFlag>();
            public Dictionary<string, IFlag> FlagsAliasNames { get; set; } = new Dictionary<string, IFlag>();

            #endregion

            public void TryAddShort(Worker w, ICommand cmd)
            {
                if (!string.IsNullOrWhiteSpace(cmd.Short))
                    if (!SubCommandsShortNames.TryAdd(cmd.Short, cmd))
                    {
                        w.OnDuplicatedCommandChar?.Invoke(true, cmd.Short, cmd);
                        if (w.EnableDuplicatedCharThrows)
                            throw new DuplicationCommandCharException(true, cmd.Short, cmd);
                    }
            }

            public void TryAddLong(Worker w, ICommand cmd)
            {
                if (!string.IsNullOrWhiteSpace(cmd.Long))
                {
                    if (!SubCommandsLongNames.TryAdd(cmd.Long, cmd))
                    {
                        w.OnDuplicatedCommandChar?.Invoke(false, cmd.Long, cmd);
                        if (w.EnableDuplicatedCharThrows)
                            throw new DuplicationCommandCharException(false, cmd.Long, cmd);
                    }
                }
                else
                {
                    if (w.EnableEmptyLongFieldThrows)
                        throw new EmptyCommandLongFieldException(false, cmd.Long, cmd);
                }
            }

            public void TryAddAliases(Worker w, ICommand cmd)
            {
                if (cmd.Aliases != null)
                {
                    foreach (var a in cmd.Aliases)
                    {
                        if (!string.IsNullOrWhiteSpace(a))
                            if (!SubCommandsAliasNames.TryAdd(a, cmd))
                            {
                                w.OnDuplicatedCommandChar?.Invoke(false, a, cmd);
                                if (w.EnableDuplicatedCharThrows)
                                    throw new DuplicationCommandCharException(false, a, cmd);
                            }
                    }
                }
            }

            public void TryAddShort(Worker w, ICommand owner, IFlag flag)
            {
                if (!string.IsNullOrWhiteSpace(flag.Short))
                    if (!FlagsShortNames.TryAdd(flag.Short, flag))
                    {
                        w.OnDuplicatedFlagChar?.Invoke(true, flag.Short, owner, flag);
                        if (w.EnableDuplicatedCharThrows)
                            throw new DuplicationFlagCharException(true, flag.Short, flag, owner);
                    }
            }

            public void TryAddLong(Worker w, ICommand owner, IFlag flag)
            {
                if (!string.IsNullOrWhiteSpace(flag.Long)){
                    if (!FlagsLongNames.TryAdd(flag.Long, flag))
                    {
                        w.OnDuplicatedFlagChar?.Invoke(false, flag.Long, owner, flag);
                        if (w.EnableDuplicatedCharThrows)
                            throw new DuplicationFlagCharException(false, flag.Long, flag, owner);
                    }
                }
                else
                {
                    if (w.EnableEmptyLongFieldThrows)
                        throw new EmptyFlagLongFieldException(false, flag.Long, flag, owner);
                }
            }

            public void TryAddAliases(Worker w, ICommand owner, IFlag flag)
            {
                if (flag.Aliases != null)
                {
                    foreach (var a in flag.Aliases)
                    {
                        if (!string.IsNullOrWhiteSpace(a))
                            if (!FlagsAliasNames.TryAdd(a, flag))
                            {
                                w.OnDuplicatedFlagChar?.Invoke(false, a, owner, flag);
                                if (w.EnableDuplicatedCharThrows)
                                    throw new DuplicationFlagCharException(false, a, flag, owner);
                            }
                    }
                }
            }
        }

        #endregion

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<ICommand, Xref> _xrefs = new Dictionary<ICommand, Xref>();

        #endregion

        [SuppressMessage("ReSharper", "InvertIf")]
        private void preloadCommandDefinitions()
        {
            walkFor(_root, (owner, cmd) =>
            {
                if (!_xrefs.ContainsKey(owner)) _xrefs.TryAdd(owner, new Xref());
                if (!_xrefs.ContainsKey(cmd)) _xrefs.TryAdd(cmd, new Xref());
                _xrefs[owner].TryAddShort(this, cmd);
                _xrefs[owner].TryAddLong(this, cmd);
                _xrefs[owner].TryAddAliases(this, cmd);
                return true;
            }, (owner, flag) =>
            {
                if (!_xrefs.ContainsKey(owner)) _xrefs.TryAdd(owner, new Xref());
                _xrefs[owner].TryAddShort(this, owner, flag);
                _xrefs[owner].TryAddLong(this, owner, flag);
                _xrefs[owner].TryAddAliases(this, owner, flag);
                return true; // return false to break the walkForFlags' loop.
            });
            log.Debug($"_xrefs was built.");
        }

        #endregion

        #region helpers for Walk()

        private bool walkFor(ICommand parent,
            Func<ICommand, ICommand, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, bool>? watcher = null)
        {
            foreach (var f in parent.Flags)
            {
                if (watcher != null && watcher(parent, f) == false)
                    return false;
            }

            if (parent.SubCommands == null) return true;

            foreach (var cmd in parent.SubCommands)
            {
                if (commandsWatcher != null && commandsWatcher.Invoke(parent, cmd) == false)
                    return false;
                if (walkFor(cmd, commandsWatcher, watcher) == false)
                    return false;
            }

            return true;
        }

        private bool walkForFlags(ICommand parent, Func<ICommand, IFlag, bool> watcher)
        {
            if (parent.Flags.Any(f => watcher != null && watcher(parent, f) == false))
            {
                return false;
            }

            if (parent.SubCommands == null) return true;

            return parent.SubCommands.All(cmd => walkForFlags(cmd, watcher) != false);
        }

        #endregion

        public void Run(string[] args)
        {
            // Entry.Log.Information("YES IT IS");
            // log.Information("YES IT IS");
            if (_root == null) return;

            // ReSharper disable once NotAccessedVariable
            var position = 0;
            try
            {
                position = match(_root, args, position, 1);

                if (position < 0)
                {
                    // -1-position
                    // m 0: -1 => 0 (x+1)
                    // m 1: -2 => 1
                    var pos = -(1 + position);
                    onCommandMatched(args, pos > 0 ? pos + 1 : pos, "", _root);
                }

                Parsed = true;
            }
            catch (WantHelpScreenException)
            {
                // show help screen
            }
            catch (CmdrException ex)
            {
                log.Error(ex, "Error occurs");
            }
        }

        #region helpers for Run()

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private int match(ICommand command, string[] args, int position, int level)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                var ok = false;
                var arg = args[i];
                var isOpt = arg.StartsWith("-");
                var longOpt = arg.StartsWith("--");

                if (!isOpt && command.SubCommands != null)
                {
                    foreach (var cmd in command.SubCommands)
                    {
                        // ReSharper disable once RedundantArgumentDefaultValue
                        ok = cmd.Match(arg, false);
                        if (!ok) ok = cmd.Match(arg, true);
                        if (!ok) continue;

                        if (cmd.SubCommands.Count > 0)
                        {
                            if (i == arg.Length - 1)
                            {
                                throw new WantHelpScreenException();
                            }

                            return match(cmd, args, i + 1, level + 1);
                        }

                        onCommandMatched(args, i + 1, arg, cmd);
                        return i + 1;
                    }

                    onCommandCannotMatched(args, i, arg, command);
                    return -position - 1;
                }

                // if (ok) continue;

                // ReSharper disable once InvertIf
                if (isOpt && command.Flags != null)
                {
                    var fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
                    ok = false;
                    foreach (var flg in command.Flags)
                    {
                        ok = flg.Match(fragment, longOpt);
                        if (!ok) continue;

                        onFlagMatched(args, i + 1, fragment, longOpt, flg);
                        break;
                    }

                    if (!ok)
                    {
                        onFlagCannotMatched(args, i, fragment, longOpt, command);
                    }
                }

                // if (ok) continue;

                // 
            }

            return -position - 1;
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private void onCommandMatched(IEnumerable<string> args, int position, string arg, ICommand cmd)
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            var root = cmd.FindRoot();
            if (root?.PreAction != null && !root.PreAction.Invoke(this, remainArgs))
                return;
            if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(this, remainArgs))
                return;

            try
            {
                if (!(cmd is IAction action))
                    cmd.Action?.Invoke(this, remainArgs);
                else
                    action.Invoke(this, remainArgs);
            }
            finally
            {
                if (root != cmd) cmd.PostAction?.Invoke(this, remainArgs);
                root?.PostAction?.Invoke(this, remainArgs);
            }

            // throw new NotImplementedException();
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


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void onFlagMatched(IEnumerable<string> args, int position, string fragment, in bool longOpt, IFlag flag)
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            if (flag.PreAction != null && !flag.PreAction.Invoke(this, remainArgs))
                return;

            try
            {
                flag.OnSet?.Invoke(this, flag.DefaultValue, flag.DefaultValue);

                // ReSharper disable once SuspiciousTypeConversion.Global
                // ReSharper disable once UseNegatedPatternMatching
                var action = flag as IAction;
                if (action == null)
                    flag.Action?.Invoke(this, remainArgs);
                else
                    action.Invoke(this, remainArgs);
            }
            finally
            {
                flag.PostAction?.Invoke(this, remainArgs);
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once InconsistentNaming
        private void onCommandCannotMatched(string[] args, in int position, string arg, ICommand command)
        {
            // throw new NotImplementedException();
            errPrint($"- cannot parsed command: '{arg}'. context: '{command.backtraceTitles}'.");
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void onFlagCannotMatched(string[] args, in int position, string fragment, in bool longOpt,
            ICommand command)
        {
            errPrint($"- cannot parsed command: '{args[position]}'. context: '{command.backtraceTitles}'");
        }

        #endregion

        // ReSharper disable once InconsistentNaming
        private void errPrint(string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}