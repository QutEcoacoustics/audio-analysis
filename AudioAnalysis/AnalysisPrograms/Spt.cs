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

    public class Spt
    {
        public static void Dev(string[] args)
        {
            Log.Verbosity = 1;

            if (args.Length != 2)
            {
                Console.WriteLine("The arguments for SPT are: wavFile intensityThreshold");
                Console.WriteLine();
                Console.WriteLine("wavFile:            path to .wav recording.");
                Console.WriteLine(
                    "                    eg: \"trunk\\AudioAnalysis\\AED\\Test\\matlab\\SPT\\Female1_HoneymoonBay_StBees_20081027-023000.wav\"");
                Console.WriteLine("intensityThreshold: is mandatory");
                Environment.Exit(1);
            }

            string wavFilePath = args[0];
            double intensityThreshold = Convert.ToDouble(args[1]);

            var result = doSPT(wavFilePath, intensityThreshold);
            var sonogram = result.Item1;

            // TODO Is this bad?
            sonogram.Data = result.Item2;

            // TODO: do something with this?
            string savePath = System.Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(wavFilePath);
            string suffix = string.Empty;
            while (File.Exists(savePath + suffix + ".jpg"))
            {
                suffix = (suffix == string.Empty) ? "1" : (int.Parse(suffix) + 1).ToString();
            }

            Image im = sonogram.GetImage(false, false);
            im.Save(savePath + suffix + ".jpg");
            Log.WriteIfVerbose("imagePath = " + savePath);
        }

        public static Tuple<BaseSonogram,double[,]> doSPT(string wavPath, double intensityThreshold)
        {
            var sonogram = AED.fileToSonogram(wavPath);
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
            var p = SpectralPeakTrack.spt(intensityThreshold, s);
            Log.WriteLine("SPT finished");

            var r = MatrixModule.toArray2D(MatrixModule.transpose(p));
            return Tuple.Create(sonogram, r);
        }
    }
}
