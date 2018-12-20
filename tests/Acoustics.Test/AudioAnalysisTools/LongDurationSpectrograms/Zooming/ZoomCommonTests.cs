// <copyright file="ZoomCommonTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;

    using Acoustics.Test.TestHelpers;

    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::AudioAnalysisTools.LongDurationSpectrograms.Zooming;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ZoomCommonTests
    {
        public ZoomCommonTests()
        {
        }

        /// <summary>
        /// This test is designed to ensure that images are produced at the correct length.
        /// We had a beviour where they were being trimmed by a whole pixel at the end of
        /// recordings that didn't wholly fill a minute. At very low scales this is acceptable, but
        /// at high scales just because 1/6th of a pixel's worth of data is absent does not mean
        /// that we shouldn't render the other 5/6ths of the pixel. And if we don't, it creates
        /// a very unsightly effect when the tiles are rendered adjacently.
        /// This test assumes a recording of 3599 seconds. All examples below assume this.
        /// </summary>
        [DataTestMethod]
        [DataRow(60.0, 60)] // at this scale 1/60th of a pixel is missing... but we expect the width to be padded
        [DataRow(10.0, 360)] // at this scale 1/6th of a pixel is missing... but we expect the width to be padded
        [DataRow(1.6, 2249)] // at this scale 5/8th of a pixel is missing... but we expect the width to be padded
        [DataRow(1.0, 3599)] // at this scale 1 whole pixel is missing... thus the image is not padded
        [DataRow(0.8, 4499)] // at this scale 1.25 pixels are missing... image is padded by 0.25 px, and misisng 1px
        [DataRow(0.4, 8998)] // at this scale 2.5 pixels are missing... image is padded by 0.5 px, and misisng 2px
        [DataRow(0.2, 17995)] // at this scale 5 pixels are missing... image is padded by 0 px, and misisng 5px
        [DataRow(0.1, 35990)] // at this scale 10 pixels are missing... image is padded by 0 px, and misisng 10px
        public void TestImagesHaveCorrectLength(double renderScale, int expectedWidth)
        {
            // this value just represents a 'render as much as you can until this limit' threshold
            const int ImageWidth = 1024;

            var dataScale = 0.1.Seconds();
            var imageScale = renderScale.Seconds();

            var recordingDuration = 3599.Seconds();

            // we simulate rendering the last tile at each resolution
            var tileDuration = imageScale.Multiply(ImageWidth);
            var numberOfTiles = (int)Math.Floor(recordingDuration.TotalSeconds / tileDuration.TotalSeconds);
            var startTime = tileDuration.Multiply(numberOfTiles);

            // 1 second short of an hour - this is the test case
            var endTime = recordingDuration;

            var config = new LdSpectrogramConfig();
            var generationData = new IndexGenerationData()
                                     {
                                         IndexCalculationDuration = dataScale,
                                         FrameLength = 512,
                                         FrameStep = 0,
                                         RecordingDuration = recordingDuration,
                                     };
            var indexProperties =
                IndexProperties.GetIndexProperties(PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml"));

            // create some fake spectra
            int duration = (int)(recordingDuration.TotalSeconds / dataScale.TotalSeconds);
            var spectra = new Dictionary<string, double[,]>();
            foreach (var key in SpectralIndexValues.Keys)
            {
                spectra.Add(key, new double[256, duration]);
                var bgnKey = nameof(SpectralIndexValues.BGN);
                if (key == bgnKey)
                {
                    spectra[bgnKey].Fill(indexProperties[bgnKey].DefaultValue);
                }

                // due to a bug in DrawRgbColourMatrix the ACI calculation ends up being NaN.
                // This we fill one of our other spectra to a non-zero value to get the black colour we desire
                // Bug reference: https://github.com/QutEcoacoustics/audio-analysis/issues/154
                var sumKey = nameof(SpectralIndexValues.SUM);
                if (key == sumKey)
                {
                    // this forces ACI calculation to 0 / 1 instead of 0 / 0
                    spectra[sumKey].Fill(1.0);
                }
            }

            string basename = "abc";

            var image = ZoomCommon.DrawIndexSpectrogramCommon(
                config,
                generationData,
                indexProperties,
                startTime,
                endTime,
                dataScale,
                imageScale,
                ImageWidth,
                spectra,
                basename);

            // since we just asked for a fragment of the image at the end,
            // the expected width is just for that last fragment
            var lastWidth = expectedWidth - (numberOfTiles * ImageWidth);
            Assert.That.ImageIsSize(lastWidth, 256, image);

            Bitmap bitmap = (Bitmap)image;

            // test output for debugging
            //image.Save("./" + renderScale + ".png");
            Assert.That.ImageRegionIsColor(new Rectangle(0, 0, lastWidth, 256), Color.Black, bitmap, 0);
        }
    }
}
