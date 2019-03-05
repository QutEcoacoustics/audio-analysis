// <copyright file="Logging.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.Contracts;
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Layout.Pattern;
    using log4net.Repository.Hierarchy;
    using log4net.Util;
    using static log4net.Appender.ManagedColoredConsoleAppender;

    public class Logging
    {
        public const string CleanLogger = "CleanLogger";
        public const string LogFileOnly = "LogFileOnly";

        private readonly Logger rootLogger;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Logger cleanLogger;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Logger noConsoleLogger;
        private readonly AppenderSkeleton standardConsoleAppender;
        private readonly AppenderSkeleton cleanConsoleAppender;
        private readonly Hierarchy repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logging"/> class.
        /// </summary>
        /// <param name="colorConsole">If True, colored logs will be used.</param>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/>.</param>
        public Logging(bool colorConsole, Level defaultLevel, bool quietConsole)
            : this(enableMemoryLogger: false, enableFileLogger: true, colorConsole, defaultLevel, quietConsole)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logging"/> class.
        /// </summary>
        /// <param name="enableMemoryLogger">If true, stores a copy of all log events in memory. Used for testing.</param>
        /// <param name="enableFileLogger">If true, outputs log events to a file.</param>
        /// <param name="colorConsole">If True, colored logs will be used.</param>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/>.</param>
        internal Logging(
            bool enableMemoryLogger,
            bool enableFileLogger,
            bool colorConsole,
            Level defaultLevel,
            bool quietConsole)
        {
            LogManager.ResetConfiguration();

            this.repository = (Hierarchy)LogManager.GetRepository();

            this.repository.LevelMap.Add(LogExtensions.PromptLevel);
            this.repository.LevelMap.Add(LogExtensions.SuccessLevel);

            this.rootLogger = this.repository.Root;
            this.cleanLogger = (Logger)this.repository.GetLogger(CleanLogger);
            this.noConsoleLogger = (Logger)this.repository.GetLogger(LogFileOnly);

            // cleanLogger.Hierarchy = repository;
            // noConsoleLogger.Hierarchy = repository;

            // our two special loggers do not forward log events to the root logger
            this.cleanLogger.Additivity = false;
            this.noConsoleLogger.Additivity = false;

            // this is the base level for the logging system
            this.repository.Threshold = defaultLevel;
            this.rootLogger.Level = defaultLevel;

            // log to a file
            PatternLayout standardPattern = new PatternLayout
            {
                ConversionPattern = "%date{o} [%thread] %-5level %logger - %message%newline%exception",
            };
            standardPattern.ActivateOptions();

            string logFilePath = null;
            if (enableFileLogger)
            {
                var fileAppender = new RollingFileAppender()
                {
                    AppendToFile = false,
                    Encoding = Encoding.UTF8,
                    StaticLogFileName = true,

                    // We clean our logs ourselves, so it might be assumed that MaxSizeRollBackups is not needed,
                    // however this constraint is needed to trigger log4net's dedupe function for duplicate file names
                    MaxSizeRollBackups = 100,
                    Layout = standardPattern,
                    Name = nameof(RollingFileAppender),

                    // ReSharper disable StringLiteralTypo
                    File = new PatternString("Logs/log_%utcdate{yyyyMMddTHHmmssZ}.txt").Format(),

                    // ReSharper restore StringLiteralTypo

                    PreserveLogFileNameExtension = true,
                    RollingStyle = RollingFileAppender.RollingMode.Once,
                };
                this.rootLogger.AddAppender(fileAppender);
                this.cleanLogger.AddAppender(fileAppender);
                this.noConsoleLogger.AddAppender(fileAppender);
                fileAppender.ActivateOptions();
                logFilePath = fileAppender.File;
            }

            if (enableMemoryLogger)
            {
                this.MemoryAppender = new MemoryAppender()
                {
                    Layout = standardPattern,
                };
                this.rootLogger.AddAppender(this.MemoryAppender);
                this.cleanLogger.AddAppender(this.MemoryAppender);
                this.noConsoleLogger.AddAppender(this.MemoryAppender);
                this.MemoryAppender.ActivateOptions();
            }

            // log to a console
            PatternLayout consolePattern = new PatternLayout
            {
                ConversionPattern = "[%date{o}] %-5level - %message%newline%exception",
            };
            consolePattern.ActivateOptions();
            this.standardConsoleAppender =
                colorConsole ? (AppenderSkeleton)new ManagedColoredConsoleAppender() : new ConsoleAppender();
            this.standardConsoleAppender.Layout = consolePattern;

            PatternLayout cleanPattern = new PatternLayout
            {
                ConversionPattern = "%message%newline",
            };
            cleanPattern.ActivateOptions();
            this.cleanConsoleAppender =
                colorConsole ? (AppenderSkeleton)new ManagedColoredConsoleAppender() : new ConsoleAppender();
            this.cleanConsoleAppender.Layout = cleanPattern;

            if (colorConsole)
            {
                var mapping = new[]
                {
                    new LevelColors { ForeColor = ConsoleColor.Magenta, Level = LogExtensions.PromptLevel },
                    new LevelColors { ForeColor = ConsoleColor.Red, Level = Level.Fatal },
                    new LevelColors { ForeColor = ConsoleColor.DarkRed, Level = Level.Error },
                    new LevelColors { ForeColor = ConsoleColor.Yellow, Level = Level.Warn },
                    new LevelColors { ForeColor = ConsoleColor.Green, Level = LogExtensions.SuccessLevel },
                    new LevelColors { ForeColor = ConsoleColor.Green, Level = Level.Notice },
                    new LevelColors { ForeColor = ConsoleColor.Gray, Level = Level.Info },
                    new LevelColors { ForeColor = ConsoleColor.Cyan, Level = Level.Debug },
                    new LevelColors { ForeColor = ConsoleColor.DarkCyan, Level = Level.Trace },
                    new LevelColors { ForeColor = ConsoleColor.DarkBlue, Level = Level.Verbose },
                };

                foreach (var map in mapping)
                {
                    ((ManagedColoredConsoleAppender)this.standardConsoleAppender).AddMapping(map);
                    ((ManagedColoredConsoleAppender)this.cleanConsoleAppender).AddMapping(map);
                }
            }

            this.standardConsoleAppender.ActivateOptions();
            this.cleanConsoleAppender.ActivateOptions();

            this.rootLogger.AddAppender(this.standardConsoleAppender);
            this.cleanLogger.AddAppender(this.cleanConsoleAppender);

            this.repository.Configured = true;
            this.ModifyVerbosity(defaultLevel, quietConsole);

            if (enableFileLogger)
            {
                // Fire and forget the async cleaning task.
                // We'll never know if this fails or not, the exception is captured in the task
                // that we do NOT await. We do however log any exceptions.
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = this.CleanLogs(logFilePath);
            }
        }

        internal MemoryAppender MemoryAppender { get; }

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/></param>
        public void ModifyVerbosity(Level defaultLevel, bool quietConsole)
        {
            this.repository.Threshold = defaultLevel;
            this.rootLogger.Level = defaultLevel;

            var quietThreshold = quietConsole ? Level.Error : Level.All;

            if (this.cleanConsoleAppender != null)
            {
                this.cleanConsoleAppender.Threshold = quietThreshold;
            }

            if (this.standardConsoleAppender != null)
            {
                this.standardConsoleAppender.Threshold = quietThreshold;
            }

            this.repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        public void TestLogging()
        {
            var log = LogManager.GetLogger(nameof(Logging));

            log.Prompt("Log test PROMPT");
            log.Fatal("Log test FATAL");
            log.Error("Log test ERROR");
            log.Warn("Log test WARN");
            log.Success("Log test SUCCESS");
            log.Info("Log test INFO");
            log.Debug("Log test DEBUG");
            log.Trace("Log test TRACE");
            log.Verbose("Log test VERBOSE");
            LoggedConsole.WriteFatalLine("Clean wrapper FATAL", new Exception("I'm a fake"));
            LoggedConsole.Log.Fatal("Clean log FATAL", new Exception("I'm a fake"));
            LoggedConsole.WriteErrorLine("Clean wrapper ERROR");
            LoggedConsole.Log.Error("Clean log ERROR");
            LoggedConsole.WriteWarnLine("Clean wrapper WARN");
            LoggedConsole.Log.Warn("Clean log WARN");
            LoggedConsole.WriteSuccessLine("Clean wrapper SUCCESS");
            LoggedConsole.Log.Success("Clean log SUCCESS");
            LoggedConsole.WriteLine("Clean wrapper INFO");
            LoggedConsole.Log.Info("Clean log INFO");
        }

        /// <summary>
        /// Rolling log file appender has no concept of cleaning up logs with a date stamp in their name.
        /// This we have to clean them manually.
        /// </summary>
        /// <returns>A task.</returns>
        private async Task CleanLogs(string logFilePath)
        {
            Contract.RequiresNotNull(logFilePath);
            const int threshold = 60;
            const int target = 50;

            void CleanFiles()
            {
                var logsPath = Path.GetDirectoryName(logFilePath) ??
                               throw new InvalidOperationException("Could not resolve logs directory path: " + logFilePath);
                var files = Directory.GetFiles(logsPath, "log_*.txt");
                if (files.Length > threshold)
                {
                    var sorted = new SortedDictionary<DateTime, List<string>>();
                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);

                        // assuming a format of log_20180717T130822Z.1.txt
                        var datePart = name.Substring(4, name.IndexOf(".", StringComparison.Ordinal) - 4);
                        var date = DateTime.ParseExact(
                            datePart,
                            "yyyyMMddTHHmmssZ",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal);

                        if (sorted.ContainsKey(date))
                        {
                            sorted[date].Add(file);
                        }
                        else
                        {
                            sorted.Add(date, new List<string>() { file });
                        }
                    }

                    // then delete the last 10 or so (this way we batch deletes)
                    var toDelete = files.Length - target;
                    foreach (var kvp in sorted)
                    {
                        foreach (var file in kvp.Value)
                        {
                            File.Delete(file);
                            toDelete--;

                            if (toDelete <= 0)
                            {
                                return;
                            }
                        }
                    }
                }
            }

            try
            {
                // ReSharper disable once RedundantCast
                await Task.Run((Action)CleanFiles);
            }
            catch (Exception ex)
            {
                LoggedConsole.WriteFatalLine("Log cleaning failed, this is a bug, please report it.", ex);
            }
        }
    }
}
