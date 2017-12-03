// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramRGB.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This class generates false-colour spectrograms of long duration audio recordings.
//   Important properties are:
//   1) the colour map which maps three acoutic indices to RGB.
//   2) The scale of the x and y axes which are determined by the sample rate, frame size etc.
//   In order to create false colour spectrograms, copy the method
//   public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
//   All the arguments can be passed through a config file.
//   Create the config file through an instance of the class LDSpectrogramConfig
//   and then call config.WritConfigToYAML(FileInfo path).
//   Then pass that path to the above static method.
//
//  Activity Codes for other tasks to do with spectrograms and audio files:
//
// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
//
// audiofilecheck - Writes information about audio files to a csv file.
// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
// audiocutter - Cuts audio into segments of desired length and format
// createfoursonograms
//
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
    using DSP;
    using Indices;
    using log4net;
    using StandardSpectrograms;
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
        // string[] keys = { "ACI", "TEN", "CVR", "BGN", "AVG", "VAR" }; // the OLDEST default i.e. used in 2014
        // string[] keys = { "ACI", "ENT", "EVN", "BGN", "POW", "EVN" }; // the OLD default i.e. since July 2015
        // assign indices to RGB - the next two lines are for experimental purposes
        // public static readonly string DefaultColorMap1 = "ACI, ENT, EVN";
        // public static readonly string DefaultColorMap2 = "BGN, PMN, R3D";

        // the defaults
        public static readonly string DefaultColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
        public static readonly string DefaultColorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_R3D;

        // Before May 2017, only the required six spectral indices were incorporated in a dicitonary of spectral matrices.
        // Since May 2017, all the likely available matrices are incorporated into a dictionary. Note the new names for PMN and R3D, previously POW and HPN respectively.
        // Note 1: This default array will be subsequently over-written by the indices in the IndexPropertiesConfig file if one is available.
        // Note 1: RHZ, SPT and CVR are correlated with POW and do not add much. CLS is not particularly useful. Currently using R3D
        private static readonly string[] DefaultKeys = { "ACI", "BGN", "CLS", "CVR", "ENT", "EVN", "PMN", "POW", "RHZ", "RVT", "RPS", "RNG", "R3D", "SPT" };

        public static string[] GetArrayOfAvailableKeys()
        {
            return DefaultKeys;
        }

        // used to save all spectrograms as dictionary of matrices
        // IMPORTANT: The matrices are stored as they would appear in the LD spectrogram image. i.e. rotated 90 degrees anti-clockwise.
        public Dictionary<string, double[,]> SpectrogramMatrices = new Dictionary<string, double[,]>();

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Index properties - conatins user defined min and max values for index normalisation - required when drawing images.
        /// </summary>
        private Dictionary<string, IndexProperties> spectralIndexProperties;

        // Rarely used. Only if reading standard deviation matrices for tTest
        private Dictionary<string, double[,]> spgrStdDevMatrices;

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramRGB"/> class.
        /// No Arguments CONSTRUCTOR
        /// </summary>
        public LDSpectrogramRGB()
        {
            this.ColorMode = "NEGATIVE"; // the default
            this.BackgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // default BackgroundFilter value
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE; // default recording starts at midnight
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH; // default value - from which spectrogram was derived
            this.XTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default = one minute spectra and hourly time lines
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.SpectrogramKeys = DefaultKeys;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramRGB"/> class.
        /// CONSTRUCTOR
        /// </summary>
        public LDSpectrogramRGB(TimeSpan xScale, int sampleRate, string colourMap)
        {
            this.ColorMode = "NEGATIVE"; // the default
            this.BackgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // default BackgroundFilter value
            this.SampleRate = sampleRate;

            // assume default linear Herz scale
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH;
            this.FreqScale = new FrequencyScale(nyquist: this.SampleRate / 2, frameSize: this.FrameWidth, hertzLinearGridInterval: 1000);

            // set the X and Y axis scales for the spectrograms
            this.XTicInterval = xScale;
            this.SampleRate = sampleRate;
            this.ColorMap = colourMap;
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;

            // IMPORTANT NOTE: If an IndexPropertiesConfig file is available, these default keys are later over-written in the method
            // SetSpectralIndexProperties(Dictionary < string, IndexProperties > dictionaryOfSpectralIndexProperties)
            // Consequently the INDEX names in DefaultKeys should match those in IndexPropertiesConfig file. If they do not, consequences are undefined!
            this.SpectrogramKeys = DefaultKeys;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramRGB"/> class.
        /// CONSTRUCTOR
        /// WARNING: Ths will create a linear herz scale spectrogram
        /// </summary>
        /// <param name="minuteOffset">minute of day at which the spectrogram starts</param>
        /// <param name="xScale">time scale : pixels per hour</param>
        /// <param name="sampleRate">recording sample rate which also determines scale of Y-axis.</param>
        /// <param name="frameWidth">frame size - which also determines scale of Y-axis.</param>
        /// <param name="colourMap">acoustic indices used to assign  the three colour mapping.</param>
        public LDSpectrogramRGB(TimeSpan minuteOffset, TimeSpan xScale, int sampleRate, int frameWidth, string colourMap)
            : this(xScale, sampleRate, colourMap)
        {
            this.StartOffset = minuteOffset;
            this.FrameWidth = frameWidth;
        }

        public LDSpectrogramRGB(LdSpectrogramConfig config, IndexGenerationData indexGenerationData, string colourMap)
        {
            this.SampleRate = indexGenerationData.SampleRateResampled;
            this.FrameWidth = indexGenerationData.FrameLength;
            this.StartOffset = indexGenerationData.AnalysisStartOffset;

            // default BackgroundFilter value
            this.BackgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            if (config.ColourFilter != null)
            {
                this.BackgroundFilter = (double)config.ColourFilter;
            }

            // set the X and Y axis scales for the spectrograms
            this.IndexCalculationDuration = indexGenerationData.IndexCalculationDuration;
            this.XTicInterval = config.XAxisTicInterval;

            // the Herz scale
            int nyquist = indexGenerationData.SampleRateResampled / 2;
            int yAxisTicInterval = config.YAxisTicInterval;

            int frameSize = indexGenerationData.FrameLength;
            FreqScaleType fst;
            switch (config.FreqScale)
            {
                case "Linear":
                    this.FreqScale = new FrequencyScale(nyquist, frameSize, hertzLinearGridInterval: yAxisTicInterval);
                    break;
                case "Mel":
                    fst = FreqScaleType.Mel;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Mel Scale is not yet implemented");

                    //break;
                case "Linear62Octaves7Tones31Nyquist11025":
                    fst = FreqScaleType.Linear62Octaves7Tones31Nyquist11025;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Linear62Octaves7Tones31Nyquist11025 Scale is not yet implemented");

                    //break;
                case "Linear125Octaves6Tones30Nyquist11025":
                    fst = FreqScaleType.Linear125Octaves6Tones30Nyquist11025;
                    this.FreqScale = new FrequencyScale(fst);
                    break;
                case "Octaves24Nyquist32000":
                    fst = FreqScaleType.Octaves24Nyquist32000;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Octaves24Nyquist32000 Scale is not yet implemented");

                    //break;
                case "Linear125Octaves7Tones28Nyquist32000":
                    fst = FreqScaleType.Linear125Octaves7Tones28Nyquist32000;
                    this.FreqScale = new FrequencyScale(fst);
                    break;
                default:
                    throw new ArgumentException($"{config.FreqScale} is an unknown option for drawing a frequency scale");
            }

            this.ColorMode = "NEGATIVE"; // the default
            this.ColorMap = colourMap;
        }

        public string[] SpectrogramKeys { get; private set; }

        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the current LDspectrogram starts
        /// This can be used to correctly
        /// </summary>
        public DateTimeOffset RecordingStartDate { get; set; }

        public string SiteName { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the time at which the current LDspectrogram starts.
        /// </summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the temporal duration of one subsegment interval for which indices are calculated
        /// </summary>
        public TimeSpan IndexCalculationDuration { get; set; }

        public TimeSpan XTicInterval { get; set; }

        /// <summary>
        /// Gets or sets the frame width. Used only to calculate scale of Y-axis to draw grid lines.
        /// </summary>
        public int FrameWidth { get; set; }

        /// <summary>
        /// Gets or sets the sample rate.
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the ColorMap within current recording.
        /// </summary>
        public FrequencyScale FreqScale { get; set; }

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
        /// Gets or sets pOSITIVE or NEGATIVE
        /// </summary>
        public string ColorMode { get; set; }

        /// <summary>
        /// Gets or sets used where the spectrograms are derived from averages and want to do t-test of difference.
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// Gets index distribution statistics are now calulated after the indices have been calculated.
        /// </summary>
        public Dictionary<string, IndexDistributions.SpectralStats> IndexStats { get; private set; }

        public List<GapsAndJoins> ErroneousSegments { get; private set; }

        /// <summary>
        /// Gets or sets a file from which can be obtained information about sunrise and sunset times for the recording site.
        /// The csv file needs to be in the correct format and typically should contain 365 lines.
        /// Have not attempted to deal with leap years!
        /// </summary>
        [Obsolete]
        public FileInfo SunriseDataFile { get; set; }

        public Dictionary<string, IndexProperties> GetSpectralIndexProperties()
        {
            return this.spectralIndexProperties;
        }

        /// <summary>
        /// This method sets default indices to use if passed Dictionary = null.
        /// This may not be a good idea. Trying it out. Maybe better to crash!
        /// </summary>
        public void SetSpectralIndexProperties(Dictionary<string, IndexProperties> dictionaryOfSpectralIndexProperties)
        {
            if (dictionaryOfSpectralIndexProperties != null && dictionaryOfSpectralIndexProperties.Count > 0)
            {
                this.spectralIndexProperties = dictionaryOfSpectralIndexProperties;
                this.SpectrogramKeys = this.spectralIndexProperties.Keys.ToArray();
            }
        }

        public bool ReadCsvFiles(DirectoryInfo ipdir, string fileName)
        {
            return this.ReadCsvFiles(ipdir, fileName, this.SpectrogramKeys);
        }

        public bool ReadCsvFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            bool allOk = true;
            string warning = null;
            for (int i = 0; i < keys.Length; i++)
            {
                // TODO: this string constant is dodgy... but should never change... fix me when broken :-)
                const string analysisType = "Towsey.Acoustic";
                var path = FilenameHelpers.AnalysisResultPath(ipdir, fileName, analysisType + "." + keys[i], "csv");
                var file = new FileInfo(path);
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    this.SpectrogramMatrices.Add(this.SpectrogramKeys[i], matrix);
                    this.FrameWidth = freqBinCount * 2;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method LDSpectrogramRGB.ReadSpectralIndices()";
                    }

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], path);
                    allOk = false;
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (this.SpectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method LDSpectrogramRGB.ReadSpectralIndices()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
                allOk = false;
            }

            return allOk;
        }

        public bool ReadStandardDeviationSpectrogramCsvs(DirectoryInfo ipdir, string fileName)
        {
            int freqBinCount;
            this.spgrStdDevMatrices = IndexMatrices.ReadSpectrogramCSVFiles(ipdir, fileName, this.ColorMap, out freqBinCount);
            this.FrameWidth = freqBinCount * 2;
            if (this.spgrStdDevMatrices == null)
            {
                return false;
            }

            if (this.spgrStdDevMatrices.Count < 3)
            {
                return false;
            }

            return true;
        }

        public double[,] GetSpectrogramMatrix(string key)
        {
            return this.SpectrogramMatrices[key];
        }

        public double[,] GetStandarDeviationMatrix(string key)
        {
            return this.spgrStdDevMatrices[key];
        }

        public int GetCountOfSpectrogramMatrices()
        {
            return this.SpectrogramMatrices.Count;
        }

        public int GetCountOfStandardDeviationMatrices()
        {
            return this.spgrStdDevMatrices.Count;
        }

        /// <summary>
        /// Call this method if already have a dictionary of Matrix spectorgrams and wish to load directly
        /// For example, call this method from AnalyseLongRecordings.
        /// </summary>
        public void LoadSpectrogramDictionary(Dictionary<string, double[,]> dictionary)
        {
            this.SpectrogramMatrices = dictionary;
        }

        /// <summary>
        /// Call this method to access a spectrogram matrix
        /// </summary>
        public double[,] GetMatrix(string key)
        {
            if (this.SpectrogramMatrices.ContainsKey(key))
            {
                return this.SpectrogramMatrices[key];
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
        public double[,] GetNormalisedSpectrogramMatrix(string key)
        {
            if (!this.spectralIndexProperties.ContainsKey(key))
            {
                LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                LoggedConsole.WriteLine("Dictionary of Spectral Properties does not contain key {0}", key);
                return null;
            }

            if (!this.SpectrogramMatrices.ContainsKey(key))
            {
                LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
                LoggedConsole.WriteLine("Dictionary of Spectrogram Matrices does not contain key {0}", key);
                return null;
            }

            var matrix = this.GetMatrix(key);

            // get min, max normalisation bounds from the index properties file
            var indexProperties = this.spectralIndexProperties[key];
            double minBound = indexProperties.NormMin;
            double maxBound = indexProperties.NormMax;

            // check if user wants to use the automated bounds.
            if (this.IndexStats != null)
            {
                if (indexProperties.CalculateNormMin)
                {
                    //min = this.IndexStats[key].Mode;
                    minBound = this.IndexStats[key].Mode - (this.IndexStats[key].StandardDeviation * 0.1);

                    // fix case where signal is defective &= zero. We do not want ACI min ever set too low.
                    if (key.Equals("ACI") && minBound < 0.3)
                    {
                        minBound = indexProperties.NormMin;
                    }
                }

                if (indexProperties.CalculateNormMax)
                {
                    maxBound = this.IndexStats[key].GetValueOfNthPercentile(IndexDistributions.UpperPercentileDefault);

                    // correct for case where max bound = zero. This can happen where ICD is very short i.e. 0.1s.
                    if (maxBound < 0.0001)
                    {
                        maxBound = this.IndexStats[key].Maximum * 0.1;
                    }
                }
            }

            // check min, max values
            Log.Debug("GetNormalisedSpectrogramMatrix(key=" + key + "): min bound=" + minBound + "      max bound=" + maxBound);
            matrix = MatrixTools.NormaliseInZeroOne(matrix, minBound, maxBound);

            // de-demphasize the background small values
            matrix = MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter);
            return matrix;
        }

        /// <summary>
        /// Draws all available spectrograms in grey scale
        /// </summary>
        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName)
        {
            string[] keys = this.SpectrogramKeys;
            this.DrawGreyScaleSpectrograms(opdir, opFileName, keys);
        }

        /// <summary>
        /// draws only those spectrograms in the passed array of keys
        /// </summary>
        public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName, string[] keys)
        {
            foreach (string key in keys)
            {
                if (!this.SpectrogramMatrices.ContainsKey(key))
                {
                    LoggedConsole.WriteErrorLine("\n\nWARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteErrorLine("         Dictionary of spectrogram matrices does NOT contain key: {0}", key);
                    LoggedConsole.WriteErrorLine("         This may prove to be a fatal error - just depends - wait and see!", key);
                    List<string> keyList = new List<string>(this.SpectrogramMatrices.Keys);
                    string list = string.Empty;
                    foreach (string str in keyList)
                    {
                        list += str + ", ";
                    }

                    LoggedConsole.WriteLine("          List of keys in dictionary = {0}", list);
                    continue;
                }

                if (this.SpectrogramMatrices[key] == null)
                {
                    LoggedConsole.WriteErrorLine("WARNING: From method LDSpectrogram.DrawGreyScaleSpectrograms()");
                    LoggedConsole.WriteErrorLine("         Null matrix returned with key: {0}", key);
                    LoggedConsole.WriteErrorLine("         This may be prove to be a fatal error - just depends - wait and see!", key);
                    continue;
                }

                // the directory for the following path must exist
                var path = FilenameHelpers.AnalysisResultPath(opdir, opFileName, key, "png");
                var bmp = this.DrawGreyscaleSpectrogramOfIndex(key);
                bmp?.Save(path);
            }
        }

        /// <summary>
        /// Assume calling method has done all the reality checks
        /// </summary>
        public Image DrawGreyscaleSpectrogramOfIndex(string key)
        {
            double[,] matrix = this.GetNormalisedSpectrogramMatrix(key);
            if (matrix == null)
            {
                return null;
            }

            var bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(matrix);
            var xAxisPixelDuration = this.IndexCalculationDuration;
            var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);

            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp, this.StartOffset, fullDuration, xAxisPixelDuration, this.FreqScale);
            const int trackHeight = 20;
            var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, this.RecordingStartDate, bmp.Width, trackHeight);
            var array = new Image[2];
            array[0] = bmp;
            array[1] = timeBmp;
            var returnImage = ImageTools.CombineImagesVertically(array);
            return returnImage;
        }

        public void DrawNegativeFalseColourSpectrogram(DirectoryInfo outputDirectory, string outputFileName)
        {
            var bmpNeg = this.DrawFalseColourSpectrogramChromeless("NEGATIVE", this.ColorMap);
            if (bmpNeg == null)
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawNegativeFalseColourSpectrograms()");
                LoggedConsole.WriteLine("         Null image returned");
                return;
            }

            bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + ".COLNEG.png"));

            string key = InitialiseIndexProperties.KeYspectralBgn;
            if (!this.SpectrogramMatrices.ContainsKey(key))
            {
                LoggedConsole.WriteLine("\nWARNING: SG {0} does not contain key: {1}", outputFileName, key);
            }
            else
            {
                var bmpBgn = this.DrawGreyscaleSpectrogramOfIndex(key);
                bmpNeg = this.DrawDoubleSpectrogram(bmpNeg, bmpBgn, "NEGATIVE");
                bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + ".COLNEGBGN.png"));
            }
        }

        /// <summary>
        /// Draw a chromeless false colour spectrogram.
        /// Chromeless means WITHOUT all the trimmings, such as title bar axis labels, grid lines etc.
        /// However it does add in notated error segments
        /// </summary>
        public Image DrawFalseColourSpectrogramChromeless(string colorMode, string colorMap)
        {
            if (!this.ContainsMatrixForKeys(colorMap))
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawFalseColourSpectrogram()");
                LoggedConsole.WriteLine("         No matrices are available for the passed colour map.");
                return null;
            }

            string[] rgbMap = colorMap.Split('-');

            // NormaliseMatrixValues spectrogram values and de-emphasize the low background values
            var redMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[0]);
            var grnMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[1]);
            var bluMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[2]);
            bool doReverseColour = colorMode.StartsWith("POS");

            var bmp = DrawRgbColourMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColour);

            if (bmp == null)
            {
                LoggedConsole.WriteWarnLine($" No image returned for ColorMap: {colorMap}!");
            }

            // now add in image patches for possible erroneous index segments
            bool errorsExist = (this.ErroneousSegments != null) && (this.ErroneousSegments.Count > 0);
            if (errorsExist)
            {
                bmp = GapsAndJoins.DrawErrorSegments(bmp, this.ErroneousSegments);
            }

            return bmp;
        }

        public Image DrawBlendedFalseColourSpectrogram(string colorMap1, string colorMap2, double blendWt1, double blendWt2)
        {
            if (!this.ContainsMatrixForKeys(colorMap1) || !this.ContainsMatrixForKeys(colorMap2))
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawBlendedFalseColourSpectrogram() line 662");
                LoggedConsole.WriteLine("         There is no Matrix for one or more spectral indices.");
                throw new ArgumentException("Required spectral matrices are not available");
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

            var bmp = DrawRgbColourMatrix(redMatrix, grnMatrix, bluMatrix, false);
            return bmp;
        }

        public bool ContainsMatrixForKeys(string keys)
        {
            if (this.SpectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("ERROR! ERROR! ERROR! - There are no indices with which to construct a spectrogram!");
                return false;
            }

            var containsKey = true;
            string[] rgbMap = keys.Split('-');
            foreach (var key in rgbMap)
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
            if (this.SpectrogramMatrices.ContainsKey(key))
            {
                return true;
            }

            LoggedConsole.WriteLine("ERROR! - spectrogramMatrices does not contain key: <{0}> !", key);
            return false;
        }

        public Image DrawDoubleSpectrogram(Image bmp1, Image bmp2, string colorMode)
        {
            var fullDuration = TimeSpan.FromSeconds(bmp2.Width); // assume one minute per pixel.
            const int trackHeight = 20;
            int imageHt = bmp2.Height + bmp1.Height + trackHeight + trackHeight + trackHeight;
            var title =
                $"FALSE COLOUR and BACKGROUND NOISE SPECTROGRAMS      (scale: hours x kHz)      (colour: R-G-B = {this.ColorMap})         {Meta.OrganizationTag}  ";
            var titleBmp = ImageTrack.DrawTitleTrack(bmp2.Width, trackHeight, title);
            var timeScale = SpectrogramConstants.X_AXIS_TIC_INTERVAL;
            var offsetMinute = TimeSpan.Zero;
            var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, offsetMinute, timeScale, bmp2.Width, trackHeight, "hours");

            var compositeBmp = new Bitmap(bmp2.Width, imageHt); //get canvas for entire image
            var gr = Graphics.FromImage(compositeBmp);
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
            var scale = DrawColourScale(maxScaleLength, trackHeight - 2);
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
            double[] indices1 = this.GetSummaryIndexArray(this.SpectrogramKeys[0]);
            double[] indices2 = this.GetSummaryIndexArray(this.SpectrogramKeys[1]);
            double[] indices3 = this.GetSummaryIndexArray(this.SpectrogramKeys[2]);

            int count = indices1.Length;
            double[] averagedIndices = new double[count];
            for (int r = 0; r < count; r++)
            {
                averagedIndices[r] = (indices1[r] + indices2[r] + indices3[r]) / 3;
            }

            return averagedIndices;
        }

        public Image GetSummaryIndexRibbon(string colorMap)
        {
            string colourKeys = colorMap;
            string[] keys = colourKeys.Split('-');
            var indices1 = this.GetSummaryIndexArray(keys[0]);
            var indices2 = this.GetSummaryIndexArray(keys[1]);
            var indices3 = this.GetSummaryIndexArray(keys[2]);

            int width = indices1.Length;
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            var image = new Bitmap(width, height);
            var g = Graphics.FromImage(image);
            for (int i = 0; i < width; i++)
            {
                Pen pen;
                if (double.IsNaN(indices1[i]) || double.IsNaN(indices2[i]) || double.IsNaN(indices3[i]))
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
            double step = maxweight / higBandWidth;
            for (int c = 0; c < higBandWidth; c++)
            {
                hibandWeights[c] = maxweight - (c * step);
            }

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
            double[,] indices1 = this.GetLoMidHiSummaryIndexArrays(keys[0]);
            double[,] indices2 = this.GetLoMidHiSummaryIndexArrays(keys[1]);
            double[,] indices3 = this.GetLoMidHiSummaryIndexArrays(keys[2]);

            int width = indices1.GetLength(1);
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            var image = new Bitmap(width, height);
            var g = Graphics.FromImage(image);

            // get the low, mid and high band averages of indices in each minute.
            for (int i = 0; i < width; i++)
            {
                //  get the average of the three indices in the low bandwidth
                var index = (indices1[0, i] + indices2[0, i] + indices3[0, i]) / 3;
                Pen pen;
                if (double.IsNaN(index))
                {
                    pen = new Pen(Color.Gray);
                }
                else
                {
                    int red = (int)(255 * index);
                    if (red > 255)
                    {
                        red = 255;
                    }

                    index = (indices1[1, i] + indices2[1, i] + indices3[1, i]) / 3;
                    int grn = (int)(255 * index);
                    if (grn > 255)
                    {
                        grn = 255;
                    }

                    index = (indices1[2, i] + indices2[2, i] + indices3[2, i]) / 3;
                    int blu = (int)(255 * index);
                    if (blu > 255)
                    {
                        blu = 255;
                    }

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
            // calculate the reduction factor i.e. freq bins per pixel row
            int bandWidth = indices1.GetLength(0) / height;

            for (int i = 0; i < width; i++)
            {
                var spectrum1 = MatrixTools.GetColumn(indices1, i);
                var spectrum2 = MatrixTools.GetColumn(indices2, i);
                var spectrum3 = MatrixTools.GetColumn(indices3, i);
                for (int h = 0; h < height; h++)
                {
                    int start = h * bandWidth;
                    double[] subArray = DataTools.Subarray(spectrum1, start, bandWidth);
                    double index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int red = (int)(255 * index);
                    if (red > 255)
                    {
                        red = 255;
                    }

                    subArray = DataTools.Subarray(spectrum2, start, bandWidth);
                    index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int grn = (int)(255 * index);
                    if (grn > 255)
                    {
                        grn = 255;
                    }

                    subArray = DataTools.Subarray(spectrum3, start, bandWidth);
                    index = subArray.Average();
                    if (double.IsNaN(index))
                    {
                        index = 0.5;
                    }

                    int blu = (int)(255 * index);
                    if (blu > 255)
                    {
                        blu = 255;
                    }

                    image.SetPixel(i, h, Color.FromArgb(red, grn, blu));
                }
            }

            return image;
        }

        /// <summary>
        /// This method not currently called but might be useful in future
        /// </summary>
        public double[] GetSummaryIndicesWeightedAtDistance(double[,] normalisedIndex1, double[,] normalisedIndex2, double[,] normalisedIndex3, int minuteInDay, int distanceInMeters)
        {
            double decayConstant = 20.0;
            double[] indices1 = MatrixTools.GetColumn(normalisedIndex1, minuteInDay);
            indices1 = DataTools.reverseArray(indices1);
            indices1 = CalculateDecayedSpectralIndices(indices1, distanceInMeters, decayConstant);

            //double[] indices2 = MatrixTools.GetColumn(normalisedIndex2, minuteInDay);
            //indices2 = DataTools.reverseArray(indices2);
            //indices2 = CalculateDecayedSpectralIndices(indices2, distanceInMeters, decayConstant);

            //double[] indices3 = MatrixTools.GetColumn(normalisedIndex3, minuteInDay);
            //indices3 = DataTools.reverseArray(indices3);
            //indices3 = CalculateDecayedSpectralIndices(indices3, distanceInMeters, decayConstant);

            return indices1; // or indices2 or indices 3
        }

        public static double[] CalculateDecayedSpectralIndices(double[] spectralIndices, int distanceInMeters, double halfLife)
        {
            double log2 = Math.Log(2.0);
            double differentialFrequencyDecay = 0.1;

            int length = spectralIndices.Length;

            double[] returned = new double[length];
            for (int i = 0; i < length; i++)
            {
                // half life decreases with increasing frequency.
                // double frequencyDecay = differentialFrequencyDecay * i;
                double tau = (halfLife - (differentialFrequencyDecay * i)) / log2;

                // check tau is not negative
                if (tau < 0.0)
                {
                    tau = 0.001;
                }

                double exponent = distanceInMeters / tau;
                returned[i] = spectralIndices[i] * Math.Pow(Math.E, -exponent);
            }

            return returned;
        }

        //############################################################################################################################################################
        //# STATIC METHODS ###########################################################################################################################################
        //############################################################################################################################################################

        //========================================================================================================================================================
        //========= NEXT FEW METHODS ARE STATIC AND RETURN VARIOUS KINDS OF IMAGE
        //========================================================================================================================================================

        /// <summary>
        /// Frames a false-colourspectrogram.
        /// That is, it creates the title bar and the time scale. Also adds frequency grid lines to the image.
        /// Note that to call this method, the field cs.Freqscale MUST NOT = null.
        /// </summary>
        public static Image FrameFalseColourSpectrogram(Image bmp1, Image titleBar, LDSpectrogramRGB cs)
        {
            if (cs.FreqScale == null)
            {
                LoggedConsole.WriteErrorLine("Error: LDSpectrogramRGB.FreqScale==null. You cannot call this method. Call the overload.");
            }

            Image compositeBmp = FrameLDSpectrogram(bmp1, titleBar, cs, 0, 0);
            return compositeBmp;
        }

        /// <summary>
        /// Frames a false-colourspectrogram.
        /// Creates the title bar and the time scale. Also adds frequency grid lines to the image.
        /// Note that the 'nyquist' and 'hertzLinearGridInterval' arguments are used ONLY if the cs.Freqscale field==null.
        /// Also note that in this case, the frequency scale will be linear.
        /// </summary>
        public static Image FrameLDSpectrogram(Image bmp1, Image titleBar, LDSpectrogramRGB cs, int nyquist, int herzInterval)
        {
            var xAxisPixelDuration = cs.IndexCalculationDuration;
            var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            int trackHeight = 18;
            Bitmap timeBmp1 = ImageTrack.DrawTimeRelativeTrack(fullDuration, bmp1.Width, trackHeight);
            Bitmap timeBmp2 = (Bitmap)timeBmp1.Clone();
            Bitmap suntrack = null;

            DateTimeOffset? dateTimeOffset = cs.RecordingStartDate;
            if (dateTimeOffset.HasValue)
            {
                // draw extra time scale with absolute start time. AND THEN Do SOMETHING WITH IT.
                timeBmp2 = ImageTrack.DrawTimeTrack(fullDuration, cs.RecordingStartDate, bmp1.Width, trackHeight);
                suntrack = SunAndMoon.AddSunTrackToImage(bmp1.Width, dateTimeOffset, cs.SunriseDataFile);
            }

            if (cs.FreqScale == null)
            {
                // WARNING: The following will create a linear frequency scale.
                int frameSize = bmp1.Height;
                var freqScale = new FrequencyScale(nyquist, frameSize, herzInterval);
                cs.FreqScale = freqScale;
            }

            FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)bmp1, cs.FreqScale);

            // draw the composite bitmap
            var imageList = new List<Image> { titleBar, timeBmp1, bmp1, timeBmp2 };
            if (suntrack != null)
            {
                imageList.Add(suntrack);
            }

            var compositeBmp = (Bitmap)ImageTools.CombineImagesVertically(imageList);
            return compositeBmp;
        }

        public static Image DrawTitleBarOfGrayScaleSpectrogram(string title, int width)
        {
            var bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            var stringFont = new Font("Arial", 9);

            //string text = title;
            int x = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(x, 3));

            var stringSize = g.MeasureString(title, stringFont);
            x += stringSize.ToSize().Width + 70;
            var text = $"SCALE:(time x kHz)   {Meta.OrganizationTag}";
            stringSize = g.MeasureString(text, stringFont);
            int x2 = width - stringSize.ToSize().Width - 2;
            if (x2 > x)
            {
                g.DrawString(text, stringFont, Brushes.Wheat, new PointF(x2, 3));
            }

            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0); //draw upper boundary

            // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }

        public static Image DrawTitleBarOfFalseColourSpectrogram(string title, int width)
        {
            var bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            // Font stringFont = new Font("Tahoma", 9);
            var stringFont = new Font("Arial", 9, FontStyle.Bold);

            int x = 2;
            g.DrawString(title, stringFont, Brushes.White, new PointF(x, 3));

            var stringSize = g.MeasureString(title, stringFont);
            x += stringSize.ToSize().Width + 300;

            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            // Draw colour chart on title bar. Discontinued this because not helpful.
            //var colourChart = LDSpectrogramRGB.DrawColourScale(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 2);
            //g.DrawImage(colourChart, X, 1);

            var text = $"SCALE:(time x kHz)        {Meta.OrganizationTag}";
            stringSize = g.MeasureString(text, stringFont);
            int x2 = width - stringSize.ToSize().Width - 2;
            if (x2 > x)
            {
                g.DrawString(text, stringFont, Brushes.Wheat, new PointF(x2, 3));
            }

            // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0); //draw upper boundary
            return bmp;
        }

        public static Image DrawRgbColourMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColour)
        {
            // assume all matricies are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            var bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            const int maxRgbValue = 255;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    var d1 = redM[row, column];
                    var d2 = grnM[row, column];
                    var d3 = bluM[row, column];

                    // blank indices painted grey.
                    if (double.IsNaN(d1))
                    {
                        d1 = 0.5;
                    }

                    if (double.IsNaN(d2))
                    {
                        d2 = 0.5;
                    }

                    if (double.IsNaN(d3))
                    {
                        d3 = 0.5;
                    }

                    // enhance blue colour - it is difficult to see on a black background
                    // This is a hack - there should be a principled way to do this.
                    if ((d1 < 0.1) && (d2 < 0.1) && (d3 > 0.2))
                    {
                        d2 += 0.7 * d3;
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

                    var v1 = Convert.ToInt32(Math.Max(0, d1 * maxRgbValue));
                    var v2 = Convert.ToInt32(Math.Max(0, d2 * maxRgbValue));
                    var v3 = Convert.ToInt32(Math.Max(0, d3 * maxRgbValue));
                    var colour = Color.FromArgb(v1, v2, v3);
                    bmp.SetPixel(column, row, colour);
                } //end all columns
            }

            return bmp;
        }

        /// <summary>
        /// A technique to derive a spectrogram from four different indices
        /// same as above method but multiply index value by the amplitude value instead of squaring the value
        /// </summary>
        public static Image DrawFourColourSpectrogram(double[,] redM, double[,] grnM, double[,] bluM, double[,] greM, bool doReverseColour)
        {
            // assume all matrices are normalised and of the same dimensions
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number

            var bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            int maxRgbValue = 255;

            for (int row = 0; row < rows; row++)
            {
                // note that the matrix values are multiplied by the grey matrix values.
                for (int column = 0; column < cols; column++)
                {
                    double gv = Math.Sqrt(greM[row, column]);
                    var d1 = redM[row, column] * gv;
                    var d2 = grnM[row, column] * gv;
                    var d3 = bluM[row, column] * gv;
                    if (doReverseColour)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }

                    var v1 = Convert.ToInt32(Math.Max(0, d1 * maxRgbValue));
                    var v2 = Convert.ToInt32(Math.Max(0, d2 * maxRgbValue));
                    var v3 = Convert.ToInt32(Math.Max(0, d3 * maxRgbValue));
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
        public static Image DrawColourScale(int maxScaleLength, int ht)
        {
            int width = maxScaleLength / 7;
            if (width > ht)
            {
                width = ht;
            }
            else if (width < 3)
            {
                width = 3;
            }

            Bitmap colorScale = new Bitmap(8 * width, ht);
            Graphics gr = Graphics.FromImage(colorScale);
            int offset = width + 1;
            if (width < 5)
            {
                offset = width;
            }

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
            return colorScale;
        }

        /// <summary>
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS
        /// IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE.
        /// WARNING: Make sure the parameters in the CONFIG file are consistent with the CSV files.
        /// </summary>
        /// <param name="inputDirectory">inputDirectory</param>
        /// <param name="outputDirectory">outputDirectory</param>
        /// <param name="ldSpectrogramConfig">config for drawing FCSs</param>
        /// <param name="indexPropertiesConfigPath">The indices Config Path. </param>
        /// <param name="indexGenerationData">indexGenerationData</param>
        /// <param name="basename">stem name of the original recording</param>
        /// <param name="analysisType">will usually be "Towsey.Acoustic"</param>
        /// <param name="indexSpectrograms">Optional spectra to pass in. If specified the spectra will not be loaded from disk! </param>
        /// <param name="summaryIndices">an array of summary index results</param>
        /// <param name="indexStatistics">Info about the distributions of the spectral statistics</param>
        /// <param name="siteDescription">Optionally specify details about the site where the audio was recorded.</param>
        /// <param name="sunriseDataFile">This is only available for locations near Brisbane, Austalia.</param>
        /// <param name="segmentErrors">Note that these segment errors were derived from previous analysis of the summary indices.</param>
        /// <param name="imageChrome">If true, this method generates and returns separate chromeless images used for tiling website images.</param>
        /// <param name="verbose">default is false</param>
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
            Dictionary<string, IndexDistributions.SpectralStats> indexStatistics = null,
            SiteDescription siteDescription = null,
            FileInfo sunriseDataFile = null,
            List<GapsAndJoins> segmentErrors = null,
            ImageChrome imageChrome = ImageChrome.With,
            bool verbose = false)
        {
            var config = ldSpectrogramConfig;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = config.ColorMap1 ?? DefaultColorMap1;   // assigns indices to RGB
            string colorMap2 = config.ColorMap2 ?? DefaultColorMap2;   // assigns indices to RGB

            // Set ColourFilter: Must lie between +/-1. A good value is -0.25
            if (config.ColourFilter == null)
            {
                config.ColourFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            }

            var cs1 = new LDSpectrogramRGB(config, indexGenerationData, colorMap1);
            string fileStem = basename;

            cs1.FileName = fileStem;

            // we do not necessarily want to keep previous BackgroundFilter
            // cs1.BackgroundFilter = indexGenerationData.BackgroundFilterCoeff;
            cs1.SiteName = siteDescription?.SiteName;
            cs1.Latitude = siteDescription?.Latitude;
            cs1.Longitude = siteDescription?.Longitude;
            cs1.SunriseDataFile = sunriseDataFile;
            cs1.ErroneousSegments = segmentErrors;

            // calculate start time by combining DatetimeOffset with minute offset.
            cs1.StartOffset = indexGenerationData.AnalysisStartOffset;
            if (indexGenerationData.RecordingStartDate.HasValue)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                cs1.RecordingStartDate = dto;
                if (dto != null)
                {
                    cs1.StartOffset = dto.TimeOfDay + cs1.StartOffset;
                }
            }

            // following line is debug purposes only
            //cs.StartOffset = cs.StartOffset + TimeSpan.FromMinutes(15);

            // Get and set the dictionary of index properties
            Dictionary<string, IndexProperties> dictIp = IndexProperties.GetIndexProperties(indexPropertiesConfigPath);
            dictIp = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIp);
            cs1.SetSpectralIndexProperties(dictIp);

            // Load the Index Spectrograms into a Dictionary
            if (indexSpectrograms == null)
            {
                var sw = Stopwatch.StartNew();
                if (verbose)
                {
                    Logger.Info("Reading spectra files from disk");
                }

                // reads all known files spectral indices
                cs1.ReadCsvFiles(inputDirectory, fileStem, cs1.SpectrogramKeys);

                //var now2 = DateTime.Now;
                sw.Stop();
                if (verbose)
                {
                    LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds");
                }
            }
            else
            {
                if (verbose)
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

            // If index distribution statistics has not been passed, then read from json file.
            if (indexStatistics == null)
            {
                indexStatistics = IndexDistributions.ReadSpectralIndexDistributionStatistics(inputDirectory, fileStem);

                if (indexStatistics == null)
                {
                    Log.Warn("A .json file of Index Distribution Statistics was not found in dir: <" + outputDirectory.FullName + ">");
                    Log.Warn("        This is not required in most cases. Only required if doing \"difference\" spectrograms.");

                    //throw new InvalidOperationException("Cannot proceed without index distribution data");
                }
            }

            cs1.IndexStats = indexStatistics;

            // draw gray scale spectrogram for each index.
            string[] keys = colorMap1.Split('-');
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);
            keys = colorMap2.Split('-');
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);

            // create and save first false-colour spectrogram image
            var image1NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap1);
            Image image1 = null;
            if (image1NoChrome == null)
            {
                LoggedConsole.WriteErrorLine("ERROR: Null spectrogram image");
            }
            else
            {
                image1 = SpectrogramFraming(cs1, image1NoChrome);
                var outputPath1 = FilenameHelpers.AnalysisResultPath(outputDirectory, cs1.FileName, colorMap1, "png");
                image1.Save(outputPath1);
            }

            // create and save second false-colour spectrogram image
            var image2NoChrome = cs1.DrawFalseColourSpectrogramChromeless(cs1.ColorMode, colorMap2);
            Image image2 = null;
            if (image2NoChrome == null)
            {
                LoggedConsole.WriteErrorLine("ERROR: Null spectrogram image");
            }
            else
            {
                cs1.ColorMap = colorMap2;
                image2 = SpectrogramFraming(cs1, image2NoChrome);
                var outputPath2 = FilenameHelpers.AnalysisResultPath(outputDirectory, cs1.FileName, colorMap2, "png");
                image2.Save(outputPath2);
            }

            // read high amplitude and clipping info into an image
            Image imageX;
            if (summaryIndices == null)
            {
                string indicesFile = FilenameHelpers.AnalysisResultPath(
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

            imageX?.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, ".ClipHiAmpl", "png"));

            if (image1 == null || image2 == null)
            {
                throw new Exception("NULL image returned. Cannot proceed!");
            }

            CreateTwoMapsImage(outputDirectory, fileStem, image1, imageX, image2);

            //ribbon = cs.GetSummaryIndexRibbon(colorMap1);
            var ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap1);

            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap1 + ".SummaryRibbon", "png"));

            //ribbon = cs.GetSummaryIndexRibbon(colorMap2);
            ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap2);
            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap2 + ".SummaryRibbon", "png"));

            ribbon = cs1.GetSpectrogramRibbon(colorMap1, 32);
            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap1 + ".SpectralRibbon", "png"));
            ribbon = cs1.GetSpectrogramRibbon(colorMap2, 32);
            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap2 + ".SpectralRibbon", "png"));

            // only return images if chromeless
            return imageChrome == ImageChrome.Without
                       ? new[] { Tuple.Create(image1NoChrome, colorMap1), Tuple.Create(image2NoChrome, colorMap2) }
                       : null;
        }

        /// <summary>
        /// This method CHROMES the passed spectrogram.
        /// Chroming means to add all the trimmings, title bar axis labels, grid lines, cinnamon powder, etc.
        /// </summary>
        private static Image SpectrogramFraming(LDSpectrogramRGB cs, Image imageSansChrome)
        {
            string startTime = $"{cs.StartOffset.Hours:d2}{cs.StartOffset.Minutes:d2}h";

            // then pass that image into chromer
            int nyquist = cs.SampleRate / 2;
            string title = $"<{cs.ColorMap}> SPECTROGRAM  of \"{cs.FileName}\".   Starts at {startTime}; Nyquist={nyquist}";
            var titleBar = DrawTitleBarOfFalseColourSpectrogram(title, imageSansChrome.Width);
            var image = FrameFalseColourSpectrogram(imageSansChrome, titleBar, cs);
            return image;
        }

        private static void CreateTwoMapsImage(DirectoryInfo outputDirectory, string fileStem, Image image1, Image imageX, Image image2)
        {
            var imageList = new[] { image1, imageX, image2 };
            Image image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);
        }
    }
}
