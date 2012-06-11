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
    public class PlanesTrainsAndAutomobiles : IAnalysis
    {
        //private const int COL_NUMBER = 7;
        //private static Type[] COL_TYPES = new Type[COL_NUMBER];
        //private static string[] HEADERS = new string[COL_NUMBER];
        //private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
        //private static double[] COMBO_WEIGHTS = new double[COL_NUMBER];

        //public static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        //{
        //    HEADERS[0] = "count";      COL_TYPES[0] = typeof(int);     DISPLAY_COLUMN[0] = false;
        //    HEADERS[1] = "EvStartAbs"; COL_TYPES[1] = typeof(int);     DISPLAY_COLUMN[1] = false;
        //    HEADERS[2] = "EvStartMin"; COL_TYPES[2] = typeof(int);     DISPLAY_COLUMN[2] = false;
        //    HEADERS[3] = "EvStartSec"; COL_TYPES[3] = typeof(int);     DISPLAY_COLUMN[3] = false;
        //    HEADERS[4] = "SegmentDur"; COL_TYPES[4] = typeof(string);  DISPLAY_COLUMN[4] = false;
        //    HEADERS[5] = "Density";    COL_TYPES[5] = typeof(int);     DISPLAY_COLUMN[5] = true;
        //    HEADERS[6] = "MachineScore"; COL_TYPES[6] = typeof(double);  DISPLAY_COLUMN[6] = true;
        //    return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        //}

        //public static bool[] MachineColumns2Display()
        //{
        //    bool[] machineColumns2Display = { false, false, true, true, true };
        //    return machineColumns2Display;
        //}


        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_MIN_FORMANT_GAP = "MIN_FORMANT_GAP";
        public static string key_MAX_FORMANT_GAP = "MAX_FORMANT_GAP";
        public static string key_EXPECTED_HARMONIC_COUNT = "EXPECTED_HARMONIC_COUNT";
        public static string key_MIN_AMPLITUDE   = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION    = "MIN_FORMANT_DURATION";
        public static string key_MAX_DURATION    = "MAX_FORMANT_DURATION";
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";


        //KEYS TO OUTPUT EVENTS and INDICES
        public static string key_COUNT = "count";
        public static string key_SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string key_START_ABS = "EvStartAbs";
        public static string key_START_MIN = "EvStartMin";
        public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE = "CallScore";
        public static string key_EVENT_TOTAL = "# events";

        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "PlanesTrainsAndAutomobiles";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;
        //public const string imageViewer = @"C:\Program Files\Windows Photo Viewer\ImagingDevices.exe";
        public const string imageViewer = @"C:\Windows\system32\mspaint.exe";


        public string DisplayName
        {
            get { return "Analysis Template"; }
        }

        public string Identifier
        {
            get { return "Towsey." + ANALYSIS_NAME; }
        }


        public static void Dev(string[] args)
        {
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min173Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min449Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\DM420036_min700Airplane.wav";
            string recordingPath = @"C:\SensorNetworks\WavFiles\Machines\KAPITI2-20100219-202900_Airplane.mp3";

            string configPath = @"C:\SensorNetworks\Output\Machines\Machine.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\Machines\";


            string title = "# FOR DETECTION OF PLANES, TRAINS AND AUTOMOBILES using CROSS-CORRELATION & FFT";
            string date  = "# DATE AND TIME: " + DateTime.Now;
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
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                Console.WriteLine("Output directory does not exist: " + recordingPath);
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
            IAnalysis analyser = new PlanesTrainsAndAutomobiles();
            AnalysisResult result = analyser.Analyse(analysisSettings);
            DataTable dt = result.Data;
            //#############################################################################################################################################

            //ADD IN ADDITIONAL INFO TO RESULTS TABLE
            if (dt != null)
            {
                DataTable augmentedTable = AnalysisTemplate.AddContext2Table(dt, tsStart, result.AudioDuration);
                CsvTools.DataTable2CSV(augmentedTable, analysisSettings.EventsFile.FullName);
                //DataTableTools.WriteTable(augmentedTable);
            }

            return status;
        }


        public AnalysisResult Analyse(AnalysisSettings analysisSettings)
        {
            var configuration = new ConfigDictionary(analysisSettings.ConfigFile.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();
            var fiAudioF = analysisSettings.AudioFile;
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
                                                                                   Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict)
        {
            string analysisName = configDict[key_ANALYSIS_NAME];
            int minFormantgap = Int32.Parse(configDict[key_MIN_FORMANT_GAP]);
            int maxFormantgap = Int32.Parse(configDict[key_MAX_FORMANT_GAP]);
            int minHz = Int32.Parse(configDict[key_MIN_HZ]);
            double intensityThreshold = Double.Parse(configDict[key_INTENSITY_THRESHOLD]); //in 0-1
            double minDuration = Double.Parse(configDict[key_MIN_DURATION]);  // seconds

            AudioRecording recording = new AudioRecording(fiSegmentOfSourceFile.FullName);
            if (recording == null)
            {
                Console.WriteLine("AudioRecording == null. Analysis not possible.");
                return null;
            }

            //#############################################################################################################################################
            var results = DetectHarmonics(recording, intensityThreshold, minFormantgap, maxFormantgap, minDuration); //uses XCORR and FFT
            recording.Dispose();
            //#############################################################################################################################################

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            foreach (AcousticEvent ev in predictedEvents)
            {
                ev.SourceFileName = recording.FileName;
                ev.Name = analysisName;
            }

            TimeSpan tsRecordingtDuration = recording.Duration();
            return System.Tuple.Create(sonogram, hits, scores, predictedEvents, tsRecordingtDuration);
        } //Analysis()



        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> DetectHarmonics(AudioRecording recording, double intensityThreshold,
                                                                                         int minFormantgap, int maxFormantgap, double minDuration)
        {
            //i: MAKE SONOGRAM
            int frameSize = 2048;
            int numberOfBins = 32;
            int minBin = 8;
            double windowOverlap = 0.0;
            double binWidth = recording.SampleRate / (double)frameSize;
            int sr = recording.SampleRate;
            double framesPerSecond = binWidth;

            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            //double[] envelope = results2.Item2;
            double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            double windowPower = results2.Item4;

            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int minHz = (int)Math.Round(minBin * binWidth);
            int maxHz  = (int)Math.Round(minHz + (numberOfBins * binWidth));

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int maxbin = minBin + numberOfBins;
            double[,] subMatrix = MatrixTools.Submatrix(matrix, 0, (minBin + 1), (rowCount - 1), maxbin);

            //ii: DETECT HARMONICS
            int zeroBinCount = 5; //to remove low freq content which dominates the spectrum
            var results = CrossCorrelation.DetectBarsInTheRowsOfaMatrix(subMatrix, intensityThreshold, zeroBinCount);
            double[] intensity = results.Item1;     //an array of periodicity scores
            double[] periodicity = results.Item2;

            //transfer periodicity info to a hits matrix.
            //intensity = DataTools.filterMovingAverage(intensity, 3);
            double[] scoreArray = new double[intensity.Length];
            var hits = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                double relativePeriod = periodicity[r] / numberOfBins / 2;
                if (intensity[r] > intensityThreshold)
                {
                    for (int c = minBin; c < maxbin; c++)
                    {
                        hits[r, c] = relativePeriod;
                    }
                }
                double herzPeriod = periodicity[r] * binWidth;
                if ((herzPeriod > minFormantgap) && (herzPeriod < maxFormantgap))
                       scoreArray[r] = 2 * intensity[r] * intensity[r];    //enhance high score wrt low score.
            }
            scoreArray = DataTools.filterMovingAverage(scoreArray, 11);

            //iii: CONVERT TO ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scoreArray, minHz, maxHz, framesPerSecond, binWidth,
                                                                                         intensityThreshold, minDuration, 100000.0);
            hits = null;

            //set up the songogram to return. Use the existing amplitude sonogram
            int bitsPerSample = recording.GetWavReader().BitsPerSample;
            TimeSpan duration = recording.Duration();
            //NoiseReductionType nrt = SNR.Key2NoiseReductionType("NONE");
            NoiseReductionType nrt = SNR.Key2NoiseReductionType("STANDARD");

            var sonogram = (BaseSonogram)SpectralSonogram.GetSpectralSonogram(recording.FileName, frameSize, windowOverlap, bitsPerSample, windowPower, sr, duration, nrt, matrix);

            sonogram.DecibelsNormalised = new double[rowCount];
            for (int i = 0; i < rowCount; i++) //foreach frame or time step
            {
                sonogram.DecibelsNormalised[i] = 2 * Math.Log10(avAbsolute[i]);
            }
            sonogram.DecibelsNormalised = DataTools.normalise(sonogram.DecibelsNormalised);

           return System.Tuple.Create(sonogram, hits, scoreArray, predictedEvents);
        }//end Execute_HDDetect


        public static DataTable WriteEvents2DataTable(List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            string[] headers = { key_START_SEC, key_CALL_SCORE };
            Type[] types = { typeof(double), typeof(double) };

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
            // double[] processedColumn = null;

            for (int i = 0; i < headers2Display.Count; i++)
            {
                string header = headers2Display[i];
                if (!originalHeaderList.Contains(header)) continue;

                List<double> values = DataTableTools.Column2ListOfDouble(dt, header); //get list of values
                if ((values == null) || (values.Count == 0)) continue;

                double min = 0;
                double max = 1;
                if (header.Equals(key_COUNT))
                {
                    newColumns.Add(DataTools.normalise(values.ToArray())); //normalise all values in [0,1]
                    newHeaders.Add(header);
                }
                else if (header.Equals(Keys.AV_AMPLITUDE))
                {
                    min = -50;
                    max = -5;
                    newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                    newHeaders.Add(header + "  (-50..-5dB)");
                }
                else //default is to normalise in [0,1]
                {
                    newColumns.Add(DataTools.normalise(values.ToArray())); //normalise all values in [0,1]
                    newHeaders.Add(header);
                }
            }

            //convert type int to type double due to normalisation
            Type[] types = new Type[newHeaders.Count];
            for (int i = 0; i < newHeaders.Count; i++) types[i] = typeof(double);
            var processedtable = DataTableTools.CreateTable(newHeaders.ToArray(), types, newColumns);

            return processedtable;
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
            if (units % 1 > 0.0) unitCount += 1;
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
            Type[] types = { typeof(int), typeof(int), typeof(int) };
            var newtable = DataTableTools.CreateTable(headers, types);

            for (int i = 0; i < eventsPerMinute.Length; i++)
            {
                newtable.Rows.Add(i, eventsPerMinute[i], bigEvsPerMinute[i]);
            }
            return newtable;
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
                    SegmentTargetSampleRate = PlanesTrainsAndAutomobiles.RESAMPLE_RATE
                };
            }
        }



    } //end class PlanesTrainsAndAutomobiles
}
