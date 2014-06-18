// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisResult2.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
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

    using AnalysisBase.ResultBases;

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
        /// Represents the settings used for the analysis.
        /// </param>
        /// <param name="durationAnalysed">
        /// Records the actual duration analyzed by the analysis.
        /// </param>
        public AnalysisResult2(AnalysisSettings settingsUsed, TimeSpan durationAnalysed)
        {
            this.SettingsUsed = (AnalysisSettings)settingsUsed.Clone();
            this.OutputFiles = new Dictionary<string, FileInfo>();
            this.SegmentAudioDuration = durationAnalysed;
            this.MiscellaneousResults = new Dictionary<string, object>();
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
        public SpectrumBase[] SpectralIndices { get; set; }

        /// <summary>
        /// Gets a loosely typed dictionary that can store arbitrary result data.
        /// Added as a cheap form of extensibility.
        /// </summary>
        public Dictionary<string, object> MiscellaneousResults { get; private set; }

        /// <summary>
        /// Gets a the settings used to run the analysis.
        /// </summary>
        public AnalysisSettings SettingsUsed { get; private set; }

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
        public Dictionary<string, FileInfo> OutputFiles { get; private set; }

        /// <summary>
        /// Gets the duration of the analyzed segment.
        /// </summary>
        public TimeSpan SegmentAudioDuration { get; private set; }

        /// <summary>
        /// Gets the offset of the segment from the original entire audio file.
        /// </summary>
        public TimeSpan SegmentStartOffset
        {
            get { return this.SettingsUsed.SegmentStartOffset ?? TimeSpan.Zero; }
        }

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