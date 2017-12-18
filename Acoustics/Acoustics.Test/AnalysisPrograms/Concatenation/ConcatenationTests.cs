// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Concatenation
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;

    using Acoustics.Shared;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    /// <summary>
    /// Test methods for the various Frequency Scales
    /// Notes on TESTS: (from Anthony in email @ 05/04/2017)
    /// (1) small tests are better
    /// (2) simpler tests are better
    /// (3) use an appropriate serialisation format
    /// (4) for binary large objects(BLOBs) make sure git-lfs is tracking them
    /// </summary>
    [TestClass]
    // [Ignore]
    public class ConcatenationTests : OutputDirectoryTest
    {
        private static DirectoryInfo indonesiaIndicesDirectory;

        private static DirectoryInfo newZealandArk01IndicesDirectory;

        private const string IndonesiaReduced = "Indonesia_2Reduced";

        private const string NewZealandArk01 = "NewZealandArk01";

        /// Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            var indonesiaIndices = PathHelper.ResolveAsset("Concatenation", IndonesiaReduced + ".zip");
            var newZealandIndices = PathHelper.ResolveAsset("Concatenation", NewZealandArk01 + ".zip");

            indonesiaIndicesDirectory = SharedDirectory.Combine(IndonesiaReduced);
            newZealandArk01IndicesDirectory = SharedDirectory.Combine(NewZealandArk01);

            ZipFile.ExtractToDirectory(indonesiaIndices.FullName, indonesiaIndicesDirectory.FullName);
            ZipFile.ExtractToDirectory(newZealandIndices.FullName, newZealandArk01IndicesDirectory.FullName);
        }

        /*
         * An example of modifying a default config file
            // get the default config file
            var defaultConfigFile = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");
            var config = Yaml.Deserialise<LdSpectrogramConfig>(defaultConfigFile);

            // make changes to config file as required for test
            var testConfig = new FileInfo(this.outputDirectory + "\\SpectrogramFalseColourConfig.yml");
            Yaml.Serialise(testConfig, config);
        */

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when
        /// ConcatenateEverythingYouCanLayYourHandsOn = true
        /// </summary>
        [TestMethod]
        public void ConcatenateEverythingYouCanLayYourHandsOn()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { indonesiaIndicesDirectory };
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160725";

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test1_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = testConfig,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-POW-EVN", // POW was depracated post May 2017
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
            ImageAssert.IsSize(722, 632, actualImage);
            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage);
            ImageAssert.PixelIsColor(new Point(200, 100), Color.FromArgb(54, 29, 18), actualImage);
            ImageAssert.PixelIsColor(new Point(675, 600), Color.FromArgb(255, 105, 180), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only. In this test we concatenate only 26/07/2016
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24Hour()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { indonesiaIndicesDirectory };
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = testConfig,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-POW-EVN", // POW was depracated post May 2017
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
            ImageAssert.IsSize(512, 632, actualImage);
            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(32, 25, 36), actualImage);
            ImageAssert.PixelIsColor(new Point(100, 160), Color.FromArgb(0, 80, 132), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only.
        /// This test is same as TEST2 above escept that the start and end date have been set to null.
        /// Start and end dates will be set to first and last by default and all available data will be concatentated in 24 hour blocks.
        /// In the case of this dataset, the two partial days of data will be concatenated separately.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24HourWithoutDateRange()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { indonesiaIndicesDirectory };
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");

            // get the default config file
            var testConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test3_Indonesia",
                StartDate = null,
                EndDate = null,
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = testConfig,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-POW-EVN", // POW was depracated post May 2017
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
            ImageAssert.IsSize(210, 632, actualImage1);
            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage1);
            ImageAssert.PixelIsColor(new Point(50, 50), Color.FromArgb(86, 29, 17), actualImage1);

            // IMAGE 2: Compare image files - check that image exists and dimensions are correct
            var dateString2 = "20160726";
            var outputDataDir2= this.outputDirectory.Combine(arguments.FileStemName, dateString2);
            var prefix2 = arguments.FileStemName + "_" + dateString2 + "__";

            var image2FileInfo = outputDataDir2.CombineFile(prefix2 + "2Maps.png");
            Assert.IsTrue(image2FileInfo.Exists);

            Assert.That.FileExists(outputDataDir2.CombineFile(prefix2 + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir2.CombineFile(prefix2 + "SummaryIndex.csv"));

            var actualImage2 = ImageTools.ReadImage2Bitmap(image2FileInfo.FullName);
            ImageAssert.IsSize(512, 632, actualImage2);
            ImageAssert.PixelIsColor(new Point(50, 124), Color.FromArgb(70, 38, 255), actualImage2);
            ImageAssert.PixelIsColor(new Point(460, 600), Color.FromArgb(255, 105, 180), actualImage2);
        }

        /// <summary>
        /// This test checks that settings in an SpectrogramFalseColourConfig are honored.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTestConfigFileChanges()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { indonesiaIndicesDirectory };
            var indexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // get the default config file
            var defaultConfigFile = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml");
            var config = Yaml.Deserialise<LdSpectrogramConfig>(defaultConfigFile);

            // make changes to config file as required for test
            config.ColorMap1 = "BGN-ENT-POW";
            config.ColorMap2 = "ACI-RNG-EVN";

            // write new config
            var testConfig = this.outputDirectory.CombineFile("SpectrogramFalseColourConfig.yml");
            Yaml.Serialise(testConfig, config);

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 27, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = testConfig,
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                DrawImages = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Make sure files that match our config file are actully created!
            var outputDataDir = this.outputDirectory.Combine(arguments.FileStemName, dateString);
            var prefix = arguments.FileStemName + "_" + dateString + "__";

            Assert.That.FileExists(outputDataDir.CombineFile(prefix + "Towsey.Acoustic.Indices.csv"));
            Assert.That.FileNotExists(outputDataDir.CombineFile(prefix + "SummaryIndex.csv"));

            var imageFileInfo1 = outputDataDir.CombineFile(prefix + "BGN-ENT-POW.png");
            Assert.IsTrue(imageFileInfo1.Exists);

            var imageFileInfo2 = outputDataDir.CombineFile(prefix + "ACI-RNG-EVN.png");
            Assert.IsTrue(imageFileInfo2.Exists);
        }

        [TestMethod]
        public void ConcatenateIndexFilesSampledDataGapsTest()
        {
            var arguments = new ConcatenateIndexFiles.Arguments
                {
                    InputDataDirectories = new DirectoryInfo[] { newZealandArk01IndicesDirectory },
                    OutputDirectory = this.outputDirectory,
                    DirectoryFilter = "*.wav",
                    FileStemName = "Test3_Indonesia",
                    StartDate = null,
                    EndDate = null,
                    IndexPropertiesConfig = PathHelper.ResolveConfigFile("IndexPropertiesConfig.yml"),
                    FalseColourSpectrogramConfig = PathHelper.ResolveConfigFile("SpectrogramFalseColourConfig.yml"),
                    ColorMap1 = null,
                    ColorMap2 = null,
                    ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                    TimeSpanOffsetHint = TimeSpan.FromHours(12),
                    DrawImages = true,
                };

            ConcatenateIndexFiles.Execute(arguments);

            // There should be three sets of output images one for each partial day.
            // TODO: finish new zealand tests for all 3 modes - TimeGaps, No Gaps, EchoGaps
            Assert.Fail("Not yet completed");

            // IMAGE 1: Compare image files - check that image exists and dimensions are correct
            var dateString1 = "20160725";
            var outputDataDir1 = this.outputDirectory.Combine(arguments.FileStemName, dateString1);
            string image1FileName = arguments.FileStemName + "_" + dateString1 + "__2Maps.png";
            var image1FileInfo = outputDataDir1.CombineFile(outputDataDir1.FullName, image1FileName);
            Assert.IsTrue(image1FileInfo.Exists);

            var actualImage1 = ImageTools.ReadImage2Bitmap(image1FileInfo.FullName);
            ImageAssert.IsSize(210, 632, actualImage1);
            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage1);
            ImageAssert.PixelIsColor(new Point(50, 50), Color.FromArgb(86, 29, 17), actualImage1);

            // IMAGE 2: Compare image files - check that image exists and dimensions are correct
            var dateString2 = "20160726";
            var outputDataDir2 = this.outputDirectory.Combine(arguments.FileStemName, dateString2);
            string image2FileName = arguments.FileStemName + "_" + dateString2 + "__2Maps.png";
            var image2FileInfo = outputDataDir2.CombineFile(image2FileName);
            Assert.IsTrue(image2FileInfo.Exists);

            var actualImage2 = ImageTools.ReadImage2Bitmap(image2FileInfo.FullName);
            ImageAssert.IsSize(512, 632, actualImage2);
            ImageAssert.PixelIsColor(new Point(50, 124), Color.FromArgb(70, 38, 255), actualImage2);
            ImageAssert.PixelIsColor(new Point(460, 600), Color.FromArgb(255, 105, 180), actualImage2);
        }
    }
}
