// <copyright file="LDSpectrogramRGBTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using global::AnalysisBase.ResultBases;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class LDSpectrogramRGBTests : OutputDirectoryTest
    {
        [TestMethod]
        public void TestChromelessImage()
        {
            var indexPropertiesFile = ConfigFile.Default<IndexPropertiesCollection>();
            var indexProperties = ConfigFile.Deserialize<IndexPropertiesCollection>(indexPropertiesFile);

            var indexSpecotrgrams = new Dictionary<string, double[,]>(6);
            var indexStatistics = new Dictionary<string, IndexDistributions.SpectralStats>();
            var keys = (LDSpectrogramRGB.DefaultColorMap1 + "-" + LDSpectrogramRGB.DefaultColorMap2).Split('-');
            foreach (var key in keys)
            {
                var matrix = new double[256, 60].Fill(indexProperties[key].DefaultValue);
                indexSpecotrgrams.Add(key, matrix);

                indexStatistics.Add(key, IndexDistributions.GetModeAndOneTailedStandardDeviation(matrix, 300, IndexDistributions.UpperPercentileDefault));
            }

            var images = LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                inputDirectory: null,
                outputDirectory: this.outputDirectory,
                ldSpectrogramConfig: new LdSpectrogramConfig(),
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
                indexSpectrograms: indexSpecotrgrams,
                summaryIndices: Enumerable.Range(0, 60)
                    .Select((x) => new SummaryIndexValues(60.0.Seconds(), indexProperties)).ToArray(),
                indexStatistics: indexStatistics,
                imageChrome: ImageChrome.Without);

            foreach (var (image, key) in images)
            {
                // image.Save(Path.Combine(this.outputDirectory.FullName, key + ".png"), ImageFormat.Png);
                Assert.That.ImageRegionIsColor(Rectangle.FromLTRB(0, 0, 60, 256), Color.Black, (Bitmap)image);
            }
        }

        [TestMethod]
        public void TestDrawRgbColourMatrix()
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

            var image = (Bitmap)LDSpectrogramRGB.DrawRgbColourMatrix(redM, grnM, bluM, true);

            Assert.That.PixelIsColor(new Point(1, 1), Color.DarkGray, image);
            Assert.That.PixelIsColor(new Point(2, 2), Color.DarkGray, image);

            // To intensify the blue, this method adds 0.7 * d3 to the green value;
            // and it adds 0.2 to the blue value;
            // The effect is to create a more visible cyan colour.
            int r = (int)(redM[3, 3] * 255);
            int g = (int)((grnM[3, 3] * 255) + (0.7 * bluM[3, 3]));
            int b = (int)((bluM[3, 3] + 0.2) * 255);
            var cyanColour = Color.FromArgb(r, g, b);
            Assert.That.PixelIsColor(new Point(3, 3), cyanColour, image);
        }
    }
}
