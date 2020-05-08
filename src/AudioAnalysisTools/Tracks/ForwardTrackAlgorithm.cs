// <copyright file="ForwardTrackAlgorithm.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Tracks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Acoustics.Shared;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Drawing;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    public static class ForwardTrackAlgorithm
    {
        /// <summary>
        /// This method returns foward (spectral peak) tracks enclosed in spectral events.
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
            double minDuration = parameters.MinDuration.Value;
            double maxDuration = parameters.MaxDuration.Value;
            double decibelThreshold = parameters.DecibelThreshold.Value;

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
                    if (sonogramData[row, col] < decibelThreshold)
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

            var tracks = GetForwardTracks(peaks, minDuration, maxDuration, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            // list of accumulated acoustic events
            var events = new List<SpectralEvent>();
            var combinedIntensityArray = new double[frameCount];

            // The following lines are used only for debug purposes.
            //var options = new EventRenderingOptions(new UnitConverters(segmentStartOffset.TotalSeconds, sonogram.Duration.TotalSeconds, nyquist, frameCount, binCount));
            //var spectrogram = sonogram.GetImage(doHighlightSubband: false, add1KHzLines: true, doMelScale: false);

            // Initialise events with tracks.
            foreach (var track in tracks)
            {
                //Following line used only for debug purposes. Can save as image.
                //spectrogram.Mutate(x => track.Draw(x, options));
                var maxScore = decibelThreshold * 5;
                var scoreRange = new Interval<double>(0, maxScore);
                var ae = new ChirpEvent(track, scoreRange)
                {
                    SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                    SegmentDurationSeconds = frameCount * converter.SecondsPerFrameStep,
                    Name = "noName",
                };

                events.Add(ae);

                // fill the intensity array
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    combinedIntensityArray[startRow + i] = Math.Max(combinedIntensityArray[startRow + i], amplitudeTrack[i]);
                }
            }

            List<EventCommon> returnEvents = events.Cast<EventCommon>().ToList();

            // Combine coincident events that are stacked one above other.
            // This will help in some cases to combine related events.
            if (parameters.CombinePossibleHarmonics)
            {
                returnEvents = CompositeEvent.CombinePotentialStackedTracks(events, parameters.HarmonicsStartDifference, parameters.HarmonicsHertzGap);
            }

            // Combine events that are temporally close and in the same frequency band.
            // This will help in some cases to combine related events.
            if (parameters.CombinePossibleSyllableSequence)
            {
                var timeDiff = TimeSpan.FromSeconds(parameters.SyllableStartDifference);
                returnEvents = CompositeEvent.CombineSimilarProximalEvents(events, timeDiff, parameters.SyllableHertzGap);
            }

            return (returnEvents, combinedIntensityArray);
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
            var track = new Track(converter, Events.Tracks.TrackType.FowardTrack);
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
                //var info = track.CheckPoint(row, bin);
            }

            return track;
        }
    }
}
