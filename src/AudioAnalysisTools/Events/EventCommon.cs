// <copyright file="EventCommon.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using Acoustics.Shared;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    public abstract class EventCommon : EventBase, IDrawableEvent
    {
        // PUT OTHER EVENT DATA HERE

        /// <summary>
        /// Gets or sets the name for this event.
        /// The name should be a friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the profile used to create this event.
        /// If a profile was not used this value should be null.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Gets or sets the Decibel threshold at which the event was detected.
        /// This is used during post-processing to group events according to the threshold of their detection..
        /// </summary>
        public double DecibelDetectionThreshold { get; set; }

        /// <summary>
        /// Gets the component name for this event.
        /// The component name should indicate what type of event this.
        /// E.g. a click, a whistle, a stacked harmonic, ...
        /// </summary>
        public string ComponentName => this.GetType().Name;

        public override double ResultStartSeconds => this.EventStartSeconds;

        /// <summary>
        /// Gets or sets the event score.
        /// This is a score in absolute units as determined by context.
        /// <see cref="ScoreRange"/> determines the scale.
        /// </summary>
        public override double Score { get; set; }

        /// <summary>
        /// Gets or sets a min-max range of values for the score for this event.
        /// This is used to establish a score scale and thereby normalize the score.
        /// By default the minimum value of range = zero.
        /// </summary>
        public Interval<double> ScoreRange { get; set; }

        /// <summary>
        /// Gets a score in the range [0, 1].
        /// Up to user to determine a suitable range maximum.
        /// </summary>
        public virtual double ScoreNormalized => this.ScoreRange.NormalizeValue(this.Score);

        /// <summary>
        /// Draw this event on an image.
        /// </summary>
        /// <param name="graphics">The image prcessing context to draw an event on.</param>
        /// <param name="options">The options associated with this render request.</param>
        public abstract void Draw(IImageProcessingContext graphics, EventRenderingOptions options);
    }
}
