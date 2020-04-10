// <copyright file="ISpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Interfaces
{
    public interface ISpectralBand
    {
        /// <summary>
        /// Gets the bottom frequency bound of the acoustic event.
        /// </summary>
        double LowFrequencyHertz { get; }

        double HighFrequencyHertz { get; }

        double BandWidth => this.HighFrequencyHertz - this.LowFrequencyHertz;
    }
}