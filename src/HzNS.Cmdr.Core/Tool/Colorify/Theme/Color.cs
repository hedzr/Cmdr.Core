using System;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool.Colorify.Theme
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct Color
    {
        public ConsoleColor _background { get; private set; }
        public ConsoleColor _foreground { get; private set; }

        public Color(ConsoleColor background, ConsoleColor foreground) : this()
        {
            _background = background;
            _foreground = foreground;
        }
    }
}
