using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysis
{
	[Serializable]
	public class SonogramConfig
	{

        #region Properties
        public string SourceFName { get; set; }
        public string CallName { get; set; }//label to use for segmentation of call and silence.

        public FftConfiguration FftConfig { get; set; }
        public TimeSpan Duration { get; set; }
        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; } // Percent overlap of frames
        public int  FreqBinCount { get { return WindowSize / 2; } } // other half is phase info
        public bool DoPreemphasis { get; set; }
        public bool DoMelScale { get; set; }
        public ConfigKeys.NoiseReductionType NoiseReductionType { get; set; }
        public string SilenceRecordingPath { get; set; }
        public double[] SilenceModel { get; set; }
        public double   DynamicRange { get; set; }

        public int? MinFreqBand { get; set; }
        public int? MaxFreqBand { get; set; }
        public int? MidFreqBand { get; set; }
        public bool DoFullBandwidth { get; set; }
        #endregion


		public static SonogramConfig Load(string configFile)
		{
            if (!File.Exists(configFile))
            {
                Log.WriteLine("The configuraiton file <" + configFile + "> does not exist!");
                Log.WriteLine("Initialising application with default parameter values.");
                return new SonogramConfig();
            }
            else
            {
                Configuration config = new Configuration(configFile);
                if (config.GetInt("VERBOSITY") > 0)
                {
                    Log.Verbosity = 1;
                    Log.WriteIfVerbose("Verbosity set true in Application Config file.");

                }
                return new SonogramConfig(config);
            }
		}

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SonogramConfig()
        {
            Configuration config = new Configuration();

            config.SetPair(ConfigKeys.Windowing.Key_SampleRate, "0");
            config.SetPair(ConfigKeys.Windowing.Key_WindowSize, "512");
            config.SetPair(ConfigKeys.Windowing.Key_WindowOverlap, "0.5");

            config.SetPair(ConfigKeys.Mfcc.Key_NoiseReductionType, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_WindowFunction, ConfigKeys.WindowFunctions.HAMMING.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_NPointSmoothFFT, "3");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold, "3.5");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold, "6.0");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K1K2Latency, "0.05");
            config.SetPair(ConfigKeys.EndpointDetection.Key_VocalGap, "0.2");
            config.SetPair(ConfigKeys.EndpointDetection.Key_MinVocalDuration, "0.075");


            config.SetPair(ConfigKeys.Mfcc.Key_DoMelScale, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_FilterbankCount, "64");
            config.SetPair(ConfigKeys.Mfcc.Key_CcCount, "12");
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDelta, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDoubleDelta, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DeltaT, "2");

            config.SetPair(ConfigKeys.Sonogram.Key_SonogramType, ConfigKeys.SonogramTypes.spectral.ToString());

            config.SetPair(ConfigKeys.ImageSave.Key_AddGrid, false.ToString());

            Initialize(config);
                        
        }
        

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
		public SonogramConfig(Configuration config)
		{
            Initialize(config);
		}

        private void Initialize(Configuration config)
        {
            CallName    = config.GetString(ConfigKeys.Recording.Key_RecordingCallName);
            SourceFName = config.GetString(ConfigKeys.Recording.Key_RecordingFileName);
            FftConfig = new FftConfiguration(config);
            WindowSize = config.GetInt(ConfigKeys.Windowing.Key_WindowSize);
            WindowOverlap = config.GetDouble(ConfigKeys.Windowing.Key_WindowOverlap);

            DynamicRange = config.GetDouble(ConfigKeys.Snr.Key_DynamicRange);
            DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            string noisereduce = config.GetString(ConfigKeys.Mfcc.Key_NoiseReductionType);
            NoiseReductionType = (ConfigKeys.NoiseReductionType)Enum.Parse(typeof(ConfigKeys.NoiseReductionType), noisereduce);
            SilenceRecordingPath = config.GetString(ConfigKeys.Snr.Key_SilenceRecording);
            MinFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MinFreq);
            MaxFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MaxFreq);
            int? delta = MaxFreqBand - MinFreqBand;
            MidFreqBand = MinFreqBand + (delta / 2);
            DoFullBandwidth = false;

            EndpointDetectionConfiguration.SetConfig(config);
        }


		public virtual void Save(TextWriter writer)
		{
            writer.WriteLine("#**************** INFO ABOUT FRAMES");
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_WindowSize, WindowSize);
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_WindowOverlap, WindowOverlap);
            EndpointDetectionConfiguration.Save(writer);
            writer.WriteLine("#**************** INFO ABOUT SONOGRAM");
            writer.WriteConfigValue("MIN_FREQ", MinFreqBand);
			writer.WriteConfigValue("MAX_FREQ", MaxFreqBand);
            writer.WriteConfigValue("MID_FREQ", MidFreqBand); //=3500
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_NoiseReductionType, this.NoiseReductionType.ToString());
            if (this.DynamicRange > 1.0)
                writer.WriteConfigValue(ConfigKeys.Snr.Key_DynamicRange, this.DynamicRange.ToString("F1"));
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT FEATURE EXTRACTION");
            writer.WriteLine("FEATURE_TYPE=mfcc");
            FftConfig.Save(writer);
		}


		public double GetFrameDuration(int sampleRate)
		{
			return WindowSize / (double)sampleRate; // Duration of full frame or window in seconds
		}

        public double GetFrameOffset()
        {
            double frameDuration = GetFrameDuration(this.FftConfig.SampleRate); // Duration of full frame or window in seconds
            double frameOffset = frameDuration * (1 - WindowOverlap);           // Duration of non-overlapped part of window/frame in seconds
            return frameOffset; 
        }

        public double GetFrameOffset(int sampleRate)
        {
            double frameDuration = GetFrameDuration(sampleRate);
            double frameOffset = frameDuration * (1 - WindowOverlap);
            return frameOffset;
        }


    } // end BaseSonogramConfig()


	[Serializable]
	public class CepstralSonogramConfig : SonogramConfig
	{
		public new static CepstralSonogramConfig Load(string configFile)
		{
			var config = new Configuration(configFile);
			return new CepstralSonogramConfig(config);
		}

        private bool saveSonogramImage = false;
        public  bool SaveSonogramImage { get { return saveSonogramImage; } set { saveSonogramImage = value; } }
        private string imageDir = null;
        public  string ImageDir { get { return imageDir; } set { imageDir = value; } }
        public MfccConfiguration MfccConfiguration { get; set; }
        public int DeltaT { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="config"></param>
		public CepstralSonogramConfig(Configuration config) : base(config)
		{
			MfccConfiguration = new MfccConfiguration(config);
            //SampleRate   = config.GetInt(ConfigKeys.Windowing.Key_SampleRate);
            DeltaT       = config.GetInt(ConfigKeys.Mfcc.Key_DeltaT); // Frames between acoustic vectors
            var duration = config.GetDoubleNullable("WAV_DURATION");
            if (duration != null) Duration = TimeSpan.FromSeconds(duration.Value);
        }

		public override void Save(TextWriter writer)
		{
			base.Save(writer);
			MfccConfiguration.Save(writer);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_DeltaT, DeltaT);
            writer.WriteLine("#");
            writer.Flush();
        }

    } //end class CepstralSonogramConfig

}