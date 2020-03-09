// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpectrogramConstants.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SpectrogramConstants type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using Acoustics.Shared;

    public static class SpectrogramConstants
    {
        public const string DefaultAnalysisType = "Towsey.Acoustic";

        public const string RGBMap_DEFAULT = "ACI-ENT-CVR";

        public const string RGBMap_ACI_ENT_PMN = "ACI-ENT-PMN"; //R-G-B
        public const string RGBMap_ACI_ENT_CVR = "ACI-ENT-CVR"; //R-G-B
        public const string RGBMap_ACI_ENT_EVN = "ACI-ENT-EVN"; //R-G-B
        public const string RGBMap_ACI_ENT_SPT = "ACI-ENT-SPT"; //R-G-B

        // A second set of RGB mappings based on BGN and PMN.
        public const string RGBMap_BGN_PMN_EVN = "BGN-PMN-EVN"; //R-G-B
        public const string RGBMap_BGN_PMN_SPT = "BGN-PMN-SPT"; //R-G-B
        public const string RGBMap_BGN_PMN_OSC = "BGN-PMN-OSC";
        public const string RGBMap_BGN_PMN_RHZ = "BGN-PMN-RHZ";
        public const string RGBMap_BGN_PMN_CVR = "BGN-PMN-CVR"; //R-G-B

        // these parameters manipulate the colour map and appearance of the false-colour LONG DURATION spectrogram
        public const double BACKGROUND_FILTER_COEFF = -0.25; //value must lie in -1.0 to +1.0

        // These parameters describe the time and frequency scales for drawing X and Y axes on LONG DURATION spectrograms
        public static TimeSpan X_AXIS_TIC_INTERVAL = TimeSpan.FromMinutes(60);  // default assumes one minute spectra and 60 spectra per hour
        public static TimeSpan MINUTE_OFFSET = TimeSpan.Zero;    // assume recording starts at zero minute of day i.e. midnight
        public static int SAMPLE_RATE = AppConfigHelper.DefaultTargetSampleRate;
        public const int FRAME_LENGTH = 512;    // default value - from which spectrogram was derived
        public const int HEIGHT_OF_TITLE_BAR = 24;
    }
}
