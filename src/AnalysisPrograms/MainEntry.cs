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
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using AnalysisPrograms.Production.Parsers;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using McMaster.Extensions.CommandLineUtils.Abstractions;
    using Production;
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
            // HACK: Disable the following two line when argument refactoring is done
            //var options = DebugOptions.Yes;
            //AttachDebugger(ref options);

            ParseEnvirionemnt();

            Copyright();

            AttachExceptionHandler();

            NoConsole.Log.Info($"Executable called with these arguments: {NewLine}{CommandLine}{NewLine}");

            // Note: See MainEntry.BeforeExecute for commands run before invocation.
            // note: Exception handling can be found in CurrentDomainOnUnhandledException
            var console = PhysicalConsoleLogger.Default;
            var app = CommandLineApplication = new CommandLineApplication<MainArgs>(console);

            app.HelpTextGenerator = new CustomHelpTextGenerator { EnvironmentOptions = EnvironmentOptions };
            app.ValueParsers.Add(new DateTimeOffsetParser());
            app.ValueParsers.Add(new TimeSpanParser());
            app.ValueParsers.Add(new FileInfoParser());
            app.ValueParsers.Add(new DirectoryInfoParser());
            app.Conventions.UseDefaultConventions();

            var result = await Task.FromResult(app.Execute(args));

            LogProgramStats();

            HangBeforeExit();

            // finally return error level
            NoConsole.Log.Info("ERRORLEVEL: " + result);
            return result;
        }
    }
}
