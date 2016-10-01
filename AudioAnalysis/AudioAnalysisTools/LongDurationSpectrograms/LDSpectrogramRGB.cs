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
//
//
//  Activity Codes for other tasks to do with spectrograms and audio files:
/// 
/// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
/// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
/// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
/// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
/// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
/// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
///
/// audiofilecheck - Writes information about audio files to a csv file.
/// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
/// audiocutter - Cuts audio into segments of desired length and format
/// createfoursonograms 
///
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;

    using AnalysisBase.ResultBases;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;

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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string FileName { get; set; }

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public LDSpectrogramRGB()
        {
            this.BackgroundFilter = 1.0; // default value = no filtering
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE; // default recording starts at midnight
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH; // default value - from which spectrogram was derived
            this.XTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default = one minute spectra and hourly time lines
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;
        }



        /// <summary>
        /// The date and time at which the current LDspectrogram starts
        /// </summary>
        public DateTimeOffset RecordingStartDate { get; set; }

        public string SiteName{ get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>
        /// The time at which the current LDspectrogram starts.
        /// </summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>
        /// The temporal duration of one subsegment interval for which indices are calculated
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        public TimeSpan XTicInterval { get; set; }

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

        public string[] spectrogramKeys { get; private set; }

        // used to save all spectrograms as dictionary of matrices 
        // IMPORTANT: The matrices are stored as they would appear in the LD spectrogram image. i.e. rotated 90 degrees anti-clockwise.
        public Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
        // used if reading standard devaition matrices for tTest
        private Dictionary<string, double[,]> spgr_StdDevMatrices;                                      

        /// <summary>
        /// used where the spectrograms are derived from averages and want to do t-test of difference.
        /// </summary>
        public int SampleCount { get; set; }


        /// <summary>
        /// Index properties - conatins user defined min and max values for index normalisation - required when drawing images.
        /// </summary>
        private Dictionary<string, IndexProperties> spectralIndexProperties;
        
        /// <summary>
        /// Index distribution statistics are now calulated after the indices have been calculated.
        /// </summary>
        private readonly Dictionary<string, IndexDistributions.SpectralStats> indexStats;

        public Dictionary<string, IndexDistributions.SpectralStats> IndexStats { get; private set; }

        public List<ErroneousIndexSegments> ErroneousSegments { get; private set; }

        /// <summary>
        /// A file from which can be obtained information about sunrise and sunset times for the recording site.
        /// The csv file needs to be in the correct format and typically should contain 365 lines.
        /// Have not attempted to deal with leap years!
        /// </summary>
        public FileInfo SunriseDataFile { get; set; }



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
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH;
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;
            // set the X and Y axis scales for the spectrograms 
            this.XTicInterval = Xscale; 
            this.SampleRate = sampleRate; 
            this.ColorMap = colourMap;
        }


        public LDSpectrogramRGB(LdSpectrogramConfig config, IndexGenerationData indexGenerationData, string colourMap)
        {
            this.BackgroundFilter = 1.0;
            this.SampleRate = indexGenerationData.SampleRateResampled;
            this.FrameWidth = indexGenerationData.FrameLength;
            this.StartOffset = indexGenerationData.MinuteOffset;
            // set the X and Y axis scales for the spectrograms 
            this.IndexCalculationDuration = indexGenerationData.IndexCalculationDuration;
            this.XTicInterval = config.XAxisTicInterval; 
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
            this.StartOffset = minuteOffset;
            this.FrameWidth = frameWidth;
        }

        public Dictionary<string, IndexProperties> GetSpectralIndexProperties()
        {
            return this.spectralIndexProperties;
        }


        /// <summary>
        /// This method sets default indeces to use if passed Dictionary = null.
        /// This may not be a good idea. Trying it out. Maybe better to crash! 
        /// </summary>
        /// <param name="_spectralIndexProperties"></param>
        public void SetSpectralIndexProperties(Dictionary<string, IndexProperties> dictionaryOfSpectralIndexProperties)
        {
            string[] keys = { "ACI", "ENT", "EVN", "BGN", "POW", "EVN" }; // the NEW default i.e. since July 2015
            //string[] keys = { "ACI", "TEN", "CVR", "BGN", "AVG", "VAR" }; // the OLD default i.e. used in 2014
            this.spectrogramKeys = keys;
            if ((dictionaryOfSpectralIndexProperties != null) && ((dictionaryOfSpectralIndexProperties.Count > 0)))
            {
                this.spectralIndexProperties = dictionaryOfSpectralIndexProperties;
                this.spectrogramKeys = this.spectralIndexProperties.Keys.ToArray();
            }
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
                // TODO: this string constant is dodgy... but should never change... fix me when broken :-)
                const string analysisType = "Towsey.Acoustic";
                var path = FilenameHelpers.AnalysisResultName(ipdir, fileName, analysisType + "." + keys[i], "csv");
                var file = new FileInfo(path);
                //string path = Path.Combine(ipdir.FullName, fileName + "." + keys[i] + ".csv");
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    this.spectrogramMatrices.Add(this.spectrogramKeys[i], matrix);
                    this.FrameWidth = freqBinCount * 2;

                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method LDSpectrogramRGB.ReadCSVFiles()";
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
                LoggedConsole.WriteLine("WARNING: from method LDSpectrogramRGB.ReadCSVFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
                allOk = false;
            }

            return allOk;
        }


        public bool ReadStandardDeviationSpectrogramCSVs(DirectoryInfo ipdir, string fileName)
        {
            int freqBinCount;
            this.spgr_StdDevMatrices = IndexMatrices.ReadSpectrogramCSVFiles(ipdir, fileName, this.ColorMap, out freqBinCount);
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
            this.spectrogramMatrices.Add(key, matrix);
        }

        public void AddRotatedSpectrogram(string key, double[,] matrix)
        {
            matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
            this.spectrogramMatrices.Add(key, matrix);
        }


        /// <summary>
        /// Call this method if already have a dictionary of Matrix spectorgrams and wish to load directly
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
        /// NOTE: The matrix is oriented as it would appear in the spectrogram image; i.e. rows = freq bins.
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

            var matrix = this.GetMatrix(key);
            // get min, max from index properties file
            IndexProperties indexProperties = this.spectralIndexProperties[key];
            double min = indexProperties.NormMin;
            double max = indexProperties.NormMax;

            // check to determine if user wants to use the automated bound.
            if (this.IndexStats != null)
            {
                if (indexProperties.CalculateNormMin)
                {
                    min = this.IndexStats[key].Mode;
                    //fix bug if signal is defective & = zero. We do not want ACI min ever set too low.
                    if((key.Equals("ACI"))&&(min < 0.3)) min = indexProperties.NormMin;
                }

                if (indexProperties.CalculateNormMax)
                {
                    max = this.IndexStats[key].GetValueOfNthPercentile(IndexDistributions.UPPER_PERCENTILE_DEFAULT);
                }
            }

            Log.Debug("GetNormalisedSpectrogramMatrix(key=" + key + "): min bound=" + min + "      max bound=" + max); // check min, max values
            matrix = MatrixTools.NormaliseInZeroOne(matrix, min, max);
            matrix = MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
            return matrix;
        }


        /// <summary>
        /// Draws all available spectrograms in grey scale 
        /// </summary>
        /// <param name="opdir"></param>
        /// <param name="opFileName"></param>
        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName)
        {
            // string[] keys = LdSpectrogramConfig.GetKeys(this.ColorMap);
            string[] keys = this.spectrogramKeys;
            this.DrawGreyScaleSpectrograms(opdir, opFileName, keys);
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
                    LoggedConsole.WriteErrorLine("\n\nWARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteErrorLine("         Dictionary of spectrogram matrices does NOT contain key: {0}", key);
                    LoggedConsole.WriteErrorLine("         This may prove to be a fatal error - just depends - wait and see!", key);
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
                    LoggedConsole.WriteErrorLine("WARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteErrorLine("         Null matrix returned with key: {0}", key);
                    LoggedConsole.WriteErrorLine("         This may be prove to be a fatal error - just depends - wait and see!", key);
                    continue;
                }

                string path = FilenameHelpers.AnalysisResultName(opdir, opFileName, key, "png");
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

            Image bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(matrix);
            TimeSpan xAxisPixelDuration = this.IndexCalculationDuration;
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);
            int nyquist = this.SampleRate / 2; 
            int herzInterval = 1000;
            //double secondsDuration = xAxisPixelDuration.TotalSeconds * bmp.Width;
            //TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp, this.StartOffset, fullDuration, xAxisPixelDuration, nyquist, herzInterval);
            const int trackHeight = 20;
            TimeSpan timeScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, this.RecordingStartDate, bmp.Width, trackHeight);
            var array = new Image[2];
            array[0] = bmp;
            array[1] = timeBmp;
            var returnImage = ImageTools.CombineImagesVertically(array);
            return returnImage;
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
                bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + "." + colorMap + "png"));
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

        public Image DrawFalseColourSpectrogram(string colorMODE, bool withChrome = true)
        {
            Image bmp = this.DrawFalseColourSpectrogram(colorMODE, this.ColorMap, withChrome);
            return bmp;
        }

        public Image DrawFalseColourSpectrogram(string colorMode, string colorMap, bool withChrome = true)
        {
            if (!this.ContainsMatrixForKeys(colorMap))
            {
                return null;
            }

            string[] rgbMap = colorMap.Split('-');

            //var indexProperties = this.spProperties[rgbMap[0]];
            var redMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[0]);
            var grnMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[1]);
            var bluMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[2]);
            bool doReverseColour = colorMode.StartsWith("POS");

            Image bmp = LDSpectrogramRGB.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);

            // now add in image patches for possible erroneous index segments 
            if ((this.ErroneousSegments != null) && (this.ErroneousSegments.Count > 0))
            {
                bool verticalText = false;
                Graphics g = Graphics.FromImage(bmp);
                foreach (ErroneousIndexSegments errorsegment in ErroneousSegments)
                {
                    Bitmap errorPatch = errorsegment.DrawErrorPatch(bmp.Height, verticalText);
                    g.DrawImage(errorPatch, errorsegment.StartPosition, 1);
                }
            }

            if (!withChrome)
            {
                return bmp; 
            }

            TimeSpan xAxisPixelDuration = this.IndexCalculationDuration; // TimeSpan.FromSeconds(60);
            int herzInterval = 1000;
            int nyquist = this.SampleRate / 2;
            if (nyquist < 5000) herzInterval = 500;
            if (nyquist < 2000) herzInterval = 200;
            double secondsDuration = xAxisPixelDuration.TotalSeconds * bmp.Width;
            TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp, this.StartOffset, fullDuration, this.XTicInterval, nyquist, herzInterval);
            return bmp;
        }


        public Image DrawBlendedFalseColourSpectrogram(string colorMODE, string colorMap1, string colorMap2, double blendWt1, double blendWt2)
        {
            if (!this.ContainsMatrixForKeys(colorMap1) || !this.ContainsMatrixForKeys(colorMap2))
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawBlendedFalseColourSpectrogram() line 662");
                LoggedConsole.WriteLine("         There is no Matrix for one or more spectral indices.");
                LoggedConsole.WriteLine("         Null image returned");
                return null;
            }

            string[] rgbMap1 = colorMap1.Split('-');
            string[] rgbMap2 = colorMap2.Split('-');

            var matrix1 = this.GetNormalisedSpectrogramMatrix(rgbMap1[0]);
            var matrix2 = this.GetNormalisedSpectrogramMatrix(rgbMap2[0]);
            var redMatrix = MatrixTools.AddMatricesWeightedSum(matrix1, blendWt1, matrix2, blendWt2);

            matrix1 = this.GetNormalisedSpectrogramMatrix(rgbMap1[1]);
            matrix2 = this.GetNormalisedSpectrogramMatrix(rgbMap2[1]);
            var grnMatrix = MatrixTools.AddMatricesWeightedSum(matrix1, blendWt1, matrix2, blendWt2);

            matrix1 = this.GetNormalisedSpectrogramMatrix(rgbMap1[2]);
            matrix2 = this.GetNormalisedSpectrogramMatrix(rgbMap2[2]);
            var bluMatrix = MatrixTools.AddMatricesWeightedSum(matrix1, blendWt1, matrix2, blendWt2);

            bool doReverseColour = colorMODE.StartsWith("POS");

            Image bmp = LDSpectrogramRGB.DrawRGBColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);
            //bmp.Save(@"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\TiledImages\TESTIMAGE.png");

            //TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(60);
            //int herzInterval = 1000;
            //int nyquist = this.SampleRate / 2;
            //double secondsDuration = xAxisPixelDuration.TotalSeconds * bmp.Width;
            //TimeSpan fullDuration = TimeSpan.FromSeconds(secondsDuration);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp, this.StartOffset, fullDuration, this.XTicInterval, nyquist, herzInterval);
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
            TimeSpan fullDuration = TimeSpan.FromSeconds(bmp2.Width); // assume one minute per pixel.
            const int TrackHeight = 20;
            int imageHt = bmp2.Height + bmp1.Height + TrackHeight + TrackHeight + TrackHeight;
            string title = string.Format("FALSE COLOUR and BACKGROUND NOISE SPECTROGRAMS      (scale: hours x kHz)      (colour: R-G-B = {0})         (c) QUT.EDU.AU.  ", this.ColorMap);
            Bitmap titleBmp = Image_Track.DrawTitleTrack(bmp2.Width, TrackHeight, title);
            TimeSpan timeScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            TimeSpan offsetMinute = TimeSpan.Zero;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, offsetMinute, timeScale, bmp2.Width, TrackHeight, "hours");

            Bitmap compositeBmp = new Bitmap(bmp2.Width, imageHt); //get canvas for entire image
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
            int maxScaleLength = bmp2.Width / 3;
            Image scale = LDSpectrogramRGB.DrawColourScale(maxScaleLength, TrackHeight - 2);
            int xLocation = bmp2.Width * 2 / 3;
            gr.DrawImage(scale, xLocation, 1); //dra
            return compositeBmp;
        }

        //############################################################################################################################################################
        //# BELOW METHODS CALCULATE SUMMARY INDEX RIBBONS ############################################################################################################
        
        /// <summary>
        /// Returns an array of summary indices, where each element of the array (one element per minute) is a single summary index 
        /// derived by averaging the spectral indices for that minute.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double[] GetSummaryIndexArray(string key)
        {
            // return matrices have spectrogram orientation
            double[,] m = this.GetNormalisedSpectrogramMatrix(key);
            int colcount = m.GetLength(1);
            double[] indices = new double[colcount];

            for (int r = 0; r < colcount; r++)
            {
                indices[r] = MatrixTools.GetColumn(m, r).Average();
            }

            return indices;
        }

        public double[] GetSummaryIndicesAveraged()
        {
            double[] indices1 = GetSummaryIndexArray(this.spectrogramKeys[0]);
            double[] indices2 = GetSummaryIndexArray(this.spectrogramKeys[1]);
            double[] indices3 = GetSummaryIndexArray(this.spectrogramKeys[2]);

            int count = indices1.Length;
            double[] averagedIndices = new double[count];
            for (int r = 0; r < count; r++)
            {
                averagedIndices[r] = (indices1[r] +indices2[r] +indices3[r]) / 3;
            }
            return averagedIndices;            
        }

        public Image GetSummaryIndexRibbon(string colorMap)
        {
            string colourKeys = colorMap;
            string[] keys = colourKeys.Split('-');
            double[] indices1 = GetSummaryIndexArray(keys[0]);
            double[] indices2 = GetSummaryIndexArray(keys[1]);
            double[] indices3 = GetSummaryIndexArray(keys[2]);

            int width = indices1.Length;
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            Pen pen;
            var image = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(image);
            for (int i = 0; i < width; i++)
            {
                if (Double.IsNaN(indices1[i]) || Double.IsNaN(indices2[i]) || Double.IsNaN(indices3[i]))
                {
                    pen = new Pen(Color.Gray);
                }
                else
                {
                    int red = (int)(255 * indices1[i]);
                    int grn = (int)(255 * indices2[i]);
                    int blu = (int)(255 * indices3[i]);
                    pen = new Pen(Color.FromArgb(red, grn, blu));
                }

                g.DrawLine(pen, i, 0, i, height);
            }
            return image;
        }


        /// <summary>
        /// Returns three arrays of summary indices, for the LOW, MIDDLE and HIGH frequency bands respectively. 
        /// Each element of an array (one element per minute) is a single summary index 
        /// derived by averaging the spectral indices in the L, M or H freq band for that minute.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double[,] GetLoMidHiSummaryIndexArrays(string key)
        {
            // return matrices have spectrogram orientation
            double[,] m = this.GetNormalisedSpectrogramMatrix(key);
            int colcount = m.GetLength(1);
            double[,] indices = new double[3, colcount];

            int spectrumLength = m.GetLength(0);

            // here set bounds between the three freq bands
            // this is important beacuse it will affect appearance of the final colour ribbon.
            int lowBound = (int)(spectrumLength * 0.2);
            int midBound = (int)(spectrumLength * 0.5);
            int midBandWidth = midBound - lowBound;
            int higBandWidth = spectrumLength - midBound;

            // create highband weights - triangular weighting
            double[] hibandWeights = new double[higBandWidth];
            double maxweight = 2 / (double)higBandWidth;
            double step = maxweight / (double)higBandWidth;
            for (int c = 0; c < higBandWidth; c++)
            {
                hibandWeights[c] = maxweight - (c * step);
            }
            // check that sum = 1.0;
            // double sum = hibandWeights.Sum();

            for (int c = 0; c < colcount; c++)
            {
                double[] spectrum = MatrixTools.GetColumn(m, c);
                // reverse the array because low frequencies are at the end of the array.
                // because matrices are stored in the orientation that they appear in the final image.
                spectrum = DataTools.reverseArray(spectrum);
                // get the low freq band index
                indices[0, c] = DataTools.Subarray(spectrum, 0, lowBound).Average();
                // get the mid freq band index
                indices[1, c] = DataTools.Subarray(spectrum, lowBound, midBandWidth).Average();
                // get the hig freq band index. Here the weights are triangular, sum = 1.0
                double[] subarray = DataTools.Subarray(spectrum, midBound, higBandWidth);
                indices[2, c] = DataTools.DotProduct(subarray, hibandWeights);
            }

            return indices;
        }

        public Image GetSummaryIndexRibbonWeighted(string colorMap)
        {
            string colourKeys = colorMap;
            string[] keys = colourKeys.Split('-');
            // get the matrices for each of the three indices. 
            // each matrix has three rows one for each of low, mid and high band averages
            // each matrix has one column per minute.
            double[,] indices1 = GetLoMidHiSummaryIndexArrays(keys[0]);
            double[,] indices2 = GetLoMidHiSummaryIndexArrays(keys[1]);
            double[,] indices3 = GetLoMidHiSummaryIndexArrays(keys[2]);

            int width = indices1.GetLength(1);
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            Pen pen;
            var image = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(image);

            // get the low, mid and high band averages of indices in each minute.
            double index = 0;
            for (int i = 0; i < width; i++)
            {
                //  get the average of the three indices in the low bandwidth
                index = (indices1[0, i] + indices2[0, i] + indices3[0, i]) / 3; 
                if(Double.IsNaN(index))
                {
                    pen = new Pen(Color.Gray);
                }
                else
                {
                    int red = (int)(255 * index);
                    if (red > 255) red = 255;
                    index = (indices1[1, i] + indices2[1, i] + indices3[1, i]) / 3;
                    int grn = (int)(255 * index);
                    if (grn > 255) grn = 255;
                    index = (indices1[2, i] + indices2[2, i] + indices3[2, i]) / 3;
                    int blu = (int)(255 * index);
                    if (blu > 255) blu = 255;
                    pen = new Pen(Color.FromArgb(red, grn, blu));
                }
                g.DrawLine(pen, i, 0, i, height);
            }
            return image;
        }

        /// <summary>
        /// returns a LD spectrogram of same image length as the full-scale LDspectrogram but the frequency scale reduced to the passed vlaue of height.
        /// This produces a LD spectrogram "ribbon" which can be used in circumstances where the full image is not appropriate.
        /// Note that if the height passed is a power of 2, then the full frequency scale (also a power of 2 due to FFT) can be scaled down exactly.
        /// A height of 32 is quite good - small but still discriminates frequency bands.
        /// </summary>
        /// <param name="colorMap"></param>
        /// <returns></returns>
        public Image GetSpectrogramRibbon(string colorMap, int height)
        {
            string colourKeys = colorMap;
            string[] keys = colourKeys.Split('-');
            // get the matrices for each of the three indices. 
            double[,] indices1 = this.GetNormalisedSpectrogramMatrix(keys[0]);
            double[,] indices2 = this.GetNormalisedSpectrogramMatrix(keys[1]);
            double[,] indices3 = this.GetNormalisedSpectrogramMatrix(keys[2]);

            int width = indices1.GetLength(1);
            var image = new Bitmap(width, height);

            // get the reduced spectra of indices in each minute.
            double[] spectrum1 = null;
            double[] spectrum2 = new double[height];
            double[] spectrum3 = new double[height];
            // calculate the reduction factor i.e. freq bins per pixel row
            int bandWidth = indices1.GetLength(0) / height;

            for (int i = 0; i < width; i++)
            {
                spectrum1 = MatrixTools.GetColumn(indices1, i);
                spectrum2 = MatrixTools.GetColumn(indices2, i);
                spectrum3 = MatrixTools.GetColumn(indices3, i);
                for (int h = 0; h < height; h++)
                {
                    int start = (h * bandWidth);
                    double[] subArray = DataTools.Subarray(spectrum1, start, bandWidth);
                    double index = subArray.Average();
                    if (Double.IsNaN(index)) index = 0.5;
                    int red = (int)(255 * index);
                    if (red > 255) red = 255;

                    subArray = DataTools.Subarray(spectrum2, start, bandWidth);
                    index = subArray.Average();
                    if (Double.IsNaN(index)) index = 0.5;
                    int grn = (int)(255 * index);
                    if (grn > 255) grn = 255;

                    subArray = DataTools.Subarray(spectrum3, start, bandWidth);
                    index = subArray.Average();
                    if (Double.IsNaN(index)) index = 0.5;
                    int blu = (int)(255 * index);
                    if (blu > 255) blu = 255;

                    image.SetPixel(i, h, Color.FromArgb(red, grn, blu));
                }
            }
            return image;
        }

        public double[] GetSummaryIndicesWeightedAtDistance(double[,] normalisedIndex1, double[,] normalisedIndex2, double[,] normalisedIndex3, 
                                                            int minuteInDay, int distanceInMeters)
        {
            double decayConstant = 20.0;

            double[] indices1 = MatrixTools.GetColumn(normalisedIndex1, minuteInDay);
            indices1 = DataTools.reverseArray(indices1);
            indices1 = CalculateDecayedSpectralIndices(indices1, distanceInMeters, decayConstant);
            double[] indices2 = MatrixTools.GetColumn(normalisedIndex2, minuteInDay);
            indices2 = DataTools.reverseArray(indices2);
            indices2 = CalculateDecayedSpectralIndices(indices2, distanceInMeters, decayConstant);
            double[] indices3 = MatrixTools.GetColumn(normalisedIndex3, minuteInDay);
            indices3 = DataTools.reverseArray(indices3);
            indices3 = CalculateDecayedSpectralIndices(indices3, distanceInMeters, decayConstant);

            // ####################### TO DO
            // COMBINE THE INDCES IN SOME WAY

            return indices1;
        }

        public static double[] CalculateDecayedSpectralIndices(double[] spectralIndices, int distanceInMeters, double halfLife)
        {
            double log2 = Math.Log(2.0);
            double differentialFrequencyDecay = 0.1;
            
            int length = spectralIndices.Length;

            double[]  returned = new double[length];
            for (int i = 0; i < length; i++)
            {
                // half life decreases with increasing frquency.
                double frequencyDecay = differentialFrequencyDecay * i;
                double tau = (halfLife - (differentialFrequencyDecay * i)) / log2;
                // check tau is not negative
                if (tau < 0.0) tau = 0.001; 
                double exponent = distanceInMeters / tau;
                returned[i] = spectralIndices[i] * Math.Pow(Math.E, -exponent);
            }
            return returned;
        }



        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################


        //public static double[,] NormaliseSpectrogramMatrix(IndexProperties indexProperties, double[,] matrix, double backgroundFilterCoeff)
        //{
        //    matrix = MatrixTools.NormaliseInZeroOne(matrix, indexProperties.NormMin, indexProperties.NormMax);
        //    matrix = MatrixTools.FilterBackgroundValues(matrix, backgroundFilterCoeff); // to de-demphasize the background small values
        //    return matrix;
        //}



        //========================================================================================================================================================
        //========= NEXT FEW METHODS ARE STATIC AND RETURN VARIOUS KINDS OF IMAGE
        //========================================================================================================================================================

        public static Image FrameLDSpectrogram(Image bmp1, Image titleBar, LDSpectrogramRGB cs, int nyquist, int herzInterval)
        {
            TimeSpan xAxisPixelDuration = cs.IndexCalculationDuration;
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            int trackHeight = 18;
            int imageHt = titleBar.Height + trackHeight + bmp1.Height + trackHeight + 1;
            Bitmap timeBmp1 = Image_Track.DrawTimeRelativeTrack(fullDuration, bmp1.Width, trackHeight);
            Bitmap timeBmp2 = (Bitmap)timeBmp1.Clone();
            Bitmap suntrack = null;

            DateTimeOffset? dateTimeOffset = cs.RecordingStartDate;
            if (dateTimeOffset.HasValue)
            {
                // draw extra time scale with absolute start time. AND THEN Do SOMETHING WITH IT.
                timeBmp2 = Image_Track.DrawTimeTrack(fullDuration, cs.RecordingStartDate, bmp1.Width, trackHeight);
                suntrack = SunAndMoon.AddSunTrackToImage(bmp1.Width, dateTimeOffset, cs.SunriseDataFile);
            }

            //draw the composite bitmap
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, TimeSpan.Zero, fullDuration, cs.XTicInterval, nyquist, herzInterval);
            var imageList = new List<Image>();
            imageList.Add(titleBar);
            imageList.Add(timeBmp1);
            imageList.Add(bmp1);
            imageList.Add(timeBmp2);
            if (suntrack != null) imageList.Add(suntrack);
            Bitmap compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;

            //Bitmap compositeBmp = new Bitmap(bmp1.Width, imageHt); //get canvas for entire image
            //Graphics gr = Graphics.FromImage(compositeBmp);
            //gr.Clear(Color.Black);
            //int offset = 0;
            //gr.DrawImage(titleBar, 0, offset); //draw in the top time scale
            //offset = timeBmp1.Height;
            //gr.DrawImage(timeBmp1, 0, offset); //draw
            //offset += titleBar.Height;
            //gr.DrawImage(bmp1, 0, offset); //draw
            //offset += bmp1.Height;
            //gr.DrawImage(timeBmp2, 0, offset); //draw
            //return compositeBmp;
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
            string text = string.Format("SCALE:(time x kHz)     (c) QUT.EDU.AU");
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
            //Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9, FontStyle.Bold);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            int X = 2;
            g.DrawString(title, stringFont, Brushes.White, new PointF(X, 3));

            stringSize = g.MeasureString(title, stringFont);
            X += (stringSize.ToSize().Width + 300);
            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1);
            g.DrawImage(colourChart, X, 1);

            string text = string.Format("SCALE:(time x kHz)        (c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0);//draw upper boundary
            //g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }


        public static Image DrawRGBColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColour)
        {
            // assume all matricies are normalised and of the same dimensions
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

                    // blank indices painted grey.
                    if (Double.IsNaN(d1)) d1 = 0.5;
                    if (Double.IsNaN(d2)) d2 = 0.5;
                    if (Double.IsNaN(d3)) d3 = 0.5;

                    // enhance blue colour - it is difficult to see on a black background
                    // This is a hack - there should be a principled way to do this.
                    if((d1 < 0.1) && (d2 < 0.1) && (d3 > 0.2))
                    {
                        d2 += (0.7 * d3);
                        d3 += 0.2;
                        d2 = Math.Min(1.0, d2);
                        d3 = Math.Min(1.0, d3);
                    }

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

        /// <summary>
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        ///  IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. 
        /// WARNING: Make sure the parameters in the CONFIG file are consistent with the CSV files.
        /// </summary>
        /// <param name="inputDirectory"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="ldSpectrogramConfig"></param>
        /// <param name="indexPropertiesConfigPath">
        ///     The indices Config Path.
        /// </param>
        /// <param name="indexGenerationData"></param>
        /// <param name="basename"></param>
        /// <param name="analysisType"></param>
        /// <param name="indexSpectrograms">
        ///     Optional spectra to pass in. If specified the spectra will not be loaded from disk!
        /// </param>
        /// <param name="indexDistributions"></param>
        /// <param name="siteDescription">Optionally specify details about the site where the audio was recorded.</param>
        /// <param name="imageChrome">If true, this method generates and returns separate chromeless images.</param>
        /// <param name="longDurationSpectrogramConfig">
        /// </param>
        public static Tuple<Image, string>[] DrawSpectrogramsFromSpectralIndices(
            DirectoryInfo inputDirectory,
            DirectoryInfo outputDirectory,
            LdSpectrogramConfig ldSpectrogramConfig,
            FileInfo indexPropertiesConfigPath,
            IndexGenerationData indexGenerationData,
            string basename,
            string analysisType,
            Dictionary<string, double[,]> indexSpectrograms = null,
            SummaryIndexBase[] summaryIndices = null,
            Dictionary<string, IndexDistributions.SpectralStats> indexDistributions = null,
            SiteDescription siteDescription = null,
            FileInfo sunriseDataFile = null,
            List<ErroneousIndexSegments> segmentErrors = null,
            ImageChrome imageChrome = ImageChrome.With,
            bool Verbose = false)
        {
            LdSpectrogramConfig config = ldSpectrogramConfig;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = config.ColorMap1 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB
            string colorMap2 = config.ColorMap2 ?? SpectrogramConstants.RGBMap_BGN_POW_EVN;   // assigns indices to RGB

            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation
            
            var cs1 = new LDSpectrogramRGB(config, indexGenerationData, colorMap1);
            string fileStem = basename;
            
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = indexGenerationData.BackgroundFilterCoeff;
            cs1.SiteName  = siteDescription?.SiteName;
            cs1.Latitude  = siteDescription?.Latitude;
            cs1.Longitude = siteDescription?.Longitude;
            cs1.SunriseDataFile = sunriseDataFile;

            cs1.ErroneousSegments = segmentErrors;

            // calculate start time by combining DatetimeOffset with minute offset.
            cs1.StartOffset = indexGenerationData.MinuteOffset;
            if (indexGenerationData.RecordingStartDate.HasValue)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                cs1.RecordingStartDate = dto;
                if (dto != null) cs1.StartOffset = dto.TimeOfDay + cs1.StartOffset;
            }

            // following line is debug purposes only
            //cs1.StartOffset = cs1.StartOffset + TimeSpan.FromMinutes(15);

            // Get and set the dictionary of index properties
            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfigPath);
            dictIP = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIP);
            cs1.SetSpectralIndexProperties(dictIP); 

            // Load the Index Spectrograms into a Dictionary
            if (indexSpectrograms == null)
            {
                var sw = Stopwatch.StartNew();
                if (Verbose) Logger.Info("Reading spectra files from disk");
                
                // reads all known files spectral indices
                cs1.ReadCSVFiles(inputDirectory, fileStem, cs1.spectrogramKeys);
                DateTime now2 = DateTime.Now;
                sw.Stop();
                if (Verbose)
                {
                    LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds");
                }
            }
            else
            {
                if (Verbose)
                {
                    Logger.Info("Spectra loaded from memory");
                }
                cs1.LoadSpectrogramDictionary(indexSpectrograms);
            }

            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Log.Error("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                throw new InvalidOperationException("Cannot find spectrogram matrix files");
            }

            // Get index distribution statistics. 
            // Either read from input variable or json file. 
            // Stats are useful because needed if drawing difference spectrograms etc.     
            if (indexDistributions == null)
            {
                indexDistributions = IndexDistributions.ReadSpectralIndexDistributionStatistics(inputDirectory, fileStem);
                //cs1.IndexStats = IndexDistributions.WriteIndexDistributionStatistics(cs1.spectrogramMatrices, ipDir, fileStem);
                Log.Warn("A .json file of index distribution statistics was not found in directory <" + outputDirectory.FullName + ">");

                if (indexDistributions == null)
                {
                    //throw new InvalidOperationException("Cannot proceed without index distribution data");
                }
            }
            cs1.IndexStats = indexDistributions;

            // draw gray scale spectrogram for each index.
            string[] keys = colorMap1.Split('-');
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);
            keys = colorMap2.Split('-');
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);


            Image image1;
            Image image1NoChrome;
            CreateSpectrogramFromSpectralIndices(cs1, colorMap1, imageChrome, outputDirectory).Decompose(out image1, out image1NoChrome);

            Image image2;
            Image image2NoChrome;
            CreateSpectrogramFromSpectralIndices(cs1, colorMap2, imageChrome, outputDirectory).Decompose(out image2, out image2NoChrome);

            // read high amplitude and clipping info into an image
            Image imageX;
            if (summaryIndices == null)
            {
                string indicesFile = FilenameHelpers.AnalysisResultName(
                    inputDirectory,
                    fileStem,
                    analysisType + ".Indices",
                    "csv");
                imageX = IndexDisplay.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
            }
            else
            {
                imageX = IndexDisplay.DrawHighAmplitudeClippingTrack(summaryIndices);
            }

            if (imageX != null)
            {
                imageX.Save(FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, colorMap1 + ".ClipHiAmpl", "png"));
            }

            if ((image1 == null) || (image2 == null)) throw new Exception("NULL image returned. Cannot proceed!");

            CreateTwoMapsImage(outputDirectory, fileStem, image1, imageX, image2);

            Image ribbon;
            //ribbon = cs1.GetSummaryIndexRibbon(colorMap1);
            ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap1);

            ribbon.Save(FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, colorMap1 + ".SummaryRibbon", "png"));
            //ribbon = cs1.GetSummaryIndexRibbon(colorMap2);
            ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap2);
            ribbon.Save(FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, colorMap2 + ".SummaryRibbon", "png"));

            ribbon = cs1.GetSpectrogramRibbon(colorMap1, 32);
            ribbon.Save(FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, colorMap1 + ".SpectralRibbon", "png"));
            ribbon = cs1.GetSpectrogramRibbon(colorMap2, 32);
            ribbon.Save(FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, colorMap2 + ".SpectralRibbon", "png"));

            // only return images if chromeless
            return imageChrome == ImageChrome.Without
                       ? new[] { Tuple.Create(image1NoChrome, colorMap1), Tuple.Create(image2NoChrome, colorMap2) }
                       : null;
        }

        private static Tuple<Image, Image> CreateSpectrogramFromSpectralIndices(LDSpectrogramRGB cs1, string colorMap, 
                                                                                ImageChrome imageChrome, 
                                                                                DirectoryInfo outputDirectory)
        {
            const int HertzInterval = 1000;
            int nyquist = cs1.SampleRate / 2;

            bool errorsExist = false;
            if ((cs1.ErroneousSegments != null) && (cs1.ErroneousSegments.Count > 0))
            {
                errorsExist = true;
            }

            // create a chromeless false color image for tiling
            Image imageNoChrome = null;
            if (imageChrome == ImageChrome.Without)
            {
                imageNoChrome = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap, withChrome: false);
                if (errorsExist)
                {
                    Bitmap errorPatch = cs1.ErroneousSegments[0].DrawErrorPatch(imageNoChrome.Height, true);
                    Graphics g = Graphics.FromImage(imageNoChrome);
                    g.DrawImage(errorPatch, cs1.ErroneousSegments[0].StartPosition, 1);
                }

            }

            // create a normal image with chrome
            Image image = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            if(image == null)
            {
                LoggedConsole.WriteFatalLine(" No image returned!", new Exception("FATAL ERROR!"));
                return Tuple.Create(image, image);
            }


            if (errorsExist)
            {
                Bitmap errorPatch = cs1.ErroneousSegments[0].DrawErrorPatch(image.Height, true);
                Graphics g = Graphics.FromImage(image);
                g.DrawImage(errorPatch, cs1.ErroneousSegments[0].StartPosition, 1);
            }
            string startTime = string.Format("{0:d2}{1:d2}h", cs1.StartOffset.Hours, cs1.StartOffset.Minutes);

            // then pass that image into chromer
            string title = string.Format("<{0}> SPECTROGRAM  of \"{1}\".   Starts at {2}; Nyquist={3}", 
                                                                                      colorMap, cs1.FileName, startTime, nyquist);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image.Width);
            image = LDSpectrogramRGB.FrameLDSpectrogram(image, titleBar, cs1, nyquist, HertzInterval);
            var outputPath = FilenameHelpers.AnalysisResultName(outputDirectory, cs1.FileName, colorMap, "png");
            image.Save(outputPath);
            return Tuple.Create(image, imageNoChrome);
        }

        private static void CreateTwoMapsImage(DirectoryInfo outputDirectory, string fileStem, Image image1, Image imageX, Image image2)
        {
            var imageList = new[] { image1, imageX, image2 };
            Image image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultName(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);
        }
    }
}
