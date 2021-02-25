// <copyright file="GenericRecognizerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.Events;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.Tracks;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static global::AudioAnalysisTools.Events.Types.EventPostProcessing;

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
                // set up an array of decibel threhsolds.
                //DecibelThresholds = new double?[] { 3, 6, 9 },
                Profiles = new Dictionary<string, object>()
                {
                    { "TestAed", new Aed.AedConfiguration() { BandpassMinimum = 12345 } },
                    { "TestOscillation", new OscillationParameters() },
                    { "TestBlob", new BlobParameters() { BottomHertzBuffer = 456 } },
                    { "TestWhistle", new OnebinTrackParameters() { TopHertzBuffer = 789 } },
                },
            };

            var target = PathHelper.GetTempFile(this.TestOutputDirectory, ".yml");
            Yaml.Serialize(target, config);

            var lines = target.ReadAllLines();

            CollectionAssert.Contains(lines, "  TestAed: !AedParameters");
            CollectionAssert.Contains(lines, "  TestOscillation: !OscillationParameters");
            CollectionAssert.Contains(lines, "  TestBlob: !BlobParameters");
            CollectionAssert.Contains(lines, "  TestWhistle: !OnebinTrackParameters");

            //lines.ForEach(x => Trace.WriteLine(x));

            var config2 = Yaml.Deserialize<GenericRecognizer.GenericRecognizerConfig>(target);

            Assert.IsNotNull(config2.Profiles);
            Assert.AreEqual(4, config2.Profiles.Count);
            CollectionAssert.AreEquivalent(
                new[] { "TestAed", "TestOscillation", "TestBlob", "TestWhistle" },
                config2.Profiles.Keys);

            Assert.IsInstanceOfType(config2.Profiles["TestAed"], typeof(Aed.AedConfiguration));
            Assert.IsInstanceOfType(config2.Profiles["TestOscillation"], typeof(OscillationParameters));
            Assert.IsInstanceOfType(config2.Profiles["TestBlob"], typeof(BlobParameters));
            Assert.IsInstanceOfType(config2.Profiles["TestWhistle"], typeof(OnebinTrackParameters));

            Assert.AreEqual((config2.Profiles["TestAed"] as Aed.AedConfiguration)?.BandpassMinimum, 12345);

            //THIS TEST FAILING - DO NOT KNOW WHY
            //Assert.AreEqual((config2.Profiles["TestOscillation"] as OscillationParameters)?.DecibelThresholds, thresholdArray);
            Assert.AreEqual((config2.Profiles["TestBlob"] as BlobParameters)?.BottomHertzBuffer, 456);
            Assert.AreEqual((config2.Profiles["TestWhistle"] as OnebinTrackParameters)?.TopHertzBuffer, 789);
        }

        [TestMethod]
        public void TestBlobAlgorithm()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    {
                        "TestBlob", new BlobParameters()
                        {
                            FrameSize = 512,
                            FrameStep = 512,
                            BgNoiseThreshold = 0.0,
                            MinHertz = 4800,
                            MaxHertz = 7200,
                            BottomHertzBuffer = 1000,
                            TopHertzBuffer = 500,

                            // set up an array of decibel threhsolds.
                            DecibelThresholds = new double?[] { 0.0 },
                        }
                    },
                },
                PostProcessing = new PostProcessingConfig()
                {
                    CombineOverlappingEvents = false,

                    // filter on bandwidth
                    Bandwidth = new BandwidthConfig()
                    {
                        ExpectedBandwidth = 2400,
                        BandwidthStandardDeviation = 10,
                    },

                    // filter on acousstic activity in sidebands.
                    // zero indicates no filtering.
                    SidebandAcousticActivity = new SidebandConfig()
                    {
                        UpperSidebandWidth = 0,
                        LowerSidebandWidth = 0,
                        MaxBackgroundDecibels = 0,
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(1, results.NewEvents.Count);
            var @event = (SpectralEvent)results.NewEvents[0];

            Assert.AreEqual(120, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(122, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(4800, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(7200, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestBlob", @event.Profile);
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
                        "LowerBandDTMF_z",
                        new OscillationParameters()
                        {
                            FrameSize = 512,
                            FrameStep = 512,
                            BgNoiseThreshold = 0.0,
                            MaxHertz = 1050,
                            MinHertz = 700,
                            SpeciesName = "DTMF",
                            DctDuration = 1.0,
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            MinDuration = 4,
                            MaxDuration = 8,
                            EventThreshold = 0.3,
                            DecibelThresholds = new double?[] { 0.0 },
                        }
                    },
                },
                PostProcessing = new PostProcessingConfig()
                {
                    CombineOverlappingEvents = false,

                    // filter on bandwidth
                    Bandwidth = new BandwidthConfig()
                    {
                        ExpectedBandwidth = 350,
                        BandwidthStandardDeviation = 20,
                    },

                    // filter on acousstic activity in sidebands.
                    // zero indicates no filtering.
                    SidebandAcousticActivity = new SidebandConfig()
                    {
                        UpperSidebandWidth = 0,
                        LowerSidebandWidth = 0,
                        MaxBackgroundDecibels = 0.0,
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(1, results.NewEvents.Count);
            var @event = (SpectralEvent)results.NewEvents[0];

            Assert.AreEqual(108.1, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.15, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(700, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1050, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual(350, @event.BandWidthHertz);
            Assert.AreEqual("LowerBandDTMF_z", @event.Profile);
            Assert.AreEqual("DTMF", @event.Name);
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
                        new OnebinTrackParameters()
                        {
                            FrameSize = 512,
                            FrameStep = 512,
                            WindowFunction = WindowFunctions.HANNING,
                            BgNoiseThreshold = 0.0,
                            MinHertz = 340,
                            MaxHertz = 560,
                            BottomHertzBuffer = 0,
                            TopHertzBuffer = 0,
                            MinDuration = 4,
                            MaxDuration = 6,
                            SpeciesName = "NoName",
                            DecibelThresholds = new double?[] { 1.0 },
                        }
                    },
                },
                PostProcessing = new PostProcessingConfig()
                {
                    CombineOverlappingEvents = false,

                    // filter on bandwidth
                    Bandwidth = new BandwidthConfig()
                    {
                        ExpectedBandwidth = 90,
                        BandwidthStandardDeviation = 10,
                    },

                    // filter on acousstic activity in sidebands.
                    // zero indicates no filtering.
                    SidebandAcousticActivity = new SidebandConfig()
                    {
                        UpperSidebandWidth = 0,
                        LowerSidebandWidth = 0,
                        MaxBackgroundDecibels = 0,
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(1, results.NewEvents.Count);
            var @event = (SpectralEvent)results.NewEvents[0];

            Assert.AreEqual(101.2, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(106.2, @event.EventEndSeconds, 0.1);

            // NOTE: The whistle algorithm assigns the top and bottom freq bounds of an event based on where it finds the whistle.
            //       Not on what the user has set.
            //       In this test the margin of error has been set arbitrarily to width of one frequency bin.
            Assert.AreEqual(430, @event.LowFrequencyHertz, 44.0);
            Assert.AreEqual(516, @event.HighFrequencyHertz, 44.0);
            Assert.AreEqual("TestWhistle", @event.Profile);
            Assert.AreEqual("NoName", @event.Name);
        }

        [TestMethod]
        public void TestHarmonicsAlgorithm()
        {
            // Set up the recognizer parameters.
            var windowSize = 512;
            var windowStep = 512;
            var minHertz = 500;
            var maxHertz = 5000;
            var dctThreshold = 0.15;
            var minFormantGap = 400;
            var maxFormantGap = 1200;
            var minDuration = 0.2;
            var maxDuration = 1.1;
            var decibelThreshold = 2.0;

            //Set up the virtual recording.
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);

            //var image1 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, null, null, null);
            //results.Sonogram.GetImage().Save(this.outputDirectory + "\\debug.png");

            //var results = recognizer.Recognize(recording, sonoConfig, 100.Seconds(), null, this.TestOutputDirectory, null);
            //get the array of intensity values minus intensity in side/buffer bands.
            var segmentStartOffset = TimeSpan.Zero;
            var plots = new List<Plot>();
            var (acousticEvents, dBArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
                spectrogram,
                minHertz,
                maxHertz,
                spectrogram.NyquistFrequency,
                decibelThreshold,
                dctThreshold,
                minDuration,
                maxDuration,
                minFormantGap,
                maxFormantGap,
                segmentStartOffset);

            // draw a plot of max decibels in each frame
            double decibelNormalizationMax = 3 * decibelThreshold;
            var dBThreshold = decibelThreshold / decibelNormalizationMax;
            var normalisedDecibelArray = DataTools.NormaliseInZeroOne(dBArray, 0, decibelNormalizationMax);
            var plot1 = new Plot("decibel max", normalisedDecibelArray, dBThreshold);
            plots.Add(plot1);

            // draw a plot of dct intensity
            double intensityNormalizationMax = 3 * dctThreshold;
            var eventThreshold = dctThreshold / intensityNormalizationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(harmonicIntensityScores, 0, intensityNormalizationMax);
            var plot2 = new Plot("dct intensity", normalisedIntensityArray, eventThreshold);
            plots.Add(plot2);

            var allResults = new RecognizerResults();

            // combine the results i.e. add the events list of call events.
            allResults.NewEvents.AddRange(acousticEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES COMMENT NEXT LINE
            //GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "name");

            Assert.AreEqual(4, allResults.NewEvents.Count);

            var @event = (SpectralEvent)allResults.NewEvents[0];
            Assert.AreEqual("Harmonics", @event.Name);
            Assert.AreEqual("SpectralEvent", @event.ComponentName);
            Assert.AreEqual(3.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(4.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(500, @event.LowFrequencyHertz);
            Assert.AreEqual(5000, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[1];
            Assert.AreEqual(5.2, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(5.5, @event.EventEndSeconds, 0.1);

            @event = (SpectralEvent)allResults.NewEvents[2];
            Assert.AreEqual(7.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(8.0, @event.EventEndSeconds, 0.1);

            @event = (SpectralEvent)allResults.NewEvents[3];
            Assert.AreEqual(11.3, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(11.6, @event.EventEndSeconds, 0.1);
        }

        [TestMethod]
        public void TestOnebinTrackAlgorithm()
        {
            // Set up the recognizer parameters.
            double? decibelThreshold = 2.0;

            var parameters = new OnebinTrackParameters()
            {
                MinHertz = 500,
                MaxHertz = 6000,
                MinDuration = 0.2,
                MaxDuration = 1.1,
                CombinePossibleSyllableSequence = false,
            };

            //Set up the virtual recording.
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds
            var segmentStartOffset = TimeSpan.FromSeconds(60.0);

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);

            var (spectralEvents, plotList) = OnebinTrackAlgorithm.GetOnebinTracks(
                spectrogram,
                parameters,
                decibelThreshold,
                segmentStartOffset,
                "TestProfile");

            // draw a plot of max decibels in each frame
            var plots = new List<Plot>();
            plots.AddRange(plotList);

            var allResults = new RecognizerResults()
            {
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.NewEvents.AddRange(spectralEvents);
            allResults.Plots.AddRange(plots);
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES
            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "WhistleTrack"));

            //NOTE: There are 16 whistles in the test spectrogram ...
            // but three of them are too weak to be detected at this threshold.
            Assert.AreEqual(13, allResults.NewEvents.Count);

            var @event = (SpectralEvent)allResults.NewEvents[0];
            Assert.AreEqual(60 + 0.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(60 + 0.35, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(2150, @event.LowFrequencyHertz);
            Assert.AreEqual(2193, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[4];
            Assert.AreEqual(60 + 5.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(60 + 6.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(989, @event.LowFrequencyHertz);
            Assert.AreEqual(1032, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[11];
            Assert.AreEqual(60 + 11.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(60 + 12.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(989, @event.LowFrequencyHertz);
            Assert.AreEqual(1032, @event.HighFrequencyHertz);
        }

        [TestMethod]
        public void TestForwardTrackAlgorithm()
        {
            // Set up the recognizer parameters.
            double? decibelThreshold = 2.0;
            var parameters = new ForwardTrackParameters()
            {
                MinHertz = 500,
                MaxHertz = 6000,
                MinDuration = 0.2,
                MaxDuration = 1.1,
                CombinePossibleHarmonics = false,
                HarmonicsStartDifference = TimeSpan.FromSeconds(0.2),
                HarmonicsHertzGap = 200,
            };

            //Set up the virtual recording.
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);

            //var image1 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, null, null, null);
            //results.Sonogram.GetImage().Save(this.outputDirectory + "\\debug.png");

            var segmentStartOffset = TimeSpan.Zero;
            var (spectralEvents, plotList) = ForwardTrackAlgorithm.GetForwardTracks(
                spectrogram,
                parameters,
                decibelThreshold,
                segmentStartOffset,
                "TestProfile");

            var plots = new List<Plot>();
            plots.AddRange(plotList);

            var allResults = new RecognizerResults()
            {
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.NewEvents.AddRange(spectralEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES COMMENT NEXT LINE
            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "ForwardTrack"));

            Assert.AreEqual(23, allResults.NewEvents.Count);

            var @event = (SpectralEvent)allResults.NewEvents[4];
            Assert.AreEqual(2.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(2.5, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(1720, @event.LowFrequencyHertz);
            Assert.AreEqual(2107, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[11];
            Assert.AreEqual(6.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.5, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(2150, @event.LowFrequencyHertz);
            Assert.AreEqual(2580, @event.HighFrequencyHertz);
        }

        [TestMethod]
        public void TestOneframeTrackAlgorithm()
        {
            // Set up the recognizer parameters.
            double? decibelThreshold = 2.0;
            var parameters = new OneframeTrackParameters()
            {
                MinHertz = 6000,
                MaxHertz = 11000,
                MinBandwidthHertz = 100,
                MaxBandwidthHertz = 5000,
            };

            //Set up the virtual recording.
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);

            //var image1 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, null, null, null);
            //results.Sonogram.GetImage().Save(this.outputDirectory + "\\debug.png");

            var segmentStartOffset = TimeSpan.Zero;
            var (spectralEvents, plotList) = OneframeTrackAlgorithm.GetOneFrameTracks(
                spectrogram,
                parameters,
                decibelThreshold,
                segmentStartOffset,
                "TestProfile");

            var plots = new List<Plot>();
            plots.AddRange(plotList);

            var allResults = new RecognizerResults()
            {
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.NewEvents.AddRange(spectralEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES COMMENT NEXT LINE
            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "ClickTrack"));

            Assert.AreEqual(6, allResults.NewEvents.Count);

            var @event = (SpectralEvent)allResults.NewEvents[0];
            Assert.AreEqual(10.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(10.1, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(6450, @event.LowFrequencyHertz);
            Assert.AreEqual(10750, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[2];
            Assert.AreEqual(11.05, @event.EventStartSeconds, 0.05);
            Assert.AreEqual(11.07, @event.EventEndSeconds, 0.05);
            Assert.AreEqual(6450, @event.LowFrequencyHertz);
            Assert.AreEqual(7310, @event.HighFrequencyHertz);
        }

        /// <summary>
        /// Tests the upward-track recognizer on the same artifical spectrogram as used for foward-tracks and harmonics.
        /// </summary>
        [TestMethod]
        public void Test1UpwardsTrackAlgorithm()
        {
            // Set up the recognizer parameters.
            var decibelThreshold = 2.0;
            var parameters = new UpwardTrackParameters()
            {
                MinHertz = 6000,
                MaxHertz = 11000,
                MinBandwidthHertz = 100,
                MaxBandwidthHertz = 5000,

                // these params are to detect calls that consist of a rapid sequence of chirps/whips.
                CombineProximalSimilarEvents = true,
                SyllableStartDifference = TimeSpan.FromSeconds(0.2),
                SyllableHertzDifference = 300,
            };

            //Set up the virtual recording.
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);

            var segmentStartOffset = TimeSpan.Zero;
            var plots = new List<Plot>();
            var (spectralEvents, plotList) = UpwardTrackAlgorithm.GetUpwardTracks(
                spectrogram,
                parameters,
                decibelThreshold,
                segmentStartOffset,
                "TestProfile");

            plots.AddRange(plotList);

            var allResults = new RecognizerResults()
            {
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.NewEvents.AddRange(spectralEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES ONLY - COMMENT NEXT LINE
            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "UpwardsTrack1"));

            Assert.AreEqual(2, allResults.NewEvents.Count);

            var @event = (SpectralEvent)allResults.NewEvents[0];
            Assert.AreEqual(10.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(10.1, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(6450, @event.LowFrequencyHertz);
            Assert.AreEqual(10750, @event.HighFrequencyHertz);

            @event = (SpectralEvent)allResults.NewEvents[1];
            Assert.AreEqual(11.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(11.24, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(6450, @event.LowFrequencyHertz);
            Assert.AreEqual(7310, @event.HighFrequencyHertz);
        }

        /// <summary>
        /// Tests the upward-track recognizer on the same artifical spectrogram as used for foward-tracks and harmonics.
        /// </summary>
        [TestMethod]
        public void Test2UpwardsTrackAlgorithm()
        {
            // Set up the recognizer parameters.
            var decibelThreshold = 2.0;
            var parameters = new UpwardTrackParameters()
            {
                MinHertz = 500,
                MaxHertz = 6000,
                MinBandwidthHertz = 200,
                MaxBandwidthHertz = 5000,

                // these params are to detect calls that consist of a rapid sequence of chirps/whips.
                CombineProximalSimilarEvents = false,
                SyllableStartDifference = TimeSpan.FromSeconds(0.2),
                SyllableHertzDifference = 300,
            };

            //Set up the virtual recording.
            var segmentStartOffset = TimeSpan.Zero;
            int samplerate = 22050;
            double signalDuration = 13.0; //seconds

            // set up the config for a virtual spectrogram.
            var sonoConfig = new SonogramConfig()
            {
                WindowSize = 512,
                WindowStep = 512,
                WindowOverlap = 0.0, // this must be set
                WindowFunction = WindowFunctions.HANNING.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = 0.0,
                Duration = TimeSpan.FromSeconds(signalDuration),
                SampleRate = samplerate,
            };

            var spectrogram = this.CreateArtificialSpectrogramToTestTracksAndHarmonics(sonoConfig);
            var plots = new List<Plot>();

            // do a SECOND TEST of the vertical tracks
            var (spectralEvents, plotList) = UpwardTrackAlgorithm.GetUpwardTracks(
                spectrogram,
                parameters,
                decibelThreshold,
                segmentStartOffset,
                "TestProfile");

            // draw a plot of max decibels in each frame
            plots.AddRange(plotList);

            var allResults2 = new RecognizerResults()
            {
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults2.NewEvents.AddRange(spectralEvents);
            allResults2.Plots.AddRange(plots);
            allResults2.Sonogram = spectrogram;

            // DEBUG PURPOSES ONLY - COMMENT NEXT LINE
            this.SaveTestOutput(
                outputDirectory => GenericRecognizer.SaveDebugSpectrogram(allResults2, null, outputDirectory, "UpwardTracks2"));

            Assert.AreEqual(10, allResults2.NewEvents.Count);
        }

        public SpectrogramStandard CreateArtificialSpectrogramToTestTracksAndHarmonics(SonogramConfig config)
        {
            int samplerate = config.SampleRate;
            double signalDuration = config.Duration.TotalSeconds;
            int windowSize = config.WindowSize;
            int frameCount = (int)Math.Floor(samplerate * signalDuration / windowSize);
            int binCount = windowSize / 2;

            // set up the spectrogram with stacked harmonics
            var amplitudeSpectrogram = new double[frameCount, binCount];
            double framesPerSecond = samplerate / (double)windowSize;
            double hertzPerBin = samplerate / (double)windowSize;

            // draw first set of harmonics
            int bottomHertz = 1000;
            int bottomBin = (int)Math.Round(bottomHertz / hertzPerBin);
            int formantGap = 300;
            int binGap = (int)Math.Round(formantGap / hertzPerBin);
            int startframe = (int)Math.Round(framesPerSecond);
            int endframe = (int)Math.Round(framesPerSecond * 2);
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 6.0;
                amplitudeSpectrogram[frame, bottomBin + binGap] = 6.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap] = 6.0;
            }

            // draw second set of harmonics
            formantGap = 600;
            binGap = (int)Math.Round(formantGap / hertzPerBin);
            startframe = (int)Math.Round(framesPerSecond * 3);
            endframe = (int)Math.Round(framesPerSecond * 4);
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 3.0;
                amplitudeSpectrogram[frame, bottomBin + binGap] = 3.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap] = 3.0;
            }

            // draw third set of harmonics
            formantGap = 300;
            binGap = (int)Math.Round(formantGap / hertzPerBin);
            startframe = (int)Math.Round(framesPerSecond * 5) - 1;
            endframe = (int)Math.Round(framesPerSecond * 6);
            int offset = 0;
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + offset] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap + offset + offset] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap + offset + offset + 1] = 6.0;
                offset++;
            }

            // draw fourth set of harmonics
            formantGap = 1000;
            binGap = (int)Math.Round(formantGap / hertzPerBin);
            startframe = (int)Math.Round(framesPerSecond * 7);
            endframe = (int)Math.Round(framesPerSecond * 8);
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 6.0;
                amplitudeSpectrogram[frame, bottomBin + binGap] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap] = 6.0;
            }

            // draw fifth set of harmonics
            formantGap = 1500;
            binGap = (int)Math.Round(formantGap / hertzPerBin);
            startframe = (int)Math.Round(framesPerSecond * 9);
            endframe = (int)Math.Round(framesPerSecond * 10);
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap] = 9.0;
            }

            // draw sixth set of harmonics
            formantGap = 1800;
            binGap = (int)Math.Round(formantGap / hertzPerBin);
            startframe = (int)Math.Round(framesPerSecond * 11) - 1;
            endframe = (int)Math.Round(framesPerSecond * 12);
            offset = 0;
            for (int frame = startframe; frame < endframe; frame++)
            {
                amplitudeSpectrogram[frame, bottomBin] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap - offset] = 9.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap - offset - offset - 1] = 3.0;
                amplitudeSpectrogram[frame, bottomBin + binGap + binGap - offset - offset] = 9.0;
                offset++;
            }

            // draw a set of sequential tracks
            for (int i = 0; i < 15; i++)
            {
                // track that starts in very first time frame
                amplitudeSpectrogram[i, 50] = 6.0;

                // track that goes to end of spectrogram
                amplitudeSpectrogram[frameCount - 1 - i, 50] = 6.0;
            }

            //boobook owl look-alike
            startframe = (int)Math.Round(framesPerSecond * 2) + 3;
            var startBin = 40;
            for (int i = 0; i < 9; i++)
            {
                amplitudeSpectrogram[startframe + i, startBin + i] = 9.0;
                amplitudeSpectrogram[startframe + 16 - i, startBin + i] = 6.0;
            }

            startframe = (int)Math.Round(framesPerSecond * 6) + 3;
            startBin = 50;
            for (int i = 0; i < 10; i++)
            {
                amplitudeSpectrogram[startframe + i, startBin + i] = 6.0;
                amplitudeSpectrogram[startframe + 20 - i, startBin + i] = 9.0;
            }

            startframe = (int)Math.Round(framesPerSecond * 10) + 3;
            startBin = 50;
            for (int i = 0; i < 8; i++)
            {
                amplitudeSpectrogram[startframe + i, startBin + i] = 9.0;
                amplitudeSpectrogram[startframe + 16 - i, startBin + i] = 9.0;
            }

            amplitudeSpectrogram[startframe + 8, startBin + 8] = 6.0;
            amplitudeSpectrogram[startframe + 8, startBin + 7] = 9.0;

            // draw a click
            var clickFrame = (int)Math.Round(framesPerSecond * 10) + 3;
            startBin = 150;
            var endBin = 250;
            for (int i = startBin; i < endBin; i++)
            {
                amplitudeSpectrogram[clickFrame, i] = 9.0;
                amplitudeSpectrogram[clickFrame + 1, i] = 6.0;
            }

            // Draw a series of clicks similar to bird kek-kek
            clickFrame = (int)Math.Round(framesPerSecond * 11);
            startBin = 150;
            endBin = 170;
            for (int i = startBin; i < endBin; i++)
            {
                amplitudeSpectrogram[clickFrame, i] = 6.0;
                amplitudeSpectrogram[clickFrame + 2, i] = 6.0;
                amplitudeSpectrogram[clickFrame + 4, i] = 6.0;
                amplitudeSpectrogram[clickFrame + 6, i] = 6.0;
                amplitudeSpectrogram[clickFrame + 8, i] = 6.0;
            }

            var spectrogram = new SpectrogramStandard(config)
            {
                SampleRate = samplerate,
                Data = amplitudeSpectrogram,
            };

            return spectrogram;
        }

        [TestMethod]
        public void TestAedAlgorithm()
        {
            var resampledRecordingPath = PathHelper.GetTempFile(this.TestOutputDirectory, ".wav");
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
                            //DecibelThresholds = new double?[] { 0.0 },
                            NoiseReductionType = NoiseReductionType.None,
                            NoiseReductionParameter = 15,
                            SmallAreaThreshold = 150,
                            IntensityThreshold = 20,
                        }
                    },
                },
                PostProcessing = new PostProcessingConfig()
                {
                    CombineOverlappingEvents = false,

                    // filter on bandwidth
                    Bandwidth = new BandwidthConfig()
                    {
                        ExpectedBandwidth = 3000,
                        BandwidthStandardDeviation = 1000,
                    },

                    // filter on acousstic activity in sidebands.
                    // zero indicates no filtering.
                    SidebandAcousticActivity = new SidebandConfig()
                    {
                        UpperSidebandWidth = 0,
                        LowerSidebandWidth = 0,
                        MaxBackgroundDecibels = 0,
                    },
                },
            };

            var results = recognizer.Recognize(new AudioRecording(resampledRecordingPath), config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(14, results.NewEvents.Count);
        }

        [TestMethod]
        public void TestMultipleAlgorithms()
        {
            var config = new GenericRecognizer.GenericRecognizerConfig()
            {
                Profiles = new Dictionary<string, object>()
                {
                    {
                        "TestBlob", new BlobParameters()
                        {
                            FrameSize = 512,
                            FrameStep = 512,
                            MaxHertz = 7200,
                            MinHertz = 4800,
                            BgNoiseThreshold = 0.0,
                            BottomHertzBuffer = 1000,
                            TopHertzBuffer = 500,
                            DecibelThresholds = new double?[] { 0.0 },
                        }
                    },
                    {
                        "LowerBandDTMF_z",
                        new OscillationParameters()
                        {
                            SpeciesName = "DTMFlower",
                            FrameSize = 512,
                            FrameStep = 512,
                            BgNoiseThreshold = 0.0,
                            MaxHertz = 1050,
                            MinHertz = 700,
                            BottomHertzBuffer = 0,
                            TopHertzBuffer = 0,
                            DctDuration = 1.0,
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            MinDuration = 4,
                            MaxDuration = 6,
                            EventThreshold = 0.3,
                            DecibelThresholds = new double?[] { 0.0 },
                        }
                    },
                    {
                        "UpperBandDTMF_z",
                        new OscillationParameters()
                        {
                            SpeciesName = "DTMFupper",
                            FrameSize = 512,
                            FrameStep = 512,
                            BgNoiseThreshold = 0.0,
                            MaxHertz = 1650,
                            MinHertz = 1350,
                            BottomHertzBuffer = 0,
                            TopHertzBuffer = 0,
                            DctDuration = 1.0,
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            MinDuration = 4,
                            MaxDuration = 6,
                            EventThreshold = 0.3,
                            DecibelThresholds = new double?[] { 0.0 },
                        }
                    },
                },
                PostProcessing = new PostProcessingConfig()
                {
                    CombineOverlappingEvents = false,

                    // filter on bandwidth
                    Bandwidth = new BandwidthConfig()
                    {
                        ExpectedBandwidth = 3000,
                        BandwidthStandardDeviation = 1000,
                    },

                    // filter on acousstic activity in sidebands.
                    // zero indicates no filtering.
                    SidebandAcousticActivity = new SidebandConfig()
                    {
                        UpperSidebandWidth = 0,
                        LowerSidebandWidth = 0,
                        MaxBackgroundDecibels = 0,
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(3, results.NewEvents.Count);

            var @event = (SpectralEvent)results.NewEvents[0];
            Assert.AreEqual(120, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(122, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(4800, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(7200, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("TestBlob", @event.Profile);
            Assert.AreEqual(null, @event.Name);
            Assert.AreEqual("SpectralEvent", @event.ComponentName);

            @event = (SpectralEvent)results.NewEvents[1];
            Assert.AreEqual(107.78, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.57, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(700, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1050, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("DTMFlower", @event.Name);
            Assert.AreEqual("LowerBandDTMF_z", @event.Profile);
            Assert.AreEqual("OscillationEvent", @event.ComponentName);
            Assert.AreEqual("acoustic_components", @event.FileName);

            @event = (SpectralEvent)results.NewEvents[2];
            Assert.AreEqual(108.1, @event.EventStartSeconds, 0.4);
            Assert.AreEqual(113.15, @event.EventEndSeconds, 0.5);
            Assert.AreEqual(1350, @event.LowFrequencyHertz, 0.1);
            Assert.AreEqual(1650, @event.HighFrequencyHertz, 0.1);
            Assert.AreEqual("DTMFupper", @event.Name);
            Assert.AreEqual("UpperBandDTMF_z", @event.Profile);
        }
    }
}