using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
    [Serializable]
    public static class FftConfiguration
	{
        public static void SetConfig(Configuration config)
		{
            int sr = config.GetIntNullable("SAMPLE_RATE") ?? 0;
            SetSampleRate(sr);
			WindowFunction = config.GetString("WINDOW_FUNCTION");
			NPointSmoothFFT = config.GetIntNullable("N_POINT_SMOOTH_FFT") ?? 0;
		}

        public static void SetSampleRate(int sr)
        {
            SampleRate = sr;
            NyquistFreq = sr / 2;
        }

		public static void Save(TextWriter writer)
		{
            writer.WriteConfigValue("NYQUIST_FREQ",  NyquistFreq);
			writer.WriteConfigValue("WINDOW_FUNCTION", WindowFunction);
			writer.WriteConfigValue("N_POINT_SMOOTH_FFT", NPointSmoothFFT);
            writer.Flush();
        }

		#region Properties
        public static int SampleRate { get; set; }
        public static int NyquistFreq { get; set; }
        private static string windowFunction = "Hamming";
        public  static string WindowFunction { get { return windowFunction; } set { windowFunction = value; } }
		public static int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
		#endregion
	}

    [Serializable]
    public class MfccConfiguration
	{
		public MfccConfiguration(Configuration config)
		{
			FilterbankCount = config.GetInt("FILTERBANK_COUNT");
			DoMelScale = config.GetBoolean("DO_MELSCALE");
			CcCount = config.GetInt("CC_COUNT"); //number of cepstral coefficients
			IncludeDelta = config.GetBoolean("INCLUDE_DELTA");
			IncludeDoubleDelta = config.GetBoolean("INCLUDE_DOUBLE_DELTA");
		}

		public void Save(TextWriter writer)
		{
            writer.WriteConfigValue("DO_MELSCALE", DoMelScale);
            writer.WriteConfigValue("FILTERBANK_COUNT", FilterbankCount);
			writer.WriteConfigValue("CC_COUNT", CcCount);
			writer.WriteConfigValue("INCLUDE_DELTA", IncludeDelta);
			writer.WriteConfigValue("INCLUDE_DOUBLE_DELTA", IncludeDoubleDelta);
            writer.Flush();
		}

		#region Properties
        public int FilterbankCount { get; set; }
		public bool DoMelScale { get; set; }
		public int CcCount { get; set; }     //number of cepstral coefficients
		public bool IncludeDelta { get; set; }
		public bool IncludeDoubleDelta { get; set; }
		#endregion
	}


    /// <summary>
    /// SETS PARAMETERS CONCERNING ENERGY, END-POINT DETECTION AND SEGMENTATION
    /// </summary>
    [Serializable]
    public static class EndpointDetectionConfiguration
	{
		public static void SetEndpointDetectionParams(Configuration config)
		{
			K1Threshold = config.GetDouble("SEGMENTATION_THRESHOLD_K1"); //dB threshold for recognition of vocalisations
			K2Threshold = config.GetDouble("SEGMENTATION_THRESHOLD_K2"); //dB threshold for recognition of vocalisations
			K1K2Latency = config.GetDouble("K1_K2_LATENCY");						//seconds delay between signal reaching k1 and k2 thresholds
			VocalDelay = config.GetDouble("VOCAL_DELAY");             //seconds delay required to separate vocalisations 
			MinPulseDuration = config.GetDouble("MIN_VOCAL_DURATION");      //minimum length of energy pulse - do not use this - 
        }

		public static void Save(TextWriter writer)
		{
            writer.WriteLine("#**************** INFO ABOUT SEGMENTATION");
			writer.WriteConfigValue("SEGMENTATION_THRESHOLD_K1", K1Threshold);
			writer.WriteConfigValue("SEGMENTATION_THRESHOLD_K2", K2Threshold);
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