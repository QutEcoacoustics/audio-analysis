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
    public class GratingDetection
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

        public const string key_START_FRAME = "startFrame";
        public const string key_END_FRAME = "endFrame";
        //public const string key_FRAME_COUNT = "frameCount";
        //public const string key_START_SECOND = "startSecond";
        //public const string key_END_SECOND = "endSecond";
        public const string key_MIN_FREQBIN = "minFreqBin";
        public const string key_MAX_FREQBIN = "maxFreqBin";
        //public const string key_MIN_FREQ = "minFreq";
        //public const string key_MAX_FREQ = "maxFreq";
        public const string key_SCORE = "score";
        public const string key_PERIODICITY = "periodicity";
        



        //KEYS TO OUTPUT INDICES
        public static string key_COUNT     = "count";
        public static string key_START_ABS = "EvStartAbs";
        public static string key_START_MIN = "EvStartMin";
        public static string key_START_SEC = "EvStartSec";
        public static string key_CALL_DENSITY = "CallDensity";
        public static string key_CALL_SCORE   = "CallScore";

        //INITIALISE OUTPUT TABLE OF EVENTS
        private const  int    EVENT_COL_NUMBER = 6;
        private static Type[] EVENT_COL_TYPES = new Type[EVENT_COL_NUMBER];
        private static string[] EVENT_HEADERS = new string[EVENT_COL_NUMBER];
        private static System.Tuple<string[], Type[]> GetTableHeadersAndTypesForEvents()
        {
            EVENT_HEADERS[0] = key_START_ABS;        EVENT_COL_TYPES[0] = typeof(int);
            EVENT_HEADERS[1] = key_START_MIN;        EVENT_COL_TYPES[1] = typeof(int);
            EVENT_HEADERS[2] = key_START_SEC;        EVENT_COL_TYPES[2] = typeof(int);
            EVENT_HEADERS[3] = key_SEGMENT_DURATION; EVENT_COL_TYPES[3] = typeof(double);
            EVENT_HEADERS[4] = key_CALL_DENSITY;     EVENT_COL_TYPES[4] = typeof(int);
            EVENT_HEADERS[5] = key_CALL_SCORE;       EVENT_COL_TYPES[5] = typeof(double); 
            return Tuple.Create(EVENT_HEADERS,       EVENT_COL_TYPES);
        }
        //INITIALISE OUTPUT TABLE OF INDICES
        private const int INDICES_COL_NUMBER = 7;
        private static Type[] INDICES_COL_TYPES = new Type[INDICES_COL_NUMBER];
        private static string[] INDICES_HEADERS = new string[INDICES_COL_NUMBER];
        private static System.Tuple<string[], Type[]> GetTableHeadersAndTypesForIndices()
        {
            INDICES_HEADERS[0] = key_COUNT;      INDICES_COL_TYPES[0] = typeof(int);
            INDICES_HEADERS[1] = key_START_ABS;  INDICES_COL_TYPES[1] = typeof(int);
            INDICES_HEADERS[2] = key_START_MIN;  INDICES_COL_TYPES[2] = typeof(int);
            INDICES_HEADERS[3] = key_START_SEC;  INDICES_COL_TYPES[3] = typeof(int);
            INDICES_HEADERS[4] = key_SEGMENT_DURATION; INDICES_COL_TYPES[4] = typeof(double);
            INDICES_HEADERS[5] = key_CALL_DENSITY; INDICES_COL_TYPES[5] = typeof(int);
            INDICES_HEADERS[6] = key_CALL_SCORE; INDICES_COL_TYPES[6] = typeof(double);
            return Tuple.Create(INDICES_HEADERS, INDICES_COL_TYPES);
        }

        //OTHER CONSTANTS
        public const string ANALYSIS_NAME = "Grids";
        public const int RESAMPLE_RATE = 17640;
        //public const int RESAMPLE_RATE = 22050;

        public static void Dev(string[] args)
        {
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\DM420036_min173Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\DM420036_min449Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\DM420036_min700Airplane.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\KAPITI2_20100219_202900_min48AirplaneAndBirds.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\DM420036_min302MorningChorus.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\Honeymoon_Bay_St_Bees_KoalaDistant_20080914-213000.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Grids\BAC2_20071005-145040.wav";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Crows_Cassandra\Crows111216-001Mono5-7min.mp3";
            //string recordingPath = @"C:\SensorNetworks\WavFiles\Human\DM420036_min465Speech.wav";
            string recordingPath = @"C:\SensorNetworks\Software\AudioAnalysis\AudioBrowser\bin\Debug\Audio-samples\Wimmer_DM420011.wav";
    
            string configPath = @"C:\SensorNetworks\Output\Grids\Grids.cfg";
            string outputDir  = @"C:\SensorNetworks\Output\Grids\";

            string opFName       = ANALYSIS_NAME + ".txt";
            string opPath        = outputDir + opFName;
            string audioFileName = Path.GetFileName(recordingPath);
            Log.Verbosity = 1;

            string title = "# FOR DETECTION OF ACOUSTIC EVENTS HAVING A GRID OR GRATING STRUCTURE";
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
                Log.WriteLine("\n\n\n##############################\n WARNING! No events returned.");
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
            var results = Analysis(fiSegmentOfSourceFile, configDict, diOutputDir, opFileName);
            //######################################################################

            if (results == null) return null;
            var sonogram = results.Item1;
            var hits = results.Item2;
            var scores = results.Item3;
            var predictedEvents = results.Item4;
            var recordingTimeSpan = results.Item5;


            double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
            double segmentStartMinute = segmentDuration * iter;
            DataTable dataTable = null;

            if ((predictedEvents == null) || (predictedEvents.Count == 0))
            {
                Console.WriteLine("############ WARNING: No acoustic events were returned from the analysis.");
            }
            else
            {
                string analysisName = configDict[key_ANALYSIS_NAME];
                string fName = Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.Name);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = fName;
                    //ev.Name = analysisName; //name is the periodicity
                    ev.SourceFileDuration = recordingTimeSpan.TotalSeconds;
                }
                //write events to a data table to return.
                dataTable = WriteEvents2DataTable(segmentStartMinute, recordingTimeSpan, predictedEvents);
                string sortString = key_START_ABS + " ASC";
                dataTable = DataTableTools.SortTable(dataTable, sortString); //sort by start time before returning
            }


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

            return dataTable;
        }




        public static Image AnalysisReturnsSonogram(int iter, FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir)
        {
            double segmentDuration = Double.Parse(configDict[key_SEGMENT_DURATION]);
            double segmentStartMinute = segmentDuration * iter;
            string newFileNameWithoutExtention = Path.GetFileNameWithoutExtension(fiSegmentOfSourceFile.FullName) + "_" + (int)segmentStartMinute + "min";
            string opFileName = newFileNameWithoutExtention + ".wav";

            //######################################################################
            var results = Analysis(fiSegmentOfSourceFile, configDict, diOutputDir, opFileName);
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
        /// Does the Analysis
        /// Returns a DataTable
        /// </summary>
        /// <param name="config"></param>
        /// <param name="segmentAudioFile"></param>
        public static System.Tuple<BaseSonogram, Double[,], double[], List<AcousticEvent>, TimeSpan> 
                                        Analysis(FileInfo fiSegmentOfSourceFile, Dictionary<string, string> configDict, DirectoryInfo diOutputDir, string opFileName)
        {
            //set default values
            int bandWidth = 500; //detect bars in bands of this width.
            int frameSize = 1024;
            double windowOverlap = 0.0;
            double intensityThreshold = Double.Parse(configDict[key_INTENSITY_THRESHOLD]);
            //intensityThreshold = 0.01;

            AudioRecording recording = AudioRecording.GetAudioRecording(fiSegmentOfSourceFile, RESAMPLE_RATE, diOutputDir.FullName, opFileName);
            if (recording == null)
            {
                Console.WriteLine("############ WARNING: Recording could not be obtained - likely file does not exist.");
                return null;
            }
            int sr = recording.SampleRate;
            double binWidth = recording.SampleRate / (double)frameSize;
            double frameDuration = frameSize / (double)sr; 
            double frameOffset   = frameDuration * (1 - windowOverlap); //seconds between start of each frame
            double framesPerSecond = 1 / frameOffset;
            TimeSpan tsRecordingtDuration = recording.Duration();
            int colStep = (int)Math.Round(bandWidth / binWidth);


            //i: GET SONOGRAM AS MATRIX
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, sr, frameSize, windowOverlap);
            double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            //double[] envelope = results2.Item2;
            double[,] spectrogram = results2.Item3;  //amplitude spectrogram. Note that column zero is the DC or average energy value and can be ignored.
            double windowPower = results2.Item4;

            //############################ NEXT LINE FOR DEBUGGING ONLY
            //spectrogram = GetTestSpectrogram(spectrogram.GetLength(0), spectrogram.GetLength(1), 0.01, 0.03);

            var output = DetectGratingEvents(spectrogram, colStep, intensityThreshold);
            var amplitudeArray = output.Item2; //for debug purposes only

            //convert List of Dictionary events to List of ACousticevents.
            //also set up the hits matrix.
            int rowCount = spectrogram.GetLength(0);
            int colCount = spectrogram.GetLength(1);
            var hitsMatrix = new double[rowCount, colCount];
            var acousticEvents = new List<AcousticEvent>();

            double minFrameCount = 8; //this assumes that the minimum grid is 2 * 4 = 8 long
            foreach (Dictionary<string, double> item in output.Item1)
            {
                int minRow = (int)item[key_START_FRAME];
                int maxRow = (int)item[key_END_FRAME];
                int frameCount = maxRow - minRow + 1;
                if (frameCount < minFrameCount) continue; //only want events that are over a minimum length

                int minCol = (int)item[key_MIN_FREQBIN];
                int maxCol = (int)item[key_MAX_FREQBIN];
                double periodicity = item[key_PERIODICITY];

                double[] subarray = DataTools.Subarray(avAbsolute, minRow, maxRow - minRow + 1);
                double severity = 0.1;
                int[] bounds = DataTools.Peaks_CropToFirstAndLast(subarray, severity);
                minRow = minRow + bounds[0];
                maxRow = minRow + bounds[1];
                if (maxRow >= rowCount) maxRow = rowCount-1;

                Oblong o = new Oblong(minRow, minCol, maxRow, maxCol);
                var ae = new AcousticEvent(o, frameOffset, binWidth);
                ae.Name = String.Format("p={0:f0}", periodicity);
                ae.Score = item[key_SCORE];
                ae.ScoreNormalised = item[key_SCORE] / 0.5;
                acousticEvents.Add(ae);

                //display event on the hits matrix
                for (int r = minRow; r < maxRow; r++)
                    for (int c = minCol; c < maxCol; c++)
                    {
                        hitsMatrix[r, c] = periodicity;
                    }

            } //foreach

            //set up the songogram to return. Use the existing amplitude sonogram
            int bitsPerSample = recording.GetWavReader().BitsPerSample;
            //NoiseReductionType nrt = SNR.Key2NoiseReductionType("NONE");
            NoiseReductionType nrt = SNR.Key2NoiseReductionType("STANDARD");
            var sonogram = (BaseSonogram)SpectralSonogram.GetSpectralSonogram(recording.FileName, frameSize, windowOverlap, bitsPerSample, windowPower, sr, tsRecordingtDuration, nrt, spectrogram);
            sonogram.DecibelsNormalised = new double[sonogram.FrameCount];
            for (int i = 0; i < sonogram.FrameCount; i++) //foreach frame or time step
            {
                sonogram.DecibelsNormalised[i] = 2 * Math.Log10(avAbsolute[i]);
            }
            sonogram.DecibelsNormalised = DataTools.normalise(sonogram.DecibelsNormalised);

            return System.Tuple.Create(sonogram, hitsMatrix, amplitudeArray, acousticEvents, tsRecordingtDuration);
        } //Analysis()


        public static System.Tuple<List<Dictionary<string, double>>, double[]> DetectGratingEvents(double[,] matrix, int colStep, double intensityThreshold)
        {
            bool doNoiseremoval = true;
            int minPeriod = 2;    //both period values must be even numbers
            int maxPeriod = 20;   //Note: 17.2 frames per second i.e. period=20 is just over 1s.
            int numberOfCycles = 4; 
            int step = 1;

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int numberOfColSteps = colCount / colStep;

            var events2return = new List<Dictionary<string, double>>();
            double[] array2return = null;

            for (int b = 0; b < numberOfColSteps; b++)
            {
                int minCol = (b * colStep);
                int maxCol = minCol + colStep - 1;

                double[,] subMatrix = MatrixTools.Submatrix(matrix, 0, minCol, (rowCount - 1), maxCol);
                double[] amplitudeArray = MatrixTools.GetRowAverages(subMatrix);

                if (doNoiseremoval)
                {
                    double Q, oneSD;
                    amplitudeArray = SNR.NoiseSubtractMode(amplitudeArray, out Q, out oneSD);
                }

                //var events = CrossCorrelation.DetectBarsEventsBySegmentationAndXcorrelation(amplitudeArray, intensityThreshold);
                
                var scores           = Gratings.ScanArrayForGratingPattern(amplitudeArray, minPeriod, maxPeriod, numberOfCycles, step);
                var mergedOutput     = Gratings.MergePeriodicScoreArrays(scores, minPeriod, maxPeriod);
                double[] intensity   = mergedOutput.Item1; 
                double[] periodicity = mergedOutput.Item2;
                var events = Gratings.ExtractPeriodicEvents(intensity, periodicity, intensityThreshold);
                
                foreach (Dictionary<string, double> item in events)
                {
                    item[key_MIN_FREQBIN] = minCol;
                    item[key_MAX_FREQBIN] = maxCol;
                    events2return.Add(item);
                }
                
                if (b == 3) array2return = amplitudeArray; //returned for debugging purposes only
            } //for loop over bands of columns

            return System.Tuple.Create(events2return, array2return);
        }//end DetectGratingEvents()


        static Image DrawSonogram(BaseSonogram sonogram, double[,] hits, double[] scores, List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            //Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            double maxScore = 32.0; //assume max period = 64.
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


        public static DataTable WriteEvents2DataTable(double segmentStartMinute, TimeSpan tsSegmentDuration, List<AcousticEvent> predictedEvents)
        {
            if ((predictedEvents == null) || (predictedEvents.Count == 0)) return null;

            GetTableHeadersAndTypesForEvents();
            var dataTable = DataTableTools.CreateTable(EVENT_HEADERS, EVENT_COL_TYPES);
            //int count = 0;
            foreach (var ev in predictedEvents)
            {
                int segmentStartSec = (int)(segmentStartMinute * 60);
                int eventStartAbsoluteSec = (int)(segmentStartSec + ev.TimeStart);
                int eventStartMin = eventStartAbsoluteSec / 60;
                int eventStartSec = eventStartAbsoluteSec % 60;
                string segmentDuration = DataTools.Time_ConvertSecs2Mins(tsSegmentDuration.TotalSeconds);

                DataRow row = dataTable.NewRow();
                row[key_START_ABS]    = eventStartAbsoluteSec;   //EvStartAbsolute - from start of source ifle
                row[key_START_MIN]    = eventStartMin;           //EvStartMin
                row[key_START_SEC]    = eventStartSec;           //EvStartSec
                row[key_SEGMENT_DURATION] = tsSegmentDuration.TotalSeconds; //segment Duration in seconds
                row[key_CALL_DENSITY] = predictedEvents.Count;   //Density
                row[key_CALL_SCORE]   = ev.Score;                //Score
                dataTable.Rows.Add(row);
               // count++;
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



        //public static List<Dictionary<string, double>> ExtractMyPeriodicEvents(double[] intensity, double[] periodicity, double intensityThreshold)
        //{
        //    //could do a possible adjustment of the threshold for period.
        //    //double adjustedThreshold = intensityThreshold * factor;  //adjust threshold to period. THis is a correction for pink noise
        //    var events = DataTools.SegmentArrayOnThreshold(intensity, intensityThreshold);

        //    var list = new List<Dictionary<string, double>>();
        //    foreach (double[] item in events)
        //    {
        //        var ev = new Dictionary<string, double>();
        //        ev[key_START_FRAME] = item[0];
        //        ev[key_END_FRAME] = item[1];
        //        ev[key_SCORE] = item[2];
        //        double cyclePeriod = 0.0;
        //        for (int n = (int)item[0]; n <= (int)item[1]; n++) cyclePeriod += periodicity[n];
        //        ev[key_PERIODICITY] = cyclePeriod / (item[1] - item[0] + 1);
        //        list.Add(ev);
        //    } //foreach
        //    return list;
        //} //ExtractPeriodicEvents()



        //public static List<Dictionary<string, double>> ExtractPeriodicEvents(double[] intensity, double[] periodicity, double intensityThreshold)
        //{
        //    //could do a possible adjustment of the threshold for period.
        //    //double adjustedThreshold = intensityThreshold * factor;  //adjust threshold to period. THis is a correction for pink noise
        //    var events = DataTools.SegmentArrayOnThreshold(intensity, intensityThreshold);

        //    var list = new List<Dictionary<string, double>>();
        //    foreach (double[] item in events)
        //    {
        //        var ev = new Dictionary<string, double>();
        //        ev[key_START_FRAME] = item[0];
        //        ev[key_END_FRAME] = item[1];
        //        ev[key_SCORE] = item[2];
        //        double cyclePeriod = 0.0;
        //        for (int n = (int)item[0]; n <= (int)item[1]; n++) cyclePeriod += periodicity[n];
        //        ev[key_PERIODICITY] = cyclePeriod / (item[1] - item[0] + 1);
        //        list.Add(ev);
        //    } //foreach
        //    return list;
        //} //ExtractPeriodicEvents()



    } //end class GratingDetection
}
