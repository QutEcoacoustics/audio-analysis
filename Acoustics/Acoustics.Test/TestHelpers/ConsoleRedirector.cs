namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.IO;

    internal class ConsoleRedirector : IDisposable
    {
        private readonly StringWriter consoleOutput = new StringWriter();
        private readonly TextWriter originalConsoleOutput;

        public ConsoleRedirector()
        {
            this.originalConsoleOutput = Console.Out;
            Console.SetOut(this.consoleOutput);
        }

        public void Dispose()
        {
            Console.SetOut(this.originalConsoleOutput);
            LoggedConsole.Write(this.ToString());
            this.consoleOutput.Dispose();
        }

        public override string ToString()
        {
            return this.consoleOutput.ToString();
        }
    }
}