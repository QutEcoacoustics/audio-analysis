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
		}

		#region Properties
		public FftConfiguration FftConfiguration { get; private set; }

		public int WindowSize { get; set; }
		public double WindowOverlap { get; set; }  // Percent overlap of frames

		int? minFreqBand;
		public int? MinFreqBand
		{
			get { return minFreqBand; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("MinFreqBand must be >= 0");
				minFreqBand = value;
			}
		}

		int? maxFreqBand;
		public int? MaxFreqBand
		{
			get { return maxFreqBand; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("MaxFreqBand must be >= 0");
				maxFreqBand = value;
			}
		}
		#endregion
	}

	public class FeatureSonogramConfig : BaseSonogramConfig
	{
		public FeatureSonogramConfig(Configuration config) : base(config)
		{
		}

		public MfccConfiguration MfccConfiguration { get; set; }
	}
}