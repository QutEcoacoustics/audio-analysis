// <copyright file="AustBitternTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Species name = Australasian Bittern = Botaurus poiciloptilus.
    /// Recognizer class = BotaurusPoiciloptilus.cs.
    /// </summary>
    [TestClass]
    public class AustBitternTests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer is a 30 second recording made by Liz Znidersic at Medeas Cove, St Helens, 2016-12-17.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "medeas_cove_2-2_1831_471228_20161217_232352_30_0.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.BotaurusPoiciloptilus.yml");
        private static readonly BotaurusPoiciloptilus Recognizer = new BotaurusPoiciloptilus();

        //NOTE: If testing recording at its original sample-rate, then use line below.
        //private static readonly AudioRecording Recording = new AudioRecording(TestAsset);

        // If needing to resample, then must call AudioRecording.GetAudioRecording(TestAsset, resampleRate, opDir, opFileName);
        //  as in the TestRecognizer() method below.

        [TestMethod]
        public void TestRecognizer()
        {
            var config = Recognizer.ParseConfig(ConfigFile);
            int resampleRate = config.ResampleRate.Value;
            string opDir = this.TestOutputDirectory.FullName;
            string opFileName = "tempFile";
            var recording = AudioRecording.GetAudioRecording(TestAsset, resampleRate, opDir, opFileName);

            var results = Recognizer.Recognize(
                audioRecording: recording,
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

            Assert.AreEqual(1, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(3, plots.Count);
            Assert.AreEqual(938, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[0], typeof(CompositeEvent));

            var onlyEvent = (CompositeEvent)events[0];

            //note this event contains four syllables and one echo, therefore five components.
            Assert.AreEqual(5, onlyEvent.ComponentCount);
            Assert.AreEqual(5.12, onlyEvent.EventStartSeconds);
            Assert.AreEqual(12.256, onlyEvent.EventEndSeconds);
            Assert.AreEqual(105, onlyEvent.LowFrequencyHertz);
            Assert.AreEqual(180, onlyEvent.HighFrequencyHertz);
            Assert.AreEqual(21.716400254142027, onlyEvent.Score, TestHelper.AllowedDelta);
            Assert.AreEqual(0.947014602243972, onlyEvent.ScoreNormalized, TestHelper.AllowedDelta);
        }
    }
}
