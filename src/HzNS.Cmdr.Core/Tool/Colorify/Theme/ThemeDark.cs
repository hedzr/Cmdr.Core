#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool.Colorify.Theme
{
    public static partial class Theme
    {
        public static ITheme Dark => new ThemeDark(false);
    }

    [SuppressMessage("ReSharper", "RedundantBaseQualifier")]
    public sealed class ThemeDark : ThemeAction
    {
        public ThemeDark(bool consoleDefault = true)
        {
            base._consoleDefault = consoleDefault;
            SetColors();
            SetComponents();
        }

        public override void SetColors()
        {
            if (base._consoleDefault)
            {
                ResetColor();
            }
            else
            {
                base._consoleBackground = ConsoleColor.Black;
                base._consoleForeground = ConsoleColor.White;
            }
        }

        public override void SetComponents()
        {
            var colors = new Dictionary<string, Color>
            {
                { "text-default", AddColor(null, null) },
                { "text-muted", AddColor(null, ConsoleColor.DarkGray) },
                { "text-primary", AddColor(null, ConsoleColor.Gray) },
                { "text-warning", AddColor(null, ConsoleColor.Yellow) },
                { "text-danger", AddColor(null, ConsoleColor.Red) },
                { "text-success", AddColor(null, ConsoleColor.DarkGreen) },
                { "text-info", AddColor(null, ConsoleColor.DarkCyan) },
                { "bg-default", AddColor(null, null) },
                { "bg-muted", AddColor(ConsoleColor.DarkGray, ConsoleColor.Black) },
                { "bg-primary", AddColor(ConsoleColor.Gray, ConsoleColor.White) },
                { "bg-warning", AddColor(ConsoleColor.Yellow, ConsoleColor.Black) },
                { "bg-danger", AddColor(ConsoleColor.Red, ConsoleColor.White) },
                { "bg-success", AddColor(ConsoleColor.DarkGreen, ConsoleColor.White) },
                { "bg-info", AddColor(ConsoleColor.DarkCyan, ConsoleColor.White) }
            };
            base._colors = colors;
        }
    }
}