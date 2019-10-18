// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using TowseyLibrary;

    public class ContentDescription : AbstractStrongAnalyser
    {
        public const string AnalysisName = "ContentDescription";

        // TASK IDENTIFIERS
        //public const string TaskAnalyse = AnalysisName;
        //public const string TaskLoadCsv = "loadCsv";
        public const string TowseyAcoustic = "Towsey." + AnalysisName;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override string DisplayName => "Content Description";

        public override string Identifier => TowseyAcoustic;

        public override string Description => "[BETA] Generates six spectral indices for Content Description.";

        public override AnalysisSettings DefaultSettings => new AnalysisSettings
        {
            AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
            AnalysisMinSegmentDuration = TimeSpan.FromSeconds(30),
            SegmentMediaType = MediaTypes.MediaTypeWav,
            SegmentOverlapDuration = TimeSpan.Zero,
            AnalysisTargetSampleRate = 22050,
        };

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            //var configuration = (CdConfig)analysisSettings.Configuration;
            //var configuration = (IndexCalculateConfig)analysisSettings.Configuration;

            //configuration.Validate(analysisSettings.AnalysisMaxSegmentDuration.Value);

            //analysisSettings.AnalysisAnalyzerSpecificConfiguration = configuration;
            //analysisSettings.AnalysisAnalyzerSpecificConfiguration = analysisSettings.Configuration;
        }

        //public AnalyzerConfig ParseConfig(FileInfo file)
        //{
        //    return ConfigFile.Deserialize<CdConfig>(file);
        //}

        [Serializable]
        public class CdConfig : IndexCalculateConfig
        {
            private LdSpectrogramConfig ldfcsConfig = new LdSpectrogramConfig();

            /// <summary>
            /// Gets or sets the LDFC spectrogram configuration.
            /// </summary>
            public LdSpectrogramConfig LdSpectrogramConfig { get; protected set; } = new LdSpectrogramConfig();
        }

        /// <summary>
        /// This method calls IndexCalculateSixOnly.Analysis() to do the work!.
        /// </summary>
        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            //var cdConfiguration = (IndexCalculateConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            var cdConfiguration = analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            //var indexCalculationDuration = cdConfiguration.IndexCalculationDuration.Seconds();
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
            var selectors = results.First().GetSelectors();

            selectors.Remove("CVR");
            selectors.Remove("DIF");
            selectors.Remove("RHZ");
            selectors.Remove("RVT");
            selectors.Remove("RPS");
            selectors.Remove("RNG");
            selectors.Remove("R3D");
            selectors.Remove("SPT");
            selectors.Remove("SUM");

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

        public override void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment,
            EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
            //var cdConfig = (CdConfig)settings.AnalysisAnalyzerSpecificConfiguration;

            var sourceAudio = inputFileSegment.Source;
            var resultsDirectory = AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, this);
            //var frameWidth = cdConfig.FrameLength;
            var frameWidth = 512;
            int sampleRate = 22050;

            //int sampleRate = AppConfigHelper.DefaultTargetSampleRate;
            //sampleRate = cdConfig.ResampleRate ?? sampleRate;

            // Gather settings for rendering false color spectrograms
            //var ldSpectrogramConfig = cdConfig.LdSpectrogramConfig;
            var ldSpectrogramConfig = new LdSpectrogramConfig();

            string basename = Path.GetFileNameWithoutExtension(sourceAudio.Name);

            var indexCalculationDurationTimeSpan = TimeSpan.FromSeconds(60);
            var bgNoiseBuffer = TimeSpan.FromSeconds(5);
            // output to disk (so other analyzers can use the data,
            // only data - configuration settings that generated these indices
            // this data can then be used by post-process analyses
            /* NOTE: The value for FrameStep is used only when calculating a standard spectrogram
             * FrameStep is NOT used when calculating Spectral indices.
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
                //IndexCalculationDuration = cdConfig.IndexCalculationDurationTimeSpan,
                //BgNoiseNeighbourhood = cdConfig.BgNoiseBuffer,
                IndexCalculationDuration = indexCalculationDurationTimeSpan,
                BgNoiseNeighbourhood = bgNoiseBuffer,
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

            // Draw ldfc spectrograms
            Tuple<Image, string>[] images =
                DrawSpectrogramsFromSpectralIndices(
                inputDirectory: resultsDirectory,
                outputDirectory: resultsDirectory,
                ldSpectrogramConfig: ldSpectrogramConfig,
                indexGenerationData: indexConfigData,
                basename: basename,
                analysisType: this.Identifier,
                indexSpectrograms: dictionaryOfSpectra,
                imageChrome: true.ToImageChrome());
        }

        /// <summary>
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS
        /// IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE.
        /// WARNING: Make sure the parameters in the CONFIG file are consistent with the CSV files.
        /// </summary>
        /// <param name="inputDirectory">inputDirectory</param>
        /// <param name="outputDirectory">outputDirectory</param>
        /// <param name="ldSpectrogramConfig">config for drawing FCSs</param>
        /// <param name="indexGenerationData">indexGenerationData</param>
        /// <param name="basename">stem name of the original recording</param>
        /// <param name="analysisType">will usually be "Towsey.Acoustic"</param>
        /// <param name="indexSpectrograms">Optional spectra to pass in. If specified the spectra will not be loaded from disk! </param>
        /// <param name="imageChrome">If true, this method generates and returns separate chromeless images used for tiling website images.</param>
        public static Tuple<Image, string>[] DrawSpectrogramsFromSpectralIndices(
            DirectoryInfo inputDirectory,
            DirectoryInfo outputDirectory,
            LdSpectrogramConfig ldSpectrogramConfig,
            //FileInfo indexPropertiesConfigPath,
            IndexGenerationData indexGenerationData,
            string basename,
            string analysisType,
            Dictionary<string, double[,]> indexSpectrograms = null,
            ImageChrome imageChrome = ImageChrome.With)
        {
            var config = ldSpectrogramConfig;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_EVN;

            // Set ColourFilter: Must lie between +/-1. A good value is -0.25
            if (config.ColourFilter == null)
            {
                config.ColourFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            }

            var cs1 = new LDSpectrogramRGB(config, indexGenerationData, colorMap1);
            string fileStem = basename;
            cs1.FileName = fileStem;

            // calculate start time by combining DatetimeOffset with minute offset.
            cs1.StartOffset = indexGenerationData.AnalysisStartOffset;
            if (indexGenerationData.RecordingStartDate.HasValue)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                cs1.RecordingStartDate = dto;
                if (dto != null)
                {
                    cs1.StartOffset = dto.TimeOfDay + cs1.StartOffset;
                }
            }

            // following line is debug purposes only
            //cs.StartOffset = cs.StartOffset + TimeSpan.FromMinutes(15);

            var indexProperties = IndexCalculateSixOnly.GetIndexProperties();
            cs1.SetSpectralIndexProperties(indexProperties);

            // Load the Index Spectrograms into a Dictionary
            cs1.LoadSpectrogramDictionary(indexSpectrograms);

            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Log.Error("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                throw new InvalidOperationException("Cannot find spectrogram matrix files");
            }

            // draw all available gray scale index spectrograms.
            var keys = indexProperties.Keys.ToArray();
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);

            // create two false-colour spectrogram images
            var image1NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap1);
            var image2NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap2);
            var spacer = new Bitmap(image1NoChrome.Width, 10);
            var imageList = new[] { image1NoChrome, spacer, image2NoChrome };
            Image image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);

            // only return images if chromeless
            return imageChrome == ImageChrome.Without
                       ? new[] { Tuple.Create(image1NoChrome, colorMap1), Tuple.Create(image2NoChrome, colorMap2) }
                       : null;
        }
    }
}
