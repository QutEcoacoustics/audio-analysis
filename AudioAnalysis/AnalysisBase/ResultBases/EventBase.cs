// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventBase.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
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
        /// Gets or sets the time the current audio segment is offset from the start of the file/recording.
        ///
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
        /// IMPORTANT: This field is time offset relative to the current segment.
        ///            The  time offset relative to the start of the original file can be found in <c>ResultBase.StartOffset</c>.
        /// Setting this field will update <c>ResultBase.StartOffset</c>.
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

        /// <summary>
        /// Gets or sets the bottom frequency bound of the acoustic event.
        /// NOTE: When MinHz is set to null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
        ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
        ///       Implementers may implement their own MaxHz if needed.
        /// </summary>
        public double? LowFrequencyHertz { get; protected set; }

        //// THIS IS REMOVED because the IComparer on ResultBase should achieve a similar effect,
        //// provided both EventStartSeconds and SegmentStartOffset are set.
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
