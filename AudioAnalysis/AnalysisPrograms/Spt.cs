// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Spt.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the Spt type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    using AudioAnalysisTools;

    using TowseyLib;

    public class Spt
    {
        /// <summary>
        /// The method used to invoke the SPT filter from a console app.
        /// </summary>
        /// <param name="args">An array of arguments passed in from console app. </param>
        /// /// <exception cref="ArgumentException">This filter needs both arguments to work.</exception>
        public static void Dev(string[] args)
        {
            Log.Verbosity = 1;
            double intensityThreshold = 0;

            if (args.Length == 2)
            {
                intensityThreshold = Convert.ToDouble(args[1]);
            }
            else
            {
                Console.WriteLine("The arguments for SPT are: wavFile intensityThreshold");
                Console.WriteLine();
                Console.WriteLine("wavFile:            path to .wav recording.");
                Console.WriteLine(
                    "                    eg: \"trunk\\AudioAnalysis\\AED\\Test\\matlab\\BAC2_20071015-045040.wav\"");
                Console.WriteLine("intensityThreshold: is mandatory");
                Environment.Exit(1);
            }

            string wavFilePath = args[0];
            
            var result = Detect(wavFilePath, intensityThreshold);

            // TODO: do something with this?
            string savePath = System.Environment.CurrentDirectory + "\\" + Path.GetFileNameWithoutExtension(wavFilePath);
            string suffix = string.Empty;
            while (File.Exists(savePath + suffix + ".jpg"))
            {
                suffix = (suffix == string.Empty) ? "1" : (int.Parse(suffix) + 1).ToString();
            }

            Image im = result.GetImage(false, false);
            im.Save(savePath + suffix + ".jpg");

            Console.WriteLine("Image saved to: " + savePath);
        }

        public static BaseSonogram Detect(string wavPath, double intensityThreshold)
        {
            AudioRecording recording = new AudioRecording(wavPath);
            if (recording.SampleRate != 22050)
            {
                recording.ConvertSampleRate22kHz(); // TODO this will be common
            }
            SonogramConfig config = new SonogramConfig(); // default values config
            config.NoiseReductionType = ConfigKeys.NoiseReductionType.NONE;
            BaseSonogram sonogram = new SpectralSonogram(config, recording.GetWavReader());

            return Detect(sonogram, intensityThreshold);
        }

        /// <summary>
        /// This method run SPT over a given sonogram.
        /// </summary>
        /// <param name="sonogram">The sonogram to process.</param>
        /// <param name="intensityThreshold">The intensity threshold to use.</param>
        /// <returns>A filtered sonogram.</returns>
        /// <exception cref="ArgumentNullException"><c>sonogram</c> is null.</exception>
        public static BaseSonogram Detect(BaseSonogram sonogram, double intensityThreshold)
        {
            if (sonogram == null)
            {
                throw new ArgumentNullException("sonogram");
            }

            double[,] filtered = QutSensors.AudioAnalysis.AED.SpectralPeakTrack.spt(intensityThreshold, sonogram.Data);
            
            // overwrite old data, not sure if this will work
            sonogram.Data = filtered;
            return sonogram;
        }
    }
}
