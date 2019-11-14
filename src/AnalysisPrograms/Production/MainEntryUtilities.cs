// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainEntryUtilities.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MainEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Contracts;
    using Acoustics.Shared.Logging;
    using log4net.Core;
#if DEBUG
    using Acoustics.Shared.Debugging;
#endif
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Parsers;
    using Fasterflect;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using static System.Environment;

    public static partial class MainEntry
    {
        // ReSharper disable once InconsistentNaming
#if DEBUG
        public const bool InDEBUG = true;
#else
        public const bool InDEBUG = false;
#endif

        public const string ApDefaultLogVerbosityKey = "AP_DEFAULT_LOG_VERBOSITY";
        public const string ApPlainLoggingKey = "AP_PLAIN_LOGGING";
        public const string ApMetricsKey = "AP_METRICS";
        public const string ApAutoAttachKey = "AP_AUTO_ATTACH";

        public static readonly Dictionary<string, string> EnvironmentOptions =
            new Dictionary<string, string>
            {
                {
                    ApPlainLoggingKey,
                    "<true|false>\tEnable simpler logging - the default value is `false`"
                },
                {
                    ApMetricsKey,
                    "<true|false>\t(Not implemented) Enable or disable metrics - default value is `true`"
                },
                {
                    ApDefaultLogVerbosityKey,
                    $"<{typeof(LogVerbosity).PrintEnumOptions()}>\tSet the default log level"
                },
#if DEBUG
                {
                    ApAutoAttachKey,
                    "<true|false>\tEnable or disable auto attach for debugging - default value is `false`"
                },
#endif
            };

        internal enum Usages
        {
            All,
            Single,
            ListAvailable,
            NoAction,
        }

        public static bool AppConfigOverridden { get; private set; }

        /// <summary>
        /// Gets the default log level set by an environment variable.
        /// </summary>
        public static LogVerbosity? ApDefaultLogVerbosity { get; private set; } = null;

        /// <summary>
        /// Gets a value indicating whether or not we should use simpler logging semantics. Usually means no color.
        /// </summary>
        public static bool ApPlainLogging { get; private set; }

        /// <summary>
        /// Gets a value indicating whether we will submit metrics to the remote metric server.
        /// </summary>
        public static bool ApMetricRecording { get; private set; }

        public static CommandLineApplication CommandLineApplication { get; private set; }

        public static bool IsDebuggerAttached => Debugger.IsAttached;

        public static Logging Logging { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the debugger should automatically attach.
        /// </summary>
        internal static bool ApAutoAttach { get; private set; }

        public static void SetLogVerbosity(LogVerbosity logVerbosity, bool quietConsole = false)
        {
            var modifiedLevel = VerbosityToLevel(logVerbosity);

            Logging.ModifyVerbosity(modifiedLevel, quietConsole);
            Log.Debug("Log level changed to: " + logVerbosity);

            // log test
            //Logging.TestLogging();
        }

        internal static void AttachDebugger(DebugOptions options)
        {
            if (options == DebugOptions.No)
            {
                return;
            }
#if DEBUG
            if (!Debugger.IsAttached && !IsMsTestRunningMe())
            {
                if (options == DebugOptions.Prompt)
                {
                    var response = Prompt.GetYesNo(
                        "Do you wish to debug? Attach now or press [Y] and [ENTER] to attach. Press [N] or [ENTER] to continue.",
                        defaultAnswer: false,
                        promptColor: ConsoleColor.Cyan);
                    options = response ? DebugOptions.Yes : DebugOptions.No;
                }

                if (options == DebugOptions.Yes || options == DebugOptions.YesSilent)
                {
                    var vsProcess =
                        VisualStudioAttacher.GetVisualStudioForSolutions(
                            new List<string> { "AudioAnalysis.sln" });

                    if (vsProcess != null)
                    {
                        VisualStudioAttacher.AttachVisualStudioToProcess(vsProcess, Process.GetCurrentProcess());
                    }
                    else
                    {
                        // try and attach the old fashioned way
                        Debugger.Launch();
                    }
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (Debugger.IsAttached)
                {
                    if (options != DebugOptions.YesSilent)
                    {
                        LoggedConsole.WriteLine("\t>>> Attach successful");
                        LoggedConsole.WriteLine();
                    }
                }
            }
#endif
        }

        internal static void BeforeExecute(MainArgs main, CommandLineApplication application)
        {
            // re-assign here... the application will be a sub-command here (which is technically a different CLA)
            CommandLineApplication = application;

            AttachDebugger(main.DebugOption);

            ModifyVerbosity(main);

            Log.Debug($"Metric reporting is {(ApMetricRecording ? "en" : "dis")}abled (but not yet functional).");

            LoadNativeCode();
        }

        /// <summary>
        /// If AnalysisPrograms.exe is launched with a 8.3 short name then it
        /// fails to find it's application config. .NET looks for a config named
        /// the same name as the executable. Normally AnalysisPrograms.exe looks
        /// for and finds AnalysisPrograms.exe.config. But if the program is launched
        /// as ANALYS~1.EXE it tries to search for ANALYS~2.EXE.config which does not
        /// exist.
        /// <para>
        /// This has shown to be an issue with R's system and system2 functions. They
        /// translate the executable function using `Sys.which` which automatically
        /// converts executable names to 8.3 short format on Windows.
        /// </para>
        /// <para>
        /// This method checks if this is the case and overrides the setting for
        /// checking where to find a config file.
        /// </para>
        /// </summary>
        /// <returns><value>True</value> if modifications were made.</returns>
        internal static bool CheckAndUpdateApplicationConfig()
        {
            // TODO: DOTNET CORE - we can expect most of this method to not work, it needs to
            // tested again.
            var executableName = Process.GetCurrentProcess().MainModule.ModuleName;
            var expectedName = Assembly.GetAssembly(typeof(MainEntry)).ManifestModule.ScopeName;

            Log.Verbose($"Executable name is {executableName} and expected name is {expectedName}");

            if (expectedName != executableName)
            {
                var correctConfig = expectedName + ".config";
                Log.Verbose($"Updating AppDomain APP_CONFIG_FILE to point to `{correctConfig}`");
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", correctConfig);
                AppDomain.CurrentDomain.SetupInformation.ConfigurationFile = correctConfig;
                AppDomain.CurrentDomain.SetupInformation.ApplicationName = expectedName;
                ResetConfigMechanism(AppDomain.CurrentDomain.SetupInformation);
                return true;
            }

            return false;

            void ResetConfigMechanism(AppDomainSetup setup)
            {
                // https://stackoverflow.com/a/6151688/224512
                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
                var mangerType = typeof(ConfigurationManager);

                mangerType.SetFieldValue("s_initState", 0, bindingFlags);
                mangerType.SetFieldValue("s_configSystem", null, bindingFlags);

                typeof(ConfigurationManager)
                    .Assembly
                    .GetTypes()
                    .First(x => x.FullName == "System.Configuration.ClientConfigPaths")
                    .SetFieldValue("s_current", null, bindingFlags);

                // finally force fusion to reload the current app domain
                // https://referencesource.microsoft.com/#mscorlib/system/appdomain.cs,3515
                var setupMethod = typeof(AppDomain)
                    .GetMethod(
                        "SetupFusionStore",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                setupMethod.Invoke(AppDomain.CurrentDomain, new object[] { setup, null });
            }
        }

        /// <summary>
        /// Checks to see whether we can load a DLL that we depend on from the GAC.
        /// We have to check this early on or else we get some pretty hard to
        /// diagnose errors (and also because our CLI parser depends on it).
        /// </summary>
        /// <returns>A message if there is a problem.</returns>
        internal static string CheckForDataAnnotations()
        {
            // https://github.com/QutEcoacoustics/audio-analysis/issues/225
            try
            {
                Assembly.Load(
                    "System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                return null;
            }
            catch (FileNotFoundException fnfex)
            {
                return $@"{fnfex.ToString()}
!
!
!
In cases where System.ComponentModel.DataAnnotations is not found we have
previously found that the mono install is corrupt. Try installing mono again
and make sure you install the `mono-complete` package.
!
!
!";
            }
        }

        internal static void Copyright()
        {
            LoggedConsole.WriteLine(
                $@"{Meta.Description} - version {BuildMetadata.VersionString} ({(InDEBUG ? "DEBUG" : "RELEASE")} build, {BuildMetadata.BuildDate}){NewLine}" +
                $@"Git branch-version: {BuildMetadata.GitBranch}-{BuildMetadata.GitCommit}, DirtyBuild:{BuildMetadata.IsDirty}, CI:{BuildMetadata.CiBuild}{NewLine}" +
                $@"Copyright {Meta.NowYear} {Meta.Organization}");
        }

        internal static void WarnIfDeveloperEntryUsed(string message = null)
        {
            if (!InDEBUG)
#pragma warning disable 162
            {
                message = message == null ? string.Empty : "\n!    " + message;
                Log.Warn($@"!
!
!    The entry point called is designed for use by developers and debuggers.
!    It is likely that this entry point does not do what you want and will fail.{message}
!
!");
            }
#pragma warning restore 162
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

            if (IsMsTestRunningMe())
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

#if DEBUG
        internal static bool IsMsTestRunningMe()
        {
            // https://stackoverflow.com/questions/3167617/determine-if-code-is-running-as-part-of-a-unit-test
            string testAssemblyName = "Microsoft.VisualStudio.TestPlatform.TestFramework";
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.FullName.StartsWith(testAssemblyName));
        }
#endif

        internal static void PrintUsage(string message, Usages usageStyle, string commandName = null)
        {
            //Contract.Requires(usageStyle != Usages.Single || commandName != null);

            var root = CommandLineApplication.Root();

            if (!string.IsNullOrWhiteSpace(message))
            {
                LoggedConsole.WriteErrorLine(message);
            }

            if (usageStyle == Usages.All)
            {
                // print entire usage
                root.ShowHelp(false);
            }
            else if (usageStyle == Usages.Single)
            {
                CommandLineApplication command;
                if (commandName == root.Name)
                {
                    command = root;
                }
                else
                {
                    command = root.Commands.FirstOrDefault(x =>
                        x.Name.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));

                    // sometimes this is called from AppDomainUnhandledException, in which case throwing another exception
                    // just gets squashed!
                    if (command == null)
                    {
                        var commandNotFoundMessage = $"Could not find a command with name that matches `{commandName}`.";
                        Log.Fatal(commandNotFoundMessage);

                        throw new CommandParsingException(CommandLineApplication, commandNotFoundMessage);
                    }
                }

                command.ShowHelp(false);
            }
            else if (usageStyle == Usages.ListAvailable)
            {
                var commands = root.Commands;

                using (var sb = new StringWriter())
                {
                    ((CustomHelpTextGenerator)CommandLineApplication.HelpTextGenerator).FormatCommands(sb, commands);

                    LoggedConsole.WriteLine(sb.ToString());
                }
            }
            else if (usageStyle == Usages.NoAction)
            {
                CommandLineApplication.ShowHint();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        internal static CommandLineApplication CreateCommandLineApplication()
        {
            var console = new PhysicalConsoleLogger();
            var app = CommandLineApplication = new CommandLineApplication<MainArgs>(console);
            app.UsePagerForHelpText = false;
            app.ClusterOptions = false;
            app.HelpTextGenerator = new CustomHelpTextGenerator { EnvironmentOptions = EnvironmentOptions };
            app.ValueParsers.AddOrReplace(new DateTimeOffsetParser());
            app.ValueParsers.AddOrReplace(new TimeSpanParser());
            app.ValueParsers.AddOrReplace(new FileInfoParser());
            app.ValueParsers.AddOrReplace(new DirectoryInfoParser());
            app.Conventions.UseDefaultConventions();

            return app;
        }

        /// <summary>
        /// Attaches an exception handler and also does a check
        /// to see if necessary DLLs exist. WARNING: can quit process!.
        /// </summary>
        private static void PrepareForErrors()
        {
            ExitCode = ExceptionLookup.UnhandledExceptionErrorCode;

            AppConfigOverridden = CheckAndUpdateApplicationConfig();

            if (CheckForDataAnnotations() is string message)
            {
                Console.WriteLine(message);
                Exit(ExceptionLookup.UnhandledExceptionErrorCode);
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Contract.Requires(unhandledExceptionEventArgs != null);
            Contract.Requires(unhandledExceptionEventArgs.ExceptionObject != null);

            const string fatalMessage = "Fatal error:\n  ";

            var ex = (Exception)unhandledExceptionEventArgs.ExceptionObject;

            ExceptionLookup.ExceptionStyle style;
            bool found;
            Exception inner = ex;

            // TODO: it looks like all exceptions will always be wrapped in a TargetInvocationException now so we always want to unwrap at least once
            switch (ex)
            {
                case TargetInvocationException _:
                case AggregateException _:
                    // unwrap
                    inner = ex.InnerException ?? ex;
                    Log.Debug($"Unwrapped {ex.GetType().Name} exception to show a  {inner.GetType().Name}");
                    found = ExceptionLookup.ErrorLevels.TryGetValue(inner.GetType(), out style);
                    break;
                default:
                    found = ExceptionLookup.ErrorLevels.TryGetValue(ex.GetType(), out style);
                    break;
            }

            found = found && style.Handle;

            // format the message
            string message = fatalMessage + (style?.FormatMessage?.Invoke(inner, Log.IsVerboseEnabled()) ?? inner.Message);

            // if found, print message only if usage printing disabled
            if (found && !style.PrintUsage)
            {
                // this branch prints the message, but the stack trace is only output in the log
                NoConsole.Log.Fatal(message, ex);
                LoggedConsole.WriteFatalLine(message);
            }
            else if (found && ex.GetType() != typeof(Exception))
            {
                // this branch prints the message, and command usage, but the stack trace is only output in the log
                NoConsole.Log.Fatal(message, ex);

                // the static CommandLineApplication is not set when CommandLineException is thrown
                var command = inner is CommandParsingException exception ? exception.Command.Name : CommandLineApplication?.Name;
                PrintUsage(message, Usages.Single, command ?? string.Empty);
            }
            else
            {
                // otherwise its a unhandled exception, log and raise
                // trying to print cleaner errors in console, so printing a full one to log, and the inner to the console
                // this results in duplication in the log though
                NoConsole.Log.Fatal("Unhandled exception ->\n", ex);
                Log.Fatal("Unhandled exception ->\n", inner);

                PrintAggregateException(ex);
            }

            int returnCode = style?.ErrorCode ?? ExceptionLookup.UnhandledExceptionErrorCode;

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + returnCode);
            if (Debugger.IsAttached)
            {
                // no don't exit, we want the exception to be raised to Window's Exception handling
                // this will allow the debugger to appropriately break on the right line
                ExitCode = returnCode;
            }
            else
            {
                // If debugger is not attached, we *do not* want to raise the error to the Windows level
                // Everything has already been logged, just exit with appropriate errorlevel
                Exit(returnCode);
            }
        }

        /// <summary>
        /// This method is used to do application wide loading of native code.
        /// </summary>
        /// <remarks>
        /// Until we convert this application to a .NET Core, there is no support for "runtimes" backed into the build
        /// system. Thus instead we:
        /// - copy runtimes manually as a build step
        ///   (due to a mono bug, the folder to copy in is named `libruntimes`. See https://github.com/libgit2/libgit2sharp/issues/1170)
        /// - map Dlls to their appropriate native DLLs in the dllmap entry in the App.config (which is used by the
        ///   mono runtime
        /// - and finally, call any initialization code that is needed here in this method.
        /// </remarks>
        private static void LoadNativeCode()
        {
            Log.Debug("Loading native code");

            // for sqlite
            // note: a custom dll map for sqlite can be found in SQLitePCLRaw.provider.e_sqlite3.dll.config
            SQLitePCL.Batteries_V2.Init();
        }

        private static void LogProgramStats()
        {
            var thisProcess = Process.GetCurrentProcess();
            var stats = new
            {
                Platform = OSVersion.ToString(),
                ProcessorCount,
                ExecutionTime = (DateTime.Now - thisProcess.StartTime).TotalSeconds,
                PeakWorkingSet = thisProcess.PeakWorkingSet64,
                AppConfigOverridden,
            };

            var statsString = "Programs stats:\n" + Json.SerializeToString(stats, prettyPrint: true);

            NoConsole.Log.Info(statsString);
        }

        private static void ModifyVerbosity(MainArgs arguments)
        {
            SetLogVerbosity(arguments.LogLevel, arguments.QuietConsole);
        }

        private static void ParseEnvironment()
        {
            var verbosityVariable = GetEnvironmentVariable(ApDefaultLogVerbosityKey);
            if (verbosityVariable.IsNotWhitespace()
                && Enum.TryParse(verbosityVariable, true, out LogVerbosity verbosity))
            {
                ApDefaultLogVerbosity = verbosity;
            }

            ApPlainLogging = bool.TryParse(GetEnvironmentVariable(ApPlainLoggingKey), out var plainLogging) && plainLogging;

            // default value is true
            ApMetricRecording = !bool.TryParse(GetEnvironmentVariable(ApMetricsKey), out var parseMetrics) || parseMetrics;

            ApAutoAttach = bool.TryParse(GetEnvironmentVariable(ApAutoAttachKey), out var autoAttach) && autoAttach;
        }

        private static void PrintAggregateException(Exception ex, int depth = 0)
        {
            var depthString = "==".PadLeft(depth * 2, '=');

            //innerExceptions = innerExceptions ?? new StringBuilder();

            if (ex is AggregateException aex)
            {
                //innerExceptions.AppendLine("Writing detailed information about inner exceptions!");

                foreach (var exception in aex.InnerExceptions)
                {
                    //innerExceptions.AppendLine();
                    Log.Fatal("\n\n" + depthString + "> Inner exception:", exception);

                    if (exception is AggregateException)
                    {
                        PrintAggregateException(exception, depth++);
                    }
                }
            }
        }

        private static Level VerbosityToLevel(LogVerbosity logVerbosity)
        {
            Level modifiedLevel;
            switch (logVerbosity)
            {
                case LogVerbosity.None:

                    // we never turn the logger completely off - sometimes the logger just really needs to log something.
                    modifiedLevel = LogExtensions.PromptLevel;
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

            return modifiedLevel;
        }
    }
}
