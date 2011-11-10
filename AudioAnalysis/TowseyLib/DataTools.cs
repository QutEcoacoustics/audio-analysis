using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TowseyLib
{
    public class DataTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\"; 

        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");

            bool doit1 = false;
            if (doit1) //test Submatrix()
            {
                Console.WriteLine(""); 
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                //int rowCount = matrix.GetLength(0);//height
                //int colCount = matrix.GetLength(1);//width
                //Console.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                DataTools.writeMatrix(matrix);
                Console.WriteLine("");
                int r1 = 2;
                int c1 = 3;
                int r2 = 4;
                int c2 = 6;
                Console.WriteLine("r1="+r1+" c1="+c1+" r2="+r2+" c2="+c2);
                Console.WriteLine("Indices start at [0,0] in top left.");
                int smRows = r2 - r1 + 1;
                int smCols = c2 - c1 + 1;
                Console.WriteLine("Submatrix has " + smRows + " rows and " + smCols + " columns");
                double[,] sub = Submatrix(matrix, r1, c1, r2, c2);
                DataTools.writeMatrix(sub);
            }//end test ReadDoubles2Matrix(string fName)


            if (true) //test normalise(double[,] m, double normMin, double normMax)
            {   
                string fName = testDir + "testOfReadingMatrixFile.txt";
                double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                Console.WriteLine("\n");
                DataTools.writeMatrix(matrix);
                double normMin=-2.0;
                double normMax= 1.0;
                matrix = Normalise(matrix, normMin, normMax);
                Console.WriteLine("\n\n");
                DataTools.writeMatrix(matrix);
                matrix = Normalise(matrix, 0, 1);
                Console.WriteLine("\n\n");
                DataTools.writeMatrix(matrix);

            }//end test normalise(double[,] m, double normMin, double normMax)


            //COPY THIS TEST TEMPLATE
            if (false) //test Method(parameters)
            {
            }//end test Method(string fName)


            Console.WriteLine("FINISHED"); //end
            Console.WriteLine("FINISHED"); //end
        }//end of MAIN

        //***************************************************************************
        //***************************************************************************
        //***************************************************************************
        //***************************************************************************


        public static double AntiLogBase10(double value)
        {
            return Math.Exp(value * Math.Log(10));
        }

        public static double AntiLog(double value, double logBase)
        {
            return Math.Exp(value * Math.Log(logBase));
        }




        public static int[] Subarray(int[] A, int start, int length)
        {
            int end = start + length - 1;
            if(end >= A.Length)
            {
                Console.WriteLine("WARNING! DataTools.Subarray(): subarray extends to far.");
                return null;
            }
            int[] sa = new int[length];

            for (int i = 0; i < length; i++)
            {
                sa[i] = A[start+i];
            }
            return sa;
        }



        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// Assume that r1 < r2, c1 < c2. 
        /// Row, column indices start at 0
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static double[,] Submatrix(double[,] M, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 +1;
            int subColCount = c2 - c1 + 1;

            double[,] sm = new double[subRowCount, subColCount];

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {   sm[i,j] = M[r1+i,c1+j];
                }
            }
            return sm;
        }


        public static List<Double[]> RemoveNullElementsFromList(List<Double[]> list)
        {
            var newList = new List<Double[]>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null) newList.Add(list[i]);
            }
            return newList;
        }

        public static double[,] ConvertList2Matrix(List<double[]> list)
        {
            int rows = list.Count;
            int cols = list[0].Length; //assume all vectors in list are of same length
            double[,] op = new double[rows,cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    op[i, j] = list[i][j];
                }
            }
            return op;
        }

        /*
         * converts a matrix to a vector by concatenating columns.
         */
        public static double[] Matrix2Array(double[,] M)
        {
            int ht  = M.GetLength(0);
            int width     = M.GetLength(1);
            double[] v = new double[ht * width];

            int id = 0;
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < ht; row++)
                {   v[id++] = M[row,col];
                }
            }
            return v;
        }


        /*
         * converts a matrix of doubles to binary using passed threshold
         */
        public static double[,] Matrix2Binary(double[,] M, double threshold)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > threshold) op[r, c] = 1.0;
                }
            }
            return op;
        }


        public static double[,] TwoOfThree(double[,] m1, double[,] m2, double[,] m3)
        {
            int rows = m1.GetLength(0); //assume all matrices have same dimensions
            int cols = m2.GetLength(1);
            double[,] M = new double[rows, cols];

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    int count = 0;
                    if (m1[row, col] == 1.0) count++;
                    if (m2[row, col] == 1.0) count++;
                    if (m3[row, col] == 1.0) count++;
                    if (count >=2) M[row, col] = 1;
                }
            }
            return M;
        }


        /// <summary>
        /// returns the euclidian length of vector
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static double VectorLength(double[] V)
        {
            int length = V.Length;

            double d = 0.0;

            for (int i = 0; i < length; i++)
            {
                d += (V[i] * V[i]);
            }
            return Math.Sqrt(d);
        }


        public static double[] Vector_NormLength(double[] V)
        {
            double euclidLength = VectorLength(V);
            double[] Vnorm = new double[V.Length];

            for (int i = 0; i < V.Length; i++)
            {
                Vnorm[i] = V[i] / euclidLength;
            }

             
            // now that length = 1.0;
            double L = VectorLength(Vnorm);
            if (L > 1.00001) Console.WriteLine("WARNING:DataUtilities.Vector_NormLength() LENGTH=" + L);
            if (L < 0.99999) Console.WriteLine("WARNING:DataUtilities.Vector_NormLength() LENGTH=" + L);

            return Vnorm;
        }

        /// <summary>
        /// returns the vector normalised for min and max values
        /// </summary>
        /// <param name="V"></param>
        /// <returns></returns>
        public static double[] Vector_NormRange(double[] V)
        {
            double min;
            double max;
            MinMax(V, out min, out max);
            double range = max - min;
            
            double[] Vnorm = new double[V.Length];

            for (int i = 0; i < V.Length; i++)
            {
                Vnorm[i] = (V[i] - min) / range;
                //Console.WriteLine(i + ", " + Vnorm[i]);
            }
            return Vnorm;
        }

        /// <summary>
        /// subtracts the mean from each value of an array 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] SubtractMean(double[] v)
        {
            double mean = 0.0;
            for (int i = 0; i < v.Length; i++) mean += v[i];
            mean /= v.Length;
            double[] vOut = new double[v.Length];
            for (int i = 0; i < v.Length; i++) vOut[i] = v[i] - mean;
            return vOut;
        }





//=============================================================================


        public static double[] reverseArray(double[] V)
        {
            int L = V.Length;
            double[] newArray = new double[L];

            for (int i = 0; i < L; i++)
            {
                newArray[i] = V[L - i - 1];
            }
            return newArray;
        }
        public static int[] reverseArray(int[] V)
        {
            int L = V.Length;
            int[] newArray = new int[L];

            for (int i = 0; i < L; i++)
            {
                newArray[i] = V[L - i - 1];
            }
            return newArray;
        }


        /// <summary>
        /// bounds a sequence of numbers between a minimum and a maximum.
        /// Numbers that fall outside the bound are truncated to the bound.
        /// </summary>
        /// <param name="A">array to be bound</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns></returns>
        public static double[] boundArray(double[] A, double min, double max)
        {
            int L = A.Length;
            double[] newArray = new double[L];

            for (int i = 0; i < L; i++)
            {
                if     (A[i] < min) newArray[i] = min;
                else if(A[i] > max) newArray[i] = max;
                else                newArray[i] = A[i];
            }
            return newArray;
        }


        public static System.Tuple<int[], double[]> SortRowIDsByRankOrder(double[] array)
        {
            int[] rankOrder = new int[array.Length];
            double[] sort   = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                int maxIndex = DataTools.GetMaxIndex(array);
                rankOrder[i] = maxIndex;
                sort[i] = array[maxIndex];
                //if(i % 100==0)
                //    Console.WriteLine("{0}: {1}   {2:f2}", i, maxIndex, array[maxIndex]);
                array[maxIndex] = -Double.MaxValue;
            }
            return Tuple.Create(rankOrder, sort);
        }




        /// <summary>
        /// bounds a matrix of numbers between a minimum and a maximum.
        /// Numbers that fall outside the bound are truncated to the bound.
        /// </summary>
        /// <param name="M">the matrix to be bound</param>
        /// <param name="min">The minimum bound</param>
        /// <param name="max">The maximum bound</param>
        /// <returns></returns>
        public static double[,] boundMatrix(double[,] M, double min, double max)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] newM = new double[rows,cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    if (M[i, j] <= min) newM[i, j] = min;
                    else if (M[i,j] >= max) newM[i,j] = max;
                    else newM[i,j] = M[i,j];
                }
            return newM;
        }




        /// <summary>
        /// returns the min and max percentile values of the values in passed matrix.
        /// Must have previously calculated the min and max values in the matrix.
        /// </summary>
        /// <param name="matrix">the matrix</param>
        /// <param name="min">min power value in matrix</param>
        /// <param name="max">max power value in matrix</param>
        /// <param name="minPercentile"></param>
        /// <param name="maxPercentile"></param>
        /// <param name="minCut">power value equivalent to minPercentile</param>
        /// <param name="maxCut">power value equivalent to maxPercentile</param>
        public static void PercentileCutoffs(double[,] matrix, double minPercentile, double maxPercentile, out double minCut, out double maxCut)
        {
            if (maxPercentile < minPercentile) throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
            if (minPercentile < 0.0) minPercentile = 0.0;
            if (maxPercentile > 1.0) maxPercentile = 1.0;
            //if (minPercentile < 0.0) throw new ArgumentException("minPercentile must be at least 0.0");
            //if (maxPercentile > 1.0) throw new ArgumentException("maxPercentile must be at most 1.0");
            double min;
            double max;
            MinMax(matrix, out min, out max);
            if (max <= min) throw new ArgumentException("max="+max+" must be > min="+min);
            minCut = min;
            maxCut = max;
            if ((minPercentile == 0.0) && (maxPercentile == 1.0)) return;


            const int n = 1024;      //number of bins for histogram
            int[] bins = new int[n]; //histogram of power in sonogram
            int M = matrix.GetLength(0); //width
            int N = matrix.GetLength(1); //height
            double range = max - min;
            for (int i = 0; i < M; i++)
                for (int j = 0; j < N; j++)
                {   //normalise power for given min and max
                    int k = (int)Math.Floor(n * (matrix[i, j] - min) / range);//normalise
                    if (k < 0) k = 0;      //range check
                    if (k >= n) k = n - 1;
                    bins[k]++;
                }

            int minThres = (int)Math.Floor(minPercentile * M * N);
            minCut = min;
            for (int k = 0; k < n; k++)
            {
                minThres -= bins[k];
                if (minThres < 0.0)
                {
                    minCut = min + k * range / n;
                    break;
                }
            }

            int maxThres = (int)Math.Ceiling((1.0 - maxPercentile) * M * N);
            maxCut = max;
            for (int k = n; k > 0; k--)
            {
                maxThres -= bins[k - 1];
                if (maxThres < 0.0)
                {
                    maxCut = min + k * range / n;
                    break;
                }
            }
        }// end of GetPercentileCutoffs()



//=============================================================================
        /// <summary>
        /// counts the positive values in an array. Called by Classifier.Scan(Sonogram s)
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static int CountPositives(double[] values)
        {
            int count = 0;
            foreach (double d in values)
            {
                if (d > 0.0) count++;
            }
            return count;
        }
        public static int CountPositives(int[] values)
        {
            int count = 0;
            foreach (int v in values)
            {
                if (v > 0) count++;
            }
            return count;
        }





    public static int CountTrues(bool[] peaks)
    {
        int count = 0;
        foreach (bool b in peaks)
        {
           if (b) count++;
        }
        return count;
    }


    public static double[] Bool2Binary(bool[] boolArray)
    {
        int L = boolArray.Length;
        double[] binary = new double[L];
        for (int i = 0; i < L; i++) // iterate through boolArray
            if (boolArray[i]) binary[i] = 1.0;
        return binary;
    }


    public static void CountPeaks(double[] array, out int count, out double sum)
    {
        count = 0;
        sum = 0.0;
        int L = array.Length;

        for (int i = 1; i < L-1; i++) // iterate through array
            if ((array[i] > array[i - 1]) && (array[i] > array[i + 1]))
            {
                count ++;
                sum += array[i];
            }
    }

        /// <summary>
        /// returns an array showing locaiton of peaks
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        /// <param name="sum"></param>
    public static void PeakLocations(double[] array, double threshold, out int count, out double[] locations)
    {
        count = 0;
        int L = array.Length;
        locations = new double[L];

        for (int i = 1; i < L - 1; i++) // iterate through array
        {
            if (array[i] < threshold) continue;
            if ((array[i] > array[i - 1]) && (array[i] > array[i + 1]))
            {
                count++;
                locations[i] = 1.0;
            }
        }
    }
        /// <summary>
        /// returns a list of gaps between 1s in a binary array
        /// </summary>
        /// <param name="peakLocations">a binary array</param>
        /// <returns></returns>
        public static List<int> GapLengths(double[] binaryArray)
        {
            int L = binaryArray.Length;
            var gaps = new List<int>();
            int prev = 0;

            for (int i = 1; i < L; i++) // iterate through array
            {
                if ((binaryArray[i] == 1.0) && (prev > 0))
                {
                    gaps.Add(i - prev);
                    prev = i;
                }
                else if (binaryArray[i] == 1.0) prev = i;
            }
            return gaps;
        }

//=============================================================================

    public static double[] AutoCorrelation(double[] X, int minLag, int maxLag)
    {
        if(maxLag > X.Length) maxLag = X.Length;
        int lagCount = maxLag - minLag + 1;
        var A = new double[lagCount];
        for (int lag = minLag; lag <= maxLag; lag++)
        {
            double sum = 0.0;
            for (int i = 0; i < X.Length-lag; i++)
            {
                sum += (X[i] * X[i+lag]);
            }
            A[lag - minLag] = sum / (X.Length - lag);
        }
        return A;
    }

//=============================================================================

  static public double[] counts2RF(int[] counts)
  { int L = counts.Length;
  	double[] rf = new double[L];
    // get the sum
  	int sum = 0;
  	for (int i=0; i<L; i++) // iterate through array
    { sum += counts[i];
    }
  	for (int i=0; i<L; i++) // iterate through array
  	rf[i] = (double)counts[i] / (double)sum;

  	// check total sums to 1.0;
  	double p = 0.0;
  	for (int i=0; i<L; i++) p += rf[i];
  	if(p > 1.00001) Console.WriteLine("WARNING:DataUtilities.counts2RF() total prob="+p);
  	if(p < 0.99999) Console.WriteLine("WARNING:DataUtilities.counts2RF() total prob="+p);

  	return rf;
  }

  static public double[] values2RF(double[] values)
  { int length = values.Length;
  	double[] rf = new double[length];
    // get the sum
  	double sum = 0;
  	for (int i=0; i<length; i++) // iterate through array
    { sum += values[i];
    }
  	for (int i=0; i<length; i++) // iterate through array
  	rf[i] = values[i] / sum;

  	// check total sums to 1.0;
  	double p = 0.0;
  	for (int i=0; i<length; i++) p += rf[i];
  	if(p > 1.00001) Console.WriteLine("WARNING:DataUtilities.counts2RF() total prob="+p);
  	if(p < 0.99999) Console.WriteLine("WARNING:DataUtilities.counts2RF() total prob="+p);

  	return rf;
  }
   
//=============================================================================

	public static Dictionary<string, int> WordsHisto(List<string> list)
	{
		var ht = new Dictionary<string, int>();
		foreach (var item in list)
		{
			if (!ht.ContainsKey(item))
				ht.Add(item, 1);
			else
				ht[item] = ht[item] + 1;
		}
		return ht;
	}

  public static void WriteArrayList(List<string> list)
  {
      for (int i = 0; i < list.Count; i++)
          Console.WriteLine(i + "  " + list[i]);
  }

  public static void writeArray(string[] array)
  {
      for (int i = 0; i < array.Length; i++)
      {
          Console.WriteLine(i + "  " + array[i]);
      }
  }
  public static void writeArray(int[] array)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i + "  " + array[i]);
  }
  public static string writeArray2String(int[] array)
  {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < array.Length; i++) sb.Append("  "+array[i]);
      return sb.ToString();
  }
  public static void writeArray(double[] array)
  {
      string format = "F3";
      writeArray(array, format);
  }
  public static void writeArray(double[] array, string format)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i + "  " + array[i].ToString(format));
  }
  public static void writeArray(bool[] array)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i + "  " + array[i]);
  }

  public static void WriteArrayInLine(double[] array, string format)
  {
      int count = array.Length;//dimension
      for (int i = 0; i < count; i++)
      {
          Console.Write(" " + array[i].ToString(format));
      }
      Console.WriteLine();
  }

  public static void writeMatrix(double[,] matrix, string format)
  {
      int rowCount = matrix.GetLength(0);//height
      int colCount = matrix.GetLength(1);//width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              Console.Write(" " + matrix[i, j].ToString(format));
              if(j<colCount-1)Console.Write(",");
          }
          Console.WriteLine();
      }
  }
  public static void writeMatrix(double[,] matrix)
  {  
      writeMatrix(matrix, "F2");
  }

  public static void writeMatrix(int[,] matrix)
  {
      int rowCount = matrix.GetLength(0);//height
      int colCount = matrix.GetLength(1);//width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              Console.Write(" " + matrix[i, j]);
              if (j < colCount - 1) Console.Write(",");
          }
          Console.WriteLine();
      }
  }

  public static void writeMatrix2File(double[,] matrix, string fPath)
  {
      int rowCount = matrix.GetLength(0);//height
      int colCount = matrix.GetLength(1);//width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              Console.Write(" " + matrix[i, j].ToString("F2"));
              if (j < colCount - 1) Console.Write(",");
          }
          Console.WriteLine();
      }
  }


        /// <summary>
        /// this method written to display silence/noise profile of wav file. May not fit general use.
        /// Must be shifted and scaled because all values are neg
        /// </summary>
        /// <param name="data"></param>
  public static void writeBarGraph(double[] data)
  {
      Console.WriteLine("BAR GRAPH OF DOUBLES DATA");
      double min;
      double max;
      MinMax(data, out min, out max);
      if(max < 10.0)
      {
          min = 0;
          max = 80;
          data = DataTools.Normalise(data, min, max);
      }
      int[] intdata = new int[data.Length];
      for(int i=0; i<data.Length; i++) intdata[i] = (int)Math.Round(2*(data[i] - min));//subtract min to remove neg values
      writeBarGraph(intdata);
  }



  public static void writeBarGraph(int[] data)
  {
    int min;
    int max;
    MinMax(data, out min, out max);
    int sf = 1;  // scaling factor for graphing bars
    if(max>80) sf=2;
    if(max>160)sf=3;
    if(max>240)sf=4;
    if(max>320)sf=5;
    if(max>400)sf=6;
    if(max>480)sf=7;
    if(max>560)sf=8;
    if(max>640)sf=9;
    if(max>720)sf=10;
    if(max>800)sf=20;
    if(max>1600)sf=30;
    if(max>3200)sf=50;
    if(max>4800)sf=70;
    if(max>6400)sf=90;
    if(max>7200)sf=100;
    if(max>8000)sf=200;
    if(max>16000)sf=300;
    if(max>32000)sf=600;
    if(max>64000)sf=1200;
    if(max>128000)sf=2400;

    for (int i=0; i < data.Length; i++)
    {
      if(max<20) data[i]*=2;
      if(i<10)Console.Write(" ");// formatting only
      Console.Write(" "+i+"|");
      double v = data[i] / (double)sf;
      int ht = (int)Math.Floor(v);
      if((v>0.00)&&(v<1.00)) ht = 1;
      for (int j = 0; j <ht; j++) Console.Write("=");
      Console.WriteLine();
      //if(i % 50 == 0) Console.ReadLine();
    }
    Console.WriteLine("Min="+min+" Max="+max+"  Scaling factor="+sf);
    Console.WriteLine();
  }

        static public void writeConciseHistogram(int[] data)
        {
            int min;
            int max;
            MinMax(data, out min, out max);
            int[] histo = new int[max + 1];
            for (int i = 0; i < data.Length; i++)
            {
                histo[data[i]]++;
            }
            for (int i = min; i <= max; i++)
                Console.WriteLine(" " + i + "|" + histo[i]);
            Console.WriteLine();
        }




  static public void writeConcise2DHistogram(int[,] array, int max)
  { 
    int[,]matrix = new int[max,max];
    for (int i=0; i < array.Length; i++)
    { matrix[array[i,0], array[i,1]] ++;
    }

    for (int r=0; r<max; r++)
    { 
      for (int c=0; c<max; c++)
      {
          if (matrix[r,c] > 0) Console.WriteLine(r + "|" + c + "|" + matrix[r,c] + "  "); 
      }
      Console.WriteLine();
    }
  }


  public static int[] Histo(double[] data, int binCount)
  {
      double min;
      double max;
      MinMax(data, out min, out max);
      double range = max - min;
      double binWidth = range / (double)binCount;
      // init freq bin array
      int[] bins = new int[binCount];
      for (int i = 0; i < data.Length; i++)
      {
          int id = (int)((data[i]-min) / binWidth);
          if (id == binCount) id--;
          bins[id]++;
      }
      return bins;
  }

  public static int[] Histo(double[] data, int binCount, out double binWidth, out double min, out double max)
  {
      MinMax(data, out min, out max);
      double range = max - min;
      binWidth = range / (double)binCount;
      // init freq bin array
      int[] bins = new int[binCount];
      for (int i = 0; i < data.Length; i++)
      {
          int id = (int)((data[i] - min) / binWidth);
          if (id == binCount) id--;
          bins[id]++;
      }
      return bins;
  }

        /// <summary>
  /// returns a fixed width histogram.
  /// Width is determined by user supplied min and max.
  /// </summary>
  /// <param name="data"></param>
  /// <param name="binWidth"> should be an integer width</param>
  /// <param name="min"></param>
  /// <param name="max"></param>
  /// <returns></returns>
  public static int[] Histo_FixedWidth(int[] data, int binWidth, int min, int max)
  {
      int range = max - min + 1;
      int binCount = range / binWidth;
      // init freq bin array
      int[] bins = new int[binCount];
      for (int i = 0; i < data.Length; i++)
      {
          int id = (int)((data[i] - min) / binWidth);
          if (id >= binCount) id = binCount - 1;
          else
              if (id < 0) id = 0;
          bins[id]++;
      }
      return bins;
  }
  /// <summary>
  /// returns a fixed width histogram.
  /// Width is determined by user supplied min and max.
  /// </summary>
  /// <param name="data"></param>
  /// <param name="binWidth"> should be an integer width</param>
  /// <param name="min"></param>
  /// <param name="max"></param>
  /// <returns></returns>
  public static int[] Histo_FixedWidth(double[] data, double binWidth, double min, double max)
  {
      double range = max - min + 1;
      int binCount = (int)(range / binWidth);
      // init freq bin array
      int[] bins = new int[binCount];
      for (int i = 0; i < data.Length; i++)
      {
          int id = (int)((data[i] - min) / binWidth);
          if (id >= binCount) id = binCount - 1;
          else
              if (id < 0) id = 0;
          bins[id]++;
      }
      return bins;
  }




  static public int[] Histo(double[,] data, int binCount, out double binWidth, out double min, out double max)
  {
      int rows = data.GetLength(0);
      int cols = data.GetLength(1);
      int[] histo = new int[binCount];
      min = double.MaxValue;
      max = -double.MaxValue;
      MinMax(data, out min, out max);
      binWidth = (max - min) / binCount;

      for (int i = 0; i < rows; i++)
          for (int j = 0; j < cols; j++)
          {
              int bin = (int)((data[i, j] - min) / binWidth);
              if (bin >= binCount) bin = binCount - 1;
              if (bin < 0) bin = 0;
              histo[bin]++;
          }

      return histo;
  }

  static public int[] Histo(int[] data, int binCount, out double binWidth, out int min, out int max)
  {
      int length = data.Length;
      int[] histo = new int[binCount];
      min = Int32.MaxValue;
      max = -Int32.MaxValue;
      MinMax(data, out min, out max);
      binWidth = (max - min + 1) / (double)binCount;

      for (int i = 0; i < length; i++)
      {
              int bin = (int)((double)(data[i] - min) / binWidth);
              if (bin >= binCount) bin = binCount - 1;
              histo[bin]++;
      }

      return histo;
  }

  static public int[] Histo(double[,] data, int binCount)
  {
      double min; 
      double max;
      DataTools.MinMax(data, out min, out max);
      double binWidth = (max - min + 1) / (double)binCount;
      Console.WriteLine("data min="+min+"  data max="+ max + " binwidth="+binWidth);

      return Histo(data, binCount, min, max, binWidth);
  }

  static public int[] Histo(double[,] data, int binCount, double min, double max, double binWidth)
  {
      int rows = data.GetLength(0);
      int cols = data.GetLength(1);
      int[] histo = new int[binCount];

      for (int i = 0; i < rows; i++)
          for (int j = 0; j < cols; j++)
          {
              int bin = (int)((data[i, j] - min) / binWidth);
              if (bin >= binCount) bin = binCount - 1;
              if (bin < 0) bin = 0;
              histo[bin]++;
          }

      return histo;
  }


  static public int[] Histo_addition(double[,] data, int[] histo, double min, double max, double binWidth)
  {
      int rows = data.GetLength(0);
      int cols = data.GetLength(1);
      int binCount = histo.Length;

      for (int i = 0; i < rows; i++)
          for (int j = 0; j < cols; j++)
          {
              int bin = (int)((data[i, j] - min) / binWidth);
              if (bin >= binCount) bin = binCount - 1;
              if (bin < 0) bin = 0;
              histo[bin]++;
          }

      return histo;
  }


  /// <summary>
  /// Logical AND of two vectors vector v2 to v1
  /// </summary>
  /// <param name="v1"></param>
  /// <param name="v2"></param>
  /// <returns></returns>
  public static byte[] LogicalORofTwoVectors(byte[] v1, byte[] v2)
  {
      int L1 = v1.Length;
      int L2 = v2.Length;
      if (L1 != L2) throw new Exception("ERROR! Vectors must be of same length.");

      byte[] addition = new byte[L1];
      for (int i = 0; i < L1; i++)
      {
          if ((v1[i] >= 1) || (v2[i] >= 1)) addition[i] = 1;
      }
      return addition;
  }
        

  /// <summary>
  /// Add vector v2 to v1
  /// </summary>
  /// <param name="v1"></param>
  /// <param name="v2"></param>
  /// <returns></returns>
  public static double[] AddVectors(double[] v1, double[] v2)
  {
      int L1 = v1.Length;
      int L2 = v2.Length;
      if (L1 != L2) throw new Exception("ERROR! Vectors must be of same length.");

      double[] addition = new double[L1];
      for (int i = 0; i < L1; i++)
          {
              addition[i] = v1[i] + v2[i];
          }
      return addition;
  }

  /// <summary>
  /// Subtract vector v2 from vector v1
  /// </summary>
  /// <param name="v1"></param>
  /// <param name="v2"></param>
  /// <returns></returns>
  public static double[] SubtractVectors(double[] v1, double[] v2)
  {
      int L1 = v1.Length;
      int L2 = v2.Length;
      if (L1 != L2) throw new Exception("ERROR! Vectors must be of same length.");

      double[] difference = new double[L1];
      for (int i = 0; i < L1; i++)
      {
          difference[i] = v1[i] - v2[i];
      }
      return difference;
  }
     

  /// <summary>
  /// ADD matrix m2 to matrix m1
  /// </summary>
  /// <param name="m1"></param>
  /// <param name="m2"></param>
  /// <returns></returns>
  public static double[,] AddMatrices(double[,] m1, double[,] m2)
  {
      int m1Rows = m1.GetLength(0);
      int m1Cols = m1.GetLength(1);
      int m2Rows = m2.GetLength(0);
      int m2Cols = m2.GetLength(1);
      if (!(m1Rows == m2Rows)) throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
      if (!(m1Cols == m2Cols)) throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");

      double[,] newMatrix = (double[,])m1.Clone();
      for (int i = 0; i < m1Rows; i++)
          for (int j = 0; j < m1Cols; j++)
          {
              newMatrix[i, j] = m1[i, j] + m2[i, j];
          }
      return newMatrix;
  }

  /// <summary>
  /// Subtract matrix m2 from matrix m1
  /// </summary>
  /// <param name="m1"></param>
  /// <param name="m2"></param>
  /// <returns></returns>
  public static double[,] SubtractMatrices(double[,] m1, double[,] m2)
  {
      int m1Rows = m1.GetLength(0);
      int m1Cols = m1.GetLength(1);
      int m2Rows = m2.GetLength(0);
      int m2Cols = m2.GetLength(1);
      if (!(m1Rows == m2Rows)) throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
      if (!(m1Cols == m2Cols)) throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");

      double[,] newMatrix = (double[,])m1.Clone();
      for (int i = 0; i < m1Rows; i++)
          for (int j = 0; j < m1Cols; j++)
          {
              newMatrix[i, j] = m1[i, j] - m2[i, j];
          }
      return newMatrix;
  }



  /// <summary>
  /// Returns binary matrix with values set = 1 if they exceed the threshold else set = 0;
  /// </summary>
  /// <param name="matrix"></param>
  /// <returns></returns>
  public static double[,] Threshold(double[,] matrix, double threshold)
  {
      int height = matrix.GetLength(0);
      int width = matrix.GetLength(1);
      double[,] M = new double[height, width];

      for (int col = 0; col < width; col++)//for all cols
      {
          for (int y = 0; y < height; y++) //for all rows
          {
              if (matrix[y, col] < threshold) M[y, col] = 0.0;
              else M[y, col] = 1.0;
          }
      }//for all cols
      return M;
  }// end of Threshold()




  public static double[] GetColumn(double[,] m, int colID)
  {
      int rows = m.GetLength(0);
      double[] column = new double[rows];
      for (int i = 0; i < rows; i++) column[i] = m[i,colID];
      return column;
  }

  public static void SetColumn(double[,] m, int colID, double[] array)
  {
      int rows = m.GetLength(0);
      for (int r = 0; r < rows; r++) m[r, colID] = array[r];
  }


  /// <summary>
  /// set all values in the passed column to zero.
  /// </summary>
  /// <param name="m"></param>
  /// <param name="colID"></param>
  public static void SetColumnZero(byte[,] m, int colID)
  {
      int rows = m.GetLength(0);
      for (int r = 0; r < rows; r++) m[r, colID] = 0;
  }

  public static int SumColumn(int[,] m, int colID)
  {
      int rows = m.GetLength(0);
      int sum = 0;
      for (int i = 0; i < rows; i++) sum += m[i,colID];
      return sum;
  }

  public static double[] GetRow(double[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      double[] row = new double[cols];
      for (int i = 0; i < cols; i++) row[i] = m[rowID, i];
      return row;
  }

  public static byte[] GetRow(byte[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      byte[] row = new byte[cols];
      for (int i = 0; i < cols; i++) row[i] = m[rowID, i];
      return row;
  }

  public static double GetRowSum(double[,] m, int rowID)
  {
      double[] row = GetRow(m, rowID);
      return row.Sum();
  }

  public static int GetRowSum(byte[,] m, int rowID)
  {
      int sum = 0;
      for (int j = 0; j < m.GetLength(1); j++) sum += m[rowID, j];
      return sum;
  }

  public static int[] GetRowSums(byte[,] m)
  {
      int[] rowSums = new int[m.GetLength(0)];
      for (int i = 0; i < m.GetLength(0); i++)
      {
          int sum = 0;
          for (int j = 0; j < m.GetLength(1); j++) sum += m[i, j];
          rowSums[i] = sum;
      }
      return rowSums;
  }

  public static int GetRowSum(int[,] m, int rowID)
  {
      int cols = m.GetLength(1);
      int sum = 0;
      for (int i = 0; i < cols; i++) sum += m[rowID, i];
      return sum;
  }

  public static double[] SumColumns(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double sum = 0.0;
      double[] colSums = new double[cols];
      for (int j = 0; j < cols; j++)
      {
          sum = 0.0;
          for (int i = 0; i < rows; i++)
          {
              sum += m[i, j];
          }
          colSums[j] = sum;
      }
      //sum = 0.0;
      //for (int j = 0; j < cols; j++) sum += colSums[j];
      //Console.WriteLine("sum="+sum.ToString("F5"));
      return colSums;
  }

  public static double[] GetColumnsAverages(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double sum = 0.0;
      double[] colSums = new double[cols];
      for (int j = 0; j < cols; j++)
      {
          sum = 0.0;
          for (int i = 0; i < rows; i++)
          {
              sum += m[i, j];
          }
          colSums[j] = sum / rows;
      }
      return colSums;
  }



        //copy first n values of vector1 into vector 2}
    public static double[] CopyVector(int n, double[] v1)
    {
        double[] v2 = new double[v1.Length];
        for (int i = 0; i < n; i++) v2[i] = v1[i];
        return v2;
    }
    //returns copy of a vector
    public static double[] CopyVector(double[] v1)
    {
        double[] v2 = new double[v1.Length];
        for (int i = 0; i < v1.Length; i++) v2[i] = v1[i];
        return v2;
    }



        /// <summary>
        /// normalise an array of double between 0 and 1
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static double[] normalise(double[] v)
        {
            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            double[] ret = (double[])v.Clone();
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max)
                    max = v[i];
                if (v[i] < min)
                    min = v[i];
            }

            for (int i = 0; i < v.Length; i++)
                ret[i] = (v[i] - min) / (max - min);

            return (ret);
		}


        public static double[] ScaleArray(double[] v, int newLength)
        {
            int L = v.Length;
            double[] ret = new double[newLength];
            double ratio = newLength / (double)L;

            for (int i = 0; i < newLength; i++)
            {
                int index = (int)(i / ratio);
                ret[i] = v[index];
            }
            return (ret);
        }



		/// <summary>
		/// returns an array of double initialised with passed value
		/// </summary>
		public static double[] GetInitialisedArray(int length, double iniValue)
		{
		  double[] array = new double[length];
		  for (int i = 0; i < length; i++) array[i] = iniValue;
		  return (array);
		}

		/// <summary>
		/// normalised matrix of real values to [0,1].
		/// </summary>
		public static double[,] normalise(double[,] m)
		{
			double min = Double.MaxValue;
			double max = -Double.MaxValue;

			int rows = m.GetLength(0);
			int cols = m.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max) max = m[i, j];
                    else
                        if (m[i, j] < min) min = m[i, j];
                }
            }
			double range = max - min;

            double[,] ret = new double[rows, cols];
            if (range == 0.0) return ret;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    ret[i, j] = (m[i, j] - min) / range;
            }

			return (ret);
		}

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// Used for Syntactic pattern recognition
        /// Converts Image matrix to Spectrogram data orientation
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static char[,] MatrixRotate90Clockwise(char[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new char[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[c, r] = m[rows-r, c];
            return ret;
        }

        /// <summary>
        /// Rotates a matrix 90 degrees clockwise.
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static double[,] MatrixRotate90Clockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[c, r] = m[rows - r - 1, c];
            return ret;
        }
        /// <summary>
        /// Rotates a matrix 90 degrees anticlockwise.
        /// Used for Syntactic pattern recognition
        /// Converts Image matrix to Spectrogram data orientation
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static double[,] MatrixRotate90Anticlockwise(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[c, r] = m[r, cols -1-c];
            return ret;
        }

        /// <summary>
        /// performs a matrix transform
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>
        public static double[,] MatrixTranspose(double[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] Mt = new double[cols, rows];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    Mt[c, r] = M[r, c];
                }
            return Mt;
        }

        /// <summary>
        /// transforms a matrix of char.
        /// Used for Syntatctic pttern recognition
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>

        public static char[,] MatrixTranspose(char[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new char[cols, rows];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[c, r] = m[r, c];
            return ret;
        }

        /// <summary>
        /// performs a matrix transform
        /// </summary>
        /// <param name="M">the matrix to transform</param>
        /// <returns></returns>
        public static byte[,] MatrixTranspose(byte[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            byte[,] Mt = new byte[cols, rows];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    Mt[c, r] = M[r, c];
                }
            return Mt;
        }


        public static double[,] ScaleMatrix(double[,] matrix, int newRowCount, int newColCount)
    {
            int mrows = matrix.GetLength(0);
            int mcols = matrix.GetLength(1);
            var newMatrix = new double[newRowCount, newColCount];
            double rowScale = newRowCount / (double)matrix.GetLength(0);
            double colScale = newColCount / (double)matrix.GetLength(1);
            for (int r = 0; r < mrows; r++)
            {
                    for (int c = 0; c < mcols; c++)
                    {
                        if (matrix[r,c] == 0.0) continue;
                        int newRow = (int)Math.Round(r * rowScale);
                        int newCol = (int)Math.Round(c * colScale);
                        newCol = (int)Math.Round(c * 4.1);
                        //newCol = 1;
                        if (newRow >= newRowCount) newRow = newRowCount - 1;
                        if (newCol >= newColCount) newCol = newColCount - 1;
                        //if (newMatrix[newRow, newCol] < matrix[r, c]) 
                            newMatrix[newRow, newCol] = matrix[r, c];
                    }
            }

            return newMatrix;
    }


//===========================================================================================================================================================

        /// <summary>
        /// This algorithm is derived from the Lamel et al algorithm used in the SNR class.
        /// Only difference is return the true model value whereever it is.
        /// The relevant lines have been commented.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="Q"></param>
        /// <param name="oneSD"></param>
        public static void ModalValue(double[] array, out double Q, out double oneSD)
        {
            int L = array.Length;
            //CALCULATE THE MIN AND MAX OF THE ARRAY
            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            for (int i = 0; i < L; i++)
            {
                if (array[i] < min) min = array[i];
                else
                    if (array[i] > max) max = array[i];
            }

            //set up Histogram
            int binCount = 100;
            double binWidth = (max - min) / binCount;
            int[] histo = new int[binCount];

            for (int i = 0; i < L; i++)
            {
                int id = (int)((array[i] - min) / binWidth);
                if (id >= binCount)
                {
                    id = binCount - 1;
                }
                else
                    if (id < 0) id = 0;
                histo[id]++;
            }
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 3);
            //DataTools.writeBarGraph(histo);

            // find peak of lowBins histogram
            // FIND MAX VALUE IN BOTTOM FRACTION OF RANGE. ASSUMES NOISE IS GAUSSIAN and that their is some signal.
            //int upperBound = (int)(binCount * SNR.FRACTIONAL_BOUND_FOR_MODE);
            //for (int i = upperBound; i < binCount; i++) smoothHisto[i] = 0;//set top N% of intensity bins = 0. 
            int peakID = DataTools.GetMaxIndex(smoothHisto);
            Q = min + ((peakID + 1) * binWidth); //modal noise level

            //calculate SD of the background noise
            double total = 0;
            double ssd = 0.0; //sum of squared deviations
            for (int i = 0; i < peakID; i++)
            {
                total += smoothHisto[i];
                double dev = (peakID - i) * binWidth;
                ssd += dev * dev; //sum of squared deviations
            }

            if (peakID > 0) oneSD = Math.Sqrt(ssd / total);
            else oneSD = Math.Sqrt((binWidth * binWidth) / smoothHisto[0]); //deal with case where peakID = 0 to prevent division by 0;
        }




        /// <summary>
        /// shift values by their mean.
        /// </summary>
        public static double[,] DiffFromMean(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double av; double sd;
            NormalDist.AverageAndSD(m, out av, out sd);  

            double[,] ret = new double[rows,cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    ret[i, j] = m[i, j] - av;

            return ret;
        }

        /// <summary>
        /// shift values by their mean.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] DiffFromMean(double[] V)
        {
            int L = V.Length;
            double av; double sd;
            NormalDist.AverageAndSD(V, out av, out sd);

            double[] returnV = new double[L];

            for (int i = 0; i < L; i++) returnV[i] = V[i] - av;

            return returnV;
        }
        /// <summary>
        /// shift values by their mean.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[] Vector2Zscores(double[] V)
        {
            int L = V.Length;
            double av; double sd;
            NormalDist.AverageAndSD(V, out av, out sd);

            double[] returnV = new double[L];

            for (int i = 0; i < L; i++) returnV[i] = (V[i] - av)/sd;

            return returnV;
        }


        public static double DotProduct(double[] v1, double[] v2)
        {
            //assume v1 and v2 have same dimensions
            int L = v1.Length;
            double sum = 0.0;
            for (int i = 0; i < L; i++) sum += (v1[i] * v2[i]);
            return sum;
        }

        /// <summary>
        /// Clculates Hamming distance for two vectors of doubles.
        /// d[i] = 1 if((int)Math.Round(Math.Abs(v1[i] - v2[i])) == 1 )
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int HammingDistance(double[] v1, double[] v2)
        {
            //assume v1 and v2 have same dimensions
            int L = v1.Length;
            int d = 0;
            for (int i = 0; i < L; i++) if((int)Math.Round(Math.Abs(v1[i] - v2[i])) == 1 ) d++;
            return d;
        }


        /// <summary>
        /// returns EUCLIDIAN DISTANCE BETWEEN two matrices
        /// </summary>
        /// <param name="template"></param>
        /// <param name="signal"></param>
        /// <returns></returns>
        public double EuclidianDistance(double[,] m1, double[,] m2)
        {
            //check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2) throw new System.Exception("Matrices have unequal row numbers.");
            if (cols1 != cols2) throw new System.Exception("Matrices have unequal column numbers.");

            //calculate euclidian distance
            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols1; j++)
                {
                    double v = m1[i, j] - m2[i, j];
                    sum += (v * v);
                }
            }
            return Math.Sqrt(sum);
        } //end



        public static double DotProduct(double[,] m1, double[,] m2)
        {
            //check m1 and m2 have same dimensions
            int rows1 = m1.GetLength(0);
            int cols1 = m1.GetLength(1);
            int rows2 = m2.GetLength(0);
            int cols2 = m2.GetLength(1);
            if (rows1 != rows2) throw new System.Exception("Matrices have unequal row numbers.");
            if (cols1 != cols2) throw new System.Exception("Matrices have unequal column numbers.");
            double sum = 0.0;
            for (int i = 0; i < rows1; i++)
                for (int j = 0; j < cols1; j++)
                    sum += (m1[i, j] * m2[i, j]);
            return sum;
        }

        /// <summary>
        /// convert values to Decibels.
        /// Assume that all values are positive
        /// </summary>
        /// <param name="m">matrix of positive power values</param>
        /// <returns></returns>
        public static double[,] DeciBels(double[,] m, out double min, out double max)
        {
            min = Double.MaxValue;
            max = -Double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    double dBels = 10 * Math.Log10(m[i,j]);    //convert power to decibels
                    //NOTE: the decibels calculation should be a ratio. 
                    // Here the ratio is implied ie relative to the power in the original normalised signal
            //        if (dBels <= min) min = dBels;
              //      else
                //    if (dBels >= max) max = dBels;
                    ret[i, j] = dBels;
                }
            return ret;
        }


        /// <summary>
        /// normalises the values in a matrix between the passed min and max.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] Normalise(double[,] m, double normMin, double normMax)
        {
            //m = normalise(m);
            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                        max = m[i, j];
                    if (m[i, j] < min)
                        min = m[i, j];
                }
            double range = max - min;
            double normRange = normMax - normMin;
            //Console.WriteLine("range ="+ range+"  normRange="+normRange);

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {   
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }

            return (ret);
        }


        /// <summary>
        /// normalises the values in a vector between the passed min and max.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[] Normalise(double[] v, double normMin, double normMax)
        {
            //m = normalise(m);
            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            int length = v.Length;
            double[] ret = new double[length];
            for (int i = 0; i < length; i++)
            {
                if (v[i] > max) max = v[i];
                if (v[i] < min) min = v[i];
            }
            double range = max - min;
            double normRange = normMax - normMin;
            //Console.WriteLine("range ="+ range+"  normRange="+normRange);

            for (int i = 0; i < length; i++)
            {
                double norm01 = (v[i] - min) / range;
                ret[i] = normMin + (norm01 * normRange);
            }

            return (ret);
        }

        
        public static double[] normalise(int[] v)
        {
            int min = Int32.MaxValue;
            int max = -Int32.MaxValue;

            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] > max) max = v[i];
                if (v[i] < min) min = v[i];
            }

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
                ret[i] = (v[i] - min) / (double)(max - min);

            return (ret);
        }


        /**
         * differs from above in that want area under curve = 1;
         * @param v
         * @return
         */
        public static double[] NormaliseArea(int[] v)
        {
            int sum = 0;
            for (int i = 0; i < v.Length; i++) sum += v[i];

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++) ret[i] = v[i] / (double)(sum);

            return (ret);
        }
        /**
         * @param v
         * @return
         */
        public static double[] NormaliseArea(double[] v)
        {
            double sum = 0.0;
            for (int i = 0; i < v.Length; i++) sum += v[i];

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++) ret[i] = v[i] / sum;

            return ret;
        }

        /// <summary>
        /// normalises the values in a matrix such that the minimum value
        /// is the average of the edge values.
        /// Truncate thos original values that are below the edge average.
        /// This method is used to normalise image templates where there should be no power at the edge
        /// </summary>
        /// <param name="m"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] normalise_zeroEdge(double[,] m, double normMin, double normMax)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double edge = 0.0;
            for (int i = 0; i < rows; i++)   edge += (m[i, 0] + m[i, (cols-1)]); //sum the sides
            for (int i = 1; i < cols-1; i++) edge += (m[0, i] + m[(rows-1), i]); //sum top and bottom
            edge /= ((2*rows) + (2*cols) - 4);

            //truncate everything to edge average
            //for (int i = 0; i < rows; i++)
            //    for (int j = 0; j < cols; j++) if (m[i, j] < edge) m[i, j] = edge;
            //set all edges to edge average
            for (int i = 0; i < rows; i++)   m[i, 0]      = edge;
            for (int i = 0; i < rows; i++)   m[i, cols-1] = edge;
            for (int i = 1; i < cols-1; i++) m[0, i]      = edge;
            for (int i = 1; i < cols-1; i++) m[rows-1, i] = edge; 

            //find min and max
            double min = Double.MaxValue;
            double max = -Double.MaxValue;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] > max)
                        max = m[i, j];
                    if (m[i, j] < min)
                        min = m[i, j];
                }

            double range = max - min;
            double normRange = normMax - normMin;
            //Console.WriteLine("range ="+ range+"  normRange="+normRange);

            double[,] ret = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    double norm01 = (m[i, j] - min) / range;
                    ret[i, j] = normMin + (norm01 * normRange);
                }

            return (ret);
        }




  public static double[] normalise2UnitLength(int[] v)
  {
    double SS = 0.0;
    int length = v.Length;
    
    for(int i=0; i<length; i++) 
    {
      SS = SS + (v[i] * v[i]);
    }
    // square root of sum to get vector mapLength
    double norm = Math.Sqrt(SS);
    //Console.WriteLine("SS="+SS+"  norm="+norm);
    
    double[] ret = new double[length]; // vector to return
    for(int i=0; i<length; i++) 
    {
      ret[i] = ((double)v[i])/norm; 
    }
    
    return(ret); 
  }
  public static double[] normalise2UnitLength(double[] v)
  {
      double SS = 0.0; //sum of squares
      int length = v.Length;

      for (int i = 0; i < length; i++) SS += (v[i] * v[i]);
      
      // square root of sum to get vector mapLength
      double norm = Math.Sqrt(SS);
      //Console.WriteLine("SS=" + SS + "  norm=" + norm);

      double[] ret = new double[length]; // vector to return
      for (int i = 0; i < length; i++)
      {
          ret[i] = v[i] / norm;
      }

      return (ret);
  }



  //normalise and compress/bound the values
  public static double[,] Clip(double[,] m, double minPercentile, double maxPercentile)
  {
      double minCut;
      double maxCut;
      PercentileCutoffs(m, minPercentile, maxPercentile, out minCut, out maxCut);
      return boundMatrix(m, minCut, maxCut);
  }

  public static int Sum(int[] data)
  {
      if (data == null) return 0;
      int sum = 0;
      for (int i = 0; i < data.Length; i++) sum += data[i];
      return sum;
  }
  public static double Sum(double[] data)
  {
      if (data == null) return 0;
      double sum = 0;
      for (int i = 0; i < data.Length; i++) sum += data[i];
      return sum;
  }



//***************************************************************************************************************************************

  public const double ln2 = 0.69314718;   //log 2 base e

    /// <summary>
    /// normalises an array of doubles to probabilities that sum to one.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
  static public double[] NormaliseProbabilites(double[] data)
  {
      int length = data.Length;
      double[] probs = new double[length];

      double sum = 0;
      for (int i = 0; i < length; i++) sum += data[i];
      if(sum == 0.0) return probs;
      for (int i = 0; i < length; i++)
      {
          probs[i] = data[i] / sum;
      }
      return probs;
  } // end NormaliseProbabilites()


  /// <summary>
  /// Calculates the entropy of the passed discrete distribution.
  /// 
  /// It is assumed that each of the elements in distr[] represent the 
  /// probability of that state and that the probabilities sum to 1.0
  /// 
  /// Math.log() is base e. To convert to log base 2 need to divide by the natural log of 2 = ln2 = 0.69314.  
  /// NOTE: In the limit as rf approaches 0, rf*log(rf) = 0.
  /// 
  /// </summary>
  /// <param name="data"></param>
  /// <returns></returns>  
  static public double Entropy(double[] distr)
  {
      double H=0.0;

      for (int i = 0; i < distr.Length; i++)
      {
          if (distr[i] != 0.00)
          {
              H -= distr[i] * Math.Log(distr[i]);
          }
      }
      return H / DataTools.ln2;
  }
  static public double Entropy(double[,] matrixDistr)
  {
      double H = 0.0;
      int RowCount = matrixDistr.GetLength(0);
      int ColCount = matrixDistr.GetLength(1);

      for (int i = 0; i < RowCount; i++)
      {
          for (int j = 0; j < ColCount; j++)
          {
              if (matrixDistr[i, j] != 0.00) H -= matrixDistr[i,j] * Math.Log(matrixDistr[i,j]);
          }
      }
      return H / DataTools.ln2;
  }
	
 	
 	/**
 	 * Calculates the relative entropy of the passed 
 	 * discrete probability distribution.
 	 * It is assumed that each of the elements in dist[] 
 	 * represents the probability of a symbol/state and the 
 	 * probabilities sum to 1.0
 	 * The relative entropy is with respect to a uniform distribution. 
 	 */
  public static double RelativeEntropy(double[] distr)
 	{   int length = distr.Length;
 	    // generate a uniform reference distribution
 	 	double[] refDistr = new double[length];
        for(int i=0; i<length; i++)refDistr[i]= 1/(double)length;
     
 	 	double H1 = Entropy(refDistr);
 	 	double H2 = Entropy(distr);
 	    return H1-H2; 		
 	}
  	
 	/**
 	 * Calculates the relative entropy of the passed 
 	 * discrete probability distribution.
 	 * It is assumed that each of the elements in dist[] 
 	 * represents the probability of a symbol/state and the 
 	 * probabilities sum to 1.0
 	 * The relative entropy is with respect to the background
 	 * or reference distribution contained in the array refDist. 
 	 */
	public static double RelativeEntropy(double[] dist, double[] refDist)
 	{  
 	 	double H1 = Entropy(refDist);
 	 	double H2 = Entropy(dist);
 	  return H1-H2; 		
 	}




//***************************************************************************************************************************************
  /// <summary>
  ///  Returns the min and max of a set of integer values in the passed array
  /// </summary>
  /// <param name="data">array ofintegers</param>
  /// <param name="min">min value to return</param>
  /// <param name="max">max value to return</param>
  static public void MinMax(int[] data, out int min, out int max)
  { min = data[0];
    max = data[0];
    for(int i=1; i<data.Length; i++)
    { if(data[i] < min) min = data[i];
      if(data[i] > max) max = data[i];
    }
    //Console.WriteLine(data.Length+"  min="+min+" max="+max);
  }

//=============================================================================

        static public void MinMax(double[] data, out double min, out double max)
        {
            min = data[0];
            max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                } else
                if (data[i] > max)
                {
                    max = data[i];
                }
            }
        }

        static public void MinMaxAv(double[] data, out double min, out double max, out double av)
        {
            min = data[0];
            max = data[0];
            av  = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] < min)
                {
                    min = data[i];
                }
                else
                    if (data[i] > max)
                    {
                        max = data[i];
                    }
                av += data[i];
            }//end for loop
            av /= data.Length;
        }

        /// <summary>
        /// returns the min and max values in a matrix of doubles
        /// </summary>
        static public void MinMax(double[,] data, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];
            for (int i = 1; i < rows; i++)
            for (int j = 1; j < cols; j++)
            {
                if (data[i,j] < min)
                    min = data[i,j];
				else if (data[i,j] > max)
                    max = data[i,j];
            }//end double loop
        }

        /**
         * returns the min and max of an array of doubles
         * and the index for both.
         * @param data
         * @return 
         */  
  static public void MinMax(double[] data, out int indexMin, out int indexMax, out double min, out double max)
  {
      indexMin = 0;
      indexMax = 0;
      //if (data == null) return;
      min = data[0];
      max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] < min)
          {
              min = data[i];
              indexMin = i;
          }
          if (data[i] > max)
          {
              max = data[i];
              indexMax = i;
          }
      }
  }

  static public void WriteMinMaxOfArray(double[] data)
  {
      double min;
      double max;
      MinMax(data, out min, out max);
      Console.WriteLine("Array Min={0:F5}  Array Max={1:F5}", min, max);
  }


  static public void WriteMinMaxOfFeatures(double[,] m)
  {

      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double min=0;
      double max=0;
      double av=0;
      for (int i = 0; i < cols; i++)
      {
          double[] column = GetColumn(m, i);
          MinMaxAv(column, out min, out max, out av);
          Console.WriteLine("Column:{0:D} min={1:F4}  max={2:F4}  av={3:F4}", i, min, max, av);
      }
  }
  static public void WriteMinMaxOfArray(string arrayname, double[] data)
  {
      double min;
      double max;
      MinMax(data, out min, out max);
      Console.WriteLine(arrayname+":  Min={0:F5}  Max={1:F5}", min, max);
  }


  /// <summary>
  /// Same as above method but returns index instead of outting it!
  /// returns the index of max value in an array of doubles.
  /// array index starts at zero.
  /// If more than one value is equal max, then returns location of first.
  /// </summary>
  /// <param name="data"></param>
  /// <returns></returns>
  static public int GetMaxIndex(int[] data)
  {
      int indexOfMax = 0;
      int max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] > max)
          {
              max = data[i];
              indexOfMax = i;
          }
      }
      return indexOfMax;
  }
  /**
   * returns the index of max value in an array of doubles.
   * array index starts at zero.
   * @param data
   * @return 
   */
  static public void getMaxIndex(double[] data, out int indexMax)
  { 
  	//if(data == null) return -1;
    indexMax = 0;
    double max = data[0];
    for(int i=1; i<data.Length; i++)
    { if(data[i] > max)
      {  max = data[i];
         indexMax = i;
         //Console.WriteLine("max="+max+"@ i="+i);
      }
    }
  }

    /// <summary>
    /// Same as above method but returns index instead of outting it!
    /// returns the index of max value in an array of doubles.
    /// array index starts at zero.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
  static public int GetMaxIndex(double[] data)
  {
      //if(data == null) return -1;
      int indexMax = 0;
      double max = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] > max)
          {
              max = data[i];
              indexMax = i;
          }
      }
      return indexMax;
  }
        /// <summary>
        /// same as above but returns the index of data element having minimum value
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
  static public int GetMinIndex(double[] data)
  {
      //if(data == null) return -1;
      int indexMin = 0;
      double min = data[0];
      for (int i = 1; i < data.Length; i++)
      {
          if (data[i] < min)
          {
              min = data[i];
              indexMin = i;
          }
      }
      return indexMin;
  }

  /**
   * returns the index of max value in an array of int
   * @param data
   * @return 
   */
  static public void getMaxIndex(int[] data, out int indexMax)
  { 
    //if(data == null) return -1;
    indexMax = 0;
    int max = data[0];
    for(int i=1; i<data.Length; i++)
    { if(data[i] > max)
      { max = data[i];
        indexMax = i;
        //Console.WriteLine("max="+max+"@ i="+i);
      }
    }
  }

   static public bool ValueInList(int value, int[] data)
   {
       for (int i = 0; i < data.Length; i++)
       {
           if(value == data[i]) return true;
       }
       return false;
   }


  /**
   * returns an array of ranked indices where the indices point to 
   * the data array with values ranked in descending order
   * @param data
   * @return 
   */
  static public int[] getRankedIndices(double[] data)
  { 
  	if((data == null)||(data.Length==0)) return null;
  	double[] dataCopy = (double[])data.Clone();  	
  	int[] order = new int[data.Length];

    for(int i=0; i<data.Length; i++)
    {   int maxIndex;
        getMaxIndex(dataCopy, out maxIndex);
    	order[i] = maxIndex;
    	dataCopy[maxIndex] = -Double.MaxValue;
    }
    return order;
  }

//=============================================================================
        static public bool[] GetPeaks(double[] data)
        {
            int length = data.Length;
            bool[] peaks = new bool[length];
            for (int i = 1; i < data.Length-1; i++)
            {
                double Dm = data[i]-data[i-1];
                double Dp = data[i+1]-data[i];
                if((Dm>0.0)&&(Dp<0.0)) peaks[i] = true;
                else peaks[i] = false;
            }
            return peaks;
        }

//=============================================================================

  /**
   *
   * @param data
   * @param min
   * @param max
   * @param binWidth
   * @return
   */
  static public int[] Data2Bins(int[] data, int min, int binWidth, int numBins)
  {
    //Console.WriteLine("numBins="+numBins);
    // initialise bin counts
    int[] binCounts = new int[numBins];
    for(int i=0; i<numBins; i++) binCounts[i] = 0;

    for(int i=0; i<data.Length; i++)
    { int bin = (data[i] - min) / binWidth;
      binCounts[bin] ++;
    }

    return binCounts;
  }




//=============================================================================



  /**
   * returns the area under the curve between two points on 
   * the curve given by x1,y1,x2,y2. 
   * Assumes that y1 and y2 are both positive.
   */
  static public double areaUnderCurve(double x1, double y1, double x2, double y2)
  { 
    double width = Math.Abs(x2-x1);
    double area = (width * y1)+ (width*(y2-y1)/2);
    //Console.WriteLine(x1+"  "+y1+"  "+x2+"  "+y2+"  area="+area);
  	return area;
  }
		
		
		


 //******************************************************************************

  /**
   * CALCULATES THE COMBINATORIAL(N,K) as
   *                 N!
   *    C(N,K) = ---------
   *             (N-K)! K!
   *
   *    C(N,K) = C(N,N-K), so it is more efficient to calculate C(N,N-K)
   *                       if K is > half of N
   *
   * To minimise chance of out of range numbers, I calculate C(N,K) by
   * multiplying a sequence of numerator/denominator values.
   *
   * NOTE: This method was originally used to calculate word association statistics.
   *       But it is no longer necessary for these calculations. Hence method not used.
   *
   *
   * @param N
   * @param K
   * @return the COMBINATORIAL as integer
   */
  static public long combinatorial(int N, int K)
  {
    if(K == 0) return 0;
    if(K >  N) return 0;
    if(K == N) return 1;
    //Console.WriteLine("N="+N+", K="+K);

    // COMB(N,K)  = COMB(N, N-K);
    // more efficient to compute combinations if use K < N/2
    if(K > (N/2)) K = N - K;

    // NOTE ON ALGORITHM:
    // factorial numbers can get very large. To prevent possible out of range errors
    // this method divides numerator and denominator numbers as it goes.
    // finally caste the double to a long.

    //  first component of C(N,K)
    double comb = N/(double)(N-K);
    //Console.WriteLine("c=0  comb="+comb);

    // now multiply by remaining components of C(N,K)
    for(int c=1; c<(N-K); c++)
    { comb *= ((N-c)/(double)(N-K-c));
      //Console.WriteLine("c="+c+"  NOM  ="+(N-c)+"  DENOM="+(N-K-c)+"   comb="+comb);
    }
    return (long)comb;
  }

//=============================================================================


  /**
   * wrapper so one can call moving average filter with array of float
   */
  public static float[] filterMovingAverage(float[] signal, int width)
  {
  	int length = signal.Length;
  	double[] dbSignal = new double[length];
    for(int i=0; i<length; i++) dbSignal[i] = signal[i];
    dbSignal = filterMovingAverage(dbSignal, width);
    for(int i=0; i<length; i++) signal[i] = (float)dbSignal[i];
    return signal;
  }
  
  /**
   * wrapper so one can call moving average filter with array of int
   */
  public static double[] filterMovingAverage(int[] signal, int width)
  {
    int length = signal.Length;
    double[] dbSignal = new double[length];
    for(int i=0; i<length; i++) dbSignal[i] = (double)signal[i];//clone
    return filterMovingAverage(dbSignal, width);
  }
  


  public static double[] filterMovingAverage(double[] signal, int width)
  { 
    if(width <= 1) return signal;    // no filter required
    int  length = signal.Length;
    if(length <= 3) return signal;   // not worth the effort!
    
    double[] fs = new double[length]; // filtered signal
    int    edge = width/2;            // half window width.
    //int    odd  = width%2;          // odd or even filter window.
    double  sum = 0.0;
    // filter leading edge
    for(int i=0; i<edge; i++)
    { sum = 0.0;
      for(int j=0; j<=(i+edge); j++) {sum += signal[j];}
      fs[i] = sum / (double)(i+edge+1);
    }
    
    for(int i=edge; i<length-edge; i++)
    { sum = 0.0;
      for(int j=0; j<width; j++) {sum += signal[i-edge+j];}
      //sum = signal[i-1]+signal[i]+signal[i+1];
      fs[i] = sum / (double)width;
    }
    
    // filter trailing edge
    for(int i=length-edge; i<length; i++)
    { sum = 0.0;
      for(int j=i; j<length; j++) {sum += signal[j];}
      fs[i] = sum / (double)(length - i);
    }
    return fs;
  }


//=========================================================================================================================
//   THE FOLLOWING GROUP OF METHODS DETECT PERIODICITY IN ARRAYS
  // Only CountHarmonicTracks() was found to be useful in the end.

  /// <summary>
  /// Counts the number of spectral tracks or harmonics in the passed ferquency band.
  /// Also calculates the average amplitude of the peaks to each succeeding trough.
  /// </summary>
  /// <param name="values">Spectral values in the frequency band.</param>
  /// <param name="expectedPeriod">Use supplied parameter. Expected number of harmonic tracks in the frequency band.</param>
  /// <param name="row">This argument is NOT used. Is included only for debugging purposes.</param>
  /// <returns></returns>
  public static Tuple<double, int> CountHarmonicTracks(double[] values, int expectedHarmonicCount, int row)
  {
      int L = values.Length;
      int expectedPeriod = L / expectedHarmonicCount;
      int midPeriod = expectedPeriod / 2;
      int smoothingWindow = midPeriod;
      // set upper limit to smoothing because large windows can displace a peak.
      // upper limit of 10 is good for spectra with 256 freq bins.
      if (smoothingWindow > 10) smoothingWindow = 10; 
      double[] smooth = DataTools.filterMovingAverage(values, smoothingWindow);
      bool[] peaks    = DataTools.GetPeaks(smooth);

      // Count the peaks and store their locations.
      var peakLocations = new int[L];
      int peakCount = -1;
      for (int i = 0; i < L; i++)
      {
          if (peaks[i])
          {
              peakCount++;
              peakLocations[peakCount] = i;
          }
      }

      // If have too many peaks (local maxima), remove the lowest of them 
      if (peakCount > (expectedHarmonicCount+1))
      {
          var peakValues = new double[peakCount];
          for (int i = 0; i < peakCount; i++) peakValues[i] = values[peakLocations[i]];
          IEnumerable<double> ordered = peakValues.OrderByDescending(d => d);
          double avValue = ordered.Take(expectedHarmonicCount).Average();
          double min = ordered.Last();
          double threshold = min + ((avValue - min) / 2);
          // apply threshold to remove low peaks
          for (int i = 0; i < L; i++)
          {
              if ((peaks[i]) && (values[i] < threshold)) peaks[i] = false;
          }

          // recalculate the number of peaks
          peakCount = -1;
          for (int i = 0; i < L; i++)
          {
              if (peaks[i])
              {
                  peakCount++;
                  peakLocations[peakCount] = i;
              }
          }
      }
      
      // Now get amplitude (peak-trough) for remaining peaks.
      double amplitude = 0.0;
      for (int i = 0; i < peakCount; i++)
      {
          int troughIndex = peakLocations[i] + midPeriod;
          if (troughIndex >= L) troughIndex = peakLocations[i] - midPeriod;
          double delta = values[peakLocations[i]] - values[troughIndex];
          if (delta > 2.0) amplitude += delta; // dB threshold - required a minimum perceptible difference
      }
      double avAmplitude = amplitude / (double)peakCount;
      //if (row > 565)
      //{
      //    Console.Write(" ");
      //}
      return Tuple.Create(avAmplitude, peakCount);
  }



        /// <summary>
        /// Returns for each position in an array a periodicity score.
        /// That score is the maximum obtained for a range of periods over three cycles.
        /// This allowes the periodicity to change over the array and still return the maximum periodicity score.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>
  public static double[] PeriodicityDetection(double[] values, int minPeriod, int maxPeriod)
  {

      int L = values.Length;
      double[] oscillationScores = new double[L];
      int midPeriod = minPeriod + ((maxPeriod - minPeriod) / 2);
      int buffer = (int)(maxPeriod * 2.5);//avoid end of recording/array

      for (int r = 0; r < L - buffer; r++)
      {
          double maxScore = -double.MaxValue;
          for (int period = minPeriod; period < maxPeriod; period++)
          {
              double periodScore    = values[r] + values[r + period]  + values[r + (period * 2)];
              double offPeriodScore = values[r + (int)(period * 0.5)] + values[r + (int)(period * 1.5)] + values[r + (int)(period * 2.5)];
              periodScore -= offPeriodScore;
              if (periodScore > maxScore) maxScore = periodScore;
          }
          oscillationScores[r + midPeriod] = maxScore / 3;
      }
      return oscillationScores;
  }


        /// <summary>
        /// Searches an array of values for periodicity.
        /// Calls the method PeriodicityDetection() to obtain a score for every combination of period and phase between the passed min and max bounds.
        /// The score is an estimate of the maximum amplitude for all combinations of period and phase. 
        /// More accurately, the score is the difference between the average of the periodic indices and the average of the mid-period indices.
        /// Returns the maximum periodic score and the period at which it was obtained 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="minPeriod"></param>
        /// <param name="maxPeriod"></param>
        /// <returns></returns>
   
  public static System.Tuple<double, int, int> Periodicity(double[] values, int minPeriod, int maxPeriod)
  {

      double maxScore = -double.MaxValue;
      double returnScore = 0.0;
      int bestPeriod = 0;
      int bestPhase = 0;
      for (int period = minPeriod; period <= maxPeriod; period++)
      {
          for (int phase = 0; phase < period; phase++)
          {
              var result =  PeriodicityAndPhaseDetection(values, period, phase);
              double onPeriodScore  = result.Item1;
              //double offPeriodScore = result.Item2;
              //int onCount  = result.Item3;
              //int offCount = result.Item4;
              //double periodScore = onPeriodScore - offPeriodScore;
              double periodScore = onPeriodScore;

              if (periodScore > maxScore)
              {
                  maxScore = periodScore;
                  bestPeriod = period;
                  bestPhase  = phase;
                  //returnScore = (onPeriodScore / onCount) - (offPeriodScore / offCount);
                  //returnScore = periodScore;
                  returnScore = maxScore;
              }
          }//phase
      }//period

      return System.Tuple.Create(returnScore, bestPeriod, bestPhase);
  }



  /// <summary>
  /// returns the amplitude of an oscillation in an array having the given period.
  /// </summary>
  /// <param name="values"></param>
  /// <param name="period"></param>
  /// <param name="phase"></param>
  /// <returns></returns>
  public static System.Tuple<double, int> PeriodicityAndPhaseDetection(double[] values, int period, int phase)
  {
      int L = values.Length;
      int midPeriod = period / 2;
      double amplitude = 0.0;
      double[] smooth = DataTools.filterMovingAverage(values, midPeriod);
      bool[] peaks = DataTools.GetPeaks(smooth);
      int peakCount = DataTools.CountTrues(peaks);
      int count = 0;
      int index = phase;
      while (index < (L - midPeriod))
      {
          amplitude += (values[index + midPeriod] - values[index]);
          count++;
          index += period;
      }
      amplitude /= (double)peakCount;
      return System.Tuple.Create(amplitude, peakCount); // amplitude of oscillation i.e. difference between min and max values 
  }

  //============================================================================================================================


  public static int String2Int(string str)
  {
      int int32 = 0;
      try
      {
          int32 = Int32.Parse(str);
      }
      catch (System.FormatException ex)
      {
          System.Console.WriteLine("DataTools.String2Int(string str): WARNING! INVALID INTEGER=" + str);
          System.Console.WriteLine(ex);
          int32 = 0;
      }
      return int32;
  }



  //*************************************************************************************************************************************






  /**
   * Pads a string to given length.
   * @param str the string
   * @return formatted string.
   */
  static public String padString_pre(String str, int width)
  {
      while (str.Length < width) str = " " + str;
      return str;
  }
  /**
   * Pads a string to given length.
   * @param str the string
   * @return formatted string.
   */
  static public String padString_post(String str, int width)
  {
      while (str.Length < width) str = str + " ";
      return str;
  }

  /**
   * Rounds a double to n places and returns as double.
   * @param d the double value
   * @param places - maximum number of decimal places
   * @return formatted string.
   */
  static public double roundDouble(double d, int places)
  {
      int n = 10;
      for (int i = 1; i < places; i++) n *= 10;
      double rd = ((double)(Math.Round(d * n))) / n;
      return rd;
  }

  //=============================================================================
  




    }
}
