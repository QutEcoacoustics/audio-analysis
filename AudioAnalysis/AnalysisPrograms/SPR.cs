using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using TowseyLib;



namespace AnalysisPrograms
{   
    /// <summary>
    /// Implements a simple form of Syntactic Pattern Recognition to find defined bird calls in spectra.
    /// </summary>
    class SPR  //Syntactic Pattern Recognition
    {

        public static void Dev(string[] args)
        {
            //spr C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3 C:\SensorNetworks\Output\SPT\ 2.0

            Console.WriteLine("DATE AND TIME:" + DateTime.Now);
            Console.WriteLine("Syntactic Pattern Recognition\n");
            //StringBuilder sb = new StringBuilder("DATE AND TIME:" + DateTime.Now + "\n");
            //sb.Append("SCAN ALL RECORDINGS IN A DIRECTORY USING HTK-RECOGNISER\n");
            
            Log.Verbosity = 1;

            if (args.Length != 3)
            {
                Console.WriteLine("The arguments for SPR are: wavFile intensityThreshold");
                Console.WriteLine();
                Console.WriteLine("wavFile:            path to recording file.");
                Console.WriteLine("output dir:         where output files and images will be placed.");
                Console.WriteLine("intensityThreshold: is mandatory");
                Console.ReadLine();
                Environment.Exit(1);
            }

            string wavFilePath = args[0];
            string opDir = args[1];
            double intensityThreshold = Convert.ToDouble(args[2]);
            int smallLengthThreshold = 50;

            var result1 = SPT.doSPT(wavFilePath, intensityThreshold, smallLengthThreshold);

            var result2 = SPR.doSPR(result1.Item2, intensityThreshold+1.0, smallLengthThreshold);

            // SAVE IMAGE
            var sonogram = result1.Item1;
            sonogram.Data = result2.Item1;
            string savePath = opDir + Path.GetFileNameWithoutExtension(wavFilePath);
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
            Log.WriteLine("SPR Intensity Threshold = " + intensityThreshold);
            var mOut = MarkLine(matrix, lineLength, intensityThreshold, 0);
            mOut     = MarkLine(mOut,   lineLength, intensityThreshold, 2);

            // Sonograms in Matlab (which F# AED was modelled on) are orientated the opposite way
            Log.WriteLine("SPR start");
            int nh = 3;
            Log.WriteLine("SPR finished");
            return Tuple.Create(mOut);
        }

        public static double[,] MarkLine(double[,] m, int lineLength, double intensityThreshold, int direction)
        {
            int rows = m.GetLength(0);
            int cols = m.GetLength(1);
            var mOut = new double[rows, cols];
            double sumThreshold = lineLength * 0.75;

            switch (direction)
            {

                case 0:                 // find horizontal lines
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = lineLength; c < cols - lineLength; c++)
                        {
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r + l, c] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r + l, c] = 1.0;
                        }
                    }
                    break;

                case 1:                 // find diagonal lines
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = lineLength; c < cols - lineLength; c++)
                        {
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r, c + l] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r, c + l] = 1.0;
                        }
                    }
                    break;

                case 2:                 // find vertical lines
                    for (int r = lineLength; r < rows - lineLength; r++)
                    {
                        for (int c = lineLength; c < cols - lineLength; c++)
                        {
                            double sum = 0.0;
                            for (int l = 0; l < lineLength; l++) if (m[r, c + l] > intensityThreshold) sum++;
                            if (sum > sumThreshold) for (int l = 0; l < lineLength; l++) mOut[r, c + l] = 1.0;
                        }
                    }
                    break;

            } //switch
            return mOut;
        }// MarkLine()


    }
}
