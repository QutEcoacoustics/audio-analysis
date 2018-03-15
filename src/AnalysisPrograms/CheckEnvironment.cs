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
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools.Audio;
    using AnalyseLongRecordings;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using Production.Arguments;

    public class CheckEnvironment
    {
        public const string CommandName = "CheckEnvironment";

        private static readonly ILog Log = LogManager.GetLogger(nameof(CheckEnvironment));

        private void Execute(Arguments arguments)
        {
            Log.Info("Checking required executables can be found");

            // master audio utlility checks for available executables
            var utility = new MasterAudioUtility();

            if (AppConfigHelper.IsMono)
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type != null)
                {
                    MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (displayName != null)
                    {
                        var name = displayName.Invoke(null, null);
                        Log.Info($"Mono version is {name}, we require at least Mono 5.5");
                    }
                    else
                    {
                        Log.Warn("Could not check Mono version");
                    }
                }
            }

            // don't have much more to check at the current time
            Log.Success("Valid environment");
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
