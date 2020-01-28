// <copyright file="GenericRecognizerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.GenericRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.WavTools;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GenericRecognizerTests : OutputDirectoryTest
    {
        private static readonly FileInfo TestAsset = PathHelper.ResolveAsset("acoustic_components.wav");
        private static AudioRecording recording;
        private static GenericRecognizer recognizer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            recording = new AudioRecording(TestAsset);
            recognizer = new GenericRecognizer();
        }

        [TestMethod]
        public void TestConfigSerialization()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    { "TestAed", new Aed.AedConfiguration() { BandpassMinimum = 12345 } },
                    { "TestOscillation", new OscillationParameters() { DecibelThreshold = 123 } },
                    { "TestBlob", new BlobParameters() { BottomHertzBuffer = 456 } },
                    { "TestWhistle", new WhistleParameters() { TopHertzBuffer = 789 } },
                },
            };

            var target = PathHelper.GetTempFile(this.outputDirectory, ".yml");
            Yaml.Serialize(target, config);

            var lines = target.ReadAllLines();

            CollectionAssert.Contains(lines, "  TestAed: !AedParameters");
            CollectionAssert.Contains(lines, "  TestOscillation: !OscillationParameters");
            CollectionAssert.Contains(lines, "  TestBlob: !BlobParameters");
            CollectionAssert.Contains(lines, "  TestWhistle: !WhistleParameters");

            //lines.ForEach(x => Debug.WriteLine(x));

            var config2 = Yaml.Deserialize<GenericRecognizer.GenericRecognizerConfig>(target);

            Assert.IsNotNull(config2.Profiles);
            Assert.AreEqual(4, config2.Profiles.Count);
            CollectionAssert.AreEquivalent(
                new[] { "TestAed", "TestOscillation", "TestBlob", "TestWhistle" },
                config2.Profiles.Keys);

            Assert.IsInstanceOfType(config2.Profiles["TestAed"], typeof(Aed.AedConfiguration));
            Assert.IsInstanceOfType(config2.Profiles["TestOscillation"], typeof(OscillationParameters));
            Assert.IsInstanceOfType(config2.Profiles["TestBlob"], typeof(BlobParameters));
            Assert.IsInstanceOfType(config2.Profiles["TestWhistle"], typeof(WhistleParameters));

            Assert.AreEqual((config2.Profiles["TestAed"] as Aed.AedConfiguration)?.BandpassMinimum, 12345);
            Assert.AreEqual((config2.Profiles["TestOscillation"] as OscillationParameters)?.DecibelThreshold, 123);
            Assert.AreEqual((config2.Profiles["TestBlob"] as BlobParameters)?.BottomHertzBuffer, 456);
            Assert.AreEqual((config2.Profiles["TestWhistle"] as WhistleParameters)?.TopHertzBuffer, 789);
        }

        [TestMethod]
        public void TestBlobAlgorithm()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    { "TestBlob", new BlobParameters() { MaxHertz = 7200, MinHertz = 4800 } },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.outputDirectory, null);

            Assert.AreEqual(1, results.Events.Count);
            var @event = results.Events[0];

            Assert.AreEqual(120, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(122, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(4800, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(7200, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestBlob", @event.Profile);
            Assert.AreEqual(null, @event.SpeciesName);
            Assert.AreEqual(null, @event.Name);
        }

        [TestMethod]
        public void TestOscillationAlgorithm()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    {
                        "TestOscillation",
                        new OscillationParameters()
                        {
                            MaxHertz = 1050,
                            MinHertz = 700,
                            SpeciesName = "DTMF",
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            ComponentName = "LowerBandDTMF_z",
                            MinDuration = 4,
                            MaxDuration = 6,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.outputDirectory, null);

            Assert.AreEqual(1, results.Events.Count);
            var @event = results.Events[0];

            Assert.AreEqual(108.1, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.15, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(700, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1050, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestOscillation", @event.Profile);
            Assert.AreEqual("DTMF", @event.SpeciesName);
            Assert.AreEqual("LowerBandDTMF_z", @event.Name);
        }

        [TestMethod]
        public void TestWhistleAlgorithm()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    {
                        "TestWhistle",
                        new WhistleParameters()
                        {
                            MaxHertz = 300,
                            MinHertz = 500,
                            ComponentName = "Whistle400Hz",
                            MinDuration = 4,
                            MaxDuration = 8,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.outputDirectory, null);

            Assert.AreEqual(1, results.Events.Count);
            var @event = results.Events[0];

            Assert.AreEqual(101.2, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(106.2, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(300, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(500, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestWhistle", @event.Profile);
            Assert.AreEqual("", @event.SpeciesName);
            Assert.AreEqual("Whistle400Hz", @event.Name);
        }

        [TestMethod]
        public void TestAedAlgorithm()
        {
            var resampledRecordingPath = PathHelper.GetTempFile(this.outputDirectory, ".wav");
            TestHelper.GetAudioUtility().Modify(
                TestAsset,
                MediaTypes.MediaTypeWav,
                resampledRecordingPath,
                MediaTypes.MediaTypeWav,
                new AudioUtilityRequest()
                {
                    TargetSampleRate = 22050,
                });
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    {
                        "TestAed",
                        new Aed.AedConfiguration()
                        {
                            NoiseReductionType = NoiseReductionType.None,
                            NoiseReductionParameter = 15,
                            SmallAreaThreshold = 150,
                            IntensityThreshold = 20,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(new AudioRecording(resampledRecordingPath), config, 100.Seconds(), null, this.outputDirectory, null);

            Assert.AreEqual(14, results.Events.Count);
        }

        [TestMethod]
        public void TestMultipleAlgorithms()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    { "TestBlob", new BlobParameters() { MaxHertz = 7200, MinHertz = 4800 } },
                    {
                        "TestOscillationA",
                        new OscillationParameters()
                        {
                            MaxHertz = 1050,
                            MinHertz = 700,
                            SpeciesName = "DTMF",
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            ComponentName = "LowerBandDTMF_z",
                            MinDuration = 4,
                            MaxDuration = 6,
                        }
                    },
                    {
                        "TestOscillationB",
                        new OscillationParameters()
                        {
                            MaxHertz = 1650,
                            MinHertz = 1350,
                            SpeciesName = "DTMF",
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            ComponentName = "UpperBandDTMF_z",
                            MinDuration = 4,
                            MaxDuration = 6,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.outputDirectory, null);

            Assert.AreEqual(3, results.Events.Count);

            var @event = results.Events[0];
            Assert.AreEqual(120, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(122, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(4800, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(7200, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestBlob", @event.Profile);
            Assert.AreEqual(null, @event.SpeciesName);
            Assert.AreEqual(null, @event.Name);

            @event = results.Events[1];
            Assert.AreEqual(108.1, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.15, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(700, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1050, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestOscillationA", @event.Profile);
            Assert.AreEqual("DTMF", @event.SpeciesName);
            Assert.AreEqual("LowerBandDTMF_z", @event.Name);

            @event = results.Events[2];
            Assert.AreEqual(108.1, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.15, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(1350, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1650, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestOscillationB", @event.Profile);
            Assert.AreEqual("DTMF", @event.SpeciesName);
            Assert.AreEqual("UpperBandDTMF_z", @event.Name);
        }
    }
}