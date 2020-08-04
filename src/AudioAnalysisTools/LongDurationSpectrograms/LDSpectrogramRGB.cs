// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogramRGB.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   This class generates false-color spectrograms of long duration audio recordings.
//   Important properties are:
//   1) the color map which maps three acoustic indices to RGB.
//   2) The scale of the x and y axes which are determined by the sample rate, frame size etc.
//   In order to create false-color spectrograms, copy the following method:
//                                           public static void DrawFalseColorSpectrograms(LDSpectrogramConfig configuration)
//   All the arguments can be passed through a config file.
//   Create the config file through an instance of the class LDSpectrogramConfig and then call config.WritConfigToYAML(FileInfo path).
//   Then pass that path to the above static method.
//
//  Activity Codes for other tasks to do with spectrograms and audio files:
//
// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-color spectrograms.
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;
    using AnalysisBase.ResultBases;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.StandardSpectrograms;
    using log4net;
    using SixLabors.Fonts;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Path = System.IO.Path;

    /// <summary>
    /// This class generates false-color spectrograms of long duration audio recordings.
    /// Important properties are:
    /// 1) the color map which maps three acoustic indices to RGB.
    /// 2) The scale of the x and y axes which are determined by the sample rate, frame size etc.
    /// In order to create false color spectrograms, copy the method
    ///         public static void DrawFalseColorSpectrograms(LDSpectrogramConfig configuration)
    /// All the arguments can be passed through a config file.
    /// Create the config file through an instance of the class LDSpectrogramConfig
    /// and then call config.WritConfigToYAML(FileInfo path).
    /// Then pass that path to the above static method.
    /// </summary>
    public class LDSpectrogramRGB
    {
        // Below is some history about how indices were assigned to the RGB channels to make long-duration false-colour spectrograms
        // string[] keys = { "ACI", "TEN", "CVR", "BGN", "AVG", "VAR" }; // the OLDEST default i.e. used in 2014
        // string[] keys = { "ACI", "ENT", "EVN", "BGN", "POW", "EVN" }; // the OLD default i.e. since July 2015
        // More recently (2018 onwards) other combinations have been used expecially for the blue channel index.
        // public static readonly string DefaultColorMap1 = "ACI, ENT, EVN";
        // public static readonly string DefaultColorMap2 = "BGN, PMN, SPT";

        // the defaults
        public static readonly string DefaultColorMap1 = SpectrogramConstants.RGBMap_ACI_ENT_EVN;
        public static readonly string DefaultColorMap2 = SpectrogramConstants.RGBMap_BGN_PMN_SPT;

        public static string[] GetArrayOfAvailableKeys()
        {
            // Before May 2017, only the required six spectral indices were incorporated in a dictionary of spectral matrices.
            // Since May 2017, all the likely available matrices are incorporated into a dictionary. Note the new name for PMN, previously POW.
            // Note: RHZ, SPT and CVR tend to be correlated with PMN and do not add much.
            return SpectralIndexValues.Keys;
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
        /// No Arguments CONSTRUCTOR.
        /// </summary>
        public LDSpectrogramRGB()
        {
            this.ColorMode = "NEGATIVE"; // the default
            this.BackgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // default BackgroundFilter value
            this.SampleRate = SpectrogramConstants.SAMPLE_RATE; // default recording starts at midnight
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH; // default value - from which spectrogram was derived
            this.XTicInterval = SpectrogramConstants.X_AXIS_TIC_INTERVAL; // default = one minute spectra and hourly time lines
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;
            this.SpectrogramKeys = GetArrayOfAvailableKeys();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramRGB"/> class.
        /// CONSTRUCTOR.
        /// </summary>
        public LDSpectrogramRGB(TimeSpan xScale, int sampleRate, string colorMap)
        {
            this.ColorMode = "NEGATIVE"; // the default
            this.BackgroundFilter = SpectrogramConstants.BACKGROUND_FILTER_COEFF; // default BackgroundFilter value
            this.SampleRate = sampleRate;

            // assume default linear Hertz scale
            this.FrameWidth = SpectrogramConstants.FRAME_LENGTH;
            this.FreqScale = new FrequencyScale(nyquist: this.SampleRate / 2, frameSize: this.FrameWidth, hertzGridInterval: 1000);

            // set the X and Y axis scales for the spectrograms
            this.XTicInterval = xScale;
            this.SampleRate = sampleRate;
            this.ColorMap = colorMap;
            this.StartOffset = SpectrogramConstants.MINUTE_OFFSET;

            // IMPORTANT NOTE: If an IndexPropertiesConfig file is available, these default keys are later over-written in the method
            // SetSpectralIndexProperties(Dictionary < string, IndexProperties > dictionaryOfSpectralIndexProperties)
            // Consequently the INDEX names in DefaultKeys should match those in IndexPropertiesConfig file. If they do not, consequences are undefined!
            this.SpectrogramKeys = GetArrayOfAvailableKeys();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LDSpectrogramRGB"/> class.
        /// CONSTRUCTOR
        /// WARNING: Ths will create a linear Hertz scale spectrogram.
        /// </summary>
        /// <param name="minuteOffset">minute of day at which the spectrogram starts.</param>
        /// <param name="xScale">time scale : pixels per hour.</param>
        /// <param name="sampleRate">recording sample rate which also determines scale of Y-axis.</param>
        /// <param name="frameWidth">frame size - which also determines scale of Y-axis.</param>
        /// <param name="colorMap">acoustic indices used to assign  the three color mapping.</param>
        public LDSpectrogramRGB(TimeSpan minuteOffset, TimeSpan xScale, int sampleRate, int frameWidth, string colorMap)
            : this(xScale, sampleRate, colorMap)
        {
            this.StartOffset = minuteOffset;
            this.FrameWidth = frameWidth;
        }

        public LDSpectrogramRGB(LdSpectrogramConfig config, IndexGenerationData indexGenerationData, string colorMap)
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

            // the Hertz scale
            int nyquist = indexGenerationData.SampleRateResampled / 2;
            int yAxisTicInterval = config.YAxisTicInterval;

            int frameSize = indexGenerationData.FrameLength;
            FreqScaleType fst;
            switch (config.FreqScale)
            {
                case "Linear":
                    this.FreqScale = new FrequencyScale(nyquist, frameSize, hertzGridInterval: yAxisTicInterval);
                    break;
                case "Mel":
                    fst = FreqScaleType.Mel;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Mel Scale is not yet implemented");

                //break;
                case "Linear62Octaves7Tones31Nyquist11025":
                    fst = FreqScaleType.Linear62OctaveTones31Nyquist11025;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Linear62Octaves7Tones31Nyquist11025 Scale is not yet implemented");

                //break;
                case "Linear125Octaves6Tones30Nyquist11025":
                    fst = FreqScaleType.Linear125OctaveTones30Nyquist11025;
                    this.FreqScale = new FrequencyScale(fst);
                    break;
                case "Octaves24Nyquist32000":
                    fst = FreqScaleType.Octaves24Nyquist32000;
                    this.FreqScale = new FrequencyScale(fst);
                    throw new ArgumentException("Octaves24Nyquist32000 Scale is not yet implemented");

                //break;
                case "Linear125Octaves7Tones28Nyquist32000":
                    fst = FreqScaleType.Linear125OctaveTones28Nyquist32000;
                    this.FreqScale = new FrequencyScale(fst);
                    break;
                default:
                    throw new ArgumentException($"{config.FreqScale} is an unknown option for drawing a frequency scale");
            }

            this.ColorMode = "NEGATIVE"; // the default
            this.ColorMap = colorMap;
        }

        public string[] SpectrogramKeys { get; private set; }

        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the current LDspectrogram starts
        /// This can be used to correctly.
        /// </summary>
        public DateTimeOffset RecordingStartDate { get; set; }

        public string SiteName { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the time at which the current LDFC spectrogram starts.
        /// </summary>
        public TimeSpan StartOffset { get; set; }

        /// <summary>
        /// Gets or sets the temporal duration of one sub-segment interval for which indices are calculated.
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

        /// <summary>
        /// Gets the 1 kHz intervals.
        /// </summary>
        public int GetYinterval
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
        /// Gets or sets POSITIVE or NEGATIVE.
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
        /// Have not attempted to deal with leap years!.
        /// </summary>
        [Obsolete]
        public FileInfo SunriseDataFile { get; set; }

        public Dictionary<string, IndexProperties> GetSpectralIndexProperties()
        {
            return this.spectralIndexProperties;
        }

        /// <summary>
        /// This method sets default indices to use if passed Dictionary = null.
        /// This may not be a good idea. Trying it out. Maybe better to crash!.
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
                    double[,] matrix = IndexMatrices.ReadSpectrogram(file, out var freqBinCount);
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
            this.spgrStdDevMatrices = IndexMatrices.ReadSpectrogramCsvFiles(ipdir, fileName, this.ColorMap, out var freqBinCount);
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
        /// Call this method to access a spectrogram matrix.
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
                // get the stats for this key
                var stats = this.IndexStats[key];
                if (indexProperties.CalculateNormBounds)
                {
                    // FIRST CALCULATE THE MIN BOUND
                    // By default the minimum bound is set slightly below the modal value of the index.
                    minBound = stats.Mode - (stats.StandardDeviation * 0.1);

                    // case where mode = min. Usually this occurs when mode = 0.0.
                    if (stats.Mode < 0.001)
                    {
                        var binWidth = (stats.Maximum - stats.Minimum) / stats.Distribution.Length;
                        minBound += binWidth;
                    }

                    // fix case where signal is defective &= zero. We do not want ACI min ever set too low.
                    if (key.Equals("ACI") && minBound < 0.3)
                    {
                        minBound = indexProperties.NormMin;
                    }

                    // Do not want OSC min set too low. Happens because min can = zero
                    if (key.Equals("OSC") && minBound < indexProperties.NormMin)
                    {
                        minBound = indexProperties.NormMin;
                    }

                    // NOW CALCULATE THE MAX BOUND
                    stats.GetValueOfNthPercentile(IndexDistributions.UpperPercentileDefault, out int _, out maxBound);

                    // correct for case where max bound = zero. This can happen where ICD is very short i.e. 0.1s.
                    if (maxBound < 0.0001)
                    {
                        maxBound = stats.Maximum * 0.1;
                    }
                }

                // In some rare cases the resulting range is zero which will produce NaNs when normalized.
                // In this case we just reset the bounds backs to the defaults in the config file.
                // ReSharper disable once CompareOfFloatsByEqualityOperator - we are interested in ranges that are exactly zero distance
                if (maxBound == minBound)
                {
                    minBound = indexProperties.NormMin;
                    maxBound = indexProperties.NormMax;
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
        /// draws only those spectrograms in the passed array of keys.
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

                var ipList = this.GetSpectralIndexProperties();
                if (!ipList[key].DoDisplay)
                {
                    continue;
                }

                var bmp = this.DrawGreyscaleSpectrogramOfIndex(key);

                var header = Drawing.NewImage(bmp.Width, 20, Color.LightGray);
                header.Mutate(g =>
                {
                    g.DrawTextSafe(key, Drawing.Tahoma9, Color.Black, new PointF(4, 4));
                });

                var indexImage = ImageTools.CombineImagesVertically(header, bmp);

                // save the image - the directory for the path must exist
                var path = FilenameHelpers.AnalysisResultPath(opdir, opFileName, key, "png");
                indexImage?.Save(path);
            }
        }

        /// <summary>
        /// Assume calling method has done all the reality checks.</summary>
        public Image<Rgb24> DrawGreyscaleSpectrogramOfIndex(string key)
        {
            var matrix = this.GetNormalisedSpectrogramMatrix(key);
            if (matrix == null)
            {
                return null;
            }

            var bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(matrix);
            var xAxisPixelDuration = this.IndexCalculationDuration;
            var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);

            SpectrogramTools.DrawGridLinesOnImage((Image<Rgb24>)bmp, this.StartOffset, fullDuration, xAxisPixelDuration, this.FreqScale);
            const int trackHeight = 20;
            var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, this.RecordingStartDate, bmp.Width, trackHeight);
            var returnImage = ImageTools.CombineImagesVertically(bmp, timeBmp);
            return returnImage;
        }

        public void DrawNegativeFalseColorSpectrogram(DirectoryInfo outputDirectory, string outputFileName, double blueEnhanceParameter)
        {
            var bmpNeg = this.DrawFalseColorSpectrogramChromeless("NEGATIVE", this.ColorMap, blueEnhanceParameter);
            if (bmpNeg == null)
            {
                LoggedConsole.WriteLine("WARNING: From method ColorSpectrogram.DrawNegativeFalseColorSpectrograms()");
                LoggedConsole.WriteLine("         Null image returned");
                return;
            }

            bmpNeg.Save(Path.Combine(outputDirectory.FullName, outputFileName + ".COLNEG.png"));

            string key = "BGN";
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
        /// However it does add in notated error segments.
        /// </summary>
        public Image<Rgb24> DrawFalseColorSpectrogramChromeless(string colorMode, string colorMap, double blueEnhanceParameter)
        {
            if (!this.ContainsMatrixForKeys(colorMap))
            {
                LoggedConsole.WriteLine("WARNING: From method ColourSpectrogram.DrawFalseColorSpectrogram()");
                LoggedConsole.WriteLine("         No matrices are available for the passed color map.");
                return null;
            }

            string[] rgbMap = colorMap.Split('-');

            // NormalizeMatrixValues spectrogram values and de-emphasize the low background values
            var redMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[0]);
            var grnMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[1]);
            var bluMatrix = this.GetNormalisedSpectrogramMatrix(rgbMap[2]);
            bool doReverseColor = colorMode.StartsWith("POS");

            var bmp = DrawRgbColorMatrix(redMatrix, grnMatrix, bluMatrix, doReverseColor, blueEnhanceParameter);

            if (bmp == null)
            {
                LoggedConsole.WriteWarnLine($" No image returned for ColorMap: {colorMap}!");
            }

            // now add in image patches for possible erroneous index segments
            bool errorsExist = this.ErroneousSegments != null && this.ErroneousSegments.Count > 0;
            if (errorsExist)
            {
                bmp = GapsAndJoins.DrawErrorSegments(bmp, this.ErroneousSegments, false);
            }

            return bmp;
        }

        public Image<Rgb24> DrawBlendedFalseColourSpectrogram(string colorMap1, string colorMap2, double blendWt1, double blendWt2)
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

            var blueEnhanceParameter = 0.0;
            var bmp = DrawRgbColorMatrix(redMatrix, grnMatrix, bluMatrix, false, blueEnhanceParameter);
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

        public Image<Rgb24> DrawDoubleSpectrogram(Image bmp1, Image bmp2, string colorMode)
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

            var compositeBmp = new Image<Rgb24>(bmp2.Width, imageHt); //get canvas for entire image
            compositeBmp.Mutate(gr =>
            {
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
                gr.DrawImage(scale, new Point(xLocation, 1), 1f); //dra
            });
            return compositeBmp;
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
        public static Image<Rgb24> FrameFalseColourSpectrogram(Image<Rgb24> bmp1, Image<Rgb24> titleBar, LDSpectrogramRGB cs)
        {
            if (cs.FreqScale == null)
            {
                LoggedConsole.WriteErrorLine("Error: LDSpectrogramRGB.FreqScale==null. You cannot call this method. Call the overload.");
            }

            var compositeBmp = FrameLDSpectrogram(bmp1, titleBar, cs, 0, 0);
            return compositeBmp;
        }

        /// <summary>
        /// Frames a false-color spectrogram.
        /// Creates the title bar and the time scale. Also adds frequency grid lines to the image.
        /// Note that the 'nyquist' and 'hertzGridInterval' arguments are used ONLY if the cs.FreqScale field==null.
        /// Also note that in this case, the frequency scale will be linear.
        /// </summary>
        public static Image<Rgb24> FrameLDSpectrogram(Image<Rgb24> bmp1, Image<Rgb24> titleBar, LDSpectrogramRGB cs, int nyquist, int hertzInterval)
        {
            var xAxisPixelDuration = cs.IndexCalculationDuration;
            var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            int trackHeight = 18;
            Image<Rgb24> timeBmp1 = ImageTrack.DrawTimeRelativeTrack(fullDuration, bmp1.Width, trackHeight);
            Image<Rgb24> timeBmp2 = (Image<Rgb24>)timeBmp1.Clone();
            DateTimeOffset? dateTimeOffset = cs.RecordingStartDate;
            if (dateTimeOffset.HasValue)
            {
                // draw extra time scale with absolute start time. AND THEN DO SOMETHING WITH IT.
                timeBmp2 = ImageTrack.DrawTimeTrack(fullDuration, cs.RecordingStartDate, bmp1.Width, trackHeight);
            }

            if (cs.FreqScale == null)
            {
                // WARNING: The following will create a linear frequency scale.
                int frameSize = bmp1.Height;
                var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);
                cs.FreqScale = freqScale;
            }

            FrequencyScale.DrawFrequencyLinesOnImage((Image<Rgb24>)bmp1, cs.FreqScale, includeLabels: true);

            // draw the composite bitmap
            var compositeBmp = ImageTools.CombineImagesVertically(new List<Image<Rgb24>> { titleBar, timeBmp1, bmp1, timeBmp2 });
            return compositeBmp;
        }

        public static Image<Rgb24> DrawTitleBarOfGrayScaleSpectrogram(string title, int width)
        {
            var bmp = new Image<Rgb24>(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            bmp.Mutate(g =>
            {
                g.Clear(Color.Black);
                var stringFont = Drawing.Arial9;

                //string text = title;
                int x = 4;
                g.DrawTextSafe(title, stringFont, Color.Wheat, new PointF(x, 3));

                var stringSize = g.MeasureString(title, stringFont);
                x += stringSize.ToSize().Width + 70;
                var text = $"SCALE:(time x kHz)   {Meta.OrganizationTag}";
                stringSize = g.MeasureString(text, stringFont);
                int x2 = width - stringSize.ToSize().Width - 2;
                if (x2 > x)
                {
                    g.DrawTextSafe(text, stringFont, Color.Wheat, new PointF(x2, 3));
                }

                g.DrawLine(new Pen(Color.Gray, 1), 0, 0, width, 0); //draw upper boundary
            });

            // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            return bmp;
        }

        /// <summary>
        /// Assume calling method has done all the reality checks.
        /// Assume the Index Calculation Duration = 60 seconds.
        /// </summary>
        public static Image DrawGreyscaleSpectrogramOfIndex(string key, double[,] matrix)
        {
            var bmp = ImageTools.DrawReversedMatrixWithoutNormalisation(matrix);
            var xAxisPixelDuration = TimeSpan.FromSeconds(60);
            var fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp.Width);
            var freqScale = new FrequencyScale(11025, 512, 1000);

            SpectrogramTools.DrawGridLinesOnImage((Image<Rgb24>)bmp, TimeSpan.Zero, fullDuration, xAxisPixelDuration, freqScale);
            const int trackHeight = 20;
            var recordingStartDate = default(DateTimeOffset);
            var timeBmp = ImageTrack.DrawTimeTrack(fullDuration, recordingStartDate, bmp.Width, trackHeight);
            var array = new Image<Rgb24>[2];
            array[0] = bmp;
            array[1] = timeBmp;
            var returnImage = ImageTools.CombineImagesVertically(array);
            return returnImage;
        }

        public static Image<Rgb24> DrawTitleBarOfFalseColourSpectrogram(string title, int width)
        {
            var bmp = new Image<Rgb24>(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            bmp.Mutate(g =>
            {
                g.Clear(Color.Black);

                // Font stringFont = Drawing.Tahoma9;
                var stringFont = Drawing.Arial9Bold;

                int x = 2;
                g.DrawTextSafe(title, stringFont, Color.White, new PointF(x, 3));

                var stringSize = TextMeasurer.Measure(title, new RendererOptions(stringFont));
                x += (int)stringSize.Width + 300;

                var text = $"SCALE:(time x kHz)        {Meta.OrganizationTag}";
                var stringSize2 = g.MeasureString(text, stringFont);
                int x2 = width - stringSize2.ToSize().Width - 2;
                if (x2 > x)
                {
                    g.DrawTextSafe(text, stringFont, Color.Wheat, new PointF(x2, 3));
                }
            });

            return bmp;
        }

        /// <summary>
        /// This method assumes that all the passed matrices are normalised and of the same dimensions.
        /// The method implements a hack to enhance the blue color because the human eye is less sensitive to blue.
        /// If there is a problem with one or more of the three rgb values, a gray pixel is substituted not a black pixel.
        /// Black is a frequent color in LDFC spectrograms, but gray is highly unlikely,
        /// and therefore its presence stands out as indicating an error in one or more of the rgb values.
        /// </summary>
        public static Image<Rgb24> DrawRgbColorMatrix(double[,] redM, double[,] grnM, double[,] bluM, bool doReverseColor, double blueEnhanceParameter)
        {
            int rows = redM.GetLength(0); //number of rows
            int cols = redM.GetLength(1); //number
            var bmp = new Image<Rgb24>(cols, rows);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < cols; column++)
                {
                    var r = redM[row, column];
                    var g = grnM[row, column];
                    var b = bluM[row, column];

                    // if any of the indices is NaN then render as grey.
                    if (double.IsNaN(r) || double.IsNaN(g) || double.IsNaN(b))
                    {
                        r = 0.5;
                        g = 0.5;
                        b = 0.5;
                    }

                    // Enhance blue color where it is difficult to see on a black background.
                    // Go for a more visible pale cyan color.
                    if (r < 0.1 && g < 0.1 && b > 0.1 && blueEnhanceParameter > 0.0)
                    {
                        r += blueEnhanceParameter * 0.5 * (b - r);
                        g += blueEnhanceParameter * (b - g);
                        b += 0.1;

                        // check for values over 1.0
                        //g = Math.Min(1.0, g);
                        b = Math.Min(1.0, b);
                    }

                    if (doReverseColor)
                    {
                        r = 1 - r;
                        g = 1 - g;
                        b = 1 - b;
                    }

                    var v1 = r.ScaleUnitToByte();
                    var v2 = g.ScaleUnitToByte();
                    var v3 = b.ScaleUnitToByte();
                    var color = Color.FromRgb(v1, v2, v3);
                    bmp[column, row] = color;
                } //end all columns
            }

            return bmp;
        }

        /// <summary>
        /// A technique to derive a spectrogram from four different indices
        /// same as above method but multiply index value by the amplitude value instead of squaring the value.
        /// </summary>
        public static Image DrawFourColorSpectrogram(double[,] redM, double[,] grnM, double[,] bluM, double[,] greM, bool doReverseColor)
        {
            // assume all matrices are normalised and of the same dimensions
            int rows = redM.GetLength(0);
            int cols = redM.GetLength(1);

            var bmp = new Image<Rgb24>(cols, rows);
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
                    if (doReverseColor)
                    {
                        d1 = 1 - d1;
                        d2 = 1 - d2;
                        d3 = 1 - d3;
                    }

                    var v1 = Convert.ToByte(Math.Max(0, d1 * maxRgbValue));
                    var v2 = Convert.ToByte(Math.Max(0, d2 * maxRgbValue));
                    var v3 = Convert.ToByte(Math.Max(0, d3 * maxRgbValue));
                    Color colour = Color.FromRgb(v1, v2, v3);
                    bmp[column, row] = colour;
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
        /// Returns an image of an array of color patches.
        /// It shows the three primary colors and pairwise combinations.
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

            Image<Rgb24> colorScale = new Image<Rgb24>(8 * width, ht);
            colorScale.Mutate(gr =>
            {

                int offset = width + 1;
                if (width < 5)
                {
                    offset = width;
                }

                Image<Rgb24> colorBmp = new Image<Rgb24>(width - 1, ht);
                colorBmp.Mutate(gr2 =>
                {

                    Color c = Color.FromRgb(250, 15, 250);
                    gr2.Clear(c);
                    int x = 0;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);
                    c = Color.FromRgb(250, 15, 15);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);

                    //yellow
                    c = Color.FromRgb(250, 250, 15);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);

                    //green
                    c = Color.FromRgb(15, 250, 15);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);

                    // pale blue
                    c = Color.FromRgb(15, 250, 250);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);

                    // blue
                    c = Color.FromRgb(15, 15, 250);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);

                    // purple
                    c = Color.FromRgb(250, 15, 250);
                    gr2.Clear(c);
                    x += offset;
                    gr.DrawImage(colorBmp, new Point(x, 0), 1);
                });
            });

            return colorScale;
        }

        /// <summary>
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS
        /// IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE.
        /// WARNING: Make sure the parameters in the CONFIG file are consistent with the CSV files.
        /// </summary>
        /// <param name="inputDirectory">inputDirectory.</param>
        /// <param name="outputDirectory">outputDirectory.</param>
        /// <param name="ldSpectrogramConfig">config for drawing FCSs.</param>
        /// <param name="indexPropertiesConfigPath">The indices Config Path. </param>
        /// <param name="indexGenerationData">indexGenerationData.</param>
        /// <param name="basename">stem name of the original recording.</param>
        /// <param name="analysisType">will usually be "Towsey.Acoustic".</param>
        /// <param name="indexSpectrograms">Optional spectra to pass in. If specified the spectra will not be loaded from disk.</param>
        /// <param name="summaryIndices">an array of summary index results.</param>
        /// <param name="indexStatistics">Info about the distributions of the spectral statistics.</param>
        /// <param name="siteDescription">Optionally specify details about the site where the audio was recorded.</param>
        /// <param name="sunriseDataFile">This is only available for locations near Brisbane, Austalia.</param>
        /// <param name="segmentErrors">Note that these segment errors were derived from previous analysis of the summary indices.</param>
        /// <param name="imageChrome">If true, this method generates and returns separate chromeless images used for tiling website images.</param>
        public static Tuple<Image<Rgb24>, string>[] DrawSpectrogramsFromSpectralIndices(
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
            ImageChrome imageChrome = ImageChrome.With)
        {
            var config = ldSpectrogramConfig;

            // These parameters manipulate the color map and appearance of the false-color spectrogram
            string colorMap1 = config.ColorMap1 ?? DefaultColorMap1;   // assigns indices to RGB
            string colorMap2 = config.ColorMap2 ?? DefaultColorMap2;   // assigns indices to RGB
            var blueEnhanceParameter = config.BlueEnhanceParameter ?? 0.0;

            // Set ColorFilter: Must lie between +/-1. A good value is -0.25
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

            // Get and set the dictionary of index properties
            Dictionary<string, IndexProperties> dictIp = IndexProperties.GetIndexProperties(indexPropertiesConfigPath);
            dictIp = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIp);
            cs1.SetSpectralIndexProperties(dictIp);

            // Load the Index Spectrograms into a Dictionary
            if (indexSpectrograms == null)
            {
                var sw = Stopwatch.StartNew();
                Logger.Info("Reading spectra files from disk");

                // reads all known files spectral indices
                cs1.ReadCsvFiles(inputDirectory, fileStem, cs1.SpectrogramKeys);

                //var now2 = DateTime.Now;
                sw.Stop();
                LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds.ToString(CultureInfo.InvariantCulture) + " seconds");
            }
            else
            {
                Logger.Info("Spectra loaded from memory");

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
            var keys = SpectralIndexValues.Keys;
            cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem, keys);

            // create and save first false-color spectrogram image
            var image1NoChrome = cs1.DrawFalseColorSpectrogramChromeless(cs1.ColorMode, colorMap1, blueEnhanceParameter);
            Image<Rgb24> image1 = null;
            if (image1NoChrome == null)
            {
                LoggedConsole.WriteErrorLine("ERROR: Null spectrogram image");
            }
            else
            {
                image1 = SpectrogramFraming(cs1, (Image<Rgb24>)image1NoChrome.Clone());
                var outputPath1 = FilenameHelpers.AnalysisResultPath(outputDirectory, cs1.FileName, colorMap1, "png");
                image1.Save(outputPath1);
            }

            // create and save second false-color spectrogram image
            var image2NoChrome = cs1.DrawFalseColorSpectrogramChromeless(cs1.ColorMode, colorMap2, blueEnhanceParameter);
            Image<Rgb24> image2 = null;
            if (image2NoChrome == null)
            {
                LoggedConsole.WriteErrorLine("ERROR: Null spectrogram image");
            }
            else
            {
                cs1.ColorMap = colorMap2;
                image2 = SpectrogramFraming(cs1, (Image<Rgb24>)image2NoChrome.Clone());
                var outputPath2 = FilenameHelpers.AnalysisResultPath(outputDirectory, cs1.FileName, colorMap2, "png");
                image2.Save(outputPath2);
            }

            // read high amplitude and clipping info into an image
            Image<Rgb24> imageX;
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

            // AS OF DECEMBER 2018, no longer produce SUMMARY RIBBONS. Did not prove useful.
            //double[,] m = cs1.GetNormalisedSpectrogramMatrix(key);
            //var ribbon = LdSpectrogramRibbons.GetSummaryIndexRibbon(colorMap1);
            //var ribbon = LdSpectrogramRibbons.GetSummaryIndexRibbonWeighted(colorMap1);
            //ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap1 + ".SummaryRibbon", "png"));
            //ribbon = LdSpectrogramRibbons.GetSummaryIndexRibbon(colorMap2);
            //ribbon = LdSpectrogramRibbons.GetSummaryIndexRibbonWeighted(colorMap2);
            //ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap2 + ".SummaryRibbon", "png"));

            //NEXT produce SPECTROGRAM RIBBONS
            // These are useful for viewing multiple consecutive days of recording.
            // Get matrix for each of the three indices.
            string[] keys1 = colorMap1.Split('-');
            double[,] indices1 = cs1.GetNormalisedSpectrogramMatrix(keys1[0]);
            double[,] indices2 = cs1.GetNormalisedSpectrogramMatrix(keys1[1]);
            double[,] indices3 = cs1.GetNormalisedSpectrogramMatrix(keys1[2]);

            var ribbon = LdSpectrogramRibbons.GetSpectrogramRibbon(indices1, indices2, indices3);
            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap1 + LdSpectrogramRibbons.SpectralRibbonTag, "png"));

            string[] keys2 = colorMap2.Split('-');
            indices1 = cs1.GetNormalisedSpectrogramMatrix(keys2[0]);
            indices2 = cs1.GetNormalisedSpectrogramMatrix(keys2[1]);
            indices3 = cs1.GetNormalisedSpectrogramMatrix(keys2[2]);
            ribbon = LdSpectrogramRibbons.GetSpectrogramRibbon(indices1, indices2, indices3);
            ribbon.Save(FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, colorMap2 + LdSpectrogramRibbons.SpectralRibbonTag, "png"));

            // only return images if chromeless
            return imageChrome == ImageChrome.Without
                       ? new[] { Tuple.Create(image1NoChrome, colorMap1), Tuple.Create(image2NoChrome, colorMap2) }
                       : null;
        }

        /// <summary>
        /// This method CHROMES the passed spectrogram.
        /// Chroming means to add all the trimmings, title bar axis labels, grid lines, cinnamon powder, etc.
        /// </summary>
        private static Image<Rgb24> SpectrogramFraming(LDSpectrogramRGB cs, Image<Rgb24> imageSansChrome)
        {
            string startTime = $"{cs.StartOffset.Hours:d2}{cs.StartOffset.Minutes:d2}h";

            // then pass that image into chromer
            int nyquist = cs.SampleRate / 2;
            string title = $"<{cs.ColorMap}> SPECTROGRAM  of \"{cs.FileName}\".   Starts at {startTime}; Nyquist={nyquist}";
            var titleBar = DrawTitleBarOfFalseColourSpectrogram(title, imageSansChrome.Width);
            var image = FrameFalseColourSpectrogram(imageSansChrome, titleBar, cs);
            return image;
        }

        private static void CreateTwoMapsImage(DirectoryInfo outputDirectory, string fileStem, Image<Rgb24> image1, Image<Rgb24> imageX, Image<Rgb24> image2)
        {
            var imageList = new[] { image1, imageX, image2 };
            var image3 = ImageTools.CombineImagesVertically(imageList);
            var outputPath = FilenameHelpers.AnalysisResultPath(outputDirectory, fileStem, "2Maps", "png");
            image3.Save(outputPath);
        }
    }
}