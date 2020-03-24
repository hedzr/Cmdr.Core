using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IFlag : IBaseOpt
    {
        string PlaceHolder { get; set; }
        string ToggleGroup { get; set; }

        /// <summary>
        /// For a TimeSpan Flag, enable string parser with
        /// MomentJs format.
        /// The form likes: "3d89s5139ms".
        /// </summary>
        bool UseMomentTimeFormat { get; set; }
        
        object? getDefaultValue();
    }

    public interface IFlag<T> : IFlag // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
        T DefaultValue { get; set; }
    }
}