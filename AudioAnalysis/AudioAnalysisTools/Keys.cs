using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioAnalysisTools
{
    public static class Keys
    {


        //KEYS TO PARAMETERS IN CONFIG FILE
        public static string ANNOTATE_SONOGRAM  = "ANNOTATE_SONOGRAM";
        public static string ANALYSIS_NAME = "ANALYSIS_NAME";
        public static string CALL_DURATION = "CALL_DURATION";
        public static string DECIBEL_THRESHOLD = "DECIBEL_THRESHOLD";
        public static string DRAW_SONOGRAMS = "DRAW_SONOGRAMS";
        public static string DISPLAY_COLUMNS = "DISPLAY_COLUMNS";
        public static string EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string FRAME_LENGTH = "FRAME_LENGTH";
        public static string FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string INTENSITY_THRESHOLD = "INTENSITY_THRESHOLD";
        public static string MIN_HZ = "MIN_HZ";
        public static string MAX_HZ = "MAX_HZ";
        public static string MIN_GAP = "MIN_GAP";
        public static string MAX_GAP = "MAX_GAP";
        public static string MIN_AMPLITUDE = "MIN_AMPLITUDE";
        public static string MIN_DURATION = "MIN_DURATION";
        public static string MAX_DURATION = "MAX_DURATION";
        public static string MIN_PERIODICITY = "MIN_PERIOD";
        public static string MAX_PERIODICITY = "MAX_PERIOD";
        public static string NOISE_REDUCTION_TYPE = "NOISE_REDUCTION_TYPE";
        public static string NOISE_DO_REDUCTION = "DO_NOISE_REDUCTION";
        public static string NOISE_BG_REDUCTION = "BG_NOISE_REDUCTION";
        public static string RESAMPLE_RATE = "RESAMPLE_RATE";
        public static string SAVE_INTERMEDIATE_FILES = "SAVE_INTERMEDIATE_FILES";
        public static string SAVE_SONOGRAM_FILES = "SAVE_SONOGRAM_FILES";
        public static string SEGMENT_DURATION = "SEGMENT_DURATION";
        public static string SEGMENT_OVERLAP  = "SEGMENT_OVERLAP";

        //KEYS TO OUTPUT INDICES HEADERS
        public static string INDICES_COUNT    = "Indices Count";
        public static string SEGMENT_TIMESPAN = "SegTimeSpan";
        public static string AV_AMPLITUDE = "avAmp-dB";
        public static string START_MIN    = "start-min";
        public static string CALL_DENSITY = "CallDensity";

        //KEYS TO OUTPUT EVENT HEADERS
        public static string EVENT_COUNT     = "Event Count";
        public static string EVENT_INTENSITY = "EvIntensity";
        public static string EVENT_NAME      = "Event Name";
        public static string EVENT_NORMSCORE = "EvScoreNorm";
        public static string EVENT_SCORE     = "EvScore";
        public static string EVENT_START_ABS = "EvStartAbs";
        public static string EVENT_START_SEC = "EvStartSec";
        public static string EVENT_START_MIN = "EvStartMin";
        public static string EVENT_DURATION  = "EvDuration";
        public static string EVENT_TOTAL     = "# events";

        public static string SNR_SCORE       = "SNRscore";

    }
}
