#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr;
using Xunit;
using Xunit.Abstractions;

namespace Tests.HzNS.Cmdr
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class WorkerTest
    {
        // https://xunit.github.io/docs/capturing-output.html

        #region XUnit: capture the output

        private readonly ITestOutputHelper output;

        public WorkerTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        #endregion

        [Fact]
        public void Test1()
        {
            foreach (var ti in new[]
            {
                new testItem
                {
                    args = "tags mode s1 s2 --address consul.local -h", ok = true, expected = (w, args) =>
                    {
                        // ReSharper disable once ConvertToLambdaExpression
                        return w.ParsedCommand.IsEqual("s2");
                    }
                },

                new testItem {args = "--help", ok = true, expected = (w, args) => true},
                new testItem {args = "-h", ok = true, expected = (w, args) => true},
                new testItem {args = "-?", ok = true, expected = (w, args) => true},
                new testItem {args = "--info", ok = true, expected = (w, args) => true},
                new testItem {args = "--usage", ok = true, expected = (w, args) => true},

                new testItem {args = "--debug", ok = true, expected = (w, args) => true},
                new testItem {args = "-D", ok = true, expected = (w, args) => true},
                new testItem {args = "--verbose", ok = true, expected = (w, args) => true},
                new testItem {args = "-v", ok = true, expected = (w, args) => true},

                new testItem {args = "--versions", ok = true, expected = (w, args) => true},
                new testItem {args = "--version", ok = true, expected = (w, args) => true},
                new testItem {args = "--ver", ok = true, expected = (w, args) => true},
                new testItem {args = "-V", ok = true, expected = (w, args) => true},
                new testItem {args = "versions", ok = true, expected = (w, args) => true},
                new testItem {args = "version", ok = true, expected = (w, args) => true},
                new testItem {args = "ver", ok = true, expected = (w, args) => true},

                new testItem {args = "-#", ok = true, expected = (w, args) => true},
                new testItem {args = "--build-info", ok = true, expected = (w, args) => true},

                new testItem {args = "--tree", ok = true, expected = (w, args) => true},
                new testItem {args = "~~debug", ok = true, expected = (w, args) => true},

                new testItem {args = "tags mode s1 s2 -h", ok = true, expected = (w, args) => true},
                new testItem {args = "tags mode s1 s2 -D -k -s2 -h", ok = true, expected = (w, args) => true},
                new testItem {args = "tags mode s1 s2 -Dks2 -h", ok = true, expected = (w, args) => true},
                new testItem
                    {args = "tags mode s1 s2 -Dk -2mpgtcid test-ok -h jit", ok = true, expected = (w, args) => true},
                new testItem
                {
                    args = "tags mode s1 s2 --name test-redis -a consul.local -h", ok = true,
                    expected = (w, args) => true
                },
                new testItem
                {
                    args = "tags mode s1 s2 --address consul.local -h", ok = true, expected = (w, args) =>
                    {
                        // ReSharper disable once ConvertToLambdaExpression
                        return w.ParsedCommand.IsEqual("s2");
                    }
                },
            })
            {
                TestLarge(ti.args, ti.ok, ti.expected);
            }
        }

        //[Theory]
        //[InlineData("--help", true, (w, args) => true)]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void TestLarge(string inputArgs, bool ok, Func<Worker, string, bool>? expected)
        {
            output.WriteLine($"- test for: {inputArgs} ...");
            var w = global::HzNS.Cmdr.Cmdr.NewWorker(root.RootCmd);
            w.Run(inputArgs.Split(" "));
            Assert.Equal(ok, w.Parsed);
            Assert.True(expected?.Invoke(w, inputArgs));
            output.WriteLine($"  DONE");
        }

        private class testItem
        {
            // ReSharper disable once UnusedMember.Local
            public string group { get; set; } = "default";
            public string args { get; set; } = "";
            public bool ok { get; set; } = true;
            public Func<Worker, string, bool>? expected { get; set; }
        }


        // private TestContext testContextInstance;
        //
        // /// <summary>
        // ///  Gets or sets the test context which provides
        // ///  information about and functionality for the current test run.
        // ///</summary>
        // [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
        // public TestContext TestContext
        // {
        //     get { return testContextInstance; }
        //     set { testContextInstance = value; }
        // }
    }
}