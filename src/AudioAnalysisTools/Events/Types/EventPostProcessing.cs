// <copyright file="EventPostProcessing.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Types
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;

    public static class EventPostProcessing
    {
        private const float SigmaThreshold = 3.0F;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This method post-processes a set of acoustic events that have been detected by all profiles at the passed decibel threshold.
        /// </summary>
        /// <param name="newEvents">A list of events before post-processing.</param>
        /// <param name="postprocessingConfig">The config file to be used for post-processing.</param>
        /// <param name="decibelThreshold">Decibel threshold used to detect the passed events.</param>
        /// <param name="spectrogram">A spectrogram of the events.</param>
        /// <param name="segmentStartOffset">Time  in seconds since beginning of the recording.</param>
        /// <returns>A list of events after post-processing.</returns>
        public static List<EventCommon> PostProcessingOfSpectralEvents(
            List<EventCommon> newEvents,
            PostProcessingConfig postprocessingConfig,
            double decibelThreshold,
            BaseSonogram spectrogram,
            TimeSpan segmentStartOffset)
        {
            // The following generic post-processing steps are determined by config settings.
            // Step 1: Combine overlapping events - events derived from profiles.
            // Step 2: Combine possible syllable sequences and filter on excess syllable count.
            // Step 3: Remove events whose duration is too small or large.
            // Step 4: Remove events whose bandwidth is too small or large.
            // Step 5: Remove events that have excessive noise in their side-bands.

            Log.Debug($"\nBEFORE post-processing.");
            Log.Debug($"TOTAL EVENTS detected by profiles at {decibelThreshold:F0} dB threshold = {newEvents.Count}");

            // 1: Combine overlapping events.
            // This will be necessary where many small events have been found - possibly because the dB threshold is set low.
            if (postprocessingConfig.CombineOverlappingEvents && (newEvents.Count > 0))
            {
                Log.Debug($"COMBINE EVENTS HAVING TEMPORAL&SPECTRAL OVERLAP");
                newEvents = CompositeEvent.CombineOverlappingEvents(newEvents.Cast<EventCommon>().ToList());
                Log.Debug($" Event count after combining overlapped events = {newEvents.Count}");
            }

            // 2: Combine proximal events, that is, events that may be a sequence of syllables in the same strophe.
            //    Can also use this parameter to combine events that are in the upper or lower neighbourhood.
            //    Such combinations will increase bandwidth of the event and this property can be used later to weed out unlikely events.
            var sequenceConfig = postprocessingConfig.SyllableSequence;

            if (sequenceConfig.NotNull() && (newEvents.Count > 0))
            {
                Log.Debug($"COMBINE PROXIMAL EVENTS");

                // Must first convert events to spectral events.
                var spectralEvents1 = newEvents.Cast<SpectralEvent>().ToList();
                var startDiff = sequenceConfig.SyllableStartDifference;
                var hertzDiff = sequenceConfig.SyllableHertzGap;
                newEvents = CompositeEvent.CombineProximalEvents(spectralEvents1, TimeSpan.FromSeconds(startDiff), (int)hertzDiff);
                Log.Debug($" Event count after combining proximal events = {newEvents.Count}");

                // Now filter on properties of the sequences which are treated as Composite events.
                if (sequenceConfig.FilterSyllableSequence)
                {
                    // filter on number of syllables and their periodicity.
                    var maxComponentCount = sequenceConfig.SyllableMaxCount;
                    var periodAv = sequenceConfig.ExpectedPeriod;
                    var periodSd = sequenceConfig.PeriodStandardDeviation;
                    var minPeriod = periodAv - (SigmaThreshold * periodSd);
                    var maxPeriod = periodAv + (SigmaThreshold * periodSd);
                    Log.Debug($"FILTER ON SYLLABLE SEQUENCE");
                    Log.Debug($" Expected Syllable Sequence: max={maxComponentCount},  Period: av={periodAv}s, sd={periodSd:F3} min={minPeriod:F3}s, max={maxPeriod:F3}s");
                    if (minPeriod <= 0.0)
                    {
                        Log.Error($"Expected period={periodAv};sd={periodSd:F3} => min={minPeriod:F3}s;max={maxPeriod:F3}",
                            new Exception("FATAL ERROR: This combination of values is invalid => negative minimum value."));
                        System.Environment.Exit(1);
                    }

                    newEvents = EventFilters.FilterEventsOnSyllableCountAndPeriodicity(newEvents, maxComponentCount, periodAv, periodSd);
                    Log.Debug($" Event count after filtering on periodicity = {newEvents.Count}");
                }
            }

            // 3: Filter the events for time duration (seconds)
            if ((postprocessingConfig.Duration != null) && (newEvents.Count > 0))
            {
                Log.Debug($"FILTER ON EVENT DURATION");
                var expectedEventDuration = postprocessingConfig.Duration.ExpectedDuration;
                var sdEventDuration = postprocessingConfig.Duration.DurationStandardDeviation;
                var minDuration = expectedEventDuration - (SigmaThreshold * sdEventDuration);
                var maxDuration = expectedEventDuration + (SigmaThreshold * sdEventDuration);
                Log.Debug($" Duration: expected={expectedEventDuration}s, sd={sdEventDuration} min={minDuration:F3}s, max={maxDuration:F3}s");
                newEvents = EventFilters.FilterOnDuration(newEvents, expectedEventDuration, sdEventDuration, SigmaThreshold);
                Log.Debug($" Event count after filtering on duration = {newEvents.Count}");
            }

            // 4: Filter the events for bandwidth in Hertz
            if ((postprocessingConfig.Bandwidth != null) && (newEvents.Count > 0))
            {
                Log.Debug($"FILTER ON EVENT BANDWIDTH");
                var expectedEventBandwidth = postprocessingConfig.Bandwidth.ExpectedBandwidth;
                var sdBandwidth = postprocessingConfig.Bandwidth.BandwidthStandardDeviation;
                var minBandwidth = expectedEventBandwidth - (SigmaThreshold * sdBandwidth);
                var maxBandwidth = expectedEventBandwidth + (SigmaThreshold * sdBandwidth);
                Log.Debug($" Bandwidth: expected={expectedEventBandwidth}Hz, sd={sdBandwidth} min={minBandwidth}Hz, max={maxBandwidth}Hz");
                newEvents = EventFilters.FilterOnBandwidth(newEvents, expectedEventBandwidth, sdBandwidth, SigmaThreshold);
                Log.Debug($" Event count after filtering on bandwidth = {newEvents.Count}");
            }

            // 5: Filter events on the amount of acoustic activity in their upper and lower sidebands - their buffer zone.
            //    The idea is that an unambiguous event should have some acoustic space above and below.
            //    The filter requires that the average acoustic activity in each frame and bin of the upper and lower buffer zones should not exceed the user specified decibel threshold.
            var sidebandActivity = postprocessingConfig.SidebandAcousticActivity;
            if ((sidebandActivity != null) && (newEvents.Count > 0))
            {
                if ((sidebandActivity.LowerSidebandWidth != null) || (sidebandActivity.UpperSidebandWidth != null))
                {
                    var spectralEvents2 = newEvents.Cast<SpectralEvent>().ToList();
                    newEvents = EventFilters.FilterEventsOnSidebandActivity(
                        spectralEvents2,
                        spectrogram,
                        sidebandActivity.LowerSidebandWidth,
                        sidebandActivity.UpperSidebandWidth,
                        sidebandActivity.MaxBackgroundDecibels,
                        sidebandActivity.MaxActivityDecibels,
                        segmentStartOffset);
                    Log.Debug($" Event count after filtering on sideband activity = {newEvents.Count}");
                }
                else
                {
                    Log.Debug($"DO NOT FILTER ON SIDEBAND ACTIVITY: both sidebands assigned zero width.");
                }
            }

            return newEvents;
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
            /// Gets or sets the parameters required to filter events on the acoustic activity in their sidebands.
            /// </summary>
            public SidebandConfig SidebandAcousticActivity { get; set; }

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
        /// The two properties in this class determine filtering of events based on their duration.
        /// The filter removes events whose duration lies outside three standard deviations (SDs) of an expected value.
        /// Assuming the duration is normally distributed, three SDs sets hard upper and lower duration bounds that includes 99.7% of instances.
        /// The filtering algorithm calculates these hard (3 SD) bounds and removes acoustic events that fall outside the bounds.
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
        /// This filter removes events whose bandwidth lies outside three standard deviations (SDs) of an expected value.
        /// Assuming the bandwidth is normally distributed, three SDs sets hard upper and lower bandwidth bounds that includes 99.7% of instances.
        /// The filtering algorithm calculates these hard bounds and removes acoustic events that fall outside the bounds.
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
            public int? UpperSidebandWidth { get; set; }

            /// <summary>
            /// Gets or sets a value indicating Whether or not to filter events based on the acoustic content of their lower buffer zone.
            /// If value = 0, the lower sideband is ignored.
            /// </summary>
            public int? LowerSidebandWidth { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the maximum permissible value of background acoustic activity in the upper and lower sidebands of an event.
            /// The background is claculated as the average decibel value over all spectrogram cells in each sideband.
            /// This value is used only if LowerHertzBuffer > 0 OR UpperHertzBuffer > 0.
            /// </summary>
            public double? MaxBackgroundDecibels { get; set; }

            /// <summary>
            /// Gets or sets a value indicating the maximum decibel value in a sideband frequency bin or timeframe.
            /// The decibel value is an average over all spectrogram cells in any frame or bin.
            /// This value is used only if LowerHertzBuffer > 0 OR UpperHertzBuffer > 0.
            /// </summary>
            public double? MaxActivityDecibels { get; set; }
        }

        /// <summary>
        /// The properties in this config class are required to combine a sequence of similar syllables into a single event.
        /// The first three properties concern the combining of syllables into a sequence or stroph.
        /// The next four properties concern the filtering/removal of sequences that do not satisfy expected properties.
        /// </summary>
        public class SyllableSequenceConfig
        {
            // ################ The first two properties concern the combining of syllables into a sequence or stroph.

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

            // ################ The next four properties concern the filtering or removal of sequences that do not satisfy expected properties.

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
            /// When setting ExpectedPeriod, you are actually setting a permissible range of values for the Period.
            /// The maximum permitted period will be the value assigned to SyllableStartDifference.
            /// The minimum period will be the ExpectedPeriod minus (SyllableStartDifference - ExpectedPeriod).
            /// For example: if SyllableStartDifference = 3 seconds and ExpectedPeriod = 2.5 seconds, then the minimum allowed period will be 2 seconds.
            /// THese bounds are hard bounds.
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
