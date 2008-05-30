using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    public enum Kernal
    {
        LowPass, HighPass1, HighPass2, VerticalLine, HorizontalLine3, HorizontalLine5, 
                         DiagLine1, DiagLine2, Laplace1, Laplace2, Laplace3, Laplace4, ERRONEOUS }

    
    public class ImageTools
    {
        static double[,] lowPassKernal = { { 0.1, 0.1, 0.1 }, { 0.1, 0.2, 0.1 }, { 0.1, 0.1, 0.1 } };
        static double[,] highPassKernal1 = { { -1.0, -1.0, -1.0 }, { -1.0, 9.0, -1.0 }, { -1.0, -1.0, -1.0 } };
        static double[,] highPassKernal2 = { { -0.3, -0.3, -0.3, -0.3, -0.3},
                                             { -0.3, -0.3, -0.3, -0.3, -0.3}, 
                                             { -0.3, -0.3,  9.7, -0.3, -0.3},
                                             { -0.3, -0.3, -0.3, -0.3, -0.3},
                                             { -0.3, -0.3, -0.3, -0.3, -0.3}};

        static double[,] vertLineKernal = {{-0.5, 1.0, -0.5},{-0.5,1.0,-0.5},{-0.5,1.0,-0.5}};
        static double[,] horiLineKernal3 = { { -0.5, -0.5, -0.5 }, { 1.0, 1.0, 1.0 }, { -0.5, -0.5, -0.5 } };
        static double[,] horiLineKernal5 = { { -0.5, -0.5, -0.5, -0.5, -0.5 }, { 1.0, 1.0, 1.0, 1.0, 1.0 }, { -0.5, -0.5, -0.5, -0.5, -0.5 } };
        static double[,] diagLineKernal1 = { { 2.0, -1.0, -1.0 }, { -1.0, 2.0, -1.0 }, { -1.0, -1.0, 2.0 } };
        static double[,] diagLineKernal2 = { { -1.0, -1.0, 2.0 }, { -1.0, 2.0, -1.0 }, { 2.0, -1.0, -1.0 } };

        static double[,] Laplace1Kernal = { { 0.0, -1.0, 0.0 }, { -1.0, 4.0, -1.0 }, { 0.0, -1.0, 0.0 } };
        static double[,] Laplace2Kernal = { { -1.0, -1.0, -1.0 }, { -1.0, 8.0, -1.0 }, { -1.0, -1.0, -1.0 } };
        static double[,] Laplace3Kernal = { { 1.0, -2.0, 1.0 }, { -2.0, 4.0, -2.0 }, { 1.0, -2.0, 1.0 } };
        static double[,] Laplace4Kernal = { { -1.0, -1.0, -1.0 }, { -1.0, 9.0, -1.0 }, { -1.0, -1.0, -1.0 } }; //subtracts original




        public static double[,] Subtract(double[,] m1, double[,] m2)
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

        public static double[,] Invert(double[,] m)
        {
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] newMatrix = DataTools.normalise(m);
            for (int i = 0; i < mRows; i++)
                for (int j = 0; j < mCols; j++)
                {
                    newMatrix[i, j] = 255 - newMatrix[i, j];
                }
            return newMatrix;
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
            //Console.WriteLine("cNH=" + cNH + ", rNH" + rNH);
            int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood
            double[,] newMatrix = new double[rows, cols];//init new matrix to return

            // fix up the edges first
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cNH; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
                for (int c = (cols - cNH); c < cols; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }
            // fix up other edges
            for (int c = 0; c < cols; c++)
            {
                for (int r = 0; r < rNH; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
                for (int r = (rows - rNH); r < rows; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }

            for (int r = rNH; r < (rows - rNH); r++)
                for (int c = cNH; c < (cols - cNH); c++)
                {
                    double sum = 0.0;
                    for (int y = (r - rNH); y <= (r + rNH); y++)
                    {
                        //System.Console.WriteLine(r+", "+c+ "  y="+y);
                        for (int x = (c - cNH); x <= (c + cNH); x++)
                        {
                            sum += matrix[y, x];
                        }
                    }
                    newMatrix[r, c] = sum / (double)area;
                }

            return newMatrix;
        }//end method Blur()


        public static double[,] Texture1(double[,] matrix, int window)
        {

            int nh = window / 2;
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            int cellCount = ((2 * nh) + 1) * ((2 * nh) + 1);
            double[,] newMatrix = new double[M, N];

            for (int i = nh; i < (M - nh); i++)
                for (int j = nh; j < (N - nh); j++)
                {
                    double sum = 0.0;
                    for (int x = (i - nh + 1); x < (i + nh); x++)
                        for (int y = (j - nh + 1); y < (j + nh); y++)
                        {
                            //values[id++] = matrix[x, y];
                            sum += Math.Abs(matrix[x, y] - matrix[x-1, y-1]);
                        }
                    newMatrix[i, j] = sum;
                }

            return newMatrix;
        }
        public static double[,] Texture2(double[,] matrix, int window)
        {

            int nh = window / 2;
            int M = matrix.GetLength(0);
            int N = matrix.GetLength(1);

            int cellCount = ((2 * nh) + 1) * ((2 * nh) + 1);
            double[,] newMatrix = new double[M, N];

            for (int i = nh; i < (M - nh); i++)
                for (int j = nh; j < (N - nh); j++)
                {
                    int id = 0;
                    double[] values = new double[cellCount];
                    for (int x = i - nh; x < (i + nh); x++)
                        for (int y = j - nh; y < (j + nh); y++)
                        {
                            values[id++] = matrix[x, y];
                    
                        }
                    double av;
                    double sd;
                    NormalDist.getAverageAndSD(values, out av, out sd);
                    newMatrix[i, j] = sd*sd;
                }

            return newMatrix;
        }


        
        public static double[,] Convolve(double[,] matrix, Kernal name)
        {
            double[,] kernal;

            //SWITCH KERNALS
            switch (name)
            {
                case Kernal.LowPass: kernal = lowPassKernal;
                    break;
                case Kernal.HighPass1: kernal = highPassKernal1;
                    break;
                case Kernal.HighPass2: kernal = highPassKernal2;
                    Console.WriteLine("Applied highPassKernal2 Kernal");
                    break;
                case Kernal.HorizontalLine3: kernal = horiLineKernal3;
                    break;
                case Kernal.HorizontalLine5: kernal = horiLineKernal5;
                    Console.WriteLine("Applied Horizontal Line5 Kernal");
                    break;
                case Kernal.VerticalLine: kernal = vertLineKernal;
                    break;
                case Kernal.DiagLine1: kernal = diagLineKernal1;
                    Console.WriteLine("Applied diagLine1 Kernal");
                    break;
                case Kernal.DiagLine2: kernal = diagLineKernal2;
                    Console.WriteLine("Applied diagLine2 Kernal");
                    break;
                case Kernal.Laplace1: kernal = Laplace1Kernal;
                    Console.WriteLine("Applied Laplace1 Kernal");
                    break;
                case Kernal.Laplace2: kernal = Laplace2Kernal;
                    Console.WriteLine("Applied Laplace2 Kernal");
                    break;
                case Kernal.Laplace3: kernal = Laplace3Kernal;
                    Console.WriteLine("Applied Laplace3 Kernal");
                    break;
                case Kernal.Laplace4: kernal = Laplace4Kernal;
                    Console.WriteLine("Applied Laplace4 Kernal");
                    break;
                    

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }//end of switch statement


            int mRows = matrix.GetLength(0);
            int mCols = matrix.GetLength(1);
            int kRows = kernal.GetLength(0);
            int kCols = kernal.GetLength(1);
            int rNH   = kRows / 2;
            int cNH   = kCols / 2;

            if ((rNH <= 0) && (cNH <= 0)) return matrix; //no operation required

            //int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            //double[,] newMatrix = (double[,])matrix.Clone();
            double[,] newMatrix = new double[mRows, mCols];//init new matrix to return

            // fix up the edges first
            for (int r = 0; r < mRows; r++)
            {
                for (int c = 0; c < cNH; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
                for (int c = (mCols - cNH); c < mCols; c++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }
            // fix up other edges
            for (int c = 0; c < mCols; c++)
            {
                for (int r = 0; r < rNH; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
                for (int r = (mRows - rNH); r < mRows; r++)
                {
                    newMatrix[r, c] = matrix[r, c];
                }
            }
            
            //now do bulk of image
            for (int r = rNH; r < (mRows - rNH); r++)
                for (int c = cNH; c < (mCols - cNH); c++)
                {
                    double sum = 0.0;
                    for (int y = -rNH; y <rNH; y++)
                    {
                        for (int x = -cNH; x < cNH; x++)
                        {
                            sum += (matrix[r + y, c + x] * kernal[rNH - y, cNH - x]);
                        }
                    }
                    newMatrix[r, c] = sum;// / (double)area;
                }
            return newMatrix;
        }//end method Convolve()




        public static double[,] PointProcess(double[,] m, int bandCount, int type)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double bandWidth = width / (double)bandCount;

            double[,] M = new double[height, width];
            int prevStop = -1;
            for (int b = 0; b < bandCount; b++)//for all bands
            {
                int start = prevStop+1;
                int stop = (int)((b + 1) * bandWidth);
                if (stop >= width) stop = width - 1;
                prevStop = stop;
                //extract the submatrix
                double[,] subMatrix = DataTools.Submatrix(m, 0, start, height - 1, stop);

                //now do operation
                //double threshold = DataTools.ImageThreshold(subMatrix);
                //double threshold = 20.0;
                //Console.WriteLine("Threshold " + b + " = " + threshold);
                //subMatrix = DataTools.Clip(subMatrix, 0.0, 1.0);

                //return subMatrix to output matrix;
                for (int x = start; x < stop; x++)
                    for (int y = 0; y < height; y++)
                    {
                        //M[y, x] = subMatrix[y, x-start];
                        M[y, x] = m[y, x];
                    }//for all x in a band
            }//for all bands

            return M;

        }// end of PointProcess()



        public static double[,] Shapes(double[,] matrix)
        {
            double gradThreshold = 1.2;
            int fWindow = 11;
            int tWindow =9;
            int bandCount = 20;
            double[,] blurM = ImageTools.Blur(matrix, fWindow, tWindow);
            double[] threshold = DataTools.ImageThreshold(blurM, bandCount);

            int height = blurM.GetLength(0);
            int width = blurM.GetLength(1);
            double bandWidth = width / (double)bandCount;

            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++) M[0, x] = 0.0; //patch in first  time step with zero gradient
            for (int x = 0; x < width; x++) M[1, x] = 0.0; //patch in second time step with zero gradient

            for (int b = 0; b < bandCount; b++)//for all bands
            {
                //Console.WriteLine("Threshold " + b + " = " + threshold[b]);
                int start = (int)(b * bandWidth);
                int stop = (int)((b + 1) * bandWidth);
                for (int x = start; x < stop; x++)
                {
                    int state = 0;
                    for (int y = 2; y < height - 1; y++)
                    {

                        double grad1 = blurM[y, x] - blurM[y - 1, x];//calculate one step gradient
                        double grad2 = blurM[y + 1, x] - blurM[y - 1, x];//calculate two step gradient

                        if (blurM[y, x] < threshold[b]) state = 0;
                        else
                            if (grad1 < -gradThreshold) state = 0;    // local decrease
                            else
                                if (grad1 > gradThreshold) state = 1;     // local increase
                                else
                                    if (grad2 < -gradThreshold) state = 0;    // local decrease
                                    else
                                        if (grad2 > gradThreshold) state = 1;     // local increase

                        M[y, x] = (double)state;
                    }
                }//for all x in a band
            }//for all bands

            return Shapes_CleanUp(M);

        }// end of Shapes()


        public static double[,] Shapes_CleanUp(double[,] m)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            //remove single lines in height dimension
            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    M[y, x] = m[y, x];
                    if ((m[y - 1, x] == 0.0) && (m[y + 1, x] == 0.0)) M[y, x] = 0.0;
                    else if ((m[y - 1, x] == 1.0) && (m[y + 1, x] == 1.0)) M[y, x] = 1.0;
                }
            }
            //remove double lines in height dimension
            //for (int x = 0; x < width; x++)
            //{
            //    for (int y = 3; y < height - 2; y++)
            //    {
            //        if ((M[y - 3, x] == 0.0) && (M[y - 2, x] == 0.0) && (M[y + 1, x] == 0.0) && (m[y + 2, x] == 0.0))
            //        { M[y-1, x] = 0.0; M[y, x] = 0.0; }
            //        else if ((M[y - 3, x] == 1.0) && (M[y - 2, x] == 1.0) && (M[y + 1, x] == 1.0) && (m[y + 2, x] == 1.0))
            //        { M[y - 1, x] = 1.0; M[y, x] = 1.0; }
            //    }
            //}

            //remove single lines in width dimension
            for (int y = 0; y < height; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if ((M[y, x - 1] == 0.0) && (M[y, x + 1] == 0.0)) M[y, x] = 0.0;
                    else if ((M[y, x - 1] == 1.0) && (M[y, x + 1] == 1.0)) M[y, x] = 1.0;
                }
            }
            //remove double lines in width dimension
            for (int y = 0; y < height; y++)
            {
                for (int x = 3; x < width - 2; x++)
                {
                    if ((M[y, x - 3] == 0.0) && (M[y, x - 2] == 0.0) && (M[y, x + 1] == 0.0) && (M[y, x + 2] == 0.0))
                    { M[y, x - 1] = 0.0; M[y, x] = 0.0; }
                    else if ((M[y, x - 3] == 1.0) && (M[y, x - 2] == 1.0) && (M[y, x + 1] == 1.0) && (M[y, x + 2] == 1.0))
                    { M[y, x - 1] = 1.0; M[y, x] = 1.0; }
                }
            }
            //remove triple lines in width dimension
            for (int y = 0; y < height; y++)
            {
                for (int x = 4; x < width - 3; x++)
                {
                    if ((M[y, x - 4] == 0.0) && (M[y, x - 3] == 0.0) && (M[y, x + 1] == 0.0) && (M[y, x + 2] == 0.0))
                    { M[y, x - 2] = 0.0; M[y, x - 1] = 0.0; M[y, x] = 0.0; }
                    else if ((M[y, x - 4] == 1.0) && (M[y, x - 3] == 1.0) && (M[y, x + 1] == 1.0) && (M[y, x + 2] == 1.0))
                    { M[y, x - 2] = 1.0; M[y, x - 1] = 1.0; M[y, x] = 1.0; }
                }
            }

            return M;
        }



    }
}
