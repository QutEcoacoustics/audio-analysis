// <copyright file="CheckEnvironment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalysisBase;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;

    public class CheckEnvironment
    {
        public const string CommandName = "CheckEnvironment";

        private static readonly ILog Log = LogManager.GetLogger(typeof(CheckEnvironment));

        private int Execute(Arguments arguments)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            Log.Info("Checking required executables and libraries can be found and loaded");

            // this is an important call used in analyze long recordings.
            // This call effectively check is we can load types and if files are present (I think)
            try
            {
                AnalysisCoordinator.GetAnalyzers<IAnalyser2>(typeof(MainEntry).Assembly);
            }
            catch (ReflectionTypeLoadException rtlex)
            {
                errors.Add(ExceptionLookup.FormatReflectionTypeLoadException(rtlex, true));
            }

            // master audio utility checks for available executables
            try
            {
                var utility = new MasterAudioUtility();
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }

            if (AppConfigHelper.WvunpackExe == null)
            {
                warnings.Add("Cannot find wvunpack - we'll be unable to process any wavpack files.");
            }

            if (!new SoxAudioUtility(new FileInfo(AppConfigHelper.SoxExe)).SupportsMp3)
            {
                warnings.Add(SoxAudioUtility.Mp3NotSupportedOnOSX);
            }

            if (MainEntry.CheckForDataAnnotations() is string message)
            {
                errors.Add(message);
            }

            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                errors.Add($"We no longer use Mono with ${Meta.Name}. DO NOT prefix the {Meta.Name} prefix with `mono`.");
            }

            foreach (var warning in warnings)
            {
                Log.Warn(warning);
            }

            // don't have much more to check at the current time
            if (errors.Count == 0)
            {
                Log.Success("Valid environment");

                return ExceptionLookup.Ok;
            }
            else
            {
                foreach (var error in errors)
                {
                    Log.Error(error);
                }

                // not using exception lookup on purpose - it's static constructor loads more types
                return ExceptionLookup.UnhandledExceptionErrorCode;
            }
        }

        [Command(
            CommandName,
            Description = "Tests that the required external dependencies are available")]
        public class Arguments : SubCommandBase
        {
            public override Task<int> Execute(CommandLineApplication app)
            {
                var instance = new CheckEnvironment();
                return Task.FromResult(instance.Execute(this));
            }
        }

    }
}
