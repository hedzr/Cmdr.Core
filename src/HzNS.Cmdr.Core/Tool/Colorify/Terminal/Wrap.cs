#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HzNS.Cmdr.Tool.Colorify.Terminal
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    public static class Wrapper
    {
        static int _screenWidth { get; set; }

        public static void Text(string text)
        {
            var line = new StringBuilder();
            var words = text.Split(' ');
            _screenWidth = (Console.WindowWidth - 3);
            foreach (var item in words)
            {
                Line(ref line, item);
                Item(ref line, item);
            }

            if (!String.IsNullOrEmpty(line.ToString().Trim()))
            {
                Out.WriteLine($"{line.ToString().TrimEnd()}");
            }
        }

        static void Line(ref StringBuilder line, string item)
        {
            if (
                ((line.Length + item.Length) >= _screenWidth) ||
                (line.ToString().Contains(Environment.NewLine))
            )
            {
                Out.WriteLine($"{line.ToString().TrimEnd()}");
                line.Clear();
            }
        }

        static void Item(ref StringBuilder line, string item)
        {
            while (true)
            {
                if (item.Length >= _screenWidth)
                {
                    if (line.Length > 0)
                    {
                        Out.WriteLine($" {line.ToString().TrimEnd()}");
                        line.Clear();
                    }

                    var chunkSize = item.Length - _screenWidth;
                    var chunk = item[.._screenWidth];
                    line.Append(chunk).Append(' ');
                    Line(ref line, item);
                    item = item.Substring(_screenWidth, chunkSize);
                    continue;
                }
                else
                {
                    line.Append(item).Append(' ');
                }

                break;
            }
        }
    }
}