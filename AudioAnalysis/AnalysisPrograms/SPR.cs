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


            double intensityThreshold = Convert.ToDouble(dict[key_SPT_INTENSITY_THRESHOLD]);
            int smallLengthThreshold = Convert.ToInt32(dict[key_SPT_SMALL_LENGTH_THRESHOLD]);
            //SPT_SMALL_LENGTH_THRESHOLD

            var result1 = SPT.doSPT(recordingPath, intensityThreshold, smallLengthThreshold);

            var result2 = SPR.doSPR(result1.Item2, intensityThreshold+1.0, smallLengthThreshold);

            var events = DetectWhipBird(result2.Item1);

            // SAVE IMAGE
            var sonogram = result1.Item1;
            sonogram.Data = result2.Item1;
            string savePath = outputDir + Path.GetFileNameWithoutExtension(recordingPath);
            string suffix = string.Empty;
            while (File.Exists(savePath + suffix + ".jpg"))
            {
                suffix = (suffix == string.Empty) ? "1" : (int.Parse(suffix) + 1).ToString();
            }

            Image im = sonogram.GetImage(false, false);
            string newPath = savePath + suffix + ".jpg";
            Log.WriteIfVerbose("imagePath = " + newPath);
            im.Save(newPath);

            Console.WriteLine("\nFINISHED!");
            Console.ReadLine();
        }//end Main


        public static Tuple<double[,]> doSPR(double[,] matrix, double intensityThreshold, int lineLength)
        {
            Log.WriteLine("SPR start");
            Log.WriteLine("SPR Intensity Threshold = " + intensityThreshold);
            var mHori = MarkLine(matrix, lineLength, intensityThreshold, 0);
            //var mHori = MarkLine(matrix, lineLength, intensityThreshold, 180);
            var mVert = MarkLine(matrix, lineLength, intensityThreshold, 90);
            var mOut = DataTools.AddMatrices(mHori, mVert);
            Log.WriteLine("SPR finished");
            return Tuple.Create(mOut);
        }

        public static double[,] MarkLine(double[,] m, int lineLength, double intensityThreshold, int degrees)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var mOut = new double[rows, cols];
            double sumThreshold = lineLength * 0.75;

            switch (degrees)
            {

                case 0:                 // find horizontal lines
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = lineLength; c < cols - lineLength; c++)
                        {
                            if(m[r, c] < 0.00001) continue;
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r + l, c] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r + l, c] = 1.0;
                        }
                    }
                    break;

                case 90:                 // find vertical lines
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = lineLength; c < cols - lineLength; c++)
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

        public static List<AcousticEvent> DetectWhipBird(double[,] m)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);

            var predictedEvents = new List<AcousticEvent>();
            return predictedEvents;
        }//end detect Whipbird
    }
}
