// <copyright file="EventFilters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using TowseyLibrary;

    public static class EventFilters
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //NOTES on SYNTAX:
        //"Select" is a transform - fails if it encounters anything that is not of type SpectralEvent.
        //var spectralEvents = events.Select(x => (SpectralEvent)x).ToList();

        //"Where" is a FILTER - only returns spectral events.
        //var spectralEvents = events.Where(x => x is SpectralEvent).Cast<SpectralEvent>().ToList();
        //var spectralEvents = events.Where(x => x is ChirpEvent).ToList();
        //var chirpEvents = events.Cast<ChirpEvent>().ToList();

        public static (List<T> TargetEvents, List<U> OtherEvents) FilterForEventType<T, U>(this List<U> events)
            where U : EventCommon
            where T : EventCommon
        {
            var target = new List<T>(events.Count);
            var other = new List<U>(events.Count);

            foreach (var @event in events)
            {
                if (@event is T t)
                {
                    target.Add(t);
                }
                else
                {
                    other.Add(@event);
                }
            }

            return (target, other);
        }

        /// <summary>
        /// Filters lists of spectral events based on their bandwidth.
        /// Note: The typical sigma threshold would be 2 to 3 sds.
        /// </summary>
        /// <param name="events">The list of events.</param>
        /// <param name="average">The expected value of the bandwidth.</param>
        /// <param name="sd">The standard deviation of the bandwidth.</param>
        /// <param name="sigmaThreshold">THe sigma value which determines the max and min thresholds.</param>
        /// <returns>The filtered list of events.</returns>
        public static List<EventCommon> FilterOnBandwidth(List<EventCommon> events, double average, double sd, double sigmaThreshold)
        {
            var minBandwidth = average - (sd * sigmaThreshold);
            if (minBandwidth < 0.0)
            {
                throw new Exception("Invalid bandwidth passed to method EventExtentions.FilterOnBandwidth().");
            }

            var maxBandwidth = average + (sd * sigmaThreshold);
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).BandWidthHertz >= minBandwidth && ((SpectralEvent)ev).BandWidthHertz <= maxBandwidth).ToList();
            return outputEvents;
        }

        public static List<EventCommon> FilterOnBandwidth(List<EventCommon> events, double minBandwidth, double maxBandwidth)
        {
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).BandWidthHertz >= minBandwidth && ((SpectralEvent)ev).BandWidthHertz <= maxBandwidth).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Removes short events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterShortEvents(List<SpectralEvent> events, double minimumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ev.EventDurationSeconds >= minimumDurationSeconds).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Removes long events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterLongEvents(List<SpectralEvent> events, double maximumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ev.EventDurationSeconds <= maximumDurationSeconds).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Remove events from a list of events whose time duration is either too short or too long.
        /// </summary>
        public static List<EventCommon> FilterOnDuration(List<EventCommon> events, double minimumDurationSeconds, double maximumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).EventDurationSeconds >= minimumDurationSeconds && ((SpectralEvent)ev).EventDurationSeconds <= maximumDurationSeconds).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Filters lists of spectral events based on their duration.
        /// Note: The typical sigma threshold would be 2 to 3 sds.
        /// </summary>
        /// <param name="events">The list of events.</param>
        /// <param name="average">The expected value of the duration.</param>
        /// <param name="sd">The standard deviation of the duration.</param>
        /// <param name="sigmaThreshold">THe sigma value which determines the max and min thresholds.</param>
        /// <returns>The filtered list of events.</returns>
        public static List<EventCommon> FilterOnDuration(List<EventCommon> events, double average, double sd, double sigmaThreshold)
        {
            var minDuration = average - (sd * sigmaThreshold);
            if (minDuration < 0.0)
            {
                throw new Exception("Invalid seconds duration passed to method EventExtentions.FilterOnDuration().");
            }

            var maxDuration = average + (sd * sigmaThreshold);
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).EventDurationSeconds >= minDuration && ((SpectralEvent)ev).EventDurationSeconds <= maxDuration).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Removes composite events from a list of EventCommon that contain more than the specfied number of SpectralEvent components.
        /// </summary>
        public static List<EventCommon> FilterEventsOnComponentCount(
            List<EventCommon> events,
            int maxComponentCount)
        {
            var filteredEvents = new List<EventCommon>();

            foreach (var ev in events)
            {
                if (ev is CompositeEvent && ((CompositeEvent)ev).ComponentCount > maxComponentCount)
                {
                    // ignore composite events which contain more than the specified component events.
                    continue;
                }

                filteredEvents.Add(ev);
            }

            return filteredEvents;
        }

        /// <summary>
        /// Removes composite events from a list of EventCommon where the component syllables do not have the correct periodicity.
        /// </summary>
        public static List<EventCommon> FilterEventsOnSyllableCountAndPeriodicity(List<EventCommon> events, int maxSyllableCount, double expectedPeriod, double expectedSd)
        {
            var minExpectedPeriod = expectedPeriod - (3 * expectedSd);
            var maxExpectedPeriod = expectedPeriod + (3 * expectedSd);

            var filteredEvents = new List<EventCommon>();

            foreach (var ev in events)
            {
                // ignore non-composite events
                if (ev is CompositeEvent == false)
                {
                    filteredEvents.Add(ev);
                    continue;
                }

                // Get the temporal footprint of the component events.
                (bool[] temporalFootprint, double timeScale) = GetTemporalFootprint(ev);

                // calculate the actual periods in seconds
                int syllableCount = 1;
                var actualPeriodSeconds = new List<double>();
                int previousEventStart = 0;
                for (int f = 1; f < temporalFootprint.Length; f++)
                {
                    if (temporalFootprint[f] && !temporalFootprint[f - 1])
                    {
                        // calculate the event interval in seconds.
                        syllableCount++;
                        actualPeriodSeconds.Add((f - previousEventStart + 1) * timeScale);
                        previousEventStart = f;
                    }
                }

                string strArray = DataTools.Array2String(actualPeriodSeconds.ToArray());
                Log.Debug($" Actual periods: {strArray}");

                    // reject composite events whose total syllable count exceeds the user defined max.
                if (syllableCount > maxSyllableCount)
                {
                    Log.Debug($" EventRejected: Actual syllable count > max: {syllableCount} > {maxSyllableCount}");
                    continue;
                }

                // now filter on syllable periodicity.
                if (syllableCount == 1)
                {
                    // there was only one event - the multiple events all overlapped as one event
                    // accept this as valid outcome. There is no interval on which to filter.
                    filteredEvents.Add(ev);
                }
                else
                {
                    // If there are only two events, with one interval, THEN ...
                    var actualAvPeriod = actualPeriodSeconds[0];

                    // ... BUT if there are more than two events, get the average interval
                    if (syllableCount > 2)
                    {
                        actualAvPeriod = actualPeriodSeconds.Average();
                    }

                    // Require that the actual average period or interval should fall between required min and max period.
                    if (actualAvPeriod >= minExpectedPeriod && actualAvPeriod <= maxExpectedPeriod)
                    {
                        Log.Debug($" EventAccepted: Actual average syllable interval = {actualAvPeriod}");
                        filteredEvents.Add(ev);
                    }
                    else
                    {
                        Log.Debug($" EventRejected: Actual average syllable interval = {actualAvPeriod}");
                    }
                }
            }

            return filteredEvents;
        }

        public static (bool[] TemporalFootprint, double TimeScale) GetTemporalFootprint(EventCommon compositeEvent)
        {
            if (compositeEvent is CompositeEvent == false)
            {
                throw new Exception("Invalid event type. Event passed to GetTemporalFotprint() must be of type CompositeEvent.");
            }

            // get the composite events.
            var events = ((CompositeEvent)compositeEvent).ComponentEvents;

            var startEnds = new List<double[]>();
            double firstStart = double.MaxValue;
            double lastEnd = 0.0;

            foreach (var ev in events)
            {
                var startAndDuration = new double[2] { ev.EventStartSeconds, ((SpectralEvent)ev).EventDurationSeconds };
                startEnds.Add(startAndDuration);

                if (firstStart > ev.EventStartSeconds)
                {
                    firstStart = ev.EventStartSeconds;
                }

                if (lastEnd < ((SpectralEvent)ev).EventEndSeconds)
                {
                    lastEnd = ((SpectralEvent)ev).EventEndSeconds;
                }
            }

            // set up a temporal array to contain event footprint info.
            int arrayLength = 100;
            bool[] temporalFootprint = new bool[arrayLength];
            var compositeTimeDuration = lastEnd - firstStart;
            var timeScale = compositeTimeDuration / (double)arrayLength;

            foreach (var pair in startEnds)
            {
                int startFrame = (int)Math.Floor((pair[0] - firstStart) / timeScale);
                int endFrame = startFrame - 1 + (int)Math.Floor(pair[1] / timeScale);

                for (int f = startFrame; f <= endFrame; f++)
                {
                    temporalFootprint[f] = true;
                }
            }

            return (temporalFootprint, timeScale);
        }

        public static (int Count, double AveragePeriod, double SdPeriod) GetPeriodicity(bool[] temporalFootprint, double timeScale)
        {
            int count = 0;
            double averagePeriod = 0.0;
            double sdPeriod = 0.0;
            return (count, averagePeriod, sdPeriod);
        }

        /// <summary>
        /// Removes events from a list of events that contain excessive noise in the lower and/or upper neighbourhood.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method measures noise as the average decibel value in the buffer zones above and below the events.
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="spectrogram">A matrix of the spectrogram in which event occurs.</param>
        /// <param name="lowerHertzBuffer">The band width of the required lower buffer. 100-200Hz is often appropriate.</param>
        /// <param name="upperHertzBuffer">The band width of the required upper buffer. 300-500Hz is often appropriate.</param>
        /// <param name="thresholdForAverageDecibelsInSidebands">The max allowed value for the average decibels value (over all spectrogram cells) in a sideband of an event.</param>
        /// <param name="segmentStartOffset">Start time of the current recording segment.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnSidebandActivity(
            List<SpectralEvent> events,
            BaseSonogram spectrogram,
            int lowerHertzBuffer,
            int upperHertzBuffer,
            double thresholdForAverageDecibelsInSidebands,
            TimeSpan segmentStartOffset)
        {
            // allow bin gaps below the event.
            int lowerBinGap = 2;
            int upperBinGap = 2;

            //The decibel value of any other event in the sidebands of a focal event
            // cannot come within 3 dB of the dB value of the focal event.
            var decibelBuffer = 3.0;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            var spectrogramData = spectrogram.Data;

            var filteredEvents = new List<SpectralEvent>();
            foreach (var ev in events)
            {
                var avEventDecibels = EventExtentions.GetAverageDecibelsInEvent(ev, spectrogramData, converter);
                var maxSidebandEventDecibels = Math.Max(0.0, avEventDecibels - decibelBuffer);

                var retainEvent1 = true;
                var retainEvent2 = true;

                if (lowerHertzBuffer > 0)
                {
                    var lowerSidebandMatrix = GetLowerEventSideband(ev, spectrogramData, lowerHertzBuffer, lowerBinGap, converter);
                    retainEvent1 = IsSidebandActivityBelowThreshold(
                        lowerSidebandMatrix,
                        maxSidebandEventDecibels,
                        thresholdForAverageDecibelsInSidebands);
                }

                if (upperHertzBuffer > 0)
                {
                    var upperSidebandMatrix = GetUpperEventSideband(ev, spectrogramData, upperHertzBuffer, upperBinGap, converter);
                    retainEvent2 = IsSidebandActivityBelowThreshold(
                        upperSidebandMatrix,
                        maxSidebandEventDecibels,
                        thresholdForAverageDecibelsInSidebands);
                }

                if (retainEvent1 && retainEvent2)
                {
                    // The acoustic activity in event sidebands is below the threshold. It is likely to be a discrete event.
                    filteredEvents.Add(ev);
                }
            }

            var eventsCommon = filteredEvents.Cast<EventCommon>().ToList();
            return eventsCommon;
        }

        public static bool IsSidebandActivityBelowThreshold(
            double[,] sidebandMatrix,
            double maxSidebandEventDecibels,
            double thresholdForAverageDecibelsInSidebands)
        {
            var averageRowDecibels = MatrixTools.GetRowAverages(sidebandMatrix);
            var averageColDecibels = MatrixTools.GetColumnAverages(sidebandMatrix);
            var averageMatrixDecibels = averageColDecibels.Average();

            // Is the average acoustic activity in the sideband below the user set threshold?
            //bool avBgBelowThreshold = averageMatrixDecibels < analysisThreshold;
            bool avBgBelowThreshold = averageMatrixDecibels < thresholdForAverageDecibelsInSidebands;
            if (!avBgBelowThreshold)
            {
                return false;
            }

            // Also need to cover possibility that there is much acoustic activity concentrated in one freq bin or time frame.
            // Therefore, also require that there be at most one sideband bin and one sideband frame containing acoustic activity
            // that is greater than the average in the event.
            int noisyRowCount = averageRowDecibels.Count(x => x > maxSidebandEventDecibels);
            int noisyColCount = averageColDecibels.Count(x => x > maxSidebandEventDecibels);
            bool doRetain = noisyRowCount <= 1 && noisyColCount <= 1;
            return doRetain;
        }

        /// <summary>
        /// Returns the matrix of neighbourhood values below an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">THe bandwidth of the buffer zone in Hertz.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>The sideband as a matrix.</returns>
        public static double[,] GetLowerEventSideband(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int gap, UnitConverters converter)
        {
            var bufferBins = (int)Math.Round(bufferHertz / converter.HertzPerFreqBin);
            var topBufferBin = converter.GetFreqBinFromHertz(ev.LowFrequencyHertz) - gap;
            var bottomBufferBin = topBufferBin - bufferBins + 1;
            bottomBufferBin = Math.Max(0, bottomBufferBin);
            var frameStart = converter.FrameFromStartTime(ev.EventStartSeconds);
            var frameEnd = converter.FrameFromStartTime(ev.EventEndSeconds);
            int dataLength = spectrogramData.GetLength(0);
            frameEnd = Math.Min(dataLength - 1, frameEnd);
            var subMatrix = MatrixTools.Submatrix<double>(spectrogramData, frameStart, bottomBufferBin, frameEnd, topBufferBin);
            return subMatrix;
        }

        /// <summary>
        /// Returns the matrix of neighbourhood values above an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">The bandwidth of the buffer zone in Hertz.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>The neighbourhood as a matrix.</returns>
        public static double[,] GetUpperEventSideband(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int gap, UnitConverters converter)
        {
            var bufferBins = (int)Math.Round(bufferHertz / converter.HertzPerFreqBin);
            var bottomBufferBin = converter.GetFreqBinFromHertz(ev.HighFrequencyHertz) + gap;
            var topBufferBin = bottomBufferBin + bufferBins - 1;
            var frameStart = converter.FrameFromStartTime(ev.EventStartSeconds);
            var frameEnd = converter.FrameFromStartTime(ev.EventEndSeconds);
            int dataLength = spectrogramData.GetLength(0);
            frameEnd = Math.Min(dataLength - 1, frameEnd);
            var subMatrix = MatrixTools.Submatrix<double>(spectrogramData, frameStart, bottomBufferBin, frameEnd, topBufferBin);
            return subMatrix;
        }
    }
}
