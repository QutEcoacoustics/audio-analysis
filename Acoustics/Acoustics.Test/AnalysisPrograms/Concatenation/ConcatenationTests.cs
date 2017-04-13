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
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo("Concatenation\\Indonesia20160726"), };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var outputDir = this.outputDirectory;
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
            // 1: Compare image files - check that image dimensions are correct
            // Get ACTUAL IMAGE
            var outputDataDir = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString);

            //string imageFileName = arguments.FileStemName + "_" + dateString + "__2Maps.png";
            string imageFileName = arguments.FileStemName + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            Assert.AreEqual(722, actualImage.Width);
            Assert.AreEqual(632, actualImage.Height);

            var pixel1 = actualImage.GetPixel(100, 100);
            var pixel1Txt = pixel1.ToString();
            string c1 = "Color [A=255, R=59, G=21, B=3]";
            Assert.AreEqual(c1, pixel1Txt);

            var pixel2 = actualImage.GetPixel(200, 200);
            var pixel2Txt = pixel2.ToString();
            string c2 = "Color [A=255, R=0, G=6, B=0]";
            Assert.AreEqual(c2, pixel2Txt);

            /*
            var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
            Json.Serialise(resultFile2, freqScale.GridLineLocations);
            FileEqualityHelpers.TextFileEqual(expectedFile2, resultFile2);

            // Check that image dimensions are correct
            Assert.AreEqual(645, image.Width);
            Assert.AreEqual(310, image.Height);


            // DO EQUALITY TEST
            Get a DATA_MATRIX
            var expectedDataFile = new FileInfo("StandardSonograms\\BAC2_20071008_AmplSonogramData.EXPECTED.bin");

            // run this once to generate expected test data (and remember to copy out of bin/debug!)
            //Binary.Serialize(expectedFile, DATA_MATRIX);

            var expectedDATA = Binary.Deserialize<double[,]>(expectedDataFile);

            CollectionAssert.AreEqual(expectedDATA, DATA_MATRIX);
            */
        }

        /// <summary>
        /// METHOD TO CHECK Concatenation of spectral and summary index files when ConcatenateEverythingYouCanLayYourHandsOn = false
        /// that is, concatenate in 24 hour blocks only
        /// </summary>
        [TestMethod]
        public void ConcatenateIndexFilesTest2()
        {
            // top level directory
            DirectoryInfo[] dataDirs = { new DirectoryInfo("Concatenation\\Indonesia20160726"), };
            var indexPropertiesConfig = new FileInfo("Configs\\IndexPropertiesConfig.yml");
            var outputDir = this.outputDirectory;
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
            // 1: Compare image files - check that image dimensions are correct
            // Get ACTUAL IMAGE
            var outputDataDir = new DirectoryInfo(outputDir.FullName + "\\" + arguments.FileStemName + "\\" + dateString);
            string imageFileName = arguments.FileStemName + "_" + dateString + "__2Maps.png";
            var imageFileInfo = new FileInfo(Path.Combine(outputDataDir.FullName, imageFileName));
            Assert.IsTrue(imageFileInfo.Exists);

            var actualImage = ImageTools.ReadImage2Bitmap(imageFileInfo.FullName);
            Assert.AreEqual(512, actualImage.Width);
            Assert.AreEqual(632, actualImage.Height);

            var pixel1 = actualImage.GetPixel(100, 100);
            var pixel1Txt = pixel1.ToString();
            string c1 = "Color [A=255, R=15, G=13, B=14]";
            Assert.AreEqual(c1, pixel1Txt);

            var pixel2 = actualImage.GetPixel(200, 200);
            var pixel2Txt = pixel2.ToString();
            string c2 = "Color [A=255, R=0, G=1, B=0]";
            Assert.AreEqual(c2, pixel2Txt);

            /*
// Do TESTS on one of the concatenated csv files
// construct name of expected csv file to save
var stem = "ConcatenationTest";
string imageName = stem + ".EXPECTED.csv";
string imagePath = Path.Combine(outputDir.FullName, imageName);
var expectedFile = new FileInfo("StandardSonograms\\BAC2_20071008_AmplSonogramData.EXPECTED.bin");

// run this once to generate expected image and data files (############ IMPORTANT: remember to move saved files OUT of bin/Debug directory!)
bool saveOutput = false;
if (saveOutput)
{
    // 1: save image of oscillation spectrogram
    //tuple.Item1.Save(imagePath, ImageFormat.Png);
}

// run this once to generate expected test data (and remember to copy out of bin/debug!)
    // Binary.Serialize(expectedFile, sonogram.Data);
    //var expected = Binary.Deserialize<double[,]>(expectedFile);

// CollectionAssert.AreEqual(expected, sonogram.Data);
var resultFile2 = new FileInfo(Path.Combine(outputDir.FullName, stemOfActualFile));
Json.Serialise(resultFile2, freqScale.GridLineLocations);
FileEqualityHelpers.TextFileEqual(expectedFile2, resultFile2);

// Check that image dimensions are correct
Assert.AreEqual(645, image.Width);
Assert.AreEqual(310, image.Height);


// DO EQUALITY TEST
Get a DATA_MATRIX
var expectedDataFile = new FileInfo("StandardSonograms\\BAC2_20071008_AmplSonogramData.EXPECTED.bin");

// run this once to generate expected test data (and remember to copy out of bin/debug!)
//Binary.Serialize(expectedFile, DATA_MATRIX);

var expectedDATA = Binary.Deserialize<double[,]>(expectedDataFile);

CollectionAssert.AreEqual(expectedDATA, DATA_MATRIX);
*/

            // Assert.Fail("in progrexss");
        }
    }
}
