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
    public class LSKiwi2 : IAnalysis
    {
        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_MIN_HZ_MALE = "MIN_HZ_MALE";
        public static string key_MAX_HZ_MALE = "MAX_HZ_MALE";
        public static string key_MIN_HZ_FEMALE = "MIN_HZ_FEMALE";
        public static string key_MAX_HZ_FEMALE = "MAX_HZ_FEMALE";
        public static string key_DCT_DURATION = "DCT_DURATION";
        public static string key_DCT_THRESHOLD = "DCT_THRESHOLD";
        public static string key_MIN_PERIODICITY = "MIN_PERIOD";
        public static string key_MAX_PERIODICITY = "MAX_PERIOD";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";

        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        //public static string key_CALL_DURATION = "CALL_DURATION";
        //public static string key_DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        //public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        //public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        //public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        //public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        //public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        //public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        //public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        //public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        //public static string key_MIN_HZ = "MIN_HZ";
        //public static string key_MAX_HZ = "MAX_HZ";
        //public static string key_MIN_GAP = "MIN_GAP";
        //public static string key_MAX_GAP = "MAX_GAP";
        //public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        //public static string key_MIN_DURATION = "MIN_DURATION";
        //public static string key_MAX_DURATION = "MAX_DURATION";
        //public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        //KEYS TO OUTPUT EVENTS and INDICES
        public static string key_COUNT     = "count";
        public static string key_SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string key_START_ABS = "EvStartAbs";
        public static string key_START_MIN = "EvStartMin";
        public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE = "CallScore";
        public static string key_EVENT_TOTAL= "# events";


        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "LSKiwi2";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Little Spotted Kiwi"; }
        }

        public string Identifier
        {
            get { return "Towsey." + ANALYSIS_NAME; }
        }


        public static void Dev(string[] args)
        {
            //Following lines are used for the debug command line.
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-female\TOWER_20091107_07200_21.LSK.F.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\Samples\lsk-male\TOWER_20091112_072000_25.LSK.M.wav"  "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_Cropped.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Samples\lskiwi_Params.txt"
            // 8 min test recording  // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004_CroppedAnd2.wav" "C:\SensorNetworks\WavFiles\Kiwi\Results_MixedTest\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav"   "C:\SensorNetworks\WavFiles\Kiwi\Results\lskiwi_Params.txt"
            // kiwi "C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav"     "C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\lskiwi_Params.txt"

            string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TOWER_20100208_204500.wav";
            string outputDir     = @"C:\SensorNetworks\Output\LSKiwi2\Tower_20100208_204500\";
            string configPath    = @"C:\SensorNetworks\Output\LSKiwi2\Tower_20100208_204500\lskiwi2.cfg";
            string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\TOWER_20100208_204500_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\KAPITI2_20100219_202900.wav";
            //string outputDir     = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\";
            //string configPath    = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\lskiwi_Params.txt";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_KAPITI2_20100219_202900\KAPITI2_20100219_202900_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_220004.wav";
            //string outputDir     = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\";
            //string configPath    = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\lskiwi_Params.txt";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_220004\TUITCE_20091215_220004_ANDREWS_SELECTIONS.csv";

            //string recordingPath = @"C:\SensorNetworks\WavFiles\Kiwi\TUITCE_20091215_210000.wav";
            //string outputDir     = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\";
            //string configPath    = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\lskiwi_Params.txt";
            //string ANDREWS_SELECTION_PATH = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TUITCE_20091215_210000\TUITCE_20091215_210000_ANDREWS_SELECTIONS.csv";

            string title = "# FOR DETECTION OF THE LITTLE SPOTTED KIWI - version 2 - started June 2012";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 40;
            int durationSeconds = 300; //set zero to get entire recording
            var tsStart = new TimeSpan(0, startMinute, 0); //hours, minutes, seconds
            var tsDuration = new TimeSpan(0, 0, durationSeconds); //hours, minutes, seconds
            var segmentFileStem = Path.GetFileNameWithoutExtension(recordingPath);
            var segmentFName = string.Format("{0}_converted.wav", segmentFileStem);
            var sonogramFname = string.Format("{0}_{1}min.png", segmentFileStem, startMinute);
            var eventsFname = string.Format("{0}_Events{1}min.csv", segmentFileStem, startMinute);
            var indicesFname = string.Format("{0}_Indices{1}min.csv", segmentFileStem, startMinute);

            var cmdLineArgs = new List<string>();
            cmdLineArgs.Add(recordingPath);
            cmdLineArgs.Add(configPath);
            cmdLineArgs.Add(outputDir);
            cmdLineArgs.Add("-tmpwav:" + segmentFName);
            cmdLineArgs.Add("-events:" + eventsFname);
            cmdLineArgs.Add("-indices:" + indicesFname);
            cmdLineArgs.Add("-sgram:" + sonogramFname);
            cmdLineArgs.Add("-start:" + tsStart.TotalSeconds);
            cmdLineArgs.Add("-duration:" + tsDuration.TotalSeconds);

            //#############################################################################################################################################
            int status = Execute(cmdLineArgs.ToArray());
            if (status != 0)
            {
                Console.WriteLine("\n\n# FATAL ERROR. CANNOT PROCEED!");
                Console.ReadLine();
                System.Environment.Exit(99);
            }
            //#############################################################################################################################################

            string eventsPath = Path.Combine(outputDir, eventsFname);
            FileInfo fiCsvEvents = new FileInfo(eventsPath);
            if (!fiCsvEvents.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Events CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(eventsPath, true);
                DataTableTools.WriteTable(dt);
            }
            string indicesPath = Path.Combine(outputDir, indicesFname);
            FileInfo fiCsvIndices = new FileInfo(indicesPath);
            if (!fiCsvIndices.Exists)
            {
                Log.WriteLine("\n\n\n############\n WARNING! Indices CSV file not returned from analysis of minute {0} of file <{0}>.", startMinute, recordingPath);
            }
            else
            {
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable(dt);
            }
            string imagePath = Path.Combine(outputDir, sonogramFname);
            FileInfo fiImage = new FileInfo(imagePath);
            if (fiImage.Exists)
            {
                ProcessRunner process = new ProcessRunner(imageViewer);
                process.Run(imagePath, outputDir);
            }

            Console.WriteLine("\n\n# Finished analysis:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()




        /// <summary>
        /// A WRAPPER AROUND THE analyser.Analyse(analysisSettings) METHOD
        /// To be called as an executable with command line arguments.
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="configPath"></param>
        /// <param name="outputPath"></param>
        public static int Execute(string[] args)
        {
            int status = 0;
            if (args.Length < 4)
            {
                Console.WriteLine("Require at least 4 command line arguments.");
                status = 1;
                return status;
            }
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + recordingPath);
                status = 2;
                return status;
            }

            //INIT SETTINGS
            AnalysisSettings analysisSettings = new AnalysisSettings();
            analysisSettings.ConfigFile = fiConfig;
            analysisSettings.AnalysisRunDirectory = diOP;
            analysisSettings.AudioFile = null;
            analysisSettings.EventsFile = null;
            analysisSettings.IndicesFile = null;
            analysisSettings.ImageFile = null;
            TimeSpan tsStart = new TimeSpan(0, 0, 0);
            TimeSpan tsDuration = new TimeSpan(0, 0, 0);

            //PROCESS REMAINDER OF THE OPTIONAL COMMAND LINE ARGUMENTS
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
                    else
                        if (parts[0].StartsWith("-indices"))
                        {
                            string indicesPath = Path.Combine(outputDir, parts[1]);
                            analysisSettings.IndicesFile = new FileInfo(indicesPath);
                        }
                        else
                            if (parts[0].StartsWith("-sgram"))
                            {
                                string sonoImagePath = Path.Combine(outputDir, parts[1]);
                                analysisSettings.ImageFile = new FileInfo(sonoImagePath);
                            }
                            else
                                if (parts[0].StartsWith("-start"))
                                {
                                    int s = Int32.Parse(parts[1]);
                                    tsStart = new TimeSpan(0, 0, s);
                                }
                                else
                                    if (parts[0].StartsWith("-duration"))
                                    {
                                        int s = Int32.Parse(parts[1]);
                                        tsDuration = new TimeSpan(0, 0, s);
                                        if (tsDuration.TotalMinutes > 10)
                                        {
                                            Console.WriteLine("Segment duration cannot exceed 10 minutes.");
                                            status = 3;
                                            return status;
                                        }
                                    }
            }

            //EXTRACT THE REQUIRED RECORDING SEGMENT
            FileInfo tempF = analysisSettings.AudioFile;
            if (tsDuration.TotalSeconds == 0)   //Process entire file
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, RESAMPLE_RATE);
                //var fiSegment = AudioFilePreparer.PrepareFile(diOutputDir, fiSourceFile, , Human2.RESAMPLE_RATE);
            }
            else
            {
                AudioFilePreparer.PrepareFile(fiSource, tempF, RESAMPLE_RATE, tsStart, tsStart.Add(tsDuration));
                //var fiSegmentOfSourceFile = AudioFilePreparer.PrepareFile(diOutputDir, new FileInfo(recordingPath), MediaTypes.MediaTypeWav, TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(3), RESAMPLE_RATE);
            }

            //DO THE ANALYSIS
            //#############################################################################################################################################
            IAnalysis analyser = new LSKiwi2();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                DataTable augmentedTable = AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(augmentedTable, analysisSettings.EventsFile.FullName);
                //DataTableTools.WriteTable(augmentedTable);
            }

            return status;
        }



        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var configuration = new Configuration(analysisSettings.ConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            //######################################################################
            var results = Analysis(fiAudioF, configDict);
            //######################################################################

            if (results == null) return analysisResults; //nothing to process 
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;
            analysisResults.AudioDuration = recordingTimeSpan;

            DataTable dataTable = null;

            if ((predictedEvents != null) && (predictedEvents.Count != 0))
            {
                string analysisName = configDict[key_ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(predictedEvents);
                string sortString = key_START_SEC + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }

            if ((analysisSettings.EventsFile != null) && (dataTable != null))
            {
                CsvTools.DataTable2CSV(dataTable, analysisSettings.EventsFile.FullName);
            }

            if ((analysisSettings.IndicesFile != null) && (dataTable != null))
            {
                double scoreThreshold = 0.1;
                TimeSpan unitTime = TimeSpan.FromSeconds(60); //index for each time span of i minute
                var indicesDT = ConvertEvents2Indices(dataTable, unitTime, recordingTimeSpan, scoreThreshold);
                CsvTools.DataTable2CSV(indicesDT, analysisSettings.IndicesFile.FullName);
            }

            //save image of sonograms
            if (analysisSettings.ImageFile != null)
            {
                string imagePath = analysisSettings.ImageFile.FullName;
                double eventThreshold = 0.1;
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }

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
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan>
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> config)
        {
            int minHzMale = Configuration.GetInt(LSKiwi.key_MIN_HZ_MALE, config);
            int maxHzMale = Configuration.GetInt(LSKiwi.key_MAX_HZ_MALE, config);
            int minHzFemale = Configuration.GetInt(LSKiwi.key_MIN_HZ_FEMALE, config);
            int maxHzFemale = Configuration.GetInt(LSKiwi.key_MAX_HZ_FEMALE, config);
            int frameLength = Configuration.GetInt(LSKiwi.key_FRAME_LENGTH, config);
            double frameOverlap = Configuration.GetDouble(LSKiwi.key_FRAME_OVERLAP, config);
            double dctDuration = Configuration.GetDouble(LSKiwi.key_DCT_DURATION, config);
            double dctThreshold = Configuration.GetDouble(LSKiwi.key_DCT_THRESHOLD, config);
            double minPeriod = Configuration.GetDouble(LSKiwi.key_MIN_PERIODICITY, config);
            double maxPeriod = Configuration.GetDouble(LSKiwi.key_MAX_PERIODICITY, config);
            double eventThreshold = Configuration.GetDouble(LSKiwi.key_EVENT_THRESHOLD, config);
            double minDuration = Configuration.GetDouble(LSKiwi.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = Configuration.GetDouble(LSKiwi.key_MAX_DURATION, config); //maximum event duration to qualify as species call
            //double segmentDuration = Configuration.GetDouble(LSKiwi.key_SEGMENT_DURATION, config);
            //double segmentStartMinute = segmentDuration * iter;

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                Console.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }
            TimeSpan tsRecordingtDuration = recording.Duration();

            //i: MAKE SONOGRAM
            //Log.WriteLine("Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD; //DO NOT DO NOISE REMOVAL BECAUSE CAN LOSE SOME KIWI INFO
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            

            //var results = LSKiwi.Execute_KiwiDetect(recording, minHzMale, maxHzMale, minHzFemale, maxHzFemale, frameLength, frameOverlap, dctDuration, dctThreshold,
            //                                        minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);

            var results = DetectKiwi(sonogram, minHzMale, maxHzMale, dctDuration, dctThreshold,  minPeriod, maxPeriod, eventThreshold, minDuration, maxDuration);
            var intensity = results.Item1;  //lowerArray, intensity, hits, events
            var scores    = results.Item2;
            var hits      = results.Item3;
            var predictedEvents = results.Item4; 


            //i: MAKE SONOGRAM
            //SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            //sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = frameSize;
            //sonoConfig.WindowOverlap = windowOverlap;
            ////sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            //int sr = recording.SampleRate;
            //double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            //double framesPerSecond = freqBinWidth;



            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            //int numberOfBins = 64;
            //int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
            //int maxbin = minBin + numberOfBins - 1;
            //int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));

            //BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //int rowCount = sonogram.Data.GetLength(0);
            //int colCount = sonogram.Data.GetLength(1);
            //recording.Dispose();
            //double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

            //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;


            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            //List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scoreArray1, minHz, maxHz, sonogram.FramesPerSecond, freqBinWidth,
            //                                                                             intensityThreshold, minDuration, maxDuration);
            return System.Tuple.Create(sonogram, hits, scores, predictedEvents, tsRecordingtDuration);
        } //Analysis()

        public static System.Tuple<double[], double[], double[,], List<AcousticEvent>> DetectKiwi(BaseSonogram sonogram, int minHz, int maxHz, 
                                    double dctDuration, double dctThreshold, double minPeriod, double maxPeriod, double eventThreshold, double minDuration, double maxDuration)
        {
            int rowCount = sonogram.Data.GetLength(0);
            int colCount = sonogram.Data.GetLength(1);
            double minFramePeriod = minPeriod * sonogram.FramesPerSecond;
            double maxFramePeriod = maxPeriod * sonogram.FramesPerSecond;

            int minBin = (int)(minHz / sonogram.FBinWidth);
            //int maxBin = (int)(maxHz / sonogram.FBinWidth);
            double[] fullArray  = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin, (rowCount - 1), minBin + 30);

            double[] lowerArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin,    (rowCount - 1), minBin+10);
            double[] upperArray = MatrixTools.GetRowAveragesOfSubmatrix(sonogram.Data, 0, minBin+21, (rowCount - 1), minBin+30);
            //int maxBin = (int)(maxHz / sonogram.FBinWidth);


            //#############################################################################################################################################
            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz
            int step = (int)Math.Round(sonogram.FramesPerSecond); //take one second steps
            int sampleLength = 32; //64 frames = 3.7 seconds. Suitable for Lewins Rail.
            var result = CrossCorrelation.DetectXcorrelationInTwoArrays(lowerArray, upperArray, step, sampleLength, minFramePeriod, maxFramePeriod);
            double[] intensity   = result.Item1;
            double[] periodicity = result.Item2;

            //iii: CONVERT SCORES TO ACOUSTIC EVENTS
            intensity = DataTools.filterMovingAverage(intensity, 5);
            List<AcousticEvent> events = AcousticEvent.ConvertScoreArray2Events(intensity, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                                         eventThreshold, minDuration, maxDuration);
            CropEvents(events, lowerArray);
            var hits = new double[rowCount, colCount];

            return System.Tuple.Create(intensity, fullArray, hits, events);
        }

        public static void CropEvents(List<AcousticEvent> events, double[] intensity)
        {
            double severity = 0.2;
            int length = intensity.Length;

            foreach (AcousticEvent ev in events)
            {
                int start = ev.oblong.r1;
                int end = ev.oblong.r2;
                double[] subArray = DataTools.Subarray(intensity, start, end - start + 1);
                //DataTools.writeArray(subArray, "f2");
                //DataTools.writeBarGraph(subArray);
                int maxID = DataTools.GetMaxIndex(subArray);
                if (subArray[maxID] < 3.0) //< 5dB
                {
                    ev.oblong = null;
                    continue;
                }
                int[] bounds = DataTools.Peaks_CropLowAmplitude(subArray, severity);

                int newMinRow = start + bounds[0];
                int newMaxRow = start + bounds[1];
                if (newMaxRow >= length) newMaxRow = length - 1;

                Oblong o = new Oblong(newMinRow, ev.oblong.c1, newMaxRow, ev.oblong.c2);
                ev.oblong = o;
                ev.TimeStart = newMinRow * ev.FrameOffset;
                ev.TimeEnd = newMaxRow * ev.FrameOffset;
            }
        }


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int maxFreq = sonogram.NyquistFrequency / 2;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(maxFreq, 1, doHighlightSubband, add1kHzLines));

            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 0.5, eventThreshold));
            //if (hits != null) image.OverlayRedTransparency(hits);
            if (hits != null) image.OverlayRainbowTransparency(hits);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount);
            return image.GetImage();
        } //DrawSonogram()




        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { key_START_SEC,  key_CALL_SCORE };
            Type[] types     = { typeof(double), typeof(double) };

            var dataTable = DataTableTools.CreateTable(headers, types);
            if (predictedEvents.Count == 0) return dataTable;

            foreach (var ev in predictedEvents)
            {
                DataRow row = dataTable.NewRow();
                row[key_START_SEC] = (double)ev.TimeStart;  //EvStartSec
                row[key_CALL_SCORE] = (double)ev.Score;     //Score
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }



        /// <summary>
        /// Converts a DataTable of events to a datatable where one row = one minute of indices
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
        {
            double units = timeDuration.TotalSeconds / unitTime.TotalSeconds;
            int unitCount = (int)(units / 1);
            if(units % 1 > 0.0) unitCount += 1; 
            int[] eventsPerMinute = new int[unitCount]; //to store event counts
            int[] bigEvsPerMinute = new int[unitCount]; //to store counts of high scoring events

            foreach (DataRow ev in dt.Rows)
            {
                double eventStart = (double)ev[key_START_SEC];
                double eventScore = (double)ev[key_CALL_SCORE]; 
                int timeUnit = (int)(eventStart / timeDuration.TotalSeconds);
                eventsPerMinute[timeUnit]++;
                if (eventScore > scoreThreshold) bigEvsPerMinute[timeUnit]++;
            }

            string[] headers = { key_START_MIN, key_EVENT_TOTAL, ("#Ev>" + scoreThreshold) };
            Type[]   types   = { typeof(int),   typeof(int), typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < eventsPerMinute.Length; i++)
            {
                newtable.Rows.Add(i, eventsPerMinute[i], bigEvsPerMinute[i]);
            }
            return newtable;
        }



     public static DataTable AddContext2Table(DataTable dt, TimeSpan segmentStartMinute, TimeSpan recordingTimeSpan)
     {
         string[] headers = DataTableTools.GetColumnNames(dt);
         Type[] types     = DataTableTools.GetColumnTypes(dt);

         //set up a new augmented table with more headers and types
         List<string> newHeaders = new List<string>();
         List<Type>   newTypes   = new List<Type>();

         newHeaders.Add(key_SEGMENT_TIMESPAN);
         newHeaders.Add(key_START_ABS);
         newHeaders.Add(key_START_MIN);
         newTypes.Add(typeof(double));
         newTypes.Add(typeof(double));
         newTypes.Add(typeof(double));
         for (int i = 0; i < headers.Length; i++)
         {
             newHeaders.Add(headers[i]);
             newTypes.Add(types[i]);
         }

         double start = segmentStartMinute.TotalSeconds;
         DataTable augmentedTable = DataTableTools.CreateTable(newHeaders.ToArray(), newTypes.ToArray());
         foreach (DataRow row in dt.Rows)
         {
             DataRow newRow = augmentedTable.NewRow();
             newRow[key_SEGMENT_TIMESPAN] = recordingTimeSpan.TotalSeconds;
             newRow[key_START_ABS]        = start + (double)row[key_START_SEC];
             newRow[key_START_MIN]        = start;
             for (int i = 0; i < row.ItemArray.Length; i++)
             {
                 newRow[headers[i]] = (double)row.ItemArray[i];
             }
             augmentedTable.Rows.Add(newRow);
         }

         return augmentedTable;
     }



        public AnalysisSettings DefaultSettings
        {
            get
            {
                return new AnalysisSettings
                {
                    SegmentMaxDuration = TimeSpan.FromMinutes(1),
                    SegmentMinDuration = TimeSpan.FromSeconds(30),
                    SegmentMediaType   = MediaTypes.MediaTypeWav,
                    SegmentOverlapDuration  = TimeSpan.Zero,
                    SegmentTargetSampleRate = AnalysisTemplate.RESAMPLE_RATE
                };
            }
        }


        public string DefaultConfiguration
        {
            get
            {
                return string.Empty;
            }
        }

    } //end class LSKiwi2
}
