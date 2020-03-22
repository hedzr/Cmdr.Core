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
            set
            {
                _subCommands.Clear();
                _subCommands.AddRange(value);
            }
        }

        public List<IFlag> Flags
        {
            get => _flags;
            set
            {
                _flags.Clear();
                _flags.AddRange(value);
            }
        }

        public ICommand AddCommand(ICommand cmd)
        {
            cmd.Owner = this;
            _subCommands.Add(cmd);
            return this;
        }

        public ICommand AddFlag<T>(IFlag<T> flag)
        {
            flag.Owner = this;
            _flags.Add(flag);
            return this;
        }

        public bool IsRoot => Owner == null || Equals(this, Owner);

        public IRootCommand? FindRoot()
        {
            ICommand? o = this;
            while (o?.Owner != null && o.Owner != o) o = o?.Owner;
            return (IRootCommand?) o;
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