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
    using System.IO;
    using System.Linq;
    using System.Reflection;

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
            [ArgOneOfThese(TaskAnalyse, TaskLoadCsv, ExceptionMessage = "The task to execute is not recognised.")]
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

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string DisplayName
        {
            get { return "Acoustic Indices"; }
        }

        public string Identifier
        {
            get { return "Towsey." + AnalysisName; }
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

        public void BeforeAnalyse(AnalysisSettings analysisSettings)
        {
            FileInfo indicesPropertiesConfig = FindIndicesConfig.Find(analysisSettings.Configuration, analysisSettings.ConfigFile);
            var indexProperties = IndexProperties.GetIndexProperties(indicesPropertiesConfig);
            SpectralIndexValues.CheckExistenceOfSpectralIndexValues(indexProperties);
        }

        public AnalysisResult2 Analyse(AnalysisSettings analysisSettings)
        {
            var audioFile = analysisSettings.AudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult2(analysisSettings, recording.Duration());
            analysisResults.AnalysisIdentifier = this.Identifier;

            double recordingDuration = recording.Duration().TotalSeconds;
            TimeSpan ts = (TimeSpan)analysisSettings.IndexCalculationDuration;
            double subsegmentDuration = ts.TotalSeconds;
            
            int subsegmentCount = Math.Max((int)Math.Floor(recordingDuration / subsegmentDuration), 1);

            analysisResults.SummaryIndices  = new SummaryIndexBase[subsegmentCount];
            analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentCount];

            // calculate indices for each subsegment
            for (int i = 0; i < subsegmentCount; i++)
            {

                analysisSettings.SubsegmentOffset = analysisSettings.SegmentStartOffset  + TimeSpan.FromSeconds(i * subsegmentDuration);

                // ######################################################################
                var indexCalculateResult = IndexCalculate.Analysis(recording, analysisSettings);

                // ######################################################################

                analysisResults.SummaryIndices[i]  = indexCalculateResult.SummaryIndexValues;
                analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;
            }

            if (analysisSettings.SummaryIndicesFile != null)
            {
                this.WriteSummaryIndicesFile(analysisSettings.SummaryIndicesFile, analysisResults.SummaryIndices);
                analysisResults.SummaryIndicesFile = analysisSettings.SummaryIndicesFile;

                // ############################### SAVE OSCILLATION CSV HERE ###############################
            }

            if (analysisSettings.SpectrumIndicesDirectory != null)
            {
                this.WriteSpectrumIndicesFiles(analysisSettings.SpectrumIndicesDirectory, Path.GetFileNameWithoutExtension(analysisSettings.AudioFile.Name), analysisResults.SpectralIndices);
                analysisResults.SpectraIndicesFiles = spectralIndexFiles;
            }

            // write the segment spectrogram (typically of one minute duration) to CSV
            // this is required if you want to produced zoomed spectrograms at a resolution greater than 0.2 seconds/pixel 
            bool saveSonogramData = (bool?)analysisSettings.Configuration[AnalysisKeys.SaveSonogramData] ?? false;
            if (saveSonogramData) 
            {
                SonogramConfig sonoConfig = new SonogramConfig(); // default values config
                sonoConfig.SourceFName = recording.FilePath;
                sonoConfig.WindowSize = (int?)analysisSettings.Configuration[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
                sonoConfig.WindowStep = (int?)analysisSettings.Configuration[AnalysisKeys.FrameStep] ?? sonoConfig.WindowSize; // default = no overlap
                sonoConfig.WindowOverlap = (sonoConfig.WindowSize - sonoConfig.WindowStep) / (double)sonoConfig.WindowSize;
                //sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
                // remove the DC row of the spectrogram
                sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
                string csvPath = Path.Combine(outputDirectory.FullName, recording.FileName + ".csv");
                Csv.WriteMatrixToCsv(csvPath.ToFileInfo(), sonogram.Data);
            }


            //if ((indexCalculateResult.Sg != null) && (analysisSettings.ImageFile != null))
            //{
            //    string imagePath = Path.Combine(outputDirectory.FullName, analysisSettings.ImageFile.Name);
            //    var image = DrawSonogram(indexCalculateResult.Sg, indexCalculateResult.Hits, indexCalculateResult.TrackScores, indexCalculateResult.Tracks);
            //    image.Save(imagePath, ImageFormat.Png);
            //    analysisResults.ImageFile = new FileInfo(imagePath);

            //    // ############################### SAVE OSCILLATION IMAGE HERE ###############################
            //}


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

        private List<FileInfo> spectralIndexFiles;

        public void WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            var selectors = results.First().GetSelectors();

            spectralIndexFiles = new List<FileInfo>(selectors.Count);

            foreach (var kvp in selectors)
            {   
                // write spectrogram to disk as CSV file
                ////string fileName = ;
                var saveCsvPath = destination.CombineFile(fileNameBase + "." + kvp.Key + ".csv");
                spectralIndexFiles.Add(saveCsvPath);
                Csv.WriteMatrixToCsv(saveCsvPath, results, kvp.Value);
            }
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold, bool absolute = false)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(AnalysisSettings settings, FileSegment inputFileSegment, EventBase[] events, SummaryIndexBase[] indices, SpectralIndexBase[] spectralIndices, AnalysisResult2[] results)
        {
            var sourceAudio = inputFileSegment.OriginalFile;
            var resultsDirectory = settings.AnalysisInstanceOutputDirectory;
            var configFile = settings.ConfigFile;
            bool tileOutput = (bool?)settings.Configuration[AnalysisKeys.TileImageOutput] ?? false;

            int frameWidth = 512;
            frameWidth = settings.Configuration[AnalysisKeys.FrameLength] ?? frameWidth;
            int sampleRate = AppConfigHelper.DefaultTargetSampleRate;
            sampleRate = settings.Configuration[AnalysisKeys.ResampleRate] ?? sampleRate;
     
            // gather settings for rendering false color spectrograms
            string fileName = Path.GetFileNameWithoutExtension(sourceAudio.Name);
            var configInfo = new LdSpectrogramConfig
                             {
                                 AnalysisType = settings.Configuration[AnalysisKeys.AnalysisName],
                                 FileName = fileName,
                                 SampleRate = sampleRate,
                                 FrameWidth = frameWidth,
                                 FrameStep  = settings.Configuration[AnalysisKeys.FrameStep],

                                 IndexCalculationDuration = settings.IndexCalculationDuration.Value,
                                 BGNoiseNeighbourhood     = settings.BGNoiseNeighbourhood.Value,
                                 XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,

                                 // this next line is probably wrong - does not give start of the source recording only a segment of it.
                                 MinuteOffset = inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero,
                                 ColourMap2   = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                                 ColourMap1 = SpectrogramConstants.RGBMap_BGN_POW_CVR,
                                 BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF       
                             };

            // copy relevant config settings to the output directory so user can later refer to the parameters.
            var configFileDestination = new FileInfo(Path.Combine(resultsDirectory.FullName, fileName + ".config.yml"));
            if (configFileDestination.Exists)
            {
#if DEBUG
                configFileDestination.Delete();
#else
                throw new InvalidOperationException("The given file should not exist: " + configFileDestination.FullName);
#endif
            }

            Json.Serialise(configFileDestination, configInfo);

            // var statistics = IndexDistributions.Calculate();
            //Json.Serialise(index distributions path, index statistics);

            // HACK: do not render false color spectrograms unless IndexCalculationDuration = 60.0 (the normal resolution)
            if (settings.IndexCalculationDuration.Value != TimeSpan.FromSeconds(60.0))
            {
                Log.Warn("False color spectrograms were not rendered (or !");
            }
            else
            {
                FileInfo indicesPropertiesConfig = FindIndicesConfig.Find(settings.Configuration, settings.ConfigFile);

                // gather spectra to form spectrograms.  Assume same spectra in all analyser results
                // this is the most effcient way to do this
                // gather up numbers and strings store in memory, write to disk one time
                // this method also AUTOMATICALLY SORTS because it uses array indexing
                var dictionaryOfSpectra = spectralIndices.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

                Tuple<Image, string>[] images = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                    resultsDirectory,
                    resultsDirectory,
                    configFileDestination,
                    indicesPropertiesConfig,
                    dictionaryOfSpectra, 
                    tileOutput);

                if (tileOutput)
                {
                    Debug.Assert(images.Length == 2);

                    Log.Info("Tiling output at scale: " + settings.IndexCalculationDuration.Value);

                    var image = images[1];
                    TileOutput(resultsDirectory, Path.GetFileNameWithoutExtension(sourceAudio.Name) + "_" + image.Item2 + ".Tile", inputFileSegment.OriginalFileStartDate.Value, image.Item1);
                }
            }
        }

        private static void TileOutput(DirectoryInfo outputDirectory, string fileStem, DateTimeOffset recordingStartDate, Image image)
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

            var tilingProfile = new AbsoluteDateTilingProfile(fileStem, tilingStartDate, TileHeight, TileWidth);

            // pad out image so it produces a whole number of tiles
            // this solves the asymetric right padding of short audio files
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
                    //assumes data normalised in 0,1
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
