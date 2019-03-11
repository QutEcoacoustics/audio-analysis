// <copyright file="CheckEnvironment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalysisPrograms.AnalyseLongRecordings;
    using AnalysisPrograms.Production.Arguments;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;

    public class CheckEnvironment
    {
        public const string CommandName = "CheckEnvironment";

        private static readonly ILog Log = LogManager.GetLogger(nameof(CheckEnvironment));

        private void Execute(Arguments arguments)
        {
            List<Exception> errors = new List<Exception>();
            Log.Info("Checking required executables can be found");

            // master audio utility checks for available executables
            try
            {
                var utility = new MasterAudioUtility();
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }

            if (AppConfigHelper.IsMono)
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type != null)
                {
                    MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (displayName != null)
                    {
                        var name = displayName.Invoke(null, null);
                        var version = Regex.Match(name as string, @".*(\d+\.\d+\.\d+\.\d+).*").Groups[1].Value;
                        Console.WriteLine(version);
                        if (new Version(version) > new Version(5, 5))
                        {
                            Log.Success($"Your mono version {name} is greater than our required Mono version 5.5");
                        }
                        else
                        {
                            errors.Add(new Exception($"Mono version is {name}, we require at least Mono 5.5"));
                        }
                    }
                    else
                    {
                        errors.Add(new Exception("Could not check Mono version"));
                    }
                }
            }

            // don't have much more to check at the current time
            if (errors.Count == 0)
            {
                Log.Success("Valid environment");
            }
            else
            {
                throw new AggregateException(errors.ToArray());
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
                instance.Execute(this);
                return this.Ok();
            }
        }

    }
}
