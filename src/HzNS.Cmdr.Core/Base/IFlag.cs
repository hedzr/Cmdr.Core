using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IFlag : IBaseOpt
    {
        string PlaceHolder { get; set; }
        string ToggleGroup { get; set; }

        object? getDefaultValue();
    }

    public interface IFlag<T> : IFlag // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
        T DefaultValue { get; set; }
    }
}