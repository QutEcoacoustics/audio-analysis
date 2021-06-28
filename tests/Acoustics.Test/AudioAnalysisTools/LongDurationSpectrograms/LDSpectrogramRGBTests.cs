// <copyright file="LDSpectrogramRGBTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisBase.ResultBases;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;

    [TestClass]
    public class LDSpectrogramRGBTests : OutputDirectoryTest
    {
        private void GenerateAndTestFakeFCS(LdSpectrogramConfig config, int expectedHeight)
        {
            var indexPropertiesFile = ConfigFile.Default<IndexPropertiesCollection>();
            var indexProperties = ConfigFile.Deserialize<IndexPropertiesCollection>(indexPropertiesFile);

            var indexSpectrograms = new Dictionary<string, double[,]>(6);
            var indexStatistics = new Dictionary<string, IndexDistributions.SpectralStats>();
            var keys = (LDSpectrogramRGB.DefaultColorMap1 + "-" + LDSpectrogramRGB.DefaultColorMap2).Split('-');
            foreach (var key in keys)
            {
                var matrix = new double[256, 60].Fill(indexProperties[key].DefaultValue);
                indexSpectrograms.Add(key, matrix);
                double[] array = DataTools.Matrix2Array(matrix);
                indexStatistics.Add(key, IndexDistributions.GetModeAndOneTailedStandardDeviation(array, 300, IndexDistributions.UpperPercentileDefault));
            }

            var images = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                inputDirectory: null,
                outputDirectory: this.TestOutputDirectory,
                ldSpectrogramConfig: config,
                indexPropertiesConfigPath: indexPropertiesFile,
                indexGenerationData: new IndexGenerationData()
                {
                    AnalysisStartOffset = 0.Seconds(),
                    FrameLength = 512,
                    IndexCalculationDuration = 60.0.Seconds(),
                    RecordingBasename = "RGB_TEST",
                    RecordingDuration = 60.0.Seconds(),
                    SampleRateResampled = 22050,
                },
                basename: "RGB_TEST",
                analysisType: AcousticIndices.AnalysisName,
                indexSpectrograms: indexSpectrograms,
                summaryIndices: Enumerable
                    .Range(0, 60)
                    .Select((x) => new SummaryIndexValues(60.0.Seconds(), indexProperties))
                    .Cast<SummaryIndexBase>()
                    .ToArray(),
                indexStatistics: indexStatistics);

            Assert.IsNotNull(images);

            // chromeless images returned
            foreach (var (image, key) in images)
            {
                Assert.That.ImageIsSize(60, 256, image);
                Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(0, 0, 60, 256), Color.Black, image);
            }

            // check images on disk are chromed/chromeless
            var imagesToCheck = images.Select(item => item.Item2).Concat(keys);
            foreach (var key in imagesToCheck)
            {
                var image = FilenameHelpers.AnalysisResultPath(this.TestOutputDirectory, "RGB_TEST", key, "png");
                this.TestContext.WriteLine($"Testing {key}, found {image}");
                var info = Image.Identify(image);

                Assert.AreEqual(60, info.Bounds().Width);
                Assert.AreEqual(expectedHeight, info.Bounds().Height, $"Image {image} was not expected height {expectedHeight} but was  {info.Height}");
            }
        }

        [TestMethod]
        public void TestChromedImage()
        {
            var config = new LdSpectrogramConfig();
            Assert.IsTrue(config.ImageChrome, "Should render chrome by default");

            var expectedHeight = 256 + SpectrogramConstants.HEIGHT_OF_TITLE_BAR + (LDSpectrogramRGB.TrackHeight * 2); // track height, bottom and top

            this.GenerateAndTestFakeFCS(config, expectedHeight);
        }

        [TestMethod]
        public void TestChromelessImage()
        {
            var config = new LdSpectrogramConfig() { ImageChrome = false };

            this.GenerateAndTestFakeFCS(config, 256);
        }

        [TestMethod]
        public void TestDrawRgbColorMatrix()
        {
            // init three matrices
            double[,] redM = new double[5, 5];
            double[,] grnM = new double[5, 5];
            double[,] bluM = new double[5, 5];

            //convert some values to null or NaN
            redM[1, 1] = double.NaN;
            grnM[1, 1] = double.NaN;
            bluM[1, 1] = double.NaN;

            redM[2, 2] = 1.0;
            grnM[2, 2] = 1.0;
            bluM[2, 2] = double.NaN;

            redM[3, 3] = 0.01;
            grnM[3, 3] = 0.01;
            bluM[3, 3] = 0.11;

            var image = (Image<Rgb24>)LDSpectrogramRGB.DrawRgbColorMatrix(redM, grnM, bluM, doReverseColor: true, 0.5);

            Assert.That.PixelIsColor(new Point(1, 1), Color.FromRgb(128, 128, 128), image);
            Assert.That.PixelIsColor(new Point(2, 2), Color.FromRgb(128, 128, 128), image);

            // empty values are rendered as white because of `doReverseColour`
            Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(0, 0, 1, 5), Color.FromRgb(255, 255, 255), image);
            Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(4, 0, 5, 5), Color.FromRgb(255, 255, 255), image);
        }
    }
}