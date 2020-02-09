// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEventRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the IEventRecognizer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    /// <summary>
    /// This interface specializes IAnalyser2 to be a species recognizer.
    /// </summary>
    public interface IEventRecognizer : IAnalyser2
    {
        /// <summary>
        /// Do your analysis. This method is called once per segment (typically one-minute segments).
        /// </summary>
        /// <param name="audioRecording">The audio recording to process - it should be a minute or two long.</param>
        /// <param name="configuration">The configuration to use for this analysis.</param>
        /// <param name="segmentStartOffset">In analyze long recording scenarios this is the time from the start of the original audio recording for this segment.</param>
        /// <param name="getSpectralIndexes">Invoke this lazy function to get indices for the current segment.</param>
        /// <param name="outputDirectory">The current output directory.</param>
        /// <param name="imageWidth">The expected width of output images.</param>
        /// <returns>A recognizer results object.</returns>
        RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset, Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth);
    }
}