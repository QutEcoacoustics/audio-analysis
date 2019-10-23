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
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
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
        //public const string TaskAnalyze = AnalysisName;
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
            AnalysisTargetSampleRate = ContentSignatures.SampleRate,
        };

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
        }

        public override AnalyzerConfig ParseConfig(FileInfo file) => ConfigFile.Deserialize<CdConfig>(file);

        [Serializable]
        public class CdConfig : AnalyzerConfig
        {
            public string TemplatesList { get; protected set; }

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
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);

            // Calculate six spectral indices.
            var segmentResults = IndexCalculateSixOnly.Analysis(
                recording,
                segmentSettings.SegmentStartOffset,
                segmentSettings.Segment.SourceMetadata.SampleRate);

            segmentResults.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration)
            {
                AnalysisIdentifier = this.Identifier,
                SpectralIndices = new SpectralIndexBase[1],
            };

            //Transfer the spectral index results to AnalysisResults
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
            //get selectors and removed unwanted because these indices were never calculated.
            var spectralIndexBases = results.ToList();
            var selectors = spectralIndexBases.First().GetSelectors();
            foreach (var indexName in ContentSignatures.UnusedIndexNames)
            {
                selectors.Remove(indexName);
            }

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);
            foreach (var kvp in selectors)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, TowseyAcoustic + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, spectralIndexBases, kvp.Value);
            }

            return spectralIndexFiles;
        }

        public override void SummariseResults(
            AnalysisSettings analysisSettings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // below is example of how to access values in ContentDescription config file.
            //sampleRate = analysisSettings.Configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? sampleRate;
            var cdConfiguration = (CdConfig)analysisSettings.Configuration;
            var ldSpectrogramConfig = cdConfiguration.LdSpectrogramConfig;
            var cdConfigFile = analysisSettings.ConfigFile;
            var configDirectory = cdConfigFile.DirectoryName ?? throw new ArgumentNullException(nameof(cdConfigFile), "Null value");
            var sourceAudio = inputFileSegment.Source;
            string basename = Path.GetFileNameWithoutExtension(sourceAudio.Name);
            var resultsDirectory = AnalysisCoordinator.GetNamedDirectory(analysisSettings.AnalysisOutputDirectory, this);

            // check for null values - this was recommended by ReSharper!
            if (inputFileSegment.TargetFileDuration == null || inputFileSegment.TargetFileSampleRate == null)
            {
                throw new NullReferenceException();
            }

            // set the start time for the current recording segment. Default is zero.
            var startTimeOffset = TimeSpan.Zero;
            if (inputFileSegment.SegmentStartOffset != null)
            {
                startTimeOffset = (TimeSpan)inputFileSegment.SegmentStartOffset;
            }

            // output config data to disk so other analyzers can use the data,
            // Should contain data only - i.e. the configuration settings that generated these indices
            // this data can then be used by later analysis processes.
            var indexConfigData = new IndexGenerationData()
            {
                RecordingExtension = inputFileSegment.Source.Extension,
                RecordingBasename = basename,
                RecordingStartDate = inputFileSegment.TargetFileStartDate,
                RecordingDuration = inputFileSegment.TargetFileDuration.Value,
                SampleRateOriginal = inputFileSegment.TargetFileSampleRate.Value,
                SampleRateResampled = ContentSignatures.SampleRate,
                FrameLength = ContentSignatures.FrameSize,
                FrameStep = ContentSignatures.FrameSize,
                IndexCalculationDuration = TimeSpan.FromSeconds(ContentSignatures.IndexCalculationDurationInSeconds),
                BgNoiseNeighbourhood = TimeSpan.FromSeconds(5), // default value for content description
                AnalysisStartOffset = inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero,
                MaximumSegmentDuration = analysisSettings.AnalysisMaxSegmentDuration,
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
            // The following method returns var indexDistributions =, but we have no use for them.
            IndexDistributions.WriteSpectralIndexDistributionStatistics(dictionaryOfSpectra, resultsDirectory, basename);

            // Draw ldfc spectrograms and return path to 2maps image.
            string ldfcSpectrogramPath =
                DrawSpectrogramsFromSpectralIndices(
                    ldSpectrogramConfig,
                    outputDirectory: resultsDirectory,
                    indexGenerationData: indexConfigData,
                    basename: basename,
                    indexSpectrograms: dictionaryOfSpectra);

            // now get the content description for each minute.
            var templatesFileName = cdConfiguration.TemplatesList;
            var templatesFile = new FileInfo(Path.Combine(configDirectory, templatesFileName));
            var contentDictionary = GetContentDescription(spectralIndices, templatesFile, startTimeOffset);

            // Write the results to a csv file
            var filePath = Path.Combine(resultsDirectory.FullName, "AcousticSignatures.csv");
            FileTools.WriteDictionaryAsCsvFile(contentDictionary, filePath);

            // prepare graphical plots of the acoustic signatures.
            var contentPlots = GetPlots(contentDictionary);
            var images = GraphsAndCharts.DrawPlotDistributions(contentPlots);
            var plotsImage = ImageTools.CombineImagesVertically(images);
            plotsImage.Save(Path.Combine(resultsDirectory.FullName, "DistributionsOfContentScores.png"));

            // Attach content description plots to LDFC spectrogram and write to file
            var ldfcSpectrogram = Image.FromFile(ldfcSpectrogramPath);
            var image = ContentVisualization.DrawLdfcSpectrogramWithContentScoreTracks(ldfcSpectrogram, contentPlots);
            var path3 = Path.Combine(resultsDirectory.FullName, basename + ".CONTENTnew07.png");
            image.Save(path3);
        }

        /// <summary>
        /// Calculate the content description for each minute.
        /// </summary>
        /// <param name="spectralIndices">set of spectral indices for each minute.</param>
        /// <param name="templatesFile">json file containing description of templates.</param>
        /// <param name="elapsedTimeAtStartOfRecording">minute Id for start of recording.</param>
        private static Dictionary<string, double[]> GetContentDescription(
            SpectralIndexBase[] spectralIndices,
            FileInfo templatesFile,
            TimeSpan elapsedTimeAtStartOfRecording)
        {
            // Read in the content description templates
            var templates = Json.Deserialize<TemplateManifest[]>(templatesFile);
            var templatesAsDictionary = DataProcessing.ExtractDictionaryOfTemplateDictionaries(templates);
            var startMinuteId = (int)Math.Round(elapsedTimeAtStartOfRecording.TotalMinutes);

            // create dictionary of index vectors
            var results = new List<DescriptionResult>();

            int length = spectralIndices.Length;

            //loop over all minutes in the recording
            for (int i = 0; i < length; i++)
            {
                var oneMinuteOfIndices = spectralIndices[i];

                // Transfer acoustic indices to dictionary
                var indicesDictionary = IndexCalculateSixOnly.ConvertIndicesToDictionary(oneMinuteOfIndices);

                // normalize the index values
                foreach (string key in ContentSignatures.IndexNames)
                {
                    var indexBounds = ContentSignatures.IndexValueBounds[key];
                    var indexArray = indicesDictionary[key];
                    var normalisedVector = DataTools.NormaliseInZeroOne(indexArray, indexBounds[0], indexBounds[1]);
                    indicesDictionary[key] = normalisedVector;
                }

                // scan templates over one minute of indices
                var resultsForOneMinute = ContentSignatures.AnalyzeOneMinute(
                    templates,
                    templatesAsDictionary,
                    indicesDictionary,
                    startMinuteId + i);
                results.Add(resultsForOneMinute);
            }

            // convert results to dictionary of score arrays
            var dictionaryOfScores = DataProcessing.ConvertResultsToDictionaryOfArrays(results, length, startMinuteId);
            return dictionaryOfScores;
        }

        /// <summary>
        /// Produce plots for graphical display.
        /// NOTE: The threshold can be changed later.
        /// </summary>
        /// <returns>A list of graphical plots.</returns>
        private static List<Plot> GetPlots(Dictionary<string, double[]> contentDictionary)
        {
            double threshold = 0.25;
            var plotDict = DataProcessing.ConvertArraysToPlots(contentDictionary, threshold);
            var contentPlots = DataProcessing.ConvertPlotDictionaryToPlotList(plotDict);

            // convert scores to z-scores
            //contentPlots = DataProcessing.SubtractMeanPlusSd(contentPlots);

            //the following did not work as well.
            //contentPlots = DataProcessing.SubtractModeAndSd(contentPlots);

            // Use percentile thresholding followed by normalize in 0,1.
            contentPlots = DataProcessing.PercentileThresholding(contentPlots, 90);
            return contentPlots;
        }

        /// <summary>
        /// This is cut down version of the method of same name in LDSpectrogramRGB.cs.
        /// </summary>
        /// <param name="ldSpectrogramConfig">config for ldfc spectrogram.</param>
        /// <param name="outputDirectory">outputDirectory.</param>
        /// <param name="indexGenerationData">indexGenerationData.</param>
        /// <param name="basename">stem name of the original recording.</param>
        /// <param name="indexSpectrograms">Optional spectra to pass in. If specified the spectra will not be loaded from disk!.</param>
        private static string DrawSpectrogramsFromSpectralIndices(
            LdSpectrogramConfig ldSpectrogramConfig,
            DirectoryInfo outputDirectory,
            IndexGenerationData indexGenerationData,
            string basename,
            Dictionary<string, double[,]> indexSpectrograms = null)
        {
            string colorMap1 = ldSpectrogramConfig.ColorMap1; // SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            string colorMap2 = ldSpectrogramConfig.ColorMap2; // SpectrogramConstants.RGBMap_BGN_PMN_OSC;

            var cs1 = new LDSpectrogramRGB(ldSpectrogramConfig, indexGenerationData, colorMap1);
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
            var imageList = new[] { image1NoChrome, spacer, image2NoChrome, spacer };
            Image image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);
            return outputPath;
        }
    }
}
