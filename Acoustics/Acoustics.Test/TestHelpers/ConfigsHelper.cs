// <copyright file="ConfigsHelper.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.TestHelpers
{
    using System;
    using System.IO;
    using Acoustics.Shared;
    using global::AudioAnalysisTools.LongDurationSpectrograms;

    public class ConfigsHelper
    {
        public static LdSpectrogramConfig GetDefaultFalseColourSpgmConfig()
        {
            var config = new LdSpectrogramConfig
            {
                XAxisTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT,
                ColourGain = 2.0,
                ColourFilter = 0.75,
            };

            // minutes x-axis scale
            config.XAxisTicInterval = TimeSpan.FromMinutes(60);

            // Hertz y-axis scale
            config.YAxisTicInterval = 1000;
            return config;
        }

        public static void WriteDefaultFalseColourSpgmConfig(FileInfo file)
        {
            var config = GetDefaultFalseColourSpgmConfig();
            Yaml.Serialise(file, config);
        }
    }
}