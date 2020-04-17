// <copyright file="TrackExtractor.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Events.Tracks
{
    using System;
    using System.Collections.Generic;
    using TowseyLibrary;

    public static class TrackExtractor
    {
        public static List<Track> GetForwardTracks(double[,] peaks, double minDuration, double maxDuration, double threshold, UnitConverters converter)
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
                    // Visit each spectral peak in order. Each may be start of possible track
                    var track = GetForwardTrack(peaks, row, col, threshold, converter);

                    //If track has length within duration bounds, then add the track to list.
                    if (track.TrackDurationSeconds >= minDuration && track.TrackDurationSeconds <= maxDuration)
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

        /*
public static SpectralTrack_TO_BE_REMOVED GetVerticalTrack(double[,] peaks, int startRow, int startBin, int maxBin, double threshold)
{
    var track = new SpectralTrack_TO_BE_REMOVED(SpectralTrackType.VerticalTrack, startRow, startBin, peaks[startRow, startBin]);

    // set the start point in peaks matrix to zero to prevent return to this point.
    peaks[startRow, startBin] = 0.0;
    int row = startRow;

    for (int bin = startBin; bin < maxBin - 1; bin++)
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

        // explore options for track vertical
        double optionStraight = Math.Max(peaks[row, bin] + peaks[row, bin + 1], peaks[row, bin] + peaks[row - 1, bin + 1]);
        optionStraight = Math.Max(optionStraight, peaks[row, bin] + peaks[row + 1, bin + 1]);

        // option for track with negative slope i.e. return to previous row/frame.
        double optionDown = Math.Max(peaks[row - 1, bin] + peaks[row - 1, bin + 1], peaks[row - 1, bin] + peaks[row - 2, bin + 1]);
        optionDown = Math.Max(optionDown, peaks[row - 1, bin] + peaks[row - 1, bin + 1]);

        // option for track with positive slope
        double optionUp = Math.Max(peaks[row + 1, bin] + peaks[row + 1, bin + 1], peaks[row + 1, bin] + peaks[row + 2, bin + 1]);
        optionUp = Math.Max(optionUp, peaks[row + 1, bin] + peaks[row + 2, bin + 1]);

        // get max of the three next possible steps
        double[] options = { optionStraight, optionDown, optionUp };
        var maxId = DataTools.GetMaxIndex(options);

        // Check if track has come to an end - average value of the two values is less than threshold.
        var maxValue = options[maxId] / 2;
        if (maxValue < threshold)
        {
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
    }

    return track;
}
*/

        /// <summary>
        /// A general method to convert an array of score values to a list of AcousticEvents.
        /// NOTE: The score array is assumed to be a spectrum of dB intensity.
        /// The method uses the passed scoreThreshold in order to calculate a normalised score.
        /// Max possible score := threshold * 5.
        /// normalised score := score / maxPossibleScore.
        /// Some analysis techniques (e.g. Oscillation Detection) have their own methods for extracting events from score arrays.
        /// </summary>
        /// <param name="trackIntensityArray">the array of click intensity.</param>
        /// <param name="minHz">lower freq bound of the search band for click events.</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class.</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class.</param>
        /// <param name="scoreThreshold">threshold for the intensity values.</param>
        /// <param name="minBandwidth">bandwidth of click must exceed this to qualify as an event.</param>
        /// <param name="maxBandwidth">bandwidth of click must be less than this to qualify as an event.</param>
        /// <param name="frameNumber">time of start of the current frame.</param>
        /// <returns>a list of acoustic events.</returns>
        public static List<AcousticEvent> ConvertSpectralArrayToVerticalTrackEvents(
            double[] trackIntensityArray,
            int minHz,
            double framesPerSec,
            double freqBinWidth,
            double scoreThreshold,
            int minBandwidth,
            int maxBandwidth,
            int frameNumber,
            TimeSpan segmentStartOffset)
        {
            int binCount = trackIntensityArray.Length;
            var events = new List<AcousticEvent>();
            double maxPossibleScore = 5 * scoreThreshold; // used to calculate a normalised score between 0 - 1.0
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            int bottomFrequency = minHz; // units = Hertz
            int bottomBin = 0;

            // pass over all frequency bins except last two due to edge effect later.
            for (int i = 0; i < binCount - 2; i++)
            {
                if (isHit == false && trackIntensityArray[i] >= scoreThreshold)
                {
                    //low freq end of a track event
                    isHit = true;
                    bottomBin = i;
                    bottomFrequency = minHz + (int)Math.Round(i * freqBinWidth);
                }
                else // check for the high frequency end of a track event
                if (isHit && trackIntensityArray[i] <= scoreThreshold)
                {
                    // now check if there is acoustic intensity in next two frequncy bins
                    double avIntensity = (trackIntensityArray[i] + trackIntensityArray[i + 1] + trackIntensityArray[i + 2]) / 3;

                    if (avIntensity >= scoreThreshold)
                    {
                        // this is not top of vertical track - it continues through to higher frequency bins.
                        continue;
                    }

                    // bin(i - 1) is the upper Hz end of an event, so initialise it
                    isHit = false;
                    double eventBinWidth = i - bottomBin;
                    double hzBandwidth = (int)Math.Round(eventBinWidth * freqBinWidth);

                    //skip events having wrong bandwidth
                    if (hzBandwidth < minBandwidth || hzBandwidth > maxBandwidth)
                    {
                        continue;
                    }

                    // obtain an average score for the bandwidth of the potential event.
                    double av = 0.0;
                    for (int n = bottomBin; n <= i; n++)
                    {
                        av += trackIntensityArray[n];
                    }

                    av /= eventBinWidth;

                    // Initialize the event with: TimeSpan segmentStartOffset, double eventStartSegmentRelative, double eventDuration, etc
                    // Vertical track events are assumed to be two frames duration.  FIX THIS FIX THIS ################################################
                    double eventDuration = frameOffset * 2;
                    double startTimeRelativeSegment = frameOffset * frameNumber;
                    var ev = new AcousticEvent(segmentStartOffset, startTimeRelativeSegment, eventDuration, bottomFrequency, bottomFrequency + hzBandwidth);
                    ev.SetTimeAndFreqScales(frameOffset, freqBinWidth);
                    ev.Score = av;

                    // normalised to the user supplied threshold
                    ev.ScoreNormalised = ev.Score / maxPossibleScore;
                    if (ev.ScoreNormalised > 1.0)
                    {
                        ev.ScoreNormalised = 1.0;
                    }

                    ev.Score_MaxPossible = maxPossibleScore;

                    //find max score
                    double max = -double.MaxValue;
                    for (int n = bottomBin; n <= i; n++)
                    {
                        if (trackIntensityArray[n] > max)
                        {
                            max = trackIntensityArray[n];
                            ev.Score_MaxInEvent = trackIntensityArray[n];
                        }
                    }

                    events.Add(ev);
                }
            }

            return events;
        }
    }
}