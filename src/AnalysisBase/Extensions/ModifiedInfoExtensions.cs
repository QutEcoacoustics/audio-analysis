// <copyright file="ModifiedInfoExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.Extensions
{
    using Acoustics.Tools;

    public static class ModifiedInfoExtensions
    {
        public static FileSegment ToSegment(this AudioUtilityInfo info, FileSegment.FileDateBehavior dateBehavior = FileSegment.FileDateBehavior.Try)
        {
            var source = new FileSegment(
                info.SourceFile,
                info.SampleRate.Value,
                info.Duration.Value,
                dateBehavior);

            return source;
        }
    }
}