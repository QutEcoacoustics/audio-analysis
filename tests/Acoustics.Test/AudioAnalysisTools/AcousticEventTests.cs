// <copyright file="AcousticEventTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Types;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class AcousticEventTests : GeneratedImageTest<Rgb24>
    {
        [TestMethod]
        public void TestSonogramWithEventsOverlay()
        {
            // make a substitute sonogram image
            var substituteSonogram = Drawing.NewImage(100, 256, Color.Black);

            // make a list of events
            var framesPerSecond = 10.0;
            var freqBinWidth = 43.0664;
            double maxPossibleScore = 10.0;

            var events = new List<AcousticEvent>();
            var event1 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 1.0, eventDuration: 5.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 10.0,
                Name = "Event1",
                ScoreNormalised = 10.0 / maxPossibleScore,
            };

            events.Add(event1);
            var event2 = new AcousticEvent(segmentStartOffset: TimeSpan.Zero, eventStartSegmentRelative: 7.0, eventDuration: 2.0, minFreq: 1000, maxFreq: 8000)
            {
                Score = 1.0,
                Name = "Event2",
                ScoreNormalised = 1.0 / maxPossibleScore,
            };
            events.Add(event2);

            // now add events into the spectrogram image.
            // set color for the events
            foreach (AcousticEvent ev in events)
            {
                // because we are testing placement of box not text.
                ev.Name = string.Empty;
                ev.BorderColour = AcousticEvent.DefaultBorderColor;
                ev.ScoreColour = AcousticEvent.DefaultScoreColor;
                ev.DrawEvent(substituteSonogram, framesPerSecond, freqBinWidth, 256);
            }

            this.ActualImage = substituteSonogram;

            // BUG: this asset is faulty. See https://github.com/QutEcoacoustics/audio-analysis/issues/300#issuecomment-601537263
            this.ExpectedImage = Image.Load<Rgb24>(
                PathHelper.ResolveAssetPath("AcousticEventTests_SuperimposeEventsOnImage.png"));

            this.AssertImagesEqual();
        }
    }
}