// <copyright file="WhistleParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

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
        public static (List<AcousticEvent>, double[]) GetWhistles(
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

            // list of accumulated acoustic events
            var events = new List<AcousticEvent>();
            var combinedIntensityArray = new double[frameCount];

            // for all frequency bins except top and bottom
            for (int bin = minBin + 1; bin < maxBin; bin++)
            {
                // set up an intensity array for the frequency bin.
                double[] intensity = new double[frameCount];

                // buffer zone around whistle is four bins wide.
                if (minBin < 4)
                {
                    // for all time frames in this frequency bin
                    for (int t = 0; t < frameCount; t++)
                    {
                        var bandIntensity = (sonogramData[t, bin - 1] + sonogramData[t, bin] + sonogramData[t, bin + 1]) / 3.0;
                        var topSideBandIntensity = (sonogramData[t, bin + 3] + sonogramData[t, bin + 4] + sonogramData[t, bin + 5]) / 3.0;
                        intensity[t] = bandIntensity - topSideBandIntensity;
                        intensity[t] = Math.Max(0.0, intensity[t]);
                    }
                }
                else
                {
                    // for all time frames in this frequency bin
                    for (int t = 0; t < frameCount; t++)
                    {
                        var bandIntensity = (sonogramData[t, bin - 1] + sonogramData[t, bin] + sonogramData[t, bin + 1]) / 3.0;
                        var topSideBandIntensity = (sonogramData[t, bin + 3] + sonogramData[t, bin + 4] + sonogramData[t, bin + 5]) / 6.0;
                        var bottomSideBandIntensity = (sonogramData[t, bin - 3] + sonogramData[t, bin - 4] + sonogramData[t, bin - 5]) / 6.0;
                        intensity[t] = bandIntensity - topSideBandIntensity - bottomSideBandIntensity;
                        intensity[t] = Math.Max(0.0, intensity[t]);
                    }
                }

                // smooth the decibel array to allow for brief gaps.
                intensity = DataTools.filterMovingAverageOdd(intensity, 7);

                //calculate the Hertz bounds of the acoustic events for these freq bins
                int bottomHzBound = (int)Math.Floor(sonogram.FBinWidth * (bin - 1));
                int topHzBound = (int)Math.Ceiling(sonogram.FBinWidth * (bin + 2));

                //extract the events based on length and threshhold.
                // Note: This method does NOT do prior smoothing of the dB array.
                var acousticEvents = AcousticEvent.ConvertScoreArray2Events(
                    intensity,
                    bottomHzBound,
                    topHzBound,
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

            // combine adjacent acoustic events
            events = AcousticEvent.CombineOverlappingEvents(events, segmentStartOffset);

            return (events, combinedIntensityArray);
        }
    }
}
