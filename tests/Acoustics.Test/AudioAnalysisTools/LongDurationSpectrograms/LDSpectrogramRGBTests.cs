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
    }
}
