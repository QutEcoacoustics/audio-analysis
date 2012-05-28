using System;
using System.Collections.Generic;
using System.IO;

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
        public static string key_MIN_DURATION    = "MIN_DURATION";
        public static string key_MAX_DURATION    = "MAX_DURATION";
        public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS  = "DRAW_SONOGRAMS";


        public static void Dev(string[] args)
        {
            //WHIPBIRD
            //spr C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3  C:\SensorNetworks\Output\SPR_WHIPBIRD\SPR_WHIPBIRD_Params.txt events.txt 
            //spr C:\SensorNetworks\WavFiles\BridgeCreek\WhipbirdCalls\file0151mono.wav_segment_19.wav C:\SensorNetworks\Output\SPR_WHIPBIRD\SPR_WHIPBIRD_Params.txt events.txt 
            //CURLEW
            //spr C:\SensorNetworks\WavFiles\Curlew\Curlew2\HoneymoonBay_StBees_20080914-003000.wav  C:\SensorNetworks\Output\SPR_CURLEW\SPR_CURLEW_Params.txt events.txt 
            //spr C:\SensorNetworks\WavFiles\Curlew\Curlew_JasonTagged\West_Knoll_Bees_20091102-210000.mp3  C:\SensorNetworks\Output\SPR_CURLEW\SPR_CURLEW_Params.txt events.txt 
            //CURRAWONG
            //spr C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.wav  C:\SensorNetworks\Output\SPR_CURRAWONG\SPR_CURRAWONG_Params.txt events.txt  
            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("Syntactic Pattern Recognition\n");
            //StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            //sb.Append("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");
            
            Log.Verbosity = 1;

            if (args.Length != 3)
            {
                Console.WriteLine("INCORRECT NUMBER OF ARGUMENTS, i.e. " + args.Length);
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


            // A: READ PARAMETER VALUES FROM INI FILE
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
            int whip_MaxHz      = (dict.ContainsKey(key_WHIP_MAX_HZ))   ? Int32.Parse(dict[key_WHIP_MAX_HZ])    : 0;
            double whipDuration = (dict.ContainsKey(key_WHIP_DURATION)) ? Double.Parse(dict[key_WHIP_DURATION]) : 0.0; //duration of whip in seconds 
            //CURLEW PARAMETERS
            double minDuration = (dict.ContainsKey(key_MIN_DURATION)) ? Double.Parse(dict[key_MIN_DURATION]) : 0.0; //min duration of call in seconds 
            double maxDuration = (dict.ContainsKey(key_MAX_DURATION)) ? Double.Parse(dict[key_MAX_DURATION]) : 0.0; //duration of call in seconds 
            
            double eventThreshold = Double.Parse(dict[key_EVENT_THRESHOLD]);     //min score for an acceptable event
            int DRAW_SONOGRAMS = Convert.ToInt16(dict[key_DRAW_SONOGRAMS]);


            // B: CHECK to see if conversion from .MP3 to .WAV is necessary
            var destinationAudioFile = recordingPath;

            //LOAD RECORDING AND MAKE SONOGRAM
            BaseSonogram sonogram = null;
            using (var recording = new AudioRecording(destinationAudioFile))
            {
                if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz(); // down sample if necessary

                var sonoConfig = new SonogramConfig
                {
                    NoiseReductionType = NoiseReductionType.NONE,
                    //NoiseReductionType = NoiseReductionType.STANDARD,
                    WindowOverlap = frameOverlap
                };
                sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            }

            List<AcousticEvent> predictedEvents = null;
            double[,] hits = null;
            double[] scores = null;

            var audioFileName = Path.GetFileNameWithoutExtension(destinationAudioFile);

            if (callName.Equals("WHIPBIRD"))
            {
                //SPT
                var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);
                //SPR
                Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
                int slope = 0; //degrees of the circle. i.e. 90 = vertical line.
                double sensitivity = 0.7; //lower value = more sensitive
                var mHori = MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
                slope = 87; //84
                sensitivity = 0.8;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item1, slope, smallLengthThreshold - 4, intensityThreshold+1, sensitivity);
                Log.WriteLine("SPR finished");
                Log.WriteLine("Extract Whipbird calls - start");

                int minBound_Whistle = (int)(whistle_MinHz / sonogram.FBinWidth);
                int maxBound_Whistle = (int)(whistle_MaxHz / sonogram.FBinWidth);
                int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration); //86 = frames/sec.
                int minBound_Whip = (int)(whip_MinHz / sonogram.FBinWidth);
                int maxBound_Whip = (int)(whip_MaxHz / sonogram.FBinWidth);
                int whipFrames = (int)(sonogram.FramesPerSecond * whipDuration); //86 = frames/sec.
                var result3 = DetectWhipBird(mHori, mVert, minBound_Whistle, maxBound_Whistle, whistleFrames, minBound_Whip, maxBound_Whip, whipFrames, smallLengthThreshold);
                scores = result3.Item1;
                hits = DataTools.AddMatrices(mHori, mVert);

                predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, whip_MinHz, whip_MaxHz, 
                                                              sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, minDuration, maxDuration);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = audioFileName;
                    ev.Name = callName;
                }

                sonogram.Data = result1.Item1;
                Log.WriteLine("Extract Whipbird calls - finished");
            }
            else if (callName.Equals("CURLEW"))
            {
                //SPT
                double backgroundThreshold = 4.0;
                var result1 = SNR.NoiseReduce(sonogram.Data, NoiseReductionType.STANDARD, backgroundThreshold);
                //var result1 = SPT.doSPT(sonogram, intensityThreshold, smallLengthThreshold);
                //var result1 = doNoiseRemoval(sonogram, intensityThreshold, smallLengthThreshold);

                //SPR
                Log.WriteLine("SPR start: intensity threshold = " + intensityThreshold);
                int slope = 20; //degrees of the circle. i.e. 90 = vertical line.
                double sensitivity = 0.8; //lower value = more sensitive
                var mHori = MarkLine(result1.Item1, slope, smallLengthThreshold, intensityThreshold, sensitivity);
                slope = 160;
                sensitivity = 0.8;        //lower value = more sensitive
                var mVert = MarkLine(result1.Item1, slope, smallLengthThreshold - 3, intensityThreshold + 1, sensitivity);
                Log.WriteLine("SPR finished");

                //detect curlew calls
                int minBound_Whistle = (int)(whistle_MinHz / sonogram.FBinWidth);
                int maxBound_Whistle = (int)(whistle_MaxHz / sonogram.FBinWidth);
                int whistleFrames = (int)(sonogram.FramesPerSecond * optimumWhistleDuration); 
                var result3 = DetectCurlew(mHori, mVert, minBound_Whistle, maxBound_Whistle, whistleFrames, smallLengthThreshold);

                //process curlew scores - look for curlew characteristic periodicity
                double minPeriod = 1.2;
                double maxPeriod = 1.8;
                int minPeriod_frames = (int)Math.Round(sonogram.FramesPerSecond * minPeriod);
                int maxPeriod_frames = (int)Math.Round(sonogram.FramesPerSecond * maxPeriod);
                scores = DataTools.filterMovingAverage(result3.Item1, 21);
                scores = DataTools.PeriodicityDetection(scores, minPeriod_frames, maxPeriod_frames);

                //extract events
                predictedEvents = AcousticEvent.ConvertScoreArray2Events(scores, whistle_MinHz, whistle_MaxHz, sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, minDuration, maxDuration);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = audioFileName;
                    ev.Name = callName;
                }

                hits = DataTools.AddMatrices(mHori, mVert);
                sonogram.Data = result1.Item1;
                Log.WriteLine("Extract Curlew calls - finished");
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
                scores = result3.Item1;
                hits = DataTools.AddMatrices(mHori, mVert);

                predictedEvents = AcousticEvent.ConvertIntensityArray2Events(scores, whistle_MinHz, whistle_MaxHz,
                                                              sonogram.FramesPerSecond, sonogram.FBinWidth,
                                                              eventThreshold, 0.5, maxDuration);
                foreach (AcousticEvent ev in predictedEvents)
                {
                    ev.SourceFileName = audioFileName;
                    //ev.Name = callName;
                }
            }

            //write event count to results file. 
            double sigDuration = sonogram.Duration.TotalSeconds;
            //string fname = Path.GetFileName(recordingPath);
            int count = predictedEvents.Count;
            Log.WriteIfVerbose("Number of Events: " + count);
            string str = String.Format("{0}\t{1}\t{2}", callName, sigDuration, count);
            FileTools.WriteTextFile(opPath, AcousticEvent.WriteEvents(predictedEvents, str).ToString());


            // SAVE IMAGE
            string imageName = outputDir + audioFileName;
            string imagePath = imageName + ".png";
            if (File.Exists(imagePath))
            {
                int suffix = 1;
                while (File.Exists(imageName + "." + suffix.ToString() + ".png")) suffix++;
                //{
                //    suffix = (suffix == string.Empty) ? "1" : (int.Parse(suffix) + 1).ToString();
                //}
                //File.Delete(outputDir + audioFileName + "." + suffix.ToString() + ".png");
                File.Move(imagePath, imageName + "." + suffix.ToString() + ".png");
            }
            //string newPath = imagePath + suffix + ".png";
            if (DRAW_SONOGRAMS == 2)
            {
                DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold);
            }
            else
                if ((DRAW_SONOGRAMS == 1) && (predictedEvents.Count > 0))
                {
                    DrawSonogram(sonogram, imagePath, hits, scores, predictedEvents, eventThreshold);
                }


            Log.WriteIfVerbose("Image saved to: " + imagePath);
            //string savePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath);
            //string suffix = string.Empty;
            //Image im = sonogram.GetImage(false, false);
            //string newPath = savePath + suffix + ".jpg";
            //im.Save(newPath);

            Console.WriteLine("\nFINISHED RECORDING!");
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
                    for (int r = 0; r < rows - lineLength; r++)
                    {
                        for (int c = 0; c < cols - lineLength; c++)
                        {
                            if(m[r, c] < 0.00001) continue;
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r + l, c] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r + l, c] = 1.0;
                        }
                    }
                    break;

                case 90:                 // find vertical lines in spectorgram
                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols - lineLength; c++)
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


        //public static Tuple<double[,]> doNoiseRemoval(BaseSonogram sonogram, double intensityThreshold, int smallLengthThreshold)
        //{
        //    Log.WriteLine("Wiener filter start");
        //    var w = Matlab.wiener2(7, m);
        //    Log.WriteLine("Wiener filter end");

        //    Log.WriteLine("Remove subband mode intensities start");
        //    var s = AcousticEventDetection.removeSubbandModeIntensities(w);
        //    Log.WriteLine("Remove subband mode intensities end");

        //    return Tuple.Create(w);
        //}



        public static Tuple<double[]> DetectWhipBird(double[,] mHori, double[,] mVert, 
                                                 int minBound_Whistle, int maxBound_Whistle, int optimumWhistleDuration, 
                                                 int minBound_Whip,    int maxBound_Whip,    int whipDuration, int lineLength)
        {
            int rows = mHori.GetLength(0);
            int cols = mHori.GetLength(1);

            int whistleBand = maxBound_Whistle - minBound_Whistle;
            var whistleScores = new double[rows];

            int whipBand = maxBound_Whip - minBound_Whip;
            int whipThreshold = 3;
            double whipNormalise = whipBand * 0.8;
            var whipScores = new double[rows];

            for (int r = 0; r < rows - lineLength; r++)
            {
                // whistle detector
                var whistle = new double[whistleBand];
                int start = r - optimumWhistleDuration;
                if (start < 0) start = 0;
                for (int j = 0; j < whistleBand; j++)
                {
                    for (int i = start; i <= r; i++)
                    {
                        whistle[j] += mHori[i, minBound_Whistle + j];
                    }
                }
                whistleScores[r] = whistle[DataTools.GetMaxIndex(whistle)] / (double)optimumWhistleDuration; //normalise;
                if (whistleScores[r] > 1.0) whistleScores[r] = 1.0;

                //whip detector
                var whip = new double[whipBand];
                double total = 0.0;
                for (int j = 0; j < whipBand; j++)//for each freq bin
                {
                    for (int i = 0; i < whipDuration; i++) whip[j] += mVert[r + i, minBound_Whip + j];
                    if (whip[j] > whipThreshold) total++;
                }
                double score = total / whipNormalise;
                if (score > 1.0) score = 1.0;
                //extend the whip score
                for (int i = 0; i < whipDuration; i++)
                {
                    if (whipScores[r+i] < score) whipScores[r + i] = score;
                }

            } //for all rows
            
            //combine scores and extract events
            var scores = new double[rows];
            for (int i = 0; i < whipScores.Length; i++)
                if ((whistleScores[i] > 0.3) && (whipScores[i] > 0.3))//impose score thresholds
                {
                    scores[i] = (whistleScores[i] + whipScores[i]) / 2;
                    //if (scores[i]>0.45) Console.WriteLine("{0}\t{1:f3}\t{2:f3}\t{3:f3}", i, whistleScores[i], whipScores[i], scores[i]);
                }
            //scores = whipScores;
            var tuple = Tuple.Create(scores);
            return tuple;
        }//end detect Whipbird



        public static Tuple<double[]> DetectCurlew(double[,] rising, double[,] falling,
                                         int minBound_Whistle, int maxBound_Whistle, int whistleDuration, int lineLength)
        {
            int rows = rising.GetLength(0);
            int cols = rising.GetLength(1);

            double cosRisingAngle  = Math.Cos(Math.PI * 200 / 180); //rising 20 degrees
            double sinRisingAngle  = Math.Sin(Math.PI * 200 / 180);
            double cosFallingAngle = Math.Cos(Math.PI * 340 / 180); //falling 20 degrees
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
                double score = whistle[DataTools.GetMaxIndex(whistle)] / (double)whistleDuration;
                if (score > 1.0) score = 1.0;
                risingScores[r] = score;

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
                score = whistle[DataTools.GetMaxIndex(whistle)] / (double)whistleDuration;
                if (score > 1.0) score = 1.0;
                fallingScores[r] = score;
            } //for all rows

            //combine scores
            var scores = new double[rows];
            for (int i = 0; i < risingScores.Length; i++)
                //if ((risingScores[i] > 0.2) && (fallingScores[i] > 0.2)) scores[i] = (risingScores[i] + fallingScores[i])/2;
                scores[i] = (risingScores[i]*0.9) + (fallingScores[i]*0.1);  // weighted average

            var tuple = Tuple.Create(scores);
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
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
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
                image.AddEvents(predictedEvents, sonogram.NyquistFrequency, sonogram.Configuration.FreqBinCount); 
                image.Save(path);
            }
        }//drawSonogram()



    }//class
}
