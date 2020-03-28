#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool.Colorify.Theme
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public interface ITheme
    {
        Dictionary<string, Color> _colors { get; set; }
        Color AddColor(ConsoleColor? background, ConsoleColor? foreground);
        void SetColors();
        void SetComponents();
    }
}