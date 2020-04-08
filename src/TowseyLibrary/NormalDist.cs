// <copyright file="NormalDist.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NormalDist
    {
        private readonly double av;        //mean of the data
        private readonly double sd;        //SD of the data
        private readonly double[,] bins;   //histogram of distribution over 16 bins about the mean
        private readonly int totalCount;   //number of samples
        private readonly double min;       //min and max of data
        private readonly double max;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalDist"/> class.
        /// CONSTRUCTOR for integer data.
        /// </summary>
        public NormalDist(int[] data)
        {
            this.totalCount = data.Length;
            AverageAndSD(data, out var average, out var stdev);
            this.av = average;
            this.sd = stdev;

            //convert data to doubles
            double[] doubleData = new double[this.totalCount];
            for (int i = 0; i < this.totalCount; i++)
            {
                doubleData[i] = data[i];
            }

            this.bins = get16binDistribution(doubleData, this.av, this.sd);

            DataTools.MinMax(doubleData, out this.min, out this.max);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalDist"/> class.
        /// CONSTRUCTOR for real valued data.
        /// </summary>
        public NormalDist(double[] data)
        {
            this.totalCount = data.Length;
            AverageAndSD(data, out var average, out var stdev);
            this.av = average;
            this.sd = stdev;
            this.bins = get16binDistribution(data, average, stdev);
            DataTools.MinMax(data, out this.min, out this.max);
        }

        public NormalDist(double[] data, double AV, double SD)
        {
            this.totalCount = data.Length;
            this.av = AV;
            this.sd = SD;
            this.bins = get16binDistribution(data, this.av, SD);
            DataTools.MinMax(data, out this.min, out this.max);
        }

        //=============================================================================

        /**
         * returns the histogram bins containing data distribution
         * @return
         */
        public double[,] getBins()
        {
            return this.bins;
        }

        /**
         * returns the sum of a set of integer values
         * @param data
         * @return
         */
        public static int getSum(int[] data)
        {
            int N = data.Length;
            int sum = 0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }

            return sum;
        }

        public static double[] convert2ZScores(int[] scores, double av, double sd)
        {
            int length = scores.Length;
            double[] zs = new double[length];
            for (int i = 0; i < length; i++)
            {
                zs[i] = (scores[i] - av) / sd;
            }

            return zs;
        }

        public static double[] Convert2ZScores(double[] scores)
        {
            int length = scores.Length;
            double[] zs = new double[length];

            AverageAndSD(scores, out var av, out var sd);

            for (int i = 0; i < length; i++)
            {
                zs[i] = (scores[i] - av) / sd;
            }

            return zs;
        }

        /**
         * returns the average and SD of a set of integer values
         * @param data
         * @return
         */
        public static void AverageAndSD(int[] data, out double av, out double sd)
        {
            int N = data.Length;
            int sum = 0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }

            if (N == 0)
            {
                av = 0.0;
            }

            //else if (N == 1) av = sum;
            else
            {
                av = sum / (double)N;
            }

            double var = 0.0;
            for (int i = 0; i < N; i++)
            {
                double diff = data[i] - av;
                var += diff * diff;
            }

            if (N > 30)
            {
                var /= N;
            }
            else
                if (N > 2)
            {
                var /= N - 1;
            }
            else
            {
                var = 0.0;
            }

            sd = Math.Sqrt(var);

            //LoggedConsole.WriteLine("av="+av+" SD="+SD);
            //LoggedConsole.WriteLine("VAR="+var+" SD="+SD+"  N="+N);
        }

        public static void AverageAndSD(List<int> data, out double av, out double sd)
        {
            AverageAndSD(data.ToArray(), out av, out sd);
        }

        /**
         * returns the average and SD of a set of real values
         * @param data
         * @return
         */
        public static void AverageAndSD(double[] data, out double av, out double sd)
        {
            AverageAndVariance(data, out av, out var var);
            sd = Math.Sqrt(var);
        }

        /**
         * returns the average and variance of a set of real values
         * @param data
         * @return
         */
        public static void AverageAndVariance(double[] data, out double av, out double variance)
        {
            int N = data.Length;
            double sum = 0.0;
            for (int i = 0; i < N; i++)
            {
                sum += data[i];
            }

            av = sum / N;

            variance = 0.0;
            for (int i = 0; i < N; i++)
            {
                variance += (data[i] - av) * (data[i] - av);
            }

            if (N > 30)
            {
                variance /= N;
            }
            else
                if (N > 2)
            {
                variance /= N - 1;
            }
            else
            {
                variance = 0.0;
            }

            //LoggedConsole.WriteLine("av="+av+" SD="+SD);
        }

        public static void AverageAndSD(List<double> data, out double av, out double sd)
        {
            AverageAndSD(data.ToArray(), out av, out sd);
        }

        public static void AverageAndSD(double[,] data, out double av, out double sd)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);

            double[] values = new double[rows * cols];
            int id = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    values[id++] = data[i, j];
                }
            }

            AverageAndSD(values, out av, out sd);
        }

        /// <summary>
        /// This was written for the local contrast normalisation (LCN) of amplitude spectrograms.
        /// However the contrast is calculated wrt the local part of frequency bin or column.
        /// Plugging up of ends of the returned array is a hack but it does not really matter.
        /// </summary>
        public static double[] CalculateLocalVariance(double[] data, int window)
        {
            int L = data.Length;
            int halfwindow = window / 2;
            double[] variances = new double[L];

            for (int i = 0; i <= L - window; i++)
            {
                double[] subV = DataTools.Subarray(data, i, window);
                AverageAndVariance(subV, out var av, out var variance);
                variances[i + halfwindow] = variance;
            }

            // plug up the ends
            for (int i = 0; i < halfwindow; i++)
            {
                variances[i] = variances[halfwindow];
                variances[L - i - 1] = variances[L - halfwindow - 1];
            }

            return variances;
        }

        public static double[] Normalise(double[] data)
        {
            AverageAndSD(data, out var av, out var sd);
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = (data[i] - av) / sd;
            }

            return ndata;
        }

        /**
         * Variance = (av of squares) - (square of the average)
         * i.e.  SS/n - mean^2
         *    =  SS/n - (sum/n)^2
         *    = (SS*n - sum^2)/n^2
         * @param SumSq
         * @param Sum
         * @param count
         * @return
         */
        public static double Variance(double SumSq, double Sum, int count)
        {
            double var = (count * SumSq) - (Sum * Sum);
            var /= count * (count - 1);
            return var;
        }

        public static double SD(double SumSq, double Sum, int count)
        {
            double var = Variance(SumSq, Sum, count);
            return Math.Sqrt(var);
        }

        public static double[] bias(double[] data, double bias)
        {
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = data[i] + bias;
            }

            return ndata;
        }

        public static double[] threshold(double[] data, double th)
        {
            double[] ndata = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                ndata[i] = data[i] - th;
                if (ndata[i] < 0.0)
                {
                    ndata[i] = 0.0000;
                }
            }

            return ndata;
        }

        /// <summary>
        /// Converts an array of values (assumed to be a signal superimposed on Gaussian noise)
        /// to z-scores and then converts z-scores to probabilites.
        /// </summary>
        /// <param name="values">array of score values.</param>
        /// <returns>array of probability scores.</returns>
        public static double[] Values2Probabilities(double[] values)
        {
            //get mode and std dev of the background variation
            double sd = 0.0;
            double mode = 0.0;

            //NormalDist.AverageAndSD(tsdScore, out mode, out sdBG);
            DataTools.ModalValue(values, out mode, out sd);

            //calculate a threshold using 3 standard deviations;
            double threshold1 = mode - (2.31 * sd); //area =  1%
            double threshold2 = mode - (1.64 * sd); //area =  5%
            double threshold3 = mode - (1.48 * sd); //area =  7%
            double threshold4 = mode - (1.28 * sd); //area = 10%
            double threshold5 = mode - (1.00 * sd); //area = 16%
            double threshold6 = mode - (0.52 * sd); //area = 30%
            double threshold7 = mode - (0.25 * sd); //area = 40%
            if (threshold1 < 0.0)
            {
                threshold1 = 0.0;
            }

            if (threshold2 < 0.0)
            {
                threshold2 = 0.0;
            }

            if (threshold3 < 0.0)
            {
                threshold3 = 0.0;
            }

            if (threshold4 < 0.0)
            {
                threshold4 = 0.0;
            }

            if (threshold5 < 0.0)
            {
                threshold5 = 0.0;
            }

            if (threshold6 < 0.0)
            {
                threshold6 = 0.0;
            }

            if (threshold7 < 0.0)
            {
                threshold7 = 0.0;
            }

            //double threshold7 = avBG - (0.50 * sdBG);

            int L = values.Length;
            var op = new double[L];
            for (int i = 0; i < L; i++)
            {
                if (values[i] < threshold1)
                {
                    op[i] = 0.99;  // = (1.00-0.5)*2
                }
                else
                    if (values[i] < threshold2)
                {
                    op[i] = 0.90; // = (0.95-0.5)*2
                }
                else
                        if (values[i] < threshold3)
                {
                    op[i] = 0.86; // = (0.93-0.5)*2
                }
                else
                            if (values[i] < threshold4)
                {
                    op[i] = 0.80; // = (0.90-0.5)*2
                }
                else
                                if (values[i] < threshold5)
                {
                    op[i] = 0.68; // = (0.84-0.5)*2
                }
                else
                                    if (values[i] < threshold6)
                {
                    op[i] = 0.40; // = (0.70-0.5)*2
                }
                else
                                        if (values[i] < threshold7)
                {
                    op[i] = 0.20; // = (0.60-0.5)*2
                }
                else
                                            if (values[i] >= mode)
                {
                    op[i] = 0.00;  // = (0.50-0.5)*2
                }
                else
                {
                    op[i] = 0.0;
                }
            }

            return op;
        }

        /**
         * returns the upper limit for each bin in a 16 bin histogram
         * from an array of data given the data's
         * average and standard deviation.
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static double[] getBinLimits(double av, double SD)
        {
            double[] limits = new double[16];
            double halfSD = SD / 2;

            //set upper limits for 15 bins. bin 16 holds highest values.
            limits[0] = av - (3 * SD) - halfSD;
            limits[1] = av - (3 * SD);
            limits[2] = av - (2 * SD) - halfSD;
            limits[3] = av - (2 * SD);
            limits[4] = av - SD - halfSD;
            limits[5] = av - SD;
            limits[6] = av - halfSD;
            limits[7] = av;
            limits[8] = av + halfSD;
            limits[9] = av + SD;
            limits[10] = av + SD + halfSD;
            limits[11] = av + (2 * SD);
            limits[12] = av + (2 * SD) + halfSD;
            limits[13] = av + (3 * SD);
            limits[14] = av + (3 * SD) + halfSD;
            limits[15] = av + (100 * SD); // upper bound to very large number
            return limits;
        }

        /**
         * returns an 8 bin histogram from an array of data given the data's
         * average and standard deviation.
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static int[] get8binDistribution(double[] data, double av, double SD)
        {
            int[] dist = new int[8];
            for (int i = 0; i < 8; i++)
            {
                dist[i] = 0; // initialise array
            }

            //set upper limits for 7 bins. bin8 holds highest values.
            double bin1 = av - (3 * SD);
            double bin2 = av - (2 * SD);
            double bin3 = av - SD;
            double bin4 = av;
            double bin5 = av + SD;
            double bin6 = av + (2 * SD);
            double bin7 = av + (3 * SD);

            // loop through data and put values in bins.
            for (int i = 0; i < data.Length; i++)
            {
                double dd = data[i];
                if (dd < bin1)
                {
                    dist[0]++;
                }
                else
                    if (dd < bin2)
                {
                    dist[1]++;
                }
                else
                        if (dd < bin3)
                {
                    dist[2]++;
                }
                else
                            if (dd < bin4)
                {
                    dist[3]++;
                }
                else
                                if (dd < bin5)
                {
                    dist[4]++;
                }
                else
                                    if (dd < bin6)
                {
                    dist[5]++;
                }
                else
                                        if (dd < bin7)
                {
                    dist[6]++;
                }
                else
                {
                    dist[7]++;
                }
            }

            return dist;
        }

        /**
         * returns a 16 bin histogram from an array of data given
         * the data's average and standard deviation.
         * The histogram is represented by a matrix of doubles.
         * One row represents data in one bin.
         * 5 columns are: 1 upper bound of bin
         *                2 midpoint of bin
         *                3 absolute counts in bin
         *                4 fraction of counts in bin
         *                5 ln(fraction)
         * @param data
         * @param av
         * @param SD
         * @return
         */
        public static double[,] get16binDistribution(double[] data, double av, double SD)
        {
            //get the upper bounds on the 16 bins
            //highest bin has no upper bound - therefore bounds.mapLength=15
            double[] bounds = getBinLimits(av, SD);

            // init array to collect counts
            int[] counts = new int[16];
            for (int c = 0; c < 16; c++)
            {
                counts[c] = 0;
            }

            // loop through data and collect the counts for each bin.
            for (int i = 0; i < data.Length; i++)
            {
                double dd = data[i];
                counts[getBin(dd, bounds)]++;
            }

            int totalCounts = data.Length;
            double delta = SD / 4;  //delta for midpoint of each bin.

            // init matrix to return
            double[,] dist = new double[16, 5];
            for (int r = 0; r < 16; r++)
            {
                dist[r, 0] = bounds[r];
                dist[r, 1] = bounds[r] - delta; //there is error on last midpoint
                dist[r, 2] = counts[r];
                dist[r, 3] = counts[r] / (double)totalCounts;

                // put ln(fraction) in fifth column
                if (dist[r, 3] == 0.0)
                {
                    dist[r, 4] = -1000.0;
                }
                else
                {
                    dist[r, 4] = Math.Log(dist[r, 3]);
                }
            }

            // correct error on last midpoint
            dist[15, 1] = bounds[14] + delta; //there is error on last midpoint

            return dist;
        }

        public static double[,] get16binDistribution(int[] data, double av, double SD)
        {
            int length = data.Length;
            double[] ddData = new double[length];

            // convert array of integers to array of doubles.
            for (int i = 0; i < data.Length; i++)
            {
                ddData[i] = data[i];
            }

            return get16binDistribution(ddData, av, SD);
        }

        private static int getBin(double dd, double[] bounds)
        {
            if (dd < bounds[0])
            {
                return 0;
            }
            else
                if (dd < bounds[1])
            {
                return 1;
            }
            else
                    if (dd < bounds[2])
            {
                return 2;
            }
            else
                        if (dd < bounds[3])
            {
                return 3;
            }
            else
                            if (dd < bounds[4])
            {
                return 4;
            }
            else
                                if (dd < bounds[5])
            {
                return 5;
            }
            else
                                    if (dd < bounds[6])
            {
                return 6;
            }
            else
                                        if (dd < bounds[7])
            {
                return 7;
            }
            else
                                            if (dd < bounds[8])
            {
                return 8;
            }
            else
                                                if (dd < bounds[9])
            {
                return 9;
            }
            else
                                                    if (dd < bounds[10])
            {
                return 10;
            }
            else
                                                        if (dd < bounds[11])
            {
                return 11;
            }
            else
                                                            if (dd < bounds[12])
            {
                return 12;
            }
            else
                                                                if (dd < bounds[13])
            {
                return 13;
            }
            else
                                                                    if (dd < bounds[14])
            {
                return 14;
            }
            else
            {
                return 15;
            }
        }

        public string write16binDistribution()
        {
            return write16binDistribution(this.bins);
        }

        public static string write16binDistribution(double[,] bins)
        {
            StringBuilder sb = new StringBuilder("NORMAL DISTRIBUTION (16 bins)\n");
            sb.Append("bin\tup_bnd\tmidpt\tcount\t\tfraction\tln\n");
            for (int r = 0; r < 16; r++)
            {
                if (r < 9)
                {
                    sb.Append(" ");
                }

                sb.Append(r + 1 + "\t");
                if (bins[r, 0] >= 0.0)
                {
                    sb.Append("+");
                }

                sb.Append(bins[r, 0].ToString("F2.2") + "\t");
                if (bins[r, 1] >= 0.0)
                {
                    sb.Append("+");
                }

                sb.Append(bins[r, 1].ToString("F2.2") + "\t");
                sb.Append(bins[r, 2].ToString("F0") + "\t\t");
                sb.Append(bins[r, 3].ToString("F3") + "\t\t");
                sb.Append(bins[r, 4].ToString("F3") + "\n");
            }

            return sb.ToString();
        }

        public string writeAllStatistics()
        {
            StringBuilder sb = new StringBuilder("   DISTRIBUTION STATISTICS \n");
            sb.Append("Total count=" + this.totalCount + "\n");
            sb.Append("Average = " + DataTools.roundDouble(this.av, 5));
            sb.Append(" +/- " + DataTools.roundDouble(this.sd, 5) + "\n");
            sb.Append("Min=" + this.min + "  Max=" + this.max + "\n");
            sb.Append(this.write16binDistribution());
            return sb.ToString();
        }

        public static void writeBinDistribution(int[] data, int binWidth)
        {
            DataTools.MinMax(data, out var min, out var max);
            int length = max / binWidth;

            int[] counts = new int[length + 1];
            for (int i = 0; i < data.Length; i++)
            {
                counts[data[i] / binWidth]++;
            }

            LoggedConsole.WriteLine("\n DISTRIBUTION");
            LoggedConsole.WriteLine("bin width = " + binWidth);
            LoggedConsole.WriteLine("geneStart \tend \tcount");
            for (int i = 0; i < counts.Length; i++)
            {
                LoggedConsole.WriteLine((i * binWidth) + " \t" + ((i + 1) * binWidth) + " \t" + counts[i]);
            }
        }

        public static void writeScoreDistribution(double[] scores)
        {
            DataTools.MinMax(scores, out var min, out var max);
            AverageAndSD(scores, out var av, out var sd);

            double[,] histo = get16binDistribution(scores, av, sd);
            LoggedConsole.WriteLine(" ===== SCORE STATISTICS =====");
            LoggedConsole.WriteLine("Average =" + av + "+/-" + sd);
            LoggedConsole.WriteLine("Min score =" + min + "  Max=" + max);
            LoggedConsole.WriteLine(write16binDistribution(histo));
            LoggedConsole.WriteLine(" =============================");
        }

        public static void writeScoreDistribution(int[] scores)
        {
            AverageAndSD(scores, out var av, out var sd);
            DataTools.MinMax(scores, out var min, out var max);
            double[,] histo = get16binDistribution(scores, av, sd);
            LoggedConsole.WriteLine(" ===== SCORE STATISTICS =====");
            LoggedConsole.WriteLine("Average =" + av + "+/-" + sd);
            LoggedConsole.WriteLine("Min score =" + min + "  Max=" + max);
            LoggedConsole.WriteLine(write16binDistribution(histo));
            LoggedConsole.WriteLine(" =============================");
        }

        public static void writeScoreStatistics(double[] scores)
        {
            AverageAndSD(scores, out var av, out var sd);
            DataTools.MinMax(scores, out var min, out var max);

            //double[,] histo = NormalDist.get16binDistribution(scores, av, sd);
            LoggedConsole.WriteLine(" ===== SCORE STATISTICS =====");
            LoggedConsole.WriteLine("Average =" + av + "+/-" + sd);
            LoggedConsole.WriteLine("Min score =" + min + "  Max=" + max);
            LoggedConsole.WriteLine(" =============================");
        }

        public static string formatAvAndSD(double[] avsd, int places)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(avsd[0].ToString("F4"));
            sb.Append("+/-");
            sb.Append(avsd[1].ToString("F4"));
            return sb.ToString();
        }

        public static double[] CalculateZscores(double[] scores, double av, double sd)
        {
            int length = scores.Length;
            double[] zscores = new double[length];
            for (int i = 0; i < length; i++)
            {
                zscores[i] = (scores[i] - av) / sd;
            }

            return zscores;
        }

        /**
         * returns the Z score (absolute value) of a value with reference to
         * a normal distribution whose av and sd are as passed.
         * @param value
         * @param av
         * @param sd
         * @return
         */
        public static double zScore(double value, double av, double sd)
        {
            return Math.Abs((value - av) / sd);
        }

        public static double zScore2pValue(double z)
        {
            //double[] table = { 1.2816, 1.6449, 1.9600, 2.3263, 2.5758, 3.0902, 3.7190, 4.26490 };
            //double[] alpha = { 0.1000, 0.0500, 0.0250, 0.0100, 0.0050, 0.0010, 0.0001, 0.00001 };
            double[] zTable = { 0.0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 2.0, 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 3.0, 3.1 };
            double[] alpha = { 0.5, 0.54, 0.5793, 0.6179, 0.6554, 0.6915, 0.7257, 0.7580, 0.7881, 0.8159, 0.8413, 0.8643, 0.8849, 0.9032, 0.9192, 0.9332, 0.9452, 0.9554, 0.9641, 0.9713, 0.9772, 0.9821, 0.9861, 0.9893, 0.9918, 0.9938, 0.9953, 0.9965, 0.9974, 0.9981, 0.9987, 0.9990 };
            double p = 0.5;
            for (int i = zTable.Length - 1; i >= 0; i--)
            {
                if (z > zTable[i])
                {
                    p = alpha[i];
                    break;
                }
            }

            return 2 - (2 * p); // convert alpha to probability event drawn from same population having given mean and SD.
        }

        public static void main(string[] args)
        {
            /*
                double[] roots = NormalDist.quadraticRoots(6, -13, 6);
                    LoggedConsole.WriteLine("root1="+roots[0]+"  root2="+roots[1]);
            */
        }
    }
}