using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalysisPrograms
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Acoustics.Shared.Debugging;

    using AnalysisPrograms.Production;

    using PowerArgs;

    using log4net;

    public static partial class MainEntry
    {
        internal static ArgAction<MainEntryArguments> Arguments { get; set; }

        private static void Copyright()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            LoggedConsole.WriteLine(
                "QUT Bioacoustic Analysis Program - version " + fvi.FileVersion + "\n"
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
                return false;
            }
        }

        private static void AttachExceptionHandler()
        {
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

        private static ArgAction<MainEntryArguments> ParseArguments(string[] args)
        {
            ArgUsage.RegisterHook(null, new CustomUsageHook());
            return Args.ParseAction<MainEntryArguments>(args);
        }

        /// <summary>
        /// Run the selected action
        /// </summary>
        private static void Execute(ArgAction<MainEntryArguments> arguments)
        {
            // just a pointer for executing dev methods
            // however, actions with no args get a plain old Object, so don't log in that case
            if (arguments.EmptyArgActionValue && arguments.ActionArgsProperty.PropertyType != typeof(object))
            {

                if (!InDEBUG)
                {
                    throw new ArgumentException("Must provide arguments to an analysis in a RELEASE build.");
                }

                Log.Warn("Empty (null) analysis arguments recieved. A Dev method should be executed.");
            }

            arguments.Invoke();
        }

        internal enum Usages
        {
            All,
            Single,
            ListAvailable,
            NoAction
        }

        private static readonly ArgUsageOptions UsagePrintOptions = new ArgUsageOptions()
                                                                   {
                                                                       CompactFormat = true,
                                                                       ShortcutThenName = true,
                                                                       ShowColumnHeaders = false,
                                                                       ShowPosition = true,
                                                                       ShowType = true,
                                                                       NoOptionsMessage = "<< no arguments >>"
                                                                   };

        internal static void PrintUsage(string message, Usages usageStyle, string actionName = null)
        {
            Contract.Requires(usageStyle != Usages.Single || actionName != null);

            if (!String.IsNullOrWhiteSpace(message))
            {
                LoggedConsole.WriteLine(message);
            }

            // TODO print additional usage (i.e. IUSAGE)

            if (usageStyle == Usages.All)
            {
                // print entire usage

                LoggedConsole.WriteLine(ArgUsage.GetStyledUsage<MainEntryArguments>(options: UsagePrintOptions));
            }
            else if (usageStyle == Usages.Single)
            {
                var usage = ArgUsage.GetStyledUsage<MainEntryArguments>(options: UsagePrintOptions, includedActions: new[] { actionName });
                LoggedConsole.WriteLine(usage);
            }
            else if (usageStyle == Usages.ListAvailable)
            {
                var actions = ArgUsage.GetActionsList<MainEntryArguments>();

                var sb = new StringBuilder();

                sb.AppendLine("Available actions - ");

                foreach (var tuple in actions)
                {
                    sb.AppendLine("\t" + tuple.Item2 + (string.IsNullOrWhiteSpace(tuple.Item3) ? "" : " - " + tuple.Item3));
                }

                LoggedConsole.WriteLine(sb.ToString());
            }
            else if (usageStyle == Usages.NoAction)
            {
                var usage = ArgUsage.GetStyledUsage<MainEntryArguments>(options: UsagePrintOptions, includedActions: new[] { "list", "help" });
                LoggedConsole.WriteLine(usage);
            }
            else
            {
                throw new InvalidOperationException();
            }

        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;
            int returnCode;

            // print usage, if exception is recognised
            if (ExceptionLookup.ErrorLevels.TryGetValue(ex.GetType(), out returnCode) && ex.GetType() != typeof(Exception))
            {
                // attempt to retrieve action
                string action = null;
                if (Arguments != null)
                {
                    action = ((MainEntryArguments)Arguments.Value).Action;
                }
                else if (ex is ArgException)
                {
                    action = (ex as ArgException).Action;
                }

                if (ex is MissingArgException && ex.Message.Contains("action"))
                {
                    PrintUsage("An action is required to run the program, here are some suggestions:", Usages.NoAction);
                }
                else if (ex is UnknownActionArgException)
                {
                    PrintUsage(ex.Message, Usages.NoAction);
                }
                else if (ex is ValidationArgException)
                {
                    // for validation exceptions, use the inner exception
                    ExceptionLookup.ErrorLevels.TryGetValue(ex.InnerException.GetType(), out returnCode);
                    PrintUsage(ex.Message, Usages.Single, action);
                }
                else
                {
                    PrintUsage(ex.Message, Usages.Single, action);
                }
            }
            else
            {
                // otherwise its a unhandled exception, log and raise
                Log.Fatal("Unhandled exception ->", ex);
                returnCode = ExceptionLookup.SpecialExceptionErrorLevel;
            }

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

        /// <summary>
        /// This method will stop the program from exiting if the solution was built in #DEBUG
        /// and the program was started by Visual Studio.
        /// </summary>
        [Conditional("DEBUG")]
        internal static void HangBeforeExit()
        {
#if DEBUG
            // if Michael is debugging with visual studio, this will prevent the window closing.
            Process parentProcess = ProcessExtensions.ParentProcessUtilities.GetParentProcess();
            if (parentProcess.ProcessName == "devenv")
            {
                LoggedConsole.WriteLine("FINISHED: Press RETURN key to exit.");
                Console.ReadLine();
            }
#endif
        }
    }
}
