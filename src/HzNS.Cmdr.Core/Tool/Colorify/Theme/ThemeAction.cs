#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool.Colorify.Theme
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public abstract class ThemeAction : ITheme
    {
        protected bool _consoleDefault { get; set; }
        protected ConsoleColor _consoleBackground { get; set; }
        protected ConsoleColor _consoleForeground { get; set; }

#pragma warning disable CS8618
        public Dictionary<string, Color> _colors { get; set; }
#pragma warning restore CS8618

        public abstract void SetColors();

        public abstract void SetComponents();

        public void ResetColor()
        {
            Console.ResetColor();
            _consoleBackground = Console.BackgroundColor;
            _consoleForeground = Console.ForegroundColor;
        }

        public Color AddColor(ConsoleColor? background, ConsoleColor? foreground)
        {
            var color = new Color(
                background ?? _consoleBackground,
                foreground ?? _consoleForeground
            );
            return color;
        }
    }
}