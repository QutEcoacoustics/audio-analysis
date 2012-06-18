// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SPT.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the SPT type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Drawing;
    using System.IO;

    using AudioAnalysisTools;

    using Microsoft.FSharp.Math;

    using AnalysisPrograms.AED;

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
            Log.WriteLine("intensityThreshold = " + intensityThreshold);
            int smallLengthThreshold = 50;
            Log.WriteLine("smallLengthThreshold = " + smallLengthThreshold);

            var sonogram = AED.FileToSonogram(wavFilePath);

            var result = doSPT(sonogram, intensityThreshold, smallLengthThreshold);
            sonogram.Data = result.Item1;

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
        /// Returns a matrix derived from the sonogram
        /// </summary>
        /// <param name="sonogram">the sonogram</param>
        /// <param name="intensityThreshold">Intensity threshold in decibels above backgorund</param>
        /// <param name="smallLengthThreshold">remove event swhose length is less than this threshold</param>
        /// <returns></returns>
        public static Tuple<double[,]> doSPT(BaseSonogram sonogram, double intensityThreshold, int smallLengthThreshold)
        {
            // Sonograms in Matlab (which F# AED was modelled on) are orientated the opposite way
            var m = MatrixModule.transpose(MatrixModule.ofArray2D(sonogram.Data));

            Log.WriteLine("Wiener filter start");
            var w = Matlab.wiener2(7, m);
            Log.WriteLine("Wiener filter end");

            Log.WriteLine("Remove subband mode intensities start");
            var s = AcousticEventDetection.removeSubbandModeIntensities(w);
            Log.WriteLine("Remove subband mode intensities end");

            Log.WriteLine("SPT start");
            int nh = 3;
            var p = SpectralPeakTrack.spt(s, intensityThreshold, nh, smallLengthThreshold);
            Log.WriteLine("SPT finished");

            var r = MatrixModule.toArray2D(MatrixModule.transpose(p));
            return Tuple.Create(r);
        }


        /// <summary>
        /// Performs Spectral Peak Tracking on a recording
        /// Returns a matrix derived from the passed sonogram.Data()
        /// </summary>
        /// <param name="sonogram">the sonogram</param>
        /// <param name="intensityThreshold">Intensity threshold in decibels above backgorund</param>
        /// <param name="smallLengthThreshold">remove event swhose length is less than this threshold</param>
        /// <returns></returns>
        public static Tuple<double[,]> doSPT(double[,] matrix, double intensityThreshold, int smallLengthThreshold)
        {
            // Sonograms in Matlab (which F# AED was modelled on) are orientated the opposite way
            var m = MatrixModule.transpose(MatrixModule.ofArray2D(matrix));

           // Log.WriteLine("Wiener filter start");
           // var w = Matlab.wiener2(7, m);

           // Log.WriteLine("Remove subband mode intensities start");
           // var s = AcousticEventDetection.removeSubbandModeIntensities(w);

            Log.WriteLine("SPT start");
            int nh = 3;
            var p = SpectralPeakTrack.spt(m, intensityThreshold, nh, smallLengthThreshold);

            var r = MatrixModule.toArray2D(MatrixModule.transpose(p));
            return Tuple.Create(r);
        }

    }

}
