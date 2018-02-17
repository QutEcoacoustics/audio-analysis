// <copyright file="SourceConfigOutputDirArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using AnalysisBase;
    using McMaster.Extensions.CommandLineUtils;
    using Validation;

    public abstract class SourceConfigOutputDirArguments
        : SourceAndConfigArguments
    {
        [Argument(
            2,
            Description = "A directory to write output to")]
        [Required]
        [DirectoryExistsOrCreate(createIfNotExists: true)]
        [LegalFilePath]
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
            Config configuration = null)
        {
            var analysisSettings = defaults ?? new AnalysisSettings();

            analysisSettings.ConfigFile = this.Config.ToFileInfo();

            var output = this.Output;
            var resultDirectory = resultSubDirectory.IsNullOrEmpty() ? output : output.Combine(resultSubDirectory);
            resultDirectory.Create();

            analysisSettings.AnalysisOutputDirectory = output;
            analysisSettings.AnalysisTempDirectory = output;

            if (outputIntermediate)
            {
                analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;
                analysisSettings.AnalysisDataSaveBehavior = true;
            }

            analysisSettings.Configuration = configuration ?? ConfigFile.Deserialize(this.Config);

            return analysisSettings;
        }
    }
}