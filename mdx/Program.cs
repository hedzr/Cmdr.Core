using System;
using System.Diagnostics;
using System.Threading;
using HzNS.Cmdr;
using HzNS.Cmdr.Tool;
using HzNS.MdxLib.MDict;
using Serilog;

namespace mdx
{
    /// <summary>
    ///
    /// ll
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            using var log = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            // log.Information("Hello, Serilog!");
            // log.Warning("Goodbye, Serilog.");

            SingletonTest(args);

            //
            Entry.Try(args);

            Console.WriteLine("\nHello World!");

            foreach (var filename in args)
            {
                if (string.IsNullOrEmpty(filename)) continue;

                using var l = new MDictLoader(filename);
                try
                {
                    l.Process();
                    Console.WriteLine($"header: {l.DictHeader}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    // throw;
                }
                finally
                {
                    Worker.Instance.Parsed++;
                    log.Information($"#{Worker.Instance.Parsed} parsed.");

                    // l.Dispose();
                }
            }

            if (Worker.Instance.Parsed == 0)
                log.Warning("Nothing to parsed.");

            // HzNS.MdxLib.Core.Open("*.mdx,mdd,sdx,wav,png,...") => mdxfile
            // mdxfile.Preload()
            // mdxfile.GetEntry("beta") => entryInfo.{item,index}
            // mdxfile.Find("a")           // "a", "a*b", "*b"
            // mdxfile.Close()
            // mdxfile.Find()
            // mdxfile.Find()
            // mdxfile.Find()
        }

        private static void SingletonTest(string[] args)
        {
            // The client code.

            Console.WriteLine(
                "{0}\n{1}\n\n{2}\n",
                "If you see the same value, then singleton was reused (yay!)",
                "If you see different values, then 2 singletons were created (booo!!)",
                "RESULT:"
            );

            var process1 = new Thread(() => { TestSingleton("FOO"); });
            var process2 = new Thread(() => { TestSingleton("BAR"); });

            process1.Start();
            process2.Start();

            process1.Join();
            process2.Join();
        }

        private static void TestSingleton(string value)
        {
            var singleton = Singleton.GetInstance(value);
            Console.WriteLine(singleton.Value);
        }
    }
}