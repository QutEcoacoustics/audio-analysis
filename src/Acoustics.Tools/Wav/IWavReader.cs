// <copyright file="IWavReader.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Tools.Wav
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Wav streaming interface.
    /// </summary>
    public interface IWavReader : IDisposable
    {
        /// <summary>
        /// Gets Samples. Make no assumptions about the position of the stream.
        /// Use this.Chunks to position the stream at the start of the chunk you want.
        /// </summary>
        Stream SampleStream { get; }

        /// <summary>
        /// Gets Samples.
        /// </summary>
        double[] Samples { get; }

        /// <summary>
        /// Gets Chunks in the wav stream.
        /// </summary>
        IEnumerable<WavChunk> Chunks { get; }

        /// <summary>
        /// Gets wav Audio Info.
        /// </summary>
        WavAudioInfo AudioInfo { get; }
    }
}