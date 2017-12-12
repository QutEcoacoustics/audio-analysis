// <copyright file="Enums.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared
{
    /// <summary>
    /// The spectrogram type.
    /// </summary>
    public enum SpectrogramType
    {
        /// <summary>
        /// The wave form.
        /// </summary>
        WaveForm = 0,

        /// <summary>
        /// The spectrogram.
        /// </summary>
        Spectrogram = 1
    }

    public enum ImageChrome
    {
        Without = 0,
        With = 1
    }

    public static class EnumExtenstions
    {
        public static ImageChrome ToImageChrome(this bool chromeOrNot) => chromeOrNot ? ImageChrome.With : ImageChrome.Without;
    }
}