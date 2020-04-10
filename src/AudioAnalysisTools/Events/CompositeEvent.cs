// <copyright file="CompositeEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using SixLabors.ImageSharp.Processing;

    public class CompositeEvent : SpectralEvent
    {
        public List<EventCommon> ComponentEvents { get; set; } = new List<EventCommon>();

        public override double EventStartSeconds =>
            this.ComponentEvents.Min(x => x.EventStartSeconds);

        public override double EventEndSeconds =>
            this.ComponentEvents.Max(x => (x as ITemporalEvent)?.EventEndSeconds) ?? double.PositiveInfinity;

        public override double LowFrequencyHertz =>
            this.ComponentEvents.Min(x => (x as ISpectralEvent)?.LowFrequencyHertz) ?? 0;

        public override double HighFrequencyHertz =>
            this.ComponentEvents.Max(x => (x as ISpectralEvent)?.HighFrequencyHertz) ?? double.PositiveInfinity;

        public override void Draw<T>(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            foreach (var @event in this.ComponentEvents)
            {
                @event.Draw<T>(graphics, options);
            }

            // draw a border around all of it
            base.Draw<T>(graphics, options);
        }
    }
}