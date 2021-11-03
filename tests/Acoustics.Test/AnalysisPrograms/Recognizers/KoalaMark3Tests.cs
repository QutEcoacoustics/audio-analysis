// <copyright file="KoalaMark3Tests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static global::AnalysisPrograms.Recognizers.PhascolarctosCinereusMark3;

    // [Ignore("Currently failing but also this work has not yet been implemented")]

    /// <summary>
    /// Species name = Koala = Botaurus poiciloptilus.
    /// Recognizer class = Phascolarctos cinereus.cs.
    /// </summary>
    [TestClass]
    public class KoalaMark3Tests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer is #############################.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "HoneymoonBay_StBees_20080905-001000.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.PhascolarctosCinereusMark3.yml");
        private static readonly PhascolarctosCinereusMark3 Recognizer = new PhascolarctosCinereusMark3();

        //NOTE: If testing recording at its original sample-rate, then use line below.
        //private static readonly AudioRecording Recording = new AudioRecording(TestAsset);

        // If needing to resample, then must call AudioRecording.GetAudioRecording(TestAsset, resampleRate, opDir, opFileName);
        //  as in the TestRecognizer() method below.

        [TestMethod]
        public void TestKoala3Recognizer()
        {
            var config = Recognizer.ParseConfig(ConfigFile);
            int resampleRate = config.ResampleRate.Value;
            string opDir = this.TestOutputDirectory.FullName;
            string opFileName = "KoalaTempFile";
            var recording = AudioRecording.GetAudioRecording(TestAsset, resampleRate, opDir, opFileName);

            var results = Recognizer.Recognize(
                audioRecording: recording,
                configuration: config,
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

            Assert.AreEqual(3, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(3, plots.Count);
            //Assert.AreEqual(5888, sonogram.FrameCount);
            Assert.AreEqual(12025, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[0], typeof(CompositeEvent));

            var ev = (CompositeEvent)events[1];
            Assert.AreEqual(73.9, ev.EventStartSeconds, 0.05);
            Assert.AreEqual(98.1, ev.EventEndSeconds, 0.05);
            Assert.AreEqual(170, ev.LowFrequencyHertz);
            Assert.AreEqual(800, ev.HighFrequencyHertz);

            Assert.AreEqual(14, ev.ComponentEvents.Count);
            var componentEv = ev.ComponentEvents[6];
            Assert.IsInstanceOfType(componentEv, typeof(OscillationEvent));

            Assert.AreEqual(0.71, componentEv.Score, 0.05);

            var oscEv = (OscillationEvent)componentEv;
            Assert.AreEqual(1.3, oscEv.EventDurationSeconds, 0.05);
            Assert.AreEqual(0.0248, oscEv.Periodicity, 0.005);
            Assert.AreEqual(40.24, oscEv.OscillationRate, 0.1);
        }
    }
}