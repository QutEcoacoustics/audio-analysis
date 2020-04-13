// <copyright file="BlobEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System.Collections.Generic;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Am acoustic event that also includes data about the content identified by the event.
    /// </summary>
    public class BlobEvent : SpectralEvent, IPointData
    {
        public BlobEvent()
        {

        }

        public ISet<ISpectralPoint> Points { get; } = new HashSet<ISpectralPoint>();

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            ((IPointData)this).DrawPointsAsFill(graphics, options);

            //  base drawing (border)
            base.Draw(graphics, options);
        }
    }
}