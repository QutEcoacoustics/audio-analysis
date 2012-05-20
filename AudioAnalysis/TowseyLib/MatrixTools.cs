using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TowseyLib
{
    public class MatrixTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\"; 

        static void Main()
        {
            Console.WriteLine("TESTING METHODS IN CLASS DataTools");

            bool doit1 = false;
            if (doit1) //test Submatrix()
            {
                Console.WriteLine(""); 
                //string fName = testDir + "testOfReadingMatrixFile.txt";
                //double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                ////int rowCount = matrix.GetLength(0);//height
                ////int colCount = matrix.GetLength(1);//width
                ////Console.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                //DataTools.writeMatrix(matrix);
                //Console.WriteLine("");
                //int r1 = 2;
                //int c1 = 3;
                //int r2 = 4;
                //int c2 = 6;
                //Console.WriteLine("r1="+r1+" c1="+c1+" r2="+r2+" c2="+c2);
                //Console.WriteLine("Indices start at [0,0] in top left.");
                //int smRows = r2 - r1 + 1;
                //int smCols = c2 - c1 + 1;
                //Console.WriteLine("Submatrix has " + smRows + " rows and " + smCols + " columns");
                //double[,] sub = Submatrix(matrix, r1, c1, r2, c2);
                //DataTools.writeMatrix(sub);
            }//end test ReadDoubles2Matrix(string fName)


            if (true) //test normalise(double[,] m, double normMin, double normMax)
            {   
                //string fName = testDir + "testOfReadingMatrixFile.txt";
                //double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                //Console.WriteLine("\n");
                //DataTools.writeMatrix(matrix);
                //double normMin=-2.0;
                //double normMax= 1.0;
                //matrix = Normalise(matrix, normMin, normMax);
                //Console.WriteLine("\n\n");
                //DataTools.writeMatrix(matrix);
                //matrix = Normalise(matrix, 0, 1);
                //Console.WriteLine("\n\n");
                //DataTools.writeMatrix(matrix);

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
        /// Subtract matrix m2 from matrix m1
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        public static bool MatrixDimensionsAreEqual(double[,] m1, double[,] m2)
        {
            if ((m1.GetLength(0) == m2.GetLength(0)) && (m1.GetLength(1) == m2.GetLength(1))) return true;
            //  throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            return false;
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
        public static double[,] ThresholdMatrix2Binary(double[,] M, double threshold)
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

  public static double GetRowSum(double[,] m, int rowID)
  {
      double[] row = GetRow(m, rowID);
      return row.Sum();
  }

  public static int SumRow(int[,] m, int rowID)
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

  public static double[] GetRowAverages(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double sum = 0.0;
      double[] rowSums = new double[rows];
      for (int r = 0; r < rows; r++)
      {
          sum = 0.0;
          for (int c = 0; c < cols; c++)
          {
              sum += m[r, c];
          }
          rowSums[r] = sum / cols;
      }
      return rowSums;
  }



		/// <summary>
		/// normalise matrix of real values to [0,1].
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



        /// <summary>
        /// a difference filter over the rows from row 0.
        /// To be used to remove background noise from sonogram
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
    public static double[,] FilterMatrixRows(double[,] m)
    {
        int rowCount = m.GetLength(0);
        int colCount = m.GetLength(1);
        double[,] returnMatrix = new double[rowCount, colCount];

        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 1; c < colCount; c++)
            {
                returnMatrix[r, c] = m[r, c] - m[r, c-1];
            }
        }

        return returnMatrix;
    }

//===========================================================================================================================================================



        /// <summary>
        /// shift values by their mean.
        /// </summary>
        //public static double[,] DiffFromMean(double[,] m)
        //{
        //    int rows = m.GetLength(0);
        //    int cols = m.GetLength(1);
        //    double av; double sd;
        //    NormalDist.AverageAndSD(m, out av, out sd);  

        //    double[,] ret = new double[rows,cols];

        //    for (int i = 0; i < rows; i++)
        //        for (int j = 0; j < cols; j++)
        //            ret[i, j] = m[i, j] - av;

        //    return ret;
        //}



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



//***************************************************************************************************************************************

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

  //=============================================================================





    } //class MatrixTools
}
