using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;
using HzNS.Cmdr.Tool;
using Serilog;

namespace HzNS.Cmdr.Internal
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ILoggable
    {
        ILogger log { get; }
    }

    public interface IBaseWorker : ILoggable, IWorkerFunctions
    {
        int ParsedCount { get; set; }

        bool Parsed { get; set; }

        ICommand? ParsedCommand { get; set; }
        IFlag? ParsedFlag { get; set; }

        bool EnableDuplicatedCharThrows { get; set; }
        bool EnableEmptyLongFieldThrows { get; set; }
        bool EnableUnknownCommandThrows { get; set; }
        bool EnableUnknownFlagThrows { get; set; }
        int TabStop { get; set; }


        // ReSharper disable once CollectionNeverUpdated.Local
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        Dictionary<ICommand, Xref> xrefs { get; }

        IRootCommand RootCommand { get; }
    }


    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    // ReSharper disable once InconsistentNaming
    public interface IDefaultMatchers : IBaseWorker
    {
    }


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class DefaultMatchers
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        public static int match<T>(this T @this, ICommand command, string[] args, int position, int level)
            where T : IDefaultMatchers
        {
            @this.log.Debug("  - match for command: {CommandTitle}", command.backtraceTitles);

            var matchedPosition = -1;
            // ReSharper disable once TooWideLocalVariableScope
            string fragment;
            // ReSharper disable once TooWideLocalVariableScope
            int pos, len, size;
            // ReSharper disable once TooWideLocalVariableScope
            int ate;
            // ReSharper disable once TooWideLocalVariableScope
            object? value;

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = position; i < args.Length; i++)
            {
                bool ok;
                var arg = args[i];
                var isOpt = arg.StartsWith("-");
                var longOpt = arg.StartsWith("--");

                @this.log.Debug("    -> arg {Index}: {Argument}", i, arg);
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

                            @this.log.Debug("    ++ command matched: {CommandTitle}", cmd.backtraceTitles);

                            @this.ParsedCommand = cmd;
                            @this.ParsedFlag = null;

                            positionCopy = i + 1;
                            if (cmd.SubCommands.Count > 0)
                            {
                                if (i == arg.Length - 1) throw new WantHelpScreenException();

                                var pos1 = match(@this, cmd, args, i + 1, level + 1);
                                if (pos1 < 0) matchedPosition = positionCopy;
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
                            @this.log.Debug("level {Level} (cmd can't matched): returning {Position}", level,
                                -position - 1);
                            onCommandCannotMatched(@this, args, i, arg, command);
                            return -position - 1;
                        }

                        if (positionCopy < 0 && matchedPosition > 0)
                            return positionCopy;
                    }
                    else
                    {
                        @this.log.Debug("level {Level} (no sub-cmds): returning {Position}", level, matchedPosition);
                        onCommandCannotMatched(@this, args, i, arg, command);
                        return matchedPosition;
                    }

                    #endregion

                    continue;
                }

                // matching for flags of 'command'

                var ccc = command;
                fragment = longOpt ? arg.Substring(2) : arg.Substring(1);
                pos = 0;
                len = 1;
                size = fragment.Length;
                ate = 0;

                forEachFragmentParts:
                var part = fragment.Substring(pos, len);

                @this.log.Debug("    - try finding flags for ccc: {CommandTitle}", ccc.backtraceTitles);

                backtraceAllParentFlags:
                ok = false;
                foreach (var flg in ccc.Flags)
                {
                    ok = flg.Match(ref part, fragment, pos, longOpt);
                    if (!ok) continue;

                    // a flag matched ok, try extracting its value from commandline arguments
                    (ate, value) = tryExtractingValue(@this, flg, args, i, part, pos);

                    @this.log.Debug("    ++ flag matched: {SW:l}{flgLong:l} {value}",
                        Util.SwitchChar(longOpt), flg.Long, value);

                    len = part.Length;
                    @this.ParsedFlag = flg;
                    onFlagMatched(@this, args, i + 1, part, longOpt, flg);
                    matchedPosition = i + 1;
                    break;
                }

                // ReSharper disable once InvertIf
                if (!ok)
                {
                    if (ccc.Owner != null && ccc.Owner != ccc)
                    {
                        ccc = ccc.Owner;
                        @this.log.Debug("    - try finding flags for its(ccc) parent: {CommandTitle}",
                            ccc.backtraceTitles);
                        goto backtraceAllParentFlags;
                    }

                    @this.log.Debug("can't match a flag: {Argument}/part={Part}/fragment={Fragment}.", arg, part,
                        fragment);
                    onFlagCannotMatched(@this, args, i, part, longOpt, command);
                }

                if (pos + len < size)
                {
                    pos += len;
                    len = 1;
                    @this.log.Debug("    - for next part: {Part}", fragment.Substring(pos, len));
                    ccc = command;
                    goto forEachFragmentParts;
                }

                if (ate > 0)
                {
                    i += ate;
                }

                // 
            }

            // ReSharper disable once InvertIf
            if (matchedPosition < 0)
            {
                @this.log.Debug("level {Level}: returning {Position}", level, -position - 1);
                return -position - 1;
            }

            return matchedPosition;
        }


        #region for match(), tryExtractingValue

        // ReSharper disable once SuggestBaseTypeForParameter
        internal static (int ate, object? value) tryExtractingValue<T>(this T @this, IFlag flg, string[] args, int i,
            string part, int pos)
            where T : IDefaultMatchers
        {
            var ate = 0;
            object? val = null;

            var remains = args[i].Substring(pos + part.Length);
            bool? flipChar = null;
            if (remains.Length > 0)
            {
                flipChar = remains[0] switch
                {
                    '-' => false,
                    '+' => true,
                    _ => null
                };
            }

            var dv = flg.getDefaultValue();
            switch (dv)
            {
                case bool _:
                    val = flipChar ?? true;
                    break;

                case string _:
                    ate = 1;
                    val = args[i + ate];
                    break;

                case string[] _:
                    val = true;
                    break;
            }

            return (ate, val);
        }

        #endregion

        #region helpers for Run() - match(ed)

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
        internal static bool onCommandMatched<T>(this T @this, IEnumerable<string> args, int position, string arg,
            ICommand cmd)
            where T : IDefaultMatchers
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            var root = cmd.FindRoot();
            if (root?.PreAction != null && !root.PreAction.Invoke(@this, cmd, remainArgs))
                throw new ShouldBeStopException();
            if (root != cmd && cmd.PreAction != null && !cmd.PreAction.Invoke(@this, cmd, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                @this.log.Debug("---> matched command: {cmd}, remains: {Args}", cmd, string.Join(",", remainArgs));

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

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private static void onFlagMatched<T>(this T @this, IEnumerable<string> args, int position, string fragment,
            in bool longOpt,
            IFlag flag)
            where T : IDefaultMatchers
        {
            var remainArgs = args.Where((it, idx) => idx >= position).ToArray();

            if (flag.PreAction != null && !flag.PreAction.Invoke(@this, flag, remainArgs))
                throw new ShouldBeStopException();

            try
            {
                // ReSharper disable once UnusedVariable
                var sw = Util.SwitchChar(longOpt);
                @this.log.Debug("  ---> flag matched: {SW:l}{Fragment:l}", sw, fragment);
                if (flag.OnSet != null)
                    flag.OnSet?.Invoke(@this, flag, flag.getDefaultValue(), flag.getDefaultValue());
                else
                    defaultOnSet?.Invoke(@this, flag, flag.getDefaultValue(), flag.getDefaultValue());

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

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static Action<IBaseWorker, IBaseOpt, object?, object?>? defaultOnSet = (w, flg, oldVal, newVal) =>
        {
            w.log.Debug("");
        };

        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once UnusedParameter.Local
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once SuggestBaseTypeForParameter
        private static void onCommandCannotMatched<T>(this T @this, string[] args, in int position, string arg,
            ICommand cmd)
            where T : IDefaultMatchers
        {
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
            var xref = @this.xrefs[cmd];
            if (longOpt)
            {
                @this.suggestFor(fragment, xref.FlagsLongNames);
                @this.suggestFor(fragment, xref.FlagsAliasNames);
            }
            else
            {
                @this.suggestFor(fragment, xref.FlagsShortNames);
            }
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFor<T>(this T @this, string tag, Dictionary<string, ICommand> dataset)
            where T : IDefaultMatchers
        {
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void suggestFor<T>(this T @this, string tag, Dictionary<string, IFlag> dataset)
            where T : IDefaultMatchers
        {
        }

        #endregion

        #region debug helpers

        // ReSharper disable once InconsistentNaming
        private static void errPrint(string message)
        {
            Console.Error.WriteLine(message);
        }

        #endregion
    }
}