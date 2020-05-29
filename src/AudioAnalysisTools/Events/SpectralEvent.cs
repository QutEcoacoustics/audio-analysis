// <copyright file="SpectralEvent.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public class SpectralEvent : EventCommon, ISpectralEvent, ITemporalEvent
    {
        public SpectralEvent()
        {
            // empty constructor to prevent obligatory requirement for arguments.
        }

        public SpectralEvent(TimeSpan segmentStartOffset, double eventStartRecordingRelative, double eventEndRecordingRelative, double minFreq, double maxFreq)
        {
            this.SegmentStartSeconds = segmentStartOffset.TotalSeconds;
            this.EventStartSeconds = eventStartRecordingRelative;
            this.EventEndSeconds = eventEndRecordingRelative;
            this.LowFrequencyHertz = minFreq;
            this.HighFrequencyHertz = maxFreq;
        }

        public virtual double EventEndSeconds { get; set; }

        public virtual double HighFrequencyHertz { get; set; }

        public virtual double LowFrequencyHertz { get; set; }

        //public double Duration => base.Duration;

        /// DIMENSIONS OF THE EVENT
        /// <summary>Gets the event duration in seconds.</summary>
        public double EventDurationSeconds => this.EventEndSeconds - this.EventStartSeconds;

        public double BandWidthHertz => this.HighFrequencyHertz - this.LowFrequencyHertz;

        public override void Draw(IImageProcessingContext graphics, EventRenderingOptions options)
        {
            // draw a border around this event
            if (options.DrawBorder)
            {
                var border = options.Converters.GetPixelRectangle(this);
                graphics.NoAA().DrawBorderInset(options.Border, border);
            }

            this.DrawScoreIndicator(graphics, options);

            this.DrawEventLabel(graphics, options);
        }

        // ############################### THE FOLLOWING STATIC EVENTS FILTER LISTS OF SPECTRAL EVENTS

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
            var minHertzWidth = average - (sd * sigmaThreshold);
            var maxHertzWidth = average + (sd * sigmaThreshold);
            return FilterOnBandwidth(events, minHertzWidth, maxHertzWidth);
        }

        public static List<EventCommon> FilterOnBandwidth(List<EventCommon> events, double minHertzWidth, double maxHertzWidth)
        {
            var outputEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                if (((SpectralEvent)ev).BandWidthHertz > minHertzWidth && ((SpectralEvent)ev).BandWidthHertz < maxHertzWidth)
                {
                    outputEvents.Add(ev);
                }
            }

            return outputEvents;
        }

        /// <summary>
        /// Removes short events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterShortEvents(List<SpectralEvent> events, double minimumDurationSeconds)
        {
            var outputEvents = new List<SpectralEvent>();
            foreach (var ev in events)
            {
                if (ev.EventDurationSeconds > minimumDurationSeconds)
                {
                    outputEvents.Add(ev);
                }
            }

            return outputEvents;
        }

        /// <summary>
        /// Removes long events from a list of events.
        /// </summary>
        public static List<SpectralEvent> FilterLongEvents(List<SpectralEvent> events, double maximumDurationSeconds)
        {
            var outputEvents = new List<SpectralEvent>();
            foreach (var ev in events)
            {
                if (ev.EventDurationSeconds < maximumDurationSeconds)
                {
                    outputEvents.Add(ev);
                }
            }

            return outputEvents;
        }

        /// <summary>
        /// Removes long events from a list of events.
        /// </summary>
        public static List<EventCommon> FilterOnDuration(List<EventCommon> events, double minimumDurationSeconds, double maximumDurationSeconds)
        {
            var outputEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                if (((SpectralEvent)ev).EventDurationSeconds > minimumDurationSeconds && ((SpectralEvent)ev).EventDurationSeconds < maximumDurationSeconds)
                {
                    outputEvents.Add(ev);
                }
            }

            return outputEvents;
        }

        /// <summary>
        /// Returns the matrix of neighbourhood values below an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="sonogramData">The spectrogram data as matrix with origin top/left.</param>
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
            int dataLength = sonogramData.GetLength(0);
            frameEnd = Math.Min(dataLength - 1, frameEnd);

            //Console.WriteLine($"{frameStart}, {bottomBufferBin}, {frameEnd}, {topBufferBin}, Sonogram[{sonogramData.GetLength(0)}, {sonogramData.GetLength(1)}]");
            var subMatrix = MatrixTools.Submatrix<double>(sonogramData, frameStart, bottomBufferBin, frameEnd, topBufferBin);
            return subMatrix;
        }

        /// <summary>
        /// Returns the matrix of neighbourhood values above an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="sonogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">THe bandwidth of the buffer zone in Hertz.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>The neighbourhood as a matrix.</returns>
        public static double[,] GetUpperNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int gap, UnitConverters converter)
        {
            var bufferBins = (int)Math.Round(bufferHertz / converter.HertzPerFreqBin);
            var bottomBufferBin = converter.GetFreqBinFromHertz(ev.HighFrequencyHertz) + gap;
            var topBufferBin = bottomBufferBin + bufferBins - 1;
            var frameStart = converter.FrameFromStartTime(ev.EventStartSeconds);
            var frameEnd = converter.FrameFromStartTime(ev.EventEndSeconds);
            int dataLength = sonogramData.GetLength(0);
            frameEnd = Math.Min(dataLength - 1, frameEnd);

            //Console.WriteLine($"{frameStart}, {bottomBufferBin}, {frameEnd}, {topBufferBin}, Sonogram[{sonogramData.GetLength(0)}, {sonogramData.GetLength(1)}]");
            var subMatrix = MatrixTools.Submatrix<double>(sonogramData, frameStart, bottomBufferBin, frameEnd, topBufferBin);
            return subMatrix;
        }

        public static double[,] GetNeighbourhoodAsOneMatrix(
            SpectralEvent ev,
            double[,] sonogramData,
            double lowerHertzBuffer,
            int lowerBinGap,
            double upperHertzBuffer,
            int upperBinGap,
            UnitConverters converter)
        {
            double[,] subMatrix1 = null;
            if (upperHertzBuffer > 0)
            {
                subMatrix1 = GetUpperNeighbourhood(ev, sonogramData, upperHertzBuffer, upperBinGap, converter);
            }

            double[,] subMatrix2 = null;
            if (upperHertzBuffer > 0)
            {
                subMatrix2 = GetLowerNeighbourhood(ev, sonogramData, lowerHertzBuffer, lowerBinGap, converter);
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
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="sonogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">THe bandwidth of the buffer zone in Hertz.</param>
        /// <param name="binGap">Number of freq bins as gap between event and buffer zone.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>Average of the spectrogram amplitude in buffer band above the event.</returns>
        public static double GetAverageAmplitudeInUpperNeighbourhood(SpectralEvent ev, double[,] spectrogramData, double bufferHertz, int binGap, UnitConverters converter)
        {
            var subMatrix = GetUpperNeighbourhood(ev, sonogramData, bufferHertz, binGap, converter);
            var averageRowDecibels = MatrixTools.GetRowAverages(subMatrix);
            var av = averageRowDecibels.Average();
            return av;
        }

        /// <summary>
        /// Calculates the average amplitude in the frequency bins just below the event.
        /// If it contains above threshold acoustic content, this is unlikely to be a discrete event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="sonogramData">The spectrogram data as matrix with origin top/left.</param>
        /// <param name="bufferHertz">The bandwidth of the buffer zone in bins.</param>
        /// <param name="binGap">Number of freq bins as gap between event and buffer zone.</param>
        /// <param name="converter">A converter to convert seconds/Hertz to frames/bins.</param>
        /// <returns>Average of the spectrogram amplitude in buffer band below the event.</returns>
        public static double GetAverageAmplitudeInLowerNeighbourhood(SpectralEvent ev, double[,] sonogramData, double bufferHertz, int binGap, UnitConverters converter)
        {
            var subMatrix = GetLowerNeighbourhood(ev, sonogramData, bufferHertz, binGap, converter);
            var averageRowDecibels = MatrixTools.GetRowAverages(subMatrix);
            var av = averageRowDecibels.Average();
            return av;
        }

        /// <summary>
        /// Removes events from a list of events that contain excessive noise in the upper neighbourhood.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method measures noise as the average decibel value in the buffer zones above and below the events.
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="sonogramData">A matrix of the spectrogram in which event occurs.</param>
        /// <param name="lowerHertzBuffer">The band width of the required lower buffer. 100-200Hz is often appropriate.</param>
        /// <param name="upperHertzBuffer">The band width of the required upper buffer. 300-500Hz is often appropriate.</param>
        /// <param name="converter">Converts sec/Hz to frame/bin.</param>
        /// <param name="decibelThreshold">Threshold noise level - assumed to be in decibels.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnNeighbourhoodAverage(
            List<SpectralEvent> events,
            double[,] sonogramData,
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
                var avLowerNhAmplitude = SpectralEvent.GetAverageAmplitudeInLowerNeighbourhood((SpectralEvent)ev, sonogramData, lowerHertzBuffer, lowerBinGap, converter);
                var avUpperNhAmplitude = SpectralEvent.GetAverageAmplitudeInUpperNeighbourhood((SpectralEvent)ev, sonogramData, upperHertzBuffer, upperBinGap, converter);

                //Console.WriteLine($"################################### Buffer Average decibels = {avUpperNhAmplitude}");
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
        /// Removes events from a list of events that contain excessive noise in the upper neighbourhood.
        /// Excess noise can indicate that this is not a legitimate event.
        /// This method counts the bins and frames containing above threshold activity (decibel value) in the buffer zones above and below the events.
        /// 
        /// </summary>
        /// <param name="events">A list of spectral events.</param>
        /// <param name="spectrogram">The spectrogram in which the event occurs.</param>
        /// <param name="lowerHertzBuffer">The band width of the required lower buffer. 100-200Hz is often appropriate.</param>
        /// <param name="upperHertzBuffer">The band width of the required upper buffer. 300-500Hz is often appropriate.</param>
        /// <param name="decibelThreshold">Threshold noise level - assumed to be in decibels.</param>
        /// <returns>A list of filtered events.</returns>
        public static List<EventCommon> FilterEventsOnNeighbourhood(
            List<SpectralEvent> events,
            BaseSonogram spectrogram,
            int lowerHertzBuffer,
            int upperHertzBuffer,
            TimeSpan segmentStartOffset,
            double decibelThreshold)
        {
            // allow bin gaps above and below the event.
            int upperBinGap = 4;
            int lowerBinGap = 2;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            var filteredEvents = new List<EventCommon>();
            foreach (var ev in events)
            {
                var matrix = GetNeighbourhoodAsOneMatrix(ev, spectrogram.Data, lowerHertzBuffer, lowerBinGap, upperHertzBuffer, upperBinGap, converter);
                var averageRowDecibels = MatrixTools.GetRowAverages(matrix);
                var averageColDecibels = MatrixTools.GetColumnAverages(matrix);
                int noisyRowCount = averageRowDecibels.Count(x => x > decibelThreshold);
                int noisyColCount = averageColDecibels.Count(x => x > decibelThreshold);

                // Require that there be at most one buffer bin and one buffer frame containing acoustic activity.
                if (noisyRowCount <= 1 && noisyColCount <= 1)
                {
                    // There is little acoustic activity in the upper and lower buffer zones. It is likely to be a discrete event.
                    filteredEvents.Add(ev);
                }
            }

            return filteredEvents;
        }

        /// <summary>
        /// Removes composite events from a list of EventCommon that contain more than the specfied number of SpectralEvent components.
        /// </summary>
        public static List<EventCommon> FilterEventsOnCompositeContent(
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
    }
}
