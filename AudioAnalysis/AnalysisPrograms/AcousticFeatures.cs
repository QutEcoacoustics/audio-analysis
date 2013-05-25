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
    using System.Reflection;
    using System.Text;

    using AnalysisBase;
    using AudioAnalysisTools;
    using NeuralNets;
    using TowseyLib;

    using log4net;

    public class AcousticFeatures
    {
        // public const string ANALYSIS_NAME = "Indices"; 
        public const double DEFAULT_activityThreshold_dB = 3.0; // used to select frames that have 3dB > background
        public const int DEFAULT_WINDOW_SIZE = 256;
        public static int lowFreqBound = 500; // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        public static int midFreqBound = 3500;
        public const int RESAMPLE_RATE = 17640; //chose this value because it is simple fraction (4/5) of 22050Hz. However this now appears to be irrelevant.
        // public const int RESAMPLE_RATE = 22050;

        // Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public static string key_LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public static string key_MID_FREQ_BOUND = "MID_FREQ_BOUND";
        public static string key_DISPLAY_COLUMNS = "DISPLAY_COLUMNS";

        private const int COL_NUMBER = 26;

        public static string header_count = Keys.INDICES_COUNT;
        public const string header_startMin = "start-min";
        public const string header_SecondsDuration = "SegTimeSpan";
        public const string header_avAmpdB = "avAmp-dB";
        public const string header_snrdB = "snr-dB";
        public const string header_activeSnrdB = "activeSnr-dB";
        public const string header_bgdB = "bg-dB";
        public const string header_activity = "activity";
        public const string header_segCount = "segCount";
        public const string header_avSegDur = "avSegDur";
        public const string header_hfCover = "hfCover";
        public const string header_mfCover = "mfCover";
        public const string header_lfCover = "lfCover";
        public const string header_HAmpl = "H[temporal]";
        public const string header_HPeakFreq = "H[peakFreq]";
        public const string header_HAvSpectrum = "H[spectral]";
        public const string header_HVarSpectrum = "H[spectralVar]";
        public const string header_AcComplexity = "AcComplexity";
        public const string header_NumClusters = "clusterCount";
        public const string header_avClustDur = "avClustDur";
        public const string header_TrigramCount = "3gramCount";
        public const string header_TrigramRate = "av3gramRepetition";
        public const string header_SPTracksPerSec = "SpPkTracks/Sec";
        public const string header_SPTracksDur = "SpPkTracks%Dur";


        private static System.Tuple<string[], Type[], bool[], double[]> InitOutputTableColumns()
        {
            Type[] COL_TYPES = new Type[COL_NUMBER];
            string[] HEADERS = new string[COL_NUMBER];
            bool[] DISPLAY_COLUMN = new bool[COL_NUMBER];
            double[] COMBO_WEIGHTS = new double[COL_NUMBER];
            HEADERS[0] = header_count; COL_TYPES[0] = typeof(int); DISPLAY_COLUMN[0] = false; COMBO_WEIGHTS[0] = 0.0;
            HEADERS[1] = header_startMin; COL_TYPES[1] = typeof(double); DISPLAY_COLUMN[1] = false; COMBO_WEIGHTS[1] = 0.0;
            HEADERS[2] = header_SecondsDuration; COL_TYPES[2] = typeof(double); DISPLAY_COLUMN[2] = false; COMBO_WEIGHTS[2] = 0.0;
            HEADERS[3] = header_avAmpdB; COL_TYPES[3] = typeof(double); DISPLAY_COLUMN[3] = true; COMBO_WEIGHTS[3] = 0.0;
            HEADERS[4] = header_snrdB; COL_TYPES[4] = typeof(double); DISPLAY_COLUMN[4] = true; COMBO_WEIGHTS[4] = 0.0;
            HEADERS[5] = header_activeSnrdB; COL_TYPES[5] = typeof(double); DISPLAY_COLUMN[5] = true; COMBO_WEIGHTS[5] = 0.0;
            HEADERS[6] = header_bgdB; COL_TYPES[6] = typeof(double); DISPLAY_COLUMN[6] = true; COMBO_WEIGHTS[6] = 0.0;
            HEADERS[7] = header_activity; COL_TYPES[7] = typeof(double); DISPLAY_COLUMN[7] = true; COMBO_WEIGHTS[7] = 0.0;
            HEADERS[8] = header_segCount; COL_TYPES[8] = typeof(int); DISPLAY_COLUMN[8] = true; COMBO_WEIGHTS[8] = 0.0;
            HEADERS[9] = header_avSegDur; COL_TYPES[9] = typeof(double); DISPLAY_COLUMN[9] = true; COMBO_WEIGHTS[9] = 0.0;
            HEADERS[10] = header_hfCover; COL_TYPES[10] = typeof(double); DISPLAY_COLUMN[10] = true; COMBO_WEIGHTS[10] = 0.0;
            HEADERS[11] = header_mfCover; COL_TYPES[11] = typeof(double); DISPLAY_COLUMN[11] = true; COMBO_WEIGHTS[11] = 0.0;
            HEADERS[12] = header_lfCover; COL_TYPES[12] = typeof(double); DISPLAY_COLUMN[12] = true; COMBO_WEIGHTS[12] = 0.0;
            HEADERS[13] = header_HAmpl; COL_TYPES[13] = typeof(double); DISPLAY_COLUMN[13] = true; COMBO_WEIGHTS[13] = -0.2;
            HEADERS[14] = header_HPeakFreq; COL_TYPES[14] = typeof(double); DISPLAY_COLUMN[14] = false; COMBO_WEIGHTS[14] = 0.2;
            HEADERS[15] = header_HAvSpectrum; COL_TYPES[15] = typeof(double); DISPLAY_COLUMN[15] = true; COMBO_WEIGHTS[15] = 0.0;
            HEADERS[16] = header_HVarSpectrum; COL_TYPES[16] = typeof(double); DISPLAY_COLUMN[16] = false; COMBO_WEIGHTS[16] = 0.2;
            HEADERS[17] = header_AcComplexity; COL_TYPES[17] = typeof(double); DISPLAY_COLUMN[17] = true; COMBO_WEIGHTS[17] = 0.2;
            HEADERS[18] = header_NumClusters; COL_TYPES[18] = typeof(int); DISPLAY_COLUMN[18] = true; COMBO_WEIGHTS[18] = 0.2;
            HEADERS[19] = header_avClustDur; COL_TYPES[19] = typeof(double); DISPLAY_COLUMN[19] = true; COMBO_WEIGHTS[19] = 0.0;
            HEADERS[20] = header_TrigramCount; COL_TYPES[20] = typeof(int); DISPLAY_COLUMN[20] = true; COMBO_WEIGHTS[20] = 0.0;
            HEADERS[21] = header_TrigramRate; COL_TYPES[21] = typeof(double); DISPLAY_COLUMN[21] = true; COMBO_WEIGHTS[21] = 0.0;
            HEADERS[22] = header_SPTracksPerSec; COL_TYPES[22] = typeof(double); DISPLAY_COLUMN[22] = true; COMBO_WEIGHTS[22] = 0.0;
            HEADERS[23] = header_SPTracksDur; COL_TYPES[23] = typeof(double); DISPLAY_COLUMN[23] = true; COMBO_WEIGHTS[23] = 0.0;
            HEADERS[24] = Rain.header_rain; COL_TYPES[24] = typeof(double); DISPLAY_COLUMN[24] = true; COMBO_WEIGHTS[24] = 0.0;
            HEADERS[25] = Rain.header_cicada; COL_TYPES[25] = typeof(double); DISPLAY_COLUMN[25] = true; COMBO_WEIGHTS[25] = 0.0;
            //HEADERS[26] = "Weighted index"; COL_TYPES[26] = typeof(double); DISPLAY_COLUMN[26] = false; COMBO_WEIGHTS[26] = 0.0;
            return Tuple.Create(HEADERS, COL_TYPES, DISPLAY_COLUMN, COMBO_WEIGHTS);
        }


        public static double[] GetComboWeights()
        {
            var items = InitOutputTableColumns();
            return items.Item4; // COMBO_WEIGHTS;
        }

        // CONST string for referring to indicies - these should really be an enum                
        public const string BackgroundNoiseKey = "backgroundNoise";

        public const string AcousticComplexityIndexKey = "acousticComplexityIndex";

        public const string AverageKey = "average";

        public const string VarianceKey = "variance";

        public const string BinCoverageKey = "binCoverage";

        public const string TemporalEntropyKey = "temporalEntropy";

        public const string CombinationKey = "combination";

        // NORMALISING CONSTANTS FOR EXTRACTED FEATURES
        public const double AVG_MIN = -7.0;
        public const double AVG_MAX = 0.5;
        public const double VAR_MIN = -10.0;
        public const double VAR_MAX = 0.5;
        public const double BGN_MIN = -7.0;
        public const double BGN_MAX = 0.5;
        public const double ACI_MIN = 0.3;
        public const double ACI_MAX = 0.7;
        public const double CVR_MIN = 0.0;
        public const double CVR_MAX = 0.8;
        public const double TEN_MIN = 0.5;
        public const double TEN_MAX = 1.0;

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


            public Parameters(double _segmentDuration, int _segmentOverlap, int _resampleRate,
                              int _frameLength, int _frameOverlap, int _lowFreqBound, int _DRAW_SONOGRAMS, string _fileFormat)
            {
                this.SegmentDuration = _segmentDuration;
                this.SegmentOverlap = _segmentOverlap;
                this.ResampleRate = _resampleRate;
                this.FrameLength = _frameLength;
                this.FrameOverlap = _frameOverlap;
                this.LowFreqBound = _lowFreqBound;
                // DRAW_SONOGRAMS  = _DRAW_SONOGRAMS; // av length of clusters > 1 frame.
                // reportFormat    = _fileFormat;
            } // Parameters
        } // struct Parameters


        /// <summary>
        /// a set of indices to describe level of acoustic activity in recording.
        /// </summary>
        public struct Activity
        {
            public double activeFrameCover, activeAvDB;
            public TimeSpan avSegmentDuration;
            public int activeFrameCount, segmentCount;
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
        /// a set of indices or features derived from each recording.
        /// </summary>
        public struct Features
        {
            // the following are scalar indices 
            public double snr, activeSnr, bgNoise, activity, avSig_dB, temporalEntropy; //amplitude indices
            public double lowFreqCover, midFreqCover, hiFreqCover;
            public double entropyOfPeakFreqDistr, entropyOfAvSpectrum, entropyOfVarianceSpectrum; //spectral indices
            public double ACI; // acoustic complexity index
            public double rainScore, cicadaScore;
            public int segmentCount, clusterCount;
            public TimeSpan recordingDuration, avSegmentDuration, avClusterDuration, trackDuration_total;
            public int triGramUniqueCount;
            public double triGramRepeatRate;
            public int trackCount, trackDuration_percent;
            public double tracksPerSec;

            // the following are vector spectra 
            public double[] bgNoiseSpectrum, ACIspectrum, averageSpectrum, varianceSpectrum, coverSpectrum, HtSpectrum, comboSpectrum;

            public Features(TimeSpan _recordingDuration, double _snr, double _activeSnr, double _bgNoise, double _activity, TimeSpan _avSegmentDuration, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _peakFreqEntropy, double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, double _ACI,
                            int _clusterCount, TimeSpan _avClusterDuration, int _triGramUniqueCount, double _triGramRepeatRate,
                            TimeSpan _trackDuration_total, int _trackDuration_percent, int _trackCount, double _rainScore, double _cicadaScore,
                            double[] _bgNoiseSpectrum, double[] _ACIspectrum, double[] _averageSpectrum, double[] _varianceSpectrum,
                            double[] _coverSpectrum, double[] _HtSpectrum, double[] _comboSpectrum)
            {
                recordingDuration = _recordingDuration;
                snr = _snr;
                activeSnr = _activeSnr;
                bgNoise = _bgNoise;
                activity = _activity;
                segmentCount = _segmentCount;
                avSegmentDuration = _avSegmentDuration;
                avSig_dB = _avSig_dB;
                temporalEntropy = _entropyAmp;
                hiFreqCover = _hiFreqCover;
                midFreqCover = _midFreqCover;
                lowFreqCover = _lowFreqCover;

                entropyOfPeakFreqDistr = _peakFreqEntropy;
                entropyOfAvSpectrum = _entropyOfAvSpectrum;
                entropyOfVarianceSpectrum = _entropyOfVarianceSpectrum;
                ACI = _ACI;

                clusterCount = _clusterCount;
                avClusterDuration = _avClusterDuration; //av length of clusters > 1 frame.
                triGramUniqueCount = _triGramUniqueCount; // unique cluster trigrams
                triGramRepeatRate = _triGramRepeatRate;  // average repetitions of each cluster trigram

                trackDuration_total = _trackDuration_total;
                trackCount = _trackCount;
                trackDuration_percent = _trackDuration_percent;
                tracksPerSec = _trackCount / recordingDuration.TotalSeconds;

                rainScore = _rainScore;
                cicadaScore = _cicadaScore;

                // assign spectra
                bgNoiseSpectrum = _bgNoiseSpectrum;
                ACIspectrum = _ACIspectrum;
                averageSpectrum = _averageSpectrum;
                varianceSpectrum = _varianceSpectrum;
                coverSpectrum = _coverSpectrum;
                HtSpectrum = _HtSpectrum;
                comboSpectrum = _comboSpectrum;
            }
        } // struct Features


        public struct ClusterInfo
        {
            public int clusterCount;
            public double av2;
            public bool[] selectedFrames;
            public List<double[]> prunedClusterWts;
            public int[] clusterHits2;
            public double triGramRepeatRate;
            public int triGramUniqueCount;
            public ClusterInfo(List<double[]> _PrunedClusterWts, double _av2, bool[] _SelectedFrames, int[] _ClusterHits2, int _triGramUniqueCount, double _triGramRepeatRate)
            {
                clusterCount = 0;
                if (_PrunedClusterWts != null)
                {
                    clusterCount = _PrunedClusterWts.Count;
                    if (_PrunedClusterWts[0] == null)
                        clusterCount = _PrunedClusterWts.Count - 1; // because a null at the zero position implies not belonging to a cluster.
                }
                av2 = _av2;
                selectedFrames = _SelectedFrames;
                prunedClusterWts = _PrunedClusterWts;
                clusterHits2 = _ClusterHits2;
                triGramUniqueCount = _triGramUniqueCount;
                triGramRepeatRate = _triGramRepeatRate;
            }
        }

        /// <summary>
        /// this struct describes spectral tracks ie whistles and chirps.
        /// </summary>
        public struct TrackInfo
        {
            public List<SpectralTrack> tracks;
            public TimeSpan totalTrackDuration;
            public int percentDuration; // percent of recording length
            public TrackInfo(List<SpectralTrack> _tracks, TimeSpan _totalTrackDuration, int _percentDuration)
            {
                tracks = _tracks;
                totalTrackDuration = _totalTrackDuration;
                percentDuration = _percentDuration;
            }
        } // TrackInfo()




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
        public static Tuple<Features, TimeSpan, BaseSonogram, double[,], List<Plot>, List<SpectralTrack>> Analysis(FileInfo fiSegmentAudioFile, AnalysisSettings analysisSettings)
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
            int signalLength = recording.GetWavReader().Samples.Length;
            TimeSpan wavDuration = TimeSpan.FromSeconds(recording.GetWavReader().Time.TotalSeconds);
            double duration = frameSize * (1 - windowOverlap) / (double)recording.SampleRate;
            TimeSpan frameDuration = TimeSpan.FromTicks((long)(duration * TimeSpan.TicksPerSecond));


            // i: EXTRACT ENVELOPE and FFTs
            var results2 = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            ////double[] avAbsolute = results2.Item1; //average absolute value over the minute recording
            double[] signalEnvelope = results2.Envelope;

            // get amplitude spectrogram and remove the DC column ie column zero.
            double[,] spectrogramData = results2.Spectrogram;
            spectrogramData = MatrixTools.Submatrix(spectrogramData, 0, 1, spectrogramData.GetLength(0) - 1, spectrogramData.GetLength(1) - 1);
            int nyquistFreq = recording.Nyquist;
            double binWidth = nyquistFreq / (double)spectrogramData.GetLength(1);
            int nyquistBin = spectrogramData.GetLength(1) - 1;


            // ii: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            var bgNoiseResults = SNR.SubtractBackgroundNoise_dB(SNR.Signal2Decibels(signalEnvelope));
            var dBarray = SNR.TruncateNegativeValues2Zero(bgNoiseResults.DBFrames);


            // iii: ACTIVITY and SEGMENT STATISTICS for NOISE REDUCED ARRAY
            var activity = CalculateActivity(dBarray, frameDuration, DEFAULT_activityThreshold_dB);

            Features indices; // struct in which to store all indices
            indices.recordingDuration = wavDuration;                        // total duration of recording
            indices.activity = activity.activeFrameCover;                   // fraction of frames having acoustic activity 
            indices.bgNoise = bgNoiseResults.NoiseMode;                    // bg noise in dB
            indices.snr = bgNoiseResults.Snr;                               // snr
            indices.activeSnr = activity.activeAvDB;                         // snr calculated from active frames only
            indices.avSig_dB = 20 * Math.Log10(signalEnvelope.Average());   // 10 times log of amplitude squared 
            indices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope)); // ENTROPY of ENERGY ENVELOPE
            indices.segmentCount = activity.segmentCount;              //number of segments whose duration > one frame
            indices.avSegmentDuration = activity.avSegmentDuration;         //av segment duration in milliseconds


            // calculate the bin id of boundary between mid and low frequency spectrum
            int lowBinBound = (int)Math.Ceiling(lowFreqBound / binWidth);

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            int originalNyquistFreq = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (nyquistFreq > originalNyquistFreq) // i.e. upsampling has been done
            {
                nyquistFreq = originalNyquistFreq;
                nyquistBin = (int)Math.Floor(originalNyquistFreq / binWidth); // note that binwidth does not change
            }

            // iv: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            double[] aciArray = AcousticComplexityIndex(spectrogramData);
            indices.ACIspectrum = aciArray;
            int reducedLength = aciArray.Length - lowBinBound;
            double[] reducedSpectrum = DataTools.Subarray(aciArray, lowBinBound, reducedLength);  // remove low band
            indices.ACI = reducedSpectrum.Average();

            // v: Calculate Temporal Entropy Spectrum 
            indices.HtSpectrum = CalculateTemporalEntropySpectrum(spectrogramData);

            // vi: remove background noise from the full spectrogram
            double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            double[] modalValues = SNR.CalculateModalValues(spectrogramData); // calculate modal value for each freq bin.
            modalValues = DataTools.filterMovingAverage(modalValues, 7);      // smooth the modal profile
            spectrogramData = SNR.SubtractBgNoiseFromSpectrogramAndTruncate(spectrogramData, modalValues);
            spectrogramData = SNR.RemoveNeighbourhoodBackgroundNoise(spectrogramData, SpectralBgThreshold);
            indices.bgNoiseSpectrum = DataTools.SquareValues(modalValues);
            indices.bgNoiseSpectrum = DataTools.LogValues(indices.bgNoiseSpectrum);
            //ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            //DataTools.writeBarGraph(modalValues);

            // vii: SPECTROGRAM ANALYSIS - SPECTRAL COVER. NOTE: spectrogram is now a noise reduced amplitude spectrogram
            var tuple_Cover = CalculateSpectralCoverage(spectrogramData, SpectralBgThreshold, lowFreqBound, midFreqBound, binWidth);
            indices.lowFreqCover = tuple_Cover.Item1;
            indices.midFreqCover = tuple_Cover.Item2;
            indices.hiFreqCover = tuple_Cover.Item3;
            indices.coverSpectrum = tuple_Cover.Item4;


            // viii: ENTROPY OF AVERAGE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            // Entropy is a measure of ENERGY dispersal, therefore must square the amplitude.
            var tuple = CalculateSpectralAvAndVariance(spectrogramData);
            indices.averageSpectrum = DataTools.LogValues(tuple.Item1);
            reducedSpectrum = DataTools.Subarray(tuple.Item1, lowBinBound, reducedLength); // remove low band
            indices.entropyOfAvSpectrum = DataTools.Entropy_normalised(reducedSpectrum);     // ENTROPY of spectral averages
            if (double.IsNaN(indices.entropyOfAvSpectrum)) indices.entropyOfAvSpectrum = 1.0;

            // ix: ENTROPY OF VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            indices.varianceSpectrum = DataTools.LogValues(tuple.Item2);
            reducedSpectrum = DataTools.Subarray(tuple.Item2, lowBinBound, reducedLength);       // remove low band
            indices.entropyOfVarianceSpectrum = DataTools.Entropy_normalised(reducedSpectrum);   // ENTROPY of spectral variances
            if (double.IsNaN(indices.entropyOfVarianceSpectrum)) indices.entropyOfVarianceSpectrum = 1.0;
            // DataTools.writeBarGraph(indices.varianceSpectrum);
            // Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);


            // EXTRACT High band SPECTROGRAM which is now noise reduced
            var midBandSpectrogram = MatrixTools.Submatrix(spectrogramData, 0, lowBinBound, spectrogramData.GetLength(0) - 1, nyquistBin);

            // x: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS. 
            var tuple_Peaks = HistogramOfSpectralPeaks(midBandSpectrogram);
            indices.entropyOfPeakFreqDistr = DataTools.Entropy_normalised(tuple_Peaks.Item1);
            if (Double.IsNaN(indices.entropyOfPeakFreqDistr)) indices.entropyOfPeakFreqDistr = 1.0;

            // xi: Get Spectral tracks
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            double threshold = 0.005;
            TrackInfo trackInfo = GetTrackIndices(midBandSpectrogram, framesPerSecond, binWidth, lowFreqBound, threshold);
            indices.trackDuration_total = trackInfo.totalTrackDuration;
            indices.trackDuration_percent = trackInfo.percentDuration;
            indices.trackCount = trackInfo.tracks.Count;
            indices.tracksPerSec = trackInfo.tracks.Count / wavDuration.TotalSeconds;

            //######################################################################
            // xii: calculate RAIN and CICADA indices.
            indices.rainScore = 0.0;
            indices.cicadaScore = 0.0;
            DataTable dt = Rain.GetIndices(signalEnvelope, wavDuration, frameDuration, spectrogramData, lowFreqBound, midFreqBound, binWidth);
            if (dt != null)
            {
                DataRow row = dt.Rows[0];
                indices.rainScore = (double)row[Rain.header_rain];
                indices.cicadaScore = (double)row[Rain.header_cicada];
            }

            // #V#####################################################################################################################################################
            // xiii: calculate the COMBO INDEX from equal wieghted normalised indices.
            indices.comboSpectrum = new double[spectrogramData.GetLength(1)];
            for (int i = 0; i < indices.comboSpectrum.Length; i++)
            {
                double cover = indices.coverSpectrum[i];
                cover = DataTools.NormaliseInZeroOne(cover, CVR_MIN, CVR_MAX);
                double aci = indices.ACIspectrum[i];
                aci = DataTools.NormaliseInZeroOne(aci, ACI_MIN, ACI_MAX);
                double entropy = indices.HtSpectrum[i];
                entropy = DataTools.NormaliseInZeroOne(entropy, TEN_MIN, TEN_MAX);
                entropy = 1 - entropy;
                double avg = indices.averageSpectrum[i];
                avg = DataTools.NormaliseInZeroOne(avg, AVG_MIN, AVG_MAX);
                indices.comboSpectrum[i] = (cover + aci + entropy + avg) / (double)4;
            }

            // #V#####################################################################################################################################################
            // xiv:  set up other info to return
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



            // ######################################################################################################################################################
            // return if activeFrameCount too small or segmentCount = 0  because no point doing clustering
            if ((activity.activeFrameCount <= 2) || (indices.segmentCount == 0))
            {
                indices.clusterCount = 0;
                indices.avClusterDuration = TimeSpan.Zero; //av cluster durtaion in milliseconds
                indices.triGramUniqueCount = 0;
                indices.triGramRepeatRate = 0.0;
                return Tuple.Create(indices, wavDuration, sonogram, hits, scores, trackInfo.tracks);
            }
            //#V#####################################################################################################################################################

            // xv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband spectrum
            double binaryThreshold = 0.07; // for deriving binary spectrogram
            ClusterInfo clusterInfo = ClusterAnalysis(midBandSpectrogram, binaryThreshold);
            indices.clusterCount = clusterInfo.clusterCount;
            indices.avClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds); //av cluster duration
            indices.triGramUniqueCount = clusterInfo.triGramUniqueCount;
            indices.triGramRepeatRate = clusterInfo.triGramRepeatRate;


            //TO DO: calculate av track duration and total duration as fraction of recording duration
            //indices.avTrackDuration = indices.avTrackDuration / (double)tracks.Count;

            //wavDuration

            // xvi: STORE CLUSTERING IMAGES
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

            return Tuple.Create(indices, wavDuration, sonogram, hits, scores, trackInfo.tracks);
        } //Analysis()



        public static double CalculateSpikeIndex(double[] envelope, double spikeThreshold)
        {
            int length = envelope.Length;
            // int isolatedSpikeCount = 0;
            double peakIntenisty = 0.0;
            double spikeIntensity = 0.0;

            var peaks = DataTools.GetPeaks(envelope);
            int peakCount = 0;
            for (int i = 1; i < length - 1; i++)
            {
                if (!peaks[i]) continue; //count spikes
                peakCount++;
                double diffMinus1 = Math.Abs(envelope[i] - envelope[i - 1]);
                double diffPlus1 = Math.Abs(envelope[i] - envelope[i + 1]);
                double avDifference = (diffMinus1 + diffPlus1) / 2;
                peakIntenisty += avDifference;
                if (avDifference > spikeThreshold)
                {
                    //isolatedSpikeCount++; // count isolated spikes
                    spikeIntensity += avDifference;
                }
            }
            if (peakCount == 0) return 0.0;
            return spikeIntensity / peakIntenisty;
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
            bool[] segments = new bool[dBarray.Length];
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
        public static Tuple<double, double, double, double[]> CalculateSpectralCoverage(double[,] spectrogram, double bgThreshold, int lowFreqBound, int midFreqBound, double binWidth)
        {
            //calculate boundary between hi, mid and low frequency spectrum
            //int freqBinCount = spectrogram.GetLength(1);
            int lowFreqBinIndex = (int)Math.Ceiling(lowFreqBound / binWidth);
            int midFreqBinIndex = (int)Math.Ceiling(midFreqBound / binWidth);
            int highFreqBinIndex = spectrogram.GetLength(1) - 1; // avoid top row which can have edge effects
            int rows = spectrogram.GetLength(0); // frames
            int cols = spectrogram.GetLength(1); // # of freq bins

            double[] coverSpectrum = new double[cols];
            for (int c = 0; c < cols; c++) // calculate coverage for each freq band
            {
                int cover = 0;
                for (int r = 0; r < rows; r++) // for all rows of spectrogram
                {
                    if (spectrogram[r, c] >= bgThreshold) cover++;
                }
                coverSpectrum[c] = cover / (double)rows;
            }
            //calculate coverage for low freq band
            int count = 0;
            double sum = 0;
            for (int j = 0; j < lowFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double lowFreqCover = sum / (double)count;
            //calculate coverage for mid freq band
            count = 0;
            sum = 0;
            for (int j = lowFreqBinIndex; j < midFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double midFreqCover = sum / (double)count;
            //calculate coverage for high freq band
            count = 0;
            sum = 0;
            for (int j = midFreqBinIndex; j < highFreqBinIndex; j++) { sum += coverSpectrum[j]; count++; }
            double highFreqCover = sum / (double)count;

            return System.Tuple.Create(lowFreqCover, midFreqCover, highFreqCover, coverSpectrum);
        }

        /// <summary>
        /// Returns a HISTORGRAM OF THE DISTRIBUTION of SPECTRAL maxima.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="peakThreshold">required amplitude threshold to qualify as peak</param>
        /// <param name="nyquistFreq"></param>
        /// <returns></returns>
        public static Tuple<int[], int[]> HistogramOfSpectralPeaks(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);

            int[] peakBins = new int[frameCount];   // store bin id of peaks - use later for imaging purposes
            int[] histogram = new int[freqBinCount]; // histogram of peak locations
            for (int r = 0; r < frameCount; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);

                int j = DataTools.GetMaxIndex(spectrum); //locate maximum peak
                //if (spectrogram[r, j] > peakThreshold) 
                //{
                histogram[j]++; //
                peakBins[r] = j;  //store bin of peak
                //}
            } // over all frames in dB array

            //DataTools.writeBarGraph(histogram);
            return System.Tuple.Create(histogram, peakBins);
        }


        /// <summary>
        /// Returns AVERAGE POWER SPECTRUM and VARIANCE OF POWER SPECTRUM
        /// Have been passed the amplitude spectrum but square amplitude values to get power or energy.
        /// </summary>
        /// <param name="spectrogram">this is an amplitude spectrum. Must square values to get power</param>
        /// <returns></returns>
        public static Tuple<double[], double[]> CalculateSpectralAvAndVariance(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] avgSpectrum = new double[freqBinCount];   // for average  of the spectral bins
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
                avgSpectrum[j] = av; // store average of the bin
                varSpectrum[j] = sd * sd; // store var of the bin
            }
            return System.Tuple.Create(avgSpectrum, varSpectrum);
        } // CalculateSpectralAvAndVariance()

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
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] aciArray = new double[freqBinCount];      // array of acoustic complexity indices, one for each freq bin
            for (int j = 0; j < freqBinCount; j++)             // for all frequency bins
            {
                var deltaI = 0.0;          // set up an array to take all values in a freq bin i.e. column of matrix
                var sumI = 0.0;
                for (int r = 0; r < frameCount - 1; r++)
                {
                    sumI += spectrogram[r, j];
                    deltaI += Math.Abs(spectrogram[r, j] - spectrogram[r + 1, j]);
                }
                if (sumI > 0.0) aciArray[j] = deltaI / sumI;      //store normalised ACI value
            }
            //DataTools.writeBarGraph(aciArray);

            return aciArray;
        } // AcousticComplexityIndex()


        public static double[] CalculateTemporalEntropySpectrum(double[,] spectrogram)
        {
            int frameCount = spectrogram.GetLength(0);
            int freqBinCount = spectrogram.GetLength(1);
            double[] tenSp = new double[freqBinCount];      // array of H[t] indices, one for each freq bin
            for (int j = 0; j < freqBinCount; j++)         // for all frequency bins
            {
                double[] column = MatrixTools.GetColumn(spectrogram, j);
                tenSp[j] = DataTools.Entropy_normalised(DataTools.SquareValues(column)); // ENTROPY of freq bin                
            }
            return tenSp;
        }


        /// <summary>
        /// Clusters the spectra in a spectrogram. USED to determine the spectral diversity and persistence of spectral types.
        /// The spectrogram is passed as a matrix. Note that the spectrogram is in amplitude values in [0, 1];
        /// First convert spectrogram to Binary using threshold. An amplitude threshold of 0.03 = -30 dB.   An amplitude threhold of 0.05 = -26dB.
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <param name="excludeBins"></param>
        /// <param name="binaryThreshold"></param>
        /// <returns></returns>
        public static ClusterInfo ClusterAnalysis(double[,] spectrogram, double binaryThreshold)
        {
            //binaryThreshold = 0.15;
            spectrogram = ImageTools.WienerFilter(spectrogram, 3);

            int spectroLength = spectrogram.GetLength(0);
            bool[] selectedFrames = new bool[spectroLength];
            var trainingData = new List<double[]>();    //training data that will be used for clustering

            double rowSumThreshold = 5.0;  //ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            int selectedFrameCount = 0;
            for (int r = 0; r < spectroLength; r++)
            {
                double[] spectrum = DataTools.GetRow(spectrogram, r);
                spectrum = DataTools.VectorReduceLength(spectrum, 3);  // reduce length of the vector by factor of N
                spectrum = DataTools.filterMovingAverage(spectrum, 3); // additional smoothing to remove noise
                //convert to binary
                for (int i = 0; i < spectrum.Length; i++)
                {
                    if (spectrum[i] >= binaryThreshold) spectrum[i] = 1.0;
                    else spectrum[i] = 0.0;
                }

                for (int i = 1; i < spectrum.Length - 1; i++)
                {
                    if ((spectrum[i] == 1.0) && (spectrum[i - 1] == 0.0) && (spectrum[i + 1] == 0.0))
                    {
                        spectrum[i] = 0.0; //remove isolated peaks.
                    }
                }

                for (int i = 1; i < spectrum.Length - 2; i++)
                {
                    if ((spectrum[i] == 1.0) && (spectrum[i + 1] == 1.0) && (spectrum[i - 1] == 0.0) && (spectrum[i + 2] == 0.0))
                    {
                        spectrum[i] = 0.0; //remove isolated peaks.
                        spectrum[i + 1] = 0.0; //remove isolated peaks.
                    }
                }

                if (spectrum.Sum() > rowSumThreshold)  //only include frames where activity exceeds threshold 
                {
                    trainingData.Add(spectrum);
                    selectedFrames[r] = true;
                    selectedFrameCount++;
                }
            }

            // Return if no suitable training data for clustering
            if (trainingData.Count <= 8)
            {
                double avLength = 0.0;
                return new ClusterInfo(null, avLength, selectedFrames, null, 0, 0);
            }

            //DO CLUSTERING - if have suitable data
            BinaryCluster.Verbose = false;
            //if (Log.Verbosity > 0) BinaryCluster.Verbose = true;
            BinaryCluster.RandomiseTrnSetOrder = false;
            double vigilance = 0.1;    //vigilance parameter - increasing this proliferates categories
            //if vigilance=0.1, require similarity (AND/OR) > 10%
            var tuple_Clusters = BinaryCluster.ClusterBinaryVectors(trainingData, vigilance);//cluster[] stores the category (winning F2 node) for each input vector
            int[] clusterHits1 = tuple_Clusters.Item1;   //the cluster to which each frame belongs
            List<double[]> clusterWts = tuple_Clusters.Item2;
            //if (BinaryCluster.Verbose) BinaryCluster.DisplayClusterWeights(clusterWts, clusterHits1);

            //PRUNE THE CLUSTERS
            double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold = 4;               // used to remove wt vectors which have fewer than the threshold hits
            var tuple_output2 = BinaryCluster.PruneClusters(clusterWts, clusterHits1, wtThreshold, hitThreshold);
            int[] prunedClusterHits = tuple_output2.Item1;
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

            int ngramValue = 3;     // length of character n-grams
            Dictionary<string, int> nGrams = TextUtilities.ConvertIntegerArray2NgramCount(clusterHits2, ngramValue);
            int triGramUniqueCount = 0;
            int repeats = 0;
            foreach (KeyValuePair<string, int> kvp in nGrams)
            {
                if (kvp.Key.Contains(",0")) continue; // not interested in ngrams with no hit
                triGramUniqueCount++;
                if (kvp.Value > 1)
                {
                    repeats += kvp.Value;
                }
            }
            double triGramRepeatRate = 0.0;
            if (triGramUniqueCount != 0) triGramRepeatRate = repeats / (double)triGramUniqueCount;

            return new ClusterInfo(prunedClusterWts, av2, selectedFrames, clusterHits2, triGramUniqueCount, triGramRepeatRate);
        }


        public static TrackInfo GetTrackIndices(double[,] spectrogram, double framesPerSecond, double binWidth, int herzOffset, double threshold)
        {
            var minDuration = TimeSpan.FromMilliseconds(150);
            var permittedGap = TimeSpan.FromMilliseconds(100);
            int maxFreq = 10000;

            var tracks = SpectralTrack.GetSpectralPeakTracks(spectrogram, framesPerSecond, binWidth, herzOffset, threshold, minDuration, permittedGap, maxFreq);
            var duration = TimeSpan.Zero;
            int trackLength = 0;
            foreach (SpectralTrack track in tracks)
            {
                duration += track.Duration();
                trackLength += track.Length;
            }
            int percentDuration = (int)Math.Round(100 * trackLength / (double)spectrogram.GetLength(0));
            return new TrackInfo(tracks, duration, percentDuration);
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
                weightedIndices[i] = combo * combo; // square the index for display purposes only. Does not change ranking.
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

        public static DataTable Indices2DataTable(Features indices)
        {
            var parameters = InitOutputTableColumns();
            var headers = parameters.Item1;
            var types = parameters.Item2;
            var dt = DataTableTools.CreateTable(headers, types);
            dt.Rows.Add(0, 0.0, 0.0, //add dummy values to the first two columns. These will be entered later.
                        indices.avSig_dB, indices.snr, indices.activeSnr, indices.bgNoise,
                        indices.activity, indices.segmentCount, indices.avSegmentDuration.TotalMilliseconds, indices.hiFreqCover, indices.midFreqCover, indices.lowFreqCover,
                        indices.temporalEntropy, indices.entropyOfPeakFreqDistr, indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectrum,
                        indices.ACI,
                        indices.clusterCount, indices.avClusterDuration.TotalMilliseconds, indices.triGramUniqueCount, indices.triGramRepeatRate,
                        indices.tracksPerSec, indices.trackDuration_percent,
                        indices.rainScore, indices.cicadaScore);

            //foreach (DataRow row in dt.Rows) { }
            return dt;
        }

        public static double[,] DrawSpectrogramsOfIndices(double[][] jaggedMatrix, string imagePath, string id, int xInterval, int yInterval)
        {
            double[,] matrix = DataTools.ConvertJaggedToMatrix(jaggedMatrix);

            DrawSpectrogramsOfIndices(matrix, imagePath, id, xInterval, yInterval);

            return matrix;
        }

        public static void DrawSpectrogramsOfIndices(string spectrogramCsvPath, string imagePath, string id, int xInterval, int yInterval)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(spectrogramCsvPath);

            DrawSpectrogramsOfIndices(matrix, imagePath, id, xInterval, yInterval);
        }

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrogramCsvPath"></param>
        /// <param name="imagePath"></param>
        /// <param name="id"></param>
        /// <param name="xInterval">pixel interval between X-axis lines</param>
        /// <param name="yInterval">pixel interval between Y-axis lines</param>
        public static void DrawSpectrogramsOfIndices(double[,] matrix, string imagePath, string id, int xInterval, int yInterval)
        {
            
            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);

            if (id == AcousticComplexityIndexKey) //.Equals("ACI"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.ACI_MIN, AcousticFeatures.ACI_MAX);
            }
            else if (id == TemporalEntropyKey)//.Equals("TEN"))
            {
                // normalise and reverse
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.TEN_MIN, AcousticFeatures.TEN_MAX);
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        matrix[r, c] = 1 - matrix[r, c];
                    }
                }
            }
            else if (id == AverageKey)//.Equals("AVG"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.AVG_MIN, AcousticFeatures.AVG_MAX);
            }
            else if (id == BackgroundNoiseKey)//.Equals("BGN"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.BGN_MIN, AcousticFeatures.BGN_MAX);
            }
            else if (id == VarianceKey)//.Equals("VAR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.VAR_MIN, AcousticFeatures.VAR_MAX);
            }
            else if (id == BinCoverageKey)//.Equals("CVR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, AcousticFeatures.CVR_MIN, AcousticFeatures.CVR_MAX);
            }
            else
            {
                Logger.Warn("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                matrix = DataTools.Normalise(matrix, 0, 1);
            }

            ImageTools.DrawMatrixWithAxes(matrix, imagePath, xInterval, yInterval);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="avgCsvPath"></param>
        /// <param name="aciCsvPath"></param>
        /// <param name="tenCsvPath"></param>
        /// <param name="imagePath"></param>
        /// <param name="colorSchemeID">Not yet used but could be used to determine type of false colour encoding</param>
        /// <param name="X_interval">pixel interval between X-axis lines</param>
        /// <param name="Y_interval">pixel interval between Y-axis lines</param>
        public static void DrawColourSpectrogramsOfIndices(string avgCsvPath, string csvAciPath, string csvTenPath, 
            string imagePath, string colorSchemeID, int X_interval, int Y_interval)
        {
            double[,] matrixAvg = PrepareSpectrogramMatrix(avgCsvPath);
            double[,] matrixAci = PrepareSpectrogramMatrix(csvAciPath);
            double[,] matrixTen = PrepareSpectrogramMatrix(csvTenPath);  // prepare, normalise and reverse

            DrawColourSpectrogramsOfIndices(imagePath, colorSchemeID, X_interval, Y_interval, matrixAvg, matrixAci, matrixTen);
        }

        public static void DrawColourSpectrogramsOfIndices(Dictionary<string, double[,]> spectrogramMatrixes, string savePath, string colorSchemeId, int xInterval, int yInterval)
        {
            DrawColourSpectrogramsOfIndices(
                savePath,
                colorSchemeId,
                xInterval,
                yInterval,
                spectrogramMatrixes[AverageKey],
                spectrogramMatrixes[AcousticComplexityIndexKey],
                spectrogramMatrixes[TemporalEntropyKey]);
        }


        public static void DrawColourSpectrogramsOfIndices(string imagePath, string colorSchemeID, int X_interval, int Y_interval, double[,] matrixAvg, double[,] matrixAci, double[,] matrixTen)
        {
    
            matrixAvg = DataTools.NormaliseInZeroOne(matrixAvg, AcousticFeatures.AVG_MIN, AcousticFeatures.AVG_MAX);
            matrixAci = DataTools.NormaliseInZeroOne(matrixAci, AcousticFeatures.ACI_MIN, AcousticFeatures.ACI_MAX);
            matrixTen = DataTools.NormaliseReverseInZeroOne( matrixTen, AcousticFeatures.TEN_MIN, AcousticFeatures.TEN_MAX);
            //int rowCount = matrixTen.GetLength(0);
            //int colCount = matrixTen.GetLength(1);
            //for (int r = 0; r < rowCount; r++)
            //{
            //    for (int c = 0; c < colCount; c++)
            //    {
            //        matrixTen[r, c] = 1 - matrixTen[r, c];
            //    }
            //}
            ImageTools.DrawColourMatrixWithAxes(matrixAvg, matrixAci, matrixTen, imagePath, X_interval, Y_interval);
        }

        public static double[,] PrepareSpectrogramMatrix(string csvPath)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            return matrix;
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
