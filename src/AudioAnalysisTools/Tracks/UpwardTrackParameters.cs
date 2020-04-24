// <copyright file="UpwardTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
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
        public static (List<EventCommon> Events, double[] CombinedIntensity) GetUpwardTracks(
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
            var events = new List<EventCommon>();
            var temporalIntensityArray = new double[frameCount];
            foreach (var track in tracks)
            {
                var ae = new WhipEvent(track)
                {
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
                // ########################################################################################## TODO TODO TODO
                //events = AcousticEvent.CombineSimilarProximalEvents(events, startDifference, hertzDifference);
            }

            return (events, temporalIntensityArray);
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
    }
}
