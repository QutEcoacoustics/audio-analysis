using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
    public static class AnalysisKeys
    {


        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string ANNOTATE_SONOGRAM  = "ANNOTATE_SONOGRAM";
        public static string ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string CALL_DURATION = "CALL_DURATION";
        public static string DCT_DURATION = "DCT_DURATION";
        public static string DCT_THRESHOLD = "DCT_THRESHOLD";
        public static string DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string DISPLAY_CSV_IMAGE = "DISPLAY_CSV_IMAGE";
        public static string DISPLAY_COLUMNS = "DISPLAY_COLUMNS";
        public static string DISPLAY_WEIGHTED_INDICES = "DISPLAY_WEIGHTED_INDICES";
        public static string DOMINANT_FREQUENCY = "DOMINANT_FREQUENCY";
        public static string DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string FRAME_LENGTH = "FRAME_LENGTH";
        public static string FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string MIN_HZ = "MIN_HZ";
        public static string MAX_HZ = "MAX_HZ";
        public static string MIN_FORMANT_GAP = "MIN_FORMANT_GAP";
        public static string MAX_FORMANT_GAP = "MAX_FORMANT_GAP";
        public static string MIN_GAP = "MIN_GAP";
        public static string MAX_GAP = "MAX_GAP";
        public static string MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string MIN_DURATION = "MIN_DURATION";
        public static string MAX_DURATION = "MAX_DURATION";
        public static string MIN_OSCIL_FREQ = "MIN_OSCIL_FREQ";
        public static string MAX_OSCIL_FREQ = "MAX_OSCIL_FREQ";
        public static string MIN_PERIODICITY = "MIN_PERIOD";
        public static string MAX_PERIODICITY = "MAX_PERIOD";
        public static string OSCILLATION_RATE = "OSCILLATION_RATE";
        public static string PARALLEL_PROCESSING = "PARALLEL_PROCESSING";
        public static string RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string SAVE_INTERMEDIATE_CSV_FILES = "SAVE_INTERMEDIATE_CSV_FILES";
        public static string SAVE_INTERMEDIATE_WAV_FILES = "SAVE_INTERMEDIATE_WAV_FILES";
        public const string SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";
        public static string SMOOTHING_WINDOW = "SMOOTHING_WINDOW";
        public static string THRESHOLD        = "THRESHOLD";


        // Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public static string LOW_FREQ_BOUND = "LOW_FREQ_BOUND";
        public static string MID_FREQ_BOUND = "MID_FREQ_BOUND";

        // KEYS to SONOGRAMS
        public static string SAVE_SONOGRAMS         = "SAVE_SONOGRAM_FILES";
        public static string TIME_REDUCTION_FACTOR  = "TIME_REDUCTION_FACTOR";
        public static string FREQ_REDUCTION_FACTOR  = "FREQ_REDUCTION_FACTOR";
        public static string ADD_AXES               = "ADD_AXES";
        public static string ADD_TIME_SCALE         = "ADD_TIME_SCALE";
        public static string ADD_SEGMENTATION_TRACK = "ADD_SEGMENTATION_TRACK";
        public static string MAKE_SOX_SONOGRAM      = "MAKE_SOX_SONOGRAM";
        public static string SONOGRAM_TITLE         = "SONOGRAM_TITLE";
        public static string SONOGRAM_COMMENT       = "SONOGRAM_COMMENT";
        public static string SONOGRAM_COLOURED      = "SONOGRAM_COLOURED";
        public static string SONOGRAM_QUANTISATION  = "SONOGRAM_QUANTISATION";
        public static string SOX_PATH               = "SOX_PATH";
        public static string NOISE_REDUCTION_TYPE   = "NOISE_REDUCTION_TYPE";
        public static string NOISE_DO_REDUCTION     = "DO_NOISE_REDUCTION";
        public static string NOISE_BG_THRESHOLD     = "BG_NOISE_THRESHOLD";



        //KEYS TO OUTPUT INDICES HEADERS
        public static string INDICES_COUNT    = "IndicesCount";
        public static string SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string AV_AMPLITUDE     = "avAmp-dB";
        public const string START_MIN        = "start-min";
        public static string CALL_DENSITY     = "CallDensity";
        public static string SNR_SCORE        = "SNRscore";

        //KEYS TO OUTPUT EVENT HEADERS
        public static string EVENT_COUNT     = "EventCount";
        public static string EVENT_DURATION  = "EvDuration";
        public static string EVENT_INTENSITY = "EvIntensity";
        public static string EVENT_NAME      = "EventName";
        public static string EVENT_NORMSCORE = "EvScoreNorm";
        public static string EVENT_SCORE     = "EvScore";
        public static string EVENT_START_ABS = "EvStartAbs";
        public static string EVENT_START_SEC = "EvStartSec";
        public static string EVENT_START_MIN = "EvStartMin";
        public static string EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string EVENT_TOTAL     = "EventTotal";
    }
}
