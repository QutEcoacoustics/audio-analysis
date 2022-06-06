// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainEntry.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the MainEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Threading.Tasks;
    using Acoustics.Shared.Logging;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Spectre.Console;
    using log4net;
    using Spectre.Console;
    using static System.Environment;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public static partial class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainEntry));

        public static async Task<int> Main(string[] args)
        {
            ParseEnvironment();

            // Uses an env var to  attach debugger before argument parsing
            AttachDebugger(ApAutoAttach ? DebugOptions.YesSilent : DebugOptions.No);

            Logging = new Logging(
                colorConsole: !ApPlainLogging,
                VerbosityToLevel(ApDefaultLogVerbosity ?? LogVerbosity.Info),
                quietConsole: false);

            // ensure spectre console fancy-ness is logged
            AnsiConsole.Console = new LoggedAnsiConsole();

            Copyright();

            PrepareForErrors();

            NoConsole.Log.Info($"Executable called with these arguments: {NewLine}{CommandLine}{NewLine}");

            var app = CreateCommandLineApplication();

            // Note: See MainEntry.BeforeExecute for commands run before invocation.
            // Note: Exception handling can be found in CurrentDomainOnUnhandledException.
            var result = await Task.FromResult(app.Execute(args));

            LogProgramStats();

            HangBeforeExit();

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + result);
            return result;
        }
    }
}