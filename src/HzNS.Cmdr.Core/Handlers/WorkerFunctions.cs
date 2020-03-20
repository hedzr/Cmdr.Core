using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Colorify;

namespace HzNS.Cmdr.Handlers
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
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

        private void ShowOne(SortedDictionary<string, List<TwoString>> lines, Format writer, int tabStop)
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
        private void ShowIt(SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<string, List<TwoString>> optionLines, Format writer, int tabStop, bool treeMode = false)
        {
            if (commandLines.Count > 0)
            {
                if (!treeMode) writer.WriteLine("\nCommands:"); else writer.WriteLine("\nCommands Tree:"); 
                ShowOne(commandLines, writer, tabStop);
            }

            if (optionLines.Count > 0)
            {
                if (!treeMode) writer.WriteLine("\nOptions:");
                ShowOne(optionLines, writer, tabStop);
            }
        }

        #region ShowEverything

        public void ShowEverything(Worker w, params string[] remainArgs)
        {
            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<string, List<TwoString>>();
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

                    if (!optionLines.ContainsKey(flag.Group))
                        optionLines.TryAdd(flag.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    optionLines[flag.Group].Add(new TwoString
                        {Level = level, Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                });

            ShowIt(commandLines, optionLines, writer, tabStop);

            writer.WriteLine("");

            Test();
        }

        #endregion

        public void ShowHelpScreen(Worker w, params string[] remainArgs)
        {
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<string, List<TwoString>>();
            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var tabStop = w.TabStop;

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
                    commandLines[cmd.Group].Add(new TwoString {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                flagsWatcher: (owner, flag, level) =>
                {
                    if (level > 0) return true;
                    if (flag.Hidden) return true;

                    var sb = new StringBuilder("  ");
                    if (!string.IsNullOrWhiteSpace(flag.Short)) sb.Append($"{flag.Short}, ");
                    if (!string.IsNullOrWhiteSpace(flag.Long)) sb.Append($"{flag.Long}, ");
                    if (flag.Aliases.Length > 0) sb.AppendJoin(',', flag.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(flag.Description)) sb2.Append(flag.Description);
                    else if (!string.IsNullOrWhiteSpace(flag.DescriptionLong)) sb2.Append(flag.DescriptionLong);

                    if (!optionLines.ContainsKey(flag.Group))
                        optionLines.TryAdd(flag.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    optionLines[flag.Group].Add(new TwoString {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                });

            ShowIt(commandLines, optionLines, writer, tabStop);

            writer.WriteLine("");
        }

        public void DumpTreeForAllCommands(Worker w, params string[] remainArgs)
        {
            w.log.Debug("dump tree");

            var writer = ColorifyEnabler.Colorify; // Console.Out;
            var commandLines = new SortedDictionary<string, List<TwoString>>();
            var optionLines = new SortedDictionary<string, List<TwoString>>();
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
                        {Part1 = new string(' ', level * 2) + sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                },
                (owner, flag, level) =>
                {
                    if (level >= 0) return true;
                    if (flag.Hidden) return true;

                    var sb = new StringBuilder("  ");
                    if (!string.IsNullOrWhiteSpace(flag.Short)) sb.Append($"{flag.Short}, ");
                    if (!string.IsNullOrWhiteSpace(flag.Long)) sb.Append($"{flag.Long},");
                    if (flag.Aliases.Length > 0) sb.AppendJoin(',', flag.Aliases);

                    var sb2 = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(flag.Description)) sb2.Append(flag.Description);
                    else if (!string.IsNullOrWhiteSpace(flag.DescriptionLong)) sb2.Append(flag.DescriptionLong);

                    if (!optionLines.ContainsKey(flag.Group))
                        optionLines.TryAdd(flag.Group, new List<TwoString>());

                    if (sb.Length >= tabStop) tabStop = sb.Length + 1;
                    optionLines[flag.Group].Add(new TwoString {Part1 = sb.ToString(), Part2 = sb2.ToString()});

                    return true;
                });

            ShowIt(commandLines, optionLines, writer, tabStop, true);

            writer.WriteLine("");
        }

        private class TwoString
        {
            internal int Level { get; set; }
            internal string Part1 { get; set; } = "";
            internal string Part2 { get; set; } = "";
        }

        private const string ColorDesc = Colors.txtMuted;
        private const string ColorGroup = Colors.txtMuted;
        private const string ColorNormal = Colors.txtPrimary;

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