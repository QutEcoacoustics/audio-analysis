using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Numerics;

using MathNet.Numerics;
//using MathNet.Numerics.ComplexExtensions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;


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
                //int RowTop = 2;
                //int ColumnLeft = 3;
                //int RowBottom = 4;
                //int ColumnRight = 6;
                //LoggedConsole.WriteLine("RowTop="+RowTop+" ColumnLeft="+ColumnLeft+" RowBottom="+RowBottom+" ColumnRight="+ColumnRight);
                //LoggedConsole.WriteLine("Indices start at [0,0] in top left.");
                //int smRows = RowBottom - RowTop + 1;
                //int smCols = ColumnRight - ColumnLeft + 1;
                //LoggedConsole.WriteLine("Submatrix has " + smRows + " rows and " + smCols + " columns");
                //double[,] sub = Submatrix(matrix, RowTop, ColumnLeft, RowBottom, ColumnRight);
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



        public static double[,] ConcatenateMatrixRows(double[,] M1, double[,] M2)
        {
            Matrix<double> MN1 = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(M1);
            Matrix<double> MN2 = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.OfArray(M2);
            MN1.Append(MN2);
            return MN1.ToArray();
        }


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
        /// Adds a frame around a matrix by adding row and columns of zeros.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="frameWidth">The number of rows/columns of zeros to be added</param>
        /// <returns></returns>
        public static double[,] FrameMatrixWithZeros(double[,] M, int frameWidth)
        {            
            int inRowCount = M.GetLength(0);
            int inColCount = M.GetLength(1);

            int outRowCount = inRowCount + (2 * frameWidth);
            int outColCount = inColCount + (2 * frameWidth);

            var outputMatrix = new double[outRowCount, outColCount];

            for (int r = 0; r < inRowCount; r++)
            {
                for (int c = 0; c < inColCount; c++)
                {
                    outputMatrix[r + frameWidth, c + frameWidth] = M[r, c];
                }
            }

            return outputMatrix;
        }




        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// The returned submatrix includes the rows and column passed as bounds.
        /// Assume that RowTop < RowBottom, ColumnLeft < ColumnRight. 
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
            int subRowCount = r2 - r1 + 1;
            int subColCount = c2 - c1 + 1;

            double[,] sm = new double[subRowCount, subColCount];

            for (int i = 0; i < subRowCount; i++)
            {
                for (int j = 0; j < subColCount; j++)
                {                       
                    sm[i, j] = M[r1 + i, c1 + j];
                }
            }

            return sm;
        }

        /// <summary>
        /// Returns an array of row averages in the submatrix of passed matrix.
        /// This method combines two methods, Submatrix() & GetRowAverages(), for efficiency
        /// Assume that RowTop < RowBottom, ColumnLeft < ColumnRight. 
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

        public static double[,] ConvertMatrixOfFloat2Double(float[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = (double)matrix[r, c];
                }
            }
            return outM;
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


        /*
         * converts a matrix of doubles to binary using passed threshold
         */
        public static double[,] ThresholdMatrix2RealBinary(double[,] M, double threshold)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            var op = new double[rowCount, colCount];

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > threshold) op[r, c] = 1.0;
                }
            }
            return op;
        }


        public static double[,] SubtractAndTruncate2Zero(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    outM[r, c] = matrix[r, c] - threshold;
                    if (outM[r, c] < 0.0) outM[r, c] = 0.0;
                }
            }
            return outM;
        }

        /// <summary>
        /// truncate values below threshold to zero.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static double[,] Truncate2Zero(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] outM = new double[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (matrix[r, c] > threshold) 
                        outM[r, c] = matrix[r, c];
                }
            }
            return outM;
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
            Console.WriteLine("Zeroed {0} of {1} non-zero cells.", count, total);
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
            Console.WriteLine("Removed {0} doublets.", total);
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
  public static void SetRow(double[,] m, int rowID, double[] array)
  {
      int cols = m.GetLength(1);
      for (int c = 0; c < cols; c++) m[rowID, c] = array[c];
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
		/// Normalises matrix values so that the min and max values become [0,1], respectively.
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
        /// Turns a matrix upside-down and back-to-front!
        /// This is  a rotation NOT a transpose.
        /// </summary>
        /// <param name="M">the matrix to rotate</param>
        /// <returns></returns>

        public static double[,] MatrixRotate180(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new double[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    ret[r, c] = m[rows - r - 1, cols - c - 1];
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
        public static byte[,] MatrixRotate90Clockwise(byte[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var ret = new byte[cols, rows];
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
        /// Rescales the values of a matrix so that its in and max values are those passed.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="normMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] RescaleMatrixBetweenMinAndMax(double[,] m, double normMin, double normMax)
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
        /// Normalises a matrix so that all values below the passed MIN are truncated to 0 and all values greater than the
        /// passed MAX are truncated to 1.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="truncateMin"></param>
        /// <param name="normMax"></param>
        /// <returns></returns>
        public static double[,] NormaliseInZeroOne(double[,] M, double truncateMin, double truncateMax)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);
            double range = truncateMax - truncateMin;
            double[,] M2return = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    M2return[r, c] = (M[r, c] - truncateMin) / range;
                    if (M2return[r, c] > 1.0)
                    {
                        M2return[r, c] = 1.0;
                    }
                    else if (M2return[r, c] < 0.0)
                    {
                        M2return[r, c] = 0.0;
                    }
                }
            }

            return M2return;
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


        public static double[,] SmoothColumns(double[,] matrix, int window)
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

        public static double[,] SmoothRows(double[,] matrix, int window)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var smoothMatrix = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                var row = DataTools.GetRow(matrix, r);
                row = DataTools.filterMovingAverage(row, window);
                MatrixTools.SetRow(smoothMatrix, r, row);
            }
            return smoothMatrix;
        }
        //=============================================================================


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



        /*
         * writes the r, c location of the maximum valuesin the matrix
         */
        public static void WriteLocationOfMaximumValues(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double max = -Double.MaxValue;

            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > max)
                    {
                        max = M[r, c];
                        Console.WriteLine("Max value to now at r={0}, c={1},  value={2}", r, c, M[r, c]);
                    }
                }
            }
        }


        /*
         * 
         */
        public static double[] GetMaximumRowValues(double[,] M)
        {
            int rowCount = M.GetLength(0);
            int colCount = M.GetLength(1);
            double max;
            double[] returnV = new double[rowCount];

            for (int r = 0; r < rowCount; r++)
            {
                max = -Double.MaxValue;
                for (int c = 0; c < colCount; c++)
                {
                    if (M[r, c] > max)
                    {
                        max = M[r, c];
                    }
                }
                returnV[r] = max;
            }
            return returnV;
        }



    } //class MatrixTools
}
