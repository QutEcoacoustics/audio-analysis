// <copyright file="KoalaV3Tests.cs" company="QutEcoacoustics">
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
    /// Species name = Koala = Botaurus poiciloptilus.
    /// Recognizer class = Phascolarctos cinereus.cs.
    /// </summary>
    [TestClass]
    [Ignore("Currently failing but also this work has not yet been implemented")]
    public class KoalaV3Tests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer is #############################.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "koala.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.PhascolarctosCinereus.v3.yml");
        private static readonly PhascolarctosCinereus Recognizer = new PhascolarctosCinereus();

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
            Assert.AreEqual(1, plots.Count);
            Assert.AreEqual(938, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[0], typeof(CompositeEvent));

            var onlyEvent = (CompositeEvent)events[0];

            Assert.AreEqual(5.12, onlyEvent.EventStartSeconds);
            Assert.AreEqual(12.26, onlyEvent.EventEndSeconds);
            Assert.AreEqual(105, onlyEvent.LowFrequencyHertz);
            Assert.AreEqual(180, onlyEvent.HighFrequencyHertz);
            Assert.AreEqual(21.7, onlyEvent.Score);
            Assert.AreEqual(0.95, onlyEvent.ScoreNormalized);
        }
    }
}