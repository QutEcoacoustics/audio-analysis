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
        public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_FRAME_OVERLAP   = "FRAME_OVERLAP";
        public static string key_SPT_INTENSITY_THRESHOLD    = "SPT_INTENSITY_THRESHOLD";
        public static string key_SPT_SMALL_LENGTH_THRESHOLD = "SPT_SMALL_LENGTH_THRESHOLD";
        public static string key_WHISTLE_MIN_HZ  = "WHISTLE_MIN_HZ";
        public static string key_WHISTLE_MAX_HZ  = "WHISTLE_MAX_HZ";
        public static string key_WHISTLE_DURATION= "WHISTLE_DURATION";
        public static string key_WHIP_MIN_HZ     = "WHIP_MIN_HZ";
        public static string key_WHIP_MAX_HZ     = "WHIP_MAX_HZ";
        public static string key_WHIP_DURATION   = "WHIP_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";


        public static void Dev(string[] args)
        {
            //WHIPBIRD
            //spr C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3  C:\SensorNetworks\Output\SPT\SPR_WHIPBIRD_Params.txt events.txt 
            //CURLEW
            //spr C:\SensorNetworks\WavFiles\Curlew\Curlew2\HoneymoonBay_StBees_20080914-003000.wav  C:\SensorNetworks\Output\SPR_CURLEW\SPR_CURLEW_Params.txt events.txt 
            //CURRAWONG
            //spr C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.wav  C:\SensorNetworks\Output\SPR_CURRAWONG\SPR_CURRAWONG_Params.txt events.txt  
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

            string callName   = dict[key_CALL_NAME];
            double frameOverlap = Convert.ToDouble(dict[key_FRAME_OVERLAP]);
            //SPT PARAMETERS
            double intensityThreshold = Convert.ToDouble(dict[key_SPT_INTENSITY_THRESHOLD]);
            int smallLengthThreshold  = Convert.ToInt32(dict[key_SPT_SMALL_LENGTH_THRESHOLD]);
            //WHIPBIRD PARAMETERS
            int whistle_MinHz = Int32.Parse(dict[key_WHISTLE_MIN_HZ]);
            int whistle_MaxHz = Int32.Parse(dict[key_WHISTLE_MAX_HZ]);
            double optimumWhistleDuration = Double.Parse(dict[key_WHISTLE_DURATION]);   //optimum duration of whistle in seconds 
            int whip_MinHz      = (dict.ContainsKey(key_WHIP_MIN_HZ)) ? Int32.Parse(dict[key_WHIP_MIN_HZ]) : 0;
            //if() Int32.Parse(dict[key_WHIP_MIN_HZ]);
            int whip_MaxHz      = (dict.ContainsKey(key_WHIP_MAX_HZ))   ? Int32.Parse(dict[key_WHIP_MAX_HZ])    : 0;
            double whipDuration = (dict.ContainsKey(key_WHIP_DURATION)) ? Double.Parse(dict[key_WHIP_DURATION]) : 0.0; //duration of whip in seconds 

            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);     //min score for an acceptable event
            int DRAW_SONOGRAMS = Convert.ToInt16(dict[key_DRAW_SONOGRAMS]);


            //LOAD RECORDING AND MAKE SONOGRAM
            var recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); //down sample if necessary

            var sonoConfig = new SonogramConfig
            {
                NoiseReductionType = NoiseReductionType.NONE,
                WindowOverlap = frameOverlap
            };
            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            //BaseSonogram sonogram = sonogram = AED.FileToSonogram(recordingPath);


            List<AcousticEvent> predictedEvents = null;
            double[,] hits = null;
            double[] scores = null;

            if (callName.Equals("WHIPBIRD"))
            {
                //SPT
                var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);
                //SPR
                Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
                int slope = 0; //degrees of the circle. i.e. 90 = vertical line.
                double sensitivity = 0.7; //lower value = more sensitive
                var mHori = MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
                slope = 85;
                sensitivity = 0.8;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item1, slope, smallLengthThreshold - 3, intensityThreshold+1, sensitivity);
                Log.WriteLine("SPR finished");

                //int minBound_Whistle = 60;
                //int maxBound_Whistle = 70;
                int minBound_Whistle = (int)(whistle_MinHz / sonogram.FBinWidth);
                int maxBound_Whistle = (int)(whistle_MaxHz / sonogram.FBinWidth);
                int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration); //86 = frames/sec.
                //int minBound_Whip = 15;
                //int maxBound_Whip = 200;
                int minBound_Whip = (int)(whip_MinHz / sonogram.FBinWidth);
                int maxBound_Whip = (int)(whip_MaxHz / sonogram.FBinWidth);
                int whipFrames = (int)(sonogram.FramesPerSecond * whipDuration); //86 = frames/sec.
                var result3 = DetectWhipBird(mHori, mVert, minBound_Whistle, maxBound_Whistle, whistleFrames, minBound_Whip, maxBound_Whip, whipFrames, smallLengthThreshold);
                scores = result3.Item2;
                hits = DataTools.AddMatrices(mHori, mVert);

                string fileName = Path.GetFileNameWithoutExtension(recordingPath);
                double maxDuration = 2 * (optimumWhistleDuration + whipDuration);
                predictedEvents = AcousticEvent.ConvertIntensityArray2Events(scores, whip_MinHz, whip_MaxHz, 
                                                              sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, whipDuration*0.6, maxDuration, fileName);
            }
            else if (callName.Equals("CURLEW"))
            {
                //SPT
                var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);
                //SPR
                Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
                int slope = 20; //degrees of the circle. i.e. 90 = vertical line.
                //slope = 210;
                double sensitivity = 0.7; //lower value = more sensitive
                var mHori = MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
                slope = 160;
                //slope = 340;
                sensitivity = 0.7;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item1, slope, smallLengthThreshold - 3, intensityThreshold + 1, sensitivity);
                Log.WriteLine("SPR finished");

                int minBound_Whistle = (int)(whistle_MinHz / sonogram.FBinWidth);
                int maxBound_Whistle = (int)(whistle_MaxHz / sonogram.FBinWidth);
                int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration); //86 = frames/sec.
                var result3 = DetectCurlew(mHori, mVert, minBound_Whistle, maxBound_Whistle, whistleFrames + 10, smallLengthThreshold);
                scores = result3.Item2;
                hits = DataTools.AddMatrices(mHori, mVert);

                string fileName = Path.GetFileNameWithoutExtension(recordingPath);
                double maxDuration = 3 * optimumWhistleDuration;
                predictedEvents = AcousticEvent.ConvertIntensityArray2Events(scores, whistle_MinHz, whistle_MaxHz,
                                                              sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, 0.5, maxDuration, fileName);
            }
            else if (callName.Equals("CURRAWONG"))
            {
                //SPT
                var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);
                //SPR
                Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
                int slope = 70; //degrees of the circle. i.e. 90 = vertical line.
                //slope = 210;
                double sensitivity = 0.7; //lower value = more sensitive
                var mHori = MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
                slope = 110;
                //slope = 340;
                sensitivity = 0.7;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item1, slope, smallLengthThreshold - 3, intensityThreshold + 1, sensitivity);
                Log.WriteLine("SPR finished");

                int minBound_Whistle = (int)(whistle_MinHz / sonogram.FBinWidth);
                int maxBound_Whistle = (int)(whistle_MaxHz / sonogram.FBinWidth);
                int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration); //86 = frames/sec.
                var result3 = DetectCurlew(mHori, mVert, minBound_Whistle, maxBound_Whistle, whistleFrames + 10, smallLengthThreshold);
                scores = result3.Item2;
                hits = DataTools.AddMatrices(mHori, mVert);

                string fileName = Path.GetFileNameWithoutExtension(recordingPath);
                double maxDuration = 3 * optimumWhistleDuration;
                predictedEvents = AcousticEvent.ConvertIntensityArray2Events(scores, whistle_MinHz, whistle_MaxHz,
                                                              sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, 0.5, maxDuration, fileName);
            }

            Log.WriteIfVerbose("Number of Events: " + predictedEvents.Count);
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



        public static double[,] MarkLine(double[,] m, int degrees, int lineLength, double intensityThreshold, double sensitivity)
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
                        for (int c = lineLength; c < cols - lineLength; c++)
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


        public static Tuple<List<AcousticEvent>, double[]> DetectWhipBird(double[,] mHori, double[,] mVert, 
                                                 int minBound_Whistle, int maxBound_Whistle, int whistleDuration, 
                                                 int minBound_Whip,    int maxBound_Whip,    int whipDuration, int lineLength)
        {
            int rows = mHori.GetLength(0);
            int cols = mHori.GetLength(1);

            int whistleBand = maxBound_Whistle - minBound_Whistle;
            var whistleScores = new double[rows];

            int whipBand = maxBound_Whip - minBound_Whip;
            double whipThreshold = (whipDuration * whipBand) * 0.33;
            var whipScores = new double[rows];

            for (int r = whistleDuration; r < rows - lineLength; r++)
            {
                //whistle detector
                var whistle = new double[whistleBand];
                for (int j = 0; j < whistleBand; j++)
                {
                    for (int i = r - whistleDuration; i <= r; i++)
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
                whistleScores[i] = whistleScores[i] / (double)whistleDuration;
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
                if ((whistleScores[i] > 0.2) && (whipScores[i] > 0.2)) scores[i] = (whistleScores[i] + whipScores[i]) /2;
            var tuple = Tuple.Create(predictedEvents, scores);
            return tuple;
        }//end detect Whipbird



        public static Tuple<List<AcousticEvent>, double[]> DetectCurlew(double[,] rising, double[,] falling,
                                         int minBound_Whistle, int maxBound_Whistle, int whistleDuration, int lineLength)
        {
            int rows = rising.GetLength(0);
            int cols = rising.GetLength(1);

            double cosRisingAngle  = Math.Cos(Math.PI * 200 / 180);
            double sinRisingAngle  = Math.Sin(Math.PI * 200 / 180);
            double cosFallingAngle = Math.Cos(Math.PI * 340 / 180);
            double sinFallingAngle = Math.Sin(Math.PI * 340 / 180);

            int whistleBand = maxBound_Whistle - minBound_Whistle;
            var risingScores  = new double[rows];
            var fallingScores = new double[rows];

            for (int r = whistleDuration; r < rows - lineLength; r++)
            {
                //rising whistle detector
                var whistle = new double[whistleBand];
                for (int j = 0; j < whistleBand; j++) //for each freq bin in the band
                {
                    if (rising[r, minBound_Whistle + j] < 0.0001) continue;
                    for (int i = 0; i < whistleDuration; i++) //for length of line
                    {
                        int px = (int)(cosRisingAngle * i);
                        int py = (int)(sinRisingAngle * i);
                        whistle[j] += rising[r+px, minBound_Whistle + j + py];
                    }
                }
                risingScores[r] = whistle[DataTools.GetMaxIndex(whistle)];
                //if (risingScores[r] > 0.0) Console.WriteLine("{0}  {1}", r, risingScores[r]);

                //falling whistle detector
                whistle = new double[whistleBand];
                for (int j = 0; j < whistleBand; j++)
                {
                    if (falling[r, minBound_Whistle + j] < 0.0001) continue;
                    for (int i = 0; i < whistleDuration; i++) //for length of line
                    {
                        int px = (int)(cosFallingAngle * i);
                        int py = (int)(sinFallingAngle * i);
                        whistle[j] += falling[r + px, minBound_Whistle + j + py];
                    }
                }
                fallingScores[r] = whistle[DataTools.GetMaxIndex(whistle)];
                //if (fallingScores[r] > 0.0) Console.WriteLine("{0}  {1}", r, fallingScores[r]);

            } //for all rows

            //normalise whistle scores
            for (int i = 0; i < risingScores.Length; i++)
            {
                risingScores[i] = risingScores[i] / (double)whistleDuration;
                if (risingScores[i] > 1.0) risingScores[i] = 1.0;
                fallingScores[i] = fallingScores[i] / (double)whistleDuration;
                if (fallingScores[i] > 1.0) fallingScores[i] = 1.0;
                //if (risingScores[i]  > 0.0) Console.WriteLine("{0} \t{1:f2} \t{2:f2}", i, risingScores[i], fallingScores[i]);

            }

            //combine scores and extract events
            var predictedEvents = new List<AcousticEvent>();
            var scores = new double[rows];
            for (int i = 0; i < risingScores.Length; i++)
                if ((risingScores[i] > 0.2) && (fallingScores[i] > 0.2)) scores[i] = (risingScores[i] + fallingScores[i])/2;
            var tuple = Tuple.Create(predictedEvents, scores);
            return tuple;
        }//end detect Curlew


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
