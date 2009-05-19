using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{

    /// <summary>
    /// Defined string constants for keys in config tables
    /// </summary>
    public static class ConfigKeys
    {
        public enum WindowFunctions { HAMMING };
        public enum SonogramTypes { amplitude, spectral, cepsral, acousticVectors, sobelEdge };
        public enum Feature_Type { UNDEFINED, MFCC, CC_AUTO, DCT_2D }
        public enum NoiseReductionType { NONE, STANDARD, FIXED_DYNAMIC_RANGE }


        public struct Recording 
        {
            public const string Key_RecordingFileName = "WAV_FILE_NAME";
            public const string Key_RecordingDirName  = "WAV_DIR_NAME";
        }

        public struct Windowing //or Framing
        {
            public const string Key_SampleRate = "SAMPLE_RATE";
            public const string Key_SubSample = "SUBSAMPLE";
            public const string Key_WindowSize = "FRAME_SIZE";
            public const string Key_WindowOverlap = "FRAME_OVERLAP";
        }

        public struct Mfcc
        {
            public const string Key_WindowFunction = "WINDOW_FUNCTION";
            public const string Key_NPointSmoothFFT = "N_POINT_SMOOTH_FFT";
            public const string Key_NyquistFrequency = "NYQUIST_FREQ";
            public const string Key_StartTime = "START_TIME";
            public const string Key_EndTime          = "END_TIME";
            public const string Key_DoMelScale       = "DO_MEL_CONVERSION";
            public const string Key_NoiseReductionType = "NOISE_REDUCTION_TYPE";
            public const string Key_MinFreq          = "MIN_FREQ";
            public const string Key_MaxFreq          = "MAX_FREQ";
            public const string Key_FilterbankCount  = "FILTERBANK_COUNT";
            public const string Key_CcCount          = "CC_COUNT";
            public const string Key_IncludeDelta     = "INCLUDE_DELTA";
            public const string Key_IncludeDoubleDelta = "INCLUDE_DOUBLEDELTA";
            public const string Key_DeltaT           = "DELTA_T";
        }

        public struct Snr
        {
            public const string Key_DynamicRange = "DYNAMIC_RANGE";
        }

            
        public struct EndpointDetection
        {
            public const string Key_K1SegmentationThreshold = "SEGMENTATION_THRESHOLD_K1";
            public const string Key_K2SegmentationThreshold = "SEGMENTATION_THRESHOLD_K2";
            public const string Key_K1K2Latency = "K1_K2_LATENCY";
            public const string Key_VocalDelay = "VOCAL_DELAY";
            public const string Key_MinVocalDuration = "MIN_VOCAL_DURATION";
        }

        public struct Sonogram
        {
            public const string Key_SonogramType = "SONOGRAM_TYPE";
        }

        public struct Template
        {
            public const string Key_TemplateType = "TEMPLATE_TYPE";
        }

        public struct ImageSave
        {
            public const string Key_AddGrid = "ADDGRID";
        }
    } //end class ConfigKeys




    [Serializable]
    public static class FftConfiguration
	{
        

        public static void SetConfig(Configuration config)
		{
            int sr = config.GetIntNullable(ConfigKeys.Windowing.Key_SampleRate) ?? 0;
            SetSampleRate(sr);
            WindowFunction = config.GetString(ConfigKeys.Mfcc.Key_WindowFunction);
            NPointSmoothFFT = config.GetIntNullable(ConfigKeys.Mfcc.Key_NPointSmoothFFT) ?? 0;
		}

        public static void SetSampleRate(int sr)
        {
            SampleRate = sr;
            NyquistFreq = sr / 2;
        }

		public static void Save(TextWriter writer)
		{
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_NyquistFrequency, NyquistFreq);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_WindowFunction, WindowFunction);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_NPointSmoothFFT, NPointSmoothFFT);
            writer.Flush();
        }

		#region Properties
        public static int SampleRate { get; set; }
        public static int NyquistFreq { get; set; }
        private static string windowFunction = ConfigKeys.WindowFunctions.HAMMING.ToString();
        public  static string WindowFunction { get { return windowFunction; } set { windowFunction = value; } }
		public static int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
		#endregion
	}

    [Serializable]
    public class MfccConfiguration
	{
        #region Properties
        public int FilterbankCount { get; set; }
        public bool DoMelScale { get; set; }
        public int CcCount { get; set; }     //number of cepstral coefficients
        public bool IncludeDelta { get; set; }
        public bool IncludeDoubleDelta { get; set; }
        #endregion

		public MfccConfiguration(Configuration config)
		{
            FilterbankCount = config.GetInt(ConfigKeys.Mfcc.Key_FilterbankCount);
            DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            CcCount = config.GetInt(ConfigKeys.Mfcc.Key_CcCount); //number of cepstral coefficients
            IncludeDelta = config.GetBoolean(ConfigKeys.Mfcc.Key_IncludeDelta);
            IncludeDoubleDelta = config.GetBoolean(ConfigKeys.Mfcc.Key_IncludeDoubleDelta);
		}

		public void Save(TextWriter writer)
		{
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_DoMelScale, DoMelScale);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_FilterbankCount, FilterbankCount);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_CcCount, CcCount);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_IncludeDelta, IncludeDelta);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_IncludeDoubleDelta, IncludeDoubleDelta);
            writer.Flush();
		}
    }//end class MfccConfiguration



    /// <summary>
    /// SETS PARAMETERS CONCERNING ENERGY, END-POINT DETECTION AND SEGMENTATION
    /// </summary>
    [Serializable]
    public static class EndpointDetectionConfiguration
	{

		public static void SetEndpointDetectionParams(Configuration config)
		{
            K1Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold); //dB threshold for recognition of vocalisations
            K2Threshold = config.GetDouble(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold); //dB threshold for recognition of vocalisations
            K1K2Latency = config.GetDouble(ConfigKeys.EndpointDetection.Key_K1K2Latency);				//seconds delay between signal reaching k1 and k2 thresholds
            VocalDelay = config.GetDouble(ConfigKeys.EndpointDetection.Key_VocalDelay);               //seconds delay required to separate vocalisations 
            MinPulseDuration = config.GetDouble(ConfigKeys.EndpointDetection.Key_MinVocalDuration);   //minimum length of energy pulse - do not use this - 
        }

		public static void Save(TextWriter writer)
		{
            writer.WriteLine("#**************** INFO ABOUT SEGMENTATION");
            writer.WriteConfigValue(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold, K1Threshold);
            writer.WriteConfigValue(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold, K2Threshold);
			//writer.WriteConfigValue("K1_K2_LATENCY", K1K2Latency);
			//writer.WriteConfigValue("VOCAL_DELAY", VocalDelay);
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
        /// <param name="k1"></param>
        /// <param name="k2"></param>
        /// <param name="k1_k2delay"></param>
        /// <param name="syllableDelay"></param>
        /// <param name="minPulse"></param>
        /// <returns></returns>
        public static int[] DetermineVocalisationEndpoints(double[] dbArray, double frameOffset)
        {
            var k1_k2delay = (int)(K1K2Latency / frameOffset);    //=5  frames delay between signal reaching k1 and k2 thresholds
            var syllableDelay = (int)(VocalDelay / frameOffset);  //=10 frames delay required to separate vocalisations 
            var minPulse = (int)(MinPulseDuration / frameOffset); //=2  frames is min vocal length
            //Console.WriteLine("k1_k2delay=" + k1_k2delay + "  syllableDelay=" + syllableDelay + "  minPulse=" + minPulse);
            return Speech.VocalizationDetection(dbArray, K1Threshold, K2Threshold, k1_k2delay, syllableDelay, minPulse, null);
        }



		#region Six Static Properties
        //these should be the same for all threads and processes
        //these k1 and k2 thresholds are dB above the base line minimum value. Different from values finally used in Sonogram classes
		public static double K1Threshold { get; set; }	// dB threshold for recognition of vocalisations
		public static double K2Threshold { get; set; }	// dB threshold for recognition of vocalisations
		public static double K1K2Latency { get; set; }	// Seconds delay between signal reaching k1 and k2 thresholds
		public static double VocalDelay { get; set; }	// Seconds delay required to separate vocalisations 
		public static double MinPulseDuration { get; set; }		// Minimum length of energy pulse - do not use this
		#endregion
	}


}