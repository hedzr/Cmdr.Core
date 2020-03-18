using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

        public string Short { get; set; }
        public string Long { get; set; }
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public string DescriptionLong { get; set; }
        public string Examples { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public ICommand Owner { get; set; }

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