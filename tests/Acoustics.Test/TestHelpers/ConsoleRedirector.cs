namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class ConsoleRedirector : IDisposable
    {
        private readonly ArrayStringWriter capturedOutput = new ArrayStringWriter();
        private readonly TextWriter originalConsoleOutput;

        public ConsoleRedirector()
        {
            Console.Out.Close();
            Console.SetOut(this.capturedOutput);
        }

        public ReadOnlyCollection<string> Lines => this.capturedOutput.Lines;

        public void Dispose()
        {
            var writer = new StreamWriter(Console.OpenStandardOutput())
            {
                AutoFlush = true,
            };
            Console.SetOut(writer);

            LoggedConsole.Write($"------CAPTURED CONSOLE OUTPUT ({this.Lines.Count} lines)--------\n{this.GetString()}");
            this.capturedOutput.Dispose();
        }

        public string GetString() => string.Join(Environment.NewLine, this.capturedOutput.Lines);

        public class ArrayStringWriter : TextWriter
        {
            private readonly List<string> lines;

            public ArrayStringWriter(int size)
            {
                this.lines = new List<string>(size)
                {
                    string.Empty,
                };
            }

            public ArrayStringWriter()
                : this(10)
            {
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public ReadOnlyCollection<string> Lines => this.lines.AsReadOnly();

            public override void Write(char c)
            {
                switch (c)
                {
                    case '\r':
                        break;
                    case '\n':
                        this.lines.Add(string.Empty);
                        break;
                    default:
                        this.lines[this.lines.Count - 1] += c;
                        break;
                }
            }

            public override void Write(string value)
            {
                this.Append(value);
            }

            public override void WriteLine(string value)
            {
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

                if (split.Length > 0)
                {
                    this.lines[this.lines.Count - 1] += split[0];
                }

                if (split.Length > 1)
                {
                    foreach (var line in split.Skip(1))
                    {
                        this.lines.Add(line);
                    }
                }
            }
        }
    }
}