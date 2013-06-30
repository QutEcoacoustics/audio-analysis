using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

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

        // CONST string for referring to different types of spectrogram - these should really be an enum                
        public const string KEY_BackgroundNoise = "BGN";
        public const string KEY_AcousticComplexityIndex = "ACI";
        public const string KEY_Average = "AVG";
        public const string KEY_Variance = "VAR";
        public const string KEY_BinCover = "CVR";
        public const string KEY_TemporalEntropy = "TEN";
        public const string KEY_Combined = "CMB";
        public const string KEY_Colour   = "COL";

        // NORMALISING CONSTANTS FOR INDICES
        public const double AVG_MIN = -7.0;
        public const double AVG_MAX = 0.5;
        public const double VAR_MIN = -10.0;
        public const double VAR_MAX = 0.5;
        public const double BGN_MIN = -7.0;
        public const double BGN_MAX = 0.5;
        public const double ACI_MIN = 0.3;
        public const double ACI_MAX = 0.7;
        public const double CVR_MIN = 0.1;
        public const double CVR_MAX = 0.8;
        public const double TEN_MIN = 0.5;
        public const double TEN_MAX = 1.0;

        //private static readonly ILog Logger =
        //    LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int x_interval = 60;    // assume one minute spectra and hourly time lines
        public int X_interval
        {
            get { return x_interval; }
            set { x_interval = value; }
        }
        private int frameWidth = 512;   // default value - from which spectrogram was derived
        public int FrameWidth
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
            get { double freqBinWidth = sampleRate / (double)frameWidth;
                  return (int)Math.Round(1000 / freqBinWidth); 
            } 
        }

        public string ColorSchemeID { get; set; } //within current recording     

        Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 





        public void ReadSpectrogram(string key, string csvPath)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);
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

        public void DrawGreyscaleSpectrogramOfIndex(string key, string imagePath)
        {
            double[,] matrix = ColourSpectrogram.NormaliseSpectrogramMatrix(key, this.spectrogramMatrices[key]);
            Image bmp = ImageTools.DrawMatrix(matrix);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, Y_interval);
            bmp.Save(imagePath);
        }


        /// <summary>
        /// Calculates a COMBO spectrogram from four equal weighted normalised indices.
        /// </summary>
        /// <param name="imagePath"></param>
        public void DrawCombinedAverageSpectrogram(string imagePath) 
        {
            var avgMatrix = NormaliseSpectrogramMatrix(ColourSpectrogram.KEY_Average, spectrogramMatrices[ColourSpectrogram.KEY_Average]);
            var cvrMatrix = NormaliseSpectrogramMatrix(ColourSpectrogram.KEY_BinCover, spectrogramMatrices[ColourSpectrogram.KEY_BinCover]);
            var aciMatrix = NormaliseSpectrogramMatrix(ColourSpectrogram.KEY_AcousticComplexityIndex, spectrogramMatrices[ColourSpectrogram.KEY_AcousticComplexityIndex]);
            var tenMatrix = NormaliseSpectrogramMatrix(ColourSpectrogram.KEY_TemporalEntropy, spectrogramMatrices[ColourSpectrogram.KEY_TemporalEntropy]);

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
            bmp.Save(imagePath);
        }

        public void DrawFalseColourSpectrogramOfIndices(string imagePath)
        {
            var avgMatrix = spectrogramMatrices[ColourSpectrogram.KEY_Average];
            var cvrMatrix = spectrogramMatrices[ColourSpectrogram.KEY_BinCover];
            var aciMatrix = spectrogramMatrices[ColourSpectrogram.KEY_AcousticComplexityIndex];
            var tenMatrix = spectrogramMatrices[ColourSpectrogram.KEY_TemporalEntropy];

            Image bmp = ColourSpectrogram.DrawFalseColourSpectrogramOfIndices(this.ColorSchemeID, this.X_interval, this.Y_interval, avgMatrix, cvrMatrix, aciMatrix, tenMatrix);            
            bmp.Save(imagePath);
        }

        public void DrawDoubleSpectrogram(string imagePath)
        {
            var bgnMatrix = ColourSpectrogram.NormaliseSpectrogramMatrix(ColourSpectrogram.KEY_BackgroundNoise, this.spectrogramMatrices[ColourSpectrogram.KEY_BackgroundNoise]);
            Image bmp1 = ImageTools.DrawMatrix(bgnMatrix);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp1, X_interval, Y_interval);

            var avgMatrix = this.spectrogramMatrices[ColourSpectrogram.KEY_Average];
            var cvrMatrix = this.spectrogramMatrices[ColourSpectrogram.KEY_BinCover];
            var aciMatrix = this.spectrogramMatrices[ColourSpectrogram.KEY_AcousticComplexityIndex];
            var tenMatrix = this.spectrogramMatrices[ColourSpectrogram.KEY_TemporalEntropy];

            Image bmp2 = ColourSpectrogram.DrawFalseColourSpectrogramOfIndices(this.ColorSchemeID, this.X_interval, this.Y_interval, avgMatrix, cvrMatrix, aciMatrix, tenMatrix);

            int imageWidth = bmp1.Width;
            int trackHeight = 20;
            int imageHt = bmp1.Height + bmp2.Height + trackHeight + trackHeight + trackHeight;
            string title = String.Format("FALSE COLOUR and BACKGROUND NOISE SPECTROGRAMS      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", this.ColorSchemeID);
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, trackHeight, title);
            int timeScale = 60;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, timeScale, imageWidth, trackHeight, "hours");

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            int offset = 0;
            gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale
            offset += titleBmp.Height;
            gr.DrawImage(bmp2, 0, offset); //dra
            offset += bmp2.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra
            offset += timeBmp.Height;
            gr.DrawImage(bmp1, 0, offset); //dr
            offset += bmp1.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra

            //draw a colour spectrum of basic colours
            int maxScaleLength = imageWidth / 3;
            Image scale = ColourSpectrogram.DrawColourScale(maxScaleLength, trackHeight - 2);
            int xLocation = imageWidth * 2 / 3;
            gr.DrawImage(scale, xLocation, 1); //dra
            compositeBmp.Save(imagePath);
        }


        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################

        public static double[,] NormaliseSpectrogramMatrix(string key, double[,] matrix)
        {
            if (key == KEY_AcousticComplexityIndex) //.Equals("ACI"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.ACI_MIN, ColourSpectrogram.ACI_MAX);
            }
            else if (key == KEY_TemporalEntropy)//.Equals("TEN"))
            {
                // normalise and reverse
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.TEN_MIN, ColourSpectrogram.TEN_MAX);
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
            else if (key == KEY_Average)//.Equals("AVG"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.AVG_MIN, ColourSpectrogram.AVG_MAX);
            }
            else if (key == KEY_BackgroundNoise)//.Equals("BGN"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.BGN_MIN, ColourSpectrogram.BGN_MAX);
            }
            else if (key == KEY_Variance)//.Equals("VAR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.VAR_MIN, ColourSpectrogram.VAR_MAX);
            }
            else if (key == KEY_BinCover)//.Equals("CVR"))
            {
                matrix = DataTools.NormaliseInZeroOne(matrix, ColourSpectrogram.CVR_MIN, ColourSpectrogram.CVR_MAX);
            }
            else
            {
                //Logger.Warn("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                Console.WriteLine("DrawSpectrogramsOfIndicies is rendering an INDEX that is not specially normalised");
                matrix = DataTools.Normalise(matrix, 0, 1);
            }
            return matrix;
        }
                
        public static Image DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        {
            avgMatrix = DataTools.NormaliseInZeroOne(avgMatrix, ColourSpectrogram.AVG_MIN, ColourSpectrogram.AVG_MAX);
            aciMatrix = DataTools.NormaliseInZeroOne(aciMatrix, ColourSpectrogram.ACI_MIN, ColourSpectrogram.ACI_MAX);
            cvrMatrix = DataTools.NormaliseInZeroOne(cvrMatrix, ColourSpectrogram.CVR_MIN, ColourSpectrogram.CVR_MAX);
            tenMatrix = DataTools.NormaliseReverseInZeroOne(tenMatrix, ColourSpectrogram.TEN_MIN, ColourSpectrogram.TEN_MAX);

            // default is R,G,B -> aci, ten, avg/cvr
            bool doReverseColour = false;
            Image bmp = null;
            if (colorSchemeID.Equals("ACI-TEN-CVR-REV"))
            {
                doReverseColour = true;
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-TEN-CVR"))
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-CVR-TEN"))
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, cvrMatrix, tenMatrix, doReverseColour);
            }
            else
            if (colorSchemeID.Equals("ACI-TEN-AVG-REV")) //R-G-B
            {
                doReverseColour = true;
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, avgMatrix, doReverseColour);
            }
            else // the default
            if (colorSchemeID.Equals("ACI-TEN-CVR_AVG")) //R-G-B-GREY
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, avgMatrix, doReverseColour);
            }
            else // the default
            if (colorSchemeID.Equals("ACI-TEN-CVR_AVG-REV")) //R-G-B-GREY
            {
                doReverseColour = true;
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, avgMatrix, doReverseColour);
            }
            else // the default
            {
                bmp = ColourSpectrogram.DrawRGBColourMatrix(aciMatrix, tenMatrix, cvrMatrix, doReverseColour);
            }
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
                for (int column = 0; column < cols; column++) // note that the matrix valeus are squared.
                {
                    d1 = redM[row, column] * redM[row, column];
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

            Bitmap colorBmp = new Bitmap(width-1, ht);
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


            public static void Sandpit()
            {
                //KIWI one hour recording
                //var csvAvg = @"C:\Work\Software Dev\ColourSpectrogram\TUITCE_20091215_220004.avgSpectrum.csv";
                //var csvCvr = @"C:\Work\Software Dev\ColourSpectrogram\TUITCE_20091215_220004.cvrSpectrum.csv";
                //var csvAci = @"C:\Work\Software Dev\ColourSpectrogram\TUITCE_20091215_220004.aciSpectrum.csv";
                //var csvTen = @"C:\Work\Software Dev\ColourSpectrogram\TUITCE_20091215_220004.tenSpectrum.csv";
                //string imagePath = @"C:\Work\Software Dev\ColourSpectrogram\TUITCE_20091215_220004.cmbSpectrum_colour_towardsblack.png";

                // SERF 13th October 2010
                string bgnCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.bgnSpectrum.csv";
                string cvrCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.cvrSpectrum.csv";
                string avgCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.avgSpectrum.csv";
                string aciCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.aciSpectrum.csv";
                string tenCsvPath = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.tenSpectrum.csv";

                string imagePath1 = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.COLSpectroTest11.png";
                string imagePath2 = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.BGNSpectroTest11.png";
                string imagePath3 = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.TENSpectroTest11.png";
                string imagePath4 = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.CMBSpectroTest11.png";
                string imagePath5 = @"C:\SensorNetworks\Output\FalseColourSpectrograms\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.TWOSpectroTest11.png";

                // colour scheme IDs for RGB plus reverse
                // Need to add new ones into DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
                //string colorSchemeID = "DEFAULT"; //R-G-B
                //string colorSchemeID = "ACI-TEN-AVG-REV"; //R-G-B
                string colorSchemeID = "ACI-TEN-CVR"; //R-G-B
                //string colorSchemeID = "ACI-TEN-CVR-REV";
                //string colorSchemeID = "ACI-CVR-TEN";
                //string colorSchemeID = "ACI-TEN-CVR_AVG-REV";
                //string colorSchemeID = "ACI-TEN-CVR_AVG";

                var cs = new ColourSpectrogram();
                // set the X and Y axis scales for the spectrograms 
                cs.X_interval = 60;    // assume one minute spectra and hourly time lines
                cs.FrameWidth = 512;   // default value - from which spectrogram was derived
                cs.SampleRate = 17640; // default value - after resampling
                cs.ColorSchemeID = colorSchemeID;
                cs.ReadSpectrogram(ColourSpectrogram.KEY_BackgroundNoise, bgnCsvPath);
                cs.ReadSpectrogram(ColourSpectrogram.KEY_BinCover, cvrCsvPath);
                cs.ReadSpectrogram(ColourSpectrogram.KEY_Average, avgCsvPath);
                cs.ReadSpectrogram(ColourSpectrogram.KEY_AcousticComplexityIndex, aciCsvPath);
                cs.ReadSpectrogram(ColourSpectrogram.KEY_TemporalEntropy, tenCsvPath);
                cs.DrawFalseColourSpectrogramOfIndices(imagePath1);
                // draw gray scale spectrogram
                cs.DrawGreyscaleSpectrogramOfIndex(ColourSpectrogram.KEY_BackgroundNoise, imagePath2);
                cs.DrawGreyscaleSpectrogramOfIndex(ColourSpectrogram.KEY_TemporalEntropy, imagePath3);

                cs.DrawCombinedAverageSpectrogram(imagePath4);
                cs.DrawDoubleSpectrogram(imagePath5);
            }


    }
}
