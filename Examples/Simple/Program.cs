using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;

namespace Simple
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ArrangeTypeModifiers
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    class Program
    {
        static void Main(string[] args)
        {
            // Console.WriteLine("Hello World!");

            // Cmdr: A CommandLine Arguments Parser
            Cmdr.NewWorker(RootCmd.New(
                        new AppInfo {AppName = "tag-tool", AppVersion = "1.0.0"},
                        (root) =>
                        {
                            root.AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                                .AddCommand(new TagsAddCmd())
                                .AddCommand(new TagsRemoveCmd())
                                // .AddCommand(new TagsAddCmd { }) // dup-test
                                .AddCommand(new TagsListCmd())
                                .AddCommand(new TagsModifyCmd())
                            );
                        }
                    ), // <- RootCmd

                    // Options ->
                    (w) =>
                    {
                        //
                        // w.UseSerilog((configuration) => configuration.WriteTo.Console().CreateLogger())
                        //

                        // w.EnableDuplicatedCharThrows = true;

                        // w.EnableEmptyLongFieldThrows = true;
                    })
                .Run(args);

            // Log.CloseAndFlush();
            // Console.ReadKey();
        }
    }
}