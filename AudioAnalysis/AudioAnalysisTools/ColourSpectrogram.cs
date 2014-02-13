﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

//using Acoustics.Shared;
//using Acoustics.Tools;
//using Acoustics.Tools.Audio;

//using AnalysisBase;
//using log4net;

using TowseyLib;





namespace AudioAnalysisTools
{
    public class ColourSpectrogram
    {

        //Define new colour schemes in class SpectorgramConstants and implement the code in the class Colourspectrogram, 
        //            method DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        public string colorSchemeID = SpectrogramConstants.RGBMap_DEFAULT; //R-G-B

        //private static readonly ILog Logger =
        //    LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int x_interval = 60;    // assume one minute spectra and hourly time lines
        public int X_interval
        {
            get { return x_interval; }
            set { x_interval = value; }
        }
        private int frameWidth = 512;   // default value - from which spectrogram was derived
        public int FrameWidth           // used only to calculate scale of Y-axis to draw grid lines
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }
        private int sampleRate = 17640; // default value - after resampling
        public int SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }

        public int Y_interval // mark 1 kHz intervals
        {
            get
            {
                double freqBinWidth = sampleRate / (double)frameWidth;
                return (int)Math.Round(1000 / freqBinWidth);
            }
        }

        public string ColorMap { get; set; }    //within current recording     
        public string ColorMODE { get; set; }   //POSITIVE or NEGATIVE     

        Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 


        public ColourSpectrogram(int Xscale, int sampleRate, string colourMap)
        {
            // set the X and Y axis scales for the spectrograms 
            this.X_interval = Xscale; 
            this.SampleRate = sampleRate; 
            this.ColorMap = colourMap; 
        }

        public void ReadCSVFiles(string ipdir, string fileName)
        {
            string key = SpectrogramConstants.KEY_BackgroundNoise;
            string path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

            key = SpectrogramConstants.KEY_AcousticComplexityIndex;
            path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

            key = SpectrogramConstants.KEY_TemporalEntropy;
            path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

            key = SpectrogramConstants.KEY_BinCover;
            path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

            key = SpectrogramConstants.KEY_Average;
            path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

            key = SpectrogramConstants.KEY_Variance;
            path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);

        }

        public void ReadSpectrogram(string key, string csvPath)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);
            int binCount = matrix.GetLength(1) - 1; // -1 because first bin is the index numbers 
            // calculate the window/frame that was used to generate the spectra. This value is only used to place grid lines on the final images
            this.FrameWidth = binCount * 2;

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            spectrogramMatrices.Add(key, matrix);
        }

        /// <summary>
        /// All matrices must be in spectrogram orientation before adding to list of spectrograms.
        /// Call this method if the matrix rows are freq bins and the matrix columns are spectra for one frame.
        /// If not, then call the next method AddRotatedSpectrogram(string key, double[,] matrix)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="matrix"></param>
        public void AddSpectrogram(string key, double[,] matrix)
        {
            spectrogramMatrices.Add(key, matrix);
        }
        public void AddRotatedSpectrogram(string key, double[,] matrix)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            spectrogramMatrices.Add(key, matrix);
        }

        public void DrawGreyScaleSpectrograms(string opdir, string opFileName)
        {
            string path = Path.Combine(opdir, opFileName + ".ACI.png");
            Image bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_AcousticComplexityIndex);
            bmp.Save(path);

            path = Path.Combine(opdir, opFileName + ".AVG.png");
            bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_Average);
            bmp.Save(path);

            path = Path.Combine(opdir, opFileName + ".CVR.png");
            bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BinCover);
            bmp.Save(path);

            path = Path.Combine(opdir, opFileName + ".TEN.png");
            bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_TemporalEntropy);
            bmp.Save(path);

            path = Path.Combine(opdir, opFileName + ".VAR.png");
            bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_Variance);
            bmp.Save(path);

            path = Path.Combine(opdir, opFileName + ".CMB.png");
            bmp = bmp = this.DrawCombinedAverageSpectrogram();
            bmp.Save(path);

            // must return the background image because it will be used elsewhere
            path = Path.Combine(opdir, opFileName + ".BGN.png");
            bmp = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BackgroundNoise);
            bmp.Save(path);
        }

        public Image DrawGreyscaleSpectrogramOfIndex(string key)
        {
            double[,] matrix = ColourSpectrogram.NormaliseSpectrogramMatrix(key, this.spectrogramMatrices[key]);
            Image bmp = ImageTools.DrawMatrix(matrix);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, this.Y_interval);
            return bmp;
        }

        public void DrawFalseColourSpectrograms(string opdir, string opFileName)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEG.png"));

            Image bmpPos = this.DrawFalseColourSpectrogram("POSITIVE");
            bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOS.png"));

            Image bmpBgn = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BackgroundNoise);

            bmpNeg = this.DrawDoubleSpectrogram(bmpNeg, bmpBgn, "NEGATIVE");
            bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEGBGN.png"));

            bmpPos = this.DrawDoubleSpectrogram(bmpPos, bmpBgn, "POSITIVE");
            bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOSBGN.png"));
        }

        /// <summary>
        /// Calculates a COMBO spectrogram from four equal weighted normalised indices.
        /// </summary>
        /// <param name="imagePath"></param>
        public Image DrawCombinedAverageSpectrogram()
        {
            var avgMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_Average, spectrogramMatrices[SpectrogramConstants.KEY_Average]);
            var cvrMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_BinCover, spectrogramMatrices[SpectrogramConstants.KEY_BinCover]);
            var aciMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_AcousticComplexityIndex, spectrogramMatrices[SpectrogramConstants.KEY_AcousticComplexityIndex]);
            var tenMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_TemporalEntropy, spectrogramMatrices[SpectrogramConstants.KEY_TemporalEntropy]);

            // assume all matrices have same rows and columns
            int rows = avgMatrix.GetLength(0);
            int cols = avgMatrix.GetLength(1);
            var combo = new double[rows, cols];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // comboSpectrum[r, c] = (cover + aci + entropy + avg) / (double)4;
                    double value = avgMatrix[r, c] + cvrMatrix[r, c] + aciMatrix[r, c] + (1 - tenMatrix[r, c]); // reverse temporal entropy
                    combo[r, c] = value / (double)4;
                }
            }

            Image bmp = ImageTools.DrawMatrix(combo);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, Y_interval);
            return bmp;
        }

        public Image DrawFalseColourSpectrogram(string colorMODE)
        {
            string[] rgbMap = this.ColorMap.Split('-');

            var redMatrix = NormaliseSpectrogramMatrix(rgbMap[0], spectrogramMatrices[rgbMap[0]]);
            var grnMatrix = NormaliseSpectrogramMatrix(rgbMap[1], spectrogramMatrices[rgbMap[1]]);
            var bluMatrix = NormaliseSpectrogramMatrix(rgbMap[2], spectrogramMatrices[rgbMap[2]]);
            string bgnKey = SpectrogramConstants.KEY_BackgroundNoise;
            var bgnMatrix = NormaliseSpectrogramMatrix(bgnKey, spectrogramMatrices[bgnKey]);
            //var bgnMatrix = spectrogramMatrices[SpectrogramConstants.KEY_BackgroundNoise];
            //bgnMatrix = DataTools.NormaliseInZeroOne(bgnMatrix, SpectrogramConstants.BGN_MIN, SpectrogramConstants.BGN_MAX);

            bool doReverseColour = false;
            if (colorMODE.StartsWith("POS")) doReverseColour = true;

            Image bmp = ColourSpectrogram.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, Y_interval);
            return bmp;
        }

        public Image DrawDoubleSpectrogram(Image bmp1, Image bmp2, string colorMODE)
        {
            int imageWidth = bmp2.Width;
            int trackHeight = 20;
            int imageHt = bmp2.Height + bmp1.Height + trackHeight + trackHeight + trackHeight;
            string title = String.Format("FALSE COLOUR and BACKGROUND NOISE SPECTROGRAMS      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", this.ColorMap);
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            int timeScale = 60;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, timeScale, imageWidth, trackHeight, "hours");

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
            offset += titleBmp.Height;
            gr.DrawImage(bmp1, 0, offset); //dra
            offset += bmp1.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra
            offset += timeBmp.Height;
            gr.DrawImage(bmp2, 0, offset); //dr
            offset += bmp2.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra

            //draw a colour spectrum of basic colours
            int maxScaleLength = imageWidth / 3;
            Image scale = ColourSpectrogram.DrawColourScale(maxScaleLength, trackHeight - 2);
            int xLocation = imageWidth * 2 / 3;
            gr.DrawImage(scale, xLocation, 1); //dra
            return compositeBmp;
        }





        //========================================================================================================================================================
        //========= DEV AND EXECUTE STATIC METHODS BELOW HERE ====================================================================================================================
        //========================================================================================================================================================

        public class Arguments
        {
        }

        public static void Dev(Arguments arguments)
        {
            bool executeDev = (arguments == null);
            if (executeDev)
            {

                // INPUT CSV FILES
                //string ipdir = @"C:\SensorNetworks\Output\SERF\AfterRefactoring\Towsey.Acoustic";// SERF 13th October 2010
                //string ipdir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM20036.Towsey.Acoustic"; // SUNSHINE COAST 13th October 2011 DM420036.MP3
                //string ipdir = @"C:\SensorNetworks\Output\SERF\2013Sept15th_MergedCSVs"; // SERF
                //string ipdir = @"C:\SensorNetworks\Output\SERF\2013August30th_MergedCSVs"; // SERF
                string ipdir = @"C:\SensorNetworks\Output\SERF\AfterRefactoring\Towsey.Acoustic"; // SERF

                string ipFileName = @"7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
                //string ipFileName = "DM420233_20120302_000000";
                //string ipFileName = "SERF_20130915_Merged";
                //string ipFileName = "SERF_20130730_Merged";

                // OUTPUT CSV FILES
                //string opdir = @"C:\SensorNetworks\Output\SERF\AfterRefactoring\Towsey.Acoustic";  //SERF
                //string opdir = @"Z:\Results\2013Dec22-220529 - SERF VEG 2011\SERF\VEG\DM420233_20120302_000000.MP3\Towsey.Acoustic"; // SERF
                //string opdir = @"C:\SensorNetworks\Output\SERF\2014Jan30";
                //string opdir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM20036.Towsey.Acoustic"; // SUNSHINE COAST
                string opdir = @"C:\SensorNetworks\Output\temp";
                string opFileName = ipFileName + ".Test1";


                // set the X and Y axis scales for the spectrograms 
                int xScale = 60;  // assume one minute spectra and hourly time lines
                int sampleRate = 17640; // default value - after resampling
                string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_BGN; //CHANGE RGB mapping here.
                var cs = new ColourSpectrogram(xScale, sampleRate, colorMap);
                cs.ReadCSVFiles(ipdir, ipFileName);
                cs.DrawGreyScaleSpectrograms(opdir, opFileName);
                cs.DrawFalseColourSpectrograms(opdir, opFileName);
            }

            Execute(arguments);

            if (executeDev)
            {

            }
        }

        public static void Execute(Arguments arguments)
        {
            // doesn't do anything for now
        }



        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################

        public static double[,] NormaliseSpectrogramMatrix(string key, double[,] matrix)
        {
            if (key == SpectrogramConstants.KEY_AcousticComplexityIndex) //.Equals("ACI"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.ACI_MIN, SpectrogramConstants.ACI_MAX);
            }
            else if (key == SpectrogramConstants.KEY_TemporalEntropy)//.Equals("TEN"))
            {
                // normalise and reverse
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.TEN_MIN, SpectrogramConstants.TEN_MAX);
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount; c++)
                    {
                        matrix[r, c] = 1 - matrix[r, c];
                    }
                }
            }
            else if (key == SpectrogramConstants.KEY_Average)//.Equals("AVG"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.AVG_MIN, SpectrogramConstants.AVG_MAX);
            }
            else if (key == SpectrogramConstants.KEY_BackgroundNoise)//.Equals("BGN"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.BGN_MIN, SpectrogramConstants.BGN_MAX);
            }
            else if (key == SpectrogramConstants.KEY_Variance)//.Equals("VAR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.VAR_MIN, SpectrogramConstants.VAR_MAX);
            }
            else if (key == SpectrogramConstants.KEY_BinCover)//.Equals("CVR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, SpectrogramConstants.CVR_MIN, SpectrogramConstants.CVR_MAX);
            }
            else
            {
                //Logger.Warn("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                Console.WriteLine("DrawSpectrogramsOfIndicies is rendering an UNKNOWN INDEX or one not normalised");
                matrix = DataTools.Normalise(matrix, 0, 1);
            }
            return matrix;
        }

        public static Image DrawFalseColourSpectrogramOfIndice(string colorSchemeID, string colorMODE, int X_interval, int Y_interval, double[,] redMatrix, double[,] grnMatrix, double[,] bluMatrix, double[,] bgnMatrix)
        {
            // default is R,G,B -> aci, ten, avg/cvr
            bool doReverseColour = false;
            if (colorMODE.StartsWith("POS")) doReverseColour = true;

            Image bmp = ColourSpectrogram.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);

            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, Y_interval);
            return bmp;
        }

        public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColour)
        {
            // assume all amtricies are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            // int MinRGBValue = 0;
            int v1, v2, v3;
            double d1, d2, d3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++) 
                {
                    //d1 = redM[row, column];
                    //d2 = grnM[row, column];
                    //d3 = bluM[row, column];
                    d1 = redM[row, column] * redM[row, column]; // NOTE: MATRIX VALUES ARE SQUARED
                    d2 = grnM[row, column] * grnM[row, column];
                    d3 = bluM[row, column] * bluM[row, column];
                    if (doReverseColour)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }
                    v1 = Convert.ToInt32(Math.Max(0, d1 * MaxRGBValue));
                    v2 = Convert.ToInt32(Math.Max(0, d2 * MaxRGBValue));
                    v3 = Convert.ToInt32(Math.Max(0, d3 * MaxRGBValue));
                    Color colour = Color.FromArgb(v1, v2, v3);
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }

        /// <summary>
        /// A technique to derive a spectrogram from four different indices 
        /// same as above method but multiply index value by the amplitude value instead of squaring the value
        /// </summary>
        /// <param name="redM"></param>
        /// <param name="grnM"></param>
        /// <param name="bluM"></param>
        /// <param name="greM"></param>
        /// <param name="doReverseColour"></param>
        /// <returns></returns>
        public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, double[,] greM, bool doReverseColour)
        {
            // assume all amtricies are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            // int MinRGBValue = 0;
            int v1, v2, v3;
            double d1, d2, d3;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++) // note that the matrix values are multiplied by the grey matrix values.
                {
                    double gv = Math.Sqrt(greM[row, column]);
                    d1 = redM[row, column] * gv;
                    d2 = grnM[row, column] * gv;
                    d3 = bluM[row, column] * gv;
                    if (doReverseColour)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }
                    v1 = Convert.ToInt32(Math.Max(0, d1 * MaxRGBValue));
                    v2 = Convert.ToInt32(Math.Max(0, d2 * MaxRGBValue));
                    v3 = Convert.ToInt32(Math.Max(0, d3 * MaxRGBValue));
                    Color colour = Color.FromArgb(v1, v2, v3);
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }

        /// <summary>
        /// draw a colour spectrum of basic colours
        /// </summary>
        /// <param name="ht"></param>
        /// <returns></returns>
        public static Image DrawColourScale(int maxScaleLength, int ht)
        {
            int width = maxScaleLength / 7;
            if (width > ht) width = ht;
            else if (width < 3) width = 3;
            Bitmap colorScale = new Bitmap(8 * width, ht);
            Graphics gr = Graphics.FromImage(colorScale);
            int offset = width + 1;
            if (width < 5) offset = width;

            Bitmap colorBmp = new Bitmap(width - 1, ht);
            Graphics gr2 = Graphics.FromImage(colorBmp);
            Color c = Color.FromArgb(250, 15, 250);
            gr2.Clear(c);
            int x = 0;
            gr.DrawImage(colorBmp, x, 0); //dra
            c = Color.FromArgb(250, 15, 15);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            //yellow
            c = Color.FromArgb(250, 250, 15);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            //green
            c = Color.FromArgb(15, 250, 15);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            // pale blue
            c = Color.FromArgb(15, 250, 250);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            // blue
            c = Color.FromArgb(15, 15, 250);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            // purple
            c = Color.FromArgb(250, 15, 250);
            gr2.Clear(c);
            x += offset;
            gr.DrawImage(colorBmp, x, 0); //dra
            return (Image)colorScale;
        }





    }
}
