// <copyright file="ClickEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using SixLabors.ImageSharp.Processing;

    public class ClickEvent : SpectralEvent, ITracks<Track>
    {
        private readonly double maxScore;

        public ClickEvent(Track click, double maxScore)
        {
            this.Tracks.Add(click);
            this.maxScore = maxScore;
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

        /// <summary>
        /// Gets the normalised value for the event's track score.
        /// NOTE: It is assumed that the minimum value of the score range = zero.
        /// </summary>
        public double ScoreNormalised
        {
            get
            {
                return this.Score / this.maxScore;
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
    }
}