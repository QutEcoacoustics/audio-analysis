namespace AudioAnalysisTools
{
    using System;
    using System.Collections.Generic;
    using DSP;
    using StandardSpectrograms;
    using TowseyLibrary;

    /// <summary>
    /// NOTE: 21st June 2012.
    ///
    /// This class contains methods to detect oscillations in a the sonogram of an audio signal.
    /// The method Execute() returns all info about oscillaitons in the passed sonogram.
    /// This method should be called in preference to those in the class OscillationAnalysis.
    /// (The latter should be depracated.)
    /// </summary>
    public static class Oscillations2012
    {
        public static void Execute(SpectrogramStandard sonogram, int minHz, int maxHz,
                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double dctThreshold, double scoreThreshold,
                                   double minDuration, double maxDuration, int smoothingWindow,
                                   out double[] scores, out List<AcousticEvent> events, out double[,] hits)
        {
            // smooth the frames to make oscillations more regular.
            sonogram.Data = MatrixTools.SmoothRows(sonogram.Data, 5);

            //DETECT OSCILLATIONS
            hits = DetectOscillations(sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, dctThreshold);

            // debug
            ////var sum = hits.Fold((x, y) => x + y, 0.0);

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
            double[] oscFreq = GetOscillationFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            events = ConvertOscillationScores2Events(
                scores,
                oscFreq,
                minHz,
                maxHz,
                sonogram.FramesPerSecond,
                sonogram.FBinWidth,
                scoreThreshold,
                minDuration,
                maxDuration,
                sonogram.Configuration.SourceFName,
                TimeSpan.Zero);
        }

        public static void Execute(SpectrogramStandard sonogram, int minHz, int maxHz,
                           double dctDuration, int minOscilFreq, int maxOscilFreq, double dctThreshold, double scoreThreshold,
                           double minDuration, double maxDuration,
                           out double[] scores, out List<AcousticEvent> events, out double[,] hits)
        {
            int scoreSmoothingWindow = 11; // sets a default that is good for Canetoad but not necessarily for other recognisers

            Execute(sonogram, minHz, maxHz, dctDuration, minOscilFreq, maxOscilFreq, dctThreshold, scoreThreshold,
                                               minDuration, maxDuration, scoreSmoothingWindow,
                                               out scores, out events, out hits);
        }

        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) DCTLength: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coeff returned by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="minHz">min freq bin of search band</param>
        /// <param name="maxHz">max freq bin of search band</param>
        /// <param name="dctDuration">number of values</param>
        /// <param name="maxOscilFreq"></param>
        /// <param name="dctThreshold">threshold - do not accept a DCT coefficient if its value is less than this threshold</param>
        /// <param name="minOscilFreq"></param>
        /// <returns></returns>
        public static double[,] DetectOscillations(SpectrogramStandard sonogram, int minHz, int maxHz,
                                                   double dctDuration, int minOscilFreq, int maxOscilFreq, double dctThreshold)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            int midOscilFreq = minOscilFreq + ((maxOscilFreq - minOscilFreq) / 2);

            if (maxIndex > dctLength) return null;       //safety check

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            double[,] hits = new double[rows, cols];
            double[,] matrix = sonogram.Data;

            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string txtPath = @"C:\SensorNetworks\Output\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, txtPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);

            for (int c = minBin; c <= maxBin; c++)//traverse columns - skip DC column
            {
                var dctArray = new double[dctLength];

                for (int r = 0; r < rows - dctLength; r++)
                {
                    // extract array and ready for DCT
                    for (int i = 0; i < dctLength; i++)
                        dctArray[i] = matrix[r + i, c];
                    dctArray = DataTools.SubtractMean(dctArray);
                    //dctArray = DataTools.Vector2Zscores(dctArray);

                    double[] dctCoeff = MFCCStuff.DCT(dctArray, cosines);
                    // convert to absolute values because not interested in negative values due to phase.
                    for (int i = 0; i < dctLength; i++)
                        dctCoeff[i] = Math.Abs(dctCoeff[i]);
                    // remove low freq oscillations from consideration
                    int thresholdIndex = minIndex / 4;
                    for (int i = 0; i < thresholdIndex; i++)
                        dctCoeff[i] = 0.0;

                    dctCoeff = DataTools.normalise2UnitLength(dctCoeff);
                    //dct = DataTools.NormaliseMatrixValues(dct); //another option to NormaliseMatrixValues
                    int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);
                    //double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                    // #### Tried this option for scoring oscillation hits but did not work well.
                    // #### Requires very fine tuning of thresholds
                    //dctCoeff = DataTools.Normalise2Probabilites(dctCoeff);
                    //// sum area under curve where looking for oscillations
                    //double sum = 0.0;
                    //for (int i = minIndex; i <= maxIndex; i++)
                    //    sum += dctCoeff[i];
                    //if (sum > dctThreshold)
                    //{
                    //    for (int i = 0; i < dctLength; i++) hits[r + i, c] = midOscilFreq;
                    //}

                    // DEBUGGING
                    // DataTools.MinMax(dctCoeff, out min, out max);
                     //DataTools.writeBarGraph(dctArray);
                     //DataTools.writeBarGraph(dctCoeff);

                    //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                    if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dctCoeff[indexOfMaxValue] > dctThreshold))
                    {
                        for (int i = 0; i < dctLength; i++) hits[r + i, c] = midOscilFreq;
                    }
                    r += 5; //skip rows
                }
                c++; //do alternate columns
            }
            return hits;
        }

        public static double[] DetectOscillations(double[] ipArray, double framesPerSecond, double dctDuration, double minOscilFreq, double maxOscilFreq, double dctThreshold)
        {
            int dctLength = (int)Math.Round(framesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi

            //double midOscilFreq = minOscilFreq + ((maxOscilFreq - minOscilFreq) / 2);

            if (maxIndex > dctLength) return null;       //safety check

            int length = ipArray.Length;
            var dctScores = new double[length];
            //var hits = new double[length];

            double[,] cosines = MFCCStuff.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write bmp image of cosine matrix values for checking.
            //string bmpPath = @"C:\SensorNetworks\Output\cosines.png";
            //ImageTools.DrawMatrix(cosines, bmpPath, true);

            for (int r = 1; r < length - dctLength; r ++)
            {
                // only stop if current location is a peak
                if ((ipArray[r] < ipArray[r - 1]) || (ipArray[r] < ipArray[r + 1]))
                {
                    continue;
                }

                // extract array and ready for DCT
                //for (int i = 0; i < dctLength; i++) dctArray[i] = ipArray[r + i];
                var dctArray = DataTools.Subarray(ipArray, r, dctLength);

                dctArray = DataTools.SubtractMean(dctArray);
                //dctArray = DataTools.Vector2Zscores(dctArray);

                double[] dctCoeff = MFCCStuff.DCT(dctArray, cosines);
                // convert to absolute values because not interested in negative values due to phase.
                for (int i = 0; i < dctLength; i++)
                    dctCoeff[i] = Math.Abs(dctCoeff[i]);
                // remove low freq oscillations from consideration
                int thresholdIndex = minIndex / 4;
                for (int i = 0; i < thresholdIndex; i++)
                    dctCoeff[i] = 0.0;

                dctCoeff = DataTools.normalise2UnitLength(dctCoeff);
                //dct = DataTools.NormaliseMatrixValues(dct); //another option to NormaliseMatrixValues
                int indexOfMaxValue = DataTools.GetMaxIndex(dctCoeff);
                //double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi

                // #### Tried this option for scoring oscillation hits but did not work well.
                // #### Requires very fine tuning of thresholds
                //dctCoeff = DataTools.Normalise2Probabilites(dctCoeff);
                //// sum area under curve where looking for oscillations
                //double sum = 0.0;
                //for (int i = minIndex; i <= maxIndex; i++)
                //    sum += dctCoeff[i];
                //if (sum > dctThreshold)
                //{
                //    for (int i = 0; i < dctLength; i++) hits[r + i, c] = midOscilFreq;
                //}

                // DEBUGGING
                // DataTools.MinMax(dctCoeff, out min, out max);
                //DataTools.writeBarGraph(dctArray);
                //DataTools.writeBarGraph(dctCoeff);

                //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dctCoeff[indexOfMaxValue] > dctThreshold))
                {
                    //for (int i = 0; i < dctLength; i++) dctScores[r + i] = midOscilFreq;
                    for (int i = 0; i < dctLength; i++)
                    {
                        if (dctScores[r + i] < dctCoeff[indexOfMaxValue]) dctScores[r + i] = dctCoeff[indexOfMaxValue];
                    }
                }
            }
            //return hits; //dctArray
            return dctScores;
        }

        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix">the Oscillation matrix</param>
        /// <returns></returns>
        public static double[,] RemoveIsolatedOscillations(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] cleanMatrix = matrix;
            const double tolerance = double.Epsilon;
            for (int c = 3; c < cols - 3; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (Math.Abs(cleanMatrix[r, c]) < tolerance) continue;
                    if ((Math.Abs(matrix[r, c - 2]) < tolerance) && (Math.Abs(matrix[r, c + 2]) < tolerance))  //+2 because alternate columns
                        cleanMatrix[r, c] = 0.0;
                }
            }
            return cleanMatrix;
        } //end method RemoveIsolatedOscillations()

        /// <summary>
        /// Converts the hits derived from the oscilation detector into a score for each frame.
        /// NOTE: The oscillation detector skips every second row, so score must be adjusted for this.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of oscillation hits</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <returns></returns>
        public static double[] GetOscillationScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            double hitRange = binCount * 0.5 * 0.8; //set hit range slightly < half the bins. Half because only scan every second bin.
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0) score++;
                }
                scores[r] = score / hitRange; //NormaliseMatrixValues the hit score in [0,1]
                if (scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }//end method GetODScores()

        public static double[] GetOscillationFrequency(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;

            var oscFreq = new double[rows]; //to store the oscillation frequency
            for (int r = 0; r < rows; r++)
            {
                double freq = 0;
                int count = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required band
                {
                    if (hits[r, c] > 0)
                    {
                        freq += hits[r, c];
                        count ++;
                    }
                }
                if (count == 0) oscFreq[r] = 0;
                else            oscFreq[r] = freq / count; //return the average frequency
                //if (oscFreq[r] > 1.0) oscFreq[r] = 1.0;
            }
            return oscFreq;
        }//end method GetODFrequency()

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents.
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="oscFreq"></param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="maxScoreThreshold"></param>
        /// <param name="minDurationThreshold"></param>
        /// <param name="maxDurationThreshold"></param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <param name="segmentStartOffset"></param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertOscillationScores2Events(
            double[] scores,
            double[] oscFreq,
            int minHz,
            int maxHz,
            double framesPerSec,
            double freqBinWidth,
            double maxScoreThreshold,
            double minDurationThreshold,
            double maxDurationThreshold,
            string fileName,
            TimeSpan segmentStartOffset)
        {
            //double minThreshold = 0.1;
            //double scoreThreshold = minThreshold; //set this to the minimum threshold to start with
            double scoreThreshold = maxScoreThreshold;   //set this to the maximum threshold to start with
            int count = scores.Length;
            //int minBin = (int)(minHz / freqBinWidth);
            //int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;
            var events = new List<AcousticEvent>();
            bool isHit = false;
            double frameOffset = 1 / framesPerSec;
            double startTime = 0.0;
            int startFrame = 0;

            for (int i = 0; i < count; i++)//pass over all frames
            {
                if ((isHit == false) && (scores[i] >= scoreThreshold))//start of an event
                {
                    isHit = true;
                    startTime = i * frameOffset;
                    startFrame = i;
                }
                else  //check for the end of an event
                    if (isHit && ((scores[i] < scoreThreshold)||(i == count-1)))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        //double endTime = i * frameOffset;
                        //double duration = endTime - startTime;
                        double duration = (i - startFrame + 1)  * frameOffset;
                        if (duration < minDurationThreshold) continue; //skip events with duration shorter than threshold
                        if (duration > maxDurationThreshold) continue; //skip events with duration longer than threshold
                        var ev = new AcousticEvent(segmentStartOffset, startTime, duration, minHz, maxHz);
                        ev.Name = "Oscillation"; //default name
                        //ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.FileName = fileName;
                        //obtain average score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        ev.Score = av / (i - startFrame + 1);
                        //obtain oscillation freq.
                        av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += oscFreq[n];
                        ev.Score2 = av / (i - startFrame + 1);
                        ev.Intensity = (int)ev.Score2; // store this info for later inclusion in csv file as Event Intensity
                        events.Add(ev);
                    }

                //adapt the threshold
                //if ((scores[i] >= maxThreshold) && (maxThreshold >= scoreThreshold)) scoreThreshold *= 1.01;
                //else
                //if ((scores[i] <= minThreshold) && (minThreshold <= scoreThreshold)) scoreThreshold *= 0.95;

            } //end of pass over all frames
            return events;
        }//end method ConvertODScores2Events()

        /// <summary>
        /// Calculates the optimal frame overlap for the given sample rate, frame width and max oscilation or pulse rate.
        /// Pulse rate is determined using a DCT and efficient use of the DCT requires that the dominant pulse sit somewhere 3.4 along the array of coefficients.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="frameWidth"></param>
        /// <param name="maxOscilation"></param>
        /// <returns></returns>
        public static double CalculateRequiredFrameOverlap(int sr, int frameWidth, double maxOscilation)
        {
            double optimumFrameRate = 3 * maxOscilation; //so that max oscillation sits in 3/4 along the array of DCT coefficients
            int frameOffset = (int)(sr / optimumFrameRate);

            // this line added 17 Aug 2016 to deal with high Oscillation rate frog ribits.
            if (frameOffset > frameWidth) frameOffset = frameWidth;

            double overlap = (frameWidth - frameOffset) / (double)frameWidth;
            return overlap;
        }

    }//end class
} //AudioAnalysisTools
