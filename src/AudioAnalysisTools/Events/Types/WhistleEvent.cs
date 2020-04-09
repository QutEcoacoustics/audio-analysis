// <copyright file="WhistleEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    public class WhistleEvent : SpectralEvent, ITrack
    {

        public WhistleEvent(ITrack spectralTrack)
        {
            this.Track = spectralTrack;
        }

        public ISet<ISpectralPoint> Points => this.Track.Points;

        public ITrack Track { get; private set; }

        public override void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            this.Track.Draw<T>(graphics, options);

            //  base drawing (border)
            base.Draw<T>(graphics, options);
        }
    }
}