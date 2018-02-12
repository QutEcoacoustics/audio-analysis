// <copyright file="EventStatisticsConfiguration.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;

    using AnalysisBase;

    [Serializable]
    public class EventStatisticsConfiguration : AnalyzerConfig
    {
        public int FrameSize { get; set; } = 512;

        public int FrameStep { get; set; } = 512;
    }
}