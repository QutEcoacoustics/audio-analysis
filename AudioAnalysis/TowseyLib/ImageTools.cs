using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;


namespace TowseyLib
{
    public enum Kernal
    {
        LowPass, HighPass1, HighPass2, VerticalLine, HorizontalLine3, HorizontalLine5, 
                            DiagLine1, DiagLine2,
                            Grid2, Grid3, Grid4, Grid2Wave, Grid3Wave, //grid filters
                            Laplace1, Laplace2, Laplace3, Laplace4, ERRONEOUS }

    
    public class ImageTools
    {

        public static bool Verbose { set; get; }

        // this is a list of predefined colors in the Color class.
        public static string[] colorNames={"AliceBlue","AntiqueWhite","Aqua","Aquamarine","Azure","Beige","Bisque","Black","BlanchedAlmond","Blue","BlueViolet",
                            "Brown","BurlyWood","CadetBlue","Chartreuse","Chocolate","Coral","CornflowerBlue","Cornsilk","Crimson","Cyan",
                            "DarkBlue", "DarkCyan","DarkGoldenrod","DarkGray","DarkGreen","DarkKhaki","DarkMagenta","DarkOliveGreen","DarkOrange",
                            "DarkOrchid","DarkRed","DarkSalmon","DarkSeaGreen","DarkSlateBlue","DarkSlateGray","DarkTurquoise","DarkViolet",
                            "DeepPink","DeepSkyBlue","DimGray","DodgerBlue","Firebrick","FloralWhite","ForestGreen","Fuchsia","Gainsboro",
                            "GhostWhite","Gold","Goldenrod","Gray","Green","GreenYellow","Honeydew","HotPink","IndianRed","Indigo","Ivory","Khaki",
                            "Lavender","LavenderBlush","LawnGreen","LemonChiffon","LightBlue","LightCoral","LightCyan","LightGoldenrodYellow",
                            "LightGray","LightGreen","LightPink","LightSalmon","LightSeaGreen","LightSkyBlue","LightSlateGray","LightSteelBlue",
                            "LightYellow","Lime","LimeGreen","Linen","Magenta","Maroon","MediumAquamarine","MediumBlue","MediumOrchid",
                            "MediumPurple","MediumSeaGreen","MediumSlateBlue","MediumSpringGreen","MediumTurquoise","MediumVioletRed",
                            "MidnightBlue","MintCream","MistyRose","Moccasin","NavajoWhite","Navy","OldLace","Olive","OliveDrab","Orange",
                            "OrangeRed","Orchid","PaleGoldenrod","PaleGreen","PaleTurquoise","PaleVioletRed","PapayaWhip","PeachPuff","Peru",
                            "Pink","Plum","PowderBlue","Purple","Red","RosyBrown","RoyalBlue","SaddleBrown","Salmon","SandyBrown","SeaGreen",
                            "SeaShell","Sienna","Silver","SkyBlue","SlateBlue","SlateGray","Snow","SpringGreen","SteelBlue","Tan","Teal",
                            "Thistle","Tomato",/*"Transparent",*/"Turquoise","Violet","Wheat","White","WhiteSmoke","Yellow","YellowGreen"};
        public static Color[] colors = { Color.AliceBlue, Color.AntiqueWhite, Color.Aqua, Color.Aquamarine, Color.Azure, Color.Beige, Color.Bisque, Color.Black,
                             Color.BlanchedAlmond, Color.Blue, Color.BlueViolet, Color.Brown, Color.BurlyWood, Color.CadetBlue, Color.Chartreuse, 
                             Color.Chocolate, Color.Coral, Color.CornflowerBlue, Color.Cornsilk, Color.Crimson, Color.Cyan, Color.DarkBlue, 
                             Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, 
                             Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, 
                             Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue, 
                             Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.FloralWhite, Color.ForestGreen, Color.Fuchsia, 
                             Color.Gainsboro, Color.GhostWhite, Color.Gold, Color.Goldenrod, Color.Gray, Color.Green, Color.GreenYellow, 
                             Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, Color.Ivory, Color.Khaki, Color.Lavender, 
                             Color.LavenderBlush, Color.LawnGreen, Color.LemonChiffon, Color.LightBlue, Color.LightCoral, Color.LightCyan, 
                             Color.LightGoldenrodYellow, Color.LightGray, Color.LightGreen, Color.LightPink, Color.LightSalmon, 
                             Color.LightSeaGreen, Color.LightSkyBlue, Color.LightSlateGray, Color.LightSteelBlue, Color.LightYellow, Color.Lime, 
                             Color.LimeGreen, Color.Linen, Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, 
                             Color.MediumOrchid, Color.MediumPurple, Color.MediumSeaGreen, Color.MediumSlateBlue, Color.MediumSpringGreen, 
                             Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, Color.MintCream, Color.MistyRose, Color.Moccasin, 
                             Color.NavajoWhite, Color.Navy, Color.OldLace, Color.Olive, Color.OliveDrab, Color.Orange, Color.OrangeRed, 
                             Color.Orchid, Color.PaleGoldenrod, Color.PaleGreen, Color.PaleTurquoise, Color.PaleVioletRed, Color.PapayaWhip, 
                             Color.PeachPuff, Color.Peru, Color.Pink, Color.Plum, Color.PowderBlue, Color.Purple, Color.Red, Color.RosyBrown, 
                             Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, Color.SandyBrown, Color.SeaGreen, Color.SeaShell, Color.Sienna, 
                             Color.Silver, Color.SkyBlue, Color.SlateBlue, Color.SlateGray, Color.Snow, Color.SpringGreen, Color.SteelBlue, 
                             Color.Tan, Color.Teal, Color.Thistle, Color.Tomato, /*Color.Transparent,*/ Color.Turquoise, Color.Violet, Color.Wheat, 
                             Color.White, Color.WhiteSmoke, Color.Yellow, Color.YellowGreen };

        public static Color[] darkColors = { /*Color.AliceBlue,*/ /*Color.Aqua, Color.Aquamarine, Color.Azure, Color.Bisque,*/ Color.Black,
                             Color.Blue, Color.BlueViolet, /*Color.Brown, Color.BurlyWood,*/ Color.CadetBlue, /*Color.Chartreuse,*/ 
                             Color.Chocolate, /*Color.Coral,*/ /*Color.CornflowerBlue,*/ /*Color.Cornsilk,*/ Color.Crimson, Color.Cyan, Color.DarkBlue, 
                             Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, 
                             Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, 
                             Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue, 
                             Color.DimGray, Color.DodgerBlue, Color.Firebrick, Color.ForestGreen, Color.Fuchsia, 
                             Color.Gainsboro, Color.Gold, Color.Goldenrod, /*Color.Gray,*/ Color.Green, /*Color.GreenYellow,*/ 
                             Color.Honeydew, Color.HotPink, Color.IndianRed, Color.Indigo, /*Color.Khaki,*/ Color.Lavender, 
                             /*Color.LavenderBlush,*/ Color.LawnGreen, /*Color.LemonChiffon,*/ Color.Lime, 
                             Color.LimeGreen, /*Color.Linen,*/ Color.Magenta, Color.Maroon, Color.MediumAquamarine, Color.MediumBlue, 
                             /*Color.MediumOrchid,*/ Color.MediumPurple, /*Color.MediumSeaGreen,*/ Color.MediumSlateBlue, Color.MediumSpringGreen, 
                             Color.MediumTurquoise, Color.MediumVioletRed, Color.MidnightBlue, /*Color.MistyRose,*/ /*Color.Moccasin,*/ 
                             Color.Navy, /*Color.OldLace,*/ Color.Olive, /*Color.OliveDrab,*/ Color.Orange, Color.OrangeRed, 
                             /*Color.Orchid, Color.PaleVioletRed, Color.PapayaWhip, */
                             /*Color.PeachPuff,*/ /*Color.Peru,*/ Color.Pink, Color.Plum, /*Color.PowderBlue,*/ Color.Purple, Color.Red, Color.RosyBrown, 
                             Color.RoyalBlue, Color.SaddleBrown, Color.Salmon, /*Color.SandyBrown,*/ Color.SeaGreen, /*Color.Sienna,*/ 
                             /*Color.Silver,*/ Color.SkyBlue, Color.SlateBlue, /*Color.SlateGray,*/ Color.SpringGreen, Color.SteelBlue, 
                             /*Color.Tan,*/ Color.Teal, Color.Thistle, Color.Tomato, Color.Turquoise, Color.Violet, /*Color.Wheat,*/ 
                             /*Color.Yellow,*/ Color.YellowGreen };





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
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
//                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5},
                                            { -0.5, 1.0, -1.0, 1.0, -1.0, 1.0, -0.5}};

        //static double[,] grid2Wave =      { { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5},
        //                                    { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5}, 
        //                                    { -0.5, 1.0, -1.5, 2.0, -1.5, 1.0, -0.5}};
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

        static double[,] grid2Wave =      { { -0.5, -0.5, -0.5 },
                                            {  1.0,  1.0,  1.0 },
                                            { -1.5, -1.5, -1.5 }, 
                                            {  2.0,  2.0,  2.0 }, 
                                            { -1.5, -1.5, -1.5 }, 
                                            {  1.0,  1.0,  1.0 },
                                            { -0.5, -0.5, -0.5 }};

        static double[,] grid3Wave =      { { -0.5, -0.5, -0.5 },
                                            {  1.0,  1.0,  1.0 },
                                            { -0.5, -0.5, -0.5 }, 
                                            { -1.0, -1.0, -1.0 }, 
                                            {  2.0,  2.0,  2.0 }, 
                                            { -1.0, -1.0, -1.0 }, 
                                            { -0.5, -0.5, -0.5 }, 
                                            {  1.0,  1.0,  1.0 },
                                            { -0.5, -0.5, -0.5 }};




        
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
                    if (ImageTools.Verbose) Console.WriteLine("Applied highPassKernal2 Kernal");
                    break;
                case Kernal.HorizontalLine3: kernal = horiLineKernal3;
                    break;
                case Kernal.HorizontalLine5: kernal = horiLineKernal5;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Horizontal Line5 Kernal");
                    break;
                case Kernal.VerticalLine: kernal = vertLineKernal;
                    break;
                case Kernal.DiagLine1: kernal = diagLineKernal1;
                    if (ImageTools.Verbose) Console.WriteLine("Applied diagLine1 Kernal");
                    break;
                case Kernal.DiagLine2: kernal = diagLineKernal2;
                    if (ImageTools.Verbose) Console.WriteLine("Applied diagLine2 Kernal");
                    break;
                case Kernal.Laplace1: kernal = Laplace1Kernal;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Laplace1 Kernal");
                    break;
                case Kernal.Laplace2: kernal = Laplace2Kernal;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Laplace2 Kernal");
                    break;
                case Kernal.Laplace3: kernal = Laplace3Kernal;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Laplace3 Kernal");
                    break;
                case Kernal.Laplace4: kernal = Laplace4Kernal;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Laplace4 Kernal");
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



        public static double[,] GridFilter(double[,] m, Kernal name)
        {
            double[,] kernal;
            int noiseSampleCount = 500000;
            //double thresholdZScore = 3.1;  //zscore threshold for p=0.001
            //double thresholdZScore = 2.58; //zscore threshold for p=0.005
            //double thresholdZScore = 2.33; //zscore threshold for p=0.01
            double thresholdZScore = 1.98;   //zscore threshold for p=0.05

            //SWITCH KERNALS
            switch (name)
            {
                case Kernal.Grid2: kernal = grid2;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid3: kernal = grid3;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid4: kernal = grid4;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Grid Kernal 2");
                    break;
                case Kernal.Grid2Wave: kernal = grid2Wave;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Grid Wave Kernal 2");
                    break;
                case Kernal.Grid3Wave: kernal = grid3Wave;
                    if (ImageTools.Verbose) Console.WriteLine("Applied Grid Wave Kernal 3");
                    break;


                default:
                    throw new System.Exception("\nWARNING: INVALID MODE!");
            }//end of switch statement


            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            int kRows = kernal.GetLength(0);
            int kCols = kernal.GetLength(1);
            int rNH = kRows / 2;
            int cNH = kCols / 2;
            if ((rNH <= 0) && (cNH <= 0)) return m; //no operation required
            //int area = ((2 * cNH) + 1) * ((2 * rNH) + 1);//area of rectangular neighbourhood

            double[,] normM = DataTools.normalise(m);

            double[] noiseScores = new double[noiseSampleCount];
            for (int n = 0; n < noiseSampleCount; n++)
            {
                double[,] noise = GetNoise(normM, kRows, kCols);
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
            if (ImageTools.Verbose) Console.WriteLine("noiseAv=" + noiseAv + "   noiseSd=" + noiseSd);

            double[,] newMatrix = new double[mRows, mCols];//init new matrix to return

            //now do bulk of image
            for (int r = rNH; r < (mRows - rNH); r++)
                for (int c = cNH; c < (mCols - cNH); c++)
                {
                    double sum = 0.0;
                    for (int y = -rNH; y < rNH; y++)
                        for (int x = -cNH; x < cNH; x++)
                        {
                            sum += (normM[r + y, c + x] * kernal[rNH + y, cNH + x]);
                        }
                    sum /= (double)kRows;
                    double zScore = (sum - noiseAv) / noiseSd;


                    if (zScore >= thresholdZScore)
                    {
                        newMatrix[r, c] = 1.0;
                        for (int n = -rNH; n < rNH; n++) newMatrix[r + n, c] = 1.0;
                        //newMatrix[r, c] = zScore;
                        //newMatrix[r + 1, c] = zScore;
                    }
                    //else newMatrix[r, c] = 0.0;
                }//end of loops
            return newMatrix;
        }//end method GridFilter()


        /// <summary>
        /// Returns a small matrix of pixels chosen randomly from the passed matrix, m.
        /// The row and column is chosen randomly and then the reuired number of consecutive pixels is transferred.
        /// These noise matrices are used to obtain statistics for cross-correlation coefficients.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="kRows"></param>
        /// <param name="kCols"></param>
        /// <returns></returns>
        public static double[,] GetNoise(double[,] m, int kRows, int kCols)
        {
            int mHeight = m.GetLength(0);
            int mWidth  = m.GetLength(1);

            double[,] noise = new double[kRows, kCols];
            RandomNumber rn = new RandomNumber();
            for (int r = 0; r < kRows; r++)
            {
                int randomRow = rn.GetInt(mHeight - kRows);
                int randomCol = rn.GetInt(mWidth - kCols);
                for (int c = 0; c < kCols; c++)
                    noise[r, c] = m[randomRow, randomCol+c];
            }
            return noise;
        } //end getNoise()




        /// <summary>
        /// This version of Sobel's edge detection taken from  Graig A. Lindley, Practical Image Processing
        /// which includes C code.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double[,] SobelEdgeDetection(double[,] m)
        {
            //define indices into grid using Lindley notation
            const int a = 0; const int b = 1; const int c = 2; const int d = 3; const int e = 4;
            const int f = 5; const int g = 6; const int h = 7; const int i = 8;
            int mRows = m.GetLength(0);
            int mCols = m.GetLength(1);
            double[,] normM = DataTools.normalise(m);
            double[,] newMatrix = new double[mRows, mCols];//init new matrix to return
            double[] grid = new double[9]; //to represent 3x3 grid
            double min = Double.MaxValue; double max = -Double.MaxValue;

            for (int y = 1; y < mRows-1; y++)
                for (int x = 1; x < mCols-1; x++)
                {
                    grid[a] = normM[y - 1, x - 1];
                    grid[b] = normM[y,     x - 1];
                    grid[c] = normM[y + 1, x - 1];
                    grid[d] = normM[y - 1, x];
                    grid[e] = normM[y,     x];
                    grid[f] = normM[y + 1, x];
                    grid[g] = normM[y - 1, x + 1];
                    grid[h] = normM[y,     x + 1];
                    grid[i] = normM[y + 1, x + 1];
                    double[] differences = new double[4];
                    double DivideAEI_avBelow = (grid[d] + grid[g] + grid[h]) / (double)3;
                    double DivideAEI_avAbove = (grid[b] + grid[c] + grid[f]) / (double)3;
                    differences[0] = Math.Abs(DivideAEI_avAbove - DivideAEI_avBelow);

                    double DivideBEH_avBelow = (grid[a] + grid[d] + grid[g]) / (double)3;
                    double DivideBEH_avAbove = (grid[c] + grid[f] + grid[i]) / (double)3;
                    differences[1] = Math.Abs(DivideBEH_avAbove - DivideBEH_avBelow);

                    double DivideCEG_avBelow = (grid[f] + grid[h] + grid[i]) / (double)3;
                    double DivideCEG_avAbove = (grid[a] + grid[b] + grid[d]) / (double)3;
                    differences[2] = Math.Abs(DivideCEG_avAbove - DivideCEG_avBelow);

                    double DivideDEF_avBelow = (grid[g] + grid[h] + grid[i]) / (double)3;
                    double DivideDEF_avAbove = (grid[a] + grid[b] + grid[c]) / (double)3;
                    differences[3] = Math.Abs(DivideDEF_avAbove - DivideDEF_avBelow);
                    double gridMin; double gridMax;
                    DataTools.MinMax(differences, out gridMin, out gridMax);

                    newMatrix[y, x] = gridMax;
                    if(min > gridMin) min = gridMin;
                    if(max < gridMax) max = gridMax;
                }

            double threshold = min + (max - min) / 5;

            for (int y = 1; y < mRows - 1; y++)
                for (int x = 1; x < mCols - 1; x++)
                    if (newMatrix[y, x] > threshold) newMatrix[y, x] = 1.0;
                    else newMatrix[y, x] = 0.0;

            return newMatrix;
        }



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




        // ###################################################################################################################################

        /// <summary>
        /// returns the upper and lower thresholds for the pass upper and lower percentile cuts of matrix M
        /// Used for some of the noise reduciton algorithms
        /// </summary>
        /// <param name="M"></param>
        /// <param name="lowerCut"></param>
        /// <param name="upperCut"></param>
        /// <param name="lowerThreshold"></param>
        /// <param name="upperThreshold"></param>
        public static void PercentileThresholds(double[,] M, double lowerCut, double upperCut, out double lowerThreshold, out double upperThreshold)
        {
            int binCount = 50;
            int count = M.GetLength(0) * M.GetLength(1); 
            double binWidth;
            double min; double max;
            int[] powerHisto = DataTools.Histo(M, binCount, out binWidth, out min, out max);
            powerHisto[binCount - 1] = 0;   //just in case it is the max ????????????????????????????????????? !!!!!!!!!!!!!!!
            double[] smooth = DataTools.filterMovingAverage(powerHisto, 3);
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
        }// end of TrimPercentiles()

// ###################################################################################################################################


        /// <summary>
        /// Calculates the local signal to noise ratio in the neighbourhood of side=window
        /// SNR is defined as local mean / local std dev.
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



        // ###################################################################################################################################


        public static double[] TransferFunction(int[] histo, double min, double max, double binWidth, int binLimit, out double modalIntensity)
        {
            int binCount = histo.Length;
            double[] smoothHisto = DataTools.filterMovingAverage(histo, 7);
            int maxindex; //mode
            DataTools.getMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
            if (maxindex > binLimit) maxindex = binLimit;
            modalIntensity = (maxindex * binWidth) + min;
            //Console.WriteLine("  modal index=" + maxindex + "  modalIntensity=" + modalIntensity.ToString("F3"));

            //init transfer function, tf
            double[] tf = new double[binCount];
            //set all values below mode = 0.0
            for (int i = 0; i < maxindex; i++) tf[i] = 0.0;
            //assume noise is gaussian and upper bound is twice mode
            int upperBound = 2 * maxindex;
            if (upperBound > binCount) upperBound = binCount;
            //set tf[i] to probability that value is signal
            int offset = 0;
            for (int i = maxindex; i < upperBound; i++)
            {
                double noise = smoothHisto[maxindex - offset];
                offset++;
                double prob = (smoothHisto[i] - noise) / smoothHisto[i];
                if (prob < 0.0) prob = 0.0;
                tf[i] = prob;
                //tf[i] = Math.Sqrt(prob);
            }
            // above the noise band set transfer function = 1.0;
            for (int i = upperBound; i < binCount; i++) tf[i] = 1.0;
            //if (tf[binCount - 1] == 0.0) return new double[binCount];
            return tf;
        }


        public static double[,] NoiseReduction(double[,] matrix)
        {
            //set parameters for noise histograms based on overlapping bands.
            //*******************************************************************************************************************
            int ncbbc = 4;  //number of columns between band centres
            int bandWidth = 4;
            int binCount = 64;  //number of pixel intensity bins
            int binLimit = (int)(binCount * 0.7); //sets upper limit to location of modal noise bin. Higher values = more severe noise removal.
            double momentum = 0.4;  //determines how rapidly Transfer Function changes as one ascends the freq bands
            double noiseThreshold = 0.0; // trim all values if < X dB above modal noise
            //*******************************************************************************************************************


            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            DataTools.MinMax(matrix, out minIntensity, out maxIntensity);
            double binWidth = (maxIntensity - minIntensity) / binCount;  //width of an intensity bin

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            if (bandWidth > colCount) bandWidth = colCount - 1;
            int halfWidth = bandWidth / 2;
            int bandCount = colCount / ncbbc;

            //Console.WriteLine("matrix rows =" + rowCount + " matrix cols=" + colCount+ "  bandWidth=" + bandWidth);
            //Console.WriteLine("minIntensity=" + minIntensity+"  maxIntensity="+maxIntensity+"  binWidth="+binWidth);

            //init matrix from which histogram and transfer fnc derived
            double[,] submatrix = DataTools.Submatrix(matrix, 0, 0, rowCount - 1, bandWidth);
           // double[,] tmpM = new double[rowCount, ncbbc];  //matrix to which transfer function is applied
            double[,] outM = new double[rowCount, colCount];
            double[] transferFnc = null;
            double[] prevTransferFnc = null;
            int nextCol = 0;
            double thresholdIntensity = -Double.MaxValue;

            for (int col = 0; col < colCount; col++)//for all cols
            {
                //Console.WriteLine("#########################################col=" + col + "   nextCol=" + nextCol);
                if (col == nextCol)  //recalculate the transfer function
                {

                    prevTransferFnc = transferFnc; //to smooth the transition between functions
                    nextCol += ncbbc;

                    //construct new submatrix to recalculate the current transfer function
                    int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                    if (start < 0) start = 0;
                    int stop = col + halfWidth;
                    if (stop >= colCount) stop = colCount - 1;
                    submatrix = DataTools.Submatrix(matrix, 0, start, rowCount - 1, stop);
                    double modalNoise;
                    int[] histo = DataTools.Histo(submatrix, binCount, minIntensity, maxIntensity, binWidth);
                    //DataTools.writeBarGraph(histo);
                    transferFnc = TransferFunction(histo, minIntensity, maxIntensity, binWidth, binLimit, out modalNoise);
                    thresholdIntensity = modalNoise + noiseThreshold;
                    //Console.WriteLine("        have calc transfer func");
                }

                for (int y = 0; y < rowCount; y++) //for all rows
                {
                    int intensityIndex = (int)Math.Floor((matrix[y, col] - minIntensity) / binWidth);
                    if (intensityIndex >= binCount) intensityIndex = binCount - 1;
                    else if (intensityIndex < 0) intensityIndex = 0;
                    double factor = transferFnc[intensityIndex]; //do not use momentum
                    if (prevTransferFnc != null) factor = ((1 - momentum) * transferFnc[intensityIndex]) + (momentum * prevTransferFnc[intensityIndex]);
                    //factor = factor * factor;
                    int newIndex = (int)(intensityIndex * factor);
                    double newIntensity = minIntensity + (newIndex * binWidth);
                    if (newIntensity < thresholdIntensity) newIntensity = minIntensity;

                    // SEVERAL DIFFERENT IMAGE OUTPUT OPTIONS
                    outM[y, col] = newIntensity;          // (1) multiply pixel by factor
                    //if (factor > 0.7) outM[y, col] = 1.0; // (2) produce a binary image
                }
            }//for all cols
            return outM;
        }// end of NoiseReduction()



        /// <summary>
        /// Detect high intensity / high energy regions in an image using blurring
        /// followed by rules involving positive and negative gradients.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] DetectHighEnergyRegions1(double[,] matrix)
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
                double lowerThreshold; double upperThreshold;
                PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);
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

            int minRowWidth = 2;
            int minColWidth = 5;
            M = Shapes_RemoveSmall(M, minRowWidth, minColWidth);
            return M;
        }// end of DetectHighEnergyRegions1()



        /// <summary>
        /// Detect high intensity / high energy regions in an image using blurring
        /// followed by bandwise thresholding.
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] DetectHighEnergyRegions3(double[,] matrix)
        {
            double lowerShoulder = 0.3;   //used to increase/decrease the intensity threshold from modal value
            double upperShoulder = 0.4;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 7;
            int tWindow = 7;
            double[,] blurM = ImageTools.Blur(matrix, fWindow, tWindow);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(blurM, 0, 0, height - 1, bandWidth);
            double lowerThreshold; double upperThreshold;
            PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);

            for (int col = 0; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                {
                    subMatrix = DataTools.Submatrix(blurM, 0, start, height - 1, stop);
                    PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);
                }

                for (int y = 0; y < height; y++)
                {
                    if (blurM[y, col] < upperThreshold) M[y, col] = 0.0;
                    else M[y, col] = 1.0;
                }
            }//for all cols
            return M;
        }// end of Shapes2()



        public static double[,] Shapes3(double[,] m)
        {
            double[,] m1 = ImageTools.DetectHighEnergyRegions3(m); //detect blobs of high acoustic energy
            double[,] m2 = ImageTools.Shapes_lines(m);

            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (m2[y, x] == 0.0) continue; //nothing here
                    if (tmpM[y, x] == 1.0) continue; //already have something here

                    int colWidth; //colWidth of object
                    Shape.Col_Width(m2, x, y, out colWidth);
                    int x2 = x + colWidth; 
                    for (int j = x; j < x2; j++) tmpM[y, j] = 1.0;
 
                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 15;
                    int dist = 1;
                    while (((x2 + dist) < width) && (m2[y, x2 + dist] == 0)) { dist++; }
                    if (((x2 + dist) < width) && (dist < thresholdDistance)) for (int d = 0; d < dist; d++) tmpM[y, x2 + d] = 1.0;
                }
            }

            //transfer line objects to output matrix IF they overlap a high energy blob in m1
            double[,] outM = new double[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0) continue; //nothing here
                    if (outM[y, x] == 1.0) continue; //already have something here

                    //int rowWidth; //rowWidth of object
                    //Shape.Row_Width(m2, x, y, out rowWidth);
                    int colWidth; //colWidth of object
                    Shape.Col_Width(tmpM, x, y, out colWidth);
                    int x2 = x + colWidth;
                    //check to see if object is in blob
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if (m1[y, j] == 1.0)
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }//end of ascertaining if line overlapsHighEnergy
                    if (overlapsHighEnergy) for (int j = x; j < x2; j++) outM[y, j] = 1.0;
                }
            }

            outM = FillGaps(outM);
            int minRowWidth = 2;
            int minColWidth = 4;
            outM = Shapes_RemoveSmallUnattached(outM, minRowWidth, minColWidth);
            return outM;
        }



        public static ArrayList Shapes4(double[,] m)
        {
            double[,] m1 = ImageTools.DetectHighEnergyRegions3(m); //binary matrix showing areas of high acoustic energy
            double[,] m2 = ImageTools.Shapes_lines(m); //binary matrix showing high energy lines

            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];
            ArrayList shapes = new ArrayList();

            //transfer m2 lines spectrogram to temporary matrix and merge adjacent high energy objects
            for (int y = 0; y < height; y++) //row at a time
            {
                for (int x = 0; x < width; x++) //transfer values to tmpM
                {
                    if (m2[y, x] == 0.0) continue; //nothing here
                    if (tmpM[y, x] == 1.0) continue; //already have something here

                    int colWidth; //colWidth of object
                    Shape.Col_Width(m2, x, y, out colWidth);
                    int x2 = x + colWidth-1;
                    for (int j = x; j < x2; j++) tmpM[y, j] = 1.0;

                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 10;
                    int dist = 1;
                    while (((x2 + dist) < width) && (m2[y, x2 + dist] == 0)) { dist++; }
                    if (((x2 + dist) < width) && (dist < thresholdDistance)) for (int d = 0; d < dist; d++) tmpM[y, x2 + d] = 1.0;
                }
                y++; //only even rows
            }

            //transfer line objects to output matrix IF they overlap a high energy region in m1
            int objectCount = 0;
            double[,] outM = new double[height, width];
            for (int y = 0; y < height-2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0) continue; //nothing here
                    if (outM[y, x] == 1.0) continue; //already have something here

                    int colWidth; //colWidth of object
                    Shape.Col_Width(tmpM, x, y, out colWidth);

                    int x2 = x + colWidth;
                    //check to see if object is in high energy region
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if ((m1[y+1, j] == 1.0) || (m1[y, j] == 1.0))
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }//end of ascertaining if line overlapsHighEnergy
                    if (overlapsHighEnergy)
                    {
                        shapes.Add(new Shape(y, x, y + 1, x2));
                        objectCount++;
                        for (int j = x; j < x2; j++) outM[y, j] = 1.0;
                        for (int j = x; j < x2; j++) tmpM[y, j] = 0.0;
                        for (int j = x; j < x2; j++) outM[y+1, j] = 1.0;
                        for (int j = x; j < x2; j++) tmpM[y+1, j] = 0.0;
                    }
                }//end cols
            }//end rows

            //NOW DO SHAPE MERGING TO REDUCE NUMBERS
            if (ImageTools.Verbose) Console.WriteLine("Object Count 1 =" + objectCount);
            int dxThreshold = 25; //upper limit on centroid displacement - set higher for fewer bigger shapes
            double widthRatio = 5.0; //upper limit on difference in shape width - set higher for fewer bigger shapes
            shapes = Shape.MergeShapesWithAdjacentRows(shapes, dxThreshold, widthRatio);
            if (ImageTools.Verbose) Console.WriteLine("Object Count 2 =" + shapes.Count);
            //shapes = Shape.RemoveEnclosedShapes(shapes);
            shapes = Shape.RemoveOverlappingShapes(shapes);
            int minArea = 14;
            shapes = Shape.RemoveSmall(shapes, minArea);
            if (ImageTools.Verbose) Console.WriteLine("Object Count 3 =" + shapes.Count);
            return shapes;
        }

        /// <summary>
        /// Returns an ArrayList of rectabgular shapes that represent syllables in the sonogram.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static ArrayList Shapes5(double[,] m)
        {
            //get binary matrix showing high energy regions
            int fWindow = 5;
            int tWindow = 3;
            double[,] tmp = ImageTools.Blur(m, fWindow, tWindow);
            double threshold = 0.2;
            double[,] m1 = DataTools.Threshold(tmp, threshold);

            //get binary matrix showing high energy lines
            double[,] m2 = ImageTools.Convolve(tmp, Kernal.HorizontalLine5);
            threshold = 0.2;
            m2 = DataTools.Threshold(m2, threshold); 


            //prepare to extract acoustic events or shapes
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] tmpM = new double[height, width];
            ArrayList shapes = new ArrayList();
            //transfer m2 lines spectrogram to temporary matrix and join adjacent high energy objects
            for (int y = 0; y < height; y++) //row at a time
            {
                for (int x = 0; x < width; x++) //transfer values to tmpM
                {
                    if (m2[y, x] == 0.0) continue; //nothing here
                    if (tmpM[y, x] == 1.0) continue; //already have something here

                    int colWidth; //colWidth of object
                    Shape.Col_Width(m2, x, y, out colWidth);
                    int x2 = x + colWidth - 1;
                    for (int j = x; j < x2; j++) tmpM[y, j] = 1.0;

                    //find distance to nearest object in hi frequency direction
                    // and join the two if within threshold distance
                    int thresholdDistance = 10;
                    int dist = 1;
                    while (((x2 + dist) < width) && (m2[y, x2 + dist] == 0)) { dist++; }
                    if (((x2 + dist) < width) && (dist < thresholdDistance)) for (int d = 0; d < dist; d++) tmpM[y, x2 + d] = 1.0;
                }
                y++; //only even rows
            }

            //transfer line objects to output matrix IF they overlap a high energy region in m1
            int objectCount = 0;
            double[,] outM = new double[height, width];
            for (int y = 0; y < height - 2; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tmpM[y, x] == 0.0) continue; //nothing here
                    if (outM[y, x] == 1.0) continue; //already have something here

                    int colWidth; //colWidth of object
                    Shape.Col_Width(tmpM, x, y, out colWidth);

                    int x2 = x + colWidth;
                    //check to see if object is in high energy region
                    bool overlapsHighEnergy = false;
                    for (int j = x; j < x2; j++)
                    {
                        if ((m1[y + 1, j] == 1.0) || (m1[y, j] == 1.0))
                        {
                            overlapsHighEnergy = true;
                            break;
                        }
                    }//end of ascertaining if line overlapsHighEnergy
                    if (overlapsHighEnergy)
                    {
                        shapes.Add(new Shape(y, x, y + 1, x2));
                        objectCount++;
                        for (int j = x; j < x2; j++) outM[y, j] = 1.0;
                        for (int j = x; j < x2; j++) tmpM[y, j] = 0.0;
                        for (int j = x; j < x2; j++) outM[y + 1, j] = 1.0;
                        for (int j = x; j < x2; j++) tmpM[y + 1, j] = 0.0;
                    }
                }//end cols
            }//end rows

            //NOW DO SHAPE MERGING TO REDUCE NUMBERS
            if (ImageTools.Verbose) Console.WriteLine("Object Count 1 =" + objectCount);
            int dxThreshold = 25; //upper limit on centroid displacement - set higher for fewer bigger shapes
            double widthRatio = 4.0; //upper limit on difference in shape width - set higher for fewer bigger shapes
            shapes = Shape.MergeShapesWithAdjacentRows(shapes, dxThreshold, widthRatio);
            if (ImageTools.Verbose) Console.WriteLine("Object Count 2 =" + shapes.Count);
            shapes = Shape.RemoveEnclosedShapes(shapes);
            //shapes = Shape.RemoveOverlappingShapes(shapes);
            int minArea = 30;
            shapes = Shape.RemoveSmall(shapes, minArea);
            if (ImageTools.Verbose) Console.WriteLine("Object Count 3 =" + shapes.Count);
            return shapes;
        }
        
        
        
        /// <summary>
        /// Returns a binary matrix containing high energy lines in the oriignal spectrogram 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Shapes_lines(double[,] matrix)
        {
            double threshold = 0.3;   

            int fWindow = 5;
            int tWindow = 3;
            double[,] tmpM = ImageTools.Blur(matrix, fWindow, tWindow);
            //double[,] tmpM = ImageTools.Convolve(matrix, Kernal.HorizontalLine5);
            tmpM = ImageTools.Convolve(tmpM, Kernal.HorizontalLine5);
            //tmpM = ImageTools.Convolve(tmpM, Kernal.HorizontalLine5);

            //int height = matrix.GetLength(0);
            //int width = matrix.GetLength(1);
            //double[,] M = new double[height, width];
            double[,] M = DataTools.Threshold(tmpM, threshold); 
            return M;
        }// end of Shapes_lines()



        /// <summary>
        /// Returns a binary matrix containing high energy lines in the original spectrogram
        /// calculates the threshold bandwise
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Shapes_lines_bandwise(double[,] matrix)
        {
            double lowerShoulder = 0.7;   //used to increase or decrease the threshold from modal value
            double upperShoulder = 0.1;
            int bandWidth = 64;
            int halfWidth = bandWidth / 2;

            int fWindow = 3;
            int tWindow = 3;
            double[,] tmpM = ImageTools.Blur(matrix, fWindow, tWindow);
            tmpM = ImageTools.Convolve(tmpM, Kernal.HorizontalLine5);
            tmpM = ImageTools.Convolve(tmpM, Kernal.HorizontalLine5);

            int height = matrix.GetLength(0);
            int width = matrix.GetLength(1);
            double[,] M = new double[height, width];

            double[,] subMatrix = DataTools.Submatrix(tmpM, 0, 0, height - 1, bandWidth);
            double lowerThreshold; double upperThreshold;
            PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);

            for (int col = 2; col < width; col++)//for all cols
            {
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= width) stop = width - 1;

                if ((col % 8 == 0) && (!(col == 0)))
                {
                    subMatrix = DataTools.Submatrix(tmpM, 0, start, height - 1, stop);
                    PercentileThresholds(subMatrix, lowerShoulder, upperShoulder, out lowerThreshold, out upperThreshold);
                }

                for (int y = 1; y < height - 1; y++)
                {
                    bool evenRow = (y % 2 == 0);
                    if (tmpM[y, col] > upperThreshold)
                    {
                        M[y, col] = 1;
                        if (evenRow) M[y + 1, col] = 1;
                        else M[y - 1, col] = 1;
                        //fill in gaps
                        if ((M[y, col - 2] == 1.0) && (M[y, col - 1] == 0.0)) M[y, col - 1] = 1;
                    }
                }
            }//for all cols
            int minRowWidth = 2;
            int minColWidth = 5;
            M = Shapes_RemoveSmall(M, minRowWidth, minColWidth);
            return M;
        }// end of Shapes_lines()

        


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
                    bool sizeOK = false;
                    if ((rowWidth >= minRowWidth) && (colWidth >= minColWidth)) sizeOK = true;

                    if (sizeOK)
                    {
                        for (int c = 0; c < colWidth; c++)
                        {
                            for (int r = 0; r < minRowWidth; r++)
                            {
                                M[y + r, x + c] = 1.0;
                            }
                        }
                    }
                    y += (rowWidth-1);
                }//end y loop
            }//end x loop
            //M = m;

            return M;
        }


        public static double[,] Shapes_RemoveSmallUnattached(double[,] m, int minRowWidth, int minColWidth)
        {
            int height = m.GetLength(0);
            int width = m.GetLength(1);
            double[,] M = new double[height, width];

            for (int x = 0; x < width; x++)
            {
                for (int y = 1; y < height - 3; y++)
                {
                    if (m[y, x] == 0.0) continue; //nothing here
                    if (M[y, x] == 1.0) continue; //already have something here

                    int rowWidth; //rowWidth of object
                    Shape.Row_Width(m, x, y, out rowWidth);
                    int colWidth; //colWidth of object
                    Shape.Col_Width(m, x, y, out colWidth);
                    bool sizeOK = false;
                    if ((rowWidth >= minRowWidth) && (colWidth >= minColWidth)) sizeOK = true;

                    //now check if object is unattached to other object
                    bool attachedOK = false;
                    for (int j = x; j < x + colWidth; j++)
                    {
                        if ((m[y - 1, j] == 1.0) || /*(m[y + 1, j] == 1.0) ||*/ (m[y + 2, j] == 1.0) || (m[y + 3, j] == 1.0))
                        {
                            attachedOK = true;
                            break;
                        }
                    }//end of ascertaining if line overlapsHighEnergy

                    //attachedOK = true;
                    if (sizeOK && attachedOK)
                    {
                        for (int c = 0; c < colWidth; c++)
                        {
                            //Shape.Row_Width(m, x+c, y, out rowWidth);
                            for (int r = 0; r < minRowWidth; r++)
                            {
                                M[y + r, x + c] = 1.0;
                            }
                        }
                    }
                }//end y loop
            }//end x loop
            //M = m;

            return M;
        }

        public static double[,] FillGaps(double[,] m)
        {
            double coverThreshold = 0.7;
            int cNH = 4; //neighbourhood
            int rNH = 11;

            int height = m.GetLength(0);
            int width = m.GetLength(1);
            //double[,] M = new double[height, width];
            int area = ((2*cNH)+1)*((2*rNH)+1);
            //Console.WriteLine("area=" + area);

            for (int x = cNH; x < width - cNH; x++)
            {
                for (int y = rNH; y < height - rNH; y++)
                {
                    double sum = 0.0;
                    for (int r = -rNH; r < rNH; r++)
                        for (int c = -cNH; c < cNH; c++)
                        {
                            sum += m[y + r, x + c];
                        }
                    double cover = sum /(double) area;

                    if (cover >= coverThreshold)
                    {
                        m[y, x] = 1.0;
                        m[y-1, x] = 1.0;
                        m[y+1, x] = 1.0;
                        //m[y - 2, x] = 1.0;
                        //m[y + 2, x] = 1.0;
                    }
                }//end y loop
            }//end x loop

            return m;
        }


        public static Color[] GrayScale()
        {
            int max = 256;
            Color[] grayScale = new Color[256];
            for (int c = 0; c < max; c++) grayScale[c] = Color.FromArgb(c, c, c);
            return grayScale;
        }


        public static void DrawMatrix(double[,] matrix, string pathName)
        {
            int maxYpixels = 1000;
            int maxXpixels = 1000;
            
            int rows = matrix.GetLength(0); //number of rows
            int cols = matrix.GetLength(1); //number

            int cellYpixels = maxYpixels / rows;
            int cellXpixels = maxXpixels / cols;

            if (cellYpixels == 0) cellYpixels = 1;
            if (cellXpixels == 0) cellXpixels = 1;

            int Ypixels = cellYpixels * rows;
            int Xpixels = cellXpixels * cols;
            Console.WriteLine("Xpixels=" + Xpixels + "  Ypixels=" + Ypixels);
            Console.WriteLine("cellXpixels=" + cellXpixels + "  cellYpixels=" + cellYpixels);

            Color[] grayScale = GrayScale();


            Bitmap bmp = new Bitmap(Xpixels, Ypixels, PixelFormat.Format24bppRgb);

            double[,] norm = DataTools.normalise(matrix);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int colorId = (int)Math.Floor(norm[r, c] * 255);
                    int xOffset = (cellXpixels * c);
                    int yOffset = (cellYpixels * r);
                    //Console.WriteLine("xOffset=" + xOffset + "  yOffset=" + yOffset + "  colorId=" + colorId);

                    for (int x = 0; x < cellXpixels; x++)
                    {
                        for (int y = 0; y < cellYpixels; y++)
                        {
                            //Console.WriteLine("x=" + (xOffset+x) + "  yOffset=" + (yOffset+y) + "  colorId=" + colorId);
                            bmp.SetPixel(xOffset + x, yOffset + y, grayScale[colorId]);
                        }
                    }
                }//end all columns
            }//end all rows


            bmp.Save(pathName);
        }

    }//end class
}
