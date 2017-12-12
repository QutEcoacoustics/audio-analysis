// <copyright file="IndexCalculateConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using DSP;

    /// <summary>
    /// CONFIG CLASS FOR the class IndexCalculate.cs
    /// </summary>
    public class IndexCalculateConfig
    {
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 22050) THEN frame duration = 5.805ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 22050) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 512 AND sample rate = 22050) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 128 AND sample rate = 11025) THEN frame duration = 11.61ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 11025) THEN frame duration = 23.22ms.
        // EXTRACT INDICES: IF (frameLength = 256 AND sample rate = 17640) THEN frame duration = 18.576ms.

        public const int DefaultResampleRate = 22050;
        public const int DefaultWindowSize = 512;
        public const int DefaultIndexCalculationDurationInSeconds = 60;

        // semi-arbitrary bounds between lf, mf and hf bands of the spectrum
        // The midband, 1000Hz to 8000Hz, covers the bird-band in SERF & Gympie recordings.
        public const int DefaultHighFreqBound = 11000;
        public const int DefaultMidFreqBound = 8000;
        public const int DefaultLowFreqBound = 1000;

        public const string DefaultFrequencyScaleType = "Linear";

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexCalculateConfig"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public IndexCalculateConfig()
        {
            this.IndexCalculationDuration = TimeSpan.FromSeconds(DefaultIndexCalculationDurationInSeconds);
            this.BgNoiseBuffer = TimeSpan.FromSeconds(5);

            this.FrameLength = DefaultWindowSize;

            this.ResampleRate = DefaultResampleRate;

            this.LowFreqBound = DefaultLowFreqBound;
            this.MidFreqBound = DefaultMidFreqBound;
            this.SetTypeOfFreqScale(DefaultFrequencyScaleType);
        }

        /// <summary>
        /// Gets or sets the Timespan (in seconds) over which summary and spectral indices are calculated
        /// Default=
        /// Units=seconds
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        /// <summary>
        /// Gets or sets bG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
        /// Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
        /// The  BG noise interval is not extended beyond start or end of recording segment.
        /// Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
        /// Default=5 seconds
        /// Units=seconds
        /// </summary>
        public TimeSpan BgNoiseBuffer { get; set; }

        /// <summary>
        /// Gets or sets the FrameWidth.
        /// FrameWidth is used WITHOUT overlap to calculate the spectral indices.
        /// Default value = 512.
        /// Units=samples
        /// </summary>
        public int FrameLength { get; set; }

        /// <summary>
        /// Gets or sets the ResampleRate.
        /// ResampleRate must be 2 X the desired Nyquist.
        /// Default value = 22050.
        /// Once upon a time we used 17640.
        /// Units=samples
        /// </summary>
        public int ResampleRate { get; set; }

        /// <summary>
        /// Gets or sets the LowFreqBound.
        /// Default value = 1000.
        /// Units=Herz
        /// </summary>
        public int LowFreqBound { get; set; }

        /// <summary>
        /// Gets or sets the MidFreqBound.
        /// Default value = 8000.
        /// Units=Herz
        /// </summary>
        public int MidFreqBound { get; set; }

        /// <summary>
        /// Frequency scale is Linear or OCtave
        /// </summary>
        public FreqScaleType frequencyScaleType;

        /// <summary>
        /// Gets or sets the type of Herz frequency scale
        /// </summary>
        public void SetTypeOfFreqScale(string scaleName)
        {
            if (scaleName == "Linear")
            {
                this.frequencyScaleType = FreqScaleType.Linear;
            }
            else
            if (scaleName == "Linear125Octaves7Tones28Nyquist32000")
            {
                this.frequencyScaleType = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            }
            else
            if (scaleName == "Octave")
            {
                this.frequencyScaleType = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
            }
            else
            {
                throw new Exception("ERROR:    Invalid FrequencyScaleType");
            }
        }

        public FreqScaleType GetTypeOfFreqScale()
        {
            return this.frequencyScaleType;
        }

        public static IndexCalculateConfig GetDefaultConfig()
        {
            return new IndexCalculateConfig();
        }

        /// <summary>
        /// Link method to one which does the real work.
        /// </summary>
        /// <param name="configFile">the config file to be read dynamically</param>
        public static IndexCalculateConfig GetConfig(FileInfo configFile)
        {
            dynamic dynamicConfig = Yaml.Deserialise(configFile);
            var config = IndexCalculateConfig.GetConfig(dynamicConfig, false);
            return config;
        }

        /// <summary>
        /// WARNING: This method does not incorporate all the parameters in the config.yml file.
        /// Only those that are likely to change.
        /// If you want to change a config parameter in the yml file make sure it appears in this method.
        /// </summary>
        /// <param name="dynamicConfig">the dynamic config</param>
        /// <param name="writeParameters">default = false</param>
        public static IndexCalculateConfig GetConfig(dynamic dynamicConfig, bool writeParameters = false)
        {
            var config = new IndexCalculateConfig
            {
                ResampleRate = (int?)dynamicConfig[AnalysisKeys.ResampleRate] ?? DefaultResampleRate,
                FrameLength = (int?)dynamicConfig[AnalysisKeys.FrameLength] ?? DefaultWindowSize,
                MidFreqBound = (int?)dynamicConfig[AnalysisKeys.MidFreqBound] ?? DefaultMidFreqBound,
                LowFreqBound = (int?)dynamicConfig[AnalysisKeys.LowFreqBound] ?? DefaultLowFreqBound,
            };

            double duration = (double?)dynamicConfig[AnalysisKeys.IndexCalculationDuration] ?? DefaultIndexCalculationDurationInSeconds;
            config.IndexCalculationDuration = TimeSpan.FromSeconds(duration);
            duration = (double?)dynamicConfig[AnalysisKeys.BgNoiseNeighbourhood] ?? 5.0;
            config.BgNoiseBuffer = TimeSpan.FromSeconds(duration);
            string stringvalue = (string)dynamicConfig["FrequencyScale"];
            config.SetTypeOfFreqScale(stringvalue);

            if (writeParameters)
            {
                // print out the sonogram parameters
                LoggedConsole.WriteLine("\nPARAMETERS");
                //foreach (KeyValuePair<string, string> kvp in configDict)
                //{
                //    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                //}
            }

            return config;
        }

        public static void WriteConfig(IndexCalculateConfig config, FileInfo configFile)
        {
            Yaml.Serialise<IndexCalculateConfig>(configFile, config);
        }
    }
}
