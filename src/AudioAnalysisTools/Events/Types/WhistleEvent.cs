// <copyright file="WhistleEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Events.Types;
    using SixLabors.ImageSharp.Processing;

    public class WhistleEvent : SpectralEvent, ITracks<Track>
    {
        public WhistleEvent(Track whistle, double maxScore)
        {
            this.Tracks.Add(whistle);
            this.ScoreRange = new Interval<double>(0, maxScore);
        }

        public List<Track> Tracks { get; private set; } = new List<Track>(1);

        public override double EventStartSeconds =>
            this.Tracks.Min(x => x.StartTimeSeconds);

        public override double EventEndSeconds =>
            this.Tracks.Max(x => x.EndTimeSeconds);

        public override double LowFrequencyHertz =>
            this.Tracks.Min(x => x.LowFreqHertz);

        public override double HighFrequencyHertz =>
            this.Tracks.Max(x => x.HighFreqHertz);

        /// <summary>
        /// Gets the average track amplitude.
        /// </summary>
        /// <remarks>
        /// Thevent score is an average value of the track score.
        /// </remarks>
        public override double Score
        {
            get
            {
                return this.Tracks.Single().GetAverageTrackAmplitude();
            }
        }

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // foreach (var track in tracks) {
            // track.Draw(...)
            // }

            this.Tracks.First().Draw(graphics, options);

            //  base drawing (border)
            // TODO: unless border is disabled
            base.Draw(graphics, options);
        }

        public static List<EventCommon> CombineAdjacentWhistleEvents(List<WhistleEvent> events, double hertzDifference)
        {
            if (events.Count < 2)
            {
                return events.Cast<EventCommon>().ToList();
            }

            //var whistleEvents = new List<EventCommon>();
            for (int i = events.Count - 1; i >= 0; i--)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    var a = events[i] as WhistleEvent;
                    var b = events[j] as WhistleEvent;

                    bool eventsOverlapInTime = CompositeEvent.EventsOverlapInTime(a, b);
                    bool eventsAreInSimilarFreqBand = Math.Abs(a.LowFrequencyHertz - b.HighFrequencyHertz) < hertzDifference || Math.Abs(a.HighFrequencyHertz - b.LowFrequencyHertz) < hertzDifference;
                    if (eventsOverlapInTime && eventsAreInSimilarFreqBand)
                    {
                        var newEvent = MergeTwoWhistleEvents(a, b);
                        events[j] = newEvent;
                        events.RemoveAt(i);
                        break;
                    }
                }
            }

            return events.Cast<EventCommon>().ToList();
        }

        /// <summary>
        /// Merges two whistle events into one event.
        /// </summary>
        /// <param name="e1">first event.</param>
        /// <param name="e2">second event.</param>
        /// <returns>a composite event.</returns>
        public static WhistleEvent MergeTwoWhistleEvents(WhistleEvent e1, WhistleEvent e2)
        {
            // Assume that we only merge events that are in the same recording segment.
            // Therefore the value of segmentStartOffset and SegmentDurationSeconds are same for both events.
            var track1 = e1.Tracks.First();
            var track2 = e2.Tracks.First();

            foreach (var point in track2.Points)
            {
                track1.Points.Add(point);
            }

            var newEvent = new WhistleEvent(track1, 1.0)
            {
                Name = e1.Name,
                EventEndSeconds = Math.Max(e1.EventEndSeconds, e2.EventEndSeconds),
                EventStartSeconds = Math.Min(e1.EventStartSeconds, e2.EventStartSeconds),
                HighFrequencyHertz = Math.Max(e1.EventEndSeconds, e2.EventEndSeconds),
                LowFrequencyHertz = Math.Max(e1.EventEndSeconds, e2.EventEndSeconds),
                Score = e1.Score,
                ScoreRange = e1.ScoreRange,
                SegmentDurationSeconds = e1.SegmentDurationSeconds,
                SegmentStartSeconds = e1.SegmentStartSeconds,
                FileName = e1.FileName,
            };

            newEvent.ResultStartSeconds = e1.EventStartSeconds;

            return newEvent;
        }
    }
}