using System;
using System.Text;
using System.Text.RegularExpressions;

namespace HzNS.Cmdr.Tool.Ext
{
    /// <summary>
    /// ref: https://stackoverflow.com/questions/11/calculate-relative-time-in-c-sharp
    /// </summary>
    public static class DateTimeExtensions
    {
        // private static readonly Dictionary<long, string> Thresholds;
        //
        // static DateTimeExtensions()
        // {
        //     Thresholds = new Dictionary<long, string>();
        //     const int minute = 60;
        //     const int hour = 60 * minute;
        //     const int day = 24 * hour;
        //     
        //     Thresholds.Add(2, "one second ago");
        //     Thresholds.Add(60, "{0} seconds ago");
        //     Thresholds.Add(minute * 2, "a minute ago");
        //     Thresholds.Add(45 * minute, "{0} minutes ago");
        //     // Thresholds.Add(60 * minute, "an hour ago");
        //     // Thresholds.Add(90 * minute, "an hour ago");
        //     Thresholds.Add(120 * minute, "an hour ago");
        //     Thresholds.Add(day, "{0} hours ago");
        //     Thresholds.Add(day * 2, "yesterday");
        //     Thresholds.Add(day * 28, "{0} days ago");
        //     Thresholds.Add(day * (28+31), "one month ago");
        //     Thresholds.Add(day * 365, "{0/month} months ago");
        //     Thresholds.Add(day * 365*2, "one year ago");
        //     Thresholds.Add(long.MaxValue, "{0} years ago");
        // }

        // public static string ToRelativeDateString(this DateTime theDate)
        // {
        //     var since = (DateTime.Now.Ticks - theDate.Ticks) / 10000000;
        //     foreach (var threshold in Thresholds.Keys)
        //     {
        //         // ReSharper disable once InvertIf
        //         if (since < threshold)
        //         {
        //             var t = new TimeSpan((DateTime.Now.Ticks - theDate.Ticks));
        //             return string.Format(Thresholds[threshold],
        //                 (t.Days > 365
        //                     ? t.Days / 365
        //                     : (t.Days > 0
        //                         ? t.Days
        //                         : (t.Hours > 0
        //                             ? t.Hours
        //                             : (t.Minutes > 0 ? t.Minutes : (t.Seconds > 0 ? t.Seconds : 0))))).ToString());
        //         }
        //     }
        //
        //     return "";
        // }

        public static string ToRelativeDateString(this DateTime dt)
        {
            var ts = new TimeSpan(DateTime.Now.Ticks - dt.Ticks);
            return impl(ts);
        }

        public static string ToUtcRelativeDateString(this DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            return impl(ts);
        }

        // ReSharper disable once InconsistentNaming
        private static string impl(TimeSpan ts)
        {
            var delta = (int) Math.Floor(Math.Abs(ts.TotalSeconds));

            if (delta < 60)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 120)
                return "a minute ago";

            if (delta < 2700) // 45 * 60
                return ts.Minutes + " minutes ago";

            if (delta < 5400) // 90 * 60
                return "an hour ago";

            if (delta < 2 * 60 * 60)
                return "2 hours ago";

            if (delta < 86400) // 24 * 60 * 60
                return ts.Hours + " hours ago";

            if (delta < 172800) // 48 * 60 * 60
                return "yesterday";

            if (delta < 2592000) // 30 * 24 * 60 * 60
                return ts.Days + " days ago";

            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                var months = Convert.ToInt32(Math.Floor((double) ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }

            var years = Convert.ToInt32(Math.Floor((double) ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }


        // TimeSpan


        public static string ToRelativeDateString(this TimeSpan ts)
        {
            return impl(ts);
        }

        public static string ToMoment(this TimeSpan ts)
        {
            var sb = new StringBuilder();

            if (ts.Days > 0)
                sb.Append($"{ts.Days}d");
            if (ts.Hours > 0)
                sb.Append($"{ts.Hours}h");
            if (ts.Minutes > 0)
                sb.Append($"{ts.Minutes}m");
            if (ts.Seconds > 0)
                sb.Append($"{ts.Seconds}s");
            if (ts.Milliseconds > 0)
                sb.Append($"{ts.Milliseconds}ms");

            return sb.ToString();
        }

        public static TimeSpan FromMoment(this TimeSpan ts, string s)
        {
            var start = 0;
            while (start < s.Length)
            {
                var (ok, num, pos) = WantDigits(s, start);
                if (!ok) return ts;
                (ok, pos) = addTo(ref ts, num, s, start + pos);
                if (!ok) return ts;
                start = pos;
            }

            return ts;
        }

        // ReSharper disable once InconsistentNaming
        private static (bool ok, int pos) addTo(ref TimeSpan ts, int num, string s, int start)
        {
            if (start + 2 <= s.Length && s.Substring(start, 2) == "ms")
            {
                ts = ts.Add(new TimeSpan(0, 0, 0, 0, num));
                return (true, start + 2);
            }

            if (start > s.Length - 1) return (false, 0);

            switch (s[start])
            {
                case 'd':
                    ts = ts.Add(new TimeSpan(num, 0, 0, 0, 0));
                    break;
                case 'h':
                    ts = ts.Add(new TimeSpan(0, num, 0, 0, 0));
                    break;
                case 'm':
                    ts = ts.Add(new TimeSpan(0, 0, num, 0, 0));
                    break;
                case 's':
                    ts = ts.Add(new TimeSpan(0, 0, 0, num, 0));
                    break;
                default:
                    return (false, start);
            }

            return (true, start + 1);
        }

        private static (bool ok, int num, int length) WantDigits(string s, int start)
        {
            if (string.IsNullOrWhiteSpace(s)) return (false, 0, 0);
            var m = Regex.Match(s.Substring(start), DigitPattern);
            return m.Success ? (true, int.Parse(m.Groups[1].Value), m.Groups[1].Value.Length) : (false, 0, 0);
        }

        private const string DigitPattern = @"^(\d+)";
    }
}