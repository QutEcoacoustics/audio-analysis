// <copyright file="Distribution.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace TowseyLibrary
{
    using System;

    /*
 * Created on 15/03/2005
 * This class represents a distribution of values.
 * Convert to a curve whch returns the probability for any value.
 * The distribution is assumed to be discrete.
 *
 * THIS CLASS REWORKED 4/04/06
 * Add functionality whereby facilitate creation of the distribution
 * rather than just pass a set of pre-existing values
 *
 * Ported to C# in March 2008
 */

    public class Distribution
{
  //new functions - the arrays to hold the distribution
    private readonly int binCount;
    private readonly int barWidth = 1;  // int equivalent of binWidth below
    private readonly int[] posDistribution;
    private readonly int[] negDistribution;
    private int valueCount = 0; // number of values in Distribution
    private int minIndex = int.MaxValue; // index of lowest bin containing a value
    private int maxIndex = int.MinValue;

  // all these variables below were part of the original class
    private readonly double[] prob;
    private double[] IC;   // info content of each value
    private double totalIC = 0.0;
    private double expectedIC = 0.0; //assuming uniform distribution
    private readonly int min = 0;
    private readonly int max = 0;

  // vars for real valued distribution
    private readonly double minD = 0.0;
    private readonly double maxD = 0.0;
    private readonly double binWidth = 1.0;

  /// <summary>
  /// CONSTRUCTOR
  /// The minimum integer is assumed to be zero
  /// </summary>
  /// <param name="binCount"></param>
  /// <param name="binWidth"></param>
    public Distribution(int binCount, int binWidth)
  {
        this.binCount = binCount;
        this.barWidth = binWidth;
        this.posDistribution = new int[binCount];
        this.negDistribution = new int[binCount];
  }

  /**
   * CONSTRUCTOR
   * Passed an array of integers and a maximum possible integer value.
   * The minimum integer is assumed to be zero
   */
    public Distribution(int[] values, int max)
  {
    this.max = max;
    int counts = values.Length;
    int size = max + 1; // allow for a zero position
    double[] freq = new double[size];

    //make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i = 0; i < counts; i++)
            {
                freq[values[i]]++;
            }

    this.prob = normalise(freq);  //calculate probabilities
  }

  /**
   * CONSTRUCTOR
   * Passed an array of integers and a maximum possible integer value.
   * The minimum integer is assumed to be zero
   */
    public Distribution(int[] values, int max, bool regularise)
  {
    this.max = max;
    int counts = values.Length;
    int size = max + 1; // allow for a zero position
    double[] freq = new double[size];

    for (int i = 0; i < size; i++)
            {
                freq[i] = 0.0; //initialise
            }

            //  make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i = 0; i < counts; i++)
            {
                freq[values[i]] += 1.0;
            }

            // regularise, smooth and NormaliseMatrixValues
    if (regularise)
            {
                freq = regulariseDistribution(freq);
            }

            //prob = freq;
    this.prob = normalise(freq);
    this.calculateIC();
  }

  /**
   * CONSTRUCTOR
   * Passed an array of doubles, min & max possible values and binWidth.
   */
    public Distribution(double[] values, double min, double max,
                      double binWidth, bool regularise)
  {
    this.minD = min;
    this.maxD = max;
    int counts = values.Length;
    this.binWidth = binWidth;
    int size = (int)((this.maxD - this.minD) / binWidth) + 1;
    double[] freq = new double[size];

    for (int i = 0; i < size; i++)
            {
                freq[i] = 0.0; //initialise
            }

            //  make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i = 0; i < counts; i++)
    {
        if (values[i] < min)
                {
                    continue;
                }

        if (values[i] > max)
                {
                    continue;
                }

        double range = values[i] - min;
        int index = (int)(range / binWidth);
        freq[index] += 1.0;
    }

    // regularise, smooth and NormaliseMatrixValues
    if (regularise)
            {
                freq = regulariseDistribution(freq);
            }

            //prob = freq;
    this.prob = normalise(freq);
    this.calculateIC();
  }

    public static double[] regulariseDistribution(double[] values)
  {
    for (int i = 0; i < values.Length; i++)
            {
                values[i] += 0.01; // regularise
            }

    return DataTools.filterMovingAverage(values, 7);        //smooth
  }

    public static double[] normalise(double[] values) // NormaliseMatrixValues
  {
        int length = values.Length;
        double[] prob = new double[length];
        double sum = 0.0;
        for (int i = 0; i < length; i++)
            {
                sum += values[i];        // sum all values
            }

        for (int i = 0; i < length; i++)
            {
                prob[i] = values[i] / sum; // get probs
            }

        return prob;
  }

  /// <summary>
  /// calculates the information content
  /// </summary>
    public void calculateIC()
  {
//    double[] gaps = {0.108, 0.192, 0.464, 0.136, 0.10};
//    prob = gaps;
    double log2 = Math.Log(2.0);
    this.expectedIC = -Math.Log(1 / (double)this.prob.Length) / log2;  //calc expected info content
    this.IC = this.calculateIC(this.prob);
    this.totalIC = getSum(this.IC);
  }

  /**
   * returns the probability of a discrete variable having
   * the passed value.
   * @param value
   * @return
   */
    public double getProbability(int value)
  {
        if (value < this.min)
            {
                return 0.0;
            }

        if (value > this.max)
            {
                return 0.0;
            }

        return this.prob[value];
  }

  /**
   * returns the probability that a variable takes the value between
   * start and end, inclusive.
   * @param geneStart
   * @param geneEnd
   * @return
   */
    public double getProbability(int start, int end)
  {
    double sum = 0;
    for (int i = start; i <= end; i++)
    {
        if (i < 0)
                {
                    continue;
                }

        if (i > this.max)
                {
                    break;
                }

        sum += this.prob[i];
    }

    return sum;
  }

    public double getLogProbability(int value)
  {
        return Math.Log(this.prob[value]) / Math.Log(2);
  }

    public double getLogProbability(int start, int end)
  {
        double p = this.getProbability(start, end);
        return Math.Log(p) / Math.Log(2);
  }

    public double[] getProbDistribution()
  {
    return this.prob;
  }

    public double[] getInfoDistribution()
  {
    return this.IC;
  }

    public double getSum()
  {
    double sum = 0.0;
    for (int i = 0; i < this.prob.Length; i++)
    {
      sum += this.prob[i];
    }

    return sum;
  }

    public static double getSum(double[] values)
  {
    double sum = 0.0;
    for (int i = 0; i < values.Length; i++)
    {
      sum += values[i];
    }

    return sum;
  }

    public double[] calculateIC(double[] p)
  {
    double log2 = Math.Log(2.0);
    int length = p.Length;
    double[] ic = new double[length];

    // calc info content for each positon in array.
    for (int i = 0; i < length; i++)
    {
      ic[i] = -p[i] * Math.Log(p[i]) / log2;
    }

    return ic;
  }

  /**
   *  write info content for each positon in array.
   *
   */
    public void writeIC()
  {
    for (int i = 0; i < this.IC.Length; i++)
            {
                LoggedConsole.WriteLine(i + "   " + this.IC[i]);
            }
        }

    /**
     * @return Returns the expectedIC.
     */
    public double getExpectedIC()
    {
        return this.expectedIC;
    }

    /**
     * @return Returns the max.
     */
    public int getMax()
    {
        return this.max;
    }

    /**
     * @return Returns the min.
     */
    public int getMin()
    {
        return this.min;
    }

    /**
     * @return Returns the totalIC.
     */
    public double getTotalIC()
    {
        return this.totalIC;
    }

//##########################################################################
  //  METHODS BELOW ADDED IN APRIL 2006.

    public void addValue(int value)
  {
        if (value < 0)
            {
                this.addNegativeValue(value);
            }
            else
            {
                this.addPositiveValue(value);
            }

        this.valueCount++;
  }

    public void addPositiveValue(int value)
  {
    int index = value / this.barWidth;
    if (index >= this.binCount)
            {
                index = this.binCount - 1;
            }

    this.posDistribution[index]++;
    if (index < this.minIndex)
            {
                this.minIndex = index;
            }

    if (index > this.maxIndex)
            {
                this.maxIndex = index;
            }
        }

    public void addNegativeValue(int value)
  {
    int index = -(value / this.barWidth); // make values positive for storage purposes
    if (index >= this.binCount)
            {
                index = this.binCount - 1;
            }

    this.negDistribution[index]++;
    if (index < this.minIndex)
            {
                this.minIndex = index;
            }

    if (index > this.maxIndex)
            {
                this.maxIndex = index;
            }
        }

    /**
     * @return Returns the maxIndex.
     */
    public int getMaxIndex()
    {
        return this.maxIndex;
    }

    /**
     * @return Returns the minIndex.
     */
    public int getMinIndex()
    {
        return this.minIndex;
    }

    /**
     * @return Returns the valueCount.
     */
    public int getValueCount()
    {
        return this.valueCount;
    }

    private int[] concatenatePosAndNeg()
  {
    int[] combinedDist = new int[this.binCount * 2];
    for (int i = 0; i < this.binCount; i++) // add in negative bins
    {
      combinedDist[this.binCount - 1 - i] = this.negDistribution[i];
    }

    for (int i = 0; i < this.binCount; i++) // add in positive bins
    {
      combinedDist[this.binCount + i] = this.posDistribution[i];
    }

    return combinedDist;
  }

    public int[] getDistribution()
  {
    return this.concatenatePosAndNeg();
  }

    public double[] getNormalisedDistribution()
  {
    int[] combinedDist = this.concatenatePosAndNeg();
    return DataTools.NormaliseArea(combinedDist);
  }

    public static void main(string[] args)
  {
    int max = 9;
    int[] var = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9 };
    Distribution d = new Distribution(var, max, true);

    for (int i = -3; i <= +20; i++)
            {
                LoggedConsole.WriteLine("p(" + i + ")=" + d.getProbability(i));
            }

    LoggedConsole.WriteLine("Sum=" + d.getSum());
    LoggedConsole.WriteLine("Sum=" + d.getProbability(-3, 40));
    LoggedConsole.WriteLine("TotalIC=" + d.getTotalIC());
    LoggedConsole.WriteLine("ExpecIC=" + d.getExpectedIC());
  }
}// end of class
}
