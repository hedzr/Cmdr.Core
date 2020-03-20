using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Colorify;

namespace HzNS.Cmdr.Handlers
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class WorkerFunctions : DefaultHandlers
    {
        public void ShowVersions(Worker w, params string[] remainArgs)
        {
            //
        }

        public void ShowBuildInfo(Worker w, params string[] remainArgs)
        {
            //
        }

        // ReSharper disable once UnusedParameter.Local
        private void ShowOne(SortedDictionary<string, List<TwoString>> lines, Format writer, int tabStop, int level = 0)
        {
            foreach (var (group, twoStrings) in lines)
            {
                var g = Util.StripFirstKnobble(group);
                if (g != "Unsorted")
                {
                    var c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    writer.WriteLine($"  [{g}]:", ColorGroup);
                    Console.ForegroundColor = c;
                }

                foreach (var ts in twoStrings)
                {
                    writer.Write(ts.Part1, ColorNormal);
                    writer.Write(new string(' ', tabStop - ts.Part1.Length));

                    var c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (tabStop + ts.Part2.Length > Console.WindowWidth)
                    {
                        var width = Console.WindowWidth - tabStop - 1;
                        var start = 0;
                        while (start < ts.Part2.Length)
                        {
                            if (start + width > ts.Part2.Length) width = ts.Part2.Length - start;
                            writer.WriteLine(ts.Part2.Substring(start, width), ColorDesc);
                            start += width;
                            if (start < ts.Part2.Length)
                            {
                                writer.Write(new string(' ', tabStop));
                            }
                        }
                    }
                    else
                        writer.WriteLine(ts.Part2, ColorDesc);

                    Console.ForegroundColor = c;
                }
            }
        }


        [SuppressMessage("ReSharper", "InvertIf")]
        [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
        private void ShowIt(SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int, CmdFlags> optionLines,
            Format writer, int tabStop, bool treeMode = false)
        {
            if (commandLines.Count > 0)
            {
                if (!treeMode) writer.WriteLine("\nCommands:");
                else writer.WriteLine("\nCommands Tree:");
                ShowOne(commandLines, writer, tabStop);
            }

            if (optionLines.Count > 0)
            {
                var step = 0;
                foreach (var (lvl, cf) in optionLines)
                {
                    if (!treeMode)
                    {
                        // writer.WriteLine($"\nOptions {UpperBoundLevel - lvl}:");
                        if (step == 0)
                            writer.WriteLine($"\nOptions:");
                        else if (cf.cmd.IsRoot)
                            writer.WriteLine($"\nGlobal Options:");
                        else if (step == 1)
                            writer.WriteLine($"\nParent Options ({cf.cmd.backtraceTitles}):");
                        else
                            writer.WriteLine($"\nParents Options ({cf.cmd.backtraceTitles}):");
                    }

                    ShowOne(cf.lines, writer, tabStop, step);
                    step++;
                }
            }
        }

        #region ShowEverything

        public void ShowEverything(Worker w, params string[] remainArgs)
        {
            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlags>();
            var tabStop = w.TabStop;

            w.Walk(w.ParsedCommand,
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

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
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
                            new CmdFlags {cmd = owner, lines = new SortedDictionary<string, List<TwoString>>()});
                    if (!optionLines[lvl].lines.ContainsKey(flag.Group))
                        optionLines[lvl].lines.TryAdd(flag.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
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

            ShowIt(commandLines, optionLines, writer, tabStop);

            writer.WriteLine("");

            Test();
        }

        #endregion

        // ReSharper disable once InconsistentNaming
        private bool noBacktrace;

        public void ShowHelpScreen(Worker w, params string[] remainArgs)
        {
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlags>();
            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var tabStop = w.TabStop;
            noBacktrace = false;

            w.Walk(w.ParsedCommand,
                (owner, cmd, level) =>
                {
                    if (level > 0) return false;
                    if (cmd.Hidden) return true;

                    var sb = new StringBuilder("  ");
                    if (!string.IsNullOrWhiteSpace(cmd.Short)) sb.Append($"{cmd.Short}, ");
                    if (!string.IsNullOrWhiteSpace(cmd.Long)) sb.Append($"{cmd.Long}, ");
                    if (cmd.Aliases.Length > 0) sb.AppendJoin(',', cmd.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(cmd.Description)) sb2.Append(cmd.Description);
                    else if (!string.IsNullOrWhiteSpace(cmd.DescriptionLong)) sb2.Append(cmd.DescriptionLong);

                    if (!commandLines.ContainsKey(cmd.Group))
                        commandLines.TryAdd(cmd.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    commandLines[cmd.Group].Add(new TwoString
                        {Level = level, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                flagsWatcher(w, optionLines, tabStop));

            ShowIt(commandLines, optionLines, writer, tabStop);

            writer.WriteLine("");
        }

        public void DumpTreeForAllCommands(Worker w, params string[] remainArgs)
        {
            w.log.Debug("dump tree");

            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<int, CmdFlags>();
            var tabStop = w.TabStop;

            w.Walk(w.ParsedCommand,
                (owner, cmd, level) =>
                {
                    if (cmd.Hidden) return true;

                    var sb = new StringBuilder(new string(' ', (1 + level) * 2));
                    if (!string.IsNullOrWhiteSpace(cmd.Short)) sb.Append($"{cmd.Short}, ");
                    if (!string.IsNullOrWhiteSpace(cmd.Long)) sb.Append($"{cmd.Long},");
                    if (cmd.Aliases.Length > 0) sb.AppendJoin(',', cmd.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(cmd.Description)) sb2.Append(cmd.Description);
                    else if (!string.IsNullOrWhiteSpace(cmd.DescriptionLong)) sb2.Append(cmd.DescriptionLong);

                    if (!commandLines.ContainsKey(cmd.Group))
                        commandLines.TryAdd(cmd.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    commandLines[cmd.Group].Add(new TwoString
                        {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                (owner, flag, level) => true);

            ShowIt(commandLines, optionLines, writer, tabStop, true);

            writer.WriteLine("");
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private Func<ICommand, IFlag, int /*level*/, bool> flagsWatcher(Worker w,
            // SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int /*level*/, CmdFlags> optionLines,
            int tabStop)
        {
            return (owner, flag, level) =>
            {
                if (level > 0) return true;
                if (flag.Hidden) return true;

                var sb = new StringBuilder("  ");
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (!string.IsNullOrWhiteSpace(flag.Short))
                    sb.Append($"{Util.SwitchChar(false)}{flag.Short}, ");
                else
                    sb.Append("    ");
                if (!string.IsNullOrWhiteSpace(flag.Long)) sb.Append($"{Util.SwitchChar(true)}{flag.Long}, ");
                if (flag.Aliases.Length > 0) sb.Append("--").AppendJoin(",--", flag.Aliases);

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
                        new CmdFlags {cmd = owner, lines = new SortedDictionary<string, List<TwoString>>()});
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
                            flagsWatcher(w, optionLines, tabStop)
                        );
                        oo = oo.Owner;
                    }
                }

                return true;
            };
        }

        private class CmdFlags
        {
#pragma warning disable CS8618
            internal ICommand cmd { get; set; }
            internal SortedDictionary<string /*group*/, List<TwoString>> lines { get; set; }
#pragma warning restore CS8618
        }

        private class TwoString
        {
#pragma warning disable CS8618
            internal int Level { get; set; }
            internal IFlag Flag { get; set; }
            internal string Part1 { get; set; } = "";
            internal string Part2 { get; set; } = "";
#pragma warning restore CS8618
        }

        private const string ColorDesc = Colors.txtMuted;
        private const string ColorGroup = Colors.txtMuted;
        private const string ColorNormal = Colors.txtPrimary;

        private const int UpperBoundLevel = int.MaxValue;

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