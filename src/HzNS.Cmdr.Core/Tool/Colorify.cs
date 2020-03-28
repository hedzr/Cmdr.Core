#nullable enable
// ReSharper disable once RedundantUsingDirective

using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Tool.Colorify;
using HzNS.Cmdr.Tool.Colorify.Theme;

namespace HzNS.Cmdr.Tool
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class ColorifyEnabler
    {
#pragma warning disable CS8618
        public static Format Colorify { get; set; }
#pragma warning restore CS8618

        /// <summary>
        ///
        /// see also:
        /// https://github.com/deinsoftware/colorify
        /// </summary>
        public static void Enable()
        {
            // Console.WriteLine($"{OS.Current},{OS.IsGnu()},{OS.IsMac()},{OS.IsWin()}");
            Colorify = OS.Current switch
            {
                "win" => new Format(Theme.Dark),
                "gnu" => new Format(Theme.Dark),
                "mac" => new Format(Theme.Dark),
                _ => Colorify
            };
            // Console.WriteLine($"{Colorify}");
        }

        public static void Reset()
        {
            Colorify.ResetColor();
            // Colorify.Clear();
            // Console.ResetColor();
        }
    }
}