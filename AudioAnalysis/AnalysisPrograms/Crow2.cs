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
    public class Crow2
    {
        public const string defaultRecordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\111216-001Mono5-7min.mp3";
        //public const string defaultRecordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
        //public const string defaultRecordingPath = @"C:\SensorNetworks\Software\AudioAnalysis\AudioBrowser\bin\Debug\Audio-samples\Wimmer_DM420011.wav";
        public const string defaultConfigPath = @"C:\SensorNetworks\Output\Crow\Crow.cfg";
        public const string defaultOutputDir = @"C:\SensorNetworks\Output\Crow\";


        public const string ANALYSIS_NAME = "Crow";
        //public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        //public const int DEFAULT_WINDOW_SIZE = 256;
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;


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
            HEADERS[6] = "CrowScore";  COL_TYPES[6] = typeof(double);  DISPLAY_COLUMN[6] = true;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }


        public static bool[] CrowColumns2Display()
        {
            bool[] humanColumns2Display = { false, false, true, true, true };
            return humanColumns2Display;

        }


        //Keys to recognise identifiers in CFG file. 
        public static string key_ANALYSIS_NAME     = "ANALYSIS_NAME";
        public static string key_CALL_DURATION     = "CALL_DURATION";
        public static string key_DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_HARMONIC_INTENSITY_THRESHOLD = "HARMONIC_INTENSITY_THRESHOLD";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE    = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH     = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP    = "FRAME_OVERLAP";
        public static string key_NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string key_MIN_HZ           = "MIN_HZ";
        public static string key_MAX_HZ           = "MAX_HZ";
        public static string key_MIN_FORMANT_GAP  = "MIN_FORMANT_GAP";
        public static string key_MAX_FORMANT_GAP  = "MAX_FORMANT_GAP";
        public static string key_EXPECTED_HARMONIC_COUNT = "EXPECTED_HARMONIC_COUNT";
        public static string key_MIN_AMPLITUDE    = "MIN_AMPLITUDE";
        public static string key_MIN_DURATION     = "MIN_FORMANT_DURATION";
        public static string key_MAX_DURATION     = "MAX_FORMANT_DURATION";
        public static string key_DRAW_SONOGRAMS   = "DRAW_SONOGRAMS";
        

        public static void Dev(string[] args)
        {
            string title = "# FOR DETECTION OF CROW CALLS - version 2";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);
            Log.Verbosity = 1;

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
            var configuration = new Configuration(iniPath);
            Dictionary<string, string> configDict = configuration.GetTable();
            Dictionary<string, string>.KeyCollection keys = configDict.Keys;
            try
            {
                string analysisName = configDict[key_ANALYSIS_NAME];
                double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]); 
                //NoiseReductionType nrt = SNR.Key2NoiseReductionType(dict[key_NOISE_REDUCTION_TYPE]);
                int minHz = Int32.Parse(configDict[key_MIN_HZ]);
                //double minDuration = Double.Parse(configDict[key_MIN_DURATION]);          // lower bound for the duration of an event
                //double maxDuration = Double.Parse(configDict[key_MAX_DURATION]);          // upper bound for the duration of an event
                int DRAW_SONOGRAMS = Int32.Parse(configDict[key_DRAW_SONOGRAMS]);         // options to draw sonogram
                int minFormantgap = Int32.Parse(configDict[key_MIN_FORMANT_GAP]);
                int maxFormantgap = Int32.Parse(configDict[key_MAX_FORMANT_GAP]);
                double decibelThreshold = Double.Parse(configDict[key_DECIBEL_THRESHOLD]); ;   //dB
                double harmonicIntensityThreshold = Double.Parse(configDict[key_HARMONIC_INTENSITY_THRESHOLD]); //in 0-1
                double callDuration = Double.Parse(configDict[key_CALL_DURATION]);  // seconds


                Console.WriteLine("\tBottom of searched freq band: {0} Hz.)", minHz);
                Console.WriteLine("\tHarmonic intensity threshold for hit = " + harmonicIntensityThreshold);
                //Console.WriteLine("\tThreshold Min Amplitude = " + minAmplitude + " dB (peak to trough)");

                string opFileName = "temp.wav";
                var fiSourceRecording = new FileInfo(recordingPath);
                var diOutputDir = new DirectoryInfo(outputDir);
                AudioRecording recording = AudioRecording.GetAudioRecording(fiSourceRecording, Crow.RESAMPLE_RATE, diOutputDir.FullName, opFileName);
                Console.WriteLine("\tRecording Duration: {0:f2}seconds", recording.Duration().TotalSeconds);



                //#############################################################################################################################################
                var results = Execute_HDDetect(recording, minHz, decibelThreshold, harmonicIntensityThreshold, minFormantgap, maxFormantgap, callDuration); //uses XCORR and FFT

                //#############################################################################################################################################

                var sonogram = results.Item1;
                var hits = results.Item2;
                var scores = results.Item3;
                var predictedEvents = results.Item4;
                Console.WriteLine("# Event Count = " + predictedEvents.Count());

                //write event count to results file.            
                //WriteEventsInfo2TextFile(predictedEvents, opPath);
                double displayThreshold = 0.2; //relative position of threhsold in image of score track.
                double normMax = harmonicIntensityThreshold / displayThreshold; //threshold
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
        public static DataTable Analysis(int iter, FileInfo fiSegmentAudioFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir)
        {
            string analysisName = configDict[key_ANALYSIS_NAME];
            double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
            int minHz = Int32.Parse(configDict[key_MIN_HZ]);
            //double minDuration = Double.Parse(configDict[key_MIN_DURATION]);          // lower bound for the duration of an event
            //double maxDuration = Double.Parse(configDict[key_MAX_DURATION]);          // upper bound for the duration of an event
            int DRAW_SONOGRAMS = Int32.Parse(configDict[key_DRAW_SONOGRAMS]);         // options to draw sonogram
            int minFormantgap = Int32.Parse(configDict[key_MIN_FORMANT_GAP]);
            int maxFormantgap = Int32.Parse(configDict[key_MAX_FORMANT_GAP]);
            double decibelThreshold = Double.Parse(configDict[key_DECIBEL_THRESHOLD]); ;   //dB
            double harmonicIntensityThreshold = Double.Parse(configDict[key_HARMONIC_INTENSITY_THRESHOLD]); //in 0-1
            double callDuration = Double.Parse(configDict[key_CALL_DURATION]);  // seconds


            string tempSegmentPath = Path.Combine(diOutputDir.FullName, "temp.wav"); //path location/name of extracted recording segment
            var fiTempSegmentFile = new FileInfo(tempSegmentPath);

            IAudioUtility audioUtility = new MasterAudioUtility();
            var mimeType = MediaTypes.GetMediaType(fiSegmentAudioFile.Extension);
            var sourceDuration = audioUtility.Duration(fiSegmentAudioFile, mimeType); // Get duration of the source file
            int startMilliseconds = 0;
            int endMilliseconds = (int)sourceDuration.TotalMilliseconds;
            Console.WriteLine("\tRecording Duration: {0:f2}seconds", sourceDuration.TotalSeconds);



            MasterAudioUtility.SegmentToWav(RESAMPLE_RATE, fiSegmentAudioFile, new FileInfo(tempSegmentPath), startMilliseconds, endMilliseconds);
            AudioRecording recording = new AudioRecording(tempSegmentPath);

            //#############################################################################################################################################
            var results = Execute_HDDetect(recording, minHz, decibelThreshold, harmonicIntensityThreshold, minFormantgap, maxFormantgap, callDuration); //uses XCORR and FFT
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

            double segmentStartMinute = segmentDuration * iter;

            //draw images of sonograms
            bool saveSonogram = false;
            if ((DRAW_SONOGRAMS == 2) || ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))) saveSonogram = true;
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



        //public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_HDDetect(AudioRecording recording, NoiseReductionType nrt, int frameLength,
        //                                           double frameOverlap, int minHz, int maxHz, int harmonicCount, double amplitudeThreshold, double minDuration, double maxDuration)
        //{
        //    //i: MAKE SONOGRAM
        //    SonogramConfig sonoConfig = new SonogramConfig(); //default values config
        //    sonoConfig.SourceFName = recording.FileName;
        //    sonoConfig.WindowSize = frameLength;
        //    sonoConfig.WindowOverlap = frameOverlap;
        //    sonoConfig.NoiseReductionType = nrt;

        //    BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
        //    recording.Dispose();

        //    //ii: DETECT HARMONICS
        //    var results = HarmonicAnalysis.Execute((SpectralSonogram)sonogram,minHz,maxHz,harmonicCount, amplitudeThreshold);
        //    double[] scores = results.Item1;     //an array of periodicity scores
        //    Double[,] hits = results.Item2;      //hits matrix - to superimpose on sonogram image
        //    // ACOUSTIC EVENTS
        //    List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
        //                                                                                 amplitudeThreshold, minDuration, maxDuration);

        //    return System.Tuple.Create(sonogram, hits, scores, predictedEvents);

        //}//end Execute_HDDetect


        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>> Execute_HDDetect(AudioRecording recording, int minHz, double callThreshold,
                                                                                         double intensityThreshold, int minFormantgap, int maxFormantgap, double callDuration)
        {
            int frameSize = 1024;
            double windowOverlap = 0.0;
            
            //i: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.WindowSize     = frameSize;
            sonoConfig.WindowOverlap  = windowOverlap;
            sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("NONE");
            //sonoConfig.NoiseReductionType = SNR.Key2NoiseReductionType("STANDARD");
            int sr = recording.SampleRate;
            double freqBinWidth = sr / (double)sonoConfig.WindowSize;
            double framesPerSecond = freqBinWidth;

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

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            double[,] subMatrix = MatrixTools.Submatrix(matrix, 0, minBin, (rowCount - 1), maxbin);


            int callSpan = (int)Math.Round(callDuration * framesPerSecond);

            //ii: DETECT HARMONICS
            var results = CrossCorrelation.DetectHarmonicsInSonogramMatrix(subMatrix, callThreshold, callSpan);
            double[] dBArray   = results.Item1;
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

            return System.Tuple.Create(sonogram, hits, intensity, predictedEvents);

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



        //public static Tuple<DataTable, DataTable, bool[]> ProcessCsvFile(FileInfo fiCsvFile)
        //{
        //    AcousticIndices.InitOutputTableColumns(); //initialise just in case have not been before now.
        //    DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
        //    if ((dt == null) || (dt.Rows.Count == 0)) return null;

        //    dt = DataTableTools.SortTable(dt, "count ASC");
        //    List<bool> columns2Display = CrowColumns2Display().ToList();
        //    DataTable processedtable = ProcessDataTableForDisplayOfColumnValues(dt);
        //    return System.Tuple.Create(dt, processedtable, columns2Display.ToArray());
        //} // ProcessCsvFile()


        /// <summary>
        /// takes a data table of indices and converts column values to values in [0,1].
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        //public static DataTable ProcessDataTableForDisplayOfColumnValues(DataTable dt)
        //{
        //    List<double[]> columns = DataTableTools.ListOfColumnValues(dt);
        //    List<double[]> newColumns = new List<double[]>();
        //    for (int i = 0; i < columns.Count; i++)
        //    {
        //        double[] processedColumn = DataTools.normalise(columns[i]); //normalise all values in [0,1]
        //        newColumns.Add(processedColumn);
        //    }
        //    string[] headers = DataTableTools.GetColumnNames(dt);
        //    Type[] types = DataTableTools.GetColumnTypes(dt);
        //    for (int i = 0; i < columns.Count; i++)
        //    {
        //        if (types[i] == typeof(int)) types[i] = typeof(double);
        //        else
        //            if (types[i] == typeof(Int32)) types[i] = typeof(double);
        //    }
        //    var processedtable = DataTableTools.CreateTable(headers, types, newColumns);

        //    return processedtable;
        //}




    } //end class Crow2
}
