#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HzNS.Cmdr.Tool
{
    /// <summary>
    /// refer to https://github.com/deinsoftware/toolbox/blob/master/ToolBox/Platform/Platform.cs
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class OS
    {
        public static bool IsWin() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMac() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsGnu() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string Current =>
            (IsWin()
                ? "win"
                : (IsMac() ? "mac" : (IsGnu() ? "gnu" : "unknown")
                )
            );
    }
}