// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Concatenation
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test methods for the various Frequency Scales
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialization format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    ///
    /// Note: these tests are poorly designed. We need to use/generate mock data
    /// because the data stored in the zip files can easily become out dated.
    /// </summary>
    [TestClass]
    public class ConcatenationTests : OutputDirectoryTest
    {
        private const string IndonesiaReduced = "Indonesia_2Reduced";

        private const string NewZealandArk01 = "NewZealandArk01";

        private static DirectoryInfo indonesiaIndicesDirectory;

        private static DirectoryInfo newZealandArk01IndicesDirectory;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            var indonesiaIndices = PathHelper.ResolveAsset("Concatenation", IndonesiaReduced + ".zip");
            var newZealandIndices = PathHelper.ResolveAsset("Concatenation", NewZealandArk01 + ".zip");

            indonesiaIndicesDirectory = SharedDirectory.Combine(IndonesiaReduced);
            newZealandArk01IndicesDirectory = SharedDirectory.Combine(NewZealandArk01);

            ZipFile.ExtractToDirectory(indonesiaIndices.FullName, indonesiaIndicesDirectory.FullName);
            ZipFile.ExtractToDirectory(newZealandIndices.FullName, newZealandArk01IndicesDirectory.FullName);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when
        /// ConcatenateEverythingYouCanLayYourHandsOn = true.
        /// </summary>
        [TestMethod]
        public void ConcatenateEverythingYouCanLayYourHandsOn()
        {
            // top level directory
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160725";

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = new[] { indonesiaIndicesDirectory },
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test1_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig.FullName,
                FalseColourSpectrogramConfig = testConfig.FullName,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-PMN-EVN",
                ConcatenateEverythingYouCanLayYourHandsOn = true, // join everything found
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Do TESTS on the 2Maps image
            // Compare image files - check that image dimensions are correct
            var outputDataDir = this.outputDirectory.Combine(arguments.FileStemName, dateString);
            var prefix = arguments.FileStemName + "__";

            var imageFileInfo = outputDataDir.CombineFile(prefix + "2Maps.png");
            Assert.IsTrue(imageFileInfo.Exists);

            Assert.That.FileExists(outputDataDir.CombineFile(prefix + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir.CombineFile(prefix + "SummaryIndex.csv"));

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            Assert.That.ImageIsSize(722, 632, actualImage);
            Assert.That.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage);
            Assert.That.PixelIsColor(new Point(200, 125), Color.FromArgb(60, 44, 203), actualImage);
            Assert.That.PixelIsColor(new Point(675, 600), Color.FromArgb(255, 105, 180), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only. In this test we concatenate only 26/07/2016.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24Hour()
        {
            // top level directory
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = new[] { indonesiaIndicesDirectory },
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig.FullName,
                FalseColourSpectrogramConfig = testConfig.FullName,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-PMN-EVN",
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Do TESTS on the 2Maps image
            // Compare image files - check that image dimensions are correct
            var outputDataDir = this.outputDirectory.Combine(arguments.FileStemName, dateString);
            var prefix = arguments.FileStemName + "_" + dateString + "__";

            var imageFileInfo = outputDataDir.CombineFile(prefix + "2Maps.png");
            Assert.IsTrue(imageFileInfo.Exists);

            Assert.That.FileExists(outputDataDir.CombineFile(prefix + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir.CombineFile(prefix + "SummaryIndex.csv"));

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);

            // we expect only the second half (past midnight) of the image to be rendered
            Assert.That.ImageIsSize(512, 632, actualImage);
            Assert.That.PixelIsColor(new Point(105, 154), Color.FromArgb(34, 30, 126), actualImage);
            Assert.That.PixelIsColor(new Point(100, 160), Color.FromArgb(8, 28, 64), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only.
        /// This test is same as TEST2 above except that the start and end date have been set to null.
        /// Start and end dates will be set to first and last by default and all available data will be concatenated in 24 hour blocks.
        /// In the case of this dataset, the two partial days of data will be concatenated separately.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24HourWithoutDateRange()
        {
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = new[] { indonesiaIndicesDirectory },
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test3_Indonesia",
                StartDate = null,
                EndDate = null,
                IndexPropertiesConfig = indexPropertiesConfig.FullName,
                FalseColourSpectrogramConfig = testConfig.FullName,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-PMN-EVN",
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // There should be two sets of output images one for each partial day.
            // IMAGE 1: Compare image files - check that image exists and dimensions are correct
            var dateString1 = "20160725";
            var outputDataDir1 = this.outputDirectory.Combine(arguments.FileStemName, dateString1);
            var prefix1 = arguments.FileStemName + "_" + dateString1 + "__";

            var image1FileInfo = outputDataDir1.CombineFile(prefix1 + "2Maps.png");
            Assert.IsTrue(image1FileInfo.Exists);

            Assert.That.FileExists(outputDataDir1.CombineFile(prefix1 + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir1.CombineFile(prefix1 + "SummaryIndex.csv"));

            var actualImage1 = ImageTools.ReadImage2Bitmap(image1FileInfo.FullName);
            Assert.That.ImageIsSize(210, 632, actualImage1);
            Assert.That.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage1);
            Assert.That.PixelIsColor(new Point(50, 50), Color.FromArgb(86, 27, 8), actualImage1);

            // IMAGE 2: Compare image files - check that image exists and dimensions are correct
            var dateString2 = "20160726";
            var outputDataDir2 = this.outputDirectory.Combine(arguments.FileStemName, dateString2);
            var prefix2 = arguments.FileStemName + "_" + dateString2 + "__";

            var image2FileInfo = outputDataDir2.CombineFile(prefix2 + "2Maps.png");
            Assert.IsTrue(image2FileInfo.Exists);

            Assert.That.FileExists(outputDataDir2.CombineFile(prefix2 + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir2.CombineFile(prefix2 + "SummaryIndex.csv"));

            var actualImage2 = ImageTools.ReadImage2Bitmap(image2FileInfo.FullName);
            Assert.That.ImageIsSize(512, 632, actualImage2);
            Assert.That.PixelIsColor(new Point(50, 124), Color.FromArgb(70, 37, 255), actualImage2);
            Assert.That.PixelIsColor(new Point(460, 600), Color.FromArgb(255, 105, 180), actualImage2);
        }

        /// <summary>
        /// This test checks that settings in an SpectrogramFalseColourConfig are honored.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTestConfigFileChanges()
        {
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // get the default config file
            var defaultConfigFile = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");
            var config = Yaml.Deserialize<LdSpectrogramConfig>(defaultConfigFile);

            // make changes to config file as required for test
            config.ColorMap1 = "BGN-ENT-PMN";
            config.ColorMap2 = "ACI-RNG-EVN";

            // write new config
            var testConfig = this.outputDirectory.CombineFile("SpectrogramFalseColourConfig.yml");
            Yaml.Serialize(testConfig, config);

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = new[] { indonesiaIndicesDirectory },
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig.FullName,
                FalseColourSpectrogramConfig = testConfig.FullName,
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Make sure files that match our config file are actually created!
            var outputDataDir = this.outputDirectory.Combine(arguments.FileStemName, dateString);
            var prefix = arguments.FileStemName + "_" + dateString + "__";

            Assert.That.FileExists(outputDataDir.CombineFile(prefix + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir.CombineFile(prefix + "SummaryIndex.csv"));

            var imageFileInfo1 = outputDataDir.CombineFile(prefix + "BGN-ENT-PMN.png");
            Assert.IsTrue(imageFileInfo1.Exists);

            var imageFileInfo2 = outputDataDir.CombineFile(prefix + "ACI-RNG-EVN.png");
            Assert.IsTrue(imageFileInfo2.Exists);
        }

        /// <summary>
        /// These tests use a dataset constructed from one-minute long samples, recorded every 20-min.
        /// Thus there's a column of data for every 20 pixels.
        /// </summary>
        [DataTestMethod]
        [DataRow(ConcatMode.TimedGaps, new[] { 1440, 1420, 1440 })]
        [DataRow(ConcatMode.NoGaps, new[] { 73, 72, 73 })]
        [DataRow(ConcatMode.EchoGaps, new[] { 1440, 1420, 1440 })]
        public void SampledDataConcatModeTests(ConcatMode gapRendering, int[] expectedWidths)
        {
            const string ark01 = "Ark01";

            var arguments = new ConcatenateIndexFiles.Arguments
                {
                    InputDataDirectories = new[] { newZealandArk01IndicesDirectory },
                    OutputDirectory = this.outputDirectory,
                    DirectoryFilter = "*.wav",
                    FileStemName = ark01,
                    StartDate = null,
                    EndDate = null,
                    IndexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml").FullName,
                    FalseColourSpectrogramConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml").FullName,
                    ColorMap1 = null,
                    ColorMap2 = null,
                    ConcatenateEverythingYouCanLayYourHandsOn = false,
                    GapRendering = gapRendering,
                    TimeSpanOffsetHint = TimeSpan.FromHours(12),
                    DrawImages = true,
                };

            ConcatenateIndexFiles.Execute(arguments);

            var dateStrings = new[] { "20161209", "20161210", "20161211" }.Zip(expectedWidths, ValueTuple.Create);
            foreach (var (dateString, expectedWidth) in dateStrings)
            {
                var prefix = Path.Combine(this.outputDirectory.FullName, ark01, dateString, ark01 + "_" + dateString + "__");

                Assert.That.PathExists(prefix + "Towsey.Acoustic.Indices.csv");
                Assert.That.PathNotExists(prefix + "SummaryIndex.csv");

                var imagePath = prefix + "2Maps.png";
                Assert.That.FileExists(imagePath);

                var actualImage = ImageTools.ReadImage2Bitmap(imagePath);
                Assert.That.ImageIsSize(expectedWidth, 632, actualImage);

                // target region for each image: 40, 254, 20,20
                switch (gapRendering)
                {
                    case ConcatMode.TimedGaps:
                        // for timed gap, first column has content, other 19 don't and should be gray (missing data)
                        Assert.That.ImageRegionIsColor(
                            new Rectangle(40 + 1, 254, 20 - 1, 20),
                            Color.LightGray,
                            actualImage,
                            0.001);
                        break;
                    case ConcatMode.NoGaps:
                        // There should basically be no pattern here
                        var histogram = ImageTools.GetColorHistogramNormalized(
                            actualImage,
                            new Rectangle(40, 254, 20, 20));

                        // should not have empty space
                        var hasGray = histogram.TryGetValue(Color.LightGray, out var grayPercentage);
                        Assert.IsTrue(!hasGray || grayPercentage < 0.01);

                        // the rest of the values should be well distributed (not a perfect test)
                        Assert.That.ImageColorsWellDistributed(
                            actualImage,
                            allowedError: 0.1,
                            colorHistogram: histogram);

                        break;
                    case ConcatMode.EchoGaps:
                        // The first column should be repeated 19 times
                        Assert.That.ImageRegionIsRepeatedHorizontally(
                            new Rectangle(40, 254, 1, 20),
                            19,
                            1,
                            actualImage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(gapRendering), gapRendering, null);
                }
            }
        }
    }
}
