#nullable enable
using System;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;

// ReSharper disable MemberCanBePrivate.Global
namespace Simple
{
    public class TagsAddCmd : BaseCommand
    {
        public TagsAddCmd() : base("a", "add", new[] {"create", "new"}, "", "", "")
        {
            Action = (worker, remainArgs) => { Console.WriteLine("[HIT] add a tag."); };
        }
    }

    public class TagsRemoveCmd : BaseCommand
    {
        public TagsRemoveCmd() : base("rm", "remove", new[] {"delete", "del", "erase"}, "", "", "")
        {
            Action = (worker, remainArgs) => { Console.WriteLine("[HIT] remove a tag."); };
        }
    }

    public class TagsListCmd : BaseCommand
    {
        public TagsListCmd() : base("l", "list", new[] {"ls", "lst"}, "", "", "")
        {
            Action = (worker, remainArgs) => { Console.WriteLine("[HIT] list all tags."); };
        }
    }

    //[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsModifyCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public TagsModifyCmd() : base("m", "modify", new[] {"mod", "modi", "update"}, "", "", "")
        {
            Action = (worker, remainArgs) => { Console.WriteLine("[HIT] modify a tag."); };

            // adds flags here
            AddFlag(new Flag<string> {Long = "name", DefaultValue = "test-redis"});
            AddFlag(new Flag<string[]>
            {
                DefaultValue = new string[] { },
                Long = "add",
                Description = "",
                PlaceHolder = "LIST",
                Examples = "",
                Aliases = new[] {"a", "add-list"},
            });
            AddFlag(new Flag<string[]>
            {
                DefaultValue = new string[] { },
                Long = "remove",
                Description = "",
                PlaceHolder = "LIST",
                Examples = "",
                Aliases = new[] {"r", "rm", "del", "rm-list"},
            });

            // adds sub-commands here
        }

        // public TagsModifyCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }
}