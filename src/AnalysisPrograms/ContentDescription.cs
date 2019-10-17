// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using Acoustics.Shared;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;

    public class ContentDescription : AbstractStrongAnalyser
    {
        public const string AnalysisName = "ContentDescription";

        // TASK IDENTIFIERS
        //public const string TaskAnalyse = AnalysisName;
        //public const string TaskLoadCsv = "loadCsv";
        public const string TowseyAcoustic = "Towsey." + AnalysisName;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const int ResampleRate = 17640;

        public override string DisplayName => "Content Description";

        public override string Identifier => TowseyAcoustic;

        public override string Description => "[BETA] Generates six spectral indices for Content Description.";

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            AnalysisTargetSampleRate = ResampleRate,
        };

        /// <summary>
        /// This method calls others to do the work!.
        /// TODO: SIMPLIFY THIS METHOD.
        /// </summary>
        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            //var acousticIndicesConfiguration = (AcousticIndices.AcousticIndicesConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            //var indexCalculationDuration = acousticIndicesConfiguration.IndexCalculationDuration.Seconds();
            var indexProperties = IndexCalculateSixOnly.GetIndexProperties();

            var config = new IndexCalculateConfig();
            var indexCalculationDuration = TimeSpan.FromSeconds(60);

            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            var segmentResults = IndexCalculateSixOnly.Analysis(
                recording,
                segmentSettings.SegmentStartOffset,
                indexProperties,
                segmentSettings.Segment.SourceMetadata.SampleRate,
                indexCalculationDuration,
                config);

            segmentResults.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration)
            {
                AnalysisIdentifier = this.Identifier,
                SpectralIndices = new SpectralIndexBase[1],
            };
            analysisResults.SpectralIndices[0] = segmentResults.SpectralIndexValues;

            return analysisResults;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var list = new List<FileInfo>();
            return list;
        }

        public override void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment,
            EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
            var acousticIndicesConfig = (AcousticIndices.AcousticIndicesConfig)settings.AnalysisAnalyzerSpecificConfiguration;

            var sourceAudio = inputFileSegment.Source;
            var resultsDirectory = AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, this);
            bool tileOutput = acousticIndicesConfig.TileOutput;

            var frameWidth = acousticIndicesConfig.FrameLength;
            int sampleRate = AppConfigHelper.DefaultTargetSampleRate;
            sampleRate = acousticIndicesConfig.ResampleRate ?? sampleRate;

            // Gather settings for rendering false color spectrograms
            var ldSpectrogramConfig = acousticIndicesConfig.LdSpectrogramConfig;

            string basename = Path.GetFileNameWithoutExtension(sourceAudio.Name);

            // output to disk (so other analyzers can use the data,
            // only data - configuration settings that generated these indices
            // this data can then be used by post-process analyses
            /* NOTE: The value for FrameStep is used only when calculating a standard spectrogram
             * FrameStep is NOT used when calculating Summary and Spectral indices.
             */
            var indexConfigData = new IndexGenerationData()
            {
                RecordingExtension = inputFileSegment.Source.Extension,
                RecordingBasename = basename,
                RecordingStartDate = inputFileSegment.TargetFileStartDate,
                RecordingDuration = inputFileSegment.TargetFileDuration.Value,
                SampleRateOriginal = inputFileSegment.TargetFileSampleRate.Value,
                SampleRateResampled = sampleRate,
                FrameLength = frameWidth,
                FrameStep = settings.Configuration.GetIntOrNull(AnalysisKeys.FrameStep) ?? frameWidth,
                IndexCalculationDuration = acousticIndicesConfig.IndexCalculationDurationTimeSpan,
                BgNoiseNeighbourhood = acousticIndicesConfig.BgNoiseBuffer,
                AnalysisStartOffset = inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero,
                MaximumSegmentDuration = settings.AnalysisMaxSegmentDuration,
                BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF,
                LongDurationSpectrogramConfig = ldSpectrogramConfig,
            };
            var icdPath = FilenameHelpers.AnalysisResultPath(
                resultsDirectory,
                basename,
                IndexGenerationData.FileNameFragment,
                "json");
            Json.Serialise(icdPath.ToFileInfo(), indexConfigData);

            // gather spectra to form spectrograms.  Assume same spectra in all analyzer results
            // this is the most efficient way to do this
            // gather up numbers and strings store in memory, write to disk one time
            // this method also AUTOMATICALLY SORTS because it uses array indexing
            var dictionaryOfSpectra = spectralIndices.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.Rotate90ClockWise);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionaryOfSpectra, resultsDirectory, basename);

            // HACK: do not render false color spectrograms unless IndexCalculationDuration = 60.0 (the normal resolution)
            if (acousticIndicesConfig.IndexCalculationDurationTimeSpan != 60.0.Seconds())
            {
                Log.Warn("False color spectrograms were not rendered");
            }
            else
            {
                FileInfo indicesPropertiesConfig = acousticIndicesConfig.IndexPropertiesConfig.ToFileInfo();

                // Actually draw false color / long duration spectrograms
                Tuple<Image, string>[] images =
                    LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                        inputDirectory: resultsDirectory,
                        outputDirectory: resultsDirectory,
                        ldSpectrogramConfig: ldSpectrogramConfig,
                        indexPropertiesConfigPath: indicesPropertiesConfig,
                        indexGenerationData: indexConfigData,
                        basename: basename,
                        analysisType: this.Identifier,
                        indexSpectrograms: dictionaryOfSpectra,
                        indexStatistics: indexDistributions,
                        imageChrome: (!tileOutput).ToImageChrome());
            }
        }
    }
}
