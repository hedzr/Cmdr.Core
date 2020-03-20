#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public abstract class BaseOpt : IBaseOpt
    {
        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseOpt()
        {
        }

        // ReSharper disable once PublicConstructorInAbstractClass
        public BaseOpt(string shortTitle, string longTitle, string[] aliases, string description,
            string descriptionLong, string examples)
        {
            Short = shortTitle;
            Long = longTitle;
            Aliases = aliases;
            Description = description;
            DescriptionLong = descriptionLong;
            Examples = examples;
        }


        public string Long { get; set; } = "";
        public string Short { get; set; } = "";
        public string[] Aliases { get; set; } = { };
        public string Description { get; set; } = "";
        public string DescriptionLong { get; set; } = "";
        public string Examples { get; set; } = "";
        public string Group { get; set; } = "";
        public bool Hidden { get; set; } = false;
        public string[] EnvVars { get; set; } = { };

        public Func<Worker, IEnumerable<string>, bool>? PreAction { get; set; }
        public Action<Worker, IEnumerable<string>>? PostAction { get; set; }
        public Action<Worker, IEnumerable<string>>? Action { get; set; }
        public Action<Worker, object?, object?>? OnSet { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand? Owner { get; set; } = null;

        public bool Match(string s, bool isLongOpt = false)
        {
            if (!isLongOpt)
            {
                if (s == Short) return true;
            }
            else
            {
                return s == Long || Aliases.Any(title => s == title);
            }

            return false;
        }
    }
}