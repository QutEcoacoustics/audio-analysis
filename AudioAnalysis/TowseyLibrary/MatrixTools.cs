using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace TowseyLibrary
{
    public class MatrixTools
    {
        private static string testDir = @"D:\SensorNetworks\Software\TowseyLib\TestResources\"; 

        static void Main()
        {
            LoggedConsole.WriteLine("TESTING METHODS IN CLASS DataTools");

            bool doit1 = false;
            if (doit1) //test Submatrix()
            {
                LoggedConsole.WriteLine(""); 
                //string fName = testDir + "testOfReadingMatrixFile.txt";
                //double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                ////int rowCount = matrix.GetLength(0);//height
                ////int colCount = matrix.GetLength(1);//width
                ////LoggedConsole.WriteLine("rowCount=" + rowCount + "  colCount=" + colCount);
                //DataTools.writeMatrix(matrix);
                //LoggedConsole.WriteLine("");
                //int r1 = 2;
                //int c1 = 3;
                //int r2 = 4;
                //int c2 = 6;
                //LoggedConsole.WriteLine("r1="+r1+" c1="+c1+" r2="+r2+" c2="+c2);
                //LoggedConsole.WriteLine("Indices start at [0,0] in top left.");
                //int smRows = r2 - r1 + 1;
                //int smCols = c2 - c1 + 1;
                //LoggedConsole.WriteLine("Submatrix has " + smRows + " rows and " + smCols + " columns");
                //double[,] sub = Submatrix(matrix, r1, c1, r2, c2);
                //DataTools.writeMatrix(sub);
            }//end test ReadDoubles2Matrix(string fName)


            if (true) //test normalise(double[,] m, double normMin, double normMax)
            {   
                //string fName = testDir + "testOfReadingMatrixFile.txt";
                //double[,] matrix = FileTools.ReadDoubles2Matrix(fName);
                //LoggedConsole.WriteLine("\n");
                //DataTools.writeMatrix(matrix);
                //double normMin=-2.0;
                //double normMax= 1.0;
                //matrix = Normalise(matrix, normMin, normMax);
                //LoggedConsole.WriteLine("\n\n");
                //DataTools.writeMatrix(matrix);
                //matrix = Normalise(matrix, 0, 1);
                //LoggedConsole.WriteLine("\n\n");
                //DataTools.writeMatrix(matrix);

            }//end test normalise(double[,] m, double normMin, double normMax)


            //COPY THIS TEST TEMPLATE
            if (false) //test Method(parameters)
            {
            }//end test Method(string fName)


            LoggedConsole.WriteLine("FINISHED"); //end
            LoggedConsole.WriteLine("FINISHED"); //end
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


        public static bool CentreIsLocalMaximum(double[,] m, double threshold)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            int centreRow = rows / 2;
            int centreCol = cols / 2;
            double centreValue = m[centreRow, centreCol];
            double sum = 0;
            int count = rows * cols;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] > centreValue) return false;
                    sum += m[r, c];
                }
            }
            sum -= centreValue;
            double av = sum / (double)(count - 1);
            if ((centreValue - av) < threshold) return false;
            //  throw new Exception("ERROR! Matrix dims must be same for matrix subtraction.");
            return true;
        }





        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// The returned submatrix includes the rows and column passed as bounds.
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

        /// <summary>
        /// Returns an array of row averages in the submatrix of passed matrix.
        /// This method combines two methods, Submatrix() & GetRowAverages(), for efficiency
        /// Assume that r1 < r2, c1 < c2. 
        /// Row, column indices start at 0
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static double[] GetRowAveragesOfSubmatrix(double[,] M, int r1, int c1, int r2, int c2)
        {
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;

            double[] array = new double[subRowCount];

            for (int i = 0; i < subRowCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < subColCount; j++)
                {
                    sum += M[r1 + i, c1 + j];
                    array[i] = sum / (double)subColCount;
                }
            }
            return array;
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
        public static int[,] ThresholdMatrix2Binary(double[,] M, double threshold)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            var op = new int[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > threshold) op[r, c] = 1;
                }
            }
            return op;
        }

        public static double[,] Matrix2LogValues(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    op[r, c] = Math.Log10(M[r, c]);
                }
            }
            return op;
        }

        public static double[,] Matrix2ZScores(double[,] M, double av, double sd)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double[,] op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    op[r, c] = (M[r, c] - av) / sd;
                }
            }
            return op;
        }



        public static double[,] SquareValues(double[,] M)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = M[i, j] * M[i, j];
                }
            return newM;
        }

        /// <summary>
        /// Filters background values by applying a polynomial that lies between y=x and y=x^2.
        /// That is, y=x represents the unfiltered matrix and y=x^2 represents the maximally filtered matrix.
        /// In a grey scale image, this has the effect of diminshing the low amplitude values, thereby enhancing the highlights.
        /// 
        /// </summary>
        /// <param name="M"></param>
        /// <param name="filterCoeff"></param>
        /// <returns></returns>
        public static double[,] FilterBackgroundValues(double[,] M, double filterCoeff)
        {
            if (filterCoeff >= 1.0) return M;
            if (filterCoeff <= 0.1) return MatrixTools.SquareValues(M);

            double param = 1 / (double)filterCoeff;
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double[,] newM = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    newM[i, j] = (((param - 1) * (M[i, j] * M[i, j])) + M[i, j]) / param;
                }
            }
            return newM;
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
        /// Sets any element in matrix with value> 0.0 to zero if all surrounding elements also = zero
        /// </summary>
        /// <param name="m"></param>
        public static void SetSingletonsToZero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            int count = 0;
            int total = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] <= 0.0) continue;
                    total++;
                    if ( (m[r, c + 1] == 0.0) && (m[r - 1, c + 1] == 0.0) && (m[r - 1, c] == 0.0) && (m[r - 1, c - 1] == 0.0)
                      && (m[r, c - 1] == 0.0) && (m[r + 1, c - 1] == 0.0) && (m[r + 1, c] == 0.0) && (m[r + 1, c + 1] == 0.0))
                    {
                        m[r, c] = 0.0;
                        count++;
                    }

                }
            } // for loop
            LoggedConsole.WriteLine("Zeroed {0} of {1} non-zero cells.", count, total);
        } // SetSingletonsToZero


        /// <summary>
        /// Sets any element in matrix with value> 0.0 to zero if all surrounding elements also = zero
        /// </summary>
        /// <param name="m"></param>
        public static void SetDoubletsToZero(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            int total = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (m[r, c] <= 0.0) continue;

                    // check if more than two poi's in nearest neighbours.
                    int count = 0;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            if (m[r + i, c + j] > 0.0) count++;
                        }
                    }
                    if (count > 2) continue;

                    // now check the 8 directions. Assume adjacent because have already called PruneSingletons();
                    if ((m[r, c + 1] > 0.0) && (m[r, c + 2] > 0.0)) continue; // three in a row 
                    if ((m[r - 1, c + 1] > 0.0) && (m[r - 2, c + 2] > 0.0)) continue; // three on a diagonal 
                    if ((m[r - 1, c] > 0.0) && (m[r - 2, c] > 0.0)) continue; // three in a col 
                    if ((m[r - 1, c - 1] > 0.0) && (m[r - 2, c - 2] > 0.0)) continue; // three on a diagonal 
                    if ((m[r, c - 1] > 0.0) && (m[r, c - 2] > 0.0)) continue; // three in a row 
                    if ((m[r + 1, c - 1] > 0.0) && (m[r + 2, c - 2] > 0.0)) continue; // three on a diagonal 
                    if ((m[r + 1, c] > 0.0) && (m[r + 2, c] > 0.0)) continue; // three in a col 
                    if ((m[r + 1, c + 1] > 0.0) && (m[r + 2, c + 2] > 0.0)) continue; // three on a diagonal 

                    //if ((m[r - 1, c] > 0.0) && (m[r + 1, c] == 0.0)) continue; // three in a column 
                    //if ((m[r - 1, c - 1] > 0.0) && (m[r + 1, c + 1] == 0.0)) continue; // three on a diagonal
                    //if ((m[r - 1, c + 1] > 0.0) && (m[r + 1, c - 1] == 0.0)) continue; // three on a diagonal

                    // if get to here then must be a doublet.
                    total++;
                    // zero all cells
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            m[r + i, c + j] = 0.0;
                        }
                    }
                }
            } // for loop
            LoggedConsole.WriteLine("Removed {0} doublets.", total);
        } // SetSingletonsToZero


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
              LoggedConsole.Write(" " + matrix[i, j].ToString(format));
              if(j<colCount-1)LoggedConsole.Write(",");
          }
          LoggedConsole.WriteLine();
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
              LoggedConsole.Write(" " + matrix[i, j]);
              if (j < colCount - 1) LoggedConsole.Write(",");
          }
          LoggedConsole.WriteLine();
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
              LoggedConsole.Write(" " + matrix[i, j].ToString("F2"));
              if (j < colCount - 1) LoggedConsole.Write(",");
          }
          LoggedConsole.WriteLine();
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
      if (m1 == null) return m2;
      if (m2 == null) return m1;
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

    public static double[,] SubtractValuesFromOne(double[,] m)
    {
        int rows = m.GetLength(0);
        int cols = m.GetLength(1);

        double[,] newMatrix = new double[rows, cols];
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                newMatrix[r, c] = 1 - m[r, c];
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
  public static int SumColumn(byte[,] m, int colID)
  {
      int rows = m.GetLength(0);
      int sum = 0;
      for (int i = 0; i < rows; i++) sum += m[i, colID];
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

  public static double[] SumRows(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      double[] rowSums = new double[rows];
      for (int r = 0; r < rows; r++)
      {
          double sum = 0.0;
          for (int c = 0; c < cols; c++)
          {
              sum += m[r, c];
          }
          rowSums[r] = sum;
      }
      //sum = 0.0;
      //for (int j = 0; j < cols; j++) sum += colSums[j];
      //LoggedConsole.WriteLine("sum="+sum.ToString("F5"));
      return rowSums;
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
      //LoggedConsole.WriteLine("sum="+sum.ToString("F5"));
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

  public static double SumPositiveDiagonal(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols) return Double.NaN;
      double sum = 0.0;
      for (int r = 0; r < rows; r++)
      {
          sum += m[r, cols-1-r];
      }
      return sum;
  }
  public static void SumTriangleAbovePositiveDiagonal(double[,] m, out double sum, out int count)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols)
      {    
          sum = Double.NaN;
          count = 0;
          return;
      }
      sum = 0.0;
      count = 0;
      for (int r = 0; r < rows-1; r++)
      {
          for (int c = 0; c < cols - 1 - r; c++)
          {
              sum += m[r, c];
              count ++;
          }
      }
  }
  public static void SumTriangleBelowPositiveDiagonal(double[,] m, out double sum, out int count)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols)
      {
          sum = Double.NaN;
          count = 0;
          return;
      }
      sum = 0.0;
      count = 0;
      for (int r = 1; r < rows; r++)
      {
          for (int c = (cols-r); c < cols; c++)
          {
              sum += m[r, c];
              count++;
          }
      }
  }
  public static void AverageValuesInTriangleAboveAndBelowPositiveDiagonal(double[,] m, out double upperAv, out double lowerAv)
  {
      int count;
      double sum;
      MatrixTools.SumTriangleAbovePositiveDiagonal(m, out sum, out count);
      upperAv = sum / (double)count;
      MatrixTools.SumTriangleBelowPositiveDiagonal(m, out sum, out count);
      lowerAv = sum / (double)count;
  }

  public static double SumNegativeDiagonal(double[,] m)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols) return Double.NaN;
      double sum = 0.0;
      for (int r = 0; r < rows; r++)
      {
          sum += m[r, r];
      }
      return sum;
  }
  public static void SumTriangleAboveNegativeDiagonal(double[,] m, out double sum, out int count)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols)
      {
          sum = Double.NaN;
          count = 0;
          return;
      }
      sum = 0.0;
      count = 0;
      for (int r = 0; r < rows - 1; r++)
      {
          for (int c = r+1; c < cols; c++)
          {
              sum += m[r, c];
              count++;
          }
      }
  }
  public static void SumTriangleBelowNegativeDiagonal(double[,] m, out double sum, out int count)
  {
      int rows = m.GetLength(0);
      int cols = m.GetLength(1);
      if (rows != cols)
      {
          sum = Double.NaN;
          count = 0;
          return;
      }
      sum = 0.0;
      count = 0;
      for (int r = 1; r < rows; r++)
      {
          for (int c = 0; c < r; c++)
          {
              sum += m[r, c];
              count++;
          }
      }
  }
  public static void AverageValuesInTriangleAboveAndBelowNegativeDiagonal(double[,] m, out double upperAv, out double lowerAv)
  {
      int count;
      double sum;
      MatrixTools.SumTriangleAboveNegativeDiagonal(m, out sum, out count);
      upperAv = sum / (double)count;
      MatrixTools.SumTriangleBelowNegativeDiagonal(m, out sum, out count);
      lowerAv = sum / (double)count;
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
        /// Rotates a matrix 90 degrees clockwise.
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static int[,] MatrixRotate90Clockwise(int[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new int[cols, rows];
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


        /// <summary>
        /// Multiplies two matrices by summing m1[r,c]*m2[r,c]
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
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
            {
                for (int j = 0; j < cols1; j++)
                    sum += (m1[i, j] * m2[i, j]);
            }
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
            //LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

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
            //LoggedConsole.WriteLine("range ="+ range+"  normRange="+normRange);

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
        /// <summary>
        /// returns the min and max values in a matrix
        /// </summary>
        static public void MinMax(int[,] data, out int min, out int max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];
            for (int i = 1; i < rows; i++)
                for (int j = 1; j < cols; j++)
                {
                    if (data[i, j] < min)
                        min = data[i, j];
                    else if (data[i, j] > max)
                        max = data[i, j];
                }//end double loop
        }

  //=============================================================================



        public static double[,] SmoothInTemporalDirectionOnly(double[,] matrix, int window)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var smoothMatrix = new double[rows, cols];
            for (int c = 0; c < cols; c++)
            {
                var array = DataTools.GetColumn(matrix, c);
                array = DataTools.filterMovingAverage(array, window);
                DataTools.SetColumn(smoothMatrix, c, array);
            }
            return smoothMatrix;
        }

        public static byte[,] IdentifySpectralRidges(double[,] matrix)
        {
            var m1 = matrix;

            var binary1 = IdentifySpectralRidgesInTemporalDirection(m1);
            binary1 = JoinDisconnectedRidgesInBinaryMatrix1(binary1, m1);

            var m2 = DataTools.MatrixTranspose(m1);
            var binary2 = IdentifySpectralRidgesInFreqDirection(m2);
            binary2 = JoinDisconnectedRidgesInBinaryMatrix1(binary2, m2);
            binary2 = DataTools.MatrixTranspose(binary2);

            //merge the two binary matrices
            int rows = binary1.GetLength(0);
            int cols = binary1.GetLength(1);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    if (binary2[r, c] == 1) binary1[r, c] = 1;
                }

            //int rows = matrix.GetLength(0);
            //int cols = matrix.GetLength(1);
            //byte[,] binary1 = new byte[rows,cols];
            //for (int r = 0; r < rows; r++)
            //    for (int c = 0; c < cols; c++)
            //    {
            //        if ((r % 3 == 0) && (c % 3 == 0)) binary1[r, c] = 1;
            //    }

            return binary1;
        }

        public static byte[,] IdentifySpectralRidgesInFreqDirection(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 0; r < rows; r++) //row at a time, each row = one frame.
            {
                double[] row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, 3); //## SMOOTH FREQ BIN - high value breaks up vertical tracks
                for (int c = 3; c < cols - 3; c++)
                {
                    double d1 = row[c] - row[c - 1];
                    double d2 = row[c] - row[c + 1];
                    double d3 = row[c] - row[c - 2];
                    double d4 = row[c] - row[c + 2];
                    //identify a peak
                    if ((d1 > 0.0) && (d2 > 0.0) && (d3 > 0.0) && (d4 > 0.0) && (row[c] > row[c - 3])
                        && (row[c] > row[c + 3])
                        //&& (d1 > d2)
                        ) binary[r, c] = 1;
                } //end for every col
            } //end for every row
            return binary;
        }

        public static byte[,] IdentifySpectralRidgesInTemporalDirection(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL RIDGES
            var binary = new byte[rows, cols];
            for (int r = 0; r < rows; r++) //row at a time, each row = one frame.
            {
                double[] row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, 3);
                //## SMOOTH FRAME SPECTRUM - high value breaks up horizontal tracks
                for (int c = 3; c < cols - 3; c++)
                {
                    //identify a peak
                    if ((row[c] > row[c - 1]) && (row[c] > row[c + 1]) && (row[c] > row[c - 2]) && (row[c] > row[c + 2])
                        && (row[c] > row[c - 3]) && (row[c] > row[c + 3])
                        //&& (row[c] > row[c - 4]) && (row[c] > row[c + 4])
                        //&& (row[c] > row[c - 4]) && (row[c] > row[c - 5])
                        ) binary[r, c] = 1;
                } //end for every col
            } //end for every row
            return binary;
        }


        /// <summary>
        ///JOINS DISCONNECTED RIDGES
        /// </summary>
        /// <returns></returns>
        public static byte[,] JoinDisconnectedRidgesInBinaryMatrix(byte[,] binary, double[,] matrix)
        {
            double threshold = 20.0;
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];

            for (int r = 0; r < rows - 3; r++) //row at a time, each row = one frame.
            {
                for (int c = 3; c < cols - 3; c++)
                {
                    if (binary[r, c] == 0) continue; //no peak to join
                    if (matrix[r, c] < threshold)
                    {
                        binary[r, c] = 0;
                        continue; // peak too weak to join
                    }

                    newM[r, c] = 1; // pixel r,c = 1.0
                    // skip if adjacent pixels in next row also = 1.0
                    if (binary[r + 1, c] == 1) continue;
                    if (binary[r + 1, c - 1] == 1) continue;
                    if (binary[r + 1, c + 1] == 1) continue;

                    // fill in the same column
                    if ((binary[r + 3, c] == 1.0)) newM[r + 2, c] = 1; //fill gap
                    if ((binary[r + 2, c] == 1.0)) newM[r + 1, c] = 1; //fill gap

                    if ((binary[r + 2, c - 3] == 1.0)) newM[r + 1, c - 2] = 1; //fill gap
                    if ((binary[r + 2, c + 3] == 1.0)) newM[r + 1, c + 2] = 1; //fill gap


                    //if ((binary[r + 2, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    //if ((binary[r + 2, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap

                    if ((binary[r + 1, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    if ((binary[r + 1, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap
                }
            }
            return newM;
        }

        public static byte[,] JoinDisconnectedRidgesInBinaryMatrix1(byte[,] binary, double[,] matrix)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];

            for (int r = 0; r < rows - 3; r++) //row at a time, each row = one frame.
            {
                for (int c = 3; c < cols - 3; c++)
                {
                    if (binary[r, c] == 0.0) continue;

                    newM[r, c] = 1;
                    // pixel r,c = 1.0 - skip if adjacent pixels in next row also = 1.0
                    if (binary[r + 1, c] == 1) continue;
                    if (binary[r + 1, c - 1] == 1) continue;
                    if (binary[r + 1, c + 1] == 1) continue;

                    //fill in the same column
                    if ((binary[r + 3, c] == 1.0)) newM[r + 2, c] = 1; //fill gap
                    if ((binary[r + 2, c] == 1.0)) newM[r + 1, c] = 1; //fill gap

                    if ((binary[r + 2, c - 3] == 1.0)) newM[r + 1, c - 2] = 1; //fill gap
                    if ((binary[r + 2, c + 3] == 1.0)) newM[r + 1, c + 2] = 1; //fill gap


                    //if ((binary[r + 2, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    //if ((binary[r + 2, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap

                    if ((binary[r + 1, c - 2] == 1.0)) newM[r + 1, c - 1] = 1; //fill gap
                    if ((binary[r + 1, c + 2] == 1.0)) newM[r + 1, c + 1] = 1; //fill gap
                }
            }
            return newM;
        }



        /// <summary>
        /// REMOVE ORPHAN PEAKS
        /// </summary>
        /// <param name="binary"></param>
        /// <returns></returns>
        public static byte[,] RemoveOrphanOnesInBinaryMatrix(byte[,] binary)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] newM = new byte[rows, cols];
            for (int r = 1; r < rows - 1; r++) //row at a time, each row = one frame.
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    if (binary[r, c] == 0.0) continue;
                    newM[r, c] = 1;
                    if ((binary[r - 1, c] == 0) && (binary[r + 1, c] == 0.0) && (binary[r + 1, c + 1] == 0)
                        && (binary[r, c + 1] == 0.0) && (binary[r - 1, c + 1] == 0.0) && (binary[r + 1, c - 1] == 0)
                        && (binary[r, c - 1] == 0.0) && (binary[r - 1, c - 1] == 0.0)) newM[r, c] = 0;
                }
            }
            return newM;
        }

        public static byte[,] ThresholdBinarySpectrum(byte[,] binary, double[,] m, double threshold)
        {
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);
            byte[,] mOut = new byte[rows, cols];
            for (int r = 1; r < rows - 1; r++) //row at a time, each row = one frame.
            {
                for (int c = 1; c < cols - 1; c++)
                {
                    //LoggedConsole.WriteLine("m[r, c]=" + m[r, c]);
                    if (binary[r, c] == 0) continue;
                    if (m[r, c] < threshold)
                    {
                        mOut[r, c] = 0;
                    }
                    else mOut[r, c] = 1;
                }
            }
            return mOut;
        }


        public static double[,] IdentifySpectralPeaks(double[,] matrix)
        {
            double buffer = 3.0; //dB peak requirement
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //A: CONVERT MATRIX to BINARY FORM INDICATING SPECTRAL PEAKS
            double[,] binary = new double[rows, cols];
            for (int r = 2; r < rows - 2; r++) //row at a time, each row = one frame.
            {
                for (int c = 2; c < cols - 2; c++)
                {
                    //identify a peak
                    if ((matrix[r, c] > matrix[r, c - 2] + buffer) && (matrix[r, c] > matrix[r, c + 2] + buffer)
                        //same row
                        && (matrix[r, c] > matrix[r - 2, c] + buffer) && (matrix[r, c] > matrix[r + 2, c] + buffer)
                        //same col
                        && (matrix[r, c] > matrix[r - 1, c - 1] + buffer)
                        && (matrix[r, c] > matrix[r + 1, c + 1] + buffer) //diagonal
                        && (matrix[r, c] > matrix[r - 1, c + 1] + buffer)
                        && (matrix[r, c] > matrix[r + 1, c - 1] + buffer)) //other diag
                    {
                        binary[r, c] = 1.0; // maxIntensity;
                        binary[r - 1, c - 1] = 1.0; // maxIntensity;
                        binary[r + 1, c + 1] = 1.0; // maxIntensity;
                        binary[r - 1, c + 1] = 1.0; // maxIntensity;
                        binary[r + 1, c - 1] = 1.0; // maxIntensity;
                        binary[r, c - 1] = 1.0; // maxIntensity;
                        binary[r, c + 1] = 1.0; // maxIntensity;
                        binary[r - 1, c] = 1.0; // maxIntensity;
                        binary[r + 1, c] = 1.0; // maxIntensity;
                    }
                    //else binary[r, c] = 0.0; // minIntensity;
                } //end for every col
                //binary[r, 0] = 0; // minIntensity;
                //binary[r, 1] = 0; // minIntensity;
                //binary[r, cols - 2] = 0; //minIntensity;
                //binary[r, cols - 1] = 0; //minIntensity;
            } //end for every row

            return binary;
        }


        /// <summary>
        /// CONVERTs a binary matrix of spectral peak tracks to an output matrix containing the acoustic intensity
        /// in the neighbourhood of those peak tracks.
        /// </summary>
        /// <param name="binary">The spectral peak tracks</param>
        /// <param name="matrix">The original sonogram</param>
        /// <returns></returns>
        public static double[,] SpectralRidges2Intensity(byte[,] binary, double[,] sonogram)
        {
            //speak track neighbourhood
            int rNH = 5;
            int cNH = 1;

            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(sonogram, out minIntensity, out maxIntensity);

            int rows = sonogram.GetLength(0);
            int cols = sonogram.GetLength(1);
            double[,] outM = new double[rows, cols];
            //initialise the output matrix/sonogram to the minimum acoustic intensity
            for (int r = 0; r < rows; r++) //init matrix to min
            {
                for (int c = 0; c < cols; c++) outM[r, c] = minIntensity; //init output matrix to min value
            }

            double localdb;
            for (int r = rNH; r < rows - rNH; r++) //row at a time, each row = one frame.
            {
                for (int c = cNH; c < cols - cNH; c++)
                {
                    if (binary[r, c] == 0.0) continue;

                    localdb = sonogram[r, c] - 3.0; //local lower bound = twice min perceptible difference
                    //scan neighbourhood
                    for (int i = r - rNH; i <= (r + rNH); i++)
                    {
                        for (int j = c - cNH; j <= (c + cNH); j++)
                        {
                            if (sonogram[i, j] > localdb) outM[i, j] = sonogram[i, j];
                            if (outM[i, j] < minIntensity) outM[i, j] = minIntensity;
                        }
                    } //end local NH
                }
            }
            return outM;
        }


        public static byte[,] PickOutLines(byte[,] binary)
        {
            int N = 7;
            int L = N - 1;
            int side = N / 2;
            int threshold = N - 1; //6 out 7 matches required

            //initialise the syntactic elements - four straight line segments
            int[,] LH00 = new int[2, L]; //{ {0,0,0,0,0,0 }, {-3,-2,-1,1,2,3 } };
            for (int i = 0; i < L; i++) LH00[0, i] = 0;
            for (int i = 0; i < side; i++) LH00[1, i] = i - side;
            for (int i = 0; i < side; i++) LH00[1, side + i] = i + 1;

            int[,] LV90 = new int[2, L]; // = { { -3, -2, -1, 1, 2, 3 }, { 0, 0, 0, 0, 0, 0 } };
            for (int i = 0; i < L; i++) LV90[1, i] = 0;
            for (int i = 0; i < side; i++) LV90[0, i] = i - side;
            for (int i = 0; i < side; i++) LV90[0, side + i] = i + 1;


            int[,] Lp45 = { { 3, 2, 1, -1, -2, -3 }, { -3, -2, -1, 1, 2, 3 } };
            int[,] Lm45 = { { -3, -2, -1, 1, 2, 3 }, { -3, -2, -1, 1, 2, 3 } };
            int rows = binary.GetLength(0);
            int cols = binary.GetLength(1);

            byte[,] op = new byte[rows, cols];
            for (int r = side; r < rows - side; r++) //row at a time, each row = one frame.
            {
                for (int c = side; c < cols - side; c++)
                {
                    int HL00sum = binary[r, c];
                    int VL90sum = binary[r, c];
                    int Lm45sum = binary[r, c];
                    int Lp45sum = binary[r, c];

                    for (int i = 0; i < L; i++)
                    {
                        if (binary[r + LH00[0, i], c + LH00[1, i]] == 1) HL00sum++;
                        if (binary[r + LV90[0, i], c + LV90[1, i]] == 1) VL90sum++;
                        //      if (binary[r + Lm45[0, i], c + Lm45[1, i]] == 1) Lm45sum++;
                        //      if (binary[r + Lp45[0, i], c + Lp45[1, i]] == 1) Lp45sum++;
                    }

                    int[] scores = new int[4];
                    scores[0] = HL00sum;
                    scores[1] = Lp45sum;
                    scores[2] = VL90sum;
                    scores[3] = Lm45sum;
                    int maxIndex = 0;
                    DataTools.getMaxIndex(scores, out maxIndex);

                    if ((maxIndex == 0) && (HL00sum >= threshold))
                    {
                        for (int i = 0; i < L; i++) op[r + LH00[0, i], c + LH00[1, i]] = 1;
                    }
                    //if ((maxIndex == 1) && (Lp45sum >= threshold))
                    //{
                    //    for (int i = 0; i < L; i++) op[r + Lp45[0, i], c + Lp45[1, i]] = 1;
                    //}
                    if ((maxIndex == 2) && (VL90sum >= threshold))
                    {
                        for (int i = 0; i < L; i++) op[r + LV90[0, i], c + LV90[1, i]] = 1;
                    }
                    //if ((maxIndex == 3) && (Lm45sum >= threshold))
                    //{
                    //    for (int i = 0; i < L; i++) op[r + Lm45[0, i], c + Lm45[1, i]] = 1;
                    //}
                }
            }
            return op;
        }


    } //class MatrixTools
}
