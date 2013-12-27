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
    //using NeuralNets;
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
            public double entropyOfPeakFreqDistr;
            public double entropyOfAvSpectrum, entropyOfVarianceSpectrum; //spectral indices
            public double ACI; // acoustic complexity index
            public double rainScore, cicadaScore;
            public int segmentCount, clusterCount;
            public TimeSpan recordingDuration, avSegmentDuration, avClusterDuration, trackDuration_total;
            public int triGramUniqueCount;
            public double triGramRepeatRate;
            public int trackCount, trackDuration_percent;
            public double tracksPerSec;

            // the following are vector spectra 
            public double[] backgroundSpectrum, ACIspectrum, averageSpectrum, varianceSpectrum, coverSpectrum, HtSpectrum;

            public Features(TimeSpan _recordingDuration, double _snr, double _activeSnr, double _bgNoise, double _activity, TimeSpan _avSegmentDuration, int _segmentCount, double _avSig_dB,
                            double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                            double _peakFreqEntropy, 
                            double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, double _ACI,
                            int _clusterCount, TimeSpan _avClusterDuration, int _triGramUniqueCount, double _triGramRepeatRate,
                            TimeSpan _trackDuration_total, int _trackDuration_percent, int _trackCount, double _rainScore, double _cicadaScore,
                            double[] _bgNoiseSpectrum, double[] _ACIspectrum, double[] _averageSpectrum, double[] _varianceSpectrum,
                            double[] _coverSpectrum, double[] _HtSpectrum)
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
                backgroundSpectrum = _bgNoiseSpectrum;
                ACIspectrum = _ACIspectrum;
                averageSpectrum = _averageSpectrum;
                varianceSpectrum = _varianceSpectrum;
                coverSpectrum = _coverSpectrum;
                HtSpectrum = _HtSpectrum;
            }
        } // struct Features

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
            int frameSize = AcousticFeatures.DEFAULT_WINDOW_SIZE;
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


            // EXTRACT ENVELOPE and SPECTROGRAM
            var dspOutput = DSP_Frames.ExtractEnvelopeAndFFTs(recording.GetWavReader().Samples, recording.SampleRate, frameSize, windowOverlap);
            //double[] avAbsolute = dspOutput.Average; //average absolute value over the minute recording

            // (A) ################################## EXTRACT INDICES FROM THE SIGNAL WAVEFORM ##################################
            double[] signalEnvelope = dspOutput.Envelope;
            double avSignalEnvelope = signalEnvelope.Average();

            Features indices; // struct in which to store all indices
            int freqBinCount = frameSize / 2;

            // following deals with case where the signal waveform is continuous flat with values < 0.001. Has happened!! 
            if (avSignalEnvelope < 0.001) // although signal appears zero, this condition is required
            {
                indices.recordingDuration = wavDuration;                        // total duration of recording
                indices.activity = 0.0;                                         // fraction of frames having acoustic activity 
                indices.bgNoise = -100.0;                                       // bg noise in dB
                indices.snr = 0.0;                                              // snr
                indices.activeSnr = 0.0;                                        // snr calculated from active frames only
                indices.avSig_dB = -100.0; 
                indices.segmentCount = 0;                                       // number of segments whose duration > one frame
                indices.avSegmentDuration = TimeSpan.Zero;                      // av segment duration in milliseconds
                indices.clusterCount = 0;
                indices.avClusterDuration = TimeSpan.Zero;                      // av cluster durtaion in milliseconds
                indices.triGramUniqueCount = 0;
                indices.triGramRepeatRate = 0.0;
                indices.ACI = 0.0;
                indices.temporalEntropy = 1.00;                                 // ENTROPY of ENERGY ENVELOPE
                indices.entropyOfAvSpectrum = 1.0;
                indices.entropyOfVarianceSpectrum = 1.0;
                indices.entropyOfPeakFreqDistr = 1.0;

                indices.rainScore    = 0.0;
                indices.cicadaScore  = 0.0;
                indices.lowFreqCover = 0.0;
                indices.midFreqCover = 0.0;
                indices.hiFreqCover  = 0.0;

                indices.backgroundSpectrum = new double[freqBinCount];
                indices.ACIspectrum = new double[freqBinCount];
                indices.coverSpectrum = new double[freqBinCount];
                indices.HtSpectrum = new double[freqBinCount];
                indices.averageSpectrum = new double[freqBinCount];
                indices.varianceSpectrum = new double[freqBinCount];

                indices.trackDuration_total = TimeSpan.Zero;
                indices.trackDuration_percent = 0;
                indices.trackCount = 0;
                indices.tracksPerSec = 0.0;
                
                BaseSonogram sg = null;
                double[,] hitsDummy = null;
                var dummyScores = new List<Plot>();
                List<SpectralTrack> list = null;
                return Tuple.Create(indices, wavDuration, sg, hitsDummy, dummyScores, list);
            }

            // i: FRAME ENERGIES -
            // convert signal to decibels and subtract background noise.
            double StandardDeviationCount = 0.1; // number of noise SDs to calculate noise threshold - determines severity of noise reduction
            var bgNoise = SNR.SubtractBackgroundNoiseFromWaveform_dB(SNR.Signal2Decibels(signalEnvelope), StandardDeviationCount);
            var dBarray = SNR.TruncateNegativeValues2Zero(bgNoise.noiseReducedSignal);


            // ii: ACTIVITY and SEGMENT STATISTICS for NOISE REDUCED ARRAY
            var activity = CalculateActivity(dBarray, frameDuration, DEFAULT_activityThreshold_dB);

            indices.recordingDuration = wavDuration;                        // total duration of recording
            indices.activity = activity.activeFrameCover;                   // fraction of frames having acoustic activity 
            indices.bgNoise = bgNoise.NoiseMode;                            // bg noise in dB
            indices.snr = bgNoise.Snr;                                      // snr
            indices.activeSnr = activity.activeAvDB;                        // snr calculated from active frames only
            indices.avSig_dB = 20 * Math.Log10(signalEnvelope.Average());   // 10 times log of amplitude squared 
            indices.temporalEntropy = DataTools.Entropy_normalised(DataTools.SquareValues(signalEnvelope)); // ENTROPY of ENERGY ENVELOPE
            indices.segmentCount = activity.segmentCount;              //number of segments whose duration > one frame
            indices.avSegmentDuration = activity.avSegmentDuration;         //av segment duration in milliseconds


            // (B) ################################## EXTRACT INDICES FROM THE AMPLITUDE SPECTROGRAM ##################################           
            double[,] amplitudeSpectrogram = dspOutput.amplitudeSpectrogram; // get amplitude spectrogram.
            //int nyquistFreq = dspOutput.NyquistFreq;
            //double binWidth = dspOutput.BinWidth;
            int nyquistBin = dspOutput.NyquistBin;

            // calculate the bin id of boundary between low & mid frequency bins. This is to avoid low freq bins that contain anthrophony.
            int lowerBinBound = (int)Math.Ceiling(lowFreqBound / dspOutput.FreqBinWidth);
            // calculate reduced spectral width.
            int reducedFreqBinCount = amplitudeSpectrogram.GetLength(1) - lowerBinBound;

            // IFF there has been UP-SAMPLING, calculate bin of the original audio nyquist. this will be less than 17640/2.
            int originalNyquistFreq = (int)analysisSettings.SampleRateOfOriginalAudioFile / 2; // original sample rate can be anything 11.0-44.1 kHz.
            if (dspOutput.NyquistFreq > originalNyquistFreq) // i.e. upsampling has been done
            {
                dspOutput.NyquistFreq = originalNyquistFreq;
                dspOutput.NyquistBin  = (int)Math.Floor(originalNyquistFreq / dspOutput.FreqBinWidth); // note that binwidth does not change
            }

            // i: CALCULATE THE ACOUSTIC COMPLEXITY INDEX
            double[] aciArray = AcousticComplexityIndex(amplitudeSpectrogram);
            indices.ACIspectrum = aciArray; //store ACI spectrum
            double[] reducedSpectrum = DataTools.Subarray(aciArray, lowerBinBound, reducedFreqBinCount);  // remove low freq band
            indices.ACI = reducedSpectrum.Average(); // average of ACI spectrum with low freq bins removed

            // ii: CALCULATE H(t) or Temporal ENTROPY Spectrum 
            indices.HtSpectrum = CalculateTemporalEntropySpectrum(amplitudeSpectrogram);

            // iii: remove background noise from the amplitude spectrogram
            double SD_COUNT = 0.0;
            double SpectralBgThreshold = 0.015; // SPECTRAL AMPLITUDE THRESHOLD for smoothing background
            SNR.NoiseProfile profile = SNR.CalculateNoiseProfile(amplitudeSpectrogram, SD_COUNT); // calculate noise value for each freq bin.
            double[] noiseValues = DataTools.filterMovingAverage(profile.noiseThresholds, 7);      // smooth the modal profile
            amplitudeSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(amplitudeSpectrogram, noiseValues);
            amplitudeSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(amplitudeSpectrogram, SpectralBgThreshold);
            //ImageTools.DrawMatrix(spectrogramData, @"C:\SensorNetworks\WavFiles\Crows\image.png", false);
            //DataTools.writeBarGraph(modalValues);


            // iv: ENTROPY OF AVERAGE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            // Entropy is a measure of ENERGY dispersal, therefore must square the amplitude.
            var tuple = CalculateSpectralAvAndVariance(amplitudeSpectrogram);
            reducedSpectrum = DataTools.Subarray(tuple.Item1, lowerBinBound, reducedFreqBinCount); // remove low band
            indices.entropyOfAvSpectrum = DataTools.Entropy_normalised(reducedSpectrum);           // ENTROPY of spectral averages
            if (double.IsNaN(indices.entropyOfAvSpectrum)) indices.entropyOfAvSpectrum = 1.0;

            // v: ENTROPY OF VARIANCE SPECTRUM - at this point the spectrogram is a noise reduced amplitude spectrogram
            reducedSpectrum = DataTools.Subarray(tuple.Item2, lowerBinBound, reducedFreqBinCount); // remove low band
            indices.entropyOfVarianceSpectrum = DataTools.Entropy_normalised(reducedSpectrum);     // ENTROPY of spectral variances
            if (double.IsNaN(indices.entropyOfVarianceSpectrum)) indices.entropyOfVarianceSpectrum = 1.0;
            // DataTools.writeBarGraph(indices.varianceSpectrum);
            // Log.WriteLine("H(Spectral Variance) =" + HSpectralVar);


            // ###### SPECTRAL PEAK INDEX DISCONTINUED ON 22-11-2013
            // vi: ENTROPY OF DISTRIBUTION of maximum SPECTRAL PEAKS.
            //     First extract High band SPECTROGRAM which is now noise reduced
            var midBandSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var tuple_AmplitudePeaks = HistogramOfSpectralPeaks(midBandSpectrogram);
            indices.entropyOfPeakFreqDistr = DataTools.Entropy_normalised(tuple_AmplitudePeaks.Item1);
            if (Double.IsNaN(indices.entropyOfPeakFreqDistr)) indices.entropyOfPeakFreqDistr = 1.0;

            // viii: calculate RAIN and CICADA indices.
            indices.rainScore = 0.0;
            indices.cicadaScore = 0.0;
            DataTable dt = Rain.GetIndices(signalEnvelope, wavDuration, frameDuration, amplitudeSpectrogram, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);
            if (dt != null)
            {
                DataRow row = dt.Rows[0];
                indices.rainScore = (double)row[Rain.header_rain];
                indices.cicadaScore = (double)row[Rain.header_cicada];
            }


            // (C) ################################## EXTRACT INDICES FROM THE DECIBEL SPECTROGRAM ##################################           
                        
            // i: generate deciBel spectrogram from amplitude spectrogram
            double epsilon = Math.Pow(0.5, recording.BitsPerSample - 1);
            double[,] deciBelSpectrogram = Speech.DecibelSpectra(dspOutput.amplitudeSpectrogram, dspOutput.WindowPower, recording.SampleRate, epsilon);

            // ii: Calculate background noise spectrum in decibels
            SD_COUNT = 0.0; // number of SDs above the mean for noise removal
            SNR.NoiseProfile dBProfile = SNR.CalculateNoiseProfile(deciBelSpectrogram, SD_COUNT);       // calculate noise value for each freq bin.
            indices.backgroundSpectrum = DataTools.filterMovingAverage(dBProfile.noiseThresholds, 7);   // smooth the modal profile
            deciBelSpectrogram = SNR.TruncateBgNoiseFromSpectrogram(deciBelSpectrogram, dBProfile.noiseThresholds);
            double dBThreshold = 3.0; // SPECTRAL dB THRESHOLD for smoothing background
            deciBelSpectrogram = SNR.RemoveNeighbourhoodBackgroundNoise(deciBelSpectrogram, dBThreshold);
            //ImageTools.DrawMatrix(deciBelSpectrogram, @"C:\SensorNetworks\Output\LSKiwi3\AfterRefactoring\Towsey.Acoustic\image.png", false);
            //DataTools.writeBarGraph(indices.backgroundSpectrum);

            // iii: CALCULATE AVERAGE DECIBEL SPECTRUM - and variance spectrum 
            var tuple2 = CalculateSpectralAvAndVariance(deciBelSpectrogram);
            indices.averageSpectrum  = tuple2.Item1;
            indices.varianceSpectrum = tuple2.Item2;

            // iv: CALCULATE SPECTRAL COVER. NOTE: spectrogram is a noise reduced decibel spectrogram
            dBThreshold = 2.0; // dB THRESHOLD for calculating spectral coverage
            var tuple_Cover = CalculateSpectralCoverage(deciBelSpectrogram, dBThreshold, lowFreqBound, midFreqBound, dspOutput.FreqBinWidth);
            indices.lowFreqCover = tuple_Cover.Item1;
            indices.midFreqCover = tuple_Cover.Item2;
            indices.hiFreqCover = tuple_Cover.Item3;
            indices.coverSpectrum = tuple_Cover.Item4;

            // vii: Get Spectral tracks
            double framesPerSecond = 1 / frameDuration.TotalSeconds;
            var midBandDecibelSpectrogram = MatrixTools.Submatrix(deciBelSpectrogram, 0, lowerBinBound, deciBelSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            dBThreshold = 3.0;
            TrackInfo trackInfo = GetTrackIndices(midBandDecibelSpectrogram, framesPerSecond, dspOutput.FreqBinWidth, lowFreqBound, dBThreshold);
            indices.trackDuration_total = trackInfo.totalTrackDuration;
            indices.trackDuration_percent = trackInfo.percentDuration;
            indices.trackCount = trackInfo.tracks.Count;
            indices.tracksPerSec = trackInfo.tracks.Count / wavDuration.TotalSeconds;


            // #V#####################################################################################################################################################
            // iv:  set up other info to return
            BaseSonogram sonogram = null;
            double[,] hits = null;
            var scores = new List<Plot>();

            bool returnSonogramInfo = false;
            if (config.ContainsKey(Keys.SAVE_SONOGRAMS)) returnSonogramInfo = ConfigDictionary.GetBoolean(Keys.SAVE_SONOGRAMS, config);

            if (returnSonogramInfo)
            {
                SonogramConfig sonoConfig = new SonogramConfig(); //default values config
                sonoConfig.SourceFName = recording.FileName;
                sonoConfig.WindowSize = 1024; //the default
                if (config.ContainsKey(Keys.FRAME_LENGTH)) 
                    sonoConfig.WindowSize =  ConfigDictionary.GetInt(Keys.FRAME_LENGTH, config);
                sonoConfig.WindowOverlap = 0.0; // the default
                if (config.ContainsKey(Keys.FRAME_OVERLAP))
                    sonoConfig.WindowOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, config);
                sonoConfig.NoiseReductionType = NoiseReductionType.NONE; // the default
                bool doNoiseReduction = false;  // the default
                if (config.ContainsKey(Keys.NOISE_DO_REDUCTION)) 
                    doNoiseReduction = ConfigDictionary.GetBoolean(Keys.NOISE_DO_REDUCTION, config);
                if (doNoiseReduction) 
                    sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;

                //init sonogram
                sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
                scores.Add(new Plot("Decibels", DataTools.normalise(dBarray), AcousticFeatures.DEFAULT_activityThreshold_dB));
                scores.Add(new Plot("Active Frames", DataTools.Bool2Binary(activity.activeFrames), 0.0));

                //convert spectral peaks to frequency
                var tuple_DecibelPeaks = HistogramOfSpectralPeaks(deciBelSpectrogram);
                int[] peaksBins = tuple_DecibelPeaks.Item2;
                double[] freqPeaks = new double[peaksBins.Length];
                int binCount = sonogram.Data.GetLength(1);
                for (int i = 1; i < peaksBins.Length; i++) freqPeaks[i] = (lowerBinBound + peaksBins[i]) / (double)nyquistBin;
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

            // xiv: CLUSTERING - to determine spectral diversity and spectral persistence. Only use midband AMPLITDUE SPECTRUM
            double binaryThreshold = 0.06; // for deriving binary spectrogram
            double rowSumThreshold = 2.0;  // ACTIVITY THRESHOLD - require activity in at least N bins to include for training
            var midBandAmplSpectrogram = MatrixTools.Submatrix(amplitudeSpectrogram, 0, lowerBinBound, amplitudeSpectrogram.GetLength(0) - 1, nyquistBin - 1);
            var parameters = new SpectralClustering.ClusteringParameters(lowerBinBound, midBandAmplSpectrogram.GetLength(1), binaryThreshold, rowSumThreshold);

            SpectralClustering.TrainingDataInfo data = SpectralClustering.GetTrainingDataForClustering(midBandAmplSpectrogram, parameters);

            SpectralClustering.ClusterInfo clusterInfo;
            // cluster pruning parameters
            double wtThreshold = rowSumThreshold; // used to remove wt vectors whose sum of wts <= threshold
            int hitThreshold = 4;                 // used to remove wt vectors which have fewer than the threshold hits
            if (data.trainingData.Count <= 8)     // Skip clustering if not enough suitable training data
            {
                clusterInfo.clusterHits2 = null;
                indices.clusterCount       = 0;
                indices.avClusterDuration  = TimeSpan.Zero;
                indices.triGramUniqueCount = 0;
                indices.triGramRepeatRate  = 0;
            }
            else
            {
                clusterInfo = SpectralClustering.ClusterAnalysis(data.trainingData, wtThreshold, hitThreshold, data.selectedFrames);
                //Console.WriteLine("Cluster Count=" + clusterInfo.clusterCount);
                indices.clusterCount = clusterInfo.clusterCount;
                indices.avClusterDuration = TimeSpan.FromSeconds(clusterInfo.av2 * frameDuration.TotalSeconds); //av cluster duration
                indices.triGramUniqueCount = clusterInfo.triGramUniqueCount;
                indices.triGramRepeatRate = clusterInfo.triGramRepeatRate;
            }

            //TO DO: calculate av track duration and total duration as fraction of recording duration
            //indices.avTrackDuration = indices.avTrackDuration / (double)tracks.Count;

            //wavDuration

            // xv: STORE CLUSTERING IMAGES
            if (returnSonogramInfo)
            {
                //bool[] selectedFrames = tuple_Clustering.Item3;
                //scores.Add(DataTools.Bool2Binary(selectedFrames));
                //List<double[]> clusterWts = tuple_Clustering.Item4;
                int[] clusterHits = clusterInfo.clusterHits2;
                string label = String.Format(indices.clusterCount + " Clusters");
                if (clusterHits == null) clusterHits = new int[dBarray.Length];      // array of zeroes
                scores.Add(new Plot(label, DataTools.normalise(clusterHits), 0.0));  // location of cluster hits
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
                        indices.temporalEntropy, indices.entropyOfPeakFreqDistr, 
                        indices.entropyOfAvSpectrum, indices.entropyOfVarianceSpectrum,
                        indices.ACI,
                        indices.clusterCount, indices.avClusterDuration.TotalMilliseconds, indices.triGramUniqueCount, indices.triGramRepeatRate,
                        indices.tracksPerSec, indices.trackDuration_percent,
                        indices.rainScore, indices.cicadaScore);

            //foreach (DataRow row in dt.Rows) { }
            return dt;
        }


        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

 

        //############################################################################################################################################################

        // ############## THE FOLLOWING METHOD COULD BE DELETED - IT WAS USED ONLY IN DEC 2011!
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
            const int MinsIn24Hrs = 24 * 60;
            if (MinsIn24Hrs > minPrev + 1)
            {
                for (int j = minPrev + 1; j < MinsIn24Hrs; j++)
                {
                    line = string.Format("{0}  time={1}:{2}   Count={3}", j, j / 60, j % 60, 0);
                    LoggedConsole.WriteLine(line);
                    line = string.Format("{0},{1}:{2},{3}", j, j / 60, j % 60, 0);
                    FileTools.Append2TextFile(opFile, line);
                }
            }

            LoggedConsole.WriteLine("speciesTotal= " + speciesTotal);
        }

     
    } // class AcousticFeatures
}
