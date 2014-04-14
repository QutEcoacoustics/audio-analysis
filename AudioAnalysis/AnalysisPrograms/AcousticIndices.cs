// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticIndices.cs" company="MQUTeR">
//   -
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
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools;
    using AudioAnalysisTools.Indices;

    using PowerArgs;

    using TowseyLibrary;

    public class Acoustic : IAnalyser
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

        private const string identifier = "Towsey." + AnalysisName;

        public string Identifier
        {
            get { return identifier; }
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
                var eventsFname = string.Format("{0}_{1}min.{2}.Events.csv", segmentFileStem, startMinute, identifier);
                var indicesFname = string.Format("{0}_{1}min.{2}.Indices.csv", segmentFileStem, startMinute, identifier);

                if (true) // task_ANALYSE
                {
                    arguments.Task = TaskAnalyse;
                    arguments.Source = recordingPath.ToFileInfo();
                    arguments.Config = configPath.ToFileInfo();
                    arguments.Output = outputDir.ToDirectoryInfo();
                    arguments.TmpWav = segmentFName;
                    arguments.Indices = indicesFname;
                    arguments.Start = (int?)tsStart.TotalSeconds;
                    arguments.Duration = (int?)tsDuration.TotalSeconds;
                }
                if (false) // task_LOAD_CSV
                {
                    //string indicesImagePath = "some path or another";
                    arguments.Task = TaskLoadCsv;
                    arguments.InputCsv = csvPath.ToFileInfo();
                    arguments.Config = configPath.ToFileInfo();
                }
            }

            //Execute(arguments);

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
        } // Dev()

        /// <summary>
        /// Directs task to the appropriate method based on the first argument in the command line string.
        /// </summary>
        /// <returns></returns>
        //public static void Execute(Arguments arguments)
        //{
        //    Contract.Requires(arguments != null);
  
        //    // loads a csv file for visulisation
        //    if (arguments.TaskIsLoadCsv)
        //    {
        //        string[] defaultColumns2Display = { "avAmp-dB", "snr-dB", "bg-dB", "activity", "segCount", "avSegDur", "hfCover", "mfCover", "lfCover", "H[ampl]", "H[avSpectrum]", "#clusters", "avClustDur" };
        //        var fiCsvFile = arguments.InputCsv;
        //        var fiConfigFile = arguments.Config;
        //        //var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
        //        IAnalyser analyser = new Acoustic();
        //        var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
        //        //returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
        //    } 
        //    else if (arguments.TaskIsAnalyse)
        //    {
        //        // perform the analysis task
        //        ExecuteAnalysis(arguments);
        //    }
        //} // Execute()


        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        //public static void ExecuteAnalysis(Arguments args)
        //{
        //    // Check arguments and that paths are valid
        //    AnalysisSettings analysisSettings = GetAndCheckAllArguments(args);
        //    analysisSettings.StartOfSegment = new TimeSpan(0, 0, args.Start ?? 0);
        //    analysisSettings.SegmentMaxDuration = new TimeSpan(0, 0, args.Duration ?? 0);

        //    // EXTRACT THE REQUIRED RECORDING SEGMENT
        //    FileInfo fiSource = analysisSettings.SourceFile;
        //    FileInfo tempF = analysisSettings.AudioFile;
        //    if (tempF.Exists) { tempF.Delete(); }

        //    // GET INFO ABOUT THE SOURCE and the TARGET files - esp need the sampling rate
        //    AudioUtilityModifiedInfo beforeAndAfterInfo;

        //    if (analysisSettings.SegmentMaxDuration != null) // Process entire file
        //    {
        //        beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = IndexCalculate.RESAMPLE_RATE }, analysisSettings.AnalysisBaseTempDirectoryChecked);
        //    }
        //    else
        //    {
        //        beforeAndAfterInfo = AudioFilePreparer.PrepareFile(
        //            fiSource,
        //            tempF,
        //            new AudioUtilityRequest
        //            {
        //                TargetSampleRate = IndexCalculate.RESAMPLE_RATE,
        //                OffsetStart = analysisSettings.StartOfSegment,
        //                OffsetEnd = analysisSettings.StartOfSegment.Value.Add(analysisSettings.SegmentMaxDuration.Value)
        //            }, analysisSettings.AnalysisBaseTempDirectoryChecked);
        //    }

        //    // Store source sample rate - may need during the analysis if have upsampled the source.
        //    analysisSettings.SampleRateOfOriginalAudioFile = beforeAndAfterInfo.SourceInfo.SampleRate;

        //    // DO THE ANALYSIS
        //    // #############################################################################################################################################
        //    IAnalyser analyser = new Acoustic();
        //    AnalysisResult result = analyser.Analyse(analysisSettings);
        //    DataTable dt = result.Data;
        //    if (dt == null) { throw new InvalidOperationException("Data table of results is null"); }
        //    // #############################################################################################################################################

        //    // ADD IN ADDITIONAL INFO TO RESULTS TABLE
        //    int iter = 0; // dummy - iteration number would ordinarily be available at this point.
        //    int startMinute = (int)(args.Start ?? 0);
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        row[IndexProperties.header_count] = iter;
        //        row[IndexProperties.header_startMin] = startMinute;
        //        row[IndexProperties.header_SecondsDuration] = result.AudioDuration.TotalSeconds;
        //    }

        //    CsvTools.DataTable2CSV(dt, analysisSettings.IndicesFile.FullName);
        //    //DataTableTools.WriteTable2Console(dt);

        //    // WRITE SUMMARY SPECTRA TO FILE HERE
        //    int ID = result.SegmentStartOffset.Minutes;
        //    string path = analysisSettings.IndicesFile.FullName;
        //    string dir = Path.GetDirectoryName(path);
        //    string fname = Path.GetFileNameWithoutExtension(path);
        //    string csvFilePath1 = Path.Combine(dir, fname + ".bgnSpectrum.csv");
        //    CsvTools.AppendRow2CSVFile(csvFilePath1, ID, result.Spectra[SpectrogramConstants.KEY_BackgroundNoise]);
        //    string csvFilePath2 = Path.Combine(dir, fname + ".aciSpectrum.csv");
        //    CsvTools.AppendRow2CSVFile(csvFilePath2, ID, result.Spectra[SpectrogramConstants.KEY_AcousticComplexityIndex]);
        //    string csvFilePath3 = Path.Combine(dir, fname + ".avgSpectrum.csv");
        //    CsvTools.AppendRow2CSVFile(csvFilePath3, ID, result.Spectra[SpectrogramConstants.KEY_Average]);
        //    string csvFilePath4 = Path.Combine(dir, fname + ".varSpectrum.csv");
        //    CsvTools.AppendRow2CSVFile(csvFilePath4, ID, result.Spectra[SpectrogramConstants.KEY_Variance]);
        //} // ExecuteAnalysis()



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.SegmentStartOffset = analysisSettings.StartOfSegment.HasValue ? analysisSettings.StartOfSegment.Value : TimeSpan.Zero;
            analysisResults.Data = null;

            // ######################################################################
            var indicesStore = IndexCalculate.Analysis(fiAudioF, analysisSettings);

            // ######################################################################
            if (indicesStore == null)
            {
                return analysisResults; // nothing to process 
            }

            // analysisResults.Data = IndexStore.Indices2DataTable(indicesStore);
            analysisResults.Data = null; // indicates that the analysis is of acoustic indices and not events.
            analysisResults.indexBase = indicesStore;
            analysisResults.AudioDuration = indicesStore.GetIndexAsTimeSpan(IndexProperties.keySEC_DUR);
            analysisResults.SegmentStartOffset = (TimeSpan)analysisSettings.StartOfSegment;

            

            if ((indicesStore.Sg != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = Path.Combine(diOutputDir.FullName, analysisSettings.ImageFile.Name);
                var image = DrawSonogram(indicesStore.Sg, indicesStore.Hits, indicesStore.TrackScores, indicesStore.Tracks);
                //var fiImage = new FileInfo(imagePath);
                //if (fiImage.Exists) fiImage.Delete();
                image.Save(imagePath, ImageFormat.Png);
                analysisResults.ImageFile = new FileInfo(imagePath);
            }

            if ((analysisSettings.IndicesFile != null) && (analysisResults.Data != null))
            {
                CsvTools.DataTable2CSV(analysisResults.Data, analysisSettings.IndicesFile.FullName);
            }

            return analysisResults;
        } // Analyse()


        /// <summary>
        /// This analysis type only calculates acoustic indices. There are no events to convert to indices.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="unitTime"></param>
        /// <param name="timeDuration"></param>
        /// <param name="scoreThreshold"></param>
        /// <returns></returns>
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            return null;
        }



        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<SpectralTrack> tracks)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency;
            //int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));

            if (scores != null)
            {
                foreach (Plot plot in scores)
                    image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
            }
            if (tracks != null) image.AddTracks(tracks, sonogram.FramesPerSecond, sonogram.FBinWidth);
            if (hits != null) image.OverlayRedMatrix(hits, 1.0);
            return image.GetImage();
        } //DrawSonogram()




        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true); //get original data table
            if ((dt == null) || (dt.Rows.Count == 0)) return null;
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
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.INDICES_COUNT))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.INDICES_COUNT + " ASC");
            }
            else if (dt.Columns.Contains(AudioAnalysisTools.AnalysisKeys.START_MIN))
            {
                dt = DataTableTools.SortTable(dt, AudioAnalysisTools.AnalysisKeys.START_MIN + " ASC");
            }

            DataTable table2Display = null;
            return System.Tuple.Create(dt, table2Display);
        } // ProcessCsvFile()


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
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
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
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.SourceFile = args.Source;
            analysisSettings.ConfigFile = args.Config;
            analysisSettings.AnalysisInstanceOutputDirectory = args.Output;
            analysisSettings.AudioFile = null;
            analysisSettings.EventsFile = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile = null;

            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);
            var configuration = new ConfigDictionary(args.Config);
            analysisSettings.ConfigDict = configuration.GetTable();

            if (!string.IsNullOrWhiteSpace(args.TmpWav))
            {
                string indicesPath = Path.Combine(args.Output.FullName, args.TmpWav);
                analysisSettings.IndicesFile = new FileInfo(indicesPath);
            }


            if (!string.IsNullOrWhiteSpace(args.Indices))
            {
                string indicesPath = Path.Combine(args.Output.FullName, args.Indices);
                analysisSettings.IndicesFile = new FileInfo(indicesPath);
            }

            return analysisSettings;
        } // CheckAllArguments()


    } //end class Acoustic
}
