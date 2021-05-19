// <copyright file="OnebinTrackAlgorithm.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.Tracks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using TowseyLibrary;
    using TrackType = AudioAnalysisTools.Events.Tracks.TrackType;

    /// <summary>
    /// This class searches a spectrogram for whistles, that is, for tones or spectral peaks that persist in one frequency bin.
    /// In practice, the whistles of birds and other natural sources do not occupy a single frequency bin,
    /// although this statement is confounded by the choice of recording sample rate and frame size.
    /// But typically, a bird whistle spreads itself across three or more frequency bins using typical values for SR etc.
    /// In this class, we make an assumption about the spectral profile of a whistle and the user is expected to find the appropriate
    /// sample rate, frame size and frame step such that the target whistle is detected using the profile.
    /// We define a whistle profile that is 11 bins wide. The actual whistle occupies the centre three bins, ie bins -1, 0 , +1.
    /// Bins -2 and +2 are ignored to allow for some flexibility in getting he right combination of sample rate, frame size and frame step.
    /// To establish that the centre three bins contain a spectral peak (i.e. are part of a potential whistle),
    /// we define top and bottom sidebands, each of width three bins.
    /// These are used to establish a baseline intensity which must be less than that of the centre three bins.
    /// The bottom sideband = bins -3, -4, -5. The top sideband = bins +3, +4, +5.
    /// Defining a whistle this way introduces edge effects at the top and bottom of the spectrogram.
    /// In case of the low frequency edge, in order to get as close as possible to the frequency bin zero, we do not incorporate a bottom sidebound into the calculations.
    /// Also note that a typical bird whistle is not exactly a pure tone. It typically fluctuates slightly from one frequency bin to an adjacent bin and back.
    /// Consequently a final step in this whistle detection algorithm is to merge adjacent whistle tracks.
    /// The algorithm is not perfect but it does detect constant tone sounds. Theis algorithm is designed so as not to pick up chirps,
    /// i.e. gradually rising and falling tones. However, here again the right choice of SR, frame size and frame step are important.
    /// </summary>
    public static class OnebinTrackAlgorithm
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static (List<EventCommon> Events, List<Plot> DecibelPlots) GetOnebinTracks(
            SpectrogramStandard spectrogram,
            OnebinTrackParameters parameters,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            double[] decibelArray;
            List<EventCommon> events;

            (events, decibelArray) = GetOnebinTracks(
            spectrogram,
            parameters,
            segmentStartOffset,
            decibelThreshold.Value);

            foreach (var ev in events)
            {
                ev.Name = profileName;
            }

            spectralEvents.AddRange(events);

            var plot = Plot.PreparePlot(decibelArray, $"{profileName} (Whistles:{decibelThreshold.Value:F0}dB)", decibelThreshold.Value);
            plots.Add(plot);

            return (spectralEvents, plots);
        }

        /// <summary>
        /// This method returns whistle (spectral peak) tracks enclosed as spectral events.
        /// It averages dB log values incorrectly but it is faster than doing many log conversions.
        /// </summary>
        /// <param name="spectrogram">The spectrogram to be searched.</param>
        /// <param name="parameters">The parameters that determine the search.</param>
        /// <param name="segmentStartOffset">Enables assignment of a start time (relative to recording) to any valid event.</param>
        /// <param name="decibelThreshold">The threshold for detection of a track.</param>
        /// <returns>A list of acoustic events containing whistle tracks.</returns>
        public static (List<EventCommon> ListOfevents, double[] CombinedIntensityArray) GetOnebinTracks(
            SpectrogramStandard spectrogram,
            OnebinTrackParameters parameters,
            TimeSpan segmentStartOffset,
            double decibelThreshold)
        {
            var spectroData = spectrogram.Data;
            int frameCount = spectroData.GetLength(0);
            int binCount = spectroData.GetLength(1);
            int nyquist = spectrogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;

            // calculate the frequency bin for bottom of search band
            // Allow for whistle sideband = one bin
            int minSearchBin = (int)Math.Floor(parameters.MinHertz.Value / binWidth);
            if (minSearchBin < 1)
            {
                minSearchBin = 1;
            }

            // calculate the frequency bin for top of search band, allowing for the top sideband.
            // see class summary above.
            int topSideband = 6;
            int maxSearchBin = (int)Math.Floor(parameters.MaxHertz.Value / binWidth) - 1;
            if (maxSearchBin > binCount - topSideband)
            {
                maxSearchBin = binCount - topSideband;
            }

            // get max and min duration for the whistle event.
            double minDuration = parameters.MinDuration.Value;
            double maxDuration = parameters.MaxDuration.Value;

            var converter = new UnitConverters(
                segmentStartOffset: segmentStartOffset.TotalSeconds,
                sampleRate: spectrogram.SampleRate,
                frameSize: spectrogram.Configuration.WindowSize,
                frameOverlap: spectrogram.Configuration.WindowOverlap);

            //Find all bin peaks and place in peaks matrix
            // tf = timeframe and bin = frequency bin.
            var peaksMatrix = new double[frameCount, binCount];
            for (int tf = 0; tf < frameCount; tf++)
            {
                for (int bin = minSearchBin; bin <= maxSearchBin; bin++)
                {
                    //skip spectrogram cells below threshold
                    if (spectroData[tf, bin] < decibelThreshold)
                    {
                        continue;
                    }

                    // Here we define the amplitude profile of a whistle. The profile is 11 bins wide.
                    // The whistle occupies the centre three bins, ie bins -1, 0 , +1. Bins -2 and +2 are ignored.
                    // A top and bottom sidebands, each of width three bins, are used to establish a baseline intensity.
                    // The bottom sideband = bins -3, -4, -5. The top sideband = bins +3, +4, +5.
                    // For more detail see the class summary.
                    var bandIntensity = ((spectroData[tf, bin - 1] * 0.5) + spectroData[tf, bin] + (spectroData[tf, bin + 1] * 0.5)) / 2.0;
                    var topSidebandIntensity = (spectroData[tf, bin + 3] + spectroData[tf, bin + 4] + spectroData[tf, bin + 5]) / 3.0;
                    var netAmplitude = 0.0;
                    if (bin < 5)
                    {
                        // if bin < 5, i.e. too close to the bottom bin of the spectrogram, then only subtract intensity of the top sideband.
                        // see class summary above.
                        netAmplitude = bandIntensity - topSidebandIntensity;
                    }
                    else
                    {
                        var bottomSideBandIntensity = (spectroData[tf, bin - 3] + spectroData[tf, bin - 4] + spectroData[tf, bin - 5]) / 3.0;
                        netAmplitude = bandIntensity - ((topSidebandIntensity + bottomSideBandIntensity) / 2.0);
                    }

                    if (netAmplitude >= decibelThreshold)
                    {
                        peaksMatrix[tf, bin] = spectroData[tf, bin];
                    }
                }
            }

            var tracks = GetOnebinTracks(peaksMatrix, minDuration, maxDuration, decibelThreshold, converter);

            // Initialise tracks as events and get the combined intensity array.
            var events = new List<WhistleEvent>();
            var combinedIntensityArray = new double[frameCount];
            int scalingFactor = 5; // used to make plot easier to interpret.
            var scoreRange = new Interval<double>(0, decibelThreshold * scalingFactor);

            foreach (var track in tracks)
            {
                // fill the intensity array with decibel values
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    combinedIntensityArray[startRow + i] = Math.Max(combinedIntensityArray[startRow + i], amplitudeTrack[i]);
                }

                // Skip tracks that do not have duration within required duration bounds.
                if (track.DurationSeconds < minDuration || track.DurationSeconds > maxDuration)
                {
                    continue;
                }

                //Following line used only for debug purposes. Can save as image.
                //spectrogram.Mutate(x => track.Draw(x, options));
                var ae = new WhistleEvent(track, scoreRange)
                {
                    SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                    SegmentDurationSeconds = frameCount * converter.SecondsPerFrameStep,
                    Name = "Whistle", // this name can be overridden later.
                };

                events.Add(ae);
            }

            // This algorithm tends to produce temporally overlapped whistle events in adjacent channels.
            // This is because a typical bird whistle is not exactly horozontal.
            // Combine overlapping whistle events if they are within four frequency bins of each other.
            // The value 4 is somewhat arbitrary but is consistent with the whistle profile described in the class comments above.
            var hertzDifference = 4 * binWidth;
            var whistleEvents = WhistleEvent.CombineAdjacentWhistleEvents(events, hertzDifference);

            return (whistleEvents, combinedIntensityArray);
        }

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
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            // Cannot include the three edge columns/frequency bins because of edge effects when determining a valid peak.
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

                    // a track should have length > 2
                    if (track.PointCount > 2)
                    {
                        tracks.Add(track);
                    }
                }
            }

            return tracks;
        }

        public static Track GetOnebinTrack(double[,] peaks, int startRow, int bin, double threshold, UnitConverters converter)
        {
            var track = new Track(converter, TrackType.OneBinTrack);
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
    }
}
