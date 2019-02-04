// <copyright file="IndexCalculateConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Indices
{
    using System;
    using Acoustics.Shared.ConfigFile;

    using DSP;
    using Equ;

    using Fasterflect;

    using log4net;

    using Newtonsoft.Json;

    using YamlDotNet.Serialization;

    /// <summary>
    /// CONFIG CLASS FOR the class IndexCalculate.cs
    /// </summary>
    public class IndexCalculateConfig : AnalyzerConfigIndexProperties, IEquatable<IndexCalculateConfig>, ICloneable
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

        public const FreqScaleType DefaultFrequencyScaleType = FreqScaleType.Linear;

        public const double DefaultMinBandWidth = 0.0;
        public const double DefaultMaxBandWidth = 1.0;

        public const int DefaultMelScale = 0;

        public const double DefaultBgNoiseNeighborhood = 5;

        private static readonly ILog Log = LogManager.GetLogger(nameof(IndexCalculateConfig));

        // Make sure the comparer is static, so that the equality operations are only generated once
        private static readonly MemberwiseEqualityComparer<IndexCalculateConfig> Comparer =
            MemberwiseEqualityComparer<IndexCalculateConfig>.ByFields;

        private FreqScaleType frequencyScale;
        private double indexCalculationDuration = DefaultIndexCalculationDurationInSeconds;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexCalculateConfig"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public IndexCalculateConfig()
        {
            this.IndexCalculationDurationTimeSpan = TimeSpan.FromSeconds(DefaultIndexCalculationDurationInSeconds);
            this.BgNoiseNeighborhood = DefaultBgNoiseNeighborhood;

            this.FrameLength = DefaultWindowSize;

            this.ResampleRate = DefaultResampleRate;

            this.LowFreqBound = DefaultLowFreqBound;
            this.MidFreqBound = DefaultMidFreqBound;
            this.FrequencyScale = DefaultFrequencyScaleType;

            this.MinBandWidth = DefaultMinBandWidth;
            this.MaxBandWidth = DefaultMaxBandWidth;

            this.MelScale = DefaultMelScale;
        }

        /// <summary>
        /// Gets or sets the Timespan (in seconds) over which summary and spectral indices are calculated
        /// Default=60.0
        /// Units=seconds
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public TimeSpan IndexCalculationDurationTimeSpan
        {
            get => this.indexCalculationDuration.Seconds();
            set => this.indexCalculationDuration = value.TotalSeconds;
        }

        /// <summary>
        /// Gets or sets the duration of the sub-segment for which indices are calculated.
        /// Default = 60 seconds i.e. same duration as the Segment.
        /// </summary>
        public double IndexCalculationDuration
        {
            get => this.indexCalculationDuration;
            protected set => this.indexCalculationDuration = value;
        }

        /// <summary>
        /// Gets bG noise for any location is calculated by extending the region of index calculation from 5 seconds before start to 5 sec after end of current index interval.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public TimeSpan BgNoiseBuffer => this.BgNoiseNeighborhood.Seconds();

        /// <summary>
        /// Gets or sets the amount of audio either side of the required subsegment from which to derive an estimate of background noise.
        /// Units = seconds
        /// As an example: IF (IndexCalculationDuration = 1 second) AND (BGNNeighborhood = 10 seconds)
        ///                THEN BG noise estimate will be derived from 21 seconds of audio centred on the subsegment.
        ///                In case of edge effects, the BGnoise neighborhood will be truncated to start or end of the audio segment (typically expected to be one minute long).
        /// </summary>
        /// <remarks>
        /// Ten seconds is considered a minimum interval to obtain a reliable estimate of BG noise.
        /// The  BG noise interval is not extended beyond start or end of recording segment.
        /// Consequently for a 60sec Index calculation duration, the  BG noise is calculated form the 60sec segment only.
        /// Default=5 seconds
        /// </remarks>
        public double BgNoiseNeighborhood { get; set; }

        /// <summary>
        /// Gets or sets the FrameWidth - the number of samples to use per FFT window.
        /// FrameWidth is used WITHOUT overlap to calculate the spectral indices.
        /// Default value = 512.
        /// Units=samples
        /// </summary>
        public int FrameLength { get; set; }

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
        /// Gets or sets frequency scale is Linear or OCtave
        /// </summary>
        public FreqScaleType FrequencyScale
        {
            get => this.frequencyScale;
            set
            {
                // only a subset of FreqScaleType are supported
                switch (value)
                {
                    case FreqScaleType.Linear:
                    case FreqScaleType.Octave:
                        this.frequencyScale = value;
                        break;
                    default:
                        throw new ArgumentException($"Invalid value set for {nameof(this.frequencyScale)}");
                }

                this.frequencyScale = value;
            }
        }

        /// <summary>
        /// Gets or sets the fraction-valued minimum to be used in a pseudo-bandpass filter.
        /// </summary>
        public double MinBandWidth { get; set; }

        /// <summary>
        /// Gets or sets the fraction-valued maximum to be used in a pseudo-bandpass filter.
        /// </summary>
        public double MaxBandWidth { get; set; }

        /// <summary>
        /// Gets or sets the number of Mel-scale filter banks to use.
        /// </summary>
        /// <remarks>
        /// The default, 0, implies no operation
        /// </remarks>
        public int MelScale { get; set; }

        /// <summary>
        /// WARNING: This method does not incorporate all the parameters in the config.yml file.
        /// Only those that are likely to change.
        /// If you want to change a config parameter in the yml file make sure it appears in this method.
        /// </summary>
        /// <param name="configuration">the Config config</param>
        /// <param name="writeParameters">default = false</param>
        [Obsolete("Incorporation of statically typed config should obviate need for this method")]
        public static IndexCalculateConfig GetConfig(Config configuration,  bool writeParameters = false)
        {
            var config = new IndexCalculateConfig
            {
                ResampleRate = configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? DefaultResampleRate,
                FrameLength = configuration.GetIntOrNull(AnalysisKeys.FrameLength) ?? DefaultWindowSize,
                MidFreqBound = configuration.GetIntOrNull(AnalysisKeys.MidFreqBound) ?? DefaultMidFreqBound,
                LowFreqBound = configuration.GetIntOrNull(AnalysisKeys.LowFreqBound) ?? DefaultLowFreqBound,
            };

            var duration = configuration.GetDoubleOrNull(AnalysisKeys.IndexCalculationDuration);
            config.IndexCalculationDurationTimeSpan = (duration ?? DefaultIndexCalculationDurationInSeconds).Seconds();
            duration = configuration.GetDoubleOrNull(AnalysisKeys.BgNoiseNeighbourhood);
            config.BgNoiseNeighborhood = duration ?? DefaultBgNoiseNeighborhood;

            if (!Enum.TryParse<FreqScaleType>(configuration["FrequencyScale"], true, out var scaleType))
            {
                scaleType = DefaultFrequencyScaleType;
            }

            config.FrequencyScale = scaleType;

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

        public bool Equals(IndexCalculateConfig other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IndexCalculateConfig);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        object ICloneable.Clone()
        {
            IndexCalculateConfig deepClone = this.DeepClone();
            Log.Trace("Cloning a copy of IndexCalculateConfig");
            return deepClone;
        }
    }
}
