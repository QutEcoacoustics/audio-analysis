// <copyright file="GenericRecognizer.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Acoustics.Shared.ConfigFile;
    using AnalysisBase;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// This class calls algorithms for generic syllable/component types.
    /// </summary>
    public class GenericRecognizer : RecognizerBase
    {
        private bool combineOverlappedEvents = false;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public override string Author => "Ecosounds";

        /// <inheritdoc />
        public override string SpeciesName => "GenericRecognizer";

        /// <inheritdoc />
        public override string Description => "[ALPHA] Finds acoustic events with generic component detection algorithms";

        /// <inheritdoc />
        public override AnalyzerConfig ParseConfig(FileInfo file)
        {
            RuntimeHelpers.RunClassConstructor(typeof(GenericRecognizerConfig).TypeHandle);
            var result = ConfigFile.Deserialize<GenericRecognizerConfig>(file);

            this.combineOverlappedEvents = result.CombineOverlappedEvents;

            // validation of configs can be done here
            // sanity check the algorithm
            string algorithmName;
            foreach (var (profileName, profile) in result.Profiles)
            {
                if (profile is CommonParameters c)
                {
                    c.MinHertz.ConfigNotNull(nameof(c.MinHertz), file);
                    c.MaxHertz.ConfigNotNull(nameof(c.MaxHertz), file);
                }

                switch (profile)
                {
                    case BlobParameters _:
                        algorithmName = "Blob";
                        break;
                    case OscillationParameters _:
                        algorithmName = "Oscillation";
                        break;
                    case WhistleParameters _:
                        algorithmName = "Whistle";
                        break;
                    case HarmonicParameters _:
                        algorithmName = "Harmonics";
                        break;
                    case SpectralPeakTrackParameters _:
                        algorithmName = "SpectralTracks";
                        break;
                    case ClickParameters _:
                        algorithmName = "Click";
                        throw new NotImplementedException("The Click algorithm has not been implemented yet");
                        break;
                    case Aed.AedConfiguration _:
                        algorithmName = "AED";
                        break;
                    default:
                        var allowedAlgorithms =
                            $"{nameof(BlobParameters)}, {nameof(OscillationParameters)}, {nameof(WhistleParameters)}, {nameof(HarmonicParameters)}, {nameof(Aed.AedConfiguration)}";
                        throw new ConfigFileException($"The algorithm type in profile {profileName} is not recognized. It must be one of {allowedAlgorithms}");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override RecognizerResults Recognize(
            AudioRecording audioRecording,
            Config genericConfig,
            TimeSpan segmentStartOffset,
            Lazy<IndexCalculateResult[]> getSpectralIndexes,
            DirectoryInfo outputDirectory,
            int? imageWidth)
        {
            var configuration = (GenericRecognizerConfig)genericConfig;

            if (configuration.Profiles.NotNull() && configuration.Profiles.Count == 0)
            {
                throw new ConfigFileException(
                    "The generic recognizer needs at least one profile set. 0 were found.");
            }

            int count = configuration.Profiles.Count;
            var message = $"Found {count} analysis profile(s): " + configuration.Profiles.Keys.Join(", ");
            Log.Info(message);

            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // Now process each of the profiles
            foreach (var (profileName, profileConfig) in configuration.Profiles)
            {
                Log.Info("Processing profile: " + profileName);

                List<AcousticEvent> acousticEvents;
                var plots = new List<Plot>();
                SpectrogramStandard sonogram;

                Log.Debug($"Using the {profileName} algorithm... ");
                if (profileConfig is CommonParameters parameters)
                {
                    if (profileConfig is BlobParameters || profileConfig is OscillationParameters || profileConfig is WhistleParameters || profileConfig is HarmonicParameters || profileConfig is SpectralPeakTrackParameters)
                    {
                        sonogram = new SpectrogramStandard(ParametersToSonogramConfig(parameters), audioRecording.WavReader);

                        if (profileConfig is OscillationParameters op)
                        {
                            Oscillations2012.Execute(
                                sonogram,
                                op.MinHertz.Value,
                                op.MaxHertz.Value,
                                op.DctDuration,
                                op.MinOscillationFrequency,
                                op.MaxOscillationFrequency,
                                op.DctThreshold,
                                op.EventThreshold,
                                op.MinDuration.Value,
                                op.MaxDuration.Value,
                                out var scores,
                                out acousticEvents,
                                out var hits,
                                segmentStartOffset);

                            //plots.Add(new Plot($"{profileName} (:OscillationScore)", scores, op.EventThreshold));
                            var plot = PreparePlot(scores, $"{profileName} (:OscillationScore)", op.EventThreshold);
                            plots.Add(plot);
                        }
                        else if (profileConfig is BlobParameters bp)
                        {
                            //get the array of intensity values minus intensity in side/buffer bands.
                            //i.e. require silence in side-bands. Otherwise might simply be getting part of a broader band acoustic event.
                            var decibelArray = SNR.CalculateFreqBandAvIntensityMinusBufferIntensity(
                                sonogram.Data,
                                bp.MinHertz.Value,
                                bp.MaxHertz.Value,
                                bp.BottomHertzBuffer.Value,
                                bp.TopHertzBuffer.Value,
                                sonogram.NyquistFrequency);

                            // prepare plot of resultant blob decibel array.
                            var plot = PreparePlot(decibelArray, $"{profileName} (Blob:db Intensity)", bp.DecibelThreshold.Value);
                            plots.Add(plot);

                            // iii: CONVERT blob decibel SCORES TO ACOUSTIC EVENTS.
                            // Note: This method does NOT do prior smoothing of the dB array.
                            acousticEvents = AcousticEvent.GetEventsAroundMaxima(
                                decibelArray,
                                segmentStartOffset,
                                bp.MinHertz.Value,
                                bp.MaxHertz.Value,
                                bp.DecibelThreshold.Value,
                                TimeSpan.FromSeconds(bp.MinDuration.Value),
                                TimeSpan.FromSeconds(bp.MaxDuration.Value),
                                sonogram.FramesPerSecond,
                                sonogram.FBinWidth);
                        }
                        else if (profileConfig is WhistleParameters wp)
                        {
                            //get the array of intensity values minus intensity in side/buffer bands.
                            double[] decibelArray;
                            (acousticEvents, decibelArray) = WhistleParameters.GetWhistles(
                                sonogram,
                                wp.MinHertz.Value,
                                wp.MaxHertz.Value,
                                sonogram.NyquistFrequency,
                                wp.DecibelThreshold.Value,
                                wp.MinDuration.Value,
                                wp.MaxDuration.Value,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (Whistle:dB Intensity)", wp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is HarmonicParameters hp)
                        {
                            double[] decibelMaxArray;
                            double[] harmonicIntensityScores;
                            (acousticEvents, decibelMaxArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
                                sonogram,
                                hp.MinHertz.Value,
                                hp.MaxHertz.Value,
                                sonogram.NyquistFrequency,
                                hp.DecibelThreshold.Value,
                                hp.DctThreshold.Value,
                                hp.MinDuration.Value,
                                hp.MaxDuration.Value,
                                hp.MinFormantGap.Value,
                                hp.MaxFormantGap.Value,
                                segmentStartOffset);

                            var plot = PreparePlot(harmonicIntensityScores, $"{profileName} (Harmonics:dct intensity)", hp.DctThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is SpectralPeakTrackParameters tp)
                        {
                            double[] decibelArray;
                            (acousticEvents, decibelArray) = SpectralPeakTrackParameters.GetSpectralPeakTracks(
                                sonogram,
                                tp.MinHertz.Value,
                                tp.MaxHertz.Value,
                                sonogram.NyquistFrequency,
                                tp.DecibelThreshold.Value,
                                tp.MinDuration.Value,
                                tp.MaxDuration.Value,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (SpectralPeaks:dB Intensity)", tp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is ClickParameters cp)
                        {
                            double[] decibelArray;
                            (acousticEvents, decibelArray) = ClickParameters.GetClicks(
                                sonogram,
                                cp.MinHertz.Value,
                                cp.MaxHertz.Value,
                                sonogram.NyquistFrequency,
                                cp.DecibelThreshold.Value,
                                cp.MinBandwidthHertz.Value,
                                cp.MaxBandwidthHertz.Value,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (Click:dB Intensity)", cp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                    //iV add additional info to the acoustic events
                    acousticEvents.ForEach(ae =>
                    {
                        ae.FileName = audioRecording.BaseName;
                        ae.SpeciesName = parameters.SpeciesName;
                        ae.Name = parameters.ComponentName;
                        ae.Profile = profileName;
                        ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                        ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                        ae.SetTimeAndFreqScales(sonogram.FrameStep, sonogram.FrameDuration, sonogram.FBinWidth);
                    });
                }
                else if (profileConfig is Aed.AedConfiguration ac)
                {
                    var config = new SonogramConfig
                    {
                        NoiseReductionType = ac.NoiseReductionType,
                        NoiseReductionParameter = ac.NoiseReductionParameter,
                    };
                    sonogram = new SpectrogramStandard(config, audioRecording.WavReader);

                    acousticEvents = Aed.CallAed(sonogram, ac, segmentStartOffset, audioRecording.Duration).ToList();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                // combine the results i.e. add the events list of call events.
                allResults.Events.AddRange(acousticEvents);
                allResults.Plots.AddRange(plots);

                // effectively keeps only the *last* sonogram produced
                allResults.Sonogram = sonogram;
                Log.Debug($"{profileName} event count = {acousticEvents.Count}");

                // DEBUG PURPOSES COMMENT NEXT LINE
                //SaveDebugSpectrogram(allResults, genericConfig, outputDirectory, "name");
            }

            // combine adjacent acoustic events
            if (this.combineOverlappedEvents)
            {
                allResults.Events = AcousticEvent.CombineOverlappingEvents(allResults.Events, segmentStartOffset);
            }

            return allResults;
        }

        /*
        /// <summary>
        /// Summarize your results. This method is invoked exactly once per original file.
        /// </summary>
        public override void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // No operation - do nothing. Feel free to add your own logic.
            base.SummariseResults(settings, inputFileSegment, events, indices, spectralIndices, results);
        }
        */

        private static SonogramConfig ParametersToSonogramConfig(CommonParameters common)
        {
            int windowSize = (int)common.FrameSize;
            int windowStep = (int)common.FrameStep;
            return new SonogramConfig()
            {
                WindowSize = windowSize,
                WindowStep = windowStep,
                WindowOverlap = (windowSize - windowStep) / (double)windowSize,
                WindowFunction = (string)common.WindowFunction,
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = common.BgNoiseThreshold ?? 0.0,
            };
        }

        /// <summary>
        /// Prepares a plot of an array of score values.
        /// To obtain a more useful display, the maximum display value is set to 3 times the threshold value.
        /// </summary>
        /// <param name="array">an array of double.</param>
        /// <param name="title">to accompany the plot.</param>
        /// <param name="threshold">A threshold value to be drawn on the plot.</param>
        /// <returns>the plot.</returns>
        private static Plot PreparePlot(double[] array, string title, double threshold)
        {
            double intensityNormalizationMax = 3 * threshold;
            var eventThreshold = threshold / intensityNormalizationMax;
            var normalisedIntensityArray = DataTools.NormaliseInZeroOne(array, 0, intensityNormalizationMax);
            var plot = new Plot(title, normalisedIntensityArray, eventThreshold);
            return plot;
        }

        /// <summary>
        /// THis method can be modified if want to do something non-standard with the output spectrogram.
        /// </summary>
        public static void SaveDebugSpectrogram(RecognizerResults results, Config genericConfig, DirectoryInfo outputDirectory, string baseName)
        {
            var image3 = SpectrogramTools.GetSonogramPlusCharts(results.Sonogram, results.Events, results.Plots, null);

            image3.Save(Path.Combine(outputDirectory.FullName, baseName + ".profile.png"));
        }

        /// <inheritdoc cref="RecognizerConfig"/> />
        public class GenericRecognizerConfig : RecognizerConfig, INamedProfiles<object>
        {
            public bool CombineOverlappedEvents { get; set; }

            /// <inheritdoc />
            public Dictionary<string, object> Profiles { get; set; }
        }
    }
}