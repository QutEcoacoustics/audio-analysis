// <copyright file="PowerfulOwlTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.IO;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AnalysisPrograms.Recognizers;
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
        /// The canonical recording used for this recognizer is at:
        /// "C:\Ecoacoustics\WavFiles\PowerfulOwl_NinoxStrenua\XC269666_PowerfulOwl_NinoxStrenua.mp3".
        /// </summary>
        //private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "gympie_np_1192_331618_20150818_054959_31_0.wav");
        //private static readonly FileInfo TestAsset = new FileInfo(@"C:\Ecoacoustics\WavFiles\PowerfulOwl_NinoxStrenua\XC269666_PowerfulOwl_NinoxStrenua.wav");
        private static readonly FileInfo TestAsset = new FileInfo(@"C:\Ecoacoustics\WavFiles\PowerfulOwl_NinoxStrenua\Powerful3AndBoobook0_ksh3_1773_510819_20171109_174311_30_0.wav");
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

            Assert.AreEqual(3, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(1, plots.Count);
            Assert.AreEqual(4380, sonogram.FrameCount);

            Assert.IsInstanceOfType(events[1], typeof(CompositeEvent));

            var secondEvent = (CompositeEvent)events[1];

            Assert.AreEqual(23.858503401360544, secondEvent.EventStartSeconds, TestHelper.AllowedDelta);
            Assert.AreEqual(27.7362358276644, secondEvent.EventEndSeconds, TestHelper.AllowedDelta);
            Assert.AreEqual(399, secondEvent.LowFrequencyHertz);
            Assert.AreEqual(525, secondEvent.HighFrequencyHertz);
            Assert.AreEqual(38.291398284647158, secondEvent.Score, TestHelper.AllowedDelta);
            Assert.AreEqual(0.0109879605278185, secondEvent.ScoreNormalized, TestHelper.AllowedDelta);
        }
    }
}