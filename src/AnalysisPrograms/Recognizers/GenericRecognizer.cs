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

        public override string Author => "Ecosounds";

        public override string SpeciesName => "GenericRecognizer";

        public override string Description => "[ALPHA] Finds acoustic events with generic component detection algorithms";

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
                    c.MinHertz.NotNull(file);
                    c.MaxHertz.NotNull(file);
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

            // ############################### PRE-PROCESSING ###############################
            // may at some future date do pre-processing

            // ############################### PROCESSING: DETECTION OF GENERIC EVENTS ###############################
            var results = RunProfiles(audioRecording, configuration, segmentStartOffset);

            // ############################### POST-PROCESSING OF GENERIC EVENTS ###############################

            var postprocessingConfig = configuration.PostProcessing;
            var postEvents = new List<EventCommon>();

            // count number of events detected at each decibel threshold.
            for (int i = 1; i <= 39; i++)
            {
                var dbEvents = EventFilters.FilterOnDecibelDetectionThreshold(results.NewEvents, (double)i);

                if (dbEvents.Count > 0)
                {
                    //Log.Debug($"Profiles detected {dbEvents.Count} events at threshold {i} dB.");
                    var ppEvents = EventPostProcessing.PostProcessingOfSpectralEvents(
                        dbEvents,
                        postprocessingConfig,
                        (double)i,
                        results.Sonogram,
                        segmentStartOffset);

                    postEvents.AddRange(ppEvents);
                }
            }

            // Running profiles with multiple dB thresholds produces nested (Russian doll) events.
            // Remove all but the outermost event.
            // Add a spacer for easier reading of the debug output.
            Log.Debug($" ");
            Log.Debug($"Event count BEFORE removing enclosed events = {postEvents.Count}.");
            results.NewEvents = CompositeEvent.RemoveEnclosedEvents(postEvents);
            Log.Debug($"Event count AFTER  removing enclosed events = {postEvents.Count}.");

            // Write out the events to log.
            //Log.Debug($"FINAL event count = {postEvents.Count}.");
            if (postEvents.Count > 0)
            {
                int counter = 0;
                foreach (var ev in postEvents)
                {
                    counter++;
                    var spEvent = (SpectralEvent)ev;
                    Log.Debug($"  Event[{counter}]: Start={spEvent.EventStartSeconds:f1}; Duration={spEvent.EventDurationSeconds:f2}; Bandwidth={spEvent.BandWidthHertz} Hz");
                }
            }

            //results.NewEvents = EventPostProcessing.PostProcessingOfSpectralEvents(
            //    results.NewEvents,
            //    postprocessingConfig,
            //    results.Sonogram,
            //    segmentStartOffset);
            return results;
        }

        public static RecognizerResults RunProfiles(
            AudioRecording audioRecording,
            GenericRecognizerConfig configuration,
            TimeSpan segmentStartOffset)
        {
            var combinedResults = new RecognizerResults()
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
                string speciesName;

                //Mostly the config will be for generic events ...
                if (profileConfig is CommonParameters parameters)
                {
                    speciesName = parameters.SpeciesName;
                    var decibelThresholds = parameters.DecibelThresholds;
                    var message = $"Number of decibel thresholds = {decibelThresholds.Length}: " + decibelThresholds.Join(", ");
                    Log.Info(message);

                    var spectrogram = new SpectrogramStandard(ParametersToSonogramConfig(parameters), audioRecording.WavReader);

                    var profileResults = new RecognizerResults();
                    foreach (var threshold in decibelThresholds)
                    {
                        var thresholdResults = RunOneProfile(spectrogram, profileName, parameters, threshold, segmentStartOffset);

                        //Log.Debug($"Event count from all profiles at {threshold} dB threshold = {results.NewEvents.Count}");
                        profileResults.NewEvents.AddRange(thresholdResults.NewEvents);
                        profileResults.Plots.AddRange(thresholdResults.Plots);
                        profileResults.Sonogram = thresholdResults.Sonogram;
                    }

                    // Add additional info to the remaining acoustic events
                    profileResults.NewEvents.ForEach(ae =>
                    {
                        ae.FileName = audioRecording.BaseName;
                        ae.Name = speciesName;
                        ae.Profile = profileName;
                    });

                    Log.Debug($"Profile {profileName}: event count = {profileResults.NewEvents.Count}");
                    combinedResults.NewEvents.AddRange(profileResults.NewEvents);
                    combinedResults.Plots.AddRange(profileResults.Plots);
                    combinedResults.Sonogram = spectrogram;
                }
                else if (profileConfig is Aed.AedConfiguration ac)
                {
                    // ... but may be calling the old AED algorithm.
                    var config = new SonogramConfig
                    {
                        NoiseReductionType = ac.NoiseReductionType,
                        NoiseReductionParameter = ac.NoiseReductionParameter,
                    };

                    speciesName = "aed";
                    var spectrogram = new SpectrogramStandard(config, audioRecording.WavReader);

                    // GET THIS TO RETURN BLOB EVENTS.
                    var spectralEvents = Aed.CallAed(spectrogram, ac, segmentStartOffset, audioRecording.Duration).ToList();

                    // Add additional info to the acoustic events
                    spectralEvents.ForEach(ae =>
                    {
                        ae.FileName = audioRecording.BaseName;
                        ae.Name = speciesName;
                        ae.Profile = profileName;
                    });

                    // AED does not return plots.
                    combinedResults.NewEvents.AddRange(spectralEvents);
                    combinedResults.Sonogram = spectrogram;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            return combinedResults;
        }

        /// <summary>
        /// Gets the events for one profile at one decibel threshold.
        /// </summary>
        /// <param name="spectrogram">Spectrogram derived from audio segment.</param>
        /// <param name="profileName">Profile name in the config file.</param>
        /// <param name="decibelThreshold">Threshold for this pass.</param>
        /// <param name="segmentStartOffset">The same for any given recording segment.</param>
        /// <returns>A results object.</returns>
        public static RecognizerResults RunOneProfile(
            SpectrogramStandard spectrogram,
            string profileName,
            CommonParameters profileConfig,
            double? decibelThreshold,
            TimeSpan segmentStartOffset)
        {
            List<EventCommon> spectralEvents;
            var plots = new List<Plot>();

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

            // put events, plots and spectrogram into a results object.
            var allResults = new RecognizerResults()
            {
                Events = new List<AcousticEvent>(),
                NewEvents = new List<EventCommon>(),
                Hits = null,
                ScoreTrack = null,
                Plots = new List<Plot>(),
                Sonogram = null,
            };

            //add info about decibel threshold into the event.
            //This info is used later during post-processing of events.
            foreach (var ev in spectralEvents)
            {
                ev.DecibelDetectionThreshold = decibelThreshold.Value;
            }

            allResults.NewEvents.AddRange(spectralEvents);
            allResults.Plots.AddRange(plots);
            allResults.Sonogram = spectrogram;
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
                WindowFunction = common.WindowFunction?.ToString(),
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = common.BgNoiseThreshold ?? 0.0,
            };
        }

        /// <summary>
        /// A generic recognizer is a user-defined combinations of component
        /// algorithms.
        /// </summary>
        public class GenericRecognizerConfig : RecognizerConfig, INamedProfiles<object>
        {
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