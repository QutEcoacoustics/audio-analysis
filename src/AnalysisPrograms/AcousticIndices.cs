// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticIndices.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Acoustic type.
// This class is derived from IAnalyser2 and is typically called from AnalyseLongRecording.Dev with "audio2csv" as first argument on the command line.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.LongDurationSpectrograms.Zooming;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.TileImage;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;
    using Path = System.IO.Path;
    using SpectrogramType = AudioAnalysisTools.LongDurationSpectrograms.SpectrogramType;

    public class AcousticIndices : IAnalyser2
    {
        // OTHER CONSTANTS
        public const string AnalysisName = "Acoustic";

        // TASK IDENTIFIERS
        public const string TaskAnalyse = AnalysisName;
        public const string TaskLoadCsv = "loadCsv";
        public const string TowseyAcoustic = "Towsey." + AnalysisName;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string DisplayName => "Acoustic Indices";

        public string Identifier => TowseyAcoustic;

        public string Description
            => "Generates all our default summary & spectral acoustic indices. Also generates false color spectrograms IFF IndexCalculationDuration==60.0";

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            var configuration = (AcousticIndicesConfig)analysisSettings.Configuration;

            configuration.Validate(analysisSettings.AnalysisMaxSegmentDuration.Value);

            analysisSettings.AnalysisAnalyzerSpecificConfiguration = configuration;
        }

        public AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<AcousticIndicesConfig>(file);
        }

        [Serializable]
        public class AcousticIndicesConfig : IndexCalculateConfig
        {
            /// <summary>
            /// Gets or sets the LDFC spectrogram configuration.
            /// </summary>
            public LdSpectrogramConfig LdSpectrogramConfig { get; protected set; } = new LdSpectrogramConfig();

            public bool TileOutput { get; private set; } = false;

            public void Validate(TimeSpan defaultIndexCalculationDuration)
            {
                SpectralIndexValues.CheckExistenceOfSpectralIndexValues(this.IndexProperties);

                // if tiling output we need to be able to parse the date from the file name
                Log.Info("Image tiling is " + (this.TileOutput ? string.Empty : "NOT ") + "enabled");
                if (this.TileOutput)
                {
                    if (!this.RequireDateInFilename)
                    {
                        throw new ConfigFileException(
                            "If TileImageOutput is set then RequireDateInFilename must be set as well");
                    }
                }

                // set IndexCalculationDuration i.e. duration of a subsegment
                if (this.IndexCalculationDuration <= 0)
                {
                    this.IndexCalculationDuration = defaultIndexCalculationDuration.TotalSeconds;
                    Log.Warn(
                        "IndexCalculationDuration from config file is invalid"
                        + $" (Used default value = {this.IndexCalculationDuration})");
                }

                if (this.TileOutput && this.IndexCalculationDuration != 60.0)
                {
                    throw new ConfigFileException(
                        "Invalid configuration detected: tile image output is enabled but "
                        + "ICD != 60.0 so the images won'tbe created");
                }
            }
        }

        public AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var acousticIndicesConfiguration = (AcousticIndicesConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            var indexCalculationDuration = acousticIndicesConfiguration.IndexCalculationDuration.Seconds();

            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;

            // calculate indices for each subsegment
            IndexCalculateResult[] subsegmentResults = CalculateIndicesInSubsegments(
                recording,
                segmentSettings.SegmentStartOffset,
                segmentSettings.AnalysisIdealSegmentDuration,
                indexCalculationDuration,
                acousticIndicesConfiguration.IndexProperties,
                segmentSettings.Segment.SourceMetadata.SampleRate,
                acousticIndicesConfiguration);

            var trackScores = new List<Plot>(subsegmentResults.Length);
            var tracks = new List<Track>(subsegmentResults.Length);

            analysisResults.SummaryIndices = new SummaryIndexBase[subsegmentResults.Length];
            analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentResults.Length];
            for (int i = 0; i < subsegmentResults.Length; i++)
            {
                var indexCalculateResult = subsegmentResults[i];
                indexCalculateResult.SummaryIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;
                indexCalculateResult.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

                analysisResults.SummaryIndices[i] = indexCalculateResult.SummaryIndexValues;
                analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;
                trackScores.AddRange(indexCalculateResult.TrackScores);
                if (indexCalculateResult.Tracks != null)
                {
                    tracks.AddRange(indexCalculateResult.Tracks);
                }
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                analysisResults.SpectraIndicesFiles =
                    WriteSpectrumIndicesFilesCustom(
                        segmentSettings.SegmentSpectrumIndicesDirectory,
                        Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name),
                        analysisResults.SpectralIndices);
            }

            // write the segment spectrogram (typically of one minute duration) to CSV
            // this is required if you want to produced zoomed spectrograms at a resolution greater than 0.2 seconds/pixel
            bool saveSonogramData = analysisSettings.Configuration.GetBoolOrNull(AnalysisKeys.SaveSonogramData) ?? false;
            if (saveSonogramData || analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResults.Events.Length))
            {
                var sonoConfig = new SonogramConfig(); // default values config
                sonoConfig.SourceFName = recording.FilePath;
                sonoConfig.WindowSize = acousticIndicesConfiguration.FrameLength;
                sonoConfig.WindowStep = analysisSettings.Configuration.GetIntOrNull(AnalysisKeys.FrameStep) ?? sonoConfig.WindowSize; // default = no overlap
                sonoConfig.WindowOverlap = (sonoConfig.WindowSize - sonoConfig.WindowStep) / (double)sonoConfig.WindowSize;

                // Linear or Octave frequency scale?
                bool octaveScale = analysisSettings.Configuration.GetBoolOrNull(AnalysisKeys.KeyOctaveFreqScale) ?? false;
                if (octaveScale)
                {
                    sonoConfig.WindowStep = sonoConfig.WindowSize;
                    sonoConfig.WindowOverlap = (sonoConfig.WindowSize - sonoConfig.WindowStep) / (double)sonoConfig.WindowSize;
                }

                ////sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                ////sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);

                if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
                {
                    string imagePath = Path.Combine(outputDirectory.FullName, segmentSettings.SegmentImageFile.Name);

                    // NOTE: hits (SPT in this case) is intentionally not supported
                    var image = DrawSonogram(sonogram, null, trackScores, tracks);
                    image.Save(imagePath);
                    analysisResults.ImageFile = new FileInfo(imagePath);
                }

                if (saveSonogramData)
                {
                    string csvPath = Path.Combine(outputDirectory.FullName, recording.BaseName + ".csv");
                    Csv.WriteMatrixToCsv(csvPath.ToFileInfo(), sonogram.Data);
                }
            }

            return analysisResults;
        }

        public void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<SummaryIndexValues>());
        }

        public static List<FileInfo> WriteSpectrumIndicesFilesCustom(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var selectors = results.First().GetSelectors();

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, TowseyAcoustic + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, results, kvp.Value);
            }

            return spectralIndexFiles;
        }

        public List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            return WriteSpectrumIndicesFilesCustom(destination, fileNameBase, results);
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
            var acousticIndicesConfig = (AcousticIndicesConfig)settings.AnalysisAnalyzerSpecificConfiguration;

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
                Tuple<Image<Rgb24>, string>[] images =
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

                if (tileOutput)
                {
                    Debug.Assert(images.Length == 2);

                    Log.Info("Tiling output at scale: " + acousticIndicesConfig.IndexCalculationDuration);

                    foreach (var image in images)
                    {
                        TileOutput(resultsDirectory, Path.GetFileNameWithoutExtension(sourceAudio.Name), image.Item2 + ".Tile", inputFileSegment, image.Item1);
                    }
                }
            }
        }

        private static void TileOutput(DirectoryInfo outputDirectory, string fileStem, string analysisTag, FileSegment fileSegment, Image<Rgb24> image)
        {
            const int TileHeight = 256;
            const int TileWidth = 60;

            // seconds per pixel
            const double Scale = 60.0;
            TimeSpan scale = Scale.Seconds();

            if (image.Height != TileHeight)
            {
                throw new InvalidOperationException("Expecting images exactly the same height as the defined tile height");
            }

            // if recording does not start on an absolutely aligned hour of the day
            // align it, then adjust where the tiling starts from, and calculate the offset for the super tile (the gap)
            var recordingStartDate = fileSegment.TargetFileStartDate.Value;

            // TODO: begin remove duplicate code
            var timeOfDay = recordingStartDate.TimeOfDay;
            var previousAbsoluteHour = TimeSpan.FromSeconds(Math.Floor(timeOfDay.TotalSeconds / (Scale * TileWidth)) * (Scale * TileWidth));
            var gap = timeOfDay - previousAbsoluteHour;
            var tilingStartDate = recordingStartDate - gap;
            var tilingStartDate2 = ZoomTiledSpectrograms.GetPreviousTileBoundary(TileWidth, Scale, recordingStartDate);
            var padding = recordingStartDate - tilingStartDate2;
            Debug.Assert(tilingStartDate == tilingStartDate2, "tilingStartDate != tilingStartDate2: these methods should be equivalent");

            // TODO: end remove duplicate code

            var tilingProfile = new AbsoluteDateTilingProfile(fileStem, analysisTag, tilingStartDate, TileHeight, TileWidth);

            // pad out image so it produces a whole number of tiles
            // this solves the asymmetric right padding of short audio files
            var width = (int)(Math.Ceiling(image.Width / Scale) * Scale);
            var tiler = new Tiler(outputDirectory, tilingProfile, Scale, width, 1.0, image.Height);

            // prepare super tile
            var tile = new TimeOffsetSingleLayerSuperTile(
                padding,
                SpectrogramType.Index,
                scale,
                image.CloneAs<Rgba32>(),
                fileSegment.SegmentStartOffset ?? TimeSpan.Zero);

            tiler.Tile(tile);
        }

        private static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<Track> tracks)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage());
            image.AddTrack(ImageTrack.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(ImageTrack.GetSegmentationTrack(sonogram));

            if (scores != null)
            {
                foreach (Plot plot in scores)
                {
                    // assumes data normalized in 0,1
                    image.AddTrack(ImageTrack.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
                }
            }

            if (tracks != null)
            {
                image.AddTracks(tracks, sonogram.FramesPerSecond, sonogram.FBinWidth);
            }

            if (hits != null)
            {
                image.OverlayRedMatrix(hits, 1.0);
            }

            return image.GetImage();
        }

        public static IndexCalculateResult[] CalculateIndicesInSubsegments(
            AudioRecording recording,
            TimeSpan segmentStartOffset,
            TimeSpan segmentDuration,
            TimeSpan indexCalculationDuration,
            Dictionary<string, IndexProperties> indexProperties,
            int sampleRateOfOriginalAudioFile,
            IndexCalculateConfig config)
        {
            if (recording.WavReader.Channels > 1)
            {
                throw new InvalidOperationException(
                    @"A multi-channel recording MUST be mixed down to MONO before calculating acoustic indices!");
            }

            double recordingDuration = recording.Duration.TotalSeconds;
            double subsegmentDuration = indexCalculationDuration.TotalSeconds;

            // intentional possible null ref, throw if not null
            double segmentDurationSeconds = segmentDuration.TotalSeconds;
            double audioCuttingError = subsegmentDuration - segmentDurationSeconds;

            // using the expected duration, each call to analyze will always produce the same number of results
            // round, we expect perfect numbers, warn if not
            double subsegmentsInSegment = segmentDurationSeconds / subsegmentDuration;
            int subsegmentCount = (int)Math.Round(segmentDurationSeconds / subsegmentDuration);
            const double warningThreshold = 0.01; // 1%
            double fraction = subsegmentsInSegment - subsegmentCount;
            if (Math.Abs(fraction) > warningThreshold)
            {
                Log.Warn(
                    string.Format(
                        "The IndexCalculationDuration ({0}) does not fit well into the provided segment ({1}). This means a partial result has been {3}, {2} results will be calculated",
                        subsegmentDuration,
                        segmentDurationSeconds,
                        subsegmentCount,
                        fraction >= 0.5 ? "added" : "removed"));
            }

            Log.Trace(subsegmentCount + " sub segments will be calculated");

            var indexCalculateResults = new IndexCalculateResult[subsegmentCount];

            // calculate indices for each subsegment
            for (int i = 0; i < subsegmentCount; i++)
            {
                var subsegmentOffset = segmentStartOffset + TimeSpan.FromSeconds(i * subsegmentDuration);
                var indexCalculateResult = IndexCalculate.Analysis(
                    recording,
                    subsegmentOffset,
                    indexProperties,
                    sampleRateOfOriginalAudioFile,
                    segmentStartOffset,
                    config);

                indexCalculateResults[i] = indexCalculateResult;
            }

            return indexCalculateResults;
        }

        public AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(1),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
        };

        public Status Status => Status.Maintained;
    }
}