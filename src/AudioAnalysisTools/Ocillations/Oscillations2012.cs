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
        public static (List<EventCommon> SpectralEvents, List<Plot> DecibelPlots) GetComponentsWithOscillations(
            SpectrogramStandard spectrogram,
            OscillationParameters op,
            double? decibelThreshold,
            TimeSpan segmentStartOffset,
            string profileName)
        {
            var oscEvents = new List<EventCommon>();
            var plots = new List<Plot>();

            Oscillations2012.Execute(
                spectrogram,
                op.MinHertz.Value,
                op.MaxHertz.Value,
                op.DctDuration,
                op.MinOscillationFrequency,
                op.MaxOscillationFrequency,
                op.DctThreshold,
                op.EventThreshold,
                op.MinDuration.Value,
                op.MaxDuration.Value,
                out var scores,
                out var oscillationEvents,
                out var hits,
                segmentStartOffset);

            oscEvents.AddRange(oscillationEvents);

            // prepare plot of resultant Harmonics decibel array.
            var plot = Plot.PreparePlot(scores, $"{profileName} (Oscillations:{decibelThreshold:F0}db)", decibelThreshold.Value);
            plots.Add(plot);

            return (oscEvents, plots);
        }

        public static void Execute(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double dctDuration,
            int minOscilFreq,
            int maxOscilFreq,
            double dctThreshold,
            double scoreThreshold,
            double minDuration,
            double maxDuration,
            out double[] scores,
            out List<OscillationEvent> events,
            out double[,] hits,
            TimeSpan segmentStartOffset)
        {
            int scoreSmoothingWindow = 11; // sets a default that is good for Cane toad but not necessarily for other recognizers

            Execute(
                sonogram,
                minHz,
                maxHz,
                dctDuration,
                minOscilFreq,
                maxOscilFreq,
                dctThreshold,
                scoreThreshold,
                minDuration,
                maxDuration,
                scoreSmoothingWindow,
                out scores,
                out events,
                out hits,
                segmentStartOffset);
        }

        public static void Execute(
            SpectrogramStandard sonogram,
            int minHz,
            int maxHz,
            double dctDuration,
            int minOscilFreq,
            int maxOscilFreq,
            double dctThreshold,
            double scoreThreshold,
            double minDuration,
            double maxDuration,
            int smoothingWindow,
            out double[] scores,
            out List<OscillationEvent> events,
            out double[,] hits,
            TimeSpan segmentStartOffset)
        {
            // smooth the frames to make oscillations more regular.
            sonogram.Data = MatrixTools.SmoothRows(sonogram.Data, 5);

            //DETECT OSCILLATIONS
            hits = DetectOscillations(sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, dctThreshold);
            if (hits == null)
            {
                LoggedConsole.WriteLine("###### WARNING: DCT length too short to detect the maxOscilFreq");
                scores = null;
                events = null;
                return;
            }

            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            scores = GetOscillationScores(hits, minHz, maxHz, sonogram.FBinWidth);

            // smooth the scores - window=11 has been the DEFAULT. Now letting user set this.
            scores = DataTools.filterMovingAverage(scores, smoothingWindow);
            events = ConvertOscillationScores2Events(
                sonogram,
                scores,
                minHz,
                maxHz,
                scoreThreshold,
                minDuration,
                maxDuration,
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
        public static double[,] DetectOscillations(SpectrogramStandard sonogram, int minHz, int maxHz, double dctDuration, int minOscilFreq, int maxOscilFreq, double dctThreshold)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            int midOscilFreq = minOscilFreq + ((maxOscilFreq - minOscilFreq) / 2);

            //safety check
            if (maxIndex > dctLength)
            {
                return null;
            }

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            double[,] hits = new double[rows, cols];
            double[,] matrix = sonogram.Data;

            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); //set up the cosine coefficients

            //traverse columns - skip DC column
            for (int c = minBin; c <= maxBin; c++)
            {
                var dctArray = new double[dctLength];

                for (int r = 0; r < rows - dctLength; r++)
                {
                    // extract array and ready for DCT
                    for (int i = 0; i < dctLength; i++)
                    {
                        dctArray[i] = matrix[r + i, c];
                    }

                    int lowerDctBound = minIndex / 4;
                    var dctCoeff = DoDct(dctArray, cosines, lowerDctBound);
                    int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);

                    //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if (indexOfMaxValue >= minIndex && indexOfMaxValue <= maxIndex && dctCoeff[indexOfMaxValue] > dctThreshold)
                    {
                        for (int i = 0; i < dctLength; i++)
                        {
                            hits[r + i, c] = midOscilFreq;
                        }
                    }

                    r += 5; //skip rows
                }

                c++; //do alternate columns
            }

            return hits;
        }

        public static double[] DoDct(double[] vector, double[,] cosines, int lowerDctBound)
        {
            //var dctArray = DataTools.Vector2Zscores(dctArray);
            var dctArray = DataTools.SubtractMean(vector);
            int dctLength = dctArray.Length;
            double[] dctCoeff = MFCCStuff.DCT(dctArray, cosines);

            // convert to absolute values because not interested in negative values due to phase.
            for (int i = 0; i < dctLength; i++)
            {
                dctCoeff[i] = Math.Abs(dctCoeff[i]);
            }

            // remove lower coefficients from consideration because they dominate
            for (int i = 0; i < lowerDctBound; i++)
            {
                dctCoeff[i] = 0.0;
            }

            dctCoeff = DataTools.normalise2UnitLength(dctCoeff);
            return dctCoeff;
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
            double hitRange = binCount * 0.5 * 0.8;
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                //traverse columns in required band
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)
                {
                    if (hits[r, c] > 0)
                    {
                        score++;
                    }
                }

                //Normalize the Matrix Values the hit score in [0,1]
                scores[r] = score / hitRange;
                if (scores[r] > 1.0)
                {
                    scores[r] = 1.0;
                }
            }

            return scores;
        }

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of Oscillation Events.
        /// </summary>
        /// <param name="scores">the array of OD scores.</param>
        /// <param name="minHz">lower freq bound of the acoustic event.</param>
        /// <param name="maxHz">upper freq bound of the acoustic event.</param>
        /// <param name="scoreThreshold">threshold.</param>
        /// <param name="minDurationThreshold">min threshold.</param>
        /// <param name="maxDurationThreshold">max threshold.</param>
        /// <param name="segmentStartOffset">time offset.</param>
        public static List<OscillationEvent> ConvertOscillationScores2Events(
            SpectrogramStandard spectrogram,
            double[] scores,
            int minHz,
            int maxHz,
            double scoreThreshold,
            double minDurationThreshold,
            double maxDurationThreshold,
            TimeSpan segmentStartOffset)
        {
            // The name of source file
            string fileName = spectrogram.Configuration.SourceFName;
            double framesPerSec = spectrogram.FramesPerSecond;
            double freqBinWidth = spectrogram.FBinWidth;
            int count = scores.Length;

            // get the bin bounds of the frequency band of interest.
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);

            var events = new List<OscillationEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            int startFrame = 0;

            //pass over all frames
            for (int i = 0; i < count; i++)
            {
                if (isHit == false && scores[i] >= scoreThreshold)
                {
                    //start of an event
                    isHit = true;
                    startFrame = i;
                }
                else //check for the end of an event
                    if (isHit && (scores[i] < scoreThreshold || i == count - 1))
                {
                    isHit = false;
                    double duration = (i - startFrame + 1) * frameOffset;
                    if (duration < minDurationThreshold)
                    {
                        //skip events with duration shorter than threshold
                        continue;
                    }

                    if (duration > maxDurationThreshold)
                    {
                        //skip events with duration longer than threshold
                        continue;
                    }

                    // This is end of an event, so initialise it
                    // First trim the event because oscillation events spill over the edges of the true event due to use of the DCT.
                    (int trueStartFrame, int trueEndFrame, double framePeriodicity) = OscillationEvent.TrimEvent(spectrogram, startFrame, minBin, i, maxBin);
                    double trueStartTime = trueStartFrame * frameOffset;
                    double trueEndTime = trueEndFrame * frameOffset;
                    int trueFrameLength = trueEndFrame - trueStartFrame + 1;

                    //obtain average score.
                    double sum = 0.0;
                    for (int n = trueStartFrame; n <= trueEndFrame; n++)
                    {
                        sum += scores[n];
                    }

                    double score = sum / trueFrameLength;

                    var ev = new OscillationEvent()
                    {
                        Name = "Oscillation",
                        SegmentStartSeconds = segmentStartOffset.TotalSeconds,
                        ResultStartSeconds = segmentStartOffset.TotalSeconds + trueStartTime,
                        EventStartSeconds = segmentStartOffset.TotalSeconds + trueStartTime,
                        EventEndSeconds = segmentStartOffset.TotalSeconds + trueEndTime,
                        LowFrequencyHertz = minHz,
                        HighFrequencyHertz = maxHz,
                        Periodicity = framePeriodicity * frameOffset,
                        Score = score,
                        FileName = fileName,
                    };

                    //##########################################################################################
                    //ev.Score2 = av / (i - startFrame + 1);
                    //ev.Intensity = (int)ev.Score2; // store this info for later inclusion in csv file as Event Intensity
                    events.Add(ev);
                }
            }

            return events;
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