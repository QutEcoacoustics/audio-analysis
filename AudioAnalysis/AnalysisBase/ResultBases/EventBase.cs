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

        private double segmentStartSeconds;

        /// <summary>
        /// Gets or sets the time the current audio segment is offset from the start of the file/recording.
        /// </summary>
        /// <remarks>
        /// <see cref="EventStartSeconds"/> will always be greater than or equal to <see cref="SegmentStartSeconds"/>.
        /// </remarks>
        public double SegmentStartSeconds
        {
            get
            {
                return this.segmentStartSeconds;
            }

            set
            {
                this.segmentStartSeconds = value;
            }
        }

        //AudioAnalysisTools.Keys.EVENT_SCORE,
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the Event's Start Seconds.
        /// IMPORTANT: This field is time offset relative to the recording.
        /// It automatically updates <see cref="ResultBase.ResultStartSeconds"/> when set.
        /// </summary>
        /// <remarks>
        /// 2017-09: This field USED to be offset relative to the current segment.
        /// 2017-09: This field is NOW equivalent to <see cref="ResultBase.ResultStartSeconds"/>
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
                base.ResultStartSeconds = value;
            }
        }

        /// <summary>
        /// Gets or sets the bottom frequency bound of the acoustic event.
        /// NOTE: When MinHz is set to null, this indicates that the event is broad band or has undefined frequency. The event is an instant.
        ///       When MinHz has a value, this indicates the event is a point in time/frequency space.
        ///       Implementers may implement their own MaxHz if needed.
        /// </summary>
        public virtual double? LowFrequencyHertz { get; protected set; }

        protected void SetEventStartRelative(TimeSpan segmentStart, double eventStartSegmentRelative)
        {
            this.SegmentStartSeconds = segmentStart.TotalSeconds;
            this.EventStartSeconds = this.SegmentStartSeconds + eventStartSegmentRelative;
        }
    }
}
