using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
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
  int binCount;
  int barWidth = 1;  // int equivalent of binWidth below
  int[] posDistribution;
  int[] negDistribution;
  int valueCount = 0; // number of values in Distribution
  int minIndex   = Int32.MaxValue; // index of lowest bin containing a value
  int maxIndex   = Int32.MinValue;
  
  
  
  // all these variables below were part of the original class
  double[] prob;
  double[] IC;   // info content of each value
  double totalIC = 0.0;
  double expectedIC = 0.0;//assuming uniform distribution
  int min = 0;
  int max = 0;
  
  // vars for real valued distribution
  double minD = 0.0;
  double maxD = 0.0;
  double binWidth = 1.0;

  
  /// <summary>
  /// CONSTRUCTOR
  /// The minimum integer is assumed to be zero
  /// </summary>
  /// <param name="binCount"></param>
  /// <param name="binWidth"></param>    
  public Distribution(int binCount, int binWidth) 
  { this.binCount   = binCount; 
    this.barWidth   = binWidth;
    posDistribution = new int[binCount];
    negDistribution = new int[binCount];
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
    int size   = max +1; // allow for a zero position
    double[] freq = new double[size];
    
    //make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i=0; i<counts; i++) freq[values[i]]++; 
    
    prob = normalise(freq);  //calculate probabilities
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
    int size   = max +1; // allow for a zero position
    double[] freq = new double[size];
    
    for (int i=0; i<size; i++) freq[i] = 0.0; //initialise
    //  make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i=0; i<counts; i++) freq[values[i]] += 1.0; 

    // regularise, smooth and normalise
    if(regularise) freq = regulariseDistribution(freq); 
    //prob = freq;
    prob = normalise(freq);
    calculateIC();
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
    int size   = (int)((maxD-minD)/binWidth) + 1; 
    double[] freq = new double[size];
    
    for (int i=0; i<size; i++) freq[i] = 0.0; //initialise
    //  make histogram: value=1 goes in histo at index=1, etc etc.
    for (int i=0; i<counts; i++)
    { if(values[i] < min) continue;
      if(values[i] > max) continue;
      
      double range = values[i] - min;
      int index = (int)(range / binWidth);
      freq[index] += 1.0; 
    }

    // regularise, smooth and normalise
    if(regularise) freq = regulariseDistribution(freq); 
    //prob = freq;
    prob = normalise(freq);
    calculateIC();
  }
  

  static public double[] regulariseDistribution(double[] values)
  {
    for (int i=0; i<values.Length; i++) values[i] += 0.01; // regularise
    return DataTools.filterMovingAverage(values,7);        //smooth
  }
  
  
  static public double[] normalise(double[] values) // normalise
  { int    length = values.Length;
    double[] prob = new double[length];
    double    sum = 0.0;
    for (int i=0; i<length; i++) sum += values[i];        // sum all values
    for (int i=0; i<length; i++) prob[i] = values[i]/sum; // get probs
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
    expectedIC = -Math.Log(1/(double)prob.Length) /log2;  //calc expected info content
    IC = calculateIC(prob);
    totalIC = getSum(IC);
  }
  
  
  
  /**
   * returns the probability of a discrete variable having
   * the passed value. 
   * @param value
   * @return
   */
  public double getProbability(int value)
  { if(value < min) return 0.0;
    if(value > max) return 0.0;
    return prob[value];
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
    for (int i=start; i<= end; i++) 
    { if(i < 0) continue;
      if(i > max) break; 
      sum += prob[i];
    }
    return sum;
  }
  
  
  public double getLogProbability(int value)
  { return Math.Log(prob[value])/Math.Log(2);
  }

  
  public double getLogProbability(int start, int end)
  { double p = getProbability(start, end);
    return Math.Log(p)/Math.Log(2);
  }

  public double[] getProbDistribution()
  { 
    return prob;
  }
  
  public double[] getInfoDistribution()
  { 
    return IC;
  }

  
  public double getSum()
  { 
    double sum = 0.0;
    for (int i=0; i< prob.Length; i++) 
    { 
      sum += prob[i];
    }
    return sum;
  }
  
  static public double getSum(double[] values)
  { 
    double sum = 0.0;
    for (int i=0; i< values.Length; i++) 
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
    for (int i=0; i< length; i++) 
    { 
      ic[i] = -p[i] *Math.Log(p[i]) /log2;
    }
    return ic;
  }
  
  /**
   *  write info content for each positon in array.
   *
   */
  public void writeIC()
  { 
    for (int i=0; i< IC.Length; i++) LoggedConsole.WriteLine(i+"   "+IC[i]);
  }
  
  
	/**
	 * @return Returns the expectedIC.
	 */
	public double getExpectedIC()
	{
		return expectedIC;
	}
	/**
	 * @return Returns the max.
	 */
	public int getMax()
	{
		return max;
	}
	/**
	 * @return Returns the min.
	 */
	public int getMin()
	{
		return min;
	}
	/**
	 * @return Returns the totalIC.
	 */
	public double getTotalIC()
	{
		return totalIC;
	}
  

  
//##########################################################################
  //  METHODS BELOW ADDED IN APRIL 2006.
  
  
  public void addValue(int value)
  { if(value < 0) addNegativeValue(value);
    else          addPositiveValue(value);
    valueCount ++;
  }
  public void addPositiveValue(int value)
  { 
    int index = value / barWidth;
    if(index >= binCount) index = binCount-1;
    posDistribution[index]++;
    if(index<minIndex) minIndex = index;
    if(index>maxIndex) maxIndex = index;
  }
  public void addNegativeValue(int value)
  { 
    int index = -(value / barWidth);// make values positive for storage purposes
    if(index >= binCount) index = binCount-1;
    negDistribution[index]++;
    if(index<minIndex) minIndex = index;
    if(index>maxIndex) maxIndex = index;
  }
  
  
  
  
	/**
	 * @return Returns the maxIndex.
	 */
	public int getMaxIndex()
	{
		return maxIndex;
	}
	/**
	 * @return Returns the minIndex.
	 */
	public int getMinIndex()
	{
		return minIndex;
	}
	/**
	 * @return Returns the valueCount.
	 */
	public int getValueCount()
	{
		return valueCount;
	}
  private int[] concatenatePosAndNeg()
  {
    int[] combinedDist = new int[binCount*2];
    for(int i=0; i<binCount; i++) // add in negative bins
    {
      combinedDist[binCount-1-i] = negDistribution[i];
    }
    for(int i=0; i<binCount; i++) // add in positive bins
    {
      combinedDist[binCount+i] = posDistribution[i];
    }
    return  combinedDist;  
  }
  public int[] getDistribution()
  {
    return concatenatePosAndNeg();
  }
  public double[] getNormalisedDistribution()
  { 
    int[] combinedDist = concatenatePosAndNeg();
    return DataTools.NormaliseArea(combinedDist);    
  }
  
  
  public static void main(String[] args)
  { 
    int max = 9;
    int[] var = {0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7,8,8,9,9};
    Distribution d = new Distribution(var, max, true);
    
    for(int i=-3; i<= +20; i++)
      LoggedConsole.WriteLine("p("+i+")="+d.getProbability(i));
    LoggedConsole.WriteLine("Sum="+d.getSum());
    LoggedConsole.WriteLine("Sum="+d.getProbability(-3,40));
    LoggedConsole.WriteLine("TotalIC="+d.getTotalIC());
    LoggedConsole.WriteLine("ExpecIC="+d.getExpectedIC());
  }


  
}// end of class
}
