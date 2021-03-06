#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public interface IPainter
    {
        void Setup(ICommand cmd, IBaseWorker w, IList<string> remainArgs)
        {
        }

        void PrintPrologue(ICommand cmd, IBaseWorker w, IList<string> remainArgs)
        {
        }

        void PrintEpilogue(ICommand cmd, IBaseWorker w, IList<string> remainArgs)
        {
        }

        void PrintPreface(ICommand cmd, IBaseWorker w, IList<string> remainArgs)
        {
        }

        void PrintHeadLines(ICommand cmd, IBaseWorker w, bool singleLine, IList<string> remainArgs);
        void PrintTailLines(ICommand cmd, IBaseWorker w, IList<string> remainArgs);


        void PrintUsages(ICommand cmd, IBaseWorker w, IList<string> remainArgs);
        void PrintExamples(ICommand cmd, IBaseWorker w, IList<string> remainArgs);

        void PrintCommandsAndOptions(ICommand cmd,
            SortedDictionary<string, List<TwoString>> commandLines,
            SortedDictionary<int, CmdFlagLines> optionLines,
            // Format writer, 
            int tabStop, bool treeMode,
            IBaseWorker w, IList<string> remainArgs);

        void PrintDumpForDebug(ICommand cmd, IBaseWorker w, int tabStop = 45, bool hitOnly = true, bool enabled = false)
        {
        }

        void PrintBuildInfo(ICommand cmd, in int tabStop, IBaseWorker w, IList<string> remainArgs);
        void PrintVersions(ICommand cmd, in int tabStop, IBaseWorker w, IList<string> remainArgs);
    }


    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CmdFlagLines
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