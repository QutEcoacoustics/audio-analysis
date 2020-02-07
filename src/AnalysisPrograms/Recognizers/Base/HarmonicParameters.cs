// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HarmonicParameters.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>

namespace AnalysisPrograms.Recognizers.Base
{
    using System;
    using System.Collections.Generic;
    using Acoustics.Shared;
    using AudioAnalysisTools;
    using AudioAnalysisTools.StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// TODO TODO: THIS METHOD IS WORK IN PROGESS AND CURRENTLY DOES YIELD A SUCCESSFUL RESULT. To BE FURTHER WORKED ON!!.
    /// Parameters needed from a config file to detect the stacked harmonic components of a soundscape.
    /// This can also be used for recognizing the harmonics of non-biological sounds such as from turbines, motor-bikes, compressors and other hi-revving motors.
    /// </summary>
    [YamlTypeTag(typeof(HarmonicParameters))]
    public class HarmonicParameters : CommonParameters
    {
        public static (List<AcousticEvent>, double[]) GetComponentsWithHarmonics(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            int nyquist,
            double decibelThreshold,
            double minDuration,
            double maxDuration,
            TimeSpan segmentStartOffset)
        {
            // paramters to be passed
            double harmonicIntensityThreshold = 0.15;
            int minFormantGap = 180;
            int maxFormantGap = 450;

            // Event threshold - Determines FP / FN trade-off for events.
            //double eventThreshold = 0.2;

            var sonogramData = sonogram.Data;
            int frameCount = sonogramData.GetLength(0);
            int binCount = sonogramData.GetLength(1);

            double freqBinWidth = nyquist / (double)binCount;
            int minBin = (int)Math.Round(minHz / freqBinWidth);
            int maxBin = (int)Math.Round(maxHz / freqBinWidth);

            // set up score arrays
            var harmonicScores = new double[frameCount];
            var formantGaps = new double[frameCount];

            // now look for harmonics in search band using the Xcorrelation-FFT technique.
            int bandWidthBins = maxBin - minBin + 1;

            //the Xcorrelation-FFT technique requires number of bins to scan to be power of 2.
            //assuming sr=22050 and window= 512, then freq bin width = 43.0664 and  64 fft bins span 2756 Hz above the min Hz level.
            //assuming sr=22050 and window= 512, then freq bin width = 43.0664 and 128 fft bins span 5513 Hz above the min Hz level.
            //assuming sr=22050 and window=1024, then freq bin width = 21.5332 and  64 fft bins span 1378 Hz above the min Hz level.
            //assuming sr=22050 and window=1024, then freq bin width = 21.5332 and 128 fft bins span 2756 Hz above the min Hz level.
            //assuming sr=22050 and window=1024, then freq bin width = 21.5332 and 256 fft bins span 5513 Hz above the min Hz level.

            int numberOfFftBins = 2048;
            while (numberOfFftBins > bandWidthBins && (minBin + numberOfFftBins) > binCount)
            {
                numberOfFftBins /= 2;
            }

            //numberOfFftBins = Math.Min(256, numberOfFftBins);
            maxBin = minBin + numberOfFftBins - 1;
            int maxFftHertz = (int)Math.Ceiling(maxBin * sonogram.FBinWidth);

            double[,] subMatrix = MatrixTools.Submatrix(sonogram.Data, 0, minBin, frameCount - 1, maxBin);

            int minCallSpan = (int)Math.Round(minDuration * sonogram.FramesPerSecond);

            //ii: DETECT HARMONICS
            var results = CrossCorrelation.DetectHarmonicsInSonogramMatrix(subMatrix, decibelThreshold, minCallSpan);

            double[] dBArray = results.Item1;
            double[] intensity = results.Item2;     //an array of periodicity scores
            double[] periodicity = results.Item3;

            //intensity = DataTools.filterMovingAverage(intensity, 3);
            int noiseBound = (int)(100 / freqBinWidth); //ignore 0-100 hz - too much noise
            double[] scoreArray = new double[intensity.Length];
            for (int r = 0; r < frameCount; r++)
            {
                if (intensity[r] < harmonicIntensityThreshold)
                {
                    continue;
                }

                //ignore locations with incorrect formant gap
                double herzPeriod = periodicity[r] * freqBinWidth;
                if (herzPeriod < minFormantGap || herzPeriod > maxFormantGap)
                {
                    continue;
                }

                //find freq having max power and use info to adjust score.
                //expect humans to have max < 1000 Hz
                double[] spectrum = MatrixTools.GetRow(sonogram.Data, r);
                for (int j = 0; j < noiseBound; j++)
                {
                    spectrum[j] = 0.0;
                }

                int maxIndex = DataTools.GetMaxIndex(spectrum);
                int freqWithMaxPower = (int)Math.Round(maxIndex * freqBinWidth);
                double discount = 1.0;
                if (freqWithMaxPower < 1200)
                {
                    discount = 0.0;
                }

                if (intensity[r] > harmonicIntensityThreshold)
                {
                    scoreArray[r] = intensity[r] * discount;
                }
            }

            // smooth the decibel array to allow for brief gaps.
            harmonicScores = DataTools.filterMovingAverageOdd(harmonicScores, 5);

            //extract the events based on length and threshhold.
            // Note: This method does NOT do prior smoothing of the score array.
            var acousticEvents = AcousticEvent.ConvertScoreArray2Events(
                    harmonicScores,
                    minHz,
                    maxFftHertz,
                    sonogram.FramesPerSecond,
                    sonogram.FBinWidth,
                    decibelThreshold,
                    minDuration,
                    maxDuration,
                    segmentStartOffset);

            return (acousticEvents, harmonicScores);
        }
    }
}
