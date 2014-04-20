using System;

namespace AnalysisBase.StrongAnalyser.ResultBases
{
    public abstract class ResultBase
    {
        public string FileName { get; set; }

        public int SegmentCount { get; set; }

        public TimeSpan SegmentOffsetFromStartOfSource { get; set; }

        public TimeSpan SegmentDuration { get; set; } //SEGMENT_TIMESPAN = "SegTimeSpan";

        public int MinuteOffset { get; set; } //START_MIN = "start-min" = AudioAnalysisTools.Keys.EVENT_START_MIN

    }
}