// <copyright file="BoobookOwlTest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.PteropusSp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Test.TestHelpers;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.Indices;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;

    /// <summary>
    /// Species name = Ninnox boobook.
    /// </summary>
    [TestClass]
    public class BoobookOwlTest : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this Boobook Owl recognizer is a 31 second recording made by Yvonne Phillips at Gympie National Park, 2015-08-18.
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

            //this.SaveTestOutput(outputDirectory => /* save debug image here */);

            Assert.AreEqual(8, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(1, plots.Count);
            Assert.AreEqual(2667, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[1], typeof(CompositeEvent));

            var secondEvent = (CompositeEvent)events[1];

            Assert.AreEqual(5.38, secondEvent.EventStartSeconds, 0.05);
            Assert.AreEqual(6.07, secondEvent.EventEndSeconds, 0.05);
            Assert.AreEqual(483, secondEvent.LowFrequencyHertz);
            Assert.AreEqual(735, secondEvent.HighFrequencyHertz);
            Assert.AreEqual(20.90, secondEvent.Score, 0.05);
            Assert.AreEqual(0.208, secondEvent.ScoreNormalized, 0.05);
        }
    }
}