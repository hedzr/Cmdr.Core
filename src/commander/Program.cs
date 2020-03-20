using System.Diagnostics.CodeAnalysis;
using commander.Cmd;
using HzNS.Cmdr;
using HzNS.Cmdr.Base;
using Serilog;

namespace commander
{
    /// <summary>
    ///
    /// ll
    /// </summary>
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "ArrangeTypeModifiers")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    class Program
    {
        static void Main(string[] args)
        {
            // Cmdr: A CommandLine Arguments Parser
            Cmdr.NewWorker(RootCommand.New(new AppInfo {AppName = "commander", AppVersion = "1.0.0"}, (root) =>
                    {
                        root.AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                            .AddCommand(new TagsAddCmd())
                            .AddCommand(new TagsRemoveCmd())
                            // .AddCommand(new TagsAddCmd { }) // dup-test
                            .AddCommand(new TagsListCmd())
                            .AddCommand(new TagsModifyCmd())
                        );
                    }), // <- RootCmd
                    // Options ->
                    (w) =>
                    {
                        //
                        // w.UseSerilog((configuration) => configuration.WriteTo.Console().CreateLogger())
                        //
                        
                        // w.EnableDuplicatedCharThrows = true;
                    })
                .Run(args);

            // HzNS.MdxLib.Core.Open("*.mdx,mdd,sdx,wav,png,...") => mdxfile
            // mdxfile.Preload()
            // mdxfile.GetEntry("beta") => entryInfo.{item,index}
            // mdxfile.Find("a")           // "a", "a*b", "*b"
            // mdxfile.Close()
            // mdxfile.Find()
            // mdxfile.Find()
            // mdxfile.Find()

            // Log.CloseAndFlush();
            // Console.ReadKey();
        }

        // ReSharper disable once UnusedType.Local
        private class Example
        {
            // ReSharper disable once ArrangeTypeMemberModifiers
            readonly ILogger _log = Log.ForContext<Example>();

            // ReSharper disable once UnusedMember.Local
            public void Show()
            {
                _log.Information("Hello!");
            }
        }
    }
}