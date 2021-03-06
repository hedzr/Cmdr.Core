using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using Simple;

namespace Tests.HzNS.Cmdr
{
    // ReSharper disable once InconsistentNaming
    [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
    public abstract class root
    {
        public static IRootCommand RootCmd => RootCommand.New(
            new AppInfo {AppName = "tag-tool"},
            (root) =>
            {
                root.AddCommand(new Command
                    {
                        Short = "dz", Long = "dz", Description = "test divide by zero",
                        Action = (worker, opt, remainArgs) => { Console.WriteLine($"{B / _a}"); },
                    })
                    .AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                        .AddCommand(new TagsAddCmd())
                        .AddCommand(new TagsRemoveCmd())
                        // .AddCommand(new TagsAddCmd { }) // dup-test
                        .AddCommand(new TagsListCmd())
                        .AddCommand(new TagsModifyCmd())
                        .AddCommand(new TagsModeCmd())
                        .AddCommand(new TagsToggleCmd())
                        .AddFlag(new Flag<string>
                        {
                            DefaultValue = "consul.ops.local",
                            Long = "addr", Short = "a", Aliases = new[] {"address", "host"},
                            Description = "Consul IP/Host and/or Port: HOST[:PORT] (No leading 'http(s)://')",
                            PlaceHolder = "HOST[:PORT]",
                            Group = "Consul",
                        })
                        .AddFlag(new Flag<string>
                        {
                            DefaultValue = "",
                            // ReSharper disable once StringLiteralTypo
                            Long = "cacert", Short = "", Aliases = new string[] {"ca-cert"},
                            Description = "Consul Client CA cert)",
                            PlaceHolder = "FILE",
                            Group = "Consul",
                        })
                        .AddFlag(new Flag<string>
                        {
                            DefaultValue = "",
                            Long = "cert", Short = "", Aliases = new string[] { },
                            Description = "Consul Client Cert)",
                            PlaceHolder = "FILE",
                            Group = "Consul",
                        })
                        .AddFlag(new Flag<bool>
                        {
                            DefaultValue = false,
                            Long = "insecure", Short = "k", Aliases = new string[] { },
                            Description = "Ignore TLS host verification",
                            Group = "Consul",
                        })
                    )
                    .AddCommand(new Command
                    {
                        Short = "zm", Long = "zm", Description = "test mazy",
                        Action = (worker, opt, remainArgs) => { Console.WriteLine($"{B / _a}"); },
                    });

                root.AddCommand(new ServerCmd());

                _a = 0;
            }
        );

        private static int _a = 9;
        private const int B = 10;
    }
}