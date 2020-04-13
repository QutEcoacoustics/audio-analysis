// <copyright file="UnitConverterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitConverterTests
    {
        [TestMethod]
        public void TestGetPixelRectangle()
        {
            // arrange
            var @event = new SpectralEvent()
            {
                EventEndSeconds = 9,
                EventStartSeconds = 1,
                HighFrequencyHertz = 900,
                LowFrequencyHertz = 100,
            };

            var converters = new UnitConverters(0, 10, 1000, 100, 100);

            // act
            var rect = converters.GetPixelRectangle(@event);

            // assert
            Assert.AreEqual(10, rect.Left);
            Assert.AreEqual(10, rect.Top);
            Assert.AreEqual(80, rect.Width);
            Assert.AreEqual(80, rect.Height);

        }
    }
}
