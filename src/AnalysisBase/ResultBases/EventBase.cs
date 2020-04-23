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
    /// The base class for all Event style results.
    /// </summary>
    public abstract class EventBase : ResultBase
    {
        private double eventStartSeconds;

        /// <summary>
        /// Gets or sets the time (in seconds) from start of the file/recording to start of the current audio segment.
        /// </summary>
        /// <remarks>
        /// <see cref="EventStartSeconds"/> will always be greater than or equal to <see cref="SegmentStartSeconds"/>.
        /// </remarks>
        public virtual double SegmentStartSeconds { get;  set; }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public virtual double Score { get; set; }

        /// <summary>
        /// Gets or sets the Event's Start Seconds.
        /// IMPORTANT: This field is time offset relative to the recording.
        /// It automatically updates <see cref="ResultBase.ResultStartSeconds"/> when set.
        /// </summary>
        /// <remarks>
        /// 2017-09: This field USED to be offset relative to the current segment.
        /// 2017-09: This field is NOW equivalent to <see cref="ResultBase.ResultStartSeconds"/>.
        /// </remarks>
        public virtual double EventStartSeconds
        {
            get
            {
                return this.eventStartSeconds;
            }

            set
            {
                this.eventStartSeconds = value;
                this.ResultStartSeconds = value;
            }
        }

        /// <summary>
        /// Sets both the Segment start and the Event start.
        /// <paramref name="segmentStart"/> is measured relative to the start of the recording.
        /// <paramref name="eventStartSegmentRelative"/> is measured relative to the start of the segment.
        /// This method sets both <see cref="SegmentStartSeconds"/> and <see cref="EventStartSeconds"/> which
        /// are both measured relative to the start of the recording.
        /// </summary>
        protected void SetEventStartRelative(TimeSpan segmentStart, double eventStartSegmentRelative)
        {
            this.SegmentStartSeconds = segmentStart.TotalSeconds;
            this.EventStartSeconds = this.SegmentStartSeconds + eventStartSegmentRelative;
        }
    }
}