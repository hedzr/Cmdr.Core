#nullable enable
using System;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;

namespace HzNS.Cmdr.Internal
{
    /// <summary>
    /// injected by IDefaultHandlers.
    /// ref:
    /// Worker : WorkerFunctions, IDefaultHandlers, IDefaultMatchers
    /// </summary>
    public static class DefaultHandlers
    {
        public static void DefaultOnDuplicatedCommandChar<T>(this T t, bool isShort, string ch, ICommand cmd)
            where T : IDefaultHandlers
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated command char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        }

        public static void DefaultOnDuplicatedFlagChar<T>(this T t, bool isShort, string ch, ICommand cmd, IFlag flg)
            where T : IDefaultHandlers
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated flag char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        }
    }
}