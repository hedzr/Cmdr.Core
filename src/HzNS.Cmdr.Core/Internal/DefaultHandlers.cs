#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Base;

namespace HzNS.Cmdr.Internal
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    // ReSharper disable once InconsistentNaming
    public interface IDefaultHandlers
    {
    }

    public static class DefaultHandlers
    {
        public static void OnDuplicatedCommandChar<T>(this T t, bool isShort, string ch, ICommand cmd)
            where T : IDefaultHandlers
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated command char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        }

        public static void OnDuplicatedFlagChar<T>(this T t, bool isShort, string ch, ICommand cmd, IFlag flg)
            where T : IDefaultHandlers
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated flag char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        }
    }
}