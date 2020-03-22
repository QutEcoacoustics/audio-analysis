// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseSonogramConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SonogramConfig type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using DSP;
    using TowseyLibrary;

    [Serializable]
    public class SonogramConfig
    {
        public const int DEFAULT_WINDOW_SIZE = 512;

        // TODO  THIS DEFAULT SHOULD BE SET TO ZERO OVERLAP
        public const double DEFAULT_WINDOW_OVERLAP = 0.5;

        public string SourceFName { get; set; } // name of source file for recordingt

        public string SourceDirectory { get; set; } // location of source file - used only for debugging.

        public string CallName { get; set; } // label to use for segmentation of call and silence.

        public TimeSpan Duration { get; set; }

        public int WindowSize { get; set; }

        public int WindowStep { get; set; } // Exact frame step in samples - an alternative to overlap

        public double WindowOverlap { get; set; } // Percent overlap of frames

        public double WindowPower { get; set; } // Power of the Hamming Window

        private int sampleRate = 0;

        /// <summary>
        /// Gets or sets the channel to extract from the WavReader
        /// </summary>
        public int Channel { get; set; } = 0;

        public int SampleRate
        {
            get { return this.sampleRate; }

            set
            {
                this.sampleRate = value;
                this.NyquistFreq = value / 2;
            }
        }

        public int NyquistFreq { get; private set; }

        public string WindowFunction { get; set; } = WindowFunctions.HAMMING.ToString();

        public int NPointSmoothFFT { get; set; } // Number of points to smooth FFT spectra
= 3; // Number of points to smooth FFT spectra

        public double epsilon { get; set; } //small value to prevent log of zero value

        public bool DoPreemphasis { get; set; }

        public int? MinFreqBand { get; set; }

        public int? MaxFreqBand { get; set; }

        public int? MidFreqBand { get; set; }

        public bool DoFullBandwidth { get; set; }

        public bool DoSnr { get; set; }

        public NoiseReductionType NoiseReductionType { get; set; }

        public double NoiseReductionParameter { get; set; }

        //public FftConfiguration fftConfig { get; set; }
        public MfccConfiguration mfccConfig { get; set; }

        /// <summary>
        /// For linear frequency scale assume that the freq bin count = half the frame size.
        /// </summary>
        public int FreqBinCount => this.WindowSize / 2;  // other half is phase info

        public bool DoMelScale { get; set; }

        /// <summary>
        /// For linear frequency scale assume that the freq bin count = half the frame size.
        /// But with Mel scale, the user can set arbitrary number of mel bins.
        /// By default, MelBincount can be set = FreqBinCount.
        /// </summary>
        public int MelBinCount { get; set; }

        public int DeltaT { get; set; }

        public bool SaveSonogramImage { get; set; } = false;

        private readonly SonogramConfig config;
        private readonly Acoustics.Tools.Wav.WavReader wavReader;

        public string ImageDir { get; set; } = null;

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
                var config = new ConfigDictionary(configFile);
                if (config.GetInt("VERBOSITY") > 0)
                {
                    Log.Verbosity = 1;
                    Log.WriteIfVerbose("Verbosity set true in Application Config file.");
                }

                return new SonogramConfig(config);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonogramConfig"/> class.
        /// Default Constructor - initialises a configuration with the default values
        /// </summary>
        public SonogramConfig()
        {
            var config = new ConfigDictionary();

            config.SetPair(ConfigKeys.Windowing.Key_SampleRate, "0");
            config.SetPair(ConfigKeys.Windowing.Key_WindowSize,    DEFAULT_WINDOW_SIZE.ToString());
            config.SetPair(ConfigKeys.Windowing.Key_WindowOverlap, DEFAULT_WINDOW_OVERLAP.ToString());

            config.SetPair(ConfigKeys.EndpointDetection.Key_K1SegmentationThreshold, "3.5");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K2SegmentationThreshold, "6.0");
            config.SetPair(ConfigKeys.EndpointDetection.Key_K1K2Latency, "0.05");
            config.SetPair(ConfigKeys.EndpointDetection.Key_VocalGap, "0.2");
            config.SetPair(ConfigKeys.EndpointDetection.Key_MinVocalDuration, "0.075");

            config.SetPair(AnalysisKeys.NoiseReductionType, NoiseReductionType.None.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_WindowFunction, WindowFunctions.HAMMING.ToString());

            //config.SetPair(ConfigKeys.Mfcc.Key_WindowFunction, WindowFunctions.HANNING.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_NPointSmoothFFT, "3");
            config.SetPair(ConfigKeys.Mfcc.Key_DoMelScale, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_FilterbankCount, "64");
            config.SetPair(ConfigKeys.Mfcc.Key_CcCount, "12");
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDelta, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_IncludeDoubleDelta, false.ToString());
            config.SetPair(ConfigKeys.Mfcc.Key_DeltaT, "2");
            config.SetPair(ConfigKeys.Sonogram.Key_SonogramType, ConfigKeys.SonogramTypes.Spectral.ToString());
            config.SetPair(ConfigKeys.ImageSave.Key_AddGrid, false.ToString());
            this.Initialize(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonogramConfig"/> class.
        /// CONSTRUCTOR
        /// Initialises sonogram config with key-value-pairs in the passed ConfigDictionary
        /// </summary>
        public SonogramConfig(ConfigDictionary config)
        {
            this.Initialize(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SonogramConfig"/> class.
        /// CONSTRUCTOR
        /// Initialises sonogram config with key-value-pairs in the passed dictionary
        /// </summary>
        public SonogramConfig(Dictionary<string, string> dictionary)
        {
            this.Initialize(dictionary);
        }

        public SonogramConfig(SonogramConfig config, Acoustics.Tools.Wav.WavReader wavReader)
        {
            // TODO: Complete member initialization
            this.config = config;
            this.wavReader = wavReader;
        }

        /// <summary>
        /// DoSnr = true;
        /// DoFullBandwidth = false;
        /// </summary>
        /// <param name="config">read from file</param>
        private void Initialize(ConfigDictionary config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            this.CallName = config.GetString(ConfigKeys.Recording.Key_RecordingCallName);
            this.SourceFName = config.GetString(ConfigKeys.Recording.Key_RecordingFileName);
            var duration = config.GetDoubleNullable("WAV_DURATION");
            if (duration != null)
            {
                this.Duration = TimeSpan.FromSeconds(duration.Value);
            }

            //FRAMING PARAMETERS
            this.WindowSize = config.GetInt(ConfigKeys.Windowing.Key_WindowSize);
            this.WindowOverlap = config.GetDouble(ConfigKeys.Windowing.Key_WindowOverlap);

            //NOISE REDUCTION PARAMETERS
            this.DoSnr = true; // set false if only want to
            string noisereduce = config.GetString(AnalysisKeys.NoiseReductionType);

            //this.NoiseReductionType = (NoiseReductionType)Enum.Parse(typeof(NoiseReductionType), noisereduce.ToUpperInvariant());
            this.NoiseReductionType = (NoiseReductionType)Enum.Parse(typeof(NoiseReductionType), noisereduce);

            //FREQ BAND PARAMETERS
            this.DoFullBandwidth = false; // set true if only want to
            this.MinFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MinFreq);
            this.MaxFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MaxFreq);
            this.MidFreqBand = this.MinFreqBand + ((this.MaxFreqBand - this.MinFreqBand) / 2);

            //SEGMENTATION PARAMETERS
            EndpointDetectionConfiguration.SetConfig(config);

            //MFCC PARAMETERS
            this.DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            this.mfccConfig = new MfccConfiguration(config);
            this.DeltaT = config.GetInt(ConfigKeys.Mfcc.Key_DeltaT); // Frames between acoustic vectors

            // for generating only spectrogram.
        }

        /// <summary>
        /// DoSnr = true;
        /// DoFullBandwidth = false;
        /// </summary>
        /// <param name="configDict">Dictionary of config values</param>
        private void Initialize(Dictionary<string, string> configDict)
        {
            this.CallName = configDict[ConfigKeys.Recording.Key_RecordingCallName];
            this.SourceFName = configDict[ConfigKeys.Recording.Key_RecordingFileName];

            // var duration = config.GetDoubleNullable("WAV_DURATION");
            // if (duration != null) Duration = TimeSpan.FromSeconds(duration.Value);

            //FRAMING PARAMETERS
            this.WindowSize = 512; // default value
            if (configDict.ContainsKey(AnalysisKeys.FrameLength))
            {
                this.WindowSize = ConfigDictionary.GetInt(AnalysisKeys.FrameLength, configDict);
            }

            this.WindowOverlap = 0.0; // default value
            if (configDict.ContainsKey(AnalysisKeys.FrameOverlap))
            {
                this.WindowOverlap = ConfigDictionary.GetDouble(AnalysisKeys.FrameOverlap, configDict);
            }

            this.sampleRate = 0;
            if (configDict.ContainsKey(AnalysisKeys.ResampleRate))
            {
                this.sampleRate = ConfigDictionary.GetInt("ResampleRate", configDict);
            }

            //NOISE REDUCTION PARAMETERS
            // NoiseReductionParameter = config.GetDouble(SNR.key_Snr.key_);
            this.DoSnr = true; // set false if only want to
            this.NoiseReductionType = NoiseReductionType.None;
            if (configDict.ContainsKey(AnalysisKeys.NoiseReductionType))
            {
                string noiseReductionType = configDict[AnalysisKeys.NoiseReductionType];

                // this.NoiseReductionType = (NoiseReductionType)Enum.Parse(typeof(NoiseReductionType), noiseReductionType.ToUpperInvariant());
                this.NoiseReductionType = (NoiseReductionType)Enum.Parse(typeof(NoiseReductionType), noiseReductionType);
            }

            // FREQ BAND PARAMETERS
            this.DoFullBandwidth = true; // set true if only want to

            // MinFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MinFreq);
            // MaxFreqBand = config.GetIntNullable(ConfigKeys.Mfcc.Key_MaxFreq);
            // MidFreqBand = MinFreqBand + ((MaxFreqBand - MinFreqBand) / 2);

            // SEGMENTATION PARAMETERS
            // EndpointDetectionConfiguration.SetConfig(config);

            // MFCC PARAMETERS
            // DoMelScale = config.GetBoolean(ConfigKeys.Mfcc.Key_DoMelScale);
            // mfccConfig = new MfccConfiguration(config);
            // DeltaT = config.GetInt(ConfigKeys.Mfcc.Key_DeltaT); // Frames between acoustic vectors
        }

        public virtual void Save(TextWriter writer)
        {
            writer.WriteLine("#**************** INFO ABOUT FRAMES");
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_WindowSize, this.WindowSize);
            writer.WriteConfigValue(ConfigKeys.Windowing.Key_WindowOverlap, this.WindowOverlap);
            EndpointDetectionConfiguration.Save(writer);
            writer.WriteLine("#**************** INFO ABOUT SONOGRAM");
            writer.WriteConfigValue("MIN_FREQ", this.MinFreqBand);
            writer.WriteConfigValue("MAX_FREQ", this.MaxFreqBand);
            writer.WriteConfigValue("MID_FREQ", this.MidFreqBand); //=3500
            writer.WriteConfigValue(AnalysisKeys.NoiseReductionType, this.NoiseReductionType.ToString());
            if (this.NoiseReductionParameter > 1.0)
            {
                writer.WriteConfigValue(SNR.KeyDynamicRange, this.NoiseReductionParameter.ToString("F1"));
            }

            writer.WriteLine("#");
            writer.WriteLine("#**************** INFO ABOUT FEATURE EXTRACTION");
            writer.WriteLine("FEATURE_TYPE=mfcc");
            this.mfccConfig.Save(writer);
            writer.WriteConfigValue(ConfigKeys.Mfcc.Key_DeltaT, this.DeltaT);
            writer.WriteLine("#");
            writer.Flush();
        }

        /// <summary>
        /// returns duration of a full frame or window in seconds
        /// Assumes that the Window size is already available
        /// </summary>
        /// <returns>seconds</returns>
        public double GetFrameDuration(int sampleRate)
        {
            return this.WindowSize / (double)sampleRate;
        }

        /// <summary>
        /// returns the duration of that part of frame not overlapped with follwoing frame.
        /// Duration is given in seconds.
        /// Assumes that the sample rate, window size and overlap fraction are already known.
        /// </summary>
        public double GetFrameOffset()
        {
            double frameDuration = this.GetFrameDuration(this.SampleRate); // Duration of full frame or window in seconds
            double frameOffset = frameDuration * (1 - this.WindowOverlap); // Duration of non-overlapped part of window/frame in seconds
            return frameOffset;
        }

        /// <summary>
        /// returns the duration of that part of frame not overlapped with follwoing frame.
        /// Duration is given in seconds.
        /// Assumes window size and overlap fraction already known.
        /// </summary>
        public double GetFrameOffset(int sampleRate)
        {
            int step = DSP_Frames.FrameStep(this.WindowSize, this.WindowOverlap);
            return step / (double)sampleRate;
        }
    }
}
