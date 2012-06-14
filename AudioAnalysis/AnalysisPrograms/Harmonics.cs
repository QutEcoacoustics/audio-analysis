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
    public class Harmonics : IAnalysis
    {
        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string key_CALL_DURATION = "CALL_DURATION";
        public static string key_DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_MIN_GAP = "MIN_GAP";
        public static string key_MAX_GAP = "MAX_GAP";
        public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

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
        public const string ANALYSIS_NAME = "Harmonics";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Harmonics-general"; }
        }

        public string Identifier
        {
            get { return "Towsey." + ANALYSIS_NAME; }
        }


        public static void Dev(string[] args)
        {
            //HUMAN
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\PramukSpeech_20090615.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Wimmer_DM420011.wav";         
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\BAC2_20071018-143516_speech.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Human\Planitz.wav";
            //MACHINES
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min173Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min449Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min700Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\KAPITI2-20100219-202900_Airplane.mp3";
            //CROW
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";

            string configPath = @"C:\SensorNetworks\Output\Harmonics\Harmonics.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\Harmonics\";

            string title = "# FOR DETECTION OF HUMAN, CROW AMD MACHINE HARMONICS ising Xcorrelation and FFT";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            var diOutputDir = new DirectoryInfo(outputDir);

            Log.Verbosity = 1;
            int startMinute = 0;
            int durationSeconds = 0; //set zero to get entire recording
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
                Console.WriteLine("\n");
                DataTable dt = CsvTools.ReadCSVToTable(indicesPath, true);
                DataTableTools.WriteTable2Console(dt);
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
            IAnalysis analyser = new Harmonics();
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
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF    = analysisSettings.AudioFile;
            var diOutputDir = analysisSettings.AnalysisRunDirectory;

            var analysisResults = new AnalysisResult();
            analysisResults.AnalysisIdentifier = this.Identifier;
            analysisResults.SettingsUsed = analysisSettings;
            analysisResults.Data = null;

            Dictionary<string, string> newDict;
            BaseSonogram sonogram = null;
            DataTable dataTable = null;
            var events = new List<AcousticEvent>();
            double[,] hits = null;
            var recordingTimeSpan = new TimeSpan();
            var scores = new List<double[]>();

            //HUMAN
            //######################################################################
            newDict = new Dictionary<string, string>();
            string filter = "HUMAN";
            var keysFiltered= DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)
            {
                string newKey = key.Substring(6);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(key_ANALYSIS_NAME, Human2.ANALYSIS_NAME);
            var results1 = Human2.Analysis(fiAudioF, newDict);
            if (results1 != null)
            {
                sonogram = results1.Item1;
                hits = results1.Item2;
                scores.Add(results1.Item3);
                foreach (AcousticEvent ae in results1.Item4)
                {
                    ae.Name = Human2.ANALYSIS_NAME;
                    events.Add(ae);
                }
                recordingTimeSpan = results1.Item5;
            }
            //######################################################################
            //CROW
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "CROW";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)
            {
                string newKey = key.Substring(5);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(key_ANALYSIS_NAME, Crow.ANALYSIS_NAME);
            var results2 = Crow.Analysis(fiAudioF, newDict);
            if (results2 != null)
            {
                if (sonogram == null) sonogram = results2.Item1;
                hits = MatrixTools.AddMatrices(hits, results2.Item2);
                scores.Add(results2.Item3);
                foreach (AcousticEvent ae in results2.Item4)
                {
                    ae.Name = Crow.ANALYSIS_NAME;
                    events.Add(ae);
                }
                recordingTimeSpan = results2.Item5;
            }
            //######################################################################
            //MACHINES
            //######################################################################
            newDict = new Dictionary<string, string>();
            filter = "MACHINE";
            keysFiltered = DictionaryTools.FilterKeysInDictionary(configDict, filter);

            foreach (string key in keysFiltered)
            {
                string newKey = key.Substring(8);
                newDict.Add(newKey, configDict[key]);
            }
            newDict.Add(key_ANALYSIS_NAME, PlanesTrainsAndAutomobiles.ANALYSIS_NAME);
            var results3 = PlanesTrainsAndAutomobiles.Analysis(fiAudioF, newDict);
            if (results3 != null)
            {
                if (sonogram == null) sonogram = results3.Item1;
                hits = MatrixTools.AddMatrices(hits, results3.Item2);
                scores.Add(results3.Item3);
                foreach (AcousticEvent ae in results3.Item4)
                {
                    ae.Name = PlanesTrainsAndAutomobiles.ANALYSIS_NAME;
                    events.Add(ae);
                }
                recordingTimeSpan = results3.Item5;
            }
            //######################################################################


            if ((events != null) && (events.Count != 0))
            {
                string analysisName = configDict[key_ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiAudioF.Name);
                foreach (AcousticEvent ev in events)
                {
                    ev.SourceFileName = fName;
                    //ev.Name = analysisName;
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(events);
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
                Image image = DrawSonogram(sonogram, hits, scores[0], events, eventThreshold);
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
        /// A WRAPPER AROUND THE Execute_HarmonicDetection() method
        /// Returns a DataTable
        /// The Execute_HDDetect() method returns a System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>>
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        //public static DataTable AnalysisReturnsDataTable(int iter, FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir)
        //{
        //    string analysisName = configDict[key_ANALYSIS_NAME];
        //    double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
        //    int DRAW_SONOGRAMS = Int32.Parse(configDict[key_DRAW_SONOGRAMS]);         // options to draw sonogram

        //    var acousticEvents = new List<AcousticEvent>();

        //    string opFileName = "temp.wav";
        //    AudioRecording recording = AudioRecording.GetAudioRecording(fiSegmentOfSourceFile, Harmonics.RESAMPLE_RATE, diOutputDir.FullName, opFileName);
        //    TimeSpan tsSegmentDuration = recording.Duration();
        //    Console.WriteLine("\tRecording Duration: {0:f2}seconds", tsSegmentDuration.TotalSeconds);

        //    //CROWS #############################################################################################################################################
        //    //key_CROW_FRAME_LENGTH = "CROW_FRAME_LENGTH";
        //    //key_CROW_MIN_HZ = "CROW_MIN_HZ";
        //    //key_CROW_CALL_DURATION = "CROW_CALL_DURATION";
        //    //key_CROW_DECIBEL_THRESHOLD = "CROW_DECIBEL_THRESHOLD";
        //    //key_CROW_HARMONIC_INTENSITY_THRESHOLD = "CROW_HARMONIC_INTENSITY_THRESHOLD";
        //    //key_CROW_MIN_FORMANT_GAP = "CROW_MIN_FORMANT_GAP";
        //    //key_CROW_MAX_FORMANT_GAP = "CROW_MAX_FORMANT_GAP";
        //    //int frameLength = Int32.Parse(configDict[key_CROW_FRAME_LENGTH]);
        //    int minHz = Int32.Parse(configDict[key_CROW_MIN_HZ]);
        //    int minFormantgap = Int32.Parse(configDict[key_CROW_MIN_FORMANT_GAP]);
        //    int maxFormantgap = Int32.Parse(configDict[key_CROW_MAX_FORMANT_GAP]);
        //    double decibelThreshold = Double.Parse(configDict[key_CROW_DECIBEL_THRESHOLD]);  //dB
        //    double harmonicIntensityThreshold = Double.Parse(configDict[key_CROW_HARMONIC_INTENSITY_THRESHOLD]); //in 0-1
        //    double callDuration = Double.Parse(configDict[key_CROW_CALL_DURATION]);  // seconds

        //    var results1 = Crow2.Execute_HarmonicDetection(recording, minHz, decibelThreshold, harmonicIntensityThreshold, minFormantgap, maxFormantgap, callDuration); //uses XCORR and FFT
        //    var sonogram1 = results1.Item1;
        //    var hits1 = results1.Item2;
        //    var scores1 = results1.Item3;
        //    var predictedEvents1 = results1.Item4;
        //    foreach (AcousticEvent ev in predictedEvents1)
        //    {
        //        ev.SourceFileName = recording.FileName;
        //        ev.Name = Crow2.ANALYSIS_NAME;
        //        acousticEvents.Add(ev);
        //    }
        //    //HUMANS #############################################################################################################################################
        //    //key_HUMAN_FRAME_LENGTH = "HUMAN_FRAME_LENGTH";
        //    //key_HUMAN_MIN_HZ = "HUMAN_MIN_HZ";
        //    //key_HUMAN_MAX_HZ = "HUMAN_MAX_HZ";
        //    //key_HUMAN_MIN_FORMANT_GAP = "HUMAN_MIN_FORMANT_GAP";
        //    //key_HUMAN_MAX_FORMANT_GAP = "HUMAN_MAX_FORMANT_GAP";
        //    //key_HUMAN_MIN_FORMANT_DURATION = "HUMAN_MIN_FORMANT_DURATION";
        //    //key_HUMAN_MAX_FORMANT_DURATION = "HUMAN_MAX_FORMANT_DURATION";
        //    //key_HUMAN_INTENSITY_THRESHOLD = "HUMAN_INTENSITY_THRESHOLD";
        //    //frameLength = Int32.Parse(configDict[key_HUMAN_FRAME_LENGTH]);
        //    minHz = Int32.Parse(configDict[key_HUMAN_MIN_HZ]);
        //    int maxHz = Int32.Parse(configDict[key_HUMAN_MAX_HZ]);
        //    minFormantgap = Int32.Parse(configDict[key_HUMAN_MIN_HZ]);
        //    maxFormantgap = Int32.Parse(configDict[key_HUMAN_MAX_HZ]);
        //    double minDuration = Double.Parse(configDict[key_HUMAN_MIN_FORMANT_DURATION]);
        //    double maxDuration = Double.Parse(configDict[key_HUMAN_MAX_FORMANT_DURATION]);
        //    harmonicIntensityThreshold = Double.Parse(configDict[key_HUMAN_INTENSITY_THRESHOLD]);

        //    var results2 = Human.Execute_HDDetectByXcorrelation(recording, minHz, harmonicIntensityThreshold, minFormantgap, maxFormantgap, minDuration, maxDuration); //uses XCORR and FFT
        //    //var sonogram = results2.Item1;
        //    var hits2 = results2.Item2;
        //    var scores2 = results2.Item3;
        //    var predictedEvents2 = results2.Item4;
        //    foreach (AcousticEvent ev in predictedEvents2)
        //    {
        //        ev.SourceFileName = recording.FileName;
        //        ev.Name = Human.ANALYSIS_NAME;
        //        acousticEvents.Add(ev);
        //    }

        //    //MACHINES ###########################################################################################################################################
        //    //key_MACHINE_MIN_HZ               = "MACHINE_MIN_HZ";
        //    //key_MACHINE_MAX_HZ               = "MACHINE_MAX_HZ";
        //    //key_MACHINE_MIN_FORMANT_GAP      = "MACHINE_MIN_FORMANT_GAP";
        //    //key_MACHINE_MAX_FORMANT_GAP      = "MACHINE_MAX_FORMANT_GAP";
        //    //key_MACHINE_MIN_FORMANT_DURATION = "MACHINE_MIN_FORMANT_DURATION";
        //    //key_MACHINE_INTENSITY_THRESHOLD  = "MACHINE_INTENSITY_THRESHOLD";
        //    minHz = Int32.Parse(configDict[key_MACHINE_MIN_HZ]);
        //    maxHz = Int32.Parse(configDict[key_MACHINE_MAX_HZ]);
        //    minFormantgap = Int32.Parse(configDict[key_MACHINE_MIN_HZ]);
        //    maxFormantgap = Int32.Parse(configDict[key_MACHINE_MAX_HZ]);
        //    harmonicIntensityThreshold = Double.Parse(configDict[key_MACHINE_INTENSITY_THRESHOLD]);
        //    var results3 = PlanesTrainsAndAutomobiles.Execute_HDDetect(recording, harmonicIntensityThreshold, minFormantgap, maxFormantgap, minDuration); //uses XCORR and FFT
        //    //var sonogram = results3.Item1;
        //    var hits3 = results3.Item2;
        //    var scores3 = results3.Item3;
        //    var predictedEvents3 = results3.Item4;
        //    foreach (AcousticEvent ev in predictedEvents3)
        //    {
        //        ev.SourceFileName = recording.FileName;
        //        ev.Name = PlanesTrainsAndAutomobiles.ANALYSIS_NAME;
        //        acousticEvents.Add(ev);
        //    }

        //    //#############################################################################################################################################
        //    recording.Dispose();

        //    double segmentStartMinute = segmentDuration * iter;

        //    //draw images of sonograms
        //    bool saveSonogram = false;
        //    if ((DRAW_SONOGRAMS == 2) || ((DRAW_SONOGRAMS == 1) && (acousticEvents.Count > 0))) saveSonogram = true;
        //    if (saveSonogram)
        //    {
        //        double eventThreshold = 0.1;
        //        string imagePath = Path.Combine(diOutputDir.FullName, Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.FullName) + "_" + (int)segmentStartMinute + "min.png");
        //        Image image = DrawSonogram(sonogram1, hits1, scores1, acousticEvents, eventThreshold);
        //        image.Save(imagePath, ImageFormat.Png);
        //    }

        //    if (acousticEvents.Count == 0) return null;

        //    //write events to a data table to return.
        //    DataTable dataTable = WriteEvents2DataTable(iter, segmentStartMinute, tsSegmentDuration, acousticEvents);

        //    string sortString = "EvStartAbs ASC";
        //    return DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
        //} //Analysis()



        /// <summary>
        /// ################ THE KEY ANALYSIS METHOD
        /// Returns a DataTable
        /// </summary>
        /// <param name="fiSegmentOfSourceFile"></param>
        /// <param name="configDict"></param>
        /// <param name="diOutputDir"></param>
        //public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan>
        //                                                                           Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        //{
        //    //set default values - ignore those set by user
        //    int frameSize = 1024;
        //    double windowOverlap = 0.0;

        //    int minHz = Int32.Parse(configDict[key_MIN_HZ]);
        //    double intensityThreshold = Double.Parse(configDict[key_INTENSITY_THRESHOLD]); //in 0-1
        //    double minDuration = Double.Parse(configDict[key_MIN_DURATION]);  // seconds
        //    double maxDuration = Double.Parse(configDict[key_MAX_DURATION]);  // seconds

        //    AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
        //    if (recording == null)
        //    {
        //        Console.WriteLine("AudioRecording == null. Analysis not possible.");
        //        return null;
        //    }

        //    //i: MAKE SONOGRAM
        //    SonogramConfig sonoConfig = new SonogramConfig(); //default values config
        //    sonoConfig.SourceFName = recording.FileName;
        //    sonoConfig.WindowSize = frameSize;
        //    sonoConfig.WindowOverlap = windowOverlap;
        //    //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
        //    sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
        //    TimeSpan tsRecordingtDuration = recording.Duration();
        //    int sr = recording.SampleRate;
        //    double freqBinWidth = sr / (double)sonoConfig.WindowSize;
        //    double framesPerSecond = freqBinWidth;



        //    //#############################################################################################################################################
        //    //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
        //    // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
        //    // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
        //    // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

        //    //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
        //    //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
        //    //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
        //    int numberOfBins = 64;
        //    int minBin = (int)Math.Round(minHz / freqBinWidth) + 1;
        //    int maxbin = minBin + numberOfBins - 1;
        //    int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));

        //    BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
        //    int rowCount = sonogram.Data.GetLength(0);
        //    int colCount = sonogram.Data.GetLength(1);
        //    recording.Dispose();
        //    double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, (rowCount - 1), maxbin);

        //    //ALTERNATIVE IS TO USE THE AMPLITUDE SPECTRUM
        //    //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
        //    //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
        //    //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
        //    ////double[] envelope = results2.Item2;
        //    //double windowPower = results2.Item4;



        //    //######################################################################
        //    //ii: DO THE ANALYSIS AND RECOVER SCORES OR WHATEVER
        //    var results = System.Tuple.Create(new double[100], new double[100]);
        //    double[] scoreArray1 = results.Item1;
        //    double[] scoreArray2 = results.Item2; 
        //    //######################################################################

        //    var hits = new double[rowCount, colCount];

        //    //iii: CONVERT SCORES TO ACOUSTIC EVENTS
        //    List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scoreArray1, minHz, maxHz, sonogram.FramesPerSecond, freqBinWidth,
        //                                                                                 intensityThreshold, minDuration, maxDuration);
        //    return System.Tuple.Create(sonogram, hits, scoreArray1, predictedEvents, tsRecordingtDuration);
        //} //Analysis()



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
            if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
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
        public DataTable ConvertEvents2Indices(DataTable dt, TimeSpan unitTime, TimeSpan timeDuration, double scoreThreshold)
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



     public Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, FileInfo fiConfigFile)
     {
         var configuration = new ConfigDictionary(fiConfigFile.FullName);
         Dictionary<string, string> configDict = configuration.GetTable();
         List<string> displayHeaders = configDict[Keys.DISPLAY_COLUMNS].Split(',').ToList();

         DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
         if ((dt == null) || (dt.Rows.Count == 0)) return null;
         dt = DataTableTools.SortTable(dt, key_COUNT + " ASC");

         //bool addColumnOfweightedIndices = true;
         //if (addColumnOfweightedIndices)
         //{
         //    AcousticIndices.InitOutputTableColumns();
         //    double[] weightedIndices = null;
         //    weightedIndices = AcousticIndices.GetArrayOfWeightedAcousticIndices(dt, AcousticIndices.COMBO_WEIGHTS);
         //    string colName = "WeightedIndex";
         //    displayHeaders.Add(colName);
         //    DataTableTools.AddColumn2Table(dt, colName, weightedIndices);
         //}

         DataTable table2Display = ProcessDataTableForDisplayOfColumnValues(dt, displayHeaders);
         return System.Tuple.Create(dt, table2Display);
     } // ProcessCsvFile()

     /// <summary>
     /// takes a data table of indices and normalises column values to values in [0,1].
     /// </summary>
     /// <param name="dt"></param>
     /// <returns></returns>
     public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt, List<string> headers2Display)
     {
         string[] headers = DataTableTools.GetColumnNames(dt);
         List<string> originalHeaderList = headers.ToList();
         List<string> newHeaders = new List<string>();

         List<double[]> newColumns = new List<double[]>();

         for (int i = 0; i < headers2Display.Count; i++)
         {
             string header = headers2Display[i];
             if (!originalHeaderList.Contains(header)) continue;

             double[] values = DataTableTools.Column2ArrayOfDouble(dt, header); //get list of values
             if ((values == null) || (values.Length == 0)) continue;

             double min = 0;
             double max = 1;
             if (header.Equals(Keys.AV_AMPLITUDE))
             {
                 min = -50;
                 max = -5;
                 newColumns.Add(DataTools.NormaliseInZeroOne(values, min, max));
                 newHeaders.Add(header + "  (-50..-5dB)");
             }
             else //default is to normalise in [0,1]
             {
                 newColumns.Add(DataTools.normalise(values)); //normalise all values in [0,1]
                 newHeaders.Add(header);
             }
         }

         //convert type int to type double due to normalisation
         Type[] types = new Type[newHeaders.Count];
         for (int i = 0; i < newHeaders.Count; i++) types[i] = typeof(double);
         var processedtable = DataTableTools.CreateTable(newHeaders.ToArray(), types, newColumns);

         return processedtable;
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

    } //end class Harmonics
}
