// <copyright file="EventConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class EventConverters
    {
        public static SpectralEvent ConvertAcousticEventToSpectralEvent(this AcousticEvent ae)
        {
            var segmentOffset = TimeSpan.FromSeconds(ae.SegmentStartSeconds);
            var eventStart = ae.EventStartSeconds;
            var eventEnd = ae.EventEndSeconds;

            return new SpectralEvent(segmentStartOffset: segmentOffset, eventStartRecordingRelative: eventStart, eventEndRecordingRelative: eventEnd, ae.LowFrequencyHertz, ae.HighFrequencyHertz)
            {
                Name = ae.Name,
                //SegmentStartSeconds = ae.SegmentStartSeconds,
                SegmentDurationSeconds = ae.SegmentDurationSeconds,
            };
        }

        public static AcousticEvent ConvertSpectralEventToAcousticEvent(this SpectralEvent se)
        {
            var segmentStartOffset = TimeSpan.FromSeconds(se.SegmentStartSeconds);
            double startTime = se.EventStartSeconds - segmentStartOffset.TotalSeconds;
            double duration = se.EventDurationSeconds;
            double minHz = se.HighFrequencyHertz;
            double maxHz = se.HighFrequencyHertz;
            var ae = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz)
            {
                Name = se.Name,
                SegmentDurationSeconds = se.SegmentDurationSeconds,
            };

            return ae;
        }

        public static List<AcousticEvent> ConvertSpectralEventsToAcousticEvents<T>(this List<T> spectralEvents)
            where T : SpectralEvent
        {
            var result = new List<AcousticEvent>(spectralEvents.Count);
            foreach (var spectralEvent in spectralEvents)
            {
                result.Add(spectralEvent.ConvertSpectralEventToAcousticEvent());
            }

            return result;
        }

        public static List<EventCommon> ConvertAcousticEventsToSpectralEvents<T>(this List<T> acousticEvents)
            where T : AcousticEvent
        {
            var result = new List<EventCommon>(acousticEvents.Count);
            foreach (var acousticEvent in acousticEvents)
            {
                result.Add(acousticEvent.ConvertAcousticEventToSpectralEvent());
            }

            return result;
        }
    }
}
