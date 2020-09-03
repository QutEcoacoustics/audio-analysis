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
                SpectrogramStandard sonogram;

                Log.Debug($"Using the {profileName} algorithm... ");
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
                        sonogram = new SpectrogramStandard(ParametersToSonogramConfig(parameters), audioRecording.WavReader);

                        if (profileConfig is BlobParameters bp)
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
                            var acEvents = AcousticEvent.GetEventsAroundMaxima(
                                decibelArray,
                                segmentStartOffset,
                                bp.MinHertz.Value,
                                bp.MaxHertz.Value,
                                bp.DecibelThreshold.Value,
                                TimeSpan.FromSeconds(bp.MinDuration.Value),
                                TimeSpan.FromSeconds(bp.MaxDuration.Value),
                                sonogram.FramesPerSecond,
                                sonogram.FBinWidth);
                            spectralEvents = acEvents.ConvertAcousticEventsToSpectralEvents();
                        }
                        else if (profileConfig is OnebinTrackParameters wp)
                        {
                            //get the array of intensity values minus intensity in side/buffer bands.
                            double[] decibelArray;
                            (spectralEvents, decibelArray) = OnebinTrackAlgorithm.GetOnebinTracks(
                                sonogram,
                                wp,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (Whistle:dB Intensity)", wp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is ForwardTrackParameters tp)
                        {
                            double[] decibelArray;
                            (spectralEvents, decibelArray) = ForwardTrackAlgorithm.GetForwardTracks(
                                sonogram,
                                tp,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (Chirps:dB Intensity)", tp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is OneframeTrackParameters cp)
                        {
                            double[] decibelArray;
                            (spectralEvents, decibelArray) = OneframeTrackAlgorithm.GetOneFrameTracks(
                                sonogram,
                                cp,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (Clicks:dB Intensity)", cp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is UpwardTrackParameters vtp)
                        {
                            double[] decibelArray;
                            (spectralEvents, decibelArray) = UpwardTrackAlgorithm.GetUpwardTracks(
                                sonogram,
                                vtp,
                                segmentStartOffset);

                            var plot = PreparePlot(decibelArray, $"{profileName} (VerticalTrack:dB Intensity)", vtp.DecibelThreshold.Value);
                            plots.Add(plot);
                        }
                        else if (profileConfig is HarmonicParameters hp)
                        {
                            double[] decibelMaxArray;
                            double[] harmonicIntensityScores;
                            (spectralEvents, decibelMaxArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
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
                        else if (profileConfig is OscillationParameters op)
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
                                out var oscillationEvents,
                                out var hits,
                                segmentStartOffset);

                            spectralEvents = new List<EventCommon>(oscillationEvents);

                            //plots.Add(new Plot($"{profileName} (:OscillationScore)", scores, op.EventThreshold));
                            var plot = PreparePlot(scores, $"{profileName} (:OscillationScore)", op.EventThreshold);
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
                    sonogram = new SpectrogramStandard(config, audioRecording.WavReader);

                    // GET THIS TO RETURN BLOB EVENTS.
                    spectralEvents = Aed.CallAed(sonogram, ac, segmentStartOffset, audioRecording.Duration).ToList();
                }
                else
                {
                    throw new InvalidOperationException();
                }

                // combine the results i.e. add the events list of call events.
                allResults.NewEvents.AddRange(spectralEvents);
                allResults.Plots.AddRange(plots);

                // effectively keeps only the *last* sonogram produced
                allResults.Sonogram = sonogram;
                Log.Debug($"{profileName} event count = {spectralEvents.Count}");

                // DEBUG PURPOSES COMMENT NEXT LINE
                //SaveDebugSpectrogram(allResults, genericConfig, outputDirectory, "name");
            }

            // ######################################################## POST-PROCESSING OF GENERIC EVENTS

            Log.Debug($"Total event count = {allResults.NewEvents.Count}");

            // 1: Combine overlapping events.
            // This will be necessary where many small events have been found - possibly because the dB threshold is set low.
            if (configuration.CombineOverlappingEvents)
            {
                allResults.NewEvents = CompositeEvent.CombineOverlappingEvents(allResults.NewEvents.Cast<EventCommon>().ToList());
                Log.Debug($"Event count after combining overlapped events = {allResults.NewEvents.Count}");
            }

            // 2: Combine proximal events, that is, events that may be a sequence of syllables in the same strophe.
            //    Can also use this parameter to combine events that are in the upper or lower neighbourhood.
            //    Such combinations will increase bandwidth of the event and this property can be used later to weed out unlikely events.
            var sequenceConfig = configuration.SyllableSequence;

            if (sequenceConfig.CombinePossibleSyllableSequence)
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
                    // filter on number of components
                    var maxComponentCount = sequenceConfig.SyllableMaxCount;
                    allResults.NewEvents = EventExtentions.FilterEventsOnCompositeContent(allResults.NewEvents, maxComponentCount);
                    Log.Debug($"Event count after filtering on component count = {allResults.NewEvents.Count}");

                    // filter on syllable periodicity
                    var period = sequenceConfig.ExpectedPeriod;
                    var periodSd = sequenceConfig.PeriodStdDev;
                    allResults.NewEvents = EventExtentions.FilterEventsOnSyllablePeriodicity(allResults.NewEvents, period, periodSd);
                    Log.Debug($"Event count after filtering on component count = {allResults.NewEvents.Count}");
                }
            }

            // 3: Filter the events for bandwidth in Hertz
            var expectedEventBandwidth = configuration.ExpectedBandwidth;
            var sd = configuration.BandwidthStandardDeviation;
            allResults.NewEvents = EventExtentions.FilterOnBandwidth(allResults.NewEvents, expectedEventBandwidth, sd, sigmaThreshold: 3.0);
            Log.Debug($"Event count after filtering on bandwidth = {allResults.NewEvents.Count}");

            // 4: Filter events on the amount of acoustic activity in their upper and lower neighbourhoods - their buffer zone.
            //    The idea is that an unambiguous event should have some acoustic space above and below.
            //    The filter requires that the average acoustic activity in each frame and bin of the upper and lower buffer zones should not exceed the user specified decibel threshold.
            if (configuration.NeighbourhoodUpperHertzBuffer > 0 || configuration.NeighbourhoodLowerHertzBuffer > 0)
            {
                var spectralEvents2 = allResults.NewEvents.Cast<SpectralEvent>().ToList();
                allResults.NewEvents = EventExtentions.FilterEventsOnNeighbourhood(
                    spectralEvents2,
                    allResults.Sonogram,
                    configuration.NeighbourhoodLowerHertzBuffer,
                    configuration.NeighbourhoodUpperHertzBuffer,
                    segmentStartOffset,
                    configuration.NeighbourhoodDecibelBuffer);

                Log.Debug($"Event count after filtering on acoustic activity in upper/lower neighbourhood = {allResults.NewEvents.Count}");
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

            // ########### THE FOLLOWING PROPERTIES ARE FOR POST-PROCESSING OF EVeNTS.

            /// <summary>
            /// Gets or sets a value indicating Whether or not to combine overlapping events.
            /// </summary>
            public bool CombineOverlappingEvents { get; set; }

            /// <summary>
            /// Gets or sets the parameters required to combine and filter syllable sequences.
            /// </summary>
            public SyllableSequenceConfig SyllableSequence { get; set; }

            // #### The next two properties determine filtering of events based on their bandwidth

            /// <summary>
            /// Gets or sets a value indicating the Expected bandwidth of an event.
            /// </summary>
            public int ExpectedBandwidth { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the standard deviation of the expected bandwidth.
            /// </summary>
            public int BandwidthStandardDeviation { get; set; }

            // #### The next three properties determine filtering of events based on acoustic conctent of upper and lower buffer zones.

            /// <summary>
            /// Gets or sets a value indicating Whether or not to filter events based on acoustic conctent of upper buffer zone.
            /// If value = 0, the upper neighbourhood is ignored.
            /// </summary>
            public int NeighbourhoodUpperHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets a value indicating Whether or not to filter events based on the acoustic content of their lower buffer zone.
            /// If value = 0, the lower neighbourhood is ignored.
            /// </summary>
            public int NeighbourhoodLowerHertzBuffer { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the decibel gap/difference between acoustic activity in the event and in the upper/lower buffer zones.
            /// BufferAcousticActivity must be LessThan (EventAcousticActivity - NeighbourhoodDecibelBuffer)
            /// This value is used only if NeighbourhoodLowerHertzBuffer > 0 OR NeighbourhoodUpperHertzBuffer > 0.
            /// </summary>
            public double NeighbourhoodDecibelBuffer { get; set; }
        }

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
            public double PeriodStdDev
            {
                get => (this.SyllableStartDifference - this.ExpectedPeriod) / 3;
            }
        }
    }
}