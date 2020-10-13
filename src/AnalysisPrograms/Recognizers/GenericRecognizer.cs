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
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.Tracks;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool combineOverlappedEvents = false;

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
            var configuration = (GenericRecognizerConfig)genericConfig;

            if (configuration.Profiles?.Count < 1)
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

            // ############################### POST-PROCESSING OF GENERIC EVENTS ###############################
            // The following generic post-processing steps are determined by config settings.
            // Step 1: Combine overlapping events - events derived from all profiles.
            // Step 2: Combine possible syllable sequences and filter on excess syllable count.
            // Step 3: Remove events whose bandwidth is too small or large.
            // Step 4: Remove events that have excessive noise in their side-bands.

            Log.Debug($"Total event count BEFORE post-processing = {allResults.NewEvents.Count}");
            var postprocessingConfig = configuration.PostProcessing;

            // 1: Combine overlapping events.
            // This will be necessary where many small events have been found - possibly because the dB threshold is set low.
            if (postprocessingConfig.CombineOverlappingEvents)
            {
                allResults.NewEvents = CompositeEvent.CombineOverlappingEvents(allResults.NewEvents.Cast<EventCommon>().ToList());
                Log.Debug($"Event count after combining overlapped events = {allResults.NewEvents.Count}");
            }

            // 2: Combine proximal events, that is, events that may be a sequence of syllables in the same strophe.
            //    Can also use this parameter to combine events that are in the upper or lower neighbourhood.
            //    Such combinations will increase bandwidth of the event and this property can be used later to weed out unlikely events.
            var sequenceConfig = postprocessingConfig.SyllableSequence;

            if (sequenceConfig.NotNull() && sequenceConfig.CombinePossibleSyllableSequence)
            {
                // Must first convert events to spectral events.
                var spectralEvents1 = allResults.NewEvents.Cast<SpectralEvent>().ToList();
                var startDiff = sequenceConfig.SyllableStartDifference;
                var hertzDiff = sequenceConfig.SyllableHertzGap;
                allResults.NewEvents = CompositeEvent.CombineProximalEvents(spectralEvents1, TimeSpan.FromSeconds(startDiff), (int)hertzDiff);
                Log.Debug($"Event count after combining proximal events = {allResults.NewEvents.Count}");

                // Now filter on properties of the sequences which are treated as Composite events.
                if (sequenceConfig.FilterSyllableSequence)
                {
                    // filter on number of syllables and their periodicity.
                    var maxComponentCount = sequenceConfig.SyllableMaxCount;
                    var period = sequenceConfig.ExpectedPeriod;
                    var periodSd = sequenceConfig.PeriodStandardDeviation;
                    allResults.NewEvents = EventFilters.FilterEventsOnSyllableCountAndPeriodicity(allResults.NewEvents, maxComponentCount, period, periodSd);
                    Log.Debug($"Event count after filtering on periodicity = {allResults.NewEvents.Count}");
                }
            }

            // 3: Filter the events for time duration (seconds)
            if (postprocessingConfig.Duration != null)
            {
                var expectedEventDuration = postprocessingConfig.Duration.ExpectedDuration;
                var sdEventDuration = postprocessingConfig.Duration.DurationStandardDeviation;
                allResults.NewEvents = EventFilters.FilterOnDuration(allResults.NewEvents, expectedEventDuration, sdEventDuration, sigmaThreshold: 3.0);
                Log.Debug($"Event count after filtering on duration = {allResults.NewEvents.Count}");
            }

            // 4: Filter the events for bandwidth in Hertz
            if (postprocessingConfig.Bandwidth != null)
            {
                var expectedEventBandwidth = postprocessingConfig.Bandwidth.ExpectedBandwidth;
                var sdBandwidth = postprocessingConfig.Bandwidth.BandwidthStandardDeviation;
                allResults.NewEvents = EventFilters.FilterOnBandwidth(allResults.NewEvents, expectedEventBandwidth, sdBandwidth, sigmaThreshold: 3.0);
                Log.Debug($"Event count after filtering on bandwidth = {allResults.NewEvents.Count}");
            }

            // 5: Filter events on the amount of acoustic activity in their upper and lower neighbourhoods - their buffer zone.
            //    The idea is that an unambiguous event should have some acoustic space above and below.
            //    The filter requires that the average acoustic activity in each frame and bin of the upper and lower buffer zones should not exceed the user specified decibel threshold.
            var sidebandActivity = postprocessingConfig.SidebandActivity;
            if (sidebandActivity.UpperHertzBuffer > 0 || sidebandActivity.LowerHertzBuffer > 0)
            {
                var spectralEvents2 = allResults.NewEvents.Cast<SpectralEvent>().ToList();
                allResults.NewEvents = EventFilters.FilterEventsOnSidebandActivity(
                    spectralEvents2,
                    allResults.Sonogram,
                    sidebandActivity.LowerHertzBuffer,
                    sidebandActivity.UpperHertzBuffer,
                    segmentStartOffset,
                    sidebandActivity.DecibelBuffer);

                Log.Debug($"Event count after filtering on acoustic activity in upper/lower neighbourhood = {allResults.NewEvents.Count}");
            }

            // Write out the events to log.
            Log.Debug($"Final event count = {allResults.NewEvents.Count}.");
            if (allResults.NewEvents.Count > 0)
            {
                int counter = 0;
                foreach (var ev in allResults.NewEvents)
                {
                    counter++;
                    var spEvent = (SpectralEvent)ev;
                    Log.Debug($"  Event[{counter}]: Start={spEvent.EventStartSeconds:f1}; Duration={spEvent.EventDurationSeconds:f2}; Bandwidth={spEvent.BandWidthHertz} Hz");
                }
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
            /// <inheritdoc />
            public Dictionary<string, object> Profiles { get; set; }

            /// <summary>
            /// Gets or sets the post-processing config.
            /// Used to obtain parameters for all post-processing steps.
            /// </summary>
            public PostProcessingConfig PostProcessing { get; set; }
        }

        /// <summary>
        /// The properties in this config class are required to combine a sequence of similar syllables into a single event.
        /// </summary>
        public class PostProcessingConfig
        {
            /// <summary>
            /// Gets or sets a value indicating Whether or not to combine overlapping events.
            /// </summary>
            public bool CombineOverlappingEvents { get; set; }

            /// <summary>
            /// Gets or sets the parameters required to combine and filter syllable sequences.
            /// </summary>
            public SyllableSequenceConfig SyllableSequence { get; set; }

            /// <summary>
            /// Gets or sets the parameters required to filter events on the acoustic acticity in their sidebands.
            /// </summary>
            public SidebandConfig SidebandActivity { get; set; }

            /// <summary>
            /// Gets or sets the parameters required to filter events on their duration.
            /// </summary>
            public DurationConfig Duration { get; set; }

            /// <summary>
            /// Gets or sets the parameters required to filter events on their bandwidth.
            /// </summary>
            public BandwidthConfig Bandwidth { get; set; }
        }

        /// <summary>
        /// The next two properties determine filtering of events based on their duration.
        /// </summary>
        public class DurationConfig
        {
            /// <summary>
            /// Gets or sets a value indicating the Expected duration of an event.
            /// </summary>
            public double ExpectedDuration { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the standard deviation of the expected duration.
            /// </summary>
            public double DurationStandardDeviation { get; set; }
        }

        /// <summary>
        /// The next two properties determine filtering of events based on their bandwidth.
        /// </summary>
        public class BandwidthConfig
        {
            /// <summary>
            /// Gets or sets a value indicating the Expected bandwidth of an event.
            /// </summary>
            public int ExpectedBandwidth { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the standard deviation of the expected bandwidth.
            /// </summary>
            public int BandwidthStandardDeviation { get; set; }
        }

        /// <summary>
        /// The properties in this config class are required to filter events based on the amount of acoustic activity in their sidebands.
        /// </summary>
        public class SidebandConfig
        {
            /// <summary>
            /// Gets or sets a value indicating Whether or not to filter events based on acoustic conctent of upper buffer zone.
            /// If value = 0, the upper sideband is ignored.
            /// </summary>
            public int UpperHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets a value indicating Whether or not to filter events based on the acoustic content of their lower buffer zone.
            /// If value = 0, the lower sideband is ignored.
            /// </summary>
            public int LowerHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the decibel gap/difference between acoustic activity in the event and in the upper and lower buffer zones.
            /// BufferAcousticActivity must be LessThan (EventAcousticActivity - DecibelBuffer)
            /// This value is used only if LowerHertzBuffer > 0 OR UpperHertzBuffer > 0.
            /// </summary>
            public double DecibelBuffer { get; set; }
        }

        /// <summary>
        /// The properties in this config class are required to combine a sequence of similar syllables into a single event.
        /// The first three properties concern the combining of syllables into a sequence or stroph.
        /// The next four properties concern the filtering/removal of sequences that do not satisfy expected properties.
        /// </summary>
        public class SyllableSequenceConfig
        {
            // ################ The first three properties concern the combining of syllables into a sequence or stroph.

            /// <summary>
            /// Gets or sets a value indicating Whether or not to combine events that constitute a sequence of the same strophe.
            /// </summary>
            public bool CombinePossibleSyllableSequence { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the maximum allowable start time gap (seconds) between events within the same strophe.
            /// The gap between successive syllables is the "period" of the sequence.
            /// This value is used only where CombinePossibleSyllableSequence = true.
            /// </summary>
            public double SyllableStartDifference { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the maximum allowable difference (in Hertz) between the frequency bands of two events. I.e. events should be in similar frequency band.
            /// NOTE: SIMILAR frequency band means the differences between two top Hertz values and the two low Hertz values are less than hertzDifference.
            /// This value is used only where CombinePossibleSyllableSequence = true.
            /// </summary>
            public double SyllableHertzGap { get; set; }

            // ################ The next four properties concern the filtering/removal of sequences that do not satisfy expected properties.

            /// <summary>
            /// Gets or sets a value indicating Whether or not to remove/filter sequences having incorrect properties.
            /// </summary>
            public bool FilterSyllableSequence { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the maximum allowable number of syllables in a sequence.
            /// This value is used only where FilterSyllableSequence = true.
            /// </summary>
            public int SyllableMaxCount { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the expected periodicity in seconds.
            /// This value is used only where FilterSyllableSequence = true.
            /// Important Note: This property interacts with SyllableStartDifference.
            ///                 SyllableStartDifference - ExpectedPeriod = 3 x SD of the period.
            /// </summary>
            public double ExpectedPeriod { get; set; }

            /// <summary>
            /// Gets a value indicating the stadndard deviation of the expected period in seconds.
            /// This value is used only where FilterSyllableSequence = true.
            /// Important Note: This property is derived from two of the above properties.
            ///                 SD of the period = (SyllableStartDifference - ExpectedPeriod) / 3.
            ///                 The intent is that the maximum allowable syllable period is the expected value plus three times its standard deviation.
            /// </summary>
            public double PeriodStandardDeviation
            {
                get => (this.SyllableStartDifference - this.ExpectedPeriod) / 3;
            }
        }
    }
}