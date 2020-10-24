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
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.Tracks;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using SixLabors.ImageSharp;
    using TowseyLibrary;
    using static AudioAnalysisTools.Events.Types.EventPostProcessing;
    using Path = System.IO.Path;

    /// <summary>
    /// This class calls algorithms for generic syllable/component types.
    /// </summary>
    public class GenericRecognizer : RecognizerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private readonly bool combineOverlappedEvents = false;

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

            // validation of configs can be done here
            ValidateProfileTagsMatchAlgorithms(result.Profiles, file);

            return result;
        }

        public static void ValidateProfileTagsMatchAlgorithms(Dictionary<string, object> profiles, FileInfo file)
        {
            // validation of configs can be done here
            // sanity check the algorithm
            foreach (var (profileName, profile) in profiles)
            {
                if (profile is CommonParameters c)
                {
                    c.MinHertz.ConfigNotNull(nameof(c.MinHertz), file);
                    c.MaxHertz.ConfigNotNull(nameof(c.MaxHertz), file);
                }

                string algorithmName;
                switch (profile)
                {
                    case BlobParameters _:
                        algorithmName = "Blob";
                        break;
                    case OscillationParameters _:
                        algorithmName = "Oscillation";
                        break;
                    case OnebinTrackParameters _:
                        algorithmName = "Whistle";
                        break;
                    case HarmonicParameters _:
                        algorithmName = "Harmonics";
                        break;
                    case ForwardTrackParameters _:
                        algorithmName = "SpectralTrack";
                        break;
                    case UpwardTrackParameters _:
                        algorithmName = "VerticalTrack";
                        break;
                    case OneframeTrackParameters _:
                        algorithmName = "Click";
                        break;
                    case Aed.AedConfiguration _:
                        algorithmName = "AED";
                        break;
                    default:
                        var allowedAlgorithms =
                            $"{nameof(BlobParameters)}," +
                            $"{nameof(OscillationParameters)}," +
                            $"{nameof(OnebinTrackParameters)}," +
                            $"{nameof(HarmonicParameters)}," +
                            $"{nameof(ForwardTrackParameters)}," +
                            $"{nameof(UpwardTrackParameters)}," +
                            $"{nameof(OneframeTrackParameters)}," +
                            $"{nameof(Aed.AedConfiguration)}";
                        throw new ConfigFileException($"The algorithm type in profile {profileName} is not recognized. It must be one of {allowedAlgorithms}");
                }
            }
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
            // ############################### PRE-PROCESSING: PREPARATION FOR DETECTION OF GENERIC EVENTS ###############################
            var configuration = (GenericRecognizerConfig)genericConfig;

            if (configuration.Profiles?.Count < 1)
            {
                throw new ConfigFileException("The generic recognizer needs at least one profile set. Zero were found.");
            }

            int count = configuration.Profiles.Count;
            var message = $"Found {count} analysis profile(s): " + configuration.Profiles.Keys.Join(", ");
            Log.Info(message);

            var decibelThresholds = configuration.DecibelThresholds;
            message = $"Number of decibel thresholds = {decibelThresholds.Length}: " + decibelThresholds.Join(", ");
            Log.Info(message);

            // init object to store the combined results from all decibel thresholds.
            var combinedResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            foreach (var threshold in decibelThresholds)
            {
                // ############################### PROCESSING: DETECTION OF GENERIC EVENTS ###############################
                var profileResults = RunProfiles(audioRecording, configuration, threshold, segmentStartOffset);

                // ############################### POST-PROCESSING OF GENERIC EVENTS ###############################
                var postprocessingConfig = configuration.PostProcessing;
                profileResults.NewEvents = EventPostProcessing.PostProcessingOfSpectralEvents(profileResults.NewEvents, threshold.Value, postprocessingConfig, profileResults.Sonogram, segmentStartOffset);
                Log.Debug($"Event count from all profiles at {threshold} dB threshold = {profileResults.NewEvents.Count}");

                // combine the results i.e. add the events list of call events.
                combinedResults.NewEvents.AddRange(profileResults.NewEvents);
                combinedResults.Plots.AddRange(profileResults.Plots);

                // effectively keeps only the *last* sonogram produced
                combinedResults.Sonogram = profileResults.Sonogram;
            }

            combinedResults.NewEvents = CompositeEvent.RemoveEnclosedEvents(combinedResults.NewEvents);
            return combinedResults;
        }

        public static RecognizerResults RunProfiles(
            AudioRecording audioRecording,
            GenericRecognizerConfig configuration,
            double? decibelThreshold,
            TimeSpan segmentStartOffset)
        {
            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            // Now process each of the profiles
            foreach (var (profileName, profileConfig) in configuration.Profiles)
            {
                Log.Info("Processing profile: " + profileName);

                //List<AcousticEvent> acousticEvents;
                List<EventCommon> spectralEvents;
                var plots = new List<Plot>();
                SpectrogramStandard spectrogram;

                //Log.Debug($"Using the {profileName} algorithm... ");
                if (profileConfig is CommonParameters parameters)
                {
                    if (profileConfig is BlobParameters
                        || profileConfig is OscillationParameters
                        || profileConfig is OnebinTrackParameters
                        || profileConfig is HarmonicParameters
                        || profileConfig is ForwardTrackParameters
                        || profileConfig is UpwardTrackParameters
                        || profileConfig is OneframeTrackParameters)
                    {
                        spectrogram = new SpectrogramStandard(ParametersToSonogramConfig(parameters), audioRecording.WavReader);

                        if (profileConfig is BlobParameters bp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = BlobEvent.GetBlobEvents(
                                spectrogram,
                                bp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is OnebinTrackParameters wp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = OnebinTrackAlgorithm.GetOnebinTracks(
                                spectrogram,
                                wp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is ForwardTrackParameters tp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = ForwardTrackAlgorithm.GetForwardTracks(
                                spectrogram,
                                tp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is OneframeTrackParameters cp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = OneframeTrackAlgorithm.GetOneFrameTracks(
                                spectrogram,
                                cp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is UpwardTrackParameters vtp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = UpwardTrackAlgorithm.GetUpwardTracks(
                                spectrogram,
                                vtp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is HarmonicParameters hp)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = HarmonicParameters.GetComponentsWithHarmonics(
                                spectrogram,
                                hp,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
                        }
                        else if (profileConfig is OscillationParameters op)
                        {
                            List<Plot> decibelPlots;
                            (spectralEvents, decibelPlots) = Oscillations2012.GetComponentsWithOscillations(
                                spectrogram,
                                op,
                                decibelThreshold,
                                segmentStartOffset,
                                profileName);

                            plots.AddRange(decibelPlots);
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
                    spectralEvents.ForEach(ae =>
                    {
                        ae.FileName = audioRecording.BaseName;
                        ae.Name = parameters.SpeciesName;
                        ae.Profile = profileName;

                        //ae.SegmentDurationSeconds = audioRecording.Duration.TotalSeconds;
                        //ae.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
                        //ae.SetTimeAndFreqScales(sonogram.FrameStep, sonogram.FrameDuration, sonogram.FBinWidth);
                    });
                }
                else if (profileConfig is Aed.AedConfiguration ac)
                {
                    var config = new SonogramConfig
                    {
                        NoiseReductionType = ac.NoiseReductionType,
                        NoiseReductionParameter = ac.NoiseReductionParameter,
                    };
                    spectrogram = new SpectrogramStandard(config, audioRecording.WavReader);

                    // GET THIS TO RETURN BLOB EVENTS.
                    spectralEvents = Aed.CallAed(spectrogram, ac, segmentStartOffset, audioRecording.Duration).ToList();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                // combine the results i.e. add the events list of call events.
                allResults.NewEvents.AddRange(spectralEvents);
                allResults.Plots.AddRange(plots);

                // effectively keeps only the *last* sonogram produced
                allResults.Sonogram = spectrogram;
                Log.Debug($"Profile {profileName}: event count = {spectralEvents.Count}");
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

        /// <summary>
        /// THis method can be modified if want to do something non-standard with the output spectrogram.
        /// </summary>
        public static string SaveDebugSpectrogram(RecognizerResults results, Config genericConfig, DirectoryInfo outputDirectory, string baseName)
        {
            var image3 = SpectrogramTools.GetSonogramPlusCharts(results.Sonogram, results.NewEvents, results.Plots, null);

            var path = Path.Combine(outputDirectory.FullName, baseName + ".profile.png");
            image3.Save(path);

            return path;
        }

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

        /// <inheritdoc cref="GenericRecognizerConfig"/> />
        public class GenericRecognizerConfig : RecognizerConfig, INamedProfiles<object>
        {
            /// <summary>
            /// Gets or sets an array of decibel thresholds.
            /// Each threshold determines the minimum "loudness" of an event that can be detected.
            /// Units are decibels.
            /// </summary>
            public double?[] DecibelThresholds { get; set; }

            /// <inheritdoc />
            public Dictionary<string, object> Profiles { get; set; }

            /// <summary>
            /// Gets or sets the post-processing config.
            /// Used to obtain parameters for all post-processing steps.
            /// </summary>
            public PostProcessingConfig PostProcessing { get; set; }
        }
    }
}