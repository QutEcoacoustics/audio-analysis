// <copyright file="ClickEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.Processing;

    public class ClickEvent : SpectralEvent
    {


        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // TODO: render click event


            // don't call base draw method because don't want the border
        }
    }
}