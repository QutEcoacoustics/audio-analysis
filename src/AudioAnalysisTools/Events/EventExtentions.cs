// <copyright file="EventExtentions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AudioAnalysisTools.Events.Tracks;
    using MoreLinq;
    using TowseyLibrary;

    public static class EventExtentions
    {
        /// <summary>
        /// Returns the average of the maximum decibel value in each frame of an event.
        /// </summary>
        /// <param name="ev">The event.</param>
        /// <param name="spectrogramData">The spectrogramin decibels.</param>
        /// <param name="converter">Converter between real values and spectrogram frames/bins.</param>
        /// <returns>The average decibel value.</returns>
        public static double[] GetDecibelArrayFromEvent(SpectralEvent ev, double[,] spectrogramData, UnitConverters converter)
        {
            // Get the frame and bin counts of the spectrogram
            var frameCount = spectrogramData.GetLength(0);
            var binCount = spectrogramData.GetLength(1);

            // extract the event from the spectrogram
            var lowerBin = converter.GetFreqBinFromHertz(ev.LowFrequencyHertz);
            var upperBin = converter.GetFreqBinFromHertz(ev.HighFrequencyHertz);
            if (upperBin >= binCount)
            {
                upperBin = binCount - 1;
            }

            var frameStart = converter.FrameFromStartTime(ev.EventStartSeconds);
            var frameEnd = converter.FrameFromStartTime(ev.EventEndSeconds);
            if (frameEnd >= frameCount)
            {
                frameEnd = frameCount - 1;
            }

            var subMatrix = MatrixTools.Submatrix<double>(spectrogramData, frameStart, lowerBin, frameEnd, upperBin);

            // extract the decibel array. Get the maximum decibel value in each frame.
            int arrayLength = subMatrix.GetLength(0);
            var decibelArray = new double[arrayLength];
            for (int i = 0; i < arrayLength; i++)
            {
                var spectralBins = MatrixTools.GetRow(subMatrix, i);
                decibelArray[i] = spectralBins.Max();
            }

            return decibelArray;
        }

        public static double GetAverageDecibelsInEvent(SpectralEvent ev, double[,] spectrogramData, UnitConverters converter)
        {
            var decibelArray = GetDecibelArrayFromEvent(ev, spectrogramData, converter);
            double avDecibels = decibelArray.Average();
            return avDecibels;
        }

        /// <summary>
        /// Combines all the tracks in all the events in the passed list into a single track.
        /// Each frame in the composite event is assigned the spectral point having maximum amplitude.
        /// The points in the returned array are in temporal order.
        /// </summary>
        /// <param name="events">List of spectral events.</param>
        public static IEnumerable<ISpectralPoint> GetCompositeTrack<T>(IEnumerable<T> events)
        where T : ITracks<Track>
        {
            var points = events.SelectMany(x => x.Tracks.SelectMany(t => t.Points));

            // group all the points by their start time.
            var groupStarts = points.GroupBy(p => p.Seconds);

            // for each group, for each point in group, choose the point having maximum (amplitude) value.
            // Since there maybe multiple points having maximum amplitude, we pick the first one.
            var maxAmplitudePoints = groupStarts.Select(g => g.MaxBy(p => p.Value).First());

            return maxAmplitudePoints.OrderBy(p => p);
        }
    }
}
