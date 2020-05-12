// <copyright file="TrackTestsDrawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events.Tracks
{
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events.Drawing;
    using global::AudioAnalysisTools.Events.Tracks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class TrackTestsDrawing : GeneratedImageTest<Rgb24>
    {
        /// <summary>
        /// A set of unit converters that scales up some points so that they're visible
        /// and easily debuggable in a diagnostic image.
        /// </summary>
        public static readonly UnitConverters ScaledUnitConverters =
            new UnitConverters(
                segmentStartOffset: 60,
                segmentDuration: ImageSize * 0.1,
                nyquistFrequency: ImageSize * 10,
                imageWidth: ImageSize,
                imageHeight: ImageSize);

        public static readonly EventRenderingOptions Options = new EventRenderingOptions(ScaledUnitConverters);

        /// <summary>
        /// Each frame is 100 Hz, each bin is 0.1 seconds.
        /// </summary>
        public static readonly UnitConverters NiceTestConverter =
            new UnitConverters(
                segmentStartOffset: 60,
                sampleRate: 1000,
                frameSize: 100,
                frameOverlap: 0.0);

#pragma warning disable SA1310 // Field names should not contain underscore
        /// <summary>
        /// Get a track that is diagonal, increasing one unit
        /// both in time and frequency for each subsequent point.
        /// </summary>
        public static readonly Track TestTrack_TimePositive_FrequencyPositive =
            new Track(
                NiceTestConverter,
                TrackType.ForwardTrack,
                (5, 5, 1),
                (6, 6, 2),
                (7, 7, 3),
                (8, 8, 4),
                (9, 9, 5));

        public static readonly Track TestTrack_Whistle =
            new Track(
                NiceTestConverter,
                TrackType.OneBinTrack,
                (5, 5, 1),
                (6, 5, 2),
                (7, 5, 3),
                (8, 5, 4),
                (9, 5, 5));

        public static readonly Track TestTrack_ChevronRight =
            new Track(
                NiceTestConverter,
                TrackType.ForwardTrack,
                (5, 5, 1),
                (6, 6, 2),
                (7, 7, 3),
                (8, 8, 4),
                (9, 9, 5),
                (8, 10, 6),
                (7, 11, 7),
                (6, 12, 8),
                (5, 13, 9));
#pragma warning restore SA1310 // Field names should not contain underscore

        private const int ImageSize = 15;

        [TestMethod("Test draw ↗")]
        public void TestDrawUpRight()
        {
            var specification = @"
...............
...............
...............
...............
...............
.........G.....
........G......
.......G.......
......G........
.....G.........
...............
...............
...............
...............
...............
";
            this.ExpectedImage = TestImage.Create(ImageSize, ImageSize, Color.Black, specification);
            this.ActualImage = Drawing.NewImage(ImageSize, ImageSize, Color.Black);

            this.ActualImage.Mutate(x => TestTrack_TimePositive_FrequencyPositive.Draw(x, Options));

            this.AssertImagesEqual();
        }

        [TestMethod("Test draw →")]
        public void TestDrawRight()
        {
            var specification = @"
...............
...............
...............
...............
...............
...............
...............
...............
...............
.....GGGGG.....
...............
...............
...............
...............
...............
";
            this.ExpectedImage = TestImage.Create(ImageSize, ImageSize, Color.Black, specification);
            this.ActualImage = Drawing.NewImage(ImageSize, ImageSize, Color.Black);

            this.ActualImage.Mutate(x => TestTrack_Whistle.Draw(x, Options));

            this.AssertImagesEqual();
        }

        [TestMethod("Test draw >")]
        public void TestDrawChevronRight()
        {
            var specification = @"
...............
.....G.........
......G........
.......G.......
........G......
.........G.....
........G......
.......G.......
......G........
.....G.........
...............
...............
...............
...............
...............
";
            this.ExpectedImage = TestImage.Create(ImageSize, ImageSize, Color.Black, specification);
            this.ActualImage = Drawing.NewImage(ImageSize, ImageSize, Color.Black);

            this.ActualImage.Mutate(x => TestTrack_ChevronRight.Draw(x, Options));

            this.AssertImagesEqual();
        }
    }
}
