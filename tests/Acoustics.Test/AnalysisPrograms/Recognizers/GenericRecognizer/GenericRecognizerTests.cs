// <copyright file="GenericRecognizerTests.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Test.AnalysisPrograms.Recognizers.GenericRecognizer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Acoustics.Shared;
    using Acoustics.Test.TestHelpers;
    using Acoustics.Tools;
    using global::AnalysisBase;
    using global::AnalysisPrograms;
    using global::AnalysisPrograms.Recognizers;
    using global::AnalysisPrograms.Recognizers.Base;
    using global::AudioAnalysisTools;
    using global::AudioAnalysisTools.DSP;
    using global::AudioAnalysisTools.StandardSpectrograms;
    using global::AudioAnalysisTools.WavTools;
    using global::TowseyLibrary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SixLabors.ImageSharp;

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

            var target = PathHelper.GetTempFile(this.TestOutputDirectory, ".yml");
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
                    { "TestBlob", new BlobParameters() { FrameSize = 512, FrameStep = 512, BgNoiseThreshold = 0.0, MinHertz = 4800, MaxHertz = 7200, BottomHertzBuffer = 1000, TopHertzBuffer = 500 } },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

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
                            FrameSize = 512,
                            FrameStep = 512,
                            BgNoiseThreshold = 0.0,
                            MaxHertz = 1050,
                            MinHertz = 700,
                            BottomHertzBuffer = 0,
                            TopHertzBuffer = 0,
                            SpeciesName = "DTMF",
                            DctDuration = 1.0,
                            MinOscillationFrequency = 1,
                            MaxOscillationFrequency = 2,
                            ComponentName = "LowerBandDTMF_z",
                            MinDuration = 4,
                            MaxDuration = 8,
                            EventThreshold = 0.3,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);
            //results.Plots.
            //results.Sonogram.GetImage().Save(this.outputDirectory + "\\debug.png");

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
                            FrameSize = 512,
                            FrameStep = 512,
                            WindowFunction = WindowFunctions.HANNING.ToString(),
                            BgNoiseThreshold = 0.0,
                            MinHertz = 340,
                            MaxHertz = 560,
                            BottomHertzBuffer = 0,
                            TopHertzBuffer = 0,
                            MinDuration = 4,
                            MaxDuration = 6,
                            DecibelThreshold = 1.0,
                            SpeciesName = "NoName",
                            ComponentName = "Whistle400Hz",
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(1, results.Events.Count);
            var @event = results.Events[0];

            Assert.AreEqual(101.2, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(106.2, @event.EventEndSeconds, 0.1);

            // NOTE: The whistle algorithm assigns the top and bottom freq bounds of an event based on where it finds the whistle.
            //       Not on what the user has set.
            //       In this test the margin of error has been set arbitrarily to 10.
            Assert.AreEqual(340, @event.LowFrequencyHertz, 20.0);
            Assert.AreEqual(560, @event.HighFrequencyHertz, 50.0);
            Assert.AreEqual("TestWhistle", @event.Profile);
            Assert.AreEqual("NoName", @event.SpeciesName);
            Assert.AreEqual("Whistle400Hz", @event.Name);
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
            //var minDuration = 0.35;
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

            var spectrogram = this.CreateArtificialSpectrogramContainingHarmonics(sonoConfig);
            //var image1 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, null, null, null);

            //var results = recognizer.Recognize(recording, sonoConfig, 100.Seconds(), null, this.TestOutputDirectory, null);
            //get the array of intensity values minus intensity in side/buffer bands.
            var segmentStartOffset = TimeSpan.Zero;
            var plots = new List<Plot>();
            double[] dBArray;
            double[] harmonicIntensityScores;
            List<AcousticEvent> acousticEvents;
            (acousticEvents, dBArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
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

            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.Events.AddRange(acousticEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES COMMENT NEXT LINE
            //var outputDirectory = new DirectoryInfo("C:\\temp");
            //GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "name");

            Assert.AreEqual(4, allResults.Events.Count);

            var @event = allResults.Events[0];
            Assert.AreEqual("NoName", @event.SpeciesName);
            Assert.AreEqual("Harmonics", @event.Name);
            Assert.AreEqual(3.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(4.0, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(500, @event.LowFrequencyHertz);
            Assert.AreEqual(5000, @event.HighFrequencyHertz);

            @event = allResults.Events[1];
            Assert.AreEqual(5.2, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(5.5, @event.EventEndSeconds, 0.1);

            @event = allResults.Events[2];
            Assert.AreEqual(7.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(8.0, @event.EventEndSeconds, 0.1);

            @event = allResults.Events[3];
            Assert.AreEqual(11.3, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(11.6, @event.EventEndSeconds, 0.1);
        }

        [TestMethod]
        public void TestSpectralPeakTracksAlgorithm()
        {
            // Set up the recognizer parameters.
            var windowSize = 512;
            var windowStep = 512;
            var minHertz = 500;
            var maxHertz = 6000;
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

            var spectrogram = this.CreateArtificialSpectrogramContainingHarmonics(sonoConfig);
            //var image1 = SpectrogramTools.GetSonogramPlusCharts(spectrogram, null, null, null);

            var segmentStartOffset = TimeSpan.Zero;
            var plots = new List<Plot>();
            double[] dBArray;
            List<AcousticEvent> acousticEvents;
            (acousticEvents, dBArray) = SpectralPeakTrackParameters.GetSpectralPeakTracks(
                spectrogram,
                minHertz,
                maxHertz,
                spectrogram.NyquistFrequency,
                decibelThreshold,
                minDuration,
                maxDuration,
                segmentStartOffset);

            // draw a plot of max decibels in each frame
            double decibelNormalizationMax = 3 * decibelThreshold;
            var dBThreshold = decibelThreshold / decibelNormalizationMax;
            var normalisedDecibelArray = DataTools.NormaliseInZeroOne(dBArray, 0, decibelNormalizationMax);
            var plot1 = new Plot("decibel max", normalisedDecibelArray, dBThreshold);
            plots.Add(plot1);

            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // combine the results i.e. add the events list of call events.
            allResults.Events.AddRange(acousticEvents);
            allResults.Plots.AddRange(plots);

            // effectively keeps only the *last* sonogram produced
            allResults.Sonogram = spectrogram;

            // DEBUG PURPOSES COMMENT NEXT LINE
            var outputDirectory = new DirectoryInfo("C:\\temp");
            GenericRecognizer.SaveDebugSpectrogram(allResults, null, outputDirectory, "track");

            Assert.AreEqual(21, allResults.Events.Count);

            var @event = allResults.Events[3];
            Assert.AreEqual(2.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(2.5, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(1680, @event.LowFrequencyHertz);
            Assert.AreEqual(2110, @event.HighFrequencyHertz);

            @event = allResults.Events[10];
            Assert.AreEqual(6.0, @event.EventStartSeconds, 0.1);
            Assert.AreEqual(6.6, @event.EventEndSeconds, 0.1);
            Assert.AreEqual(2110, @event.LowFrequencyHertz);
            Assert.AreEqual(2584, @event.HighFrequencyHertz);

        }

        public SpectrogramStandard CreateArtificialSpectrogramContainingHarmonics(SonogramConfig config)
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
            startframe = (int)Math.Round(framesPerSecond * 2) + 3;
            int startBin = 40;
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

            var spectrogram = new SpectrogramStandard(config)
            {
                //FrameCount = amplitudeSpectrogram.GetLength(0),
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
                            NoiseReductionType = NoiseReductionType.None,
                            NoiseReductionParameter = 15,
                            SmallAreaThreshold = 150,
                            IntensityThreshold = 20,
                        }
                    },
                },
            };

            var results = recognizer.Recognize(new AudioRecording(resampledRecordingPath), config, 100.Seconds(), null, this.TestOutputDirectory, null);

            Assert.AreEqual(14, results.Events.Count);
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
                        }
                    },
                    {
                        "TestOscillationA",
                        new OscillationParameters()
                        {
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
                            SpeciesName = "DTMF",
                            ComponentName = "LowerBandDTMF_z",
                        }
                    },
                    {
                        "TestOscillationB",
                        new OscillationParameters()
                        {
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
                            SpeciesName = "DTMF",
                            ComponentName = "UpperBandDTMF_z",
                        }
                    },
                },
            };

            var results = recognizer.Recognize(recording, config, 100.Seconds(), null, this.TestOutputDirectory, null);

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