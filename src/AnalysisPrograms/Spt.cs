// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Spt.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the SPT type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using SixLabors.ImageSharp;
    using System.IO;
    using System.Threading.Tasks;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using Microsoft.FSharp.Math;
    using Production.Arguments;
    using Production.Validation;
    using Acoustics.AED;
    using TowseyLibrary;

    public class SPT
    {
        public const string CommandName = "SPT";

        [Command(
            CommandName,
            Description = "[UNMAINTAINED] Spectral Peak Tracking. Probably not useful anymore.")]
        public class Arguments : SubCommandBase
        {
            [Option(Description = "The source audio file to operate on")]
            [ExistingFile]
            [Required]
            [LegalFilePath]
            public string Source { get; set; }

            [Option(Description = "A directory to write output to")]
            [DirectoryExistsOrCreate(createIfNotExists: true)]
            [LegalFilePath]
            public string Output{ get; set; }

            [Option(Description = "Intensity Threshold")]
            [Required]
            public double IntensityThreshold { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                SPT.Execute(this);
                return this.Ok();
            }
        }

        public static void Execute(Arguments arguments)
        {
            Log.Verbosity = 1;

            FileInfo wavFilePath = arguments.Source.ToFileInfo();
            DirectoryInfo opDir = arguments.Output.ToDirectoryInfo();
            double intensityThreshold = arguments.IntensityThreshold;
            Log.WriteLine("intensityThreshold = " + intensityThreshold);
            int smallLengthThreshold = 50;
            Log.WriteLine("smallLengthThreshold = " + smallLengthThreshold);

            string wavFilePath1 = wavFilePath.FullName;
            var recording = new AudioRecording(wavFilePath1);
            const int sampleRate = 22050;
            if (recording.SampleRate != sampleRate)
            {
                throw new ArgumentException(
                    "Sample rate of recording ({0}) does not match the desired sample rate ({1})".Format2(
                        recording.SampleRate,
                        sampleRate),
                    "sampleRate");
            }

            var config = new SonogramConfig
                             {
                                 NoiseReductionType = NoiseReductionType.Standard,
                                 NoiseReductionParameter = 3.5,
                             };
            var sonogram = (BaseSonogram)new SpectrogramStandard(config, recording.WavReader);

            var result = doSPT(sonogram, intensityThreshold, smallLengthThreshold);
            sonogram.Data = result.Item1;

            // SAVE IMAGE
            string savePath = opDir + System.IO.Path.GetFileNameWithoutExtension(wavFilePath.Name);
            string suffix = string.Empty;
            while (File.Exists(savePath + suffix + ".jpg"))
            {
                suffix = suffix == string.Empty ? "1" : (int.Parse(suffix) + 1).ToString();
            }

            Image im = sonogram.GetImage(false, false, doMelScale: false);
            string newPath = savePath + suffix + ".jpg";
            Log.WriteIfVerbose("imagePath = " + newPath);
            im.Save(newPath);

            LoggedConsole.WriteLine("\nFINISHED!");
        }

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

            //Log.WriteLine("Wiener filter start");
            var w = Matlab.wiener2(7, m);

            //Log.WriteLine("Wiener filter end");

            //Log.WriteLine("Remove subband mode intensities start");
            var s = AcousticEventDetection.removeSubbandModeIntensities(w);

            //Log.WriteLine("Remove subband mode intensities end");

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
