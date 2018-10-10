// <copyright file="SpectralPeakTracking2018Tests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using global::AnalysisPrograms.SpectralPeakTracking;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class SpectralPeakTracking2018Tests
    {
        [TestMethod]
        public void GetPeakBinsIndexTest()
        {
            var inputMatrix = PathHelper.ResolveAsset("SpectralPeakTracking", "matrix1.csv");
            var matrix = Csv.ReadMatrixFromCsv<double>(inputMatrix, TwoDimensionalArray.None);
            int minFreqBin = 7;
            int maxFreqBin = 15;

            int[] expectedPeakBinsIndex =
            {
                7, 11, 12, 14, 14, 14, 14, 6, 6, 6, 6, 6, 6, 6, 6, 7, 7, 9, 10, 10, 12, 14,
                14, 14, 14, 14, 6, 6, 6, 6, 6, 6, 6, 6, 8, 8, 12, 12, 14, 14, 14, 14, 14, 14, 14, 6, 6, 6, 6, 6
            };

            var actualPeakBinsIndex = SpectralPeakTracking2018.GetPeakBinsIndex(matrix, minFreqBin, maxFreqBin);

            for (int i = 0; i < expectedPeakBinsIndex.Length; i++)
            {
                Assert.AreEqual(expectedPeakBinsIndex[i], actualPeakBinsIndex[i]);
            }
        }

        [TestMethod]
        public void FindLocalSpectralPeaksTest()
        {
            var inputMatrix = PathHelper.ResolveAsset("SpectralPeakTracking", "matrix2.csv");
            var matrix = Csv.ReadMatrixFromCsv<double>(inputMatrix, TwoDimensionalArray.None);

            int[][] expectedLocalPeaksIndex = { new[] { 0, 7 }, new[] { 1, 11 } };

            int[] peakBinsIndex = new int[] { 7, 11, 6, 6, 6, 6, 6, 6, 6, 6 };

            int widthMidBand = 4;
            int topBufferSize = 2;
            int bottomBufferSize = 2;
            double threshold = 4.0;

            var actualLocalPeaks = SpectralPeakTracking2018.FindLocalSpectralPeaks(matrix, peakBinsIndex, widthMidBand,
                topBufferSize, bottomBufferSize, threshold).Item1;

            for (int i = 0; i < expectedLocalPeaksIndex.GetLength(0); i++)
            {
                Assert.AreEqual(expectedLocalPeaksIndex[i][0], actualLocalPeaks[i][0]);
                Assert.AreEqual(expectedLocalPeaksIndex[i][1], actualLocalPeaks[i][1]);
            }
        }

        [Ignore]
        [TestMethod]
        public void LocalSpectralPeakTest()
        {
            var configPath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\SpectralPeakTrackingConfig.yml";
            var recordingPath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\JY-(cleaned)-3-Night_Parrot-pair.Western_Qld_downsampled.wav"; //"C:\Users\kholghim\Mahnoosh\Night_parrot\SM16 24 Sep 2018 6.30 am.wav"; //"C:\Users\kholghim\Mahnoosh\Night_parrot\S4A07296_20180419_050023_11-12min.wav"; //"M:\Postdoc\Night_parrot\S4A07296_20180419_050023_Ch1.wav"; //
            var imagePath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\image.bmp"; //image_NP_SM16 24 Sep 2018 6.30 am.bmp";
            var trackImagePath = @"C:\Users\kholghim\Mahnoosh\Night_parrot\trackImage.bmp";

            var configFile = configPath.ToFileInfo();

            if (configFile == null)
            {
                throw new FileNotFoundException("No config file argument provided");
            }
            else if (!configFile.Exists)
            {
                throw new ArgumentException($"Config file {configFile.FullName} not found");
            }

            var configuration = ConfigFile.Deserialize<SpectralPeakTrackingConfig>(configFile);

            var recording = new AudioRecording(recordingPath);

            // get the nyquist value from the recording
            int nyquist = new AudioRecording(recordingPath).Nyquist;
            int frameSize = configuration.FrameWidth;
            double frameOverlap = configuration.FrameOverlap;
            int finalBinCount = 512;
            var hertzPerFreqBin = nyquist / finalBinCount;
            FreqScaleType scaleType = FreqScaleType.Linear;

            var sonoConfig = new SonogramConfig
            {
                WindowSize = frameSize,
                WindowOverlap = frameOverlap,
                //DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                //MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            var amplitudeSpectrogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var energySpectrogram = new EnergySpectrogram(amplitudeSpectrogram);
            var decibelSpectrogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // Noise Reduction to be added
            //var noiseReducedSpectrogram = SNR.NoiseReduce_Standard(energySpectrogram.Data);

            var output = SpectralPeakTracking2018.SpectralPeakTracking(energySpectrogram.Data, configuration.SptSettings, hertzPerFreqBin);

            // draw the local peaks
            double[,] hits = SpectralPeakTracking2018.MakeHitMatrix(energySpectrogram.Data, output.TargetPeakBinsIndex, output.BandIndex);
            var image = SpectralPeakTracking2018.DrawSonogram(decibelSpectrogram, hits);
            image.Save(imagePath, ImageFormat.Bmp);

            // draw spectral tracks
            var trackImage = SpectralPeakTracking2018.DrawTracks(decibelSpectrogram, hits, output.SpecTracks);
            trackImage.Save(trackImagePath, ImageFormat.Bmp);

        }
    }
}
