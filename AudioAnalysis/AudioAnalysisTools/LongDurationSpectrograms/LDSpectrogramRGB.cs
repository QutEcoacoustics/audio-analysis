using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

//using AnalysisBase;
//using log4net;
//using AnalysisPrograms.Production;
using Acoustics.Shared;
using TowseyLibrary;
using AudioAnalysisTools.Indices;





namespace AudioAnalysisTools
{

    /// <summary>
    /// This class generates false-colour spectrograms of long duration audio recordings.
    /// Important properties are:
    /// 1) the colour map which maps three acoutic indices to RGB.
    /// 2) The scale of the x and y axes which are dtermined by the sample rate, frame size etc.
    /// 
    /// In order to create false colour spectrograms, copy the method 
    ///         public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
    /// All the arguments can be passed through a config file.
    /// Create the config file throu an instance of the class LDSpectrogramConfig
    /// and then call config.WritConfigToYAML(FileInfo path).
    /// Then pass that path to the above static method.
    /// </summary>
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
        private int frameWidth = SpectrogramConstants.FRAME_WIDTH;   // default value - from which spectrogram was derived
        public int FrameWidth           // used only to calculate scale of Y-axis to draw grid lines
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }
        private int sampleRate = SpectrogramConstants.SAMPLE_RATE; // default value - after resampling
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

        private Dictionary<string, IndexProperties> spectralIndexProperties; 

        public string[] spectrogramKeys { get; private set; } 


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
        public LDSpectrogramRGB(int minuteOffset, int Xscale, int sampleRate, int frameWidth, string colourMap): this(Xscale, sampleRate, colourMap)
        {
            this.minOffset = minuteOffset;
            this.FrameWidth = frameWidth;
        }

        public Dictionary<string, IndexProperties> GetSpectralIndexProperties()
        {
            return spectralIndexProperties;
        }



        public void SetSpectralIndexProperties(Dictionary<string, IndexProperties> _spectralIndexProperties)
        {
            spectralIndexProperties = _spectralIndexProperties;
            spectrogramKeys = spectralIndexProperties.Keys.ToArray();
        }



        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName)
        {            
            return ReadCSVFiles(ipdir, fileName, this.spectrogramKeys);
        }


        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            bool allOK = true;
            string warning = null;
            for (int i = 0; i < keys.Length; i++)
            {
                string path = Path.Combine(ipdir.FullName, fileName + "." + keys[i] + ".csv");
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = LDSpectrogramRGB.ReadSpectrogram(path, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    this.spectrogramMatrices.Add(this.spectrogramKeys[i], matrix);
                    this.FrameWidth = freqBinCount * 2;

                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method ColourSpectrogram.ReadCSVFiles()";
                    }
                    warning += String.Format("\n      {0} File does not exist: {1}", keys[i], path);
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


        public void CalculateStatisticsForAllIndices()
        {
            double[,] matrix;

            foreach (string key in this.spectrogramKeys)
            {
                if(this.spectrogramMatrices.ContainsKey(key)) 
                {
                    matrix = this.spectrogramMatrices[key];
                    var dict = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(matrix);
                    indexStats.Add(key, dict); // add index statistics
                }
            }
        }

        public List<string> WriteStatisticsForAllIndices()
        {
            List<string> lines = new List<string>();
            foreach (string key in this.spectrogramKeys)
            {
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    string outString = "STATS for "+key+":   ";
                    Dictionary<string, double> stats = this.GetIndexStatistics(key);
                    foreach (string stat in stats.Keys)
                    {
                        outString = String.Format("{0}  {1}={2:f3} ", outString, stat, stats[stat]);
                    }
                    lines.Add(outString);
                }
            }
            return lines;
        }

        public void DrawIndexDistributionsAndSave(string imagePath)
        {
            int width = 100;  //pixels 
            int height = 100; // pixels
            var list = new List<Image>();
            foreach (string key in this.spectrogramMatrices.Keys)
            {
                Dictionary<string, double> stats = this.indexStats[key];
                int[] histogram = Histogram.Histo(this.spectrogramMatrices[key], width);
                list.Add(ImageTools.DrawHistogram(key, histogram, stats, width, height));
            }
            Image image3 = ImageTools.CombineImagesVertically(list.ToArray());
            image3.Save(imagePath);
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
        /// In addition, small background values are reduced as per filter coefficient. 1.0 = unchanged. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="backgroundFilterCoeff"></param>
        /// <returns></returns>
        public double[,] GetNormalisedSpectrogramMatrix(string key)
        {
            if (! this.spectralIndexProperties.ContainsKey(key))
            {
                Console.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                Console.WriteLine("Dictionary of Spectral Properties does not contain key {0}", key);
                return null;
            }
            if (!this.spectrogramMatrices.ContainsKey(key))
            {
                Console.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                Console.WriteLine("Dictionary of Spectrogram Matrices does not contain key {0}", key);
                return null;
            }

            IndexProperties indexProperties = this.spectralIndexProperties[key];
            var matrix = indexProperties.NormaliseIndexValues(this.GetMatrix(key));
            return MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
        }


        /// <summary>
        /// Draws all available spectrograms in grey scale 
        /// </summary>
        /// <param name="opdir"></param>
        /// <param name="opFileName"></param>
        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName)
        {
            DrawGreyScaleSpectrograms(opdir, opFileName, this.spectrogramKeys);
        }

        /// <summary>
        /// draws only those spectrograms in the passed array of keys
        /// </summary>
        /// <param name="opdir"></param>
        /// <param name="opFileName"></param>
        /// <param name="keys"></param>
        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName, string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];

                if (!this.spectrogramMatrices.ContainsKey(key))
                {
                    Console.WriteLine("\n\nWARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    Console.WriteLine("         Dictionary of spectrogram matrices does NOT contain key: {0}", key);
                    List<string> keyList = new List<string>(this.spectrogramMatrices.Keys);
                    string list = "";
                    foreach (string str in keyList)
                    {
                        list += (str + ", ");
                    }
                    Console.WriteLine("          List of keys in dictionary = {0}", list);
                    continue;
                }
                if (this.spectrogramMatrices[key] == null)
                {
                    Console.WriteLine("WARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    Console.WriteLine("         Null matrix returned with key: {0}", key);
                    continue;
                }

                string path = Path.Combine(opdir.FullName, opFileName + "." + key + ".png");
                Image bmp = this.DrawGreyscaleSpectrogramOfIndex(key);
                if (bmp != null) bmp.Save(path);
            } // end for loop
        }

        /// <summary>
        /// Assume calling method has done all the reality checks
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Image DrawGreyscaleSpectrogramOfIndex(string key)
        {
            double[,] matrix = this.GetNormalisedSpectrogramMatrix(key);
            if(matrix == null) return null;
            Image bmp = ImageTools.DrawMatrixWithoutNormalisation(matrix);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, minOffset, X_interval, this.Y_interval);
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
            string key = InitialiseIndexProperties.spKEY_BkGround;
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

        public void BlurSpectrogramMatrix(string key)
        {
            double[,] matrix = ImageTools.GaussianBlur_5cell(spectrogramMatrices[key]);
            spectrogramMatrices[key] = matrix;
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

            //var indexProperties = this.spProperties[rgbMap[0]];
            var redMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[0]);
            var grnMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[1]);
            var bluMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[2]);
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
            Image scale = LDSpectrogramRGB.DrawColourScale(maxScaleLength, trackHeight - 2);
            int xLocation = imageWidth * 2 / 3;
            gr.DrawImage(scale, xLocation, 1); //dra
            return compositeBmp;
        }


        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################


        public static double[,] NormaliseSpectrogramMatrix(IndexProperties indexProperties, double[,] matrix, double backgroundFilterCoeff)
        {
            matrix = indexProperties.NormaliseIndexValues(matrix);

            matrix = MatrixTools.FilterBackgroundValues(matrix, backgroundFilterCoeff); // to de-demphasize the background small values
            return matrix;
        } //NormaliseSpectrogramMatrix()


        public static Dictionary<string, double> GetModeAndOneTailedStandardDeviation(double[,] M)
        {
            double[] values = DataTools.Matrix2Array(M);
            bool displayHistogram = false;
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, displayHistogram, out min, out max, out mode, out SD);
            var dict = new Dictionary<string, double>();
            dict["min"] = min;
            dict["max"] = max;
            dict["mode"] = mode;
            dict["sd"] = SD;
            return dict;
        }

        //========================================================================================================================================================
        //========= NEXT FEW METHODS ARE STATIC AND RETURN VARIOUS KINDS OF IMAGE
        //========================================================================================================================================================

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
            Image colourChart = LDSpectrogramRGB.DrawColourScale(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 2);

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

        /// <summary>
        /// Returns an image of an array of colour patches.
        /// It shows the three primary colours and pairwise combinations.
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


        //========================================================================================================================================================
        //========= DrawFalseColourSpectrograms() IS THE MAJOR METHOD FOR CREATING LD SPECTROGRAMS ===============================================================
        //========= IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. ===============================================================
        //========================================================================================================================================================

        /// <summary>
        ///  This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        ///  IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. 
        /// </summary>
        /// <param name="configuration"></param>
        public static void DrawFalseColourSpectrograms(FileInfo configPath )
        {
            //LDSpectrogramConfig configuration = Yaml.Deserialise<LDSpectrogramConfig>(configPath);

            LDSpectrogramConfig configuration = LDSpectrogramConfig.ReadYAMLToConfig(configPath);

            //string ipdir = configuration.InputDirectory.FullName;
            //DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            string fileStem = configuration.FileName;
            //string opdir = configuration.OutputDirectory.FullName;
            DirectoryInfo opDir = configuration.OutputDirectory;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColourMap;
            string colorMap = map != null ? map : SpectrogramConstants.RGBMap_ACI_ENT_CVR;   // assigns indices to RGB

            double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            int minuteOffset = (int?)configuration.MinuteOffset ?? SpectrogramConstants.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            int xScale = (int?)configuration.X_interval ?? SpectrogramConstants.X_AXIS_SCALE; // default is one minute spectra i.e. 60 per hour
            int sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstants.SAMPLE_RATE;
            int frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstants.FRAME_WIDTH;

            
            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            var sip = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties();
            cs1.SetSpectralIndexProperties(sip); // set the relevant dictionary of index properties
            cs1.ReadCSVFiles(configuration.InputDirectory, fileStem); // reads all known files spectral indices
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Console.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }

            cs1.DrawGreyScaleSpectrograms(opDir, fileStem);

            cs1.CalculateStatisticsForAllIndices();
            List<string> lines = cs1.WriteStatisticsForAllIndices();
            FileTools.WriteTextFile(Path.Combine(opDir.FullName, fileStem + ".IndexStatistics.txt"), lines);

            cs1.DrawIndexDistributionsAndSave(Path.Combine(opDir.FullName, fileStem + ".IndexDistributions.png"));

            colorMap = SpectrogramConstants.RGBMap_BGN_AVG_CVR;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image1.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));

            //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
            colorMap = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
            Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image2.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));
            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image image3 = ImageTools.CombineImagesVertically(array);
            image3.Save(Path.Combine(opDir.FullName, fileStem + ".2MAPS.png"));
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
                string colorMap = SpectrogramConstants.RGBMap_ACI_ENT_EVN; //CHANGE RGB mapping here.
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





    } //LDSpectrogramRGB



    //========================================================================================================================================================
    //========= CONFIG CLASS FOR LD SPECTROGRAMS ====================================================================================================================
    //========================================================================================================================================================

    /// <summary>
    /// CONFIG CLASS FOR the class LDSpectrogramConfig
    /// </summary>
    public class LDSpectrogramConfig
    {
        private string fileName;  // File name from which spectrogram was derived.
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private DirectoryInfo ipDir;
        public DirectoryInfo InputDirectory
        {
            get { return ipDir; }
            set { ipDir = value; }
        }
        private DirectoryInfo opDir;
        public DirectoryInfo OutputDirectory
        {
            get { return opDir; }
            set { opDir = value; }
        }

        //these parameters manipulate the colour map and appearance of the false-colour spectrogram
        private string colourmap = SpectrogramConstants.RGBMap_ACI_ENT_SPT;  // CHANGE default RGB mapping here.
        public string ColourMap
        {
            get { return colourmap; }
            set { colourmap = value; }
        }
        private double backgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // must be value <=1.0
        public double BackgroundFilterCoeff 
        {
            get { return backgroundFilter; }
            set { backgroundFilter = value; }
        }

        // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
        private int minOffset = SpectrogramConstants.MINUTE_OFFSET;    // default recording starts at zero minute of day i.e. midnight
        public int MinuteOffset
        {
            get { return minOffset; }
            set { minOffset = value; }
        }
        private int x_interval = SpectrogramConstants.X_AXIS_SCALE;    // assume one minute spectra and hourly time lines
        public int X_interval
        {
            get { return x_interval; }
            set { x_interval = value; }
        }
        private int frameWidth = SpectrogramConstants.FRAME_WIDTH; // default value for frame width from which spectrogram was derived. Assume no frame overlap.
        public int FrameWidth           // used only to calculate scale of Y-axis to draw grid lines
        {
            get { return frameWidth; }
            set { frameWidth = value; }
        }
        private int sampleRate = SpectrogramConstants.SAMPLE_RATE; // default value - after resampling
        public int SampleRate
        {
            get { return sampleRate; }
            set { sampleRate = value; }
        }
        private int hz_grid_interval = 1000;
        public int Y_interval // mark 1 kHz intervals
        {
            get
            {
                double freqBinWidth = sampleRate / (double)frameWidth;
                return (int)Math.Round(hz_grid_interval / freqBinWidth);
            }
        }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="_fileName"></param>
        /// <param name="_ipDir"></param>
        /// <param name="_opDir"></param>
        public LDSpectrogramConfig(string _fileName, DirectoryInfo _ipDir, DirectoryInfo _opDir)
        {
            fileName = _fileName;
            ipDir = _ipDir;
            opDir = _opDir;
        }

        /// <summary>
        /// READS A YAML CONFIG FILE into a dynamic variable and then transfers all values into the appropriate config class
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LDSpectrogramConfig ReadYAMLToConfig(FileInfo path)
        {
            // load YAML configuration
            dynamic configuration = Yaml.Deserialise(path);
            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside this method. 
             * Extract all params below.
             */

            DirectoryInfo ipDir = new DirectoryInfo((string)configuration.InputDirectory);
            DirectoryInfo opDir = new DirectoryInfo((string)configuration.OutputDirectory);

            LDSpectrogramConfig config = new LDSpectrogramConfig((string)configuration.FileName, ipDir, opDir);

            //these parameters manipulate the colour map and appearance of the false-colour spectrogram
            config.ColourMap = (string)configuration.ColourMap;
            config.BackgroundFilterCoeff = (double)configuration.BackgroundFilterCoeff; // must be value <=1.0

            // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
            config.SampleRate = (int)configuration.SampleRate;
            config.FrameWidth = (int)configuration.FrameWidth;       // frame width from which spectrogram was derived. Assume no frame overlap.
            config.MinuteOffset = (int)configuration.MinuteOffset;   // default is recording starts at zero minute of day i.e. midnight
            config.X_interval = (int)configuration.X_interval;       // default is one minute spectra and hourly time lines

            return config;            
        } // ReadYAMLToConfig()

        public void WritConfigToYAML(FileInfo path)
        {
            // WRITE THE YAML CONFIG FILE
            Yaml.Serialise(path, new
            {
                //paths to required directories and files
                FileName = this.FileName,
                InputDirectory = this.InputDirectory.FullName,
                OutputDirectory = this.OutputDirectory.FullName,

                //these parameters manipulate the colour map and appearance of the false-colour spectrogram
                ColorMap = this.ColourMap,
                BackgroundFilterCoeff = this.BackgroundFilterCoeff, // must be value <=1.0

                // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
                SampleRate = this.SampleRate,    
                FrameWidth = this.FrameWidth,       // frame width from which spectrogram was derived. Assume no frame overlap.
                MinuteOffset = this.MinuteOffset,   // default is recording starts at zero minute of day i.e. midnight
                X_interval = this.X_interval        // default is one minute spectra and hourly time lines
            });
        } // WritConfigToYAML()

    } // class LDSpectrogramConfig

}
