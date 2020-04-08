// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexGenerationData.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using log4net;
    using Newtonsoft.Json;

    public class IndexGenerationData
    {
        public const string FileNameFragment = "IndexGenerationData";

        private static readonly ILog Log = LogManager.GetLogger(typeof(IndexGenerationData));

        /// <summary>
        /// Gets or sets the configuration options used to draw long duration spectrograms.
        /// </summary>
        public LdSpectrogramConfig LongDurationSpectrogramConfig { get; set; }

        /// <summary>
        /// Gets or sets the extension of the original audio file.
        /// </summary>
        public string RecordingExtension { get; set; }

        /// <summary>
        /// Gets or sets backgroundFilterCoeff is used to adjust colour contrast of false-colour images. Default = 0.75.
        /// </summary>
        public double BackgroundFilterCoeff { get; set; }

        /// <summary>
        ///  Gets or sets default value for frame width from which spectrogram was derived.
        /// </summary>
        public int FrameLength { get; set; }

        /// <summary>
        ///  Gets or sets default value for frame step from which spectrogram was derived. There may be overlap.
        /// </summary>
        public int FrameStep { get; set; }

        /// <summary>
        /// Gets or sets the date the audio was recorded. Originally parsed from the file name by <c>FileDateHelpers</c>.
        /// </summary>
        public DateTimeOffset? RecordingStartDate { get; set; }

        /// <summary>
        /// Gets or sets how far into the recording the analysis was started.
        /// </summary>
        public TimeSpan AnalysisStartOffset { get; set; }

        public TimeSpan? MaximumSegmentDuration { get; set; }

        public int SampleRateOriginal { get; set; }

        public int SampleRateResampled { get; set; }

        /// <summary>
        /// Gets or sets the default is one minute spectra i.e. 60 per hour.  However, as of January 2015, this is not fixed.
        /// User must enter the time span over which indices are calculated.
        /// This TimeSpan is used to calculate a tic interval that is appropriate to the time scale of the spectrogram.
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        /// <summary>
        /// Gets or sets the default is the entire segment i.e. typically of one minute duration.  However, as of January 2015, this is not fixed.
        /// User must enter the time span over which indices are calculated.
        /// If IndexCalculationDuration is set to a brief duration such as 0.2 seconds, then
        /// the backgroundnoise will be calculated from N seconds before the current subsegment to N seconds after => N secs + subseg duration + N secs.
        /// </summary>
        public TimeSpan BgNoiseNeighbourhood { get; set; }

        public string RecordingBasename { get; set; }

        public TimeSpan RecordingDuration { get; set; }

        [JsonIgnore]
        public FileInfo Source { get; private set; }

        /// <summary>
        /// Returns the index generation data from file in passed directory.
        /// </summary>
        public static IndexGenerationData GetIndexGenerationData(DirectoryInfo directory)
        {
            return Json.Deserialize<IndexGenerationData>(FindFile(directory));
        }

        public static IndexGenerationData Load(FileInfo info)
        {
            var indexGenerationData = Json.Deserialize<IndexGenerationData>(info);
            indexGenerationData.Source = info;
            return indexGenerationData;
        }

        public static FileInfo FindFile(DirectoryInfo directory)
        {
            const string pattern = "*" + FileNameFragment + "*";
            return directory.EnumerateFiles(pattern).Single();
        }

        public static IEnumerable<FileInfo> FindAll(DirectoryInfo directory)
        {
            const string pattern = "*" + FileNameFragment + "*";

            return directory.EnumerateFiles(pattern, SearchOption.AllDirectories);
        }
    }
}