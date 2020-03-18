using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class BaseFlag : BaseOpt, IFlag
    {
    }
}