﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TowseyLib;


namespace AudioAnalysisTools
{
    public static class LDSpectrogramDistance
    {
        // set the X and Y axis scales for the spectrograms 
        private static int minuteOffset = 0;  // assume recording starts at zero minute of day i.e. midnight
        private static int xScale = SpectrogramConstants.XAXIS_SCALE;    // assume one minute spectra and hourly time lines
        private static int sampleRate = 17640; // default value - after resampling
        private static int frameWidth = 512;   // default value - from which spectrogram was derived
        private static string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR; //CHANGE RGB mapping here.
        private static double backgroundFilterCoeff = 0.75; //must be value <=1.0
        private static double colourGain = 1.5;


        public static void DrawDistanceSpectrogram(dynamic configuration)
        {
            string ipdir = configuration.InputDirectory;
            string ipFileName1 = configuration.IndexFile1;
            string ipFileName2 = configuration.IndexFile2;
            string opdir = configuration.OutputDirectory;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            colorMap = (string)configuration.ColorMap;                // assigns indices to RGB
            backgroundFilterCoeff = (double)configuration.BackgroundFilterCoeff;   // must be value <=1.0
            colourGain = (double)configuration.ColourGain;            // determines colour saturation of the difference spectrogram

            // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
            minuteOffset = (Int32)configuration.MinuteOffset;      // default is recording starts at zero minute of day i.e. midnight
            xScale = (Int32)configuration.X_Scale;                 // default is one minute spectra and hourly time lines
            sampleRate = (Int32)configuration.SampleRate;          // default value - after resampling
            frameWidth = (Int32)configuration.FrameWidth;          // frame width from which spectrogram was derived. Assume no frame overlap.

            DrawDistanceSpectrogram(new DirectoryInfo(ipdir), new FileInfo(ipFileName1), new FileInfo(ipFileName2), new DirectoryInfo(opdir));
        }





        /// <summary>
        /// This method compares the acoustic indices derived from two different long duration recordings of the same length. 
        /// It takes as input any number of csv files of acoustic indices in spectrogram columns.
        /// Typically there will be at least three indices csv files for each of the original recordings to be compared.
        /// The method produces four spectrogram image files:
        /// 1) A negative false-colour spectrogram derived from the indices of recording 1.   
        /// 2) A negative false-colour spectrogram derived from the indices of recording 2.
        /// 3) A spectrogram of euclidean distances bewteen the two input files.
        /// 4) The above three spectrograms combined in one image.
        /// </summary>
        /// <param name="ipdir"></param>
        /// <param name="ipFileName1"></param>
        /// <param name="ipFileName2"></param>
        /// <param name="opdir"></param>
        public static void DrawDistanceSpectrogram(DirectoryInfo ipdir, FileInfo ipFileName1, FileInfo ipFileName2, DirectoryInfo opdir)
        {
            //PARAMETERS
            int titleHt = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;


                string opFileName1 = ipFileName1.Name;
                var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
                cs1.ColorMODE = colorMap;
                cs1.BackgroundFilter = backgroundFilterCoeff;
                cs1.ReadCSVFiles(ipdir, ipFileName1.Name, colorMap);
                //ColourSpectrogram.BlurSpectrogram(cs1);
                //cs1.DrawGreyScaleSpectrograms(opdir, opFileName1);
                cs1.DrawNegativeFalseColourSpectrograms(opdir, opFileName1);
                string imagePath = Path.Combine(opdir.FullName, opFileName1 + ".COLNEG.png");
                Image spg1Image = ImageTools.ReadImage2Bitmap(imagePath);
                if (spg1Image == null)
                {
                    Console.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                    return;
                }
                string title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName1, cs1.ColorMODE);
                Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg1Image.Width, titleHt);
                spg1Image = LDSpectrogramRGB.FrameSpectrogram(spg1Image, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);

                string opFileName2 = ipFileName2.Name;
                var cs2 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
                cs2.ColorMODE = colorMap;
                cs2.BackgroundFilter = backgroundFilterCoeff;
                cs2.ReadCSVFiles(ipdir, ipFileName2.Name, colorMap);
                //cs2.DrawGreyScaleSpectrograms(opdir, opFileName2);
                cs2.DrawNegativeFalseColourSpectrograms(opdir, opFileName2);
                imagePath = Path.Combine(opdir.FullName, opFileName2 + ".COLNEG.png");
                Image spg2Image = ImageTools.ReadImage2Bitmap(imagePath);
                if (spg2Image == null)
                {
                    Console.WriteLine("SPECTROGRAM IMAGE DOES NOT EXIST: {0}", imagePath);
                    return;
                }

                title = String.Format("FALSE COLOUR SPECTROGRAM: {0}.      (scale:hours x kHz)       (colour: R-G-B={1})", ipFileName2, cs2.ColorMODE);
                titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, spg2Image.Width, titleHt);
                spg2Image = LDSpectrogramRGB.FrameSpectrogram(spg2Image, titleBar, minuteOffset, cs2.X_interval, cs2.Y_interval);

                string opFileName4 = ipFileName1 + ".EuclidianDist.COLNEG.png";
                Image deltaSp = LDSpectrogramDistance.DrawDistanceSpectrogram(cs1, cs2);
                Color[] colorArray = LDSpectrogramRGB.ColourChart2Array(LDSpectrogramDifference.GetDifferenceColourChart());
                titleBar = LDSpectrogramDifference.DrawTitleBarOfDifferenceSpectrogram(ipFileName1.Name, ipFileName2.Name, colorArray, deltaSp.Width, titleHt);
                deltaSp = LDSpectrogramRGB.FrameSpectrogram(deltaSp, titleBar, minuteOffset, cs2.X_interval, cs2.Y_interval);
                deltaSp.Save(Path.Combine(opdir.FullName, opFileName4));

                string opFileName5 = ipFileName1 + ".THREEDist.COLNEG.png";
                Image[] images = new Image[3];
                images[0] = spg1Image;
                images[1] = spg2Image;
                images[2] = deltaSp;
                Image combinedImage = ImageTools.CombineImagesVertically(images);
                combinedImage.Save(Path.Combine(opdir.FullName, opFileName5));
         }





        public static Image DrawDistanceSpectrogram(LDSpectrogramRGB cs1, LDSpectrogramRGB cs2)
        {
            string[] keys = cs1.ColorMap.Split('-');

            string key = keys[0];
            double[,] m1Red = cs1.GetNormalisedSpectrogramMatrix(key);
            var dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m1Red);
            cs1.SetIndexStatistics(key, dict);
            m1Red = MatrixTools.Matrix2ZScores(m1Red, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[1];
            double[,] m1Grn = cs1.GetNormalisedSpectrogramMatrix(key);
            dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m1Grn);
            cs1.SetIndexStatistics(key, dict);
            m1Grn = MatrixTools.Matrix2ZScores(m1Grn, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[2];
            double[,] m1Blu = cs1.GetNormalisedSpectrogramMatrix(key);
            dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m1Blu);
            cs1.SetIndexStatistics(key, dict);
            m1Blu = MatrixTools.Matrix2ZScores(m1Blu, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[0];
            double[,] m2Red = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m2Red);
            cs2.SetIndexStatistics(key, dict);
            m2Red = MatrixTools.Matrix2ZScores(m2Red, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[1];
            double[,] m2Grn = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m2Grn);
            cs2.SetIndexStatistics(key, dict);
            m2Grn = MatrixTools.Matrix2ZScores(m2Grn, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[2];
            double[,] m2Blu = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = LDSpectrogramDistance.GetModeAndOneTailedStandardDeviation(m2Blu);
            cs2.SetIndexStatistics(key, dict);
            m2Blu = MatrixTools.Matrix2ZScores(m2Blu, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);


            double[] v1 = new double[3];
            double[] mode1 = { cs1.GetIndexStatistics(keys[0], "mode"), cs1.GetIndexStatistics(keys[1], "mode"), cs1.GetIndexStatistics(keys[2], "mode") };
            double[] stDv1 = { cs1.GetIndexStatistics(keys[0], "sd"),   cs1.GetIndexStatistics(keys[1], "sd"),   cs1.GetIndexStatistics(keys[2], "sd") };
            Console.WriteLine("1: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", mode1[0], stDv1[0], mode1[1], stDv1[1], mode1[2], stDv1[2]);

            double[] v2 = new double[3];
            double[] mode2 = { cs2.GetIndexStatistics(keys[0], "mode"), cs2.GetIndexStatistics(keys[1], "mode"), cs2.GetIndexStatistics(keys[2], "mode") };
            double[] stDv2 = { cs2.GetIndexStatistics(keys[0], "sd"),   cs2.GetIndexStatistics(keys[1], "sd"),   cs2.GetIndexStatistics(keys[2], "sd") };
            Console.WriteLine("2: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", mode2[0], stDv2[0], mode2[1], stDv2[1], mode2[2], stDv2[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = m1Red.GetLength(0); //number of rows
            int cols = m1Red.GetLength(1); //number
            double[,] d12Matrix = new double[rows, cols];
            double[,] d11Matrix = new double[rows, cols];
            double[,] d22Matrix = new double[rows, cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    v1[0] = m1Red[row, col];
                    v1[1] = m1Grn[row, col];
                    v1[2] = m1Blu[row, col];

                    v2[0] = m2Red[row, col];
                    v2[1] = m2Grn[row, col];
                    v2[2] = m2Blu[row, col];

                    d12Matrix[row, col] = DataTools.EuclidianDistance(v1, v2);
                    d11Matrix[row, col] = (v1[0] + v1[1] + v1[2]) / 3; //get average of the normalised values
                    d22Matrix[row, col] = (v2[0] + v2[1] + v2[2]) / 3;

                    //following lines are for debugging purposes
                    //if ((row == 150) && (col == 1100))
                    //{
                    //    Console.WriteLine("V1={0:f3}, {1:f3}, {2:f3}", v1[0], v1[1], v1[2]);
                    //    Console.WriteLine("V2={0:f3}, {1:f3}, {2:f3}", v2[0], v2[1], v2[2]);
                    //    Console.WriteLine("EDist12={0:f4};   ED11={1:f4};   ED22={2:f4}", d12Matrix[row, col], d11Matrix[row, col], d22Matrix[row, col]);
                    //}
                }
            } // rows



            double[] array = DataTools.Matrix2Array(d12Matrix);
            double avDist, sdDist;
            NormalDist.AverageAndSD(array, out avDist, out sdDist);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    d12Matrix[row, col] = (d12Matrix[row, col] - avDist) / sdDist;
                }
            }

            //int MaxRGBValue = 255;
            //int v;
            double zScore;
            Dictionary<string, Color> colourChart = LDSpectrogramDifference.GetDifferenceColourChart();
            Color colour;

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    zScore = d12Matrix[row, col];

                    if (d11Matrix[row, col] >= d22Matrix[row, col])
                    {
                        if (zScore > 3.08) { colour = colourChart["+99.9%"]; } //99.9% conf
                        else
                        {
                            if (zScore > 2.33) { colour = colourChart["+99.0%"]; } //99.0% conf
                            else
                            {
                                if (zScore > 1.65) { colour = colourChart["+95.0%"]; } //95% conf
                                else
                                {
                                    if (zScore < 0.0) { colour = colourChart["NoValue"]; }
                                    else
                                    {
                                        //v = Convert.ToInt32(zScore * MaxRGBValue);
                                        //colour = Color.FromArgb(v, 0, v);
                                        colour = colourChart["+NotSig"];
                                    }
                                }
                            }
                        }  // if() else
                        bmp.SetPixel(col, row, colour);
                    }
                    else
                    {
                        if (zScore > 3.08) { colour = colourChart["-99.9%"]; } //99.9% conf
                        else
                        {
                            if (zScore > 2.33) { colour = colourChart["-99.0%"]; } //99.0% conf
                            else
                            {
                                if (zScore > 1.65) { colour = colourChart["-95.0%"]; } //95% conf
                                else
                                {
                                    if (zScore < 0.0) { colour = colourChart["NoValue"]; }
                                    else
                                    {
                                        //v = Convert.ToInt32(zScore * MaxRGBValue);
                                        //if()
                                        //colour = Color.FromArgb(0, v, v);
                                        colour = colourChart["-NotSig"];
                                    }
                                }
                            }
                        }  // if() else
                        bmp.SetPixel(col, row, colour);
                    }

                } //all rows
            } //all rows

            return bmp;
        } // DrawDistanceSpectrogram()

        public static Dictionary<string, double> GetModeAndOneTailedStandardDeviation(double[,] M)
        {
            double[] values = DataTools.Matrix2Array(M);
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, out min, out max, out mode, out SD);
            //Console.WriteLine("{0}: Min={1:f3}   Max={2:f3}    Mode={3:f3}+/-{4:f3} (SD=One-tailed)", key, min, max, mode, SD);
            var dict = new Dictionary<string, double>();
            dict["min"] = min;
            dict["max"] = max;
            dict["mode"] = mode;
            dict["sd"] = SD;
            return dict;
        }
   
    
    } //    class SpectrogramDistance
}
