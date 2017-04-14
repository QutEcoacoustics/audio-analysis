// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Concatenation
{
    using System;
    using System.IO;
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
        }

        [TestCleanup]
        public void Cleanup()
        {
            TestHelper.DeleteTempDir(this.outputDirectory);
        }

        #endregion

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = true
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest1()
        {
            var outputDir = this.outputDirectory;
            var zippedDataFile = new FileInfo("Concatenation\\Indonesia20160726.zip");
            ZipUnzip.UnZip(outputDir.FullName, zippedDataFile.FullName, true);

            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(outputDir.FullName + "\\Indonesia20160726"), };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var dateString = "20160725";

            // make a default config file
            var falseColourSpgConfig = new FileInfo("ConcatTest_SpectrogramFalseColourConfig.yml");
            ConfigsHelper.WriteDefaultFalseColourSpgmConfig(falseColourSpgConfig);

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = outputDir,
                DirectoryFilter = "*.wav",
                FileStemName = "Test1_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 25, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT,
                ConcatenateEverythingYouCanLayYourHandsOn = true, // 24 hour blocks only
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
            var outputDataDir = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString);
            string imageFileName = arguments.FileStemName + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            Assert.AreEqual(722, actualImage.Width);
            Assert.AreEqual(632, actualImage.Height);

            var pixel1 = actualImage.GetPixel(100, 100);
            var pixel1Txt = pixel1.ToString();
            string c1 = "Color [A=255, R=211, G=211, B=211]";
            Assert.AreEqual(c1, pixel1Txt);

            var pixel2 = actualImage.GetPixel(200, 100);
            var pixel2Txt = pixel2.ToString();
            string c2 = "Color [A=255, R=54, G=28, B=7]";
            Assert.AreEqual(c2, pixel2Txt);
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only. In this test we concatenate only 26/07/2016
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest2()
        {
            var outputDir = this.outputDirectory;
            var zippedDataFile = new FileInfo("Concatenation\\Indonesia20160726.zip");
            ZipUnzip.UnZip(outputDir.FullName, zippedDataFile.FullName, true);

            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(outputDir.FullName + "\\Indonesia20160726"), };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var dateString = "20160726";

            // make a default config file
            var falseColourSpgConfig = new FileInfo("ConcatTest_SpectrogramFalseColourConfig.yml");
            ConfigsHelper.WriteDefaultFalseColourSpgmConfig(falseColourSpgConfig);

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = outputDir,
                DirectoryFilter = "*.wav",
                FileStemName = "Test2_Indonesia",
                StartDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                EndDate = new DateTimeOffset(2016, 07, 26, 0, 0, 0, TimeSpan.Zero),
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT,
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
            var outputDataDir = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString);
            string imageFileName = arguments.FileStemName + "_" + dateString + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            Assert.AreEqual(512, actualImage.Width);
            Assert.AreEqual(632, actualImage.Height);

            var pixel1 = actualImage.GetPixel(100, 100);
            var pixel1Txt = pixel1.ToString();
            string c1 = "Color [A=255, R=32, G=24, B=14]";
            Assert.AreEqual(c1, pixel1Txt);

            var pixel2 = actualImage.GetPixel(100, 160);
            var pixel2Txt = pixel2.ToString();
            string c2 = "Color [A=255, R=0, G=22, B=30]";
            Assert.AreEqual(c2, pixel2Txt);

            // Assert.Fail("Test construction in progrexss");
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only.
        /// This test is same as TEST2 above escept that the start and end date have been set to null.
        /// Start and end dates will be set to first and last by default and all available data will be concatentated in 24 hour blocks.
        /// In the case of this dataset, the two partial days of data will be concatenated separately.
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest3()
        {
            var outputDir = this.outputDirectory;
            var zippedDataFile = new FileInfo("Concatenation\\Indonesia20160726.zip");
            ZipUnzip.UnZip(outputDir.FullName, zippedDataFile.FullName, true);

            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo(outputDir.FullName + "\\Indonesia20160726"), };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");

            // make a default config file
            var falseColourSpgConfig = new FileInfo("ConcatTest_SpectrogramFalseColourConfig.yml");
            ConfigsHelper.WriteDefaultFalseColourSpgmConfig(falseColourSpgConfig);

            var arguments = new ConcatenateIndexFiles.Arguments
            {
                InputDataDirectories = dataDirs,
                OutputDirectory = outputDir,
                DirectoryFilter = "*.wav",
                FileStemName = "Test3_Indonesia",
                StartDate = null,
                EndDate = null,
                IndexPropertiesConfig = indexPropertiesConfig,
                FalseColourSpectrogramConfig = falseColourSpgConfig,
                ColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN,
                ColorMap2 = SpectrogramConstants.RGBMap_BGN_POW_SPT,
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
            var outputDataDir1 = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString1);
            string image1FileName = arguments.FileStemName + "_" + dateString1 + "__2Maps.png";
            var image1FileInfo = new FileInfo(Path.Combine(outputDataDir1.FullName, image1FileName));
            Assert.IsTrue(image1FileInfo.Exists);

            var actualImage1 = ImageTools.ReadImage2Bitmap(image1FileInfo.FullName);
            Assert.AreEqual(210, actualImage1.Width);
            Assert.AreEqual(632, actualImage1.Height);

            var pixel1 = actualImage1.GetPixel(100, 100);
            var pixel1Txt = pixel1.ToString();
            string c1 = "Color [A=255, R=211, G=211, B=211]";
            Assert.AreEqual(c1, pixel1Txt);

            var pixel2 = actualImage1.GetPixel(50, 50);
            var pixel2Txt = pixel2.ToString();
            string c2 = "Color [A=255, R=86, G=27, B=6]";
            Assert.AreEqual(c2, pixel2Txt);

            // IMAGE 2: Compare image files - check that image exists and dimensions are correct
            var dateString2 = "20160726";
            var outputDataDir2 = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString2);
            string image2FileName = arguments.FileStemName + "_" + dateString2 + "__2Maps.png";
            var image2FileInfo = new FileInfo(Path.Combine(outputDataDir2.FullName, image2FileName));
            Assert.IsTrue(image2FileInfo.Exists);

            var actualImage2 = ImageTools.ReadImage2Bitmap(image2FileInfo.FullName);
            Assert.AreEqual(512, actualImage2.Width);
            Assert.AreEqual(632, actualImage2.Height);

            pixel1 = actualImage2.GetPixel(50, 124);
            pixel1Txt = pixel1.ToString();
            c1 = "Color [A=255, R=70, G=37, B=203]";
            Assert.AreEqual(c1, pixel1Txt);

            pixel2 = actualImage2.GetPixel(460, 600);
            pixel2Txt = pixel2.ToString();
            c2 = "Color [A=255, R=255, G=0, B=0]";
            Assert.AreEqual(c2, pixel2Txt);
        }
    }
}
