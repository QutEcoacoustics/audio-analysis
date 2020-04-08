// <copyright file="AnalyserArguments.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Production.Arguments
{
    using System.IO;
    using AnalysisBase;
    using AnalysisPrograms.Production.Validation;
    using McMaster.Extensions.CommandLineUtils;

    public abstract class AnalyserArguments
        : SourceConfigOutputDirArguments
    {
        [Option(Description = "The start offset to start analysing from (in seconds)")]
        [InRange(min: 0)]
        public double? Start { get; set; }

        [Option(Description = "The duration of each segment to analyse (seconds) - a maximum of 10 minutes")]
        [InRange(0, 10 * 60)]
        public double? Duration { get; set; }

        public (AnalysisSettings, SegmentSettings<FileInfo>) ToAnalysisSettings(
            AnalysisSettings defaults = null,
            bool outputIntermediate = false,
            FileSegment sourceSegment = null,
            FileSegment preparedSegment = null)
        {
            var analysisSettings = base.ToAnalysisSettings(defaults, true);

            var segment = sourceSegment ?? new FileSegment(this.Source, TimeAlignment.None);
            var segmentSettings = new SegmentSettings<FileInfo>(
                analysisSettings,
                segment,
                (analysisSettings.AnalysisOutputDirectory, analysisSettings.AnalysisTempDirectory),
                preparedSegment ?? segment);

            return (analysisSettings, segmentSettings);
        }
    }
}