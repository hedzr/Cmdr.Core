using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class BaseFlag<T> : BaseOpt, IFlag<T>
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

        public T DefaultValue { get; set; }
        public string PlaceHolder { get; set; }



        public BaseFlag<T> AddDefaultValue(T val)
        {
            DefaultValue = val;
            return this;
        }

        public object getDefaultValue()
        {
            return DefaultValue;
        }



        public override string ToString()
        {
            return $"Flag['{Long}', For:'{Owner?.backtraceTitles}']";
        }
    }
}