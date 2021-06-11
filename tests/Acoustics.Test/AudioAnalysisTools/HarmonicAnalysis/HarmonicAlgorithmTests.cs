// <copyright file="HarmonicAnalysisTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.HarmonicAnalysis
{
    using System;
    using System.IO;
    using System.Linq;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HarmonicAlgorithmTests : OutputDirectoryTest
    {
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("harmonic.wav");
        private readonly SpectrogramStandard spectrogram;

        private readonly double threshold = 6.0;
        private readonly NoiseReductionType noiseReductionType = NoiseReductionType.Standard;

        //private double threshold = -80;
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
                MinHertz = 400,
                MaxHertz = 6000,
                MaxFormantGap = 550,
                MinFormantGap = 300,
                MinDuration = 0.9,
                MaxDuration = 1.1,
                DecibelThresholds = new double?[] { this.threshold },
                DctThreshold = 0.5,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                this.threshold,
                TimeSpan.Zero,
                "440_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(SpectralEvent));
            //Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // first harmonic is 440Hz fundamental, with 12 harmonics, stopping at 5280 Hz
            var actual = events.First() as SpectralEvent;
            //var actual = events.First() as HarmonicEvent;

            //The actual bounds are not exact due to smoothing of the score array.
            Assert.AreEqual(1.0, actual.EventStartSeconds, 0.1);
            Assert.AreEqual(2.0, actual.EventEndSeconds, 0.1);
            Assert.AreEqual(400, actual.LowFrequencyHertz);
            Assert.AreEqual(6000, actual.HighFrequencyHertz);

            Assert.Fail("intentionally faulty test");
        }

        [TestMethod]
        public void TestHarmonicsAlgorithmOn1000HertzHarmonic()
        {
            var parameters = new HarmonicParameters
            {
                MinHertz = 400,
                MaxHertz = 6000,
                MaxFormantGap = 1100,
                MinFormantGap = 950,
                MinDuration = 0.9,
                MaxDuration = 1.1,
                DecibelThresholds = new double?[] { this.threshold },
                DctThreshold = 0.3,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                this.threshold,
                TimeSpan.Zero,
                "1000_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // second harmonic is 1000 Hz fundamental, with 4 harmonics, stopping at 5000 Hz
            var actual = events.First() as HarmonicEvent;
            Assert.AreEqual(3.0, actual.EventStartSeconds, 0.1);
            Assert.AreEqual(4.0, actual.EventEndSeconds, 0.1);
            Assert.AreEqual(400, actual.LowFrequencyHertz);
            Assert.AreEqual(6000, actual.HighFrequencyHertz);

            Assert.Fail("intentionally faulty test");
        }
    }
}