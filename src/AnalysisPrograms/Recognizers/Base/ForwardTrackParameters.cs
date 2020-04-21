// <copyright file="ForwardTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events.Interfaces;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect fowards spectral peak tracks.
    /// A FowardTrack sounds like a fluctuating tone or technically, a chirp. Each track point advances one time step. Points may move up or down by at most two frequency bins.
    /// </summary>
    [YamlTypeTag(typeof(ForwardTrackParameters))]
    public class ForwardTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether coincident tracks stacked on top of one another are to be combined.
        /// Coincident means the tracks' start and end times are not greater than the specified seconds interval.
        /// Stacked means that the frequency gap between each of the stacked tracks does not exceed the specified Hertz interval.
        /// </summary>
        public bool CombinePossibleHarmonics { get; set; }

        public TimeSpan StartDifference { get; set; }

        public int HertzGap { get; set; }

        /// <summary>
        /// This method returns foward (spectral peak) tracks enclosed in acoustic events.
        /// It averages dB log values incorrectly but it is faster than doing many log conversions.
        /// </summary>
        /// <param name="sonogram">The spectrogram to be searched.</param>
        /// <param name="minHz">Bottom of the frequency band to be searched.</param>
        /// <param name="maxHz">Top of the frequency band to be searched.</param>
        /// <param name="decibelThreshold">Ignore spectrogram cells below this amplitude.</param>
        /// <param name="minDuration">Minimum duration of a valid event.</param>
        /// <param name="maxDuration">Maximum duration of a valid event.</param>
        /// <param name="combinePossibleHarmonics">Combine tracks that are likely to be harmonics/formants.</param>
        /// <param name="segmentStartOffset">The start time of the current recording segment under analysis.</param>
        /// <returns>A list of acoustic events containing foward tracks.</returns>
        public static (List<AcousticEvent> Events, double[] CombinedIntensity) GetForwardTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double decibelThreshold,
            double minDuration,
            double maxDuration,
            bool combinePossibleHarmonics,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            int nyquist = sonogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);

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

            var tracks = TrackExtractor.GetForwardTracks(peaks, minDuration, maxDuration, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();
            var combinedIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new AcousticEvent(segmentStartOffset, track)
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
            if (combinePossibleHarmonics)
            {
                events = AcousticEvent.CombinePotentialStackedTracks(events, startDifference, hertzGap);
            }

            return (events, combinedIntensityArray);
        }
    }
}
