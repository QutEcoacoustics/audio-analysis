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
        //private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "koala.wav");
        private static readonly FileInfo TestAsset = new FileInfo("C:\\Ecoacoustics\\WavFiles\\KoalaMale\\Jackaroo_20080715-103940.wav");
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

            Assert.AreEqual(12, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(2, plots.Count);
            //Assert.AreEqual(938, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[0], typeof(OscillationEvent));

            var ev = (SpectralEvent)events[0];

            Assert.AreEqual(5.12, ev.EventStartSeconds);
            Assert.AreEqual(12.26, ev.EventEndSeconds);
            Assert.AreEqual(105, ev.LowFrequencyHertz);
            Assert.AreEqual(180, ev.HighFrequencyHertz);
            Assert.AreEqual(21.7, ev.Score);
            Assert.AreEqual(0.95, ev.ScoreNormalized);
        }
    }
}