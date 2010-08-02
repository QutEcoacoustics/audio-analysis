using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class ExtractEvent
    {

        //eventX "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  FELT_Gecko1


        //eventX "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.mp3" C:\SensorNetworks\Output\FELT_Currawong\FELT_Currawong_Params.txt  FELT_Currawong1
        //eventX "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav" C:\SensorNetworks\Output\FELT_CURLEW\FELT_CURLEW_Params.txt  FELT_Curlew1

        //ZIP THE OUTPUT FILES
        bool zipOutput = false;
        
        //Keys to recognise identifiers in PARAMETERS - INI file. 
        //public static string key_DO_SEGMENTATION = "DO_SEGMENTATION";
        public static string key_EVENT_START = "EVENT_START";
        public static string key_EVENT_END   = "EVENT_END";
        public static string key_MIN_HZ = "MIN_HZ";
        public static string key_MAX_HZ = "MAX_HZ";
        public static string key_FRAME_OVERLAP = "FRAME_OVERLAP";
        //public static string key_SMOOTH_WINDOW = "SMOOTH_WINDOW";
        //public static string key_MIN_DURATION = "MIN_DURATION";
        //public static string key_EVENT_THRESHOLD = "EVENT_THRESHOLD";
        public static string key_DRAW_SONOGRAMS = "DRAW_SONOGRAMS";

        public static string eventsFile = "events.txt";



        public static void Dev(string[] args)
        {
            string title = "# EXTRACT AND SAVE ACOUSTIC EVENT.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            Log.WriteLine(title);
            Log.WriteLine(date);

            Log.Verbosity = 1;
            Segment.CheckArguments(args);

            string recordingPath = args[0];
            string iniPath       = args[1];
            string targetName    = args[2]; //prefix of name of created files 

            string outputDir   = Path.GetDirectoryName(iniPath) + "\\";
            string opPath      = outputDir + targetName + "_info.txt";
            string matrixPath  = outputDir + targetName + "_target.txt";
            string binaryPath  = outputDir + targetName + "_binary.txt";
            string trinaryPath = outputDir + targetName + "_trinary.txt";
            string targetPath  = outputDir + targetName + "_target.png";

            Log.WriteIfVerbose("# Output folder =" + outputDir);

            //i: GET RECORDING
            AudioRecording recording = new AudioRecording(recordingPath);
            if (recording.SampleRate != 22050) recording.ConvertSampleRate22kHz();
            int sr = recording.SampleRate;

            //ii: READ PARAMETER VALUES FROM INI FILE
            var config = new Configuration(iniPath);
            Dictionary<string, string> dict = config.GetTable();
            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            double frameOverlap      = Double.Parse(dict[key_FRAME_OVERLAP]);
            NoiseReductionType nrt   = SNR.Key2NoiseReductionType(dict[SNR.key_Snr.key_NOISE_REDUCTION_TYPE]);
            double dynamicRange      = Double.Parse(dict[SNR.key_Snr.key_DYNAMIC_RANGE]);
            double eventStart        = Double.Parse(dict[key_EVENT_START]);
            double eventEnd          = Double.Parse(dict[key_EVENT_END]);            
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            double templateThreshold = 4.0; // dB threshold

            //double smoothWindow    = Double.Parse(dict[key_SMOOTH_WINDOW]);       //duration of DCT in seconds 
            //double minDuration     = Double.Parse(dict[key_MIN_DURATION]);        //min duration of event in seconds 
            //double eventThreshold  = Double.Parse(dict[key_EVENT_THRESHOLD]);     //min score for an acceptable event
            int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);             //options to draw sonogram

            //iii: Extract the event
            //#############################################################################################################################################
            Log.WriteLine("# Start extracting target event.");
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, nrt, dynamicRange);
            var sonogram = results.Item1;
            var extractedEvent = results.Item2;
            var matrix = results.Item3;          //event's matrix of data values
            Log.WriteLine("# Finished extracting target event.");
            //#############################################################################################################################################

            //iv: SAVE extracted event as matrix of dB intensity values
            FileTools.WriteMatrix2File(matrix, matrixPath);
            // Save extracted event as a matrix of char symbols '+' and '-'
            WriteTargetMatrixAsBinarySymbols(matrix, templateThreshold, binaryPath);
            var haloMMatrix = HaloTarget(matrix);                                     // ######################## CHECK THIS - NEEDS DEBUGGING
            WriteTargetMatrixAsTrinarySymbols(haloMMatrix, trinaryPath);              
            //matrix = FileTools.ReadDoubles2Matrix(matrixPath);

            //v: SAVE images of extracted event in the original sonogram 
            if (DRAW_SONOGRAMS > 0)
            {
                string imagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, imagePath, extractedEvent);

                //SAVE extracted event as noise reduced image 
                //alter matrix dynamic range so user can determine correct dynamic range from image 
                matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range
                var targetImage = BaseSonogram.Data2ImageData(matrix);
                ImageTools.DrawMatrix(targetImage, 1, 1, targetPath);
            }

            Log.WriteLine("#################################### TEST THE EXTRACTED EVENT ##################################");
            //vi: TEST THE EVENT ON ANOTHER FILE
            //felt  "C:\SensorNetworks\WavFiles\Canetoad\DM420010_128m_00s__130m_00s - Toads.mp3" C:\SensorNetworks\Output\FELT_CaneToad\FELT_CaneToad_Params.txt events.txt
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_18.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-170000.mp3";
            //string testRecording = @"C:\SensorNetworks\WavFiles\Curlew\Curlew2\Top_Knoll_-_St_Bees_20090517-210000.wav";
            string testRecording = recordingPath;
            string paramsPath    = iniPath;
            string[] arguments   = new string[3];
            arguments[0]         = testRecording;
            arguments[1]         = paramsPath;
            arguments[2]         = targetName;
            FindEventsLikeThis.Dev(arguments);

            Log.WriteLine("# Finished recording");
            Console.ReadLine();
        } //Dev()





        public static System.Tuple<BaseSonogram, AcousticEvent, double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, NoiseReductionType nrt, double dynamicRange)
        {
            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            sonoConfig.NoiseReductionType = nrt;

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            
            //subtract modal noise
            //double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            //modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            ////sonogram.Data = SNR.SubtractModalNoise(sonogram.Data, modalNoise);
            //sonogram.Data = SNR.RemoveModalNoise(sonogram.Data, modalNoise);
            
            //extract data values of the required event
            double[,] matrix = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, binCount, sonogram.FBinWidth);

            // create acoustic event with defined boundaries
            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd - eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);
            
            return System.Tuple.Create(sonogram, ae, matrix);
        }//end Execute_Extraction()


        /// <summary>
        /// This metnod converts a matrix of binary values (+1, -1) to trinary matrix of (-1,0,+1) values.
        /// Purpose is to encircle the required shape with a halo of -1 values and set values outside the halo to zero.
        /// This helps to define an arbitrary shape despite enclosing it in a rectangular matrix.
        /// The algorithm starts from the four corners of matrix and works towards the centre.
        /// This approach yields less than perfect result and target matrix should be manually edited.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double[,] HaloTarget(double[,] target)
        {
            int rows = target.GetLength(0);
            int cols = target.GetLength(1);
            int halfRows = rows / 2;
            int halfCols = cols / 2;
            var opm = new double[rows, cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++) opm[r, c] = target[r, c];
            //work from the four corners - start top left
            for (int r = 1; r < halfRows + 1; r++)
                for (int c = 1; c < halfCols + 1; c++)
                {
                    int sum = (int)(target[r - 1, c - 1] + target[r, c - 1] + target[r + 1, c - 1] + target[r, c - 1] + target[r, c] + target[r, c + 1] + target[r + 1, c - 1] + target[r + 1, c] + target[r + 1, c + 1]);

                    if (sum == -9) { opm[r - 1, c - 1] = 0; }
                }
            //bottom left
            for (int r = halfRows - 1; r < rows - 1; r++)
                for (int c = 1; c < halfCols + 1; c++)
                {
                    int sum = (int)(target[r - 1, c - 1] + target[r, c - 1] + target[r + 1, c - 1] + target[r, c - 1] + target[r, c] + target[r, c + 1] + target[r + 1, c - 1] + target[r + 1, c] + target[r + 1, c + 1]);

                    if (sum == -9) { opm[r + 1, c - 1] = 0; }
                }
            //top right
            for (int r = 1; r < halfRows + 1; r++)
                for (int c = halfCols - 1; c < cols - 1; c++)
                {
                    int sum = (int)(target[r - 1, c - 1] + target[r, c - 1] + target[r + 1, c - 1] + target[r, c - 1] + target[r, c] + target[r, c + 1] + target[r + 1, c - 1] + target[r + 1, c] + target[r + 1, c + 1]);

                    if (sum == -9) { opm[r - 1, c + 1] = 0; }
                }
            //bottom right
            for (int r = halfRows - 1; r < rows - 1; r++)
                for (int c = halfCols - 1; c < cols - 1; c++)
                {
                    int sum = (int)(target[r - 1, c - 1] + target[r, c - 1] + target[r + 1, c - 1] + target[r - 1, c] + target[r, c] + target[r + 1, c] + target[r + 1, c + 1] + target[r, c + 1] + target[r + 1, c + 1]);

                    if (sum == -9) { opm[r + 1, c + 1] = 0; }
                }

            //increase weight of negatives because there are now fewer of them.
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++) if (opm[r, c] == -1) opm[r, c] = -2;

            return opm;
        }





        public static void WriteTargetMatrixAsBinarySymbols(double[,] matrix, double threshold, string matrixPath)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            char[,] symbolic = new char[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (matrix[i, j] > threshold) symbolic[i, j] = '+';
                    else                          symbolic[i, j] = '-';

            FileTools.WriteMatrix2File(symbolic, matrixPath);
        }




        /// <summary>
        /// Writes to file the trinary matrix derived from the the previous method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="path"></param>
        public static void WriteTargetMatrixAsTrinarySymbols(double[,] m, string path)
        {
            int rows = m.GetLength(0);//height
            int cols = m.GetLength(1);//width
            //char[,] charM = new char[rows, cols]; 

            var lines = new List<string>();

            for (int i = 0; i < rows; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < cols; j++)
                {
                    if (m[i, j] == 1.0) sb.Append("+");
                    else
                        if (m[i, j] <= -1.0) sb.Append("-");//allow for weighted negatives
                        else
                            if (m[i, j] == 0.0) sb.Append("0");
                            else sb.Append("#");
                }
                lines.Add(sb.ToString());
            }//end of all rows
            FileTools.WriteTextFile(path, lines);

        }




        public static void DrawSonogram(BaseSonogram sonogram, string path, AcousticEvent ae)
        {
            Log.WriteLine("# Start to draw image of sonogram.");
            bool doHighlightSubband = false; bool add1kHzLines = true;
            //double maxScore = 50.0; //assumed max posisble oscillations per second

            using (System.Drawing.Image img = sonogram.GetImage(doHighlightSubband, add1kHzLines))
            using (Image_MultiTrack image = new Image_MultiTrack(img))
            {
                //img.Save(@"C:\SensorNetworks\WavFiles\temp1\testimage1.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                image.AddTrack(Image_Track.GetTimeTrack(sonogram.Duration, sonogram.FramesPerSecond));
                image.AddTrack(Image_Track.GetSegmentationTrack(sonogram));
                var aes = new List<AcousticEvent>();
                aes.Add(ae);
                image.AddEvents(aes);
                image.Save(path);
            }
        } //end DrawSonogram


    }//class
}
