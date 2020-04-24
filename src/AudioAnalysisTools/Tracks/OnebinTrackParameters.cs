// <copyright file="OnebinTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Tracks;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect whistle components.
    /// A one-bin sounds like a pure-tone whistle. Each track point advances one time step. Points stay in the same frequency bin.
    /// </summary>
    [YamlTypeTag(typeof(OnebinTrackParameters))]
    public class OnebinTrackParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether proximal whistle tracks are to be combined.
        /// Proximal means the whistle tracks are in the same frequency band
        /// ... and that the gap between their start times is not greater than the specified seconds interval.
        /// </summary>
        public bool CombinePossibleSequence { get; set; }

        /// <summary>
        /// This method averages dB log values incorrectly but it is faster than doing many log conversions.
        /// This method is used to find acoustic events and is accurate enough for the purpose.
        /// </summary>
        public static (List<EventCommon> ListOfevents, double[] CombinedIntensityArray) GetOnebinTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double decibelThreshold,
            double minDuration,
            double maxDuration,
            bool combinePossibleSequence,
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

            //Find all bin peaks and place in peaks matrix
            var peaks = new double[frameCount, binCount];
            for (int tf = 0; tf < frameCount; tf++)
            {
                for (int bin = minBin + 1; bin < maxBin - 1; bin++)
                {
                    if (sonogramData[tf, bin] < decibelThreshold)
                    {
                        continue;
                    }

                    // here we define the amplitude profile of a whistle. The buffer zone around whistle is five bins wide.
                    var bandIntensity = ((sonogramData[tf, bin - 1] * 0.5) + sonogramData[tf, bin] + (sonogramData[tf, bin + 1] * 0.5)) / 2.0;
                    var topSidebandIntensity = (sonogramData[tf, bin + 3] + sonogramData[tf, bin + 4] + sonogramData[tf, bin + 5]) / 3.0;
                    var netAmplitude = 0.0;
                    if (bin < 4)
                    {
                        netAmplitude = bandIntensity - topSidebandIntensity;
                    }
                    else
                    {
                        var bottomSideBandIntensity = (sonogramData[tf, bin - 3] + sonogramData[tf, bin - 4] + sonogramData[tf, bin - 5]) / 3.0;
                        netAmplitude = bandIntensity - ((topSidebandIntensity + bottomSideBandIntensity) / 2.0);
                    }

                    if (netAmplitude >= decibelThreshold)
                    {
                        peaks[tf, bin] = sonogramData[tf, bin];
                    }
                }
            }

            var tracks = TrackExtractor.GetOnebinTracks(peaks, minDuration, maxDuration, decibelThreshold, converter);

            // initialise tracks as events and get the combined intensity array.
            var events = new List<EventCommon>();
            var combinedIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new WhistleEvent(track)
                {
                    SegmentDurationSeconds = frameCount * converter.StepSize,
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

            // Combine possible related events.
            // This will help in some cases.
            var startDifference = TimeSpan.FromSeconds(0.5);
            var hertzGap = 100;
            if (combinePossibleSequence)
            {
                //################################################################################TODO TODO
                //events = AcousticEvent.CombineSimilarProximalEvents(events, startDifference, hertzGap);
            }

            return (events, combinedIntensityArray);
        }
    }
}