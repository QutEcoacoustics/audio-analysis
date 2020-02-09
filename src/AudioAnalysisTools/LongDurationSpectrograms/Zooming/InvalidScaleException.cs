// <copyright file="InvalidScaleException.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using Acoustics.Shared.ConfigFile;

    public class InvalidScaleException : ConfigFileException
    {
        public InvalidScaleException(string message)
            : base(message)
        {
        }
    }
}