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
            Action = (worker, remainArgs) =>
            {
                Console.WriteLine($"[HIT] modify a tag. remains: '{string.Join(",", remainArgs)}'");
            };

            // adds flags here

            AddFlag(new Flag<string> {Long = "name", DefaultValue = "test-redis"});
            AddFlag(new Flag<string[]>
            {
                DefaultValue = new string[] { },
                Long = "add", Short = "a", Aliases = new[] {"add-list"},
                Group = "List",
                Description = "a comma list to be added.",
                PlaceHolder = "LIST",
                Examples = "",
            });
            AddFlag(new Flag<string[]>
            {
                DefaultValue = new string[] { },
                Long = "remove", Short = "r", Aliases = new[] {"rm", "del", "rm-list"},
                Group = "List",
                Description = "a comma list to be removed.",
                PlaceHolder = "LIST",
                Examples = "",
            });
            AddFlag(new Flag<char>
            {
                DefaultValue = '=',
                Long = "delimiter", Short = "d",
                Description = "delimiter char in `non-plain` mode.",
            });

            AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "both", Short = "2", Aliases = new[] {"both-mode"},
                ToggleGroup = "Mode",
                Description = "In 'Both Mode', both of 'NodeMeta' and 'Tags' field will be updated."
            });
            AddFlag(new Flag<bool>
            {
                DefaultValue = true, Long = "meta", Short = "m", Aliases = new[] {"meta-mode"},
                ToggleGroup = "Mode",
                Description =
                    "In 'Meta Mode', service 'NodeMeta' field will be updated instead of 'Tags'. (--plain assumed false)."
            });
            AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "plain", Short = "p", Aliases = new[] {"plain-mode"},
                ToggleGroup = "Mode",
                Description =
                    "In 'Plain Mode', a tag be NOT treated as `key=value` or `key:value`, and modify with the `key`."
            });
            AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "string", Short = "g", Aliases = new[] {"string-mode"},
                ToggleGroup = "Mode",
                Description =
                    "In 'String Mode', default will be disabled: default, a tag string will be split by comma(,), and treated as a string list."
            });
            AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "tag", Short = "t", Aliases = new[] {"tag-mode"},
                ToggleGroup = "Mode",
                Description =
                    "In 'Tag Mode', a tag be treated as `key=value` or `key:value`, and modify with the `key`."
            });

            AddFlag(new Flag<bool>
            {
                DefaultValue = false, Long = "clear", Short = "c", Group = "Operate", Description = "clear all tags."
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