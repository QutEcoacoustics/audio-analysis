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
    using TowseyLibrary;

    public class StandardizedFeatureExtraction : AbstractStrongAnalyser
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(StandardizedFeatureExtraction));

        public override void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            base.BeforeAnalyze(analysisSettings);
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
            get { return "Performs a standardized feature extraction for ML tasks identifying faunal vocalisations."; }
        }

        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            // Construct variable 'configuration' that stores the properties of config file as strongly typed object
            return ConfigFile.Deserialize<StandardizedFeatureExtractionConfig>(file);
        }

        public override AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var configuration = (StandardizedFeatureExtractionConfig)analysisSettings.Configuration;
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);

            // Configurations non-specific for bands
            TimeSpan indexCalculationDuration = configuration.IndexCalculationDurationTimeSpan;
            TimeSpan bgNoiseNeighbourhood = configuration.BgNoiseBuffer;

            // Bands
            List<StandardizedFeatureExtractionConfig.BandsProperties> bandsList = configuration.Bands;

            // Check if there are identical bands
            CheckForIdenticalBands(bandsList);

            // Estimate total number of subsegments
            double segmentDurationSeconds = segmentSettings.AnalysisIdealSegmentDuration.TotalSeconds;
            double subsegmentDuration = indexCalculationDuration.TotalSeconds;
            int subsegmentCount = (int)Math.Round(segmentDurationSeconds / subsegmentDuration);
            int totalSubsegmentCount = subsegmentCount * bandsList.Count;

            // Store results of all subsegments
            var analysisResults = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            analysisResults.AnalysisIdentifier = this.Identifier;

            var trackScores = new List<Plot>(totalSubsegmentCount);
            var tracks = new List<SpectralTrack>(totalSubsegmentCount);

            analysisResults.SummaryIndices = new SummaryIndexBase[totalSubsegmentCount];
            analysisResults.SpectralIndices = new SpectralIndexBase[totalSubsegmentCount];

            // Create list to store images, one for each band. They are later combined into one image.
            var list = new List<Image>();
            string imagePath = segmentSettings.SegmentImageFile.FullName;
            int maxImageWidth = 0;

            int bandCount = 0;
            foreach (var band in bandsList)
            {
                Log.DebugFormat("Starting band {0}/{1}", bandCount + 1, bandsList.Count);

                // Calculate spectral indices

                // get a fresh copy of the ICC config
                var config = (IndexCalculateConfig)((ICloneable)configuration).Clone();

                // Add values specific for band from custom configuration file to config
                config.MinBandWidth = band.Bandwidth.Min;
                config.MaxBandWidth = band.Bandwidth.Max;
                config.FrameLength = band.FftWindow;
                if (band.MelScale != 0)
                {
                    config.FrequencyScale = FreqScaleType.Mel;
                    config.MelScale = band.MelScale;
                }
                else
                {
                    config.FrequencyScale = FreqScaleType.Linear;
                }

                // Calculate indices for each subsegment and for each band
                IndexCalculateResult[] subsegmentResults = AcousticIndices.CalculateIndicesInSubsegments(
                    recording,
                    segmentSettings.SegmentStartOffset,
                    segmentSettings.AnalysisIdealSegmentDuration,
                    indexCalculationDuration,
                    config.IndexProperties,
                    segmentSettings.Segment.SourceMetadata.SampleRate,
                    config);

                int columnsAmplitudeSpectrogram = subsegmentResults[0].AmplitudeSpectrogram.GetLength(1);
                double[,] amplitudeSpectrogramSegment = new double[0, columnsAmplitudeSpectrogram];

                for (int i = 0; i < subsegmentResults.Length; i++)
                {
                    var indexCalculateResult = subsegmentResults[i];

                    indexCalculateResult.SummaryIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;
                    indexCalculateResult.SpectralIndexValues.FileName = segmentSettings.Segment.SourceMetadata.Identifier;

                    analysisResults.SummaryIndices[bandCount + (i * bandsList.Count)] = indexCalculateResult.SummaryIndexValues;
                    analysisResults.SpectralIndices[bandCount + (i * bandsList.Count)] = indexCalculateResult.SpectralIndexValues;

                    trackScores.AddRange(indexCalculateResult.TrackScores);
                    if (indexCalculateResult.Tracks != null)
                    {
                        tracks.AddRange(indexCalculateResult.Tracks);
                    }

                    if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
                    {
                        // Add amplitude spectrograms of each subsegment together to get amplitude spectrogram of one segment
                        double[,] amplitudeSpectrogramSubsegment = indexCalculateResult.AmplitudeSpectrogram;
                        amplitudeSpectrogramSegment = MatrixTools.ConcatenateMatrixRows(
                            amplitudeSpectrogramSegment,
                            amplitudeSpectrogramSubsegment);
                    }
                }

                if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
                {
                    // Create image of amplitude spectrogram
                    var image = ImageTools.DrawReversedMatrix(MatrixTools.MatrixRotate90Anticlockwise(amplitudeSpectrogramSegment));

                    // Label information
                    string minBandWidth = band.Bandwidth.Min.ToString();
                    string maxBandWidth = band.Bandwidth.Max.ToString();
                    string fftWindow = band.FftWindow.ToString();
                    string mel;
                    string melScale;
                    if (band.MelScale != 0)
                    {
                        mel = "Mel";
                        melScale = band.MelScale.ToString();
                    }
                    else
                    {
                        mel = "Standard";
                        melScale = 0.ToString();
                    }

                    // Create label
                    string segmentSeparator = "_";
                    string[] segments = { minBandWidth, maxBandWidth, fftWindow, mel, melScale };
                    string labelText = segments.Aggregate(string.Empty, (aggregate, item) => aggregate + segmentSeparator + item);

                    Font stringFont = new Font("Arial", 14);
                    int width = 250;
                    int height = image.Height;
                    var label = new Bitmap(width, height);
                    var g1 = Graphics.FromImage(label);
                    g1.Clear(Color.Gray);
                    g1.DrawString(labelText, stringFont, Brushes.Black, new PointF(4, 30));
                    g1.DrawLine(new Pen(Color.Black), 0, 0, width, 0); //draw upper boundary
                    g1.DrawLine(new Pen(Color.Black), 0, 1, width, 1); //draw upper boundary

                    Image[] imagearray = { label, image };
                    var labelledImage = ImageTools.CombineImagesInLine(imagearray);

                    // Add labeled image to list
                    list.Add(labelledImage);

                    // Update maximal width of image
                    if (image.Width > maxImageWidth)
                    {
                        maxImageWidth = image.Width;
                    }
                }

                bandCount += 1;
                Log.InfoFormat("Completed band {0}/{1}", bandCount, bandsList.Count);
            }

            if (analysisSettings.AnalysisDataSaveBehavior)
            {
                this.WriteSummaryIndicesFile(segmentSettings.SegmentSummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = segmentSettings.SegmentSummaryIndicesFile;
                analysisResults.SpectraIndicesFiles =
                    this.WriteSpectrumIndicesFiles(
                        segmentSettings.SegmentSpectrumIndicesDirectory,
                        Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name),
                        analysisResults.SpectralIndices);
            }

            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave())
            {
                Image finalImage = ImageTools.CombineImagesVertically(list.ToArray(), maxImageWidth);
                finalImage.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
                LoggedConsole.WriteLine("See {0} for spectrogram pictures", imagePath);
            }

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

        private static Dictionary<IndexCalculateConfig, List<SpectralIndexBase>> GroupResultsOnConfiguration(IEnumerable<SpectralIndexBase> results)
        {
            Dictionary<IndexCalculateConfig, List<SpectralIndexBase>> dict = new Dictionary<IndexCalculateConfig, List<SpectralIndexBase>>();

            // Group the results based on the band configuration they have
            foreach (var spectralIndexBase in results)
            {
                var spectralIndexValues = (SpectralIndexValues)spectralIndexBase;

                var spectralIndexValuesConfiguration = spectralIndexValues.Configuration;

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

        public static void CheckForIdenticalBands(
            List<StandardizedFeatureExtractionConfig.BandsProperties> bandsList)
        {
            var distinctItems = bandsList.Distinct().ToList().Count();

            if (distinctItems != bandsList.Count)
            {
                var message = "There are one or more identical bands in the configuration file";
                throw new ConfigFileException(message);
            }
        }

        public override List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            // Group results based on configuration
            Dictionary<IndexCalculateConfig, List<SpectralIndexBase>> dict = GroupResultsOnConfiguration(results);

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
                if (config.Configuration.MelScale != 0)
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