// <copyright file="SpectralPeakTrackParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accord;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect spectral peak tracks.
    /// </summary>
    [YamlTypeTag(typeof(SpectralPeakTrackParameters))]
    public class SpectralPeakTrackParameters : CommonParameters
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
        /// This method returns spectral peak tracks enclosed in acoustic events.
        /// It averages dB log values incorrectly but it is faster than doing many log conversions.
        /// </summary>
        public static (List<AcousticEvent> Events, double[] CombinedIntensity) GetSpectralPeakTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            int nyquist,
            double decibelThreshold,
            double minDuration,
            double maxDuration,
            bool combinePossibleHarmonics,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);
            int bandwidthBinCount = maxBin - minBin + 1;
            var frameDuration = sonogram.FrameDuration;
            var frameStep = sonogram.FrameStep;
            var frameOverStep = frameDuration - frameStep;

            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();

            //Find all spectral peaks and place in peaks matrix
            var peaks = new double[frameCount, bandwidthBinCount];
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = minBin - 1; col < maxBin - 1; col++)
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

            // Look for track starts and initialise them as events.
            // Cannot include edge rows & columns because of edge effects.
            // Each row is a time frame which is a spectrum. Each column is a frequency bin
            var combinedIntensityArray = new double[frameCount];
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = 3; col < bandwidthBinCount - 3; col++)
                {
                    // Visit each spectral peak in order. Each may be start of possible track
                    if (peaks[row, col] < decibelThreshold)
                    {
                        continue;
                    }

                    //have the beginning of a potential track
                    var track = GetTrack(peaks, row, col, decibelThreshold);

                    int trackStartFrame = track.GetStartFrame();
                    int trackEndFrame = track.GetEndFrame();
                    double trackDuration = ((trackEndFrame - trackStartFrame) * frameStep) + frameOverStep;

                    // calculate max and min bin IDs in the original spectrogram
                    int trackBottomBin = track.GetBottomFreqBin();
                    int trackTopBin = track.GetTopFreqBin();

                    //If track has length within duration bounds, then create an event
                    if (trackDuration >= minDuration && trackDuration <= maxDuration)
                    {
                        var oblong = new Oblong(track.GetStartFrame(), trackBottomBin, track.GetEndFrame(), trackTopBin);
                        var ae = new AcousticEvent(segmentStartOffset, oblong, nyquist, binCount, frameDuration, frameStep, frameCount)
                        {
                            // get the track as matrix
                            TheTrack = track.GetTrackAsMatrix(frameStep, binWidth),
                        };
                        events.Add(ae);

                        // fill the intensity array
                        var amplitudeTrack = track.GetAmplitudeOverTimeFrames();
                        for (int i = 0; i < amplitudeTrack.Length; i++)
                        {
                            combinedIntensityArray[row + i] += amplitudeTrack[i];
                        }
                    }
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

        public static SpectralTrack GetTrack(double[,] peaks, int startRow, int startBin, double threshold)
        {
            var track = new SpectralTrack(startRow, startBin, peaks[startRow, startBin]);

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
                var maxValue = options[maxId] / 2;
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
            }

            return track;
        }
    }
}
