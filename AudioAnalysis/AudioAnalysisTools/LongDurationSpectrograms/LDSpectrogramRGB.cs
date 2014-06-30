// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramRGB.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   This class generates false-colour spectrograms of long duration audio recordings.
//   Important properties are:
//   1) the colour map which maps three acoutic indices to RGB.
//   2) The scale of the x and y axes which are dtermined by the sample rate, frame size etc.
//   In order to create false colour spectrograms, copy the method
//   public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
//   All the arguments can be passed through a config file.
//   Create the config file throu an instance of the class LDSpectrogramConfig
//   and then call config.WritConfigToYAML(FileInfo path).
//   Then pass that path to the above static method.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;

    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.Indices;

    using log4net;

    using TowseyLibrary;

    /// <summary>
    /// This class generates false-colour spectrograms of long duration audio recordings.
    /// Important properties are:
    /// 1) the colour map which maps three acoutic indices to RGB.
    /// 2) The scale of the x and y axes which are dtermined by the sample rate, frame size etc. 
    /// In order to create false colour spectrograms, copy the method 
    ///         public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
    /// All the arguments can be passed through a config file.
    /// Create the config file throu an instance of the class LDSpectrogramConfig
    /// and then call config.WritConfigToYAML(FileInfo path).
    /// Then pass that path to the above static method.
    /// </summary>
    public class LDSpectrogramRGB
    {
        public string FileName { get; set; }

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public LDSpectrogramRGB()
        {
            this.BackgroundFilter = 1.0; // default value = no filtering
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE; // default recording starts at midnight
            this.FrameWidth = SpectrogramConstants.FRAME_WIDTH; // default value - from which spectrogram was derived
            this.XInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default = one minute spectra and hourly time lines
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
        }

        public TimeSpan MinuteOffset { get; set; }

        public TimeSpan XInterval { get; set; }

        /// <summary>
        /// Gets or sets the frame width. Used only to calculate scale of Y-axis to draw grid lines.
        /// </summary>
        public int FrameWidth { get; set; }

        /// <summary>
        /// The sample rate.
        /// </summary>
        /// default value - after resampling
        public int SampleRate { get; set; }

        public int YInterval // mark 1 kHz intervals
        {
            get
            {
                double freqBinWidth = this.SampleRate / (double)this.FrameWidth;
                return (int)Math.Round(1000 / freqBinWidth);
            }
        }

        public double BackgroundFilter { get; set; }

        /// <summary>
        /// Gets or sets the ColorMap within current recording.
        /// </summary>
        public string ColorMap { get; set; } 
        
        /// <summary>
        /// POSITIVE or NEGATIVE
        /// </summary>
        public string ColorMode { get; set; }     

        private Dictionary<string, IndexProperties> spectralIndexProperties; 

        public string[] spectrogramKeys { get; private set; } 


        private Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>(); // used to save all spectrograms as dictionary of matrices 
        private Dictionary<string, double[,]> spgr_StdDevMatrices;                                       // used if reading standard devaition matrices for tTest

        public class SpectralStats
        {
            public double Minimum { get; set; }

            public double Maximum { get; set; }

            public double Mode { get; set; }

            public double StandardDeviation { get; set; }
        }

        // used to save mode and sd of the indices 
        private readonly Dictionary<string, SpectralStats> indexStats = new Dictionary<string, SpectralStats>();

        public Dictionary<string, SpectralStats> IndexStats
        {
            get
            {
                return indexStats;
            }
        }

        /// <summary>
        /// used where the spectrograms are derived from averages and want to do t-test of difference.
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// CONSTRUCTOR
        /// </summary>
        /// <param name="Xscale"></param>
        /// <param name="sampleRate"></param>
        /// <param name="colourMap"></param>
        public LDSpectrogramRGB(TimeSpan Xscale, int sampleRate, string colourMap)
        {
            this.BackgroundFilter = 1.0;
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE;
            this.FrameWidth = SpectrogramConstants.FRAME_WIDTH;
            this.XInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            this.MinuteOffset = SpectrogramConstants.MINUTE_OFFSET;
            // set the X and Y axis scales for the spectrograms 
            this.XInterval = Xscale; 
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
        public LDSpectrogramRGB(TimeSpan minuteOffset, TimeSpan Xscale, int sampleRate, int frameWidth, string colourMap) : this(Xscale, sampleRate, colourMap)
        {
            this.MinuteOffset = minuteOffset;
            this.FrameWidth = frameWidth;
        }

        public Dictionary<string, IndexProperties> GetSpectralIndexProperties()
        {
            return this.spectralIndexProperties;
        }



        public void SetSpectralIndexProperties(Dictionary<string, IndexProperties> _spectralIndexProperties)
        {
            this.spectralIndexProperties = _spectralIndexProperties;
            this.spectrogramKeys = this.spectralIndexProperties.Keys.ToArray();
        }



        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName)
        {            
            return this.ReadCSVFiles(ipdir, fileName, this.spectrogramKeys);
        }


        public bool ReadCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            bool allOk = true;
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

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], path);
                    allOk = false;
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (this.spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method ColourSpectrogram.ReadCSVFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
                allOk = false;
            }

            return allOk;
        }


        public static Dictionary<string, double[,]> ReadSpectrogramCSVFiles(DirectoryInfo ipdir, string fileName, string indexKeys, out int freqBinCount)
        {
            Dictionary<string, double[,]> dict = new Dictionary<string, double[,]>();
            string[] keys = indexKeys.Split('-');
            string warning = null;
            freqBinCount = 256; // the default
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

                    warning += string.Format("\n      {0} File does not exist: {1}", keys[key], path);
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (dict.Count != 0)
            {
                return dict;
            }

            LoggedConsole.WriteLine("WARNING: from method ColourSpectrogram.ReadSpectrogramCSVFiles()");
            LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);

            return dict;
        }

        public bool ReadStandardDeviationSpectrogramCSVs(DirectoryInfo ipdir, string fileName)
        {
            //string keys = "ACI-AVG-BGN-CVR-TEN-VAR";
            int freqBinCount;
            this.spgr_StdDevMatrices = ReadSpectrogramCSVFiles(ipdir, fileName, this.ColorMap, out freqBinCount);
            this.FrameWidth = freqBinCount * 2;
            if (this.spgr_StdDevMatrices == null)
            {
                return false;
            }

            if (this.spgr_StdDevMatrices.Count < 3)
            {
                return false;
            }

            return true;
        }



        public static double[,] ReadSpectrogram(string csvPath, out int binCount)
        {
            // MICHAEL: the new Csv class can read this in, and optionally transpose as it reads
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


        public void CalculateStatisticsForAllIndices()
        {
            double[,] matrix;

            foreach (string key in this.spectrogramKeys)
            {
                if(this.spectrogramMatrices.ContainsKey(key)) 
                {
                    matrix = this.spectrogramMatrices[key];
                    SpectralStats stats = LDSpectrogramRGB.GetModeAndOneTailedStandardDeviation(matrix);
                    this.indexStats.Add(key, stats); // add index statistics
                }
            }
        }

        /* public List<string> WriteStatisticsForAllIndices()
        {
           List<string> lines = new List<string>();
            foreach (string key in this.spectrogramKeys)
            {
                if (this.spectrogramMatrices.ContainsKey(key))
                {
                    string outString = "STATS for " + key + ":   ";
                    Dictionary<string, double> stats = this.GetIndexStatistics(key);
                    foreach (string stat in stats.Keys)
                    {
                        outString = string.Format("{0}  {1}={2:f3} ", outString, stat, stats[stat]);
                    }
                    lines.Add(outString);
                }
            }
            return lines;

        }*/

        public void DrawIndexDistributionsAndSave(string imagePath)
        {
            int width = 100;  // pixels 
            int height = 100; // pixels
            var list = new List<Image>();
            foreach (string key in this.spectrogramMatrices.Keys)
            {
                var stats = this.indexStats[key];
                int[] histogram = Histogram.Histo(this.spectrogramMatrices[key], width);
                list.Add(
                    ImageTools.DrawHistogram(
                        key,
                        histogram,
                        new Dictionary<string, double>()
                            {
                                { "min", stats.Minimum },
                                { "max", stats.Maximum },
                                { "mode", stats.Mode },
                                { "sd", stats.StandardDeviation },
                            },
                        width,
                        height));
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
            this.spectrogramMatrices.Add(key, matrix);
        }

        public void AddRotatedSpectrogram(string key, double[,] matrix)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            this.spectrogramMatrices.Add(key, matrix);
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
                LoggedConsole.WriteLine("SpectrogramMatrices does not contain key {0}", key);
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
                LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                LoggedConsole.WriteLine("Dictionary of Spectral Properties does not contain key {0}", key);
                return null;
            }
            if (!this.spectrogramMatrices.ContainsKey(key))
            {
                LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                LoggedConsole.WriteLine("Dictionary of Spectrogram Matrices does not contain key {0}", key);
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
            this.DrawGreyScaleSpectrograms(opdir, opFileName, this.spectrogramKeys);
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
                    LoggedConsole.WriteLine("\n\nWARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteLine("         Dictionary of spectrogram matrices does NOT contain key: {0}", key);
                    List<string> keyList = new List<string>(this.spectrogramMatrices.Keys);
                    string list = "";
                    foreach (string str in keyList)
                    {
                        list += str + ", ";
                    }
                    LoggedConsole.WriteLine("          List of keys in dictionary = {0}", list);
                    continue;
                }
                if (this.spectrogramMatrices[key] == null)
                {
                    LoggedConsole.WriteLine("WARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteLine("         Null matrix returned with key: {0}", key);
                    continue;
                }

                string path = Path.Combine(opdir.FullName, opFileName + "." + key + ".png");
                Image bmp = this.DrawGreyscaleSpectrogramOfIndex(key);
                if (bmp != null)
                {
                    bmp.Save(path);
                }
            }
        }

        /// <summary>
        /// Assume calling method has done all the reality checks
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Image DrawGreyscaleSpectrogramOfIndex(string key)
        {
            double[,] matrix = this.GetNormalisedSpectrogramMatrix(key);
            if (matrix == null)
            {
                return null;
            }

            Image bmp = ImageTools.DrawMatrixWithoutNormalisation(matrix);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, this.MinuteOffset, this.XInterval, xAxisPixelDuration, this.YInterval);
            return bmp;
        }

        public void DrawFalseColourSpectrograms(DirectoryInfo outputDirectory, string outputFileName)
        {
            this.DrawNegativeFalseColourSpectrogram(outputDirectory, outputFileName);
            this.DrawPositiveFalseColourSpectrogram(outputDirectory, outputFileName);
        }

        public void DrawNegativeFalseColourSpectrogram(DirectoryInfo outputDirectory, string outputFileName)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            if (bmpNeg == null)
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawNegativeFalseColourSpectrograms()");
                LoggedConsole.WriteLine("         Null image returned");
                return;
            } 
            else 
            {
                bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + ".COLNEG.png"));
            }

            Image bmpBgn;
            string key = InitialiseIndexProperties.KEYspectralBGN;
            if (!this.spectrogramMatrices.ContainsKey(key))
            {
                LoggedConsole.WriteLine("\nWARNING: SG {0} does not contain key: {1}", outputFileName, key);
                //return;
            }
            else
            {
                bmpBgn = this.DrawGreyscaleSpectrogramOfIndex(key);
                bmpNeg = this.DrawDoubleSpectrogram(bmpNeg, bmpBgn, "NEGATIVE");
                bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + ".COLNEGBGN.png"));
            }
        }

        public void DrawNegativeFalseColourSpectrogram(DirectoryInfo outputDirectory, string outputFileName, string colorMap)
        {
            Image bmpNeg = this.DrawFalseColourSpectrogram("NEGATIVE");
            if (bmpNeg == null)
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawNegativeFalseColourSpectrograms()");
                LoggedConsole.WriteLine("         Null image returned");
                return;
            }
            else
            {
                bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + "." + colorMap + ".png"));
            }
        }

        public void DrawPositiveFalseColourSpectrogram(DirectoryInfo opdir, string opFileName)
        {
            Image bmpPos = this.DrawFalseColourSpectrogram("POSITIVE");
            if (bmpPos == null)
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawPositiveFalseColourSpectrograms()");
                LoggedConsole.WriteLine("         Null image returned");
                return;
            }
            else
            {
                bmpPos.Save(Path.Combine(opdir.FullName, opFileName + ".COLNEG.png"));
            }
        }

        public void BlurSpectrogramMatrix(string key)
        {
            double[,] matrix = ImageTools.GaussianBlur_5cell(this.spectrogramMatrices[key]);
            this.spectrogramMatrices[key] = matrix;
        }

        public Image DrawFalseColourSpectrogram(string colorMODE)
        {
            Image bmp = this.DrawFalseColourSpectrogram(colorMODE, this.ColorMap);
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
            bool doReverseColour = colorMODE.StartsWith("POS");

            Image bmp = LDSpectrogramRGB.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp, this.MinuteOffset, this.XInterval, xAxisPixelDuration, this.YInterval);
            return bmp;
        }

        public bool ContainsMatrixForKeys(string keys)
        {
            if (this.spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("ERROR! ERROR! ERROR! - There are no indices with which to construct a spectrogram!");
                return false;
            }

            bool containsKey = true;
            string[] rgbMap = keys.Split('-');
            foreach (string key in rgbMap)
            {
                if (!this.ContainsMatrixForKey(key))
                {
                    containsKey = false;
                }
            }

            return containsKey;
        }


        public bool ContainsMatrixForKey(string key)
        {
            if (this.spectrogramMatrices.ContainsKey(key))
            {
                return true;
            }
            else
            {
                LoggedConsole.WriteLine("ERROR! - spectrogramMatrices does not contain key: <{0}> !", key);
                return false;
            }
        }



        public Image DrawDoubleSpectrogram(Image bmp1, Image bmp2, string colorMODE)
        {
            int imageWidth = bmp2.Width;
            const int TrackHeight = 20;
            int imageHt = bmp2.Height + bmp1.Height + TrackHeight + TrackHeight + TrackHeight;
            string title = string.Format("FALSE COLOUR and BACKGROUND NOISE SPECTROGRAMS      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", this.ColorMap);
            Bitmap titleBmp = Image_Track.DrawTitleTrack(imageWidth, TrackHeight, title);
            TimeSpan timeScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, timeScale, imageWidth, TrackHeight, "hours");

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

            // draw a colour spectrum of basic colours
            int maxScaleLength = imageWidth / 3;
            Image scale = LDSpectrogramRGB.DrawColourScale(maxScaleLength, TrackHeight - 2);
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
        }


        public static SpectralStats GetModeAndOneTailedStandardDeviation(double[,] M)
        {
            double[] values = DataTools.Matrix2Array(M);
            const bool DisplayHistogram = false;
            double min, max, mode, SD;
            DataTools.GetModeAndOneTailedStandardDeviation(values, DisplayHistogram, out min, out max, out mode, out SD);

            return new SpectralStats()
                       {
                           Minimum = min,
                           Maximum = max,
                           Mode = mode,
                           StandardDeviation = SD
                       };
        }

        //========================================================================================================================================================
        //========= NEXT FEW METHODS ARE STATIC AND RETURN VARIOUS KINDS OF IMAGE
        //========================================================================================================================================================

        public static Image FrameSpectrogram(Image bmp1, Image titleBar, TimeSpan minOffset, TimeSpan X_interval, int Y_interval)
        {
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            ImageTools.DrawGridLinesOnImage((Bitmap)bmp1, minOffset, X_interval, xAxisPixelDuration, Y_interval);

            int imageWidth = bmp1.Width;
            int trackHeight = 20;

            int imageHt = bmp1.Height + trackHeight + trackHeight + trackHeight;
            TimeSpan xAxisTicInterval = TimeSpan.FromMinutes(60); // assume 60 pixels per hour
            Bitmap timeBmp = Image_Track.DrawTimeTrack(imageWidth, minOffset, xAxisTicInterval, imageWidth, trackHeight, "hours");

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
            string text = string.Format("(c) QUT.EDU.AU");
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

            string text = string.Format("(c) QUT.EDU.AU");
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
                // note that the matrix values are multiplied by the grey matrix values.
                for (int column = 0; column < cols; column++) 
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
                }
            }

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
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        ///  IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. 
        /// </summary>
        /// <param name="longDurationSpectrogramConfig">
        /// </param>
        /// <param name="indicesConfigPath">
        /// The indices Config Path.
        /// </param>
        /// <param name="spectra">
        /// Optional spectra to pass in. If specified the spectra will not be loaded from disk!
        /// </param>
        public static void DrawSpectrogramsFromSpectralIndices(LdSpectrogramConfig longDurationSpectrogramConfig, FileInfo indicesConfigPath, Dictionary<string, double[,]> spectra = null)
        {
            LdSpectrogramConfig configuration = longDurationSpectrogramConfig;

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            string fileStem = configuration.FileName;
            DirectoryInfo outputDirectory = configuration.OutputDirectory;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = configuration.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
            string colorMap2 = configuration.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB

            double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            ////double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            TimeSpan minuteOffset = configuration.MinuteOffset;   // default = zero minute of day i.e. midnight
            TimeSpan xScale = configuration.XAxisTicInterval; // default is one minute spectra i.e. 60 per hour
            int sampleRate = configuration.SampleRate;
            int frameWidth = configuration.FrameWidth;

            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap1);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

            if (spectra == null)
            {
                // reads all known files spectral indices
                Logger.Info("Reading spectra files from disk");
                cs1.ReadCSVFiles(configuration.InputDirectory, fileStem);
            }
            else
            {
                // TODO: not sure if this works
                Logger.Info("Spectra loaded from memory");
                cs1.LoadSpectrogramDictionary(spectra);
            }

            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }

            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem);

            cs1.CalculateStatisticsForAllIndices();
            Json.Serialise(Path.Combine(outputDirectory.FullName, fileStem + ".IndexStatistics.json").ToFileInfo(), cs1.indexStats);


            cs1.DrawIndexDistributionsAndSave(Path.Combine(outputDirectory.FullName, fileStem + ".IndexDistributions.png"));

            string colorMap = colorMap1;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            string title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.XInterval, cs1.YInterval);
            image1.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap + ".png"));

            //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
            colorMap = colorMap2;
            Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs1.XInterval, cs1.YInterval);
            image2.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap + ".png"));
            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image image3 = ImageTools.CombineImagesVertically(array);
            image3.Save(Path.Combine(outputDirectory.FullName, fileStem + ".2MAPS.png"));
        }
    }
}
