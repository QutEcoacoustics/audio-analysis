// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Concatenation
{
    using System;
    using System.Drawing;
    using System.IO;
    using Acoustics.Shared;
    using EcoSounds.Mvc.Tests;
    using global::AnalysisPrograms;
    using global::AudioAnalysisTools.LongDurationSpectrograms;
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
    public class ConcatenationTests
    {
        private DirectoryInfo outputDirectory;

        #region Additional test attributes

        /*
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        */

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = TestHelper.GetTempDir();

            var zippedDataFile = new FileInfo("Concatenation\\Indonesia20160726.zip");
            ZipUnzip.UnZip(this.outputDirectory.FullName, zippedDataFile.FullName, true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        #endregion

        /*
         * An example of modifying a default config file
            // get the default config file
            var defaultConfigFile = ConfigsHelper.ResolveConfigFilePath("SpectrogramFalseColourConfig.yml");
            var config = Yaml.Deserialise<LdSpectrogramConfig>(defaultConfigFile);

            // make changes to config file as required for test
            var testConfig = new FileInfo(this.outputDirectory + "\\SpectrogramFalseColourConfig.yml");
            Yaml.Serialise(testConfig, config);
        */

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = true
        /// </summary>
        [TestMethod]
        public void ConcatenateEverythingYouCanLayYourHandsOn()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { this.outputDirectory.Combine("Indonesia20160726") };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var dateString = "20160725";

            // get the default config file
            var testConfig = ConfigsHelper.ResolveConfigFilePath("SpectrogramFalseColourConfig.yml");

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
                SunRiseDataFile = null,
                DrawImages = true,
                Verbose = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Do TESTS on the 2Maps image
            // Compare image files - check that image dimensions are correct
            var outputDataDir = new DirectoryInfo(this.outputDirectory.FullName + "\\" + arguments.FileStemName + "\\" + dateString);
            string imageFileName = arguments.FileStemName + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            ImageAssert.IsSize(722, 632, actualImage);

            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage);

            ImageAssert.PixelIsColor(new Point(200, 100), Color.FromArgb(54, 28, 9), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only. In this test we concatenate only 26/07/2016
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24Hour()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { this.outputDirectory.Combine("Indonesia20160726") };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // get the default config file
            var testConfig = ConfigsHelper.ResolveConfigFilePath("SpectrogramFalseColourConfig.yml");

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = this.outputDirectory,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = testConfig,
                ColorMap1 = LDSpectrogramRGB.DefaultColorMap1,
                ColorMap2 = "BGN-POW-EVN", // POW was depracated post May 2017
                ConcatenateEverythingYouCanLayYourHandsOn = false, // 24 hour blocks only
                TimeSpanOffsetHint = TimeSpan.FromHours(8),
                SunRiseDataFile = null,
                DrawImages = true,
                Verbose = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // Do TESTS on the 2Maps image
            // Compare image files - check that image dimensions are correct
            var outputDataDir = new DirectoryInfo(this.outputDirectory.FullName + "\\" + arguments.FileStemName + "\\" + dateString);
            string imageFileName = arguments.FileStemName + "_" + dateString + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            ImageAssert.IsSize(512, 632, actualImage);

            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(32, 24, 17), actualImage);

            ImageAssert.PixelIsColor(new Point(100, 160), Color.FromArgb(0, 22, 38), actualImage);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only.
        /// This test is same as TEST2 above escept that the start and end date have been set to null.
        /// Start and end dates will be set to first and last by default and all available data will be concatentated in 24 hour blocks.
        /// In the case of this dataset, the two partial days of data will be concatenated separately.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest24HourWithDateRange()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { this.outputDirectory.Combine("Indonesia20160726") };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");

            // get the default config file
            var testConfig = ConfigsHelper.ResolveConfigFilePath("SpectrogramFalseColourConfig.yml");

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
                SunRiseDataFile = null,
                DrawImages = true,
                Verbose = true,

                // following two lines can be used to add in a recognizer score track
                EventDataDirectories = null,
                EventFilePattern = null,
            };

            ConcatenateIndexFiles.Execute(arguments);

            // There should be two sets of output images one for each partial day.
            // IMAGE 1: Compare image files - check that image exists and dimensions are correct
            var dateString1 = "20160725";
            var outputDataDir1 = new DirectoryInfo(this.outputDirectory.FullName + "\\" + arguments.FileStemName + "\\" + dateString1);
            string image1FileName = arguments.FileStemName + "_" + dateString1 + "__2Maps.png";
            var image1FileInfo = new FileInfo(Path.Combine(outputDataDir1.FullName, image1FileName));
            Assert.IsTrue(image1FileInfo.Exists);

            var actualImage1 = ImageTools.ReadImage2Bitmap(image1FileInfo.FullName);
            ImageAssert.IsSize(210, 632, actualImage1);

            ImageAssert.PixelIsColor(new Point(100, 100), Color.FromArgb(211, 211, 211), actualImage1);

            ImageAssert.PixelIsColor(new Point(50, 50), Color.FromArgb(86, 27, 8), actualImage1);

            // IMAGE 2: Compare image files - check that image exists and dimensions are correct
            var dateString2 = "20160726";
            var outputDataDir2 = new DirectoryInfo(this.outputDirectory.FullName + "\\" + arguments.FileStemName + "\\" + dateString2);
            string image2FileName = arguments.FileStemName + "_" + dateString2 + "__2Maps.png";
            var image2FileInfo = new FileInfo(Path.Combine(outputDataDir2.FullName, image2FileName));
            Assert.IsTrue(image2FileInfo.Exists);

            var actualImage2 = ImageTools.ReadImage2Bitmap(image2FileInfo.FullName);
            ImageAssert.IsSize(512, 632, actualImage2);

            ImageAssert.PixelIsColor(new Point(50, 124), Color.FromArgb(70, 37, 255), actualImage2);

            ImageAssert.PixelIsColor(new Point(460, 600), Color.FromArgb(255, 0, 0), actualImage2);
        }
    }
}
