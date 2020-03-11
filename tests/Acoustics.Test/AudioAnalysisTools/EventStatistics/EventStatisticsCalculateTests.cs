// <copyright file="EventStatisticsCalculateTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AudioAnalysisTools.EventStatistics
{
    using System;
    using Acoustics.Shared;
    using Acoustics.Tools.Wav;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.EventStatistics;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EventStatisticsCalculateTests
    {

        [TestMethod]
        public void TestCalculateEventStatistics()
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

            /*
            // Assume linear scale.
            int nyquist = sampleRate / 2;
            var freqScale = new FrequencyScale(nyquist: nyquist, frameSize: statsConfig.FrameSize, hertzLinearGridInterval: 1000);

            var sonoConfig = new SonogramConfig
            {
                WindowSize = statsConfig.FrameSize,
                WindowStep = statsConfig.FrameSize,
                WindowOverlap = 0.0,
                SourceFName = "SineSignal3",
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.12,
            };
            var sonogram = new AmplitudeSonogram(sonoConfig, recording.WavReader);
            var image = sonogram.GetImage();
            string title = $"Spectrogram of Harmonics: SR={sampleRate}  Window={freqScale.WindowSize}";
            image = sonogram.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            string path = ;
            image.Save(path);

            // get spectrum from row 1300
            var normalisedIndex = DataTools.normalise(MatrixTools.GetRow(sonogram.Data, 1300));
            var image2 = GraphsAndCharts.DrawGraph("SPECTRUM", normalisedIndex, 100);
            string path2 = ;
            image2.Save(path2);
            */
        }
    }
}
