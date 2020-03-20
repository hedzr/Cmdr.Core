using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class BaseFlag<T> : BaseOpt, IFlag<T>
    {
#pragma warning disable CS8618
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag()
        {
        }
#pragma warning restore CS8618
        
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseFlag(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples, T defaultValue, string placeholder) : base(shortTitle, longTitle, aliases, description,
            descriptionLong, examples)
        {
            DefaultValue = defaultValue;
            PlaceHolder = placeholder;
        }

        public T DefaultValue { get; set; }
        public string PlaceHolder { get; set; }

        public object? getDefaultValue()
        {
            return DefaultValue;
        }


        public BaseFlag<T> AddDefaultValue(T val)
        {
            DefaultValue = val;
            return this;
        }


        public override string ToString()
        {
            return $"Flag['{Long}', For:'{Owner?.backtraceTitles}']";
        }
    }
}