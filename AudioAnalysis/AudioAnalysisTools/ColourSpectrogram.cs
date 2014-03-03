using System;
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

        private Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 
        private Dictionary<string, Dictionary<string, double>> indexStats = new Dictionary<string, Dictionary<string, double>>(); // used to save mode and sd of the indices 

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="Xscale"></param>
        /// <param name="sampleRate"></param>
        /// <param name="colourMap"></param>
        public ColourSpectrogram(int Xscale, int sampleRate, string colourMap)
        {
            // set the X and Y axis scales for the spectrograms 
            this.X_interval = Xscale; 
            this.SampleRate = sampleRate; 
            this.ColorMap = colourMap; 
        }


        public Dictionary<string, double> GetIndexStatistics(string key)
        {
            return indexStats[key]; // used to return index statistics
        }

        public void SetIndexStatistics(string key, Dictionary<string, double> dict)
        {
            indexStats.Add(key, dict); // add index statistics
        }

        public void ReadCSVFiles(string ipdir, string fileName)
        {
            string key = SpectrogramConstants.KEY_BackgroundNoise;
            string path = Path.Combine(ipdir, fileName + "." + key + ".csv");
            if (File.Exists(path)) this.ReadSpectrogram(key, path);
            else
            {
                Console.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                Console.WriteLine("         File does not exist: " + path);
            }

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
            if (this.spectrogramMatrices.Count == 0)
            {
                Console.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                Console.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }
        }


        public void ReadCSVFiles(string ipdir, string fileName, string indexKeys)
        {
            string[] keys = indexKeys.Split('-');
            for (int key = 0; key < keys.Length; key++)
            {
                string path = Path.Combine(ipdir, fileName + "." + keys[key] + ".csv");
                if (File.Exists(path)) this.ReadSpectrogram(keys[key], path);
                else
                {
                    Console.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                    Console.WriteLine("         File does not exist: " + path);
                }
            } // for loop

            if (this.spectrogramMatrices.Count == 0)
            {
                Console.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                Console.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }
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


        /// <summary>
        /// Call this method if already have a dictionary of Matrix spectorgrams and wish to loae directly
        /// For example, call this method from AnalyseLongRecordings.
        /// </summary>
        /// <param name="dictionary"></param>
        public void LoadSpectrogramDictionary(Dictionary<string, double[,]> dictionary)
        {
            this.spectrogramMatrices = dictionary;
        }

        /// <summary>
        /// Call this method to access a spectrogram matrix
        /// </summary>
        /// <param name="dictionary"></param>
        public double[,] GetMatrix(string key)
        {
            return this.spectrogramMatrices[key];
        }

        //public void CalculateAcousticIndexModeAndStandardDeviation(string key)
        //{
        //    Dictionary<string, double> dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(this.spectrogramMatrices[key]);
        //    Console.WriteLine("{0}: Min={1:f3}   Max={2:f3}    Mode={3:f3}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);
        //    double[] avSD = { dict["mode"], dict["sd"] };
        //    this.avAndSd.Add(key, avSD);
        //}

        public void BlurSpectrogramMatrix(string key)
        {
            double[,] matrix = ImageTools.GaussianBlur_5cell(spectrogramMatrices[key]);
            spectrogramMatrices[key] = matrix;
        }

        public void DrawGreyScaleSpectrograms(string opdir, string opFileName, double backgroundFilter)
        {
            string putativeIndices = "ACI-AVG-CVR-TEN-VAR-CMB-BGN";
            string[] keys = putativeIndices.Split('-');
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    //this.ReadSpectrogram(key, path);
                    string path = Path.Combine(opdir, opFileName + "." + key + ".png");
                    Image bmp = this.DrawGreyscaleSpectrogramOfIndex(key, backgroundFilter);
                    bmp.Save(path);
                }
                else
                {
                    Console.WriteLine("WARNING: from method ColourSpectrogram.DrawGreyScaleSpectrograms()");
                    Console.WriteLine("         Dictionary of SpectrogramMatrices does not contain key: " + key);
                }
            } // for loop
        }

        public Image DrawGreyscaleSpectrogramOfIndex(string key, double backgroundFilter)
        {
            if (!this.spectrogramMatrices.ContainsKey(key))
            {
                Console.WriteLine("\n\nWARNING: From method ColourSpectrogram.DrawGreyscaleSpectrogramOfIndex()");
                Console.WriteLine("         Dictionary of spectrogram matrices does NOT contain key: {0}", key);
                List<string> keyList = new List<string>(this.spectrogramMatrices.Keys);
                string list = "";
                foreach (string str in keyList)
                {
                    list += (str + ", ");
                }
                Console.WriteLine("          List of keys in dictionary = {0}", list);
                return null;
                //Console.WriteLine("  Press <RETURN> to exit.");
                //Console.ReadLine();
                //System.Environment.Exit(666);
            }
            if (this.spectrogramMatrices[key] == null)
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawGreyscaleSpectrogramOfIndex()");
                Console.WriteLine("         Null matrix returned with key: {0}", key);
                return null;
                //Console.WriteLine("  Press <RETURN> to exit.");
                //Console.ReadLine();
                //System.Environment.Exit(666);
            }

            double[,] matrix = ColourSpectrogram.NormaliseSpectrogramMatrix(key, this.spectrogramMatrices[key], backgroundFilter);
            Image bmp = ImageTools.DrawMatrix(matrix);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, X_interval, this.Y_interval);
            return bmp;
        }

        public void DrawFalseColourSpectrograms(string opdir, string opFileName, double backgroundFilter)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE", backgroundFilter);
            bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEG.png"));

            Image bmpPos = this.DrawFalseColourSpectrogram("POSITIVE", backgroundFilter);
            bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOS.png"));

            Image bmpBgn = this.DrawGreyscaleSpectrogramOfIndex(SpectrogramConstants.KEY_BackgroundNoise, backgroundFilter);
            if (bmpBgn == null)
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawFalseColourSpectrograms()");
                Console.WriteLine("         Null image returned with key: {0}", SpectrogramConstants.KEY_BackgroundNoise);
                return;
            }

            bmpNeg = this.DrawDoubleSpectrogram(bmpNeg, bmpBgn, "NEGATIVE");
            bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEGBGN.png"));

            bmpPos = this.DrawDoubleSpectrogram(bmpPos, bmpBgn, "POSITIVE");
            bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOSBGN.png"));
        }

        /// <summary>
        /// Calculates a COMBO spectrogram from four equal weighted normalised indices.
        /// </summary>
        /// <param name="imagePath"></param>
        public Image DrawCombinedAverageSpectrogram(double backgroundFilter)
        {
            var avgMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_Average, spectrogramMatrices[SpectrogramConstants.KEY_Average], backgroundFilter);
            var cvrMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_BinCover, spectrogramMatrices[SpectrogramConstants.KEY_BinCover], backgroundFilter);
            var aciMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_AcousticComplexityIndex, spectrogramMatrices[SpectrogramConstants.KEY_AcousticComplexityIndex], backgroundFilter);
            var tenMatrix = NormaliseSpectrogramMatrix(SpectrogramConstants.KEY_TemporalEntropy, spectrogramMatrices[SpectrogramConstants.KEY_TemporalEntropy], backgroundFilter);

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

        public Image DrawFalseColourSpectrogram(string colorMODE, double backgroundFilter)
        {
            string[] rgbMap = this.ColorMap.Split('-');

            var redMatrix = NormaliseSpectrogramMatrix(rgbMap[0], spectrogramMatrices[rgbMap[0]], backgroundFilter);
            var grnMatrix = NormaliseSpectrogramMatrix(rgbMap[1], spectrogramMatrices[rgbMap[1]], backgroundFilter);
            var bluMatrix = NormaliseSpectrogramMatrix(rgbMap[2], spectrogramMatrices[rgbMap[2]], backgroundFilter);
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
            Image scale = ImageTools.DrawColourScale(maxScaleLength, trackHeight - 2);
            int xLocation = imageWidth * 2 / 3;
            gr.DrawImage(scale, xLocation, 1); //dra
            return compositeBmp;
        }


        public static Image FrameSpectrogram(Image bmp1, Image titleBar, int X_interval, int Y_interval)
        {
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp1, X_interval, Y_interval);

            int imageWidth = bmp1.Width;
            int trackHeight = 20;

            int imageHt = bmp1.Height + trackHeight + trackHeight + trackHeight;
            int timeScale = 60;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, timeScale, imageWidth, trackHeight, "hours");

            Bitmap compositeBmp = new Bitmap(imageWidth, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            int offset = 0;
            gr.DrawImage(titleBar, 0, offset); //draw in the top time scale
            offset += timeBmp.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra
            offset += titleBar.Height;
            gr.DrawImage(bmp1, 0, offset); //dra
            offset += bmp1.Height;
            gr.DrawImage(timeBmp, 0, offset); //dra
            //offset += timeBmp.Height;
            //gr.DrawImage(timeBmp, 0, offset); //dra

            //draw a colour spectrum of basic colours
            //int maxScaleLength = imageWidth / 3;
            //Image scale = ColourSpectrogram.DrawColourScale(maxScaleLength, trackHeight - 2);
            //int xLocation = imageWidth * 2 / 3;
            //gr.DrawImage(scale, xLocation, 1); //dra
            return compositeBmp;
        }

        public static Image DrawTitleBarOfFalseColourSpectrogram(string title, int width, int height)
        {
            Image colourChart = ImageTools.DrawColourScale(width, height - 2);

            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            //string text = title;
            int X = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(title, stringFont);
            X += (stringSize.ToSize().Width + 70);
            //text = name1 + "  +99.9%conf";
            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            //X += colourChart.Width;
            //text = "-99.9%conf   " + name2;
            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1); //distance to end of string


            string text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;

            
            //Bitmap titleBmp = Image_Track.DrawTitleTrack(width, height, title);
            //Graphics gr = Graphics.FromImage(titleBmp);
            //int offset = 0;
            ////gr.DrawImage(titleBmp, 0, offset); //draw in the top time scale

            ////draw a colour spectrum of basic colours
            //int maxScaleLength = width / 3;
            //int xLocation = width * 2 / 3;
            //gr.DrawImage(scale, xLocation, 1); //dra
            //return titleBmp;
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
            if(X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
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
                //string ipdir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM20036.Towsey.Acoustic"; // SUNSHINE COAST 13th October 2011 DM420036.MP3
                //string ipdir = @"C:\SensorNetworks\Output\SERF\2013Sept15th_MergedCSVs"; // SERF
                //string ipdir = @"C:\SensorNetworks\Output\SERF\2013August30th_MergedCSVs"; // SERF
                string ipdir = @"C:\SensorNetworks\Output\Test2\Towsey.Acoustic";
                //string ipdir = @"C:\SensorNetworks\Output\SERF\BeforeRefactoring\Towsey.Acoustic";// SERF 13th October 2010
                //string ipdir = @"C:\SensorNetworks\Output\SERF\AfterRefactoring\Towsey.Acoustic"; // SERF

                //string ipFileName = @"7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
                string ipFileName = @"TEST_TUITCE_20091215_220004";
                //string ipFileName = "DM420233_20120302_000000";
                //string ipFileName = "SERF_20130915_Merged";
                //string ipFileName = "SERF_20130730_Merged";

                // OUTPUT CSV FILES
                string opdir = ipdir;
                //string opdir = @"C:\SensorNetworks\Output\SERF\AfterRefactoring\Towsey.Acoustic";  //SERF
                //string opdir = @"Z:\Results\2013Dec22-220529 - SERF VEG 2011\SERF\VEG\DM420233_20120302_000000.MP3\Towsey.Acoustic"; // SERF
                //string opdir = @"C:\SensorNetworks\Output\SERF\2014Jan30";
                //string opdir = @"C:\SensorNetworks\Output\SunshineCoast\Site1\2013DEC.DM20036.Towsey.Acoustic"; // SUNSHINE COAST
                //string opdir = @"C:\SensorNetworks\Output\temp";
                string opFileName = ipFileName + ".Test1";


                // set the X and Y axis scales for the spectrograms 
                int xScale = 60;  // assume one minute spectra and hourly time lines
                int sampleRate = 17640; // default value - after resampling
                string colorMap = SpectrogramConstants.RGBMap_ACI_TEN_BGN; //CHANGE RGB mapping here.
                var cs = new ColourSpectrogram(xScale, sampleRate, colorMap);
                cs.ReadCSVFiles(ipdir, ipFileName);
                double backgroundFilterCoeff = 1.0; // 1.0 = no background filtering
                cs.DrawGreyScaleSpectrograms(opdir, opFileName, backgroundFilterCoeff);
                cs.DrawFalseColourSpectrograms(opdir, opFileName, backgroundFilterCoeff);
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

        public static double[,] NormaliseSpectrogramMatrix(string key, double[,] matrix, double backgroundFilterCoeff)
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

            matrix = MatrixTools.FilterBackgroundValues(matrix, backgroundFilterCoeff); // to de-demphasize the background small values
            return matrix;
        } //NormaliseSpectrogramMatrix()


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
                    d1 = redM[row, column];
                    d2 = grnM[row, column];
                    d3 = bluM[row, column];

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
        public static Image DrawFourColourSpectrogram(double[,] redM, double[,] grnM, double[,] bluM, double[,] greM, bool doReverseColour)
        {
            // assume all matrices are normalised and of the same dimensions
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


        public static Image DrawDifferenceSpectrogram(ColourSpectrogram target, ColourSpectrogram reference, double colourGain)
        {
        //public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColour)
            double[,] tgtRedM = target.spectrogramMatrices["ACI"]; 
            double[,] tgtGrnM = target.spectrogramMatrices["TEN"]; 
            double[,] tgtBluM = target.spectrogramMatrices["CVR"];

            double[,] refRedM = reference.spectrogramMatrices["ACI"];
            double[,] refGrnM = reference.spectrogramMatrices["TEN"];
            double[,] refBluM = reference.spectrogramMatrices["CVR"];


            // assume all matricies are normalised and of the same dimensions
            int rows = tgtRedM.GetLength(0); //number of rows
            int cols = tgtRedM.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            // int MinRGBValue = 0;
            //int v1, v2, v3;
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


        public static Image DrawTStatisticSpectrogram(ColourSpectrogram cs1, ColourSpectrogram cs2, int N, double tStatisticMax)
        {
            double[,] avg1 = cs1.spectrogramMatrices["AVG"];
            double[,] var1 = cs1.spectrogramMatrices["VAR"];

            double[,] avg2 = cs2.spectrogramMatrices["AVG"];
            double[,] var2 = cs2.spectrogramMatrices["VAR"];


            // assume all matricies are normalised and of the same dimensions
            int rows = avg1.GetLength(0); //number of rows
            int cols = avg1.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            int MaxRGBValue = 255;
            double tStat;
            int i1, i2, i3;
            double u1,u2,v1,v2;
            int expectedMinAvg = 0; // expected minimum average  of spectral dB above background
            int expectedMinVar = 1; // expected minimum variance of spectral dB above background

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    //catch low values of dB used to avoid log of zero amplitude.
                    u1 = avg1[row, column];
                    u2 = avg2[row, column];
                    v1 = Math.Abs(var1[row, column]);
                    v2 = Math.Abs(var2[row, column]);
                    if (u1 < expectedMinAvg) 
                    { u1 = expectedMinAvg; v1 = expectedMinVar; } 
                    if (u2 < expectedMinAvg) 
                    { u2 = expectedMinAvg; v2 = expectedMinVar; }

                    double diffOfMeans = Math.Abs(u1 - u2);
                    if (diffOfMeans < 0.0001) tStat = 0.0;
                    else
                    {
                        double S12 = (v1 + v2) / N;
                        tStat = diffOfMeans / Math.Sqrt(S12);
                        if (tStat > tStatisticMax) tStat = tStatisticMax; // upper bound
                        tStat /= tStatisticMax; // normalise
                    }

                    i1 = Convert.ToInt32(tStat * MaxRGBValue);
                    //i1 = Math.Max(0, i1);
                    //i1 = Math.Min(MaxRGBValue, i1);

                    Color colour = Color.FromArgb(i1, i1, i1);

                    if (i1 > 240) {colour = Color.Red; } //99.9% conf
                    else 
                    {
                        if (i1 > 200) {colour = Color.Orange;}
                        else
                        {
                            if (i1 > 160) { colour = Color.Yellow; }
                        }
                    }
                    bmp.SetPixel(column, row, colour);
                }//end all columns
            }//end all rows
            return bmp;
        }


        public static Image DrawDistanceSpectrogram(ColourSpectrogram cs1, ColourSpectrogram cs2)
        {
            string key = "ACI";
            double[,] aciMatrix1 = cs1.GetMatrix(key);
            var dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(aciMatrix1);
            cs1.SetIndexStatistics(key, dict);
            aciMatrix1 = MatrixTools.Matrix2ZScores(aciMatrix1, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = "TEN";
            double[,] tenMatrix1 = cs1.GetMatrix(key);
            tenMatrix1 = MatrixTools.SubtractValuesFromOne(tenMatrix1);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(tenMatrix1);
            cs1.SetIndexStatistics(key, dict);
            tenMatrix1 = MatrixTools.Matrix2ZScores(tenMatrix1, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = "CVR";
            double[,] cvrMatrix1 = cs1.GetMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(cvrMatrix1);
            cs1.SetIndexStatistics(key, dict);
            cvrMatrix1 = MatrixTools.Matrix2ZScores(cvrMatrix1, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = "ACI";
            double[,] aciMatrix2 = cs2.GetMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(aciMatrix2);
            cs2.SetIndexStatistics(key, dict);
            aciMatrix2 = MatrixTools.Matrix2ZScores(aciMatrix2, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = "TEN";
            double[,] tenMatrix2 = cs2.GetMatrix(key);
            tenMatrix2 = MatrixTools.SubtractValuesFromOne(tenMatrix2);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(tenMatrix2);
            cs2.SetIndexStatistics(key, dict);
            tenMatrix2 = MatrixTools.Matrix2ZScores(tenMatrix2, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = "CVR";
            double[,] cvrMatrix2 = cs2.GetMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(cvrMatrix2);
            cs2.SetIndexStatistics(key, dict);
            cvrMatrix2 = MatrixTools.Matrix2ZScores(cvrMatrix2, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);


            double[] v1 = new double[3];
            double[] mode1 = { cs1.indexStats["ACI"]["mode"], cs1.indexStats["TEN"]["mode"], cs1.indexStats["CVR"]["mode"] };
            double[] stDv1 = { cs1.indexStats["ACI"]["sd"], cs1.indexStats["TEN"]["sd"], cs1.indexStats["CVR"]["sd"] };
            Console.WriteLine("1: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", mode1[0], stDv1[0],  mode1[1], stDv1[1], mode1[2], stDv1[2]);

            double[] v2 = new double[3];
            double[] mode2 = { cs2.indexStats["ACI"]["mode"], cs2.indexStats["TEN"]["mode"], cs2.indexStats["CVR"]["mode"] };
            double[] stDv2 = { cs2.indexStats["ACI"]["sd"], cs2.indexStats["TEN"]["sd"], cs2.indexStats["CVR"]["sd"] };
            Console.WriteLine("2: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", mode2[0], stDv2[0], mode2[1], stDv2[1], mode2[2], stDv2[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = aciMatrix1.GetLength(0); //number of rows
            int cols = aciMatrix1.GetLength(1); //number
            double[,] d12Matrix = new double[rows, cols];
            double[,] d11Matrix = new double[rows, cols];
            double[,] d22Matrix = new double[rows, cols];
            double av1, av2;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    v1[0] = aciMatrix1[row, col];
                    v1[1] = tenMatrix1[row, col];
                    v1[2] = cvrMatrix1[row, col];
                    av1 = (v1[0] + v1[1] + v1[2]) / 3;

                    v2[0] = aciMatrix2[row, col];
                    v2[1] = tenMatrix2[row, col];
                    v2[2] = cvrMatrix2[row, col];
                    av2 = (v2[0] + v2[1] + v2[2]) / 3;

                    d12Matrix[row, col] = DataTools.EuclidianDistance(v1, v2);
                    d11Matrix[row, col] = av1;
                    d22Matrix[row, col] = av2;
                    //d11Matrix[row, col] = DataTools.VectorEuclidianLength(v1);
                    //d22Matrix[row, col] = DataTools.VectorEuclidianLength(v2);

                    if ((row == 150) && (col == 1100))
                    {
                        Console.WriteLine("V1={0:f3}, {1:f3}, {2:f3}", v1[0], v1[1], v1[2]);
                        Console.WriteLine("V2={0:f3}, {1:f3}, {2:f3}", v2[0], v2[1], v2[2]);
                        Console.WriteLine("EDist12={0:f4};   ED11={1:f4};   ED22={2:f4}", d12Matrix[row, col], d11Matrix[row, col], d22Matrix[row, col]);
                    }
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
            Dictionary<string, Color> colourChart = ColourSpectrogram.GetDifferenceColourChart();
            Color colour;

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    zScore = d12Matrix[row, col];

                    if(d11Matrix[row, col] >= d22Matrix[row, col])
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



        public static Image FrameThreeSpectrograms(Image spg1, Image spg2, Image distanceSpg, int X_interval, int Y_interval)
        {
            int width = spg1.Width; // assume all images have the same width
            int trackHeight = 20;
            //int spgHeight = spg1.Height; // assume all images have the same width

            Bitmap timeBmp = Image_Track.DrawTimeTrack(width, X_interval, width, trackHeight, "hours");
            int compositeHeight = spg1.Height + spg2.Height + distanceSpg.Height;

            Bitmap compositeBmp = new Bitmap(width, compositeHeight, PixelFormat.Format24bppRgb);
            int yOffset = 0;
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            gr.DrawImage(spg1, 0, yOffset); //draw in the top spectrogram
            yOffset += spg1.Height;
            gr.DrawImage(spg2, 0, yOffset); //draw in the second spectrogram
            yOffset += spg2.Height;
            //gr.DrawImage(timeBmp, 0, yOffset); //draw in the top time scale
            //yOffset += timeBmp.Height;

            gr.DrawImage(distanceSpg, 0, yOffset); //draw in the distance spectrogram
            yOffset += distanceSpg.Height;
            gr.DrawImage(timeBmp, 0, yOffset); //draw in the bottom time scale
            //yOffset += timeBmp.Height;

            return (Image)compositeBmp;
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
        public static Color[] ColourChart2Array(Dictionary<string, Color> chart)
        {
            Color[] array = new Color[9];
            array[0] = chart["+99.9%"];
            array[1] = chart["+99.0%"];
            array[2] = chart["+95.0%"];
            array[3] = chart["+NotSig"];
            array[4] = chart["NoValue"];
            array[5] = chart["-NotSig"];
            array[6] = chart["-95.0%"];
            array[7] = chart["-99.0%"];
            array[8] = chart["-99.9%"];
            return array;
        }




        public static void BlurSpectrogram(ColourSpectrogram cs)
        {
            string[] rgbMap = cs.ColorMap.Split('-');

            cs.BlurSpectrogramMatrix(rgbMap[0]);
            cs.BlurSpectrogramMatrix(rgbMap[1]);
            cs.BlurSpectrogramMatrix(rgbMap[2]);
        }

        public static void BlurSpectrogram(ColourSpectrogram cs, string matrixKeys)
        {
            string[] keys = matrixKeys.Split('-');

            for (int k = 0; k < keys.Length; k++)
            {
                cs.BlurSpectrogramMatrix(keys[k]);
            }
        }


    } //ColourSpectrogram
}
