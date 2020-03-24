using System;
using HzNS.Cmdr.Tool.Ext;
using Xunit;
using Xunit.Abstractions;

namespace Tests.HzNS.Tool.Ext
{
    public class DateTimeTests : TestBase
    {
        public DateTimeTests(ITestOutputHelper output) : base(output)
        {
        }

        [Theory]
        [InlineData(-1, "one second ago")]
        [InlineData(-5, "5 seconds ago")]
        [InlineData(-59, "59 seconds ago")]
        [InlineData(-60, "a minute ago")]
        [InlineData(-119, "a minute ago")]
        [InlineData(-2 * 60, "2 minutes ago")]
        [InlineData(-7 * 60, "7 minutes ago")]
        [InlineData(-45 * 60 + 1, "44 minutes ago")]
        [InlineData(-45 * 60, "an hour ago")]
        [InlineData(-1 * 60 * 60, "an hour ago")]
        [InlineData(-90 * 60 + 1, "an hour ago")]
        [InlineData(-90 * 60, "2 hours ago")]
        [InlineData(-24 * 60 * 60 + 1, "23 hours ago")]
        [InlineData(-24 * 60 * 60, "yesterday")]
        [InlineData(-48 * 60 * 60 + 1, "yesterday")]
        [InlineData(-2 * 24 * 60 * 60, "2 days ago")]
        [InlineData(-30 * 24 * 60 * 60 + 1, "29 days ago")]
        [InlineData(-30 * 24 * 60 * 60, "one month ago")]
        [InlineData(-2 * 30 * 24 * 60 * 60, "2 months ago")]
        [InlineData(-12 * 30 * 24 * 60 * 60 + 1, "11 months ago")]
        [InlineData(-12 * 30 * 24 * 60 * 60, "one year ago")]
        [InlineData(-365 * 24 * 60 * 60, "one year ago")]
        [InlineData(-2 * 365 * 24 * 60 * 60, "2 years ago")]
        public void DateTimeTest1(int offset, string expectResult)
        {
            var now = DateTime.UtcNow;
            var dt = now.AddSeconds(offset);
            var s = dt.ToUtcRelativeDateString();
            Assert.Equal(expectResult, s);
            Output.WriteLine(s);
        }

        [Theory]
        [InlineData(-1, "one second ago")]
        [InlineData(-5, "5 seconds ago")]
        [InlineData(-59, "59 seconds ago")]
        [InlineData(-60, "a minute ago")]
        [InlineData(-119, "a minute ago")]
        [InlineData(-2 * 60, "2 minutes ago")]
        [InlineData(-7 * 60, "7 minutes ago")]
        [InlineData(-45 * 60 + 1, "44 minutes ago")]
        [InlineData(-45 * 60, "an hour ago")]
        [InlineData(-1 * 60 * 60, "an hour ago")]
        [InlineData(-90 * 60 + 1, "an hour ago")]
        [InlineData(-90 * 60, "2 hours ago")]
        [InlineData(-24 * 60 * 60 + 1, "23 hours ago")]
        [InlineData(-24 * 60 * 60, "yesterday")]
        [InlineData(-48 * 60 * 60 + 1, "yesterday")]
        [InlineData(-2 * 24 * 60 * 60, "2 days ago")]
        [InlineData(-30 * 24 * 60 * 60 + 1, "29 days ago")]
        [InlineData(-30 * 24 * 60 * 60, "one month ago")]
        [InlineData(-2 * 30 * 24 * 60 * 60, "2 months ago")]
        [InlineData(-12 * 30 * 24 * 60 * 60 + 1, "11 months ago")]
        [InlineData(-12 * 30 * 24 * 60 * 60, "one year ago")]
        [InlineData(-365 * 24 * 60 * 60, "one year ago")]
        [InlineData(-2 * 365 * 24 * 60 * 60, "2 years ago")]
        public void DateTimeTest2(int offset, string expectResult)
        {
            var now = DateTime.Now;
            var dt = now.AddSeconds(offset);
            var s = dt.ToRelativeDateString();
            Assert.Equal(expectResult, s);
            Output.WriteLine(s);
        }

        [Theory]
        [InlineData("8s", 8*1000)]
        [InlineData("89m", 89*60*1000)]
        [InlineData("3m", 3*60*1000)]
        [InlineData("399m", 399*60*1000)]
        [InlineData("2h3m", (2*3600+3*60)*1000)]
        [InlineData("2h399m", (2*3600+399*60)*1000)]
        [InlineData("3d2h3m", ((3*24+2)*3600+3*60)*1000)]
        [InlineData("3d2h399m", ((3*24+2)*3600+399*60)*1000)]
        [InlineData("3d3m8s", ((3*24)*3600+3*60+8)*1000)]
        [InlineData("3d399m89s", ((3*24)*3600+399*60+89)*1000)]
        [InlineData("3d8s9ms", ((3*24)*3600+8)*1000+9)]
        [InlineData("3d89s139ms", ((3*24)*3600+89)*1000+139)]
        [InlineData("3d89s5139ms", ((3*24)*3600+89)*1000+5139)]
        public void DateTimeTest_FromMoment(string s, long expectResultMs)
        {
            var ticks = expectResultMs * 10_000;
            var expect = new TimeSpan(ticks);
            
            var ts = new TimeSpan().FromMoment(s);
            Assert.Equal(expect, ts);
            // ts = new TimeSpan(ts.Ticks);
            Output.WriteLine($"{s}: {ts.ToRelativeDateString()}, {ts.ToMoment()}");
        }
    }
}