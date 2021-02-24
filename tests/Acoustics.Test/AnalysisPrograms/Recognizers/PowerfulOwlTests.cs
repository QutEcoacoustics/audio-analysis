// <copyright file="PowerfulOwlTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Species name = Powerful Owl = Ninox strenua.
    /// </summary>
    [TestClass]
    public class PowerfulOwlTests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer contains three calls, each with two syllables.
        /// However the calls are loud and two of them are followed by an echo which is picked up as another syllable.
        /// The config file could be adjusted to remove the extra hits,
        /// but the config has been optimised on ten recordings, provided by Kristen Thompson, that contain a variety of loud and soft calls.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "Powerful3AndBoobook0_ksh3_1773_510819_20171109_174311_30_0.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.NinoxStrenua.yml");
        private static readonly AudioRecording Recording = new AudioRecording(TestAsset);
        private static readonly NinoxStrenua Recognizer = new NinoxStrenua();

        [TestMethod]
        public void TestRecognizer()
        {
            var config = Recognizer.ParseConfig(ConfigFile);

            var results = Recognizer.Recognize(
                audioRecording: Recording,
                config: config,
                segmentStartOffset: TimeSpan.Zero,
                getSpectralIndexes: null,
                outputDirectory: this.TestOutputDirectory,
                imageWidth: null);

            var events = results.NewEvents;
            var scoreTrack = results.ScoreTrack;
            var plots = results.Plots;
            var sonogram = results.Sonogram;

            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(results, null, outputDirectory, Recognizer.SpeciesName));

            Assert.AreEqual(6, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(5, plots.Count);
            Assert.AreEqual(2580, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[1], typeof(ChirpEvent));

            //Note: due to combining and filtering of events etc, they are not in temporal order
            var secondEvent = (CompositeEvent)events[5];

            Assert.AreEqual(26.842267573, secondEvent.EventStartSeconds, 1E-06);
            Assert.AreEqual(27.898775510204082, secondEvent.EventEndSeconds, 1E-06);
            Assert.AreEqual(357, secondEvent.LowFrequencyHertz);
            Assert.AreEqual(483, secondEvent.HighFrequencyHertz);
            Assert.AreEqual(47.93789051261686, secondEvent.Score, 1E-06);
            Assert.AreEqual(0.069847040361109447, secondEvent.ScoreNormalized, 1E-06);
        }
    }
}