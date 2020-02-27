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

        public double BgNoiseThreshold { get; set; } = 3.0;

        /// <summary>
        /// DIFFERENCE SPECTROGRAM - PARAMETER (in decibels).
        /// </summary>
        public double DifferenceThreshold { get; set; } = 3.0;

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