// <copyright file="ITrack.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Interfaces
{
    using AudioAnalysisTools.Events.Drawing;

    public interface ITrack : IPointData, IDrawableEvent
    {
    }
}