// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticIndices.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the Acoustic type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Shared.Extensions;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.TileImage;
    using AudioAnalysisTools.WavTools;

    using log4net;

    using PowerArgs;

    using TowseyLibrary;

    public class Acoustic : IAnalyser2
    {
        [CustomDetailedDescription]
        public class Arguments : IArgClassValidator
        {
            [ArgDescription("The task to execute, either `" + TaskAnalyse + "` or `" + TaskLoadCsv + "`")]
            [ArgRequired]
            [ArgPosition(1)]
            [ArgOneOfThese(TaskAnalyse, TaskLoadCsv, ExceptionMessage = "The task to execute is not recognized.")]
            public string Task { get; set; }

            [ArgIgnore]
            public bool TaskIsAnalyse
            {
                get
                {
                    return string.Equals(this.Task, TaskAnalyse, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            [ArgIgnore]
            public bool TaskIsLoadCsv
            {
                get
                {
                    return string.Equals(this.Task, TaskLoadCsv, StringComparison.InvariantCultureIgnoreCase);
                }
            }

            [ArgDescription("The path to the config file")]
            [Production.ArgExistingFile()]
            [ArgRequired]
            public FileInfo Config { get; set; }

            [ArgDescription("The source csv file to operate on")]
            [Production.ArgExistingFile(Extension = ".csv")]
            public FileInfo InputCsv { get; set; }

            [ArgDescription("The source audio file to operate on")]
            [Production.ArgExistingFile()]
            public FileInfo Source { get; set; }

            [ArgDescription("A directory to write output to")]
            [Production.ArgExistingDirectory(createIfNotExists: true)]
            public DirectoryInfo Output { get; set; }

            public string TmpWav { get; set; }

            public string Indices { get; set; }

            [ArgDescription("The start offset to start analysing from (in seconds)")]
            [ArgRange(0, double.MaxValue)]
            public int? Start { get; set; }

            [ArgDescription("The duration of each segment to analyse (seconds) - a maximum of 10 minutes")]
            [ArgRange(0, 10 * 60)]
            public int? Duration { get; set; }

            public void Validate()
            {
                if (this.TaskIsLoadCsv)
                {
                    if (this.InputCsv == null || this.Output != null || this.Source != null || this.TmpWav != null
                        || this.Indices != null || this.Start != null || this.Duration != null)
                    {
                        throw new ValidationArgException(
                            "For the " + TaskLoadCsv + "task, InputCsv must be specified and other fields not specified");
                    }
                }

                if (this.TaskIsAnalyse)
                {
                    if (InputCsv != null)
                    {
                        throw new ValidationArgException(
                            "InputCsv should be specifiec in the " + TaskAnalyse + " action");
                    }

                    if (Source == null)
                    {
                        throw new MissingArgException("Source is required for action:" + TaskAnalyse);
                    }

                    if (Output == null)
                    {
                        throw new MissingArgException("Output is required for action:" + TaskAnalyse);
                    }
                }
            }

            public static string AdditionalNotes()
            {
                return "NOTE: This class has two distinct options";
            }
        }

        // OTHER CONSTANTS
        public const string AnalysisName = "Acoustic";

        // TASK IDENTIFIERS
        public const string TaskAnalyse = AnalysisName;
        public const string TaskLoadCsv = "loadCsv";
        public const string TowseyAcoustic = "Towsey." + AnalysisName;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string DisplayName
        {
            get { return "Acoustic Indices"; }
        }

        public string Identifier
        {
            get
            {
                return TowseyAcoustic;
            }
        }


        public static void Dev(Arguments arguments)
        {
            bool executeDev = arguments == null;
            if (executeDev)
            {
                arguments = new Arguments();
                //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
                //string configPath = @"C:\SensorNetworks\Output\AcousticIndices\Indices.cfg";
                //string outputDir = @"C:\SensorNetworks\Output\AcousticIndices\";
                //string csvPath = @"C:\SensorNetworks\Output\AcousticIndices\AcousticIndices.csv";

                string recordingPath = @"C:\SensorNetworks\WavFiles\SunshineCoast\DM420036_min407.wav";
                string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Acoustic.cfg";
                string outputDir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic";
                string csvPath =
                    @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic\DM420036_min407_Towsey.Acoustic.Indices.csv";

                //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows\Crows111216-001Mono5-7min.mp3";
                //string configPath = @"C:\SensorNetworks\Output\SunshineCoast\Site1\Towsey.Acoustic\temp.cfg";
                //string outputDir = @"C:\SensorNetworks\Output\Crow\";
                //string csvPath = @"C:\SensorNetworks\Output\Crow\Towsey.Acoustic.Indices.csv";


                string title = "# FOR EXTRACTION OF Acoustic Indices";
                string date = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Output folder:  " + outputDir);
                LoggedConsole.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
                var diOutputDir = new DirectoryInfo(outputDir);

                int startMinute = 0;
                int durationSeconds = 0; //set zero to get entire recording
                var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
                var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
                var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
                var segmentFName = string.Format("{0}_{1}min.wav", segmentFileStem, startMinute);
                var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, "Towsey." + AnalysisName);

                if (true)
                {
                    // task_ANALYSE
                    arguments.Task = TaskAnalyse;
                    arguments.Source = recordingPath.ToFileInfo();
                    arguments.Config = configPath.ToFileInfo();
                    arguments.Output = outputDir.ToDirectoryInfo();
                    arguments.TmpWav = segmentFName;
                    arguments.Indices = indicesFname;
                    arguments.Start = (int?)tsStart.TotalSeconds;
                    arguments.Duration = (int?)tsDuration.TotalSeconds;
                }

                if (false)
                {
                    // task_LOAD_CSV
                    ////string indicesImagePath = "some path or another";
                    arguments.Task = TaskLoadCsv;
                    arguments.InputCsv = csvPath.ToFileInfo();
                    arguments.Config = configPath.ToFileInfo();
                }
            }

            ////Execute(arguments);

            if (executeDev)
            {

                string indicesPath = Path.Combine(arguments.Output.FullName, arguments.Indices);
                FileInfo fiCsvIndices = new FileInfo(indicesPath);
                if (!fiCsvIndices.Exists)
                {
                    Log.InfoFormat(
                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{1}>.",
                        arguments.Start,
                        arguments.Source.FullName);
                }
                else
                {
                    LoggedConsole.WriteLine("\n");
                    DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                    DataTableTools.WriteTable2Console(dt);
                }

                LoggedConsole.WriteLine("\n\n# Finished analysis:- " + arguments.Source.FullName);

            }
            return;
        }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            var configuration = analysisSettings.Configuration;

            FileInfo indicesPropertiesConfig = IndexProperties.Find(configuration, analysisSettings.ConfigFile);
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);
            SpectralIndexValues.CheckExistenceOfSpectralIndexValues(indexProperties);

            bool filenameDate = (bool?)configuration[AnalysisKeys.RequireDateInFilename] ?? false;
            bool tileOutput = (bool?)configuration[AnalysisKeys.TileImageOutput] ?? false;

            // if tiling output we need to be able to parse the date from the file name
            Log.Info("Image tiling is " + (tileOutput ? string.Empty : "NOT ") + "enabled");
            if (tileOutput)
            {
                if (!filenameDate)
                {
                    throw new ConfigFileException("If TileImageOutput is set then RequireDateInFilename must be set as well");
                }
            }


            // set IndexCalculationDuration i.e. duration of a subsegment
            TimeSpan indexCalculationDuration;
            try
            {
                double indexCalculationDurationSeconds = (double)configuration[AnalysisKeys.IndexCalculationDuration];
                indexCalculationDuration = TimeSpan.FromSeconds(indexCalculationDurationSeconds);
            }
            catch (Exception ex)
            {
                indexCalculationDuration = analysisSettings.SegmentMaxDuration.Value;
                Log.Warn("Cannot read IndexCalculationDuration from config file (Exceptions squashed. Used default value = " + indexCalculationDuration.ToString() + ")");
            }

            if (tileOutput && indexCalculationDuration != TimeSpan.FromSeconds(60))
            {
                throw new ConfigFileException("Invalid configuration detected: tile image output is enabled but ICD != 60.0 so the images won'tbe created");
            }

            // set background noise neighborhood
            TimeSpan bgNoiseNeighborhood;
            try
            {
                int bgnNh = configuration[AnalysisKeys.BGNoiseNeighbourhood];
                bgNoiseNeighborhood = TimeSpan.FromSeconds(bgnNh);
            }
            catch (Exception ex)
            {
                bgNoiseNeighborhood = analysisSettings.SegmentMaxDuration.Value;
                Log.Warn("Cannot read BGNNeighborhood from config file (Exceptions squashed. Used default value = " + bgNoiseNeighborhood.ToString() + ")");
            }

            analysisSettings.AnalyzerSpecificConfiguration = new AcousticIndicesParsedConfiguration(tileOutput, indexCalculationDuration, bgNoiseNeighborhood, indicesPropertiesConfig);
        }

        [Serializable]
        private class AcousticIndicesParsedConfiguration
        {
            public AcousticIndicesParsedConfiguration(bool tileOutput, TimeSpan indexCalculationDuration, TimeSpan bgNoiseNeighborhood, FileInfo indexPropertiesFile)
            {
                this.TileOutput = tileOutput;
                this.IndexCalculationDuration = indexCalculationDuration;
                this.BgNoiseNeighborhood = bgNoiseNeighborhood;
                this.IndexPropertiesFile = indexPropertiesFile;
            }

            public bool TileOutput { get; private set; }

            /// <summary>
            /// Gets the duration of the sub-segment for which indices are calculated. 
            /// Default = 60 seconds i.e. same duration as the Segment.
            /// </summary>
            public TimeSpan IndexCalculationDuration { get; private set; }

            /// <summary>
            /// Gets the amount of audio either side of the required subsegment from which to derive an estimate of background noise. 
            /// Units = seconds
            /// As an example: IF (IndexCalculationDuration = 1 second) AND (BGNNeighborhood = 10 seconds) 
            ///                THEN BG noise estimate will be derived from 21 seconds of audio centred on the subsegment.
            ///                In case of edge effects, the BGnoise neighborhood will be truncated to start or end of the audio segment (typically expected to be one minute long).
            /// </summary>
            public TimeSpan BgNoiseNeighborhood { get; private set; }

            public FileInfo IndexPropertiesFile { get; private set; }
        }

        public AnalysisResult2 Analyze(AnalysisSettings analysisSettings)
        {
            var acousticIndicesParsedConfiguration = (AcousticIndicesParsedConfiguration)analysisSettings.AnalyzerSpecificConfiguration;

            var audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, recording.Duration());
            analysisResults.AnalysisIdentifier = this.Identifier;

            double recordingDuration = recording.Duration().TotalSeconds;
            double subsegmentDuration = acousticIndicesParsedConfiguration.IndexCalculationDuration.TotalSeconds;
            
            // intentional possible null ref, throw if not null
            double segmentDuration = analysisSettings.SegmentDuration.Value.TotalSeconds;
            double audioCuttingError = subsegmentDuration - segmentDuration;
            
            // using the expected duration, each call to analyze will always produce the same number of results
            // round, we expect perfect numbers, warn if not
            double subsegmentsInSegment = segmentDuration / subsegmentDuration;
            int subsegmentCount = (int)Math.Round(segmentDuration / subsegmentDuration);
            const double WarningThreshold = 0.01; // 10%
            double fraction = subsegmentsInSegment - subsegmentCount;
            if (Math.Abs(fraction) > WarningThreshold)
            {
                Log.Warn(
                    string.Format(
                        "The IndexCalculationDuration ({0}) does not fit well into the provided segment ({1}). This means a partial result has been {3}, {2} results will be calculated",
                        subsegmentDuration,
                        segmentDuration,
                        subsegmentCount,
                        fraction >= 0.5 ? "added" : "removed"));
            }
            Log.Trace(subsegmentCount.ToString() + " sub segments will be calculated");

            analysisResults.SummaryIndices  = new SummaryIndexBase[subsegmentCount];
            analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentCount];

            var trackScores = new List<Plot>(subsegmentCount);
            var tracks = new List<SpectralTrack>(subsegmentCount);

            // calculate indices for each subsegment
            for (int i = 0; i < subsegmentCount; i++)
            {
                var subsegmentOffset = (analysisSettings.SegmentStartOffset ?? TimeSpan.Zero)  + TimeSpan.FromSeconds(i * subsegmentDuration);

                /* ###################################################################### */

                var indexCalculateResult = IndexCalculate.Analysis(
                    recording,
                    analysisSettings,
                    subsegmentOffset,
                    acousticIndicesParsedConfiguration.IndexCalculationDuration,
                    acousticIndicesParsedConfiguration.BgNoiseNeighborhood,
                    acousticIndicesParsedConfiguration.IndexPropertiesFile);
                
                /* ###################################################################### */

                analysisResults.SummaryIndices[i]  = indexCalculateResult.SummaryIndexValues;
                analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;
                trackScores.AddRange(indexCalculateResult.TrackScores);
                if (indexCalculateResult.Tracks != null)
                {
                    tracks.AddRange(indexCalculateResult.Tracks);
                }
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = analysisSettings.SummaryIndicesFile;
            }

            if (analysisSettings.SpectrumIndicesDirectory != null)
            {
                analysisResults.SpectraIndicesFiles =
                    this.WriteSpectrumIndicesFilesCustom(
                        analysisSettings.SpectrumIndicesDirectory,
                        Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name),
                        analysisResults.SpectralIndices);
            }

            // write the segment spectrogram (typically of one minute duration) to CSV
            // this is required if you want to produced zoomed spectrograms at a resolution greater than 0.2 seconds/pixel 
            bool saveSonogramData = (bool?)analysisSettings.Configuration[AnalysisKeys.SaveSonogramData] ?? false;
            if (saveSonogramData || analysisSettings.ImageFile != null) 
            {
                var sonoConfig = new SonogramConfig(); // default values config
                sonoConfig.SourceFName = recording.FilePath;
                sonoConfig.WindowSize = (int?)analysisSettings.Configuration[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
                sonoConfig.WindowStep = (int?)analysisSettings.Configuration[AnalysisKeys.FrameStep] ?? sonoConfig.WindowSize; // default = no overlap
                sonoConfig.WindowOverlap = (sonoConfig.WindowSize - sonoConfig.WindowStep) / (double)sonoConfig.WindowSize;
                ////sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                ////sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
                
                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);

                if (analysisSettings.ImageFile != null)
                {
                    string imagePath = Path.Combine(outputDirectory.FullName, analysisSettings.ImageFile.Name);
                    
                    // NOTE: hits (SPT in this case) is intentionally not supported
                    var image = DrawSonogram(sonogram, null, trackScores, tracks);
                    image.Save(imagePath, ImageFormat.Png);
                    analysisResults.ImageFile = new FileInfo(imagePath);
                }

                if (saveSonogramData)
                {
                    string csvPath = Path.Combine(outputDirectory.FullName, recording.FileName + ".csv");
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
            Csv.WriteToCsv(destination, results);
        }

        public List<FileInfo> WriteSpectrumIndicesFilesCustom(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var selectors = results.First().GetSelectors();

            var spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {
                // write spectrogram to disk as CSV file
                var filename = FilenameHelpers.AnalysisResultName(destination, fileNameBase, this.Identifier + "." + kvp.Key, "csv").ToFileInfo();
                spectralIndexFiles.Add(filename);
                Csv.WriteMatrixToCsv(filename, results, kvp.Value);
            }

            return spectralIndexFiles;
        }

        public void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            this.WriteSpectrumIndicesFilesCustom(destination, fileNameBase, results);
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold, bool absolute = false)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
            var acousticIndicesParsedConfiguration = (AcousticIndicesParsedConfiguration)settings.AnalyzerSpecificConfiguration;

            var sourceAudio = inputFileSegment.OriginalFile;
            var resultsDirectory = settings.AnalysisInstanceOutputDirectory;
            bool tileOutput = acousticIndicesParsedConfiguration.TileOutput;

            int frameWidth = 512;
            frameWidth = settings.Configuration[AnalysisKeys.FrameLength] ?? frameWidth;
            int sampleRate = AppConfigHelper.DefaultTargetSampleRate;
            sampleRate = settings.Configuration[AnalysisKeys.ResampleRate] ?? sampleRate;

            // gather settings for rendering false color spectrograms
            var ldSpectrogramConfig = new LdSpectrogramConfig
            {
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_CVR,
            };

            /* NOTE: The value for FrameStep is used only when calculating a standard spectrogram
             * FrameStep is NOT used when calculating Summary and Spectral indices.
             */

            string basename = Path.GetFileNameWithoutExtension(sourceAudio.Name);

            // output to disk (so other analysers can use the data,
            // only data - configuration settings that generated these indices
            // this data can then be used by post-process analyses
            var indexConfigData = new IndexGenerationData()
                                      {
                                          RecordingType  = inputFileSegment.OriginalFile.Extension,
                                          RecordingStartDate = inputFileSegment.OriginalFileStartDate,
                                          SampleRateOriginal = (int)inputFileSegment.OriginalFileSampleRate,
                                          SampleRateResampled = sampleRate,
                                          FrameLength = frameWidth,
                                          FrameStep = settings.Configuration[AnalysisKeys.FrameStep],
                                          IndexCalculationDuration = acousticIndicesParsedConfiguration.IndexCalculationDuration,
                                          BGNoiseNeighbourhood = acousticIndicesParsedConfiguration.BgNoiseNeighborhood,
                                          MinuteOffset = inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero,
                                          BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF,
                                          LongDurationSpectrogramConfig = ldSpectrogramConfig
                                      };
            var icdPath = FilenameHelpers.AnalysisResultName(
                resultsDirectory,
                basename,
                IndexGenerationData.FileNameFragment,
                "json");
            Json.Serialise(icdPath.ToFileInfo(), indexConfigData);

            // gather spectra to form spectrograms.  Assume same spectra in all analyzer results
            // this is the most efficient way to do this
            // gather up numbers and strings store in memory, write to disk one time
            // this method also AUTOMATICALLY SORTS because it uses array indexing
            var dictionaryOfSpectra = spectralIndices.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

            // Calculate the index distribution statistics and write to a json file. Also save as png image
            var indexDistributions = IndexDistributions.WriteIndexDistributionStatistics(dictionaryOfSpectra, resultsDirectory, basename);

            // HACK: do not render false color spectrograms unless IndexCalculationDuration = 60.0 (the normal resolution)
            if (acousticIndicesParsedConfiguration.IndexCalculationDuration != TimeSpan.FromSeconds(60.0))
            {
                Log.Warn("False color spectrograms were not rendered");
            }
            else
            {
                FileInfo indicesPropertiesConfig = acousticIndicesParsedConfiguration.IndexPropertiesFile;

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
                        indexDistributions: indexDistributions,
                        returnChromelessImages: tileOutput);

                if (tileOutput)
                {
                    Debug.Assert(images.Length == 2);

                    Log.Info("Tiling output at scale: " + acousticIndicesParsedConfiguration.IndexCalculationDuration);

                    var image = images[1];
                    TileOutput(resultsDirectory, Path.GetFileNameWithoutExtension(sourceAudio.Name), image.Item2 + ".Tile", inputFileSegment.OriginalFileStartDate.Value, image.Item1);
                }
            }
        }

        private static void TileOutput(DirectoryInfo outputDirectory, string fileStem, string analysisTag, DateTimeOffset recordingStartDate, Image image)
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
            var timeOfDay = recordingStartDate.TimeOfDay;
            var previousAbsoluteHour = TimeSpan.FromSeconds(Math.Floor(timeOfDay.TotalSeconds / (Scale * TileWidth)) * (Scale * TileWidth));
            var gap = timeOfDay - previousAbsoluteHour;
            var tilingStartDate = recordingStartDate - gap;

            var tilingProfile = new AbsoluteDateTilingProfile(fileStem, analysisTag, tilingStartDate, TileHeight, TileWidth);

            // pad out image so it produces a whole number of tiles
            // this solves the asymmetric right padding of short audio files
            var width = (int)(Math.Ceiling(image.Width / Scale) * Scale);
            var tiler = new Tiler(outputDirectory, tilingProfile, Scale, width, 1.0, image.Height);

            // prepare super tile
            var tile = new TimeOffsetSingleLayerSuperTile() { Image = image, TimeOffset = gap, Scale = scale};

            tiler.Tile(tile);
        }

        private static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<SpectralTrack> tracks)
        {
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage());
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            if (scores != null)
            {
                foreach (Plot plot in scores)
                {
                    // assumes data normalized in 0,1
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title));
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

        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(20),
                    SegmentMediaType = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.ResampleRate
                };
            }
        }

        /// <summary>
        /// Checks the command line arguments
        /// returns Analysis Settings
        /// NEED TO REWRITE THIS METHOD AS APPROPRIATE
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static AnalysisSettings GetAndCheckAllArguments(Arguments args)
        {
            // INIT ANALYSIS SETTINGS
            var analysisSettings = new AnalysisSettings
                                       {
                                           SourceFile = args.Source,
                                           ConfigFile = args.Config,
                                           AnalysisInstanceOutputDirectory = args.Output,
                                           AudioFile = null,
                                           EventsFile = null,
                                           SummaryIndicesFile = null,
                                           ImageFile = null
                                       };

            var start = TimeSpan.Zero;
            var duration = TimeSpan.Zero;
            var configuration = new ConfigDictionary(args.Config);
            analysisSettings.ConfigDict = configuration.GetTable();

            if (!string.IsNullOrWhiteSpace(args.TmpWav))
            {
                string indicesPath = Path.Combine(args.Output.FullName, args.TmpWav);
                analysisSettings.SummaryIndicesFile = new FileInfo(indicesPath);
            }


            if (!string.IsNullOrWhiteSpace(args.Indices))
            {
                string indicesPath = Path.Combine(args.Output.FullName, args.Indices);
                analysisSettings.SummaryIndicesFile = new FileInfo(indicesPath);
            }

            return analysisSettings;
        }



    }

}
