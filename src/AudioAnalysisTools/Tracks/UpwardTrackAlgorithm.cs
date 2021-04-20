// <copyright file="UpwardTrackAlgorithm.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Tracks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;
    using TrackType = AudioAnalysisTools.Events.Tracks.TrackType;

    /// <summary>
    /// EXPANATION: A vertical track is a near click or rapidly frequency-modulated tone. A good example is the whip component of the whip-bird call.
    /// They would typically be only a few time-frames duration.
    /// </summary>
    public static class UpwardTrackAlgorithm
    {
        public static (List<EventCommon> Events, List<Plot> DecibelPlots) GetUpwardTracks(
            SpectrogramStandard spectrogram,
            UpwardTrackParameters parameters,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            double[] decibelArray;
            List<EventCommon> events;

            (events, decibelArray) = GetUpwardTracks(
            spectrogram,
            parameters,
            segmentStartOffset,
            decibelThreshold.Value);

            spectralEvents.AddRange(events);

            var plot = Plot.PreparePlot(decibelArray, $"{profileName} (Whips:{decibelThreshold.Value:F0}dB)", decibelThreshold.Value);
            plots.Add(plot);

            return (spectralEvents, plots);
        }

        /// <summary>
        /// THis method averages dB log values incorrectly but it is faster than doing many log conversions and is accurate enough for the purpose.
        /// </summary>
        /// <param name="sonogram">The spectrogram to be searched.</param>
        /// <param name="parameters">parameters for the upwards track algorithm.</param>
        /// <param name="segmentStartOffset">The start time of the current recording segment under analysis.</param>
        /// <returns>A list of acoustic events containing foward tracks.</returns>
        public static (List<EventCommon> Events, double[] CombinedIntensity) GetUpwardTracks(
            SpectrogramStandard sonogram,
            AnalysisPrograms.Recognizers.Base.UpwardTrackParameters parameters,
            TimeSpan segmentStartOffset,
            double decibelThreshold)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            var frameStep = sonogram.FrameStep;
            int nyquist = sonogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;
            int minSearchBin = (int)Math.Round(parameters.SearchbandMinHertz.Value / binWidth);
            int maxSearchBin = (int)Math.Round(parameters.SearchbandMaxHertz.Value / binWidth);
            var minBandwidthHertz = parameters.MinBandwidthHertz ?? throw new ArgumentNullException($"{nameof(UpwardTrackParameters.MinBandwidthHertz)} must be set. Check your config file?");
            var maxBandwidthHertz = parameters.MaxBandwidthHertz ?? throw new ArgumentNullException($"{nameof(UpwardTrackParameters.MinBandwidthHertz)} must be set. Check your config file?");

            // Calculate the max score for normalisation purposes
            var maxScore = decibelThreshold * 5;
            var scoreRange = new Interval<double>(0, maxScore);

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: sonogram.SampleRate,
                frameSize: sonogram.Configuration.WindowSize,
                frameOverlap: sonogram.Configuration.WindowOverlap);

            // Find all frame peaks and place in peaks matrix
            // avoid row edge effects.
            var peaks = new double[frameCount, binCount];
            for (int row = 1; row < frameCount - 1; row++)
            {
                for (int col = minSearchBin; col < maxSearchBin; col++)
                {
                    if (sonogramData[row, col] < decibelThreshold)
                    {
                        continue;
                    }

                    // if given matrix element is greater than in frame either side
                    bool isPeak = (sonogramData[row, col] > sonogramData[row - 1, col]) && (sonogramData[row, col] > sonogramData[row + 1, col]);
                    if (isPeak)
                    {
                        peaks[row, col] = sonogramData[row, col];
                    }
                }
            }

            //NOTE: the Peaks matrix is same size as the sonogram.
            var tracks = GetUpwardTracks(peaks, minSearchBin, maxSearchBin, minBandwidthHertz, maxBandwidthHertz, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            var events = new List<SpectralEvent>();
            var temporalIntensityArray = new double[frameCount];

            foreach (var track in tracks)
            {
                // fill the intensity array
                //var startRow = (int)converter.TemporalScale.To(track.StartTimeSeconds);
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    temporalIntensityArray[startRow + i] += amplitudeTrack[i];
                }

                //Skip tracks that do not sit within the correct bandWidth range.
                if (track.TrackBandWidthHertz < minBandwidthHertz || track.TrackBandWidthHertz > maxBandwidthHertz)
                {
                    continue;
                }

                //Following line used only for debug purposes. Can save as image.
                //spectrogram.Mutate(x => track.Draw(x, options));
                var ae = new WhipEvent(track, scoreRange)
                {
                    SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                    SegmentDurationSeconds = frameCount * converter.SecondsPerFrameStep,
                    Name = "Whip",
                };

                events.Add(ae);
            }

            List<EventCommon> returnEvents = events.Cast<EventCommon>().ToList();

            // combine proximal events that occupy similar frequency band
            if (parameters.CombineProximalSimilarEvents)
            {
                returnEvents = CompositeEvent.CombineProximalEvents(events, parameters.SyllableStartDifference, parameters.SyllableHertzDifference);
            }

            return (returnEvents, temporalIntensityArray);
        }

        public static List<Track> GetUpwardTracks(double[,] peaks, int minBin, int maxBin, double minBandwidthHertz, double maxBandwidthHertz, double threshold, UnitConverters converter)
        {
            int frameCount = peaks.GetLength(0);
            var tracks = new List<Track>();

            // Look for possible track starts and initialise as track.
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            // We want to scane down each freq bin starting from the bottom bin.
            for (int col = minBin; col < maxBin; col++)
            {
                for (int row = 0; row < frameCount; row++)
                {
                    if (peaks[row, col] < threshold)
                    {
                        continue;
                    }

                    // Visit each spectral peak in order. Each may be start of possible track
                    var track = GetUpwardTrack(peaks, row, col, maxBin, threshold, converter);

                    // a track should have length > 2
                    if (track.PointCount > 2)
                    {
                        tracks.Add(track);
                    }
                }
            }

            return tracks;
        }

        public static Track GetUpwardTrack(double[,] peaks, int startRow, int startBin, int maxBin, double threshold, UnitConverters converter)
        {
            var track = new Track(converter, TrackType.UpwardTrack);
            track.SetPoint(startRow, startBin, peaks[startRow, startBin]);

            // set the start point in peaks matrix to zero to prevent return to this point.
            peaks[startRow, startBin] = 0.0;

            //Now move to next higher freq bin.
            int row = startRow;
            for (int bin = startBin + 1; bin < maxBin - 1; bin++)
            {
                // Avoid row edge effects.
                if (row < 2 || row > peaks.GetLength(0) - 3)
                {
                    // arrived back at start of recording or end of recording.
                    // The track has come to end
                    return track;
                }

                if (bin >= maxBin)
                {
                    // arrived at top of the requested frequency band - track has come to end
                    return track;
                }

                // explore options for track moving to next higher frequency bin
                // We are looking for the option which has the highest combined amplitude.
                double optionStraight = Math.Max(peaks[row, bin + 1], peaks[row - 1, bin + 1]);
                optionStraight = Math.Max(optionStraight, peaks[row + 1, bin + 1]);
                optionStraight = peaks[row, bin] + optionStraight;

                // option for track with negative slope i.e. return to previous row/frame.
                //double optionNeg = Math.Max(peaks[row - 1, bin + 1], peaks[row, bin + 1]);
                double optionNeg = Math.Max(peaks[row - 1, bin + 1], peaks[row - 2, bin + 1]);
                optionNeg = peaks[row - 1, bin] + optionNeg;

                // option for track with positive slope
                //double optionPos = Math.Max(peaks[row + 1, bin + 1], peaks[row, bin + 1]);
                double optionPos = Math.Max(peaks[row + 1, bin + 1], peaks[row + 2, bin + 1]);
                optionPos = peaks[row + 1, bin] + optionPos;

                // get max of the three next possible steps
                double[] directionOptions = { optionStraight, optionNeg, optionPos };
                var maxId = DataTools.GetMaxIndex(directionOptions);

                // Check if track has come to an end - average value of the two values is less than threshold.
                var maxValue = directionOptions[maxId] / 2.0;
                if (maxValue < threshold)
                {
                    peaks[row, bin] = 0.0;
                    return track;
                }

                // else set visited values to zero so as not to revisit.....
                peaks[row - 1, bin] = 0.0;
                peaks[row, bin] = 0.0;
                peaks[row + 1, bin] = 0.0;

                // and go to the new time frame
                if (maxId == 1)
                {
                    row--;
                }
                else
                if (maxId == 2)
                {
                    row++;
                }

                track.SetPoint(row, bin, maxValue);

                // next line is for debug purposes
                //var info = track.CheckPoint(row, bin);
            }

            return track;
        }
    }
}
