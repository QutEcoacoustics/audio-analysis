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

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureLearningSettings"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public FeatureLearningSettings()
        {
            this.FrequencyScaleType = DefaultFrequencyScaleType;
            //this.HertzInterval = DefaultHertzInterval;
            this.FrameSize = DefaultFrameSize;
            this.FinalBinCount = DefaultFinalBinCount;
            this.MinFreqBin = DefaultMinFreqBin;
            this.MaxFreqBin = DefaultMaxFreqBin;
            this.NumFreqBand = DefaultNumFreqBand;
            //this.PatchWidth = DefaultPatchWidth;
            this.PatchHeight = DefaultPatchHeight;
            this.NumRandomPatches = DefaultNumRandomPatches;
            this.NumClusters = DefaultNumClusters;
            this.DoNoiseReduction = DefaultDoNoiseReduction;
            this.DoWhitening = DefaultDoWhitening;
        }

        public FreqScaleType FrequencyScaleType { get; set; }

        //public int HertzInterval { get; set; }

        public int FrameSize { get; set; }

        public int FinalBinCount { get; set; }

        public int MinFreqBin { get; set; }

        public int MaxFreqBin { get; set; }

        public int NumFreqBand { get; set; }

        //public int PatchWidth { get; set; }

        public int PatchHeight { get; set; }

        public int FrameWindowLength { get; set; }

        public int StepSize { get; set; }

        public int NumRandomPatches { get; set; }

        public int NumClusters { get; set; }

        public bool DoNoiseReduction { get; set; }

        public bool DoWhitening { get; set; }
    }
}
