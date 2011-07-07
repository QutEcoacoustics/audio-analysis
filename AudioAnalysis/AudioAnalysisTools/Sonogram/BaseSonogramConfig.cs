using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;
using System.IO;

namespace AudioAnalysisTools
{
	[Serializable]
	public class SonogramConfig
	{
        public const int DEFAULT_WINDOW_SIZE = 512;
        public const double DEFAULT_WINDOW_OVERLAP = 0.5;


        #region Properties
        public string SourceFName { get; set; }     // name of source file for recordingt
        public string SourceDirectory { get; set; } // location of source file - used only for debugging.
        public string CallName { get; set; }        // label to use for segmentation of call and silence.
        public TimeSpan Duration { get; set; }

        public int WindowSize { get; set; }
        public double WindowOverlap { get; set; } // Percent overlap of frames
        public double WindowPower   { get; set; } // Power of the Hamming Window

        public double epsilon { get; set; }         //small value to prevent log of zero value
        public int  FreqBinCount { get { return WindowSize / 2; } } // other half is phase info
        public bool DoPreemphasis { get; set; }
        public int? MinFreqBand { get; set; }
        public int? MaxFreqBand { get; set; }
        public int? MidFreqBand { get; set; }
        public bool DoFullBandwidth { get; set; }

        public bool DoSnr { get; set; }
        public NoiseReductionType NoiseReductionType { get; set; }
        public double NoiseReductionParameter { get; set; }

        public FftConfiguration fftConfig { get; set; }
        public MfccConfiguration mfccConfig { get; set; }
        public bool DoMelScale { get; set; }
        public int DeltaT { get; set; }

        private bool saveSonogramImage = false;
        public  bool SaveSonogramImage { get { return saveSonogramImage; } set { saveSonogramImage = value; } }
        private string imageDir = null;
        public  string ImageDir { get { return imageDir; } set { imageDir = value; } }

        #endregion


		public static SonogramConfig Load(string configFile)
		{
            Log.WriteLine("config file =" + configFile);
            if (!File.Exists(configFile))
            {
                Log.WriteLine("The configuration file <" + configFile + "> does not exist!");
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
        /// Initialises a configuration with the default values
        /// </summary>
        public SonogramConfig()
        {
            Configuration config = new Configuration();

            config.SetPair(ConfigKeys.Windowing.Key_SampleRate, "0");
            config.SetPair(ConfigKeys.Windowing.Key_WindowSize,    DEFAULT_WINDOW_SIZE.ToString());
            config.SetPair(ConfigKeys.Windowing.Key_WindowOverlap, DEFAULT_WINDOW_OVERLAP.ToString());

            config.SetPair(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold, "3.5");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold, "6.0");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K1K2Latency, "0.05");
            config.SetPair(ConfigKeys.EndpointDetection.Key_VocalGap, "0.2");
            config.SetPair(ConfigKeys.EndpointDetection.Key_MinVocalDuration, "0.075");

            config.SetPair(ConfigKeys.Mfcc.Key_NoiseReductionType, NoiseReductionType.NONE.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_WindowFunction, TowseyLib.WindowFunctions.HAMMING.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_NPointSmoothFFT, "3");
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

        /// <summary>
        /// DoSnr = true;
        /// DoFullBandwidth = false;
        /// </summary>
        /// <param name="config"></param>
        private void Initialize(Configuration config)
        {
            CallName    = config.GetString(ConfigKeys.Recording.Key_RecordingCallName);
            SourceFName = config.GetString(ConfigKeys.Recording.Key_RecordingFileName);
            var duration = config.GetDoubleNullable("WAV_DURATION");
            if (duration != null) Duration = TimeSpan.FromSeconds(duration.Value);

            //FRAMING PARAMETERS
            WindowSize = config.GetInt(ConfigKeys.Windowing.Key_WindowSize);
            WindowOverlap = config.GetDouble(ConfigKeys.Windowing.Key_WindowOverlap);
            fftConfig = new FftConfiguration(config);

            //NOISE REDUCTION PARAMETERS  
            DoSnr = true; // set false if only want to 
            string noisereduce = config.GetString(ConfigKeys.Mfcc.Key_NoiseReductionType);
            NoiseReductionType = (NoiseReductionType)Enum.Parse(typeof(NoiseReductionType), noisereduce.ToUpperInvariant());
            NoiseReductionParameter       = config.GetDouble(SNR.key_Snr.key_DYNAMIC_RANGE);

            //FREQ BAND PARAMETERS
            DoFullBandwidth = false; // set true if only want to 
            MinFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MinFreq);
            MaxFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MaxFreq);
            MidFreqBand = MinFreqBand + ((MaxFreqBand - MinFreqBand) / 2);

            //SEGMENTATION PARAMETERS
            EndpointDetectionConfiguration.SetConfig(config);

            //MFCC PARAMETERS
            DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            mfccConfig = new MfccConfiguration(config);
            DeltaT = config.GetInt(ConfigKeys.Mfcc.Key_DeltaT); // Frames between acoustic vectors

            // for generating only spectrogram.

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
            if (this.NoiseReductionParameter > 1.0)
                writer.WriteConfigValue(SNR.key_Snr.key_DYNAMIC_RANGE, this.NoiseReductionParameter.ToString("F1"));
            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT FEATURE EXTRACTION");
            writer.WriteLine("FEATURE_TYPE=mfcc");
            fftConfig.Save(writer);
            mfccConfig.Save(writer);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_DeltaT, DeltaT);
            writer.WriteLine("#");
            writer.Flush();

		}

        /// <summary>
        /// returns duration of a full frame or window in seconds
        /// Assumes that the Window size is already available
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns>seconds</returns>
		public double GetFrameDuration(int sampleRate)
		{
			return WindowSize / (double)sampleRate; 
		}

        /// <summary>
        /// returns the duration of that part of frame not overlapped with follwoing frame.  
        /// Duration is given in seconds.
        /// Assumes that the sample rate, window size and overlap fraction are already known.
        /// </summary>
        /// <returns></returns>
        public double GetFrameOffset()
        {
            double frameDuration = GetFrameDuration(this.fftConfig.SampleRate); // Duration of full frame or window in seconds
            double frameOffset = frameDuration * (1 - WindowOverlap);           // Duration of non-overlapped part of window/frame in seconds
            return frameOffset; 
        }

        /// <summary>
        /// returns the duration of that part of frame not overlapped with follwoing frame.  
        /// Duration is given in seconds.
        /// Assumes window size and overlap fraction already known.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public double GetFrameOffset(int sampleRate)
        {
            double frameDuration = GetFrameDuration(sampleRate);
            double frameOffset = frameDuration * (1 - WindowOverlap);
            return frameOffset;
        }


    } // end BaseSonogramConfig()


}
