// <copyright file="HarmonicEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.Processing;

    public class HarmonicEvent : SpectralEvent
    {
        /// <summary>
        /// Gets or sets the interval or spacing between harmonics/formants.
        /// </summary>
        /// <value>
        /// The ?average/calculated? gap between formant peaks, in hertz.
        /// </value?
        public double HarmonicInterval { get; set; }

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // foreach (var track in tracks) {
            // track.Draw(...)
            // }

            //this.Track.Draw(graphics, options);

            //  base drawing (border)
            // TODO: unless border is disabled
            base.Draw(graphics, options);
        }
    }
}
