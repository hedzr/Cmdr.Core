#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Handlers
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    public abstract class DefaultHandlers
    {
        public Action<bool, string, ICommand>? OnDuplicatedCommandChar { get; set; } = (isShort, ch, cmd) =>
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated command char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        };

        public Action<bool, string, ICommand, IBaseFlag>? OnDuplicatedFlagChar { get; set; } = (isShort, ch, cmd, flg) =>
        {
            Console.Error.WriteLineAsync(
                $"WARN: Duplicated flag char FOUND: '{ch}'. Context: \"{cmd.backtraceTitles}\".");
        };
    }
}