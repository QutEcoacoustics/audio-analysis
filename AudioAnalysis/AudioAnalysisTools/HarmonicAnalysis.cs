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
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int hzWidth = maxHz - minHz;

            // IDENTIFY HARMONIC TRACKS AND CALCULATE SCORES
            //var results = DetectHarmonicsUsingFormantGap(sonogram.Data, minBin, maxBin, hzWidth, minHarmonicPeriod, maxHarmonicPeriod, amplitudeThreshold);
            var results = CountHarmonicTracks(sonogram.Data, minBin, maxBin, hzWidth, harmonicCount, amplitudeThreshold);
            double[] scores = DataTools.filterMovingAverage(results.Item1, 5); //smooth the scores

            // ACOUSTIC EVENTS
            List<AcousticEvent> predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                                                         amplitudeThreshold, minDuration, maxDuration, audioFileName, callName);
            var hits = results.Item2;
            return Tuple.Create(scores, hits, predictedEvents);
        }//end method


        public static System.Tuple<double[], double[,]> CountHarmonicTracks(Double[,] matrix, int minBin, int maxBin, int hzWidth, int expectedHarmonicCount, double amplitudeThreshold)
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
                    for (int c = minBin; c < maxBin; c++) { hits[r, c] = results.Item2; c += 3; } // only used for display purposes.
                }
                //if ((r > 2450) && (r < 2550))
                //     Console.WriteLine("{0}  score={1:f2}  count={2}", r, harmonicScore[r], harmonicCount[r]);
            }// rows

            return Tuple.Create(harmonicScore, hits);
        }


        /// <summary>
        /// This method did not work much better than the DCT method - see below.
        /// Looks for a series of harmonic tracks at fixed freq intervals.
        /// Problem is that the harmonic tracks are not necessarily at fixed intervals
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minBin"></param>
        /// <param name="maxBin"></param>
        /// <param name="hzWidth"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <param name="minHarmonicPeriod"></param>
        /// <param name="amplitudeThreshold"></param>
        /// <returns></returns>
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


        /// <summary>
        /// THIS METHOD NO LONGER IN USE.
        /// NOT USEFUL FOR ANIMAL CALLS.
        /// Tried this but it is suitable only when there is guarantee of numerous spectral tracks as in the vowels of human speech.
        /// It yields SPURIOUS RESULTS where there is only one whistle track.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="minBin"></param>
        /// <param name="maxBin"></param>
        /// <param name="hzWidth"></param>
        /// <param name="normaliseDCT"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <param name="dctThreshold"></param>
        /// <returns></returns>
        public static Double[,] DetectHarmonicsUsingDCT(Double[,] matrix, int minBin, int maxBin, int hzWidth, bool normaliseDCT, int minPeriod, int maxPeriod, double dctThreshold)
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



    }//end class HarmonicAnalysis
}
