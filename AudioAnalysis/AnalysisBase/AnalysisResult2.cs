// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisResult2.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   The strong typed analysis results.
//   DO NOT CHANGE THIS CLASS UNLESS YOU ARE TOLD TO.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using ResultBases;

    /// <summary>
    /// The strong typed analysis results.
    /// DO NOT CHANGE THIS CLASS UNLESS YOU ARE TOLD TO.
    /// </summary>
    public class AnalysisResult2 : IComparable<AnalysisResult2>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisResult2"/> class.
        /// This is the standard result class for <c>IAnalyser2</c> results.
        /// </summary>
        /// <param name="settingsUsed">
        ///     Represents the settings used for the analysis.
        /// </param>
        /// <param name="segmentSettings">The settings for the segment that was analyzed.</param>
        /// <param name="durationAnalyzed">
        ///     Records the actual duration analyzed by the analysis.
        /// </param>
        public AnalysisResult2(AnalysisSettings settingsUsed, SegmentSettingsBase segmentSettings, TimeSpan durationAnalyzed)
        {
            this.SegmentSettings = segmentSettings;
            this.SettingsUsed = (AnalysisSettings)settingsUsed.Clone();
            this.OutputFiles = new Dictionary<string, FileInfo>();
            this.SegmentAudioDuration = durationAnalyzed;
            this.MiscellaneousResults = new Dictionary<string, object>();
            this.SummaryIndices = new SummaryIndexBase[0];
            this.SpectralIndices = new SpectralIndexBase[0];
            this.Events = new EventBase[0];
        }

        /// <summary>
        /// Gets or sets Analysis Identifier.
        /// </summary>
        public string AnalysisIdentifier { get; set; }

        /// <summary>
        /// Gets or sets event results.
        /// Should typically contain many results
        /// </summary>
        public EventBase[] Events { get; set; }

        /// <summary>
        /// Gets or sets summary indices results.
        /// Should typically contain just 1 result.
        /// </summary>
        public SummaryIndexBase[] SummaryIndices { get; set; }

        /// <summary>
        /// Gets or sets spectral indices results.
        /// Should typically contain just 1 result.
        /// </summary>
        public SpectralIndexBase[] SpectralIndices { get; set; }

        /// <summary>
        /// Gets a loosely typed dictionary that can store arbitrary result data.
        /// Added as a cheap form of extensibility.
        /// </summary>
        public Dictionary<string, object> MiscellaneousResults { get; }

        /// <summary>
        /// Gets a the settings used to run the analysis.
        /// </summary>
        public AnalysisSettings SettingsUsed { get; }

        /// <summary>
        /// Gets the segment settings used.
        /// </summary>
        public SegmentSettingsBase SegmentSettings { get; }

        /// <summary>
        /// Gets or sets the location of the events file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo EventsFile { get; set; }

        /// <summary>
        /// Gets or sets the location of the indices file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo SummaryIndicesFile { get; set; }

        /// <summary>
        /// Gets or sets the location of the indices file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public IEnumerable<FileInfo> SpectraIndicesFiles { get; set; }

        /// <summary>
        /// Gets or sets the debug image file for this analysis.
        /// Should be null if not written or used.
        /// </summary>
        public FileInfo ImageFile { get; set; }

        /// <summary>
        /// Gets a list of other files that were written (optional).
        /// </summary>
        public Dictionary<string, FileInfo> OutputFiles { get; }

        /// <summary>
        /// Gets the duration of the analyzed segment.
        /// </summary>
        public TimeSpan SegmentAudioDuration { get; }

        /// <summary>
        /// Gets the offset of the segment from the original entire audio file.
        /// </summary>
        public TimeSpan SegmentStartOffset => this.SegmentSettings.SegmentStartOffset;

        /// <summary>
        /// Defines an innate order of Analysis results based on the <c>SegmentStartOffset</c>.
        /// </summary>
        /// <param name="other">The other AnalysisResult to compare to.</param>
        /// <returns>A integer representing the relative order between the two instances.</returns>
        public int CompareTo(AnalysisResult2 other)
        {
            return this.SegmentStartOffset.CompareTo(other.SegmentStartOffset);
        }
    }
}