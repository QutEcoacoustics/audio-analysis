// <copyright file="SpectralPeakTracking2018Tests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
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
            double threshold = 1.0;

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
            var configPath = @"SpectralPeakTrackingConfig.yml";
            var recordingPath = @"SM27 22 Sep 2018 3.30 am.wav";
            var imagePath = @"image_whistle_peaks_1500_3500_100_250_6.bmp";
            //var trackImagePath = @"trackImage.bmp";
            var pathToCsvFile = @"PeakTrackInfo_SM27 22 Sep 2018 3.30 am.csv";

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

            var spectrogramSettings = new SpectrogramSettings()
            {
                WindowSize = frameSize,
                WindowOverlap = frameOverlap,
                //DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                //MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };


            var sonoConfig = new SonogramConfig()
            {
                WindowSize = frameSize,
                WindowOverlap = frameOverlap,
                //DoMelScale = (scaleType == FreqScaleType.Mel) ? true : false,
                //MelBinCount = (scaleType == FreqScaleType.Mel) ? finalBinCount : frameSize / 2,
                NoiseReductionType = NoiseReductionType.None,
            };

            var frameStep = frameSize * (1 - frameOverlap);
            var secondsPerFrame = frameStep / (nyquist * 2);

            //var sonogram = new SpectrogramStandard(sonoConfig, recording.WavReader);
            var amplitudeSpectrogram = new AmplitudeSpectrogram(spectrogramSettings, recording.WavReader);
            var energySpectrogram = new EnergySpectrogram(amplitudeSpectrogram);
            var decibelSpectrogram = new SpectrogramStandard(sonoConfig, recording.WavReader);

            // Noise Reduction
            //var noiseReducedSpectrogram = SNR.NoiseReduce_Standard(energySpectrogram.Data);

            var output = SpectralPeakTracking2018.SpectralPeakTracking(energySpectrogram.Data, configuration.SptSettings, hertzPerFreqBin, secondsPerFrame);

            // draw the local peaks
            double[,] hits = SpectralPeakTracking2018.MakeHitMatrix(energySpectrogram.Data, output.TargetPeakBinsIndex, output.BandIndex);
            var image = SpectralPeakTracking2018.DrawSonogram(decibelSpectrogram, hits);
            image.Save(imagePath, ImageFormat.Bmp);

            string[] header = new[] { "Frame No", "Start Time", "Bin No", "Freq", "Score", "Detection" };
            var csv = new StringBuilder();
            string content = string.Empty;
            foreach (var entry in header.ToArray())
            {
                content += entry.ToString() + ",";
            }

            csv.AppendLine(content);

            foreach (var entry in output.peakTrackInfoList)
            {
                content = string.Empty;
                foreach (var value in entry)
                {
                    content += value.ToString() + ",";
                }

                csv.AppendLine(content);
            }

            File.WriteAllText(pathToCsvFile, csv.ToString());

            //Csv.WriteMatrixToCsv(pathToCsvFile.ToFileInfo(), output.peakTrackInfoList);

            // draw spectral tracks
            //var trackImage = SpectralPeakTracking2018.DrawTracks(decibelSpectrogram, hits, output.SpecTracks);
            //trackImage.Save(trackImagePath, ImageFormat.Bmp);

        }
    }
}
