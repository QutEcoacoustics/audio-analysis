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
        /// This method returns spectral peak tracks enclosed in acoustic events.
        /// It averages dB log values incorrectly but it is faster than doing many log conversions.
        /// </summary>
        public static (List<AcousticEvent>, double[]) GetSpectralPeakTracks(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            int nyquist,
            double decibelThreshold,
            double minDuration,
            double maxDuration,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);
            int bandWidth = maxBin - minBin + 1;
            var frameDuration = sonogram.FrameDuration;
            var frameStep = sonogram.FrameStep;

            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();

            //Get the required frequency band
            var band = MatrixTools.Submatrix(sonogramData, 0, minBin, frameCount - 1, maxBin);

            //Find all spectral peaks and place in peaks matrix
            var peaks = new double[frameCount, bandWidth];
            for (int row = 0; row < frameCount; row++)
            {
                for (int col = 1; col < bandWidth - 1; col++)
                {
                    if (band[row, col] < decibelThreshold)
                    {
                        continue;
                    }

                    bool isPeak = (band[row, col] > band[row, col - 1]) && (band[row, col] > band[row, col + 1]);
                    if (isPeak)
                    {
                        peaks[row, col] = band[row, col];
                    }
                }
            }

            //Look for track starts and initialise them as events.
            // Cannot used edge rows & columns because of edge effects.
            var combinedIntensityArray = new double[frameCount];
            for (int row = 1; row < frameCount; row++)
            {
                for (int col = 3; col < bandWidth - 3; col++)
                {
                    // if this spectral peak is possible start of a track
                    if (peaks[row, col] >= decibelThreshold
                        && peaks[row - 1, col] <= decibelThreshold
                        && peaks[row - 1, col - 1] <= decibelThreshold
                        && peaks[row - 1, col + 1] <= decibelThreshold)
                    {
                        //have the beginning of a potential track
                        (int[] BinIds, double[] Amplitude) track = GetTrack(peaks, row, col, decibelThreshold);

                        // calculate max and min bin IDs in the original spectrogram
                        int trackMinBin = track.BinIds.Min() + minBin;
                        int trackMaxBin = track.BinIds.Max() + minBin;
                        double trackDuration = track.BinIds.Length * frameDuration;

                        //Ignore short tracks.
                        if (trackDuration < minDuration || trackDuration > maxDuration)
                        {
                            break;
                        }

                        for (int i = 0; i < track.Amplitude.Length; i++)
                        {
                            combinedIntensityArray[row + i] += track.Amplitude[i];
                        }

                        var oblong = new Oblong(row, trackMinBin - 1, row + track.BinIds.Length, trackMaxBin + 1);
                        var ae = new AcousticEvent(segmentStartOffset, oblong, nyquist, binCount, frameDuration, frameStep, frameCount);
                        events.Add(ae);
                    }
                }
            }

            return (events, combinedIntensityArray);
        }

        public static (int[] BinIds, double[] Amplitude) GetTrack(double[,] peaks, int startRow, int startCol, double threshold)
        {
            var binIds = new List<int>
            {
                startCol,
            };
            var ampltd = new List<double>
            {
                peaks[startRow, startCol],
            };

            // set the start point to zero to prevent return to this point.
            peaks[startRow, startCol] = 0.0;

            int bin = startCol;
            for (int row = startRow + 2; row < peaks.GetLength(0) - 2; row++)
            {
                //cannot take bin value less than 3 because of edge effects.
                if (bin < 3)
                {
                    bin = 3;
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

                binIds.Add(bin);
                ampltd.Add(maxValue);
            }

            return (binIds.ToArray(), ampltd.ToArray());
        }
    }
}
