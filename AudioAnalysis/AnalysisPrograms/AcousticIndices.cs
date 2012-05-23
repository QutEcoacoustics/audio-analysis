using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Drawing;

using TowseyLib;
using NeuralNets;
using AudioAnalysisTools;

namespace AnalysisPrograms
{

    public class AcousticIndices
    {
        public const string ANALYSIS_NAME = "Indices"; 
        public const double DEFAULT_activityThreshold_dB = 3.0; //used to select frames that have 3dB > background
        public const int    DEFAULT_WINDOW_SIZE = 256;
        private const int    COL_NUMBER = 18;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
        private static double[] COMBO_WEIGHTS = new double[COL_NUMBER];

        public const string header_count = "count";
        public const string header_startMin = "start-min";
        public const string header_SecondsDuration = "sec-dur";
        public const string header_avAmpdB  = "avAmp-dB";
        public const string header_snrdB    = "snr-dB";   
        public const string header_bgdB     = "bg-dB";
        public const string header_activity = "activity";
        public const string header_segCount = "segCount";
        public const string header_avSegDur = "avSegDur";
        public const string header_hfCover  = "hfCover";
        public const string header_mfCover  = "mfCover";
        public const string header_lfCover  = "lfCover";
        public const string header_HAmpl    = "H[ampl]";
        public const string header_HPeakFreq = "H[peakFreq]";
        public const string header_HAvSpectrum = "H[avSpectrum]";
        public const string header_HVarSpectrum = "H[varSpectrum]";
        public const string header_NumClusters = "#clusters";
        public const string header_avClustDur = "avClustDur";


        public static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        {
            HEADERS[0] = header_count;        COL_TYPES[0] = typeof(int);    DISPLAY_COLUMN[0] = false; COMBO_WEIGHTS[0] = 0.0;
            HEADERS[1] = header_startMin;     COL_TYPES[1] = typeof(double); DISPLAY_COLUMN[1] = false; COMBO_WEIGHTS[1] = 0.0;
            HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false; COMBO_WEIGHTS[2] = 0.0;
            HEADERS[3] = header_avAmpdB;      COL_TYPES[3] = typeof(double); DISPLAY_COLUMN[3] = true;  COMBO_WEIGHTS[3] = 0.0;
            HEADERS[4] = header_snrdB;        COL_TYPES[4] = typeof(double); DISPLAY_COLUMN[4] = true;  COMBO_WEIGHTS[4] = 0.0;
            HEADERS[5] = header_bgdB;         COL_TYPES[5] = typeof(double); DISPLAY_COLUMN[5] = true;  COMBO_WEIGHTS[5] = 0.0;
            HEADERS[6] = header_activity;     COL_TYPES[6] = typeof(double); DISPLAY_COLUMN[6] = true;  COMBO_WEIGHTS[6] = 0.0;
            HEADERS[7] = header_segCount;     COL_TYPES[7] = typeof(int);    DISPLAY_COLUMN[7] = true;  COMBO_WEIGHTS[7] = 0.0;
            HEADERS[8] = header_avSegDur;     COL_TYPES[8] = typeof(double); DISPLAY_COLUMN[8] = true;  COMBO_WEIGHTS[8] = 0.0;
            HEADERS[9] = "hfCover";      COL_TYPES[9] = typeof(double);  DISPLAY_COLUMN[9] = true;      COMBO_WEIGHTS[9] = 0.0;
            HEADERS[10] = "mfCover";     COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true;     COMBO_WEIGHTS[10] = 0.0;
            HEADERS[11] = "lfCover";     COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true;     COMBO_WEIGHTS[11] = 0.0;
            HEADERS[12] = "H[ampl]";     COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = true;     COMBO_WEIGHTS[12] = 0.0;
            HEADERS[13] = "H[peakFreq]"; COL_TYPES[13] = typeof(double); DISPLAY_COLUMN[13] = false;    COMBO_WEIGHTS[13] = 0.0;
            HEADERS[14] = "H[avSpectrum]"; COL_TYPES[14] = typeof(double); DISPLAY_COLUMN[14] = true;   COMBO_WEIGHTS[14] = 0.4;
            HEADERS[15] = "H[varSpectrum]"; COL_TYPES[15] = typeof(double); DISPLAY_COLUMN[15] = false; COMBO_WEIGHTS[15] = 0.1;
            HEADERS[16] = "#clusters";   COL_TYPES[16] = typeof(int);    DISPLAY_COLUMN[16] = true;     COMBO_WEIGHTS[16] = 0.4;
            HEADERS[17] = header_avClustDur; COL_TYPES[17] = typeof(double); DISPLAY_COLUMN[17] = true; COMBO_WEIGHTS[17] = 0.1;
            //HEADERS[18] = "Weighted index"; COL_TYPES[18] = typeof(double); DISPLAY_COLUMN[18] = false; COMBO_WEIGHTS[18] = 0.0;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }

        public static bool[] GetDisplayColumns()
        {
            return DISPLAY_COLUMN;
        }
        
        public static string FORMAT_STR_HEADERS = "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}";
        public static string FORMAT_STR_DATA    = "{1}{0}{2:f1}{0}{3:f3}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8}{0}{9:f2}{0}{10:f4}{0}{11:f4}{0}{12:f4}{0}{13:f4}{0}{14:f4}{0}{15:f4}{0}{16}{0}{17}{0}{18}";


        //Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public static string key_ANALYSIS_NAME    = "ANALYSIS_NAME";
        public static string key_SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string key_SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";
        public static string key_RESAMPLE_RATE    = "RESAMPLE_RATE";
        public static string key_FRAME_LENGTH     = "FRAME_LENGTH";
        public static string key_FRAME_OVERLAP    = "FRAME_OVERLAP";
        public static string key_LOW_FREQ_BOUND   = "LOW_FREQ_BOUND";
        public static string key_MID_FREQ_BOUND   = "MID_FREQ_BOUND";
        public static string key_DRAW_SONOGRAMS   = "DRAW_SONOGRAMS";
        public static string key_DISPLAY_COLUMNS  = "DISPLAY_COLUMNS";
        //public static string key_REPORT_FORMAT    = "REPORT_FORMAT";
        public static string key_STORE_INTERMEDATE_RESULTS = "STORE_INTERMEDATE_RESULTS";
        public static string key_DO_NOISE_REDUCTION = "DO_NOISE_REDUCTION"; // used when displaying sonograms
        public static string key_BG_NOISE_REDUCTION = "BG_NOISE_REDUCTION"; // used when displaying sonograms - backgroud noise reduction severity in dB.


        /// Following is list of scaling originally applied to the Acoustic Indices Tracks
        /// Amplitude track 2:              title = "1: av Sig Ampl(dB)" minDB = -50; maxDB = -20; 
        /// Background dB track 3:          title = "2: Background(dB)"  minDB = -50; maxDB = -20;
        /// SNR track 4:                    title = "3: SNR"             minDB = 0;   maxDB = 30;
        /// draw activity track 5:          title = "4: Activity(>3dB)"; min = 0.0;   max = 0.4;
        /// Segment count track 6:          title = "5: # Segments";     threshold = 1.0;
        /// avSegment Duration track 7:     title = "6: Av Seg Duration"; min = 0.0; max = 100; //milliseconds
        /// percent spectral Cover track 8: title = "7: Spectral cover"; min = 0.0;  max = 0.5; threshold = 0.05;
        /// percent spectral Cover track 9: title = "8: Low freq cover"; min = 0.0;  max = 1.0; threshold = 0.1;
        /// Spectral Cover track 10:        title = "9: H(ampl)";        min = 0.95; max = 1.0; threshold = 0.96;
        /// H(PeakFreq) track 11:           title = "10: H(PeakFreq)";   min & max = min and max
        /// H(avSpect) track 12             title = "11: H(avSpect)";    min & max = min and max
        /// H(diffSpect) track 13:          title = "12: H(varSpect)";   min & max = min and max
        /// clusterCount track 14:          title = "13: ClusterCount";  min = 0.0;  max = 15.0;  threshold = 1.0;
        /// av Cluster Duration track 15:   title = "14: Av Cluster Dur";  min = 0.0;  max = 100.0;  threshold = 5.0;
        /// weightedIndex track 16:         title = "15: Weighted Index";  






        /// <summary>
        /// a set of parameters derived from ini file
        /// </summary>
        public struct Parameters
        {
            public int frameLength, resampleRate, lowFreqBound, segmentOverlap;
            public double segmentDuration, frameOverlap;
            //public int DRAW_SONOGRAMS;
            //public string reportFormat;

            public Parameters(double _segmentDuration, int _segmentOverlap, int _resampleRate,
                              int _frameLength, int _frameOverlap, int _lowFreqBound, int _DRAW_SONOGRAMS, string _fileFormat)
            {
                segmentDuration = _segmentDuration;
                segmentOverlap  = _segmentOverlap;
                resampleRate    = _resampleRate;
                frameLength     = _frameLength;
                frameOverlap    = _frameOverlap;
                lowFreqBound    = _lowFreqBound;
                //DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
                //reportFormat    = _fileFormat;
            } //Parameters
        } //struct Parameters

        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices2
        {
            public double snr, bgNoise, activity, avSegmentDuration, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover, entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfVarianceSpectrum, avClusterDuration; //spectral indices
            public int    segmentCount, clusterCount;

            public Indices2(double _snr, double _bgNoise, double _activity, double _avSegmentLength, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, int _clusterCount, int _avClusterDuration)
            {
                snr        = _snr;
                bgNoise    = _bgNoise;
                activity   = _activity;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentLength;
                avSig_dB   = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover   = _hiFreqCover;
                midFreqCover  = _midFreqCover;
                lowFreqCover  = _lowFreqCover;
                entropyOfPeakFreqDistr = _peakFreqEntropy;
                entropyOfAvSpectrum   = _entropyOfAvSpectrum;
                entropyOfVarianceSpectrum = _entropyOfVarianceSpectrum;
                clusterCount = _clusterCount;
                avClusterDuration = _avClusterDuration; //av length of clusters > 1 frame.
            }
        } //struct Indices2

        public static AcousticIndices.Parameters ReadIniFile(string iniPath, int verbosity)
        {
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            AcousticIndices.Parameters paramaters; // st
            paramaters.segmentDuration = Double.Parse(dict[AcousticIndices.key_SEGMENT_DURATION]);
            paramaters.segmentOverlap = Int32.Parse(dict[AcousticIndices.key_SEGMENT_OVERLAP]);
            paramaters.resampleRate = Int32.Parse(dict[AcousticIndices.key_RESAMPLE_RATE]);
            paramaters.frameLength = Int32.Parse(dict[AcousticIndices.key_FRAME_LENGTH]);
            paramaters.frameOverlap = Double.Parse(dict[AcousticIndices.key_FRAME_OVERLAP]);
            paramaters.lowFreqBound = Int32.Parse(dict[AcousticIndices.key_LOW_FREQ_BOUND]);
            //paramaters.DRAW_SONOGRAMS = Int32.Parse(dict[AcousticIndices.key_DRAW_SONOGRAMS]);    //options to draw sonogram
            //paramaters.reportFormat = dict[AcousticIndices.key_REPORT_FORMAT];                    //options are TAB or COMMA separator 

            if (verbosity > 0)
            {
                Log.WriteLine("# PARAMETER SETTINGS:");
                Log.WriteLine("Segment size: Duration = {0} minutes;  Overlap = {1} seconds.", paramaters.segmentDuration, paramaters.segmentOverlap);
                Log.WriteLine("Resample rate: {0} samples/sec.  Nyquist: {1} Hz.", paramaters.resampleRate, (paramaters.resampleRate / 2));
                Log.WriteLine("Frame Length: {0} samples.  Fractional overlap: {1}.", paramaters.frameLength, paramaters.frameOverlap);
                Log.WriteLine("Low Freq Bound: {0} Hz.", paramaters.lowFreqBound);
                //Log.WriteLine("Report format: {0}     Draw sonograms: {1}", paramaters.reportFormat, paramaters.DRAW_SONOGRAMS);
                Log.WriteLine("####################################################################################");
            }
            return paramaters;
        }




        public static void Dev(string[] args)
        {
            string title = "# SOFTWARE TO EXTRACT ACOUSTIC INDICES FROM SUNSHINE COAST DATA";
            DateTime datetime = DateTime.Now;
            string date = "# DATE AND TIME: " + datetime;
            Log.WriteLine(title);
            Log.WriteLine(date);

            //SET VERBOSITY
            Log.Verbosity = 1;
            bool doStoreImages = true;
            string reportFormat = "CSV";

            //READ CSV FILE TO MASSAGE DATA
            if (false)
            {
                MASSAGE_CSV_DATA();
                Console.ReadLine();
                Environment.Exit(666);
            }

            //READ CSV FILE TO MASSAGE DATA
            if (false)
            {
                //string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Exp4\Oct13_Results.csv";
                string csvFileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\SE_5days.csv";


                //VISUALIZE_CSV_DATA(csvFileName);  //THIS METHOD NOW DELETED
                Console.ReadLine();
                Environment.Exit(666);
            }


            //i: Set up the dir and file names
            string recordingDir  = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Exp1\";
            string imagePath     = @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\wtsmatrix.png";

            var fileList         = Directory.GetFiles(recordingDir, "*.wav");
            string recordingPath = fileList[0]; //get just one from list
            string fileName      = Path.GetFileName(recordingPath);
            string outputDir     = recordingDir;

            Log.WriteLine("Directory:          " + recordingDir);
            Log.WriteLine("Directory contains: " + fileList.Count() + " wav files.");
            Log.WriteLine("Selected file:      " + fileName);
            Log.WriteLine("Output folder:      " + outputDir);
            string opFileName = "Results_ARI_" + FileTools.TimeStamp2FileName(datetime) + ".csv";
            string opPath = outputDir + opFileName; // .csv file

            //write header to results file
            if (!File.Exists(opPath)) // if file does not exist already, create the file and write a HEADER .
            {
                WriteHeaderToReportFile(opPath, reportFormat);
            }


            //i GET RECORDING
            //int resampleRate = 17640;
            //AudioRecording recording = AudioRecording.GetAudioRecording(recordingPath, resampleRate);
            //double recordingDuration = recording.GetWavReader().Time.TotalSeconds;

            //ii: EXTRACT INDICES 
            Dictionary<string, string> dict = new Dictionary<string, string>();  //set up the default parameters
            dict.Add(key_FRAME_LENGTH, AcousticIndices.DEFAULT_WINDOW_SIZE.ToString());
            dict.Add(key_LOW_FREQ_BOUND, "500");
            dict.Add(key_LOW_FREQ_BOUND, "3500");
            //dict.Add(key_RESAMPLE_RATE, resampleRate.ToString());
            int iterationNumber = 1;
            var fiRecording = new FileInfo(recordingPath);
            dict.Add(key_STORE_INTERMEDATE_RESULTS, "false");
            var results = Analysis(iterationNumber, fiRecording, dict);

            Log.WriteLine("# Finished everything!");
            Console.ReadLine();
        } //DEV()



        //#########################################################################################################################################################



        /// <summary>
        /// Extracts indices from a single  segment of recording
        /// EXTRACT INDICES   Default frameLength = 128 samples @ 22050 Hz = 5.805ms, @ 11025 Hz = 11.61ms.
        //  EXTRACT INDICES   Default frameLength = 256 samples @ 22050 Hz = 11.61ms, @ 11025 Hz = 23.22ms, @ 17640 Hz = 18.576ms.
        /// </summary>
        /// <param name="recording">an audio recording</param>
        /// <param name="int frameSize">number of signal samples in frame. Default = 256</param>
        /// <param name="int lowFreqBound">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        public static DataTable Analysis(int iter, FileInfo fiSegmentAudioFile, Dictionary<string, string> config)
        {
            //get parameters for the analysis
            int frameSize = AcousticIndices.DEFAULT_WINDOW_SIZE;
            if (config.ContainsKey(key_FRAME_LENGTH)) frameSize      = Configuration.GetInt(key_FRAME_LENGTH, config); 
            int lowFreqBound = 500; //default value
            if (config.ContainsKey(key_LOW_FREQ_BOUND)) lowFreqBound = Configuration.GetInt(key_LOW_FREQ_BOUND, config); 
            int midFreqBound = 3500;
            if (config.ContainsKey(key_MID_FREQ_BOUND)) midFreqBound = Configuration.GetInt(key_MID_FREQ_BOUND, config);
            double segmentDuration                                   = Configuration.GetDouble(LSKiwi.key_SEGMENT_DURATION, config);
            double segmentStartMinute = segmentDuration * iter;
            double windowOverlap                                     = Configuration.GetDouble(AcousticIndices.key_FRAME_OVERLAP, config);

            //get recording segment
            AudioRecording recording = new AudioRecording(fiSegmentAudioFile.FullName);
            int signalLength                  = recording.GetWavReader().Samples.Length;
            double wavSegmentDuration_seconds = recording.GetWavReader().Time.TotalSeconds;
            double frameDuration              = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;


            //i: EXTRACT ENVELOPE and FFTs
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            //double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[] envelope   = results2.Item2;
            double[,] spectrogram = results2.Item3;  //amplitude spectrogram


            //ii: FRAME ENERGIES - 
            var results3 = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(envelope));//use Lamel et al. Only search in range 10dB above min dB.
            var dBarray  = SNR.TruncateNegativeValues2Zero(results3.Item1);


            bool[] activeFrames = new bool[dBarray.Length]; //record frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++) if (dBarray[i] >= AcousticIndices.DEFAULT_activityThreshold_dB) activeFrames[i] = true;
            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB)); 
            int activeFrameCount = DataTools.CountTrues(activeFrames);

            Indices2 indices; // struct in which to store all indices
            indices.activity = activeFrameCount / (double)dBarray.Length;   //fraction of frames having acoustic activity 
            indices.bgNoise  = results3.Item2;                              //bg noise in dB
            indices.snr      = results3.Item5;                              //snr
            indices.avSig_dB = 20 * Math.Log10(envelope.Average());         //10 times log of amplitude squared 
            indices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(envelope)); //ENTROPY of ENERGY ENVELOPE

            //calculate boundary between hi and low frequency spectrum
            double binWidth = recording.Nyquist / (double)spectrogram.GetLength(1);
            int excludeLoFreqBins = (int)Math.Ceiling(lowFreqBound / binWidth);

            //iii: ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM - at this point the spectrogram is still an amplitude spectrogram
            var tuple = CalculateEntropyOfSpectralAvAndVariance(spectrogram, excludeLoFreqBins);
            indices.entropyOfAvSpectrum = tuple.Item1;
            indices.entropyOfVarianceSpectrum = tuple.Item2;

            //iv: remove background noise from the spectrogram
            double spectralBgThreshold = 0.015;      // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            double[] modalValues = SNR.CalculateModalValues(spectrogram); //calculate modal value for each freq bin.
            modalValues = DataTools.filterMovingAverage(modalValues, 7);  //smooth the modal profile
            spectrogram = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogram, modalValues);
            spectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogram, spectralBgThreshold);

            //v: SPECTROGRAM ANALYSIS - SPECTRAL COVER. NOTE: spectrogram is still a noise reduced amplitude spectrogram
            var tuple3 = CalculateSpectralCoverage(spectrogram, spectralBgThreshold, lowFreqBound, midFreqBound, recording.Nyquist);
            indices.lowFreqCover = tuple3.Item1;
            indices.midFreqCover = tuple3.Item2;
            indices.hiFreqCover  = tuple3.Item3;


            //#V#####################################################################################################################################################
            //#V#####################################################################################################################################################

            if (activeFrameCount <= 2)   //return if activeFrameCount too small
            {
                indices.segmentCount = 0;
                indices.avSegmentDuration = 0.0;
                indices.entropyOfPeakFreqDistr = 0.0;
                indices.clusterCount = 0;
                indices.avClusterDuration = 0.0; //av cluster durtaion in milliseconds

                return Indices2DataTable(iter, segmentStartMinute, wavSegmentDuration_seconds, indices);
            }
            //#V#####################################################################################################################################################

            //vi: SEGMENT STATISTICS: COUNT and AVERAGE LENGTH
            var tuple4 = CalculateSegmentCount(activeFrames, frameDuration);
            indices.segmentCount = tuple4.Item1;      //number of segments whose duration > one frame
            indices.avSegmentDuration = tuple4.Item2; //av segment duration in milliseconds
            bool[] activeSegments = tuple4.Item3;     //boolean array that stores location of frames in active segments


            //#V#####################################################################################################################################################
            if (indices.segmentCount == 0)  //return if segmentCount = 0
            {
                indices.avSegmentDuration = 0.0;
                indices.entropyOfPeakFreqDistr = 0.0;
                indices.clusterCount = 0;
                indices.avClusterDuration = 0.0; //av cluster duration in milliseconds
                return Indices2DataTable(iter, segmentStartMinute, wavSegmentDuration_seconds, indices);
            }
            //#V#####################################################################################################################################################


            //vii: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.   NOTE: spectrogram is still a noise reduced amplitude spectrogram 
            double peakThreshold = spectralBgThreshold * 3;  // THRESHOLD    for selecting spectral peaks
            var tuple2 = CalculateEntropyOfPeakLocations(spectrogram, activeSegments, peakThreshold, recording.Nyquist);
            indices.entropyOfPeakFreqDistr = tuple2.Item1;
            //Log.WriteLine("H(Spectral peaks) =" + indices.entropyOfPeakFreqDistr);
            double[] freqPeaks = tuple2.Item2;


            //viii: CLUSTERING - to determine spectral diversity and spectral persistence
            //first convert spectrogram to Binary using threshold. An amp threshold of 0.03 = -30 dB.   An amp threhold of 0.05 = -26dB.
            double binaryThreshold = 0.03;                                        // for deriving binary spectrogram
            var tuple6 = ClusterAnalysis(spectrogram, activeSegments, excludeLoFreqBins, binaryThreshold);
            indices.clusterCount = tuple6.Item1; 
            indices.avClusterDuration = tuple6.Item2 * frameDuration * 1000; //av cluster duration in milliseconds


            //iii: STORE IMAGES
            bool doStoreImages = Configuration.GetBoolean(key_STORE_INTERMEDATE_RESULTS, config); 
            if (doStoreImages)
            {
                //SET UP A LIST OF SCORE ARRAYS TO BE RETURNED
                //List<double[]> scores = null; //arrays to store scores for debugging
                //scores = new List<double[]>();
                //scores.Add(envelope);
                //scores.Add(freqPeaks); //location of peaks for spectral images

                //bool[] selectedFrames = tuple6.Item3;
                //List<double[]> clusterWts = tuple6.Item4;
                //int[] clusterHits = tuple6.Item5;

                //return System.Tuple.Create(Indices2 indices, List<double[]> scores, int[] clusterHits, List<double[]> clusterWts, double[,] clusterSpectrogram);
                //
                //var scores = results.Item2;
                //var clusterIDs = results.Item3;
                //var clusterWts = results.Item4;
                //var clusterSpectrogram = results.Item5;
                //OutputClusterAndWeightInfo(clusterIDs, clusterWts, imagePath);
                //no need for the following line in normal usage - mostly for debugging
                //double[,] clusterSpectrogram = AssembleClusterSpectrogram(signalLength, spectrogram, excludeBins, selectedFrames, binaryThreshold, clusterWts, clusterHits);
                //MakeAndDrawSonogram(recording, recordingDir, scores, clusterSpectrogram);
            }

            return Indices2DataTable(iter, segmentStartMinute, wavSegmentDuration_seconds, indices);
        } //Analysis()


        /// <summary>
        /// reutrns the number of acoustic segments and their average duration in milliseconds
        /// only counts a segment if it is LONGER than one frame. 
        /// count segments as number of transitions from active to non-active frame
        /// </summary>
        /// <param name="activeFrames"></param>
        /// <param name="frameDuration">frame duration in seconds</param>
        /// <returns></returns>
        public static System.Tuple<int, double, bool[]> CalculateSegmentCount(bool[] activeFrames, double frameDuration)
        {
            bool[] segments = activeFrames;

            int activeFrameCount = DataTools.CountTrues(activeFrames);
            if (activeFrameCount == 0) return System.Tuple.Create(0, 0.0, activeFrames);

            int segmentCount = 0;
            for (int i = 2; i < activeFrames.Length; i++)
            {
                if (!activeFrames[i] && activeFrames[i - 1] && activeFrames[i - 2]) segmentCount++; //count the ends of active segments
            }

            // store record of segments longer than one frame
            for (int i = 1; i < activeFrames.Length-1; i++)
            {
                if (!activeFrames[i - 1] && activeFrames[i] && !activeFrames[i + 1]) segments[i] = false; //remove solitary active frames
            }

            int segmentFrameCount = DataTools.CountTrues(segments);
            //double avSegmentDuration =  frameDuration * 1000 * activeFrameCount / (double)segmentCount; //av segment duration in milliseconds
            double avSegmentDuration = 0.0;
            if (segmentFrameCount > 0) avSegmentDuration = frameDuration * 1000 * segmentFrameCount / (double)segmentCount;   //av segment duration in milliseconds

            return System.Tuple.Create(segmentCount, avSegmentDuration, segments);
        }

        /// <summary>
        /// returns fraction coverage of the hi and lo frequency parts of the spectrogram
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="bgThreshold"></param>
        /// <param name="excludeBins"></param>
        /// <returns></returns>
        public static System.Tuple<double, double, double> CalculateSpectralCoverage(double[,] spectrogram, double bgThreshold, int lowFreqBound, int midFreqBound, int Nyquist)
        {
            //calculate boundary between hi, mid and low frequency spectrum
            int freqBinCount = spectrogram.GetLength(1);
            double binWidth = Nyquist / (double)freqBinCount;
            int lowFreqBinCount = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinCount = (int)Math.Ceiling(midFreqBound / binWidth);



            int hfCoverage = 0;
            int mfCoverage = 0;
            int lfCoverage = 0;
            int hfCellCount = 0;
            int mfCellCount = 0;
            int lfCellCount = 0;
            for (int i = 0; i < spectrogram.GetLength(0); i++) //for all rows of spectrogram
            {
                for (int j = 0; j < lowFreqBinCount; j++) //caluclate coverage for low freq band
                {
                    if (spectrogram[i, j] >= bgThreshold) lfCoverage++;
                    lfCellCount++;
                    spectrogram[i, j] = 0.0;
                }
                for (int j = lowFreqBinCount; j < midFreqBinCount; j++) //caluclate coverage for mid freq band
                {
                    if (spectrogram[i, j] >= bgThreshold) mfCoverage++;
                    mfCellCount++;
                }
                for (int j = midFreqBinCount; j < freqBinCount; j++) //caluclate coverage for high freq band
                {
                    if (spectrogram[i, j] >= bgThreshold) hfCoverage++;
                    hfCellCount++;
                }
            }
            double hiFreqCover  = hfCoverage / (double)hfCellCount;
            double midFreqCover = mfCoverage / (double)mfCellCount;
            double lowFreqCover = lfCoverage / (double)lfCellCount;
            return System.Tuple.Create(lowFreqCover, midFreqCover, hiFreqCover);
        }

        /// <summary>
        /// returns ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="activeFrames"></param>
        /// <param name="peakThreshold">required amplitude threshold to qualify as peak</param>
        /// <param name="nyquistFreq"></param>
        /// <returns></returns>
        public static System.Tuple<double, double[]> CalculateEntropyOfPeakLocations(double[,] spectrogram, bool[] activeFrames, double peakThreshold, int nyquistFreq)
        {
            int freqBinCount = spectrogram.GetLength(1);

            double[] freqPeaks = new double[activeFrames.Length]; //store frequency of peaks - return later for imaging purposes
            int[] freqHistogram = new int[freqBinCount];
            int peakCount = 0;
            for (int i = 0; i < activeFrames.Length; i++)
            {
                if (! activeFrames[i]) continue; //select only frames having acoustic energy >= threshold
             
                int j = DataTools.GetMaxIndex(DataTools.GetRow(spectrogram, i)); //locate maximum peak
                if (spectrogram[i, j] > peakThreshold) 
                {
                    freqHistogram[j-1] ++;  //spectrogram has a DC freq column which want to ignore.           
                    freqPeaks[i] = nyquistFreq * j / (double)spectrogram.GetLength(1); //store frequency of peak as double
                    peakCount++;
                }
            } // over all frames in dB array
            if (peakCount == 1) return System.Tuple.Create(0.0, freqPeaks); //energy concentrated in one peak i.e low entropy 
            else
            if (peakCount == 0) return System.Tuple.Create(0.5, freqPeaks); //do not know distribution 

            //DataTools.writeBarGraph(freqHistogram);
            freqHistogram[0] = 0; // remove frames having freq=0 i.e frames with no activity from calculation of entropy.
            double entropy = DataTools.Entropy_normalised(freqHistogram);
            return System.Tuple.Create(entropy, freqPeaks);
        }

        /// <summary>
        /// Returns ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM
        /// Have been passed the amplitude spectrum but square amplitude values to get power or energy.
        /// Entropy is a measure of ENERGY dispersal.
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum. Must square values to get power</param>
        /// <param name="excludeBins"></param>
        /// <returns></returns>
        public static System.Tuple<double, double> CalculateEntropyOfSpectralAvAndVariance(double[,] spectrogram, int excludeBins)
        {
            int freqBinCount = spectrogram.GetLength(1);
            double[] avSpectrum = new double[freqBinCount - excludeBins];   //for average  of the spectral bins
            double[] varSpectrum = new double[freqBinCount - excludeBins];  //for variance of the spectral bins
            for (int j = excludeBins; j < freqBinCount; j++)                //for all frequency bins (excluding low freq)
            {
                var bin = new double[spectrogram.GetLength(0)];      //set up a bin to take freq power
                for (int i = 0; i < spectrogram.GetLength(0); i++)
                {
                    bin[i] = spectrogram[i, j] * spectrogram[i, j];  //convert amplitude to energy or power.
                }
                double av, sd;
                NormalDist.AverageAndSD(bin, out av, out sd);
                avSpectrum[j - excludeBins] = av;      //store average  of the bin
                varSpectrum[j - excludeBins] = sd * sd; //store variance of the bin
            }

            //set up some safety checks but unlikely to happen
            int posCount = avSpectrum.Count(p => p > 0.0);
            if (posCount == 1) return System.Tuple.Create(0.0, 0.0); //energy concentrated in one value - i.e. low entorpy
            else
            if (posCount == 0) return System.Tuple.Create(0.5, 0.5); //low energy distributed - do not know entropy - select middle ground!

            double HSpectralAv = DataTools.Entropy_normalised(avSpectrum);               //ENTROPY of spectral averages
            //DataTools.writeBarGraph(avSpectrum);
            //Log.WriteLine("H(Spectral averages) =" + HSpectralAv);

            //sum = varSpectrum.Sum();
            posCount = varSpectrum.Count(p => p > 0.0);
            //if ((sum < 0.00000001) && (posCount < 2))
            if (posCount == 0) return System.Tuple.Create(HSpectralAv, 0.5);       //flat spectrum - do not know entropy - select middle ground!
            else
            if (posCount == 1) return System.Tuple.Create(HSpectralAv, 0.0);       //variance concentrated in few values - i.e. low entropy
            else
            {
                double HSpectralVar = DataTools.Entropy_normalised(varSpectrum);         //ENTROPY of spectral variances
                //DataTools.writeBarGraph(varSpectrum);
                //Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);
                return System.Tuple.Create(HSpectralAv, HSpectralVar);
            }
        }



        public static System.Tuple<int, double, bool[], List<double[]>, int[]> ClusterAnalysis(double[,] spectrogram, bool[] activeFrames, int excludeBins, double binaryThreshold)
        {
            //viii: CLUSTERING - to determine spectral diversity and spectral persistence
            //first convert spectrogram to Binary using threshold. An amp threshold of 0.03 = -30 dB.   An amp threhold of 0.05 = -26dB.
            spectrogram = DataTools.Matrix2Binary(spectrogram, binaryThreshold);  // convert to binary 

            int length = spectrogram.GetLength(0);

            double[,] subMatrix = DataTools.Submatrix(spectrogram, 0, excludeBins, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);
            bool[] selectedFrames = new bool[length];
            var trainingData = new List<double[]>();    //training data that will be used for clustering

            int rowSumThreshold = 1;  //ACTIVITY THREHSOLD - require activity in at least N bins to include for training
            int selectedFrameCount = 0;
            for (int i = 0; i < subMatrix.GetLength(0); i++)
            {
                if (! activeFrames[i]) continue;   //select only frames having acoustic energy >= threshold
                double[] row = DataTools.GetRow(subMatrix, i);
                if (row.Sum() >= rowSumThreshold)  //only include frames where activity exceeds threshold 
                {
                    trainingData.Add(row);
                    selectedFrames[i] = true;
                    selectedFrameCount++;
                }
            }
            //return if no suitable training data for clustering
            if (trainingData.Count  <= 10)
            {
                List<double[]> clusterWts_dummy = null;
                int[] clusterHits_dummy = null;
                return System.Tuple.Create(0, 0.0, selectedFrames, clusterWts_dummy, clusterHits_dummy);
            }



            //Log.WriteLine("ActiveFrameCount=" + activeFrameCount + "  frames selected for clustering=" + selectedFrameCount);

            //DO CLUSTERING - if have suitable data
            BinaryCluster.Verbose = false;
            //if (Log.Verbosity > 0) BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            double vigilance = 0.2;    //vigilance parameter - increasing this proliferates categories
                                       //if vigilance=0.1, require similairty (AND/OR) > 10%
            var output = BinaryCluster.ClusterBinaryVectors(trainingData, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1        = output.Item1;   //the cluster to which each frame belongs
            List<double[]> clusterWts = output.Item2;
            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            double wtThreshold = 1.0; //used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold   = 5;   //used to remove wt vectors which have fewer than the threshold hits
            var output2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            List<double[]> prunedClusterWts = output2.Item1;
            int clusterCount = prunedClusterWts.Count;

            if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
            if (BinaryCluster.Verbose) Console.WriteLine("pruned cluster count = {0}", clusterCount);
            
            //ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  first:  reassemble cluster hits into an array matching the original array of active frames.
            int hitCount = 0;
            int[] clusterHits2 = new int[length]; 
            for (int i = 0; i < length; i++)
            {
                if (selectedFrames[i]) //select only frames having acoustic energy >= threshold
                {
                    clusterHits2[i] = clusterHits1[hitCount] + 1;//+1 so do not have zero index for a cluster 
                    hitCount++;
                }
            }
            //  second:  calculate duration (ms) of each spectral event
            List<int> hitDurations = new List<int>();
            int currentDuration = 1;
            for (int i = 1; i < clusterHits2.Length; i++)
            {
                if (clusterHits2[i] != clusterHits2[i - 1]) //if the spectrum changes
                {
                    if ((clusterHits2[i - 1] != 0) && (currentDuration > 1)) hitDurations.Add(currentDuration); //do not add if cluster = 0
                    currentDuration = 1;
                }
                else
                {
                    currentDuration++;
                }
            }
            double av2, sd2;
            NormalDist.AverageAndSD(hitDurations, out av2, out sd2);
            return System.Tuple.Create(clusterCount, av2, selectedFrames, prunedClusterWts, clusterHits2);
        }


        //#########################################################################################################################################################
        //  OTHER METHODS



        public static DataTable ConstructTableOfDisplayValues(bool WriteToConsole, DataTable dt)
        {
            DataTableTools.RemoveTableColumns(dt, AcousticIndices.DISPLAY_COLUMN);

            var displayHeaders = new List<string>();          //reconstruct new list of column headers of those columns used to display
            var displayTypes   = new List<Type>();            //reconstruct new list of column types 
            var data = new List<double[]>();
            double[] norm; 

            for (int i = 0; i < AcousticIndices.DISPLAY_COLUMN.Length; i++)
            {
                if (AcousticIndices.DISPLAY_COLUMN[i])
                {
                    Type type =  dt.Columns[i].DataType;
                    displayTypes.Add(type);
                    displayHeaders.Add(dt.Columns[i].ColumnName);
                    if (type == typeof(int))
                    {
                        int[] intArray = DataTableTools.Column2ListOfInt(dt, dt.Columns[i].ColumnName).ToArray();
                        norm = DataTools.NormaliseArea(intArray);
                    }
                    else
                    {
                        double[] dbleArray = DataTableTools.Column2ListOfDouble(dt, dt.Columns[i].ColumnName).ToArray();
                        norm = DataTools.NormaliseArea(dbleArray);
                    }
                    data.Add(norm);
                }
            }

            // set up a new display datatable 
            var displayTable = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes.ToArray());

            var numItemsInArrayInDoubleData = data[0].Length;
            var numArraysInDoubleData = data.Count;

            for (int r = 0; r < numItemsInArrayInDoubleData; r++)
            {
                DataRow newRow = displayTable.NewRow();

                for (int c = 0; c < numArraysInDoubleData; c++)
                {
                    newRow[c] = data[c][r];
                }

                displayTable.Rows.Add(newRow);
            }
            return displayTable;
        }



        public static void GetListOfWeightedIndices(bool WriteToConsole, DataTable dt)
        {
            var displayHeaders = new List<string>();        //reconstruct new list of column headers of those columns used to display
            var displayTypes = new List<Type>();            //reconstruct new list of column types   
            for (int i = 0; i < AcousticIndices.DISPLAY_COLUMN.Length; i++)
            {
                if (AcousticIndices.DISPLAY_COLUMN[i])
                {
                    displayHeaders.Add(dt.Columns[i].ColumnName);
                    displayTypes.Add(dt.Columns[i].DataType);
                }
            }

            //set up an array of values normalised by column
            var data = new List<double[]>();
            foreach (DataColumn col in dt.Columns)
            {
                int[] array = DataTableTools.Column2ListOfInt(dt, col.ColumnName).ToArray();
                double[] norm = DataTools.NormaliseArea(array);
                data.Add(norm);
            }

            // set up a new display datatable 
            var displayTable = DataTableTools.CreateTable(displayHeaders.ToArray(), displayTypes.ToArray());
            for (int r = 0; r < data[0].Length; r++)
            {
                DataRow newRow = displayTable.NewRow();
                for (int c = 0; c < data.Count; c++) newRow[c] = data[r][c];
                displayTable.Rows.Add(newRow);
            }



            //append new column to existing table and return

            //RECONSTRUCT NEW LIST OF VALUES to CALCULATE WEIGHTED COMBINATION INDEX
            //var comboWeights = new List<double>();          //reconstruct new list of column weights from those columns used to calculate weighted index
            //var comboColumns = new List<double[]>();        //reconstruct new list of columns to calculate weighted combination index

            //if (WriteToConsole) Console.WriteLine(comboHeader);
            //for (int i = 0; i < AcousticIndices.comboWeights.Length; i++)
            //{
            //    if (AcousticIndices.comboWeights[i] > 0.0)
            //    {
            ////        double[] norm = DataTools.NormaliseArea(values[i]);
            ////        comboColumns.Add(norm);
            ////        comboWeights.Add(AcousticIndices.comboWeights[i]);
            ////        comboHeaders.Add(headers[i]);
            //        if (WriteToConsole) Console.WriteLine("\t{0} = {1}", AcousticIndices.HEADERS[i], AcousticIndices.comboWeights[i]);
            //    }
            //}


            //add in weighted bias for chorus and backgorund noise
            //for (int i = 0; i < wtIndices.Length; i++)
            //{
            //if((i>=290) && (i<=470)) wtIndices[i] *= 1.1;  //morning chorus bias
            //background noise bias
            //if (bg_dB[i - 1] > -35.0) wtIndices[i] *= 0.8;
            //else
            //if (bg_dB[i - 1] > -30.0) wtIndices[i] *= 0.6;
            //}

            //double[] weightedIndices = DataTools.GetWeightedCombinationOfColumns(comboColumns, comboWeights.ToArray());
            //weightedIndices = DataTools.normalise(weightedIndices);
            //displayValues.Add(weightedIndices);
            //}
        }



        /// <summary>
        /// TODO: This method should call the Analysis() method
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


        
        public static void MakeAndDrawSonogram(AudioRecording recording, string dir, List<double[]> scores, double[,]clusterMatrix)
        {
            //i: MAKE SONOGRAM
            Log.WriteIfVerbose("# Make sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); // default values config
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.WindowSize = SonogramConfig.DEFAULT_WINDOW_SIZE;
            sonoConfig.WindowOverlap = 0.0;                   // set default value
            sonoConfig.DoMelScale = false;
            //sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
            //sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD; //MODAL values assumed to be dB values
            //sonoConfig.NoiseReductionType = NoiseReductionType.MODAL;    //MODAL values not dependent on dB values
            sonoConfig.NoiseReductionType = NoiseReductionType.BINARY;     //MODAL values assumed to be dB values
            sonoConfig.NoiseReductionParameter = 4.0; //ie 4 dB threshold for BG noise removal

            AmplitudeSonogram basegram = new AmplitudeSonogram(sonoConfig, recording.GetWavReader());
            SpectralSonogram sonogram = new SpectralSonogram(basegram);         //spectrogram has dim[N,257]



            //SCALE THE SPECTROGRAM
            double[,] newClusterMatrix = null;
            double[] array = null;
            if (null != clusterMatrix)
            {
                //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster1.png", false);
                newClusterMatrix = DataTools.ScaleMatrix(clusterMatrix, sonogram.FrameCount, sonogram.Configuration.FreqBinCount);
                //ImageTools.DrawMatrix(DataTools.MatrixRotate90Anticlockwise(clusterSpectrogram), @"C:\SensorNetworks\WavFiles\SpeciesRichness\Dev1\cluster2.png", false);
                array = DataTools.Matrix2Array(newClusterMatrix);
            }


            //ii: DRAW SONOGRAM AND SCORES
            //Log.WriteLine("# Draw sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int length = sonogram.FrameCount;

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));

                //add peak freq locations
                //var newArray = DataTools.ScaleArray(scores[0], length);
                //int[] freq = new int[newArray.Length]; //convert array of double to array of int
                //for (int i = 0; i < newArray.Length; i++) freq[i] = (int)newArray[i];
                //image.AddFreqHitValues(freq, sonogram.NyquistFrequency); //freq must be an array of int 

                int maxindex = DataTools.GetMaxIndex(array);
                image.AddSuperimposedMatrix(newClusterMatrix, array[maxindex]);

                if (scores != null)
                {
                    for (int i = 1; i < scores.Count; i++)
                    {
                        int maxIndex = DataTools.GetMaxIndex(scores[i]);
                        double max = scores[i][maxIndex];
                        if (max <= 0.0) max = 1.0;
                        image.AddTrack(Image_Track.GetScoreTrack(DataTools.ScaleArray(scores[i], length), 0.0, max, 0.1));
                    }
                }
                string imagePath = dir + recording.FileName + ".png";
                image.Save(imagePath);
            } // using
        } // MakeAndDrawSonogram()



        /// <summary>
        /// displays a histogram of cluster counts.
        /// the argument clusters is an array of integer. Indicates cluster assigned to each binary frame. 
        /// </summary>
        /// <param name="clusters"></param>
        public static void OutputClusterAndWeightInfo(int[] clusters, List<double[]> wts, string imagePath)
        {
            int min, max;
            int maxIndex;
            DataTools.getMaxIndex(clusters, out maxIndex);
            int binCount = clusters[maxIndex] + 1;
            double binWidth;
            int[] histo = DataTools.Histo(clusters, binCount, out binWidth, out min, out max);
            Console.WriteLine("Sum = " + histo.Sum());
            DataTools.writeArray(histo);
            //DataTools.writeBarGraph(histo);

            //make image of the wts matrix
            wts = DataTools.RemoveNullElementsFromList(wts);
            var m = DataTools.ConvertList2Matrix(wts);
            m = DataTools.MatrixTranspose(m);
            ImageTools.DrawMatrixInColour(m, imagePath, false);
        }

        /// <summary>
        /// this method is used only to visualize the clusters and which frames they hit.
        /// Create a new spectrogram of same size as the passed spectrogram.
        /// Later on it is superimposed on a detailed spectrogram.
        /// </summary>
        /// <param name="sigLength"></param>
        /// <param name="spectrogram">spectrogram used to derive spectral richness indices</param>
        /// <param name="excludeBins">bottom N freq bins that are excluded because likely to contain traffic and wind noise.</param>
        /// <param name="activeFrames"></param>
        /// <param name="binaryThreshold">used to select values in reduced spectrogram</param>
        /// <param name="clusterWts"></param>
        /// <param name="clusterHits"></param>
        /// <returns></returns>
        public static double[,] AssembleClusterSpectrogram(int sigLength, double[,] spectrogram, int excludeBins, bool[] activeFrames,
                                                           double binaryThreshold, List<double[]> clusterWts, int[] clusterHits)
        {
             
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1); 
            
            //reassemble spectrogram to visualise the clusters
            var clusterSpectrogram = new double[frameCount, freqBinCount];
            int count = 0;
            for (int i = 0; i < frameCount; i++) //loop over original frames
            {
                if (activeFrames[i])
                {
                    for (int j = excludeBins; j < freqBinCount; j++)
                    {
                        if (spectrogram[i, j] > binaryThreshold)
                            clusterSpectrogram[i, j] = clusterHits[count] + 1;//+1 so do not have zero index for a cluster 
                        if (clusterSpectrogram[i, j] < 0) clusterSpectrogram[i, j] = 0; //correct for case where set hit count < 0 for pruned wts.
                    }
                    count++;
                }
            }

            //add in the weights to first part of spectrogram
            //int space = 10;
            //int col = space;
            //for (int i = 0; i < clusterWts.Count; i++)
            //{
            //    if (clusterWts[i] == null) continue;
            //    for (int c = 0; c < space; c++)
            //    {
            //        col++;
            //        //for (int j = 0; j < clusterSpectrogram.GetLength(1); j++) clusterSpectrogram[col, j] = clusterWts.Count+3;
            //        for (int j = 0; j < clusterWts[i].Length; j++)
            //        {
            //            if (clusterWts[i][j] > 0.0) clusterSpectrogram[col, excludeBins + j - 1] = i + 1;
            //        }
            //    }
            //    //col += 2;
            //}

            return clusterSpectrogram;
        }

        //########################################################################################################################################################################


         public static double[] GetArrayOfWeightedAcousticIndices(DataTable dt, double[] weightArray)
         {
             if (weightArray.Length > dt.Columns.Count) return null; //weights do not match data table
             List<double[]> columns = new List<double[]>();
             List<double> weights = new List<double>();
             for (int i = 0; i < weightArray.Length; i++)
             {
                 if (weightArray[i] != 0.0)
                 {
                     weights.Add(weightArray[i]);
                     string colName = dt.Columns[i].ColumnName;
                     double[] array = DataTableTools.Column2ListOfDouble(dt, colName).ToArray();
                     columns.Add(DataTools.NormaliseArea(array)); //normalize the arrays prior to obtaining weighted index.
                 }
             } //for

            int arrayLength = columns[0].Length; //assume all columns are of same length 
            double[] weightedIndices = new double[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                double combo = 0.0;
                for (int c = 0; c < columns.Count; c++)
                {
                    combo += (weights[c] * columns[c][i]);
                }
                weightedIndices[i] = combo;
            }

            //Add in weighted bias for chorus and backgorund noise
            //IMPORTANT: this only works if DataTable is ordered correctly before this point.
            //for (int i = 0; i < wtIndices.Length; i++)
            //{
            //if((i>=290) && (i<=470)) wtIndices[i] *= 1.1;  //morning chorus bias
            //background noise bias
            //if (bg_dB[i - 1] > -35.0) wtIndices[i] *= 0.8;
            //else
            //if (bg_dB[i - 1] > -30.0) wtIndices[i] *= 0.6;
            //}

            weightedIndices = DataTools.normalise(weightedIndices); //normalise final array in [0,1]
            return weightedIndices;
        }


         public static Tuple<DataTable, DataTable> ProcessCsvFile(FileInfo fiCsvFile, string[] headers2Display)
        {
            bool addColumnOfweightedIndices = true;
            DataTable dt = CsvTools.ReadCSVToTable(fiCsvFile.FullName, true);
            if ((dt == null) || (dt.Rows.Count == 0)) return null;

            dt = DataTableTools.SortTable(dt, header_count + " ASC");

            List<string> displayHeaders = headers2Display.ToList();

            double[] weightedIndices = null;
            if (addColumnOfweightedIndices)
            {
                AcousticIndices.InitOutputTableColumns();
                weightedIndices = AcousticIndices.GetArrayOfWeightedAcousticIndices(dt, AcousticIndices.COMBO_WEIGHTS);
                string colName = "WeightedIndex";
                displayHeaders.Add(colName);
                DataTableTools.AddColumn2Table(dt, colName, weightedIndices);
            }

            DataTable processedtable = ProcessDataTableForDisplayOfColumnValues(dt, displayHeaders);
            return System.Tuple.Create(dt, processedtable);
        } // ProcessCsvFile()



         /// <summary>
         /// takes a data table of indices and converts column values to values in [0,1].
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
                 if (! originalHeaderList.Contains(header)) continue;

                 List<double> values = DataTableTools.Column2ListOfDouble(dt, header); //get list of values
                 if ((values == null) || (values.Count == 0)) continue; 

                 double min = 0;
                 double max = 1;
                 if (header.Equals(header_count))
                 {
                    newColumns.Add(DataTools.normalise(values.ToArray())); //normalise all values in [0,1]
                    newHeaders.Add(header);
                 }
                 else if (header.Equals(header_avAmpdB))
                 {
                     min = -50;
                     max = -5;
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_avAmpdB + "  (-50..-5dB)");
                 }
                 else if (header.Equals(header_snrdB))
                 {
                     min = 5;
                     max = 50;
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_snrdB + "  (5..50dB)");
                 }
                 else if (header.Equals(header_avSegDur))
                 {
                     min = 0.0;
                     max = 500.0; //av segment duration in milliseconds
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_avSegDur + "  (0..500ms)");
                 }
                 else if (header.Equals(header_bgdB))
                 {
                     min = -50;
                     max = -5;
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_bgdB + "  (-50..-5dB)");
                 }
                 else if (header.Equals(header_avClustDur))
                 {
                     min = 50.0; //note: minimum cluster length = two frames = 2*frameDuration
                     max = 200.0; //av segment duration in milliseconds
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_avClustDur + "  (50..200ms)");
                 }
                 else if (header.Equals(header_lfCover))
                 {
                     min = 0.1; //
                     max = 1.0; //
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_lfCover + "  (10..100%)");
                 }
                 else if (header.Equals(header_mfCover))
                 {
                     min = 0.0; //
                     max = 0.9; //
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_mfCover + "  (0..90%)");
                 }
                 else if (header.Equals(header_hfCover))
                 {
                     min = 0.0; //
                     max = 0.9; //
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_hfCover + "  (0..90%)");
                 }
                 else if (header.Equals(header_HAmpl))
                 {
                     min = 0.5; //
                     max = 1.0; //
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_HAmpl + "  (0.5..1.0)");
                 }
                 else if (header.Equals(header_HAvSpectrum))
                 {
                     min = 0.2; //
                     max = 1.0; //
                     newColumns.Add(DataTools.NormaliseInZeroOne(values.ToArray(), min, max));
                     newHeaders.Add(header_HAvSpectrum + "  (0.2..1.0)");
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




        public static string FormatHeader(string parmasFile_Separator)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string line = String.Format(FORMAT_STR_HEADERS, reportSeparator, HEADERS[0], HEADERS[1], HEADERS[2], HEADERS[3], HEADERS[4], HEADERS[5], HEADERS[6], HEADERS[7],
                                                                        HEADERS[8], HEADERS[9], HEADERS[10], HEADERS[11], HEADERS[12], HEADERS[13], HEADERS[14], HEADERS[15], HEADERS[16],HEADERS[17], HEADERS[18]);
            return line;
        }

        public static void WriteHeaderToReportFile(string reportfileName, string parmasFile_Separator)
        {
            string line = FormatHeader(parmasFile_Separator);
            FileTools.WriteTextFile(reportfileName, line);
        }

        public static DataTable Indices2DataTable(int count, double startMin, double sec_duration, AcousticIndices.Indices2 indices)
        {
            var parameters = InitOutputTableColumns();
            var headers = parameters.Item1;
            var types   = parameters.Item2;
            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(count, startMin, sec_duration, indices.avSig_dB, indices.snr, 
                        indices.bgNoise, indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.hiFreqCover,
                        indices.midFreqCover, indices.lowFreqCover, indices.temporalEntropy, indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum,
                        indices.entropyOfVarianceSpectrum, indices.clusterCount, indices.avClusterDuration);
            return dt;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="parmasFile_Separator"></param>
        /// <param name="fileCount"></param>
        /// <param name="elapsedTime">elapsed time in minutes to end of current segment</param>
        /// <param name="indices"></param>
        public static string FormatOneLineOfIndices(string parmasFile_Separator, int count, double startMin, double sec_duration, AcousticIndices.Indices2 indices)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";


            //string duration = DataTools.Time_ConvertSecs2Mins(segmentDuration);
            string line = String.Format(FORMAT_STR_DATA, reportSeparator,
                                       count, startMin, sec_duration, indices.avSig_dB, indices.snr, indices.bgNoise,
                                       indices.activity, indices.segmentCount, indices.avSegmentDuration, indices.hiFreqCover, indices.midFreqCover, indices.lowFreqCover, indices.temporalEntropy,
                                       indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectrum,
                                       indices.clusterCount, indices.avClusterDuration);
            return line;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="opPath"></param>
        /// <param name="parmasFile_Separator"></param>
        /// <param name="fileCount"></param>
        /// <param name="elapsedTime">elapsed time in minutes to end of current segment</param>
        /// <param name="indices"></param>
        public static void AppendIndicesToReportFile(string opPath, string parmasFile_Separator, int count, double startMin, double sec_duration, AcousticIndices.Indices2 indices)
        {
            string line = FormatOneLineOfIndices(parmasFile_Separator, count, startMin, sec_duration, indices);
            FileTools.Append2TextFile(opPath, line);
        }








        /// <summary>
        /// this method used to process Jason Wimmer's original xls spreadsheet that did not have a row for silent minutes
        /// this method added in the silent minutes to construct new csv file.
        /// </summary>
        public static void MASSAGE_CSV_DATA()
        {
            string fileName = @"C:\SensorNetworks\WavFiles\SpeciesRichness\24hrs_1MinuteChunks\SthEastSensor.csv";
            string opFile = @"C:\SensorNetworks\WavFiles\SpeciesRichness\24hrs_1MinuteChunks\SthEastSensor_Padded.csv";
            FileTools.WriteTextFile(opFile, "min,time,count");
            List<string> lines = FileTools.ReadTextFile(fileName);
            string line;
            int minPrev = 0;
            int minTotal = 0;
            int speciesTotal = 0;
            for (int i = 1; i < lines.Count - 1; i++) //ignore last line
            {
                string[] words = lines[i].Split(',');
                int speciesCount = Int32.Parse(words[1]);
                speciesTotal += speciesCount;
                string[] splitTime = words[0].Split(':');
                int hour = Int32.Parse(splitTime[0]);
                int min = Int32.Parse(splitTime[1]);
                minTotal = (hour * 60) + min;
                if (minTotal > minPrev + 1)
                {
                    for (int j = minPrev + 1; j < minTotal; j++)
                    {
                        line = String.Format("{0}  time={1}:{2}   Count={3}", j, (j / 60), (j % 60), 0);
                        Console.WriteLine(line);
                        line = String.Format("{0},{1}:{2},{3}", j, (j / 60), (j % 60), 0);
                        FileTools.Append2TextFile(opFile, line);
                    }
                }

                line = String.Format("{0}  time={1}:{2}   Count={3}", minTotal, hour, min, speciesCount);
                Console.WriteLine(line);
                line = String.Format("{0},{1}:{2},{3}", minTotal, hour, min, speciesCount);
                FileTools.Append2TextFile(opFile, line);
                minPrev = minTotal;
            }
            //fill in misisng minutes at end.
            int minsIn24hrs = 24 * 60;
            if (minsIn24hrs > minPrev + 1)
            {
                for (int j = minPrev + 1; j < minsIn24hrs; j++)
                {
                    line = String.Format("{0}  time={1}:{2}   Count={3}", j, (j / 60), (j % 60), 0);
                    Console.WriteLine(line);
                    line = String.Format("{0},{1}:{2},{3}", j, (j / 60), (j % 60), 0);
                    FileTools.Append2TextFile(opFile, line);
                }
            }
            Console.WriteLine("speciesTotal= " + speciesTotal);
        }


    }
}
