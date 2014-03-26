using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

//using AnalysisBase;
//using log4net;

using TowseyLib;





namespace AudioAnalysisTools
{
    public class LDSpectrogramRGB
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
        public int SampleCount // used where the spectrograms are derived from averages and want to do t-test of difference.
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
        public LDSpectrogramRGB(int Xscale, int sampleRate, string colourMap)
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
        public LDSpectrogramRGB(int minuteOffset, int Xscale, int sampleRate, int frameWidth, string colourMap)
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
            return indexStats[key]; // used to return a dictionary of index statistics
        }
        public double GetIndexStatistics(string key, string stat)
        {
            return indexStats[key][stat]; // used to return index statistics
        }


        public void SetIndexStatistics(string key, Dictionary<string, double> dict)
        {
            indexStats.Add(key, dict); // add index statistics
        }

        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName)
        {
            string keys = "ACI-AVG-BGN-CVR-TEN-VAR";
            return ReadCSVFiles(ipdir, fileName, keys);
        }


        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName, string indexKeys)
        {
            bool allOK = true;
            string[] keys = indexKeys.Split('-');
            string warning = null;
            for (int key = 0; key < keys.Length; key++)
            {
                string path = Path.Combine(ipdir.FullName, fileName + "." + keys[key] + ".csv");
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = LDSpectrogramRGB.ReadSpectrogram(path, out freqBinCount);
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
                    allOK = false;
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
                allOK = false;
            }
            return allOK;
        }


        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string indexKeys, out int freqBinCount)
        {
            Dictionary<string, double[,]> dict = new Dictionary<string, double[,]>();
            string[] keys = indexKeys.Split('-');
            string warning = null;
            freqBinCount = 256; //the default
            for (int key = 0; key < keys.Length; key++)
            {
                string path = Path.Combine(ipdir.FullName, fileName + "." + keys[key] + ".csv");
                if (File.Exists(path))
                {
                    int binCount;
                    double[,] matrix = LDSpectrogramRGB.ReadSpectrogram(path, out binCount);
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

        public bool ReadStandardDeviationSpectrogramCSVs(DirectoryInfo ipdir, string fileName)
        {
            //string keys = "ACI-AVG-BGN-CVR-TEN-VAR";
            int freqBinCount;
            this.spgr_StdDevMatrices = ReadSpectrogramCSVFiles(ipdir, fileName, this.ColorMap, out freqBinCount);
            this.FrameWidth = freqBinCount * 2;
            bool allOK = true;
            if (this.spgr_StdDevMatrices == null) return false;
            if (this.spgr_StdDevMatrices.Count < 3) return false;
            return allOK;
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


        public double[,] GetSpectrogramMatrix(string key)
        {
            return this.spectrogramMatrices[key];
        }
        public double[,] GetStandarDeviationMatrix(string key)
        {
            return this.spgr_StdDevMatrices[key];
        }

        public int GetCountOfSpectrogramMatrices()
        {
            return this.spectrogramMatrices.Count;
        }
        public int GetCountOfStandardDeviationMatrices()
        {
            return this.spgr_StdDevMatrices.Count;
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
            if (this.spectrogramMatrices.ContainsKey(key))
            {
                return this.spectrogramMatrices[key];
            }
            else
            {
                Console.WriteLine("SpectrogramMatrices does not contain key {0}", key);
                return null;
            }
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
            if (this.spectrogramMatrices.ContainsKey(key))
            {
                return LDSpectrogramRGB.NormaliseSpectrogramMatrix(key, this.GetMatrix(key), this.BackgroundFilter);
            }
            else
            {
                Console.WriteLine("SpectrogramMatrices does not contain key {0}", key);
                return null;
            }

        }


        public void BlurSpectrogramMatrix(string key)
        {
            double[,] matrix = ImageTools.GaussianBlur_5cell(spectrogramMatrices[key]);
            spectrogramMatrices[key] = matrix;
        }

        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName)
        {
            DrawGreyScaleSpectrograms(opdir, opFileName, this.ColorMap);
        }

        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName, string keyString)
        {
            //string putativeIndices = "ACI-AVG-CVR-TEN-VAR-CMB-BGN";
            string warning = null;

            string[] keys = keyString.Split('-');
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    string path = Path.Combine(opdir.FullName, opFileName + "." + key + ".png");
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

            double[,] matrix = LDSpectrogramRGB.NormaliseSpectrogramMatrix(key, this.spectrogramMatrices[key], this.BackgroundFilter);
            Image bmp = ImageTools.DrawMatrixWithoutNormalisation(matrix);
            //ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, this.Y_interval);
            return bmp;
        }

        public void DrawFalseColourSpectrograms(DirectoryInfo opdir, string opFileName)
        {
            DrawNegativeFalseColourSpectrogram(opdir, opFileName);
            DrawPositiveFalseColourSpectrogram(opdir, opFileName);
        }

        public void DrawNegativeFalseColourSpectrogram(DirectoryInfo opdir, string opFileName)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            if (bmpNeg == null)
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawNegativeFalseColourSpectrograms()");
                Console.WriteLine("         Null image returned");
                return;
            } else {
                bmpNeg.Save(Path.Combine(opdir.FullName, opFileName + ".COLNEG.png"));
            }

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
                bmpNeg.Save(Path.Combine(opdir.FullName, opFileName + ".COLNEGBGN.png"));
            }
        }

        public void DrawNegativeFalseColourSpectrogram(DirectoryInfo opdir, string opFileName, string colorMap)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            if (bmpNeg == null)
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawNegativeFalseColourSpectrograms()");
                Console.WriteLine("         Null image returned");
                return;
            }
            else
            {
                bmpNeg.Save(Path.Combine(opdir.FullName, opFileName + "." + colorMap + ".png"));
            }
        }

        public void DrawPositiveFalseColourSpectrogram(DirectoryInfo opdir, string opFileName)
        {
            Image bmpPos = this.DrawFalseColourSpectrogram("POSITIVE");
            if (bmpPos == null)
            {
                Console.WriteLine("WARNING: From method ColourSpectrogram.DrawPositiveFalseColourSpectrograms()");
                Console.WriteLine("         Null image returned");
                return;
            }
            else
            {
                bmpPos.Save(Path.Combine(opdir.FullName, opFileName + ".COLNEG.png"));
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

            Image bmp = ImageTools.DrawMatrixWithoutNormalisation(combo);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, Y_interval);
            return bmp;
        }

        public Image DrawFalseColourSpectrogram(string colorMODE)
        {
            Image bmp = DrawFalseColourSpectrogram(colorMODE, this.ColorMap);
            return bmp;
        }

        public Image DrawFalseColourSpectrogram(string colorMODE, string colorMap)
        {
            if (! this.ContainsMatrixForKeys(colorMap))
            {
                return null;
            }

            string[] rgbMap = colorMap.Split('-');

            var redMatrix = NormaliseSpectrogramMatrix(rgbMap[0], spectrogramMatrices[rgbMap[0]], this.BackgroundFilter);
            var grnMatrix = NormaliseSpectrogramMatrix(rgbMap[1], spectrogramMatrices[rgbMap[1]], this.BackgroundFilter);
            var bluMatrix = NormaliseSpectrogramMatrix(rgbMap[2], spectrogramMatrices[rgbMap[2]], this.BackgroundFilter);
            bool doReverseColour = false;
            if (colorMODE.StartsWith("POS")) doReverseColour = true;

            Image bmp = LDSpectrogramRGB.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, Y_interval);
            return bmp;
        }

        public bool ContainsMatrixForKeys(string keys)
        {
            if (spectrogramMatrices.Count == 0)
            {
                Console.WriteLine("ERROR! ERROR! ERROR! - There are no indices with which to construct a spectrogram!");
                return false;
            }
            bool containsKey = true;
            string[] rgbMap = keys.Split('-');
            foreach (string key in rgbMap)
            {
                if(! this.ContainsMatrixForKey(key)) containsKey = false;
            }
            return containsKey;
        }


        public bool ContainsMatrixForKey(string key)
        {
            if (spectrogramMatrices.ContainsKey(key)) return true;
            else
            {
                Console.WriteLine("ERROR! - spectrogramMatrices does not contain key: <{0}> !", key);
                return false;
            }
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
            Image scale = SpectrogramConstants.DrawColourScale(maxScaleLength, trackHeight - 2);
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
                var cs = new LDSpectrogramRGB(xScale, sampleRate, colorMap);
                cs.ReadCSVFiles(new DirectoryInfo(ipdir), ipFileName);
                cs.BackgroundFilter = 1.0; // 1.0 = no background filtering
                cs.DrawGreyScaleSpectrograms(new DirectoryInfo(opdir), opFileName);
                cs.DrawFalseColourSpectrograms(new DirectoryInfo(opdir), opFileName);
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
            gr.DrawImage(timeBmp, 0, offset); //draw
            offset += titleBar.Height;
            gr.DrawImage(bmp1, 0, offset); //draw
            offset += bmp1.Height;
            gr.DrawImage(timeBmp, 0, offset); //draw
            return compositeBmp;
        }

        public static Image DrawTitleBarOfGrayScaleSpectrogram(string title, int width)
        {
            Bitmap bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
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

        public static Image DrawTitleBarOfFalseColourSpectrogram(string title, int width)
        {
            Image colourChart = SpectrogramConstants.DrawColourScale(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 2);

            Bitmap bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            int X = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(title, stringFont);
            X += (stringSize.ToSize().Width + 70);
            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            string text = String.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
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

        public static void BlurSpectrogram(LDSpectrogramRGB cs)
        {
            string[] rgbMap = cs.ColorMap.Split('-');

            cs.BlurSpectrogramMatrix(rgbMap[0]);
            cs.BlurSpectrogramMatrix(rgbMap[1]);
            cs.BlurSpectrogramMatrix(rgbMap[2]);
        }

        public static void BlurSpectrogram(LDSpectrogramRGB cs, string matrixKeys)
        {
            string[] keys = matrixKeys.Split('-');

            for (int k = 0; k < keys.Length; k++)
            {
                cs.BlurSpectrogramMatrix(keys[k]);
            }
        }


    } //ColourSpectrogram
}
