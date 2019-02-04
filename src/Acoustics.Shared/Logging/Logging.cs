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
    using Contracts;
    using log4net;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Layout;
    using log4net.Repository.Hierarchy;
    using log4net.Util;
    using static log4net.Appender.ManagedColoredConsoleAppender;

    public static class Logging
    {
        public const string Cleanlogger = "CleanLogger";
        public const string Logfileonly = "LogFileOnly";

        private static bool configured;
        private static Logger rootLogger;
        private static Logger cleanLogger;
        private static Logger noConsoleLogger;
        private static AppenderSkeleton standardConsoleAppender;
        private static AppenderSkeleton cleanConsoleAppender;
        private static Hierarchy repository;

        internal static MemoryAppender MemoryAppender { get; private set; }

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="colorConsole">If True, colored logs will be used.</param>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/></param>
        public static void Initialize(bool colorConsole, Level defaultLevel, bool quietConsole)
        {
            // This is the default case.
            Initialize(enableMemoryLogger: false, enableFileLogger: true, colorConsole, defaultLevel, quietConsole);
        }

        /// <summary>
        /// Initializes the logging system.
        /// </summary>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/></param>
        public static void ModifyVerbosity(Level defaultLevel, bool quietConsole)
        {
            Contract.Requires<InvalidOperationException>(configured, "The logger system must be initialised before verbosity is changed");

            repository.Threshold = defaultLevel;
            rootLogger.Level = defaultLevel;

            var quietThreshold = quietConsole ? Level.Error : Level.All;

            if (cleanConsoleAppender != null)
            {
                cleanConsoleAppender.Threshold = quietThreshold;
            }

            if (standardConsoleAppender != null)
            {
                standardConsoleAppender.Threshold = quietThreshold;
            }

            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        public static void TestLogging()
        {
            Contract.Requires<InvalidOperationException>(
                configured,
                "The logger system must be initialised before the logging can be tested");

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
        /// Initializes the logging system with extra options used by unit tests.
        /// </summary>
        /// <param name="enableMemoryLogger">If true, stores a copy of all log events in memory. Used for testing.</param>
        /// <param name="enableFileLogger">If true, outputs log events to a file.</param>
        /// <param name="colorConsole">If True, colored logs will be used.</param>
        /// <param name="defaultLevel">The default level to set for the root logger.</param>
        /// <param name="quietConsole">If True limits the level on the appenders to <see cref="Level.Error"/></param>
        internal static void Initialize(
            bool enableMemoryLogger,
            bool enableFileLogger,
            bool colorConsole,
            Level defaultLevel,
            bool quietConsole)
        {
            Contract.Requires<InvalidOperationException>(!configured, "The logger system can only be initialised once");
            configured = true;

            repository = (Hierarchy)LogManager.GetRepository();

            repository.LevelMap.Add(LogExtensions.PromptLevel);
            repository.LevelMap.Add(LogExtensions.SuccessLevel);

            rootLogger = repository.Root;
            cleanLogger = (Logger)repository.GetLogger(Cleanlogger);
            noConsoleLogger = (Logger)repository.GetLogger(Logfileonly);

            // cleanLogger.Hierarchy = repository;
            // noConsoleLogger.Hierarchy = repository;

            // our two special loggers do not forward log events to the root logger
            cleanLogger.Additivity = false;
            noConsoleLogger.Additivity = false;

            // this is the base level for the logging system
            repository.Threshold = defaultLevel;
            rootLogger.Level = defaultLevel;

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
                    File = new PatternString("Logs/log_%utcdate{yyyyMMddTHHmmssZ}.txt").Format(),
                    PreserveLogFileNameExtension = true,
                    RollingStyle = RollingFileAppender.RollingMode.Once,
                };
                rootLogger.AddAppender(fileAppender);
                cleanLogger.AddAppender(fileAppender);
                noConsoleLogger.AddAppender(fileAppender);
                fileAppender.ActivateOptions();
                logFilePath = fileAppender.File;
            }

            if (enableMemoryLogger)
            {
                MemoryAppender = new MemoryAppender()
                {
                    Layout = standardPattern,
                };
                rootLogger.AddAppender(MemoryAppender);
                cleanLogger.AddAppender(MemoryAppender);
                noConsoleLogger.AddAppender(MemoryAppender);
                MemoryAppender.ActivateOptions();
            }

            // log to a console
            PatternLayout consolePattern = new PatternLayout
            {
                ConversionPattern = "[%date{o}] %-5level - %message%newline%exception",
            };
            consolePattern.ActivateOptions();
            standardConsoleAppender =
                colorConsole ? (AppenderSkeleton)new ManagedColoredConsoleAppender() : new ConsoleAppender();
            standardConsoleAppender.Layout = consolePattern;

            PatternLayout cleanPattern = new PatternLayout
            {
                ConversionPattern = "%message%newline",
            };
            cleanPattern.ActivateOptions();
            cleanConsoleAppender =
                colorConsole ? (AppenderSkeleton)new ManagedColoredConsoleAppender() : new ConsoleAppender();
            cleanConsoleAppender.Layout = cleanPattern;

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
                    ((ManagedColoredConsoleAppender)standardConsoleAppender).AddMapping(map);
                    ((ManagedColoredConsoleAppender)cleanConsoleAppender).AddMapping(map);
                }
            }

            standardConsoleAppender.ActivateOptions();
            cleanConsoleAppender.ActivateOptions();

            rootLogger.AddAppender(standardConsoleAppender);
            cleanLogger.AddAppender(cleanConsoleAppender);

            repository.Configured = true;
            ModifyVerbosity(defaultLevel, quietConsole);

            if (enableFileLogger)
            {
                // Fire and forget the async cleaning task.
                // We'll never know if this fails or not, the exception is captured in the task
                // that we do NOT await. We do however log any exceptions.
                _ = CleanLogs(logFilePath);
            }
        }

        /// <summary>
        /// Rolling log file appender has no concept of cleaning up logs with a datestamp in their name.
        /// This we have to clean them manually.
        /// </summary>
        /// <returns>A task.</returns>
        private static async Task CleanLogs(string logFilePath)
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
                await Task.Run((Action)CleanFiles);
            }
            catch (Exception ex)
            {
                LoggedConsole.WriteFatalLine("Log cleaning failed, this is a bug, please report it.", ex);
            }
        }
    }
}
