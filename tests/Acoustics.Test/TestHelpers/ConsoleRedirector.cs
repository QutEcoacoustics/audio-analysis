namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;

    internal class ConsoleRedirector : IDisposable
    {
        private readonly ArrayStringWriter consoleOutput = new ArrayStringWriter();
        private readonly TextWriter originalConsoleOutput;

        public ConsoleRedirector()
        {
            this.originalConsoleOutput = Console.Out;
            Console.SetOut(this.consoleOutput);
        }

        public ReadOnlyCollection<string> Lines => this.consoleOutput.Lines;

        public void Dispose()
        {
            Console.SetOut(this.originalConsoleOutput);
            LoggedConsole.Write("------CAPTURED CONSOLE OUTPUT--------\n" + this.GetString());
            this.consoleOutput.Dispose();
        }

        public string GetString() => string.Join(Environment.NewLine, this.consoleOutput.Lines);

        public class ArrayStringWriter : TextWriter
        {
            private static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();
            private readonly List<string> lines;
            private bool halfLine;

            public ArrayStringWriter(int size)
            {
                this.lines = new List<string>(size)
                {
                    string.Empty,
                };

                if (NewLineChars.Length > 2)
                {
                    throw new PlatformNotSupportedException("Cannot process a new line token that had more than two chars");
                }
            }

            public ArrayStringWriter()
                : this(10)
            {
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public ReadOnlyCollection<string> Lines => this.lines.AsReadOnly();

            public override void Write(char c)
            {
                bool match = NewLineChars[0] == c;

                if (NewLineChars.Length == 1)
                {
                    if (match)
                    {
                        this.lines.Add(string.Empty);
                    }
                    else
                    {
                        this.lines[this.lines.Count - 1] += c;
                    }
                }
                else
                {
                    if (match)
                    {
                        this.halfLine = true;
                    }
                    else if (this.halfLine)
                    {
                        this.halfLine = false;
                        if (NewLineChars[1] == c)
                        {
                            this.lines.Add(string.Empty);
                        }
                        else
                        {
                            this.lines[this.lines.Count - 1] += c;
                        }
                    }
                    else
                    {
                        this.lines[this.lines.Count - 1] += c;
                    }
                }
            }

            public override void Write(string value)
            {
                this.halfLine = false;
                this.Append(value);
            }

            public override void WriteLine(string value)
            {
                this.halfLine = false;
                this.AddNewLine(value);
            }

            private void AddNewLine(string line = "")
            {
                if (line != string.Empty)
                {
                    this.Append(line);
                }

                this.lines.Add(string.Empty);
            }

            private void Append(string s)
            {
                if (s == null)
                {
                    return;
                }

                var split = s.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                if (split.Length == 1)
                {
                    this.lines[this.lines.Count - 1] += split[0];
                }
                else if (split.Length > 1)
                {
                    foreach (var line in split)
                    {
                        this.AddNewLine(line);
                    }
                }
            }
        }
    }
}