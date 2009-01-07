using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

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

		public virtual void Save(TextWriter writer)
		{
			FftConfiguration.Save(writer);
			EndpointDetectionConfiguration.Save(writer);
			Configuration.WriteValue(writer, "WINDOW_SIZE", WindowSize);
			Configuration.WriteValue(writer, "WINDOW_OVERLAP", WindowOverlap);
			Configuration.WriteValue(writer, "NOISE_REDUCE", DoNoiseReduction);
			Configuration.WriteValue(writer, "MIN_FREQ", MinFreqBand);
			Configuration.WriteValue(writer, "MAX_FREQ", MaxFreqBand);
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

		public int? MinFreqBand { get; set; }
		public int? MaxFreqBand { get; set; }
		#endregion

		public double GetFrameDuration(int sampleRate)
		{
			return WindowSize / (double)sampleRate; // Duration of full frame or window in seconds
		}
	}

	public class CepstralSonogramConfig : BaseSonogramConfig
	{
		public new static CepstralSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new CepstralSonogramConfig(config);
		}

		public CepstralSonogramConfig(Configuration config) : base(config)
		{
			MfccConfiguration = new MfccConfiguration(config);
		}

		public override void Save(TextWriter writer)
		{
			base.Save(writer);
			MfccConfiguration.Save(writer);
		}

		public MfccConfiguration MfccConfiguration { get; set; }
	}

	public class AcousticVectorsSonogramConfig : CepstralSonogramConfig
	{
		public new static AcousticVectorsSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new AcousticVectorsSonogramConfig(config);
		}

		public AcousticVectorsSonogramConfig(Configuration config)
			: base(config)
		{
			DeltaT = config.GetInt("DELTA_T"); // Frames between acoustic vectors
		}

		public override void Save(TextWriter writer)
		{
			base.Save(writer);
			Configuration.WriteValue(writer, "DELTA_T", DeltaT);
		}

		public int DeltaT { get; set; }
	}
}