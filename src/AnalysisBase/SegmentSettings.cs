// <copyright file="SegmentSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase
{
    using System;
    using System.IO;
    using Acoustics.Shared.Contracts;
    using AnalysisBase.Segment;
    using static Acoustics.Shared.FilenameHelpers;

    /// <summary>
    /// Contains settings specific to the current block of audio that will be analyzed.
    /// </summary>
    public class SegmentSettingsBase
    {
        protected readonly AnalysisSettings AnalysisSettings;
        protected readonly FileSegment PreparedFile;

        internal SegmentSettingsBase(AnalysisSettings analysisSettings, FileSegment preparedFile, DirectoryInfo segmentTempDirectory, DirectoryInfo segmentOutputDirectory)
        {
            Contract.Requires<ArgumentNullException>(analysisSettings != null, $"{nameof(analysisSettings)} must not be null");
            Contract.Requires<ArgumentNullException>(segmentOutputDirectory != null, $"{nameof(segmentOutputDirectory)} must not be null");
            Contract.Requires<ArgumentNullException>(segmentTempDirectory != null, $"{nameof(segmentTempDirectory)} must not be null");
            Contract.Requires<ArgumentNullException>(preparedFile != null, $"{nameof(preparedFile)} must not be null");

            this.AnalysisSettings = analysisSettings;
            this.SegmentTempDirectory = segmentTempDirectory;
            this.SegmentOutputDirectory = segmentOutputDirectory;
            this.PreparedFile = preparedFile;

            string basename = Path.GetFileNameWithoutExtension(preparedFile.Source.Name);

            // if user requests, save the intermediate csv files

            // always save csv to output dir
            this.SegmentEventsFile = AnalysisResultPath(segmentOutputDirectory, basename, StandardEventsSuffix, "csv").ToFileInfo();
            this.SegmentSummaryIndicesFile = AnalysisResultPath(segmentOutputDirectory, basename, StandardIndicesSuffix, "csv").ToFileInfo();
            this.SegmentSpectrumIndicesDirectory = this.SegmentOutputDirectory;

            this.SegmentImageFile = AnalysisResultPath(segmentOutputDirectory, basename, "Image", "png").ToFileInfo();
        }

        /// <summary>
        /// Gets a unique identifier for this object. Tradionally used for debugging paralleism issues.
        /// Has no useful semantics other than for debugging.
        /// </summary>
        public int InstanceId => this.AnalysisSettings.InstanceId;

        /// <summary>
        /// Gets the temp directory for a single analysis run.
        /// Anything put here will be deleted when the analysis is complete.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo SegmentTempDirectory { get; }

        /// <summary>
        /// Gets the output directory for a single analysis run.
        /// Analysis implementations must not set this.
        /// </summary>
        public DirectoryInfo SegmentOutputDirectory { get; }

        /// <summary>
        /// Gets the audio file for the analysis.
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo SegmentAudioFile => this.PreparedFile.Source;

        /// <summary>
        /// Gets the events file for the analysis.
        /// </summary>
        public FileInfo SegmentEventsFile { get; }

        /// <summary>
        /// Gets the summary indices file for the analysis.
        /// </summary>
        public FileInfo SegmentSummaryIndicesFile { get; }

        /// <summary>
        /// Gets the spectrum indices directory where spectra should be written for the analysis.
        /// </summary>
        public DirectoryInfo SegmentSpectrumIndicesDirectory { get; }

        /// <summary>
        /// Gets an output image file - most likely a spectrogram
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo SegmentImageFile { get; }

        /// <summary>
        /// Gets or sets the ideal duration of audio for the segment being analyzed.
        /// This number represents what was requested for cutting whereas the actual
        /// duration of audio provided by <c>SegmentAudioFile</c> may differ slightly due to
        /// inaccuracies in cutting audio.
        /// </summary>
        public TimeSpan AnalysisIdealSegmentDuration { get; protected set; }

        /// <summary>
        /// Gets or sets the start offset of the current analysis segment.
        /// For a large file, analyzed in minute segments, this will store Minute offsets (e.g. min 1, min 2, min 3...).
        /// </summary>
        public TimeSpan SegmentStartOffset { get; protected set; }
    }

    public class SegmentSettings<TSegment> : SegmentSettingsBase
    {
        public SegmentSettings(
            AnalysisSettings analysisSettings,
            ISegment<TSegment> segment,
            (DirectoryInfo Output, DirectoryInfo Temp) dirs,
            FileSegment preparedFile)
            : base(analysisSettings, preparedFile, dirs.Temp, dirs.Output)
        {
            Contract.Requires<ArgumentNullException>(segment != null, $"{nameof(segment)} must not be null");

            this.Segment = segment;
            this.AnalysisIdealSegmentDuration = (this.Segment.EndOffsetSeconds - this.Segment.StartOffsetSeconds).Seconds();
            this.SegmentStartOffset = this.Segment.StartOffsetSeconds.Seconds();
        }

        /// <summary>
        /// Gets the segment from which audio segments are extracted for analysis.
        /// </summary>
        public ISegment<TSegment> Segment { get; }

        public override string ToString()
        {
            return $"{nameof(SegmentSettings<TSegment>)} with instance id {this.InstanceId} and " +
                $"source {this.Segment.Source} ({this.Segment.SourceMetadata?.Identifier}).";
        }
    }
}