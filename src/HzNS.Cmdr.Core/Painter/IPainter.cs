using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Tool;
using HzNS.Cmdr.Tool.Colorify;
using HzNS.Cmdr.Tool.Ext;

namespace HzNS.Cmdr.Painter
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public interface IPainter
    {
        void PrintPrologue(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
        }

        void PrintEpilogue(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
        }

        void PrintPreface(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
        }

        void PrintHeadLines(ICommand cmd, IBaseWorker w, params string[] remainArgs);
        void PrintTailLines(ICommand cmd, IBaseWorker w, params string[] remainArgs);


        void PrintCommandsAndOptions(ICommand cmd,
            SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int, CmdFlags> optionLines,
            // Format writer, 
            int tabStop, bool treeMode,
            IBaseWorker w, params string[] remainArgs);

        void PrintDumpForDebug(ICommand cmd, IBaseWorker w, int tabStop = 45, bool hitOnly = true, bool enabled = false)
        {
        }
    }

    public class DefaultPainter : IPainter
    {
        #region Constructor

// #pragma warning disable 414
//         private readonly Func<string, Task>? _func = null;
// #pragma warning restore 414
//         private readonly Action<string, string> _oln;
//         private readonly Action<string, string> _o;

        // ReSharper disable once EmptyConstructor
        public DefaultPainter()
        {
            // var output = ColorifyEnabler.Colorify;
            // _oln = output.WriteLine; // _output.WriteLineAsync;
            // _o = output.Write;
        }

        #endregion

        #region PrintDumpForDebug

        public void PrintDumpForDebug(ICommand cmd, IBaseWorker w, int tabStop, bool hitOnly = true, bool enabled = false)
        {
            if (!enabled) return;

            oln("Dump the Store:");
            var store = Cmdr.Instance.Store;
            store.Dump(o);

            oln("\n\nDump the Flags (Hit only):");
            dumpValues(w.RootCommand, w, tabStop, hitOnly);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once InconsistentNaming
        private void dumpValues(ICommand parent, IBaseWorker w, int tabStop, bool hitOnly = true)
        {
            w.Walk(parent,
                commandsWatcher: (owner, cmd, level) => true,
                flagsWatcher: (owner, flg, level) =>
                {
                    var s = Cmdr.Instance.Store;
                    var (slot, vk) = s.FindByKeys(flg.ToKeys());
                    // ReSharper disable once InvertIf
                    if (slot != null && (!hitOnly || flg.HitCount > 0))
                    {
                        var v = slot.Values[vk];
                        var txt = new StringBuilder();
                        txt.Append(" ".Repeat(level + 2))
                            .Append(flg.ToDottedKey());
                        txt.Append(" ".Repeat(tabStop - txt.Length - 1));
                        o(txt.ToString(), ColorDesc);
                        oln($"=> [{flg.HitCount}] {v?.ToStringEx()}");
                    }

                    return true;
                });
        }

        #endregion

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void oln(string text, string color = Colors.txtDefault)
        {
            ColorifyEnabler.Colorify.WriteLine(text, color);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void o(string text, string color = Colors.txtDefault)
        {
            ColorifyEnabler.Colorify.Write(text, color);
        }

        public void PrintEpilogue(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            ColorifyEnabler.Colorify.WriteLine(Console.Out.NewLine);
        }

        public void PrintHeadLine(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            // 
        }

        public void PrintHeadLines(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            //
        }

        public void PrintTailLines(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            oln(Console.Out.NewLine, ColorDesc);
            oln("Type '-h'/'-?' or '--help' to get command help screen.", ColorDesc);
            oln("More: '-D'/'--debug', '-v'|'--verbose', '-V'/'--version', '-#'/'--build-info'...", ColorDesc);
        }

        public void PrintCommandsAndOptions(ICommand cmd,
            SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int, CmdFlags> optionLines, // Format writer,
            int tabStop, bool treeMode, IBaseWorker w, params string[] remainArgs)
        {
            if (treeMode)
            {
                var title = cmd.IsRoot ? "-ROOT-" : cmd.backtraceTitles;
                if (commandLines.Count > 0)
                {
                    oln($"\nCommands Tree For '{title}':");
                    ShowOne(commandLines, tabStop);
                }
                else
                    oln($"\nNO SUB-COMMANDS For '{title}'");
            }
            else
            {
                if (commandLines.Count > 0)
                {
                    oln("\nCommands:");
                    ShowOne(commandLines, tabStop);
                }
            }

            if (optionLines.Count > 0)
            {
                var step = 0;
                // ReSharper disable once UnusedVariable
                foreach (var (lvl, cf) in optionLines)
                {
                    if (!treeMode)
                    {
                        // writer.WriteLine($"\nOptions {UpperBoundLevel - lvl}:");
                        if (step == 0)
                            oln($"\nOptions:");
                        else if (cf.cmd.IsRoot)
                            oln($"\nGlobal Options:");
                        else if (step == 1)
                            oln($"\nParent Options ({cf.cmd.backtraceTitles}):");
                        else
                            oln($"\nParents Options ({cf.cmd.backtraceTitles}):");
                    }

                    ShowOne(cf.lines, tabStop, step);
                    step++;
                }
            }
        }


        // ReSharper disable once UnusedParameter.Local
        private void ShowOne(SortedDictionary<string, List<TwoString>> lines,
            // Format writer, 
            int tabStop, int level = 0)
        {
            foreach (var (group, twoStrings) in lines)
            {
                var g = Util.StripFirstKnobble(group);
                if (g != "Unsorted")
                {
                    var c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    oln($"  [{g}]:", ColorGroup);
                    Console.ForegroundColor = c;
                }

                foreach (var ts in twoStrings)
                {
                    o(ts.Part1, ColorNormal);
                    o(" ".Repeat(tabStop - ts.Part1.Length));

                    var c = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (tabStop + ts.Part2.Length > Console.WindowWidth)
                    {
                        var width = Console.WindowWidth - tabStop - 1;
                        var start = 0;
                        while (start < ts.Part2.Length)
                        {
                            if (start + width > ts.Part2.Length) width = ts.Part2.Length - start;
                            oln(width < 0 ? ts.Part2 : ts.Part2.Substring(start, width), ColorDesc);
                            if (width > 0)
                            {
                                start += width;
                                if (start < ts.Part2.Length)
                                {
                                    o(" ".Repeat(tabStop));
                                }
                            }
                            else
                                start = ts.Part2.Length;
                        }
                    }
                    else
                        oln(ts.Part2, ColorDesc);

                    Console.ForegroundColor = c;
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public static string ColorDesc = Colors.txtMuted;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public static string ColorGroup = Colors.txtMuted;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once ConvertToConstant.Global
        public static string ColorNormal = Colors.txtPrimary;
    }


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CmdFlags
    {
#pragma warning disable CS8618
        // ReSharper disable once InconsistentNaming
        public ICommand cmd { get; set; }
        public SortedDictionary<string /*group*/, List<TwoString>> lines { get; set; }
#pragma warning restore CS8618
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class TwoString
    {
#pragma warning disable CS8618
        public int Level { get; set; }
        public IFlag Flag { get; set; }
        public string Part1 { get; set; } = "";
        public string Part2 { get; set; } = "";
#pragma warning restore CS8618
    }
}