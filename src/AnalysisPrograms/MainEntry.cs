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
    using log4net;
    using Production.Arguments;
    using static System.Environment;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public static partial class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<int> Main(string[] args)
        {
            // HACK: Use the following two lines when debugger needs to be attached before argument parsing
            //var options = DebugOptions.Yes;
            //AttachDebugger(ref options);

            ParseEnvirionemnt();

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
