namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.FSharp.Math;
    using AudioAnalysisTools;
    using QutSensors.AudioAnalysis.AED;
    using TowseyLib;

    public class SPT
    {
        public static void Dev(string[] args)
        {
            //spt C:\SensorNetworks\WavFiles\BridgeCreek\cabin_GoldenWhistler_file0127_extract1.mp3 C:\SensorNetworks\Output\SPT\ 2.0

            Log.Verbosity = 1;

            if (args.Length != 3)
            {
                Console.WriteLine("The arguments for SPT are: wavFile intensityThreshold");
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

            var result = doSPT(wavFilePath, intensityThreshold, smallLengthThreshold);
            var sonogram = result.Item1;
            sonogram.Data = result.Item2;

            // SAVE IMAGE
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

        /// <summary>
        /// Performs Spectral Peak Tracking on a recording
        /// Returns a sonogram.
        /// </summary>
        /// <param name="wavPath">the recording</param>
        /// <param name="intensityThreshold">Intensity threshold in decibels above backgorund</param>
        /// <param name="smallLengthThreshold">remove event swhose length is less than this threshold</param>
        /// <returns></returns>
        public static Tuple<BaseSonogram, double[,]> doSPT(string wavPath, double intensityThreshold, int smallLengthThreshold)
        {
            var sonogram = AED.FileToSonogram(wavPath);
            Log.WriteLine("intensityThreshold = " + intensityThreshold);

            // Sonograms in Matlab (which F# AED was modelled on) are orientated the opposite way
            var m = MatrixModule.transpose(MatrixModule.ofArray2D(sonogram.Data));

            Log.WriteLine("Wiener filter start");
            var w = Matlab.wiener2(5, m);
            Log.WriteLine("Wiener filter end");

            Log.WriteLine("Remove subband mode intensities start");
            var s = AcousticEventDetection.removeSubbandModeIntensities(w);
            Log.WriteLine("Remove subband mode intensities end");

            Log.WriteLine("SPT start");
            int nh = 3;
            var p = SpectralPeakTrack.spt(s, intensityThreshold, nh, smallLengthThreshold);
            Log.WriteLine("SPT finished");

            var r = MatrixModule.toArray2D(MatrixModule.transpose(p));
            return Tuple.Create(sonogram, r);
        }
    }
}
