// <copyright file="HarmonicParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.Events.Types;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// Parameters needed from a config file to detect the stacked harmonic components of a soundscape.
    /// This can also be used for recognizing the harmonics of non-biological sounds such as from turbines, motor-bikes, compressors, hi-revving motors, etc.
    /// </summary>
    [YamlTypeTag(typeof(HarmonicParameters))]
    public class HarmonicParameters : CommonParameters
    {
        /// <summary>
        /// Gets or sets the dctThreshold.
        /// </summary>
        public double? DctThreshold { get; set; }

        /// <summary>
        /// Gets or sets the bottom bound of the rectangle. Units are Hertz.
        /// </summary>
        public int? MinFormantGap { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of the rectangle. Units are Hertz.
        /// </summary>
        public int? MaxFormantGap { get; set; }

        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            HarmonicParameters hp,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            // get the array of decibel thresholds
            var thresholdArray = hp.DecibelThresholds;

            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            // loop through the array of decibel thresholds
            foreach (var threshold in thresholdArray)
            {
                double[] decibelMaxArray;
                double[] harmonicIntensityScores;
                (spectralEvents, decibelMaxArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
                                    spectrogram,
                                    hp.MinHertz.Value,
                                    hp.MaxHertz.Value,
                                    spectrogram.NyquistFrequency,
                                    threshold.Value,
                                    hp.DctThreshold.Value,
                                    hp.MinDuration.Value,
                                    hp.MaxDuration.Value,
                                    hp.MinFormantGap.Value,
                                    hp.MaxFormantGap.Value,
                                    segmentStartOffset);

                // prepare plot of resultant Harmonics decibel array.
                var plot = Plot.PreparePlot(decibelMaxArray, $"{profileName} (Harmonics:{threshold:F0}db)", threshold.Value);
                plots.Add(plot);
            }

            return (spectralEvents, plots);
        }

        public static (List<EventCommon> SpectralEvents, double[] AmplitudeArray, double[] HarmonicIntensityScores) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            int minHz,
            int maxHz,
            int nyquist,
            double decibelThreshold,
            double dctThreshold,
            double minDuration,
            double maxDuration,
            int minFormantGap,
            int maxFormantGap,
            TimeSpan segmentStartOffset)
        {
            var sonogramData = spectrogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double freqBinWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / freqBinWidth);
            int maxBin = (int)Math.Round(maxHz / freqBinWidth);

            // extract the sub-band
            double[,] subMatrix = MatrixTools.Submatrix(spectrogram.Data, 0, minBin, frameCount - 1, maxBin);

            //ii: DETECT HARMONICS
            // now look for harmonics in search band using the Xcorrelation-DCT method.
            var results = CrossCorrelation.DetectHarmonicsInSpectrogramData(subMatrix, decibelThreshold);

            // set up score arrays
            double[] dBArray = results.Item1;
            double[] harmonicIntensityScores = results.Item2; //an array of formant intesnity
            int[] maxIndexArray = results.Item3;

            for (int r = 0; r < frameCount; r++)
            {
                if (harmonicIntensityScores[r] < dctThreshold)
                {
                    continue;
                }

                //ignore locations with incorrect formant gap
                int maxId = maxIndexArray[r];
                int bandBinCount = maxBin - minBin + 1;
                double freqBinGap = 2 * bandBinCount / (double)maxId;
                double formantGap = freqBinGap * freqBinWidth;
                if (formantGap < minFormantGap || formantGap > maxFormantGap)
                {
                    harmonicIntensityScores[r] = 0.0;
                }
            }

            // smooth the harmonicIntensityScores array to allow for brief gaps.
            harmonicIntensityScores = DataTools.filterMovingAverageOdd(harmonicIntensityScores, 3);

            //extract the events based on length and threshhold.
            // Note: This method does NOT do prior smoothing of the score array.
            var harmonicEvents = AcousticEvent.ConvertScoreArray2Events(
                    harmonicIntensityScores,
                    minHz,
                    maxHz,
                    spectrogram.FramesPerSecond,
                    spectrogram.FBinWidth,
                    dctThreshold,
                    minDuration,
                    maxDuration,
                    segmentStartOffset);

            var spectralEvents = new List<EventCommon>();

            // add in temporary names to the events. These can be altered later.
            foreach (var he in harmonicEvents)
            {
                var se = EventConverters.ConvertAcousticEventToSpectralEvent(he);
                spectralEvents.Add(se);
                se.Name = "Harmonics";
            }

            return (spectralEvents, dBArray, harmonicIntensityScores);
        }
    }
}