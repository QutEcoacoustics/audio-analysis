// <copyright file="PteropusSpTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.PteropusSp
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
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

        /// <summary>
        /// The one-minute recording used for these tests was originally recorded at 40kHz in two channels.
        /// It was resampled to 22050 in one channel using Audacity for these tests.
        /// The number of wing-beat and call events is somewhat sensitive to parameter settings.
        /// With test settings get and extra call event.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
            this.audioRecording = new AudioRecording(PathHelper.ResolveAsset("Recordings", "20190115_Bellingen_Feeding_minute6_OneChannel22050.wav"));
            var sonoConfig = new SonogramConfig
            {
                WindowSize = 512,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 3.0,
                WindowOverlap = 0.0,
            };
            this.sonogram = new SpectrogramStandard(sonoConfig, this.audioRecording.WavReader);
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
            int minHz = 200;
            int maxHz = 2000;
            double minDurationSeconds = 1.0;
            double maxDurationSeconds = 10.0;
            double dctDuration = 0.8;
            double dctThreshold = 0.5;
            double minOscilFreq = 4.0;
            double maxOscilFreq = 6.0;
            double eventThreshold = 0.6;
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

            Assert.AreEqual(4, acousticEvents.Count);

            Assert.AreEqual(4, acousticEvents[0].Oblong.ColumnLeft);
            Assert.AreEqual(47, acousticEvents[0].Oblong.ColumnRight);
            Assert.AreEqual(1280, acousticEvents[0].Oblong.RowTop);
            Assert.AreEqual(1380, acousticEvents[0].Oblong.RowBottom);

            Assert.AreEqual(4, acousticEvents[1].Oblong.ColumnLeft);
            Assert.AreEqual(47, acousticEvents[1].Oblong.ColumnRight);
            Assert.AreEqual(1762, acousticEvents[1].Oblong.RowTop);
            Assert.AreEqual(1825, acousticEvents[1].Oblong.RowBottom);

            Assert.AreEqual(4, acousticEvents[2].Oblong.ColumnLeft);
            Assert.AreEqual(47, acousticEvents[2].Oblong.ColumnRight);
            Assert.AreEqual(2083, acousticEvents[2].Oblong.RowTop);
            Assert.AreEqual(2207, acousticEvents[2].Oblong.RowBottom);

            Assert.AreEqual(4, acousticEvents[3].Oblong.ColumnLeft);
            Assert.AreEqual(47, acousticEvents[3].Oblong.ColumnRight);
            Assert.AreEqual(2334, acousticEvents[3].Oblong.RowTop);
            Assert.AreEqual(2382, acousticEvents[3].Oblong.RowBottom);

            //Assert.AreEqual(0.6062, stats.SpectralEnergyDistribution, 1E-4);
        }

        [TestMethod]
        public void TestGetEventsAroundMaxima()
        {
            //string abbreviatedSpeciesName = "Pteropus";
            string speciesName = "Pteropus species";
            int minHz = 800;
            int maxHz = 8000;
            var minTimeSpan = TimeSpan.FromSeconds(0.15);
            var maxTimeSpan = TimeSpan.FromSeconds(0.8);
            double decibelThreshold = 9.0;
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

            Assert.AreEqual(10, acousticEvents.Count);

            Assert.AreEqual(new Rectangle(19, 1751, 168, 27), acousticEvents[0].GetEventAsRectangle());
            Assert.AreEqual(new Rectangle(19, 1840, 168, 10), acousticEvents[2].GetEventAsRectangle());
            Assert.AreEqual(new Rectangle(19, 1961, 168, 31), acousticEvents[5].GetEventAsRectangle());
            Assert.AreEqual(new Rectangle(19, 2294, 168, 17), acousticEvents[7].GetEventAsRectangle());
            Assert.AreEqual(new Rectangle(19, 2504, 168, 7), acousticEvents[9].GetEventAsRectangle());

            //Assert.AreEqual(28.Seconds() + segmentOffset, stats.ResultStartSeconds.Seconds());
        }
    }
}
