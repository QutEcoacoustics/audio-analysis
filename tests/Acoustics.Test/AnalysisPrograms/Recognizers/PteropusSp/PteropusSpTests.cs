// <copyright file="PteropusSpTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.PteropusSp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AnalysisPrograms.Recognizers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.EventStatistics;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PteropusSpTests
    {
        private DirectoryInfo outputDirectory;
        private AudioRecording audioRecording;
        private BaseSonogram sonogram;

        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
            this.audioRecording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "20190115_Bellingen_Feeding_minute6.wav"));
            var sonoConfig = new SonogramConfig
            {
                WindowSize = 512,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                WindowOverlap = 0.0,
            };
            this.sonogram = (BaseSonogram)new SpectrogramStandard(sonoConfig, this.audioRecording.WavReader);
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }

        [TestMethod]
        public void TestGetWingBeatEvents()
        {

            //string speciesName = "Pteropus species";
            //string abbreviatedSpeciesName = "Pteropus";
            double minDurationSeconds = 1.0;
            double maxDurationSeconds = 10.0;
            double dctDuration = 1.0;
            double dctThreshold = 0.5;
            double minOscilFreq = 4.0;
            double maxOscilFreq = 6.0;
            double eventThreshold = 0.3;
            int minHz = 100;
            int maxHz = 3000;
            TimeSpan segmentStartOffset = TimeSpan.Zero;

            // Look for wing beats using oscillation detector
            Oscillations2012.Execute(
                (SpectrogramStandard)this.sonogram,
                minHz,
                maxHz,
                dctDuration,
                (int)Math.Floor(minOscilFreq),
                (int)Math.Floor(maxOscilFreq),
                dctThreshold,
                eventThreshold,
                minDurationSeconds,
                maxDurationSeconds,
                out var scores,
                out var acousticEvents,
                out var hits,
                segmentStartOffset);

            //LoggedConsole.WriteLine($"Stats: Temporal entropy = {stats.TemporalEnergyDistribution:f4}");
            //LoggedConsole.WriteLine($"Stats: Spectral entropy = {stats.SpectralEnergyDistribution:f4}");
            //LoggedConsole.WriteLine($"Stats: Spectral centroid= {stats.SpectralCentroid}");
            //LoggedConsole.WriteLine($"Stats: DominantFrequency= {stats.DominantFrequency}");

            //Assert.AreEqual(0.0, stats.TemporalEnergyDistribution, 1E-4);
            //Assert.AreEqual(0.6062, stats.SpectralEnergyDistribution, 1E-4);
            //Assert.AreEqual(6687, stats.SpectralCentroid);
            //Assert.AreEqual(8003, stats.DominantFrequency);
        }

        [TestMethod]
        public void TestGetEventsAroundMaxima()
        {
            //string abbreviatedSpeciesName = "Pteropus";
            string speciesName = "Pteropus species";
            var minTimeSpan = TimeSpan.FromSeconds(0.1);
            var maxTimeSpan = TimeSpan.FromSeconds(10.0);
            double decibelThreshold = 0.5;
            int minHz = 100;
            int maxHz = 3000;
            TimeSpan segmentStartOffset = TimeSpan.Zero;

            var decibelArray = SNR.CalculateFreqBandAvIntensity(this.sonogram.Data, minHz, maxHz, this.sonogram.NyquistFrequency);

            // prepare plots
            double intensityNormalisationMax = 3 * decibelThreshold;
            var eventThreshold = decibelThreshold / intensityNormalisationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(decibelArray, 0, intensityNormalisationMax);
            var plot = new Plot(speciesName + " Territory", normalisedIntensityArray, eventThreshold);
            var plots = new List<Plot> { plot };

            //iii: CONVERT decibel SCORES TO ACOUSTIC EVENTS
            var acousticEvents = AcousticEvent.GetEventsAroundMaxima(
                decibelArray,
                segmentStartOffset,
                minHz,
                maxHz,
                decibelThreshold,
                minTimeSpan,
                maxTimeSpan,
                this.sonogram.FramesPerSecond,
                this.sonogram.FBinWidth);

            //LoggedConsole.WriteLine($"Stats: Temporal entropy = {stats.TemporalEnergyDistribution:f4}");
            //LoggedConsole.WriteLine($"Stats: Spectral entropy = {stats.SpectralEnergyDistribution:f4}");
            //LoggedConsole.WriteLine($"Stats: Spectral centroid= {stats.SpectralCentroid}");
            //LoggedConsole.WriteLine($"Stats: DominantFrequency= {stats.DominantFrequency}");

            //Assert.AreEqual(0.0, stats.TemporalEnergyDistribution, 1E-4);
            //Assert.AreEqual(0.6062, stats.SpectralEnergyDistribution, 1E-4);
            //Assert.AreEqual(6687, stats.SpectralCentroid);
            //Assert.AreEqual(8003, stats.DominantFrequency);

            //Assert.AreEqual(1500, stats.LowFrequencyHertz);
            //Assert.AreEqual(8500, stats.HighFrequencyHertz);
            //Assert.AreEqual(28.Seconds() + segmentOffset, stats.EventStartSeconds.Seconds());
            //Assert.AreEqual(32.Seconds() + segmentOffset, stats.EventEndSeconds.Seconds());
            //Assert.AreEqual(28.Seconds() + segmentOffset, stats.ResultStartSeconds.Seconds());
        }
    }
}
