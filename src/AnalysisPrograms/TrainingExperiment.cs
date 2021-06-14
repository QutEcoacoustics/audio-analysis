// <copyright file="TrainingExperiment.cs" company="QutEcoacoustics">
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
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;

    public class TrainingExperiment
    {
        public const string CommandName = "TrainingExperiment";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<int> Execute(Arguments arguments)
        {
            Log.Warn("I'm alive!");

            // scan a directory
            var input = arguments.InputDataDirectory;

            // ...
            // read your files in
            // do the mfcc transform
            // train the model
            // save the model

            return ExceptionLookup.Ok;
        }

        [Command(
            Name = CommandName,
            Description = "[ALPHA] Build a model based off of MFCCs")]
        public class Arguments : SubCommandBase
        {
            // add you desired CLI arguments here as properties
            [Option(Description = "Directory where the input data is located.")]
            [DirectoryExists]
            [LegalFilePath]
            public string InputDataDirectory { get; set; }

            // see other Arguments classes for examples

            public override async Task<int> Execute(CommandLineApplication app)
            {
                return await TrainingExperiment.Execute(this);
            }
        }
    }
}
