using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Base;

namespace mdx.Cmd
{
    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsAddCmd : BaseCommand
    {
        public TagsAddCmd() : base("a", "add", new string[] {"create", "new"}, "", "", "")
        {
        }

        // public TagsAddCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsRemoveCmd : BaseCommand
    {
        public TagsRemoveCmd() : base("rm", "remove", new string[] {"delete", "del", "erase"}, "", "", "")
        {
        }

        // public TagsRemoveCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsListCmd : BaseCommand
    {
        public TagsListCmd() : base("l", "list", new string[] {"ls", "lst"}, "", "", "")
        {
        }

        // public TagsListCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsModifyCmd : BaseCommand
    {
        public TagsModifyCmd() : base("m", "modify", new string[] {"mod", "modi", "update"}, "", "", "")
        {
        }

        // public TagsModifyCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }
}