// <copyright file="VerticalTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect vertical track components i.e. events which are completed within very few time frames, i.e. whips and near clicks.
    /// </summary>
    [YamlTypeTag(typeof(VerticalTrackParameters))]
    public class VerticalTrackParameters : CommonParameters
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
        public static (List<AcousticEvent> Events, double[] CombinedIntensity) GetVerticalTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            int nyquist,
            double decibelThreshold,
            int minBandwidthHertz,
            int maxBandwidthHertz,
            bool combineProximalSimilarEvents,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);
            var frameDuration = sonogram.FrameDuration;
            var frameStep = sonogram.FrameStep;
            var frameOverStep = frameDuration - frameStep;

            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);

            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();
            var temporalIntensityArray = new double[frameCount];

            //Find all frame peaks and place in peaks matrix
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

            // Look for track starts and initialise them as events.
            // Cannot include edge rows & columns because of edge effects.
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            var combinedIntensityArray = new double[frameCount];
            for (int col = minBin; col < maxBin; col++)
            {
                for (int row = 2; row < frameCount - 2; row++)
                {
                    // Visit each frame peak in order. Each may be start of possible track
                    if (peaks[row, col] < decibelThreshold)
                    {
                        continue;
                    }

                    //have the beginning of a potential track
                    var track = GetVerticalTrack(peaks, row, col, maxBin, decibelThreshold);

                    // calculate first and last of the frame IDs in the original spectrogram
                    int trackStartFrame = track.GetStartFrame();
                    int trackEndFrame = track.GetEndFrame();

                    // next two for debug purposes
                    //int trackMinBin = track.GetBottomFreqBin();
                    //int trackTopBin = track.GetTopFreqBin();

                    //If track has lies within the correct bandWidth range, then create an event
                    int trackBandWidth = track.GetTrackBandWidthHertz(binWidth);
                    if (trackBandWidth >= minBandwidthHertz && trackBandWidth <= maxBandwidthHertz)
                    {
                        // get the oblong and init an event
                        double trackDuration = ((trackEndFrame - trackStartFrame) * frameStep) + frameOverStep;
                        var oblong = new Oblong(trackStartFrame, col, trackEndFrame, track.GetTopFreqBin());
                        var ae = new AcousticEvent(segmentStartOffset, oblong, nyquist, binCount, frameDuration, frameStep, frameCount)
                        {
                            // get the track as matrix
                            TheTrack = track,
                        };

                        events.Add(ae);

                        // fill the intensity array
                        var amplitudeArray = track.GetAmplitudeOverTimeFrames();
                        for (int i = 0; i < amplitudeArray.Length; i++)
                        {
                            combinedIntensityArray[row + i] += amplitudeArray[i];
                        }
                    }
                } // rows/frames
            } // end cols/bins

            // combine proximal events that occupy similar frequency band
            if (combineProximalSimilarEvents)
            {
                TimeSpan startDifference = TimeSpan.FromSeconds(0.5);
                int hertzDifference = 500;
                events = AcousticEvent.CombineSimilarProximalEvents(events, startDifference, hertzDifference);
            }

            return (events, temporalIntensityArray);
        }

        public static SpectralTrack GetVerticalTrack(double[,] peaks, int startRow, int startBin, int maxBin, double threshold)
        {
            var track = new SpectralTrack(SpectralTrackType.VerticalTrack, startRow, startBin, peaks[startRow, startBin]);

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
