using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
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
            @this.logDebug("  - match for command: {CommandTitle}", command.backtraceTitles);

            var matchedPosition = -1;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                bool ok;
                var arg = args[i];
                var hiddenOpt = arg.StartsWith("~~");
                var isOpt = arg[0] == '-' || arg[0] == '/' || hiddenOpt;
                var longOpt = arg.StartsWith("--") || hiddenOpt;

                @this.logDebug("    -> arg {Index}: {Argument}", i, arg);
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

                            @this.logDebug("    ++ command matched: {CommandTitle}", cmd.backtraceTitles);

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
                            @this.logDebug("level {Level} (cmd can't matched): returning {Position}", level,
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

                        // @this.logDebug("level {Level} (no sub-cmds): returning {Position}", level, matchedPosition);
                        // onCommandCannotMatched(@this, args, i, arg, command);
                        return matchedPosition;
                    }

                    #endregion

                    continue;
                }

                #region matching for flags of 'command'

                var ccc = command;
                var fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
                var ate = 0;
                var siz = fragment.Length;

                int incLen, incPos, pos, len;
                if (longOpt)
                {
                    pos = 0;
                    len = siz;
                    incLen = 1; // unuse
                    incPos = 1; // unuse
                }
                else
                {
                    pos = 0;
                    if (EnableCmdrGreedyLongFlag)
                    {
                        len = siz;
                        incLen = -1;
                        incPos = 1;
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

                var part = fragment.Substring(pos, len).EatEnd("+", "-");
                if (part.Length == 0 && longOpt)
                {
                    // "--"
                    matchedPosition = i + 1;
                    break;
                }

                @this.logDebug("    - try finding flag part {part} for `ccc`: {CommandTitle}", part,
                    ccc.backtraceTitles);

                backtraceAllParentFlags:

                var decidedLen = 0;
                IFlag? decidedFlg = null, matchedFlag = null;
                object? value = null, oldValue = null;

                #region backtraceAllParentFlags

                // ok = false;
                foreach (var flg in ccc.Flags)
                {
                    ok = flg.Match(ref part, part, pos, longOpt, true, EnableCmdrGreedyLongFlag);
                    if (!ok) continue;

                    // a flag matched ok, try extracting its value from commandline arguments
                    (ate, value, oldValue) =
                        tryExtractingValue(@this, flg, args, i, fragment, part, pos, !longOpt && incLen < 0);

                    @this.logDebug("    ++ flag matched: {SW:l}{Part:l} = {value}",
                        Util.SwitchChar(longOpt), part, value);

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
                    if (ccc.Owner != null && ccc.Owner != ccc)
                    {
                        ccc = ccc.Owner;
                        @this.logDebug("    - try finding flag part {part} for `ccc`'s parent: {CommandTitle}",
                            part, ccc.backtraceTitles);
                        goto backtraceAllParentFlags;
                    }

                    @this.logDebug("can't match a flag: {Argument}/part={Part}/fragment={Fragment}.", arg, part,
                        fragment);
                    onFlagCannotMatched(@this, args, i, part, longOpt, command);
                    // decidedLen = 1;
                }
                else
                {
                    @this.ParsedFlag = matchedFlag;
                    onFlagMatched(@this, args, i + 1, part, longOpt, matchedFlag, oldValue, value);
                    matchedPosition = i + 1;
                }

                if (pos + part.Length < siz && !longOpt)
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
                            pos = 0;
                            len = siz;
                        }
                        else
                        {
                            pos += part.Length;
                            len = 1;
                        }
                    }

                    @this.logDebug("    - for next part: {Part}, greedy={greedy}, pos={pos}, len={len}, siz={siz}",
                        fragment.Substring(pos, len),
                        EnableCmdrGreedyLongFlag, pos, len, siz);
                    ccc = command;
                    if (len > 0 && pos < siz)
                        goto forEachFragmentParts;
                }

                #endregion

                #endregion

                if (ate > 0)
                    i += ate;

                #endregion
            }

            // ReSharper disable once InvertIf
            if (matchedPosition < 0)
            {
                @this.logDebug("level {Level}: returning {Position}", level, -position - 1);
                return -position - 1;
            }

            return matchedPosition;
        }


        #region for match() - tryExtractingValue

        // ReSharper disable once SuggestBaseTypeForParameter
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static (int ate, object? value, object? old) tryExtractingValue<T>(
            this T @this, IFlag flg,
            string[] args, int i, string fragment,
            string part, int pos, bool forward)
            where T : IDefaultMatchers
        {
            var ate = 0;
            object? val, old = null;

            var remains = fragment.Substring(pos + part.Length);
            bool? flipChar = null;
            if ((!forward && remains.Length > 0) || (forward && remains.Length == 1))
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
                return (ate, val, old);
            }

            (ate, val) = valFrom(args, i, remains, forward);

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
                            @this.logDebug("unacceptable default value ({dv}) datatype: {type}", dv, dv.GetType());
                            val = Convert.ChangeType(v, dv.GetType()); // typeof(int)
                            old = Cmdr.Instance.Store.Set(flg.ToDottedKey(), val);
                        }

                        break;
                }
            }

            return (ate, val, old);
        }

        private static (int ate, object? val) valFrom(IReadOnlyList<string> args, int i, string remains, bool forward)
        {
            var ate = 1;
            object? val;
            if (remains.Length > 0 && !forward)
            {
                // const string sep = "=";
                remains = remains.EatStart("=", ":").Trim('\'', '"');
                val = remains;
                ate = 0;
            }
            else
                val = args[i + ate];

            return (ate, val);
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
            if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(@this, cmd, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                @this.logDebug("---> matched command: {cmd}, remains: {Args}", cmd, string.Join(",", remainArgs));

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
                if (root != cmd) cmd.PostAction?.Invoke(@this, cmd, remainArgs);
                root?.PostAction?.Invoke(@this, cmd, remainArgs);
            }

            // throw new NotImplementedException();
        }

        // ReSharper disable once UnusedParameter.Local
        private static void checkRequiredFlagsReady<T>(T @this, ICommand cmd) where T : IDefaultMatchers
        {
            // ReSharper disable once SuggestVarOrType_SimpleTypes
            ICommand? o = cmd;
            while (o?.Owner != null && o.Owner != o)
            {
                // var ready = true;
                foreach (var f in (o?.RequiredFlags).Where(f =>
                    // f.getDefaultValue() != null && 
                    !Cmdr.Instance.Store.HasKeys(f.ToKeys())))
                {
                    // ready = false;
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
                @this.logDebug("  ---> flag matched: {SW:l}{Fragment:l}", sw, fragment);
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
            @this.suggestFor(tag, xref.SubCommandsLongNames);
            @this.suggestFor(tag, xref.SubCommandsAliasNames);
            @this.suggestFor(tag, xref.SubCommandsShortNames);
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
                        @this.suggestFor(fragment, longOpt, xref.FlagsLongNames);
                        @this.suggestFor(fragment, longOpt, xref.FlagsAliasNames);
                    }
                    else
                    {
                        @this.suggestFor(fragment, longOpt, xref.FlagsShortNames);
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    throw new CmdrException(
                        $"Unexpect case: suggesting for '{fragment}' (position {position} and '{args[position]}')", ex);
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
        private static void suggestFor<T>(this T @this, string token, Dictionary<string, ICommand> dataset)
            where T : IDefaultMatchers
        {
            if (OnSuggestingForCommand?.Invoke(@this, dataset, token) == true) return;

            foreach (var (key, opt) in dataset)
            {
                var rate = JaroWinkler.RateSimilarity(token, key);
                if (rate > 0.73)
                    errPrint($"  - maybe \"{opt.Long}\""); // under \"{opt.Owner?.backtraceTitles}\"");
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFor<T>(this T @this, string token, bool longOpt, Dictionary<string, IFlag> dataset)
            where T : IDefaultMatchers
        {
            if (OnSuggestingForFlag?.Invoke(@this, dataset, token) == true) return;

            foreach (var (key, opt) in dataset)
            {
                var rate = JaroWinkler.RateSimilarity(token, key);
                // ReSharper disable once InvertIf
                if (rate > 0.73)
                {
                    var sw = Util.SwitchChar(longOpt);
                    errPrint($"  - Maybe \"{sw}{opt.Long}\"?"); // (under \"{opt.Owner?.backtraceTitles}\")");
                }
            }
        }

        #endregion


        #region debug helpers: errPrint

        // ReSharper disable once InconsistentNaming
        private static void errPrint(string message)
        {
            _errorWriter.WriteLineAsync(message);
        }

        static StringWriter _errorWriter = new StringWriter();

        public static void FlushErrors<T>(this T @this) where T : IDefaultMatchers
        {
            Console.Error.WriteLineAsync(_errorWriter.ToString());
        }

        #endregion

        #region debug helpers: logInfo

        public static void logInfo<T>(this T @this, string messageTemplate)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    // logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileName",
                    // new ScalarValue(stack.GetFileName())));
                    .Information(messageTemplate);
            }
        }

        public static void logInfo<T, T0>(this T @this, string messageTemplate,
            T0 property0)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0);
            }
        }

        public static void logInfo<T, T0, T1>(this T @this, string messageTemplate,
            T0 property0, T1 property1)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0, property1);
            }
        }

        public static void logInfo<T, T0, T1, T2>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1).Information(messageTemplate, property0, property1, property2);
            }
        }

        public static void logInfo<T, T0, T1, T2, T3>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogInfo)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Information(messageTemplate, property0, property1, property2, property3);
            }
        }

        #endregion

        #region debug helpers: logDebug

        public static void logDebug<T>(this T @this, string messageTemplate)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1)
                    // logEvent.AddPropertyIfAbsent(new LogEventProperty("SourceFileName",
                    // new ScalarValue(stack.GetFileName())));
                    .Debug(messageTemplate);
            }
        }

        public static void logDebug<T, T0>(this T @this, string messageTemplate,
            T0 property0)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1).Debug(messageTemplate, property0);
            }
        }

        public static void logDebug<T, T0, T1>(this T @this, string messageTemplate,
            T0 property0, T1 property1)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1).Debug(messageTemplate, property0, property1);
            }
        }

        public static void logDebug<T, T0, T1, T2>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1).Debug(messageTemplate, property0, property1, property2);
            }
        }

        public static void logDebug<T, T0, T1, T2, T3>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1).Debug(messageTemplate, property0, property1, property2, property3);
            }
        }

        public static void logDebug<T, T0, T1, T2, T3, T4>(this T @this, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3, T4 property4)
            where T : IDefaultMatchers
        {
            if (EnableCmdrLogTrace)
            {
                @this.log.ForContext("SKIP_", 1)
                    .Debug(messageTemplate, property0, property1, property2, property3, property4);
            }
        }

        #endregion

        #region debug helpers: logWarning

        public static void logWarning<T>(this T @this, System.Exception exception, string messageTemplate)
            where T : IDefaultMatchers
        {
            @this.log.Warning(exception, messageTemplate);
        }

        public static void logWarning<T, T0>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0)
            where T : IDefaultMatchers
        {
            @this.log.Warning(exception, messageTemplate, property0);
        }

        public static void logWarning<T, T0, T1>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1)
            where T : IDefaultMatchers
        {
            @this.log.Warning(exception, messageTemplate, property0, property1);
        }

        public static void logWarning<T, T0, T1, T2>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : IDefaultMatchers
        {
            @this.log.Warning(exception, messageTemplate, property0, property1, property2);
        }

        public static void logWarning<T, T0, T1, T2, T3>(this T @this, System.Exception exception,
            string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : IDefaultMatchers
        {
            @this.log.Warning(exception, messageTemplate, property0, property1, property2, property3);
        }

        #endregion

        #region debug helpers: logError

        public static void logError<T>(this T @this, System.Exception exception, string messageTemplate)
            where T : IDefaultMatchers
        {
            @this.log.Error(exception, messageTemplate);
        }

        public static void logError<T, T0>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0)
            where T : IDefaultMatchers
        {
            @this.log.Error(exception, messageTemplate, property0);
        }

        public static void logError<T, T0, T1>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1)
            where T : IDefaultMatchers
        {
            @this.log.Error(exception, messageTemplate, property0, property1);
        }

        public static void logError<T, T0, T1, T2>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2)
            where T : IDefaultMatchers
        {
            @this.log.Error(exception, messageTemplate, property0, property1, property2);
        }

        public static void logError<T, T0, T1, T2, T3>(this T @this, System.Exception exception, string messageTemplate,
            T0 property0, T1 property1, T2 property2, T3 property3)
            where T : IDefaultMatchers
        {
            @this.log.Error(exception, messageTemplate, property0, property1, property2, property3);
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

    class JaroWinkler
    {
        private const double DefaultMismatchScore = 0.0;
        private const double DefaultMatchScore = 1.0;

        /// <summary>
        /// Gets the similarity between two strings by using the Jaro-Winkler algorithm.
        /// A value of 1 means perfect match. A value of zero represents an absolute no match
        /// </summary>
        /// <param name="firstWord"></param>
        /// <param name="secondWord"></param>
        /// <returns>a value between 0-1 of the similarity</returns>
        /// 
        public static double RateSimilarity(string? firstWord, string? secondWord)
        {
            if (firstWord == null || secondWord == null) return DefaultMismatchScore;

            // Converting to lower case is not part of the original Jaro-Winkler implementation
            // But we don't really care about case sensitivity in DIAMOND and wouldn't decrease security names similarity rate just because
            // of Case sensitivity
            firstWord = firstWord.ToLower();
            secondWord = secondWord.ToLower();

            if (firstWord == secondWord)
                //return (SqlDouble)defaultMatchScore;
                return DefaultMatchScore;


            {
                // Get half the length of the string rounded up - (this is the distance used for acceptable transpositions)
                var halfLength = Math.Min(firstWord.Length, secondWord.Length) / 2 + 1;

                // Get common characters
                var common1 = GetCommonCharacters(firstWord, secondWord, halfLength);
                var commonMatches = common1?.Length ?? 0;

                // Check for zero in common
                if (commonMatches == 0)
                    //return (SqlDouble)defaultMismatchScore;
                    return DefaultMismatchScore;

                var common2 = GetCommonCharacters(secondWord, firstWord, halfLength);

                // Check for same length common strings returning 0 if is not the same
                if (commonMatches != common2?.Length)
                    //return (SqlDouble)defaultMismatchScore;
                    return DefaultMismatchScore;

                // Get the number of transpositions
                int transpositions = 0;
                for (int i = 0; i < commonMatches; i++)
                {
                    if (common1 != null && common1[i] != common2[i])
                        transpositions++;
                }

                int j = 0;
                j += 1;

                // Calculate Jaro metric
                transpositions /= 2;
                double jaroMetric = commonMatches / (3.0 * firstWord.Length) +
                                    commonMatches / (3.0 * secondWord.Length) +
                                    (commonMatches - transpositions) / (3.0 * commonMatches);
                //return (SqlDouble)jaroMetric;
                return jaroMetric;
            }

            //return (SqlDouble)defaultMismatchScore;
        }

        /// <summary>
        /// Returns a string buffer of characters from string1 within string2 if they are of a given
        /// distance seperation from the position in string1.
        /// </summary>
        /// <param name="firstWord">string one</param>
        /// <param name="secondWord">string two</param>
        /// <param name="separationDistance">separation distance</param>
        /// <returns>A string buffer of characters from string1 within string2 if they are of a given
        /// distance seperation from the position in string1</returns>
        private static StringBuilder? GetCommonCharacters(string firstWord, string secondWord, int separationDistance)
        {
            if ((firstWord != null) && (secondWord != null))
            {
                var returnCommons = new StringBuilder(20);
                var copy = new StringBuilder(secondWord);
                var firstWordLength = firstWord.Length;
                var secondWordLength = secondWord.Length;

                for (int i = 0; i < firstWordLength; i++)
                {
                    char character = firstWord[i];
                    bool found = false;

                    for (int j = Math.Max(0, i - separationDistance);
                        !found && j < Math.Min(i + separationDistance, secondWordLength);
                        j++)
                    {
                        if (copy[j] == character)
                        {
                            found = true;
                            returnCommons.Append(character);
                            copy[j] = '#';
                        }
                    }
                }

                return returnCommons;
            }

            return null;
        }
    }
}