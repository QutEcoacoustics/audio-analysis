// <copyright file="AnalyzerConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using Acoustics.Shared.ConfigFile;

    public class AnalyzerConfig
        : Config
    {
        public const double EventThresholdDefault = 0.2;

        public string AnalysisName { get; set; }

        public double EventThreshold { get; set; } = EventThresholdDefault;

        /// <summary>
        /// Gets or sets the length of audio block to process
        /// </summary>
        public double? SegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the amount that each audio block should overlap
        /// </summary>
        public double? SegmentOverlap { get; set; }

        /// <summary>
        /// Gets or sets the default sample rate to reencode all input audio as.
        /// </summary>
        /// <remarks>
        /// ResampleRate must be 2 X the desired Nyquist.
        /// Default value = 22050.
        /// Once upon a time we used 17640.
        /// Units=samples
        /// Users of this value should always fallback to a default appropriate for the analysis. Currently that default
        /// must be non-null, but we're considering allowing it to be null to support variable sample rate analysis.
        /// </remarks>
        public int? ResampleRate { get; set; }

        public SaveBehavior SaveIntermediateWavFiles { get; set; } = SaveBehavior.Never;

        public bool SaveIntermediateCsvFiles { get; set; } = false;

        public SaveBehavior SaveSonogramImages { get; set; } = SaveBehavior.Never;

        /// <summary>
        /// Gets or sets a value indicating whether a file must have a date in the file name
        /// </summary>
        /// <remarks>
        /// if true, an unambiguous date time must be provided in the source file's name.
        /// if true, an exception will be thrown if no such date is found
        /// if false, and a valid date is still found in file name, it will still be parsed
        /// supports formats like:
        ///     prefix_20140101T235959+1000.mp3
        ///     prefix_20140101T235959+Z.mp3
        ///     prefix_20140101-235959+1000.mp3
        ///     prefix_20140101-235959+Z.mp3
        /// </remarks>
        public bool RequireDateInFilename { get; set; } = false;
    }
}
