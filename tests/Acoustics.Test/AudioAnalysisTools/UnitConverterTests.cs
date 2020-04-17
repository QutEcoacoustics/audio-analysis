// <copyright file="UnitConverterTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;

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

        public static IEnumerable<object[]> FrameAndBinData
        {
            get
            {
                var frames = Enumerable.Range(0, 10);
                var bins = Enumerable.Range(0, 10);

                var combinations = from f in frames from b in bins select new object[] { f, b };

                return combinations;
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(FrameAndBinData))]
        public void TestBackwardsAndForwardsConversionSpectrograms(int frame, int bin)
        {
            UnitConverters converter = new UnitConverters(
                60,
                22050,
                512,
                256);

            var endFrame = frame + 1;

            var secondsStart = converter.GetStartTimeInSecondsOfFrame(frame);
            var secondsEnd = converter.GetEndTimeInSecondsOfFrame(endFrame);
            var hertz = converter.GetHertzFromFreqBin(bin);

            var outFrame = converter.FrameFromStartTime(secondsStart);
            var endOutFrame = converter.FrameFromEndTime(secondsEnd);
            var outBin = converter.GetFreqBinFromHertz(hertz);

            Assert.AreEqual(frame, outFrame, $"Frames do not match, expected: {frame}, actual: {outFrame}, seconds was: {secondsStart}");
            Assert.AreEqual(endFrame, endOutFrame, $"Frame ends do not match, expected: {endFrame}, actual: {endOutFrame}, seconds was: {secondsEnd}");
            Assert.AreEqual(bin, outBin, $"Bins do not match, expected: {bin}, actual: {outBin}, hertz was: {hertz}");
        }
    }
}
