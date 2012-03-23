namespace Acoustics.Tools.Wav
{
    using System.IO;
    using System;
    using System.Collections.Generic;

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
