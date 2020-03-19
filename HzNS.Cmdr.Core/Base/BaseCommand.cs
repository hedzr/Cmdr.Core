using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public abstract class BaseCommand : BaseOpt, ICommand
    {
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

        private List<ICommand> _subCommands = new List<ICommand>();
        private List<IFlag> _flags = new List<IFlag>();

        public List<ICommand> SubCommands
        {
            get => _subCommands;
            set => _subCommands = value;
        }

        public List<IFlag> Flags
        {
            get => _flags;
            set => _flags = value;
        }

        public ICommand AddCommand(ICommand cmd)
        {
            if (_subCommands == null)
                _subCommands = new List<ICommand>();

            cmd.Owner = this;
            _subCommands.Add(cmd);
            return this;
        }

        public ICommand AddFlag(IFlag flag)
        {
            if (_flags == null)
                _flags = new List<IFlag>();

            flag.Owner = this;
            _flags.Add(flag);
            return this;
        }

        public IRootCommand FindRoot()
        {
            ICommand o = this;
            while (o.Owner != null) o = o.Owner;
            return o as IRootCommand;
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

        public override string ToString()
        {
            return $"Command['{Long}', For:'{backtraceTitles}']";
        }
    }
}