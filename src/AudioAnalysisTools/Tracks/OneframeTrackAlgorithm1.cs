// <copyright file="OneframeTrackAlgorithm.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Tracks
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;
    using TrackType = AudioAnalysisTools.Events.Tracks.TrackType;

    public static class OneframeTrackAlgorithm
    {
        /// <summary>
        /// A one-frame track sounds like a click.
        /// A click is a sharp onset broadband sound of brief duration. Geometrically it is similar to a vertical whistle.
        /// THis method averages dB log values incorrectly but it is faster than doing many log conversions.
        /// This method is used to find acoustic events and is accurate enough for the purpose.
        /// </summary>
        public static (List<EventCommon> Events, double[] Intensity) GetOneFrameTracks(
            SpectrogramStandard sonogram,
            OneframeTrackParameters parameters,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            var frameStep = sonogram.FrameStep;
            int nyquist = sonogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(parameters.MinHertz.Value / binWidth);
            int maxBin = (int)Math.Round(parameters.MaxHertz.Value / binWidth);
            var decibelThreshold = parameters.DecibelThreshold.Value;
            var minBandwidthHertz = parameters.MinBandwidthHertz.Value;
            var maxBandwidthHertz = parameters.MaxBandwidthHertz.Value;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: sonogram.SampleRate,
                frameSize: sonogram.Configuration.WindowSize,
                frameOverlap: sonogram.Configuration.WindowOverlap);

            // Find all frame peaks and place in peaks matrix
            // avoid row edge effects.
            var peaks = new double[frameCount, binCount];

            // for all time frames except 1st and last allowing for edge effects.
            for (int t = 1; t < frameCount - 1; t++)
            {
                // buffer zone around click is one frame wide.
                // for all frequency bins except top and bottom in this time frame
                for (int bin = minBin; bin < maxBin; bin++)
                {
                    if (sonogramData[t, bin] < decibelThreshold)
                    {
                        continue;
                    }

                    // THis is where the profile of a click is defined
                    // A click requires sudden onset, with maximum amplitude followed by decay.
                    bool isClickPeak = sonogramData[t - 1, bin] < decibelThreshold && sonogramData[t, bin] > sonogramData[t + 1, bin];
                    if (isClickPeak)
                    {
                        peaks[t, bin] = sonogramData[t, bin];
                    }
                }
            }

            //NOTE: the Peaks matrix is same size as the sonogram.
            var tracks = GetOneFrameTracks(peaks, minBin, maxBin, minBandwidthHertz, maxBandwidthHertz, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            var events = new List<EventCommon>();
            var temporalIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new ClickEvent(track)
                {
                    SegmentDurationSeconds = frameCount * frameStep,
                };

                events.Add(ae);

                // fill the intensity array
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    temporalIntensityArray[startRow + i] += amplitudeTrack[i];
                }
            }

            // MAY NOT WANT TO Do THIS FOR ONE-FRAME tracks
            // combine proximal events that occupy similar frequency band
            //if (combineProximalSimilarEvents)
            //{
            //    TimeSpan startDifference = TimeSpan.FromSeconds(0.5);
            //    int hertzDifference = 500;
            //    //######################################################################## TODO TODO TODOD
            //    //events = AcousticEvent.CombineSimilarProximalEvents(events, startDifference, hertzDifference);
            //}

            return (events, temporalIntensityArray);
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
    }
}
