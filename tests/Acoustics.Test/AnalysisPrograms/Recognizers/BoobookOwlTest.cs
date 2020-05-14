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
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "gympie_np_1192_331618_20150818_054959_31_0.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles\\Towsey.NinoxBoobook.yml");
        private static AudioRecording recording;
        private static Config config;
        private static RecognizerBase recognizer;
        private DirectoryInfo outputDirectory;

        [ClassInitialize]
        /// <summary>
        /// This method is called once.
        /// The canonical recording used for this Boobook Owl recognizer is a 31 second recording made by Yvonne Phillips at Gympie National Park, 2015-08-18.
        /// </summary>
        public static void ClassInitialize(TestContext context)
        {
            recording = new AudioRecording(TestAsset);
            recognizer = new NinoxBoobook();
            config = recognizer.ParseConfig(ConfigFile);
        }

        /// <summary>
        /// This method is called once before running each test in the class.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            this.outputDirectory = PathHelper.GetTempDir();
        }

        [TestMethod]
        public void TestRecognizer()
        {
            TimeSpan segmentStartOffset = TimeSpan.Zero;
            Lazy<IndexCalculateResult[]> getSpectralIndexes = null;
            int? imageWidth = null;
            var results = recognizer.Recognize(recording, config, segmentStartOffset, getSpectralIndexes, this.outputDirectory, imageWidth);
            var events = results.NewEvents;
            var scoreTrack = results.ScoreTrack;
            var plots = results.Plots;
            var sonogram = results.Sonogram;

            Assert.AreEqual(8, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(1, plots.Count);
            Assert.AreEqual(2667, sonogram.FrameCount);

            Assert.AreEqual(typeof(CompositeEvent), events[1].GetType());
            Assert.AreEqual(5.38, events[1].EventStartSeconds, 0.05);
            Assert.AreEqual(6.07, ((CompositeEvent)events[1]).EventEndSeconds, 0.05);
            Assert.AreEqual(483, ((CompositeEvent)events[1]).LowFrequencyHertz);
            Assert.AreEqual(735, ((CompositeEvent)events[1]).HighFrequencyHertz);
            Assert.AreEqual(20.90, ((CompositeEvent)events[1]).Score, 0.05);
            Assert.AreEqual(0.208, ((CompositeEvent)events[1]).ScoreNormalized, 0.05);
        }

        [TestCleanup]
        public void Cleanup()
        {
            PathHelper.DeleteTempDir(this.outputDirectory);
        }
    }
}