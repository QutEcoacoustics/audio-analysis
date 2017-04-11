// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Oscillations2014
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using EcoSounds.Mvc.Tests;
    using global::AudioAnalysisTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Test for drawing of Oscillation Spectrogram
    /// </summary>
    [TestClass]
    public class OscillationTests
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
        /// Test for drawing of two Oscillation Spectrograms using different algorithms
        /// </summary>
        [TestMethod]
        public void TwoOscillationTests()
        {
            {
                var sourceRecording = @"Recordings\BAC2_20071008-085040.wav".ToFileInfo();
                var outputDir = @"Oscillation2014\temp".ToDirectoryInfo();
                var configFile = @"Oscillations2014\Towsey.Sonogram.yml".ToFileInfo();

                // 1. get the config dictionary
                var configDict = Oscillations2014.GetConfigDictionary(configFile, true);
                configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
                configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

                // 2. Create temp directory to store output
                if (!this.outputDirectory.Exists) this.outputDirectory.Create();

                // 3. Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
                var tuple = Oscillations2014.GenerateOscillationDataAndImages(sourceRecording, configDict, true);

                // (1) Save image file of this matrix.
                // Sample length i.e. number of frames spanned to calculate oscillations per second
                int sampleLength = Oscillations2014.DefaultSampleLength;
                if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
                {
                    sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);
                }

                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                string fileName = sourceName + ".FreqOscilSpectrogram_" + sampleLength;
                string pathName = Path.Combine(outputDir.FullName, fileName);
                string imagePath = pathName + ".png";
                tuple.Item1.Save(imagePath, ImageFormat.Png);

                // construct output file names
                fileName = sourceName + ".FreqOscilDataMatrix_" + sampleLength;
                pathName = Path.Combine(outputDir.FullName, fileName);
                var csvFile1 = new FileInfo(pathName + ".csv");

                fileName = sourceName + ".OSCSpectralIndex_" + sampleLength;
                pathName = Path.Combine(outputDir.FullName, fileName);
                var csvFile2 = new FileInfo(pathName + ".csv");

                // Save matrix of oscillation data stored in freqOscilMatrix1
                Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(csvFile1, tuple.Item2);

                double[] oscillationsSpectrum = tuple.Item3;
                Acoustics.Shared.Csv.Csv.WriteToCsv(csvFile2, oscillationsSpectrum);

                // var expectedFile = new FileInfo("StandardSonograms\\BAC2_20071008_AmplSonogramData.EXPECTED.bin");

                // run this once to generate expected test data (and remember to copy out of bin/debug!)
                // Binary.Serialize(expectedFile, sonogram.Data);
                // var expected = Binary.Deserialize<double[,]>(expectedFile);

                // CollectionAssert.AreEqual(expected, sonogram.Data);
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
        }
    }
}
