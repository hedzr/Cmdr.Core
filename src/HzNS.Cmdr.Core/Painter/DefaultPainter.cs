#nullable enable
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

        private bool _quiteMode;
        private IRootCommand? _root;


        public void Setup(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            // Console.WriteLine("Setup...");
            // _quiteMode = Parse(Cmdr.Instance.Store.Get("quiet").ToString());
            _quiteMode = Cmdr.Instance.Store.GetAs("quiet", _quiteMode);
            _root = cmd.FindRoot();

            // int qi = Cmdr.Instance.Store.GetAs("quiet", 0);
            // Console.WriteLine($"qi={qi}");
        }


        public void PrintHeadLines(ICommand cmd, IBaseWorker w, bool singleLine = false, params string[] remainArgs)
        {
            if (_quiteMode) return;

            var author = _root?.AppInfo.Author;
            if (string.IsNullOrWhiteSpace(author))
                author = "Freeman";

            oln($"{_root?.AppInfo.AppName} - {_root?.AppInfo.AppVersion} - {author}");

            if (singleLine) return;

            if (!string.IsNullOrWhiteSpace(_root?.AppInfo.Copyright))
                olnIndent(_root?.AppInfo.Copyright, ColorDesc, 4);

            if (!string.IsNullOrWhiteSpace(_root?.Description))
            {
                oln("\nDescription:");
                olnIndent(_root?.Description, ColorDesc, 4);
            }

            // ReSharper disable once InvertIf
            if (Cmdr.Instance.Store.GetAs<bool>("verbose"))
            {
                if (!string.IsNullOrWhiteSpace(_root?.DescriptionLong))
                {
                    oln("");
                    olnIndent(_root?.DescriptionLong, ColorDesc, 4);
                }

                // ReSharper disable once InvertIf
                if (!string.IsNullOrWhiteSpace(_root?.Examples))
                {
                    oln("\nExamples:");
                    olnIndent(_root?.Examples, ColorDesc, 4);
                }
            }
        }

        public void PrintUsages(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            if (_quiteMode) return;

            // ReSharper disable once IdentifierTypo
            var cmds = cmd.backtraceTitles;
            if (string.IsNullOrWhiteSpace(cmds))
                cmds = " [Commands]";
            else if (cmd.SubCommands.Count > 0)
                cmds += " [Sub-Commands]";

            var tails = "[Tail Args]|[Options]|[Parent/Global Options]";
            if (!string.IsNullOrWhiteSpace(cmd.TailArgs))
                tails = cmd.TailArgs;

            oln("");
            oln("Usages:");
            oln($"    {_root?.AppInfo.AppName} {cmds} {tails}");
        }

        public void PrintExamples(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            if (_quiteMode) return;

            // ReSharper disable once InvertIf
            if (!string.IsNullOrWhiteSpace(cmd.Examples))
            {
                oln("Examples:");
                oln(cmd.Examples, ColorDesc);
            }
        }


        #region PrintCommandsAndOptions

        public void PrintCommandsAndOptions(ICommand cmd,
            SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int, CmdFlagLines> optionLines, // Format writer,
            int tabStop, bool treeMode, IBaseWorker w, params string[] remainArgs)
        {
            if (_quiteMode) return;

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
                    if (cmd.Owner != null && cmd.Owner.IsRoot)
                        oln("\nCommands:");
                    else
                        oln("\nSub-Commands:");
                    ShowOne(commandLines, tabStop);
                }
            }

            // ReSharper disable once InvertIf
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
                var g = group.StripFirstKnobble();
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
                    var tst = tabStop - ts.Part1.Length;
                    if (tst > 0) o(" ".Repeat(tst));

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

        #endregion


        public void PrintTailLines(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            if (_quiteMode) return;

            oln("", ColorDesc);
            oln("Type '-h'/'-?' or '--help' to get command help screen.", ColorDesc);
            oln("More: '-D'/'--debug', '-v'|'--verbose', '-V'/'--version', '-#'/'--build-info'...", ColorDesc);
        }

        public void PrintEpilogue(ICommand cmd, IBaseWorker w, params string[] remainArgs)
        {
            if (_quiteMode) return;
            oln("");
        }


        #region PrintDumpForDebug

        [SuppressMessage("ReSharper", "InvertIf")]
        public void PrintDumpForDebug(ICommand cmd, IBaseWorker w, int tabStop, bool hitOnly = true,
            bool enabled = false)
        {
            if (!enabled) return;

            if (Util.GetEnvValueBool("CMDR_DUMP_NO_STORE") == false)
            {
                oln("Dump the Store:");
                var store = Cmdr.Instance.Store;
                store.Dump(o);
            }

            if (Util.GetEnvValueBool("CMDR_DUMP_NO_HIT") == false)
            {
                oln("\n\nDump the Flags (Hit only):");
                dumpValues(w.RootCommand, w, tabStop, hitOnly);
                oln(string.Empty);
            }
        }

        #endregion

        #region PrintBuildInfo

        public void PrintBuildInfo(ICommand cmd, in int tabStop, IBaseWorker w, params string[] remainArgs)
        {
            var root = cmd.FindRoot();
            // ReSharper disable once InvertIf
            if (root != null)
            {
                if (_quiteMode)
                {
                    oln($"{root.AppInfo.Builder}");
                    oln($"{root.AppInfo.BuildTimestamp}");
                    oln($"{root.AppInfo.BuildVcsHash}");
                }
                else
                {
                    oln($"       Built by: {root.AppInfo.Builder}");
                    oln($"Build Timestamp: {root.AppInfo.BuildTimestamp}");
                    oln($"  Build Githash: {root.AppInfo.BuildVcsHash}");
                }

                oln(string.Empty);
            }
        }

        #endregion

        #region PrintVersions

        public void PrintVersions(ICommand cmd, in int tabStop, IBaseWorker w, params string[] remainArgs)
        {
            var root = cmd.FindRoot();
            // ReSharper disable once InvertIf
            if (root != null)
            {
                oln(string.Empty);
                
                if (_quiteMode)
                {
                    oln($"{root.AppInfo.AppVersion}");
                    oln($"{root.AppInfo.AppName}");
                    oln($"{root.AppInfo.BuildTimestamp}");
                    // oln($"{root.AppInfo.BuildVcsHash}");
                    // oln($"{root.AppInfo.Builder}");
                    oln($"{root.AppInfo.LinkerTimestampUtc}");
                    oln($"{VersionUtil.AssemblyProductAttribute}");
                }
                else
                {
                    oln($"             App Version: {root.AppInfo.AppVersion}");
                    oln($"                App Name: {root.AppInfo.AppName}");
                    oln($"         Build Timestamp: {root.AppInfo.BuildTimestamp}");
                    // oln($"                Built by: {root.AppInfo.Builder}");
                    oln($"        Linker Timestamp: {root.AppInfo.LinkerTimestampUtc}");
                    oln($"AssemblyProductAttribute: {VersionUtil.AssemblyProductAttribute}");
                    oln("");
                    oln($"            Cmdr Version: {VersionUtil.CmdrAssemblyInformationalVersion}");
                    oln($"       Cmdr File Version: {VersionUtil.CmdrAssemblyFileVersion}");
                    oln($"     Cmdr Assembly Title: {VersionUtil.CmdrAssemblyTitle}");
                    oln($"       Cmdr Git CommitId: {VersionUtil.CmdrGitCommitId}");
                    oln($"    Cmdr Git Commit Date: {VersionUtil.CmdrGitCommitDate}");
                    oln($"     Cmdr Root Namespace: {VersionUtil.CmdrRootNamespace}");
                }

                // oln(string.Empty);
            }
        }

        #endregion

        #region PrintDumpForDebug - helpers

        // ReSharper disable once SuggestBaseTypeForParameter
        // ReSharper disable once InconsistentNaming
        private void dumpValues(ICommand parent, IBaseWorker w, int tabStop, bool hitOnly = true)
        {
            w.Walk(parent,
                commandsWatcher: (owner, cmd, level) => true,
                flagsWatcher: (owner, flg, level) =>
                {
                    var s = Cmdr.Instance.Store;
                    var (slot, vk) = s.FindBy(flg.ToKeys());
                    // ReSharper disable once InvertIf
                    if (slot != null && (!hitOnly || flg.HitCount > 0))
                    {
                        var v = slot.Values[vk];
                        var txt = new StringBuilder();
                        txt.Append(" ".Repeat(level + 2))
                            .Append(flg.ToDottedKey());
                        txt.Append(" ".Repeat(tabStop - txt.Length - 1));
                        o(txt.ToString(), ColorDesc);
                        o($"=> ");
                        o($"[{flg.HitCount}] ", ColorDesc);
                        oln($"{v?.ToStringEx(true)}");
                        
                    }

                    return true;
                });
        }

        #endregion


        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void olnTabbedPrint(string textL, string textR, int tabStop = 8, string colorL = Colors.txtDefault,
            string colorR = Colors.txtDefault)
        {
            ColorifyEnabler.Colorify.WriteLine(textL, colorL);
            var ts = tabStop - textL.Length;
            if (ts <= 0) ts = textL.Length;
            var spaces = " ".Repeat(ts);
            ColorifyEnabler.Colorify.Write(spaces);
            ColorifyEnabler.Colorify.WriteLine(textR, colorR);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void olnIndent(string? textLines, string color = Colors.txtDefault, int leftPad = 8)
        {
            if (textLines == null) return;
            var spaces = " ".Repeat(leftPad);
            foreach (var line in textLines.Split('\n'))
            {
                ColorifyEnabler.Colorify.Write(spaces);
                ColorifyEnabler.Colorify.WriteLine(line, color);
            }
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void oln(string? text, string color = Colors.txtDefault)
        {
            ColorifyEnabler.Colorify.WriteLine(text ?? string.Empty, color);
        }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void o(string? text, string color = Colors.txtDefault)
        {
            ColorifyEnabler.Colorify.Write(text ?? string.Empty, color);
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
}