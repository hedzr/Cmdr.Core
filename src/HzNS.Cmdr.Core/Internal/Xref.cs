#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Exception;

namespace HzNS.Cmdr.Internal
{
    #region Xref class

    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    // ReSharper disable once ClassNeverInstantiated.Local
    public class Xref
    {
        public void TryAddShort(Worker w, ICommand cmd)
        {
            if (!string.IsNullOrWhiteSpace(cmd.Short))
                if (!SubCommandsShortNames.TryAdd(cmd.Short, cmd))
                {
                    if (w.OnDuplicatedCommandChar?.Invoke(w, cmd, false, cmd.Short) == false)
                        w.DefaultOnDuplicatedCommandChar(true, cmd.Short, cmd);
                    if (w.EnableDuplicatedCharThrows)
                        throw new DuplicationCommandCharException(true, cmd.Short, cmd);
                }
        }

        public void TryAddLong(Worker w, ICommand cmd)
        {
            if (!string.IsNullOrWhiteSpace(cmd.Long))
            {
                if (!SubCommandsLongNames.TryAdd(cmd.Long, cmd))
                {
                    if (w.OnDuplicatedCommandChar?.Invoke(w, cmd, false, cmd.Long) == false)
                        w.DefaultOnDuplicatedCommandChar(false, cmd.Long, cmd);
                    if (w.EnableDuplicatedCharThrows)
                        throw new DuplicationCommandCharException(false, cmd.Long, cmd);
                }
            }
            else
            {
                if (w.EnableEmptyLongFieldThrows)
                    throw new EmptyCommandLongFieldException(false, cmd.Long, cmd);
            }
        }

        public void TryAddAliases(Worker w, ICommand cmd)
        {
            if (cmd.Aliases != null)
                foreach (var a in cmd.Aliases)
                    if (!string.IsNullOrWhiteSpace(a))
                        if (!SubCommandsAliasNames.TryAdd(a, cmd))
                        {
                            if (w.OnDuplicatedCommandChar?.Invoke(w, cmd, false, a) == false)
                                w.DefaultOnDuplicatedCommandChar(false, a, cmd);
                            if (w.EnableDuplicatedCharThrows)
                                throw new DuplicationCommandCharException(false, a, cmd);
                        }
        }

        public void TryAddShort(Worker w, ICommand owner, IFlag flag)
        {
            if (!string.IsNullOrWhiteSpace(flag.Short))
                if (!FlagsShortNames.TryAdd(flag.Short, flag))
                {
                    if (w.OnDuplicatedFlagChar?.Invoke(w, owner, flag, true, flag.Short) == false)
                        w.DefaultOnDuplicatedFlagChar(true, flag.Short, owner, flag);
                    if (w.EnableDuplicatedCharThrows)
                        throw new DuplicationFlagCharException(true, flag.Short, flag, owner);
                }
        }

        public void TryAddLong(Worker w, ICommand owner, IFlag flag)
        {
            if (!string.IsNullOrWhiteSpace(flag.Long))
            {
                if (!FlagsLongNames.TryAdd(flag.Long, flag))
                {
                    if (w.OnDuplicatedFlagChar?.Invoke(w, owner, flag, false, flag.Long) == false)
                        w.DefaultOnDuplicatedFlagChar(false, flag.Long, owner, flag);
                    if (w.EnableDuplicatedCharThrows)
                        throw new DuplicationFlagCharException(false, flag.Long, flag, owner);
                }
            }
            else
            {
                if (w.EnableEmptyLongFieldThrows)
                    throw new EmptyFlagLongFieldException(false, flag.Long, flag, owner);
            }
        }

        public void TryAddAliases(Worker w, ICommand owner, IFlag flag)
        {
            if (flag.Aliases != null)
                foreach (var a in flag.Aliases)
                    if (!string.IsNullOrWhiteSpace(a))
                        if (!FlagsAliasNames.TryAdd(a, flag))
                        {
                            if (w.OnDuplicatedFlagChar?.Invoke(w, owner, flag, false, a) == false)
                                w.DefaultOnDuplicatedFlagChar(false, a, owner, flag);
                            if (w.EnableDuplicatedCharThrows)
                                throw new DuplicationFlagCharException(false, a, flag, owner);
                        }
        }

        #region properties

        // public ICommand Command { get; set; } = null;
        public Dictionary<string, ICommand> SubCommandsShortNames { get; } =
            new Dictionary<string, ICommand>();

        public Dictionary<string, ICommand> SubCommandsLongNames { get; } = new Dictionary<string, ICommand>();

        public Dictionary<string, ICommand> SubCommandsAliasNames { get; } =
            new Dictionary<string, ICommand>();

        public Dictionary<string, IFlag> FlagsShortNames { get; } = new Dictionary<string, IFlag>();
        public Dictionary<string, IFlag> FlagsLongNames { get; } = new Dictionary<string, IFlag>();
        public Dictionary<string, IFlag> FlagsAliasNames { get; } = new Dictionary<string, IFlag>();

        #endregion
    }

    #endregion
}