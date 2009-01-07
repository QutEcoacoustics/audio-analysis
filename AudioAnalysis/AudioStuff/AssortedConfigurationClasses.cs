using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioStuff
{
	public class FftConfiguration
	{
		public FftConfiguration(Configuration config)
		{
			WindowFunction = config.GetString("WINDOW_FUNCTION");
			NPointSmoothFFT = config.GetIntNullable("N_POINT_SMOOTH_FFT") ?? 0;
		}

		public void Save(TextWriter writer)
		{
			Configuration.WriteValue(writer, "WINDOW_FUNCTION", WindowFunction);
			Configuration.WriteValue(writer, "N_POINT_SMOOTH_FFT", NPointSmoothFFT);
		}

		#region Properties
		public string WindowFunction { get; set; }
		public int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
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
			Configuration.WriteValue(writer, "FILTERBANK_COUNT", FilterbankCount);
			Configuration.WriteValue(writer, "DO_MELSCALE", DoMelScale);
			Configuration.WriteValue(writer, "CC_COUNT", CcCount);
			Configuration.WriteValue(writer, "INCLUDE_DELTA", IncludeDelta);
			Configuration.WriteValue(writer, "INCLUDE_DOUBLE_DELTA", IncludeDoubleDelta);
		}

		#region Properties
		public int FilterbankCount { get; set; }
		public bool DoMelScale { get; set; }
		public int CcCount { get; set; }     //number of cepstral coefficients
		public bool IncludeDelta { get; set; }
		public bool IncludeDoubleDelta { get; set; }
		#endregion
	}

	public class EndpointDetectionConfiguration
	{
		public EndpointDetectionConfiguration(Configuration config)
		{
			SegmentationThresholdK1 = config.GetDouble("SEGMENTATION_THRESHOLD_K1"); //dB threshold for recognition of vocalisations
			SegmentationThresholdK2 = config.GetDouble("SEGMENTATION_THRESHOLD_K2"); //dB threshold for recognition of vocalisations
			K1K2Latency = config.GetDouble("K1_K2_LATENCY");						//seconds delay between signal reaching k1 and k2 thresholds
			VocalDelay = config.GetDouble("VOCAL_DELAY");             //seconds delay required to separate vocalisations 
			MinPulseDuration = config.GetDouble("MIN_VOCAL_DURATION");      //minimum length of energy pulse - do not use this - 
		}

		public void Save(TextWriter writer)
		{
			Configuration.WriteValue(writer, "SEGMENTATION_THRESHOLD_K1", SegmentationThresholdK1);
			Configuration.WriteValue(writer, "SEGMENTATION_THRESHOLD_K2", SegmentationThresholdK2);
			Configuration.WriteValue(writer, "K1_K2_LATENCY", K1K2Latency);
			Configuration.WriteValue(writer, "VOCAL_DELAY", VocalDelay);
			Configuration.WriteValue(writer, "MIN_VOCAL_DURATION", MinPulseDuration);
		}

		#region Properties
		public double SegmentationThresholdK1 { get; set; }	// dB threshold for recognition of vocalisations
		public double SegmentationThresholdK2 { get; set; }	// dB threshold for recognition of vocalisations
		public double K1K2Latency { get; set; }				// Seconds delay between signal reaching k1 and k2 thresholds
		public double VocalDelay { get; set; }				// Seconds delay required to separate vocalisations 
		public double MinPulseDuration { get; set; }		// Minimum length of energy pulse - do not use this
		#endregion
	}
}