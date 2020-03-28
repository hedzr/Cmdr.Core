using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Base;

namespace HzNS.Cmdr.Internal.Base
{
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

        bool AppVerboseMode { get; }
        bool AppQuietMode { get; }
        bool AppDebugMode { get; }
        bool AppTraceMode { get; }

        Store OptionsStore { get; }

        // ReSharper disable once CollectionNeverUpdated.Local
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        Dictionary<ICommand, Xref> xrefs { get; }

        IRootCommand RootCommand { get; }
    }
}