using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TowseyLib;
using AudioAnalysisTools;


namespace AnalysisPrograms
{
    class CreateFeltTemplate
    {
        //GECKO
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Gecko\Suburban_March2010\geckos_suburban_104.mp3"  C:\SensorNetworks\Output\FELT_Gecko\FELT_Gecko_Params.txt  FELT_Gecko1
        //CURRAWONG
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Currawongs\Currawong_JasonTagged\West_Knoll_Bees_20091102-183000.wav" C:\SensorNetworks\Output\FELT_CURRAWONG2\FELT_Currawong_Params.txt  FELT_Currawong2
        //CURLEW
        //createtemplate_felt "C:\SensorNetworks\WavFiles\Curlew\Curlew2\West_Knoll_-_St_Bees_20080929-210000.wav"              C:\SensorNetworks\Output\FELT_CURLEW\FELT_CURLEW2_Params.txt  FELT_Curlew2

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
            string targetPath  = outputDir + targetName + "_target.txt";
            string targetNoNoisePath  = outputDir + targetName + "_targetNoNoise.txt";
            string noisePath = outputDir + targetName + "_noise.txt";
            string binaryPath  = outputDir + targetName + "_binary.txt";
            string trinaryPath = outputDir + targetName + "_trinary.txt";
            string sprPath     = outputDir + targetName + "_spr.txt";
            string targetImagePath = outputDir + targetName + "_target.png";

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
            //NoiseReductionType nrt   = SNR.Key2NoiseReductionType(dict[SNR.key_Snr.key_NOISE_REDUCTION_TYPE]);
            //double dynamicRange      = Double.Parse(dict[SNR.key_Snr.key_DYNAMIC_RANGE]);
            //double dynamicRange      = 0.0;
            double eventStart        = Double.Parse(dict[key_EVENT_START]);
            double eventEnd          = Double.Parse(dict[key_EVENT_END]);            
            int minHz                = Int32.Parse(dict[key_MIN_HZ]);
            int maxHz                = Int32.Parse(dict[key_MAX_HZ]);
            double templateThreshold = 9.0; //threshold to set MIN DECIBEL BOUND

            //double smoothWindow    = Double.Parse(dict[key_SMOOTH_WINDOW]);       //duration of DCT in seconds 
            //double minDuration     = Double.Parse(dict[key_MIN_DURATION]);        //min duration of event in seconds 
            //double eventThreshold  = Double.Parse(dict[key_EVENT_THRESHOLD]);     //min score for an acceptable event
            int DRAW_SONOGRAMS = Int32.Parse(dict[key_DRAW_SONOGRAMS]);             //options to draw sonogram

            //iii: Extract the event
            //#############################################################################################################################################
            Log.WriteLine("# Start extracting target event.");
            var results = Execute_Extraction(recording, eventStart, eventEnd, minHz, maxHz, frameOverlap, templateThreshold);
            var sonogram = results.Item1;
            var extractedEvent = results.Item2;
            var target = results.Item3;            //event's matrix of target values before noise removal
            var noiseSubband = results.Item4;      //event's array  of noise  values
            var targetMinusNoise = results.Item5;  //event's matrix of target values after noise removal
            Log.WriteLine("# Finished extracting target event.");
            //#############################################################################################################################################

            //iv: SAVE extracted event as matrix of dB intensity values
            FileTools.WriteMatrix2File(target, targetPath);
            FileTools.WriteMatrix2File(targetMinusNoise, targetNoNoisePath);
            FileTools.WriteArray2File(noiseSubband, noisePath);

            // Save extracted event as a matrix of char symbols '+' and '-'
            char[,] symbolic = Target2BinarySymbols(targetMinusNoise, templateThreshold);
            FileTools.WriteMatrix2File(symbolic, binaryPath);
            symbolic = Target2TrinarySymbols(targetMinusNoise, templateThreshold);
            FileTools.WriteMatrix2File(symbolic, trinaryPath);

            // Save extracted event as a matrix of char symbols '+' and '-'
            char[,] spr = Target2SymbolicTracks(targetMinusNoise, templateThreshold);
            FileTools.WriteMatrix2File(spr, sprPath);


            //v: SAVE images of extracted event in the original sonogram 
            if (DRAW_SONOGRAMS > 0)
            {
                string sonogramImagePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath) + ".png";
                DrawSonogram(sonogram, sonogramImagePath, extractedEvent);

                //SAVE extracted event as noise reduced image 
                //alter matrix dynamic range so user can determine correct dynamic range from image 
                //matrix = SNR.SetDynamicRange(matrix, 0.0, dynamicRange); //set event's dynamic range
                var targetImage = BaseSonogram.Data2ImageData(targetMinusNoise);
                ImageTools.DrawMatrix(targetImage, 1, 1, targetImagePath);
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
       //     FindEventsLikeThis.Dev(arguments);

            Log.WriteLine("# Finished recording");
            Console.ReadLine();
        } //Dev()





        public static System.Tuple<BaseSonogram, AcousticEvent, double[,], double[], double[,]> Execute_Extraction(AudioRecording recording,
            double eventStart, double eventEnd, int minHz, int maxHz, double frameOverlap, double backgroundThreshold)
        {
            //ii: MAKE SONOGRAM
            SonogramConfig sonoConfig = new SonogramConfig(); //default values config
            sonoConfig.SourceFName = recording.FileName;
            //sonoConfig.WindowSize = windowSize;
            sonoConfig.WindowOverlap = frameOverlap;
            

            BaseSonogram sonogram = new SpectralSonogram(sonoConfig, recording.GetWavReader());
            recording.Dispose();
            Log.WriteLine("Frames: Size={0}, Count={1}, Duration={2:f1}ms, Overlap={5:f0}%, Offset={3:f1}ms, Frames/s={4:f1}",
                                       sonogram.Configuration.WindowSize, sonogram.FrameCount, (sonogram.FrameDuration * 1000),
                                      (sonogram.FrameOffset * 1000), sonogram.FramesPerSecond, frameOverlap);
            int binCount = (int)(maxHz / sonogram.FBinWidth) - (int)(minHz / sonogram.FBinWidth) + 1;
            Log.WriteIfVerbose("Freq band: {0} Hz - {1} Hz. (Freq bin count = {2})", minHz, maxHz, binCount);
            
            //calculate the modal noise profile
            double[] modalNoise = SNR.CalculateModalNoise(sonogram.Data); //calculate modal noise profile
            modalNoise = DataTools.filterMovingAverage(modalNoise, 7);    //smooth the noise profile
            //extract modal noise values of the required event
            double[] noiseSubband = BaseSonogram.ExtractModalNoiseSubband(modalNoise, minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);
            
            //extract data values of the required event
            double[,] target = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            // create acoustic event with defined boundaries
            AcousticEvent ae = new AcousticEvent(eventStart, eventEnd - eventStart, minHz, maxHz);
            ae.SetTimeAndFreqScales(sonogram.FramesPerSecond, sonogram.FBinWidth);

            //truncate noise
            sonogram.Data = SNR.TruncateModalNoise(sonogram.Data, modalNoise);
            sonogram.Data = SNR.RemoveBackgroundNoise(sonogram.Data, backgroundThreshold);

            double[,] targetMinusNoise = BaseSonogram.ExtractEvent(sonogram.Data, eventStart, eventEnd, sonogram.FrameOffset,
                                                         minHz, maxHz, false, sonogram.NyquistFrequency, sonogram.FBinWidth);

            return System.Tuple.Create(sonogram, ae, target, noiseSubband, targetMinusNoise);
        }//end Execute_Extraction()


        /// <summary>
        /// This method converts a matrix of doubles to binary values (+, -) and then to trinary matrix of (-,0,+) values.
        /// Purpose is to encircle the required shape with a halo of -1 values and set values outside the halo to zero.
        /// This helps to define an arbitrary shape despite enclosing it in a rectangular matrix.
        /// The algorithm starts from the four corners of matrix and works towards the centre.
        /// This approach yields less than perfect result and the final symbolic matrix should be edited manually.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static char[,] Target2TrinarySymbols(double[,] target, double threshold)
        {
            int rows = target.GetLength(0);
            int cols = target.GetLength(1);

            //A: convert target to binary using threshold
            int[,] binary = new int[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (target[i, j] > threshold) binary[i, j] =  1;
                    else                          binary[i, j] = -1;

            //B: convert numeric binary to symbolic binary
            char[,] symbolic = new char[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (target[i, j] > threshold) symbolic[i, j] = '+';
                    else symbolic[i, j] = '-';

            int halfRows = rows / 2;
            int halfCols = cols / 2;

            //C: convert symbolic binary to symbolic trinary. Add in '0' for 'do not care'.
            //work from the four corners - start top left
            for (int r = 1; r < halfRows + 1; r++)
                for (int c = 1; c < halfCols + 1; c++)
                {
                    int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r + 2, c + 2]);

                    if (sum == -10) { symbolic[r - 1, c - 1] = '0'; }
                }
            //bottom left
            for (int r = halfRows - 1; r < rows - 1; r++)
                for (int c = 1; c < halfCols + 1; c++)
                {
                    int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r - 2, c + 2]);

                    if (sum == -10) { symbolic[r + 1, c - 1] = '0'; }
                }
            //top right
            for (int r = 1; r < halfRows + 1; r++)
                for (int c = halfCols - 1; c < cols - 1; c++)
                {
                    int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r, c - 1] + binary[r, c] + binary[r, c + 1] + binary[r + 1, c - 1] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r + 2, c - 2]);

                    if (sum == -10) { symbolic[r - 1, c + 1] = '0'; }
                }
            //bottom right
            for (int r = halfRows - 1; r < rows - 1; r++)
                for (int c = halfCols - 1; c < cols - 1; c++)
                {
                    int sum = (int)(binary[r - 1, c - 1] + binary[r, c - 1] + binary[r + 1, c - 1] + binary[r - 1, c] + binary[r, c] + binary[r + 1, c] + binary[r + 1, c + 1] + binary[r, c + 1] + binary[r + 1, c + 1] + binary[r - 2, c - 2]);

                    if (sum == -10) { symbolic[r + 1, c + 1] = '0'; }
                }
            return symbolic;
        }





        public static char[,] Target2BinarySymbols(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            char[,] symbolic = new char[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    if (matrix[i, j] > threshold) symbolic[i, j] = '+';
                    else                          symbolic[i, j] = '-';

            return symbolic;
        }


        public static double[,] Target2SpectralTracks(double[,] matrix, double threshold)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[,] tracks = new double[rows, cols];

            for (int i = 1; i < rows-1; i++)
            {
                for (int j = 1; j < cols-1; j++)
                {
                    //if (matrix[i,j] < threshold) continue;
                    if (((matrix[i, j] > matrix[i, j+1]) && (matrix[i, j] > matrix[i, j-1])) ||
                        ((matrix[i, j] > matrix[i+1, j]) && (matrix[i, j] > matrix[i-1, j]))) 
                          tracks[i, j] = matrix[i, j];
                }
            }

            return tracks;
        }




        public static char[,] Target2SymbolicTracks(double[,] matrix, double threshold)
        {
            var m = ImageTools.WienerFilter(matrix, 7);
            m = Target2SpectralTracks(m, threshold);
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            //initialise symbolic matrix with spaces
            char[,] symbolic = new char[rows, cols];
            for (int r = 0; r < rows; r++) 
            {
                for (int c = 0; c < cols; c++) symbolic[r, c] = ' ';
            }

            int lineLength = 16;
            double sumThreshold = lineLength * threshold; //require average of threshold dB per pixel.
            char[] code = {'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q'};

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var result = ImageTools.DetectLine(m, r, c, lineLength, threshold);
                    if (result != null)
                    {
                        int degrees      = result.Item1;
                        double intensity = result.Item2;
                        if (intensity > sumThreshold) 
                        {
                            double cosAngle = Math.Cos(Math.PI * degrees / 180);
                            double sinAngle = Math.Sin(Math.PI * degrees / 180);
                            //symbolic[r, c] = code[degrees / 10];
                            for (int j = 0; j < lineLength; j++)
                            {
                                int row = r + (int)(cosAngle * j);
                                int col = c + (int)(sinAngle * j);
                                //if (symbolic[row, col] == ' ') 
                                    symbolic[row, col] = code[degrees / 10];
                            }//line length

                        }
                    }
                }//columns
            }//rows

            return symbolic;
        }




        /// <summary>
        /// Writes to file the trinary matrix derived from the the previous method.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="path"></param>
        //public static void WriteTargetMatrixAsTrinarySymbols(double[,] m, string path)
        //{
        //    int rows = m.GetLength(0);//height
        //    int cols = m.GetLength(1);//width
        //    //char[,] charM = new char[rows, cols]; 

        //    var lines = new List<string>();

        //    for (int i = 0; i < rows; i++)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        for (int j = 0; j < cols; j++)
        //        {
        //            if (m[i, j] == 1.0) sb.Append("+");
        //            else
        //                if (m[i, j] <= -1.0) sb.Append("-");//allow for weighted negatives
        //                else
        //                    if (m[i, j] == 0.0) sb.Append("0");
        //                    else sb.Append("#");
        //        }
        //        lines.Add(sb.ToString());
        //    }//end of all rows
        //    FileTools.WriteTextFile(path, lines);

        //}




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
