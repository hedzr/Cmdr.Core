using System;
using HzNS.Cmdr.Tool.Ext;

namespace HzNS.Cmdr.Tool.Colorify.Terminal
{
    public static class Out
    {
        public static void Write(string text)
        {
            Console.Write(text);
        }

        public static void WriteLine(string text)
        {
            var size = Console.WindowWidth - 1;
            if (size != Console.CursorLeft + 1)
            {
                size -= Console.CursorLeft;
            }

            if (size < text.Length)
            {
                size += Console.WindowWidth;
            }

            if (size < 0)
            {
                size = text.Length;
            }

            Console.WriteLine($"{text.PadRight(size)}");
        }

        public static void AlignRight(string text)
        {
            Console.WriteLine($"{text.PadLeft(Console.WindowWidth - 1)}");
        }

        public static void AlignLeft(string text)
        {
            Console.WriteLine($"{text.PadRight(Console.WindowWidth - 1)}");
        }

        public static void AlignCenter(string text)
        {
            decimal size = Console.WindowWidth - 1 - text.Length;
            var rightSize = (int)Math.Round(size / 2);
            var leftSize = (int)(size - rightSize);
            var leftMargin = new String(' ', leftSize);
            var rightMargin = new String(' ', rightSize);

            Console.Write(leftMargin);
            Console.Write(text);
            Console.WriteLine(rightMargin);
        }

        public static void Extreme(string left, string right)
        {
            decimal size = Console.WindowWidth - 1;
            var rightMargin = (int)Math.Round(size / 2);
            var leftMargin = (int)(size - rightMargin);

            Console.Write($"{left}".PadRight(rightMargin));
            Console.WriteLine($"{right}".PadLeft(leftMargin));
        }

        public static void DivisionLine(char character)
        {
            var text = new string(character, Console.WindowWidth - 1);
            Console.WriteLine(text);
        }

        public static void BlankLines(int? lines = 1)
        {
            for (var i = 0; i < lines; i++)
            {
                DivisionLine(' ');
            }
        }
    }
}