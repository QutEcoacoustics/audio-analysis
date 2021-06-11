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
        /// Gets or sets the bottom bound of the gap between formants. Units are Hertz.
        /// </summary>
        public int? MinFormantGap { get; set; }

        /// <summary>
        /// Gets or sets the the top bound of gap between formants. Units are Hertz.
        /// </summary>
        public int? MaxFormantGap { get; set; }

        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            HarmonicParameters hp,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var spectralEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            double[] decibelMaxArray;
            double[] harmonicIntensityScores;
            (spectralEvents, decibelMaxArray, harmonicIntensityScores) = HarmonicParameters.GetComponentsWithHarmonics(
                                spectrogram,
                                hp.MinHertz.Value,
                                hp.MaxHertz.Value,
                                decibelThreshold.Value,
                                hp.DctThreshold.Value,
                                hp.MinDuration.Value,
                                hp.MaxDuration.Value,
                                hp.MinFormantGap.Value,
                                hp.MaxFormantGap.Value,
                                segmentStartOffset);

            // prepare plot of resultant Harmonics decibel array.
            var plot = Plot.PreparePlot(decibelMaxArray, $"{profileName} (Harmonics:{decibelThreshold:F0}db)", decibelThreshold.Value);
            plots.Add(plot);

            return (spectralEvents, plots);
        }

        public static (List<EventCommon> SpectralEvents, double[] AmplitudeArray, double[] HarmonicIntensityScores) GetComponentsWithHarmonics(
            SpectrogramStandard spectrogram,
            int minHz,
            int maxHz,
            double decibelThreshold,
            double dctThreshold,
            double minDuration,
            double maxDuration,
            int minFormantGap,
            int maxFormantGap,
            TimeSpan segmentStartOffset)
        {
            int nyquist = spectrogram.NyquistFrequency;
            var sonogramData = spectrogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            // get the min and max bin of the freq-band of interest.
            double freqBinWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / freqBinWidth);
            int maxBin = (int)Math.Round(maxHz / freqBinWidth);
            int bandBinCount = maxBin - minBin + 1;

            // extract the sub-band of interest
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
                    //ignore frames where DCT coefficient (proxy for formant intensity) is below threshold
                    continue;
                }

                //ignore frames with incorrect formant gap
                // first get id of the maximum coefficient.
                int maxId = maxIndexArray[r];
                double freqBinGap = 2 * bandBinCount / (double)maxId;
                double formantGap = freqBinGap * freqBinWidth;

                // remove values where formantGap lies outside the expected range.
                if (formantGap < minFormantGap || formantGap > maxFormantGap)
                {
                    harmonicIntensityScores[r] = 0.0;
                }
            }

            // fill in brief gaps of one or two frames.
            var harmonicIntensityScores2 = new double[harmonicIntensityScores.Length];
            for (int r = 1; r < frameCount - 2; r++)
            {
                harmonicIntensityScores2[r] = harmonicIntensityScores[r];
                if (harmonicIntensityScores[r - 1] > dctThreshold && harmonicIntensityScores[r] < dctThreshold)
                {
                    // we have arrived at a possible gap. Fill the gap.
                    harmonicIntensityScores2[r] = harmonicIntensityScores[r - 1];
                }

                //now check if the gap is two frames wide
                if (harmonicIntensityScores[r + 1] < dctThreshold && harmonicIntensityScores[r + 2] > dctThreshold)
                {
                    harmonicIntensityScores2[r + 1] = harmonicIntensityScores[r + 2];
                    r += 1;
                }
            }

                //extract the events based on length and threshhold.
                // Note: This method does NOT do prior smoothing of the score array.
                var harmonicEvents = AcousticEvent.ConvertScoreArray2Events(
                    harmonicIntensityScores2,
                    minHz,
                    maxHz,
                    spectrogram.FramesPerSecond,
                    spectrogram.FBinWidth,
                    dctThreshold,
                    minDuration,
                    maxDuration,
                    segmentStartOffset);

            //var spectralEvents = new List<HarmonicEvent>();
            var spectralEvents = new List<EventCommon>();

            // add in temporary names to the events. These can be altered later.
            foreach (var he in harmonicEvents)
            {
                var se = EventConverters.ConvertAcousticEventToSpectralEvent(he);
                spectralEvents.Add(se);
                se.Name = "Harmonics";
            }

            return (spectralEvents, dBArray, harmonicIntensityScores2);
        }
    }
}