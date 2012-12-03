// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AcousticFeatures.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the AcousticFeatures type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    using AnalysisBase;

    using AudioAnalysisTools;
    using NeuralNets;
    using TowseyLib;

    public class AcousticFeatures
    {
        // public const string ANALYSIS_NAME = "Indices"; 
        public const double DEFAULT_activityThreshold_dB = 3.0; // used to select frames that have 3dB > background
        public const int    DEFAULT_WINDOW_SIZE = 256;
        public static int   lowFreqBound = 500; // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        public static int   midFreqBound = 3500;
        public const  int   RESAMPLE_RATE = 17640; //chose this value because it is simple fraction (4/5) of 22050Hz. However this now appears to be irrelevant.
        // public const int RESAMPLE_RATE = 22050;

        // Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public static string key_LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public static string key_MID_FREQ_BOUND = "MID_FREQ_BOUND";
        public static string key_DISPLAY_COLUMNS = "DISPLAY_COLUMNS";

        private const int    COL_NUMBER = 20;
        private static Type[] COL_TYPES = new Type[COL_NUMBER];
        private static string[] HEADERS = new string[COL_NUMBER];
        private static bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
        private static double[] COMBO_WEIGHTS = new double[COL_NUMBER];

        public static string header_count = Keys.INDICES_COUNT;
        public const string header_startMin  = "start-min";
        public const string header_SecondsDuration = "SegTimeSpan";
        public const string header_avAmpdB   = "avAmp-dB";
        public const string header_snrdB     = "snr-dB";
        public const string header_activeSnrdB = "activeSnr-dB";
        public const string header_bgdB      = "bg-dB";
        public const string header_activity  = "activity";
        public const string header_segCount  = "segCount";
        public const string header_avSegDur  = "avSegDur";
        public const string header_hfCover   = "hfCover";
        public const string header_mfCover   = "mfCover";
        public const string header_lfCover   = "lfCover";
        public const string header_HAmpl     = "H[ampl]";
        public const string header_HPeakFreq = "H[peakFreq]";
        public const string header_HAvSpectrum  = "H[avSpectrum]";
        public const string header_HVarSpectrum = "H[varSpectrum]";
        public const string header_AcComplexity = "AcComplexity";
        public const string header_NumClusters  = "#clusters";
        public const string header_avClustDur   = "avClustDur";


        private static System.Tuple<string[], Type[], bool[]> InitOutputTableColumns()
        {
            HEADERS[0] = header_count;         COL_TYPES[0] = typeof(int);      DISPLAY_COLUMN[0] = false;  COMBO_WEIGHTS[0]  = 0.0;
            HEADERS[1] = header_startMin;      COL_TYPES[1] = typeof(double);   DISPLAY_COLUMN[1] = false;  COMBO_WEIGHTS[1]  = 0.0;
            HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false;  COMBO_WEIGHTS[2]  = 0.0;
            HEADERS[3] = header_avAmpdB;       COL_TYPES[3] = typeof(double);   DISPLAY_COLUMN[3] = true;   COMBO_WEIGHTS[3]  = 0.0;
            HEADERS[4] = header_snrdB;         COL_TYPES[4] = typeof(double);   DISPLAY_COLUMN[4] = true;   COMBO_WEIGHTS[4]  = 0.0;
            HEADERS[5] = header_activeSnrdB;   COL_TYPES[5] = typeof(double);   DISPLAY_COLUMN[5] = true;   COMBO_WEIGHTS[5]  = 0.0;
            HEADERS[6] = header_bgdB;          COL_TYPES[6] = typeof(double);   DISPLAY_COLUMN[6] = true;   COMBO_WEIGHTS[6]  = 0.0;
            HEADERS[7] = header_activity;      COL_TYPES[7] = typeof(double);   DISPLAY_COLUMN[7] = true;   COMBO_WEIGHTS[7]  = 0.0;
            HEADERS[8] = header_segCount;      COL_TYPES[8] = typeof(int);      DISPLAY_COLUMN[8] = true;   COMBO_WEIGHTS[8]  = 0.0;
            HEADERS[9] = header_avSegDur;      COL_TYPES[9] = typeof(double);   DISPLAY_COLUMN[9] = true;   COMBO_WEIGHTS[9]  = 0.0;
            HEADERS[10] = header_hfCover;      COL_TYPES[10] = typeof(double);  DISPLAY_COLUMN[10] = true;  COMBO_WEIGHTS[10] = 0.0;
            HEADERS[11] = header_mfCover;      COL_TYPES[11] = typeof(double);  DISPLAY_COLUMN[11] = true;  COMBO_WEIGHTS[11] = 0.0;
            HEADERS[12] = header_lfCover;      COL_TYPES[12] = typeof(double);  DISPLAY_COLUMN[12] = true;  COMBO_WEIGHTS[12] = 0.0;
            HEADERS[13] = header_HAmpl;        COL_TYPES[13] = typeof(double);  DISPLAY_COLUMN[13] = true;  COMBO_WEIGHTS[13] = 0.0;
            HEADERS[14] = header_HPeakFreq;    COL_TYPES[14] = typeof(double);  DISPLAY_COLUMN[14] = false; COMBO_WEIGHTS[14] = 0.0;
            HEADERS[15] = header_HAvSpectrum;  COL_TYPES[15] = typeof(double);  DISPLAY_COLUMN[15] = true;  COMBO_WEIGHTS[15] = 0.4;
            HEADERS[16] = header_HVarSpectrum; COL_TYPES[16] = typeof(double);  DISPLAY_COLUMN[16] = false; COMBO_WEIGHTS[16] = 0.1;
            HEADERS[17] = header_AcComplexity; COL_TYPES[17] = typeof(double);  DISPLAY_COLUMN[17] = true;  COMBO_WEIGHTS[17] = 0.0;
            HEADERS[18] = header_NumClusters;  COL_TYPES[18] = typeof(int);     DISPLAY_COLUMN[18] = true;  COMBO_WEIGHTS[18] = 0.4;
            HEADERS[19] = header_avClustDur;   COL_TYPES[19] = typeof(double);  DISPLAY_COLUMN[19] = true;  COMBO_WEIGHTS[19] = 0.1;
            //HEADERS[20] = "Weighted index"; COL_TYPES[20] = typeof(double); DISPLAY_COLUMN[20] = false; COMBO_WEIGHTS[20] = 0.0;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN);
        }

        public static string[] GetTableHeaders()
        {
            InitOutputTableColumns();
            return HEADERS;
        }

        public static Type[] GetTableTypes()
        {
            InitOutputTableColumns();
            return COL_TYPES;
        }

        public static bool[] GetDisplayColumns()
        {
            InitOutputTableColumns();
            return DISPLAY_COLUMN;
        }

        public static double[] GetComboWeights()
        {
            InitOutputTableColumns();
            return COMBO_WEIGHTS;
        }

        public const string FORMAT_STR_HEADERS = "{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{0}{13}{0}{14}{0}{15}{0}{16}{0}{17}{0}{18}{0}{19}";
        public const string FORMAT_STR_DATA    = "{1}{0}{2:f1}{0}{3:f3}{0}{4:f2}{0}{5:f2}{0}{6:f2}{0}{7:f2}{0}{8}{0}{9:f2}{0}{10:f4}{0}{11:f4}{0}{12:f4}{0}{13:f4}{0}{14:f4}{0}{15:f4}{0}{16}{0}{17}{0}{18}{0}{19}";


        // Following is list of scaling originally applied to the Acoustic Indices Tracks
        // Amplitude track 2:              title = "1: av Sig Ampl(dB)" minDB = -50; maxDB = -20; 
        // Background dB track 3:          title = "2: Background(dB)"  minDB = -50; maxDB = -20;
        // SNR track 4:                    title = "3: SNR"             minDB = 0;   maxDB = 30;
        // draw activity track 5:          title = "4: Activity(>3dB)"; min = 0.0;   max = 0.4;
        // Segment count track 6:          title = "5: # Segments";     threshold = 1.0;
        // avSegment Duration track 7:     title = "6: Av Seg Duration"; min = 0.0; max = 100; //milliseconds
        // percent spectral Cover track 8: title = "7: Spectral cover"; min = 0.0;  max = 0.5; threshold = 0.05;
        // percent spectral Cover track 9: title = "8: Low freq cover"; min = 0.0;  max = 1.0; threshold = 0.1;
        // Spectral Cover track 10:        title = "9: H(ampl)";        min = 0.95; max = 1.0; threshold = 0.96;
        // H(PeakFreq) track 11:           title = "10: H(PeakFreq)";   min & max = min and max
        // H(avSpect) track 12             title = "11: H(avSpect)";    min & max = min and max
        // H(diffSpect) track 13:          title = "12: H(varSpect)";   min & max = min and max
        // clusterCount track 14:          title = "13: ClusterCount";  min = 0.0;  max = 15.0;  threshold = 1.0;
        // av Cluster Duration track 15:   title = "14: Av Cluster Dur";  min = 0.0;  max = 100.0;  threshold = 5.0;
        // weightedIndex track 16:         title = "15: Weighted Index";  






        /// <summary>
        /// a set of parameters derived from ini file.
        /// </summary>
        public struct Parameters
        {
            public int FrameLength;

            public int ResampleRate;

            public int LowFreqBound;

            public int SegmentOverlap;

            public double SegmentDuration;

            public double FrameOverlap;

            //public int DRAW_SONOGRAMS;
            //public string reportFormat;

            public Parameters(double _segmentDuration, int _segmentOverlap, int _resampleRate,
                              int _frameLength, int _frameOverlap, int _lowFreqBound, int _DRAW_SONOGRAMS, string _fileFormat)
            {
                this.SegmentDuration = _segmentDuration;
                this.SegmentOverlap  = _segmentOverlap;
                this.ResampleRate    = _resampleRate;
                this.FrameLength     = _frameLength;
                this.FrameOverlap    = _frameOverlap;
                this.LowFreqBound    = _lowFreqBound;
                ////DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; //av length of clusters > 1 frame.
                ////reportFormat    = _fileFormat;
            } // Parameters
        } // struct Parameters


        /// <summary>
        /// a set of indices to describe level of acoustic activity in recording.
        /// </summary>
        public struct Activity
        {
            public double activeFrameCover, activeAvDB;
            public TimeSpan avSegmentDuration;
            public int    activeFrameCount, segmentCount;
            public bool[] activeFrames, segmentLocations;

            public Activity(bool[] _activeFrames, int _activeFrameCount, double _activity, double _activeAvDB, int _segmentCount, TimeSpan _avSegmentLength, bool[] _segments)
            {
                activeFrames = _activeFrames;
                activeFrameCount = _activeFrameCount;
                activeFrameCover = _activity;
                activeAvDB = _activeAvDB;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentLength;
                segmentLocations = _segments;
            }
        } // struct Activity




        /// <summary>
        /// a set of indices derived from each recording.
        /// </summary>
        public struct Indices2
        {
            public double snr, activeSnr, bgNoise, activity, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover;
            public double entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfVarianceSpectrum; //spectral indices
            public double ACI; // acoustic complexity index
            public int    segmentCount, clusterCount;
            public TimeSpan avSegmentDuration, avClusterDuration;

            public Indices2(double _snr, double _activeSnr, double _bgNoise, double _activity, TimeSpan _avSegmentDuration, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, double _ACI,
                            int _clusterCount, TimeSpan _avClusterDuration)
            {
                snr        = _snr;
                activeSnr  = _activeSnr;
                bgNoise    = _bgNoise;
                activity   = _activity;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentDuration;
                avSig_dB   = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover   = _hiFreqCover;
                midFreqCover  = _midFreqCover;
                lowFreqCover  = _lowFreqCover;
                entropyOfPeakFreqDistr = _peakFreqEntropy;
                entropyOfAvSpectrum   = _entropyOfAvSpectrum;
                entropyOfVarianceSpectrum = _entropyOfVarianceSpectrum;
                ACI = _ACI;
                clusterCount = _clusterCount;
                avClusterDuration = _avClusterDuration; //av length of clusters > 1 frame.
            }
        } // struct Indices2


        public struct ClusterInfo
        {
            public int clusterCount;
            public double av2;
            public bool[] selectedFrames;
            public List<double[]> prunedClusterWts;
            public int[] clusterHits2;
            public ClusterInfo(List<double[]> _PrunedClusterWts, double _av2, bool[] _SelectedFrames, int[] _ClusterHits2)
            {
                clusterCount = 0;
                if (_PrunedClusterWts != null) clusterCount = _PrunedClusterWts.Count;
                av2 = _av2; 
                selectedFrames = _SelectedFrames; 
                prunedClusterWts = _PrunedClusterWts; 
                clusterHits2 = _ClusterHits2;
            }
        }


        // #########################################################################################################################################################

        /// <summary>
        /// Extracts indices from a single  segment of recording
        /// EXTRACT INDICES   Default frameLength = 128 samples @ 22050 Hz = 5.805ms, @ 11025 Hz = 11.61ms.
        /// EXTRACT INDICES   Default frameLength = 256 samples @ 22050 Hz = 11.61ms, @ 11025 Hz = 23.22ms, @ 17640 Hz = 18.576ms.
        /// </summary>
        /// <param name="recording">an audio recording</param>
        /// <param name="int frameSize">number of signal samples in frame. Default = 256</param>
        /// <param name="int LowFreqBound">Do not include freq bins below this bound in estimation of indices. Default = 500 Herz.
        ///                                      This is to exclude machine noise, traffic etc which can dominate the spectrum.</param>
        /// <param name="frameSize">samples per frame</param>
        /// <returns></returns>
        public static Tuple<DataTable, TimeSpan, BaseSonogram, double[,], List<Plot>> Analysis(FileInfo fiSegmentAudioFile, AnalysisSettings analysisSettings)
        {
            Dictionary<string, string> config = analysisSettings.ConfigDict;

            // get parameters for the analysis
            int frameSize = DEFAULT_WINDOW_SIZE;
            frameSize = config.ContainsKey(Keys.FRAME_LENGTH) ? ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config) : frameSize;
            lowFreqBound = config.ContainsKey(key_LOW_FREQ_BOUND) ? ConfigDictionary.GetInt(key_LOW_FREQ_BOUND, config) : lowFreqBound;
            midFreqBound = config.ContainsKey(key_MID_FREQ_BOUND) ? ConfigDictionary.GetInt(key_MID_FREQ_BOUND, config) : midFreqBound;
            double windowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);

            // get recording segment
            AudioRecording recording = new AudioRecording(fiSegmentAudioFile.FullName);
            int signalLength       = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration   = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration*TimeSpan.TicksPerSecond));


            // i: EXTRACT ENVELOPE and FFTs
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            ////double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[] signalEnvelope   = results2.Envelope;

            // amplitude spectrogram
            double[,] spectrogramData = results2.Spectrogram;  


            // ii: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            var bgNoiseResults = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(signalEnvelope));
            var dBarray  = SNR.TruncateNegativeValues2Zero(bgNoiseResults.DBFrames);


            // iii: ACTIVITY and SEGMENT STATISTICS for NOISE REDUCED ARRAY
            var activity = CalculateActivity(dBarray, frameDuration, DEFAULT_activityThreshold_dB);

            Indices2 indices; // struct in which to store all indices
            indices.activity = activity.activeFrameCover;                   // fraction of frames having acoustic activity 
            indices.bgNoise  = bgNoiseResults.NoiseMode;                    // bg noise in dB
            indices.snr      = bgNoiseResults.Snr;                          // snr
            indices.activeSnr= activity.activeAvDB;                         // snr calculated from active frames only
            indices.avSig_dB = 20 * Math.Log10(signalEnvelope.Average());   // 10 times log of amplitude squared 
            indices.temporalEntropy   = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope)); // ENTROPY of ENERGY ENVELOPE
            indices.segmentCount      = activity.segmentCount;             //number of segments whose duration > one frame
            indices.avSegmentDuration = activity.avSegmentDuration;        //av segment duration in milliseconds


            int nyquistFreq = recording.Nyquist; 
            int nyquistBin = spectrogramData.GetLength(1)-1;
            double binWidth = nyquistFreq / (double)spectrogramData.GetLength(1);

            // calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            int originalAudioNyquist = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (nyquistFreq > originalAudioNyquist) // i.e. upsampling has been done
            {
                nyquistFreq = originalAudioNyquist;
                nyquistBin = (int)Math.Floor(originalAudioNyquist / binWidth);
            }

            var midBandSpectrogram = MatrixTools.Submatrix(spectrogramData, 0, lowBinBound, spectrogramData.GetLength(0)-1, nyquistBin);
            
            // iv: ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM - at this point the spectrogram is still an amplitude spectrogram
            var tuple = CalculateEntropyOfSpectralAvAndVariance(midBandSpectrogram);
            indices.entropyOfAvSpectrum = tuple.Item1;
            indices.entropyOfVarianceSpectrum = tuple.Item2;

            //v: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS. 
            var tuple_Peaks = CalculateEntropyOfPeakLocations(midBandSpectrogram);
            indices.entropyOfPeakFreqDistr = tuple_Peaks.Item1;

            // vi: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            double[] aciArray = AcousticComplexityIndex(midBandSpectrogram);
            indices.ACI = aciArray.Average();


            // vii: remove background noise from the full spectrogram i.e. BIN 1 to Nyquist
            spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, nyquistBin);
            const double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            double[] modalValues = SNR.CalculateModalValues(spectrogramData); // calculate modal value for each freq bin.
            modalValues = DataTools.filterMovingAverage(modalValues, 7);      // smooth the modal profile
            spectrogramData = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogramData, modalValues);
            spectrogramData = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogramData, SpectralBgThreshold);

            // viii: SPECTROGRAM ANALYSIS - SPECTRAL COVER. NOTE: spectrogram is still a noise reduced amplitude spectrogram
            var tuple_Cover = CalculateSpectralCoverage(spectrogramData, SpectralBgThreshold, lowFreqBound, midFreqBound, binWidth);
            indices.lowFreqCover = tuple_Cover.Item1;
            indices.midFreqCover = tuple_Cover.Item2;
            indices.hiFreqCover  = tuple_Cover.Item3;


            // #V#####################################################################################################################################################
            // ix:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = null;
            var scores = new List<Plot>();

            bool returnSonogramInfo = false;
            if (config.ContainsKey(Keys.SAVE_SONOGRAMS)) returnSonogramInfo = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, config);

            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); //default values config
                sonoConfig.SourceFName = recording.FileName;
                sonoConfig.WindowSize = 1024;
                sonoConfig.WindowOverlap = 0.0;
                sonoConfig.NoiseReductionType = NoiseReductionType.NONE;
                bool doNoiseReduction = false;
                if (config.ContainsKey(Keys.NOISE_DO_REDUCTION)) doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, config);
                if (doNoiseReduction) sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
                sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
                scores.Add(new Plot("Decibels", DataTools.normalise(dBarray), AcousticFeatures.DEFAULT_activityThreshold_dB));
                scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

                //convert spectral peaks to frequency
                int[] peaksBins = tuple_Peaks.Item2;
                double[] freqPeaks = new double[peaksBins.Length];
                int binCount = sonogram.Data.GetLength(1);
                for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowBinBound + peaksBins[i]) / (double)nyquistBin;
                scores.Add(new Plot("Max Frequency", freqPeaks, 0.0));  // location of peaks for spectral images
            }


            //#V#####################################################################################################################################################
            //return if activeFrameCount too small or segmentCount = 0  because no point doing clustering
            if ((activity.activeFrameCount <= 2)||(indices.segmentCount == 0))   
            {
                indices.clusterCount = 0;
                indices.avClusterDuration = TimeSpan.Zero; //av cluster durtaion in milliseconds

                return Tuple.Create(Indices2DataTable(indices), wavDuration, sonogram, hits, scores);
            }
            //#V#####################################################################################################################################################

            //viii: CLUSTERING - to determine spectral diversity and spectral persistence
            // First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB. An amplitude threshold of 0.05 = -26dB.
            double binaryThreshold = 0.03;                                        // for deriving binary spectrogram
            ClusterInfo clusterInfo = ClusterAnalysis(spectrogramData, activity.segmentLocations, lowBinBound, binaryThreshold);
            indices.clusterCount = clusterInfo.clusterCount ; 
            indices.avClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds); //av cluster duration


            //iii: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                //bool[] selectedFrames = tuple_Clustering.Item3;
                //scores.Add(DataTools.Bool2Binary(selectedFrames));
                //List<double[]> clusterWts = tuple_Clustering.Item4;
                int[] clusterHits = clusterInfo.clusterHits2;
                string label = String.Format(indices.clusterCount + " Clusters");
                if (clusterHits == null) clusterHits = new int[dBarray.Length];      // array of zeroes
                scores.Add(new Plot(label, DataTools.normalise(clusterHits), 0.0));  // location of cluster hits

                //no need for the following line in normal usage - mostly for debugging
                //double[,] clusterSpectrogram = AssembleClusterSpectrogram(signalLength, spectrogram, excludeBins, selectedFrames, binaryThreshold, clusterWts, clusterHits);
                //MakeAndDrawSonogram(recording, recordingDir, scores, clusterSpectrogram);
            }

            return Tuple.Create(Indices2DataTable(indices), wavDuration, sonogram, hits, scores);
        } //Analysis()



        public static double CalculateSpikeIndex(double[] envelope, double spikeThreshold)
        {
            int length = envelope.Length;
            int isolatedSpikeCount = 0;

            //int spikeCount = 0;
            //for (int i = 1; i < length-1; i++)
            //{
            //    if (envelope[i] < spikeThreshold) continue; //count spikes
            //    spikeCount++;
            //    if ((envelope[i-1] < spikeThreshold) && (envelope[i+1] < spikeThreshold)) 
            //        isolatedSpikeCount++; //count isolated spikes
            //}
            //if (spikeCount == 0) return 0.0;
            //else
            //    //return isolatedSpikeCount / (double)spikeCount;
            //    return isolatedSpikeCount / (double)length;

            var peaks = DataTools.GetPeaks(envelope);
            int peakCount = 0;
            for (int i = 1; i < length - 1; i++)
            {
                if (!peaks[i]) continue; //count spikes
                peakCount++;
                double diffMinus1 = envelope[i] - envelope[i - 1];
                double diffPlus1  = envelope[i] - envelope[i + 1];
                if ((diffMinus1 > spikeThreshold) && (diffPlus1 > spikeThreshold))
                    isolatedSpikeCount++; //count isolated spikes
            }
            if (peakCount == 0) return 0.0;
            if (isolatedSpikeCount == 0) return 0.0;
            return isolatedSpikeCount / (double)peakCount;
        }

        /// <summary>
        /// reutrns the number of active frames and acoustic segments and their average duration in milliseconds
        /// only counts a segment if it is LONGER than one frame. 
        /// count segments as number of transitions from active to non-active frame
        /// </summary>
        /// <param name="activeFrames"></param>
        /// <param name="frameDuration">frame duration in seconds</param>
        /// <returns></returns>
        public static Activity CalculateActivity(double[] dBarray, TimeSpan frameDuration, double db_Threshold)
        {
            bool[] activeFrames = new bool[dBarray.Length];
            bool[] segments     = new bool[dBarray.Length];
            double activeAvDB = 0.0;
            int activeFrameCount = 0;

            // get frames with activity >= threshold dB above background and count
            for (int i = 0; i < dBarray.Length; i++)
            {
                if (dBarray[i] >= DEFAULT_activityThreshold_dB)
                {
                    activeFrames[i] = true;
                    activeAvDB += dBarray[i];
                    activeFrameCount++;
                }
            }

            //int activeFrameCount = dBarray.Count((x) => (x >= AcousticIndices.DEFAULT_activityThreshold_dB));  // this more elegant but want to keep active frame array
            double percentActivity = activeFrameCount / (double)dBarray.Length;
            if (activeFrameCount != 0) activeAvDB /= (double)activeFrameCount;

            if (activeFrameCount <= 1)
                return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, 0, TimeSpan.Zero, segments);


            // store record of segments longer than one frame
            segments = activeFrames;
            for (int i = 1; i < activeFrames.Length - 1; i++)
            {
                if (!segments[i - 1] && segments[i] && !segments[i + 1]) 
                    segments[i] = false; //remove solitary active frames
            }

            int segmentCount = 0;
            for (int i = 2; i < activeFrames.Length; i++)
            {
                if (!segments[i] && segments[i - 1] && segments[i - 2]) //count the ends of active segments
                    segmentCount++; 
            }

            if (segmentCount == 0)
                return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, segmentCount, TimeSpan.Zero, segments);

            int segmentFrameCount = DataTools.CountTrues(segments);
            var avSegmentDuration = TimeSpan.Zero;

            if (segmentFrameCount > 0) 
                avSegmentDuration = TimeSpan.FromSeconds(frameDuration.TotalSeconds * segmentFrameCount / (double)segmentCount);   //av segment duration in milliseconds

            return new Activity(activeFrames, activeFrameCount, percentActivity, activeAvDB, segmentCount, avSegmentDuration, segments);
        } // CalculateActivity()


        /// <summary>
        /// returns fraction coverage of the low, middle and high freq bands of the spectrum
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="bgThreshold"></param>
        /// <param name="lowFreqBound">Herz</param>
        /// <param name="midFreqBound">Herz</param>
        /// <param name="nyquist">Herz</param>
        /// <param name="binWidth">Herz per bin i.e. column in spectrogram - spectrogram rotated wrt to normal view.</param>
        /// <returns></returns>
        public static Tuple<double, double, double> CalculateSpectralCoverage(double[,] spectrogram, double bgThreshold, int lowFreqBound, int midFreqBound, double binWidth)
        {
            //calculate boundary between hi, mid and low frequency spectrum
            //int freqBinCount = spectrogram.GetLength(1);
            int lowFreqBinIndex  = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinIndex  = (int)Math.Ceiling(midFreqBound / binWidth);
            int highFreqBinIndex = spectrogram.GetLength(1);

            int hfCoverage  = 0;
            int mfCoverage  = 0;
            int lfCoverage  = 0;
            int hfCellCount = 0;
            int mfCellCount = 0;
            int lfCellCount = 0;
            for (int i = 0; i < spectrogram.GetLength(0); i++) //for all rows of spectrogram
            {
                for (int j = 0; j < lowFreqBinIndex; j++) //caluclate coverage for low freq band
                {
                    if (spectrogram[i, j] >= bgThreshold) lfCoverage++;
                    lfCellCount++;
                    spectrogram[i, j] = 0.0;
                }
                for (int j = lowFreqBinIndex; j < midFreqBinIndex; j++) //caluclate coverage for mid freq band
                {
                    if (spectrogram[i, j] >= bgThreshold) mfCoverage++;
                    mfCellCount++;
                }
                for (int j = midFreqBinIndex; j < highFreqBinIndex; j++) //caluclate coverage for high freq band
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
        /// Returns the ENTROPY OF DISTRIBUTION of SPECTRAL maxima.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="peakThreshold">required amplitude threshold to qualify as peak</param>
        /// <param name="nyquistFreq"></param>
        /// <returns></returns>
        public static Tuple<double, int[]> CalculateEntropyOfPeakLocations(double[,] spectrogram)
        {
            int frameCount   = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            int[] peakBins  = new int[frameCount];   // store bin id of peaks - use later for imaging purposes
            int[] histogram = new int[freqBinCount]; // histogram of peak locations
            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);

                int j = DataTools.GetMaxIndex(spectrum); //locate maximum peak
                //if (spectrogram[r, j] > peakThreshold) 
                //{
                    histogram[j] ++; //
                    peakBins[r] = j;  //store bin of peak
                //}
            } // over all frames in dB array

            //DataTools.writeBarGraph(histogram);

            double entropy = DataTools.Entropy_normalised(histogram);
            if (Double.IsNaN(entropy)) entropy = 0.5;
            //Log.WriteLine("H(Spectral peaks) =" + entropy);
            return System.Tuple.Create(entropy, peakBins);
        }


        /// <summary>
        /// Returns ENTROPY OF AVERAGE SPECTRUM and VARIANCE SPECTRUM
        /// Have been passed the amplitude spectrum but square amplitude values to get power or energy.
        /// Entropy is a measure of ENERGY dispersal, therefore must square the amplitude.
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum. Must square values to get power</param>
        /// <returns></returns>
        public static Tuple<double, double> CalculateEntropyOfSpectralAvAndVariance(double[,] spectrogram)
        {
            int frameCount   = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] avSpectrum  = new double[freqBinCount];   // for average  of the spectral bins
            double[] varSpectrum = new double[freqBinCount];   // for variance of the spectral bins
            for (int j = 0; j < freqBinCount; j++)             // for all frequency bins
            {
                var freqBin = new double[frameCount];          // set up an array to take all values in a freq bin i.e. column of matrix
                for (int r = 0; r < frameCount; r++)
                {
                    freqBin[r] = spectrogram[r, j] * spectrogram[r, j];  //convert amplitude to energy or power.
                }
                double av, sd;
                NormalDist.AverageAndSD(freqBin, out av, out sd);
                avSpectrum[j]  = av;      //store average  of the bin
                varSpectrum[j] = sd * sd; //store variance of the bin
            }

            double HSpectralAv = DataTools.Entropy_normalised(avSpectrum);     //ENTROPY of spectral averages
            if (double.IsNaN(HSpectralAv)) HSpectralAv = 0.5;
            //DataTools.writeBarGraph(avSpectrum);
            //Log.WriteLine("H(Spectral averages) =" + HSpectralAv);

            double HSpectralVar = DataTools.Entropy_normalised(varSpectrum);   //ENTROPY of spectral variances
            if (double.IsNaN(HSpectralVar)) HSpectralVar = 0.5;
            //DataTools.writeBarGraph(varSpectrum);
            //Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);

            return System.Tuple.Create(HSpectralAv, HSpectralVar);
        } // CalculateEntropyOfSpectralAvAndVariance()


        /// <summary>
        /// Returns an array of ACOUSTIC COMPLEXITY INDICES
        /// This implements the index of N. Pieretti, A. Farina, D. Morri
        /// in "A new methodology to infer the singing activity of an avian community: The Acoustic Complexity Index (ACI)"
        /// in Ecological Indicators 11 (2011) pp868–873
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum.</param>
        /// <returns></returns>
        public static double[] AcousticComplexityIndex(double[,] spectrogram)
        {
            int frameCount   = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] aciArray = new double[freqBinCount];      // array of acoustic complexity indices, one for each freq bin
            for (int j = 0; j < freqBinCount; j++)             // for all frequency bins
            {
                var deltaI = 0.0;          // set up an array to take all values in a freq bin i.e. column of matrix
                var sumI   = 0.0;
                for (int r = 0; r < frameCount-1; r++)
                {
                    sumI   += spectrogram[r, j];
                    deltaI += Math.Abs(spectrogram[r, j] - spectrogram[r + 1, j]);
                }
                aciArray[j] = deltaI / sumI;      //store normalised ACI value
            }
            //DataTools.writeBarGraph(aciArray);

            return aciArray;
        } // AcousticComplexityIndex()


        /// <summary>
        /// Clusters the spectra in a spectrogram. USED to determine the spectral diversity and persistence of spectral types.
        /// The spectrogram is passed as a matrix. Note that the spectrogram is in amplitude values in [0, 1];
        /// First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB.   An amplitude threhold of 0.05 = -26dB.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="activeFrames"></param>
        /// <param name="excludeBins"></param>
        /// <param name="binaryThreshold"></param>
        /// <returns></returns>
        public static ClusterInfo ClusterAnalysis(double[,] spectrogram, bool[] activeFrames, int excludeBins, double binaryThreshold)
        {
            spectrogram = DataTools.Matrix2Binary(spectrogram, binaryThreshold);  // convert to binary 

            int spectroLength = spectrogram.GetLength(0);

            double[,] subMatrix = DataTools.Submatrix(spectrogram, 0, excludeBins, spectroLength-1, spectrogram.GetLength(1) - 1);
            bool[] selectedFrames = new bool[spectroLength];
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

            // Return if no suitable training data for clustering
            if (trainingData.Count  <= 10)
            {
                double avLength = 0.0;
                return new ClusterInfo(null, avLength, selectedFrames, null);
            }

            //DO CLUSTERING - if have suitable data
            BinaryCluster.Verbose = false;
            //if (Log.Verbosity > 0) BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            double vigilance = 0.2;    //vigilance parameter - increasing this proliferates categories
                                       //if vigilance=0.1, require similairty (AND/OR) > 10%
            var tuple_Clusters = BinaryCluster.ClusterBinaryVectors(trainingData, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1        = tuple_Clusters.Item1;   //the cluster to which each frame belongs
            List<double[]> clusterWts = tuple_Clusters.Item2;
            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            double wtThreshold = 1.0; // used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold   = 4;   // used to remove wt vectors which have fewer than the threshold hits
            var tuple_output2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            int[] prunedClusterHits         = tuple_output2.Item1;
            List<double[]> prunedClusterWts = tuple_output2.Item2;

            if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(prunedClusterWts, clusterHits1);
            if (BinaryCluster.Verbose) LoggedConsole.WriteLine("pruned cluster count = {0}", prunedClusterWts.Count);
            
            // ix: AVERAGE CLUSTER DURATION - to determine spectral persistence
            //  first:  reassemble cluster hits into an array matching the original array of active frames.
            int hitCount = 0;
            int[] clusterHits2 = new int[spectroLength]; // after pruning of clusters
            for (int i = 0; i < spectroLength; i++)
            {
                if (selectedFrames[i]) // Select only frames having acoustic energy >= threshold
                {
                    clusterHits2[i] = prunedClusterHits[hitCount]; 
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
            return new ClusterInfo(prunedClusterWts, av2, selectedFrames, clusterHits2);
        }


        //#########################################################################################################################################################
        //  OTHER METHODS



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



        public static void MakeAndDrawSonogram(AudioRecording recording, List<double[]> scores, double[,] hits)
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


            //ii: DRAW SONOGRAM AND SCORES
            //Log.WriteLine("# Draw sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            int length = sonogram.FrameCount;

            using (Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //add time scale
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                //int maxindex = DataTools.GetMaxIndex(array);

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
            LoggedConsole.WriteLine("Sum = " + histo.Sum());
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
                     double[] array = DataTableTools.Column2ArrayOfDouble(dt, colName);
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


        public static string FormatHeader(string parmasFile_Separator)
        {
            string reportSeparator = "\t";
            if (parmasFile_Separator.Equals("CSV")) reportSeparator = ",";

            string line = string.Format(FORMAT_STR_HEADERS, reportSeparator, HEADERS[0], HEADERS[1], HEADERS[2], HEADERS[3], HEADERS[4], HEADERS[5], HEADERS[6], HEADERS[7],
                                                                        HEADERS[8], HEADERS[9], HEADERS[10], HEADERS[11], HEADERS[12], HEADERS[13], HEADERS[14], HEADERS[15], HEADERS[16],HEADERS[17], HEADERS[18]);
            return line;
        }

        public static void WriteHeaderToReportFile(string reportfileName, string parmasFile_Separator)
        {
            string line = FormatHeader(parmasFile_Separator);
            FileTools.WriteTextFile(reportfileName, line);
        }

        public static DataTable Indices2DataTable(Indices2 indices)
        {
            var parameters = InitOutputTableColumns();
            var headers = parameters.Item1;
            var types   = parameters.Item2;
            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(0, 0.0, 0.0, //add dummy values to the first two columns. These will be entered later.
                        indices.avSig_dB, indices.snr, indices.bgNoise,
                        indices.activity, indices.segmentCount, indices.avSegmentDuration.TotalMilliseconds, indices.hiFreqCover, indices.midFreqCover, indices.lowFreqCover, 
                        indices.temporalEntropy, indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectrum, 
                        indices.ACI, 
                        indices.clusterCount, indices.avClusterDuration.TotalMilliseconds);
            return dt;
        }



        //############################################################################################################################################################

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

            // ignore last line
            for (int i = 1; i < lines.Count - 1; i++) 
            {
                string[] words = lines[i].Split(',');
                int speciesCount = int.Parse(words[1]);
                speciesTotal += speciesCount;
                string[] splitTime = words[0].Split(':');
                int hour = int.Parse(splitTime[0]);
                int min = int.Parse(splitTime[1]);
                minTotal = (hour * 60) + min;
                if (minTotal > minPrev + 1)
                {
                    for (int j = minPrev + 1; j < minTotal; j++)
                    {
                        line = string.Format("{0}  time={1}:{2}   Count={3}", j, j / 60, j % 60, 0);
                        LoggedConsole.WriteLine(line);
                        line = string.Format("{0},{1}:{2},{3}", j, j / 60, j % 60, 0);
                        FileTools.Append2TextFile(opFile, line);
                    }
                }

                line = string.Format("{0}  time={1}:{2}   Count={3}", minTotal, hour, min, speciesCount);
                LoggedConsole.WriteLine(line);
                line = string.Format("{0},{1}:{2},{3}", minTotal, hour, min, speciesCount);
                FileTools.Append2TextFile(opFile, line);
                minPrev = minTotal;
            }

            // fill in misisng minutes at end.
            int minsIn24hrs = 24 * 60;
            if (minsIn24hrs > minPrev + 1)
            {
                for (int j = minPrev + 1; j < minsIn24hrs; j++)
                {
                    line = string.Format("{0}  time={1}:{2}   Count={3}", j, j / 60, j % 60, 0);
                    LoggedConsole.WriteLine(line);
                    line = string.Format("{0},{1}:{2},{3}", j, j / 60, j % 60, 0);
                    FileTools.Append2TextFile(opFile, line);
                }
            }

            LoggedConsole.WriteLine("speciesTotal= " + speciesTotal);
        }
    } // class AcousticIndicesExtraction
}
