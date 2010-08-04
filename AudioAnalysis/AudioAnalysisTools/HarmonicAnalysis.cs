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
        /// 
        /// </summary>
        /// <param name="sonogram">sonogram derived from the recording</param>
        /// <param name="minHz">min bound freq band to search</param>
        /// <param name="maxHz">max bound freq band to search</param>
        /// <param name="minOscilFreq">ignore oscillation frequencies below this threshold</param>
        /// <param name="maxOscilFreq">ignore oscillation frequencies greater than this </param>
        /// <param name="minAmplitude">ignore DCT amplitude values less than this minimum </param>
        /// <param name="scoreThreshold">used for FP/FN</param>
        /// <param name="expectedDuration">look for events of this duration</param>
        /// <param name="scores">return an array of scores over the entire recording</param>
        /// <param name="events">return a list of acoustic events</param>
        /// <param name="hits"></param>
        public static System.Tuple<double[], double[,], List<AcousticEvent>> Execute(SpectralSonogram sonogram, int minHz, int maxHz, 
                                 int minPeriod, int maxPeriod, double minAmplitude, double scoreThreshold, double expectedDuration)
        {
            //DETECT OSCILLATIONS
            bool normaliseDCT = true;
            var results = DetectHarmonics(sonogram, minHz, maxHz, normaliseDCT, minPeriod, maxPeriod, minAmplitude);
            double[] scores = DataTools.filterMovingAverage(results.Item1, 7); //smooth the scores

            var hits = results.Item2;
            hits = RemoveIsolatedHits(hits);

            //EXTRACT SCORES AND ACOUSTIC EVENTS
            //scores = GetHarmonicScores(hits, minHz, maxHz, sonogram.FBinWidth);
            double[] oscFreq = GetHDFrequency(hits, minHz, maxHz, sonogram.FBinWidth);
            List<AcousticEvent> events = ConvertHDScores2Events(scores, oscFreq, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth, scoreThreshold,
                                            expectedDuration, sonogram.Configuration.SourceFName);
            return Tuple.Create(scores, hits, events);
        }//end method


        /// <summary>
        /// Detects harmonics in a given frame.
        /// there are several important parameters for tuning.
        /// a) DCTLength: Good values are 0.25 to 0.50 sec. Do not want too long because DCT requires stationarity.
        ///     Do not want too short because too small a range of oscillations
        /// b) DCTindex: Sets lower bound for oscillations of interest. Index refers to array of coeff returned by DCT.
        ///     Array has same length as the length of the DCT. Low freq oscillations occur more often by chance. Want to exclude them.
        /// c) MinAmplitude: minimum acceptable value of a DCT coefficient if hit is to be accepted.
        ///     The algorithm is sensitive to this value. A lower value results in more oscillation hits being returned.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minBin">min freq bin of search band</param>
        /// <param name="maxBin">max freq bin of search band</param>
        /// <param name="dctLength">number of values</param>
        /// <param name="DCTindex">Sets lower bound for oscillations of interest.</param>
        /// <param name="minAmplitude">threshold - do not accept a DCT value if its amplitude is less than this threshold</param>
        /// <returns></returns>


        public static System.Tuple<double[], double[,]> DetectHarmonics(SpectralSonogram sonogram, int minHz, int maxHz, bool normaliseDCT,
                                                   int minPeriod, int maxPeriod, double dctThreshold)
        {
            //find freq bins
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);

            int hzWidth   = maxHz  - minHz;

            var results = DetectHarmonicsUsingFormantGap(sonogram.Data, minBin, maxBin, hzWidth, normaliseDCT, minPeriod, maxPeriod, dctThreshold);
            return results;
        }

        public static System.Tuple<double[], double[,]> DetectHarmonicsUsingFormantGap(Double[,] matrix, int minBin, int maxBin, int hzWidth, bool normaliseDCT,
                                                                         int minPeriod, int maxPeriod, double dctThreshold)
        {

            int binBand = maxBin - minBin + 1; //DCT spans N freq bins

            int minDeltaIndex = (int)(hzWidth / (double)maxPeriod * 2); //Times 0.5 because index = Pi and not 2Pi
            int maxDeltaIndex = (int)(hzWidth / (double)minPeriod * 2); //Times 0.5 because index = Pi and not 2Pi
            //double period = hzWidth / (double)indexOfMaxValue * 2; //Times 2 because index = Pi and not 2Pi

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            double[] periodScore = new double[rows];
            double[] periodicity = new double[rows];
            double amplitudeThreshold = 6.0;


            for (int r = 0; r < rows - 5; r++)
            {
                var array = new double[binBand];
                //accumulate J rows of values
                for (int c = 0; c < binBand; c++)
                    for (int j = 0; j < 5; j++) array[c] += matrix[r + j, c + minBin];
                for (int c = 0; c < binBand; c++) array[c] /= 5.0; //average
                //array = DataTools.SubtractMean(array);

                //if (r ==84)
                //{
                //    DataTools.writeBarGraph(array);
                //}
                var results = DataTools.Periodicity(array, minDeltaIndex, maxDeltaIndex);
                if (results.Item1 > amplitudeThreshold)
                {
                    periodScore[r] = results.Item1;
                    periodicity[r] = results.Item2;
                }
                //if ((r > 50) && (r < 200)) Log.WriteLine("{0}  score={1:f2}  period={2}, phase={3}", r, periodScore[r], periodicity[r], results.Item3);
            }//rows

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
                    //Console.WriteLine("r={0},  period={1:f0},  amplitude={2:f2}", r, period, dct[indexOfMaxValue]);
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
