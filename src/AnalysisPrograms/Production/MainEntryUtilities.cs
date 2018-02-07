// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainEntryUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MainEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using Acoustics.Shared.Contracts;

    #if DEBUG
#endif
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using EventStatistics;
    using log4net.Appender;
    using log4net.Core;
    using log4net.Filter;
    using log4net.Repository.Hierarchy;
#if DEBUG
    using Acoustics.Shared.Debugging;
#endif
    using Production;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;

    public static partial class MainEntry
    {
        // http://stackoverflow.com/questions/1600962/displaying-the-build-date?lq=1
        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                s?.Close();
            }

            int i = BitConverter.ToInt32(b, peHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + linkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }

        private static void Copyright()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            LoggedConsole.WriteLine(Meta.Description + fvi.FileVersion + " (" + (InDEBUG ? "DEBUG" : "RELEASE")
                + " build, " + RetrieveLinkerTimestamp().ToString("g") + ") \n"
                + "Git branch-version: " + fvi.ProductVersion + "\n"
                + "Copyright QUT " + DateTime.Now.Year.ToString("0000"));
        }

        // ReSharper disable once InconsistentNaming
        public static bool InDEBUG
        {
            get
            {
#if DEBUG
                return true;
#endif
#pragma warning disable 162
                return false;
#pragma warning restore 162
            }
        }

        public static bool IsDebuggerAttached => Debugger.IsAttached;

        private static void AttachExceptionHandler()
        {
            Environment.ExitCode = ExceptionLookup.SpecialExceptionErrorLevel;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        internal static void AttachDebugger(ref DebugOptions options)
        {
            if (options == DebugOptions.No)
            {
                return;
            }
#if DEBUG
            if (!Debugger.IsAttached)
            {
                if (options == DebugOptions.Prompt)
                {
                    // then prompt manually
                    LoggedConsole.WriteLine(
                        "Do you wish to debug? Attach now or press [Y] to attach. Press any key other key to continue.");
                    options = Console.ReadKey(true).KeyChar.ToString(CultureInfo.InvariantCulture).ToLower() == "y"
                        ? DebugOptions.Yes
                        : DebugOptions.No;
                }

                if (options == DebugOptions.Yes)
                {
                    var vsProcess =
                        VisualStudioAttacher.GetVisualStudioForSolutions(
                            new List<string> { "AudioAnalysis2012.sln", "AudioAnalysis.sln" });

                    if (vsProcess != null)
                    {
                        VisualStudioAttacher.AttachVisualStudioToProcess(vsProcess, Process.GetCurrentProcess());
                    }
                    else
                    {
                        // try and attach the old fashioned way
                        Debugger.Launch();
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (Debugger.IsAttached)
                    {
                        LoggedConsole.WriteLine("\t>>> Attach sucessful");
                    }

                    LoggedConsole.WriteLine();
                }
            }
#endif
        }

        public static void BeforeExecute(MainArgs main, CommandLineApplication application)
        {
            CommandLineApplication = application;
            var debugOptions = main.DebugOption;
            AttachDebugger(ref debugOptions);

            ModifyVerbosity(main);

            LoadNativeCode();
        }

        public static CommandLineApplication CommandLineApplication { get; private set; }

        internal enum Usages
        {
            All,
            Single,
            ListAvailable,
            NoAction,
        }

        private const string ApPlainLogging = "AP_PLAIN_LOGGING";

        internal static void PrintUsage(string message, Usages usageStyle, string commandName = null)
        {
            //Contract.Requires(usageStyle != Usages.Single || commandName != null);

            if (!string.IsNullOrWhiteSpace(message))
            {
                LoggedConsole.WriteErrorLine(message);
            }

            if (usageStyle == Usages.All)
            {
                // print entire usage
                CommandLineApplication.GetHelpText();
            }
            else if (usageStyle == Usages.Single)
            {
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    Log.Error("************* Can't print usage due to empty action name **************");
                }
                else
                {
                    CommandLineApplication.ShowHelp(commandName);
                }
            }
            else if (usageStyle == Usages.ListAvailable)
            {
                var commands = CommandLineApplication.Commands;

                var sb = new StringBuilder();

                sb.AppendLine("Available actions - ");

                foreach (var command in commands)
                {
                    sb.AppendLine("\t" + command.Name + " - " + command.Description);
                }

                LoggedConsole.WriteLine(sb.ToString());
            }
            else if (usageStyle == Usages.NoAction)
            {
                // this branch should no longer be needed because new command line utils handles this natively
                throw new InvalidOperationException();
                
                //var usage = ArgUsage.GetStyledUsage<MainEntryArguments>(options: UsagePrintOptions, includedActions: new[] { "list", "help" });
                //LoggedConsole.WriteLine(InsertEnvironmentVariablesIntoUsage(usage.ToString()));
            }
            else
            {
                throw new InvalidOperationException();
            }

        }

        internal static string InsertEnvironmentVariablesIntoUsage(string usage)
        {
            return usage.Insert(
                usage.IndexOf("Global options:", StringComparison.Ordinal),
                "Environment variables:\n" +
                "    " +
                ApPlainLogging + 
                "  [true|false]\t Enable simpler logging - the default value is `false`\n");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Contract.Requires(unhandledExceptionEventArgs != null);
            Contract.Requires(unhandledExceptionEventArgs.ExceptionObject != null);

            const string fatalMessage = "FATAL ERROR:\n\t";

            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;

            ExceptionLookup.ExceptionStyle style;
            bool found = false;
            if (ex is AggregateException original)
            {
                found = ExceptionLookup.ErrorLevels.TryGetValue(original.InnerException.GetType(), out style);
            }
            else
            {
                found = ExceptionLookup.ErrorLevels.TryGetValue(ex.GetType(), out style);
            }

            found = found && style.Handle;

            // if found, print message only if usage printing disabled
            if (found && !style.PrintUsage)
            {
                // this branch prints the message but suppresses the stack trace
                LoggedConsole.WriteFatalLine(fatalMessage, ex);
            }
            else if (found && ex.GetType() != typeof(Exception))
            {
                NoConsole.Log.Fatal("Fatal exception:", ex);

                var action = CommandLineApplication.Name;
                // print usage, if exception is recognized
                // --
                // attempt to retrieve action
//                string action = null;
//                if (Arguments != null)
//                {
//                    action = ((MainEntryArguments)Arguments.Value).Action;
//                }
//                else if (ex is ArgException)
//                {
//                    action = (ex as ArgException).Action;
//                }
//                else if (ArgException.LastAction.IsNotWhitespace())
//                {
//                    action = ArgException.LastAction;
//                }
//
//                if (ex is MissingArgException && ex.Message.Contains("action"))
//                {
//                    PrintUsage("An action is required to run the program, here are some suggestions:", Usages.NoAction);
//                }
//                else if (ex is UnknownActionArgException)
//                {
//                    PrintUsage(ex.Message, Usages.NoAction);
//                }
//                else if (ex is ValidationArgException)
//                {
//                    // for validation exceptions, use the inner exception
//                    ExceptionLookup.ErrorLevels.TryGetValue(ex.InnerException.GetType(), out style);
//                    PrintUsage(ex.Message, Usages.Single, action);
//                }

                if (ex.InnerException is TargetInvocationException)
                {
                    var message = fatalMessage;
                    message += FormatTargetInvocationException(ex.InnerException);
                    PrintUsage(message, Usages.Single, action ?? string.Empty);
                }
                else
                {
                    var message = fatalMessage + ex.Message;
                    PrintUsage(message, Usages.Single, action ?? string.Empty);
                }
            }
            else
            {
                // otherwise its a unhandled exception, log and raise
                Log.Fatal("Unhandled exception ->", ex);

                StringBuilder extraInformation = null;
                PrintAggregateException(ex, ref extraInformation);

                //if (extraInformation != null)
                //{
                //    Log.Error(extraInformation.ToString());
                //}
            }

            int returnCode = style?.ErrorCode ?? ExceptionLookup.SpecialExceptionErrorLevel;

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + returnCode);
            if (Debugger.IsAttached)
            {
                // no don't exit, we want the exception to be raised to Window's Exception handling
                // this will allow the debugger to appropriately break on the right line
                Environment.ExitCode = returnCode;
            }
            else
            {
                // If debugger is not attached, we *do not* want to raise the error to the Windows level
                // Everything has already been logged, just exit with appropriate errorlevel
                Environment.Exit(returnCode);
            }
        }

        private static void PrintAggregateException(Exception ex, ref StringBuilder innerExceptions, int depth = 0)
        {
            var depthString = "==".PadLeft(depth * 2, '=');
            //innerExceptions = innerExceptions ?? new StringBuilder();

            if (ex is AggregateException)
            {
                var aex = (AggregateException)ex;

                //innerExceptions.AppendLine("Writing detailed information about inner exceptions!");

                foreach (var exception in aex.InnerExceptions)
                {
                    //innerExceptions.AppendLine();
                    Log.Fatal("\n\n" + depthString + "> Inner exception:", exception);

                    if (exception is AggregateException) {
                        PrintAggregateException(exception, ref innerExceptions, depth++);
                    }
                }
            }

        }

        /// <summary>
        ///  Return the message of the first inner exception that is not an Invocation exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="depth"></param>
        private static string FormatTargetInvocationException(Exception ex, int depth = 0)
        {
            var depthString = "==".PadLeft(depth * 2, '=');

            var message = string.Empty;

            if (ex is TargetInvocationException)
            {
                var tiex = (TargetInvocationException)ex;

                message += FormatTargetInvocationException(tiex.InnerException, depth + 1);
            }
            else
            {
                message = depthString + "> Inner exception: " + ex.Message + Environment.NewLine;
            }

            return message;
        }

        /// <summary>
        /// This method will stop the program from exiting if the solution was built in #DEBUG
        /// and the program was started by Visual Studio.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void HangBeforeExit()
        {
#if DEBUG
            if (AppConfigHelper.IsMono)
            {
                return;
            }

            // if Michael is debugging with visual studio, this will prevent the window closing.
            Process parentProcess = ProcessExtensions.ParentProcessUtilities.GetParentProcess();
            if (parentProcess.ProcessName == "devenv")
            {
                LoggedConsole.WriteSuccessLine("FINISHED: Press RETURN key to exit.");
                Console.ReadLine();
            }
#endif
        }

        private static void ParseEnvirionemnt()
        {
            var simpleLogging = bool.TryParse(Environment.GetEnvironmentVariable(ApPlainLogging), out var isTrue) && isTrue;
            var repository = (Hierarchy)LogManager.GetRepository();
            var root = repository.Root;
            var cleanLogger = (Logger)repository.GetLogger("CleanLogger");

            if (simpleLogging)
            {
                root.RemoveAppender("ConsoleAppender");
                cleanLogger.RemoveAppender("CleanConsoleAppender");
            }
            else
            {
                root.RemoveAppender("SimpleConsoleAppender");
            }
        }

        private static void ModifyVerbosity(MainArgs arguments)
        {
            SetLogVerbosity(arguments.LogLevel, arguments.QuietConsole);
        }

        public static void SetLogVerbosity(LogVerbosity logVerbosity, bool quietConsole = false)
        {
            Level modifiedLevel;
            switch (logVerbosity)
            {
                case LogVerbosity.None:
                    modifiedLevel = Level.Off;
                    break;
                case LogVerbosity.Error:
                    modifiedLevel = Level.Error;
                    break;
                case LogVerbosity.Warn:
                    modifiedLevel = Level.Warn;
                    break;
                case LogVerbosity.Info:
                    modifiedLevel = Level.Info;
                    break;
                case LogVerbosity.Debug:
                    modifiedLevel = Level.Debug;
                    break;
                case LogVerbosity.Trace:
                    modifiedLevel = Level.Trace;
                    break;
                case LogVerbosity.Verbose:
                    modifiedLevel = Level.Verbose;
                    break;
                case LogVerbosity.All:
                    modifiedLevel = Level.All;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var repository = (Hierarchy)LogManager.GetRepository();
            repository.Root.Level = modifiedLevel;
            repository.Threshold = modifiedLevel;

            if (quietConsole)
            {
                var appenders = repository.GetAppenders();

                foreach (var appender in appenders)
                {
                    if (appender is ConsoleAppender || appender is ManagedColoredConsoleAppender
                        || appender is ColoredConsoleAppender)
                    {
                        ((AppenderSkeleton)appender).Threshold = Level.Notice;
                    }
                }
            }

            repository.RaiseConfigurationChanged(EventArgs.Empty);

            Log.Debug("Log level changed to: " + logVerbosity);

            // log test
            //            Log.Debug("Log test DEBUG");
            //            Log.Info("Log test INFO");
            //            Log.Success("Log test SUCCESS");
            //            Log.Warn("Log test WARN");
            //            Log.Error("Log test ERROR");
            //            Log.Fatal("Log test FATAL");
            //            Log.Trace("Log test TRACE");
            //            Log.Verbose("Log test VERBOSE");
            //            LoggedConsole.Log.Info("Clean log INFO");
            //            LoggedConsole.Log.Success("Clean log SUCCESS");
            //            LoggedConsole.Log.Warn("Clean log WARN");
            //            LoggedConsole.Log.Error("Clean log ERROR");
            //            LoggedConsole.WriteLine("Clean wrapper INFO");
            //            LoggedConsole.WriteSuccessLine("Clean wrapper SUCCESS");
            //            LoggedConsole.WriteWarnLine("Clean wrapper WARN");
            //            LoggedConsole.WriteErrorLine("Clean wrapper ERROR");
            //            LoggedConsole.WriteFatalLine("Clean wrapper FATAL", new Exception("I'm a fake"));
        }

        private static void LogProgramStats()
        {
            var thisProcess = Process.GetCurrentProcess();
            var stats = new
            {
                Platform = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                ExecutionTime = (DateTime.Now - thisProcess.StartTime).TotalSeconds,
                PeakWorkingSet = thisProcess.PeakWorkingSet64,
            };

            var statsString = "Programs stats:\n" + Json.SerialiseToString(stats, prettyPrint: true);

            NoConsole.Log.Info(statsString);
        }

        /// <summary>
        /// This method is used to do application wide loading of native code.
        /// </summary>
        /// <remarks>
        /// Until we convert this application to a .NET Core, there is no support for "runtimes" backed into the build
        /// system. Thus instead we:
        /// - copy runtimes manually as a build step
        ///   (due to a mono bug, the folder to copy in is named `libruntimes`. See https://github.com/libgit2/libgit2sharp/issues/1170)
        /// - map Dlls to their appropriate native DLLs in the dllmap entried in the App.config (which is used by the
        ///   mono runtime
        /// - and finally, call any intialization code that is needed here in this method.
        /// </remarks>
        private static void LoadNativeCode()
        {
            Log.Debug("Loading native code");

            // for sqlite
            // note: a custom dll map for sqlite can be found in SQLitePCLRaw.provider.e_sqlite3.dll.config
            SQLitePCL.Batteries_V2.Init();
        }
    }
}
