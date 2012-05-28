using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using TowseyLib;
using AudioAnalysisTools;
using Acoustics.Shared;
using Acoustics.Tools.Audio;



//Here is link to wiki page containing info about how to write Analysis techniques
//https://wiki.qut.edu.au/display/mquter/Audio+Analysis+Processing+Architecture
//


namespace AnalysisPrograms
{
    public class AnalysisTemplate
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
        public static string key_MIN_FORMANT_GAP = "MIN_FORMANT_GAP";
        public static string key_MAX_FORMANT_GAP = "MAX_FORMANT_GAP";
        public static string key_EXPECTED_HARMONIC_COUNT = "EXPECTED_HARMONIC_COUNT";
        public static string key_MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION = "MIN_FORMANT_DURATION";
        public static string key_MAX_DURATION = "MAX_FORMANT_DURATION";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        //KEYS TO OUTPUT INDICES
        public static string key_COUNT     = "count";
        public static string key_START_ABS = "EvStartAbs";
        public static string key_START_MIN = "EvStartMin";
        public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE   = "CallScore";

        //INITIALISE OUTPUT TABLE COLUMNS
        private const int COL_NUMBER = 7;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static System.Tuple<string[], Type[]> InitOutputTableColumns()
        {
            HEADERS[0] = key_COUNT;            COL_TYPES[0] = typeof(int);
            HEADERS[1] = key_START_ABS;        COL_TYPES[1] = typeof(int);
            HEADERS[2] = key_START_MIN;        COL_TYPES[2] = typeof(int);
            HEADERS[3] = key_START_SEC;        COL_TYPES[3] = typeof(int);
            HEADERS[4] = key_SEGMENT_DURATION; COL_TYPES[4] = typeof(double);
            HEADERS[5] = key_CALL_DENSITY;     COL_TYPES[5] = typeof(int);
            HEADERS[6] = key_CALL_SCORE;       COL_TYPES[6] = typeof(double); 
            return Tuple.Create(HEADERS,       COL_TYPES);
        }
        public static string[] GetOutputTableHeaders()
        {
            var  op = InitOutputTableColumns();
            return op.Item1;
        }
        public static Type[] GetOutputTableTypes()
        {
            var op = InitOutputTableColumns();
            return op.Item2;
        }

        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Crow";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;


        public static void Dev(string[] args)
        {
            string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\Software\AudioAnalysis\AudioBrowser\bin\Debug\Audio-samples\Wimmer_DM420011.wav";
            string configPath = @"C:\SensorNetworks\Output\Crow\Crow.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\Crow\";

            string opFName       = ANALYSIS_NAME + ".txt";
            string opPath        = outputDir + opFName;
            string audioFileName = Path.GetFileName(recordingPath);
            Log.Verbosity = 1;

            string title = "# FOR DETECTION OF CROW CALLS - version 2";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Console.WriteLine(title);
            Console.WriteLine(date);
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + audioFileName);

            //READ PARAMETER VALUES FROM INI FILE
            var configuration = new Configuration(configPath);
            Dictionary<string, string> configDict = configuration.GetTable();
            Dictionary<string, string>.KeyCollection keys = configDict.Keys;

            int startMinute = 5; //dummy value
            var fiSegmentOfSourceFile = new FileInfo(recordingPath);
            var diOutputDir = new DirectoryInfo(outputDir);

            //#############################################################################################################################################
            DataTable dt = AnalysisReturnsDataTable(startMinute, fiSegmentOfSourceFile, configDict, diOutputDir);
            //#############################################################################################################################################
            if(dt == null)
            {
                Log.WriteLine("\n\n\n##############################\n WARNING! Null return. Possibly source file does not exist");
            } else 
            {
                //Console.WriteLine("\tRecording Duration: {0:f2}seconds", recordingTimeSpan.TotalSeconds);
                Console.WriteLine("# Event count for minute {0} = {1}", startMinute, dt.Rows.Count);
                DataTableTools.WriteTable(dt);
            }

            Console.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static DataTable AnalysisReturnsDataTable(int iter, FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir)
        {
            string opFileName = "temp.wav";
            //######################################################################
            var results = Analysis(iter, fiSegmentOfSourceFile, configDict, diOutputDir, opFileName);
            //######################################################################
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;
            
            string analysisName = configDict[key_ANALYSIS_NAME];
            string fName = Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.Name);
            foreach (AcousticEvent ev in predictedEvents)
            {
                ev.SourceFileName     = fName;
                ev.Name               = analysisName;
                ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
            }

            double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
            double segmentStartMinute = segmentDuration * iter;

            //draw images of sonograms
            int DRAW_SONOGRAMS = Int32.Parse(configDict[key_DRAW_SONOGRAMS]);         // options to draw sonogram
            bool saveSonogram = false;
            if ((DRAW_SONOGRAMS == 2) || ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))) saveSonogram = true;
            if (saveSonogram)
            {
                double eventThreshold = 0.1;
                string imagePath = Path.Combine(diOutputDir.FullName, Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.FullName) + "_" + (int)segmentStartMinute + "min.png");
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }

            //write events to a data table to return.
            DataTable dataTable = WriteEvents2DataTable(iter, segmentStartMinute, recordingTimeSpan, predictedEvents);

            string sortString = key_START_ABS + " ASC";
            return DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
        }

        public static Image AnalysisReturnsSonogram(int iter, FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir)
        {
            double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
            double segmentStartMinute = segmentDuration * iter;
            string newFileNameWithoutExtention = Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.FullName) + "_" + (int)segmentStartMinute + "min";
            string opFileName = newFileNameWithoutExtention + ".wav";

            //######################################################################
            var results = Analysis(iter, fiSegmentOfSourceFile, configDict, diOutputDir, opFileName);
            //######################################################################
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;
            Console.WriteLine("\tRecording Duration: {0:f2}seconds", recordingTimeSpan.TotalSeconds);

            double eventThreshold = 0.1;
            Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
            string imagePath = Path.Combine(diOutputDir.FullName, newFileNameWithoutExtention + ".png");
            image.Save(imagePath, ImageFormat.Png);
            return image; 
        }

        /// <summary>
        /// A WRAPPER AROUND THE Execute_HarmonicDetection() method
        /// Returns a DataTable
        /// The Execute_HDDetect() method returns a System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>>
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan> 
                                        Analysis(int iter, FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir, string opFileName)
        {
            //set default values
            int frameSize = 1024;
            double windowOverlap = 0.0;

            int minHz = Int32.Parse(configDict[key_MIN_HZ]);
            int minFormantgap = Int32.Parse(configDict[key_MIN_FORMANT_GAP]);
            int maxFormantgap = Int32.Parse(configDict[key_MAX_FORMANT_GAP]);
            double decibelThreshold = Double.Parse(configDict[key_DECIBEL_THRESHOLD]); ;   //dB
            double intensityThreshold = Double.Parse(configDict[key_INTENSITY_THRESHOLD]); //in 0-1
            double callDuration = Double.Parse(configDict[key_CALL_DURATION]);  // seconds

            AudioRecording recording = AudioRecording.GetAudioRecording(fiSegmentOfSourceFile, RESAMPLE_RATE, diOutputDir.FullName, opFileName);
            if (recording == null) return null;

            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameSize;
            sonoConfig.WindowOverlap = windowOverlap;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            TimeSpan tsRecordingtDuration = recording.Duration();
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
            int maxHz = (int)Math.Round(minHz + (numberOfBins * freqBinWidth));
            int maxbin = minBin + numberOfBins - 1;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            double[,] matrix = sonogram.Data;

            //var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            //double[,] matrix = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            ////double[] envelope = results2.Item2;
            //double windowPower = results2.Item4;
            recording.Dispose();

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] subMatrix = MatrixTools.Submatrix(matrix, 0, minBin, (rowCount - 1), maxbin);


            int callSpan = (int)Math.Round(callDuration * framesPerSecond);

            //#############################################################################################################################################
            //ii: DETECT HARMONICS
            var results = CrossCorrelation.DetectHarmonicsInSonogramMatrix(subMatrix, decibelThreshold, callSpan);
            double[] dBArray = results.Item1;
            double[] intensity = results.Item2;     //an array of periodicity scores
            double[] periodicity = results.Item3;

            //transfer periodicity info to a hits matrix.
            //intensity = DataTools.filterMovingAverage(intensity, 3);
            double[] scoreArray = new double[intensity.Length];
            var hits = new double[rowCount, colCount];
            for (int r = 0; r < rowCount; r++)
            {
                if (periodicity[r] < 2) continue;
                //ignore locations with incorrect formant gap
                double herzPeriod = periodicity[r] * freqBinWidth;
                if ((herzPeriod < minFormantgap) || (herzPeriod > maxFormantgap)) continue;

                //set up the hits matrix
                double relativePeriod = periodicity[r] / colCount / 2;
                for (int c = minBin; c < maxbin; c++) hits[r, c] = relativePeriod;

                //set scoreArray[r]  - ignore locations with low intensity
                if (intensity[r] > intensityThreshold) scoreArray[r] = intensity[r];
            }


            //iii: CONVERT TO ACOUSTIC EVENTS
            double maxPossibleScore = 0.5;
            int halfCallSpan = callSpan / 2;
            var predictedEvents = new List<AcousticEvent>();
            for (int i = 0; i < rowCount; i++) // pass over all frames
            {
                //assume one score position per crow call
                if (scoreArray[i] < 0.001) continue;
                double startTime = (i - halfCallSpan) / framesPerSecond;
                AcousticEvent ev = new AcousticEvent(startTime, callDuration, minHz, maxHz);
                ev.SetTimeAndFreqScales(framesPerSecond, freqBinWidth);
                ev.Score = scoreArray[i];
                ev.ScoreNormalised = ev.Score / maxPossibleScore; // normalised to the user supplied threshold
                //ev.Score_MaxPossible = maxPossibleScore;
                predictedEvents.Add(ev);
            }
            return System.Tuple.Create(sonogram, hits, intensity, predictedEvents, tsRecordingtDuration);
        } //Analysis()



        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            //Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            double maxScore = 1.0;
            Image_MultiTrack image = new Image_MultiTrack(sonogram.GetImage(doHighlightSubband, add1kHzLines));


            //System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines);
            //img.Save(@"C:\SensorNetworks\temp\testimage1.png", System.Drawing.Imaging.ImageFormat.Png);

            //Image_MultiTrack image = new Image_MultiTrack(img);
            image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
            image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
            if (scores != null) image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
            if (hits != null) image.AddSuperimposedMatrix(hits, maxScore);
            if (predictedEvents.Count > 0) image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount);
            return image.GetImage();
        } //DrawSonogram()


        public static DataTable WriteEvents2DataTable(int count, double segmentStartMinute, TimeSpan tsSegmentDuration, List<AcousticEvent> predictedEvents)
        {
            if ((predictedEvents == null) || (predictedEvents.Count == 0)) return null;

            InitOutputTableColumns();
            var dataTable = DataTableTools.CreateTable(HEADERS, COL_TYPES);
            foreach (var ev in predictedEvents)
            {
                int segmentStartSec = (int)(segmentStartMinute * 60);
                int eventStartAbsoluteSec = (int)(segmentStartSec + ev.TimeStart);
                int eventStartMin = eventStartAbsoluteSec / 60;
                int eventStartSec = eventStartAbsoluteSec % 60;

                DataRow row = dataTable.NewRow();
                row[key_COUNT]        = count;                   //count
                row[key_START_ABS]    = eventStartAbsoluteSec;   //EvStartAbsolute - from start of source ifle
                row[key_START_MIN]    = eventStartMin;           //EvStartMin
                row[key_START_SEC]    = eventStartSec;           //EvStartSec
                row[key_SEGMENT_DURATION] = tsSegmentDuration.TotalSeconds; //segment Duration in seconds
                row[key_CALL_DENSITY] = predictedEvents.Count;   //Density
                row[key_CALL_SCORE]   = ev.Score;                //Score
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }



        /// <summary>
        /// Converts a DataTable of acoustic events to a datatable where one row = one minute of events
        /// WARNING: TODO: This method needs to be checked! Maybe putting events into the wrong minute.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ConvertEvents2Indices(DataTable dt)
        {
            dt = DataTableTools.SortTable(dt, key_START_ABS+ " ASC"); //must ensure a sort

            double scoreThreshold = 0.25;
            int timeScale = 60; //i.e. 60 seconds per output row.
            string[] headers = { key_COUNT, key_START_MIN, "# Events", ("#Ev>" + scoreThreshold), key_CALL_SCORE };
            Type[] types = { typeof(int), typeof(int), typeof(int), typeof(int), typeof(double) };
            var newtable = DataTableTools.CreateTable(headers, types);

            int prevMinuteStart = 0;
            int minuteStart = 0;
            int eventcount = 0;
            int eventCountThresholded = 0;
            double indexScore = 0.0;
            foreach (DataRow ev in dt.Rows)
            {
                int eventStart = (int)ev[key_START_ABS];
                double eventScore = (double)ev[key_CALL_SCORE];
                if (eventScore > indexScore) indexScore = eventScore;
                minuteStart = eventStart / timeScale;

                if (minuteStart > prevMinuteStart)
                {
                    // fill in missing minutes
                    for (int i = prevMinuteStart + 1; i < minuteStart; i++)
                        newtable.Rows.Add(i, i, 0, 0, 0.0);
                    newtable.Rows.Add(minuteStart, minuteStart, eventcount, eventCountThresholded, indexScore);
                    prevMinuteStart = minuteStart;
                    eventcount = 1;
                    if (eventScore > scoreThreshold) eventCountThresholded = 1;
                    else eventCountThresholded = 0;
                    indexScore = 0.0;
                }
                else
                {
                    eventcount++;
                    if (eventScore > scoreThreshold) eventCountThresholded++;
                }
            }
            newtable.Rows.Add(minuteStart, minuteStart, eventcount, eventCountThresholded, indexScore); //add in last minute

            //NEXT TWO LINES ARE FOR DEBUG PURPOSES ONLY
            //string outputTablePath = @"C:\SensorNetworks\WavFiles\Kiwi\Results_TOWER_20100208_204500\DELETE_ME.csv";
            //CsvTools.DataTable2CSV(newtable, outputTablePath);
            return newtable;
        }




    } //end class AnalysisTemplate
}
