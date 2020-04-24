// <copyright file="TrackExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Tracks
{
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.StandardSpectrograms;
    using System;
    using System.Collections.Generic;
    using TowseyLibrary;

    public static class TrackExtractor
    {
        /// <summary>
        /// This method finds whistle tracks, that is horizontal ridges in one frequency bin.
        /// </summary>
        /// <param name="peaks">Peaks matrix.</param>
        /// <param name="minDuration">Minimum duration of a whistle.</param>
        /// <param name="maxDuration">Maximum duration of a whistle.</param>
        /// <param name="threshold">Minimum amplitude threshold to be valid whistle.</param>
        /// <param name="converter">For the time/frequency scales.</param>
        /// <returns>A list of whistle tracks.</returns>
        public static List<Track> GetOnebinTracks(double[,] peaks, double minDuration, double maxDuration, double threshold, UnitConverters converter)
        {
            int frameCount = peaks.GetLength(0);
            int bandwidthBinCount = peaks.GetLength(1);

            var tracks = new List<Track>();

            // Look for possible track starts and initialise as track.
            // Cannot include edge rows & columns because of edge effects.
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = 3; col < bandwidthBinCount - 3; col++)
                {
                    if (peaks[row, col] < threshold)
                    {
                        continue;
                    }

                    // Visit each spectral peak in order. Each may be start of possible whistle track
                    var track = GetOnebinTrack(peaks, row, col, threshold, converter);

                    //If track has length within duration bounds, then add the track to list.
                    if (track.DurationSeconds >= minDuration && track.DurationSeconds <= maxDuration)
                    {
                        tracks.Add(track);
                    }
                }
            }

            return tracks;
        }

        public static Track GetOnebinTrack(double[,] peaks, int startRow, int bin, double threshold, UnitConverters converter)
        {
            var track = new Track(converter, TrackType.FowardTrack);
            track.SetPoint(startRow, bin, peaks[startRow, bin]);

            // set the start point in peaks matrix to zero to prevent return to this point.
            peaks[startRow, bin] = 0.0;

            //Now move to next time frame.
            for (int row = startRow + 1; row < peaks.GetLength(0) - 2; row++)
            {
                // explore track in vicinity.
                int nhStart = Math.Max(row - 2, 0);
                int nhEnd = Math.Min(row + 3, peaks.GetLength(0));
                int nhWidth = nhEnd - nhStart + 1;
                double avIntensity = 0.0;
                for (int nh = nhStart; nh < nhEnd; nh++)
                {
                    avIntensity += peaks[nh, bin];
                }

                avIntensity /= (double)nhWidth;
                track.SetPoint(row, bin, peaks[row, bin]);

                // Set visited value to zero so as not to revisit.....
                peaks[row, bin] = 0.0;

                // next line is for debug purposes
                //var info = track.CheckPoint(row, bin);

                // Check if track has come to an end - average value is less than threshold.
                if (avIntensity < threshold)
                {
                    return track;
                }
            }

            return track;
        }

        public static List<Track> GetForwardTracks(double[,] peaks, double minDuration, double maxDuration, double threshold, UnitConverters converter)
        {
            int frameCount = peaks.GetLength(0);
            int binCount = peaks.GetLength(1);

            var tracks = new List<Track>();

            // Look for possible track starts and initialise as track.
            // Cannot include edge rows & columns because of edge effects.
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = 3; col < binCount - 3; col++)
                {
                    if (peaks[row, col] < threshold)
                    {
                        continue;
                    }

                    // Visit each spectral peak in order. Each may be start of possible track
                    var track = GetForwardTrack(peaks, row, col, threshold, converter);

                    //If track has length within duration bounds, then add the track to list.
                    if (track.DurationSeconds >= minDuration && track.DurationSeconds <= maxDuration)
                    {
                        tracks.Add(track);
                    }
                }
            }

            return tracks;
        }

        public static Track GetForwardTrack(double[,] peaks, int startRow, int startBin, double threshold, UnitConverters converter)
        {
            var track = new Track(converter, TrackType.FowardTrack);
            track.SetPoint(startRow, startBin, peaks[startRow, startBin]);

            // set the start point in peaks matrix to zero to prevent return to this point.
            peaks[startRow, startBin] = 0.0;

            int bin = startBin;
            for (int row = startRow + 1; row < peaks.GetLength(0) - 2; row++)
            {
                //cannot take bin value less than 3 because of edge effects.
                if (bin < 3)
                {
                    bin = 3;
                }

                if (bin > peaks.GetLength(1) - 4)
                {
                    bin = peaks.GetLength(1) - 4;
                }

                // explore options for track ahead
                double optionStraight = Math.Max(peaks[row, bin] + peaks[row + 1, bin], peaks[row, bin] + peaks[row + 1, bin - 1]);
                optionStraight = Math.Max(optionStraight, peaks[row, bin] + peaks[row + 1, bin + 1]);

                // option for track descent
                double optionDown = Math.Max(peaks[row, bin - 1] + peaks[row + 1, bin - 1], peaks[row, bin - 1] + peaks[row + 1, bin - 2]);
                optionDown = Math.Max(optionDown, peaks[row, bin - 1] + peaks[row + 1, bin]);

                // need this option for a steep track descent
                double optionTwoDown = Math.Max(peaks[row, bin - 2] + peaks[row + 1, bin - 2], peaks[row, bin - 2] + peaks[row + 1, bin - 1]);
                optionTwoDown = Math.Max(optionTwoDown, peaks[row, bin - 2] + peaks[row + 1, bin - 3]);

                // option for track asscent
                double optionUp = Math.Max(peaks[row, bin + 1] + peaks[row + 1, bin + 1], peaks[row, bin + 1] + peaks[row + 1, bin]);
                optionUp = Math.Max(optionUp, peaks[row, bin + 1] + peaks[row + 1, bin + 2]);

                // need this option for a steep track asscent
                double optionTwoUp = Math.Max(peaks[row, bin + 2] + peaks[row + 1, bin + 2], peaks[row, bin + 2] + peaks[row + 1, bin + 1]);
                optionTwoUp = Math.Max(optionTwoUp, peaks[row, bin + 2] + peaks[row + 1, bin + 3]);

                // get max of the five next possible steps
                double[] options = { optionStraight, optionDown, optionUp, optionTwoDown, optionTwoUp };

                //double[] options = { optionStraight, optionDown, optionUp, 0.0, 0.0 };
                var maxId = DataTools.GetMaxIndex(options);

                // if track has come to an end
                var maxValue = options[maxId] / 2.0;
                if (maxValue < threshold)
                {
                    break;
                }

                // else set bins to zero so as not to revisit.....
                for (int col = -2; col <= 2; col++)
                {
                    peaks[row, bin + col] = 0.0;
                }

                // and go to the new frequency bin
                if (maxId == 1)
                {
                    bin--;
                }
                else
                if (maxId == 2)
                {
                    bin++;
                }
                else
                if (maxId == 3)
                {
                    bin -= 2;
                }
                else
                if (maxId == 4)
                {
                    bin += 2;
                }

                track.SetPoint(row, bin, maxValue);

                // next line is for debug purposes
                var info = track.CheckPoint(row, bin);
            }

            return track;
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

                    //If track lies within the correct bandWidth range, then return as track.
                    if (track.TrackBandWidthHertz >= minBandwidthHertz && track.TrackBandWidthHertz <= maxBandwidthHertz)
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
                if (row < 1 || row > peaks.GetLength(0) - 1)
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

        /// <summary>
        /// Extracts click type tracks. A click occupies one frame. In a spectrogram it is equivalent to a vertical ridge.
        /// A click is a sudden onset event but may have a training echo.
        /// </summary>
        /// <param name="peaks">A matrix that identifies the location of sudden onset peaks.</param>
        /// <param name="minBin">Bottom of the frequency band to search.</param>
        /// <param name="maxBin">Top of the frequency band to search.</param>
        /// <param name="minBandwidthHertz">Minimum band width spanned by the click event.</param>
        /// <param name="maxBandwidthHertz">Maximum band width spanned by the click event.</param>
        /// <param name="threshold">The amplitude threshold to qualify as a sudden onset ridge.</param>
        /// <param name="converter">To do the time/freq scale conversions.</param>
        /// <returns>A list of click tracks.</returns>
        public static List<Track> GetOneFrameTracks(double[,] peaks, int minBin, int maxBin, double minBandwidthHertz, double maxBandwidthHertz, double threshold, UnitConverters converter)
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
                    var track = GetOneFrameTrack(peaks, row, col, maxBin, threshold, converter);

                    //If track lies within the correct bandWidth range, then return as track.
                    if (track.TrackBandWidthHertz >= minBandwidthHertz && track.TrackBandWidthHertz <= maxBandwidthHertz)
                    {
                        tracks.Add(track);
                    }
                }
            }

            return tracks;
        }

        public static Track GetOneFrameTrack(double[,] peaks, int startRow, int startBin, int maxBin, double threshold, UnitConverters converter)
        {
            var track = new Track(converter, TrackType.UpwardTrack);
            track.SetPoint(startRow, startBin, peaks[startRow, startBin]);

            // set the start point in peaks matrix to zero to prevent return to this point.
            peaks[startRow, startBin] = 0.0;

            //Now move to next higher freq bin.
            int row = startRow;
            for (int bin = startBin + 1; bin < maxBin - 1; bin++)
            {
                if (bin >= maxBin)
                {
                    // arrived at top of the requested frequency band - track has come to end
                    return track;
                }

                // Explore possibility for track moving to next higher frequency bin
                // Check if there is acoustic intensity in next two frequncy bins
                double avIntensity = (peaks[row, bin] + peaks[row, bin + 1] + peaks[row, bin + 2]) / 3.0;

                // Set visited value to zero so as not to revisit.....
                peaks[row, bin] = 0.0;

                // Check if track has come to an end - average value is less than threshold.
                if (avIntensity < threshold)
                {
                    return track;
                }

                track.SetPoint(row, bin, avIntensity);

                // next line is for debug purposes
                //var info = track.CheckPoint(row, bin);
            }

            return track;
        }

        /// <summary>
        /// This method returns foward (spectral peak) tracks enclosed in acoustic events.
        /// It averages dB log values incorrectly but it is faster than doing many log conversions.
        /// </summary>
        /// <param name="sonogram">The spectrogram to be searched.</param>
        /// <returns>A list of acoustic events containing foward tracks.</returns>
        public static (List<EventCommon> Events, double[] CombinedIntensity) GetForwardTracks(
            SpectrogramStandard sonogram,
            ForwardTrackParameters parameters,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            int nyquist = sonogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(parameters.MinHertz.Value / binWidth);
            int maxBin = (int)Math.Round(parameters.MaxHertz.Value / binWidth);

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: sonogram.SampleRate,
                frameSize: sonogram.Configuration.WindowSize,
                frameOverlap: sonogram.Configuration.WindowOverlap);

            //Find all spectral peaks and place in peaks matrix
            var peaks = new double[frameCount, binCount];
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = minBin + 1; col < maxBin - 1; col++)
                {
                    if (sonogramData[row, col] < parameters.DecibelThreshold)
                    {
                        continue;
                    }

                    // if given matrix element is greater than in freq bin either side
                    bool isPeak = (sonogramData[row, col] > sonogramData[row, col - 1]) && (sonogramData[row, col] > sonogramData[row, col + 1]);
                    if (isPeak)
                    {
                        peaks[row, col] = sonogramData[row, col];
                    }
                }
            }

            var tracks = TrackExtractor.GetForwardTracks(peaks, parameters.MinDuration.Value, parameters.MaxDuration.Value, parameters.DecibelThreshold.Value, converter);

            // initialise tracks as events and get the combined intensity array.
            // list of accumulated acoustic events
            var events = new List<EventCommon>();
            var combinedIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new ChirpEvent(track)
                {
                    SegmentDurationSeconds = frameCount * converter.StepSize,
                };

                events.Add(ae);

                // fill the intensity array
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    //combinedIntensityArray[startRow + i] += amplitudeTrack[i];
                    combinedIntensityArray[startRow + i] = Math.Max(combinedIntensityArray[startRow + i], amplitudeTrack[i]);
                }
            }

            // Combine coincident events that are stacked one above other.
            // This will help in some cases to combine related events.
            var startDifference = TimeSpan.FromSeconds(0.2);
            var hertzGap = 200;
            if (parameters.CombinePossibleHarmonics)
            {
                //######################################################################################### TODO TODO
                //events = CompositeEvent.CombinePotentialStackedTracks(events, startDifference, hertzGap);
            }

            return (events, combinedIntensityArray);
        }
    }
}