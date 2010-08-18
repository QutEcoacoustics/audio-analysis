using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class HarmonicAnalysis
    {

        /// <summary>
        /// Returns a tuple consisting of: 
        /// 1) an array of scores over the entire recording
        /// 2) a list of acoustic events
        /// 3) a matrix of hits corresonding to the spectrogram. 
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="harmonicCount">expected number of harmonics in the frequency band</param>
        /// <param name="amplitudeThreshold">ignore harmonics with an amplitude less than this minimum dB</param>
        /// <param name="minDuration">look for events of this duration</param>
        /// <param name="maxDuration">look for events of this duration</param>
        public static System.Tuple<double[], double[,], List<AcousticEvent>> Execute(SpectralSonogram sonogram, int minHz, int maxHz, int harmonicCount, 
                                                        double amplitudeThreshold, double minDuration, double maxDuration, string audioFileName, string callName)
        {
            // DETECT OSCILLATIONS
            //find freq bins
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int hzWidth = maxHz - minHz;
            //var results = DetectHarmonicsUsingFormantGap(sonogram.Data, minBin, maxBin, hzWidth, minHarmonicPeriod, maxHarmonicPeriod, amplitudeThreshold);
            var results = CountHarmonicTracks(sonogram.Data, minBin, maxBin, hzWidth, harmonicCount, amplitudeThreshold);

            double[] scores = DataTools.filterMovingAverage(results.Item1, 5); //smooth the scores
            var hits = results.Item2;

            // EXTRACT SCORES AND ACOUSTIC EVENTS
            double[] oscFreq = GetHDFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                                         amplitudeThreshold, minDuration, maxDuration, audioFileName, callName);

            return Tuple.Create(scores, hits, predictedEvents);
        }//end method


        public static System.Tuple<double[], double[,]> CountHarmonicTracks(Double[,] matrix, int minBin, int maxBin, int hzWidth,
                                                                            int expectedHarmonicCount, double amplitudeThreshold)
        {
            int binBand = maxBin - minBin + 1; // DCT spans N freq bins
           // int expectedPeriod = binBand / expectedHarmonicCount;

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            double[] harmonicScore = new double[rows];
            int[]    harmonicCount = new int[rows];

            for (int r = 0; r < rows; r++)
            {
                var array = new double[binBand];
                for (int c = 0; c < binBand; c++) array[c] = matrix[r, c + minBin]; // assume that matrix has already been smoothed in time direction
                var results = DataTools.CountHarmonicTracks(array, expectedHarmonicCount, r);
                harmonicCount[r] = results.Item2; // number of harmonic tracks.
                double weight = 1.0;
                double delta = Math.Abs(results.Item2 - expectedHarmonicCount);  //Item2 = number of spectral tracks
                // weight the score according to difference between expected and observed track count
                if (delta > 3) weight = 3 / delta;  
                double score = weight * results.Item1; 
                if (score > amplitudeThreshold) // threshold the score
                {
                    harmonicScore[r] = score; // amplitude score
                    for (int c = minBin; c < maxBin; c++) { hits[r, c] = results.Item2; c += 3; }
                }
                //if ((r > 2450) && (r < 2550))
                //     Console.WriteLine("{0}  score={1:f2}  count={2}", r, harmonicScore[r], harmonicCount[r]);
            }// rows

            return Tuple.Create(harmonicScore, hits);
        }


        public static System.Tuple<double[], double[,]> DetectHarmonicsUsingFormantGap(Double[,] matrix, int minBin, int maxBin, int hzWidth,
            int minPeriod, int maxPeriod,  int minHarmonicPeriod, double amplitudeThreshold)
        {
            int binBand = maxBin - minBin + 1; // DCT spans N freq bins

            int minDeltaIndex = (int)(hzWidth / (double)maxPeriod * 2); // Times 0.5 because index = Pi and not 2Pi
            int maxDeltaIndex = (int)(hzWidth / (double)minPeriod * 2); // Times 0.5 because index = Pi and not 2Pi
            // double period = hzWidth / (double)indexOfMaxValue * 2;   // Times 2 because index = Pi and not 2Pi
            Console.WriteLine("minPeriod={0}    maxPeriod={1}", minDeltaIndex, maxDeltaIndex);

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            double[] periodScore = new double[rows];
            double[] periodicity = new double[rows];


            for (int r = 0; r < rows - 5; r++)
            {
                var array = new double[binBand];
                //SMOOTH the matrix in time direction - accumulate J rows of values
                //for (int c = 0; c < binBand; c++)
                //    for (int j = 0; j < 5; j++) array[c] += matrix[r + j, c + minBin];
                //for (int c = 0; c < binBand; c++) array[c] /= 5.0; //average

                //the following line assumes that matrix has already been smoothed in time direction
                for (int c = 0; c < binBand; c++) array[c] = matrix[r, c + minBin];
                var results = DataTools.Periodicity(array, minDeltaIndex, maxDeltaIndex);
                amplitudeThreshold = 5.0;

                if (results.Item1 > amplitudeThreshold) //Item1 = amplitude of the periodicity
                {
                    periodScore[r] = results.Item1; // maximum amplitude obtained over all periods and phases
                    periodicity[r] = results.Item2; // the period for which the maximum amplitude was obtained.
                    // phase[r] = results.Item3;    // the phase of period for which max amplitude was obtained.
                    for (int c = minBin; c < maxBin; c++) { hits[r, c] = results.Item2; c++; }
                }
            }// rows

            return Tuple.Create(periodScore, hits);
        }



        public static Double[,] DetectHarmonicsUsingDCT(Double[,] matrix, int minBin, int maxBin, int hzWidth, bool normaliseDCT,
                                                                         int minPeriod, int maxPeriod, double dctThreshold)
        {

            int dctLength = maxBin - minBin + 1; //DCT spans N freq bins

            int minIndex = (int)(hzWidth / (double)maxPeriod * 2); //Times 0.5 because index = Pi and not 2Pi
            int maxIndex = (int)(hzWidth / (double)minPeriod * 2); //Times 0.5 because index = Pi and not 2Pi
            //double period = hzWidth / (double)indexOfMaxValue * 2; //Times 2 because index = Pi and not 2Pi
            if (maxIndex > dctLength) maxIndex = dctLength; //safety check in case of future changes to code.

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];

            double[,] cosines = Speech.Cosines(dctLength, dctLength); //set up the cosine coefficients

            for (int r = 0; r < rows - dctLength; r++)
            {
                //for (int c = minBin; c <= minBin; c++)//traverse columns - skip DC column
                //{
                var array = new double[dctLength];
                //accumulate J rows of values
                for (int i = 0; i < dctLength; i++)
                    for (int j = 0; j < 5; j++) array[i] += matrix[r + j, minBin + i];

                array = DataTools.SubtractMean(array);
                //     DataTools.writeBarGraph(array);

                double[] dct = Speech.DCT(array, cosines);
                for (int i = 0; i < dctLength; i++) dct[i] = Math.Abs(dct[i]); //convert to absolute values
                for (int i = 0; i < 5; i++) dct[i] = 0.0;  //remove low freq values from consideration
                if (normaliseDCT) dct = DataTools.normalise2UnitLength(dct);
                int indexOfMaxValue = DataTools.GetMaxIndex(dct);
                //DataTools.writeBarGraph(dct);

                double period = hzWidth / (double)indexOfMaxValue * 2; //Times 2 because index = Pi and not 2Pi

                //mark DCT location with harmonic freq, only if harmonic freq is in correct range and amplitude
                if ((indexOfMaxValue >= minIndex) && (indexOfMaxValue <= maxIndex) && (dct[indexOfMaxValue] > dctThreshold))
                {
                    for (int i = 0; i < dctLength; i++) hits[r, minBin + i] = period;
                    for (int i = 0; i < dctLength; i++) hits[r + 1, minBin + i] = period; //alternate row
                }
                //c += 5; //skip columns
                //}
                r++; //do alternate row
            }
            return hits;
        }


        /// <summary>
        /// Removes single lines of hits from Harmonics matrix.
        /// </summary>
        /// <param name="matrix">the Harmonics matrix</param>
        /// <returns></returns>
        public static Double[,] RemoveIsolatedHits(Double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] cleanMatrix = matrix;

            for (int r = 3; r < rows - 3; r++)//traverse rows
            {
                for (int c = 2; c < cols; c++)//skip DC column
                {
                    if (cleanMatrix[r, c] == 0.0) continue;
                    if ((matrix[r-2, c] == 0.0) && (matrix[r + 2, c] == 0))  //+2 because alternate columns
                        cleanMatrix[r, c] = 0.0;
                }
            }
            return cleanMatrix;
        } //end method RemoveIsolatedHits()


        /// <summary>
        /// Converts the hits derived from the harmonic detector into a score for each frame.
        /// </summary>
        /// <param name="hits">sonogram as matrix showing location of harmonic hits</param>
        /// <param name="minHz">lower freq bound of the acoustic event</param>
        /// <param name="maxHz">upper freq bound of the acoustic event</param>
        /// <param name="freqBinWidth">the freq scale required by AcousticEvent class</param>
        /// <returns></returns>
        public static double[] GetHarmonicScores(double[,] hits, int minHz, int maxHz, double freqBinWidth)
        {
            int rows = hits.GetLength(0);
            int cols = hits.GetLength(1);
            int minBin = (int)(minHz / freqBinWidth);
            int maxBin = (int)(maxHz / freqBinWidth);
            int targetBin = minBin + ((maxBin - minBin) / 2);
            var scores = new double[rows];
            for (int r = 0; r < rows; r++) //score if hit in middle bin
            {
                if (hits[r, targetBin] > 0) scores[r] = 1.0;
            }
            return scores;
        }//end method GetHarmonicScores()

        /// <summary>
        /// TODO: This method not yet refactored for harmonic period.
        /// </summary>
        /// <param name="hits"></param>
        /// <param name="minHz"></param>
        /// <param name="maxHz"></param>
        /// <param name="freqBinWidth"></param>
        /// <returns></returns>
        public static double[] GetHDFrequency(double[,] hits, int minHz, int maxHz, double freqBinWidth)
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
        /// <param name="eventThreshold">OD score must exceed this threshold to count as an event</param>
        /// <param name="duration">expected duration of event</param>
        /// <param name="fileName">name of source file to be added to AcousticEvent class</param>
        /// <returns></returns>
        public static List<AcousticEvent> ConvertHDScores2Events(double[] scores, double[] oscFreq, int minHz, int maxHz,
                                                                 double framesPerSec, double freqBinWidth,
                                                                 double eventThreshold, double expectedDuration, string fileName)
        {
            int count = scores.Length;
            //int minBin = (int)(minHz / freqBinWidth);
            //int maxBin = (int)(maxHz / freqBinWidth);
            //int binCount = maxBin - minBin + 1;
            var events = new List<AcousticEvent>();
            
            double frameOffset = 1 / framesPerSec;
            int frameDuration = (int)(expectedDuration * framesPerSec);

            for (int i = 0; i < count - frameDuration; i++)//pass over all frames
            {
                if (scores[i] <= 0.0) continue;

                int hitCount = 0;
                double total = 0.0;
                double avPeriod = 0.0;
                for (int j = 0; j < frameDuration; j++)//check ahead over frame duration
                {
                    if (scores[i + j] > 0.0)
                    {
                        hitCount++; //get density of hits
                        total += oscFreq[i + j]; //calucalte period of harmonics
                    }
                }
                double density = hitCount * 2 / (double)frameDuration;
                if (density < eventThreshold) continue;
                avPeriod = total / (double)hitCount;

                //have found an event
                double startTime = i * frameOffset;
                AcousticEvent ev = new AcousticEvent(startTime, expectedDuration, minHz, maxHz);
                ev.Name = "HarmonicEvent"; //default name
                ev.SetTimeAndFreqScales(framesPerSec, freqBinWidth);
                ev.SourceFile = fileName;
                ev.Score = density;  //score 1
                //calculate average harmonic period and assign to ev.Score2 
                ev.Score2Name = "Period"; //score2 name
                ev.Score2 = avPeriod;
                events.Add(ev);
                i += frameDuration;
            } //end of pass over all frames
            return events;
        }//end method ConvertHDScores2Events()

    }//end class HarmonicAnalysis
}
