// <copyright file="SpectralPeakTrackingConfig.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectralPeakTracking
{
    using Acoustics.Shared.ConfigFile;
    using AudioAnalysisTools;

    public class SpectralPeakTrackingConfig : Config
    {
        // frame width and frame overlap
        public const int DefaultFrameWidth = 1024;
        public const double DefaultFrameOverlap = 0.2;

        public int FrameWidth { get; set; } = DefaultFrameWidth;

        public double FrameOverlap { get; set; } = DefaultFrameOverlap;

        public SpectralPeakTrackingSettings SptSettings { get; set; } = new SpectralPeakTrackingSettings();
    }
}