#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Internal;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Enrichers;
using HzNS.Cmdr.Tool.Ext;
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
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public sealed class Worker : WorkerFunctions, IDefaultHandlers, IDefaultMatchers
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

        // public int ParsedCount { get; set; }
        //
        // public bool Parsed { get; set; }
        //
        // public ICommand? ParsedCommand { get; set; }
        // public IFlag? ParsedFlag { get; set; }

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        // ReSharper disable once UnusedMember.Global
        public IRootCommand RootCommand => _root;

        public int ParsedCount { get; set; }
        public bool Parsed { get; set; }
        public ICommand? ParsedCommand { get; set; }
        public IFlag? ParsedFlag { get; set; }

        public bool EnableDuplicatedCharThrows { get; set; } = false;
        public bool EnableEmptyLongFieldThrows { get; set; } = false;
        public bool EnableUnknownCommandThrows { get; set; } = false;
        public bool EnableUnknownFlagThrows { get; set; } = false;
        public int TabStop { get; set; } = 45;


        internal Worker runOnce()
        {
            // NOTE that the logger `log` is not ready yet at this time.
            ColorifyEnabler.Enable();

            DefaultMatchers.EnableCmdrLogTrace = Util.GetEnvValueBool("CMDR_TRACE");
            if (DefaultMatchers.EnableCmdrLogTrace)
                DefaultMatchers.EnableCmdrLogDebug = true;
            DefaultMatchers.EnableCmdrLogDebug = Util.GetEnvValueBool("CMDR_DEBUG");
            Cmdr.Instance.Store.Set("debug", Util.GetEnvValueBool("DEBUG"));
            Cmdr.Instance.Store.Set("trace", Util.GetEnvValueBool("TRACE"));
            Cmdr.Instance.Store.Set("verbose", Util.GetEnvValueBool("VERBOSE"));
            Cmdr.Instance.Store.Set("verbose-level", Util.GetEnvValueInt("VERBOSE_LEVEL", 5));
            Cmdr.Instance.Store.Set("quiet", Util.GetEnvValueBool("QUIET"));

            return this;
        }

        public Worker With(IRootCommand rootCommand)
        {
            _root = rootCommand;
            preloadCommandDefinitions();
            return this;
        }

        // ReSharper disable once UnusedMember.Local
        private void f4()
        {
            log.Information("YES IT IS");
            // B /= A;
            throw new System.Exception("test");
        }

        public void Run(string[] args)
        {
            // Entry.Log.Information("YES IT IS");
            // log.Information("YES IT IS");
            // f4();
            if (_root == null)
                return;

            // ReSharper disable once NotAccessedVariable
            var position = 0;
            try
            {
                position = this.match(_root, args, position, 1);

                if (position < 0)
                {
                    // -1-position
                    // m 0: -1 => 0 (x+1)
                    // m 1: -2 => 1
                    var pos = -(1 + position);
                    if (!this.onCommandMatched(args, pos > 0 ? pos + 1 : pos, "", ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }
                else
                {
                    if (!this.onCommandMatched(args, position, args[position - 1], ParsedCommand ?? _root))
                        throw new WantHelpScreenException();
                }

                Parsed = true;
            }
            catch (WantHelpScreenException ex)
            {
                // f4();
                // show help screen
                this.logDebug("showing the help screen ...");
                Parsed = true;
                ShowHelpScreen(this, ex.RemainArgs);
            }
            catch (ShouldBeStopException)
            {
                // not an error
                Parsed = true;
            }
            catch (CmdrException ex)
            {
                this.logError(ex, "Error occurs");
            }
            catch (System.Exception ex)
            {
                this.logError(ex, $"args: {args}, position: {position}");
                throw;
            }
            finally
            {
                var dump = Util.GetEnvValueBool("CMDR_DUMP");
                if (dump)
                {
                    Console.WriteLine("\n\nDump the Store:");
                    var store = Cmdr.Instance.Store;
                    store.Dump();
                    Console.WriteLine("\n\nDump the Flags:");
                    DumpValues();
                }

                ColorifyEnabler.Reset();
            }
        }

        public override bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null)
        {
            return walkFor(parent ?? _root, commandsWatcher, flagsWatcher);
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

        public ILogger log { get; private set; }

        // public ILogger log;
        // public LogWrapper log;

        public Worker UseSerilog(Func<LoggerConfiguration, Logger>? func = null)
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

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once ConvertToAutoProperty
        public Dictionary<ICommand, Xref> xrefs => _xrefs;
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
                var x = xrefs;
                if (!x.ContainsKey(owner))
                    x.TryAdd(owner, new Xref());
                if (!x.ContainsKey(cmd))
                    x.TryAdd(cmd, new Xref());

                if (cmd.Owner != owner) cmd.Owner = owner;
                if (string.IsNullOrWhiteSpace(cmd.Group))
                    cmd.Group = FirstUnsortedGroup;

                var xx = x[owner];
                xx.TryAddShort(this, cmd);
                xx.TryAddLong(this, cmd);
                xx.TryAddAliases(this, cmd);
                return true;
            }, (owner, flag, level) =>
            {
                var x = xrefs;
                if (!x.ContainsKey(owner))
                    x.TryAdd(owner, new Xref());

                if (flag.Owner != owner) flag.Owner = owner;
                if (string.IsNullOrWhiteSpace(flag.Group) && !string.IsNullOrWhiteSpace(flag.ToggleGroup))
                    flag.Group = flag.ToggleGroup;
                if (string.IsNullOrWhiteSpace(flag.Group))
                    flag.Group = FirstUnsortedGroup;

                var xx = x[owner];
                xx.TryAddShort(this, owner, flag);
                xx.TryAddLong(this, owner, flag);
                xx.TryAddAliases(this, owner, flag);
                return true; // return false to break the walkForFlags' loop.
            });
            this.logDebug("_xrefs was built.");
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

        private void DumpValues()
        {
            dumpValues(RootCommand);
        }

        private void dumpValues(ICommand parent)
        {
            walkFor(parent ?? _root,
                commandsWatcher: (owner, Cmdr, level) => true,
                flagsWatcher: (owner, flg, level) =>
                {
                    var s = Cmdr.Instance.Store;
                    var (slot,vk) = s.FindByKeys(flg.ToKeys());
                    // ReSharper disable once InvertIf
                    if (slot != null)
                    {
                        var v = slot.Values[vk];
                        // if (v?.GetType().IsArray == true)
                        //     v = "[" + string.Join(",", v as object[] ?? throw new CmdrException()) + "]";
                        Console.WriteLine($"  {flg.ToDottedKey(),-45}=> [{flg.HitCount}] {v?.ToStringEx()}");
                    }

                    return true;
                });
        }

        // #region helpers for Run() - match
        //
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // private int match(ICommand command, string[] args, int position, int level)
        // {
        //     log.Debug("  - match for command: {CommandTitle}", command.backtraceTitles);
        //
        //     var matchedPosition = -1;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     string fragment;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     int pos, len, size;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     int ate;
        //     // ReSharper disable once TooWideLocalVariableScope
        //     object? value;
        //
        //     // ReSharper disable once ForCanBeConvertedToForeach
        //     for (var i = position; i < args.Length; i++)
        //     {
        //         bool ok;
        //         var arg = args[i];
        //         var isOpt = arg.StartsWith("-");
        //         var longOpt = arg.StartsWith("--");
        //
        //         log.Debug("    -> arg {Index}: {Argument}", i, arg);
        //         if (!isOpt)
        //         {
        //             #region matching command
        //
        //             if (command.SubCommands != null && command.SubCommands.Count > 0)
        //             {
        //                 var positionCopy = i + 1;
        //                 foreach (var cmd in command.SubCommands)
        //                 {
        //                     // ReSharper disable once RedundantArgumentDefaultValue
        //                     ok = cmd.Match(arg, false);
        //                     if (!ok) ok = cmd.Match(arg, true);
        //                     if (!ok) continue;
        //
        //                     log.Debug("    ++ command matched: {CommandTitle}", cmd.backtraceTitles);
        //
        //                     ParsedCommand = cmd;
        //                     ParsedFlag = null;
        //
        //                     positionCopy = i + 1;
        //                     if (cmd.SubCommands.Count > 0)
        //                     {
        //                         if (i == arg.Length - 1) throw new WantHelpScreenException();
        //
        //                         var pos1 = match(cmd, args, i + 1, level + 1);
        //                         if (pos1 < 0) matchedPosition = positionCopy;
        //                         positionCopy = pos1;
        //                     }
        //
        //                     // onCommandMatched(args, i + 1, arg, cmd);
        //
        //                     if (matchedPosition < 0 || positionCopy > 0)
        //                         matchedPosition = positionCopy;
        //                     command = cmd;
        //                     break;
        //                 }
        //
        //                 if (matchedPosition < 0)
        //                 {
        //                     log.Debug("level {Level} (cmd can't matched): returning {Position}", level, -position - 1);
        //                     onCommandCannotMatched(args, i, arg, command);
        //                     return -position - 1;
        //                 }
        //
        //                 if (positionCopy < 0 && matchedPosition > 0)
        //                     return positionCopy;
        //             }
        //             else
        //             {
        //                 log.Debug("level {Level} (no sub-cmds): returning {Position}", level, matchedPosition);
        //                 onCommandCannotMatched(args, i, arg, command);
        //                 return matchedPosition;
        //             }
        //
        //             #endregion
        //
        //             continue;
        //         }
        //
        //         // matching for flags of 'command'
        //
        //         var ccc = command;
        //         fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
        //         pos = 0;
        //         len = 1;
        //         size = fragment.Length;
        //         ate = 0;
        //
        //         forEachFragmentParts:
        //         var part = fragment.Substring(pos, len);
        //
        //         log.Debug("    - try finding flags for ccc: {CommandTitle}", ccc.backtraceTitles);
        //         
        //         backtraceAllParentFlags:
        //         ok = false;
        //         foreach (var flg in ccc.Flags)
        //         {
        //             ok = flg.Match(ref part, fragment, pos, longOpt);
        //             if (!ok) continue;
        //
        //             // a flag matched ok, try extracting its value from commandline arguments
        //             (ate, value) = tryExtractingValue(flg, args, i, part, pos);
        //
        //             log.Debug("    ++ flag matched: {SW:l}{flgLong:l} {value}",
        //                 Util.SwitchChar(longOpt), flg.Long, value);
        //
        //             len = part.Length;
        //             ParsedFlag = flg;
        //             onFlagMatched(args, i + 1, part, longOpt, flg);
        //             matchedPosition = i + 1;
        //             break;
        //         }
        //
        //         // ReSharper disable once InvertIf
        //         if (!ok)
        //         {
        //             if (ccc.Owner != null && ccc.Owner != ccc)
        //             {
        //                 ccc = ccc.Owner;
        //                 log.Debug("    - try finding flags for its(ccc) parent: {CommandTitle}", ccc.backtraceTitles);
        //                 goto backtraceAllParentFlags;
        //             }
        //
        //             log.Debug("can't match a flag: {Argument}/part={Part}/fragment={Fragment}.", arg, part, fragment);
        //             onFlagCannotMatched(args, i, part, longOpt, command);
        //         }
        //
        //         if (pos + len < size)
        //         {
        //             pos += len;
        //             len = 1;
        //             log.Debug("    - for next part: {Part}", fragment.Substring(pos, len));
        //             ccc = command;
        //             goto forEachFragmentParts;
        //         }
        //
        //         if (ate > 0)
        //         {
        //             i += ate;
        //         }
        //
        //         // 
        //     }
        //
        //     // ReSharper disable once InvertIf
        //     if (matchedPosition < 0)
        //     {
        //         log.Debug("level {Level}: returning {Position}", level, -position - 1);
        //         return -position - 1;
        //     }
        //
        //     return matchedPosition;
        // }

        // // ReSharper disable once SuggestBaseTypeForParameter
        // private (int ate, object? value) tryExtractingValue(IFlag flg, string[] args, int i, string part, int pos)
        // {
        //     var ate = 0;
        //     object? val = null;
        //     
        //     var remains = args[i].Substring(pos + part.Length);
        //     bool? flipChar = null;
        //     if (remains.Length > 0)
        //     {
        //         flipChar = remains[0] switch
        //         {
        //             '-' => false,
        //             '+' => true,
        //             _ => null
        //         };
        //     }
        //
        //     var dv = flg.getDefaultValue();
        //     switch (dv)
        //     {
        //         case bool _:
        //             val = flipChar ?? true;
        //             break;
        //         
        //         case string _:
        //             ate = 1;
        //             val = args[i + ate];
        //             break;
        //         
        //         case string[] _:
        //             val = true;
        //             break;
        //     }
        //
        //     return (ate, val);
        // }
        //
        // #endregion

        // #region helpers for Run() - match(ed)
        //
        // /// <summary>
        // /// 
        // /// </summary>
        // /// <param name="args"></param>
        // /// <param name="position"></param>
        // /// <param name="arg"></param>
        // /// <param name="cmd"></param>
        // /// <returns>false means no action triggered.</returns>
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // // ReSharper disable once UnusedParameter.Local
        // private bool onCommandMatched(IEnumerable<string> args, int position, string arg, ICommand cmd)
        // {
        //     var remainArgs = args.Where((it, idx) => idx >= position).ToArray();
        //
        //     var root = cmd.FindRoot();
        //     if (root?.PreAction != null && !root.PreAction.Invoke(this, cmd, remainArgs))
        //         throw new ShouldBeStopException();
        //     if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(this, cmd, remainArgs))
        //         throw new ShouldBeStopException();
        //
        //     try
        //     {
        //         log.Debug("---> matched command: {cmd}, remains: {Args}", cmd, string.Join(",", remainArgs));
        //
        //         if (cmd is IAction action)
        //             action.Invoke(this, remainArgs);
        //         else if (cmd.Action != null)
        //             cmd.Action.Invoke(this, cmd, remainArgs);
        //         else
        //             return false;
        //         return true;
        //     }
        //     finally
        //     {
        //         if (root != cmd) cmd.PostAction?.Invoke(this, cmd, remainArgs);
        //         root?.PostAction?.Invoke(this, cmd, remainArgs);
        //     }
        //
        //     // throw new NotImplementedException();
        // }
        //
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // private void onFlagMatched(IEnumerable<string> args, int position, string fragment, in bool longOpt,
        //     IFlag flag)
        // {
        //     var remainArgs = args.Where((it, idx) => idx >= position).ToArray();
        //
        //     if (flag.PreAction != null && !flag.PreAction.Invoke(this, flag, remainArgs))
        //         throw new ShouldBeStopException();
        //
        //     try
        //     {
        //         // ReSharper disable once UnusedVariable
        //         var sw = Util.SwitchChar(longOpt);
        //         log.Debug("  ---> flag matched: {SW:l}{Fragment:l}", sw, fragment);
        //         if (flag.OnSet != null)
        //             flag.OnSet?.Invoke(this, flag, flag.getDefaultValue(), flag.getDefaultValue());
        //         else
        //             defaultOnSet?.Invoke(this, flag, flag.getDefaultValue(), flag.getDefaultValue());
        //
        //         // ReSharper disable once SuspiciousTypeConversion.Global
        //         // ReSharper disable once UseNegatedPatternMatching
        //         var action = flag as IAction;
        //         if (action == null)
        //             flag.Action?.Invoke(this, flag, remainArgs);
        //         else
        //             action.Invoke(this, remainArgs);
        //     }
        //     finally
        //     {
        //         flag.PostAction?.Invoke(this, flag, remainArgs);
        //     }
        // }
        //
        // // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // private Action<Worker, IBaseOpt, object?, object?>? defaultOnSet = (w, flg, oldVal, newVal) =>
        // {
        //     w.log.Debug("");
        // };
        //
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once UnusedParameter.Local
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once SuggestBaseTypeForParameter
        // private void onCommandCannotMatched(string[] args, in int position, string arg, ICommand cmd)
        // {
        //     // throw new NotImplementedException();
        //     errPrint($"- Unknown command(arg): '{args[position]}'. context: '{cmd.backtraceTitles}'.");
        //     suggestCommands(args, position, arg, cmd);
        //     if (EnableUnknownCommandThrows)
        //         throw new UnknownCommandException(false, arg, cmd);
        // }
        //
        // [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // // ReSharper disable once InconsistentNaming
        // // ReSharper disable once MemberCanBeMadeStatic.Local
        // // ReSharper disable once SuggestBaseTypeForParameter
        // // ReSharper disable once UnusedParameter.Local
        // private void onFlagCannotMatched(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        // {
        //     var sw = Util.SwitchChar(longOpt);
        //     errPrint($"- Unknown flag({sw}{fragment}): '{args[position]}'. context: '{cmd.backtraceTitles}'");
        //     suggestFlags(args, position, fragment, longOpt, cmd);
        //     if (EnableUnknownFlagThrows)
        //         throw new UnknownFlagException(!longOpt, fragment, cmd);
        // }
        //
        // #endregion
        //
        // #region suggestions
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestCommands(string[] args, in int position, string tag, ICommand cmd)
        // {
        //     var xref = _xrefs[cmd];
        //     suggestFor(tag, xref.SubCommandsLongNames);
        //     suggestFor(tag, xref.SubCommandsAliasNames);
        //     suggestFor(tag, xref.SubCommandsShortNames);
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFlags(string[] args, in int position, string fragment, in bool longOpt, ICommand cmd)
        // {
        //     var xref = _xrefs[cmd];
        //     if (longOpt)
        //     {
        //         suggestFor(fragment, xref.FlagsLongNames);
        //         suggestFor(fragment, xref.FlagsAliasNames);
        //     }
        //     else
        //     {
        //         suggestFor(fragment, xref.FlagsShortNames);
        //     }
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFor(string tag, Dictionary<string, ICommand> dataset)
        // {
        // }
        //
        // [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        // private void suggestFor(string tag, Dictionary<string, IFlag> dataset)
        // {
        // }
        //
        // #endregion
    }
}