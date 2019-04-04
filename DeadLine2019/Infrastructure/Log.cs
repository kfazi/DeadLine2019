namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Caliburn.Micro;

    public class Log : PropertyChangedBase
    {
        private const int LinesLimit = 500;

        private readonly LinkedList<string> _lines;

        private readonly StringBuilder _currentLine;

        private int _writeType;

        private string _text;

        public Log()
        {
            _lines = new LinkedList<string>();
            _writeType = 0;
            _currentLine = new StringBuilder();
        }

        public bool LogCommands { get; set; } = true;

        public string Text
        {
            get => _text;

            set
            {
                if (_text == value)
                {
                    return;
                }

                _text = value;
                NotifyOfPropertyChange();
            }
        }

        public void WriteTo(string text)
        {
            if (!LogCommands)
            {
                return;
            }

            if (_writeType != 1 && _currentLine.Length > 0)
            {
                WriteLine();
            }

            _writeType = 1;
            AddText(text);
        }

        public void WriteLineFrom(string text)
        {
            if (!LogCommands)
            {
                return;
            }

            if (_writeType != 0 && _currentLine.Length > 0)
            {
                WriteLine();
            }

            _writeType = 0;
            AddText(text);
            WriteLine();
        }

        public void Write(string text)
        {
            if (_writeType != 2 && _currentLine.Length > 0)
            {
                WriteLine();
            }

            _writeType = 2;
            AddText(text);
            WriteLine();
        }

        private void AddText(string text)
        {
            foreach (var c in text)
            {
                if (c == '\n')
                {
                    WriteLine();
                    continue;
                }

                if (c == '\r')
                {
                    continue;
                }

                _currentLine.Append(c);
            }

            Text = string.Join(Environment.NewLine, _lines.Concat(new[] { GetPrefix() + _currentLine }));
        }

        private string GetPrefix()
        {
            switch (_writeType)
            {
                case 0:
                    return "<<< ";
                case 1:
                    return ">>> ";
                default:
                    return string.Empty;
            }
        }

        private void WriteLine()
        {
            while (_lines.Count > LinesLimit)
            {
                _lines.RemoveFirst();
            }

            _lines.AddLast(GetPrefix() + _currentLine);
            _currentLine.Clear();
        }
    }
}