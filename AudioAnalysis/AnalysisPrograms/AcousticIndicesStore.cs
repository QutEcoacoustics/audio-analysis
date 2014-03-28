using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using TowseyLib;
using AnalysisBase;
using AudioAnalysisTools;



namespace AnalysisPrograms
{

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

        // the following are spectra as vectors of indices  
        // NB: if you create a new spectrum of indices you need to put reference for saving it in AcousticIndices.Analyse() method, line 379
        public double[] spectrum_BGN, spectrum_ACI, spectrum_AVG, spectrum_VAR, spectrum_CVR, spectrum_ENT, spectrum_CLS;

        public Features(TimeSpan _recordingDuration, double _snr, double _activeSnr, double _bgNoise, double _activity, TimeSpan _avSegmentDuration, int _segmentCount, double _avSig_dB,
                        double _entropyAmp, double _hiFreqCover, double _midFreqCover, double _lowFreqCover,
                        double _peakFreqEntropy,
                        double _entropyOfAvSpectrum, double _entropyOfVarianceSpectrum, double _ACI,
                        int _clusterCount, TimeSpan _avClusterDuration, int _triGramUniqueCount, double _triGramRepeatRate,
                        TimeSpan _trackDuration_total, int _trackDuration_percent, int _trackCount, double _rainScore, double _cicadaScore,
                        double[] _bgNoiseSpectrum, double[] _ACIspectrum, double[] _averageSpectrum, double[] _varianceSpectrum,
                        double[] _coverSpectrum, double[] _HtSpectrum, double[] _clusterSpectrum)
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
            spectrum_ACI = _ACIspectrum;
            spectrum_AVG = _averageSpectrum;
            spectrum_BGN = _bgNoiseSpectrum;
            spectrum_CLS = _clusterSpectrum;
            spectrum_CVR = _coverSpectrum;
            spectrum_ENT = _HtSpectrum;
            spectrum_VAR = _varianceSpectrum;
        }
    } // struct Features






    public class AcousticIndicesStore
    {

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

// TODO: as of 23-01-2014 these min and max bounds for display of indices are not yet referenced.
        public const double min_avAmpdB = -50.0;
        public const double min_snrdB = 0.0;
        public const double min_activeSnrdB = 0.0;
        public const double min_bgdB = 0.0;
        public const double min_activity = 0.0;
        public const double min_segCount = 0.0;
        public const double min_avSegDur = 0.0;
        public const double min_hfCover = 0.0;
        public const double min_mfCover = 0.0;
        public const double min_lfCover = 0.0;
        public const double min_HAmpl = 0.0;
        public const double min_HPeakFreq = 0.0;
        public const double min_HAvSpectrum = 0.0;
        public const double min_HVarSpectrum = 0.0;
        public const double min_AcComplexity = 0.0;
        public const double min_NumClusters = 0.0;
        public const double min_avClustDur = 0.0;
        public const double min_TrigramCount = 0.0;
        public const double min_TrigramRate = 0.0;
        public const double min_SPTracksPerSec = 0.0;
        public const double min_SPTracksDur = 0.0;

        public const double max_avAmpdB = -20.0;
        public const double max_snrdB = 50.0;
        public const double max_activeSnrdB = 50.0;
        public const double max_bgdB = -30;
        public const double max_activity = 1.0;
        public const double max_segCount = 10.0;
        public const double max_avSegDur = 10.0;
        public const double max_hfCover = 0.3;
        public const double max_mfCover = 0.3;
        public const double max_lfCover = 0.3;
        public const double max_HAmpl = 0.95;
        public const double max_HPeakFreq = 0.95;
        public const double max_HAvSpectrum = 0.95;
        public const double max_HVarSpectrum = 0.95;
        public const double max_AcComplexity = 0.7;
        public const double max_NumClusters = 20.0;
        public const double max_avClustDur = 10.0;
        public const double max_TrigramCount = 10.0;
        public const double max_TrigramRate = 10.0;
        public const double max_SPTracksPerSec = 10.0;
        public const double max_SPTracksDur = 10.0;

        public static AnalysisPrograms.Features GetBaselineIndices(int freqBinCount, TimeSpan wavDuration)
        {

            AnalysisPrograms.Features indices; // struct in which to store all indices
            indices.recordingDuration = wavDuration;                        // total duration of recording
            indices.activity = 0.0;                                         // fraction of frames having acoustic activity 
            indices.bgNoise = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL;         // calculated from actual zero signal
            indices.snr = 0.0;                                              // snr
            indices.activeSnr = 0.0;                                        // snr calculated from active frames only
            indices.avSig_dB = SNR.MINIMUM_dB_BOUND_FOR_ZERO_SIGNAL;        // calculated from actual zero signal. 
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

            indices.rainScore = 0.0;
            indices.cicadaScore = 0.0;
            indices.lowFreqCover = 0.0;
            indices.midFreqCover = 0.0;
            indices.hiFreqCover = 0.0;

            indices.trackDuration_total = TimeSpan.Zero;
            indices.trackDuration_percent = 0;
            indices.trackCount = 0;
            indices.tracksPerSec = 0.0;

            // spectral of indices for construction of false-colour spectrograms
            indices.spectrum_ACI = new double[freqBinCount];
            indices.spectrum_AVG = new double[freqBinCount];
            indices.spectrum_BGN = new double[freqBinCount];
            for (int i = 0; i < freqBinCount; i++) indices.spectrum_BGN[i] = -150; // set rock bottom BGN level in decibels
            indices.spectrum_CLS = new double[freqBinCount];
            indices.spectrum_CVR = new double[freqBinCount];
            indices.spectrum_ENT = new double[freqBinCount];
            for (int i = 0; i < freqBinCount; i++ ) indices.spectrum_ENT[i] = 1.0; // tmporal entropy values are reversed
            indices.spectrum_VAR = new double[freqBinCount];

            return indices;
        }


        public static System.Tuple<string[], Type[], bool[], double[]> InitOutputTableColumns()
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


    }
}
