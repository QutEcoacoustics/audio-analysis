// <copyright file="AnalyzerConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using System;
    using Acoustics.Shared.ConfigFile;

    public class AnalyzerConfig
        : Config
    {
        public const double EventThresholdDefault = 0.2;

        [Obsolete("The AnalysisName property is no longer used")]
        public string AnalysisName { get; set; }

        public double EventThreshold { get; set; } = EventThresholdDefault;

        /// <summary>
        /// Gets or sets the length of audio block to process.
        /// </summary>
        public double? SegmentDuration { get; set; }

        /// <summary>
        /// Gets or sets the amount that each audio block should overlap.
        /// </summary>
        public double? SegmentOverlap { get; set; }

        /// <summary>
        /// Gets or sets the default sample rate to re-encode all input audio as.
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
        /// Gets or sets a value indicating whether a file must have a date in the file name.
        /// </summary>
        /// <remarks>
        /// If true, an unambiguous date-time must be provided in the source file's name.
        /// If true, an exception will be thrown if no such date is found.
        /// If false, and a valid date is still found in file name, it will still be parsed.
        /// Supports formats like:
        ///      prefix_20140101T235959+1000.wav,  where +1000 is in this case the time-zone offset for Brisbane.
        ///      prefix_20140101T235959+Z.wav,     where +Z is the zero time-zone offset.
        ///      prefix_20140101-235959+1000.wav
        ///      prefix_20140101-235959+Z.wav
        /// For more info on dates, see "dates.md" at https://github.com/QutEcoacoustics/audio-analysis/tree/master/docs.
        /// </remarks>
        public bool RequireDateInFilename { get; set; } = false;
    }
}