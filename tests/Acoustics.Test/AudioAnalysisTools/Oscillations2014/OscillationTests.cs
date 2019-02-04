// <copyright file="OscillationTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.Oscillations2014
{
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    /// <summary>
    /// Test for drawing of Oscillation Spectrogram
    /// </summary>
    [TestClass]
    public class OscillationTests
    {
        private DirectoryInfo outputDirectory;

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
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        /// <summary>
        /// Test for drawing of two Oscillation Spectrograms using different algorithms
        /// </summary>
        [TestMethod]
        // [Ignore("The update from 864f7a491e2ea0e938161bd390c1c931ecbdf63c possibly broke this test and I do not know how to repair it.
        // TODO @towsey")]
        public void TwoOscillationTests()
        {
            {
                var sourceRecording = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");

                // 1. get the config dictionary
                var configDict = Oscillations2014.GetDefaultConfigDictionary(sourceRecording);

                // 2. Create temp directory to store output
                if (!this.outputDirectory.Exists)
                {
                    this.outputDirectory.Create();
                }

                // 3. Generate the FREQUENCY x OSCILLATIONS Graphs and csv data
                var tuple = Oscillations2014.GenerateOscillationDataAndImages(sourceRecording, configDict, true);

                // construct name of expected image file to save
                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                var stem = sourceName + ".FreqOscilSpectrogram";

                // construct name of expected matrix osc spectrogram to save file
                var expectedMatrixFile = PathHelper.ResolveAsset("Oscillations2014", stem + ".Matrix.EXPECTED.csv");

                // SAVE THE OUTPUT if true
                // WARNING: this will overwrite fixtures
                if (false)
                {
                    // 1: save image of oscillation spectrogram
                    string imageName = stem + ".EXPECTED.png";
                    string imagePath = Path.Combine(PathHelper.ResolveAssetPath("Oscillations2014"), imageName);
                    tuple.Item1.Save(imagePath, ImageFormat.Png);

                    // 2: Save matrix of oscillation data stored in freqOscilMatrix1
                    //Csv.WriteMatrixToCsv(expectedMatrixFile, tuple.Item2);
                    Binary.Serialize(expectedMatrixFile, tuple.Item2);
                }

                // Run two tests. Have to deserialise the expected data files
                // 1: Compare image files - check that image dimensions are correct
                Assert.AreEqual(350, tuple.Item1.Width);
                Assert.AreEqual(675, tuple.Item1.Height);

                // 2. Compare matrix data
                var expectedMatrix = Binary.Deserialize<double[,]>(expectedMatrixFile);

                //TODO  Following test fails when using CSV reader because the reader cuts out first line of the matrix
                //var expectedMatrix = Csv.ReadMatrixFromCsv<double>(expectedMatrixFile);
                CollectionAssert.That.AreEqual(expectedMatrix, tuple.Item2, 0.000001);
            }
        }

        [TestMethod]
        public void SpectralIndexOsc_Test()
        {
            {
                var sourceRecording = PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav");

                // 1. Create temp directory to store output
                if (!this.outputDirectory.Exists)
                {
                    this.outputDirectory.Create();
                }

                // 2. Get the spectral index
                var recordingSegment = new AudioRecording(sourceRecording.FullName);
                var frameLength = Oscillations2014.DefaultFrameLength;
                var sampleLength = Oscillations2014.DefaultSampleLength;
                var threshold = Oscillations2014.DefaultSensitivityThreshold;
                var spectralIndex = Oscillations2014.GetSpectralIndex_Osc(recordingSegment, frameLength, sampleLength, threshold);

                // 3. construct name of spectral index vector
                // SAVE THE OUTPUT if true
                // WARNING: this will overwrite fixtures
                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                var stem = sourceName + ".SpectralIndex.OSC";
                var expectedIndexPath = PathHelper.ResolveAsset("Oscillations2014", stem + ".EXPECTED.csv");
                if (false)
                {
                    // 4. Save spectral index vector to file
                    //Csv.WriteToCsv(expectedIndexPath, spectralIndex);
                    //Json.Serialise(expectedIndexPath, spectralIndex);
                    Binary.Serialize(expectedIndexPath, spectralIndex);

                    // 5. Get the vector as image and save as image file
                    // no need to do tests on this image but it is useful to visualise output
                    var expectedVectorImage = ImageTools.DrawVectorInColour(DataTools.reverseArray(spectralIndex), cellWidth: 10);
                    var expectedImagePath = PathHelper.ResolveAsset("Oscillations2014", stem + ".png");
                    expectedVectorImage.Save(expectedImagePath.FullName, ImageFormat.Png);
                }

                // 6. Get the vector as image and save as image file
                // no need to do tests on this image but it is useful to compare with expected visual output
                var currentVectorImage = ImageTools.DrawVectorInColour(DataTools.reverseArray(spectralIndex), cellWidth: 10);
                var currentImagePath = Path.Combine(this.outputDirectory.FullName, stem + ".png");
                currentVectorImage.Save(currentImagePath, ImageFormat.Png);

                // 7. Run test. Compare vectors
                // TODO  this test fails when using CSV reader because the reader cuts out first element/line of the vector
                //var expectedVector = (double[])Csv.ReadFromCsv<double>(expectedIndexPath);
                var expectedVector = Binary.Deserialize<double[]>(expectedIndexPath);
                CollectionAssert.That.AreEqual(expectedVector, spectralIndex, 0.000001);
            }
        }
    }
}
