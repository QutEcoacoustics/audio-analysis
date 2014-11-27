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
            LoggedConsole.WriteLine("# Input .csv file: " + arguments.Source.Name);
            LoggedConsole.WriteLine("# Configure  file: " + arguments.Config.Name);
            LoggedConsole.WriteLine("# Output directry: " + arguments.Output.Name);


            bool verbose = arguments.Verbose;

            // 1. set up the necessary files
            DirectoryInfo csvFileInfo = arguments.Source;
            FileInfo configFile = arguments.Config;
            DirectoryInfo output = arguments.Output;

            // 2. get the config dictionary
            var configDict = GetConfiguration(configFile);

            // COMPONENT FILES IN DIRECTORY HAVE THIS STRUCTURE
            //SERF_20130915_201727_000.wav\Towsey.Acoustic\SERF_20130915_201727_000.ACI.csv    ; SERF_20130915_201727_000.BGN.csv etc


            // print out the parameters
            if (verbose)
            {
                LoggedConsole.WriteLine("\nPARAMETERS");
                foreach (var kvp in configDict)
                {
                    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }


            // set up header of the output file
            string outputPath = Path.Combine(output.FullName, "SNRInfoForConvDnnDataset.csv");
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                //string header = AudioToSonogramResult.GetCsvHeader();
                //writer.WriteLine(header);
            }

            // following int's are counters to monitor file availability
            int lineNumber = 0;
            int fileExistsCount = 0;
            int fileLocationNotInCsv = 0;
            int fileInCsvDoesNotExist = 0;

            // keep track of species names and distribution of classes.
            // following dictionaries are to monitor species numbers
            //var speciesCounts = new SpeciesCounts();


            // read through the csv file containing info about recording locations and call bounds
            try
            {
                var file = new FileStream(csvFileInfo.FullName, FileMode.Open);
                var sr = new StreamReader(file);

                // read the header and discard
                string strLine;
                lineNumber++;

                while ((strLine = sr.ReadLine()) != null)
                {
                    lineNumber++;
                    if (lineNumber % 5000 == 0)
                    {
                        Console.WriteLine(lineNumber);
                    }

                    // cannot use next line because reads the entire file
                    ////var data = Csv.ReadFromCsv<string[]>(csvFileInfo).ToList();

                    // read single record from csv file
                    //var record = CsvDataRecord.ReadLine(strLine);

                    //if (record.path == null)
                    //{
                    //    fileLocationNotInCsv++;
                    //    ////string warning = String.Format("######### WARNING: line {0}  NULL PATH FIELD >>>null<<<", count);
                    //    ////LoggedConsole.WriteWarnLine(warning);
                    //    continue;
                    //}

                    //var sourceRecording = record.path;
                    //var sourceDirectory = sourceRecording.Directory;
                    //string parentDirectoryName = sourceDirectory.Parent.Name;
                    //var imageOpDir = new DirectoryInfo(output.FullName + @"\" + parentDirectoryName);
                    ////DirectoryInfo imageOpDir = new DirectoryInfo(outDirectory.FullName + @"\" + parentDirectoryName + @"\" + directoryName);

                    /*#######################################
                      #######################################
                      my debug code for home to test on subset of data - comment these lines when at QUT! 
                      Anthony will tell me I should use a conditional compilation flag.
                        -- Anthony will tell you that this is completely unnecessary!
                      ####################################### */
                    ////DirectoryInfo localSourceDir = new DirectoryInfo(@"C:\SensorNetworks\WavFiles\ConvDNNData");
                    ////sourceRecording = Path.Combine(localSourceDir.FullName + @"\" + parentDirectoryName + @"\" + directoryName, fileName).ToFileInfo();
                    ////record.path = sourceRecording;

                    /* ####################################### */

                    // TO TEST PORTION OF DATA 
                    //doPreprocessing = false;
                    //if (parentDirectoryName.Equals("0"))
                    //{
                    //    doPreprocessing = true;
                    //}

                    /* #######################################
                       ####################################### */


                    //if (!sourceRecording.Exists)
                    //{
                    //    fileInCsvDoesNotExist++;
                    //    string warning = string.Format("FILE DOES NOT EXIST >>>," + sourceRecording.Name);
                    //    using (StreamWriter writer = new StreamWriter(outputPath, true))
                    //    {
                    //        writer.WriteLine(warning);
                    //    }
                    //    ////LoggedConsole.WriteWarnLine(warning);
                    //    continue;
                    //}

                    // ####################################################################
                    //if (doPreprocessing)
                    //{
                    //    AudioToSonogramResult result = AnalyseOneRecording(record, configDict, output);
                    //    string line = result.WriteResultAsLineOfCSV();

                    //    // It is helpful to write to the output file as we go, so as to keep a record of where we are up to.
                    //    // This requires to open and close the output file at each iteration
                    //    using (StreamWriter writer = new StreamWriter(outputPath, true))
                    //    {
                    //        writer.WriteLine(line);
                    //    }
                    //}

                    // everything should be OK - have jumped through all the hoops.
                    fileExistsCount++;
                    // keep track of species names and distribution of classes.
                    //speciesCounts.AddSpeciesCount(record.common_tags);
                    //speciesCounts.AddSpeciesID(record.common_tags, record.species_tags);
                    //speciesCounts.AddSiteName(record.site_name);

                } // end while()

                string classDistributionOpPath = Path.Combine(output.FullName, "ClassDistributionsForConvDnnDataset.csv");
                //speciesCounts.Save(classDistributionOpPath);
            }
            catch (IOException e)
            {
                LoggedConsole.WriteLine("Something went seriously bloody wrong!");
                LoggedConsole.WriteLine(e.ToString());
                return;
            }

            LoggedConsole.WriteLine("fileLocationNotInCsv =" + fileLocationNotInCsv);
            LoggedConsole.WriteLine("fileInCsvDoesNotExist=" + fileInCsvDoesNotExist);
            LoggedConsole.WriteLine("fileExistsCount      =" + fileExistsCount);
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
                Output = (@"C:\SensorNetworks\Output\XueyanDataset\" + datestamp).ToDirectoryInfo(),
                Verbose = true
            };
        }




    }
}
