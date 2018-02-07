// <copyright file="SourceConfigOutputDirArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Acoustics.Shared;
    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;
    using Validation;

    public abstract class SourceConfigOutputDirArguments
        : SourceAndConfigArguments
    {
        [Option(
            "A directory to write output to",
            ShortName = "o",
            ValueName = "FILE")]
        [Required]
        [DirectoryExistsOrCreate(createIfNotExists: true)]
        public virtual DirectoryInfo Output { get; set; }

        /// <summary>
        /// Helper method used for Execute and Dev entry points. Mocks the values normally set by analysis coordinator.
        /// </summary>
        /// <param name="defaults">
        /// The default AnalysisSettings used - usually from the IAnalyzer2 interface.
        /// </param>
        /// <param name="outputIntermediate">
        /// The output Intermediate switch - true to use the default writing behavior.
        /// </param>
        /// <param name="resultSubDirectory">Path to further nest results</param>
        /// <param name="configuration">The configuration object to use</param>
        /// <returns>
        /// An AnalysisSettings object.
        /// </returns>
        public virtual AnalysisSettings ToAnalysisSettings(
            AnalysisSettings defaults = null,
            bool outputIntermediate = false,
            string resultSubDirectory = null,
            dynamic configuration = null)
        {
            var analysisSettings = defaults ?? new AnalysisSettings();

            analysisSettings.ConfigFile = this.Config;

            var resultDirectory = resultSubDirectory.IsNullOrEmpty() ? this.Output : this.Output.Combine(resultSubDirectory);
            resultDirectory.Create();

            analysisSettings.AnalysisOutputDirectory = this.Output;
            analysisSettings.AnalysisTempDirectory = this.Output;

            if (outputIntermediate)
            {
                analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;
                analysisSettings.AnalysisDataSaveBehavior = true;
            }

            analysisSettings.Configuration = configuration ?? Yaml.Deserialise(this.Config);

            return analysisSettings;
        }
    }
}