#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Tool.Colorify.Theme
{
    public static partial class Theme
    {
        public static ITheme Light => new ThemeLight(false);
    }

    [SuppressMessage("ReSharper", "RedundantBaseQualifier")]
    public sealed class ThemeLight : ThemeAction
    {
        public ThemeLight(bool consoleDefault = true)
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
                base._consoleBackground = ConsoleColor.White;
                base._consoleForeground = ConsoleColor.Black;
            }
        }

        public override void SetComponents()
        {
            var colors = new Dictionary<string, Color>
            {
                { "text-default", AddColor(null, null) },
                { "text-muted", AddColor(null, ConsoleColor.Gray) },
                { "text-primary", AddColor(null, ConsoleColor.DarkGray) },
                { "text-warning", AddColor(null, ConsoleColor.DarkYellow) },
                { "text-danger", AddColor(null, ConsoleColor.DarkRed) },
                { "text-success", AddColor(null, ConsoleColor.DarkGreen) },
                { "text-info", AddColor(null, ConsoleColor.DarkCyan) },
                { "bg-default", AddColor(null, null) },
                { "bg-muted", AddColor(ConsoleColor.Gray, ConsoleColor.Black) },
                { "bg-primary", AddColor(ConsoleColor.DarkGray, ConsoleColor.White) },
                { "bg-warning", AddColor(ConsoleColor.DarkYellow, ConsoleColor.White) },
                { "bg-danger", AddColor(ConsoleColor.DarkRed, ConsoleColor.White) },
                { "bg-success", AddColor(ConsoleColor.DarkGreen, ConsoleColor.White) },
                { "bg-info", AddColor(ConsoleColor.DarkCyan, ConsoleColor.White) }
            };
            base._colors = colors;
        }
    }
}