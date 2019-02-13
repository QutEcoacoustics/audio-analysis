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
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared.Logging;
    using log4net;
    using log4net.Core;
    using Production.Arguments;
    using static System.Environment;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public static partial class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(MainEntry));

        public static async Task<int> Main(string[] args)
        {
            ParseEnvirionemnt();

            // Uses an env var to  attach debugger before argument parsing
            AttachDebugger(ApAutoAttach ? DebugOptions.YesSilent : DebugOptions.No);

            Logging = new Logging(colorConsole: !ApPlainLogging, Level.Info, quietConsole: false);

            Copyright();

            AttachExceptionHandler();

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
