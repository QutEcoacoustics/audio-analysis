// <copyright file="StandardizedFeatureExtraction.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.StandardizedFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Security.Policy;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using log4net.Repository.Hierarchy;
    using PowerArgs;
    using TowseyLibrary;

    public class StandardizedFeatureExtraction : AbstractStrongAnalyser
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(StandardizedFeatureExtraction));

        private const string Sentence = "Hello World";

        // Input is a class object of Arguments, class is made below
        public static void Execute(Arguments arguments)
        {
            LoggedConsole.WriteLine("The sentence was printed {0} times", arguments.Multiplication);
            for (int i = 0; i < arguments.Multiplication; i++)
            {
                LoggedConsole.WriteLine(Sentence);
            }
        }

        // Creates a class that constructs arguments, the description is shown in help StandardizedFeatureExtraction
        // The variable Multiplication can later be used
        public class Arguments
        {
            [ArgDescription("How many times hello world")]
            public int Multiplication { get; set; }
        }

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // Construct variable 'configuration' that stores the properties of config file in non-dynamic way
            base.BeforeAnalyze(analysisSettings);
            StandardizedFeatureExtractionConfig configuration = Yaml.Deserialise<StandardizedFeatureExtractionConfig>(analysisSettings.ConfigFile);
            analysisSettings.AnalysisAnalyzerSpecificConfiguration = configuration;
        }

        // Implemented from AbstractStrongAnalyser
        public override string DisplayName
        {
            get { return "Standardized Feature Extraction"; }
        }

        public override string Identifier
        {
            get { return "Ecosounds.StandardizedFeatures"; }
        }

        public virtual string Description
        {
            get { return "Performs a standardized feature extraction."; }
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            StandardizedFeatureExtractionConfig configuration = (StandardizedFeatureExtractionConfig)analysisSettings.AnalysisAnalyzerSpecificConfiguration;
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            //// Fileinfo class for the index properties configuration file
            FileInfo indexPropertiesConfig = new FileInfo(configuration.IndexPropertiesConfig);
            var indexProperties = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            TimeSpan indexCalculationDuration = new TimeSpan(0, 0, (int)configuration.IndexCalculationDuration);

            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;

            // Create new list to store the summary and spectral index values, because length is unknown. Transform after calculations.
            //var analysisResultsSummaryIndices = new List<SummaryIndexValues>();
            //var analysisResultsSpectralIndices = new List<SpectralIndexValues>();

            double segmentDurationSeconds = segmentSettings.AnalysisIdealSegmentDuration.TotalSeconds;
            double subsegmentDuration = indexCalculationDuration.TotalSeconds;
            int subsegmentCount = (int)Math.Round(segmentDurationSeconds / subsegmentDuration);
            subsegmentCount *= configuration.Bands.Count;

            var trackScores = new List<Plot>(subsegmentCount);
            var tracks = new List<SpectralTrack>(subsegmentCount);

            analysisResults.SummaryIndices = new SummaryIndexBase[subsegmentCount];
            analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentCount];

            // Default behaviour: set SUBSEGMENT = total recording
            AudioRecording subsegmentRecording = recording;

            // Only for debug create image
            // Create list to store images so they can be combined later
            var list = new List<Image>();
            string imagePath = segmentSettings.SegmentImageFile.FullName;
            int maxImageWidth = 0;

            int bandCount = 0;
            foreach (var band in configuration.Bands)
            {
                Log.DebugFormat("Starting band {0}/{1}", bandCount+1, configuration.Bands.Count);

                int frameSize = band.FftWindow;
                int frameStep = frameSize;

                // EXTRACT ENVELOPE and SPECTROGRAM FROM SUBSEGMENT
                var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(subsegmentRecording, frameSize, frameStep);

                // Prepare amplitude spectrogram
                double[,] amplitudeSpectrogramData = dspOutput1.AmplitudeSpectrogram; // get amplitude spectrogram.
                double[,] amplitudeSpectrogramData2;

                // Transform from Frequency to Mel Scale
                // band.Melscale defines the number of bins that will be reduced to
                if (band.MelScale != 0)
                {
                    amplitudeSpectrogramData2 = MFCCStuff.MelFilterBank(amplitudeSpectrogramData, band.MelScale, recording.Nyquist, 0, recording.Nyquist);
                }
                else
                {
                    amplitudeSpectrogramData2 = amplitudeSpectrogramData;
                }

                // Select band determined by min and max bandwidth
                int minBand = (int)(amplitudeSpectrogramData2.GetLength(1) * band.Bandwidth.Min);
                int maxBand = (int)(amplitudeSpectrogramData2.GetLength(1) * band.Bandwidth.Max) - 1;

                double[,] amplitudeSpectrogramDataBand = MatrixTools.Submatrix(amplitudeSpectrogramData2, 0, minBand, amplitudeSpectrogramData2.GetLength(0) - 1, maxBand);

                // Create image of amplitude spectrogram
                var image = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogramDataBand));

                // Add image to list
                list.Add(image);

                // Update maximal width of image
                if (image.Width > maxImageWidth)
                {
                    maxImageWidth = image.Width;
                }

                // Calculate spectral indices

                // Convert the dynamic config to IndexCalculateConfig class and merge in the unnecesary parameters.
                IndexCalculateConfig config = IndexCalculateConfig.GetConfig(analysisSettings.Configuration, false);
                config.IndexCalculationDuration = indexCalculationDuration;
                config.BgNoiseBuffer = configuration.BgNoiseNeighbourhood;

                // add values for bands
                config.MinBandWidth = band.Bandwidth.Min;
                config.MaxBandWidth = band.Bandwidth.Max;
                config.FrameLength = band.FftWindow;
                if (band.MelScale != 0)
                {
                    config.frequencyScaleType = FreqScaleType.Mel;
                    config.MelScale = band.MelScale;
                }
                else
                {
                    config.frequencyScaleType = FreqScaleType.Linear;
                }

                // calculate indices for each subsegment for each band
                IndexCalculateResult[] subsegmentResults = Acoustic.CalculateIndicesInSubsegments(
                    recording,
                    segmentSettings.SegmentStartOffset,
                    segmentSettings.AnalysisIdealSegmentDuration,
                    indexCalculationDuration,
                    configuration.BgNoiseNeighbourhood,
                    indexPropertiesConfig,
                    segmentSettings.Segment.SourceMetadata.SampleRate,
                    config);

                //var trackScores = new List<Plot>(subsegmentResults.Length);
                //var tracks = new List<SpectralTrack>(subsegmentResults.Length);

                //analysisResults.SummaryIndices = new SummaryIndexBase[subsegmentResults.Length];
                //analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentResults.Length];

                for (int i = 0; i < subsegmentResults.Length; i++)
                {
                    var indexCalculateResult = subsegmentResults[i];

                    indexCalculateResult.SummaryIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;
                    indexCalculateResult.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

                    //analysisResultsSummaryIndices.Add(indexCalculateResult.SummaryIndexValues);
                    //analysisResultsSpectralIndices.Add(indexCalculateResult.SpectralIndexValues);

                    if (i == 0)
                    {
                        analysisResults.SummaryIndices[i + bandCount] = indexCalculateResult.SummaryIndexValues;
                        analysisResults.SpectralIndices[i + bandCount] = indexCalculateResult.SpectralIndexValues;
                    }
                    else
                    {
                        analysisResults.SummaryIndices[bandCount + configuration.Bands.Count] = indexCalculateResult.SummaryIndexValues;
                        analysisResults.SpectralIndices[bandCount + configuration.Bands.Count] = indexCalculateResult.SpectralIndexValues;
                    }

                    trackScores.AddRange(indexCalculateResult.TrackScores);
                    if (indexCalculateResult.Tracks != null)
                    {
                        tracks.AddRange(indexCalculateResult.Tracks);
                    }
                }

                analysisSettings.AnalysisDataSaveBehavior = false;
                if (analysisSettings.AnalysisDataSaveBehavior)
                {
                    this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
                    analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
                }

                if (analysisSettings.AnalysisDataSaveBehavior)
                {
                    analysisResults.SpectraIndicesFiles =
                        this.WriteSpectrumIndicesFiles(
                            segmentSettings.SegmentSpectrumIndicesDirectory,
                            Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name),
                            analysisResults.SpectralIndices);
                }

                bandCount += 1;

                Log.InfoFormat("Completed band {0}/{1}", bandCount, configuration.Bands.Count);
            }

            // Set savebehavior to always so it saves image
            analysisSettings.AnalysisImageSaveBehavior = SaveBehavior.Always;

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
            {
                Image finalImage = ImageTools.CombineImagesVertically(list, maxImageWidth);
                finalImage.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
                LoggedConsole.WriteLine("See {0} for spectrogram pictures", imagePath);
            }

            // Transform lists that store summary and spectral indices for BOTH bands to array
            //analysisResults.SummaryIndices = analysisResultsSummaryIndices.ToArray();
            //analysisResults.SpectralIndices = analysisResultsSpectralIndices.ToArray();

            return analysisResults;
        }

        public override void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public override void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            Csv.WriteToCsv(destination, results.Cast<SummaryIndexValues>());
        }

        public List<FileInfo> WriteSpectrumIndicesFilesCustom(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            // Group results based on configuration
            Dictionary<string, List<SpectralIndexBase>> dict = GroupResultsOnConfiguration(results);

            var spectralIndexFiles = new List<FileInfo>();

            // For each group and for each selector?? create a csv file
            foreach (var configGroup in dict)
            {
                var groupResults = configGroup.Value;

                var selectors = groupResults.First().GetSelectors();

                // Get the values of the band configuration, since they all have same configuration, just get the first item
                var config = (SpectralIndexValues)groupResults.First();
                string minBandWidth = config.Configuration.MinBandWidth.ToString();
                string maxBandWidth = config.Configuration.MaxBandWidth.ToString();
                string mel;
                string melScale;
                if (config.Configuration.MelScale!= 0)
                {
                    mel = "Mel";
                    melScale = config.Configuration.MelScale.ToString();
                }
                else
                {
                    mel = "Standard";
                    melScale = 0.ToString();
                }

                string fftWindow = config.Configuration.FrameLength.ToString();

                foreach (var kvp in selectors)
                {
                    // write spectrogram to disk as CSV file
                    var filename = FilenameHelpers.AnalysisResultPath(destination, fileNameBase, this.Identifier + "." + kvp.Key, "csv", minBandWidth, maxBandWidth, mel, melScale, "FftWindow", fftWindow).ToFileInfo();
                    spectralIndexFiles.Add(filename);
                    Csv.WriteMatrixToCsv(filename, groupResults, kvp.Value);
                }
            }

            return spectralIndexFiles;
        }

        public static Dictionary<string, List<SpectralIndexBase>> GroupResultsOnConfiguration(IEnumerable<SpectralIndexBase> results)
        {
            Dictionary<string, List<SpectralIndexBase>> dict = new Dictionary<string, List<SpectralIndexBase>>();

            // Group the results based on the band configuration they have
            foreach (var spectralIndexBase in results)
            {
                var spectralIndexValues = (SpectralIndexValues)spectralIndexBase;

                // HACK: This is a really cheap and dodgy way to do structural equality, but Anthony told me to do it
                var spectralIndexValuesConfiguration = Json.SerialiseToString(spectralIndexValues.Configuration, false);

                if (dict.ContainsKey(spectralIndexValuesConfiguration))
                {
                    dict[spectralIndexValuesConfiguration].Add(spectralIndexBase);
                }
                else
                {
                    dict.Add(spectralIndexValuesConfiguration, new List<SpectralIndexBase>());
                    dict[spectralIndexValuesConfiguration].Add(spectralIndexBase);
                }
            }

            return dict;
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            return this.WriteSpectrumIndicesFilesCustom(destination, fileNameBase, results);
        }

        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no op
        }
    }
}