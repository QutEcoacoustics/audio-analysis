// <copyright file="NoiseRemovalTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.DSP
{
    using System.IO;
    using Acoustics.Shared;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;
    using TowseyLibrary;

    /// <summary>
    /// Test methods for noise removal from signals and their spectrograms.
    /// Only one test is implemented for Modal noise removal using the method of Lamel et al.
    /// TODO There are several more tests that could be implemented for various methods of noise removal.
    /// See the NoiseProfile class
    /// 
    /// One additional noise removal method is LOCAL CONRAST Normalisation.
    /// LCN over frequency bins is better and faster than standard noise removal.
    ///   double neighbourhoodSeconds = 0.25;
    ///   int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
    ///   double lcnContrastLevel = 0.5; // was previously 0.1
    ///   LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
    ///   LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
    ///   sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);
    /// </summary>
    [TestClass]
    public class NoiseRemovalTests
    {
        private DirectoryInfo outputDirectory;

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

        [TestMethod]
        public void TestStandardNoiseRemoval()
        {
            var recording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "BAC2_20071008-085040.wav"));
            int windowSize = 512;
            var sr = recording.SampleRate;

            // window overlap is used only for sonograms. It is not used when calculating acoustic indices.
            double windowOverlap = 0.0;
            var windowFunction = WindowFunctions.HAMMING.ToString();

            var fftdata = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                windowSize,
                windowOverlap,
                windowFunction);

            // Now recover the data
            // The following data is required when constructing sonograms
            //var duration = recording.WavReader.Time;
            //var frameCount = fftdata.FrameCount;
            //var fractionOfHighEnergyFrames = fftdata.FractionOfHighEnergyFrames;

            double[,] deciBelSpectrogram = MFCCStuff.DecibelSpectra(fftdata.AmplitudeSpectrogram, fftdata.WindowPower, sr, fftdata.Epsilon);

            // The following call to NoiseProfile.CalculateBackgroundNoise(double[,] spectrogram)
            // returns a noise profile that is used as the BGN spectral index.
            // It calculates the modal background noise for each freqeuncy bin and then returns a smoothed version.
            // By default, the number of SDs = 0 and the smoothing window = 7.
            // Method assumes that the passed spectrogram is oriented as: rows=frames, cols=freq bins.</param>

            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(deciBelSpectrogram);

            var resourcesDir = PathHelper.ResolveAssetPath("Indices");
            var expectedSpectrumFile = new FileInfo(resourcesDir + "\\NoiseProfile.bin");

            //Binary.Serialize(expectedSpectrumFile, spectralDecibelBgn);
            var expectedVector = Binary.Deserialize<double[]>(expectedSpectrumFile);
            CollectionAssert.That.AreEqual(expectedVector, spectralDecibelBgn, 0.000_000_001);
        }
    }
}
