// <copyright file="PezoporusOccidentalis.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.WavTools;

    /// <summary>
    /// Recognizer for the Night Parrot.
    /// </summary>
    public class PezoporusOccidentalis : RecognizerBase
    {
        public override string Author { get; }

        public override string SpeciesName { get; }

        public override RecognizerResults Recognize(AudioRecording audioRecording, Config configuration, TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes, DirectoryInfo outputDirectory, int? imageWidth)
        {
            throw new NotImplementedException();
        }
    }
}