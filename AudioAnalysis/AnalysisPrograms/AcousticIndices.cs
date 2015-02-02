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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisBase.ResultBases;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.WavTools;

    using PowerArgs;

    using TowseyLibrary;
    using AudioAnalysisTools.StandardSpectrograms;

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

                Log.Verbosity = 1;
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
                    Log.WriteLine(
                        "\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.",
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
            int subsegmentCount = (int)Math.Floor(recordingDuration / subsegmentDuration);

            analysisResults.SummaryIndices  = new SummaryIndexBase[subsegmentCount];
            analysisResults.SpectralIndices = new SpectralIndexBase[subsegmentCount];

            
            for (int i = 0; i < subsegmentCount; i++ )
            {

                analysisSettings.SubsegmentOffset = analysisSettings.SegmentStartOffset  + TimeSpan.FromSeconds(i * subsegmentDuration);

                // ######################################################################
                var indexCalculateResult = IndexCalculate.Analysis(recording, analysisSettings);

                // ######################################################################

                analysisResults.SummaryIndices[i]  = indexCalculateResult.SummaryIndexValues;
                analysisResults.SpectralIndices[i] = indexCalculateResult.SpectralIndexValues;

            } // subSegment loop

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

            // write the one minute spectrogram to CSV
            SonogramConfig sonoConfig = new SonogramConfig(); // default values config
            sonoConfig.SourceFName = recording.FilePath;
            sonoConfig.WindowSize = (int?)analysisSettings.Configuration[AnalysisKeys.FrameLength] ?? IndexCalculate.DefaultWindowSize;
            sonoConfig.WindowOverlap = (double?)analysisSettings.Configuration[AnalysisKeys.FrameOverlap] ?? 0.0; // the default
            //sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            // remove the DC row of the spectrogram
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.Data.GetLength(0) - 1, sonogram.Data.GetLength(1) - 1);
            string csvPath = Path.Combine(outputDirectory.FullName, recording.FileName + ".csv");
            Csv.WriteMatrixToCsv(csvPath.ToFileInfo(), sonogram.Data);





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


            string fileName = Path.GetFileNameWithoutExtension(sourceAudio.Name);


            int frameWidth = 512;
            frameWidth = settings.Configuration[AnalysisKeys.FrameLength] ?? frameWidth;
            int sampleRate = AppConfigHelper.DefaultTargetSampleRate;
            sampleRate = settings.Configuration[AnalysisKeys.ResampleRate] ?? sampleRate;

            // gather spectra to form spectrograms.  Assume same spectra in all analyser results
            // this is the most effcient way to do this
            // gather up numbers and strings store in memory, write to disk one time
            // this method also AUTOMATICALLY SORTS because it uses array indexing
            var startMinute = (int)(inputFileSegment.SegmentStartOffset ?? TimeSpan.Zero).TotalMinutes;

            var config = new LdSpectrogramConfig
                             {
                                 FileName = fileName,
                                 OutputDirectoryInfo = resultsDirectory,
                                 InputDirectoryInfo = resultsDirectory,
                                 SampleRate = sampleRate,
                                 FrameWidth = frameWidth,

                                 IndexCalculationDuration = settings.IndexCalculationDuration.Value,
                                 XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,

                                 MinuteOffset = SpectrogramConstants.MINUTE_OFFSET,
                                 ColourMap2 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                                 ColourMap1 = SpectrogramConstants.RGBMap_BGN_AVG_CVR,
                                 BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF       
                             };

            // THIS iS A TEMPORARY HACK - IT FORCES GRID LINES TO BE 60 pixels apart.
            if (config.IndexCalculationDuration != null)
            {
                double interval = config.IndexCalculationDuration.TotalSeconds * 60;
                config.XAxisTicInterval = TimeSpan.FromSeconds(interval);
            }

            FileInfo indicesPropertiesConfig = FindIndicesConfig.Find(settings.Configuration, settings.ConfigFile);

            var dictionaryOfSpectra = spectralIndices.ToTwoDimensionalArray(SpectralIndexValues.CachedSelectors, TwoDimensionalArray.ColumnMajorFlipped);

            LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(config, indicesPropertiesConfig, dictionaryOfSpectra);

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



        /* // deprecated
        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0))
            {
                return null;
            }
            //get its column headers
            var dtHeaders = new List<string>();
            var dtTypes = new List<Type>();
            foreach (DataColumn col in dt.Columns)
            {
                dtHeaders.Add(col.ColumnName);
                dtTypes.Add(col.DataType);
            }

            List<string> displayHeaders = null;
            bool addColumnOfweightedIndices = true;
            //check if config file contains list of display headers
            if (fiConfigFile != null)
            {
                var configuration = new ConfigDictionary(fiConfigFile.FullName);
                Dictionary<string, string> configDict = configuration.GetTable();
                if (configDict.ContainsKey(AnalysisKeys.DISPLAY_COLUMNS))
                {
                    displayHeaders = configDict[AudioAnalysisTools.AnalysisKeys.DISPLAY_COLUMNS].Split(',').ToList();
                    for (int i = 0; i < displayHeaders.Count; i++)
                    {
                        displayHeaders[i] = displayHeaders[i].Trim();
                    }
                }

            }
            //if config file does not exist or does not contain display headers then use the original headers
            if (displayHeaders == null) displayHeaders = dtHeaders; //use existing headers if user supplies none.

            //now determine how to display tracks in display datatable
            Type[] displayTypes = new Type[displayHeaders.Count];
            bool[] canDisplay = new bool[displayHeaders.Count];
            for (int i = 0; i < displayTypes.Length; i++)
            {
                displayTypes[i] = typeof(double);
                canDisplay[i] = false;
                if (dtHeaders.Contains(displayHeaders[i])) canDisplay[i] = true;
            }

            //order the table if possible
            if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_START_ABS + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.EVENT_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.EVENT_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KEY_RankOrder))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KEY_RankOrder + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.KEY_StartMinute))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.KEY_StartMinute + " ASC");
            }

            DataTable table2Display = null;
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()*/


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
