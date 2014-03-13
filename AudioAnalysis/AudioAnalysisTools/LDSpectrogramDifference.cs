using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class LDSpectrogramDifference
    {

        //PARAMETERS
        // set the X and Y axis scales for the spectrograms 
        private static int minOffset = 0;  // assume recording starts at zero minute of day i.e. midnight
        private static int xScale = SpectrogramConstants.XAXIS_SCALE;    // assume one minute spectra and hourly time lines
        private static int sampleRate = 17640; // default value - after resampling
        private static int frameWidth = 512;   // default value - from which spectrogram was derived
        private static string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; //CHANGE RGB mapping here.
        private static double backgroundFilterCoeff = 0.75; //must be value <=1.0
        private static double colourGain = 1.5;
        //double[] table_df_inf = { 0.25, 0.51, 0.67, 0.85, 1.05, 1.282, 1.645, 1.96, 2.326, 2.576, 3.09, 3.291 };
        //double[] table_df_15 =  { 0.26, 0.53, 0.69, 0.87, 1.07, 1.341, 1.753, 2.13, 2.602, 2.947, 3.73, 4.073 };
        //double[] alpha =        { 0.40, 0.30, 0.25, 0.20, 0.15, 0.10,  0.05,  0.025, 0.01, 0.005, 0.001, 0.0005 };

        //private static double tStatThreshold = 1.645; // 0.05% confidence @ df=infinity
        private static double tStatThreshold = 2.326; // 0.01% confidence @ df=infinity
        private static int titleHt = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;






        public static void DrawDifferenceSpectrogram(string ipdir, string ipFileName1, string ipFileName2, string opdir)
        {

            string opFileName1 = ipFileName1;
            var cs1 = new LDSpectrogramRGB(minOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = ipFileName1;
            cs1.ColorMODE = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.ReadCSVFiles(ipdir, ipFileName1, colorMap);
            //cs1.DrawGreyScaleSpectrograms(opdir, opFileName1);
            //cs1.DrawFalseColourSpectrograms(opdir, opFileName1);

            string opFileName2 = ipFileName2;
            var cs2 = new LDSpectrogramRGB(minOffset, xScale, sampleRate, frameWidth, colorMap);
            cs2.FileName = ipFileName2;
            cs2.ColorMODE = colorMap;
            cs2.BackgroundFilter = backgroundFilterCoeff;
            cs2.ReadCSVFiles(ipdir, ipFileName2, colorMap);
            //cs2.DrawGreyScaleSpectrograms(opdir, opFileName2);
            //cs2.DrawFalseColourSpectrograms(opdir, opFileName2);

            string title = String.Format("DIFFERENCE SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", ipFileName1, ipFileName2, cs1.ColorMODE);
            Image deltaSp = LDSpectrogramDifference.DrawDifferenceSpectrogram(cs2, cs1, colourGain);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, deltaSp.Width, titleHt);
            deltaSp = LDSpectrogramRGB.FrameSpectrogram(deltaSp, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
            string opFileName3 = ipFileName1 + ".Difference.COLNEG.png";
            deltaSp.Save(Path.Combine(opdir, opFileName3));
        }


        public static void DrawTStatisticThresholdedDifferenceSpectrograms(string ipdir, string ipFileName1, string ipSdFileName1,
                                                                                         string ipFileName2, string ipSdFileName2, string opdir)
        {

            string opFileName1 = ipFileName1;
            var cs1 = new LDSpectrogramRGB(minOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = ipFileName1;
            cs1.ColorMODE = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.ReadCSVFiles(ipdir, ipFileName1, colorMap);
            string imagePath = Path.Combine(opdir, opFileName1 + ".COLNEG.png");

            string opFileName2 = ipFileName2;
            var cs2 = new LDSpectrogramRGB(minOffset, xScale, sampleRate, frameWidth, colorMap);
            cs2.FileName = ipFileName2;
            cs2.ColorMODE = colorMap;
            cs2.BackgroundFilter = backgroundFilterCoeff;
            cs2.ReadCSVFiles(ipdir, ipFileName2, colorMap);

            int N = 30;
            cs1.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName1);
            cs1.SampleCount = N;
            cs2.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName2);
            cs2.SampleCount = N;

            string key = "ACI";
            Image tStatIndexImage = LDSpectrogramDifference.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            string opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir, opFileName3));

            key = "TEN";
            tStatIndexImage = LDSpectrogramDifference.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir, opFileName3));

            key = "CVR";
            tStatIndexImage = LDSpectrogramDifference.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir, opFileName3));

            tStatIndexImage = LDSpectrogramDifference.DrawTStatisticSpectrogramsOfMultipleIndices(cs1, cs2, tStatThreshold, colourGain);
            opFileName3 = ipFileName1 + ".difference.tTest.COLNEG.png";
            tStatIndexImage.Save(Path.Combine(opdir, opFileName3));
        }






        public static double[,] GetTStatisticMatrix(string key, LDSpectrogramRGB cs1, LDSpectrogramRGB cs2)
        {
            double[,] avg1 = cs1.GetSpectrogramMatrix(key);
            if (key.Equals("TEN"))
                avg1 = MatrixTools.SubtractValuesFromOne(avg1);

            double[,] std1 = cs1.GetStandarDeviationMatrix(key);

            double[,] avg2 = cs2.GetSpectrogramMatrix(key);
            if (key.Equals("TEN"))
                avg2 = MatrixTools.SubtractValuesFromOne(avg2);

            double[,] std2 = cs2.GetStandarDeviationMatrix(key);

            double[,] tStatMatrix = LDSpectrogramDifference.GetTStatisticMatrix(avg1, std1, cs1.SampleCount, avg2, std2, cs2.SampleCount);
            return tStatMatrix;
        }


        public static double[,] GetTStatisticMatrix(double[,] m1Av, double[,] m1Sd, int N1, double[,] m2Av, double[,] m2Sd, int N2)
        {
            int rows = m1Av.GetLength(0); //number of rows
            int cols = m1Av.GetLength(1); //number
            double avg1, avg2, std1, std2;
            double[,] M = new double[rows, cols];
            int expectedMinAvg = 0; // expected minimum average  of spectral dB above background
            int expectedMinVar = 1; // expected minimum variance of spectral dB above background

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    //catch low values of dB used to avoid log of zero amplitude.
                    avg1 = m1Av[row, column];
                    avg2 = m2Av[row, column];
                    std1 = m1Sd[row, column];
                    std2 = m2Sd[row, column];

                    if (avg1 < expectedMinAvg)
                    {
                        avg1 = expectedMinAvg;
                        std1 = expectedMinVar;
                    }
                    if (avg2 < expectedMinAvg)
                    {
                        avg2 = expectedMinAvg;
                        std2 = expectedMinVar;
                    }

                    M[row, column] = Statistics.tStatistic(avg1, std1, N1, avg2, std2, N2);
                }//end all columns
            }//end all rows

            return M;
        }

        //public static Dictionary<string, double[,]> GetTStatisticMatrices(ColourSpectrogram cs1, ColourSpectrogram cs2)
        //{
        //    string[] keys = cs1.ColorMap.Split('-');

        //    double[,] avg1a = cs1.spectrogramMatrices[keys[0]];
        //    if (keys[0].Equals("TEN")) avg1a = MatrixTools.SubtractValuesFromOne(avg1a);
        //    double[,] avg1b = cs1.spectrogramMatrices[keys[1]];
        //    if (keys[1].Equals("TEN")) avg1b = MatrixTools.SubtractValuesFromOne(avg1b);
        //    double[,] avg1c = cs1.spectrogramMatrices[keys[2]];
        //    if (keys[2].Equals("TEN")) avg1c = MatrixTools.SubtractValuesFromOne(avg1c);

        //    double[,] std1a = cs1.spgr_StdDevMatrices[keys[0]];
        //    double[,] std1b = cs1.spgr_StdDevMatrices[keys[1]];
        //    double[,] std1c = cs1.spgr_StdDevMatrices[keys[2]];

        //    double[,] avg2a = cs2.spectrogramMatrices[keys[0]];
        //    if (keys[0].Equals("TEN")) avg2a = MatrixTools.SubtractValuesFromOne(avg2a);
        //    double[,] avg2b = cs2.spectrogramMatrices[keys[1]];
        //    if (keys[1].Equals("TEN")) avg2b = MatrixTools.SubtractValuesFromOne(avg2b);
        //    double[,] avg2c = cs2.spectrogramMatrices[keys[2]];
        //    if (keys[2].Equals("TEN")) avg2c = MatrixTools.SubtractValuesFromOne(avg2c);

        //    double[,] std2a = cs2.spgr_StdDevMatrices[keys[0]];
        //    double[,] std2b = cs2.spgr_StdDevMatrices[keys[1]];
        //    double[,] std2c = cs2.spgr_StdDevMatrices[keys[2]];

        //    var dict = new Dictionary<string, double[,]>();
        //    dict.Add(keys[0], ColourSpectrogram.GetTStatisticMatrix(avg1a, std1a, cs1.N, avg2a, std2a, cs2.N));
        //    dict.Add(keys[1], ColourSpectrogram.GetTStatisticMatrix(avg1b, std1b, cs1.N, avg2b, std2b, cs2.N));
        //    dict.Add(keys[2], ColourSpectrogram.GetTStatisticMatrix(avg1c, std1c, cs1.N, avg2c, std2c, cs2.N));
        //    return dict;
        //}




        public static Image DrawTStatisticSpectrogramsOfSingleIndex(string key, LDSpectrogramRGB cs1, LDSpectrogramRGB cs2, double tStatThreshold)
        {
            Image image1 = cs1.DrawGreyscaleSpectrogramOfIndex(key);
            Image image2 = cs2.DrawGreyscaleSpectrogramOfIndex(key);

            if ((image1 == null) || (image2 == null))
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawTStatisticGreyscaleSpectrogramOfIndex()");
                Console.WriteLine("         Null image returned with key: {0}", key);
                return null;
                //Console.WriteLine("  Press <RETURN> to exit.");
                //Console.ReadLine();
                //System.Environment.Exit(666);
            }


            int titleHt = 20;
            int minOffset = 0;
            //frame image 1
            string title = String.Format("{0} SPECTROGRAM for: {1}.      (scale:hours x kHz)", key, cs1.FileName);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image1.Width, titleHt);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minOffset, cs1.X_interval, cs1.Y_interval);

            //frame image 2
            title = String.Format("{0} SPECTROGRAM for: {1}.      (scale:hours x kHz)", key, cs2.FileName);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image2.Width, titleHt);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            //get matrices required to calculate matrix of t-statistics
            double[,] avg1 = cs1.GetSpectrogramMatrix(key);
            if (key.Equals("TEN")) avg1 = MatrixTools.SubtractValuesFromOne(avg1);
            double[,] std1 = cs1.GetStandarDeviationMatrix(key);
            double[,] avg2 = cs2.GetSpectrogramMatrix(key);
            if (key.Equals("TEN")) avg2 = MatrixTools.SubtractValuesFromOne(avg2);
            double[,] std2 = cs2.GetStandarDeviationMatrix(key);

            //draw a spectrogram of t-statistic values
            //double[,] tStatMatrix = SpectrogramDifference.GetTStatisticMatrix(avg1, std1, cs1.SampleCount, avg2, std2, cs2.SampleCount);
            //Image image3 = SpectrogramDifference.DrawTStatisticSpectrogram(tStatMatrix);
            //titleBar = SpectrogramDifference.DrawTitleBarOfTStatisticSpectrogram(cs1.FileName, cs2.FileName, image1.Width, titleHt);
            //image3 = ColourSpectrogram.FrameSpectrogram(image3, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            //draw a difference spectrogram derived from by thresholding a t-statistic matrix 
            double colourGain = 2.5;
            Image image4 = LDSpectrogramDifference.DrawDifferenceSpectrogramDerivedFromSingleTStatistic(key, cs1, cs2, tStatThreshold, colourGain);
            title = String.Format("{0} DIFFERENCE SPECTROGRAM for: {1} - {2}.      (scale:hours x kHz)", key, cs1.FileName, cs2.FileName);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image2.Width, titleHt);
            image4 = LDSpectrogramRGB.FrameSpectrogram(image4, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            Image[] opArray = new Image[3];
            opArray[0] = image1;
            opArray[1] = image2;
            opArray[2] = image4;
            //opArray[3] = image4;

            Image combinedImage = ImageTools.CombineImagesVertically(opArray);
            return combinedImage;
        }



        /// <summary>
        /// double tStatThreshold = 1.645; // 0.05% confidence @ df=infinity
        /// double[] table_df_inf = { 0.25, 0.51, 0.67, 0.85, 1.05, 1.282, 1.645, 1.96, 2.326, 2.576, 3.09, 3.291 };
        /// double[] table_df_15 =  { 0.26, 0.53, 0.69, 0.87, 1.07, 1.341, 1.753, 2.13, 2.602, 2.947, 3.73, 4.073 };
        /// double[] alpha =        { 0.40, 0.30, 0.25, 0.20, 0.15, 0.10,  0.05,  0.025, 0.01, 0.005, 0.001, 0.0005 };
        /// </summary>
        /// <param name="tStatMatrix"></param>
        /// <returns></returns>
        public static Image DrawTStatisticSpectrogram(double[,] tStatMatrix)
        {
            double maxTStat = 20.0;
            double halfTStat = maxTStat / 2.0;
            double qtrTStat = maxTStat / 4.0;
            double tStat;

            int rows = tStatMatrix.GetLength(0); //number of rows
            int cols = tStatMatrix.GetLength(1); //number
            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    //catch low values of dB used to avoid log of zero amplitude.
                    tStat = tStatMatrix[row, col];
                    double tStatAbsolute = Math.Abs(tStat);
                    Dictionary<string, Color> colourChart = LDSpectrogramDifference.GetDifferenceColourChart();
                    Color colour;

                    if (tStat >= 0)
                    {
                        if (tStatAbsolute > maxTStat) { colour = colourChart["+99.9%"]; } //99.9% conf
                        else
                        {
                            if (tStatAbsolute > halfTStat) { colour = colourChart["+99.0%"]; } //99.0% conf
                            else
                            {
                                if (tStatAbsolute > qtrTStat) { colour = colourChart["+95.0%"]; } //95% conf
                                else
                                {
                                    //if (tStatAbsolute < fifthTStat) { colour = colourChart["NoValue"]; }
                                    //else
                                    //{
                                    colour = colourChart["NoValue"];
                                    //}
                                }
                            }
                        }  // if() else
                        bmp.SetPixel(col, row, colour);
                    }
                    else //  if (tStat < 0)
                    {
                        if (tStatAbsolute > maxTStat) { colour = colourChart["-99.9%"]; } //99.9% conf
                        else
                        {
                            if (tStatAbsolute > halfTStat) { colour = colourChart["-99.0%"]; } //99.0% conf
                            else
                            {
                                if (tStatAbsolute > qtrTStat) { colour = colourChart["-95.0%"]; } //95% conf
                                else
                                {
                                    //if (tStatAbsolute < 0.0) { colour = colourChart["NoValue"]; }
                                    //else
                                    //{
                                    colour = colourChart["NoValue"];
                                    //colour = colourChart["-NotSig"];
                                    //}
                                }
                            }
                        }  // if() else
                        bmp.SetPixel(col, row, colour);
                    }

                }//end all columns
            }//end all rows
            return bmp;
        }

        public static Image DrawDifferenceSpectrogramDerivedFromSingleTStatistic(string key, LDSpectrogramRGB cs1, LDSpectrogramRGB cs2, double tStatThreshold, double colourGain)
        {
            double[,] m1 = cs1.GetNormalisedSpectrogramMatrix(key); //the TEN matrix is subtracted from 1.
            double[,] m2 = cs2.GetNormalisedSpectrogramMatrix(key);
            double[,] tStatM = LDSpectrogramDifference.GetTStatisticMatrix(key, cs1, cs2);
            return LDSpectrogramDifference.DrawDifferenceSpectrogramDerivedFromSingleTStatistic(key, m1, m2, tStatM, tStatThreshold, colourGain);

        }

        public static Image DrawDifferenceSpectrogramDerivedFromSingleTStatistic(string key, double[,] m1, double[,] m2, double[,] tStatM, double tStatThreshold, double colourGain)
        {
            int rows = m1.GetLength(0); //number of rows
            int cols = m2.GetLength(1); //number

            Bitmap image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            int MaxRGBValue = 255;
            double diff;
            int ipos, ineg, value;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    if (Math.Abs(tStatM[row, column]) >= tStatThreshold)
                        diff = (m1[row, column] - m2[row, column]) * colourGain;
                    else diff = 0;
                    value = Math.Abs(Convert.ToInt32(diff * MaxRGBValue));
                    value = Math.Max(0, value);
                    value = Math.Min(MaxRGBValue, value);
                    ipos = 0;
                    ineg = 0;

                    if (diff >= 0)
                    {
                        ipos = value;
                        image.SetPixel(column, row, Color.FromArgb(ipos, 0, 0));
                    }
                    else
                    {
                        ineg = value;
                        image.SetPixel(column, row, Color.FromArgb(0, ineg, 0));
                    }


                }//end all columns
            }//end all rows
            return image;
        }

        public static double[,] GetDifferenceSpectrogramDerivedFromSingleTStatistic(string key, LDSpectrogramRGB cs1, LDSpectrogramRGB cs2, double tStatThreshold, double colourGain)
        {
            double[,] m1 = cs1.GetNormalisedSpectrogramMatrix(key); //the TEN matrix is subtracted from 1.
            double[,] m2 = cs2.GetNormalisedSpectrogramMatrix(key);
            double[,] tStatM = LDSpectrogramDifference.GetTStatisticMatrix(key, cs1, cs2);
            int rows = m1.GetLength(0); //number of rows
            int cols = m2.GetLength(1); //number

            var differenceM = new double[rows, cols];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    if (Math.Abs(tStatM[row, column]) >= tStatThreshold)
                        differenceM[row, column] = m1[row, column] - m2[row, column];
                }//end all columns
            }//end all rows
            return differenceM;
        }

        public static Image DrawTStatisticSpectrogramsOfMultipleIndices(LDSpectrogramRGB cs1, LDSpectrogramRGB cs2, double tStatThreshold, double colourGain)
        {
            string[] keys = cs1.ColorMap.Split('-'); //assume both spectorgrams have the same acoustic indices in same order

            double[,] m1 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[0], cs1, cs2, tStatThreshold, colourGain);
            double[,] m2 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[1], cs1, cs2, tStatThreshold, colourGain);
            double[,] m3 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[2], cs1, cs2, tStatThreshold, colourGain);

            int rows = m1.GetLength(0); //number of rows
            int cols = m1.GetLength(1); //number
            Bitmap bmp1 = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            Bitmap bmp2 = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            int MaxRGBValue = 255;
            double d1, d2, d3;
            int i1pos, i2pos, i3pos, value;
            int i1neg, i2neg, i3neg;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    d1 = m1[row, column] * colourGain;
                    d2 = m2[row, column] * colourGain;
                    d3 = m3[row, column] * colourGain;
                    i1pos = 0;
                    i2pos = 0;
                    i3pos = 0;
                    i1neg = 0;
                    i2neg = 0;
                    i3neg = 0;

                    value = Math.Abs(Convert.ToInt32(d1 * MaxRGBValue));
                    if (d1 >= 0)
                    {
                        i1pos = Math.Max(0, value);
                        i1pos = Math.Min(MaxRGBValue, i1pos);
                    }
                    else
                    {
                        i1neg = Math.Max(0, value);
                        i1neg = Math.Min(MaxRGBValue, i1neg);
                    }

                    value = Math.Abs(Convert.ToInt32(d2 * MaxRGBValue));
                    if (d2 >= 0)
                    {
                        i2pos = Math.Max(0, value);
                        i2pos = Math.Min(MaxRGBValue, i2pos);
                    }
                    else
                    {
                        i2neg = Math.Min(0, value);
                        i2neg = Math.Max(MaxRGBValue, i2neg);
                    }

                    value = Math.Abs(Convert.ToInt32(d3 * MaxRGBValue));
                    if (d3 >= 0)
                    {
                        i3pos = Math.Max(0, value);
                        i3pos = Math.Min(MaxRGBValue, i3pos);
                    }
                    else
                    {
                        i3neg = Math.Min(0, value);
                        i3neg = Math.Max(MaxRGBValue, i3neg);
                    }
                    bmp1.SetPixel(column, row, Color.FromArgb(i1pos, i2pos, i3pos));
                    bmp2.SetPixel(column, row, Color.FromArgb(i1neg, i2neg, i3neg));
                }//end all columns
            }//end all rows

            int titleHt = 20;
            string title = String.Format("t-STATISTIC DIFFERENCE SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", cs1.FileName, cs2.FileName, cs1.ColorMODE);
            Color[] colorArray = LDSpectrogramRGB.ColourChart2Array(LDSpectrogramDifference.GetDifferenceColourChart());
            Image titleBar = LDSpectrogramDifference.DrawTitleBarOfDifferenceSpectrogram(cs1.FileName, cs2.FileName, colorArray, bmp1.Width, titleHt);

            int minOffset = 0;
            Image[] array = new Image[2];
            array[0] = LDSpectrogramRGB.FrameSpectrogram(bmp1, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);;
            array[1] = LDSpectrogramRGB.FrameSpectrogram(bmp2, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);;

            Image compositeImage = ImageTools.CombineImagesVertically(array);
            return compositeImage;
        }

        //NOTE: 12th MARCH 2014:
        // BELOW METHOD WAS USED TO DRAW SPECTROGRAM OF T-STATISTIC VALUES DERIVED FROM AVERAGING OF THE T-STATISTICS FOR THREE INDICES
        // KEEP JUST IN ASE IT IS USEFUL LATER.
        //public static Image DrawTStatisticSpectrogram(ColourSpectrogram cs1, ColourSpectrogram cs2)
        //{
        //    string[] keys = cs1.ColorMap.Split('-'); //assume both spectorgrams have the same acoustic indices in same order

        //    double[,] tStatA = SpectrogramDifference.GetTStatisticMatrix(keys[0], cs1, cs2);
        //    double[,] tStatB = SpectrogramDifference.GetTStatisticMatrix(keys[1], cs1, cs2);
        //    double[,] tStatC = SpectrogramDifference.GetTStatisticMatrix(keys[2], cs1, cs2);

        //    // assume all matricies are normalised and of the same dimensions
        //    int rows = tStatA.GetLength(0); //number of rows
        //    int cols = tStatA.GetLength(1); //number

        //    Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

        //    double maxTStat = 10.0;
        //    double halfTStat = maxTStat / 1.5;
        //    double qtrTStat = maxTStat / 2.0;
        //    double tStat;
        //    //int i1, i2, i3;
        //    //double av1a, av1b, av1c, av2a, av2b, av2c;
        //    //double sd1a, sd1b, sd1c, sd2a, sd2b, sd2c;
        //    double tA, tB, tC;

        //    for (int row = 0; row < rows; row++)
        //    {
        //        for (int col = 0; col < cols; col++)
        //        {
        //            //catch low values of dB used to avoid log of zero amplitude.
        //            tA = tStatA[row, col];
        //            tB = tStatB[row, col];
        //            tC = tStatC[row, col];

        //            tStat = (tA + tB + tC) / 3.000;
        //            double tStatAbsolute = Math.Abs(tStat);
        //            Dictionary<string, Color> colourChart = SpectrogramDifference.GetDifferenceColourChart();
        //            Color colour;

        //            if (tStat >= 0)
        //            {
        //                if (tStatAbsolute > maxTStat) { colour = colourChart["+99.9%"]; } //99.9% conf
        //                else
        //                {
        //                    if (tStatAbsolute > halfTStat) { colour = colourChart["+99.0%"]; } //99.0% conf
        //                    else
        //                    {
        //                        if (tStatAbsolute > qtrTStat) { colour = colourChart["+95.0%"]; } //95% conf
        //                        else
        //                        {
        //                            if (tStatAbsolute < 0.0) { colour = colourChart["NoValue"]; }
        //                            else
        //                            {
        //                                colour = colourChart["+NotSig"];
        //                            }
        //                        }
        //                    }
        //                }  // if() else
        //                bmp.SetPixel(col, row, colour);
        //            }
        //            else //  if (tStat < 0)
        //            {
        //                if (tStatAbsolute > maxTStat) { colour = colourChart["-99.9%"]; } //99.9% conf
        //                else
        //                {
        //                    if (tStatAbsolute > halfTStat) { colour = colourChart["-99.0%"]; } //99.0% conf
        //                    else
        //                    {
        //                        if (tStatAbsolute > qtrTStat) { colour = colourChart["-95.0%"]; } //95% conf
        //                        else
        //                        {
        //                            if (tStatAbsolute < 0.0) { colour = colourChart["NoValue"]; }
        //                            else
        //                            {
        //                                //v = Convert.ToInt32(zScore * MaxRGBValue);
        //                                //if()
        //                                //colour = Color.FromArgb(0, v, v);
        //                                colour = colourChart["-NotSig"];
        //                            }
        //                        }
        //                    }
        //                }  // if() else
        //                bmp.SetPixel(col, row, colour);
        //            }

        //        }//end all columns
        //    }//end all rows
        //    return bmp;
        //}


        public static Image DrawDifferenceSpectrogram(LDSpectrogramRGB target, LDSpectrogramRGB reference, double colourGain)
        {
            string[] keys = target.ColorMap.Split('-');
            double[,] tgtRedM = target.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] tgtGrnM = target.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] tgtBluM = target.GetNormalisedSpectrogramMatrix(keys[2]);

            double[,] refRedM = reference.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] refGrnM = reference.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] refBluM = reference.GetNormalisedSpectrogramMatrix(keys[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = tgtRedM.GetLength(0); //number of rows
            int cols = tgtRedM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            double d1, d2, d3;
            int i1, i2, i3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    d1 = (tgtRedM[row, column] - refRedM[row, column]) * colourGain;
                    d2 = (tgtGrnM[row, column] - refGrnM[row, column]) * colourGain;
                    d3 = (tgtBluM[row, column] - refBluM[row, column]) * colourGain;

                    //if (doReverseColour)
                    //{
                    //    d1 = 1 - d1;
                    //    d2 = 1 - d2;
                    //    d3 = 1 - d3;
                    //}

                    i1 = 127 - Convert.ToInt32(d1 * MaxRGBValue);
                    i1 = Math.Max(0, i1);
                    i1 = Math.Min(MaxRGBValue, i1);
                    i2 = 127 - Convert.ToInt32(d2 * MaxRGBValue);
                    i2 = Math.Max(0, i2);
                    i2 = Math.Min(MaxRGBValue, i2);
                    i3 = 127 - Convert.ToInt32(d3 * MaxRGBValue);
                    i3 = Math.Max(0, i3);
                    i3 = Math.Min(MaxRGBValue, i3);

                    Color colour = Color.FromArgb(i1, i2, i3);
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }



        public static Image DrawTitleBarOfTStatisticSpectrogram(string name1, string name2, int width, int height)
        {
            Dictionary<string, Color> chart = LDSpectrogramDifference.GetDifferenceColourChart();
            Image colourChart = ImageTools.DrawColourChart(width, height, LDSpectrogramRGB.ColourChart2Array(chart));

            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            string text = String.Format("T-STATISTIC SPECTROGRAM (scale:hours x kHz)");
            int X = 4;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 70);
            text = name1 + "  +99.9%conf";
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            X += colourChart.Width;
            text = "-99.9%conf   " + name2;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1); //distance to end of string


            text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }


        public static Image DrawTitleBarOfDifferenceSpectrogram(string name1, string name2, Color[] colorArray, int width, int height)
        {
            Image colourChart = ImageTools.DrawColourChart(width, height, colorArray);

            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            string text = String.Format("EUCLIDIAN DISTANCE SPECTROGRAM (scale:hours x kHz)");
            int X = 4;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 70);
            text = name1 + "  +99.9%conf";
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            X += colourChart.Width;
            text = "-99.9%conf   " + name2;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1); //distance to end of string


            text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }


        public static Image DrawTitleBarOfDifferenceSpectrogram(string name1, string name2, int width, int height)
        {
            Dictionary<string, Color> chart = LDSpectrogramDifference.GetDifferenceColourChart();
            Image colourChart = ImageTools.DrawColourChart(width, height, LDSpectrogramRGB.ColourChart2Array(chart));

            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            string text = String.Format("EUCLIDIAN DISTANCE SPECTROGRAM (scale:hours x kHz)");
            int X = 4;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 70);
            text = name1 + "  +99.9%conf";
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            X += colourChart.Width;
            text = "-99.9%conf   " + name2;
            g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            stringSize = g.MeasureString(text, stringFont);
            X += (stringSize.ToSize().Width + 1); //distance to end of string


            text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }



        
        public static Dictionary<string, Color> GetDifferenceColourChart()
        {
            Dictionary<string, Color> colourChart = new Dictionary<string, Color>();
            colourChart.Add("+99.9%", Color.FromArgb(255, 190, 20));
            colourChart.Add("+99.0%", Color.FromArgb(240, 50, 30)); //+99% conf
            colourChart.Add("+95.0%", Color.FromArgb(200, 30, 15)); //+95% conf
            colourChart.Add("+NotSig", Color.FromArgb(50, 5, 5));   //+ not significant
            colourChart.Add("NoValue", Color.Black);
            //no value
            colourChart.Add("-99.9%", Color.FromArgb(20, 255, 230));
            colourChart.Add("-99.0%", Color.FromArgb(30, 240, 50)); //+99% conf
            colourChart.Add("-95.0%", Color.FromArgb(15, 200, 30)); //+95% conf
            colourChart.Add("-NotSig", Color.FromArgb(10, 50, 20)); //+ not significant
            return colourChart;
        }


    } // class SpectrogramDifference
}
