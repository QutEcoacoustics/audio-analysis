using System;

namespace AnalysisBase.StrongAnalyser.ResultBases
{
    public abstract class ResultBase
    {
        public string FileName { get; set; }

        // found no use for this (or usage) disabled
        //public int SegmentCount { get; set; }

        public TimeSpan SegmentStartOffset { get; set; }

        public TimeSpan SegmentDuration { get; set; } //SEGMENT_TIMESPAN = "SegTimeSpan";


        public int StartOffsetMinute { get; set; } //START_MIN = "start-min" = AudioAnalysisTools.Keys.EVENT_START_MIN

        public virtual int CompareTo(ResultBase other)
        {
            return this.SegmentStartOffset.CompareTo(other.SegmentStartOffset);
        }
    }
}