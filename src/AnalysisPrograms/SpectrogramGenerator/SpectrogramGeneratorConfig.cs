// <copyright file="SpectrogramGeneratorConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using AnalysisBase;

    public class SpectrogramGeneratorConfig : AnalyzerConfig
    {
#pragma warning disable SA1623 // Property summary documentation should match accessors
        public int WaveformHeight { get; set; } = 100;

        public double BgNoiseThreshold { get; set; } = 0.0;

        /// <summary>
        /// DIFFERENCE SPECTROGRAM - PARAMETER (in decibels).
        /// </summary>
        public double DifferenceThreshold { get; set; } = 3.0;

        /// <summary>
        /// CEPSTROGRAM - PARAMETER.
        /// Do pre-emphasis prior to FFT.
        /// </summary>
        public bool DoPreemphasis { get; set; } = false;

        /// <summary>
        /// CEPSTROGRAM - PARAMETER
        /// The size of the Mel-scale filter bank.
        /// The default value is 64.
        /// THe minimum I have seen referenced = 26.
        /// </summary>
        public int FilterbankCount { get; set; } = 64;

        /// <summary>
        /// CEPSTROGRAM - PARAMETER.
        /// Include the delta features in the returned MFCC feature vector.
        /// </summary>
        public bool IncludeDelta { get; set; } = false;

        /// <summary>
        /// CEPSTROGRAM - PARAMETER.
        /// Include the delta-delta or acceleration features in the returned MFCC feature vector.
        /// </summary>
        public bool IncludeDoubleDelta { get; set; } = false;

        /// <summary>
        /// LOCAL CONTRAST NORMALIZATION PARAMETERS.
        /// </summary>
        public double NeighborhoodSeconds { get; set; } = 0.5;

        /// <summary>
        /// LOCAL CONTRAST NORMALIZATION PARAMETERS.
        /// </summary>
        public double LcnContrastLevel { get; set; } = 0.2;

        /// <summary>
        /// Which images to draw. Defaults to all of them.
        /// </summary>
        public SpectrogramImageType[] Images { get; set; } = (SpectrogramImageType[])Enum.GetValues(typeof(SpectrogramImageType));

#pragma warning restore SA1623 // Property summary documentation should match accessors
    }
}