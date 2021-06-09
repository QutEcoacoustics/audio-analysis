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
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HarmonicAlgorithmTests : OutputDirectoryTest
    {
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("harmonic.wav");
        private readonly SpectrogramStandard spectrogram;

        public HarmonicAlgorithmTests()
        {
            var recording = new AudioRecording(TestAsset);
            this.spectrogram = new SpectrogramStandard(
                new SonogramConfig
                {
                    WindowSize = 512,
                    WindowStep = 512,
                    WindowOverlap = 0,
                    NoiseReductionType = NoiseReductionType.None,
                    NoiseReductionParameter = 0.0,
                    Duration = recording.Duration,
                    SampleRate = recording.SampleRate,
                },
                recording.WavReader);
        }

        [TestMethod]
        public void TestHarmonicsAlgorithmOn440HertzHarmonic()
        {
            var threshold = -80;
            var parameters = new HarmonicParameters
            {
                MinHertz = 400,
                MaxHertz = 5500,
                // expected value
                //MaxFormantGap = 480, 
                MaxFormantGap = 3500,// this is the lowest value that would produce a result, 3400 does not
                MinFormantGap = 400,
                MinDuration = 0.9,
                MaxDuration = 1.1,
                DecibelThresholds = new double?[] { threshold },
                DctThreshold = 0.5,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                threshold,
                TimeSpan.Zero,
                "440_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // first harmonic is 440Hz fundamental, with 12 harmonics, stopping at 5280 Hz
            var actual = events.First() as HarmonicEvent;
            Assert.AreEqual(1.0, actual.EventStartSeconds);
            Assert.AreEqual(2.0, actual.EventEndSeconds);
            Assert.AreEqual(400, actual.LowFrequencyHertz);
            Assert.AreEqual(5400, actual.HighFrequencyHertz);

            Assert.Fail("intentionally faulty test");
        }

        [TestMethod]
        public void TestHarmonicsAlgorithmOn1000HertzHarmonic()
        {
            var threshold = -80;
            var parameters = new HarmonicParameters
            {
                MinHertz = 800,
                MaxHertz = 5500,
                // expected values
                //MaxFormantGap = 1050,
                MaxFormantGap = 3200, // this is the lowest value that would produce a result, 3100 does not
                MinFormantGap = 950,
                MinDuration = 0.9,
                MaxDuration = 1.1,
                DecibelThresholds = new double?[] { threshold },
                DctThreshold = 0.5,
            };
            Assert.That.IsValid(parameters);

            var (events, plots) = HarmonicParameters.GetComponentsWithHarmonics(
                this.spectrogram,
                parameters,
                threshold,
                TimeSpan.Zero,
                "1000_harmonic");

            this.SaveImage(
                SpectrogramTools.GetSonogramPlusCharts(this.spectrogram, events, plots, null));

            Assert.AreEqual(1, events.Count);
            Assert.IsInstanceOfType(events.First(), typeof(HarmonicEvent));

            // second harmonic is 1000 Hz fundamental, with 4 harmonics, stopping at 5000 Hz
            var actual = events.First() as HarmonicEvent;
            Assert.AreEqual(3.0, actual.EventStartSeconds);
            Assert.AreEqual(4.0, actual.EventEndSeconds);
            Assert.AreEqual(900, actual.LowFrequencyHertz);
            Assert.AreEqual(5100, actual.HighFrequencyHertz);

            Assert.Fail("intentionally faulty test");

        }
    }
}