using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TowseyLibrary;


namespace AudioAnalysisTools
{
    public static class LDSpectrogramTStatistic
    {
        //PARAMETERS
        // set DEFAULT values for parameters
        private static TimeSpan minuteOffset = SpectrogramConstants.MINUTE_OFFSET;  // assume recording starts at zero minute of day i.e. midnight
        private static TimeSpan xScale      = SpectrogramConstants.X_AXIS_TIC_INTERVAL;         // assume one minute spectra and hourly time lines
        private static int sampleRate = SpectrogramConstants.SAMPLE_RATE;      // default value - after resampling
        private static int frameWidth = SpectrogramConstants.FRAME_WIDTH;      // default value - from which spectrogram was derived

        private static string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR; //CHANGE default RGB mapping here.
        private static double backgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF; //must be value <=1.0
        private static double colourGain = SpectrogramConstants.COLOUR_GAIN;



        //double[] table_df_inf = { 0.25, 0.51, 0.67, 0.85, 1.05, 1.282, 1.645, 1.96, 2.326, 2.576, 3.09, 3.291 };
        //double[] table_df_15 =  { 0.26, 0.53, 0.69, 0.87, 1.07, 1.341, 1.753, 2.13, 2.602, 2.947, 3.73, 4.073 };
        //double[] alpha =        { 0.40, 0.30, 0.25, 0.20, 0.15, 0.10,  0.05,  0.025, 0.01, 0.005, 0.001, 0.0005 };

        //public const double tStatThreshold = 1.645;   // 5% confidence @ df=infinity
        //public const double tStatThreshold = 2.326;   // 1% confidence @ df=infinity
        //public const double T_STAT_THRESHOLD = 3.09;  // 0.1% confidence @ df=infinity
        public const double T_STAT_THRESHOLD = 3.29;    // 0.05% confidence @ df=infinity
        private static double tStatThreshold = T_STAT_THRESHOLD;



        public static void DrawTStatisticThresholdedDifferenceSpectrograms(dynamic configuration)
        {
            string ipdir = configuration.InputDirectory;
            string ipFileName1 = configuration.IndexFile1;
            string ipSdFileName1 = configuration.StdDevFile1;
            string ipFileName2 = configuration.IndexFile2;
            string ipSdFileName2 = configuration.StdDevFile2;
            string opdir = configuration.OutputDirectory;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColorMap;
            colorMap = map != null ? map : SpectrogramConstants.RGBMap_ACI_ENT_CVR;           // assigns indices to RGB

            backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            minuteOffset = (TimeSpan?)configuration.MinuteOffset ?? SpectrogramConstants.MINUTE_OFFSET; // default = zero minute of day i.e. midnight
            xScale = (TimeSpan?)configuration.X_Scale ?? SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default is one minute spectra i.e. 60 per hour
            sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstants.SAMPLE_RATE;  
            frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstants.FRAME_WIDTH; 

            tStatThreshold = (double?)configuration.TStatThreshold ?? T_STAT_THRESHOLD;  
           
            DrawTStatisticThresholdedDifferenceSpectrograms(new DirectoryInfo(ipdir),
                                                            new FileInfo(ipFileName1), new FileInfo(ipSdFileName1),
                                                            new FileInfo(ipFileName2), new FileInfo(ipSdFileName2), 
                                                            new DirectoryInfo(opdir));
        }


        /// <summary>
        /// This method compares the acoustic indices derived from two different long duration recordings of the same length. 
        /// It takes as input six csv files of acoustic indices in spectrogram columns, three csv files for each of the original recordings to be compared.
        /// The method produces four spectrogram image files:
        /// 1) A triple image. Top:    The spectrogram for index 1, recording 1.
        ///                    Middle: The spectrogram for index 1, recording 2.
        ///                    Bottom: A t-statistic thresholded difference spectrogram for INDEX 1 (derived from recordings 1 and 2).   
        /// 2) A triple image. Top:    The spectrogram for index 2, recording 1.
        ///                    Middle: The spectrogram for index 2, recording 2.
        ///                    Bottom: A t-statistic thresholded difference spectrogram for INDEX 2 (derived from recordings 1 and 2).   
        /// 3) A triple image. Top:    The spectrogram for index 3, recording 1.
        ///                    Middle: The spectrogram for index 3, recording 2.
        ///                    Bottom: A t-statistic thresholded difference spectrogram for INDEX 3 (derived from recordings 1 and 2).   
        /// 4) A double image. Top:    A t-statistic thresholded difference spectrogram (t-statistic is positive).
        ///                    Bottom: A t-statistic thresholded difference spectrogram (t-statistic is negative).   
        /// </summary>
        /// <param name="ipdir"></param>
        /// <param name="ipFileName1"></param>
        /// <param name="ipSdFileName1"></param>
        /// <param name="ipFileName2"></param>
        /// <param name="ipSdFileName2"></param>
        /// <param name="opdir"></param>
        public static void DrawTStatisticThresholdedDifferenceSpectrograms(DirectoryInfo ipdir, FileInfo ipFileName1, FileInfo ipSdFileName1,
                                                                                                FileInfo ipFileName2, FileInfo ipSdFileName2, 
                                                                                                DirectoryInfo opdir)
        {
            string opFileName1 = ipFileName1.Name;
            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = opFileName1;
            cs1.ColorMODE = colorMap;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            string[] keys = colorMap.Split('-');
            cs1.ReadCSVFiles(ipdir, ipFileName1.Name, keys);
            string imagePath = Path.Combine(opdir.FullName, opFileName1 + ".COLNEG.png");

            string opFileName2 = ipFileName2.Name;
            var cs2 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs2.FileName = opFileName2;
            cs2.ColorMODE = colorMap;
            cs2.BackgroundFilter = backgroundFilterCoeff;
            cs2.ReadCSVFiles(ipdir, ipFileName2.Name, keys);

            bool allOK = true;
            int N = 30;

            allOK = cs1.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName1.Name);
            if (!allOK)
            {
                Console.WriteLine("Cannot do t-test comparison because error reading standard deviation file: {0}", ipSdFileName1.Name);
                return;
            }
            cs1.SampleCount = N;

            allOK = cs2.ReadStandardDeviationSpectrogramCSVs(ipdir, ipSdFileName2.Name);
            if (!allOK)
            {
                Console.WriteLine("Cannot do t-test comparison because error reading standard deviation file: {0}", ipSdFileName2.Name);
                return;
            }
            cs2.SampleCount = N;

            string key = "ACI";
            Image tStatIndexImage = LDSpectrogramTStatistic.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            string opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir.FullName, opFileName3));

            key = "TEN";
            tStatIndexImage = LDSpectrogramTStatistic.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir.FullName, opFileName3));

            key = "CVR";
            tStatIndexImage = LDSpectrogramTStatistic.DrawTStatisticSpectrogramsOfSingleIndex(key, cs1, cs2, tStatThreshold);
            opFileName3 = ipFileName1 + ".tTest." + key + ".png";
            tStatIndexImage.Save(Path.Combine(opdir.FullName, opFileName3));

            tStatIndexImage = LDSpectrogramTStatistic.DrawTStatisticSpectrogramsOfMultipleIndices(cs1, cs2, tStatThreshold, colourGain);
            opFileName3 = ipFileName1+"-"+ipFileName2 + ".Difference.tTestThreshold.png";
            tStatIndexImage.Save(Path.Combine(opdir.FullName, opFileName3));
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

            double[,] tStatMatrix = LDSpectrogramTStatistic.GetTStatisticMatrix(avg1, std1, cs1.SampleCount, avg2, std2, cs2.SampleCount);
            return tStatMatrix;
        }


        public static double[,] GetTStatisticMatrix(double[,] m1Av, double[,] m1Sd, int N1, double[,] m2Av, double[,] m2Sd, int N2)
        {
            int rows = m1Av.GetLength(0); //number of rows
            int cols = m1Av.GetLength(1); //number
            double avg1, avg2, std1, std2;
            double[,] M = new double[rows, cols];
            int expectedMinAvg = 0; // expected minimum average  of spectral dB above background
            int expectedMinVar = 0; // expected minimum variance of spectral dB above background

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


            //frame image 1
            string title = String.Format("{0} SPECTROGRAM for: {1}.      (scale:hours x kHz)", key, cs1.FileName);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);

            //frame image 2
            title = String.Format("{0} SPECTROGRAM for: {1}.      (scale:hours x kHz)", key, cs2.FileName);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs2.X_interval, cs2.Y_interval);

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
            Image image4 = LDSpectrogramTStatistic.DrawDifferenceSpectrogramDerivedFromSingleTStatistic(key, cs1, cs2, tStatThreshold, colourGain);
            title = String.Format("{0} DIFFERENCE SPECTROGRAM (thresholded by t-statistic={3}) for: {1} - {2}.      (scale:hours x kHz)", key, cs1.FileName, cs2.FileName, tStatThreshold);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image2.Width);
            image4 = LDSpectrogramRGB.FrameSpectrogram(image4, titleBar, minuteOffset, cs2.X_interval, cs2.Y_interval);

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
                    Dictionary<string, Color> colourChart = LDSpectrogramDistance.GetDifferenceColourChart();
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
            double[,] tStatM = LDSpectrogramTStatistic.GetTStatisticMatrix(key, cs1, cs2);
            return LDSpectrogramTStatistic.DrawDifferenceSpectrogramDerivedFromSingleTStatistic(key, m1, m2, tStatM, tStatThreshold, colourGain);

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

        public static double[,] GetDifferenceSpectrogramDerivedFromSingleTStatistic(string key, LDSpectrogramRGB cs1, LDSpectrogramRGB cs2, double tStatThreshold)
        {
            double[,] m1 = cs1.GetNormalisedSpectrogramMatrix(key); //the TEN matrix is subtracted from 1.
            double[,] m2 = cs2.GetNormalisedSpectrogramMatrix(key);
            double[,] tStatM = LDSpectrogramTStatistic.GetTStatisticMatrix(key, cs1, cs2);
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

            double[,] m1 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[0], cs1, cs2, tStatThreshold);
            double[,] m2 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[1], cs1, cs2, tStatThreshold);
            double[,] m3 = GetDifferenceSpectrogramDerivedFromSingleTStatistic(keys[2], cs1, cs2, tStatThreshold);

            int rows = m1.GetLength(0); //number of rows
            int cols = m1.GetLength(1); //number

            Bitmap spg1Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            Bitmap spg2Image = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            double dR, dG, dB;
            int iR1, iG1, iB1, value;
            int iR2, iG2, iB2;
            Color colour1, colour2;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    dR = m1[row, column] * colourGain;
                    dG = m2[row, column] * colourGain;
                    dB = m3[row, column] * colourGain;

                    iR1 = 0; iR2 = 0; iG1 = 0; iG2 = 0; iB1 = 0; iB2 = 0;

                    value = Convert.ToInt32(Math.Abs(dR) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dR > 0.0) { iR1 = value; }
                    else { iR2 = value; }

                    value = Convert.ToInt32(Math.Abs(dG) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dG > 0.0) { iG1 = value; }
                    else { iG2 = value; }

                    value = Convert.ToInt32(Math.Abs(dB) * MaxRGBValue);
                    value = Math.Min(MaxRGBValue, value);
                    if (dB > 0.0) { iB1 = value; }
                    else { iB2 = value; }

                    colour1 = Color.FromArgb(iR1, iG1, iB1);
                    colour2 = Color.FromArgb(iR2, iG2, iB2);
                    spg1Image.SetPixel(column, row, colour1);
                    spg2Image.SetPixel(column, row, colour2);
                }//end all columns
            }//end all rows

            Image[] images = new Image[2];

            string title = String.Format("DIFFERENCE SPECTROGRAM (thresholded by t-Statistic={2}) where {0} > {1}      (scale:hours x kHz)       (colour: R-G-B={2})", cs1.FileName, cs2.FileName, tStatThreshold);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg1Image.Width);
            images[0] = LDSpectrogramRGB.FrameSpectrogram(spg1Image, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval); ;
            title = String.Format("DIFFERENCE SPECTROGRAM (thresholded by t-Statistic={2}) where {1} > {0}      (scale:hours x kHz)       (colour: R-G-B={2})", cs1.FileName, cs2.FileName, tStatThreshold);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg2Image.Width);
            images[1] = LDSpectrogramRGB.FrameSpectrogram(spg2Image, titleBar, minuteOffset, cs2.X_interval, cs2.Y_interval); ;

            Image compositeImage = ImageTools.CombineImagesVertically(images);
            return compositeImage;
        }



    } // class LDSpectrogramTStatistic
}
