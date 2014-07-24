// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnalysisKeys.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools
{
    using System;

    public static class AnalysisKeys
    {
        // KEYS TO PARAMETERS IN CONFIG FILE
        public const string AnnotateSonogram = "AnnotateSonogram";
        public const string AnalysisName = "AnalysisName";
        public const string CallDuration = "CallDuration";
        public const string DctDuration = "DctDuration";
        public const string DctThreshold = "DctThreshold";
        public const string DecibelThreshold = "DecibelThreshold";
        public const string DisplayCsvImage = "DisplayCsvImage";
        public const string DisplayColumns = "DisplayColumns";
        public const string DisplayWeightedIndices = "DisplayWeightedIndices";
        public const string DominantFrequency = "DominantFrequency";
        public const string DoSegmentation = "DoSegmentation";
        public const string FrameLength = "FrameLength";
        public const string FrameOverlap = "FrameOverlap";
        public const string IntensityThreshold = "IntensityThreshold";
        public const string MinHz = "MinHz";
        public const string MaxHz = "MaxHz";
        public const string MinFormantGap = "MinFormantGap";
        public const string MaxFormantGap = "MaxFormantGap";
        public const string MinGap = "MinGap";
        public const string MaxGap = "MaxGap";
        public const string MinAmplitude = "MinAmplitude";
        public const string MinDuration = "MinDuration";
        public const string MaxDuration = "MaxDuration";
        public const string MinOscilFreq = "MinOcilFreq";
        public const string MaxOscilFreq = "MaxOcilFreq";
        public const string MinPeriodicity = "MinPeriod";
        public const string MaxPeriodicity = "MaxPeriod";
        public const string OscillationRate = "OcillationRate";
        public const string ParallelProcessing = "ParallelProcessing";
        public const string ResampleRate = "ResampleRate";
        public const string SaveIntermediateCsvFiles = "SaveIntermediateCsvFiles";
        public const string SaveIntermediateWavFiles = "SaveIntermediateWavFiles";
        public const string SegmentDuration = "SegmentDuration";
        public const string SegmentOverlap = "SegmentOverlap";
        public const string SmoothingWindow = "SmoothingWindow";
        public const string Threshold = "Threshold";

        // Keys to recognise identifiers in ANALYSIS CONFIG file. 
        public const string LowFreqBound = "LowFreqBound";
        public const string MidFreqBound = "MidFreqBound";

        // KEYS to SONOGRAMS
        public const string SaveSonograms = "SaveSonogramFiles";
        public const string TimeReductionFactor = "TimeReductionFactor";
        public const string FreqReductionFactor = "FreqReductionFactor";
        public const string AddAxes = "AddAxes";
        public const string AddTimeScale = "AddTimeScale";
        public const string AddSegmentationTrack = "AddSegmentationTrack";
        public const string MakeSoxSonogram = "MakeSoxSonogram";
        public const string SonogramTitle = "SonogramTitle";
        public const string SonogramComment = "SonogramComment";
        public const string SonogramColored = "SonogramColored";
        public const string SonogramQuantisation = "SonogramQuantisation";

        [Obsolete]
        public const string SoxPath = "SOX_PATH";

        public const string NoiseReductionType = "NoiseReductionType";
        public const string NoiseDoReduction = "DoNoiseReduction";
        public const string NoiseBgThreshold = "BgNoiseThreshold";

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
        public const string EventThreshold = "EventThreshold";
        public const string EventTotal = "EventTotal";

        // AED
        public const string KeyBandpassMaximum = "BandpassMaximum";
        public const string KeyBandpassMinimum = "BandpassMinimum";
        public const string KeyAedIntensityThreshold = "IntensityThreshold";
        public const string KeyAedSmallAreaThreshold = "SmallAreaThreshold";

        // SNR
        public const string KeyFrameSize = "FrameSize";
        public const string KeyFrameOverlap = FrameOverlap;
        public const string KeyWindowFunction = "WindowFunction";
        public const string KeyNPointSmoothFft = "NpointSmoothFFT";
        public const string KeyNoiseReductionType = "NoiseReductionType";
        public const string KeySegmentationThresholdK1 = "SegmentationThresholdK1";
        public const string KeySegmentationThresholdK2 = "SegmentationThresholdK2";
        public const string KeyK1K2Latency = "K1K2Latency";
        public const string KeyVocalGap = "VocalGap";
        public const string KeyMinVocalDuration = "MinVocalDuraton";
        public const string KeyDrawSonograms = "DrawSonograms";

        // Other config keys
        public const string KeyIndexPropertiesConfig = "IndexPropertiesConfig";
    }
}