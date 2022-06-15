#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HzNS.Cmdr.Tool.Colorify.Terminal;
using HzNS.Cmdr.Tool.Colorify.Theme;

namespace HzNS.Cmdr.Tool.Colorify
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ArrangeTypeMemberModifiers")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class Format
    {
        Dictionary<string, Color> _theme { get; set; }
        private readonly object colorLock = new object();

        public bool KeepBackgroundColor { get; set; } = true;

        public Format(ITheme theme)
        {
            if (theme == null)
            {
                throw new ArgumentException(nameof(theme));
            }

            _theme = theme._colors;
            DefaultColor();
        }

        void SetColor(string color, bool keepBackgroundColor = true)
        {
            if (_theme.TryGetValue(color, out var value))
            {
                if (!keepBackgroundColor && !KeepBackgroundColor)
                    Console.BackgroundColor = value._background;
                Console.ForegroundColor = value._foreground;
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        void DefaultColor(bool keepBackgroundColor = true, string color = Colors.bgDefault)
        {
            var theme = _theme[color];
            if (!keepBackgroundColor && !KeepBackgroundColor)
                Console.BackgroundColor = theme._background;
            Console.ForegroundColor = theme._foreground;
        }

        public void ResetColor()
        {
            lock (colorLock)
            {
                Console.ResetColor();
            }
        }

        public void Clear()
        {
            lock (colorLock)
            {
                Console.Clear();
            }
        }

        public void Write(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.Write(text);
                DefaultColor();
            }
        }

        public void WriteLine(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.WriteLine(text);
                DefaultColor();
            }
        }

        public void AlignCenter(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.AlignCenter(text);
                DefaultColor();
            }
        }

        public void AlignRight(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.AlignRight(text);
                DefaultColor();
            }
        }

        public void AlignLeft(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.AlignLeft(text);
                DefaultColor();
            }
        }

        public void AlignSplit(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                string left = text.Split('|')[0];
                string right = text.Split('|')[1];
                Out.Extreme(left, right);
                DefaultColor();
            }
        }

        public void Wrap(string text, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Wrapper.Text(text);
                DefaultColor();
            }
        }

        public void DivisionLine(char character)
        {
            DivisionLine(character, Colors.txtDefault);
        }

        public void DivisionLine(char character, string color)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.DivisionLine(character);
                DefaultColor();
            }
        }

        public void BlankLines(string color)
        {
            BlankLines(1, color);
        }

        public void BlankLines(int lines, string color = Colors.txtDefault)
        {
            lock (colorLock)
            {
                SetColor(color);
                Out.BlankLines(lines);
                DefaultColor();
            }
        }
    }
}