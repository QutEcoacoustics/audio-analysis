// <copyright file="ConcatenationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Oscillations2014
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
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
                var configFile = @"Oscillations2014\Towsey.Sonogram.yml".ToFileInfo();
                var outputDir = this.outputDirectory;

                // 1. get the config dictionary
                var configDict = Oscillations2014.GetConfigDictionary(configFile, true);
                configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
                configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

                // 2. Create temp directory to store output
                if (!this.outputDirectory.Exists) this.outputDirectory.Create();

                // 3. Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
                var tuple = Oscillations2014.GenerateOscillationDataAndImages(sourceRecording, configDict, true);

                // Calculate the sample length i.e. number of frames spanned to calculate oscillations per second
                int sampleLength = Oscillations2014.DefaultSampleLength;
                if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
                {
                    sampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SampleLength]);
                }

                // construct name of expected image file to save
                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                var stem = sourceName + ".FreqOscilSpectrogram_" + sampleLength;
                string imageName = stem + ".EXPECTED.png";
                string imagePath = Path.Combine(outputDir.FullName, imageName);

                // construct name of expected matrix osc spectrogram to save file
                var expectedMatrixFile = new FileInfo("Oscillations2014\\" + stem + ".EXPECTED.bin");

                // construct name of expected matrix osc spectrogram to save file
                var expectedSpectrumFile = new FileInfo("Oscillations2014\\" + stem + ".OSC.EXPECTED.csv");

                // run this once to generate expected image and data files (############ IMPORTANT: remember to move saved files OUT of bin/Debug directory!)
                bool saveOutput = false;
                if (saveOutput)
                {
                    // 1: save image of oscillation spectrogram
                    tuple.Item1.Save(imagePath, ImageFormat.Png);

                    // 2: Save matrix of oscillation data stored in freqOscilMatrix1
                    // Acoustics.Shared.Csv.Csv.WriteMatrixToCsv(expectedMatrixFile.FullName, tuple.Item2);
                    Binary.Serialize(expectedMatrixFile, tuple.Item2);

                    // 3: save oscillationsSpectrum OR the OSC spectral index.
                    // Acoustics.Shared.Csv.Csv.WriteToCsv(spectralFile, tuple.Item3);
                    Json.Serialise(expectedSpectrumFile, tuple.Item3);
                    // Binary.Serialize(expectedSpectrumFile, tuple.Item3);
                }

                // Run three tests. Have to deserialise the expected data files
                // 1: Compare image files - check that image dimensions are correct
                Assert.AreEqual(366, tuple.Item1.Width);
                Assert.AreEqual(678, tuple.Item1.Height);

                // 2. Compare matrix data
                var expectedMatrix = Binary.Deserialize<double[,]>(expectedMatrixFile);
                CollectionAssert.AreEqual(expectedMatrix, tuple.Item2);

                // 3. Compare OSC spectral index
                // var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
                var expectedVector = Json.Deserialise<double[]>(expectedSpectrumFile);
                CollectionAssert.AreEqual(expectedVector, tuple.Item3);
            }
        }
    }
}
