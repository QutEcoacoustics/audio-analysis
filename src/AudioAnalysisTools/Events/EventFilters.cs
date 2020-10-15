// <copyright file="EventFilters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using MoreLinq;
    using TowseyLibrary;

    public static class EventFilters
    {
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
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).BandWidthHertz > minBandwidth && ((SpectralEvent)ev).BandWidthHertz < maxBandwidth).ToList();
            return outputEvents;
        }

        public static List<EventCommon> FilterOnBandwidth(List<EventCommon> events, double minBandwidth, double maxBandwidth)
        {
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).BandWidthHertz > minBandwidth && ((SpectralEvent)ev).BandWidthHertz < maxBandwidth).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Removes short events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterShortEvents(List<SpectralEvent> events, double minimumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ev.EventDurationSeconds > minimumDurationSeconds).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Removes long events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterLongEvents(List<SpectralEvent> events, double maximumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ev.EventDurationSeconds < maximumDurationSeconds).ToList();
            return outputEvents;
        }

        /// <summary>
        /// Remove events from a list of events whose time duration is either too short or too long.
        /// </summary>
        public static List<EventCommon> FilterOnDuration(List<EventCommon> events, double minimumDurationSeconds, double maximumDurationSeconds)
        {
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).EventDurationSeconds > minimumDurationSeconds && ((SpectralEvent)ev).EventDurationSeconds < maximumDurationSeconds).ToList();
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
            var outputEvents = events.Where(ev => ((SpectralEvent)ev).EventDurationSeconds > minDuration && ((SpectralEvent)ev).EventDurationSeconds < maxDuration).ToList();
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

                // calculate the periodicity in seconds
                int syllableCount = 1;
                var periodSeconds = new List<double>();
                int previousEventStart = 0;
                for (int f = 1; f < temporalFootprint.Length; f++)
                {
                    if (temporalFootprint[f] && !temporalFootprint[f - 1])
                    {
                        // calculate the event interval in seconds.
                        syllableCount++;
                        periodSeconds.Add((f - previousEventStart + 1) * timeScale);
                        previousEventStart = f;
                    }
                }

                // reject composite events whose total syllable count exceeds the user defined max.
                if (syllableCount > maxSyllableCount)
                {
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
                    if (syllableCount == 2)
                    {
                        // there were only two events, with one interval
                        // accept this as valid outcome, iff the interval falls within the expected interval.
                        var actualInterval = periodSeconds[0];

                        if (actualInterval >= minExpectedPeriod && actualInterval <= maxExpectedPeriod)
                        {
                            filteredEvents.Add(ev);
                        }
                    }
                    else
                    {
                        // there were more than two events. Require overlap between actual and expected ranges.
                        NormalDist.AverageAndSD(periodSeconds.ToArray(), out double averagePeriod, out double sdPeriod);

                        // get the difference between the expected and absolute periods.
                        var periodDifference = Math.Abs(averagePeriod - expectedPeriod);

                        //This difference should be less than the combined SDs.
                        var combinedSds = (sdPeriod + expectedSd) * 2;
                        if (periodDifference <= combinedSds)
                        {
                            filteredEvents.Add(ev);
                        }
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
        /// Removes events from a list of events that contain excessive noise in the upper neighbourhood.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method measures noise as the average decibel value in the buffer zones above and below the events.
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="spectrogramData">A matrix of the spectrogram in which event occurs.</param>
        /// <param name="lowerHertzBuffer">The band width of the required lower buffer. 100-200Hz is often appropriate.</param>
        /// <param name="upperHertzBuffer">The band width of the required upper buffer. 300-500Hz is often appropriate.</param>
        /// <param name="converter">Converts sec/Hz to frame/bin.</param>
        /// <param name="decibelThreshold">Threshold noise level - assumed to be in decibels.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnNeighbourhoodAverage(
            List<SpectralEvent> events,
            double[,] spectrogramData,
            double lowerHertzBuffer,
            double upperHertzBuffer,
            UnitConverters converter,
            double decibelThreshold)
        {
            // allow bin gaps above and below the event.
            int upperBinGap = 4;
            int lowerBinGap = 2;

            var filteredEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                var avLowerNhAmplitude = GetAverageAmplitudeInLowerNeighbourhood((SpectralEvent)ev, spectrogramData, lowerHertzBuffer, lowerBinGap, converter);
                var avUpperNhAmplitude = GetAverageAmplitudeInUpperNeighbourhood((SpectralEvent)ev, spectrogramData, upperHertzBuffer, upperBinGap, converter);

                // Require that both the lower and upper buffer zones contain less acoustic activity than the threshold.
                if (avLowerNhAmplitude < decibelThreshold && avUpperNhAmplitude < decibelThreshold)
                {
                    // There is little acoustic activity in the designated buffer zones. It is likely to be a discrete event.
                    filteredEvents.Add(ev);
                }
            }

            return filteredEvents;
        }

        /// <summary>
        /// Removes events from a list of events that contain excessive noise in the lower event side band.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method counts the bins and frames containing above threshold activity (decibel value) in the buffer zone below the events.
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="spectrogram">The decibel spectrogram in which the events occurs.</param>
        /// <param name="lowerHertzBuffer">The band width of the required lower buffer. 100-200Hz is often appropriate.</param>
        /// <param name="decibelBuffer">Minimum required decibel difference between event activity and neighbourhood activity.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnLowerSidebandActivity(
            List<SpectralEvent> events,
            BaseSonogram spectrogram,
            int lowerHertzBuffer,
            TimeSpan segmentStartOffset,
            double decibelBuffer)
        {
            if (lowerHertzBuffer == 0)
            {
                return null;
            }

            // allow bin gaps below the event.
            int lowerBinGap = 2;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            var filteredEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                var eventDecibels = EventExtentions.GetAverageDecibelsInEvent(ev, spectrogram.Data, converter);
                var sidebandThreshold = eventDecibels - decibelBuffer;

                var sidebandMatrix = GetLowerNeighbourhood(ev, spectrogram.Data, lowerHertzBuffer, lowerBinGap, converter);

                var averageRowDecibels = MatrixTools.GetRowAverages(sidebandMatrix);
                var averageColDecibels = MatrixTools.GetColumnAverages(sidebandMatrix);
                int noisyRowCount = averageRowDecibels.Count(x => x > sidebandThreshold);
                int noisyColCount = averageColDecibels.Count(x => x > sidebandThreshold);

                // Require that there be at most one buffer bin and one buffer frame containing excessive acoustic activity.
                if (noisyRowCount <= 1 && noisyColCount <= 1)
                {
                    // There is reduced acoustic activity in the upper and lower buffer zones. It is likely to be a discrete event.
                    filteredEvents.Add(ev);
                }
            }

            return filteredEvents;
        }

        /// <summary>
        /// Removes events from a list of events that contain excessive noise in the upper event side band.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method counts the bins and frames containing above threshold activity (decibel value) in the buffer zone above the events.
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="spectrogram">The decibel spectrogram in which the events occurs.</param>
        /// <param name="upperHertzBuffer">The band width of the required upper buffer. 300-500Hz is often appropriate.</param>
        /// <param name="decibelBuffer">Minimum required decibel difference between event activity and neighbourhood activity.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnUpperSidebandActivity(
            List<SpectralEvent> events,
            BaseSonogram spectrogram,
            int upperHertzBuffer,
            TimeSpan segmentStartOffset,
            double decibelBuffer)
        {
            // allow bin gaps above the event.
            int upperBinGap = 3;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            var filteredEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                var eventDecibels = EventExtentions.GetAverageDecibelsInEvent(ev, spectrogram.Data, converter);
                var sidebandThreshold = eventDecibels - decibelBuffer;

                var sidebandMatrix = GetUpperNeighbourhood(ev, spectrogram.Data, upperHertzBuffer, upperBinGap, converter);
                var averageRowDecibels = MatrixTools.GetRowAverages(sidebandMatrix);
                var averageColDecibels = MatrixTools.GetColumnAverages(sidebandMatrix);
                int noisyRowCount = averageRowDecibels.Count(x => x > sidebandThreshold);
                int noisyColCount = averageColDecibels.Count(x => x > sidebandThreshold);

                // Require that there be at most one buffer bin and one buffer frame containing excessive acoustic activity.
                if (noisyRowCount <= 1 && noisyColCount <= 1)
                {
                    // There is reduced acoustic activity in the upper and lower buffer zones. It is likely to be a discrete event.
                    filteredEvents.Add(ev);
                }
            }

            return filteredEvents;
        }

        /// <summary>
        /// Returns the matrix of neighbourhood values below an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">THe bandwidth of the buffer zone in Hertz.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>The neighbourhood as a matrix.</returns>
        public static double[,] GetLowerNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int gap, UnitConverters converter)
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
        public static double[,] GetUpperNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int gap, UnitConverters converter)
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

        /// <summary>
        /// Gets the upper and lower buffer zones (above and below an event).
        /// Returns them as one combined matrix.
        /// This makes it easier to determine the presense of acoustic events (especially wind) in the buffer zones.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="lowerHertzBuffer">The bandwidth of the lower buffer zone in Hertz.</param>
        /// <param name="lowerBinGap">Number of freq bins left as gap below event.</param>
        /// <param name="upperHertzBuffer">The bandwidth of the upper buffer zone in Hertz.</param>
        /// <param name="upperBinGap">Number of freq bins left as gap above event.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>A single matrix.</returns>
        public static double[,] GetNeighbourhoodAsOneMatrix(
            SpectralEvent ev,
            double[,] spectrogramData,
            double lowerHertzBuffer,
            int lowerBinGap,
            double upperHertzBuffer,
            int upperBinGap,
            UnitConverters converter)
        {
            double[,] subMatrix1 = null;
            if (upperHertzBuffer > 0)
            {
                subMatrix1 = GetUpperNeighbourhood(ev, spectrogramData, upperHertzBuffer, upperBinGap, converter);
            }

            double[,] subMatrix2 = null;
            if (lowerHertzBuffer > 0)
            {
                subMatrix2 = GetLowerNeighbourhood(ev, spectrogramData, lowerHertzBuffer, lowerBinGap, converter);
            }

            if (subMatrix1 == null && subMatrix2 == null)
            {
                return null;
            }

            if (subMatrix1 == null)
            {
                return subMatrix2;
            }

            if (subMatrix2 == null)
            {
                return subMatrix1;
            }

            var matrix = MatrixTools.ConcatenateTwoMatrices(subMatrix1, subMatrix2);
            return matrix;
        }

        /// <summary>
        /// Calculates the average amplitude in the frequency bins just above the event.
        /// If it contains above threshold acoustic content, this is unlikely to be a discrete event.
        /// NOTE: This method takes a simple average of log values. This is good enough for the purpose, although not mathematically correct.
        ///       Logs are computationally expensive.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">THe bandwidth of the buffer zone in Hertz.</param>
        /// <param name="binGap">Number of freq bins as gap between event and buffer zone.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>Unweighted average of the spectrogram amplitude in buffer band above the event.</returns>
        public static double GetAverageAmplitudeInUpperNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int binGap, UnitConverters converter)
        {
            var subMatrix = GetUpperNeighbourhood(ev, spectrogramData, bufferHertz, binGap, converter);
            var averageRowDecibels = MatrixTools.GetRowAverages(subMatrix);
            var av = averageRowDecibels.Average();
            return av;
        }

        /// <summary>
        /// Calculates the average amplitude in the frequency bins just below the event.
        /// If it contains above threshold acoustic content, this is unlikely to be a discrete event.
        /// NOTE: This method takes a simple average of log values. This is good enough for the purpose, although not mathematically correct.
        ///       Logs are computationally expensive.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">The bandwidth of the buffer zone in bins.</param>
        /// <param name="binGap">Number of freq bins as gap between event and buffer zone.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>Unweighted average of the spectrogram amplitude in buffer band below the event.</returns>
        public static double GetAverageAmplitudeInLowerNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int binGap, UnitConverters converter)
        {
            var subMatrix = GetLowerNeighbourhood(ev, spectrogramData, bufferHertz, binGap, converter);
            var averageRowDecibels = MatrixTools.GetRowAverages(subMatrix);
            var av = averageRowDecibels.Average();
            return av;
        }
    }
}
