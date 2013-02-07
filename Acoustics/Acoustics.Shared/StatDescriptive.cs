// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatDescriptive.cs" company="MQUTeR">
//  From: http://www.codeproject.com/KB/recipes/DescriptiveStatisticClass.aspx
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.Shared
{
    using System;

    /// <summary>
    /// The result class the holds the analysis results.
    /// </summary>
    /// <remarks>
    /// From: http://www.codeproject.com/KB/recipes/DescriptiveStatisticClass.aspx
    /// </remarks>
    public class StatDescriptiveResult
    {
        /// <summary>
        /// Count.
        /// </summary>
        public uint Count;

        /// <summary>
        /// First quartile, at 25 percentile.
        /// </summary>
        public double FirstQuartile;

        /// <summary>
        /// Geometric mean.
        /// </summary>
        public double GeometricMean;

        /// <summary>
        /// Harmonic mean.
        /// </summary>
        public double HarmonicMean;

        /// <summary>
        /// Interquartile range.
        /// </summary>
        public double IQR;

        /// <summary>
        /// Kurtosis of the data distribution.
        /// </summary>
        public double Kurtosis;

        /// <summary>
        /// Maximum value.
        /// </summary>
        public double Max;

        /// <summary>
        /// Arithmatic mean.
        /// </summary>
        public double Mean;

        /// <summary>
        /// Median, or second quartile, or at 50 percentile.
        /// </summary>
        public double Median;

        /// <summary>
        /// Minimum value.
        /// </summary>
        public double Min;

        /// <summary>
        /// The range of the values.
        /// </summary>
        public double Range;

        /// <summary>
        /// Skewness of the data distribution.
        /// </summary>
        public double Skewness;

        /// <summary>
        /// Sample standard deviation.
        /// </summary>
        public double StdDev;

        /// <summary>
        /// Sum.
        /// </summary>
        public double Sum;

        /// <summary>
        /// Sum of Error.
        /// </summary>
        internal double SumOfError;

        /// <summary>
        /// The sum of the squares of errors.
        /// </summary>
        internal double SumOfErrorSquare;

        /// <summary>
        /// Third quartile, at 75 percentile.
        /// </summary>
        public double ThirdQuartile;

        /// <summary>
        /// Sample variance.
        /// </summary>
        public double Variance;

        /// <summary>
        /// SortedData is used to calculate percentiles
        /// </summary>
        internal double[] sortedData;

        /// <summary>
        /// Calcuate Percentile.
        /// </summary>
        /// <param name="percent">
        /// Pecentile, between 0 to 100.
        /// </param>
        /// <returns>
        /// Calcuated Percentile.
        /// </returns>
        public double Percentile(double percent)
        {
            return StatDescriptive.Percentile(this.sortedData, percent);
        }

        public override string ToString()
        {
            return
                "Count: " + this.Count + ", " +
                "FirstQuartile: " + this.FirstQuartile + ", " +
                "GeometricMean: " + this.GeometricMean + ", " +
                "HarmonicMean: " + this.HarmonicMean + ", " +
                "IQR: " + this.IQR + ", " +
                "Kurtosis: " + this.Kurtosis + ", " +
                "Max: " + this.Max + ", " +
                "Mean: " + this.Mean + ", " +
                "Median: " + this.Median + ", " +
                "Min: " + this.Min + ", " +
                "Range: " + this.Range + ", " +
                "Skewness: " + this.Skewness + ", " +
                "StdDev: " + this.StdDev + ", " +
                "Sum: " + this.Sum + ", " +
                "SumOfError: " + this.SumOfError + ", " +
                "SumOfErrorSquare: " + this.SumOfErrorSquare + ", " +
                "ThirdQuartile: " + this.ThirdQuartile + ", " +
                "Variance: " + this.Variance + ", ";
        }
    }

    /// <summary>
    /// Descriptive class.
    /// </summary>
    /// <remarks>
    /// From: http://www.codeproject.com/KB/recipes/DescriptiveStatisticClass.aspx
    /// </remarks>
    public class StatDescriptive
    {
        private readonly double[] data;

        /// <summary>
        /// Descriptive results.
        /// </summary>
        public StatDescriptiveResult Result = new StatDescriptiveResult();

        private double[] sortedData;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatDescriptive"/> class. 
        /// Descriptive analysis default constructor.
        /// </summary>
        public StatDescriptive()
        {
        }

        // default empty constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StatDescriptive"/> class. 
        /// Descriptive analysis constructor.
        /// </summary>
        /// <param name="dataVariable">
        /// Data array.
        /// </param>
        public StatDescriptive(double[] dataVariable)
        {
            this.data = dataVariable;
        }

        #endregion //  Constructors

        /// <summary>
        /// Run the analysis to obtain descriptive information of the data.
        /// </summary>
        public void Analyze()
        {
            // initializations
            this.Result.Count = 0;
            this.Result.Min =
                this.Result.Max =
                this.Result.Range =
                this.Result.Mean = this.Result.Sum = this.Result.StdDev = this.Result.Variance = 0.0d;

            double sumOfSquare = 0.0d;
            double sumOfESquare = 0.0d; // must initialize

            var squares = new double[this.data.Length];
            double cumProduct = 1.0d; // to calculate geometric mean
            double cumReciprocal = 0.0d; // to calculate harmonic mean

            // First iteration
            for (int i = 0; i < this.data.Length; i++)
            {
                if (i == 0)
                {
                    // first data point
                    this.Result.Min = this.data[i];
                    this.Result.Max = this.data[i];
                    this.Result.Mean = this.data[i];
                    this.Result.Range = 0.0d;
                }
                else
                {
                    // not the first data point
                    if (this.data[i] < this.Result.Min)
                    {
                        this.Result.Min = this.data[i];
                    }

                    if (this.data[i] > this.Result.Max)
                    {
                        this.Result.Max = this.data[i];
                    }
                }

                this.Result.Sum += this.data[i];
                squares[i] = Math.Pow(this.data[i], 2); // TODO: may not be necessary
                sumOfSquare += squares[i];

                cumProduct *= this.data[i];
                cumReciprocal += 1.0d / this.data[i];
            }

            this.Result.Count = (uint)this.data.Length;
            double n = this.Result.Count; // use a shorter variable in double type
            this.Result.Mean = this.Result.Sum / n;
            this.Result.GeometricMean = Math.Pow(cumProduct, 1.0 / n);
            this.Result.HarmonicMean = 1.0d / (cumReciprocal / n); // see http://mathworld.wolfram.com/HarmonicMean.html
            this.Result.Range = this.Result.Max - this.Result.Min;

            // second loop, calculate Stdev, sum of errors
            // double[] eSquares = new double[data.Length];
            double m1 = 0.0d;
            double m2 = 0.0d;
            double m3 = 0.0d; // for skewness calculation
            double m4 = 0.0d; // for kurtosis calculation

            // for skewness
            for (int i = 0; i < this.data.Length; i++)
            {
                double m = this.data[i] - this.Result.Mean;
                double mPow2 = m * m;
                double mPow3 = mPow2 * m;
                double mPow4 = mPow3 * m;

                m1 += Math.Abs(m);

                m2 += mPow2;

                // calculate skewness
                m3 += mPow3;

                // calculate skewness
                m4 += mPow4;
            }

            this.Result.SumOfError = m1;
            this.Result.SumOfErrorSquare = m2; // Added for Excel function DEVSQ
            sumOfESquare = m2;

            // var and standard deviation
            this.Result.Variance = sumOfESquare / ((double)this.Result.Count - 1);
            this.Result.StdDev = Math.Sqrt(this.Result.Variance);

            // using Excel approach
            double skewCum = 0.0d; // the cum part of SKEW formula
            for (int i = 0; i < this.data.Length; i++)
            {
                skewCum += Math.Pow((this.data[i] - this.Result.Mean) / this.Result.StdDev, 3);
            }

            this.Result.Skewness = n / (n - 1) / (n - 2) * skewCum;

            // kurtosis: see http://en.wikipedia.org/wiki/Kurtosis (heading: Sample Kurtosis)
            double m2_2 = Math.Pow(sumOfESquare, 2);
            this.Result.Kurtosis = ((n + 1) * n * (n - 1)) / ((n - 2) * (n - 3)) * (m4 / m2_2) -
                                   3 * Math.Pow(n - 1, 2) / ((n - 2) * (n - 3)); // second last formula for G2

            // calculate quartiles
            this.sortedData = new double[this.data.Length];
            this.data.CopyTo(this.sortedData, 0);
            Array.Sort(this.sortedData);

            // copy the sorted data to result object so that
            // user can calculate percentile easily
            this.Result.sortedData = new double[this.data.Length];
            this.sortedData.CopyTo(this.Result.sortedData, 0);

            this.Result.FirstQuartile = Percentile(this.sortedData, 25);
            this.Result.ThirdQuartile = Percentile(this.sortedData, 75);
            this.Result.Median = Percentile(this.sortedData, 50);
            this.Result.IQR = Percentile(this.sortedData, 75) - Percentile(this.sortedData, 25);
        }

        /// <summary>
        /// Calculate percentile of a sorted data set.
        /// </summary>
        /// <param name="sortedData">Sorted Data used to calculate percentile.
        /// </param>
        /// <param name="p">Percentile (1 - 100) to calculate.
        /// </param>
        /// <returns>
        /// The percentile.
        /// </returns>
        internal static double Percentile(double[] sortedData, double p)
        {
            // algo derived from Aczel pg 15 bottom
            if (p >= 100.0d)
            {
                return sortedData[sortedData.Length - 1];
            }

            double position = (sortedData.Length + 1) * p / 100.0;
            double leftNumber = 0.0d, rightNumber = 0.0d;

            double n = p / 100.0d * (sortedData.Length - 1) + 1.0d;

            if (position >= 1)
            {
                leftNumber = sortedData[(int)Math.Floor(n) - 1];
                rightNumber = sortedData[(int)Math.Floor(n)];
            }
            else
            {
                leftNumber = sortedData[0]; // first data
                rightNumber = sortedData[1]; // first data
            }

            if (leftNumber == rightNumber)
            {
                return leftNumber;
            }
            else
            {
                double part = n - Math.Floor(n);
                return leftNumber + part * (rightNumber - leftNumber);
            }
        }

        /// <summary>
        /// Get the greatest common divisor of two numbers.
        /// </summary>
        /// <param name="a">
        /// First number.
        /// </param>
        /// <param name="b">
        /// Second number.
        /// </param>
        /// <returns>
        /// Greatest common divisor.
        /// </returns>
        public static long GreatestCommonDivisior(long a, long b)
        {
            long temp;
            while (b != 0)
            {
                temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Get the lowest common multiple of two numbers.
        /// </summary>
        /// <param name="a">
        /// First number.
        /// </param>
        /// <param name="b">
        /// Second number.
        /// </param>
        /// <returns>
        /// Lowest common multiple.
        /// </returns>
        public static long LowestCommonMultiple(long a, long b)
        {
            long ret = 0, temp = GreatestCommonDivisior(a, b);
            if (temp != 0)
            {
                if (b > a)
                {
                    ret = (b / temp) * a;
                }
                else
                {
                    ret = (a / temp) * b;
                }
            }
            return ret;
        }
    }
}