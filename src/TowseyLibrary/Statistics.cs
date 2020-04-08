// <copyright file="Statistics.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Text;

    /// <summary>
    /// consists of a set of static methods to do elementary statistics
    /// NOTE: Much of stats to do with a normal distribution is in the NormDist CLASS.
    /// </summary>
    public class Statistics
    {
        public static double GetMedian(double[] v)
        {
            Tuple<int[], double[]> tuple = DataTools.SortArray(v);
            double median = tuple.Item2[v.Length / 2];
            return median;
        }

        public static int GetNthPercentileBin(int[] distribution, int percentile)
        {
            int length = distribution.Length;
            double threshold = percentile / 100D;
            double[] probs = DataTools.NormaliseArea(distribution);
            double[] cumProb = DataTools.ConvertProbabilityDistribution2CummulativeProbabilites(probs);
            int percentileBin = 0;
            for (int i = 0; i < length; i++)
            {
                if (cumProb[i] >= threshold)
                {
                    percentileBin = i;
                    break;
                }
            }

            return percentileBin;
        }

        /// <summary>
        /// NOTE: The sort routine sorts in descending order.
        /// Therefore the percentile value has to be reversed.
        /// </summary>
        public static double GetPercentileValue(double[] v, int percentile)
        {
            Tuple<int[], double[]> tuple = DataTools.SortArray(v);
            var fraction = (100 - percentile) / 100.0;
            var percentileBin = (int)Math.Round(v.Length * fraction);
            double percentileValue = tuple.Item2[percentileBin];
            return percentileValue;
        }

        /// <summary>
        /// Analyzes an array of events or hits, represented by a binary of matrix.
        /// Assumes a Poisson distribution
        /// Returns an array of Z-scores indicating the probability at any time or frame that the number of hits occuring
        /// in the window centered on that point could have occured by chance.
        /// </summary>
        public static void AnalyzeClustersOfHits(int[] hits, int window, double thresholdZ, int thresholdCount,
                                                out double[] zScores, out double expectedHits, out double sd)
        {
            int frameCount = hits.Length;
            int hitCount = DataTools.CountPositives(hits);
            expectedHits = (double)hitCount * window / frameCount;

            // assume Poisson Distribution
            sd = Math.Sqrt(expectedHits);

            // LoggedConsole.WriteLine("hitCount="+hitCount+"  expectedHits = " + expectedHits + "+/-" + sd+"  thresholdSum="+thresholdSum);
            int offset = (int)(window * 0.5); // assign score to position in window
            int sum = 0;
            for (int i = 0; i < window; i++)
            {
                // set up the song window
                if (hits[i] > 0)
                {
                    sum++;
                }
            }

            // now calculate z-scores for the number of syllable hits in a window
            zScores = new double[frameCount];
            for (int i = window; i < frameCount; i++)
            {
                if (sum < thresholdCount)
                {
                    // not enough hits to constitute a cluster - set ascore to neg value
                    zScores[i - offset] = -10.0;
                }
                else
                {
                    zScores[i - offset] = (sum - expectedHits) / sd;
                }

                sum = sum - hits[i - window] + hits[i]; // move the songwindow
            }
        }

        public static string tStatisticAndSignificance(double m1, double sd1, int count1,
                                        double m2, double sd2, int count2)
        {
            double t = tStatistic(m1, sd1, count1, m2, sd2, count2);
            int df = count1 + count2 - 2;
            double p = tStatisticAlpha(Math.Abs(t), df);
            StringBuilder sb = new StringBuilder("t=" + t.ToString("F3"));
            sb.Append(" p=" + p.ToString("F4"));
            if (p == 0.050)
            {
                sb.Append("*");
            }

            if (p == 0.025)
            {
                sb.Append("*");
            }

            if (p == 0.010)
            {
                sb.Append("**");
            }

            if (p == 0.005)
            {
                sb.Append("**");
            }

            if (p <= 0.001)
            {
                sb.Append("***");
            }

            return sb.ToString();
        }

        /**
         * Calculates the t-statistic.
         * t(df) = (m1-m2)/SE
         * where SE^2 = s^2(m+n)/(mn)
         * where s^2 = estimated variance = ((ColumnLeft-1)v1 + (ColumnRight-1)v2)/(ColumnLeft+ColumnRight-2)
         * where v1 = sd1^2 and v2 =  sd2^2
         * To calculate ASSUME df = ColumnLeft+ColumnRight-2 = infinity
         * @param m1
         * @param sd1
         * @param count1
         * @param m2
         * @param sd2
         * @param count2
         * @return
         */
        public static double tStatistic(double m1, double sd1, int count1,
                                        double m2, double sd2, int count2)
        {
            double v1 = sd1 * sd1;
            double v2 = sd2 * sd2;
            int df = count1 + count2 - 2;
            double v = (((count1 - 1) * v1) + ((count2 - 1) * v2)) / df;
            double seSquared = v * (count1 + count2) / (count1 * count2);
            double t = (m1 - m2) / Math.Sqrt(seSquared);
            return t;
        }

        public static double tStatisticAlpha(double t, int df)
        {
            double[] table_df_inf = { 0.25, 0.51, 0.67, 0.85, 1.05, 1.282, 1.645, 1.96, 2.326, 2.576, 3.09, 3.291 };
            double[] table_df_15 = { 0.26, 0.53, 0.69, 0.87, 1.07, 1.341, 1.753, 2.13, 2.602, 2.947, 3.73, 4.073 };
            double[] alpha = { 0.40, 0.30, 0.25, 0.20, 0.15, 0.10, 0.05, 0.025, 0.01, 0.005, 0.001, 0.0005 };
            double[] tTable = table_df_inf;
            if (df <= 15)
            {
                tTable = table_df_15;
            }

            double p = 0.5;
            int size = alpha.Length - 1;
            double t2 = 4.0;
            double p2 = 0.0001;

            // first check if t exceeds the max in table
            if (t > tTable[size])
            {
                return alpha[size];
            }

            for (int i = size; i >= 0; i--)
            {
                if (t > tTable[i])
                {
                    double t1 = tTable[i];
                    if (i < size)
                    {
                        t2 = tTable[i + 1];
                    }

                    double p1 = alpha[i];
                    if (i < size)
                    {
                        p2 = alpha[i + 1];
                    }

                    double slope = (p2 - p1) / (t2 - t1);
                    p = (slope * (t - t1)) + p1;
                    break;
                }
            }

            return p;
        }

        public static double[] bayesBoundary(int countC1, double meanC1, double sdC1, int countC2, double meanC2, double sdC2)
        {
            double lnRatio = Math.Log(countC1 / (double)countC2);
            double sqrMean1 = meanC1 * meanC1;
            double sqrMean2 = meanC2 * meanC2;
            double sqrSD1 = sdC1 * sdC1;
            double sqrSD2 = sdC2 * sdC2;

            double A = ((1 / sqrSD2) - (1 / sqrSD1)) / 2;
            double B = (meanC2 / sqrSD2) - (meanC1 / sqrSD1);
            B = -B;
            double C = Math.Log(sdC2 / sdC1) + lnRatio;
            C = C + (sqrMean2 / 2 / sqrSD2) - (sqrMean1 / 2 / sqrSD1);
            double[] ob = quadraticRoots(A, B, C);
            return ob;
        }

        public static double[] quadraticRoots(double A, double B, double C)
        {
            int signB = 1;
            if (B < 0.0)
            {
                signB = -1;
            }

            double sqrt = Math.Sqrt((B * B) - (4 * A * C));
            double q = -0.5 * (B + (signB * sqrt));

            double[] roots = new double[2];
            roots[0] = q / A;
            roots[1] = C / q;
            return roots;
        }

        public static double[] CreateInverseProbabilityDistribution(int length)
        {
            double[] distribution = new double[length];
            for (int i = 0; i < length; i++)
            {
                distribution[i] = 1 / (double)(i + 1);
            }

            // for (int i = 0; i < length; i++) distribution[i] = 1 / (double)((i + 1) * (i + 1));
            // double sum = 0;
            // for (int i = 0; i < length; i++) sum += distribution[i];
            // Console.WriteLine("pre-sum = {0:f3}", sum);
            distribution = DataTools.Normalise2Probabilites(distribution);
            return distribution;
        }

        public static double[] CreateQuadraticProbabilityDistribution(int length)
        {
            double[] distribution = new double[length];
            for (int i = 0; i < length; i++)
            {
                distribution[i] = i * i;
            }

            // double sum = 0;
            // for (int i = 0; i < length; i++) sum += distribution[i];
            // Console.WriteLine("pre-sum = {0:f3}", sum);
            distribution = DataTools.Normalise2Probabilites(distribution);
            distribution = DataTools.reverseArray(distribution);
            return distribution;
        }

        public static Tuple<int[], int[]> RandomSamplingUsingProbabilityDistribution(int distributionlength, int sampleCount, int seed)
        {
            double[] distribution = CreateInverseProbabilityDistribution(distributionlength);

            // double[] distribution = Statistics.CreateQuadraticProbabilityDistribution(distributionlength);
            // double sum = distribution.Sum();
            // Console.WriteLine("post-sum = {0:f3}", sum);
            distribution = DataTools.ConvertProbabilityDistribution2CummulativeProbabilites(distribution);

            // sum = distribution.Sum();
            // Console.WriteLine("cumm-sum = {0:f3}", sum);
            // Console.WriteLine("cumm-sum = {0:f3}", distribution[length-1]);

            // double refValue = 0.65;
            // int lowerIndex = 0;
            // int upperIndex = 99;
            // int location = DataTools.WhichSideOfCentre(distribution, refValue, lowerIndex, upperIndex);
            int[] samples = DataTools.SampleArrayRandomlyWithoutReplacementUsingProbabilityDistribution(distribution, sampleCount, seed);
            Tuple<int[], int[]> tuple = DataTools.SortArray(samples);
            int[] sortedSamples = tuple.Item2;
            return Tuple.Create(samples, sortedSamples);
        }

        /// <summary>
        /// This method is a test for ensuring correct bin ID is chosen for some degenerate cases.
        /// </summary>
        public static void TestGetNthPercentileBin()
        {
            int[] distribution = new int[100];

            // distribution[0] = 1;
            // distribution[99] = 1;

            for (int i = 0; i < 100; i++)
            {
                // distribution[i] = 0;
                distribution[i] = 1;
            }

            var binId = GetNthPercentileBin(distribution, 98);
        }
    }
}