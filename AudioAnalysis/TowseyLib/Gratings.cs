using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TowseyLib
{
    public class Gratings
    {

        static void Main(string[] args)
        {
            string title = "Home experiments";
            Console.WriteLine(title);

            //BarsAndStripes.TestCrossCorrelation();
            //BarsAndStripes.Test_DetectLocalizedPeriodicity();
            //BarsAndStripes.Test_DetectBarsUsingXcorrelation();

            //Test_DetectPeriod2Grating();
            Test_ScanArrayForGridPattern1();

            Console.WriteLine("FINISHED!");
            Console.Read();
        }

        //these keys are used to define an event in a sonogram.
        public const string key_COUNT = "count";
        public const string key_START_FRAME = "startFrame";
        public const string key_END_FRAME = "endFrame";
        public const string key_FRAME_COUNT = "frameCount";
        public const string key_START_SECOND = "startSecond";
        public const string key_END_SECOND = "endSecond";
        public const string key_MIN_FREQBIN = "minFreqBin";
        public const string key_MAX_FREQBIN = "maxFreqBin";
        public const string key_MIN_FREQ = "minFreq";
        public const string key_MAX_FREQ = "maxFreq";
        public const string key_SCORE = "score";
        public const string key_PERIODICITY = "periodicity";
        //public const string key_COUNT = "count";


        /// <summary>
        /// Runs a simple test of the DetectPeriod2Grating() method
        /// First construct an appropriate vector with alternating high and low values.
        /// </summary>
        public static void Test_DetectPeriod2Grating()
        {
            //double[] template = { 1.0, 0.0, 1.1, 0.1, 1.2, 0.2, 1.3, 0.3 };
            //double[] template = { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 };
            //double[] template = { 2.0, 0.0, 0.0, 0.0, 2.0, 0.0, 0.0, 0.0 };
            double[] template = { 1.5, 0.2, 0.8, 0.1, 0.9, 0.0, 0.8, 0.0 };
            //double[] template = { 2.0, 0.0, 0.7, 0.0, 0.6, 0.0, 0.7, 0.0 };
            //double[] template = { 4.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            double score = DetectPeriod2Grating(template);
            Console.WriteLine("score: {0:f4}", score);
        }


        /// <summary>
        /// Runs a test of the ScanArrayForGratingPattern() method.
        /// First constructs a grating signal, then embeds it in longer noise signal
        /// The grating is defined by a period and the number of cycles.
        /// The search is repeated many iterations in order to get everage accuracy.
        /// Accuracy depends on relative levels of noise gain and signal gain i.e. the SNR.
        /// </summary>
        public static void Test_ScanArrayForGridPattern1()
        {
            int n = 500;
            double[] v = new double[n];
            var rn = new RandomNumber();
            int maxIterations = 1000;
            int count = 0;

            //double[] template = { 1.0, 0.0, 1.1, 0.1, 1.2, 0.2, 1.3, 0.3 };
            //double[] template = { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 };
            //double[] template = { 2.0, 0.0, 0.0, 0.0, 2.0, 0.0, 0.0, 0.0 };
            //double[] template = { 1.5, 0.2, 0.8, 0.1, 0.9, 0.0, 0.8, 0.0 };
            //double[] template = { 2.0, 0.0, 0.7, 0.0, 0.6, 0.0, 0.7, 0.0 };
            //double[] template = { 2.0, 0.0, 0.6, 0.1, 0.5, 0.1, 0.7, 0.0 };
            //double[] template = { 4.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            int numberOfCycles = 4;
            int cyclePeriod = 8; //MUST BE AN EVEN NUMBER!!

            double[] signal = GetPeriodicSignal(cyclePeriod, numberOfCycles);
            DataTools.writeBarGraph(signal);

            double bgNoiseGain = 0.1;
            double signalGain = 0.15;
            int locationOfSignalStart = 100;
            int searchStep = 1;
            int errorTolerance = cyclePeriod;
            if (errorTolerance < searchStep) errorTolerance = searchStep + 1;

            //run many repeats of the detection to determine its accuracy. Noise in signal means result varies from iteration to iteration.
            double scoreSum = 0.0;
            for (int iter = 0; iter < maxIterations; iter++)
            {
                //construct background signal.
                for (int i = 0; i < n; i++) v[i] = rn.GetDouble() * bgNoiseGain;
                //add in the signal
                for (int i = 0; i < signal.Length; i++) v[locationOfSignalStart + i] += (signal[i] * signalGain);
                //DataTools.writeBarGraph(v);

                //detect grating in signal
                var output = ScanArrayForGratingPattern(v, searchStep, numberOfCycles, cyclePeriod);
                int maxLocation = DataTools.GetMaxIndex(output);
                scoreSum += output[maxLocation];
                if ((maxLocation > (locationOfSignalStart - errorTolerance)) && (maxLocation < (locationOfSignalStart + errorTolerance)))
                {
                    //Console.WriteLine("{0}\tscore: {1:f2}", iter, output[maxLocation]);
                    count++;
                }
            }//end iterations
            Console.WriteLine("% correct = {0:f1}   Avg score = {1:f4}", 100 * count / (double)maxIterations, scoreSum / (double)maxIterations);
        }//Test_ScanArrayForGridPattern1()



        public static double DetectPeriod2Grating(double[] v)
        {
            int length = v.Length;
            int n = length / 2;
            var difference = new double[n];

            var sums = new double[n];

            for (int i = 0; i < n; i++) //
            {
                difference[i] = v[2*i] - v[(2*i)+1];
            }

            double avDifference = Math.Abs(difference.Average());
            double maxDiff = difference.Max();
            double minDiff = difference.Min();
            double diffOfDiff = maxDiff - minDiff;

            double vigilance = 0.5; //determines how strictly similar the peaks heights must be - regulates the effect of range
            double score = avDifference - (vigilance * diffOfDiff);  //adjusted score
            if (score < 0.0) score = 0.0;
            return score;
        } //DetectBarsUsingGrid()


        /// <summary>
        /// Steps through the passed array and checks each segment for a grating pattern having period = 2 signal samples.
        /// Use this method when the array to be scanned has already been reduced by some method.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="step"></param>
        /// <param name="segmentLength"></param>
        /// <returns></returns>
        public static double[] ScanArrayForGridPattern(double[] array, int step, int segmentLength)
        {
            int length = array.Length;
            double[] gridScore = new double[length];

            var output = new double[length];
            for (int i = 0; i < length; i++)
            {
                int start = i;
                double[] extract = DataTools.Subarray(array, start, segmentLength);
                if (extract == null) return output; // reached end of array
                double score = DetectPeriod2Grating(extract);

                output[i] = score;

                i += (step - 1);
            }
            return output;
        } //ScanArrayForGridPattern()


        /// <summary>
        /// Steps through the passed array and and at each step cuts out a segment having length  = numberOfCycles * cyclePeriod. 
        /// Each segment is then reduced to length = numberOfCycles * 2.
        /// Then the reduced segment is passed to check for a grating pattern having period = 2 signal samples.
        /// Use this method when the array to be scanned will be reduced on the fly.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="step"></param>
        /// <param name="numberOfCycles"></param>
        /// <param name="cyclePeriod">NB! MUST BE AN EVEN NUMBER!!!</param>
        /// <returns></returns>
        public static double[] ScanArrayForGratingPattern(double[] array, int step, int numberOfCycles, int cyclePeriod)
        {
            //noise reduce the array to get acoustic events
            //double Q, oneSD;
            //double[] noiseReducedArray = SNR.NoiseSubtractMode(array, out Q, out oneSD);
            //double threshold = 0.001;

            int length = array.Length;
            double[] gridScore = new double[length];
            int segmentLength = numberOfCycles * cyclePeriod;

            var output = new double[length];
            for (int i = 0; i < length; i++)
            {
                //if (noiseReducedArray[i] < threshold) continue;
                double[] extract = DataTools.Subarray(array, i, segmentLength);
                if (extract == null) return output; // reached end of array

                //now reduce the segment
                double[] reducedSegment = null;
                if (cyclePeriod == 2) reducedSegment = extract;
                else
                {
                    int halfPeriod = cyclePeriod / 2;
                    int reducedLength = numberOfCycles * 2;
                    reducedSegment = new double[reducedLength];
                    for (int x = 0; x < reducedLength; x++)
                    {
                        //################# Two ways to reduce: 
                        //################ (1) by average of the period or (2) by max of the period
                        double sum = 0;
                        for (int c = 0; c < halfPeriod; c++) sum += extract[(x * halfPeriod) + c];
                        reducedSegment[x] = sum / (double)cyclePeriod;
                        //################ (2)
                        //double max = -Double.MaxValue;
                        //for (int c = 0; c < halfPeriod; c++)
                        //{
                        //    double value = extract[(x * halfPeriod) + c];
                        //    if (max < value) max = value;
                        //}
                        //reducedSegment[x] = max;
                    }
                }


                double score = DetectPeriod2Grating(reducedSegment);

                //if (i == 100)
                //{
                //    Console.WriteLine("iter=" + i + "    period=" + cyclePeriod + "    score=" + score);
                //    DataTools.writeBarGraph(extract);
                //    DataTools.writeBarGraph(reducedSegment);
                //}

                //transfer score to output array
                for (int x = 0; x < segmentLength; x++) if (output[i+x] < score) output[i+x] = score;

                i += (step - 1);
            }
            return output;
        } //ScanArrayForGridPattern()


        /// <summary>
        /// returns the period scores for a range of periods to be found in the passed array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <param name="intensityThreshold"></param>
        /// <returns></returns>
        public static List<double[]> ScanArrayForGratingPattern(double[] array, int minPeriod, int maxPeriod, int numberOfCycles, int step)
        {
            int minHalfPeriod = minPeriod / 2;
            int maxHalfPeriod = maxPeriod / 2;
            var list = new List<double[]>();

            //for (int halfPeriod = minHalfPeriod; halfPeriod <= maxHalfPeriod; halfPeriod++)
            //{
            //    int cyclePeriod = halfPeriod * 2;
            //    double[] scores = ScanArrayForGratingPattern(array, step, numberOfCycles, cyclePeriod);
            //    list.Add(scores);
            //} // for
            for (int p = minPeriod; p <= maxPeriod; p++)
            {
                double[] scores = ScanArrayForGratingPattern(array, step, numberOfCycles, p);
                list.Add(scores);
            } // for
            return list;
        } //ScanArrayForGratingPattern()


        public static Tuple<double[], double[]> MergePeriodicScoreArrays(List<double[]> scores, int minPeriod, int maxPeriod)
        {
            //assume all score arrays are of the same length;
            int length = scores[0].Length;
            var intensity   = new double[length];
            var periodicity = new double[length];

            //assume that the score arrays are arranged in order in the list and range from the passed min and max periods.
            for (int p = 0; p< scores.Count; p++)
            {
                int cyclePeriod = (p + 1) * 2;  //p = the halfPeriod
                for (int i = 0; i < length; i++)
                {
                    if (scores[p][i] > intensity[i])
                    {
                        intensity[i]   = scores[p][i];
                        periodicity[i] = cyclePeriod;
                    }
                }
            }

            return Tuple.Create(intensity, periodicity);
        } //MergePeriodicScoreArrays()



        public static List<Dictionary<string, double>> ExtractMyPeriodicEvents(double[] intensity, double[] periodicity, double intensityThreshold)
        {
            //could do a possible adjustment of the threshold for period.
            //double adjustedThreshold = intensityThreshold * factor;  //adjust threshold to period. THis is a correction for pink noise
            var events = DataTools.SegmentArrayOnThreshold(intensity, intensityThreshold);

            var list = new List<Dictionary<string, double>>();
            foreach (double[] item in events)
            {
                var ev = new Dictionary<string, double>();
                ev[key_START_FRAME] = item[0];
                ev[key_END_FRAME] = item[1];
                ev[key_SCORE] = item[2];
                double cyclePeriod = 0.0;
                for (int n = (int)item[0]; n <= (int)item[1]; n++) cyclePeriod += periodicity[n];
                ev[key_PERIODICITY] = cyclePeriod / (item[1] - item[0] + 1);
                list.Add(ev);
            } //foreach
            return list;
        } //ExtractPeriodicEvents()



        public static List<Dictionary<string, double>> ExtractPeriodicEvents(double[] intensity, double[] periodicity, double intensityThreshold)
        {
            //could do a possible adjustment of the threshold for period.
            //double adjustedThreshold = intensityThreshold * factor;  //adjust threshold to period. THis is a correction for pink noise
            var events = DataTools.SegmentArrayOnThreshold(intensity, intensityThreshold);

            var list = new List<Dictionary<string, double>>();
            foreach (double[] item in events)
            {
                var ev = new Dictionary<string, double>();
                ev[key_START_FRAME] = item[0];
                ev[key_END_FRAME]   = item[1];
                ev[key_SCORE]       = item[2];
                double cyclePeriod = 0.0;
                for (int n = (int)item[0]; n <= (int)item[1]; n++) cyclePeriod += periodicity[n];
                ev[key_PERIODICITY] = cyclePeriod / (item[1] - item[0] + 1);
                list.Add(ev);
            } //foreach
            return list;
        } //ExtractPeriodicEvents()



        ///used for testing purposes
        public static double[] GetPeriodicSignal(int cyclePeriod, int numberOfCycles)
        {
            //double[] template = { 1.0, 0.0, 1.1, 0.1, 1.2, 0.2, 1.3, 0.3 };
            double[] template = { 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0 };
            //double[] template = { 2.0, 0.0, 0.0, 0.0, 2.0, 0.0, 0.0, 0.0 };
            //double[] template = { 1.5, 0.2, 0.8, 0.1, 0.9, 0.0, 0.8, 0.0 };
            //double[] template = { 2.0, 0.0, 0.7, 0.0, 0.6, 0.0, 0.7, 0.0 };
            //double[] template = { 2.0, 0.0, 0.6, 0.1, 0.5, 0.1, 0.7, 0.0 };
            //double[] template = { 4.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            int signalLength = numberOfCycles * cyclePeriod;

            int halfPeriod = cyclePeriod / 2;
            double[] signal = template;
            if (cyclePeriod > 2)
            {
                int buffer = halfPeriod; //used to allow smoothing average to bleed.
                signal = new double[buffer + signalLength + buffer];
                for (int x = 0; x < template.Length; x++)
                {
                    for (int p = 0; p < halfPeriod; p++) signal[buffer + (x * halfPeriod) + p] = template[x]; //transfer signal
                    if (cyclePeriod > 12) signal = DataTools.filterMovingAverage(signal, 3);
                } //for
                signal = DataTools.Subarray(signal, buffer, signalLength);
            } // if (cyclePeriod > 2)

            signal = DataTools.normalise(signal);
            return signal;
        }



    }//class Gratings
}
