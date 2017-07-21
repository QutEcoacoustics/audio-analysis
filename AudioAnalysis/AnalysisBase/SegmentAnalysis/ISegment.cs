// <copyright file="ISegment.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisBase.SegmentAnalysis
{
    public interface ISegment<out TSource>
    {
        TSource Source { get; }

        double StartOffsetSeconds { get; }

        double EndOffsetSeconds { get; }
    }
}
