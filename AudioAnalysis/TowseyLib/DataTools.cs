using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    public class DataTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\"; 

        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");


            if (false) //test Submatrix()
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
                matrix = normalise(matrix, normMin, normMax);
                Console.WriteLine("\n\n");
                DataTools.writeMatrix(matrix);
                matrix = normalise(matrix, 0, 1);
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

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// Assume that r1<r2, c1<c2. 
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
            int smRows = r2 - r1 +1;
            int smCols = c2 - c1 + 1;

            double[,] sm = new double[smRows, smCols];

            for (int i = 0; i < smRows; i++)
            {
                for (int j = 0; j < smCols; j++)
                {   sm[i,j] = M[r1+i,c1+j];
                }
            }
            return sm;
        }

        /*
         * converts a matrix to a vector by concatenating columns.
         */
        public static double[] Matrix2Vector(double[,] M)
        {
            int width  = M.GetLength(0);
            int ht     = M.GetLength(1);
            double[] v = new double[width * ht];

            for (int i = 0; i < width; i++)
            {
                int offset = i * width;
                for (int j = 0; j < ht; j++)
                {   v[offset+j] = M[i,j];
                }
            }
            return v;
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
            getMinMax(V, out min, out max);
            double range = max - min;
            
            double[] Vnorm = new double[V.Length];

            for (int i = 0; i < V.Length; i++)
            {
                Vnorm[i] = (V[i] - min) / range;
                //Console.WriteLine(i + ", " + Vnorm[i]);
            }
            return Vnorm;
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
        public static void GetPercentileCutoffs(double[,] matrix, double min, double max, double minPercentile, double maxPercentile, out double minCut, out double maxCut)
        {
            if ((minPercentile == 0.0) && (maxPercentile == 1.0))
            {
                minCut = min;
                maxCut = max;
                return;
            }
            if (max < min) throw new ArgumentException("max must be greater than or equal to min");
            if (maxPercentile < minPercentile) throw new ArgumentException("maxPercentile must be greater than or equal to minPercentile");
            if (minPercentile < 0.0) throw new ArgumentException("minPercentile must be at least 0.0");
            if (maxPercentile > 1.0) throw new ArgumentException("maxPercentile must be at most 1.0");

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


  public static void writeArray(int[] array)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i + "  " + array[i]);
  }
  public static void writeArray(double[] array)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i+"  "+array[i]);
  }
  public static void writeArray(bool[] array)
  {
      for (int i = 0; i < array.Length; i++)
          Console.WriteLine(i + "  " + array[i]);
  }
  public static void writeMatrix(double[,] matrix)
  {
      int rowCount = matrix.GetLength(0);//height
      int colCount = matrix.GetLength(1);//width
      for (int i = 0; i < rowCount; i++)
      {
          for (int j = 0; j < colCount; j++)
          {
              Console.Write(" " + matrix[i, j].ToString("F2"));
              if(j<colCount-1)Console.Write(",");
          }
          Console.WriteLine();
      }
  }


  public static void writeBarGraph(int[] data)
  {
    int min;
    int max;
    getMinMax(data, out min, out max);
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
    }
    Console.WriteLine("Min="+min+" Max="+max+"  Scaling factor="+sf);
    Console.WriteLine();
  }

        static public void writeConciseHistogram(int[] data)
        {
            int min;
            int max;
            getMinMax(data, out min, out max);
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
      getMinMax(data, out min, out max);
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

  static public int[] Histo(double[,] data, int binCount, out double binWidth, out double min, out double max)
  {
      int rows = data.GetLength(0);
      int cols = data.GetLength(1);
      int[] histo = new int[binCount];
      min = double.MaxValue;
      max = -double.MaxValue;
      getMinMax(data, out min, out max);
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
      getMinMax(data, out min, out max);
      binWidth = (max - min) / binCount;

      for (int i = 0; i < length; i++)
      {
              int bin = (int)((double)(data[i] - min) / binWidth);
              if (bin >= binCount) bin = binCount - 1;
              histo[bin]++;
      }

      return histo;
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

        /// <summary>
        /// normalised matrix of real values to [0,1].
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[,] normalise(double[,] m)
        {
            double min = Double.MaxValue;
            double max = -Double.MaxValue;

            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            double[,] ret = new double[rows,cols];

            for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                if (m[i,j] > max)
                    max = m[i,j];
                if (m[i,j] < min)
                    min = m[i,j];
            }
            double range = max - min;

            for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                ret[i, j] = (m[i, j] - min) / range;

            return (ret);
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
                    if (dBels <= min) min = dBels;
                    else
                    if (dBels >= max) max = dBels;
                    m[i, j] = dBels;
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
        public static double[,] normalise(double[,] m, double normMin, double normMax)
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


        public static double[,] Blur(double[,] matrix, int nh)
        {
            if (nh <= 0) return matrix; //no blurring required

            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            int cellCount = ((2 * nh) + 1) * ((2 * nh) + 1);
            //double[,] newMatrix = new double[M, N];
            double[,] newMatrix = (double[,])matrix.Clone();

            for (int i = nh; i < (M - nh); i++)
                for (int j = nh; j < (N - nh); j++)
                {
                    double sum = 0.0;
                    for (int x = i - nh; x < (i + nh); x++)
                        for (int y = j - nh; y < (j + nh); y++) sum += matrix[x, y];
                    double v = sum / cellCount;
                    newMatrix[i, j] = v;
                }

            return newMatrix;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="cNH">column Window i.e. x-dimension</param>
        /// <param name="rNH">row Window i.e. y-dimension</param>
        /// <returns></returns>
        public static double[,] Blur(double[,] matrix, int cWindow, int rWindow)
        {
            if ((cWindow <= 0) && (rWindow <= 0)) return matrix; //no blurring required

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int cNH = cWindow / 2;
            int rNH = rWindow / 2;

            int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            double[,] newMatrix = new double[rows, cols];//init new matrix to return
            for (int r = rNH; r < (rows - rNH); r++)
                for (int c = cNH; c < (cols - cNH); c++)
                {
                    double sum = 0.0;
                    for (int y = (r - rNH); y <= (r + rNH); y++)
                    {
                        //System.Console.WriteLine(r+", "+c+ "  y="+y);
                        for (int x = (c - cNH); x <= (c + cNH); x++)
                        {
                            double d = matrix[y, x];
                            sum += matrix[y, x]; 
                        }
                    }
                    newMatrix[r, c] = sum / (double)area; 
                }
            return newMatrix;
        }
        



        /**
         * differs from above in that want area under curve = 1;
         * @param v
         * @return
         */
        public static double[] normaliseArea(int[] v)
        {
            int sum = 0;

            for (int i = 0; i < v.Length; i++) sum += v[i];

            double[] ret = new double[v.Length];
            for (int i = 0; i < v.Length; i++) ret[i] = v[i] / (double)(sum);

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
    Console.WriteLine("SS="+SS+"  norm="+norm);
    
    double[] ret = new double[length]; // vector to return
    for(int i=0; i<length; i++) 
    {
      ret[i] = ((double)v[i])/norm; 
    }
    
    return(ret); 
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
          if (distr[i] != 0.00) H -= distr[i] * Math.Log(distr[i]) / ln2;
      }
      return H;
  }
	
 	
 	/**
 	 * Calculates the relative entropy of the passed 
 	 * discrete probability distribution.
 	 * It is assumed that each of the elements in dist[] 
 	 * represents the probability of a symbol/state and the 
 	 * probabilities sum to 1.0
 	 * The relative entropy is with respect a uniform distribution. 
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
  
  /**
   * returns the min and max of a set of integer values
   * @param data
   * @return
   */
  static public void getMinMax(int[] data, out int min, out int max)
  { min = data[0];
    max = data[0];
    for(int i=1; i<data.Length; i++)
    { if(data[i] < min) min = data[i];
      if(data[i] > max) max = data[i];
    }
    //Console.WriteLine("min="+min+" max="+max);
  }

//=============================================================================

        static public void getMinMax(double[] data, out double min, out double max)
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

        /// <summary>
        /// returns the mina and max values in a matrix of doubles
        /// </summary>
        /// <param name="data"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        static public void getMinMax(double[,] data, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];
            for (int i = 1; i < rows; i++)
            for (int j = 1; j < cols; j++)
            {
                if (data[i,j] < min)
                {
                    min = data[i,j];
                }else
                if (data[i,j] > max)
                {
                    max = data[i,j];
                }
            }//end double loop
        }
        /**
         * returns the min and max of an array of doubles
         * and the index for both.
         * @param data
         * @return 
         */  
  static public void getMinMax(double[] data, out int indexMin, out int indexMax, out double min, out double max)
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
      getMinMax(data, out min, out max);
      Console.WriteLine("Array Min={0:F5}  Array Max={1:F5}", min, max);
  }
  static public void WriteMinMaxOfArray(string arrayname, double[] data)
  {
      double min;
      double max;
      getMinMax(data, out min, out max);
      Console.WriteLine(arrayname+":  Min={0:F5}  Max={1:F5}", min, max);
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

  public static double ImageThreshold(double[,] M)
  {
    int indexBias = 8;
    int binCount = 50;
    double binWidth;
    double min; 
    double max;
    int[] powerHisto = DataTools.Histo(M, binCount, out binWidth, out min, out max);
    powerHisto[binCount-1] = 0; //just in case it is the max
    double[] smooth = DataTools.filterMovingAverage(powerHisto, 5);
    int maxindex;
    DataTools.getMaxIndex(smooth, out maxindex);
    int i = maxindex + indexBias;
    if (i > binCount) i = maxindex;
    double threshold = min + (i * binWidth);

    //for (int b = 0; b < binCount; b++) powerHisto[b] = (int)(smooth[b] / (double)2);
    //DataTools.writeBarGraph(powerHisto);
    return threshold;
  }

  public static double[] ImageThreshold(double[,] M, int bandCount)
  {
      int height = M.GetLength(0);
      int width  = M.GetLength(1);
      double bandWidth = width / (double)bandCount;

      double[] thresholds = new double[bandCount];
      for (int b = 0; b < bandCount; b++)//for all bands
      {
          int start = (int)(b * bandWidth);
          int stop = (int)((b+1) * bandWidth);
          if (stop >= width) stop = width - 1;

          double[,] subMatrix = Submatrix(M, 0, start, height-1, stop);
          
          thresholds[b] = ImageThreshold(subMatrix);
          //Console.WriteLine("Threshold " + b + " = " + thresholds[b]);
      }
      return thresholds;
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
    if(length <= 3) return signal;   // no worth the effort!
    
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


  //=============================================================================


  /**
   * Formats a double as a string.
   * @param d the double value
   * @param places - maximum number of decimal places
   * @return formatted string.
   */
  static public String formatDouble(double d, int places)
  {
      //NumberFormat form = NumberFormat.getInstance(Locale.ENGLISH);
      //form.setMinimumIntegerDigits(1);
      //form.setMinimumFractionDigits(places);
      //form.setMaximumFractionDigits(places);
      ////System.out.println("N="+form.format(d));
      //return form.format(d);
      return "TODO: fix this method! ";
  }

  /**
   * Formats a double as a string.
   * @param d the double value
   * @param intPlaces - minimum number of integer places
   * @param decPlaces - maximum number of decimal places
   * @return formatted string.
   */
  static public String formatDouble(double d, int intPlaces, int decPlaces)
  {
      //NumberFormat form = NumberFormat.getInstance(Locale.ENGLISH);
      //form.setMinimumIntegerDigits(intPlaces);
      //form.setMinimumFractionDigits(decPlaces);
      //form.setMaximumFractionDigits(decPlaces);
      ////System.out.println("N="+form.format(d));
      return "TODO: fix this method! ";
      //return form.format(d);
  }

  /**
   * Formats an integer as a string.
   * @param num the integer value
   * @return formatted string.
   */
  static public String formatInteger(int num)
  {
      //String str = Integer.toString(num);
      //if (num >= 10000) return str;
      //if (num >= 1000) return " " + str;
      //if (num >= 100) return "  " + str;
      //if (num >= 10) return "   " + str;
      //if (num >= 0) return "    " + str;
      //return str;
      return "TODO: fix this method! ";
  }
  /**
   * Formats an integer as a string.
   * @param num the integer value
   * @return formatted string.
   */
  static public String formatInteger(int num, int width)
  {
      //String str = Int32.toString(num);
      //return padString_pre(str, width);
      return "TODO: fix this method! ";

  }

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
