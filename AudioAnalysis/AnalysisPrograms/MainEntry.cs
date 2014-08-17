// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainEntry.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the MainEntry type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    #if DEBUG
#endif
    using System;
    using System.Reflection;

    using log4net;

    /// <summary>
    /// Main Entry for Analysis Programs.
    /// </summary>
    public static partial class MainEntry
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static int Main(string[] args)
        {
            Copyright();

            AttachExceptionHandler();

            NoConsole.Log.Info("Executable called with these arguments: {1}{0}{1}".Format2(Environment.CommandLine, Environment.NewLine));

            // HACK: Remove the following two line when argument refactoring is done
            ////var options = DebugOptions.Yes;
            ////AttachDebugger(ref options);

            Arguments = ParseArguments(args);

            var debugOptions = Arguments.Args.DebugOption;
            AttachDebugger(ref debugOptions);

            // note: Exception handling can be found in CurrentDomainOnUnhandledException
            Execute(Arguments);

            
            HangBeforeExit();

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + ExceptionLookup.Ok);
            return ExceptionLookup.Ok;
        }
    }
}
