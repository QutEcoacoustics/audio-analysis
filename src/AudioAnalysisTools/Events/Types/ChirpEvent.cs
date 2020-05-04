// <copyright file="ChirpEvent.cs" company="QutEcoacoustics">
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
    using SixLabors.ImageSharp.Processing;

    public class ChirpEvent : SpectralEvent, ITracks<Track>
    {
        private readonly double maxScore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChirpEvent"/> class.
        /// </summary>
        /// <remarks>
        /// MaxScore establishes a scale for the chirp score. Typically the amplitude of track points is decibels.
        /// A satisfactory maxScore is 12.0 decibels, since this is a high SNR in enviornmental recordings.
        /// The normalised score is a linear conversion from 0 - maxScore to [0, 1].
        /// </remarks>
        /// <param name="chirp">A chirp track consisting of a sequence of spectral points.</param>
        /// <param name="maxScore">A maximum score used to normalise the track score.</param>
        public ChirpEvent(Track chirp, double maxScore)
        {
            this.Tracks.Add(chirp);
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
        /// Gets or sets the score for the frequency profile of the contained track.
        /// This score is used as a measure of how close the shape of a track matches a desired shape.
        /// </summary>
        public double FrequencyProfileScore { get; set; }

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
            this.Tracks.First().Draw(graphics, options);

            //  base drawing (border)
            // TODO: unless border is disabled
            base.Draw(graphics, options);
        }
    }
}