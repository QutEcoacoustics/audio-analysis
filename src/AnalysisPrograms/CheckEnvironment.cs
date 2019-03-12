// <copyright file="CheckEnvironment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalysisPrograms.AnalyseLongRecordings;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;

    public class CheckEnvironment
    {
        public const string CommandName = "CheckEnvironment";

        private static readonly ILog Log = LogManager.GetLogger(nameof(CheckEnvironment));

        private int Execute(Arguments arguments)
        {
            var errors = new List<string>();
            Log.Info("Checking required executables can be found");

            // master audio utility checks for available executables
            try
            {
                var utility = new MasterAudioUtility();
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }

            if (MainEntry.CheckForDataAnnotations() is string message)
            {
                errors.Add(message);
            }

            if (AppConfigHelper.IsMono)
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type != null)
                {
                    MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);

                    if (displayName?.Invoke(null, null) is string name)
                    {
                        var version = Regex.Match(name, @".*(\d+\.\d+\.\d+\.\d+).*").Groups[1].Value;
                        Console.WriteLine(version);
                        if (new Version(version) > new Version(5, 5))
                        {
                            Log.Success($"Your mono version {name} is greater than our required Mono version 5.5");
                        }
                        else
                        {
                            errors.Add($"Mono version is {name}, we require at least Mono 5.5");
                        }
                    }
                    else
                    {
                        errors.Add("Could not get Mono display name");
                    }
                }
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
