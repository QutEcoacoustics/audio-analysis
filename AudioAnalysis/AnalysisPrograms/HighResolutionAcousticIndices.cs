// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighResolutionAcousticIndices.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the HighResolutionAcousticIndices type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Acoustics.Shared;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.LongDurationSpectrograms.Zooming;
    using Draw.Zooming;
    using PowerArgs;

    using TowseyLibrary;

    public static class HighResolutionAcousticIndices
    {

        const string FEATURE_KEYS = "SPT,RHZ,RVT,RPS,RNG";
        const string HEADERS = "index,Hz(top),SPT,RHZ,RVT,RPS,RNG";

        /// <summary>
        /// This DEV method runs the EXECUTE method in this class. It sets up the input/output arguments that go into the Aruments class.
        /// Access to this DEV class is from the EXECUTE class.
        /// Access to the EXECUTE class is currently from the Sandpit.cs class.
        /// "sandpit" as the FIRST AND ONLY command line argument
        ///
        ///
        /// Activity Codes for other tasks to do with spectrograms and audio files:
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
        /// </summary>
        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        public static Arguments Dev()
        {

            //string debugRecordingPath = @"C:\SensorNetworks\WavFiles\TestRecordings\CaneToads_rural1_20.mp3";
            string debugRecordingPath = @"C:\SensorNetworks\Output\Frogs\FrogPondSamford\FrogPond_Samford_SE_555_20101023-000000_0min.wav";

            string dataDir   = @"C:\SensorNetworks\WavFiles\TestRecordings";
            string parentDir = @"C:\SensorNetworks\Output\Frogs\Test2016";

            string outputDir      = parentDir;
            string imageOutputDir = parentDir;
            string csvDir         = outputDir + @"\Towsey.Acoustic";
            string zoomOutputDir  = outputDir;

            //FileInfo fiSpectrogramConfig = null;
            string hiResZoomConfigPath   = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramHiResConfig.yml";
            string spectrogramConfig     = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml";
            string indexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigHiRes.yml";
            string audio2csvConfigPath   = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.AcousticHiRes.yml";

            return new Arguments
            {
                DebugRecording = new FileInfo(debugRecordingPath),
                InputDataDirectory = new DirectoryInfo(dataDir),
                OutputDirectory = new DirectoryInfo(outputDir),
                CsvDirectory = new DirectoryInfo(csvDir),
                ZoomOutputDir = new DirectoryInfo(zoomOutputDir),
                ImageOutputDir = new DirectoryInfo(imageOutputDir),
                // use the default set of index properties in the AnalysisConfig directory.
                IndexPropertiesConfig = new FileInfo(indexPropertiesConfig),
                SpectrogramConfig = new FileInfo(spectrogramConfig),
                Audio2CsvConfig = new FileInfo(audio2csvConfigPath),
                HiResZoomConfig = new FileInfo(hiResZoomConfigPath),

                // background threshold value that is subtracted from all spectrograms.
                BgnThreshold = 3.0,
            };
            throw new Exception();
        } //Dev()

        public class Arguments
        {
            public FileInfo DebugRecording { get; set; }

            [ArgDescription("Directory where the input data is located.")]
            public DirectoryInfo InputDataDirectory { get; set; }

            [ArgDescription("Directory where the output is to go.")]
            public DirectoryInfo OutputDirectory { get; set; }

            public DirectoryInfo CsvDirectory { get; set; }
            public DirectoryInfo ZoomOutputDir { get; set; }
            public DirectoryInfo ImageOutputDir { get; set; }
            public FileInfo Audio2CsvConfig { get; set; }
            public FileInfo HiResZoomConfig { get; set; }

            [ArgDescription("User specified file containing a list of indices and their properties.")]
            public FileInfo IndexPropertiesConfig { get; set; }

            [ArgDescription("Config file specifying directory containing indices.csv files and other parameters.")]
            //[ArgPosition(1)]
            public FileInfo SpectrogramConfig { get; set; }

            public double BgnThreshold { get; set; }
        }

        //public class Output
        //{
        //    // INIT array of instance IDs obtained from file names
        //    public string[] FileID = null;
        //    // INIT array of species ID for each instance
        //    public int[] SpeciesID = null;
        //    // INIT array of species counts
        //    public int[] InstanceNumbersPerSpecies = null;
        //    // INIT array of frame counts
        //    public int[] FrameNumbersPerInstance = null;
        //    // INIT array of frame counts
        //    public int[] FrameNumbersPerSpecies = null;
        //    // length of spectrum array have reduction by max pooling
        //    public int ReducedSpectralLength = 0;
        //    // matrix: each row= one instance;  each column = one feature
        //    public double[,] InstanceFeatureMatrix = null;
        //    // matrix: each row= one Species;  each column = one feature
        //    public double[,] SpeciesFeatureMatrix = null;

        //    public double[,] SimilarityScores = null;

        //    public int[,] ConfusionMatrix = null;

        //    public int[,] RankOrderMatrix = null;

        //    public double[] Weights;
        //}

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
                bool verbose = true; // assume verbose if in dev mode
                if (verbose)
                {
                    string date = "# DATE AND TIME: " + DateTime.Now;
                    LoggedConsole.WriteLine("# Analyse AUDIO RECORDING at HIGH RESOLUTION");
                    LoggedConsole.WriteLine(date);
                    //LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
                    //LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
                    LoggedConsole.WriteLine();
                } // if (verbose)
            } // if

            FileInfo[] wavFiles = { arguments.DebugRecording };

            // ####################### COMMENT THE NEXT TW0 LINES when debugging a single recording file
            //string match = @"*.mp3";
            //wavFiles = arguments.InputDataDirectory.GetFiles(match, SearchOption.AllDirectories);

            // READ IN THE SPECIES LABELS FILE AND SET UP THE DATA
            string[] fileID = new string[wavFiles.Length];

            //LOOP THROUGH ALL WAV FILES
            //for (int i = 0; i < 8; i++)
            for (int i = 0; i < wavFiles.Length; i++)
            {
                FileInfo file = wavFiles[i];
                string recordingPath = file.FullName;
                string name = Path.GetFileNameWithoutExtension(file.FullName);
                //string outputDir = parentDir + @"\" + @"\" + name;
                //string csvDir = arguments.CsvDirectory;
                //string zoomOutputDir = outputDir;
                Console.WriteLine("\n\n");
                Console.WriteLine(string.Format(@">>>>{0}: File<{1}>", i, name));

                try
                {
                    // A: analyse the recording files == audio2csv.
                    var audio2csvArguments = new AnalyseLongRecordings.AnalyseLongRecording.Arguments
                    {
                        Source = recordingPath.ToFileInfo(),
                        Config = arguments.Audio2CsvConfig,
                        Output = arguments.OutputDirectory,
                    };

                    if (!audio2csvArguments.Source.Exists)
                    {
                        LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Source Recording file cannot be found! This will cause an exception.");
                    }
                    if (!audio2csvArguments.Config.Exists)
                    {
                        LoggedConsole.WriteWarnLine(" >>>>>>>>>>>> WARNING! The Configuration file cannot be found! This will cause an exception.");
                    }
                    AnalyseLongRecordings.AnalyseLongRecording.Execute(audio2csvArguments);

                    // B: Concatenate the summary indices and produce images
                    // need to find out how long the recording is.
                    string fileName = Path.GetFileNameWithoutExtension(audio2csvArguments.Source.FullName);
                    string match = fileName + @"__Towsey.Acoustic.???.csv";
                    FileInfo[] files = arguments.CsvDirectory.GetFiles(match, SearchOption.AllDirectories);
                    List<string> data = FileTools.ReadTextFile(files.First().FullName);
                    int lineCount = data.Count - 1;  // -1 for header.
                    int imageWidth = lineCount;

                    //assume scale is index calculation duration = 0.1s
                    // i.e. image resolution  0.1s/px. or 600px/min
                    double focalMinute = (double)lineCount / 600 / 2;
                    if (focalMinute < 0.016666) focalMinute = 0.016666; // shortest recording = 1 second.

                    var zoomingArguments = new DrawZoomingSpectrograms.Arguments
                    {
                        // use the default set of index properties in the AnalysisConfig directory.
                        SourceDirectory = arguments.CsvDirectory.FullName,
                        Output = arguments.ZoomOutputDir.FullName,
                        SpectrogramZoomingConfig = arguments.HiResZoomConfig,

                        // draw a focused multi-resolution pyramid of images
                        ZoomAction = DrawZoomingSpectrograms.Arguments.ZoomActionType.Focused,
                        //FocusMinute = (int)focalMinute,
                    };

                    LoggedConsole.WriteLine("# Spectrogram Zooming config  : " + zoomingArguments.SpectrogramZoomingConfig);
                    LoggedConsole.WriteLine("# Input Directory             : " + zoomingArguments.SourceDirectory);
                    LoggedConsole.WriteLine("# Output Directory            : " + zoomingArguments.Output);

                    var common = new ZoomArguments();
                    common.SpectrogramZoomingConfig = Yaml.Deserialise<SpectrogramZoomingConfig>(zoomingArguments.SpectrogramZoomingConfig);
                    var indexPropertiesPath = IndexProperties.Find(common.SpectrogramZoomingConfig, zoomingArguments.SpectrogramZoomingConfig);
                    LoggedConsole.WriteLine("Using index properties file: " + indexPropertiesPath.FullName);
                    common.IndexProperties = IndexProperties.GetIndexProperties(indexPropertiesPath);

                    // get the indexDistributions and the indexGenerationData AND the //common.OriginalBasename
                    common.CheckForNeededFiles(zoomingArguments.SourceDirectory.ToDirectoryInfo());
                    // Create directory if not exists
                    if (!Directory.Exists(zoomingArguments.Output))
                    {
                        Directory.CreateDirectory(zoomingArguments.Output);
                    }

                    ZoomFocusedSpectrograms.DrawStackOfZoomedSpectrograms(zoomingArguments.SourceDirectory.ToDirectoryInfo(),
                                                                            zoomingArguments.Output.ToDirectoryInfo(),
                                                                            common,
                                                                            TimeSpan.FromMinutes(focalMinute),
                                                                            imageWidth);

                    // DRAW THE VARIOUS IMAGES
                    // i.e. greyscale images, ridge spectrogram and two-maps spectrograms.
                    string fileStem = fileName;

                    var LDFCSpectrogramArguments = new DrawLongDurationSpectrograms.Arguments
                    {
                        // use the default set of index properties in the AnalysisConfig directory.
                        InputDataDirectory = arguments.CsvDirectory,
                        OutputDirectory    = arguments.ImageOutputDir,
                        IndexPropertiesConfig = arguments.IndexPropertiesConfig,
                    };
                    // Create directory if not exists
                    if (!LDFCSpectrogramArguments.OutputDirectory.Exists)
                    {
                        LDFCSpectrogramArguments.OutputDirectory.Create();
                    }

                    // there are two possible tasks
                    // 1: draw the aggregated grey scale spectrograms
                    int secDuration = DrawLongDurationSpectrograms.DrawAggregatedSpectrograms(LDFCSpectrogramArguments, fileStem);

                    // 2: draw the coloured ridge spectrograms
                    secDuration = DrawLongDurationSpectrograms.DrawRidgeSpectrograms(LDFCSpectrogramArguments, fileStem);

                } // try block
                catch (Exception e)
                {
                    LoggedConsole.WriteErrorLine(string.Format("ERROR!!!!! RECORDING {0}   FILE {1}", i, name));
                    LoggedConsole.WriteErrorLine(string.Format(e.ToString()));

                }

            } // end loop through all wav files

        } //Execute()

        //######################################################################################################################################
        // MAX-POOLING METHODS BELOW

        public static double[,] MaxPoolingLimited(double[,] M, int startBin, int maxOf2Bin, int maxOf3Bin, int endBin, int reducedBinCount)
        {
            int rows = M.GetLength(0);
            int cols = M.GetLength(1);

            var reducedM = new double[rows, reducedBinCount];
            for (int r = 0; r < rows; r++)
            {
                var rowVector = MatrixTools.GetRow(M, r);
                double[] V = MaxPoolingLimited(rowVector, startBin, maxOf2Bin, maxOf3Bin, endBin);

                for (int c = 0; c < reducedBinCount; c++)
                {
                    reducedM[r, c] = V[c];
                }
            }
            return reducedM;
        }

        /// <summary>
        /// reduces the dimensionality of a vector by max pooling.
        /// Used specifically for representation of spectral frames in Herve Glotin work
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="startBin"></param>
        /// <param name="maxOf2Bin"></param>
        /// <param name="maxOf3Bin"></param>
        /// <param name="endBin"></param>
        /// <returns></returns>
        public static double[] MaxPoolingLimited(double[] vector, int startBin, int maxOf2Bin, int maxOf3Bin, int endBin)
        {
            double value = 0.0;
            List<double> opVector = new List<double>();
            for (int i = startBin; i < maxOf2Bin; i++)
            {
                opVector.Add(vector[i]);
            }
            for (int i = maxOf2Bin; i < maxOf3Bin; i++)
            {
                value = vector[i];
                if (value < vector[i + 1]) value = vector[i + 1];
                opVector.Add(value);
                i++;
            }
            for (int i = maxOf3Bin; i < endBin; i++)
            {
                value = vector[i];
                if (value < vector[i + 1]) value = vector[i + 1];
                if (value < vector[i + 2]) value = vector[i + 2];
                opVector.Add(value);
                i += 2;
            }

            return opVector.ToArray();
        }

    }
}
