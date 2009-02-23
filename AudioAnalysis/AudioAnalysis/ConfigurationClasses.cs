using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
	public static class FftConfiguration
	{
		//public static FftConfiguration(Configuration config)
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
		public static string WindowFunction { get; set; }
		public static int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
		#endregion
	}

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
          //  writer.WriteConfigValue("MIN_FREQ", MinFreq); //=1500
          //  writer.WriteConfigValue("MAX_FREQ", MaxFreq); //=5500
            writer.WriteConfigValue("DO_MELSCALE", DoMelScale);
            writer.WriteConfigValue("FILTERBANK_COUNT", FilterbankCount);
			writer.WriteConfigValue("CC_COUNT", CcCount);
			writer.WriteConfigValue("INCLUDE_DELTA", IncludeDelta);
			writer.WriteConfigValue("INCLUDE_DOUBLE_DELTA", IncludeDoubleDelta);
            writer.Flush();
		}

		#region Properties
      //  public int MinFreq { get; set; }
      //  public int MaxFreq { get; set; }
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
	public static class EndpointDetectionConfiguration
	{
		public static void SetEndpointDetectionParams(Configuration config)
		{
			SegmentationThresholdK1 = config.GetDouble("SEGMENTATION_THRESHOLD_K1"); //dB threshold for recognition of vocalisations
			SegmentationThresholdK2 = config.GetDouble("SEGMENTATION_THRESHOLD_K2"); //dB threshold for recognition of vocalisations
			K1K2Latency = config.GetDouble("K1_K2_LATENCY");						//seconds delay between signal reaching k1 and k2 thresholds
			VocalDelay = config.GetDouble("VOCAL_DELAY");             //seconds delay required to separate vocalisations 
			MinPulseDuration = config.GetDouble("MIN_VOCAL_DURATION");      //minimum length of energy pulse - do not use this - 
        }

		public static void Save(TextWriter writer)
		{
            writer.WriteLine("#**************** INFO ABOUT SEGMENTATION");
			writer.WriteConfigValue("SEGMENTATION_THRESHOLD_K1", SegmentationThresholdK1);
			writer.WriteConfigValue("SEGMENTATION_THRESHOLD_K2", SegmentationThresholdK2);
			//writer.WriteConfigValue("K1_K2_LATENCY", K1K2Latency);
			//writer.WriteConfigValue("VOCAL_DELAY", VocalDelay);
			//writer.WriteConfigValue("MIN_VOCAL_DURATION", MinPulseDuration);
            writer.WriteLine("#");
            writer.Flush();
		}

		#region Properties
        //these k1 and k2 thresholds are dB above the base line minimum value. Different from values finally used in Sonogram classes
		public static double SegmentationThresholdK1 { get; set; }	// dB threshold for recognition of vocalisations
		public static double SegmentationThresholdK2 { get; set; }	// dB threshold for recognition of vocalisations
		public static double K1K2Latency { get; set; }				// Seconds delay between signal reaching k1 and k2 thresholds
		public static double VocalDelay { get; set; }				// Seconds delay required to separate vocalisations 
		public static double MinPulseDuration { get; set; }		// Minimum length of energy pulse - do not use this
		#endregion
	}
}