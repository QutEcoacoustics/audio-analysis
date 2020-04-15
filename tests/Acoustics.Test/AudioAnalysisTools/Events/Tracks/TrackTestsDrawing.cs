// <copyright file="TrackTestsDrawing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Events.Tracks
{
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Test.TestHelpers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events.Drawing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    [TestClass]
    public class TrackTestsDrawing : GeneratedImageTest<Rgb24>
    {
        private const int ImageSize = 15;
        /// <summary>
        /// A set of unit converters that scales up some points so that they're visible
        /// and easily debuggable in a diagnostic image.
        /// </summary>
        public static readonly UnitConverters ScaledUnitConverters =
            new UnitConverters(
                segmentStartOffset: 60,
                segmentDuration: ImageSize * 0.05,
                nyquistFrequency: ImageSize * 10,
                imageWidth: ImageSize,
                imageHeight: ImageSize);

        public static readonly EventRenderingOptions Options = new EventRenderingOptions(ScaledUnitConverters);

        [TestMethod("Test draw ↗")]
        public void TestDraw()
        {
            var specification = @"
...............
...............
...............
...............
...............
.........R.....
........R......
.......R.......
......R........
.....R.........
...............
...............
...............
...............
...............
";
            this.ExpectedImage = TestImage.Create(ImageSize, ImageSize, Color.Black, specification);
            this.ActualImage = Drawing.NewImage(ImageSize, ImageSize, Color.Black);

            this.ActualImage.Mutate(x => TrackTests.TestTrack_TimePositive_FrequencyPositive.Draw(x, Options));

            this.AssertImagesEqual();
        }
    }
}
