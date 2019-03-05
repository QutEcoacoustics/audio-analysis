// <copyright file="PhysicalConsoleLogger.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production
{
    using System;
    using System.IO;
    using Acoustics.Shared.Logging;
    using McMaster.Extensions.CommandLineUtils;

    public class PhysicalConsoleLogger
        : IConsole
    {
        private readonly Log4NetTextWriter outWriter = new Log4NetTextWriter(Console.Out);
        private readonly Log4NetTextWriter errorWriter = new Log4NetTextWriter(Console.Error, mode: Log4NetTextWriter.Mode.Error);

        public PhysicalConsoleLogger()
        {
        }

        /// <summary>
        /// <see cref="Console.CancelKeyPress"/>.
        /// </summary>
        public event ConsoleCancelEventHandler CancelKeyPress
        {
            add => Console.CancelKeyPress += value;
            remove => Console.CancelKeyPress -= value;
        }

        public TextWriter Out => this.outWriter;

        public TextWriter Error => this.errorWriter;

        public TextReader In => Console.In;

        public bool IsInputRedirected => Console.IsInputRedirected;

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public ConsoleColor ForegroundColor
        {
            get => MainEntry.ApPlainLogging ? ConsoleColor.White : Console.ForegroundColor;
            set
            {
                if (!MainEntry.ApPlainLogging)
                {
                    Console.ForegroundColor = value;
                }
            }
        }

        public ConsoleColor BackgroundColor
        {
            get => MainEntry.ApPlainLogging ? ConsoleColor.Black : Console.BackgroundColor;
            set
            {
                if (!MainEntry.ApPlainLogging)
                {
                    Console.BackgroundColor = value;
                }
            }
        }

        public void ResetColor()
        {
            if (!MainEntry.ApPlainLogging)
            {
                Console.ResetColor();
            }
        }
    }
}