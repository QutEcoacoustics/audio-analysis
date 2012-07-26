using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;

namespace AnalysisPrograms
{
    class DimReduction
    {

        //CURRAWONG
        // dimred "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.mp3"  C:\SensorNetworks\Output\DIMRED\DIMRED_Params.txt  events.txt
        // dimred "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Output_OLD\DIMRED\DIMRED_Params.txt"  events.txt


        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_FILE_EXT    = "FILE_EXT";
        public static string key_TIME_REDUCTION_FACTOR = "TIME_REDUCTION_FACTOR";
        public static string key_FREQ_REDUCTION_FACTOR = "FREQ_REDUCTION_FACTOR";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        public static string key_SMOOTH_WINDOW = "SMOOTH_WINDOW";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";


        public static void Dev(string[] args)
        {
            string title = "# DIMENSIONALITY REDUCTION OF SPECTROGRAM";
            string date = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            OscillationRecogniser.CheckArguments(args);


            string recordingPath = args[0];
            string iniPath = args[1];
            string outputDir = Path.GetDirectoryName(iniPath) + "\\";
            string opFName = args[2];
            string opPath = outputDir + opFName;
            Log.WriteIfVerbose("# Output folder =" + outputDir);


            //READ PARAMETER VALUES FROM INI FILE
            var config = new ConfigDictionary(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            int timeRedFactor = Int32.Parse(dict[key_TIME_REDUCTION_FACTOR]);
            int freqRedFactor = Int32.Parse(dict[key_FREQ_REDUCTION_FACTOR]);

            double frameOverlap = Double.Parse(dict[key_FRAME_OVERLAP]);
            double smoothWindow = Double.Parse(dict[key_SMOOTH_WINDOW]);   //smoothing window (seconds) before segmentation
            double thresholdSD = Double.Parse(dict[key_THRESHOLD]);       //segmentation threshold in noise SD
            double minDuration = Double.Parse(dict[key_MIN_DURATION]);    //min duration of segment & width of smoothing window in seconds 
            //double maxDuration = Double.Parse(dict[key_MAX_DURATION]);    //max duration of segment in seconds 
            int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);   //options to draw sonogram

            Log.WriteIfVerbose("# Freq band: {0} Hz - {1} Hz.)", timeRedFactor, freqRedFactor);
            Log.WriteIfVerbose("# Smoothing Window: {0}s.", smoothWindow);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();

            //ii: MAKE SONOGRAM
            Log.WriteLine("# Start sonogram.");
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.WindowOverlap  = frameOverlap;
            sonoConfig.SourceFName    = recording.FileName;
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            //sonoConfig.DynamicRange = dynamicRange;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();


            //#############################################################################################################################################
            // reduced energy sonogram
            var results = AI_DimRed(sonogram, timeRedFactor, freqRedFactor); //acoustic intensity
            var reducedSono = results.Item1;
            Log.WriteLine("# Finished intensity reduction.");
            //#############################################################################################################################################
            // reduced oscillation sonogram
            var tuple2 = OR_DimRed(sonogram, timeRedFactor, freqRedFactor);
            var orHits = tuple2.Item1;
            var orReduced = tuple2.Item2;
            Log.WriteLine("# Finished oscillation reduction.");
            //#############################################################################################################################################
            // reduced oscillation sonogram
            //var tuple3 = SH_DimRed(reducedSono, timeRedFactor, freqRedFactor, sonogram.FBinWidth);
            //var shHits = tuple3.Item1;
            //var shReduced = tuple3.Item2;
            //Log.WriteLine("# Finished stacked harmonics reduction.");
            //#############################################################################################################################################

            //var predictedEvents = results.Item2; //contain the segments detected
            //var Q = results.Item3;
            //var oneSD_dB = results.Item4;
            //var dBThreshold = results.Item5;
            //var intensity = results.Item6;
            //Log.WriteLine("# Signal:  Duration={0}, Sample Rate={1}", sonogram.Duration, sonogram.SampleRate);
            //Log.WriteLine("# Frames:  Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
            //                           sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
            //                          (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            //int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            //Log.WriteLine("# FreqBand: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            //Log.WriteLine("# Intensity array - noise removal: Q={0:f1}dB. 1SD={1:f3}dB. Threshold={2:f3}dB.", Q, oneSD_dB, dBThreshold);
            //Log.WriteLine("# Events:  Count={0}", predictedEvents.Count());
            //int pcHIF = 100;
            //if (intensity != null)
            //{
            //    int hifCount = intensity.Count(p => p > dBThreshold); //count of high intensity frames
            //    pcHIF = 100 * hifCount / sonogram.FrameCount;
            //}

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            string fname = Path.GetFileName(recordingPath);
            //string str = String.Format("{0}\t{1}\t{2}\t{3}", fname, sigDuration, count, pcHIF);
            //StringBuilder sb = AcousticEvent.WriteEvents(predictedEvents, str);
            //FileTools.WriteTextFile(opPath, sb.ToString());

            //draw images of sonograms
            string imagePath   = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
            string reducedPath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_reduced.png";
            string orHitsPath  = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_OR.png";
            string shHitsPath  = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + "_SH.png";

            //double min, max;
            //DataTools.MinMax(intensity, out min, out max);
            //double threshold_norm = dBThreshold / max; //min = 0.0;
            //intensity = DataTools.normalise(intensity);
            if (DRAW_SONOGRAMS > 0)
            {
                DrawSonogram(sonogram, imagePath);

                //SAVE extracted event as noise reduced image 
                //alter matrix dynamic range so user can determine correct dynamic range from image 
                //reducedSono = SNR.SetDynamicRange(reducedSono, 0.0, dynamicRange); //set event's dynamic range
                var results1 = BaseSonogram.Data2ImageData(reducedSono);
                ImageTools.DrawMatrix(results1.Item1, 1, 1, reducedPath);

                DrawSonogram(sonogram, orHits, orHitsPath);

                //DrawSonogram(sonogram, shHits, shHitsPath);

            }

            Log.WriteLine("# Finished recording:- " + Path.GetFileName(recordingPath));
            Console.ReadLine();
        } //Dev()


        public static System.Tuple<double[,]> AI_DimRed(BaseSonogram sonogram, int timeRedFactor, int freqRedFactor)
        {


            int freqBinCount  = sonogram.Configuration.FreqBinCount;
            int frameCount    = sonogram.FrameCount;

            int timeReducedCount = frameCount   / timeRedFactor;
            int freqReducedCount = freqBinCount / freqRedFactor;

            var reducedMatrix = new double[timeReducedCount, freqReducedCount];
            int cellArea = timeRedFactor  * freqRedFactor;
            for(int r=0; r< timeReducedCount; r++)
                for (int c = 0; c < freqReducedCount; c++)
                {
                    int or = r * timeRedFactor;
                    int oc = c * freqRedFactor;
                    double sum = 0.0;
                    for (int i = 0; i < timeRedFactor; i++)
                        for (int j = 0; j < freqRedFactor; j++)
                        {
                            sum += sonogram.Data[or+i, oc+j];
                        }
                    reducedMatrix[r, c] = sum / cellArea;
                }

            var tuple2 = System.Tuple.Create(reducedMatrix);
            return tuple2;
        }//end AI_DimRed



        public static System.Tuple<double[,], double[,]> OR_DimRed(BaseSonogram sonogram, int timeRedFactor, int freqRedFactor)
        {
            bool doSegmentation = false;
            int minHz = 0;
            int maxHz = sonogram.NyquistFrequency - 300;
            double dctDuration = 0.5;
            double dctThreshold = 13.0;
            bool normaliseDCT = false;
            int minOscilFreq = 4;
            int maxOscilFreq = 100;
            double eventThreshold = 0.5;    
            double minDuration = 0.5;  //sec
            double maxDuration = 10.0; //seconds


            List<AcousticEvent> predictedEvents;  //predefinition of results event list
            double[] scores;                      //predefinition of score array
            Double[,] hits;                       //predefinition of hits matrix - to superimpose on sonogram image
            double[] segments;                    //predefinition of segmentation of recording
            TimeSpan analysisTime;                //predefinition of Time duration taken to do analysis on this file  
            OscillationAnalysis.Execute((SpectralSonogram)sonogram, doSegmentation, minHz, maxHz, dctDuration, dctThreshold, normaliseDCT,
                                         minOscilFreq, maxOscilFreq, eventThreshold, minDuration, maxDuration,
                                         out scores, out predictedEvents, out hits, out segments, out analysisTime);



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

            var tuple2 = System.Tuple.Create(hits, reducedMatrix);
            return tuple2;
        }//end OR_DimRed

        /// <summary>
        /// search for stacked harmonics
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="timeRedFactor"></param>
        /// <param name="freqRedFactor"></param>
        /// <returns></returns>
        //public static System.Tuple<double[,], double[,]> SH_DimRed(double[,] matrix, int timeRedFactor, int freqRedFactor, double freqBinWidth)
        //{
        //    int minHz = 500;
        //    int maxHz = 5000;
        //    double dctThreshold = 40.0;
        //    bool normaliseDCT = false;
        //    int minPeriod = 500;
        //    int maxPeriod = 2500;
        //    double eventThreshold = 0.5;
        //    double minDuration = 0.5;  //sec


        //    //DETECT OSCILLATIONS
        //    //int minBin = (int)(minHz / freqBinWidth);
        //    //int maxBin = (int)(maxHz / freqBinWidth);
        //    //int hzWidth = maxHz - minHz;
        //    //Double[,] hits = HarmonicAnalysis.DetectHarmonicsUsingDCT(matrix, minBin, maxBin, hzWidth, normaliseDCT, minPeriod, maxPeriod, dctThreshold);
        //    //hits = HarmonicAnalysis.RemoveIsolatedHits(hits);

        //    //EXTRACT SCORES AND ACOUSTIC EVENTS
        //   // double[] scores = HarmonicAnalysis.GetHarmonicScores(hits, minHz, maxHz, freqBinWidth);
        //    //double[] oscFreq = HarmonicAnalysis.GetHDFrequency(hits, minHz, maxHz, freqBinWidth);

        //    //TRANSFER HITS TO REDUCED MATRIX
        //    int frameCount   = matrix.GetLength(0);
        //    int freqBinCount = matrix.GetLength(1);

        //    int timeReducedCount = frameCount / timeRedFactor;
        //    int freqReducedCount = freqBinCount / freqRedFactor;

        //    var reducedMatrix = new double[timeReducedCount, freqReducedCount];
        //    int cellArea = timeRedFactor * freqRedFactor;
        //    //for (int r = 0; r < timeReducedCount; r++)
        //    //    for (int c = 0; c < freqReducedCount; c++)
        //    //    {
        //    //        int or = r * timeRedFactor;
        //    //        int oc = c * freqRedFactor;
        //    //        double sum = 0.0;
        //    //        for (int i = 0; i < timeRedFactor; i++)
        //    //            for (int j = 0; j < freqRedFactor; j++)
        //    //            {
        //    //                sum += sonogram.Data[or + i, oc + j];
        //    //            }
        //    //        reducedMatrix[r, c] = sum / cellArea;
        //    //    }

        //    var tuple2 = System.Tuple.Create(hits, reducedMatrix);
        //    return tuple2;
        //}//end SH_DimRed




        public static void DrawSonogram(BaseSonogram sonogram, string path)
        {
            Log.WriteLine("# Start sono image.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetScoreTrack(segmentation, 0.0, 1.0, eventThreshold));
                //image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }
        public static void DrawSonogram(BaseSonogram sonogram, double[,]hits, string path)
        {
            Log.WriteLine("# Start sono image with hits.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                //image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                //image.AddTrack(Image_Track.GetScoreTrack(segmentation, 0.0, 1.0, eventThreshold));
                double maxScore = 42.0; //6 oscillations per each of 7 colour. Brown = score > 42
                image.AddSuperimposedMatrix(hits, maxScore);
                image.Save(path);
            }
        }


        public static void CheckArguments(string[] args)
        {
            if (args.Length < 3)
            {
                Log.WriteLine("NUMBER OF COMMAND LINE ARGUMENTS = {0}", args.Length);
                foreach (string arg in args) Log.WriteLine(arg + "  ");
                Log.WriteLine("YOU REQUIRE {0} COMMAND LINE ARGUMENTS\n", 3);
                Usage();
            }
            CheckPaths(args);
        }

        /// <summary>
        /// this method checks for the existence of the two files whose paths are expected as first two arguments of the command line.
        /// </summary>
        /// <param name="args"></param>
        public static void CheckPaths(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("Cannot find recording file <" + args[0] + ">");
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
            if (!File.Exists(args[1]))
            {
                Console.WriteLine("Cannot find initialisation file: <" + args[1] + ">");
                Usage();
                Console.WriteLine("Press <ENTER> key to exit.");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }


        public static void Usage()
        {
            Console.WriteLine("INCORRECT COMMAND LINE.");
            Console.WriteLine("USAGE:");
            Console.WriteLine("OscillationDetection.exe recordingPath iniPath outputFileName");
            Console.WriteLine("where:");
            Console.WriteLine("recordingFileName:-(string) The path of the audio file to be processed.");
            Console.WriteLine("iniPath:-          (string) The path of the ini file containing all required parameters.");
            Console.WriteLine("outputFileName:-   (string) The name of the output file.");
            Console.WriteLine("                            By default, the output dir is that containing the ini file.");
            Console.WriteLine("");
            Console.WriteLine("\nPress <ENTER> key to exit.");
            Console.ReadLine();
            System.Environment.Exit(1);
        }   
    
    
    } //end class
}
