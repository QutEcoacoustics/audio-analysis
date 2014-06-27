// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisKeys.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools
{
    public static class AnalysisKeys
    {
        // KEYS TO PARAMETERS IN CONFIG FILE
        public const string AnnotateSonogram = "ANNOTATE_SONOGRAM";
        public const string AnalysisName = "AnalysisName";
        public const string CallDuration = "CALL_DURATION";
        public const string DctDuration = "DCT_DURATION";
        public const string DctThreshold = "DCT_THRESHOLD";
        public const string DecibelThreshold = "DECIBEL_THRESHOLD";
        public const string DisplayCsvImage = "DISPLAY_CSV_IMAGE";
        public const string DisplayColumns = "DISPLAY_COLUMNS";
        public const string DisplayWeightedIndices = "DISPLAY_WEIGHTED_INDICES";
        public const string DominantFrequency = "DOMINANT_FREQUENCY";
        public const string DoSegmentation = "DO_SEGMENTATION";
        public const string FrameLength = "FRAME_LENGTH";
        public const string FrameOverlap = "FRAME_OVERLAP";
        public const string IntensityThreshold = "INTENSITY_THRESHOLD";
        public const string MinHz = "MIN_HZ";
        public const string MaxHz = "MAX_HZ";
        public const string MinFormantGap = "MIN_FORMANT_GAP";
        public const string MaxFormantGap = "MAX_FORMANT_GAP";
        public const string MinGap = "MIN_GAP";
        public const string MaxGap = "MAX_GAP";
        public const string MinAmplitude = "MIN_AMPLITUDE";
        public const string MinDuration = "MIN_DURATION";
        public const string MaxDuration = "MAX_DURATION";
        public const string MinOscilFreq = "MIN_OSCIL_FREQ";
        public const string MaxOscilFreq = "MAX_OSCIL_FREQ";
        public const string MinPeriodicity = "MIN_PERIOD";
        public const string MaxPeriodicity = "MAX_PERIOD";
        public const string OscillationRate = "OSCILLATION_RATE";
        public const string ParallelProcessing = "PARALLEL_PROCESSING";
        public const string ResampleRate = "RESAMPLE_RATE";
        public const string SaveIntermediateCsvFiles = "SAVE_INTERMEDIATE_CSV_FILES";
        public const string SaveIntermediateWavFiles = "SAVE_INTERMEDIATE_WAV_FILES";
        public const string SegmentDuration = "SEGMENT_DURATION";
        public const string SegmentOverlap = "SEGMENT_OVERLAP";
        public const string SmoothingWindow = "SMOOTHING_WINDOW";
        public const string Threshold = "THRESHOLD";

        // Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public const string LowFreqBound = "LOW_FREQ_BOUND";
        public const string MidFreqBound = "MID_FREQ_BOUND";

        // KEYS to SONOGRAMS
        public const string SaveSonograms = "SAVE_SONOGRAM_FILES";
        public const string TimeReductionFactor = "TIME_REDUCTION_FACTOR";
        public const string FreqReductionFactor = "FREQ_REDUCTION_FACTOR";
        public const string AddAxes = "ADD_AXES";
        public const string AddTimeScale = "ADD_TIME_SCALE";
        public const string AddSegmentationTrack = "ADD_SEGMENTATION_TRACK";
        public const string MakeSoxSonogram = "MAKE_SOX_SONOGRAM";
        public const string SonogramTitle = "SONOGRAM_TITLE";
        public const string SonogramComment = "SONOGRAM_COMMENT";
        public const string SonogramColoured = "SONOGRAM_COLOURED";
        public const string SonogramQuantisation = "SONOGRAM_QUANTISATION";
        public const string SoxPath = "SOX_PATH";
        public const string NoiseReductionType = "NOISE_REDUCTION_TYPE";
        public const string NoiseDoReduction = "DO_NOISE_REDUCTION";
        public const string NoiseBgThreshold = "BG_NOISE_THRESHOLD";

        // KEYS TO CSV HEADERS FOR SUMMARY INDEX
        public const string KeyRankOrder = "RankOrder";
        public const string KeyStartMinute = "StartMinute";
        public const string KeySegmentDuration = "SegmentDuration";
        public const string KeyAvSignalAmplitude = "AvSignalAmplitude";
        public const string KeyCallDensity = "CallDensity";
        public const string KeySnrScore = "SNRscore";

        // KEYS TO CSV HEADERS FOR EVENTS
        public const string EventCount = "EventCount";
        public const string EventDuration = "EvDuration";
        public const string EventIntensity = "EvIntensity";
        public const string EventName = "EventName";
        public const string EventNormscore = "EvScoreNorm";
        public const string EventScore = "EvScore";
        public const string EventStartAbs = "EvStartAbs";
        public const string EventStartSec = "EvStartSec";
        public const string EventStartMin = "EvStartMin";
        public const string EventThreshold = "EVENT_THRESHOLD";
        public const string EventTotal = "EventTotal";
    }
}