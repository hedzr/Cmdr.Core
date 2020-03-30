#nullable enable
using System;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Tool.ObjectCloner;
using YamlDotNet.Serialization;

// ReSharper disable MemberCanBePrivate.Global

namespace commander.Cmd
{
   public class TagsAddCmd : BaseCommand
    {
        public TagsAddCmd() : base("a", "add", new[] {"create", "new"}, "add tags to a service", "", "")
        {
            Action = (worker, sender, remainArgs) => { Console.WriteLine("[HIT] add a tag."); };
        }
    }

    public class TagsRemoveCmd : BaseCommand
    {
        public TagsRemoveCmd() : base("rm", "remove", new[] {"delete", "del", "erase"}, "remove tags from a service",
            "", "")
        {
            Action = (worker, sender, remainArgs) => { Console.WriteLine("[HIT] remove a tag."); };
        }
    }

    public class TagsListCmd : BaseCommand
    {
        public TagsListCmd() : base("l", "list", new[] {"ls", "lst"}, "list tags of a service", "", "")
        {
            Action = (worker, sender, remainArgs) => { Console.WriteLine("[HIT] list all tags."); };
        }
    }

    public class TagsToggleCmd : BaseCommand
    {
        public TagsToggleCmd() : base("tx", "toggle", new[] {"tog", "flip"}, "toggle tags for a service", "", "")
        {
            Action = (worker, sender, remainArgs) => { Console.WriteLine("[HIT] toggle a tag."); };
        }
    }

    //[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsModifyCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public TagsModifyCmd() : base("m", "modify", new[] {"mod", "modi", "update"}, "modify tags for a service", "",
            "")
        {
            Action = (worker, sender, remainArgs) =>
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

    //[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    public class TagsModeCmd : BaseCommand
    {
        // ReSharper disable once StringLiteralTypo
        public TagsModeCmd() : base("md", "mode", new string[] { }, "mode settings", "", "")
        {
            Action = (worker, sender, remainArgs) =>
            {
                Console.WriteLine($"[HIT] mode settings. remains: '{string.Join(",", remainArgs)}'");
            };

            PostAction = (worker, sender, remainArgs) =>
            {
                Console.WriteLine($"[POSTACTION] [HIT] mode settings. remains: '{string.Join(",", remainArgs)}'");
            };


            // adds flags here

            #region flags

            AddFlag(new Flag<string>
                {DefaultValue = "test-redis", Long = "id", Short = "id", Description = "service id"});
            AddFlag(new Flag<string> {DefaultValue = "test-redis", Long = "name",});
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

            #endregion

            // adds sub-commands here

            var s2 = new Command {Short = "s2", Long = "sub2", Description = "sub-command 2 operations"}
                .AddFlag(new Flag<bool>
                {
                    DefaultValue = false, Long = "clear2", Short = "c2", Group = "Operate",
                    Description = "clear all tags."
                })
                .AddFlag(new Flag<bool>
                {
                    DefaultValue = false, Long = "such2", Short = "s2", Aliases = new[] {"such-a"},
                    ToggleGroup = "Mode",
                    Description = "such a bit."
                }, true)
                .AddFlag(new Flag<int>
                {
                    DefaultValue = 0, Long = "retry2", Short = "t2", Aliases = new[] {"retry-times"},
                    ToggleGroup = "Mode",
                    Description = "dify with the `key`."
                })
                .AddAction((worker, opt, args) =>
                {
                    var map = worker.OptionsStore.GetAsMap("tags.mode");

                    // worker.log.Information("tag.mode => {OptionsMap}", map);

                    {
                        var serializer = new SerializerBuilder().Build();
                        var yaml = serializer.Serialize(map);
                        Console.WriteLine(yaml);
                    }
                    // {
                    //     var m = worker.OptionsStore.FindByDottedKey("tags.mode");
                    //     var serializer = new SerializerBuilder().Build();
                    //     var yaml = serializer.Serialize(m);
                    //     Console.WriteLine(yaml);
                    // }
                });

            AddCommand(new Command {Short = "s1", Long = "sub1", Description = "sub-command 1 operations"}
                .AddFlag(new Flag<bool>
                {
                    DefaultValue = false, Long = "clear", Short = "c", Group = "Operate",
                    Description = "clear all tags."
                })
                .AddFlag(new Flag<bool>
                {
                    DefaultValue = false, Long = "such", Short = "s", Aliases = new[] {"such-a"},
                    ToggleGroup = "Mode",
                    Description =
                        "In 'Tag Mode', a tag be treated as `key=value` or `key:value`, and modify with the `key`."
                })
                .AddFlag(new Flag<int>
                {
                    DefaultValue = 0, Long = "retry", Short = "t", Aliases = new[] {"retry-times"},
                    ToggleGroup = "Mode",
                    Description = "dify with the `key`."
                })
                .AddFlag(new Flag<string>
                {
                    DefaultValue = "api.github.com", Long = "addr", Short = "", Aliases = new[] {"address"},
                    PlaceHolder = "HOST[:PORT]",
                    ToggleGroup = "Server",
                    Description = "network address of the remote server."
                })
                .AddCommand(s2.DeepClone()));

            AddCommand(s2);
        }

        // public TagsModifyCmd(string shortTitle, string longTitle, string[] aliases, string description,
        //     string descriptionLong, string examples) : base(shortTitle, longTitle, aliases, description,
        //     descriptionLong, examples)
        // {
        // }
    }
}