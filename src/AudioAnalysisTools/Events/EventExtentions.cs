// <copyright file="EventExtentions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class EventExtentions
    {
        //NOTES on SYNTAX:
        //Select is a transform - fails if it encounters anything that is not of type SpectralEvent.
        //var spectralEvents = events.Select(x => (SpectralEvent)x).ToList();

        //Where is a FILTER - only returns spectral events.
        //var spectralEvents = events.Where(x => x is SpectralEvent).Cast<SpectralEvent>().ToList();
        //var spectralEvents = events.Where(x => x is ChirpEvent).ToList();
        //var chirpEvents = events.Cast<ChirpEvent>().ToList();

        public static (List<T> TargetEvents, List<U> OtherEvents) FilterForEventType<T, U>(this List<U> events)
            where U : EventCommon
            where T : EventCommon
        {
            var target = new List<T>(events.Count);
            var other = new List<U>(events.Count);

            foreach (var @event in events)
            {
                if (@event is T t)
                {
                    target.Add(t);
                }
                else
                {
                    other.Add(@event);
                }
            }

            return (target, other);
        }
    }
}
