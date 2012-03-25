// -----------------------------------------------------------------------
// <copyright file="ISpectrogramUtility.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Acoustics.Tools.Audio
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public interface ISpectrogramUtility
    {
        /// <summary>
        /// Create a spectrogram from a segment of the <paramref name="source"/> audio file.
        /// <paramref name="output"/> image file will be created.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
        /// </param>
        /// <param name="output">
        /// The output image file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMimeType">
        /// The output Mime Type.
        /// </param>
        void Create(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType);
    }
}
