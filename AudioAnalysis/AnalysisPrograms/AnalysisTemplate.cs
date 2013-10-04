using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using Acoustics.Shared;
using Acoustics.Tools;
using Acoustics.Tools.Audio;
using AnalysisBase;

using TowseyLib;
using AudioAnalysisTools;



namespace AnalysisPrograms
{
    public class AnalysisTemplate : IAnalyser
    {
        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Default";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Default Analysis"; }
        }

        private static string identifier = "Towsey." + ANALYSIS_NAME;
        public string Identifier
        {
            get { return identifier; }
        }


        public static void Dev(string[] args)
        {
            string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            string configPath = @"C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Template.cfg";
            string outputDir = @"C:\SensorNetworks\Output\Test\";
            //string csvPath    = @"C:\SensorNetworks\Output\Test\TEST_Indices.csv";

            string title = "# FOR DETECTION OF ############ using TECHNIQUE ########";
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

            var cmdLineArgs = new List<string>();
            if (true)
            {
                cmdLineArgs.Add(recordingPath);
                cmdLineArgs.Add(configPath);
                cmdLineArgs.Add(outputDir);
                cmdLineArgs.Add("-tmpwav:" + segmentFName);
                cmdLineArgs.Add("-events:" + eventsFname);
                cmdLineArgs.Add("-indices:" + indicesFname);
                //cmdLineArgs.Add("-sgram:" + sonogramFname);
                cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
                cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);
            }
            if (false)
            {
                // loads a csv file for visualisation
                //string indicesImagePath = "some path or another";
                //var fiCsvFile    = new FileInfo(restOfArgs[0]);
                //var fiConfigFile = new FileInfo(restOfArgs[1]);
                //var fiImageFile  = new FileInfo(restOfArgs[2]); //path to which to save image file.
                //IAnalysis analyser = new AnalysisTemplate();
                //var dataTables = analyser.ProcessCsvFile(fiCsvFile, fiConfigFile);
                //returns two datatables, the second of which is to be converted to an image (fiImageFile) for display
            }

            // #############################################################################################################################################
            Execute(cmdLineArgs.ToArray());
            // #############################################################################################################################################

            string eventsPath = Path.Combine(outputDir, eventsFname);
            FileInfo fiCsvEvents = new FileInfo(eventsPath);
            if (!fiCsvEvents.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                LoggedConsole.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(eventsPath, true);
                DataTableTools.WriteTable2Console(dt);
            }
            string indicesPath = Path.Combine(outputDir, indicesFname);
            FileInfo fiCsvIndices = new FileInfo(indicesPath);
            if (!fiCsvIndices.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                LoggedConsole.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
            }
            string imagePath = Path.Combine(outputDir, sonogramFname);
            FileInfo fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                TowseyLib.ProcessRunner process = new TowseyLib.ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            LoggedConsole.WriteLine("\n\n# Finished analysis:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()



        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// Use this when you want to analyse only a short segment of recording i.e. 1-2 miniutes
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="configPath"></param>
        /// <param name="outputPath"></param>
        public static void Execute(string[] args)
        {
            // Check arguments and that paths are valid
            var tuple = GetAndCheckAllArguments(args);
            AnalysisSettings analysisSettings = tuple.Item1;
            TimeSpan tsStart    = tuple.Item2;
            TimeSpan tsDuration = tuple.Item3;

            // EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo fiSource = analysisSettings.SourceFile;
            FileInfo tempF = analysisSettings.AudioFile;
            if (tempF.Exists) { tempF.Delete(); }

            // GET INFO ABOUT THE SOURCE and the TARGET files - esp need the sampling rate
            AudioUtilityModifiedInfo beforeAndAfterInfo;

            if (tsDuration.TotalSeconds == 0) // Process entire file
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AnalysisTemplate.RESAMPLE_RATE }, analysisSettings.AnalysisBaseTempDirectoryChecked);
            }
            else
            {
                beforeAndAfterInfo = AudioFilePreparer.PrepareFile(fiSource, tempF, new AudioUtilityRequest { TargetSampleRate = AnalysisTemplate.RESAMPLE_RATE, OffsetStart = tsStart, OffsetEnd = tsStart.Add(tsDuration) }, analysisSettings.AnalysisBaseTempDirectoryChecked);
            }

            // Store source sample rate - may need during the analysis if have upsampled the source.
            analysisSettings.SampleRateOfOriginalAudioFile = beforeAndAfterInfo.SourceInfo.SampleRate;

            // DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalyser analyser = new AnalysisTemplate();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            if (dt == null) { throw new InvalidOperationException("Data table of results is null"); }
            //#############################################################################################################################################

            // ADD IN ADDITIONAL INFO TO RESULTS TABLE
            AddContext2Table(dt, tsStart, result.AudioDuration);
            CsvTools.DataTable2CSV(dt, analysisSettings.EventsFile.FullName);
            // DataTableTools.WriteTable(augmentedTable);

        } // Execute()


        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            //var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            //Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisInstanceOutputDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = Analysis(fiAudioF, analysisSettings.ConfigDict);
            //######################################################################

            if (results == null) return analysisResults; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;
            analysisResults.AudioDuration = recordingTimeSpan;

            DataTable dataTable = null;

            //if ((predictedEvents != null) && (predictedEvents.Count != 0))
            if (predictedEvents != null)
            {
                string analysisName = analysisSettings.ConfigDict[AudioAnalysisTools.Keys.ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = Keys.EVENT_START_ABS + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }
            else
                analysisResults.EventsFile = null;

            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.01;
                if (analysisSettings.ConfigDict.ContainsKey(Keys.INTENSITY_THRESHOLD))
                    scoreThreshold = ConfigDictionary.GetDouble(Keys.INTENSITY_THRESHOLD, analysisSettings.ConfigDict);
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }
            else
                analysisResults.IndicesFile = null;

            //save image of sonograms
            if ((sonogram != null) && (analysisSettings.ImageFile != null))
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = SonogramTools.Sonogram2Image(sonogram, analysisSettings.ConfigDict, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }
            else
                analysisResults.ImageFile = null;

            analysisResults.Data = dataTable;
            analysisResults.ImageFile = analysisSettings.ImageFile;
            analysisResults.AudioDuration = recordingTimeSpan;
            //result.DisplayItems = { { 0, "example" }, { 1, "example 2" }, }
            //result.OutputFiles = { { "exmaple file key", new FileInfo("Where's that file?") } }
            return analysisResults;
        } //Analyse()




        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        public static Tuple<BaseSonogram, double[,], List<Plot>, List<AcousticEvent>, TimeSpan> Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            //set default values - ignore those set by user
            int frameSize = 1024;
            double windowOverlap = 0.0;

            int minHz = int.Parse(configDict[Keys.MIN_HZ]);
            double intensityThreshold = double.Parse(configDict[Keys.INTENSITY_THRESHOLD]); //in 0-1
            //double minDuration = Double.Parse(configDict[Keys.MIN_DURATION]);  // seconds
            //double maxDuration = Double.Parse(configDict[Keys.MAX_DURATION]);  // seconds
            double minDuration = 0.0;
            double maxDuration = 0.0;

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                LoggedConsole.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }
            TimeSpan tsRecordingDuration = recording.Duration();
            double minRecordingDuration = 15;
            if (tsRecordingDuration.TotalSeconds < minRecordingDuration)
            {
                LoggedConsole.WriteLine("Audio recording must be at least {0} seconds long for analysis.", minRecordingDuration);
                return null;
            }

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = freqBinWidth;



            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int numberOfBins = 64;
            int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            int maxbin = minBin + numberOfBins - 1;
            int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            recording.Dispose();
            double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;



            //######################################################################
            //ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
            var results = System.Tuple.Create(new double[100], new double[100]);
            double[] scoreArray1 = results.Item1;
            double[] scoreArray2 = results.Item2;
            //######################################################################

            var plots = new List<Plot>();
            plots.Add(new Plot("title", scoreArray1, 0.2)); //or get the analysis to pass back plots
            var hits = new double[rowCount, colCount];

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scoreArray1, minHz, maxHz, sonogram.FramesPerSecond, freqBinWidth,
                                                                                         intensityThreshold, minDuration, maxDuration);
            return System.Tuple.Create(sonogram, hits, plots, predictedEvents, tsRecordingDuration);
        } //Analysis()



        //static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, List<Plot> scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        //{
        //    bool doHighlightSubband = false; bool add1kHzLines = true;
        //    int maxFreq = sonogram.NyquistFrequency / 2;
        //    Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

        //    //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
        //    //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

        //    //Image_MultiTrack image = new Image_MultiTrack(img);
        //    image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
        //    image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
        //    //if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
        //    if (scores != null)
        //    {
        //        foreach (Plot plot in scores)
        //            image.AddTrack(Image_Track.GetNamedScoreTrack(plot.data, 0.0, 1.0, plot.threshold, plot.title)); //assumes data normalised in 0,1
        //    }
        //    if (hits != null) image.OverlayRainbowTransparency(hits);
        //    if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount, sonogram.FramesPerSecond);
        //    return image.GetImage();
        //} //DrawSonogram()


        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { AudioAnalysisTools.Keys.EVENT_COUNT,
                                 AudioAnalysisTools.Keys.EVENT_START_MIN,
                                 AudioAnalysisTools.Keys.EVENT_START_SEC, 
                                 AudioAnalysisTools.Keys.EVENT_START_ABS,
                                 AudioAnalysisTools.Keys.SEGMENT_TIMESPAN,
                                 AudioAnalysisTools.Keys.EVENT_DURATION, 
                                 AudioAnalysisTools.Keys.EVENT_INTENSITY,
                                 AudioAnalysisTools.Keys.EVENT_NAME,
                                 AudioAnalysisTools.Keys.EVENT_SCORE,
                                 AudioAnalysisTools.Keys.EVENT_NORMSCORE 

                               };
            //                   1                2               3              4                5              6               7              8
            Type[] types = { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(string), 
                             typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[AudioAnalysisTools.Keys.EVENT_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[AudioAnalysisTools.Keys.EVENT_DURATION] = (double)ev.Duration;   //duratio in seconds
                row[AudioAnalysisTools.Keys.EVENT_INTENSITY] = (double)ev.kiwi_intensityScore;   //
                row[AudioAnalysisTools.Keys.EVENT_NAME] = (string)ev.Name;   //
                row[AudioAnalysisTools.Keys.EVENT_NORMSCORE] = (double)ev.ScoreNormalised;
                row[AudioAnalysisTools.Keys.EVENT_SCORE] = (double)ev.Score;      //Score
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }




        /// <summary>
        /// Converts a DataTable of events to a datatable where one row = one minute of indices
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan sourceDuration, double scoreThreshold)
        {
            if (dt == null) return null;

            if ((sourceDuration == null) || (sourceDuration == TimeSpan.Zero)) return null;
            double units = sourceDuration.TotalSeconds / unitTime.TotalSeconds;
            int unitCount = (int)(units / 1);   //get whole minutes
            if (units % 1 > 0.0) unitCount += 1; //add fractional minute
            int[] eventsPerUnitTime = new int[unitCount]; //to store event counts
            int[] bigEvsPerUnitTime = new int[unitCount]; //to store counts of high scoring events

            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[AudioAnalysisTools.Keys.EVENT_START_SEC];
                double eventScore = (double)ev[AudioAnalysisTools.Keys.EVENT_NORMSCORE];
                int timeUnit = (int)(eventStart / unitTime.TotalSeconds);
                eventsPerUnitTime[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerUnitTime[timeUnit]++;
            }

            string[] headers = { AudioAnalysisTools.Keys.START_MIN, AudioAnalysisTools.Keys.EVENT_TOTAL, ("#Ev>" + scoreThreshold) };
            Type[] types = { typeof(int), typeof(int), typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < eventsPerUnitTime.Length; i++)
            {
                int unitID = (int)(i * unitTime.TotalMinutes);
                newtable.Rows.Add(unitID, eventsPerUnitTime[i], bigEvsPerUnitTime[i]);
            }
            return newtable;
        }

        public static void AddContext2Table(DataTable dt, TimeSpan segmentStartMinute, TimeSpan recordingTimeSpan)
        {
            if (dt == null) return;

            if (!dt.Columns.Contains(Keys.SEGMENT_TIMESPAN)) dt.Columns.Add(AudioAnalysisTools.Keys.SEGMENT_TIMESPAN, typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_ABS)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_ABS, typeof(double));
            if (!dt.Columns.Contains(Keys.EVENT_START_MIN)) dt.Columns.Add(AudioAnalysisTools.Keys.EVENT_START_MIN, typeof(double));
            double start = segmentStartMinute.TotalSeconds;
            foreach (DataRow row in dt.Rows)
            {
                row[AudioAnalysisTools.Keys.SEGMENT_TIMESPAN] = recordingTimeSpan.TotalSeconds;
                row[AudioAnalysisTools.Keys.EVENT_START_ABS] = start + (double)row[AudioAnalysisTools.Keys.EVENT_START_SEC];
                row[AudioAnalysisTools.Keys.EVENT_START_MIN] = start;
            }
        } //AddContext2Table()


        public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
        {
            return DisplayIndices.ProcessCsvFile(fiCsvFile, fiConfigFile);
        }



        /// <summary>
        /// takes a data table of indices and normalises column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        //public static DataTable NormaliseColumnsOfDataTable(DataTable dt)
        //{
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    string[] newHeaders = new string[headers.Length];

        //    List<double[]> newColumns = new List<double[]>();

        //    for (int i = 0; i < headers.Length; i++)
        //    {
        //        double[] values = DataTableTools.Column2ArrayOfDouble(dt, headers[i]); //get list of values
        //        if ((values == null) || (values.Length == 0)) continue;

        //        double min = 0;
        //        double max = 1;
        //        if (headers[i].Equals(Keys.AV_AMPLITUDE))
        //        {
        //            min = -50;
        //            max = -5;
        //            newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
        //            newHeaders[i] = headers[i] + "  (-50..-5dB)";
        //        }
        //        else //default is to normalise in [0,1]
        //        {
        //            newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
        //            newHeaders[i] = headers[i];
        //        }
        //    } //for loop

        //    //convert type int to type double due to normalisation
        //    Type[] types = new Type[newHeaders.Length];
        //    for (int i = 0; i < newHeaders.Length; i++) types[i] = typeof(double);
        //    var processedtable = DataTableTools.CreateTable(newHeaders, types, newColumns);
        //    return processedtable;
        //}


        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }


        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
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
         public static Tuple<AnalysisSettings, TimeSpan, TimeSpan> GetAndCheckAllArguments(string[] args)
        {
            // check numbre of command line arguments
            if (args.Length < 4)
            {
                LoggedConsole.WriteLine("\nINCORRECT COMMAND LINE.");
                LoggedConsole.WriteLine("You require at least 4 command line arguments after the analysis option.");
                Usage();
                throw new AnalysisOptionInvalidArgumentsException();
            }
            CheckPaths(args); // check paths of first three command line arguments

            FileInfo fiConfig = new FileInfo(args[1]);
            string outputDir  = args[2];
            
            // INIT ANALYSIS SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.SourceFile  = new FileInfo(args[0]);
            analysisSettings.ConfigFile  = fiConfig;
            analysisSettings.AnalysisInstanceOutputDirectory = new DirectoryInfo(outputDir);
            analysisSettings.AudioFile   = null;
            analysisSettings.EventsFile  = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile   = null;
            TimeSpan tsStart    = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);
            var configuration = new ConfigDictionary(fiConfig.FullName);
            analysisSettings.ConfigDict = configuration.GetTable();

            // PROCESS REMAINDER OF THE OPTIONAL COMMAND LINE ARGUMENTS
            for (int i = 3; i < args.Length; i++)
            {
                string[] parts = args[i].Split(':');
                if (parts[0].StartsWith("-tmpwav"))
                {
                    var outputWavPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.AudioFile = new FileInfo(outputWavPath);
                }
                else
                if (parts[0].StartsWith("-events"))
                {
                    string eventsPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.EventsFile = new FileInfo(eventsPath);
                }
                else if (parts[0].StartsWith("-indices"))
                {
                    string indicesPath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.IndicesFile = new FileInfo(indicesPath);
                }
                else if (parts[0].StartsWith("-sgram"))
                {
                    string sonoImagePath = Path.Combine(outputDir, parts[1]);
                    analysisSettings.ImageFile = new FileInfo(sonoImagePath);
                }
                else if (parts[0].StartsWith("-start"))
                {
                    int s = int.Parse(parts[1]);
                    tsStart = new TimeSpan(0, 0, s);
                }
                else if (parts[0].StartsWith("-duration"))
                {
                    int s = int.Parse(parts[1]);
                    tsDuration = new TimeSpan(0, 0, s);
                    if (tsDuration.TotalMinutes > 10)
                    {
                        LoggedConsole.WriteLine("Segment duration cannot exceed 10 minutes.");

                        throw new AnalysisOptionInvalidDurationException();
                    }
                }
            }
            return System.Tuple.Create(analysisSettings, tsStart, tsDuration);
        } // CheckAllArguments()


        /// <summary>
        /// this method checks validity of first three command line arguments.
        /// Assumes that they are paths.
        /// NEED TO REWRITE THIS METHOD AS APPROPRIATE
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            // GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory does not exist: " + diSource.FullName);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                LoggedConsole.WriteLine("Source directory exists: " + diSource.FullName);
                LoggedConsole.WriteLine("\t but the source file does not exist: " + recordingPath);

                throw new AnalysisOptionInvalidPathsException();
            }

            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                LoggedConsole.WriteLine("Config file does not exist: " + fiConfig.FullName);

                throw new AnalysisOptionInvalidPathsException();
            }

            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                bool success = true;
                try
                {
                    LoggedConsole.WriteLine("Output directory does not exist: " + diOP.FullName);
                    LoggedConsole.WriteLine("Creating new output directory:   " + diOP.Name);
                    Directory.CreateDirectory(outputDir);
                    success = Directory.Exists(outputDir);
                }
                catch
                {
                    success = false;
                }

                if (!success)
                {
                    LoggedConsole.WriteLine("Output directory does not exist and unable to create new directory of that name.");

                    throw new AnalysisOptionInvalidPathsException();
                }
            }
        } //CheckPaths()



        /// <summary>
        /// NOTE: EDIT THE "Default" string to describethat indicates analysis type.
        /// </summary>
        public static void Usage()
        {
            LoggedConsole.WriteLine("USAGE:");
            LoggedConsole.WriteLine("AnalysisPrograms.exe  " + ANALYSIS_NAME + "  audioPath  configPath  outputDirectory  startOffset  endOffset");
            LoggedConsole.WriteLine(
            @"
            where:
            ANALYSIS_NAME:-   (string) Identifies the analysis type.
            audioPath:-       (string) Path of the audio file to be processed.
            configPath:-      (string) Path of the analysis configuration file.
            outputDirectory:- (string) Path of the output directory in which to store .csv result files.
            THE ABOVE THREE ARGUMENTS ARE OBLIGATORY. 
            THE NEXT TWO ARGUMENTS ARE OPTIONAL:
            startOffset:      (integer) The start (minutes) of that portion of the file to be analysed.
            duration:         (integer) The duration (in seconds) of that portion of the file to be analysed.
            IF LAST TWO ARGUMENTS ARE NOT INCLUDED, OR DURATION=0, THE ENTIRE FILE IS ANALYSED.
            ");
        }



    } //end class AnalysisTemplate
}
