using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Schema;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr;
using HzNS.Cmdr.Builder;
using HzNS.Cmdr.Tool;
using HzNS.MdxLib.MDict;
using mdx.Cmd;
using Serilog;
using Serilog.Events;

namespace mdx
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
            Entry.NewCmdrWorker(RootCmd.New(new AppInfo {AppName = "mdxTool", AppVersion = "1.0.0"}, (root) =>
                    {
                        root.AddCommand(new Command {Short = "t", Long = "tags", Description = "tags operations"}
                            .AddCommand(new TagsAddCmd { })
                            .AddCommand(new TagsRemoveCmd { })
                            // .AddCommand(new TagsAddCmd { }) // dup-test
                            .AddCommand(new TagsListCmd { })
                            .AddCommand(new TagsModifyCmd { })
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

        private class Example
        {
            // ReSharper disable once ArrangeTypeMemberModifiers
            readonly ILogger _log = Log.ForContext<Example>();

            public void Show()
            {
                _log.Information("Hello!");
            }
        }
    }
}