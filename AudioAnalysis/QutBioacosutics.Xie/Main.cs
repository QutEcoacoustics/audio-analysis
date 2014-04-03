using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioAnalysisTools.Sonogram;
using System.IO;
using MathNet.Numerics;



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

        public static void Entry(dynamic configuration, FileInfo source)
        {
            //System.Threading.Thread.Sleep(2000);

            Log.Info("Enter into Jie's personal workspace");

            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */
            FileInfo path = ((string)configuration.file).ToFileInfo();

            if (source != null)
            {
                path = source;
            }

            //Log.Info(@path);

            //string outpath;
            //var fileName = path.Name;

            //outpath = Path.GetFileNameWithoutExtension(fileName);

            //string outPath = Path.Combine("c:\\jie\\output\\csv", outpath);
            //string outPath2 = Path.ChangeExtension(outPath, ".csv");
            
            //string imagePath = outPath2;
            string imagePath = configuration.image_path;
            double amplitudeThreshold = configuration.amplitude_threshold;
            int range = configuration.range;
            int distance = configuration.distance;
            double binToreance = configuration.binToreance;
            int frameThreshold = configuration.frameThreshold;
            int duraionThreshold = configuration.duraionThreshold;
            double trackThreshold = configuration.trackThreshold;
            int maximumDuration = configuration.maximumDuration;
            int minimumDuration = configuration.minimumDuration;
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

            //var fileEntries = Directory.GetFiles("C:\\Jie\\data\\Segment_JCU_01");
            ////fileEntries = fileEntries.OrderBy(f => f.FileName).ToList();
            //var fileCount = fileEntries.Count();
            //for (int fileIndex = 0; fileIndex < fileCount; fileIndex++)
            //{
            //    string path = fileEntries[fileIndex];
            //    var recording = new AudioRecording(path);
            //}

            // generate a spectrogram
            var recording = new AudioRecording(path.FullName);

            var spectrogramConfigLongTrack = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.5, WindowSize = 512 };
            var spectrogramLongTrack = new SpectralSonogram(spectrogramConfigLongTrack,recording.GetWavReader());

            //var matrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongTrack.Data);
            // calculate the windowed correlation in 2kHz to 2.5kHz
            //int cols = spectrogramLongTrack.Data.GetLength(0);
            //int rows = spectrogramLongTrack.Data.GetLength(1);

            //var rowLow = (int)(2000 / spectrogramLongTrack.FBinWidth);
            //var rowHigh = (int)(2500 / spectrogramLongTrack.FBinWidth);

            //var tempArray = new double[cols - 1];

            //for (int c = 0; c < (cols - 1); c++)
            //{
            //    double energyDiff = 0;
            //    for (int r = rowLow; r < rowHigh; r++)
            //    {
            //        //var tempDiff = matrix[r, c] * Math.Pow(matrix[r, c] - matrix[r, c + 1], 2);
            //        var tempDiff = Math.Pow(matrix[r, c], 2);
            //        energyDiff = energyDiff + tempDiff;
            //    }

            //    tempArray[c] = energyDiff;
            //}

            ////var result = DataTools.AutoCorrelation(tempArray,0,tempArray.Length);

            //DataTools.writeBarGraph(tempArray);

            



            double[,] spectrogramMatrix = DataTools.normalise(spectrogramLongTrack.Data);
            spectrogramMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramMatrix);

            int rows = spectrogramMatrix.GetLength(0);
            int cols = spectrogramMatrix.GetLength(1);

            Color[] grayScale = ImageTools.GrayScale();
            Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int greyId = (int)Math.Floor(spectrogramMatrix[r, c] * 255);
                    if (greyId < 0) greyId = 0;
                    else
                        if (greyId > 255) greyId = 255;

                    greyId = 255 - greyId;
                    bmp.SetPixel(c, r, grayScale[greyId]);
                }
            }

            bmp.Save(imagePath);

            var spectrogramConfig = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.9, WindowSize = 512 };
            var spectrogram = new SpectralSonogram(spectrogramConfig, recording.GetWavReader());
            //var image = ImageTools.DrawMatrix(spectrogramLongTrack.Data);
            //image.Save(imagePath);


            var spectrogramConfigOscillation = new SonogramConfig() { NoiseReductionType = NoiseReductionType.STANDARD, WindowOverlap = 0.1, WindowSize = 512 };
            var spectrogramOscillation = new SpectralSonogram(spectrogramConfigOscillation, recording.GetWavReader());
            // find the oscillation through all the recordings

            //spectrogramOscillation.Data = MatrixTools.MatrixRotate90Anticlockwise(spectrogramOscillation.Data);

            var Oscillation = new FindOscillation();
            var oscillationArray = Oscillation.getOscillation(spectrogramOscillation.Data, zeroBinIndex);


            //var image = ImageTools.DrawMatrix(oscillationArray);
            //image.Save(@"C:\Jie\output\a.png");


            var norOscArray = oscillationArray;
            
            //smooth the spectrogram for extracting long tracks
            var LongTrackSmoothMatrix = ImageTools.GaussianBlur_5cell(spectrogramLongTrack.Data);

            var peakLongMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var fingLongPeaks = new FindLocalPeaks();
            peakLongMatrix = fingLongPeaks.LocalLongPeaks(LongTrackSmoothMatrix, 3, 9, 8);

            //var image = ImageTools.DrawMatrix(peakLongMatrix);
            //image.Save(imagePath);

            // extract long tracks with wide band

            var trackLongMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var trackLongArray = new double[spectrogram.Data.GetLength(1)];
            var multipleLongTracks = new ExtractTracks();

            var resultsLong = multipleLongTracks.GetLongTracks(peakLongMatrix, 3, frameThreshold, duraionThreshold, trackThreshold, 40, 20);

            trackLongArray = resultsLong.Item1;

            // normalization
            var norLongTArray = new double[trackLongArray.Length];
            //var TSum = XieFunction.Sum(trackArray);
            for (int i = 0; i < trackLongArray.Length; i++)
            {
                norLongTArray[i] = trackLongArray[i] / spectrogram.Data.GetLength(1);
            }

            trackLongMatrix = resultsLong.Item2;

            //peakMatrix = MatrixTools.MatrixRotate90Anticlockwise(peakMatrix);
            //var image = ImageTools.DrawMatrix(trackLongMatrix);
            //image.Save(imagePath);

            var peakMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var localPeaks = new FindLocalPeaks();
            peakMatrix = localPeaks.LocalPeaks(spectrogram.Data, amplitudeThreshold, range, distance);

            //var image = ImageTools.DrawMatrix(peakMatrix);
            //image.Save(imagePath);

            var trackMatrix = new double[spectrogram.Data.GetLength(1), spectrogram.Data.GetLength(0)];
            var trackArray = new double[spectrogram.Data.GetLength(1)];
            var multipleTracks = new ExtractTracks();
            //trackMatrix = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold, maximumDuration, maximumDiffBin);
            
            var results = multipleTracks.GetTracks(peakMatrix, binToreance, frameThreshold, duraionThreshold, trackThreshold, maximumDuration, minimumDuration, maximumDiffBin);

            trackArray = results.Item1;

            // normalization
            var norTArray = new double[trackArray.Length];
            //var TSum = XieFunction.Sum(trackArray);
            for (int i = 0; i < trackArray.Length; i++)
            {
                norTArray[i] = trackArray[i] / spectrogram.Data.GetLength(0);
            }

            trackMatrix = results.Item2;
            //var image = ImageTools.DrawMatrix(trackMatrix);
            //image.Save(imagePath);

            // find the harmonic structure based on tracks
            var Harmonic = new FindHarmonics();
            var harmonicMatrix = Harmonic.getHarmonic(trackMatrix, colThreshold, zeroBinIndex);

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

            for (int i = 0; i < harmonicArray.Length; i++)
            {
                norHArray[i] = harmonicArray[i] / spectrogram.Data.GetLength(0);
            }

            //trackMatrix = MatrixTools.MatrixRotate90Anticlockwise(trackMatrix);
            //double[,] spectrogramMatrix = DataTools.normalise(spectrogramOscillation.Data);
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

            //double[,] spectrogramLongMatrix = DataTools.normalise(LongTrackSmoothMatrix);
            //spectrogramLongMatrix = MatrixTools.MatrixRotate90Anticlockwise(spectrogramLongMatrix);

            //int rows = spectrogramLongMatrix.GetLength(0);
            //int cols = spectrogramLongMatrix.GetLength(1);

            //Color[] grayScale = ImageTools.GrayScale();
            //Bitmap bmp = new Bitmap(cols, rows, PixelFormat.Format24bppRgb);
            ////bmp.Save(imagePath);

            // save the arrays (1-trackArray,2-oscillationArray,3-harmonicArray) to CSV file.
            //var NormaliseHarmonicArray = norHArray.Reverse();
            //var NormaliseOscillationArray = norOscArray.Reverse();
            //var NormaliseTrackArray = norTArray.Reverse();

            var FrogIndexList = new List<FrogIndex>();
            for (int i = (norHArray.Length - 1); i > 0; i--)
            {
                var FrogIndex = new FrogIndex();
                FrogIndex.Track = norTArray[i];
                FrogIndex.LongTrack = norLongTArray[i];
                FrogIndex.Oscillation = norOscArray[i];
                FrogIndex.Harmonic = norHArray[i];

                FrogIndexList.Add(FrogIndex);
            }

            //var FrogIndex = new List<List<string>>();

            //FrogIndex.Add(new List<string> { norTArray.ToString(), norOscArray.ToString(), norHArray.ToString() });

            FileInfo fileInfo = new FileInfo(imagePath);

            CsvTools.WriteResultsToCsv(fileInfo, FrogIndexList);

            Log.Info("Analysis complete");
            
           
        }

        
    }
}
