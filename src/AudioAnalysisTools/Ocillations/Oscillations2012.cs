// <copyright file="Oscillations2012.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Metadata.Ecma335;
    using AnalysisPrograms.Recognizers.Base;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Events;
    using AudioAnalysisTools.StandardSpectrograms;
    using SixLabors.ImageSharp;
    using TowseyLibrary;

    /// <summary>
    /// NOTE: 21st June 2012.
    ///
    /// This class contains methods to detect oscillations in a the sonogram of an audio signal.
    /// The method Execute() returns all info about oscillations in the passed sonogram.
    /// This method should be called in preference to those in the class OscillationAnalysis.
    /// (The latter should be deprecated.)
    /// </summary>
    public static class Oscillations2012
    {
        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots, double[,] Hits) GetComponentsWithOscillations(
            SpectrogramStandard spectrogram,
            OscillationParameters op,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var oscEvents = new List<EventCommon>();

            Oscillations2012.Execute(
                spectrogram,
                op.MinDuration.Value,
                op.MaxDuration.Value,
                op.MinHertz.Value,
                op.MaxHertz.Value,
                op.MinOscillationFrequency,
                op.MaxOscillationFrequency,
                op.DctDuration,
                op.DctThreshold,
                op.EventThreshold,
                out var bandDecibels,
                out var oscScores,
                out var oscillationEvents,
                out var hits, // return this for debugging purposes.
                segmentStartOffset);

            oscEvents.AddRange(oscillationEvents);

            // prepare plot of resultant decibel and score arrays.
            var plots = new List<Plot>();
            var plot1 = Plot.PreparePlot(bandDecibels, $"{profileName} (Oscillations:{decibelThreshold:F0}db)", decibelThreshold.Value);
            plots.Add(plot1);
            var plot2 = Plot.PreparePlot(oscScores, $"{profileName} (Oscillation Event Score:{op.EventThreshold:F2})", op.EventThreshold);
            plots.Add(plot2);

            return (oscEvents, plots, hits);
        }

        public static void Execute(
            SpectrogramStandard sonogram,
            double minDuration,
            double maxDuration,
            int minHz,
            int maxHz,
            double? minOscillationFrequency,
            double? maxOscillationFrequency,
            double dctDuration,
            double dctThreshold,
            double eventThreshold,
            out double[] bandDecibels,
            out double[] oscScores,
            out List<OscillationEvent> events,
            out double[,] hits,
            TimeSpan segmentStartOffset)
        {
            int scoreSmoothingWindow = 11; // sets a default that is good for Cane toad but not necessarily for other recognizers

            Execute(
                sonogram,
                minDuration,
                maxDuration,
                minHz,
                maxHz,
                minOscillationFrequency,
                maxOscillationFrequency,
                dctDuration,
                dctThreshold,
                eventThreshold,
                scoreSmoothingWindow,
                out bandDecibels,
                out oscScores,
                out events,
                out hits,
                segmentStartOffset);
        }

        public static void Execute(
            SpectrogramStandard spectrogram,
            double minDuration,
            double maxDuration,
            int minHz,
            int maxHz,
            double? minOscilFrequency,
            double? maxOscilFrequency,
            double dctDuration,
            double dctThreshold,
            double eventThreshold,
            int smoothingWindow,
            out double[] decibelArray,
            out double[] oscScores,
            out List<OscillationEvent> events,
            out double[,] hits,
            TimeSpan segmentStartOffset)
        {
            // smooth the spectra in all time-frames.
            spectrogram.Data = MatrixTools.SmoothRows(spectrogram.Data, 3);

            // extract array of decibel values, frame averaged over required frequency band
            decibelArray = SNR.CalculateFreqBandAvIntensity(spectrogram.Data, minHz, maxHz, spectrogram.NyquistFrequency);

            //DETECT OSCILLATIONS in the search band.
            hits = DetectOscillations(spectrogram, minHz, maxHz, dctDuration, minOscilFrequency.Value, maxOscilFrequency.Value, dctThreshold);
            if (hits == null)
            {
                oscScores = null;
                events = null;
                return;
            }

            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            oscScores = GetOscillationScores(hits, minHz, maxHz, spectrogram.FBinWidth);

            // smooth the scores - window=11 has been the DEFAULT. Now letting user set this.
            oscScores = DataTools.filterMovingAverage(oscScores, smoothingWindow);
            events = OscillationEvent.ConvertOscillationScores2Events(
                spectrogram,
                minDuration,
                maxDuration,
                minHz,
                maxHz,
                minOscilFrequency,
                maxOscilFrequency,
                oscScores,
                eventThreshold,
                segmentStartOffset);
        }

        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) DCTLength: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coefficient returned by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// </summary>
        /// <param name="sonogram">A spectrogram.</param>
        /// <param name="minHz">min freq bin of search band.</param>
        /// <param name="maxHz">max freq bin of search band.</param>
        /// <param name="dctDuration">number of values.</param>
        /// <param name="minOscilFreq">minimum oscillation freq.</param>
        /// <param name="maxOscilFreq">maximum oscillation freq.</param>
        /// <param name="dctThreshold">threshold - do not accept a DCT coefficient if its value is less than this threshold.</param>
        public static double[,] DetectOscillations(SpectrogramStandard sonogram, int minHz, int maxHz, double dctDuration, double minOscilFreq, double maxOscilFreq, double dctThreshold)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            double midOscilFreq = minOscilFreq + ((maxOscilFreq - minOscilFreq) / 2);

            //safety check
            //if (maxIndex > dctLength)
            //{
            //    LoggedConsole.WriteLine("###### WARNING: The DCT length is too short to detect the maxOscillationFrequency");
            //    return null;
            //}

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            double[,] hits = new double[rows, cols];
            double[,] matrix = sonogram.Data;

            double[,] cosines = DctMethods.Cosines(dctLength, dctLength); //set up the cosine coefficients

            //traverse columns - skip DC column
            for (int c = minBin; c <= maxBin; c++)
            {
                var dctArray = new double[dctLength];

                for (int r = 1; r < rows - dctLength; r++)
                {
                    // only stop if current location is a peak
                    if (matrix[r, c] < matrix[r - 1, c] || matrix[r, c] < matrix[r + 1, c])
                    {
                        continue;
                    }

                    // ... AND if current peak is above a decibel threhsold.
                    if (matrix[r, c] < 3.0)
                    {
                        continue;
                    }

                    // extract array and ready for DCT
                    for (int i = 0; i < dctLength; i++)
                    {
                        dctArray[i] = matrix[r + i, c];
                    }

                    int lowerDctBound = minIndex / 4;
                    var dctCoeff = DctMethods.DoDct(dctArray, cosines, lowerDctBound);
                    int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);

                    // mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if (indexOfMaxValue >= minIndex && indexOfMaxValue <= maxIndex && dctCoeff[indexOfMaxValue] > dctThreshold)
                    {
                        for (int i = 0; i < dctLength; i++)
                        {
                            hits[r + i, c] = midOscilFreq;
                        }
                    }

                    // skip rows i.e. do every sixth time frame.
                    //r += 5;
                }

                // do alternate columns i.e. every second frequency bin.
                c++;
            }

            return hits;
        }

        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix">the Oscillation matrix.</param>
        /// <returns>a matrix.</returns>
        public static double[,] RemoveIsolatedOscillations(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] cleanMatrix = matrix;
            const double tolerance = double.Epsilon;

            //traverse columns - skip DC column
            for (int c = 3; c < cols - 3; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (Math.Abs(cleanMatrix[r, c]) < tolerance)
                    {
                        continue;
                    }

                    //+2 because alternate columns
                    if (Math.Abs(matrix[r, c - 2]) < tolerance && Math.Abs(matrix[r, c + 2]) < tolerance)
                    {
                        cleanMatrix[r, c] = 0.0;
                    }
                }
            }

            return cleanMatrix;
        }

        /// <summary>
        /// Converts the hits derived from the oscillation detector into a score for each frame.
        /// NOTE: The oscillation detector skips every second row, so score must be adjusted for this.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of oscillation hits.</param>
        /// <param name="minHz">lower freq bound of the acoustic event.</param>
        /// <param name="maxHz">upper freq bound of the acoustic event.</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class.</param>
        public static double[] GetOscillationScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;

            //set hit range slightly < half the bins. Half because only scan every second bin.
            double hitRange = binCount * 0.4;
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                //traverse columns in required band
                int hitCount = 0;
                for (int c = minBin; c <= maxBin; c++)
                {
                    if (hits[r, c] > 0)
                    {
                        hitCount++;
                    }
                }

                //Normalize the Matrix Values the hit score in [0,1]
                scores[r] = hitCount / hitRange;
                if (scores[r] > 1.0)
                {
                    scores[r] = 1.0;
                }
            }

            return scores;
        }

        /// <summary>
        /// Calculates the optimal frame overlap for the given sample rate, frame width and max oscillation or pulse rate.
        /// Pulse rate is determined using a DCT and efficient use of the DCT requires that the dominant pulse sit somewhere 3.4 along the array of coefficients.
        /// </summary>
        public static double CalculateRequiredFrameOverlap(int sr, int frameWidth, double maxOscillation)
        {
            double optimumFrameRate = 3 * maxOscillation; //so that max oscillation sits in 3/4 along the array of DCT coefficients
            int frameOffset = (int)(sr / optimumFrameRate);

            // this line added 17 Aug 2016 to deal with high Oscillation rate frog ribits.
            if (frameOffset > frameWidth)
            {
                frameOffset = frameWidth;
            }

            double overlap = (frameWidth - frameOffset) / (double)frameWidth;
            return overlap;
        }
    }
}