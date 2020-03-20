#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Builder;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Handlers;
using HzNS.Cmdr.Tool;
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
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    public sealed class Worker : WorkerFunctions
    {
        public const string FirstUnsortedGroup = "0111.Unsorted";
        public const string SysMgmtGroup = "y099.System";
        public const string SysMiscGroup = "y000.Misc";
        public const string LastUnsortedGroup = "z111.Unsorted";


        private IRootCommand _root;

        public Worker(IRootCommand root)
        {
            _root = root;
            log = Log.Logger;
        }

        public int ParsedCount { get; set; }

        public bool Parsed { get; set; }

        public ICommand? ParsedCommand { get; internal set; }
        public IFlag? ParsedFlag { get; internal set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        // ReSharper disable once UnusedMember.Global
        public IRootCommand RootCommand => _root;

        public bool EnableDuplicatedCharThrows { get; set; } = false;
        public bool EnableEmptyLongFieldThrows { get; set; } = false;
        public bool EnableUnknownCommandThrows { get; set; } = false;
        public bool EnableUnknownFlagThrows { get; set; } = false;
        public int TabStop { get; set; } = 45;


        internal Worker runOnce()
        {
            // NOTE that the logger `log` is not ready yet at this time.
            ColorifyEnabler.Enable();
            return this;
        }

        public Worker With(IRootCommand rootCommand)
        {
            _root = rootCommand;
            preloadCommandDefinitions();
            return this;
        }

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
                    if (!onCommandMatched(args, pos > 0 ? pos + 1 : pos, "", ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }
                else
                {
                    if (!onCommandMatched(args, position, args[position], ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }

                Parsed = true;
            }
            catch (WantHelpScreenException ex)
            {
                // show help screen
                log.Debug("showing the help screen ...");
                ShowHelpScreen(this, ex.RemainArgs);
            }
            catch (ShouldBeStopException)
            {
                // not error
            }
            catch (CmdrException ex)
            {
                log.Error(ex, "Error occurs");
            }
            finally
            {
                ColorifyEnabler.Reset();
            }
        }

        public bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null)
        {
            return walkFor(parent ?? _root, commandsWatcher, flagsWatcher);
        }


        #region debug helpers

        // ReSharper disable once InconsistentNaming
        private void errPrint(string message)
        {
            Console.Error.WriteLine(message);
        }

        #endregion

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
                log = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
                    .WriteTo.Console()
                    .CreateLogger();
            else
                log = func.Invoke(new LoggerConfiguration());

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

        #region helpers for With(), Xref

        #region xref class and member container

        #region Xref class

        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Xref
        {
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
                    foreach (var a in cmd.Aliases)
                        if (!string.IsNullOrWhiteSpace(a))
                            if (!SubCommandsAliasNames.TryAdd(a, cmd))
                            {
                                w.OnDuplicatedCommandChar?.Invoke(false, a, cmd);
                                if (w.EnableDuplicatedCharThrows)
                                    throw new DuplicationCommandCharException(false, a, cmd);
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
                if (!string.IsNullOrWhiteSpace(flag.Long))
                {
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
                    foreach (var a in flag.Aliases)
                        if (!string.IsNullOrWhiteSpace(a))
                            if (!FlagsAliasNames.TryAdd(a, flag))
                            {
                                w.OnDuplicatedFlagChar?.Invoke(false, a, owner, flag);
                                if (w.EnableDuplicatedCharThrows)
                                    throw new DuplicationFlagCharException(false, a, flag, owner);
                            }
            }

            #region properties

            // public ICommand Command { get; set; } = null;
            public Dictionary<string, ICommand> SubCommandsShortNames { get; } =
                new Dictionary<string, ICommand>();

            public Dictionary<string, ICommand> SubCommandsLongNames { get; } = new Dictionary<string, ICommand>();

            public Dictionary<string, ICommand> SubCommandsAliasNames { get; } =
                new Dictionary<string, ICommand>();

            public Dictionary<string, IFlag> FlagsShortNames { get; } = new Dictionary<string, IFlag>();
            public Dictionary<string, IFlag> FlagsLongNames { get; } = new Dictionary<string, IFlag>();
            public Dictionary<string, IFlag> FlagsAliasNames { get; } = new Dictionary<string, IFlag>();

            #endregion
        }

        #endregion

        // ReSharper disable once CollectionNeverUpdated.Local
        private readonly Dictionary<ICommand, Xref> _xrefs = new Dictionary<ICommand, Xref>();

        #endregion

        #region Preloading, Build Xref

        [SuppressMessage("ReSharper", "InvertIf")]
        private void preloadCommandDefinitions()
        {
            BuiltinOptions.InsertAll(_root);
            collectAndBuildXref(_root);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void collectAndBuildXref(ICommand command)
        {
            walkFor(command, (owner, cmd, level) =>
            {
                if (!_xrefs.ContainsKey(owner)) _xrefs.TryAdd(owner, new Xref());
                if (!_xrefs.ContainsKey(cmd)) _xrefs.TryAdd(cmd, new Xref());

                if (cmd.Owner != owner) cmd.Owner = owner;
                if (string.IsNullOrWhiteSpace(cmd.Group))
                    cmd.Group = FirstUnsortedGroup;

                _xrefs[owner].TryAddShort(this, cmd);
                _xrefs[owner].TryAddLong(this, cmd);
                _xrefs[owner].TryAddAliases(this, cmd);
                return true;
            }, (owner, flag, level) =>
            {
                if (!_xrefs.ContainsKey(owner)) _xrefs.TryAdd(owner, new Xref());

                if (flag.Owner != owner) flag.Owner = owner;
                if (string.IsNullOrWhiteSpace(flag.Group) && !string.IsNullOrWhiteSpace(flag.ToggleGroup))
                    flag.Group = flag.ToggleGroup;
                if (string.IsNullOrWhiteSpace(flag.Group))
                    flag.Group = FirstUnsortedGroup;

                _xrefs[owner].TryAddShort(this, owner, flag);
                _xrefs[owner].TryAddLong(this, owner, flag);
                _xrefs[owner].TryAddAliases(this, owner, flag);
                return true; // return false to break the walkForFlags' loop.
            });
            log.Debug("_xrefs was built.");
        }

        #endregion

        #endregion

        #region helpers for Walk()

        private bool walkFor(ICommand parent,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null,
            int level = 0)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var f in parent.Flags)
                if (flagsWatcher != null && flagsWatcher(parent, f, level) == false)
                    return false;

            if (parent.SubCommands == null) return true;

            foreach (var cmd in parent.SubCommands)
            {
                if (commandsWatcher != null && commandsWatcher.Invoke(parent, cmd, level) == false)
                    return false;
                if (walkFor(cmd, commandsWatcher, flagsWatcher, level + 1) == false)
                    return false;
            }

            return true;
        }

        // ReSharper disable once UnusedMember.Local
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        private bool walkForFlags(ICommand parent, Func<ICommand, IFlag, int, bool> watcher, int level = 0)
        {
            if (parent.Flags.Any(f => watcher != null && watcher(parent, f, level) == false)) return false;

            if (parent.SubCommands == null) return true;

            return parent.SubCommands.All(cmd => walkForFlags(cmd, watcher, level + 1));
        }

        #endregion

        #region helpers for Run()

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private int match(ICommand command, string[] args, int position, int level)
        {
            log.Debug($"  - match for command: {command.backtraceTitles}");
            var matchedPosition = -1;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                bool ok;
                var arg = args[i];
                var isOpt = arg.StartsWith("-");
                var longOpt = arg.StartsWith("--");

                log.Debug($"    -> arg {i}: '{arg}'");
                if (!isOpt)
                {
                    #region matching command

                    if (command.SubCommands != null && command.SubCommands.Count > 0)
                    {
                        var pos = i + 1;
                        foreach (var cmd in command.SubCommands)
                        {
                            // ReSharper disable once RedundantArgumentDefaultValue
                            ok = cmd.Match(arg, false);
                            if (!ok) ok = cmd.Match(arg, true);
                            if (!ok) continue;

                            log.Debug($"    ++ command matched: {cmd.backtraceTitles}");

                            ParsedCommand = cmd;
                            ParsedFlag = null;

                            pos = i + 1;
                            if (cmd.SubCommands.Count > 0)
                            {
                                if (i == arg.Length - 1) throw new WantHelpScreenException();

                                var pos1 = match(cmd, args, i + 1, level + 1);
                                if (pos1 < 0) matchedPosition = pos;
                                pos = pos1;
                            }

                            // onCommandMatched(args, i + 1, arg, cmd);

                            if (matchedPosition < 0 || pos > 0)
                                matchedPosition = pos;
                            command = cmd;
                            break;
                        }

                        if (matchedPosition < 0)
                        {
                            log.Debug($"level {level} (cmd can't matched): returning {-position - 1}");
                            onCommandCannotMatched(args, i, arg, command);
                            return -position - 1;
                        }

                        if (pos < 0 && matchedPosition > 0)
                            return pos;
                    }
                    else
                    {
                        log.Debug($"level {level} (no sub-cmds): returning {matchedPosition}");
                        onCommandCannotMatched(args, i, arg, command);
                        return matchedPosition;
                    }

                    #endregion

                    continue;
                }

                // matching for flags of 'command'

                var fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
                var cc = command;
                parentFlags:
                ok = false;
                foreach (var flg in cc.Flags)
                {
                    ok = flg.Match(fragment, longOpt);
                    if (!ok) continue;

                    log.Debug($"    ++ flag matched: {Util.SwitchChar(longOpt)}{flg.Long}");

                    ParsedFlag = flg;

                    onFlagMatched(args, i + 1, fragment, longOpt, flg);

                    matchedPosition = i + 1;
                    break;
                }

                // ReSharper disable once InvertIf
                if (!ok)
                {
                    if (cc.Owner != null && cc.Owner != cc)
                    {
                        cc = cc.Owner;
                        log.Debug($"    - try finding flags for its parent: {cc.backtraceTitles}");
                        goto parentFlags;
                    }

                    log.Debug($"can't match a flag: '{arg}'/fragment='{fragment}'.");
                    onFlagCannotMatched(args, i, fragment, longOpt, command);
                }

                // if (ok) continue;

                // 
            }

            // ReSharper disable once InvertIf
            if (matchedPosition < 0)
            {
                log.Debug($"level {level}: returning {-position - 1}");
                return -position - 1;
            }

            return matchedPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="position"></param>
        /// <param name="arg"></param>
        /// <param name="cmd"></param>
        /// <returns>false means no action triggered.</returns>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once UnusedParameter.Local
        private bool onCommandMatched(IEnumerable<string> args, int position, string arg, ICommand cmd)
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            var root = cmd.FindRoot();
            if (root?.PreAction != null && !root.PreAction.Invoke(this, remainArgs))
                throw new ShouldBeStopException();
            if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(this, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                log.Debug($"---> matched command: '{cmd}', remains: '{string.Join(",", remainArgs)}'");

                if (cmd is IAction action)
                    action.Invoke(this, remainArgs);
                else if (cmd.Action != null)
                    cmd.Action.Invoke(this, remainArgs);
                else
                    return false;
                return true;
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
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private void onFlagMatched(IEnumerable<string> args, int position, string fragment, in bool longOpt,
            IFlag flag)
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            if (flag.PreAction != null && !flag.PreAction.Invoke(this, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                var sw = Util.SwitchChar(longOpt);
                log.Debug($"  flag matched: {sw}{fragment}");
                flag.OnSet?.Invoke(this, flag.getDefaultValue(), flag.getDefaultValue());

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
        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once SuggestBaseTypeForParameter
        private void onCommandCannotMatched(string[] args, in int position, string arg, ICommand cmd)
        {
            // throw new NotImplementedException();
            errPrint($"- Unknown command(arg): '{args[position]}'. context: '{cmd.backtraceTitles}'.");
            suggestCommands(args, position, arg, cmd);
            if (EnableUnknownCommandThrows)
                throw new UnknownCommandException(false, arg, cmd);
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once UnusedParameter.Local
        private void onFlagCannotMatched(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        {
            var sw = Util.SwitchChar(longOpt);
            errPrint($"- Unknown flag({sw}{fragment}): '{args[position]}'. context: '{cmd.backtraceTitles}'");
            suggestFlags(args, position, fragment, longOpt, cmd);
            if (EnableUnknownFlagThrows)
                throw new UnknownFlagException(!longOpt, fragment, cmd);
        }

        #endregion

        #region suggestions

        private void suggestCommands(string[] args, in int position, string tag, ICommand cmd)
        {
            var xref = _xrefs[cmd];
            suggestFor(tag, xref.SubCommandsLongNames);
            suggestFor(tag, xref.SubCommandsAliasNames);
            suggestFor(tag, xref.SubCommandsShortNames);
        }

        private void suggestFlags(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        {
            var xref = _xrefs[cmd];
            if (longOpt)
            {
                suggestFor(fragment, xref.FlagsLongNames);
                suggestFor(fragment, xref.FlagsAliasNames);
            }
            else
            {
                suggestFor(fragment,xref.FlagsShortNames);
            }
        }

        private void suggestFor(string tag, Dictionary<string,ICommand> dataset)
        {
            
        }

        private void suggestFor(string tag, Dictionary<string,IFlag> dataset)
        {
            
        }
        #endregion
    }
}