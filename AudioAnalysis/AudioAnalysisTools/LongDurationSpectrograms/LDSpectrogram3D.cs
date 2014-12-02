// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LDSpectrogram3D.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   This class generates three dimensional false-colour spectrograms of long duration audio recordings.
//   The three dimensions are: 1) Y-axis = frequency bin; X-axis = time of day (either 1435 minutes or 24 hours); Z-axis = consecutive days through year. 
//   It does not calculate the indices but reads them from pre-calculated values in csv files.
// 
//   Important properties are:
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
    using AudioAnalysisTools.StandardSpectrograms;

    using log4net;

    using TowseyLibrary;
using Acoustics.Shared.Csv;

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
    public class LDSpectrogram3D
    {
        public string FileName { get; set; }

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        public LDSpectrogram3D()
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

        // used to save all spectrograms as dictionary of matrices 
        // IMPORTANT: The matrices are stored as they would appear in the LD spectrogram image. i.e. rotated 90 degrees anti-clockwise.
        private Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
        // used if reading standard devaition matrices for tTest
        private Dictionary<string, double[,]> spgr_StdDevMatrices;                                      

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
        public LDSpectrogram3D(TimeSpan Xscale, int sampleRate, string colourMap)
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
        public LDSpectrogram3D(TimeSpan minuteOffset, TimeSpan Xscale, int sampleRate, int frameWidth, string colourMap)
            : this(Xscale, sampleRate, colourMap)
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




        /// <summary>
        /// returns a matrix of acoustic indices whose values are normalised.
        /// In addition, small background values are reduced as per filter coefficient. 1.0 = unchanged. 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="backgroundFilterCoeff"></param>
        /// <returns></returns>
        //public double[,] GetNormalisedSpectrogramMatrix(string key)
        //{
        //    if (! this.spectralIndexProperties.ContainsKey(key))
        //    {
        //        LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
        //        LoggedConsole.WriteLine("Dictionary of Spectral Properties does not contain key {0}", key);
        //        return null;
        //    }
        //    if (!this.spectrogramMatrices.ContainsKey(key))
        //    {
        //        LoggedConsole.WriteLine("WARNING from LDSpectrogram.GetNormalisedSpectrogramMatrix");
        //        LoggedConsole.WriteLine("Dictionary of Spectrogram Matrices does not contain key {0}", key);
        //        return null;
        //    }

        //    IndexProperties indexProperties = this.spectralIndexProperties[key];
        //    var matrix = indexProperties.NormaliseIndexValues(this.GetMatrix(key));

        //    return MatrixTools.FilterBackgroundValues(matrix, this.BackgroundFilter); // to de-demphasize the background small values
        //}


        ///// <summary>
        ///// Draws all available spectrograms in grey scale 
        ///// </summary>
        ///// <param name="opdir"></param>
        ///// <param name="opFileName"></param>
        //public void DrawGreyScaleSpectrograms(DirectoryInfo opdir, string opFileName)
        //{
        //    this.DrawGreyScaleSpectrograms(opdir, opFileName, this.spectrogramKeys);
        //}


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




        //############################################################################################################################################################
        //# BELOW METHODS CALCULATE SUMMARY INDEX RIBBONS ############################################################################################################
        


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

        /// <summary>
        /// This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        ///  IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. 
        ///  
        /// WARNING: Make sure the parameters in the CONFIG file are consistent with the CSV files.
        /// </summary>
        /// <param name="longDurationSpectrogramConfig">
        /// </param>
        /// <param name="indicesConfigPath">
        /// The indices Config Path.
        /// </param>
        /// <param name="spectra">
        /// Optional spectra to pass in. If specified the spectra will not be loaded from disk!
        /// </param>
        //public static void DrawSpectrogramsFromSpectralIndices(LdSpectrogramConfig longDurationSpectrogramConfig, FileInfo indicesConfigPath, Dictionary<string, double[,]> spectra = null)
        //{
        //    LdSpectrogramConfig configuration = longDurationSpectrogramConfig;

        //    Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
        //    dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

        //    string fileStem = configuration.FileName;
        //    DirectoryInfo outputDirectory = configuration.OutputDirectoryInfo;

        //    // These parameters manipulate the colour map and appearance of the false-colour spectrogram
        //    string colorMap1 = configuration.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
        //    string colorMap2 = configuration.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB

        //    double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
        //    ////double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

        //    // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
        //    TimeSpan minuteOffset = configuration.MinuteOffset;   // default = zero minute of day i.e. midnight
        //    TimeSpan xScale = configuration.XAxisTicInterval; // default is one minute spectra i.e. 60 per hour
        //    int sampleRate = configuration.SampleRate;
        //    int frameWidth = configuration.FrameWidth;

        //    var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap1);
        //    cs1.FileName = fileStem;
        //    cs1.BackgroundFilter = backgroundFilterCoeff;
        //    cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

        //    if (spectra == null)
        //    {
        //        // reads all known files spectral indices
        //        Logger.Info("Reading spectra files from disk");
        //        cs1.ReadCSVFiles(configuration.InputDirectoryInfo, fileStem);
        //    }
        //    else
        //    {
        //        // TODO: not sure if this works
        //        Logger.Info("Spectra loaded from memory");
        //        cs1.LoadSpectrogramDictionary(spectra);
        //    }

        //    if (cs1.GetCountOfSpectrogramMatrices() == 0)
        //    {
        //        LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
        //        return;
        //    }

        //    cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem);

        //    cs1.CalculateStatisticsForAllIndices();
        //    Json.Serialise(Path.Combine(outputDirectory.FullName, fileStem + ".IndexStatistics.json").ToFileInfo(), cs1.indexStats);


        //    cs1.DrawIndexDistributionsAndSave(Path.Combine(outputDirectory.FullName, fileStem + ".IndexDistributions.png"));

        //    string colorMap = colorMap1;
        //    Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
        //    string title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
        //    Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
        //    int nyquist = cs1.SampleRate / 2;
        //    int herzInterval = 1000;
        //    image1 = LDSpectrogramRGB.FrameLDSpectrogram(image1, titleBar, minuteOffset, cs1.XInterval, nyquist, herzInterval);

        //    //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
        //    colorMap = colorMap2;
        //    Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
        //    title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
        //    titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
        //    image2 = LDSpectrogramRGB.FrameLDSpectrogram(image2, titleBar, minuteOffset, cs1.XInterval, nyquist, herzInterval);
        //    image2.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap + ".png"));

        //    // read high amplitude and clipping info into an image
        //    //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
        //    string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
        //    //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");

        //    Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
        //    if (null != imageX)
        //        imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));

        //    var imageList = new List<Image>();
        //    imageList.Add(image1);
        //    imageList.Add(imageX);
        //    imageList.Add(image2);
        //    Image image3 = ImageTools.CombineImagesVertically(imageList);
        //    image3.Save(Path.Combine(outputDirectory.FullName, fileStem + ".2MAPS.png"));

        //    Image ribbon;
        //    // ribbon = cs1.GetSummaryIndexRibbon(colorMap1);
        //    ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap1);
        //    ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap1 + ".SummaryRibbon.png"));
        //    // ribbon = cs1.GetSummaryIndexRibbon(colorMap2);
        //    ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap2);
        //    ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap2 + ".SummaryRibbon.png"));

        //    ribbon = cs1.GetSpectrogramRibbon(colorMap1, 32);
        //    ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap1 + ".SpectralRibbon.png"));
        //    ribbon = cs1.GetSpectrogramRibbon(colorMap2, 32);
        //    ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap2 + ".SpectralRibbon.png"));
        //}


        /// <summary>
        /// This method started 27-11-2014 to process consecutive days of acoustic indices data for 3-D spectrograms.
        /// </summary>
        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.Output.Exists)
            {
                arguments.Output.Create();
            }

            const string Title = "# READ LD Spectrogram csv files to prepare a 3D Spectrogram";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input directory: " + arguments.Source.Name);
            LoggedConsole.WriteLine("# Configure  file: " + arguments.Config.Name);
            LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);


            bool verbose = arguments.Verbose;

            // 1. set up the necessary files
            DirectoryInfo inputDirInfo = arguments.Source;
            FileInfo configFile = arguments.Config;
            DirectoryInfo opDir = arguments.Output;

            // 2. get the config dictionary
            var configDict = GetConfiguration(configFile);

            // COMPONENT FILES IN DIRECTORY HAVE THIS STRUCTURE
            //SERF_20130915_201727_000.wav\Towsey.Acoustic\SERF_20130915_201727_000.ACI.csv; SERF_20130915_201727_000.BGN.csv etc


            // print out the parameters
            if (verbose)
            {
                LoggedConsole.WriteLine("\nPARAMETERS");
                foreach (var kvp in configDict)
                {
                    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }

            DirectoryInfo[] dirList = inputDirInfo.GetDirectories();

            // location to write the yaml config file for producing long duration spectrograms 
            FileInfo fiSpectrogramConfig = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            // Initialise the default Yaml Config file
            var config = new LdSpectrogramConfig("null", inputDirInfo, opDir); // default values have been set
            // write the yaml file to config
            config.WriteConfigToYaml(fiSpectrogramConfig);

            // read the yaml Config file describing the Index Properties 
            FileInfo indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo();
            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indexPropertiesConfig);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);


            foreach (DirectoryInfo dir in dirList)
            {
                string targetDirectory = dir.FullName + @"\Towsey.Acoustic";
                string targetFileName = dir.Name;
                string[] nameArray = targetFileName.Split('.');
                targetFileName = nameArray[0];

                //Write the default Yaml Config file for producing long duration spectrograms and place in the output directory
                config = new LdSpectrogramConfig(targetFileName, inputDirInfo, opDir); // default values have been set
                // write the yaml file to config
                config.WriteConfigToYaml(fiSpectrogramConfig);
                // read the yaml file to a LdSpectrogramConfig object
                LdSpectrogramConfig configuration = LdSpectrogramConfig.ReadYamlToConfig(fiSpectrogramConfig);
                configuration.InputDirectoryInfo = targetDirectory.ToDirectoryInfo();

                // These parameters manipulate the colour map and appearance of the false-colour spectrogram
                //string colorMap1 = configuration.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
                string colorMap2 = configuration.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB

                double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
                //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

                // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
                TimeSpan minuteOffset = configuration.MinuteOffset;   // default = zero minute of day i.e. midnight
                TimeSpan xScale = configuration.XAxisTicInterval; // default is one minute spectra i.e. 60 per hour
                int sampleRate = configuration.SampleRate;
                int frameWidth = configuration.FrameWidth;

                var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap2);
                cs1.FileName = configuration.FileName;
                cs1.BackgroundFilter = backgroundFilterCoeff;
                cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

                // reads all known files spectral indices
                Logger.Info("Reading spectra files from disk");
                cs1.ReadCSVFiles(configuration.InputDirectoryInfo, configuration.FileName);

                if (cs1.GetCountOfSpectrogramMatrices() == 0)
                {
                    LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                    return;
                }




            } // foreach (DirectoryInfo dir in dirList)




            //LoggedConsole.WriteLine("fileLocationNotInCsv =" + fileLocationNotInCsv);
            //LoggedConsole.WriteLine("fileInCsvDoesNotExist=" + fileInCsvDoesNotExist);
            //LoggedConsole.WriteLine("fileExistsCount      =" + fileExistsCount);
        }




        // use the following paths for the command line for the <audio2sonogram> task. 
        public class Arguments
        {
            public bool Verbose { get; set; }
            public DirectoryInfo Source { get; set; }
            public FileInfo Config { get; set; }
            public DirectoryInfo Output  { get; set; }
            public static string Description()
            {
                return ".";
            }

            public static string AdditionalNotes()
            {
                return "Nothing to add.";
            }
        }

        private static Dictionary<string, string> GetConfiguration(FileInfo configFile)
        {
            dynamic configuration = Yaml.Deserialise(configFile);

            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);

            configDict[AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString();
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true;

            ////bool makeSoxSonogram = (bool?)configuration[AnalysisKeys.MakeSoxSonogram] ?? false;
            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";
            return configDict;
        }


        private static Arguments Dev()
        {
            DateTime time = DateTime.Now;
            string datestamp = String.Format("{0}{1:d2}{2:d2}", time.Year, time.Month, time.Day);
            return new Arguments
            {
                //Source = @"Y:\Results\2013Feb05-184941 - Indicies Analysis of all of availae\SERF\Veg".ToDirectoryInfo(),
                Source = @"Y:\Results\2013Nov30-023140 - SERF - November 2013 Download\SERF\November 2013 Download\Veg Plot WAV".ToDirectoryInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),
                Output = (@"C:\SensorNetworks\Output\FalseColourSpectrograms\Spectrograms3D\" + datestamp).ToDirectoryInfo(),
                Verbose = true
            };
        }




    }
}
