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
            return new SpectralEvent()
            {
                EventStartSeconds = ae.EventStartSeconds,
                EventEndSeconds = ae.EventEndSeconds,
                LowFrequencyHertz = ae.LowFrequencyHertz,
                HighFrequencyHertz = ae.HighFrequencyHertz,
                Name = ae.Name,
            };
        }

        public static AcousticEvent ConvertSpectralEventToAcousticEvent(this SpectralEvent se)
        {
            var segmentStartOffset = TimeSpan.FromSeconds(se.SegmentStartSeconds);
            double startTime = se.EventStartSeconds;
            double duration = se.EventDurationSeconds;
            double minHz = se.HighFrequencyHertz;
            double maxHz = se.HighFrequencyHertz;
            var ae = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz);
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

        public static List<SpectralEvent> ConvertAcousticEventsToSpectralEvents<T>(this List<T> acousticEvents)
            where T : AcousticEvent
        {
            var result = new List<SpectralEvent>(acousticEvents.Count);
            foreach (var acousticEvent in acousticEvents)
            {
                result.Add(acousticEvent.ConvertAcousticEventToSpectralEvent());
            }

            return result;
        }
    }
}
