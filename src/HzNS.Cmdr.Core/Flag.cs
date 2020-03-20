using HzNS.Cmdr.Base;

namespace HzNS.Cmdr
{
    public class Flag<T> : BaseFlag<T>
    {
        public Flag(string shortTitle, string longTitle, string[] aliases, string description, string descriptionLong,
            string examples, T defaultValue, string placeholder) : base(shortTitle, longTitle, aliases, description,
            descriptionLong, examples, defaultValue, placeholder)
        {
        }

        public Flag()
        {
        }
    }
}