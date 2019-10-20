// <copyright file="ContentDescription.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.ContentDescriptionTools;
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
            //private LdSpectrogramConfig ldfcsConfig = new LdSpectrogramConfig();

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
            //var cdConfiguration = analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            //var indexCalculationDuration = cdConfiguration.IndexCalculationDuration.Seconds();
            //var outputDirectory = segmentSettings.SegmentOutputDirectory;
            var indexProperties = IndexCalculateSixOnly.GetIndexProperties();
            var config = new IndexCalculateConfig();
            var indexCalculationDuration = TimeSpan.FromSeconds(60); //default value for content description
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);

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
            string basename = Path.GetFileNameWithoutExtension(sourceAudio.Name);
            var resultsDirectory = AnalysisCoordinator.GetNamedDirectory(settings.AnalysisOutputDirectory, this);
            var frameWidth = 512; //default value which should not be changed
            frameWidth = settings.Configuration.GetIntOrNull(AnalysisKeys.FrameLength) ?? frameWidth;
            var frameStep = frameWidth;
            int sampleRate = 22050; //default value which should not be changed
            sampleRate = settings.Configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? sampleRate;

            // output to disk so other analyzers can use the data,
            // only data - configuration settings that generated these indices
            // this data can then be used by post-process analyses
            var indexConfigData = new IndexGenerationData()
            {
                RecordingExtension = inputFileSegment.Source.Extension,
                RecordingBasename = basename,
                RecordingStartDate = inputFileSegment.TargetFileStartDate,
                RecordingDuration = inputFileSegment.TargetFileDuration.Value,
                SampleRateOriginal = inputFileSegment.TargetFileSampleRate.Value,
                SampleRateResampled = sampleRate,
                FrameLength = frameWidth,
                FrameStep = frameStep,
                IndexCalculationDuration = TimeSpan.FromSeconds(60), // default value for content description
                BgNoiseNeighbourhood = TimeSpan.FromSeconds(5), // default value for content description
                AnalysisStartOffset = inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero,
                MaximumSegmentDuration = settings.AnalysisMaxSegmentDuration,
                BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF,
                LongDurationSpectrogramConfig = new LdSpectrogramConfig(),
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

            // Draw ldfc spectrograms and return path to 2maps image.
            string ldfcSpectrogramPath =
                DrawSpectrogramsFromSpectralIndices(
                outputDirectory: resultsDirectory,
                indexGenerationData: indexConfigData,
                basename: basename,
                indexSpectrograms: dictionaryOfSpectra);

            // now get the content description for each minute.
            //FileInfo templatesFile = settings.ConfigFile; //TODO TODO TODO TODO
            FileInfo templatesFile = new FileInfo(@"C:\Ecoacoustics\ContentDescription\TemplateDefinitions.json");
            var contentPlots = GetContentDescription(spectralIndices, templatesFile);
            //TODO TODO TODO TODO
            string directoryPath = @"C:\Ecoacoustics\ContentDescription\TestOfSixIndices\Towsey.ContentDescription";
            var images = GraphsAndCharts.DrawPlotDistributions(contentPlots);
            var plotsImage = ImageTools.CombineImagesVertically(images);
            var path1 = Path.Combine(directoryPath, "ScoreDistributions.png");
            plotsImage.Save(path1);

            //TODO TODO TODO TODO
            // Attach content description plots to LDFC spectrogram and write to file
            var ldfcSpectrogram = Image.FromFile(ldfcSpectrogramPath);
            var image = ContentVisualization.DrawLdfcSpectrogramWithContentScoreTracks(ldfcSpectrogram, contentPlots);
            var path2 = Path.Combine(directoryPath, "Testing_2Maps.CONTENTnew08.png");
            image.Save(path2);
        }

        /// <summary>
        /// This is cut down version of the method of same name in LDSpectrogramRGB.cs.
        /// </summary>
        /// <param name="outputDirectory">outputDirectory.</param>
        /// <param name="indexGenerationData">indexGenerationData.</param>
        /// <param name="basename">stem name of the original recording.</param>
        /// <param name="indexSpectrograms">Optional spectra to pass in. If specified the spectra will not be loaded from disk!.</param>
        private static string DrawSpectrogramsFromSpectralIndices(
            DirectoryInfo outputDirectory,
            IndexGenerationData indexGenerationData,
            string basename,
            Dictionary<string, double[,]> indexSpectrograms = null)
        {
            string colorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_OSC;

            // Set Color Filter: Must lie between +/-1. A good value is -0.25
            var config = new LdSpectrogramConfig();
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

            // create two false-color spectrogram images
            var image1NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap1);
            var image2NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap2);
            var spacer = new Bitmap(image1NoChrome.Width, 10);
            var imageList = new[] { image1NoChrome, spacer, image2NoChrome, spacer};
            Image image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);
            return outputPath;
        }

        /// <summary>
        /// Calculate the content description for each minute.
        /// </summary>
        /// <param name="spectralIndices">set of spectral indices for each minute.</param>
        /// <param name="templatesFile">json file containing description of templates.</param>
        public static List<Plot> GetContentDescription(SpectralIndexBase[] spectralIndices, FileInfo templatesFile)
        {
            // Read in the content description templates
            var templates = Json.Deserialize<TemplateManifest[]>(templatesFile);
            var templatesAsDictionary = DataProcessing.ExtractDictionaryOfTemplateDictionaries(templates);
            //TODO need to get start time of recording as Minute of the Day
            var elapsedTimeAtStartOfRecording = 0; //TODO TODO TODO

            // create dictionary of index vectors
            var results = new List<DescriptionResult>();

            int length = spectralIndices.Length;

            //loop over all minutes in the recording
            for (int i = 0; i < length; i++)
            {
                var oneMinuteOfIndices = spectralIndices[i];

                // Transfer acoustic indices to dictionary
                var indicesDictionary = IndexCalculateSixOnly.ConvertIndicesToDictionary(oneMinuteOfIndices);

                //TODO TODO Remove next line and change the template manifest to indicate what indices are to be used.
                indicesDictionary.Remove("OSC");

                // scan templates over one minute of indices
                var resultsForOneMinute = AudioAnalysisTools.ContentDescriptionTools.ContentDescription.AnalyzeOneMinute(
                    templates,
                    templatesAsDictionary,
                    indicesDictionary,
                    elapsedTimeAtStartOfRecording + i);
                results.Add(resultsForOneMinute);
            }

            var plotDict = DataProcessing.ConvertResultsToPlots(results, 1440, 0);
            var contentPlots = DataProcessing.ConvertPlotDictionaryToPlotList(plotDict);

            // convert scores to z-scores
            //contentPlots = DataProcessing.SubtractMeanPlusSd(contentPlots);

            //the following did not work as well.
            //contentPlots = DataProcessing.SubtractModeAndSd(contentPlots);

            // Use percentile thresholding followed by normalize in 0,1.
            contentPlots = DataProcessing.PercentileThresholding(contentPlots, 90);
            return contentPlots;
        }
    }
}
