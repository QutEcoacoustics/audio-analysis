using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalysisTools.Sonogram;
using System.IO;


namespace QutBioacosutics.Xie
{
    using AudioAnalysisTools;

    using log4net;

    using TowseyLib;

    using System.Drawing;

    using System.Drawing.Imaging;

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
            // string path = @"C:\Jie\data\160113_min140.wav";
            //FileInfo fileInfo = new FileInfo(path);
            //var fileName = fileInfo.Name;
            var fileName = Path.GetFileNameWithoutExtension(path);

            string imagePath = configuration.image_path;            
            double amplitudeThreshold = configuration.amplitude_threshold;
            int range = configuration.range;
            int distance = configuration.distance;
            double binToreance = configuration.binToreance;
            int frameThreshold = configuration.frameThreshold;
            int duraionThreshold = configuration.duraionThreshold;
            double trackThreshold = configuration.trackThreshold;
            int maximumDuration = configuration.maximumDuration;
            double maximumDiffBin = configuration.maximumDiffBin;

            int colThreshold = configuration.colThreshold;
            int zeroBinIndex = configuration.zeroBinIndex;
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

            //var fileEntries = Directory.GetFiles(path);
            //fileEntries = fileEntries.OrderBy(f=> f.FileName).ToList();
            //var fileCount = fileEntries.Count();
            //for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            //{

            //}

            // generate a spectrogram
            var recording = new AudioRecording(path);

            var spectrogramConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = 512 };
            // if (!noiseReduction)
            // {
            //    spectrogramConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            // }

            var spectrogram = new SpectralSonogram(spectrogramConfig, recording.GetWavReader());


            var spectrogramConfigOscillation = new SonogramConfig() { NoiseReductionType = NoiseReductionType.NONE, WindowOverlap = 0.1, WindowSize = 512 };

            var spectrogramOscillation = new SpectralSonogram(spectrogramConfigOscillation, recording.GetWavReader());
            // find the oscillation through all the recordings

            var Oscillation = new FindOscillation();
            var oscillationArray = Oscillation.getOscillation(spectrogramOscillation.Data, zeroBinIndex);

            //var image = ImageTools.DrawMatrix(oscillationArray);
            //image.Save(imagePath);

            // normalization
            var norOscArray = new double[oscillationArray.Length];
            var oscSum = XieFunction.Sum(oscillationArray);
            for (int i = 0; i < oscillationArray.Length; i++)
            {
                norOscArray[i] = oscillationArray[i] / oscSum;
            
            }


            var peakMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var localPeaks = new FindLocalPeaks();
            peakMatrix = localPeaks.LocalPeaks(spectrogram.Data, amplitudeThreshold, range, distance);
            // peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            // var image = ImageTools.DrawMatrix(peakMatrix);
            // image.Save(imagePath);

            var trackMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var trackArray = new double[spectrogram.Data.GetLength(1)];
            var multipleTracks = new ExtractTracks();
            //trackMatrix = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold, maximumDuration, maximumDiffBin);

            
            var results = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold, maximumDuration, maximumDiffBin);

            trackArray = results.Item1;

            // normalization
            var norTArray = new double[trackArray.Length];
            var TSum = XieFunction.Sum(trackArray);
            for (int i = 0; i < trackArray.Length; i++)
            {
                norTArray[i] = trackArray[i] / TSum;

            }


            trackMatrix = results.Item2;

            // find the harmonic structure based on tracks
            var Harmonic = new FindHarmonics();
            var harmonicMatrix = Harmonic.getHarmonic(trackMatrix, colThreshold, zeroBinIndex);

            //var image = ImageTools.DrawMatrix(harmonicMatrix);
            //image.Save(imagePath);

            // change harmonicMarix to array

            var harmonicArray = new double[harmonicMatrix.GetLength(0)];
            for (int i = 0; i < harmonicMatrix.GetLength(0); i++)
            {
                var temp = 0.0;
                for (int j = 0; j < harmonicMatrix.GetLength(1); j++)
                {
                    temp = temp + harmonicMatrix[i, j];                
                }
                harmonicArray[i] = temp;
            }

            // normalization

            var norHArray = new double[harmonicArray.Length];
            var HarSum = XieFunction.Sum(harmonicArray);
            for (int i = 0; i < harmonicArray.Length; i++)
            {
                norHArray[i] = harmonicArray[i] / HarSum;

            }

            //trackMatrix = MatrixTools.MatrixRotate90Anticlockwise(trackMatrix);
            //double[,] spectrogramMatrix = DataTools.normalise(spectrogram.Data);
            //spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramMatrix);

            //int rows = spectrogramMatrix.GetLength(0);
            //int cols = spectrogramMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            //for (int r = 0; r < rows; r++)
            //{
            //    for (int c = 0; c < cols; c++)
            //    {
            //        int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
            //        if (greyId < 0) greyId = 0;
            //        else
            //            if (greyId > 255) greyId = 255;

            //        greyId = 255 - greyId;
            //        bmp.SetPixel(c, r, grayScale[greyId]);
            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackMatrix[i, j] == 1)
            //        {
            //            bmp.SetPixel(j, i, Color.Blue);
            //        }

            //    }
            //}

            //for (int i = 0; i < rows; i++)
            //{
            //    for (int j = 0; j < cols; j++)
            //    {
            //        if (trackMatrix[i, j] == 2)
            //        {
            //            bmp.SetPixel(j, i, Color.Red);
            //        }

            //    }
            //}

            //bmp.Save(imagePath);

            // save the arrays (1-trackArray,2-oscillationArray,3-harmonicArray) to CSV file.

            var FrogIndex = new List<List<string>>();
            FrogIndex.Add(new List<string> {path,norTArray.ToString(), norOscArray.ToString(), norHArray.ToString()});

            FileInfo fileInfo = new FileInfo(imagePath);

            CsvTools.WriteResultsToCsv(fileInfo, FrogIndex);


            Log.Info("Analysis complete");
           
        }


    }
}
