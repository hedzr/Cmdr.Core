using System;
using System.Globalization;
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
        [InlineData("8s", 8 * 1000)]
        [InlineData("89m", 89 * 60 * 1000)]
        [InlineData("3m", 3 * 60 * 1000)]
        [InlineData("399m", 399 * 60 * 1000)]
        [InlineData("2h3m", (2 * 3600 + 3 * 60) * 1000)]
        [InlineData("2h399m", (2 * 3600 + 399 * 60) * 1000)]
        [InlineData("3d2h3m", ((3 * 24 + 2) * 3600 + 3 * 60) * 1000)]
        [InlineData("3d2h399m", ((3 * 24 + 2) * 3600 + 399 * 60) * 1000)]
        [InlineData("3d3m8s", ((3 * 24) * 3600 + 3 * 60 + 8) * 1000)]
        [InlineData("3d399m89s", ((3 * 24) * 3600 + 399 * 60 + 89) * 1000)]
        [InlineData("3d8s9ms", ((3 * 24) * 3600 + 8) * 1000 + 9)]
        [InlineData("3d89s139ms", ((3 * 24) * 3600 + 89) * 1000 + 139)]
        [InlineData("3d89s5139ms", ((3 * 24) * 3600 + 89) * 1000 + 5139)]
        public void DateTimeTest_FromMoment(string s, long expectResultMs)
        {
            var ticks = expectResultMs * 10_000;
            var expect = new TimeSpan(ticks);

            var ts = new TimeSpan().FromMoment(s);
            Assert.Equal(expect, ts);
            // ts = new TimeSpan(ts.Ticks);
            Output.WriteLine($"{s}: {ts.ToRelativeDateString()}, {ts.ToMoment()}");
        }

        [Fact]
        public void ListAllFormatPatterns()
        {
            Output.WriteLine("'d' standard format string:");
            // ReSharper disable once InvertIf
            if (DateTimeFormatInfo.CurrentInfo != null)
                foreach (var customString in DateTimeFormatInfo.CurrentInfo.GetAllDateTimePatterns('d'))
                    Output.WriteLine("   {0}", customString);
        }

        [Theory]
        [InlineData("8/23/1971", "c", "1971-08-23T00:00:00")]
        [InlineData("6/15/2009 13:45:30", "d", "2009-06-15T13:45:30")]
        [InlineData("Monday, June 15, 2009 13:45:30", "D", "2009-06-15T13:45:30")]
        [InlineData("2009-6-15 13:45:30", "d", "2009-06-15T13:45:30")]
        // ReSharper disable once xUnit1026
        public void DateTimeExTest_Parse(string s, string fmtStrDesc, string expectResult)
        {
            var expect = DateTime.Parse(expectResult);

            var dt = DateTimeEx.Parse(s);
            Assert.Equal(expect, dt);
            // ts = new TimeSpan(ts.Ticks);
            Output.WriteLine($"{s} [{fmtStrDesc}]: {dt:s}");
        }

        /// <summary>
        /// not yet.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="fmtStrDesc"></param>
        /// <param name="expectResult"></param>
        [Theory]
        [InlineData("8s", "c", 8 * 1000)]
        // ReSharper disable once xUnit1026
        public void DateTimeOffsetExTest_Parse(string s, string fmtStrDesc, long expectResult)
        {
            var ticks = expectResult * 10_000;
            var expect = new TimeSpan(ticks);

            var ts = new TimeSpan().FromMoment(s);
            Assert.Equal(expect, ts);
            // ts = new TimeSpan(ts.Ticks);
            Output.WriteLine($"{s} [{fmtStrDesc}]: {ts.ToRelativeDateString()}, {ts.ToMoment()}");
        }

        [Theory]
        [InlineData("8s", "moment:g", 8 * 1000)]
        [InlineData("89m", "moment:g", 89 * 60 * 1000)]
        [InlineData("3m", "moment:g", 3 * 60 * 1000)]
        [InlineData("399m", "moment:g", (399 * 60) * 1000)]
        [InlineData("2h399m", "moment:g", (2 * 3600 + 399 * 60) * 1000)]
        [InlineData("3d2h3m", "moment:g", ((3 * 24 + 2) * 3600 + 3 * 60) * 1000)]
        [InlineData("3d2h399m", "moment:g", ((3 * 24 + 2) * 3600 + 399 * 60) * 1000)]
        [InlineData("3d3m8s", "moment:g", ((3 * 24) * 3600 + 3 * 60 + 8) * 1000)]
        [InlineData("3d399m89s", "moment:g", ((3 * 24) * 3600 + 399 * 60 + 89) * 1000)]
        [InlineData("3d8s9ms", "moment:g", ((3 * 24) * 3600 + 8) * 1000 + 9)]
        [InlineData("3d89s139ms", "moment:g", ((3 * 24) * 3600 + 89) * 1000 + 139)]
        [InlineData("3d89s5139ms", "moment:g", ((3 * 24) * 3600 + 89) * 1000 + 5139)]
        [InlineData("17:14", "h\\:mm", (17 * 60 + 14) * 60 * 1000)]
        [InlineData("00:17:14", "h\\:mm\\:ss", (17 * 60 + 14) * 1000)]
        [InlineData("17:14:48", "g", ((17 * 60 + 14) * 60 + 48) * 1000)]
        [InlineData("17:14:48.153", @"h\:mm\:ss\.fff", ((17 * 60 + 14) * 60 + 48) * 1000 + 153)]
        [InlineData("3:17:14:48.153", "G", (((3 * 24 + 17) * 60 + 14) * 60 + 48) * 1000 + 153)]
        [InlineData("12", "g", 12 * 24 * 3600 * 1000)]
        // [InlineData("17:14:48", "g", ((17*60+14)*60+48)*1000)]
        // ReSharper disable once xUnit1026
        public void TimeSpanExTest_Parse(string s, string fmtStrDesc, long expectResult)
        {
            var ticks = expectResult * 10_000;
            var expect = new TimeSpan(ticks);

            var ts = TimeSpanEx.Parse(s);
            Assert.Equal(expect, ts);
            // ts = new TimeSpan(ts.Ticks);
            Output.WriteLine($"{s} [{fmtStrDesc}]: {ts.ToRelativeDateString()}, {ts.ToMoment()}");
        }
    }
}