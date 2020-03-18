using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HzNS.Cmdr.Builder;

namespace HzNS.Cmdr.Base
{
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class BaseOpt : IBaseOpt
    {
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