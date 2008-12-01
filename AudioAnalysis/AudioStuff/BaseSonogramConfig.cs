using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;

namespace AudioStuff
{
	public class BaseSonogramConfig
	{
		public static BaseSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new BaseSonogramConfig(config);
		}

		public BaseSonogramConfig(Configuration config)
		{
			FftConfiguration = new FftConfiguration(config);
			EndpointDetectionConfiguration = new EndpointDetectionConfiguration(config);

			WindowSize = config.GetInt("WINDOW_SIZE");
			WindowOverlap = config.GetDouble("WINDOW_OVERLAP");
			DoNoiseReduction = config.GetBoolean("NOISE_REDUCE");

			MinFreqBand = config.GetIntNullable("MIN_FREQ");
			MaxFreqBand = config.GetIntNullable("MAX_FREQ");
		}

		#region Properties
		public FftConfiguration FftConfiguration { get; private set; }

		public int WindowSize { get; set; }
		public double WindowOverlap { get; set; } // Percent overlap of frames
		public int FreqBinCount { get { return WindowSize / 2; } } // other half is phase info
		public bool DoPreemphasis { get; set; }
		public bool DoFreqBandAnalysis { get; set; }
		public bool DoNoiseReduction { get; set; }

		public EndpointDetectionConfiguration EndpointDetectionConfiguration { get; private set; }

		public int? MinFreqBand { get; private set; }
		public int? MaxFreqBand { get; private set; }
		#endregion
	}

	public class CepstralSonogramConfig : BaseSonogramConfig
	{
		public CepstralSonogramConfig(Configuration config) : base(config)
		{
			MfccConfiguration = new MfccConfiguration(config);
		}

		public MfccConfiguration MfccConfiguration { get; set; }
	}

	public class AcousticVectorsSonogramConfig : CepstralSonogramConfig
	{
		public AcousticVectorsSonogramConfig(Configuration config)
			: base(config)
		{
			DeltaT = config.GetInt("DELTA_T"); //frames between acoustic vectors
		}

		public int DeltaT { get; set; }
	}
}