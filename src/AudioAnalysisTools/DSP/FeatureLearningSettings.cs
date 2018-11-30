// <copyright file="SpectrogramSettings.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using Acoustics.Shared.ConfigFile;

    public class FeatureLearningSettings : Config
    {
        public const FreqScaleType DefaultFrequencyScaleType = FreqScaleType.Mel;

        //public const int DefaultHertzInterval = 1000;

        public const int DefaultFrameSize = 1024;

        public const int DefaultFinalBinCount = 128;

        public const int DefaultMinFreqBin = 1;

        public const int DefaultMaxFreqBin = DefaultFinalBinCount;

        public const int DefaultNumFreqBand = 1;

        //public const int DefaultPatchWidth = (DefaultMaxFreqBin - DefaultMinFreqBin + 1) / DefaultNumFreqBand;

        public const int DefaultPatchHeight = 1;

        public const int DefaultFrameWindowLength = 1;

        public const int DefaultStepSize = 1;

        public const int DefaultNumRandomPatches = 4;

        public const int DefaultNumClusters = 256;

        public const bool DefaultDoNoiseReduction = true;

        public const bool DefaultDoWhitening = true;

        public const int DefaultMaxPoolingFactor = 1;

        public const bool DefaultDoSegmentation = true;

        public const double DefaultSubsegmentDurationInSeconds = 1.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureLearningSettings"/> class.
        /// CONSTRUCTOR
        /// </summary>

        /*
        public FeatureLearningSettings()
        {
            this.FrequencyScaleType = DefaultFrequencyScaleType;
            this.FrameSize = DefaultFrameSize;
            this.FinalBinCount = DefaultFinalBinCount;
            this.MinFreqBin = DefaultMinFreqBin;
            this.MaxFreqBin = DefaultMaxFreqBin;
            this.NumFreqBand = DefaultNumFreqBand;
            this.PatchHeight = DefaultPatchHeight;
            this.NumRandomPatches = DefaultNumRandomPatches;
            this.NumClusters = DefaultNumClusters;
            this.DoNoiseReduction = DefaultDoNoiseReduction;
            this.DoWhitening = DefaultDoWhitening;
            this.MaxPoolingFactor = DefaultMaxPoolingFactor;
        }
        */

        public FreqScaleType FrequencyScaleType { get; set; } = DefaultFrequencyScaleType;

        //public int HertzInterval { get; set; } = DefaultHertzInterval;

        public int FrameSize { get; set; } = DefaultFrameSize;

        public int FinalBinCount { get; set; } = DefaultFinalBinCount;

        public int MinFreqBin { get; set; } = DefaultMinFreqBin;

        public int MaxFreqBin { get; set; } = DefaultMaxFreqBin;

        public int NumFreqBand { get; set; } = DefaultNumFreqBand;

        //public int PatchWidth { get; set; } = DefaultPatchWidth;

        public int PatchHeight { get; set; } = DefaultPatchHeight;

        public int FrameWindowLength { get; set; } = DefaultFrameWindowLength;

        public int StepSize { get; set; } = DefaultStepSize;

        public int NumRandomPatches { get; set; } = DefaultNumRandomPatches;

        public int NumClusters { get; set; } = DefaultNumClusters;

        public bool DoNoiseReduction { get; set; } = DefaultDoNoiseReduction;

        public bool DoWhitening { get; set; } = DefaultDoWhitening;

        public int MaxPoolingFactor { get; set; } = DefaultMaxPoolingFactor;

        public bool DoSegmentation { get; set; } = DefaultDoSegmentation;

        public double SubsegmentDurationInSeconds { get; set; } = DefaultSubsegmentDurationInSeconds;
    }
}
