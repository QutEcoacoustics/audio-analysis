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
        public static System.Tuple<double[], double[,]> Execute(SpectralSonogram sonogram, int minHz, int maxHz, int harmonicCount, double amplitudeThreshold)
        {
            int minBin = (int)(minHz / sonogram.FBinWidth);
            int maxBin = (int)(maxHz / sonogram.FBinWidth);
            int hzWidth = maxHz - minHz;

            // IDENTIFY HARMONIC TRACKS AND CALCULATE SCORES
            //var results = DetectHarmonicsUsingFormantGap(sonogram.Data, minBin, maxBin, hzWidth, minHarmonicPeriod, maxHarmonicPeriod, amplitudeThreshold);
            var results = CountHarmonicTracks(sonogram.Data, minBin, maxBin, hzWidth, harmonicCount, amplitudeThreshold);
            double[] scores = results.Item1;
            var hits        = results.Item2;
            return Tuple.Create(scores, hits);
        }//end method


        public static System.Tuple<double[], double[,]> CountHarmonicTracks(Double[,] matrix, int minBin, int maxBin, int hzWidth, int expectedHarmonicCount, double amplitudeThreshold)
        {
            int binWidth = maxBin - minBin + 1;
            // int expectedPeriod = binWidth / expectedHarmonicCount;

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            Double[,] hits = new Double[rows, cols];
            double[] harmonicScore = new double[rows];
            int[]    harmonicCount = new int[rows];

            for (int r = 0; r < rows; r++)
            {
                var array = new double[binWidth];
                for (int c = 0; c < binWidth; c++) array[c] = matrix[r, minBin + c]; 
                var results = CountHarmonicTracks(array, expectedHarmonicCount);
                int peakCount = results.Item2; // number of harmonic tracks i.e. the peakCount.
                if (peakCount == 0) continue;

                bool[] peaks = results.Item3;
                double delta = Math.Abs(peakCount - expectedHarmonicCount);  //Item2 = number of spectral tracks
                // weight the score according to difference between expected and observed track count
                double weight = 1.0;
                if (delta > 4) weight = 4 / delta;  
                double score = weight * results.Item1;
                if (score < amplitudeThreshold) continue;

                harmonicCount[r] = peakCount;
                harmonicScore[r] = score; // amplitude score
                //for (int c = 0; c < peaks.Length; c++) if(peaks[c]) { hits[r, minBin + c] = results.Item2; }  // for display purposes.
                if (r % 2 == 0) continue; //draw hits for every second row - so can see underneath!
                for (int c = 0; c < binWidth; c++) if (peaks[c]) { hits[r, minBin + c] = 20; c++; }  // for display purposes.
                
            }// rows

            return Tuple.Create(harmonicScore, hits);
        }


        /// <summary>
        /// Counts the number of spectral tracks or harmonics in the passed ferquency band.
        /// Also calculates the average amplitude of the peaks to each succeeding trough.
        /// </summary>
        /// <param name="values">Spectral values in the frequency band.</param>
        /// <param name="expectedPeriod">Use supplied parameter. Expected number of harmonic tracks in the frequency band.</param>
        /// <param name="row">This argument is NOT used. Is included only for debugging purposes.</param>
        /// <returns></returns>
        public static Tuple<double, int, bool[]> CountHarmonicTracks(double[] values, int expectedHarmonicCount)
        {
            int L = values.Length;
            int expectedPeriod = L / expectedHarmonicCount;
            int midPeriod = expectedPeriod / 2;
            //double[] smooth = DataTools.filterMovingAverage(values, 3);
            double[] smooth = values;
            bool[] peaks = DataTools.GetPeaks(smooth);
            int peakCount = DataTools.CountTrues(peaks);

            //return if too far outside limits
            int lowerLimit = expectedHarmonicCount / 2;
            int upperLimit = expectedHarmonicCount * 2;
            if (peakCount <= lowerLimit) return Tuple.Create(0.0, 0, peaks);
            else
                if (peakCount >= upperLimit) return Tuple.Create(0.0, peakCount, peaks);

            // Store peak locations.
            var peakLocations = new List<int>();
            for (int i = 0; i < values.Length; i++)
            {
                if (peaks[i]) peakLocations.Add(i);
            }


            //// If have too many peaks (local maxima), remove the lowest of them 
            //if (peakCount > (expectedHarmonicCount + 1))
            //{
            //    var peakValues = new double[peakCount];
            //    for (int i = 0; i < peakCount; i++) peakValues[i] = values[peakLocations[i]];
            //    IEnumerable<double> ordered = peakValues.OrderByDescending(d => d);
            //    double avValue = ordered.Take(expectedHarmonicCount).Average();
            //    double min = ordered.Last();
            //    double threshold = min + ((avValue - min) / 2);
            //    // apply threshold to remove low peaks
            //    for (int i = 0; i < L; i++)
            //    {
            //        if ((peaks[i]) && (values[i] < threshold)) peaks[i] = false;
            //    }

            //    // recalculate the number of peaks
            //    peakCount = -1;
            //    for (int i = 0; i < L; i++)
            //    {
            //        if (peaks[i]) 
            //        {
            //            peakCount++;
            //            peakLocations[peakCount] = i;
            //        }
            //    }
            //}

            //if (peakCount <= 1) return Tuple.Create(0.0, 0, peaks);

            double amplitude = 0.0;
            for (int i = 0; i < peakLocations.Count; i++)
            {
                int troughIndex = peakLocations[i] + midPeriod;
                if (troughIndex >= L) troughIndex = peakLocations[i] - midPeriod;
                double delta = smooth[peakLocations[i]] - smooth[troughIndex];
                if (delta > 1.0) amplitude += delta; // dB threshold - required a minimum perceptible difference
            }
            double avAmplitude = amplitude / (double)peakCount;
            return Tuple.Create(avAmplitude, peakCount, peaks);
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
