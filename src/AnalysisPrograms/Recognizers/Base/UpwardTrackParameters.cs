// <copyright file="UpwardTrackParameters.cs" company="QutEcoacoustics">
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
    /// Parameters needed from a config file to detect vertical track components i.e. events which are completed within very few time frames, i.e. whips and near clicks.
    /// An UpwardTrack sounds like a whip. Each track point ascends one frequency bin. Points may move forwards or back one frame step.
    /// </summary>
    [YamlTypeTag(typeof(UpwardTrackParameters))]
    public class UpwardTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets the minimum bandwidth, units = Hertz.
        /// </summary>
        public int? MinBandwidthHertz { get; set; }

        /// <summary>
        /// Gets or sets maximum bandwidth, units = Hertz.
        /// </summary>
        public int? MaxBandwidthHertz { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether proximal similar vertical tracks are to be combined.
        /// Proximal means track time starts are not separated by more than the specified seconds interval.
        /// Similar means that track frequency bounds do not differ by more than the specified Hertz interval.
        /// </summary>
        public bool CombineProximalSimilarEvents { get; set; }

        public TimeSpan StartDifference { get; set; }

        public int HertzDifference { get; set; }

        /// <summary>
        /// EXPANATION: A vertical track is a near click or rapidly frequency-modulated tone. A good example is the whip component of the whip-bird call.
        /// They would typically be only a few time-frames duration.
        /// THis method averages dB log values incorrectly but it is faster than doing many log conversions and is accurate enough for the purpose.
        /// </summary>
        /// <param name="sonogram">The spectrogram to be searched.</param>
        /// <param name="minHz">Bottom of the frequency band to be searched.</param>
        /// <param name="maxHz">Top of the frequency band to be searched.</param>
        /// <param name="decibelThreshold">Ignore spectrogram cells below this amplitude.</param>
        /// <param name="minBandwidthHertz">Minimum bandwidth (Hertz) of a valid event.</param>
        /// <param name="maxBandwidthHertz">Maximum bandwidth (Hertz) of a valid event.</param>
        /// <param name="combineProximalSimilarEvents">Combine tracks that are likely to be repeated chatter.</param>
        /// <param name="segmentStartOffset">The start time of the current recording segment under analysis.</param>
        /// <returns>A list of acoustic events containing foward tracks.</returns>
        public static (List<AcousticEvent> Events, double[] CombinedIntensity) GetUpwardTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double decibelThreshold,
            int minBandwidthHertz,
            int maxBandwidthHertz,
            bool combineProximalSimilarEvents,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            var frameStep = sonogram.FrameStep;
            int nyquist = sonogram.NyquistFrequency;
            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);

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
                for (int col = minBin; col < maxBin; col++)
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
            var tracks = TrackExtractor.GetUpwardTracks(peaks, minBin, maxBin, minBandwidthHertz, maxBandwidthHertz, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            var events = new List<AcousticEvent>();
            var temporalIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new AcousticEvent(segmentStartOffset, track.StartTimeSeconds, track.TrackDurationSeconds, track.LowFreqHertz, track.HighFreqHertz)
                {
                    TheTrack = track,
                    SegmentDurationSeconds = frameCount * frameStep,
                };

                events.Add(ae);

                // fill the intensity array
                //var startRow = (int)converter.TemporalScale.To(track.StartTimeSeconds);
                var startRow = converter.FrameFromStartTime(track.StartTimeSeconds);
                var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                for (int i = 0; i < amplitudeTrack.Length; i++)
                {
                    temporalIntensityArray[startRow + i] += amplitudeTrack[i];
                }
            }

            // combine proximal events that occupy similar frequency band
            if (combineProximalSimilarEvents)
            {
                TimeSpan startDifference = TimeSpan.FromSeconds(0.5);
                int hertzDifference = 500;
                events = AcousticEvent.CombineSimilarProximalEvents(events, startDifference, hertzDifference);
            }

            return (events, temporalIntensityArray);
        }
    }
}