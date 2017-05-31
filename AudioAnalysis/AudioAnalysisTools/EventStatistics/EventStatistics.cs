// <copyright file="EventStatistics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.EventStatistics
{
    using System;
    using AnalysisBase.ResultBases;
    using DSP;
    using TowseyLibrary;
    using WavTools;

    /// <summary>
    /// Note that EventBase already has getters/setters for:
    /// TimeSpan SegmentStartOffset
    /// double Score
    /// double EventStartSeconds
    /// double? MinHz
    /// ..
    /// NOTE: When MinHz equals null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
    ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
    /// </summary>
    public class EventStatistics : EventBase
    {
        public TimeSpan EventStart { get; set; }

        public TimeSpan EventEnd { get; set; }

        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the bottom frequency bound of the acoustic event in Herz
        /// </summary>
        public int FreqLow { get; set; }

        /// <summary>
        /// Gets or sets the Top frequency bound of the acoustic event in Herz
        /// </summary>
        public int FreqTop { get; set; }

        public int BandWidth { get; set; }

        public int DominantFrequency { get; set; }

        public double AverageAmplitude { get; set; }
    }
}
