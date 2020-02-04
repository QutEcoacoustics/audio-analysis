// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WhistleParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>

using Acoustics.Shared;
using AudioAnalysisTools;
using AudioAnalysisTools.StandardSpectrograms;
using System;
using System.Collections.Generic;
using TowseyLibrary;

namespace AnalysisPrograms.Recognizers.Base
{
    /// <summary>
    /// Parameters needed from a config file to detect whistle components.
    /// </summary>
    [YamlTypeTag(typeof(WhistleParameters))]
    public class WhistleParameters : CommonParameters
    {

        /// <summary>
        /// Calculates the mean intensity in a freq band defined by its min and max freq.
        /// THis method averages dB log values incorrectly but it is faster than doing many log conversions.
        /// This method is used to find acoustic events and is accurate enough for the purpose.
        /// </summary>
        public static (List<AcousticEvent>, double[]) GetWhistles(SpectrogramStandard sonogram, int minHz, int maxHz, int nyquist, double decibelThreshold, double minDuration, double maxDuration)
        {
            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double binWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / binWidth);
            int maxBin = (int)Math.Round(maxHz / binWidth);
            //int binCountInBand = maxBin - minBin + 1;

            // buffer zone around whistle is three bins wide.
            int N = 3;

            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();
            var combinedIntensityArray = new double[frameCount];

            // for all frequency bins
            for (int bin = minBin; bin < maxBin; bin++)
            {
                // set up an intensity array for the frequency bin.
                double[] intensity = new double[frameCount];

                if (minBin < N)
                {
                    // for all time frames in this frequency bin
                    for (int t = 0; t < frameCount; t++)
                    {
                        var sideBandIntensity = ((0.5 * sonogramData[t, bin + 2]) + sonogramData[t, bin + 3]) / (double)2.0;
                        intensity[t] = sonogramData[t, bin] - sideBandIntensity;
                        //LoggedConsole.WriteLine($"t{t}  bin{bin}   intensity = {sonogramData[t, bin]} minus {sideBandIntensity}");
                        intensity[t] = Math.Max(0.0, intensity[t]);
                    }
                }
                else
                {
                    // for all time frames in this frequency bin
                    for (int t = 0; t < frameCount; t++)
                    {
                        var sideBandIntensity = ((0.5 * sonogramData[t, bin + 2]) + sonogramData[t, bin + 3] + (0.5 * sonogramData[t, bin - 2]) + sonogramData[t, bin - 3]) / (double)4.0;
                        intensity[t] = sonogramData[t, bin] - sideBandIntensity;
                        intensity[t] = Math.Max(0.0, intensity[t]);
                    }
                }

                // smooth the decibel array to allow for brief gaps.
                intensity = DataTools.filterMovingAverageOdd(intensity, 5);

                //extract the events based on length and threshhold.
                // Note: This method does NOT do prior smoothing of the dB array.
                // TODO check next line
                var segmentStartOffset = TimeSpan.Zero;
                var acousticEvents = AcousticEvent.ConvertScoreArray2Events(
                    intensity,
                    minHz,
                    maxHz,
                    sonogram.FramesPerSecond,
                    sonogram.FBinWidth,
                    decibelThreshold,
                    minDuration,
                    maxDuration,
                    segmentStartOffset);

                // add to conbined intensity array
                for (int t = 0; t < frameCount; t++)
                {
                    //combinedIntensityArray[t] += intensity[t];
                    combinedIntensityArray[t] = Math.Max(intensity[t], combinedIntensityArray[t]);
                }

                // combine events
                events.AddRange(acousticEvents);
            } //end for all freq bins

            return (events, combinedIntensityArray);
        }

        /*
        /// <summary>
        /// Calculates the average intensity in a freq band having min and max freq,
        /// AND then subtracts average intensity in the side/buffer bands, below and above.
        /// THis method adds dB log values incorrectly but it is faster than doing many log conversions.
        /// This method is used to find acoustic events and is accurate enough for the purpose.
        /// </summary>
        public static double[] CalculateFreqBandAvIntensityMinusBufferIntensity(double[,] sonogramData, int minHz, int maxHz, int nyquist)
        {
            var bandIntensity = SNR.CalculateFreqBandAvIntensity(sonogramData, minHz, maxHz, nyquist);
            var bottomSideBandIntensity = SNR.CalculateFreqBandAvIntensity(sonogramData, minHz - bottomHzBuffer, minHz, nyquist);
            var topSideBandIntensity = SNR.CalculateFreqBandAvIntensity(sonogramData, maxHz, maxHz + topHzBuffer, nyquist);

            int frameCount = sonogramData.GetLength(0);
            double[] netIntensity = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                netIntensity[i] = bandIntensity[i] - bottomSideBandIntensity[i] - topSideBandIntensity[i];
            }

            return netIntensity;
        }
        */
    }
}
