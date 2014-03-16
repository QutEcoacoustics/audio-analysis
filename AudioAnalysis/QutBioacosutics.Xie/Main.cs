using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalysisTools.Sonogram;


namespace QutBioacosutics.Xie
{
    using AudioAnalysisTools;

    using log4net;

    using TowseyLib;

    public static class Main
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Main));

        public static void Entry(dynamic configuration)
        {
            Log.Info("Enter into Jie's personal workspace");

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */
            string path = configuration.file;
            string imagePath = configuration.image_path;
            
            double amplitudeThreshold = configuration.amplitude_threshold;
            int range = configuration.range;
            int distance = configuration.distance;
            double binToreance = configuration.binToreance;
            int frameThreshold = configuration.frameThreshold;
            int duraionThreshold = configuration.duraionThreshold;
            double trackThreshold = configuration.trackThreshold;

            // bool noiseReduction = (int)configuration.do_noise_reduction == 1;

            //float noiseThreshold = configuration.noise_threshold;
            //Dictionary<string, string> complexSettings = configuration.complex_settings;
            //string[] simpleArray = configuration.array_example;
            //int[] simpleArray2 = configuration.array_example_2;
            //int? missingValue = configuration.i_dont_exist;
            //string doober = configuration.doobywacker;

            // the following will always throw an exception
            //int missingValue2 = configuration.i_also_dont_exist;

            // Execute analysis
            

            // generate a spectrogram
            var recording = new AudioRecording(path);

            var spectrogramConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = 512 };
            // if (!noiseReduction)
            // {
            //    spectrogramConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            // }

            var spectrogram = new SpectralSonogram(spectrogramConfig, recording.GetWavReader());


            var peakMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var localPeaks = new FindLocalPeaks();
            peakMatrix = localPeaks.LocalPeaks(spectrogram.Data, amplitudeThreshold, range, distance);
            // peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            // var image = ImageTools.DrawMatrix(peakMatrix);
            // image.Save(imagePath);

            var trackMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var multipleTracks = new ExtractTracks();
            trackMatrix = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold);

            peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            var image = ImageTools.DrawMatrix(peakMatrix);
            image.Save(imagePath);

            // find the harmonic structure & oscillation rate based on tracks

            //var image = spectrogram.GetImage();

            //image.Save(imagePath);

            Log.Info("Analysis complete");
           
        }


    }
}
