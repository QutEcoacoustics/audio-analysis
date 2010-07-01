using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using AudioAnalysisTools;
using TowseyLib;



namespace AnalysisPrograms
{   
    /// <summary>
    /// Implements a simple form of Syntactic Pattern Recognition to find defined bird calls in spectra.
    /// </summary>
    class SPR  //Syntactic Pattern Recognition
    {
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        public static string key_CALL_NAME       = "CALL_NAME";
        //public static string key_FILE_EXT        = "FILE_EXT";
        public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_MIN_HZ          = "MIN_HZ";
        public static string key_MAX_HZ          = "MAX_HZ";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_SPT_INTENSITY_THRESHOLD = "SPT_INTENSITY_THRESHOLD";
        public static string key_SPT_SMALL_LENGTH_THRESHOLD = "SPT_SMALL_LENGTH_THRESHOLD";
        //public static string key_DCT_DURATION    = "DCT_DURATION";
        //public static string key_DCT_THRESHOLD   = "DCT_THRESHOLD";
        //public static string key_MIN_OSCIL_FREQ = "MIN_OSCIL_FREQ";
        //public static string key_MAX_OSCIL_FREQ = "MAX_OSCIL_FREQ";
        //public static string key_MIN_DURATION = "MIN_DURATION";
        //public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_MIN_DURATION = "MIN_DURATION";
        public static string key_MAX_DURATION = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";


        public static void Dev(string[] args)
        {
            //spr C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3  C:\SensorNetworks\Output\SPT\SPR_WHIPBIRD_Params.txt events.txt 

            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("Syntactic Pattern Recognition\n");
            //StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            //sb.Append("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");
            
            Log.Verbosity = 1;

            if (args.Length != 3)
            {
                Console.WriteLine("The arguments for SPR are: wavFile intensityThreshold");
                Console.WriteLine();
                Console.WriteLine("Recording File: path to recording file.");
                Console.WriteLine("Ini File:       in directory where output files and images will be placed.");
                Console.WriteLine("Output File:    where events will be written");
                Console.ReadLine();
                Environment.Exit(1);
            }

            string recordingPath = args[0];
            string iniPath       = args[1];
            string outputDir     = Path.GetDirectoryName(iniPath) + "\\"; //output directory is the one in which ini file is located.
            string opFName       = args[2];
            string opPath        = outputDir + opFName;
            Log.WriteIfVerbose("# Output folder =" + outputDir);


            //READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            string callName = dict[key_CALL_NAME];
            int minHz = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz = Int32.Parse(dict[key_MAX_HZ]);
            double intensityThreshold = Convert.ToDouble(dict[key_SPT_INTENSITY_THRESHOLD]);
            int smallLengthThreshold = Convert.ToInt32(dict[key_SPT_SMALL_LENGTH_THRESHOLD]);
            //SPT_SMALL_LENGTH_THRESHOLD
            double minDuration  = Double.Parse(dict[key_MIN_DURATION]);       //min duration of event in seconds 
            double maxDuration  = Double.Parse(dict[key_MAX_DURATION]);       //max duration of event in seconds 
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);  //min score for an acceptable event
            int DRAW_SONOGRAMS = Convert.ToInt16(dict[key_DRAW_SONOGRAMS]);

            BaseSonogram sonogram = null;
            List<AcousticEvent> predictedEvents = null;
            double[,] hits = null;
            double[] scores = null;
            if (callName.Equals("WHIPBIRD"))
            {
                //SPT
                var result1 = SPT.doSPT(recordingPath, intensityThreshold, smallLengthThreshold);
                //var result2 = SPR.doSPR(result1.Item2, intensityThreshold + 1.0, smallLengthThreshold);

                //SPR
                Log.WriteLine("SPR start - Intensity Threshold = " + intensityThreshold);
                double sensitivity = 0.7; //lower value = more sensitive
                var mHori = MarkLine(result1.Item2, smallLengthThreshold, intensityThreshold, 0, sensitivity);
                //var mHori = MarkLine(matrix, lineLength, intensityThreshold, 180);
                sensitivity = 0.8;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item2, smallLengthThreshold - 3, intensityThreshold, 85, sensitivity);
                //var mBack = MarkLine(matrix, lineLength, intensityThreshold, 100);
                Log.WriteLine("SPR finished");

                hits = DataTools.AddMatrices(mHori, mVert);
                var result3 = DetectWhipBird(mHori, mVert, smallLengthThreshold);
                scores = result3.Item2;
                sonogram = result1.Item1;

                string fileName = Path.GetFileNameWithoutExtension(recordingPath);
                predictedEvents = AcousticEvent.ConvertIntensityArray2Events(scores, minHz, maxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, minDuration, maxDuration, fileName);
            }

            // SAVE IMAGE
            //draw images of sonograms
            //double eventThreshold = 0.2;
            string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath);
            string suffix = string.Empty;
            while (File.Exists(imagePath + suffix + ".png"))
            {
                suffix = (suffix == string.Empty) ? "1" : (int.Parse(suffix) + 1).ToString();
            }
            string newPath = imagePath + suffix + ".png";
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, newPath, hits, scores, predictedEvents, eventThreshold);
            }
            else
                if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
                {
                    DrawSonogram(sonogram, newPath, hits, scores, predictedEvents, eventThreshold);
                }


            Log.WriteIfVerbose("Image saved to: " + imagePath);
            //string savePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath);
            //string suffix = string.Empty;
            //Image im = sonogram.GetImage(false, false);
            //string newPath = savePath + suffix + ".jpg";
            //im.Save(newPath);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main


        public static double[,] MarkLine(double[,] m, int lineLength, double intensityThreshold, int degrees, double sensitivity)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var mOut = new double[rows, cols];
            double sumThreshold = lineLength * sensitivity;

            switch (degrees)
            {

                case 0:                 // find horizontal lines in spectorgram
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = 5; c < cols - lineLength; c++)
                        {
                            if(m[r, c] < 0.00001) continue;
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r + l, c] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r + l, c] = 1.0;
                        }
                    }
                    break;

                case 90:                 // find vertical lines in spectorgram
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = 5; c < cols - lineLength; c++)
                        {
                            if (m[r, c] < 0.00001) continue;
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r, c + l] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r, c + l] = 1.0;
                        }
                    }
                    break;

                default:
                    double cosAngle = Math.Cos(Math.PI * degrees / 180);
                    double sinAngle = Math.Sin(Math.PI * degrees / 180);
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = 5; c < cols - lineLength; c++)
                        {
                            if (m[r, c] < 0.00001) continue;
                            double sum = 0.0;
                            for (int j = 0; j < lineLength; j++) 
                                if (m[r + (int)(cosAngle * j), c + (int)(sinAngle * j)] > intensityThreshold) sum++;
                            if (sum > sumThreshold)
                                for (int j = 0; j < lineLength; j++) mOut[r + (int)(cosAngle * j), c + (int)(sinAngle * j)] = 1.0;
                        }
                    }
                    break;

            } //switch
            return mOut;
        }// MarkLine()


        public static Tuple<List<AcousticEvent>, double[]> DetectWhipBird(double[,] mHori, double[,] mVert, int lineLength)
        {
            int rows = mHori.GetLength(0);
            int cols = mHori.GetLength(1);

            double optimumWhistleDuration = 86 * 1.4; //86 = frames/sec.
            int minBound_Whistle = 60;
            int maxBound_Whistle = 70;
            int whistleBand = maxBound_Whistle - minBound_Whistle;
            var whistleScores = new double[rows];


            int whipDuration = 12; //frames
            int minBound_Whip = 15;
            int maxBound_Whip = 200;
            int whipBand = maxBound_Whip - minBound_Whip;
            double whipThreshold = (whipDuration * whipBand) * 0.33;
            var whipScores = new double[rows];

            for (int r = (int)optimumWhistleDuration; r < rows - lineLength; r++)
            {
                //whistle detector
                var whistle = new double[whistleBand];
                for (int j = 0; j < whistleBand; j++)
                {
                    for (int i = r - (int)optimumWhistleDuration; i <= r; i++)
                    {
                        if (mHori[r, minBound_Whistle + j] < 0.0001) continue;
                        whistle[j] += mHori[i, minBound_Whistle + j];
                    }
                }
                whistleScores[r] = whistle[DataTools.GetMaxIndex(whistle)];

                //whip detector
                var whip = new double[whipBand];
                for (int i = 0; i < whipDuration; i++)
                {
                    for (int j = 0; j < whipBand; j++)
                    {
                        whip[j] += mVert[r + i, minBound_Whip + j];
                    }
                }
                double total = 0.0;
                for (int j = 0; j < whipBand; j++) total += whip[j];
                whipScores[r] = total;

            } //for all rows
            
            //normalise whistle scores
            for (int i = 0; i < whistleScores.Length; i++)
            {
                whistleScores[i] = whistleScores[i] / optimumWhistleDuration;
                if (whistleScores[i] > 1.0) whistleScores[i] = 1.0;
            }
            //normalise whip scores
            for (int i = 0; i < whipScores.Length; i++)
            {
                whipScores[i] = whipScores[i] / whipThreshold;
                if (whipScores[i] > 1.0) whipScores[i] = 1.0;
            }

            //combine scores and extract events
            var predictedEvents = new List<AcousticEvent>();
            var scores = new double[rows];
            for (int i = 0; i < whipScores.Length; i++)
                if ((whistleScores[i] > 0.4) && (whipScores[i] > 0.4)) scores[i] = (whistleScores[i] + whipScores[i]) /2;
            var tuple = Tuple.Create(predictedEvents, scores);
            return tuple;
        }//end detect Whipbird


        public static void DrawSonogram(BaseSonogram sonogram, string path, double[,] hits, double[] scores,
                                List<AcousticEvent> predictedEvents, double eventThreshold)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration));
                image.AddTrack(Image_Track.GetScoreTrack(scores, 0.0, 1.0, eventThreshold));
                double maxScore = 50.0;
                image.AddSuperimposedMatrix(hits, maxScore);
                //if (intensity != null)
                //{
                //    double min, max;
                //    DataTools.MinMax(intensity, out min, out max);
                //    double threshold_norm = eventThreshold / max; //min = 0.0;
                //    intensity = DataTools.normalise(intensity);
                //    image.AddTrack(Image_Track.GetScoreTrack(intensity, 0.0, 1.0, eventThreshold));
                //}
                image.AddEvents(predictedEvents);
                image.Save(path);
            }
        }//drawSonogram()



    }//class
}
