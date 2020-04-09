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

    public class CompositeEvent : SpectralEvent
    {
        public List<EventBase> ComponentEvents { get; set; } = new List<EventBase>();

        public override double EventStartSeconds =>
            this.ComponentEvents.Min(x => x.EventStartSeconds);

        // TODO rest

        //public override void Draw()
        //{
        //    foreach(var @event in this.ComponentEvents)
        //    {
        //        @event.Draw();
        //    }

        //    // draw a border around all of it
        //    //DrawRectangle(this.EventStartSeconds, this.)
        //}
    }
}