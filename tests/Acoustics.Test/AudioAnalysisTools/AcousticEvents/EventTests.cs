// <copyright file="EventTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.EventStatistics
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.EventStatistics;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class EventTests
    {
        [TestMethod]
        public void TestEventMerging()
        {
            // make a list of events
            var events = new List<AcousticEvent>();

            var segmentStartOffset = TimeSpan.Zero;
            double maxPossibleScore = 10.0;
            var startTime1 = 1.0;
            var duration1 = 5.0;
            var minHz1 = 1000;
            var maxHz1 = 8000;
            var event1 = new AcousticEvent(segmentStartOffset, startTime1, duration1, minHz1, maxHz1)
            {
                Name = "Event1",
                Score = 1.0,
                ScoreNormalised = 1.0 / maxPossibleScore,
            };

            events.Add(event1);

            var startTime2 = 4.5;
            var duration2 = 2.0;
            var minHz2 = 1500;
            var maxHz2 = 6000;
            var event2 = new AcousticEvent(segmentStartOffset, startTime2, duration2, minHz2, maxHz2)
            {
                Name = "Event2",
                Score = 5.0,
                ScoreNormalised = 5.0 / maxPossibleScore,
            };
            events.Add(event2);

            var startTime3 = 7.0;
            var duration3 = 2.0;
            var minHz3 = 1000;
            var maxHz3 = 8000;
            var event3 = new AcousticEvent(segmentStartOffset, startTime3, duration3, minHz3, maxHz3)
            {
                Name = "Event3",
                Score = 9.0,
                ScoreNormalised = 9.0 / maxPossibleScore,
            };
            events.Add(event3);

            // combine adjacent acoustic events
            events = AcousticEvent.CombineOverlappingEvents(events, segmentStartOffset);

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(1.0, events[0].EventStartSeconds, 1E-4);
            Assert.AreEqual(6.5, events[0].EventEndSeconds, 1E-4);
            Assert.AreEqual(7.0, events[1].EventStartSeconds, 1E-4);
            Assert.AreEqual(9.0, events[1].EventEndSeconds, 1E-4);

            Assert.AreEqual(1000, events[0].LowFrequencyHertz);
            Assert.AreEqual(8000, events[0].HighFrequencyHertz);
            Assert.AreEqual(1000, events[1].LowFrequencyHertz);
            Assert.AreEqual(8000, events[1].HighFrequencyHertz);

            Assert.AreEqual(5.0, events[0].Score, 1E-4);
            Assert.AreEqual(9.0, events[1].Score, 1E-4);
            Assert.AreEqual(0.5, events[0].ScoreNormalised, 1E-4);
            Assert.AreEqual(0.9, events[1].ScoreNormalised, 1E-4);
        }

        [TestMethod]
        public void TestSonogramWithEventsOverlay()
        {
            // make a substitute sonogram image
            int width = 100;
            int height = 256;
            var substituteSonogram = new Image<Rgb24>(width, height);

            //substituteSonogram.Mutate(x => x.Pad(width, height, Color.Gray));
            // image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));

            // make a list of events
            var framesPerSecond = 10.0;
            var freqBinWidth = 43.0664;
            var segmentStartOffset = TimeSpan.Zero;
            var minHz = 1000;
            var maxHz = 8000;
            double maxPossibleScore = 10.0;

            var events = new List<AcousticEvent>();
            var startTime1 = 1.0;
            var duration1 = 5.0;
            var event1 = new AcousticEvent(segmentStartOffset, startTime1, duration1, minHz, maxHz)
            {
                Score = 10.0,
                Name = "Event1",
                ScoreNormalised = 10.0 / maxPossibleScore,
            };

            events.Add(event1);
            var startTime2 = 7.0;
            var duration2 = 2.0;
            var event2 = new AcousticEvent(segmentStartOffset, startTime2, duration2, minHz, maxHz)
            {
                Score = 1.0,
                Name = "Event2",
                ScoreNormalised = 1.0 / maxPossibleScore,
            };
            events.Add(event2);

            // now add events into the spectrogram image.
            // set colour for the events
            foreach (AcousticEvent ev in events)
            {
                ev.BorderColour = AcousticEvent.DefaultBorderColor;
                ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                ev.DrawEvent(substituteSonogram, framesPerSecond, freqBinWidth, height);
            }

            //substituteSonogram.Save("C:\\temp\\image.png");
            var redPixel1 = new Argb32(110, 10, 30);
            var expectedRed1 = new Color(redPixel1);
            var redPixel2 = new Argb32(124, 11, 34);
            var expectedRed2 = new Color(redPixel2);
            var greenPixel = new Argb32(55, 133, 15);
            var expectedGreen = new Color(greenPixel);

            //var actualColor = substituteSonogram[0, height - 1];
            Assert.AreEqual<Color>(expectedRed1, substituteSonogram[61, 119]);
            Assert.AreEqual<Color>(expectedRed1, substituteSonogram[70, 122]);
            Assert.AreEqual<Color>(expectedRed1, substituteSonogram[91, 181]);
            Assert.AreEqual<Color>(expectedRed2, substituteSonogram[36, 233]);
            Assert.AreEqual<Color>(expectedRed1, substituteSonogram[56, 69]);

            //actualColor = substituteSonogram[9, 72];
            Assert.AreEqual<Color>(expectedGreen, substituteSonogram[9, 72]);
            Assert.AreEqual<Color>(expectedGreen, substituteSonogram[69, 217]);
        }
    }
}
