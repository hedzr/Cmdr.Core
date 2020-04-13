#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Tool.Ext;
using cmdr = HzNS.Cmdr;
using Xunit;
using Xunit.Abstractions;

namespace Tests.HzNS.Cmdr
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class WorkerTest : TestBase
    {
        public WorkerTest(ITestOutputHelper output) : base(output)
        {
        }

        private static cmdr.Store ss => cmdr.Cmdr.Instance.Store;

        [Fact]
        public void Test1()
        {
            var idx = 0;
            foreach (var ti in new[]
            {
                new testItem
                {
                    args = "tags mode s1 s2 -Dk -2mpgcid test-ok -h jit runt boche", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal(3, w.RemainsArgs.Length);
                        Assert.Equal("jit", w.RemainsArgs[0]);
                        Assert.Equal("boche", w.RemainsArgs[2]);
                        Assert.Equal("tags mode sub1 sub2", w.ParsedCommand?.backtraceTitles);
                        Assert.Equal("test-ok", ss.GetAs<string>("tags.mode.id"));
                        //Assert.Equal(31, ss.GetAs<int>("tags.mode.sub1.retry"));

                        Assert.False(ss.GetAs<bool>("tags.mode.both"));
                        Assert.False(ss.GetAs<bool>("tags.mode.meta"));
                        Assert.False(ss.GetAs<bool>("tags.mode.plain"));
                        Assert.True(ss.GetAs<bool>("tags.mode.string"));

                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.clear"));
                        Assert.True(ss.GetAs<bool>("tags.insecure"));
                        Assert.True(ss.GetAs<bool>("debug"));
                        return true;
                    }
                },

                new testItem
                {
                    args = "tags mode s1 s2 -Dk -t271 -t371 -k- -t=45 --retry 31 -h jit", ok = true, expected =
                        (w, args) =>
                        {
                            Assert.Single(w.RemainsArgs);
                            Assert.Equal("jit", w.RemainsArgs[0]);
                            Assert.Equal("tags mode sub1 sub2", w.ParsedCommand?.backtraceTitles);
                            Assert.Equal(71, ss.GetAs<int>("tags.mode.sub1.sub2.retry2"));
                            Assert.Equal(31, ss.GetAs<int>("tags.mode.sub1.retry"));
                            Assert.False(ss.GetAs<bool>("tags.insecure"));
                            Assert.True(ss.GetAs<bool>("debug"));
                            return true;
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

                #region sub-commands matching

                new testItem
                {
                    args = "server -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("server", w.ParsedCommand?.backtraceTitles);
                        return true;
                    }
                },
                new testItem
                {
                    args = "s s -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("server start", w.ParsedCommand?.backtraceTitles);
                        return true;
                    }
                },
                new testItem
                {
                    args = "s run", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("server start", w.ParsedCommand?.backtraceTitles);
                        Assert.Equal("run", w.ParsedCommand?.HitTitle);
                        return true;
                    }
                },
                new testItem
                {
                    args = "s reload --port 8989 -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("server reload", w.ParsedCommand?.backtraceTitles);
                        Assert.Equal(8989, ss.GetAs<int>("server.port"));
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("tags", w.ParsedCommand?.backtraceTitles);
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags mode -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("tags mode", w.ParsedCommand?.backtraceTitles);
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags mode s1 -h", ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("tags mode sub1", w.ParsedCommand?.backtraceTitles);
                        return true;
                    }
                },

                #endregion

                new testItem {args = "tags mode s1 s2 -h", ok = true, expected = (w, args) => true},
                new testItem
                {
                    args = "tags mode s1 s2 -D -k -s2 -h", ok = true, expected = (w, args) =>
                    {
                        Assert.True(ss.GetAs<bool>("tags.insecure"));
                        Assert.True(ss.GetAs<bool>("debug"));
                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.sub2.such2"));
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags mode s1 s2 -Dks2 -h", ok = true, expected = (w, args) =>
                    {
                        Assert.True(ss.GetAs<bool>("tags.insecure"));
                        Assert.True(ss.GetAs<bool>("debug"));
                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.sub2.such2"));
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags mode s1 s2 -Dk -2mpgcid test-ok -h jit", ok = true, expected = (w, args) =>
                    {
                        Assert.True(ss.GetAs<bool>("tags.insecure"));
                        Assert.True(ss.GetAs<bool>("debug"));

                        Assert.Equal("test-ok", ss.GetAs<string>("tags.mode.id"));

                        Assert.False(ss.GetAs<bool>("tags.mode.both"));
                        Assert.False(ss.GetAs<bool>("tags.mode.meta"));
                        Assert.False(ss.GetAs<bool>("tags.mode.plain"));
                        Assert.True(ss.GetAs<bool>("tags.mode.string"));

                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.clear"));

                        return true;
                    }
                },

                new testItem
                {
                    args = "tags mode s1 s2 -Dk -2mpgcid test-ok -2- -h voip jit", ok = true,
                    expected = (w, args) =>
                    {
                        Assert.Equal("test-ok", ss.GetAs<string>("tags.mode.id"));

                        Assert.False(ss.GetAs<bool>("tags.mode.both"));
                        Assert.False(ss.GetAs<bool>("tags.mode.meta"));
                        Assert.False(ss.GetAs<bool>("tags.mode.plain"));
                        Assert.True(ss.GetAs<bool>("tags.mode.string"));

                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.clear"));
                        return true;
                    }
                },
                new testItem
                {
                    args =
                        "tags mode s1 s2 -Dk -2mpgcid test-ok -2-  -vvv --host cash.io.local --add 8,9,k -c2- --add 34 -h voip jit",
                    ok = true, expected = (w, args) =>
                    {
                        Assert.Equal("test-ok", ss.GetAs<string>("tags.mode.id"));
                        Assert.Equal("cash.io.local", ss.GetAs<string>("tags.addr"));
                        Assert.Equal(new[] {"8", "9", "k", "34"},
                            ss.GetAs<string[]>("tags.mode.add"));

                        Assert.False(ss.GetAs<bool>("tags.mode.both"));
                        Assert.False(ss.GetAs<bool>("tags.mode.meta"));
                        Assert.False(ss.GetAs<bool>("tags.mode.plain"));
                        Assert.True(ss.GetAs<bool>("tags.mode.string"));

                        Assert.True(ss.GetAs<bool>("tags.mode.sub1.clear"));

                        Assert.True(ss.GetAs<bool>("verbose"));
                        Assert.Equal(3, w.FindFlag("verbose")?.HitCount);
                        return true;
                    }
                },
                new testItem
                {
                    args =
                        "tags mode s1 s2 -Dk -2mpgcid test-ok -2-  -vvv --addr cash.io.local --add 8,9,k -c2- --add  -h voip jit",
                    ok = true, expected = (w, args) => true
                },
                new testItem
                {
                    args = "tags mode s1 s2 --name 1-test-redis --host 1.consul.local -h", ok = true,
                    expected = (w, args) =>
                    {
                        Assert.Equal("1-test-redis", ss.GetAs<string>("tags.mode.name"));
                        Assert.Equal("1.consul.local", ss.GetAs<string>("tags.addr"));
                        return true;
                    }
                },
                new testItem
                {
                    args = "tags mode s1 s2 --address 1.consul.local -h", ok = true, expected = (w, args) =>
                    {
                        /*
                         * IMPORTANT:
                         *
                         * In Greedy mode, `cmdr` will find the longest flags as matched result in
                         * current sub-command;
                         * And she will solve the possible result in the parent command, up to the root.
                         *
                         * The 'longest' matched one, i.e., the 'greedy' mode, will take the effect just
                         * in the flags of each one command.
                         *
                         * TODO: I'll change this behavior so that we can find out the 'real' longest one across the current command and its all parents.
                         * 
                         */
                        Output.WriteLine($"add: {ss.GetAs<string[]>("tags.mode.add").ToStringEx()}");
                        Assert.Equal(new[] {"8", "9", "k", "34", "r", "8", "9", "k", "ress"},
                            ss.GetAs<string[]>("tags.mode.add"));
                        // ReSharper disable once ConvertToLambdaExpression
                        return w.ParsedCommand?.IsEqual("s2") ?? false;
                    }
                },
            })
            {
                TestLarge(idx, ti.args, ti.ok, ti.expected);
                idx++;
            }
        }

        //[Theory]
        //[InlineData("--help", true, (w, args) => true)]
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void TestLarge(int idx, string inputArgs, bool ok, Func<cmdr.Worker, string, bool>? expected)
        {
            Output.WriteLine($"- test #{idx} for: {inputArgs} ...");
            var w = global::HzNS.Cmdr.Cmdr.NewWorker(root.RootCmd);
            w.Run(inputArgs.Split(" "));
            Assert.Equal(ok, w.Parsed);
            Assert.True(expected?.Invoke(w, inputArgs));
            Output.WriteLine($"  DONE");
        }

        private class testItem
        {
            // ReSharper disable once UnusedMember.Local
            public string group { get; set; } = "default";
            public string args { get; set; } = "";
            public bool ok { get; set; } = true;
            public Func<cmdr.Worker, string, bool>? expected { get; set; }
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