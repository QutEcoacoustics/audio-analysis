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
    public class Human
    {
        public const string defaultRecordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
        //public const string defaultRecordingPath = @"C:\SensorNetworks\Software\AudioAnalysis\AudioBrowser\bin\Debug\Audio-samples\Wimmer_DM420011.wav";
        //public const string defaultRecordingPath = @"C:\SensorNetworks\WavFiles\Human\BirgitTheTerminator.wav";
        public const string defaultConfigPath = @"C:\SensorNetworks\Output\HD_HUMAN\Human.cfg";
        public const string defaultOutputDir = @"C:\SensorNetworks\Output\HD_HUMAN\";

        // for HUMAN SPPECH
        //hd C:\SensorNetworks\WavFiles\Human\BirgitTheTerminator.wav C:\SensorNetworks\Output\HD_HUMAN\HD_HUMAN_Params.txt events.txt


        public const string ANALYSIS_NAME = "Human";
        public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        public const int DEFAULT_WINDOW_SIZE = 256;

        private const int COL_NUMBER = 7;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
        private static double[] COMBO_WEIGHTS = new double[COL_NUMBER];



        public static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        {
            HEADERS[0] = "count";      COL_TYPES[0] = typeof(int);     DISPLAY_COLUMN[0] = false;
            HEADERS[1] = "EvStartAbs"; COL_TYPES[1] = typeof(int);     DISPLAY_COLUMN[1] = false;
            HEADERS[2] = "EvStartMin"; COL_TYPES[2] = typeof(int);     DISPLAY_COLUMN[2] = false;
            HEADERS[3] = "EvStartSec"; COL_TYPES[3] = typeof(int);     DISPLAY_COLUMN[3] = false;
            HEADERS[4] = "SegmentDur"; COL_TYPES[4] = typeof(string);  DISPLAY_COLUMN[4] = false;
            HEADERS[5] = "Density";    COL_TYPES[5] = typeof(int);     DISPLAY_COLUMN[5] = true;
            HEADERS[6] = "HumanScore"; COL_TYPES[6] = typeof(double);  DISPLAY_COLUMN[6] = true;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }


        public static bool[] HumanColumns2Display()
        {
            bool[] humanColumns2Display = { false, false, true, true, true };
            return humanColumns2Display;

        }


        //Keys to recognise identifiers in CFG file. 
        public static string key_ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_EXPECTED_HARMONIC_COUNT = "EXPECTED_HARMONIC_COUNT";
       // public static string key_MIN_HARMONIC_PERIOD = "MIN_HARMONIC_PERIOD";
       // public static string key_MAX_HARMONIC_PERIOD = "MAX_HARMONIC_PERIOD";
        public static string key_INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string key_MIN_AMPLITUDE   = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
       // public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";


        public static void Dev(string[] args)
        {
            string title = "# FOR DETECTION OF THE HUMAN VOICE";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);
            Log.Verbosity = 1;

            //string recordingPath = args[0];
            //string iniPath   = args[1];
            //string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            //string opFName   = args[2];
            //string opPath    = outputDir + opFName;



            string recordingPath = defaultRecordingPath;
            string iniPath       = defaultConfigPath;
            string outputDir     = defaultOutputDir;
            string opFName       = ANALYSIS_NAME + ".txt";
            string opPath        = outputDir + opFName;

            string audioFileName = Path.GetFileName(recordingPath);

                       
            Console.WriteLine("# Output folder:  " + outputDir);
            Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            FileTools.WriteTextFile(opPath, date + "\n# Recording file: " + audioFileName);

            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;
            try
            {
                string analysisName = dict[key_ANALYSIS_NAME];
                NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[key_NOISE_REDUCTION_TYPE]);
                int minHz = Int32.Parse(dict[key_MIN_HZ]);
                int maxHz = Int32.Parse(dict[key_MAX_HZ]);
                int frameLength = Int32.Parse(dict[Human.key_FRAME_LENGTH]);
                double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
                double minAmplitude = Double.Parse(dict[key_MIN_AMPLITUDE]);        // minimum acceptable value of harmonic ocsillation in dB
                int harmonicCount = Int32.Parse(dict[key_EXPECTED_HARMONIC_COUNT]); // expected number of harmonics to find in spectrum
                double minDuration = Double.Parse(dict[key_MIN_DURATION]);          // lower bound for the duration of an event
                double maxDuration = Double.Parse(dict[key_MAX_DURATION]);          // upper bound for the duration of an event
                double intensityThreshold = Double.Parse(dict[key_INTENSITY_THRESHOLD]);
                int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);         // options to draw sonogram

                Console.WriteLine("\tFreq band: {0}-{1} Hz.)", minHz, maxHz);
                Console.WriteLine("\tNoise Reduction type: {0}", nrt.ToString());
                Console.WriteLine("\tExpected harmonic count within bandwidth: {0}", harmonicCount);
                Console.WriteLine("\tIntensity threshold for hit = " + intensityThreshold);
                //Console.WriteLine("\tThreshold Min Amplitude = " + minAmplitude + " dB (peak to trough)");
                Console.WriteLine("\tDuration Bounds min-max: {0:f2} - {1:f2} seconds", minDuration, maxDuration);

                int minFormantgap = 90;
                int maxFormantgap = 250;

                string tempSegmentPath = Path.Combine(outputDir, "temp.wav"); //path location/name of extracted recording segment
                var fiSourceRecording = new FileInfo(recordingPath);
                var fiTempSegmentFile = new FileInfo(tempSegmentPath);
                
                IAudioUtility audioUtility = new MasterAudioUtility();
                var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
                var sourceDuration = audioUtility.Duration(fiSourceRecording, mimeType); // Get duration of the source file
                int startMilliseconds = 0;
                int endMilliseconds = (int)sourceDuration.TotalMilliseconds;
                int resampleRate = 17640;
                //int resampleRate = 22050;
                Console.WriteLine("\tRecording Duration: {0:f2}seconds", sourceDuration.TotalSeconds);
                    
//#############################################################################################################################################
                MasterAudioUtility.SegmentToWav(resampleRate, fiSourceRecording, new FileInfo(tempSegmentPath), startMilliseconds, endMilliseconds);
                AudioRecording recording = new AudioRecording(tempSegmentPath);

                //double threshold = minAmplitude;
                //var results = Execute_HDDetect(recording, nrt, frameLength, frameOverlap, minHz, maxHz, harmonicCount, threshold, minDuration, maxDuration);
                double threshold = intensityThreshold;
                var results = Execute_HDDetect(recording, minHz, threshold, minFormantgap, maxFormantgap, minDuration, maxDuration); //uses XCORR and FFT

//#############################################################################################################################################

                var sonogram = results.Item1;
                var hits = results.Item2;
                var scores = results.Item3;
                var predictedEvents = results.Item4;
                Console.WriteLine("# Event Count = " + predictedEvents.Count());

                //write event count to results file.            
                //WriteEventsInfo2TextFile(predictedEvents, opPath);
                double displayThreshold = 0.2; //relative position of threhsold in image of score track.
                double normMax = threshold / displayThreshold; //threshold
                //double normMax = threshold * 4; //previously used for 4 dB threshold - so normalised eventThreshold = 0.25
                for (int i = 0; i < scores.Length; i++) scores[i] /= normMax;
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";

                if (DRAW_SONOGRAMS==2)
                {
                    Console.WriteLine("\tMin score={0:f3}  Max score={1:f3}", scores.Min(), scores.Max());
                    Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, displayThreshold);
                    image.Save(imagePath, ImageFormat.Png);
                }
                else
                if ((DRAW_SONOGRAMS==1) && (predictedEvents.Count > 0))
                {
                    Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, displayThreshold);
                    image.Save(imagePath, ImageFormat.Png);
                }

            } //try
            catch (KeyNotFoundException ex)
            {
                Console.WriteLine("KEY NOT FOUND IN PARAMS FILE: " + ex.ToString());
                Console.ReadLine();
            }

            Console.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()




        /// <summary>
        /// A WRAPPER AROUND THE Execute_HDDetect() method
        /// Returns a DataTable
        /// The Execute_HDDetect() method returns a System.Tuple<BaseSonogram, Double[,], double[], double[], List<AcousticEvent>>
        /// </summary>
        /// <param name="iter"></param>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        public static DataTable Analysis(int iter, FileInfo fiSegmentAudioFile, Dictionary<string, string> config, DirectoryInfo diOutputDir)
        {
            string analysisName = config[Human.key_ANALYSIS_NAME];
            int minHz = Configuration.GetInt(Human.key_MIN_HZ, config);
            int maxHz = Configuration.GetInt(Human.key_MAX_HZ, config);
            int frameLength = Configuration.GetInt(Human.key_FRAME_LENGTH, config);
            double frameOverlap = Configuration.GetDouble(Human.key_FRAME_OVERLAP, config);
            int harmonicCount = Configuration.GetInt(Human.key_EXPECTED_HARMONIC_COUNT, config);
            double minAmplitude = Configuration.GetDouble(Human.key_MIN_AMPLITUDE, config);
            double minDuration = Configuration.GetDouble(Human.key_MIN_DURATION, config); //minimum event duration to qualify as species call
            double maxDuration = Configuration.GetDouble(Human.key_MAX_DURATION, config); //maximum event duration to qualify as species call
            double drawSonograms = Configuration.GetInt(Human.key_DRAW_SONOGRAMS, config);
            double segmentDuration = Configuration.GetDouble(Human.key_SEGMENT_DURATION, config);
            double segmentStartMinute = segmentDuration * iter;
            string strNRT = config[Human.key_NOISE_REDUCTION_TYPE];
            NoiseReductionType nrt = SNR.Key2NoiseReductionType(strNRT);
            int minFormantGap = 90;
            int maxFormantGap = 250;

            //#############################################################################################################################################
            AudioRecording recording = new AudioRecording(fiSegmentAudioFile.FullName);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();


            //var results = Execute_HDDetect(recording, nrt, frameLength, frameOverlap, minHz, maxHz, harmonicCount, minAmplitude, minDuration, maxDuration);
            var results = Execute_HDDetect(recording, minHz, minAmplitude, minFormantGap, maxFormantGap, minDuration, maxDuration);
            recording.Dispose();
            //#############################################################################################################################################

            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            foreach (AcousticEvent ev in predictedEvents)
            {
                ev.SourceFile = recording.FileName;
                ev.Name = analysisName;
            }

            //draw images of sonograms
            bool saveSonogram = false;
            if ((drawSonograms == 2) || ((drawSonograms == 1) && (predictedEvents.Count > 0))) saveSonogram = true;
            if (saveSonogram)
            {
                double eventThreshold = 0.1;
                string imagePath = Path.Combine(diOutputDir.FullName, Path.GetFileNameWithoutExtension(fiSegmentAudioFile.FullName) + "_" + (int)segmentStartMinute + "min.png");
                Image image = DrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
                image.Save(imagePath, ImageFormat.Png);
            }

            //write events to a data table to return.
            TimeSpan tsSegmentDuration = recording.Duration();
            DataTable dataTable = WriteEvents2DataTable(iter, segmentStartMinute, tsSegmentDuration, predictedEvents);
            //Log.WriteLine("# Event count for minute {0} = {1}", startMinutes, dataTable.Rows.Count);

            string sortString = "EvStartAbs ASC";
            return DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
        } //Analysis()


        public static DataTable WriteEvents2DataTable(int count, double segmentStartMinute, TimeSpan tsSegmentDuration, List<AcousticEvent> predictedEvents)
        {
            if (predictedEvents == null) return null;
            var dataTable = DataTableTools.CreateTable(HEADERS, COL_TYPES);
            if (predictedEvents.Count == 0) return dataTable;
            foreach (var speechEvent in predictedEvents)
            {
                int segmentStartSec = (int)(segmentStartMinute * 60);
                int eventStartAbsoluteSec = (int)(segmentStartSec + speechEvent.TimeStart);
                int eventStartMin = eventStartAbsoluteSec / 60;
                int eventStartSec = eventStartAbsoluteSec % 60;
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(tsSegmentDuration.TotalSeconds);

                DataRow row = dataTable.NewRow();
                row[HEADERS[0]] = count;                   //count
                row[HEADERS[1]] = eventStartAbsoluteSec;   //EvStartAbsolute - from start of source ifle
                row[HEADERS[2]] = eventStartMin;           //EvStartMin
                row[HEADERS[3]] = eventStartSec;           //EvStartSec
                row[HEADERS[4]] = segmentDuration;         //segmentDur
                row[HEADERS[5]] = predictedEvents.Count;   //Density
                row[HEADERS[6]] = speechEvent.Score;       //Score
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }



        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_HDDetect(AudioRecording recording, NoiseReductionType nrt, int frameLength,
                                                   double frameOverlap, int minHz, int maxHz, int harmonicCount, double amplitudeThreshold, double minDuration, double maxDuration)
        {
            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = frameLength;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.NoiseReductionType = nrt;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();

            //ii: DETECT HARMONICS
            var results = HarmonicAnalysis.Execute((SpectralSonogram)sonogram,minHz,maxHz,harmonicCount, amplitudeThreshold);
            double[] scores = results.Item1;     //an array of periodicity scores
            Double[,] hits = results.Item2;      //hits matrix - to superimpose on sonogram image
            // ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                                         amplitudeThreshold, minDuration, maxDuration);

            return System.Tuple.Create(sonogram, hits, scores, predictedEvents);

        }//end Execute_HDDetect


        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_HDDetect(AudioRecording recording, int minHz, double intensityThreshold,
                                                                                         int minFormantgap, int maxFormantgap, double minDuration, double maxDuration)
        {
                        //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = 1024;
            sonoConfig.WindowOverlap  = 0.0;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            double binWidth = recording.SampleRate / (double)sonoConfig.WindowSize;
            double framesPerSecond = binWidth;

            //window    sr          frameDuration   frames/sec  hz/bin  64frameDuration hz/64bins       hz/128bins
            // 1024     22050       46.4ms          21.5        21.5    2944ms          1376hz          2752hz
            // 1024     17640       58.0ms          17.2        17.2    3715ms          1100hz          2200hz
            // 2048     17640       116.1ms          8.6         8.6    7430ms           551hz          1100hz

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=17640 and window=1024, then  64 bins span 1100 Hz above the min Hz level. i.e. 500 to 1600
            //assuming sr=17640 and window=1024, then 128 bins span 2200 Hz above the min Hz level. i.e. 500 to 2700
            int numberOfBins = 64;
            int minBin = (int)Math.Round(minHz / binWidth); 
            int maxHz  = (int)Math.Round(minHz + (numberOfBins * binWidth));

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());

            //ii: DETECT HARMONICS
            var results = BarsAndStripes.DetectStripesInColumnsOfMatrix(sonogram.Data, minBin, numberOfBins, intensityThreshold);
            double[] intensity = results.Item1;     //an array of periodicity scores
            double[] periodicity = results.Item2;
            Double[,] hits = results.Item3;      //hits matrix - to superimpose on sonogram image

            double[] score = new double[intensity.Length];
            for (int i = 0; i < intensity.Length; i++)
            {
                double herzPeriod = periodicity[i] * binWidth;
                if ((herzPeriod > minFormantgap) && (herzPeriod < maxFormantgap)) score[i] = intensity[i];
            }
            //intensity = DataTools.filterMovingAverage(intensity, 3);

            //iii: CONVERT TO ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(score, minHz, maxHz, sonogram.FramesPerSecond, binWidth,
                                                                                         intensityThreshold, minDuration, maxDuration);

            return System.Tuple.Create(sonogram, hits, score, predictedEvents);

        }//end Execute_HDDetect



        /// <summary>
        /// TODO: This method should call the Execute_HDDetect() method
        /// Get all the intermediate information and return a sonogram with annotations.
        /// </summary>
        /// <param name="fiSegmentAudioFile"></param>
        /// <param name="config"></param>
        public static Image GetImageFromAudioSegment(FileInfo fiSegmentAudioFile, Dictionary<string, string> config)
        {
            if (config == null) return null;
            Image image = null;
            //Image image = MakeAndDrawSonogram(sonogram, hits, scores, predictedEvents, eventThreshold);
            return image;
        } //GetImageFromAudioSegment()




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



        public static Tuple<DataTable, DataTable, bool[]> ProcessCsvFile(FileInfo fiCsvFile)
        {
            AcousticIndices.InitOutputTableColumns(); //initialise just in case have not been before now.
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
            if ((dt == null) || (dt.Rows.Count == 0)) return null;

            dt = DataTableTools.SortTable(dt, "count ASC");
            List<bool> columns2Display = HumanColumns2Display().ToList();
            DataTable processedtable = ProcessDataTableForDisplayOfColumnValues(dt);
            return System.Tuple.Create(dt, processedtable, columns2Display.ToArray());
        } // ProcessCsvFile()


        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt)
        {
            List<double[]> columns = DataTableTools.ListOfColumnValues(dt);
            List<double[]> newColumns = new List<double[]>();
            for (int i = 0; i < columns.Count; i++)
            {
                double[] processedColumn = DataTools.normalise(columns[i]); //normalise all values in [0,1]
                newColumns.Add(processedColumn);
            }
            string[] headers = DataTableTools.GetColumnNames(dt);
            Type[] types = DataTableTools.GetColumnTypes(dt);
            for (int i = 0; i < columns.Count; i++)
            {
                if (types[i] == typeof(int)) types[i] = typeof(double);
                else
                    if (types[i] == typeof(Int32)) types[i] = typeof(double);
            }
            var processedtable = DataTableTools.CreateTable(headers, types, newColumns);

            return processedtable;
        }




    } //end class Human
}
