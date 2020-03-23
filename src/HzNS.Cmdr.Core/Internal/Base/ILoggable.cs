using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace HzNS.Cmdr.Internal.Base
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ILoggable
    {
        ILogger log { get; }
    }
}