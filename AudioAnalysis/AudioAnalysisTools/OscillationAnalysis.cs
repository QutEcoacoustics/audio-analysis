﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class OscillationAnalysis
    {


        /// <summary>
        /// Segments a spectrogram based on acoustic energy in freq band of interest.
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="dctDuration">duration of DCT in seconds</param>
        /// <param name="dctThreshold">minimum amplitude of DCT </param>
        /// <param name="minOscilFreq">ignore oscillation frequencies below this threshold</param>
        /// <param name="maxOscilFreq">ignore oscillation frequencies greater than this </param>
        /// <param name="scoreThreshold">used for FP/FN</param>
        /// <param name="minDuration">ignore hits whose duration is shorter than this</param>
        /// <param name="maxDuration">ignore hits whose duration is longer than this</param>
        /// <param name="scores">return an array of scores over the entire recording</param>
        /// <param name="events">return a list of acoustic events</param>
        /// <param name="hits"></param>
        public static void Execute(SpectralSonogram sonogram, int minHz, int maxHz,
                                   double dctDuration, double dctThreshold, int minOscilFreq, int maxOscilFreq, 
                                   double scoreThreshold, double minDuration, double maxDuration,
                                   out double[] scores, out List<AcousticEvent> events, out Double[,] hits, out double[] intensity,
                                   out TimeSpan totalTime)
        {
            DateTime startTime1 = DateTime.Now; 

            //DO SEGMENTATION
            int nyquist = sonogram.SampleRate / 2;
            double smoothWindow = 1 / (double)minOscilFreq; //window = max oscillation period
            Log.WriteLine(" Segmentation smoothing window = {0:f2} seconds", smoothWindow);

            //################################################### USE FOR FILTER ---- COMMENT NEXT LINES WHEN NOT FILTERING
            var tuple = SNR.SubbandIntensity_NoiseReduced(sonogram.Data, minHz, maxHz, nyquist, smoothWindow, sonogram.FramesPerSecond);
            intensity = tuple.Item1;
            double Q = tuple.Item2;
            double oneSD = tuple.Item3;
            double dBThreshold = 0.0001; // thresholdSD* oneSD; NOTE:setting threhsold=0.0 works because have subtracte BG noise.
            Log.WriteLine("Intensity array - noise removal: Q={0:f1}dB. 1SD={1:f3}dB. Threshold={2:f3}dB.", Q, oneSD, dBThreshold);
            Log.WriteLine("Start event detection");
            List<AcousticEvent> segmentEvents = AcousticEvent.ConvertIntensityArray2Events(intensity, minHz, maxHz, 
                                                                sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                dBThreshold, minDuration, maxDuration, sonogram.Configuration.SourceFName);
            //segmentEvents = null; //do this if want do not want to segment before search.
            //################################################### END OF FILTER/SEGMENTATION CODE

            TimeSpan span1 = DateTime.Now.Subtract(startTime1); 
            Log.WriteLine(" SEGMENTATION COMP TIME = " + span1.TotalMilliseconds.ToString() + "ms");            
            DateTime startTime2 = DateTime.Now; 

            //DETECT OSCILLATIONS
            hits = DetectOscillations(sonogram, minHz, maxHz, dctDuration, dctThreshold, minOscilFreq, maxOscilFreq, segmentEvents);
            hits = RemoveIsolatedOscillations(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            scores = GetODScores(hits, minHz, maxHz, sonogram.FBinWidth);
            double[] oscFreq = GetODFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            events = ConvertODScores2Events(scores, oscFreq, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, scoreThreshold,
                                            minDuration, maxDuration, sonogram.Configuration.SourceFName);
            //events = segmentEvents; //return the segment events just for testing purposes

            DateTime endTime2 = DateTime.Now;
            TimeSpan span2 = endTime2.Subtract(startTime2);
            Log.WriteLine(" TOTAL COMP TIME = " + span2.ToString()+"s");
            totalTime = endTime2.Subtract(startTime1);
        }//end method


        /// <summary>
        /// Detects oscillations in a given freq bin.
        /// there are several important parameters for tuning.
        /// a) dctDuration: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) dctThreshold: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// c) Min and Max Oscillaitons: Sets lower & upper bound for oscillations of interest.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="minHz">min freq bin of search band</param>
        /// <param name="maxHz">max freq bin of search band</param>
        /// <param name="dctDuration"></param>
        /// <param name="dctThreshold"></param>
        /// <param name="minOscilFreq"></param>
        /// <param name="maxOscilFreq"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        public static Double[,] DetectOscillations(SpectralSonogram sonogram, int minHz, int maxHz, double dctDuration, double dctThreshold, 
                                                     int minOscilFreq, int maxOscilFreq, List<AcousticEvent>events)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int dctLength = (int)Math.Round(sonogram.FramesPerSecond * dctDuration);
            int minIndex = (int)(minOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            int maxIndex = (int)(maxOscilFreq * dctDuration * 2); //multiply by 2 because index = Pi and not 2Pi
            if (maxIndex > dctLength) maxIndex = dctLength; //safety check in case of future changes to code.

            int rows = sonogram.Data.GetLength(0);
            int cols = sonogram.Data.GetLength(1);
            Double[,] hits = new Double[rows, cols];

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients
            //following two lines write matrix of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Sonograms\cosines.txt";
            //FileTools.WriteMatrix2File_Formatted(cosines, fPath, "F3");

            //following two lines write bmp image of cos values for checking.
            //string fPath = @"C:\SensorNetworks\Output\cosines.bmp";
            //ImageTools.DrawMatrix(cosines, fPath);

            //WARNING TODO:  NEED TO CHECK CASE WHERE events = NULL!

            foreach (AcousticEvent av in events)
            {
                int startRow = (int)Math.Round(av.StartTime * sonogram.FramesPerSecond);
                int endRow   = (int)Math.Round(av.EndTime   * sonogram.FramesPerSecond) - dctLength;
                if (endRow <= startRow) endRow = startRow +1;  //want minimum of one row

                for (int c = minBin; c <= maxBin; c++)//traverse columns
                {
                    for (int r = startRow; r < endRow; r++)
                    {
                        var array = new double[dctLength];
                        //accumulate J columns of values
                        int N = 5; //average five rows
                        for (int i = 0; i < dctLength; i++)
                        { for (int j = 0; j < N; j++) array[i] += sonogram.Data[r + i, c + j]; }
                        for (int i = 0; i < dctLength; i++) array[i] /= N;

                        array = DataTools.SubtractMean(array);
                        //     DataTools.writeBarGraph(array);

                        double[] dct = Speech.DCT(array, cosines);
                        for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]);//convert to absolute values
                        for (int i = 0; i < 5; i++) dct[i] = 0.0;   //remove low freq oscillations from consideration
                        dct = DataTools.normalise2UnitLength(dct);
                        int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                        double oscilFreq = indexOfMaxValue / dctDuration * 0.5; //Times 0.5 because index = Pi and not 2Pi
                        //      DataTools.writeBarGraph(dct);

                        //mark DCT location with oscillation freq, only if oscillation freq is in correct range and amplitude
                        if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > dctThreshold))
                        {
                            for (int i = 0; i < dctLength; i++) hits[r + i, c] = oscilFreq;
                        }
                        r += 5; //skip rows
                    }
                    c++; //do alternate columns
                }
            } //foreach (AcousticEvent av in events)
            return hits;
        }



        /// <summary>
        /// Removes single lines of hits from Oscillation matrix.
        /// </summary>
        /// <param name="matrix">the Oscillation matrix</param>
        /// <returns></returns>
        public static Double[,] RemoveIsolatedOscillations(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] cleanMatrix = matrix;

            for (int c = 3; c < cols - 3; c++)//traverse columns - skip DC column
            {
                for (int r = 0; r < rows; r++)
                {
                    if (cleanMatrix[r, c] == 0.0) continue;
                    if ((matrix[r, c - 2] == 0.0) && (matrix[r, c + 2] == 0))  //+2 because alternate columns
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
        public static double[] GetODScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;
            double hitRange = binCount * 0.5 * 0.9; //set hit range slightly < half the bins. Half because only scan every second bin.
            var scores = new double[rows];
            for (int r = 0; r < rows; r++)
            {
                int score = 0;
                for (int c = minBin; c <= maxBin; c++)//traverse columns in required freq band
                {
                    if (hits[r, c] > 0) score++; //add up number of freq bins where have a hit
                }
                scores[r] = score / hitRange; //normalise the hit score in [0,1]
                if (scores[r] > 1.0) scores[r] = 1.0;
            }
            return scores;
        }//end method GetODScores()


        public static double[] GetODFrequency(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int binCount = maxBin - minBin + 1;

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
                else            oscFreq[r] = freq / (double)count; //return the average frequency
                //if (oscFreq[r] > 1.0) oscFreq[r] = 1.0;
            }
            return oscFreq;
        }//end method GetODFrequency()

        /// <summary>
        /// Converts the Oscillation Detector score array to a list of AcousticEvents. 
        /// NOTE: The scoreThreshold is adaptive. Starts at min threshold and adapts after that.
        /// </summary>
        /// <param name="scores">the array of OD scores</param>
        /// <param name="oscFreq"></param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="framesPerSec">the time scale required by AcousticEvent class</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <param name="maxThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="minDuration">duration of event must exceed this to count as an event</param>
        /// <param name="maxDuration">duration of event must be less than this to count as an event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertODScores2Events(double[] scores, double[] oscFreq, int minHz, int maxHz,
                                                               double framesPerSec, double freqBinWidth,
                                                               double minThreshold, double minDuration, double maxDuration, string fileName)
        {
            //double maxThreshold = 0.9;            //MAXIMUM BOUND OF ADAPTIVE SCORE THRESHOLD
            double scoreThreshold = minThreshold; //set this to the minimum threshold to start with
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
                    if ((isHit == true) && (scores[i] < scoreThreshold))//this is end of an event, so initialise it
                    {
                        isHit = false;
                        double endTime = i * frameOffset;
                        double duration = endTime - startTime;
                        if ((duration < minDuration) || (duration > maxDuration)) continue; //skip events with duration shorter than threshold
                        AcousticEvent ev = new AcousticEvent(startTime, duration, minHz, maxHz);
                        ev.Name = "OscillationEvent"; //default name
                        //ev.SetTimeAndFreqScales(22050, 512, 128);
                        ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                        ev.SourceFile = fileName;
                        //obtain average score.
                        double av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += scores[n];
                        ev.Score = av / (double)(i - startFrame + 1);
                        //calculate average oscillation freq and assign to ev.Score2 
                        ev.Score2Name = "OscRate"; //score2 name
                        av = 0.0;
                        for (int n = startFrame; n <= i; n++) av += oscFreq[n];
                        ev.Score2 = av / (double)(i - startFrame + 1);
                        events.Add(ev);
                    }

                //adapt the threshold
                //if ((scores[i] >= maxThreshold) && (maxThreshold >= scoreThreshold)) scoreThreshold *= 1.01;
                //else
                //if ((scores[i] <= minThreshold) && (minThreshold <= scoreThreshold)) scoreThreshold *= 0.95;
                

            } //end of pass over all frames
            return events;
        }//end method ConvertODScores2Events()

    }//end class
}
