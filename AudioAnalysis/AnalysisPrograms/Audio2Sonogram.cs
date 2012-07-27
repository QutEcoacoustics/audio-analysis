using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acoustics.Tools;
using Acoustics.Tools.Audio;
using Acoustics.Shared;
using AnalysisBase;
using AnalysisRunner;
//using AudioBrowser;
using AudioAnalysisTools;
using TowseyLib;


namespace AnalysisPrograms
{
    class Audio2Sonogram
    {
        //use the following paths for the command line for the <Audio2Sonogram> task. 

        // audio2sonogram "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\

        public const int DEFAULT_SAMPLE_RATE = 22050;
        
        public static int Main(string[] args)
        {
            int status = 0;
            string title = "# MAKE A SONOGRAM FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            bool verbose = true;
            if (verbose)
            {
                Console.WriteLine(title);
                Console.WriteLine(date);
            }

            if (CheckArguments(args) != 0) //checks validity of the first 3 path arguments
            {
                Console.WriteLine("\nPress <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            string recordingPath = args[0];
            string configPath    = args[1];
            string outputDir     = args[2];

            TimeSpan startOffsetMins;
            TimeSpan endOffsetMins;

            if (args.Length == 5)
            {
                startOffsetMins = TimeSpan.FromMinutes(double.Parse(args[3]));
                endOffsetMins   = TimeSpan.FromMinutes(double.Parse(args[4]));
            }

            if (verbose)
            {
                Console.WriteLine("# Output folder:  " + outputDir);
                Console.WriteLine("# Recording file: " + Path.GetFileName(recordingPath));
            }

            //1. set up the necessary files
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            FileInfo fiSourceRecording = new FileInfo(recordingPath);
            FileInfo fiConfig = new FileInfo(configPath);
            DirectoryInfo diOP = new DirectoryInfo(outputDir);

            //2. get the config dictionary
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            int resampleRate = DEFAULT_SAMPLE_RATE;
            if (configDict.ContainsKey(Keys.RESAMPLE_RATE))
                resampleRate = ConfigDictionary.GetInt(Keys.RESAMPLE_RATE, configDict);

            int frameLength = 0;
            if (configDict.ContainsKey(Keys.FRAME_LENGTH))
                frameLength = ConfigDictionary.GetInt(Keys.FRAME_LENGTH, configDict);

            double frameOverlap = 0.0;
            if (configDict.ContainsKey(Keys.FRAME_OVERLAP))
                frameOverlap = ConfigDictionary.GetDouble(Keys.FRAME_OVERLAP, configDict);

            int timeReductionFactor = 1;
            if (configDict.ContainsKey(Keys.TIME_REDUCTION_FACTOR))
                timeReductionFactor = ConfigDictionary.GetInt(Keys.TIME_REDUCTION_FACTOR, configDict);

            int freqReductionFactor = 1;
            if (configDict.ContainsKey(Keys.FREQ_REDUCTION_FACTOR))
                freqReductionFactor = ConfigDictionary.GetInt(Keys.FREQ_REDUCTION_FACTOR, configDict);

            bool addTimeScale = true;
            if (configDict.ContainsKey(Keys.ADD_TIME_SCALE))
                addTimeScale = ConfigDictionary.GetBoolean(Keys.ADD_TIME_SCALE, configDict);

            bool addSegmentationTrack = true;
            if (configDict.ContainsKey(Keys.ADD_SEGMENTATION_TRACK))
                addSegmentationTrack = ConfigDictionary.GetBoolean(Keys.ADD_SEGMENTATION_TRACK, configDict);

            //double smoothWindow = Double.Parse(configDict[Keys.SMOOTHING_WINDOW]);   //smoothing window (seconds) before segmentation
            //double thresholdSD = Double.Parse(configDict[Keys.THRESHOLD]);           //segmentation threshold in noise SD
            //int lowFrequencyBound = Double.Int(configDict[Keys.LOW_FREQ_BOUND]);     //lower bound of the freq band to be displayed
            //int hihFrequencyBound = Double.Int(configDict[Keys.HIGH_FREQ_BOUND]);    //upper bound of the freq band to be displayed

            if (verbose)
            {
                Console.WriteLine("# Freq band: {0} Hz - {1} Hz.)", timeReductionFactor, freqReductionFactor);
                //Console.WriteLine("# Smoothing Window: {0}s.", smoothWindow);
            }

            //3: GET RECORDING
            //var audioUtility = new MasterAudioUtility(analysisSettings.SegmentTargetSampleRate, SoxAudioUtility.SoxResampleQuality.VeryHigh);
            //var mimeType = MediaTypes.GetMediaType(fiSourceRecording.Extension);
            //var sourceDuration = audioUtility.Duration(fiSourceRecording, mimeType);





            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);

            //ii: MAKE SONOGRAM
            //Log.WriteLine("# Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.SourceFName = recording.FileName;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            //sonoConfig.DynamicRange = dynamicRange;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();

            //draw images of sonograms
            if ((timeReductionFactor != 1) || (freqReductionFactor != 1))
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                bool doHighlightSubband = false; bool add1kHzLines = true;
                using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
                using (Image_MultiTrack image   = new Image_MultiTrack(img))
                {
                    if (addTimeScale) image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                    if (addSegmentationTrack) image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                    image.Save(imagePath);

                }
            }
            else //sonogram to be reduced
            {
                var results = AI_DimRed(sonogram, timeReductionFactor, freqReductionFactor); //acoustic intensity
                var reducedSono = results.Item1;
                var results1 = BaseSonogram.Data2ImageData(reducedSono);
                string reducedPath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_reduced.png";
                ImageTools.DrawMatrix(results1.Item1, 1, 1, reducedPath);
            }

            Console.WriteLine("\n##### FINISHED FILE ###################################################\n");
            Console.ReadLine();
            
            return status;
        } //Main(string[] args)


        public static System.Tuple<double[,]> AI_DimRed(BaseSonogram sonogram, int timeRedFactor, int freqRedFactor)
        {
            int freqBinCount = sonogram.Configuration.FreqBinCount;
            int frameCount = sonogram.FrameCount;

            int timeReducedCount = frameCount / timeRedFactor;
            int freqReducedCount = freqBinCount / freqRedFactor;

            var reducedMatrix = new double[timeReducedCount, freqReducedCount];
            int cellArea = timeRedFactor * freqRedFactor;
            for (int r = 0; r < timeReducedCount; r++)
                for (int c = 0; c < freqReducedCount; c++)
                {
                    int or = r * timeRedFactor;
                    int oc = c * freqRedFactor;
                    double sum = 0.0;
                    for (int i = 0; i < timeRedFactor; i++)
                        for (int j = 0; j < freqRedFactor; j++)
                        {
                            sum += sonogram.Data[or + i, oc + j];
                        }
                    reducedMatrix[r, c] = sum / cellArea;
                }

            var tuple2 = System.Tuple.Create(reducedMatrix);
            return tuple2;
        }//end AI_DimRed


        public static int CheckArguments(string[] args)
        {
            if ((args.Length != 3) && (args.Length != 5))
            {
                Console.WriteLine("\nINCORRECT COMMAND LINE.");
                Console.WriteLine("\nTHE COMMAND LINE HAS {0} ARGUMENTS", args.Length);
                foreach (string arg in args) Console.WriteLine(arg + "  ");
                Console.WriteLine("\nYOU REQUIRE 3 OR 5 COMMAND LINE ARGUMENTS\n");
                Usage();
                return 666;
            }
            if (CheckPaths(args) != 0) return 999;
            return 0;
        }

        /// <summary>
        /// this method checks validity of first three command line arguments.
        /// </summary>
        /// <param name="args"></param>
        public static int CheckPaths(string[] args)
        {
            int status = 0;
            //GET FIRST THREE OBLIGATORY COMMAND LINE ARGUMENTS
            string recordingPath = args[0];
            string configPath = args[1];
            string outputDir = args[2];
            DirectoryInfo diSource = new DirectoryInfo(Path.GetDirectoryName(recordingPath));
            if (!diSource.Exists)
            {
                Console.WriteLine("Source directory does not exist: " + diSource.FullName);
                status = 2;
                return status;
            }
            FileInfo fiSource = new FileInfo(recordingPath);
            if (!fiSource.Exists)
            {
                Console.WriteLine("Source directory exists: " + diSource.FullName);
                Console.WriteLine("\t but the source file does not exist: " + recordingPath);
                status = 2;
                return status;
            }
            FileInfo fiConfig = new FileInfo(configPath);
            if (!fiConfig.Exists)
            {
                Console.WriteLine("Config file does not exist: " + fiConfig.FullName);
                status = 2;
                return status;
            }
            DirectoryInfo diOP = new DirectoryInfo(outputDir);
            if (!diOP.Exists)
            {
                bool success = true;
                try
                {
                    Directory.CreateDirectory(outputDir);
                    success = Directory.Exists(outputDir);
                }
                catch
                {
                    success = false;
                }

                if (!success)
                {
                    Console.WriteLine("Output directory does not exist: " + diOP.FullName);
                    status = 2;
                    return status;
                }
            }
            return status;
        }


        public static void Usage()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine("AnalysisPrograms.exe  audio2sonogram  audioPath  configPath  outputDirectory  startOffset  endOffset");
            Console.WriteLine("where:");
            Console.WriteLine("audio2sonogram:- (string) a short string that selects the analysis/process to be performed.");
            Console.WriteLine("input  audio  File:- (string) Path of the audio file to be processed.");
            Console.WriteLine("configuration File:- (string) Path of the analysis configuration file.");
            Console.WriteLine("output   Directory:- (string) Path of the output directory in which to store .csv result files.");
            Console.WriteLine("THE ABOVE THREE ARGUMENTS ARE OBLIGATORY. THE NEXT TWO ARGUMENTS ARE OPTIONAL:");
            Console.WriteLine("startOffset: (integer) The start (minutes) of that portion of the file to be analysed.");
            Console.WriteLine("endOffset:   (integer) The end   (minutes) of that portion of the file to be analysed.");
            Console.WriteLine("If arguments 4 and 5 are not included, the entire file is analysed.");
            Console.WriteLine("");
        }

    } //class Audio2Sonogram
}


