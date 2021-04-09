// <copyright file="AustralPipitTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools.Wav;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.SourcePreparers;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.Events.Types;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Species name = Australasian Pipit = Anthus novaeseelandiae.
    /// </summary>
    [TestClass]
    public class AustralPipitTests : OutputDirectoryTest
    {
        /// <summary>
        /// The canonical recording used for this recognizer is a 30 second recording containing five Pipit calls and a number of Cisticola and other bird calls.
        /// It was recorded in Narrabri region and forms part of the Cotton Project data set.
        /// </summary>
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("Recordings", "ms1_2559_630118_20170402_075841_30_0.wav");
        private static readonly FileInfo ConfigFile = PathHelper.ResolveConfigFile("RecognizerConfigFiles", "Towsey.AnthusNovaeseelandiae.yml");
        private static readonly AnthusNovaeseelandiae Recognizer = new AnthusNovaeseelandiae();

        [TestMethod]
        public void TestRecognizer()
        {
            var config = Recognizer.ParseConfig(ConfigFile);
            int resampleRate = config.ResampleRate.Value;
            string opDir = this.TestOutputDirectory.FullName;
            string opFileName = "tempFile.wav";
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

            Assert.AreEqual(5, events.Count);
            Assert.IsNull(scoreTrack);
            Assert.AreEqual(3, plots.Count);
            Assert.AreEqual(1874, sonogram.FrameCount);

            // events[2] should be a composite event.
            var ev = (CompositeEvent)events[2];
            Assert.IsInstanceOfType(events[2], typeof(CompositeEvent));
            Assert.AreEqual(22, ev.EventStartSeconds, TestHelper.AllowedDelta);
            Assert.AreEqual(22, ev.EventEndSeconds, TestHelper.AllowedDelta);
            Assert.AreEqual(4743, ev.BandWidthHertz);

            var componentEvents = ev.ComponentEvents;
            Assert.AreEqual(3, componentEvents.Count);

            // This tests that the component tracks are correctly combined.
            //This can also be tested somewhere else, starting with just the comosite event in json file.
            var points = EventExtentions.GetCompositeTrack(componentEvents.Cast<WhipEvent>()).ToArray();
            Assert.AreEqual(22.016, points[1].Seconds.Minimum, TestHelper.AllowedDelta);
            Assert.AreEqual(5456, points[1].Hertz.Minimum);
            Assert.AreEqual(23.13758005922, points[1].Value, TestHelper.AllowedDelta);
        }
    }
}
