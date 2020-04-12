#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface IFlag : IBaseOpt
    {
        /// <summary>
        /// In the help screen, a flag be shown like:
        /// <code>
        /// -f, --filename=[FILE], --pathname,--file    The filepath for the parser
        /// </code>
        /// NOTE that the string "FILE" is a [PlaceHolder]. 
        /// </summary>
        string PlaceHolder { get; set; }

        /// <summary>
        /// The value of a group of boolean flags, can be flipped
        /// while one of them be set, just like a radio-button group.
        /// <br/>
        /// If [ToggleGroup] is present, [Group] can be ignored.
        /// </summary>
        string ToggleGroup { get; set; }

        /// <summary>
        /// A flag can be bound to environment variables.
        /// <br/>
        /// No effect for a command.
        /// </summary>
        string[] EnvVars { get; set; }

        /// <summary>
        /// For a TimeSpan Flag, enables string parser with
        /// MomentJs format.
        /// The form likes: "3d89s5139ms".
        /// </summary>
        bool UseMomentTimeFormat { get; set; }

        /// <summary>
        /// The times of a flag input in the command-line.
        /// <br/>
        /// <code>-vvv</code> means its HitCount is 3.
        /// </summary>
        int HitCount { get; }

        /// <summary>
        /// Extracting the [DefaultValue] without strong-type.
        /// </summary>
        /// <returns></returns>
        object? getDefaultValue();
    }

    public interface IFlag<T> : IFlag // , IOnSet
    {
        // IFlag AddFlag(IFlag flag);
        
        /// <summary>
        /// As is.
        /// </summary>
        T DefaultValue { get; set; }
    }
}