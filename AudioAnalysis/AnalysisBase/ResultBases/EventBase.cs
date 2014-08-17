// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventBase.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the EventBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisBase.ResultBases
{
    using System;

    /// <summary>
    /// The base class for all Event style results
    /// </summary>
    public abstract class EventBase : ResultBase 
    {
        private double eventStartSeconds;

        private TimeSpan segmentStartOffset;

        /// <summary>
        /// Gets or sets the time the current audio segment is offset from the start of the file.
        /// This MUST BE SET before EventStartSeconds.
        /// </summary>
        public TimeSpan SegmentStartOffset
        {
            get
            {
                return this.segmentStartOffset;
            }

            set
            {
                this.segmentStartOffset = value;
                this.StartOffset = value.Add(TimeSpan.FromSeconds(this.EventStartSeconds));
            }
        }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the Event's Start Seconds.
        /// This field is relative to the segment.
        /// The version of this number that is relative to the start of the original file can be found in <c>StartOffset</c>.
        /// Setting this field will update <c>StartOffset</c>.
        /// </summary>
        public double EventStartSeconds
        {
            get
            {
                return this.eventStartSeconds;
            }

            set
            {
                this.eventStartSeconds = value;
                this.StartOffset = this.SegmentStartOffset.Add(TimeSpan.FromSeconds(value));
            }
        }

        //AudioAnalysisTools.Keys.MIN_HZ
        public double? MinHz { get; set; }

        /// <summary>
        /// AudioAnalysisTools.Keys.EVENT_COUNT,        //1
        /// .
        /// Not intended to be set by Analyzers.
        /// </summary>
        public int EventCount { get; set; }


//        /// <summary>
//        /// events should be sorted based on their EventStartSeconds property
//        /// </summary>
//        /// <param name="other"></param>
//        /// <returns></returns>
//        public override int CompareTo(ResultBase other)
//        {
//            var result = base.CompareTo(other);
//
//            if (result != 0)
//            {
//                return result;
//            }
//
//            return this.EventStartSeconds.CompareTo(((EventBase)other).EventStartSeconds);
//        }
    }
}
