// <copyright file="SpectrogramImageType.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectrogramGenerator
{
    public enum SpectrogramImageType
        {
            Waveform = 0,
            DecibelSpectrogram = 1,
            DecibelSpectrogramNoiseReduced = 2,
            Experimental = 3,
            DifferenceSpectrogram = 4,
            CepstralSpectrogram = 5,
            AmplitudeSpectrogramLocalContrastNormalization = 6,
        }
}