// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationClasses.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defined string constants for keys in config tables
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using DSP;
    using TowseyLibrary;

    /// <summary>
    /// Defined string constants for keys in config tables
    /// </summary>
    public static class ConfigKeys
    {
        public enum SonogramTypes
        {
            Amplitude,
            Spectral,
            Cepsral,
            AcousticVectors,
            SobelEdge
        }

        // ReSharper disable InconsistentNaming
        public enum FeatureType
        {
            Undefined,
            MFCC,
            CC_AUTO,
            DCT_2D
        }

        // ReSharper restore InconsistentNaming
        public struct Recording
        {
            public const string Key_RecordingCallName = "CALL_NAME";
            public const string Key_RecordingFileName = "WAV_FILE_NAME";
            public const string Key_RecordingDirName  = "WAV_DIR_NAME";
            public const string Key_TrainingDirName   = "TRAIN_DIR_NAME";
            public const string Key_TestingDirName    = "TEST_DIR_NAME";
        }

        public struct Windowing // or Framing
        {
            public const string Key_SampleRate    = "SAMPLE_RATE";
            public const string Key_SubSample     = "SUBSAMPLE";
            public const string Key_WindowSize    = "FRAME_SIZE";
            public const string Key_WindowOverlap = "FRAME_OVERLAP";
        }

        public struct Mfcc
        {
            public const string Key_WindowFunction   = "WINDOW_FUNCTION";
            public const string Key_NPointSmoothFFT = "N_POINT_SMOOTH_FFT";
            public const string Key_NyquistFrequency = "NYQUIST_FREQ";
            public const string Key_StartTime        = "START_TIME";
            public const string Key_EndTime          = "END_TIME";
            public const string Key_DoMelScale       = "DO_MEL_CONVERSION";
            public const string Key_MinFreq          = "MIN_FREQ";
            public const string Key_MaxFreq          = "MAX_FREQ";
            public const string Key_FilterbankCount  = "FILTERBANK_COUNT";
            public const string Key_CcCount          = "CC_COUNT";
            public const string Key_IncludeDelta     = "INCLUDE_DELTA";
            public const string Key_IncludeDoubleDelta = "INCLUDE_DOUBLEDELTA";
            public const string Key_DeltaT           = "DELTA_T";
        }

        public struct EndpointDetection
        {
            public const string Key_K1SegmentationThreshold = "SEGMENTATION_THRESHOLD_K1";
            public const string Key_K2SegmentationThreshold = "SEGMENTATION_THRESHOLD_K2";
            public const string Key_K1K2Latency      = "K1_K2_LATENCY";
            public const string Key_VocalGap         = "VOCAL_GAP";
            public const string Key_MinVocalDuration = "MIN_VOCAL_DURATION";
        }

        public struct Sonogram
        {
            public const string Key_SonogramType    = "SONOGRAM_TYPE";
        }

        public struct Template
        {
            public const string Key_ExtractInterval = "EXTRACTION_INTERVAL"; //determines complexity of language model
            public const string Key_TemplateType    = "TEMPLATE_TYPE";
            public const string Key_TemplateDir     = "TEMPLATE_DIR";
            public const string Key_FVCount         = "FV_COUNT";         // number of feature vectors in acoustic model
            public const string Key_FVType          = "FEATURE_TYPE";     // type of feature vector to be extracted
            public const string Key_FVDefaultNoiseFile= "FV_DEFAULT_NOISE_FILE"; // location of the deafult noise FV
            public const string Key_ModelType       = "MODEL_TYPE";       // language model
            public const string Key_WordCount       = "NUMBER_OF_WORDS";  // in the language model
            public const string Key_WordNames       = "WORD_NAMES";       // in the language model
        }

        public struct ImageSave
        {
            public const string Key_AddGrid = "ADDGRID";
        }
    }

    [Serializable]
    public class MfccConfiguration
    {
        public int FilterbankCount { get; set; }
        public bool DoMelScale { get; set; }
        public int CcCount { get; set; }     //number of cepstral coefficients
        public bool IncludeDelta { get; set; }
        public bool IncludeDoubleDelta { get; set; }

        public MfccConfiguration(ConfigDictionary config)
        {
            this.FilterbankCount = config.GetInt(ConfigKeys.Mfcc.Key_FilterbankCount);
            this.DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            this.CcCount = config.GetInt(ConfigKeys.Mfcc.Key_CcCount); //number of cepstral coefficients
            this.IncludeDelta = config.GetBoolean(ConfigKeys.Mfcc.Key_IncludeDelta);
            this.IncludeDoubleDelta = config.GetBoolean(ConfigKeys.Mfcc.Key_IncludeDoubleDelta);
        }

        public void Save(TextWriter writer)
        {
            Json.Serialise(writer, this);
        }
    }

    /// <summary>
    /// SETS PARAMETERS CONCERNING ENERGY, END-POINT DETECTION AND SEGMENTATION
    /// </summary>
    [Serializable]
    public static class EndpointDetectionConfiguration
    {
        public static void SetDefaultSegmentationConfig()
        {
            K1Threshold = 1.0;   // dB threshold for recognition of vocalisations
            K2Threshold = 1.0;   // dB threshold for recognition of vocalisations
            K1K2Latency = 0.05;  // Seconds delay between signal reaching k1 and k2 thresholds
            VocalGap = 0.1;   // Seconds gap required to separate vocalisations
            MinPulseDuration = 0.05;    // Minimum length of energy pulse - do not use this
        }

        public static void SetConfig(ConfigDictionary config)
        {
            K1Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold); //dB threshold for recognition of vocalisations
            K2Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold); //dB threshold for recognition of vocalisations
            K1K2Latency = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1K2Latency);             //seconds delay between signal reaching k1 and k2 thresholds
            VocalGap = (double?)config.GetDouble(ConfigKeys.EndpointDetection.Key_VocalGap) ?? 0.1; //seconds gap required to separate vocalisations
            MinPulseDuration = config.GetDouble(ConfigKeys.EndpointDetection.Key_MinVocalDuration);   //minimum length of energy pulse - do not use this -
        }

        public static void Save(TextWriter writer)
        {
            writer.WriteLine("#**************** INFO ABOUT SEGMENTATION");
            writer.WriteConfigValue(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold, K1Threshold);
            writer.WriteConfigValue(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold, K2Threshold);
            //writer.WriteConfigValue("K1_K2_LATENCY", K1K2Latency);
            writer.WriteConfigValue(ConfigKeys.EndpointDetection.Key_VocalGap, VocalGap);
            //writer.WriteConfigValue("MIN_VOCAL_DURATION", MinPulseDuration);
            writer.WriteLine("#");
            writer.Flush();
        }

        /// <summary>
        /// WARNING: calculation of k1 and k2 is faulty.
        /// MinDecibelReference should not be used ie k1 = EndpointDetectionConfiguration.SegmentationThresholdK1;
        /// See the alternative below
        ///
        /// ************* PARAMETERS FOR:- ENDPOINT DETECTION of VOCALISATIONS
        /// See Lamel et al 1981.
        /// They use k1, k2, k3 and k4, minimum pulse length and k1_k2Latency.
        /// Here we set k1 = k3, k4 = k2,  k1_k2Latency = 0.186s (5 frames)
        ///                  and "minimum pulse length" = 0.075s (2 frames)
        /// SEGMENTATION_THRESHOLD_K1 = decibels above the minimum level
        /// SEGMENTATION_THRESHOLD_K2 = decibels above the minimum level
        /// K1_K2_LATENCY = seconds delay between signal reaching k1 and k2 thresholds
        /// VOCAL_DELAY = seconds delay required to separate vocalisations
        /// MIN_VOCAL_DURATION = minimum length of energy pulse - do not use this - accept all pulses.
        /// SEGMENTATION_THRESHOLD_K1=3.5
        /// SEGMENTATION_THRESHOLD_K2=6.0
        /// K1_K2_LATENCY=0.05
        /// VOCAL_DELAY=0.2
        /// </summary>
        public static int[] DetermineVocalisationEndpoints(double[] dbArray, double frameStep)
        {
            var k1k2Delay = (int)(K1K2Latency / frameStep);    //=5  frames delay between signal reaching k1 and k2 thresholds
            var frameGap = (int)(VocalGap / frameStep);  //=10 frames delay required to separate vocalisations
            var minPulse = (int)(MinPulseDuration / frameStep); //=2  frames is min vocal length
            return MFCCStuff.VocalizationDetection(dbArray, K1Threshold, K2Threshold, k1k2Delay, frameGap, minPulse, null);
        }

        //these should be the same for all threads and processes
        //these k1 and k2 thresholds are dB above the base line minimum value.
        public static double K1Threshold { get; set; } // dB threshold for recognition of vocalisations

        public static double K2Threshold { get; set; } // dB threshold for recognition of vocalisations

        public static double K1K2Latency { get; set; } // Seconds delay between signal reaching k1 and k2 thresholds

        public static double VocalGap { get; set; } // Seconds gap required to separate vocalisations

        public static double MinPulseDuration { get; set; } // Minimum length of energy pulse - do not use this
    }
}