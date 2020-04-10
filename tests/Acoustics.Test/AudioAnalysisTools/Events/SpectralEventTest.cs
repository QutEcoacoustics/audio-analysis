// <copyright file="SpectralEventTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Acoustics.Test.TestHelpers;
    using AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class SpectralEventTest : GeneratedImageTest<Rgb24>
    {
        public void DrawTest()
        {
            var @event = new SpectralEvent()
            {
                EventEndSeconds = 2,
                EventStartSeconds = 1,
                HighFrequencyHertz = 1000,
                LowFrequencyHertz = 100,
            };

            // TODO: finish

        }
    }
}
