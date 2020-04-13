#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public abstract class BaseCommand : BaseOpt, ICommand, IEquatable<BaseCommand>
    {
        public bool Equals(BaseCommand? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _flags.Equals(other._flags) && _subCommands.Equals(other._subCommands);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseCommand) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_flags, _subCommands);
        }

        private readonly List<IFlag> _flags = new List<IFlag>();

        private readonly List<ICommand> _subCommands = new List<ICommand>();

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseCommand()
        {
        }

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseCommand(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
            descriptionLong, examples)
        {
            //
        }

        public List<ICommand> SubCommands
        {
            get => _subCommands;
            internal set
            {
                _subCommands.Clear();
                _subCommands.AddRange(value);
            }
        }

        public List<IFlag> Flags
        {
            get => _flags;
            internal set
            {
                _flags.Clear();
                _flags.AddRange(value);
            }
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string TailArgs { get; set; } = "";

        public List<IFlag> RequiredFlags { get; internal set; } = new List<IFlag>();

        public Dictionary<string, List<IFlag>> ToggleableFlags { get; internal set; } =
            new Dictionary<string, List<IFlag>>();

        // public string HitTitle { get; set; } = "";

        public ICommand AddCommand(ICommand cmd)
        {
            cmd.Owner = this;
            _subCommands.Add(cmd);
            return this;
        }

        public ICommand AddFlagGeneric(object flag, bool required = false)
        {
            if (flag is IFlag f)
            {
                var ts = f.GetType().GetGenericArguments();
                if (ts.Length == 1)
                {
                    f.Owner = this;

                    _flags.Add(f);

                    if (required)
                        RequiredFlags.Add(f);

                    // ReSharper disable once InvertIf
                    if (!string.IsNullOrWhiteSpace(f.ToggleGroup) && ts[0] == typeof(bool))
                    {
                        if (string.IsNullOrWhiteSpace(f.Group))
                            f.Group = f.ToggleGroup;
                        if (!ToggleableFlags.ContainsKey(f.ToggleGroup))
                            ToggleableFlags.TryAdd(f.ToggleGroup, new List<IFlag>());
                        ToggleableFlags[f.ToggleGroup].Add(f);
                    }
                }
            }

            return this;
        }

        public ICommand AddFlag<T>(IFlag<T> flag, bool required = false)
        {
            flag.Owner = this;

            _flags.Add(flag);

            if (required)
                RequiredFlags.Add(flag);

            // ReSharper disable once InvertIf
            if (!string.IsNullOrWhiteSpace(flag.ToggleGroup) && flag.DefaultValue is bool)
            {
                if (string.IsNullOrWhiteSpace(flag.Group))
                    flag.Group = flag.ToggleGroup;
                if (!ToggleableFlags.ContainsKey(flag.ToggleGroup))
                    ToggleableFlags.TryAdd(flag.ToggleGroup, new List<IFlag>());
                ToggleableFlags[flag.ToggleGroup].Add(flag);
            }

            return this;
        }

        public ICommand AddAction(Action<IBaseWorker, IBaseOpt, IEnumerable<string>> action)
        {
            Action = action;
            return this;
        }

        public bool IsRoot => Owner == null || Equals(this, Owner);

        public new IRootCommand? FindRoot()
        {
            ICommand? o = this;
            while (o?.Owner != null && o.Owner != o) o = o?.Owner;
            return (IRootCommand?) o;
        }

        public new IFlag? FindFlag(string dottedKey, IBaseOpt? from = null)
        {
            IFlag? fr = null;
            walkFor(from as ICommand ?? this,
                commandsWatcher: (owner, cmd, level) => true,
                flagsWatcher: (owner, flag, level) =>
                {
                    // if (!flag.Match(UriPartial, true, false)) return true;
                    if (flag.ToDottedKey() != dottedKey) return true;
                    fr = flag;
                    return false; // stop walkFor population.
                });
            return fr;
        }

        public new bool Walk(ICommand? from = null,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null)
        {
            return walkFor(from ?? this, commandsWatcher, flagsWatcher);
        }

        // ReSharper disable once InconsistentNaming
        private static bool walkFor(ICommand parent,
            Func<ICommand, ICommand, int, bool>? commandsWatcher = null,
            Func<ICommand, IFlag, int, bool>? flagsWatcher = null,
            int level = 0)
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var f in parent.Flags)
                if (flagsWatcher != null && flagsWatcher(parent, f, level) == false)
                    return false;

            if (parent.SubCommands == null) return true;

            foreach (var cmd in parent.SubCommands)
            {
                if (commandsWatcher != null && commandsWatcher.Invoke(parent, cmd, level) == false)
                    return false;
                if (walkFor(cmd, commandsWatcher, flagsWatcher, level + 1) == false)
                    return false;
            }

            return true;
        }

        public int FindLevel()
        {
            var lvl = 0;
            ICommand? o = this;
            while (o?.Owner != null && o.Owner != o)
            {
                o = o?.Owner;
                lvl++;
            }

            return lvl;
        }

        public bool IsEqual(string title)
        {
            return Match(title) || Match(title, true);
        }

        public bool IsEqual(ICommand command)
        {
            return Equals(this, command);
        }


        public string backtraceTitles
        {
            get
            {
                ICommand o = this;
                var titles = new List<string>();
                while (o.Owner != null)
                {
                    titles.Insert(0, o.Long);
                    o = o.Owner;
                }

                return string.Join(" ", titles);
            }
        }

        // public static bool operator ==(BaseCommand a, string s) => a != null && (a.Match(s) || a.Match(s, true));

        // public static bool operator !=(BaseCommand a, string s) => (a == null || (!a.Match(s) && !a.Match(s, true)));

        public override string ToString()
        {
            return $"Command['{Long}', For:'{backtraceTitles}']";
        }
    }
}