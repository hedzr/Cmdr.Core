using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Autofac;
using AutofacSerilogIntegration;
using HzNS.Cmdr;
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
    class Program
    {
        static void MainX(string[] args)
        {
            foreach (var item in FilterWithoutYield())
            {
                Console.WriteLine(item); //3，4，5
            }

            //可以用ToList()从IEnumerable<out T>创建一个List<T>,并获得长度3
            Console.WriteLine(FilterWithoutYield().ToList().Count());
            Console.ReadLine();
        }

        static List<int> Data()
        {
            return new List<int> {1, 2, 3, 4, 5};
        }

        //这种传统方式需要额外创建一个List<int> 增加开销，而且需要把Data()全部加载到内存才能再遍历。
        static IEnumerable<int> FilterWithoutYield()
        {
            List<int> result = new List<int>();
            foreach (int i in Data())
            {
                if (i > 2)
                    result.Add(i);
            }

            return result;
        }

        static IEnumerable<int> FilterWithYield()
        {
            foreach (int i in Data())
            {
                if (i > 2)
                    yield return i;
            }

            yield break; // 迭代器代码使用yield return 语句依次返回每个元素，yield break将终止迭代。
        }


        static void Main(string[] args)
        {
            // using var log = new LoggerConfiguration()
            //     .MinimumLevel.Debug()
            //     .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //     .Enrich.FromLogContext()
            //     .WriteTo.File(Path.Combine("logs", @"access.log"), rollingInterval: RollingInterval.Day)
            //     .WriteTo.Console()
            //     .CreateLogger();
            // log.Information("Hello, Serilog!");
            // log.Warning("Goodbye, Serilog.");

            SingletonTest(args);

            //
            var worker = Entry.NewCmdrWorker(args);

            worker.From(new RootCmd());
            worker.Run(args);
            
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