#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Ext;

namespace HzNS.Cmdr.Internal
{
    /// <summary>
    /// injected by IDefaultMatchers.
    /// ref:
    /// Worker : WorkerFunctions, IDefaultHandlers, IDefaultMatchers
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class DefaultMatchers
    {
        /// <summary>
        /// greedy mode: prefer to longest Long option.
        ///
        /// for example, think about there are two options: `--addr` and `--add`, in the
        /// greedy mode `--addr` will be picked for the input `--addr xxx`.
        /// just the opposite, `--add` && `--r` will be split out.
        /// </summary>
        public static bool EnableCmdrGreedyLongFlag { get; set; } = true;

        /// <summary>
        /// incremental mode for greedy matching algorithm.
        ///
        /// In non-incremental mode, '-t271' can't be recognized as '-t=271'. It
        /// will be treated as a short option named as 't371', and an cannot match
        /// exception should be thrown.
        /// In incremental mode, '-t271' can be recognized as '-t=271', or '-t2=71'
        /// if the short option 't2' is existed.
        /// </summary>
        public static bool EnableCmdrGreedyIncrementalMode { get; set; } = true;


        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool EnableCmdrLogInfo { get; set; } = true;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool EnableCmdrLogTrace { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool EnableCmdrLogDebug { get; set; }


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        public static int match<T>(this T @this, ICommand command, string[] args, int position, int level)
            where T : IDefaultMatchers
        {
            @this.log?.logDebug("  - match for command: {CommandTitle}", command.backtraceTitles);

            var matchedPosition = -1;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                bool ok;
                var arg = args[i];
                if (string.IsNullOrWhiteSpace(arg)) continue;
                @this.log?.logDebug(string.Empty);
                @this.log?.logDebug("    -> arg {Index}: {Argument}", i, arg);
                var hiddenOpt = arg.StartsWith("~~");
                var isOpt = arg[0] == '-' || arg[0] == '/' || hiddenOpt;
                var longOpt = arg.StartsWith("--") || hiddenOpt;
                var preAte = 0;

                if (!isOpt)
                {
                    #region matching command

                    if (command.SubCommands != null && command.SubCommands.Count > 0)
                    {
                        var positionCopy = i + 1;
                        foreach (var cmd in command.SubCommands)
                        {
                            // ReSharper disable once RedundantArgumentDefaultValue
                            ok = cmd.Match(arg, false);
                            if (!ok) ok = cmd.Match(arg, true);
                            if (!ok) continue;

                            @this.log?.logDebug("    ++ command matched: {CommandTitle}", cmd.backtraceTitles);

                            @this.ParsedCommand = cmd;
                            @this.ParsedFlag = null;

                            positionCopy = i + 1;
                            if (cmd.SubCommands.Count > 0)
                            {
                                if (i == arg.Length - 1) throw new WantHelpScreenException();

                                var pos1 = match(@this, cmd, args, i + 1, level + 1);
                                if (pos1 < 0)
                                    matchedPosition = positionCopy;
                                else
                                    i = pos1;
                                positionCopy = pos1;
                            }

                            // onCommandMatched(args, i + 1, arg, cmd);

                            if (matchedPosition < 0 || positionCopy > 0)
                                matchedPosition = positionCopy;
                            command = cmd;
                            break;
                        }

                        if (matchedPosition < 0)
                        {
                            @this.log?.logDebug("level {Level} (cmd can't matched): returning {Position}", level,
                                -position - 1);
                            onCommandCannotMatched(@this, args, i, arg, command);
                            return -position - 1;
                        }

                        if (positionCopy < 0 && matchedPosition > 0)
                            return positionCopy;
                    }
                    else
                    {
                        // treat as remains args normally

                        // @this.log?.logDebug("level {Level} (no sub-cmds): returning {Position}", level, matchedPosition);
                        // onCommandCannotMatched(@this, args, i, arg, command);
                        return matchedPosition;
                    }

                    #endregion

                    continue;
                }

                #region matching for flags

                var ccc = command;
                var fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
                var siz = fragment.Length;

                int incLen, incPos, pos, len;
                if (longOpt)
                {
                    pos = 0;
                    len = siz;
                    incLen = 1; // unused
                    incPos = 1; // unused
                }
                else
                {
                    pos = 0;
                    if (EnableCmdrGreedyLongFlag)
                    {
                        len = siz;
                        if (EnableCmdrGreedyIncrementalMode)
                        {
                            incLen = -1;
                            incPos = 0;
                        }
                        else
                        {
                            incLen = -1;
                            incPos = 1;
                        }
                    }
                    else
                    {
                        len = 1;
                        incLen = 1;
                        incPos = 0;
                    }
                }

                forEachFragmentParts:

                #region forEachFragmentParts

                var partFull = fragment.Substring(pos, len);
                var part = partFull.EatEnd("+", "-");
                var bsw = partFull.Length != part.Length ? 1 : 0;
                if (partFull.Length == 0 && longOpt)
                {
                    // "--"
                    matchedPosition = i + 1;
                    break;
                }

                @this.log?.logDebug("    - try finding flag part {part} for `ccc`: {CommandTitle}",
                    part, ccc.backtraceTitles);

                backtraceAllParentFlags:

                var decidedLen = 0;
                // ReSharper disable once NotAccessedVariable
                IFlag? decidedFlg = null, matchedFlag = null;
                object? value = null, oldValue = null;

                #region backtraceAllParentFlags

                // ok = false;
                foreach (var flg in ccc.Flags)
                {
                    ok = flg.Match(ref part, part, pos, longOpt, true,
                        EnableCmdrGreedyLongFlag, EnableCmdrGreedyIncrementalMode);
                    if (!ok) continue;

                    // a flag matched ok, try extracting its value from commandline arguments
                    int atePos, ateArgs;
                    (atePos, ateArgs, value, oldValue) = tryExtractingValue(@this, flg, args, i, fragment, part, pos,
                        !longOpt && EnableCmdrGreedyLongFlag);
                    preAte += ateArgs;
                    pos += atePos;

                    @this.log?.logDebug("    ++ flag matched: {SW:l}{Part:l} = {oldVal} -> {value}. ate: [{pos},{args}]",
                        Util.SwitchChar(longOpt), part, oldValue, value,
                        atePos, ateArgs);

                    matchedFlag = flg;
                    if (len > decidedLen)
                    {
                        decidedFlg = flg;
                        decidedLen = part.Length;
                    }

                    break;
                }

                if (matchedFlag == null)
                {
                    if (ccc.Owner != null && !ReferenceEquals(ccc,ccc.Owner))
                    {
                        ccc = ccc.Owner;
                        @this.log?.logDebug("    - try finding flag part {part} for `ccc`'s parent: {CommandTitle}",
                            part, ccc.backtraceTitles);
                        goto backtraceAllParentFlags;
                    }

                    @this.log?.logDebug("can't match a flag: {Argument}/part={Part}/fragment={Fragment}.", 
                        arg, part, fragment);
                    onFlagCannotMatched(@this, args, i, part, longOpt, command);
                    // decidedLen = 1;
                }
                else
                {
                    @this.ParsedFlag = matchedFlag;
                    onFlagMatched(@this, args, i + 1, part, longOpt, matchedFlag, oldValue, value);
                    matchedPosition = i + 1;
                }

                if (pos + part.Length + bsw < siz && !longOpt)
                {
                    if (matchedFlag == null)
                    {
                        len += incLen;
                        pos += incPos;
                    }
                    else
                    {
                        siz -= part.Length;
                        if (EnableCmdrGreedyLongFlag)
                        {
                            len = siz;
                            if (EnableCmdrGreedyIncrementalMode)
                                pos += part.Length;
                            else
                                pos = 0;
                        }
                        else
                        {
                            pos += part.Length;
                            len = 1;
                        }
                    }

                    @this.log?.logDebug("    - for next part: {Part}, greedy={greedy}, pos={pos}, len={len}, siz={siz}",
                        fragment.Substring(pos, len),
                        EnableCmdrGreedyLongFlag, pos, len, siz);
                    ccc = command;
                    if (len > 0 && pos <= siz - bsw)
                        goto forEachFragmentParts;
                }

                #endregion

                #endregion

                if (preAte > 0)
                    i += preAte;

                #endregion
            }

            // ReSharper disable once InvertIf
            if (matchedPosition < 0)
            {
                @this.log?.logDebug("level {Level}: returning {Position}", level, -position - 1);
                return -position - 1;
            }

            return matchedPosition;
        }


        #region for match() - tryExtractingValue

        // ReSharper disable once SuggestBaseTypeForParameter
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static (int atePos, int ateArgs, object? value, object? old) tryExtractingValue<T>(
            this T @this, IFlag flg,
            string[] args, int i, string fragment,
            string part, int pos, bool lookAhead)
            where T : IDefaultMatchers
        {
            int atePos, ateArgs;
            object? val, old = null;

            var remains = fragment.Substring(pos + part.Length);
            bool? flipChar = null;
            if ((!lookAhead && remains.Length > 0) || (lookAhead && remains.Length == 1))
            {
                flipChar = remains[0] switch
                {
                    '-' => false,
                    '+' => true,
                    _ => null
                };
            }

            var dv = flg.getDefaultValue();

            if (dv is bool)
            {
                val = flipChar ?? true;
                old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), val);
                return (0, 0, val, old);
            }

            (atePos, ateArgs, val) = valFrom(args, i, remains, lookAhead && !EnableCmdrGreedyIncrementalMode);

            // ReSharper disable once InvertIf
            if (val is string v)
            {
                string[] sv;
                switch (dv)
                {
                    case string _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), val);
                        break;
                    case string[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), sv);
                        val = sv;
                        break;

                    case int[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var aiv = sv.Select(int.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), aiv);
                        val = aiv;
                        break;
                    case uint[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var uaiv = sv.Select(uint.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), uaiv);
                        val = uaiv;

                        break;
                    case long[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var alv = sv.Select(long.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), alv);
                        val = alv;
                        break;
                    case ulong[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        // ReSharper disable once IdentifierTypo
                        var ualv = sv.Select(ulong.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), ualv);
                        val = ualv;
                        break;
                    case short[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        var asv = sv.Select(short.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), asv);
                        val = asv;
                        break;
                    case ushort[] _:
                        sv = v.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        // ReSharper disable once IdentifierTypo
                        var uasv = sv.Select(ushort.Parse);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), uasv);
                        val = uasv;
                        break;

                    // more built-in types

                    case char _:
                        if (!string.IsNullOrEmpty(v))
                        {
                            old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), v[0]);
                            val = v[0];
                        }

                        break;

                    case float _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), float.Parse(v));
                        // val=float.Parse(v);
                        break;
                    case double _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), double.Parse(v));
                        break;
                    case decimal _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), decimal.Parse(v));
                        break;

                    // time

                    case TimeSpan _:
                        var tsVal = flg.UseMomentTimeFormat
                            ? new TimeSpan().FromMoment(v)
                            : TimeSpanEx.Parse(v);
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), tsVal);
                        break;
                    case DateTime _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), DateTimeEx.Parse(v));
                        break;
                    case DateTimeOffset _:
                        old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), DateTimeOffsetEx.Parse(v));
                        break;

                    // fallback converter

                    default:
                        if (dv != null)
                        {
                            @this.log?.logDebug("unacceptable default value ({dv}) datatype: {type}", dv, dv.GetType());
                            val = Convert.ChangeType(v, dv.GetType()); // typeof(int)
                            old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), val);
                        }

                        break;
                }
            }

            return (atePos, ateArgs, val, old);
        }

        private static (int atePos, int ateArgs, object? val) valFrom(IReadOnlyList<string> args, int i,
            string remains, bool lookAhead)
        {
            int ate = 1, pos = 0;
            object? val;
            if (remains.Length > 0 && !lookAhead)
            {
                // const string sep = "=";
                pos = remains.Length;
                ate = 0;
                val = remains.EatStart("=", ":").Trim('\'', '"');
            }
            else
                val = args[i + ate];

            return (pos, ate, val);
        }

        private static void setValueRecursive(this object obj, string propertyName, object value,
            bool recursive = false)
        {
            var t = obj.GetType();
            foreach (var propInfo in t.GetProperties())
            {
                if (propInfo.CanWrite && propInfo.Name == propertyName)
                {
                    if (propInfo.PropertyType.IsClass && recursive)
                    {
#pragma warning disable CS8604
                        var propVal = propInfo.GetValue(obj, null);
                        setValueRecursive(propVal, propertyName, value, true);
#pragma warning restore CS8604
                    }

                    propInfo.SetValue(obj, value, null);
                }
            }
        }

        #endregion


        #region helpers for match() - onMatchedXXX

        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="args"></param>
        /// <param name="position"></param>
        /// <param name="arg"></param>
        /// <param name="cmd"></param>
        /// <returns>false means no action triggered.</returns>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once UnusedParameter.Local
        internal static bool onCommandMatched<T>(this T @this, IEnumerable<string> args,
            int position, string arg, ICommand cmd)
            where T : IDefaultMatchers
        {
            checkRequiredFlagsReady(@this, cmd);

            if (cmd is BaseCommand c)
            {
                c.HitTitle = arg;
            }

            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            var root = cmd.FindRoot();
            if (root?.PreAction != null && !root.PreAction.Invoke(@this, cmd, remainArgs))
                throw new ShouldBeStopException();
            if (!ReferenceEquals(root, cmd) && cmd.PreAction != null && !cmd.PreAction.Invoke(@this, cmd, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                @this.log?.logDebug("---> matched command: {cmd}, remains: {Args}", cmd, string.Join(",", remainArgs));

                if (cmd is IAction action)
                    action.Invoke(@this, remainArgs);
                else if (cmd.Action != null)
                    cmd.Action.Invoke(@this, cmd, remainArgs);
                else
                    return false;
                return true;
            }
            finally
            {
                if (!ReferenceEquals(root, cmd)) cmd.PostAction?.Invoke(@this, cmd, remainArgs);
                root?.PostAction?.Invoke(@this, cmd, remainArgs);
            }

            // throw new NotImplementedException();
        }

        // ReSharper disable once UnusedParameter.Local
        private static void checkRequiredFlagsReady<T>(T @this, ICommand cmd) where T : IDefaultMatchers
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            ICommand? o = cmd;
            while (o?.IsRoot == false)
            {
                // var ready = true;
                foreach (var f in o.RequiredFlags)
                {
                    if (f.HitCount < 1)
                        throw new MissedRequiredFlagException(f);
                }

                o = o?.Owner;
            }
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private static void onFlagMatched<T>(this T @this, IEnumerable<string> args, int position, string fragment,
            in bool longOpt, IFlag flag, object? oldValue, object? value)
            where T : IDefaultMatchers
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            if (flag.PreAction != null && !flag.PreAction.Invoke(@this, flag, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                // ReSharper disable once UnusedVariable
                var sw = Util.SwitchChar(longOpt);
                @this.log?.logDebug("  ---> flag matched: {SW:l}{Fragment:l}", sw, fragment);
                // if (flag is BaseFlag<bool> f)
                {
                    flag.setValueRecursive("HitCount", flag.HitCount + 1);
                    flag.setValueRecursive("HitTitle", fragment);
                }

                if (value?.GetType().IsArray == true)
                    value = Cmdr.Instance.Store.Get(flag.ToDottedKey());

                if (flag.OnSet != null)
                    flag.OnSet?.Invoke(@this, flag, oldValue, value);
                else
                    defaultOnSet?.Invoke(@this, flag, oldValue, value);

                flag.Owner?.FindRoot()?.OnSet?.Invoke(@this, flag, oldValue, value);

                // ReSharper disable once SuspiciousTypeConversion.Global
                // ReSharper disable once UseNegatedPatternMatching
                var action = flag as IAction;
                if (action == null)
                    flag.Action?.Invoke(@this, flag, remainArgs);
                else
                    action.Invoke(@this, remainArgs);
            }
            finally
            {
                flag.PostAction?.Invoke(@this, flag, remainArgs);
            }
        }

        /// <summary>
        /// <code>bool OnCommandCannotMatched(ICommand parsedCommand, string matchingArg)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Func<ICommand, string, bool>? OnCommandCannotMatched { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static Func<ICommand, string, bool, string, bool /*processed*/>? OnFlagCannotMatched { get; set; }

        /// <summary>
        /// <code>bool OnCommandCannotMatched(ICommand parsingCommand,
        ///     string fragment, bool isShort, string matchingArg)</code>
        /// returning true means the event has been processed.
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once SuggestBaseTypeForParameter
        private static void onCommandCannotMatched<T>(this T @this, string[] args, in int position, string arg,
            ICommand cmd)
            where T : IDefaultMatchers
        {
            if (OnCommandCannotMatched?.Invoke(cmd, args[position]) == true)
                return;
            // throw new NotImplementedException();
            errPrint($"- Unknown command(arg): '{args[position]}'. context: '{cmd.backtraceTitles}'.");
            @this.suggestCommands(args, position, arg, cmd);
            if (@this.EnableUnknownCommandThrows)
                throw new UnknownCommandException(false, arg, cmd);
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once UnusedParameter.Local
        private static void onFlagCannotMatched<T>(this T @this, string[] args, in int position, string fragment,
            in bool longOpt, ICommand cmd)
            where T : IDefaultMatchers
        {
            if (OnFlagCannotMatched?.Invoke(cmd, fragment, !longOpt, args[position]) == true)
                return;
            var sw = Util.SwitchChar(longOpt);
            errPrint($"- Unknown flag({sw}{fragment}): '{args[position]}'. context: '{cmd.backtraceTitles}'");
            @this.suggestFlags(args, position, fragment, longOpt, cmd);
            if (@this.EnableUnknownFlagThrows)
                throw new UnknownFlagException(!longOpt, fragment, cmd);
        }

        #endregion


        #region suggestions

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestCommands<T>(this T @this, string[] args, in int position, string tag, ICommand cmd)
            where T : IDefaultMatchers
        {
            var xref = @this.xrefs[cmd];
            @this.suggestFor(tag, xref.SubCommandsLongNames, OptionType.Long);
            @this.suggestFor(tag, xref.SubCommandsAliasNames, OptionType.Aliases);
            @this.suggestFor(tag, xref.SubCommandsShortNames, OptionType.Short);
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFlags<T>(this T @this, string[] args, in int position, string fragment,
            in bool longOpt, ICommand cmd)
            where T : IDefaultMatchers
        {
            do
            {
                try
                {
                    var xref = @this.xrefs[cmd];
                    if (longOpt)
                    {
                        @this.suggestFor(fragment, xref.FlagsLongNames, OptionType.Long);
                        @this.suggestFor(fragment, xref.FlagsAliasNames, OptionType.Aliases);
                    }
                    else
                    {
                        @this.suggestFor(fragment, xref.FlagsShortNames, OptionType.Short);
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    throw new CmdrException(
                        $"Unexpected case: suggesting for '{fragment}' (position {position} and '{args[position]}')", ex);
                }

                if (!cmd.IsRoot)
                    cmd = cmd.Owner!;
            } while (!cmd.IsRoot);
        }

        /// <summary>
        /// <code>bool OnSuggestingForCommand(object worker,
        ///     Dictionary&lt;string, ICommand&gt; dataset, string token)</code>
        /// </summary>
        public static Func<object, Dictionary<string, ICommand>, string, bool>? OnSuggestingForCommand { get; set; }

        /// <summary>
        /// <code>bool OnSuggestingForFlag(object worker,
        ///     Dictionary&lt;string, IFlag&gt; dataset, string token)</code>
        /// </summary>
        public static Func<object, Dictionary<string, IFlag>, string, bool>? OnSuggestingForFlag { get; set; }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFor<T>(this T @this, string token, Dictionary<string, ICommand> dataset,
            OptionType ot)
            where T : IDefaultMatchers
        {
            if (OnSuggestingForCommand?.Invoke(@this, dataset, token) == true) return;

            // var sw = Util.SwitchChar(ot != OptionType.Short);
            foreach (var (key, opt) in dataset)
            {
                var rate = JaroWinkler.RateSimilarity(token, key);
                if (rate > 0.73)
                    errPrint($"  - maybe \"{key}\" ?\n    (Option \"{opt}\")");
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFor<T>(this T @this, string token, Dictionary<string, IFlag> dataset, OptionType ot)
            where T : IDefaultMatchers
        {
            if (OnSuggestingForFlag?.Invoke(@this, dataset, token) == true) return;

            var sw = Util.SwitchChar(ot != OptionType.Short);
            foreach (var (key, opt) in dataset)
            {
                var rate = JaroWinkler.RateSimilarity(token, key);
                // ReSharper disable once InvertIf
                if (rate > 0.73)
                    errPrint($"  - Maybe \"{sw}{key}\" ?\n    (Option \"{opt}\")");
            }
        }

        #endregion


        #region debug helpers: errPrint

        // ReSharper disable once InconsistentNaming
        private static void errPrint(string message)
        {
            _errorWriter.WriteLineAsync(message);
        }

        private static readonly StringWriter _errorWriter = new StringWriter();

        public static void FlushErrors<T>(this T @this) where T : IDefaultMatchers
        {
            var str = _errorWriter.ToString();
            if (!string.IsNullOrWhiteSpace(str))
            {
                var line = Environment.CommandLine;
                var exe = System.Reflection.Assembly.GetEntryAssembly()?.Location;
                var exeDir = Path.GetDirectoryName(exe) ?? Path.Join(Environment.CurrentDirectory, "1");
                if (line.StartsWith(exeDir))
                    line = "<appdir>" + line.Substring(exeDir.Length);

                Console.Error.WriteLineAsync($"\nFor the input '{line}':\n");
                Console.Error.WriteLineAsync(str);
            }

            _errorWriter.Dispose();
        }

        #endregion


        #region defaultOnSet

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static Action<IBaseWorker, IBaseOpt, object?, object?>? defaultOnSet = (w, flg, oldVal, newVal) =>
        {
            if (EnableCmdrLogDebug)
                Console.WriteLine($"--> onSet: {flg} changed ({oldVal} -> {newVal})");
        };

        #endregion
    }
}