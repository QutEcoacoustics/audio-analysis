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
        public static FileInfo ResolveConcatenationConfig(string fileName)
        {
            return new FileInfo(@"..\\..\\..\\..\\AudioAnalysis\\AnalysisConfigFiles\\" + fileName);
        }
    }
}