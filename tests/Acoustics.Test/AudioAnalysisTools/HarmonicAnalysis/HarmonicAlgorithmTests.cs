// <copyright file="HarmonicAnalysisTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.HarmonicAnalysis
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.Csv;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HarmonicAlgorithmTests : OutputDirectoryTest
    {
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("harmonic.wav");
        private readonly SpectrogramStandard spectrogram;

        private readonly double decibelThreshold = 6.0;
        private readonly NoiseReductionType noiseReductionType = NoiseReductionType.Standard;

        // can also try these parameters when testing.
        //private readonly double decibelThreshold = -80;
        //private readonly NoiseReductionType noiseReductionType = NoiseReductionType.None;

        public HarmonicAlgorithmTests()
        {
            var recording = new AudioRecording(TestAsset);
            this.spectrogram = new SpectrogramStandard(
                new SonogramConfig
                {
                    WindowSize = 512,
                    WindowStep = 512,
                    WindowOverlap = 0,
                    NoiseReductionType = this.noiseReductionType,
                    NoiseReductionParameter = 0.0,
                    Duration = recording.Duration,
                    SampleRate = recording.SampleRate,
                },
                recording.WavReader);
        }

        [TestMethod]
        public void TestHarmonicsAlgorithmOn440HertzHarmonic()
        {
            var parameters = new HarmonicParameters
            {
                // Here is option to smooth the frequency bins. Can help with harmonic detection.
                SmoothingWindow = 5,
                MinHertz = 400,
                MaxHertz = 5500,
                MaxFormantGap = 470,
                MinFormantGap = 420,

                //Need to make allowance for a longer than actual duration.
                //because of smoothing of the spectrogram frames prior to the auto-crosscorrelation.
                MinDuration = 1.0,
                MaxDuration = 1.16,
                DecibelThresholds = new double?[] { this.decibelThreshold },
                DctThreshold = 0.5,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                this.decibelThreshold,
                TimeSpan.Zero,
                "440_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null, "440_harmonic"));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // first harmonic is 440Hz fundamental, with 12 harmonics, stopping at 5280 Hz
            var actual = events.First() as HarmonicEvent;

            //The actual bounds are not exact due to smoothing of the frames prior to the auto-crosscorrelation
            // that occurs in CrossCorrelation.DetectHarmonicsInSpectrogramData()
            Assert.AreEqual(1.0, actual.EventStartSeconds, 0.1);
            Assert.AreEqual(2.0, actual.EventEndSeconds, 0.1);
            Assert.AreEqual(400, actual.LowFrequencyHertz);
            Assert.AreEqual(5500, actual.HighFrequencyHertz);
            Assert.AreEqual(440, actual.HarmonicInterval, 30);
        }

        [TestMethod]
        public void TestHarmonicsAlgorithmOn1000HertzHarmonic()
        {
            var parameters = new HarmonicParameters
            {
                MinHertz = 400,
                MaxHertz = 5500,
                MaxFormantGap = 1100,
                MinFormantGap = 950,
                MinDuration = 0.95,
                MaxDuration = 1.2,
                DecibelThresholds = new double?[] { this.decibelThreshold },
                DctThreshold = 0.3,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                this.decibelThreshold,
                TimeSpan.Zero,
                "1000_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null, "1000_harmonic"));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // second harmonic is 1000 Hz fundamental, with 4 harmonics, stopping at 5000 Hz
            var actual = events.First() as HarmonicEvent;
            Assert.AreEqual(3.0, actual.EventStartSeconds, 0.1);
            Assert.AreEqual(4.0, actual.EventEndSeconds, 0.1);
            Assert.AreEqual(400, actual.LowFrequencyHertz);
            Assert.AreEqual(5500, actual.HighFrequencyHertz);
            Assert.AreEqual(1000, actual.HarmonicInterval, 80);
        }

        [TestMethod]
        public void TestCosinesMatrixForDct()
        {
            // get an 8 x 8 matrix.
            double[,] cosineBasisFunctions = MFCCStuff.Cosines(8, 8);

            //following line writes matrix of cos values for checking.
            var outputDir = new FileInfo(Path.Join(this.TestOutputDirectory.FullName, "Cosines.csv"));
            Csv.WriteMatrixToCsv<double>(outputDir, cosineBasisFunctions);

            //following line writes bmp image of cos values for checking.
            var image = ImageTools.DrawMatrix(cosineBasisFunctions, true);
            this.SaveImage(image, "Cosines.png");

            Assert.AreEqual(9, cosineBasisFunctions.GetLength(0));
            Assert.AreEqual(8, cosineBasisFunctions.GetLength(1));
            Assert.AreEqual(0.70710678118654768, cosineBasisFunctions[4, 4]);
        }
    }
}