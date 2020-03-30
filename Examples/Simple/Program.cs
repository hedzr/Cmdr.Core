using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using HzNS.Cmdr.Internal.Base;
using HzNS.Cmdr.Tool.Ext;

namespace Simple
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ArrangeTypeModifiers
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    class Program
    {
        [SuppressMessage("ReSharper", "RedundantExplicitArrayCreation")]
        static int Main(string[] args) => Cmdr.NewWorker(

                #region RootCmd Definitions

                SimpleRootCmd.New(
                    new AppInfo
                    {
                        AppName = "tag-tool",
                        // AppVersion = "v1.0.0",
                        Author = "hedzr",
                        Copyright = "Copyright © Hedzr Studio, 2020. All Rights Reserved.",
                    },
                    (root) =>
                    {
                        root.Description = "description here";
                        root.DescriptionLong = "long description here";
                        root.Examples = "examples here";

                        // for "dz"
                        _a = 0;

                        root.AddCommand(new Command
                            {
                                Short = "dz", Long = "dz", Description = "test divide by zero",
                                Action = (worker, opt, remainArgs) => { Console.WriteLine($"{B / _a}"); },
                            })
                            .AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                                .AddCommand(new TagsAddCmd())
                                .AddCommand(new TagsRemoveCmd())
                                // .AddCommand(new TagsAddCmd { }) // for dup-test
                                .AddCommand(new TagsListCmd())
                                .AddCommand(new TagsModifyCmd())
                                .AddCommand(new TagsModeCmd())
                                .AddCommand(new TagsToggleCmd())
                                .AddFlag(new Flag<string>
                                {
                                    DefaultValue = "consul.ops.local",
                                    Long = "addr", Short = "a", Aliases = new[] {"address", "host"},
                                    Description =
                                        "Consul IP/Host and/or Port: HOST[:PORT] (No leading 'http(s)://')",
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
                            );

                        root.OnSet = (worker, flag, oldValue, newValue) =>
                        {
                            if (Cmdr.Instance.Store.GetAs<bool>("quiet")) return;
                            if (Cmdr.Instance.Store.GetAs<bool>("verbose") &&
                                flag.Root?.FindFlag("verbose")?.HitCount > 1)
                                Console.WriteLine(
                                    $"--> [{Cmdr.Instance.Store.GetAs<bool>("quiet")}][root.onSet] {flag} set: {oldValue?.ToStringEx()} -> {newValue?.ToStringEx()}");
                        };
                    }
                ), // <- RootCmd Definitions

                #endregion

                #region Options for Worker

                (w) =>
                {
                    //
                    // w.UseSerilog((configuration) => configuration.WriteTo.Console().CreateLogger())
                    //

                    w.EnableCmdrGreedyLongFlag = true;
                    // w.EnableDuplicatedCharThrows = true;
                    // w.EnableEmptyLongFieldThrows = true;

                    w.RegisterExternalConfigurationsLoader(ExternalConfigLoader);
                    
                    w.OnDuplicatedCommandChar = (worker, command, isShort, matchingArg) => false;
                    w.OnDuplicatedFlagChar = (worker, command, flag, isShort, matchingArg) => false;
                    w.OnCommandCannotMatched = (parsedCommand, matchingArg) => false;
                    w.OnFlagCannotMatched = (parsingCommand, fragment, isShort, matchingArg) => false;
                    w.OnSuggestingForCommand = (worker, dataset, token) => false;
                    w.OnSuggestingForFlag = (worker, dataset, token) => false;
                }

                #endregion

            )
            .Run(args, () =>
            {
                // Wait for the user to quit the program.

                // Console.WriteLine($"         AssemblyVersion: {VersionUtil.AssemblyVersion}");
                // Console.WriteLine($"             FileVersion: {VersionUtil.FileVersion}");
                // Console.WriteLine($"    InformationalVersion: {VersionUtil.InformationalVersion}");
                // Console.WriteLine($"AssemblyProductAttribute: {VersionUtil.AssemblyProductAttribute}");
                // Console.WriteLine($"      FileProductVersion: {VersionUtil.FileVersionInfo.ProductVersion}");
                // Console.WriteLine();
                
                // Console.WriteLine("Press 'q' to quit the sample.");
                // while (Console.Read() != 'q')
                // {
                //     //
                // }

                return 0;
            });

        private static void ExternalConfigLoader(IBaseWorker w, IRootCommand root)
        {
            // throw new NotImplementedException();
        }

        
        private static int _a = 9;
        private const int B = 10;
    }
}