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
        /// <summary>
        /// File name from which spectrogram was derived.
        /// </summary>
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        //Define new colour schemes in class SpectorgramConstants and implement the code in the class Colourspectrogram, 
        //            method DrawFalseColourSpectrogramOfIndices(string colorSchemeID, int X_interval, int Y_interval, double[,] avgMatrix, double[,] cvrMatrix, double[,] aciMatrix, double[,] tenMatrix)
        public string colorSchemeID = SpectrogramConstants.RGBMap_DEFAULT; //R-G-B

        //private static readonly ILog Logger =
        //    LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private int minOffset = 0;    // default recording starts at midnight
        public int MinuteOffset
        {
            get { return minOffset; }
            set { minOffset = value; }
        }

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

        private double backgroundFilter = 1.0; // default value = no filtering
        public double BackgroundFilter
        {
            get { return backgroundFilter; }
            set { backgroundFilter = value; }
        }

        public string ColorMap { get; set; }    //within current recording     
        public string ColorMODE { get; set; }   //POSITIVE or NEGATIVE     

        private Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 
        private Dictionary<string, double[,]> spgr_StdDevMatrices;                                       // used if reading standard devaition matrices for tTest
        private Dictionary<string, Dictionary<string, double>> indexStats = new Dictionary<string, Dictionary<string, double>>(); // used to save mode and sd of the indices 

        private int N; // default value 
        public int SampleCount // only used where the spectrograms are derived from vaerages and want to do t-test of difference.
        {
            get { return N; }
            set { N = value; }
        }

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

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="minuteOffset">minute of day at which the spectrogram starts</param>
        /// <param name="Xscale">time scale : pixels per hour</param>
        /// <param name="sampleRate">recording smaple rate which also determines scale of Y-axis.</param>
        /// <param name="frameWidth">frame size - which also determines scale of Y-axis.</param>
        /// <param name="colourMap">acoustic indices used to assign  the three colour mapping.</param>
        public ColourSpectrogram(int minuteOffset, int Xscale, int sampleRate, int frameWidth, string colourMap)
        {
            // set the X-axis scale for the spectrogram 
            this.minOffset = minuteOffset;
            this.X_interval = Xscale;
            // set the Y axis scale for the spectrogram 
            this.SampleRate = sampleRate;
            this.FrameWidth = frameWidth;
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
            string keys = "ACI-AVG-BGN-CVR-TEN-VAR";
            ReadCSVFiles(ipdir, fileName, keys);
        }


        public void ReadCSVFiles(string ipdir, string fileName, string indexKeys)
        {
            string[] keys = indexKeys.Split('-');
            string warning = null;
            for (int key = 0; key < keys.Length; key++)
            {
                string path = Path.Combine(ipdir, fileName + "." + keys[key] + ".csv");
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = ColourSpectrogram.ReadSpectrogram(path, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    this.spectrogramMatrices.Add(keys[key], matrix);
                    this.FrameWidth = freqBinCount * 2;

                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method ColourSpectrogram.ReadCSVFiles()";
                    }
                    warning += String.Format("\n      {0} File does not exist: {1}", keys[key], path);
                }
            } // for loop

            if (warning != null)
            {
                Console.WriteLine(warning);
            }
            if (this.spectrogramMatrices.Count == 0)
            {
                Console.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                Console.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }
        }


        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(string ipdir, string fileName, string indexKeys, out int freqBinCount)
        {
            Dictionary<string, double[,]> dict = new Dictionary<string, double[,]>();
            string[] keys = indexKeys.Split('-');
            string warning = null;
            freqBinCount = 256; //the default
            for (int key = 0; key < keys.Length; key++)
            {
                string path = Path.Combine(ipdir, fileName + "." + keys[key] + ".csv");
                if (File.Exists(path))
                {
                    int binCount;
                    double[,] matrix = ColourSpectrogram.ReadSpectrogram(path, out binCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    dict.Add(keys[key], matrix);
                    freqBinCount = binCount;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method ColourSpectrogram.ReadSpectrogramCSVFiles()";
                    }
                    warning += String.Format("\n      {0} File does not exist: {1}", keys[key], path);
                }
            } // for loop

            if (warning != null)
            {
                Console.WriteLine(warning);
            }
            if (dict.Count == 0)
            {
                Console.WriteLine("WARNING: from method ColourSpectrogram.ReadSpectrogramCSVFiles()");
                Console.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }
            return dict;
        }

        public void ReadStandardDeviationSpectrogramCSVs(string ipdir, string fileName)
        {
            //string keys = "ACI-AVG-BGN-CVR-TEN-VAR";
            int freqBinCount;
            this.spgr_StdDevMatrices = ReadSpectrogramCSVFiles(ipdir, fileName, this.ColorMap, out freqBinCount);
            this.FrameWidth = freqBinCount * 2;
        }



        public static double[,] ReadSpectrogram(string csvPath, out int binCount)
        {
            double[,] matrix = CsvTools.ReadCSVFile2Matrix(csvPath);
            binCount = matrix.GetLength(1) - 1; // -1 because first bin is the index numbers 
            // calculate the window/frame that was used to generate the spectra. This value is only used to place grid lines on the final images

            // remove left most column - consists of index numbers
            matrix = MatrixTools.Submatrix(matrix, 0, 1, matrix.GetLength(0) - 1, matrix.GetLength(1) - 3); // -3 to avoid anomalies in top freq bin
            return matrix;
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

        /// <summary>
        /// returns a matrix of acoustic indices whose values are normalised.
        /// In addition, temporal entropy is subtracted from 1.0.
        /// In addition, small background values are reduced as per filter coeeficient. 1.0 = unchanged. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="backgroundFilterCoeff"></param>
        /// <returns></returns>
        public double[,] GetNormalisedSpectrogramMatrix(string key)
        {
            return ColourSpectrogram.NormaliseSpectrogramMatrix(key, this.GetMatrix(key), this.BackgroundFilter);
        }


        public void BlurSpectrogramMatrix(string key)
        {
            double[,] matrix = ImageTools.GaussianBlur_5cell(spectrogramMatrices[key]);
            spectrogramMatrices[key] = matrix;
        }

        public void DrawGreyScaleSpectrograms(string opdir, string opFileName)
        {
            DrawGreyScaleSpectrograms(opdir, opFileName, this.ColorMap);
        }

        public void DrawGreyScaleSpectrograms(string opdir, string opFileName, string keyString)
        {
            //string putativeIndices = "ACI-AVG-CVR-TEN-VAR-CMB-BGN";
            string warning = null;

            string[] keys = keyString.Split('-');
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    string path = Path.Combine(opdir, opFileName + "." + key + ".png");
                    Image bmp = this.DrawGreyscaleSpectrogramOfIndex(key);
                    bmp.Save(path);
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method ColourSpectrogram.DrawGreyScaleSpectrograms()";
                        warning += "\n     " + opFileName + ": Dictionary of SpectrogramMatrices does not contain key(s): ";
                    }
                    warning += String.Format("{0} ", key);
                }
            } // for loop
            if (warning != null)
            {
                Console.WriteLine(warning);
            }
        }

        public Image DrawGreyscaleSpectrogramOfIndex(string key)
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

            double[,] matrix = ColourSpectrogram.NormaliseSpectrogramMatrix(key, this.spectrogramMatrices[key], this.BackgroundFilter);
            Image bmp = ImageTools.DrawMatrix(matrix);
            //ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, this.Y_interval);
            return bmp;
        }

        public void DrawFalseColourSpectrograms(string opdir, string opFileName)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEG.png"));

            Image bmpPos = this.DrawFalseColourSpectrogram("POSITIVE");
            bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOS.png"));

            Image bmpBgn;
            string key = SpectrogramConstants.KEY_BackgroundNoise;
            if (!this.spectrogramMatrices.ContainsKey(key))
            {
                Console.WriteLine("\nWARNING: SG {0} does not contain key: {1}", opFileName, key);
                //return;
            }
            else
            {
                bmpBgn = this.DrawGreyscaleSpectrogramOfIndex(key);
                bmpNeg = this.DrawDoubleSpectrogram(bmpNeg, bmpBgn, "NEGATIVE");
                bmpNeg.Save(Path.Combine(opdir, opFileName + ".COLNEGBGN.png"));

                bmpPos = this.DrawDoubleSpectrogram(bmpPos, bmpBgn, "POSITIVE");
                bmpPos.Save(Path.Combine(opdir, opFileName + ".COLPOSBGN.png"));
            }
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
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, Y_interval);
            return bmp;
        }

        public Image DrawFalseColourSpectrogram(string colorMODE)
        {
            if (spectrogramMatrices.Count == 0)
            {
                Console.WriteLine("ERROR! ERROR! ERROR! - There are no indices with which to construct a false-colour spectrogram!");
                return null;
            }
            string[] rgbMap = this.ColorMap.Split('-');

            var redMatrix = NormaliseSpectrogramMatrix(rgbMap[0], spectrogramMatrices[rgbMap[0]], this.BackgroundFilter);
            var grnMatrix = NormaliseSpectrogramMatrix(rgbMap[1], spectrogramMatrices[rgbMap[1]], this.BackgroundFilter);
            var bluMatrix = NormaliseSpectrogramMatrix(rgbMap[2], spectrogramMatrices[rgbMap[2]], this.BackgroundFilter);
            bool doReverseColour = false;
            if (colorMODE.StartsWith("POS")) doReverseColour = true;

            Image bmp = ColourSpectrogram.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, Y_interval);
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


        public static Image FrameSpectrogram(Image bmp1, Image titleBar, int minOffset, int X_interval, int Y_interval)
        {
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp1, minOffset, X_interval, Y_interval);

            int imageWidth = bmp1.Width;
            int trackHeight = 20;

            int imageHt = bmp1.Height + trackHeight + trackHeight + trackHeight;
            int timeScale = 60; // assume 60 pixels per hour
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, minOffset, timeScale, imageWidth, trackHeight, "hours");

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

        public static Image DrawTitleBarOfGrayScaleSpectrogram(string title, int width, int height)
        {
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
            string text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
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
                cs.BackgroundFilter = 1.0; // 1.0 = no background filtering
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


        public static double[,] GetTStatisticMatrices(ColourSpectrogram cs1, ColourSpectrogram cs2, string key)
        {
            double[,] avg1 = cs1.spectrogramMatrices[key];
            if (key.Equals("TEN")) 
                avg1 = MatrixTools.SubtractValuesFromOne(avg1);

            double[,] std1 = cs1.spgr_StdDevMatrices[key];

            double[,] avg2 = cs2.spectrogramMatrices[key];
            if (key.Equals("TEN")) 
                avg2 = MatrixTools.SubtractValuesFromOne(avg2);

            double[,] std2 = cs2.spgr_StdDevMatrices[key];

            double[,] tStatMatrix = ColourSpectrogram.GetTStatisticMatrix(avg1, std1, cs1.N, avg2, std2, cs2.N);
            return tStatMatrix;
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
            double qtrTStat  = maxTStat / 4.0;
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
                    Dictionary<string, Color> colourChart = ColourSpectrogram.GetDifferenceColourChart();
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



        public static Image DrawTStatisticSpectrogram(ColourSpectrogram cs1, ColourSpectrogram cs2)
        {
            string[] keys = cs1.ColorMap.Split('-'); //assume both spectorgrams have the same acoustic indices in same order

            double[,] tStatA = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[0]);
            double[,] tStatB = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[1]);
            double[,] tStatC = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[2]);

            // assume all matricies are normalised and of the same dimensions
            int rows = tStatA.GetLength(0); //number of rows
            int cols = tStatA.GetLength(1); //number

            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            double maxTStat = 10.0;
            double halfTStat = maxTStat / 1.5;
            double qtrTStat = maxTStat / 2.0;
            double tStat;
            //int i1, i2, i3;
            //double av1a, av1b, av1c, av2a, av2b, av2c;
            //double sd1a, sd1b, sd1c, sd2a, sd2b, sd2c;
            double tA, tB, tC;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    //catch low values of dB used to avoid log of zero amplitude.
                    tA = tStatA[row, col];
                    tB = tStatB[row, col];
                    tC = tStatC[row, col];

                    tStat = (tA + tB + tC) / 3.000;
                    double tStatAbsolute = Math.Abs(tStat);
                    Dictionary<string, Color> colourChart = ColourSpectrogram.GetDifferenceColourChart();
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
                                    if (tStatAbsolute < 0.0) { colour = colourChart["NoValue"]; }
                                    else
                                    {
                                        colour = colourChart["+NotSig"];
                                    }
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
                                    if (tStatAbsolute < 0.0) { colour = colourChart["NoValue"]; }
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

                }//end all columns
            }//end all rows
            return bmp;
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
                    {   avg2 = expectedMinAvg;
                        std2 = expectedMinVar;
                    }

                    M[row, column] = Statistics.tStatistic(avg1, std1, N1, avg2, std2, N2);
                }//end all columns
            }//end all rows

            return M;
        }

        public static double[,] CreateTStatisticDifferenceMatrix(ColourSpectrogram cs1, ColourSpectrogram cs2, double[,] tStatMatrix, string key, double tStatThreshold)
        {
            double[,] m1 = cs1.GetNormalisedSpectrogramMatrix(key);
            double[,] m2 = cs2.GetNormalisedSpectrogramMatrix(key);

            // assume all matricies are of the same dimensions
            int rows = m1.GetLength(0); //number of rows
            int cols = m1.GetLength(1); //number
            var differenceM = new double[rows, cols];
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++) 
                {
                    if (Math.Abs(tStatMatrix[row, column]) >= tStatThreshold)
                        differenceM[row, column] = m1[row, column] - m2[row, column];
                }//end all columns
            }//end all rows
            return differenceM;
        }


        public static Image[] DrawTwoTStatisticSpectrograms(ColourSpectrogram cs1, ColourSpectrogram cs2, double tStatThreshold, double colourGain)
        {
            string[] keys = cs1.ColorMap.Split('-'); //assume both spectorgrams have the same acoustic indices in same order
            double[,] tStat1 = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[0]);
            double[,] tStat2 = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[1]);
            double[,] tStat3 = ColourSpectrogram.GetTStatisticMatrices(cs1, cs2, keys[2]);

            double[,] m1 = ColourSpectrogram.CreateTStatisticDifferenceMatrix(cs1, cs2, tStat1, keys[0], tStatThreshold);
            double[,] m2 = ColourSpectrogram.CreateTStatisticDifferenceMatrix(cs1, cs2, tStat2, keys[1], tStatThreshold);
            double[,] m3 = ColourSpectrogram.CreateTStatisticDifferenceMatrix(cs1, cs2, tStat3, keys[2], tStatThreshold);

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
                    if(d1 >= 0)
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
                        //i3pos = Convert.ToInt32(d3 * MaxRGBValue);
                        i3pos = Math.Max(0, value);
                        i3pos = Math.Min(MaxRGBValue, i3pos);
                    }
                    else
                    {
                        //i3neg = Math.Abs(Convert.ToInt32(d3 * MaxRGBValue));
                        i3neg = Math.Min(0, value);
                        i3neg = Math.Max(MaxRGBValue, i3neg);
                    }
                    bmp1.SetPixel(column, row, Color.FromArgb(i1pos, i2pos, i3pos));
                    bmp2.SetPixel(column, row, Color.FromArgb(i1neg, i2neg, i3neg));
                }//end all columns
            }//end all rows
            Image[] array = new Image[2];
            array[0] = bmp1;
            array[1] = bmp2;
            return array;
        }


        public static Image DrawTitleBarOfDifferenceSpectrogram(string name1, string name2, int width, int height)
        {
            Dictionary<string, Color> chart = ColourSpectrogram.GetDifferenceColourChart();
            Image colourChart = ImageTools.DrawColourChart(width, height, ColourSpectrogram.ColourChart2Array(chart));

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
        
        
        public static Image DrawTStatisticGreyscaleSpectrogramOfIndex(ColourSpectrogram cs1, ColourSpectrogram cs2, string key)
        {
            Image image1 = cs1.DrawGreyscaleSpectrogramOfIndex(key);
            Image image2 = cs2.DrawGreyscaleSpectrogramOfIndex(key);

            if ((image1 == null)||(image2 == null))
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
            Image titleBar = ColourSpectrogram.DrawTitleBarOfGrayScaleSpectrogram(title, image1.Width, titleHt);
            image1 = ColourSpectrogram.FrameSpectrogram(image1, titleBar, minOffset, cs1.X_interval, cs1.Y_interval);

            //frame image 2
            title = String.Format("{0} SPECTROGRAM for: {1}.      (scale:hours x kHz)", key, cs2.FileName);
            titleBar = ColourSpectrogram.DrawTitleBarOfGrayScaleSpectrogram(title, image2.Width, titleHt);
            image2 = ColourSpectrogram.FrameSpectrogram(image2, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            //get matrices required to calculate matrix of t-statistics
            double[,] avg1 = cs1.spectrogramMatrices[key];
            if (key.Equals("TEN")) avg1 = MatrixTools.SubtractValuesFromOne(avg1);
            double[,] std1 = cs1.spgr_StdDevMatrices[key];
            double[,] avg2 = cs2.spectrogramMatrices[key];
            if (key.Equals("TEN")) avg2 = MatrixTools.SubtractValuesFromOne(avg2);
            double[,] std2 = cs2.spgr_StdDevMatrices[key];

            double[,] tStatMatrix = ColourSpectrogram.GetTStatisticMatrix(avg1, std1, cs1.N, avg2, std2, cs2.N);
            //frame image 3
            //double tStatThreshold = 2.0; 
            //double colourGain = 1.0;
            Color[] colorArray = ColourSpectrogram.ColourChart2Array(ColourSpectrogram.GetDifferenceColourChart());
            title = String.Format("t-STATISTIC SPECTROGRAM ({0} - {1})      (scale:hours x kHz)       (colour: R-G-B={2})", cs1.FileName, cs2.FileName, cs1.ColorMODE);
            titleBar = ColourSpectrogram.DrawTitleBarOfDifferenceSpectrogram(cs1.FileName, cs2.FileName, colorArray, image1.Width, titleHt);
            Image image3 = ColourSpectrogram.DrawTStatisticSpectrogram(tStatMatrix);
            image3 = ColourSpectrogram.FrameSpectrogram(image3, titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            //Image image3 = ColourSpectrogram.DrawTStatisticSpectrogram(cs1, cs2, tStatThreshold, colourGain);

            //Image[] array = ColourSpectrogram.DrawTwoTStatisticSpectrograms(cs1, cs2, tStatThreshold, colourGain);
            //Image image3 = ColourSpectrogram.FrameSpectrogram(array[0], titleBar, minOffset, cs2.X_interval, cs2.Y_interval);
            //Image image4 = ColourSpectrogram.FrameSpectrogram(array[1], titleBar, minOffset, cs2.X_interval, cs2.Y_interval);

            Image[] opArray = new Image[3];
            opArray[0] = image1;
            opArray[1] = image2;
            opArray[2] = image3;
            //opArray[3] = image4;

            Image combinedImage = ImageTools.CombineImagesVertically(opArray);
            return combinedImage;
        }



        public static Image DrawDistanceSpectrogram(ColourSpectrogram cs1, ColourSpectrogram cs2)
        {
            string[] keys = cs1.ColorMap.Split('-');

            string key = keys[0];
            double[,] m1Red = cs1.GetNormalisedSpectrogramMatrix(key);
            var dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m1Red);
            cs1.SetIndexStatistics(key, dict);
            m1Red = MatrixTools.Matrix2ZScores(m1Red, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[1];
            double[,] m1Grn = cs1.GetNormalisedSpectrogramMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m1Grn);
            cs1.SetIndexStatistics(key, dict);
            m1Grn = MatrixTools.Matrix2ZScores(m1Grn, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[2];
            double[,] m1Blu = cs1.GetNormalisedSpectrogramMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m1Blu);
            cs1.SetIndexStatistics(key, dict);
            m1Blu = MatrixTools.Matrix2ZScores(m1Blu, dict["mode"], dict["sd"]);
            //Console.WriteLine("1.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[0];
            double[,] m2Red = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m2Red);
            cs2.SetIndexStatistics(key, dict);
            m2Red = MatrixTools.Matrix2ZScores(m2Red, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[1];
            double[,] m2Grn = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m2Grn);
            cs2.SetIndexStatistics(key, dict);
            m2Grn = MatrixTools.Matrix2ZScores(m2Grn, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);

            key = keys[2];
            double[,] m2Blu = cs2.GetNormalisedSpectrogramMatrix(key);
            dict = ColourSpectrogram.GetModeAndOneTailedStandardDeviation(m2Blu);
            cs2.SetIndexStatistics(key, dict);
            m2Blu = MatrixTools.Matrix2ZScores(m2Blu, dict["mode"], dict["sd"]);
            //Console.WriteLine("2.{0}: Min={1:f2}   Max={2:f2}    Mode={3:f2}+/-{4:f3} (SD=One-tailed)", key, dict["min"], dict["max"], dict["mode"], dict["sd"]);


            double[] v1 = new double[3];
            double[] mode1 = { cs1.indexStats[keys[0]]["mode"], cs1.indexStats[keys[1]]["mode"], cs1.indexStats[keys[2]]["mode"] };
            double[] stDv1 = { cs1.indexStats[keys[0]]["sd"],   cs1.indexStats[keys[1]]["sd"],   cs1.indexStats[keys[2]]["sd"]   };
            Console.WriteLine("1: avACI={0:f3}+/-{1:f3};   avTEN={2:f3}+/-{3:f3};   avCVR={4:f3}+/-{5:f3}", mode1[0], stDv1[0],  mode1[1], stDv1[1], mode1[2], stDv1[2]);

            double[] v2 = new double[3];
            double[] mode2 = { cs2.indexStats[keys[0]]["mode"], cs2.indexStats[keys[1]]["mode"], cs2.indexStats[keys[2]]["mode"] };
            double[] stDv2 = { cs2.indexStats[keys[0]]["sd"],   cs2.indexStats[keys[1]]["sd"],   cs2.indexStats[keys[2]]["sd"]   };
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
                    d11Matrix[row, col] = (v1[0] + v1[1] + v1[2]) / 3;
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



        public static Image FrameTwoSpectrograms(Image spg1, Image spg2, int X_interval, int Y_interval)
        {
            int width = spg1.Width; // assume all images have the same width
            int trackHeight = 20;
            //int spgHeight = spg1.Height; // assume all images have the same width

            Bitmap timeBmp = Image_Track.DrawTimeTrack(width, X_interval, width, trackHeight, "hours");
            int compositeHeight = spg1.Height + spg2.Height;

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

            gr.DrawImage(timeBmp, 0, yOffset); //draw in the bottom time scale
            //yOffset += timeBmp.Height;

            return (Image)compositeBmp;
        }
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
