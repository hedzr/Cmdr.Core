using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class BaseFlag : BaseOpt, IFlag
    {
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag()
        {
        }

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
            descriptionLong, examples)
        {
        }

        public object DefaultValue { get; set; }
        public string PlaceHolder { get; set; }
    }
}