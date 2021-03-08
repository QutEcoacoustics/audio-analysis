// <copyright file="CompositeEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using SixLabors.ImageSharp.Processing;

    public class CompositeEvent : SpectralEvent
    {
        public CompositeEvent(List<SpectralEvent> events)
        {
            this.ComponentEvents.AddRange(events);
            this.ScoreRange = (0, 1);
        }

        public List<EventCommon> ComponentEvents { get; set; } = new List<EventCommon>();

        public int ComponentCount => this.ComponentEvents.Count;

        public override double EventStartSeconds =>
            this.ComponentEvents.Min(x => x.EventStartSeconds);

        public override double EventEndSeconds =>
            this.ComponentEvents.Max(x => (x as ITemporalEvent)?.EventEndSeconds) ?? double.PositiveInfinity;

        public override double LowFrequencyHertz =>
            this.ComponentEvents.Min(x => (x as ISpectralEvent)?.LowFrequencyHertz) ?? 0;

        public override double HighFrequencyHertz =>
            this.ComponentEvents.Max(x => (x as ISpectralEvent)?.HighFrequencyHertz) ?? double.PositiveInfinity;

        public override double Score =>
            this.ComponentEvents.Max(x => (x as SpectralEvent)?.Score) ?? double.PositiveInfinity;

        public double CalculatePeriodicity()
        {
            var eventStarts = new List<double>();
            foreach (var ev in this.ComponentEvents)
            {
                eventStarts.Add(ev.EventStartSeconds);
            }

            // Sort array in ascending order.
            var array = eventStarts.ToArray();
            Array.Sort(array);
            var periodicity = Math.Abs(array[0] - array[array.Length - 1]) / array.Length;

            return periodicity;
        }

        public IEnumerable<ITrack> Tracks
        {
            get
            {
                foreach (var @event in this.ComponentEvents)
                {
                    if (@event is ITracks<ITrack> eventWithTracks)
                    {
                        foreach (var track in eventWithTracks.Tracks)
                        {
                            yield return track;
                        }
                    }
                }
            }
        }

        public override double ScoreNormalized
        {
            get
            {
                // because we are averaging normalized scores,
                // we can just multiply the values
                return this
                    .ComponentEvents
                    .Aggregate(
                        1.0,
                        (previous, current) => previous * current.ScoreNormalized);
            }
        }

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            foreach (var @event in this.ComponentEvents)
            {
                // disable border
                var newOptions = new EventRenderingOptions(options.Converters)
                {
                    DrawLabel = false,
                    DrawBorder = false,
                    DrawScore = false,
                };

                @event.Draw(graphics, newOptions);
            }

            // draw a border around all of it
            base.Draw(graphics, options);
        }

        //#################################################################################################################
        //FOLLOWING METHODS DEAL WITH THE OVERLAP OF EVENTS

        /// <summary>
        /// Determines if two events overlap in frequency.
        /// </summary>
        /// <param name="event1">event one.</param>
        /// <param name="event2">event two.</param>
        /// <returns>true if events overlap.</returns>
        public static bool EventsOverlapInFrequency(SpectralEvent event1, SpectralEvent event2)
        {
            //check if event 1 freq band overlaps event 2 freq band
            if (event1.HighFrequencyHertz >= event2.LowFrequencyHertz && event1.HighFrequencyHertz <= event2.HighFrequencyHertz)
            {
                return true;
            }

            // check if event 1 freq band overlaps event 2 freq band
            if (event1.LowFrequencyHertz >= event2.LowFrequencyHertz && event1.LowFrequencyHertz <= event2.HighFrequencyHertz)
            {
                return true;
            }

            //check if event 2 freq band overlaps event 1 freq band
            if (event2.HighFrequencyHertz >= event1.LowFrequencyHertz && event2.HighFrequencyHertz <= event1.HighFrequencyHertz)
            {
                return true;
            }

            // check if event 2 freq band overlaps event 1 freq band
            if (event2.LowFrequencyHertz >= event1.LowFrequencyHertz && event2.LowFrequencyHertz <= event1.HighFrequencyHertz)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if two events overlap in time.
        /// </summary>
        /// <param name="event1">event one.</param>
        /// <param name="event2">event two.</param>
        /// <returns>true if events overlap.</returns>
        public static bool EventsOverlapInTime(SpectralEvent event1, SpectralEvent event2)
        {
            //check if event 1 starts within event 2
            if (event1.EventStartSeconds >= event2.EventStartSeconds && event1.EventStartSeconds <= event2.EventEndSeconds)
            {
                return true;
            }

            // check if event 1 ends within event 2
            if (event1.EventEndSeconds >= event2.EventStartSeconds && event1.EventEndSeconds <= event2.EventEndSeconds)
            {
                return true;
            }

            // now check possibility that event2 is inside event1.
            //check if event 2 starts within event 1
            if (event2.EventStartSeconds >= event1.EventStartSeconds && event2.EventStartSeconds <= event1.EventEndSeconds)
            {
                return true;
            }

            // check if event 2 ends within event 1
            if (event2.EventEndSeconds >= event1.EventStartSeconds && event2.EventEndSeconds <= event1.EventEndSeconds)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Combines overlapping events in the passed List of events and returns a reduced list.
        /// </summary>
        public static List<EventCommon> CombineOverlappingEvents(List<EventCommon> events)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var a = events[i] as SpectralEvent;
                    var b = events[j] as SpectralEvent;

                    if (EventsOverlapInTime(a, b) && EventsOverlapInFrequency(a, b))
                    {
                        var compositeEvent = CombineTwoEvents(a, b);
                        events[j] = compositeEvent;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }

        /// <summary>
        /// Combines events that have similar bottom and top frequency bounds and whose start times are within the passed time range.
        /// NOTE: Proximal means (1) that the event starts are close to one another and (2) the events occupy a SIMILAR frequency band.
        /// NOTE: SIMILAR frequency band means the difference between two top Hertz values and the two low Hertz values are less than hertzDifference.
        /// NOTE: This method is used to combine events that are likely to be a syllable sequence within the same call.
        /// NOTE: Allow twice the tolerance for the upper Hertz difference because upper bounds tend to be more flexible. This is may need to be reversed if it proves to be unhelpful.
        /// </summary>
        public static List<EventCommon> CombineProximalEvents(List<SpectralEvent> events, TimeSpan startDifference, int hertzDifference)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    bool eventStartsAreProximal = Math.Abs(events[i].EventStartSeconds - events[j].EventStartSeconds) <= startDifference.TotalSeconds;
                    bool eventMinimaAreSimilar = Math.Abs(events[i].LowFrequencyHertz - events[j].LowFrequencyHertz) <= hertzDifference;
                    bool eventMaximaAreSimilar = Math.Abs(events[i].HighFrequencyHertz - events[j].HighFrequencyHertz) <= (hertzDifference * 2);
                    if (eventStartsAreProximal && eventMinimaAreSimilar && eventMaximaAreSimilar)
                    {
                        var compositeEvent = CombineTwoEvents(events[i], events[j]);
                        events[j] = compositeEvent;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }

        /// <summary>
        /// Combines events that are possible stacked harmonics or formants.
        /// Two conditions apply:
        /// (1) the events are coincident (have similar start and end times)
        /// (2) the events are stacked (their minima and maxima are within the passed frequency gap).
        /// NOTE: The difference between this method and CombineProximalEvents() is that stacked events should have similar start AND similar end times.
        ///       Having similar start and end times means the events are "stacked" (like pancakes!) in the spectrogram.
        ///       How closely stacked is determined by the hertzDifference argument. Typicaly, the formant spacing is not large, ~100-200Hz.
        /// </summary>
        public static List<EventCommon> CombineStackedEvents(List<SpectralEvent> events, TimeSpan timeDifference, int hertzDifference)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    bool eventsStartTogether = Math.Abs(events[i].EventStartSeconds - events[j].EventStartSeconds) < timeDifference.TotalSeconds;
                    bool eventsEndTogether = Math.Abs(events[i].EventEndSeconds - events[j].EventEndSeconds) < timeDifference.TotalSeconds;
                    bool eventsAreCoincident = eventsStartTogether && eventsEndTogether;
                    bool eventMinimaAreSimilar = Math.Abs(events[i].LowFrequencyHertz - events[j].LowFrequencyHertz) < hertzDifference;
                    bool eventMaximaAreSimilar = Math.Abs(events[i].HighFrequencyHertz - events[j].HighFrequencyHertz) < hertzDifference;
                    if (eventsAreCoincident && eventMinimaAreSimilar && eventMaximaAreSimilar)
                    {
                        var compositeEvent = CombineTwoEvents(events[i], events[j]);
                        events[j] = compositeEvent;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }

        /// <summary>
        /// Combines events that are possible stacked harmonics or formants.
        /// Two conditions apply:
        /// (1) the events are coincident (have similar start and end times)
        /// (2) the events are stacked (their minima and maxima are within the passed frequency gap).
        /// NOTE: The difference between this method and CombineProximalEvents() is that stacked events should have similar start AND similar end times.
        ///       Having similar start and end times means the events are "stacked" (like pancakes!) in the spectrogram.
        ///       How closely stacked is determined by the hertzDifference argument. Typicaly, the formant spacing is not large, ~100-200Hz.
        /// </summary>
        public static List<EventCommon> CombineVerticalEvents(List<SpectralEvent> events, TimeSpan timeDifference, int hertzDifference)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    bool eventsStartTogether = Math.Abs(events[i].EventStartSeconds - events[j].EventStartSeconds) < timeDifference.TotalSeconds;
                    bool eventsEndTogether = Math.Abs(events[i].EventEndSeconds - events[j].EventEndSeconds) < timeDifference.TotalSeconds;
                    bool eventsAreCoincident = eventsStartTogether && eventsEndTogether;
                    bool eventMinimaAreSimilar = Math.Abs(events[i].HighFrequencyHertz - events[j].LowFrequencyHertz) < hertzDifference;
                    bool eventMaximaAreSimilar = Math.Abs(events[j].HighFrequencyHertz - events[i].LowFrequencyHertz) < hertzDifference;
                    if (eventsAreCoincident && (eventMinimaAreSimilar || eventMaximaAreSimilar))
                    {
                        var compositeEvent = CombineTwoEvents(events[i], events[j]);
                        events[j] = compositeEvent;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }


        /// <summary>
        /// Merges two spectral events into one event.
        /// </summary>
        /// <param name="e1">first event.</param>
        /// <param name="e2">second event.</param>
        /// <returns>a composite event.</returns>
        public static CompositeEvent CombineTwoEvents(SpectralEvent e1, SpectralEvent e2)
        {
            // Assume that we only merge events that are in the same recording segment.
            // Therefore the value of segmentStartOffset has already been set and is the same for both events.
            var e1Type = e1.GetType();
            var e2Type = e2.GetType();

            // There are three possibilities
            if (e1Type == typeof(CompositeEvent) && e2Type == typeof(CompositeEvent))
            {
                var e2Events = ((CompositeEvent)e2).ComponentEvents;
                ((CompositeEvent)e1).ComponentEvents.AddRange(e2Events);
                return (CompositeEvent)e1;
            }
            else
            if (e1Type == typeof(CompositeEvent))
            {
                ((CompositeEvent)e1).ComponentEvents.Add(e2);
                return (CompositeEvent)e1;
            }
            else
            if (e2Type == typeof(CompositeEvent))
            {
                ((CompositeEvent)e2).ComponentEvents.Add(e1);
                return (CompositeEvent)e2;
            }

            var twoEvents = new List<SpectralEvent>
            {
                e1,
                e2,
            };

            var compositeEvent = new CompositeEvent(twoEvents)
            {
                Name = e1.Name,
            };

            return compositeEvent;
        }

        public static SpectralEvent OverlapsEventInList(SpectralEvent anEvent, List<SpectralEvent> events)
        {
            foreach (var se in events)
            {
                if (EventsOverlapInTime(anEvent, se))
                {
                    return se;
                }
            }

            return null;
        }

        /// <summary>
        /// Removes from a list of events, those events that are enclosed by another event in the list.
        /// Returns a reduced list.
        /// </summary>
        public static List<EventCommon> RemoveEnclosedEvents(List<EventCommon> events)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var a = events[i] as SpectralEvent;
                    var b = events[j] as SpectralEvent;

                    (SpectralEvent evAi, SpectralEvent evBj) = EnclosedEvents(a, b);

                    if (evAi != null && evBj == null)
                    {
                        events[j] = evAi;
                        events.RemoveAt(i);
                        break;
                    }
                    else
                    if (evAi == null && evBj != null)
                    {
                        events[j] = evBj;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }

        public static (SpectralEvent EvAi, SpectralEvent EvBj) EnclosedEvents(SpectralEvent a, SpectralEvent b)
        {
            bool eventAEnclosedInTime = a.EventStartSeconds >= b.EventStartSeconds && a.EventEndSeconds <= b.EventEndSeconds;
            bool eventAEnclosedInFreq = a.LowFrequencyHertz >= b.LowFrequencyHertz && a.HighFrequencyHertz <= b.HighFrequencyHertz;
            if (eventAEnclosedInTime && eventAEnclosedInFreq)
            {
                return (null, b);
            }

            bool eventBEnclosedInTime = a.EventStartSeconds <= b.EventStartSeconds && a.EventEndSeconds >= b.EventEndSeconds;
            bool eventBEnclosedInFreq = a.LowFrequencyHertz <= b.LowFrequencyHertz && a.HighFrequencyHertz >= b.HighFrequencyHertz;
            if (eventBEnclosedInTime && eventBEnclosedInFreq)
            {
                return (a, null);
            }

            return (a, b);
        }

        /*
        /// <summary>
        /// This method not currently called but is POTENTIALLY USEFUL.
        /// Returns the fractional overlap of two events.
        /// Translate time/freq dimensions to coordinates in a matrix.
        /// Freq dimension = bins   = matrix columns. Origin is top left - as per matrix in the sonogram class.
        /// Time dimension = frames = matrix rows.
        /// </summary>
        public static double EventFractionalOverlap(AcousticEvent event1, AcousticEvent event2)
        {
            int timeOverlap = Oblong.RowOverlap(event1.Oblong, event2.Oblong);
            if (timeOverlap == 0)
            {
                return 0.0;
            }

            int hzOverlap = Oblong.ColumnOverlap(event1.Oblong, event2.Oblong);
            if (hzOverlap == 0)
            {
                return 0.0;
            }

            int overlapArea = timeOverlap * hzOverlap;
            double fractionalOverlap1 = overlapArea / (double)event1.Oblong.Area();
            double fractionalOverlap2 = overlapArea / (double)event2.Oblong.Area();

            if (fractionalOverlap1 > fractionalOverlap2)
            {
                return fractionalOverlap1;
            }
            else
            {
                return fractionalOverlap2;
            }
        }
        */

        //#################################################################################################################
    }
}
