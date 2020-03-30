using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Logger;

namespace HzNS.Cmdr.Internal.Base
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ILoggable
    {
        ILogger? log { get; }
    }
}