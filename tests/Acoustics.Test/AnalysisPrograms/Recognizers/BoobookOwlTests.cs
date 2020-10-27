// <copyright file="BoobookOwlTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.SourcePreparers;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Species name = Boobook Owl = Ninnox boobook.
    /// </summary>
    [TestClass]
    public class BoobookOwlTests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer is a 31 second recording made by Yvonne Phillips at Gympie National Park, 2015-08-18.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "gympie_np_1192_331618_20150818_054959_31_0.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.NinoxBoobook.yml");
        private static readonly AudioRecording Recording = new AudioRecording(TestAsset);
        private static readonly NinoxBoobook Recognizer = new NinoxBoobook();

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

            Assert.AreEqual(8, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(3, plots.Count);
            Assert.AreEqual(2667, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[1], typeof(CompositeEvent));

            var secondEvent = (CompositeEvent)events[1];

            Assert.AreEqual(5.375419501133787, secondEvent.EventStartSeconds);
            Assert.AreEqual(6.1648979591836737, secondEvent.EventEndSeconds);
            Assert.AreEqual(441, secondEvent.LowFrequencyHertz);
            Assert.AreEqual(735, secondEvent.HighFrequencyHertz);
            Assert.AreEqual(18.753868577301958, secondEvent.Score, TestHelper.AllowedDelta);
            Assert.AreEqual(0.34061938340960607, secondEvent.ScoreNormalized, TestHelper.AllowedDelta);
        }
    }
}