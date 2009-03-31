using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
	[Serializable]
	public class BaseSonogramConfig
	{

        #region Properties
        public string SourceFName { get; private set; }

        public int SampleRate { get; set; }
        public TimeSpan Duration { get; set; }
        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; } // Percent overlap of frames
        public int FreqBinCount { get { return WindowSize / 2; } } // other half is phase info
        public bool DoPreemphasis { get; set; }
        public bool DoNoiseReduction { get; set; }

        public int? MinFreqBand { get; set; }
        public int? MaxFreqBand { get; set; }
        public int? MidFreqBand { get; set; }
        public bool DisplayFullBandwidthImage { get; set; }
        #endregion


		public static BaseSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
            if (config.GetInt("VERBOSITY") > 0)
            {
                Log.Verbosity = 1;
                //Log.WriteIfVerbose("Verbosity set true in Application Config file.");
            }
            return new BaseSonogramConfig(config);
		}



        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
		public BaseSonogramConfig(Configuration config)
		{
            SourceFName = config.GetString("WAV_FILE_NAME");
            FftConfiguration.SetConfig(config);
			WindowSize = config.GetInt("WINDOW_SIZE");
			WindowOverlap = config.GetDouble("WINDOW_OVERLAP");

			DoNoiseReduction = config.GetBoolean("NOISE_REDUCE");
			MinFreqBand = config.GetIntNullable("MIN_FREQ");
			MaxFreqBand = config.GetIntNullable("MAX_FREQ");
            int? delta  = MaxFreqBand - MinFreqBand;
            MidFreqBand = MinFreqBand + (delta / 2);
            DisplayFullBandwidthImage = false;

            EndpointDetectionConfiguration.SetEndpointDetectionParams(config);
		}


		public virtual void Save(TextWriter writer)
		{
            writer.WriteLine("#**************** INFO ABOUT FRAMES");
			writer.WriteConfigValue("FRAME_SIZE", WindowSize);
			writer.WriteConfigValue("FRAME_OVERLAP", WindowOverlap);
            EndpointDetectionConfiguration.Save(writer);
            writer.WriteLine("#**************** INFO ABOUT SONOGRAM");
            writer.WriteConfigValue("MIN_FREQ", MinFreqBand);
			writer.WriteConfigValue("MAX_FREQ", MaxFreqBand);
            writer.WriteConfigValue("MID_FREQ", MidFreqBand); //=3500
            writer.WriteConfigValue("NOISE_REDUCE", DoNoiseReduction);
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT FEATURE EXTRACTION");
            writer.WriteLine("FEATURE_TYPE=mfcc");
            FftConfiguration.Save(writer);
		}


		public double GetFrameDuration(int sampleRate)
		{
			return WindowSize / (double)sampleRate; // Duration of full frame or window in seconds
		}

        public double GetFrameOffset()
        {
            double frameDuration = GetFrameDuration(this.SampleRate);// Duration of full frame or window in seconds
            double frameOffset = frameDuration * (1 - WindowOverlap);// Duration of non-overlapped part of window/frame in seconds
            return frameOffset; 
        }

        public double GetFrameOffset(int sampleRate)
        {
            double frameDuration = GetFrameDuration(sampleRate);
            double frameOffset = frameDuration * (1 - WindowOverlap);
            return frameOffset;
        }


	}


	[Serializable]
	public class CepstralSonogramConfig : BaseSonogramConfig
	{
		public new static CepstralSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new CepstralSonogramConfig(config);
		}
        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
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
    } //end class CepstralSonogramConfig





	[Serializable]
	public class AVSonogramConfig : CepstralSonogramConfig
	{

		public new static AVSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new AVSonogramConfig(config);
		}

		public AVSonogramConfig(Configuration config) : base(config)
		{
            var duration = config.GetDoubleNullable("WAV_DURATION");
            if (duration != null)
                Duration = TimeSpan.FromSeconds(duration.Value);
            SampleRate = config.GetInt("WAV_SAMPLE_RATE");
			DeltaT = config.GetInt("DELTA_T"); // Frames between acoustic vectors
		}

		public override void Save(TextWriter writer)
		{
			base.Save(writer);
			writer.WriteConfigValue("DELTA_T", DeltaT);
            writer.WriteLine("#");
            writer.Flush();
		}

		public int DeltaT { get; set; }
	}
}