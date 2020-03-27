using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Painter;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Ext;

namespace HzNS.Cmdr.Internal
{
    public interface IWorkerFunctions
    {
        void ShowVersionsScreen(IBaseWorker w, params string[] remainArgs);

        void ShowBuildInfoScreen(IBaseWorker w, params string[] remainArgs);

        void ShowHelpScreen(IBaseWorker w, params string[] remainArgs);

        void ShowTreeDumpScreenForAllCommands(IBaseWorker w, params string[] remainArgs);

        public bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null);
    }


    // 
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class WorkerFunctions : IWorkerFunctions
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public IPainter Painter { get; set; } = new DefaultPainter();

        
        // ReSharper disable once MemberCanBePrivate.Global
        public bool EnablePlaceHolderForShortAndAliasFlag { get; set; } = false;


        public abstract bool Walk(ICommand? parent = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null);


        #region ShowVersionsScreen

        public void ShowVersionsScreen(IBaseWorker w, params string[] remainArgs)
        {
            var command = w.ParsedCommand ?? w.RootCommand;
            tabStopCalculated = w.TabStop;

            Painter.Setup(command, w, remainArgs);
            if (w.AppQuietMode)
            {
                Painter.PrintVersions(command, tabStopCalculated, w, remainArgs);
                return;
            }
            
            Painter.PrintPrologue(command, w, remainArgs);
            Painter.PrintPreface(command, w, remainArgs);
            Painter.PrintHeadLines(command, w, true, remainArgs);

            // Painter.PrintUsages(command, w, remainArgs);
            // Painter.PrintExamples(command, w, remainArgs);

            Painter.PrintVersions(command, tabStopCalculated, w, remainArgs);

            // Painter.PrintTailLines(command, w, remainArgs);
            Painter.PrintEpilogue(command, w, remainArgs);
            // writer.WriteLine("");
        }

        #endregion

        #region ShowBuildInfoScreen

        public void ShowBuildInfoScreen(IBaseWorker w, params string[] remainArgs)
        {
            var command = w.ParsedCommand ?? w.RootCommand;
            tabStopCalculated = w.TabStop;

            Painter.Setup(command, w, remainArgs);
            if (w.AppQuietMode)
            {
                Painter.PrintBuildInfo(command, tabStopCalculated, w, remainArgs);
                return;
            }
            
            Painter.PrintPrologue(command, w, remainArgs);
            Painter.PrintPreface(command, w, remainArgs);
            Painter.PrintHeadLines(command, w, true, remainArgs);

            // Painter.PrintUsages(command, w, remainArgs);
            // Painter.PrintExamples(command, w, remainArgs);

            Painter.PrintBuildInfo(command, tabStopCalculated, w, remainArgs);

            // Painter.PrintTailLines(command, w, remainArgs);
            Painter.PrintEpilogue(command, w, remainArgs);
            // writer.WriteLine("");
        }

        #endregion

        #region ShowHelpScreen

        public void ShowHelpScreen(IBaseWorker w, params string[] remainArgs)
        {
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlagLines>();
            // var writer = ColorifyEnabler.Colorify; // Console.Out;
            var command = w.ParsedCommand ?? w.RootCommand;
            tabStopCalculated = w.TabStop;
            const bool noBacktrace = false;

            w.Walk(command,
                commandsWatcherBuilder(w, commandLines, tabStopCalculated, noBacktrace),
                flagsWatcherBuilder(w, optionLines, tabStopCalculated, noBacktrace));

            Painter.Setup(command, w, remainArgs);
            Painter.PrintPrologue(command, w, remainArgs);
            Painter.PrintPreface(command, w, remainArgs);
            Painter.PrintHeadLines(command, w, false, remainArgs);

            Painter.PrintUsages(command, w, remainArgs);
            Painter.PrintExamples(command, w, remainArgs);

            // ShowIt(command, commandLines, optionLines, writer, tabStop);
            Painter.PrintCommandsAndOptions(command, commandLines, optionLines,
                tabStopCalculated, false, w, remainArgs);

            // Painter.PrintTailLines(command, w, remainArgs);
            Painter.PrintEpilogue(command, w, remainArgs);
            // writer.WriteLine("");
        }

        #endregion

        #region ShowTreeDumpScreenForAllCommands

        public void ShowTreeDumpScreenForAllCommands(IBaseWorker w, params string[] remainArgs)
        {
            w.log.Debug("dump tree");

            // var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlagLines>();
            var command = w.ParsedCommand ?? w.RootCommand;
            tabStopCalculated = w.TabStop;

            w.Walk(command,
                (owner, cmd, level) =>
                {
                    if (cmd.Hidden) return true;

                    var sb = new StringBuilder("  ".Repeat(1 + level));
                    if (!string.IsNullOrWhiteSpace(cmd.Short)) sb.Append($"{cmd.Short}, ");
                    if (!string.IsNullOrWhiteSpace(cmd.Long)) sb.Append($"{cmd.Long},");
                    if (cmd.Aliases.Length > 0) sb.AppendJoin(',', cmd.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(cmd.Description)) sb2.Append(cmd.Description);
                    else if (!string.IsNullOrWhiteSpace(cmd.DescriptionLong)) sb2.Append(cmd.DescriptionLong);

                    if (!commandLines.ContainsKey(cmd.Group))
                        commandLines.TryAdd(cmd.Group, new List<TwoString>());

                    if (sb.Length >= tabStopCalculated) tabStopCalculated = sb.Length + 1;
                    commandLines[cmd.Group].Add(new TwoString
                        {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                (owner, flag, level) => true);

            Painter.Setup(command, w, remainArgs);
            Painter.PrintPrologue(command, w, remainArgs);
            Painter.PrintPreface(command, w, remainArgs);
            Painter.PrintHeadLines(command, w, false, remainArgs);

            // ShowIt(w.ParsedCommand ?? w.RootCommand, commandLines, optionLines, writer, tabStop, true);
            Painter.PrintCommandsAndOptions(command, commandLines, optionLines,
                tabStopCalculated, true, w, remainArgs);

            // Painter.PrintTailLines(command, w, remainArgs);
            Painter.PrintEpilogue(command, w, remainArgs);
            // writer.WriteLine("");
        }

        #endregion

        #region ShowEverything

        public void ShowEverything(Worker w, params string[] remainArgs)
        {
            // var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlagLines>();
            var command = w.ParsedCommand ?? w.RootCommand;
            tabStopCalculated = w.TabStop;

            w.Walk(command,
                (owner, cmd, level) =>
                {
                    if (cmd.Hidden) return true;

                    var sb = new StringBuilder("  ");
                    if (!string.IsNullOrWhiteSpace(cmd.Short)) sb.Append($"{cmd.Short}, ");
                    if (!string.IsNullOrWhiteSpace(cmd.Long)) sb.Append($"{cmd.Long},");
                    if (cmd.Aliases.Length > 0) sb.AppendJoin(',', cmd.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(cmd.Description)) sb2.Append(cmd.Description);
                    else if (!string.IsNullOrWhiteSpace(cmd.DescriptionLong)) sb2.Append(cmd.DescriptionLong);

                    if (!commandLines.ContainsKey(cmd.Group))
                        commandLines.TryAdd(cmd.Group, new List<TwoString>());

                    if (sb.Length >= tabStopCalculated) tabStopCalculated = sb.Length + 1;
                    commandLines[cmd.Group].Add(new TwoString
                        {Level = level, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                (owner, flag, level) =>
                {
                    if (flag.Hidden) return true;

                    var sb = new StringBuilder("  ");
                    if (!string.IsNullOrWhiteSpace(flag.Short)) sb.Append($"{flag.Short}, ");
                    if (!string.IsNullOrWhiteSpace(flag.Long)) sb.Append($"{flag.Long},");
                    if (flag.Aliases.Length > 0) sb.AppendJoin(',', flag.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(flag.Description)) sb2.Append(flag.Description);
                    else if (!string.IsNullOrWhiteSpace(flag.DescriptionLong)) sb2.Append(flag.DescriptionLong);

                    var lvl = UpperBoundLevel - owner.FindLevel();
                    if (!optionLines.ContainsKey(lvl))
                        optionLines.TryAdd(lvl,
                            new CmdFlagLines {cmd = owner, lines = new SortedDictionary<string, List<TwoString>>()});
                    if (!optionLines[lvl].lines.ContainsKey(flag.Group))
                        optionLines[lvl].lines.TryAdd(flag.Group, new List<TwoString>());

                    if (sb.Length >= tabStopCalculated) tabStopCalculated = sb.Length + 1;
                    optionLines[lvl].lines[flag.Group].Add(new TwoString
                        {Level = level, Flag = flag, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    // if (!optionLines.ContainsKey(level))
                    //     optionLines.TryAdd(level, new CmdFlags{cmd=owner,lines= new SortedDictionary<string, List<TwoString>>(),}};
                    // if (!optionLines[level].ContainsKey(flag.Group))
                    //     optionLines[level].TryAdd(flag.Group, new List<TwoString>());
                    //
                    // if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    // optionLines[level][flag.Group].Add(new TwoString
                    //     {Level = level, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                });

            Painter.Setup(command, w, remainArgs);
            Painter.PrintPrologue(command, w, remainArgs);
            Painter.PrintPreface(command, w, remainArgs);
            Painter.PrintHeadLines(command, w, false, remainArgs);

            // ShowIt(w.ParsedCommand ?? w.RootCommand, commandLines, optionLines, writer, tabStop);
            // // ShowIt(command, commandLines, optionLines, writer, tabStop);
            Painter.PrintCommandsAndOptions(command, commandLines, optionLines,
                tabStopCalculated, false, w, remainArgs);

            // Painter.PrintTailLines(command, w, remainArgs);
            Painter.PrintEpilogue(command, w, remainArgs);
            // writer.WriteLine("");

            Test();
        }

        #endregion

        #region ShowDebugDumpFragment

        public void ShowDebugDumpFragment(IBaseWorker w)
        {
            var dump = Util.GetEnvValueBool("CMDR_DUMP");
            Painter.PrintDumpForDebug(w.ParsedCommand ?? w.RootCommand, w, tabStopCalculated, true, dump);
        }

        #endregion

        #region commandsWatcherBuilder

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private Func<ICommand, ICommand, int, bool>? commandsWatcherBuilder(IBaseWorker w,
            IDictionary<string, List<TwoString>> commandLines,
            int tabStop, bool noBacktrace)
        {
            return (owner, cmd, level) =>
            {
                if (level > 0) return false;
                if (cmd.Hidden) return true;

                var sb = new StringBuilder("  ");
                if (!string.IsNullOrWhiteSpace(cmd.Short)) sb.Append($"{cmd.Short}, ");
                if (!string.IsNullOrWhiteSpace(cmd.Long)) sb.Append($"{cmd.Long}");
                if (cmd.Aliases.Length > 0) sb.Append(", ").AppendJoin(',', cmd.Aliases);

                var sb2 = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(cmd.Description)) sb2.Append(cmd.Description);
                else if (!string.IsNullOrWhiteSpace(cmd.DescriptionLong)) sb2.Append(cmd.DescriptionLong);

                if (!commandLines.ContainsKey(cmd.Group))
                    commandLines.TryAdd(cmd.Group, new List<TwoString>());

                if (sb.Length >= tabStopCalculated) tabStopCalculated = sb.Length + 1;
                commandLines[cmd.Group].Add(new TwoString
                    {Level = level, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                return true;
            };
        }

        #endregion

        #region flagsWatcherBuilder

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        // ReSharper disable once SuggestBaseTypeForParameter
        private Func<ICommand, IFlag, int /*level*/, bool> flagsWatcherBuilder(IBaseWorker w,
            // SortedDictionary<string, List<TwoString>> commandLines,
            IDictionary<int, CmdFlagLines> optionLines,
            int tabStop, bool noBacktrace)
        {
            return (owner, flag, level) =>
            {
                if (level > 0) return true;
                if (flag.Hidden) return true;

                var sb = new StringBuilder("  ");

                var ph = string.IsNullOrWhiteSpace(flag.PlaceHolder) ? string.Empty : "=" + flag.PlaceHolder;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!string.IsNullOrWhiteSpace(flag.Short))
                    if (EnablePlaceHolderForShortAndAliasFlag)
                        sb.Append($"{Util.SwitchChar(false)}{flag.Short}{ph}, ");
                    else
                        sb.Append($"{Util.SwitchChar(false)}{flag.Short}, ");
                else
                    sb.Append("    ");
                if (!string.IsNullOrWhiteSpace(flag.Long))
                    sb.Append($"{Util.SwitchChar(true)}{flag.Long}{ph}");
                if (flag.Aliases.Length > 0)
                    if (EnablePlaceHolderForShortAndAliasFlag)
                        sb.Append(", --").AppendJoin($"{ph},--", flag.Aliases).Append(ph);
                    else
                        sb.Append(", --").AppendJoin(",--", flag.Aliases);

                var sb2 = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(flag.Description)) sb2.Append(flag.Description);
                else if (!string.IsNullOrWhiteSpace(flag.DescriptionLong)) sb2.Append(flag.DescriptionLong);

                // if (!optionLines.ContainsKey(flag.Group))
                //     optionLines.TryAdd(flag.Group, new List<TwoString>());
                //
                // if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                // optionLines[flag.Group].Add(new TwoString {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                var lvl = UpperBoundLevel - owner.FindLevel();
                if (!optionLines.ContainsKey(lvl))
                    optionLines.TryAdd(lvl,
                        new CmdFlagLines {cmd = owner, lines = new SortedDictionary<string, List<TwoString>>()});
                if (!optionLines[lvl].lines.ContainsKey(flag.Group))
                    optionLines[lvl].lines.TryAdd(flag.Group, new List<TwoString>());

                if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                optionLines[lvl].lines[flag.Group].Add(new TwoString
                    {Level = level, Flag = flag, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                // ReSharper disable once InvertIf
                if (!noBacktrace)
                {
                    noBacktrace = true;
                    var oo = owner.Owner;
                    while (oo != null && oo.Owner != oo)
                    {
                        w.Walk(oo,
                            (o, c, l) => true,
                            flagsWatcherBuilder(w, optionLines, tabStop, noBacktrace)
                        );
                        oo = oo.Owner;
                    }
                }

                return true;
            };
        }

        #endregion


        private const int UpperBoundLevel = int.MaxValue;

        // // ReSharper disable once InconsistentNaming
        // private bool noBacktrace;

        private int tabStopCalculated = 45;

        private static void Test()
        {
            // ColorifyEnabler.Colorify.WriteLine("Text Default", Colors.txtDefault);
            // ColorifyEnabler.Colorify.WriteLine("Text Muted", Colors.txtMuted);
            // ColorifyEnabler.Colorify.WriteLine("Text Primary", Colors.txtPrimary);
            // ColorifyEnabler.Colorify.WriteLine("Text Success", Colors.txtSuccess);
            // ColorifyEnabler.Colorify.WriteLine("Text Info", Colors.txtInfo);
            // ColorifyEnabler.Colorify.WriteLine("Text Warning", Colors.txtWarning);
            // ColorifyEnabler.Colorify.WriteLine("Text Danger", Colors.txtDanger);
            // ColorifyEnabler.Colorify.WriteLine("Background Default", Colors.bgDefault);
            // ColorifyEnabler.Colorify.WriteLine("Background Muted", Colors.bgMuted);
            // ColorifyEnabler.Colorify.WriteLine("Background Primary", Colors.bgPrimary);
            // ColorifyEnabler.Colorify.WriteLine("Background Success", Colors.bgSuccess);
            // ColorifyEnabler.Colorify.WriteLine("Background Info", Colors.bgInfo);
            // ColorifyEnabler.Colorify.WriteLine("Background Warning", Colors.bgWarning);
            // ColorifyEnabler.Colorify.WriteLine("Background Danger", Colors.bgDanger);
        }
    }
}