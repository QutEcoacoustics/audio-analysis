using System;
using System.Collections.Generic;
using System.Text;

namespace TowseyLib
{
    public enum Kernal
    {
        LowPass, HighPass1, HighPass2, VerticalLine, HorizontalLine3, HorizontalLine5, 
                            DiagLine1, DiagLine2,
                            Grid2, Grid3, Grid4, //grid filters
                            Laplace1, Laplace2, Laplace3, Laplace4, ERRONEOUS }

    
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

        static double[,] grid2 =          { { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5}, 
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5}};

        static double[,] grid3 =          { { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5}, 
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
                                            { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5}};

        static double[,] grid4 =          { { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375}, 
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
                                            { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375}};

        //static double[,] grid2 =          { { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5}, 
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5}};

        //static double[,] grid3 =          { { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5}, 
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -0.5, -0.5, 1.0, -0.5, -0.5, 1.0, -0.5}};

        //static double[,] grid4 =          { { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375}, 
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375},
        //                                    { -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375, -0.375, -0.375, 1.0, -0.375}};

        
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



        public static double[,] GridFilter(double[,] matrix, Kernal name)
        {
            double[,] kernal;
            int noiseSampleCount = 10000;
            double thresholdZScore = 3.1;

            //SWITCH KERNALS
            switch (name)
            {
                case Kernal.Grid2: kernal = grid2;
                    Console.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid3: kernal = grid3;
                    Console.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid4: kernal = grid4;
                    Console.WriteLine("Applied Grid Kernal 2");
                    break;

                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }//end of switch statement


            int mRows = matrix.GetLength(0);
            int mCols = matrix.GetLength(1);
            int kRows = kernal.GetLength(0);
            int kCols = kernal.GetLength(1);
            int rNH = kRows / 2;
            int cNH = kCols / 2;
            if ((rNH <= 0) && (cNH <= 0)) return matrix; //no operation required
            //int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            double[] noiseScores = new double[noiseSampleCount];
            for (int n = 0; n < noiseSampleCount; n++)
            {
                double[,] noise = GetNoise(matrix, kRows, kCols);
                double sum = 0.0;
                for (int i = 0; i < kRows; i++)
                {
                    for (int j = 0; j < kCols; j++)
                        sum += noise[i, j] * kernal[i, j];
                }
                noiseScores[n] = sum / (double)kRows;
            }
            double noiseAv; double noiseSd;
            NormalDist.AverageAndSD(noiseScores, out noiseAv, out noiseSd);
            Console.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);

            double[,] newMatrix = new double[mRows, mCols];//init new matrix to return

            // fix up the edges first
            //for (int r = 0; r < mRows; r++)
            //{
            //    for (int c = 0; c < cNH; c++)
            //    {
            //        newMatrix[r, c] = matrix[r, c];
            //    }
            //    for (int c = (mCols - cNH); c < mCols; c++)
            //    {
            //        newMatrix[r, c] = matrix[r, c];
            //    }
            //}
            // fix up other edges
            //for (int c = 0; c < mCols; c++)
            //{
            //    for (int r = 0; r < rNH; r++)
            //    {
            //        newMatrix[r, c] = matrix[r, c];
            //    }
            //    for (int r = (mRows - rNH); r < mRows; r++)
            //    {
            //        newMatrix[r, c] = matrix[r, c];
            //    }
            //}

            //now do bulk of image
            for (int r = rNH; r < (mRows - rNH); r++)
                for (int c = cNH; c < (mCols - cNH); c++)
                {
                    double sum = 0.0;
                    for (int y = -rNH; y < rNH; y++)
                        for (int x = -cNH; x < cNH; x++)
                        {
                            sum += (matrix[r + y, c + x] * kernal[rNH + y, cNH + x]);
                        }
                    sum /= (double)kRows;
                    //Console.WriteLine("sum="+sum);
                    double zScore = (sum - noiseAv) / noiseSd;
//                    double zScore = sum;
//                    newMatrix[r, c] = zScore;
                    //Console.WriteLine("zScore="+zScore);


                    if (zScore > thresholdZScore)
                    {
                        newMatrix[r, c] = 1.0;
                        newMatrix[r + 1, c] = 1.0;
                        newMatrix[r, c + 1] = 1.0;
                        newMatrix[r + 1, c + 1] = 1.0;
                        //newMatrix[r, c] = zScore;
                        //newMatrix[r + 1, c] = zScore;
                        //newMatrix[r, c + 1] = zScore;
                        //newMatrix[r + 1, c + 1] = zScore;
                    }
                }//end of loops
            return newMatrix;
        }//end method GridFilter()


        public static double[,] GetNoise(double[,] m, int kRows, int kCols)
        {
            int mHeight = m.GetLength(0);
            int mWidth  = m.GetLength(1);

            double[,] noise = new double[kRows, kCols];
            RandomNumber rn = new RandomNumber();
            for (int r = 0; r < kRows; r++)
            {
                int randomRow = rn.getInt(mHeight - kRows);
                int randomCol = rn.getInt(mWidth - kCols);
                for (int c = 0; c < kCols; c++)
                    noise[r, c] = m[randomRow, randomCol+c];
            }
            return noise;
        } //end getNoise()



        /// <summary>
        /// Reverses a 256 grey scale image
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[,] Reverse256GreyScale(double[,] m)
        {
            const int scaleMax = 256 - 1;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] newMatrix = DataTools.normalise(m);
            for (int i = 0; i < mRows; i++)
                for (int j = 0; j < mCols; j++)
                {
                    newMatrix[i, j] = scaleMax - newMatrix[i, j];
                }
            return newMatrix;
        }


        /// <summary>
        /// blurs an image using a square neighbourhood
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="nh">Note that neighbourhood is distance either side of central pixel.</param>
        /// <returns></returns>
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
        /// blurs and image using a rectangular neighbourhood.
        /// Note that in this method neighbourhood dimensions are full side or window.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="cNH">column Window i.e. x-dimension</param>
        /// <param name="rNH">row Window i.e. y-dimension</param>
        /// <returns></returns>
        public static double[,] Blur(double[,] matrix, int cWindow, int rWindow)
        {
            if ((cWindow <= 1) && (rWindow <= 1)) return matrix; //no blurring required

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


        public static void PercentileThresholds(double[,] M, double lowerCut, double upperCut, out double lowerThreshold, out double upperThreshold)
        {
            int binCount = 50;
            int count = M.GetLength(0) * M.GetLength(1);
            double binWidth;
            double min; double max;
            int[] powerHisto = DataTools.Histo(M, binCount, out binWidth, out min, out max);
            powerHisto[binCount - 1] = 0;   //just in case it is the max
            double[] smooth = DataTools.filterMovingAverage(powerHisto, 5);
            int maxindex;
            DataTools.getMaxIndex(smooth, out maxindex);

            //calculate threshold for upper percentile
            int clipCount = (int)(upperCut * count);
            int i = binCount-1;
            int sum = 0;
            while ((sum < clipCount) && (i > 0)) sum += powerHisto[i--];
            upperThreshold = min + (i * binWidth);

            //calculate threshold for lower percentile
            clipCount = (int)(lowerCut * count);
            int j = 0;
            sum = 0;
            while ((sum < clipCount) && (j < binCount)) sum += powerHisto[j++];
            lowerThreshold = min + (j * binWidth);

            //DataTools.writeBarGraph(powerHisto);
            //Console.WriteLine("LowerThreshold=" + lowerThreshold + "  UpperThreshold=" + upperThreshold);
        }

        public static void NoiseThresholds(double[,] M, double lowerCut, double upperCut, out double min, out double lowerThreshold, out double upperThreshold)
        {
            int binCount = 50;
            int count = M.GetLength(0) * M.GetLength(1);
            double binWidth;
            double max;
            int[] powerHisto = DataTools.Histo(M, binCount, out binWidth, out min, out max);
            powerHisto[binCount - 1] = 0;   //just in case it is the max
            double[] smooth = DataTools.filterMovingAverage(powerHisto, 5);
            int maxindex;
            DataTools.getMaxIndex(smooth, out maxindex);

            //calculate threshold for upper shoulder
            int i = maxindex;
            double thresholdCount = smooth[maxindex] * upperCut; //
            while ((smooth[i] > thresholdCount) && (i < binCount)) i++;
            upperThreshold = min + (i * binWidth);


            //calculate threshold for lower shoulder
            int j = maxindex;
            thresholdCount = smooth[maxindex] * lowerCut; //
            while ((smooth[j] > thresholdCount) && (j > 0)) j--;
            lowerThreshold = min + (j * binWidth);

            //DataTools.writeBarGraph(powerHisto);
            //Console.WriteLine("LowerThreshold=" + lowerThreshold + "  UpperThreshold=" + upperThreshold);
        }


        /// <summary>
        /// Calculates the local signal to noise ratio in the neighbourhood of side=window
        /// SNR is diefined as local mean / local std dev.
        /// Must check that the local std dev is not too small.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[,] Signal2NoiseRatio_Local(double[,] matrix, int window)
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
                    for (int x = (i - nh + 1); x < (i + nh); x++)
                        for (int y = (j - nh + 1); y < (j + nh); y++)
                        {
                            values[id++] = matrix[x, y];
                        }
                    double av; double sd;
                    NormalDist.AverageAndSD(values, out av, out sd);
                    if (sd < 0.0001) sd = 0.0001;
                    newMatrix[i, j] = (matrix[i, j] - av) / sd;
                }
            return newMatrix;
        }


        public static double[,] Signal2NoiseRatio_BandWise(double[,] matrix)
        {
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);

            double[,] M = new double[height, width];
            double[,] subMatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);

            for (int col = 0; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                    subMatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);

                double av; double sd;
                NormalDist.AverageAndSD(subMatrix, out av, out sd);
                if (sd < 0.0001) sd = 0.0001;  //to prevent division by zero

                for (int y = 0; y < height; y++)
                {
                    M[y, col] = (matrix[y, col] - av) / sd;
                }
            }//for all cols
            return M;
        }// end of SubtractAverage()



        public static double[,] SubtractAverage_BandWise(double[,] matrix)
        {
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;
            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);

            double[,] M = new double[height, width];
            double[,] subMatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);

            for (int col = 0; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                    subMatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                double av; double sd;
                NormalDist.AverageAndSD(subMatrix, out av, out sd);
                //Console.WriteLine(0 + "," + start + "," + (height - 1) + "," + stop + "   Threshold " + b + "=" + threshold);

                for (int y = 0; y < height; y++)
                {
                    M[y, col] = matrix[y, col] - av;
                }//for all rows
            }//for all cols
            return M;
        }// end of SubtractAverage()



        public static double[,] SubtractNoise(double[,] matrix)
        {
            //set up parameters for a set of overlapping bands. All numbers should be powers of 2
            int ncbbc = 16;  //number of columns between band centres
            int bandWidth = 32;
            double lowerShoulder = 1.0;
            double upperShoulder = 0.001;

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            int halfWidth = bandWidth / 2;
            int bandCount = width / ncbbc;
            int bandID = 0;
            int tmpCol = 0;

            double[,] tmpM = new double[height, ncbbc]; 
            double[,] outM = new double[height, width];
            double[,] thresholdSubatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);
            double min; double lowerThreshold; double upperThreshold;
            NoiseThresholds(thresholdSubatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);

            for (int col = 0; col < width; col++)//for all cols
            {
                bandID = col / ncbbc;  // determine band ID
                tmpCol = col % ncbbc;  // determine col relative to current band
                if ((tmpCol==0) && (!(col == 0)))
                {
                    //normalise existing submatrix and transfer to the output matrix, outM
                    tmpM = DataTools.normalise(tmpM);
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < ncbbc; x++)
                        {
                            int startCol = col - ncbbc;
                            outM[y, startCol+x] = tmpM[y, x];
                        }

                    //set up a new submatrix for processing
                    tmpM = new double[height, ncbbc]; 

                    //construct new threshold submatrix to recalculate the current threshold
                    int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                    if (start < 0) start = 0;
                    int stop = col + halfWidth;
                    if (stop >= width) stop = width - 1;
                    thresholdSubatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                    NoiseThresholds(thresholdSubatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    tmpM[y, tmpCol] = matrix[y, col];
                    if (tmpM[y, tmpCol] > upperThreshold) tmpM[y, tmpCol] = upperThreshold;
                    if (tmpM[y, tmpCol] < lowerThreshold) tmpM[y, tmpCol] = lowerThreshold;
                    //outM[y, col] = matrix[y, col] - upperThreshold;
                    //if (outM[y, col] < upperThreshold) outM[y, col] = upperThreshold;

                    //if (matrix[y, col] < upperThreshold) M[y, col] = 0.0;
                    //else M[y, col] = 1.0;
                }
            }//for all cols
            return outM;
        }// end of SubtractAverage()

        public static double[,] TrimPercentiles(double[,] matrix)
        {
            //set up parameters for a set of overlapping bands. All numbers should be powers of 2
            int ncbbc = 8;  //number of columns between band centres
            int bandWidth = 64;
            double lowerPercentile = 0.7;
            double upperPercentile = 0.001;

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            int halfWidth = bandWidth / 2;
            int bandCount = width / ncbbc;
            int bandID = 0;
            int tmpCol = 0;

            double[,] tmpM = new double[height, ncbbc];
            double[,] outM = new double[height, width];
            double[,] thresholdSubatrix = DataTools.Submatrix(matrix, 0, 0, height - 1, bandWidth);
            double lowerThreshold; double upperThreshold;
            PercentileThresholds(thresholdSubatrix, lowerPercentile, upperPercentile, out lowerThreshold, out upperThreshold);

            for (int col = 0; col < width; col++)//for all cols
            {
                bandID = col / ncbbc;  // determine band ID
                tmpCol = col % ncbbc;  // determine col relative to current band
                if ((tmpCol == 0) && (!(col == 0)))
                {
                    //normalise existing submatrix and transfer to the output matrix, outM
                    tmpM = DataTools.normalise(tmpM);
                    for (int y = 0; y < height; y++)
                        for (int x = 0; x < ncbbc; x++)
                        {
                            int startCol = col - ncbbc;
                            outM[y, startCol + x] = tmpM[y, x];
                        }

                    //set up a new submatrix for processing
                    tmpM = new double[height, ncbbc];

                    //construct new threshold submatrix to recalculate the current threshold
                    int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                    if (start < 0) start = 0;
                    int stop = col + halfWidth;
                    if (stop >= width) stop = width - 1;
                    thresholdSubatrix = DataTools.Submatrix(matrix, 0, start, height - 1, stop);
                    PercentileThresholds(thresholdSubatrix, lowerPercentile, upperPercentile, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    tmpM[y, tmpCol] = matrix[y, col];
                    if (tmpM[y, tmpCol] > upperThreshold) tmpM[y, tmpCol] = upperThreshold;
                    if (tmpM[y, tmpCol] < lowerThreshold) tmpM[y, tmpCol] = lowerThreshold;
                    //outM[y, col] = matrix[y, col] - upperThreshold;
                    //if (outM[y, col] < upperThreshold) outM[y, col] = upperThreshold;

                    //if (matrix[y, col] < upperThreshold) M[y, col] = 0.0;
                    //else M[y, col] = 1.0;
                }
            }//for all cols
            return outM;
        }// end of SubtractAverage()



        public static double[,] Shapes1(double[,] matrix)
        {
            double gradThreshold = 1.2;
            int fWindow = 9;
            int tWindow = 9;
            int bandCount = 16;  // 16 bands, width=512pixels, 32pixels/band 
            double lowerShoulder = 0.5;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.05;
            
            double[,] blurM = ImageTools.Blur(matrix, fWindow, tWindow);

            int height = blurM.GetLength(0);
            int width = blurM.GetLength(1);
            double bandWidth = width / (double)bandCount;

            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++) M[0, x] = 0.0; //patch in first  time step with zero gradient
            for (int x = 0; x < width; x++) M[1, x] = 0.0; //patch in second time step with zero gradient

            for (int b = 0; b < bandCount; b++)//for all bands
            {
                int start = (int)((b - 1) * bandWidth);   //extend range of submatrix below b for smoother changes
                if (start < 0) start = 0;
                int stop = (int)((b + 2) * bandWidth);
                if (stop >= width) stop = width - 1;

                double[,] subMatrix = DataTools.Submatrix(blurM, 0, start, height - 1, stop);
                double min; double lowerThreshold; double upperThreshold;
                NoiseThresholds(subMatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);
                //Console.WriteLine(0 + "," + start + "," + (height - 1) + "," + stop + "   Threshold " + b + "=" + threshold);


                for (int x = start; x < stop; x++)
                {
                    int state = 0;
                    for (int y = 2; y < height - 1; y++)
                    {

                        double grad1 = blurM[y, x] - blurM[y - 1, x];//calculate one step gradient
                        double grad2 = blurM[y + 1, x] - blurM[y - 1, x];//calculate two step gradient

                        if (blurM[y, x] < upperThreshold) state = 0;
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

            //M = Shapes_RemoveSmall(M);
            M = Shapes_CleanUp(M);
            return M;

        }// end of Shapes()


        public static double[,] Shapes2(double[,] matrix)
        {
            double lowerShoulder = 0.3;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.3;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 7;
            int tWindow = 5;
            //double[,] blurM = matrix;
            double[,] blurM = ImageTools.Blur(matrix, fWindow, tWindow);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(blurM, 0, 0, height - 1, bandWidth);
            double min; double lowerThreshold; double upperThreshold;
            NoiseThresholds(subMatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);

            for (int col = 0; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                {
                    subMatrix = DataTools.Submatrix(blurM, 0, start, height - 1, stop);
                    NoiseThresholds(subMatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    if (blurM[y, col] < upperThreshold) M[y, col] = 0.0;
                    else M[y, col] = 1.0;
                }
            }//for all cols
            return M;
        }// end of Shapes2()



        public static double[,] Shapes_lines(double[,] matrix)
        {
            double lowerShoulder = 0.2;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.2;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 7;
            int tWindow = 3;
            double[,] blurM = ImageTools.Blur(matrix, fWindow, tWindow);
            double[,] lines = ImageTools.Convolve(blurM, Kernal.HorizontalLine5);
            lines = ImageTools.Convolve(matrix, Kernal.HorizontalLine5);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(lines, 0, 0, height - 1, bandWidth);
            double min; double lowerThreshold; double upperThreshold;
            NoiseThresholds(subMatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);

            for (int col = 0; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                {
                    subMatrix = DataTools.Submatrix(lines, 0, start, height - 1, stop);
                    NoiseThresholds(subMatrix, lowerShoulder, upperShoulder, out min, out lowerThreshold, out upperThreshold);
                }

                for (int y = 1; y < height-1; y++)
                {
                    if (lines[y, col] > upperThreshold)
                    {
                        //M[y, col]     = lines[y, col];
                        //M[y - 1, col] = lines[y - 1, col];
                        //M[y + 1, col] = lines[y + 1, col];
                        M[y, col] = 1;
                        //M[y - 2, col] = 1;
                        M[y - 1, col] = 1;
                        //M[y + 1, col] = 1;
                    }
                }
            }//for all cols
            int minRowWidth = 2;
            int minColWidth = 10;
            M = Shapes_RemoveSmall(M, minRowWidth, minColWidth);
            return M;
        }// end of Shapes_lines()

        

        public static double[,] Shapes_CleanUp(double[,] m)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            //remove double lines in height dimension
            for (int x = 0; x < width; x++)
            {
                for (int y = 2; y < height - 1; y++)
                {
                    if ((M[y - 2, x] == 0.0) && (M[y + 1, x] == 0.0)) { M[y - 1, x] = 0.0; M[y, x] = 0.0; }
                    else if ((M[y - 2, x] == 1.0) && (M[y + 1, x] == 1.0) ) { M[y - 1, x] = 1.0; M[y, x] = 1.0; }
                }
            }
            //remove single lines in height dimension
            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    M[y, x] = m[y, x];
                    if ((m[y - 1, x] == 0.0) && (m[y + 1, x] == 0.0)) M[y, x] = 0.0;
                    //else if ((m[y - 1, x] == 1.0) && (m[y + 1, x] == 1.0)) M[y, x] = 1.0;
                }
            }

            //remove single lines in width dimension
            for (int y = 0; y < height; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if ((M[y, x - 1] == 0.0) && (M[y, x + 1] == 0.0)) M[y, x] = 0.0;
                    //else if ((M[y, x - 1] == 1.0) && (M[y, x + 1] == 1.0)) M[y, x] = 1.0;
                }
            }
            //remove double lines in width dimension
            for (int y = 0; y < height; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    if ((M[y, x - 2] == 0.0) && (M[y, x + 1] == 0.0))
                    { M[y, x - 1] = 0.0; M[y, x] = 0.0; }
                    else if ((M[y, x - 2] == 1.0) && (M[y, x + 1] == 1.0))
                    { M[y, x - 1] = 1.0; M[y, x] = 1.0; }
                }
            }
            //remove triple lines in width dimension
            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 3; x < width - 3; x++)
            //    {
            //        if ((M[y, x - 3] == 0.0) && (M[y, x - 3] == 0.0) && (M[y, x + 1] == 0.0) && (M[y, x + 2] == 0.0))
            //        { M[y, x - 2] = 0.0; M[y, x - 1] = 0.0; M[y, x] = 0.0; }
            //        else if ((M[y, x - 4] == 1.0) && (M[y, x - 3] == 1.0) && (M[y, x + 1] == 1.0) && (M[y, x + 2] == 1.0))
            //        { M[y, x - 2] = 1.0; M[y, x - 1] = 1.0; M[y, x] = 1.0; }
            //    }
            //}

            return M;
        }


        public static double[,] Shapes_RemoveSmall(double[,] m, int minRowWidth, int minColWidth)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (m[y, x] == 0.0) continue; //nothing here
                    if (M[y, x] == 1.0) continue; //already have something here

                    int rowWidth; //rowWidth of object
                    Shape.Row_Width(m, x, y, out rowWidth);
                    int colWidth; //colWidth of object
                    Shape.Col_Width(m, x, y, out colWidth);

                    if ((rowWidth >= minRowWidth) && (colWidth >= minColWidth))
                    {
                        for (int c = 0; c < colWidth; c++)
                        {
                            //Shape.Row_Width(m, x+c, y, out rowWidth);
                            for (int r = 0; r < minRowWidth; r++)
                            {
                                M[y + r, x + c] = 1.0;
                                //if ((y + r + 1) < height) M[y + r + 1, x + c] = m[y + r + 1, x + c];
                                //if ((y + r + 2) < height) M[y + r + 2, x + c] = m[y + r + 2, x + c];
                                //m[y + r, x + c] = 0.0;
                            }
                        }
                    }
                    y += (rowWidth-1);
                }//end y loop
            }//end x loop
            //M = m;

            return M;
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
                    NormalDist.AverageAndSD(values, out av, out sd);
                    double v = sd * sd;
                    if(v< 0.0001) v = 0.0001;
                    newMatrix[i, j] = v;
                }

            return newMatrix;
        }


    }
}
