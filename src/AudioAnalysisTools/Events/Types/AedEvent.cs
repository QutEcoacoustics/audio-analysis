// <copyright file="AedEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System.Collections.Generic;

    public class AedEvent : SpectralEvent, IPointData
    {

        public ISet<ISpectralPoint> Points => throw new System.NotImplementedException();
    }
}