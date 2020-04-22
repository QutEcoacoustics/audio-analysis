// <copyright file="EventConverters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class EventConverters
    {
        public static SpectralEvent ConvertAcousticEventToSpectralEvent(AcousticEvent ae)
        {
            var segmentStartOffset = TimeSpan.FromSeconds(ae.SegmentStartSeconds);
            double startTime = ae.EventStartSeconds;
            double duration = ae.EventDurationSeconds;
            double minHz = ae.HighFrequencyHertz;
            double maxHz = ae.HighFrequencyHertz;
            var be = new SpectralEvent(segmentStartOffset, startTime, duration, minHz, maxHz);
            return be;
        }

        public static AcousticEvent ConvertSpectralEventToAcousticEvent(SpectralEvent se)
        {
            var segmentStartOffset = TimeSpan.FromSeconds(se.SegmentStartSeconds);
            double startTime = se.EventStartSeconds;
            double duration = se.EventDurationSeconds;
            double minHz = se.HighFrequencyHertz;
            double maxHz = se.HighFrequencyHertz;
            var ae = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz);
            return ae;
        }

        public static List<SpectralEvent> ConvertAcousticEventsToSpectralEvents(List<AcousticEvent> aEvents)
        {
            var sEvents = new List<SpectralEvent>();
            foreach (var ae in aEvents)
            {
                sEvents.Add(EventConverters.ConvertAcousticEventToSpectralEvent(ae));
            }

            return sEvents;
        }
    }
}
