namespace AnalysisBase
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using SegmentAnalysis;

    public class SegmentSettings<TSegment>
    {
        private readonly AnalysisSettingsBase analysisSettings;
        private readonly FileSegment preparedFile;

        public int InstanceId => this.analysisSettings.InstanceId;

        public SegmentSettings(AnalysisSettingsBase analysisSettingsBase, ISegment<TSegment> segment, (DirectoryInfo Output, DirectoryInfo Temp) dirs, FileSegment preparedFile)
        {
            this.analysisSettings = analysisSettingsBase;
            this.preparedFile = preparedFile;
            this.Segment = segment;
            this.SegmentTempDirectory = dirs.Temp;
            this.SegmentOutputDirectory = dirs.Output;

            string basename = Path.GetFileNameWithoutExtension(preparedFile.TargetFile.Name);

            // if user requests, save the intermediate csv files
            if (this.saveIntermediateDataFiles)
            {
                // always save csv to output dir
                this.SegmentEventsFile = this.SegmentOutputDirectory.CombineFile(basename + ".Events.csv");

                this.SegmentSummaryIndicesFile = this.SegmentOutputDirectory.CombineFile(basename + ".Indices.csv");

                this.SegmentSpectrumIndicesDirectory = this.SegmentOutputDirectory;
            }

        }

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
        /// Gets the segment from which audio segments are extracted for analysis.
        /// </summary>
        public ISegment<TSegment> Segment { get; }

        /// <summary>
        /// Gets the audio file for the analysis.
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo SegmentAudioFile => this.preparedFile.TargetFile;

        /// <summary>
        /// Gets or sets the events file for the analysis.
        /// </summary>
        public FileInfo SegmentEventsFile { get; }

        /// <summary>
        /// Gets or sets the summary indices file for the analysis.
        /// </summary>
        public FileInfo SegmentSummaryIndicesFile { get; }

        /// <summary>
        /// Gets or sets the spectrum indices directory where spectra should be written for the analysis.
        /// </summary>
        public DirectoryInfo SegmentSpectrumIndicesDirectory { get; }

        /// <summary>
        /// Gets or sets an output image file - most likely a spectrogram
        /// Analysis implementations must not set this.
        /// </summary>
        public FileInfo SegmentImageFile { get; }

        /// <summary>
        /// Gets the ideal duration of audio for the segment being analyzed.
        /// This number represents what was requested for cutting whereas the actual
        /// duration of audio provided by <c>SegmentAudioFile</c> may differ slightly due to
        /// inaccuracies in cutting audio.
        /// </summary>
        public TimeSpan AnalysisIdealSegmentDuration =>
            (this.Segment.EndOffsetSeconds - this.Segment.StartOffsetSeconds).Seconds();

        /// <summary>
        /// Gets the start offset of the current analysis segment.
        /// For a large file, analyzed in minute segments, this will store Minute offsets (e.g. min 1, min 2, min 3...).
        /// </summary>
        public TimeSpan SegmentStartOffset => this.Segment.StartOffsetSeconds.Seconds();


    }
}