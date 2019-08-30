// <copyright file="PteropusSpTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.PteropusSp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.EventStatistics;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestHelpers;

    [TestClass]
    public class PteropusSpTests
    {
        [TestMethod]
        public void TestGetWingBeatEvents()
        {
            int sampleRate = 22050;
            double duration = 28;
            int[] harmonics1 = { 500 };
            int[] harmonics2 = { 500, 1000, 2000, 4000, 8000 };
            var signal1 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1, WaveType.Sine);
            var signal2 = DspFilters.GenerateTestSignal(sampleRate, 4, harmonics2, WaveType.Sine);
            var signal3 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1, WaveType.Sine);

            var signal = DataTools.ConcatenateVectors(signal1, signal2, signal3);
            var wr = new WavReader(signal, 1, 16, sampleRate);
            var recording = new AudioRecording(wr);

            // this value is fake, but we set it to ensure output values are calculated correctly w.r.t. segment start
            var segmentOffset = 547.123.Seconds();

            var start = TimeSpan.FromSeconds(28) + segmentOffset;
            var end = TimeSpan.FromSeconds(32) + segmentOffset;
            double lowFreq = 1500.0;
            double topFreq = 8500.0;

            var statsConfig = new EventStatisticsConfiguration()
            {
                FrameSize = 512,
                FrameStep = 512,
            };

            EventStatistics stats =
                EventStatisticsCalculate.AnalyzeAudioEvent(
                    recording,
                    (start, end).AsRange(),
                    (lowFreq, topFreq).AsRange(),
                    statsConfig,
                    segmentOffset);

            LoggedConsole.WriteLine($"Stats: Temporal entropy = {stats.TemporalEnergyDistribution:f4}");
            LoggedConsole.WriteLine($"Stats: Spectral entropy = {stats.SpectralEnergyDistribution:f4}");
            LoggedConsole.WriteLine($"Stats: Spectral centroid= {stats.SpectralCentroid}");
            LoggedConsole.WriteLine($"Stats: DominantFrequency= {stats.DominantFrequency}");

            Assert.AreEqual(0.0, stats.TemporalEnergyDistribution, 1E-4);
            Assert.AreEqual(0.6062, stats.SpectralEnergyDistribution, 1E-4);
            Assert.AreEqual(6687, stats.SpectralCentroid);
            Assert.AreEqual(8003, stats.DominantFrequency);

            Assert.AreEqual(1500, stats.LowFrequencyHertz);
            Assert.AreEqual(8500, stats.HighFrequencyHertz);
            Assert.AreEqual(28.Seconds() + segmentOffset, stats.EventStartSeconds.Seconds());
            Assert.AreEqual(32.Seconds() + segmentOffset, stats.EventEndSeconds.Seconds());
            Assert.AreEqual(28.Seconds() + segmentOffset, stats.ResultStartSeconds.Seconds());
        }

        [TestMethod]
        public void TestGetEventsAroundMaxima()
        {
            int sampleRate = 22050;
            double duration = 28;
            int[] harmonics1 = { 500 };
            int[] harmonics2 = { 500, 1000, 2000, 4000, 8000 };
            var signal1 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1, WaveType.Sine);
            var signal2 = DspFilters.GenerateTestSignal(sampleRate, 4, harmonics2, WaveType.Sine);
            var signal3 = DspFilters.GenerateTestSignal(sampleRate, duration, harmonics1, WaveType.Sine);

            var signal = DataTools.ConcatenateVectors(signal1, signal2, signal3);
            var wr = new WavReader(signal, 1, 16, sampleRate);
            var recording = new AudioRecording(wr);

            // this value is fake, but we set it to ensure output values are calculated correctly w.r.t. segment start
            var segmentOffset = 547.123.Seconds();

            var start = TimeSpan.FromSeconds(28) + segmentOffset;
            var end = TimeSpan.FromSeconds(32) + segmentOffset;
            double lowFreq = 1500.0;
            double topFreq = 8500.0;

            var statsConfig = new EventStatisticsConfiguration()
            {
                FrameSize = 512,
                FrameStep = 512,
            };

            EventStatistics stats =
                EventStatisticsCalculate.AnalyzeAudioEvent(
                    recording,
                    (start, end).AsRange(),
                    (lowFreq, topFreq).AsRange(),
                    statsConfig,
                    segmentOffset);

            LoggedConsole.WriteLine($"Stats: Temporal entropy = {stats.TemporalEnergyDistribution:f4}");
            LoggedConsole.WriteLine($"Stats: Spectral entropy = {stats.SpectralEnergyDistribution:f4}");
            LoggedConsole.WriteLine($"Stats: Spectral centroid= {stats.SpectralCentroid}");
            LoggedConsole.WriteLine($"Stats: DominantFrequency= {stats.DominantFrequency}");

            Assert.AreEqual(0.0, stats.TemporalEnergyDistribution, 1E-4);
            Assert.AreEqual(0.6062, stats.SpectralEnergyDistribution, 1E-4);
            Assert.AreEqual(6687, stats.SpectralCentroid);
            Assert.AreEqual(8003, stats.DominantFrequency);

            Assert.AreEqual(1500, stats.LowFrequencyHertz);
            Assert.AreEqual(8500, stats.HighFrequencyHertz);
            Assert.AreEqual(28.Seconds() + segmentOffset, stats.EventStartSeconds.Seconds());
            Assert.AreEqual(32.Seconds() + segmentOffset, stats.EventEndSeconds.Seconds());
            Assert.AreEqual(28.Seconds() + segmentOffset, stats.ResultStartSeconds.Seconds());
        }
    }
}
