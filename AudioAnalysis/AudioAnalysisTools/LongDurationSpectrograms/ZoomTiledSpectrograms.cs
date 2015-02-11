using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

using Acoustics.Shared;
using AudioAnalysisTools.Indices;
using log4net;
using log4net.Repository.Hierarchy;

using TowseyLibrary;
using Acoustics.Shared.Csv;

//
// Action code for this analysis = ZoomingSpectrograms
namespace AudioAnalysisTools.LongDurationSpectrograms
{
    public static class ZoomTiledSpectrograms
    {

        public static void DrawSuperTiles(LdSpectrogramConfig longDurationSpectrogramConfig, FileInfo spectrogramTilingConfig, int tileWidth)
        {
            LdSpectrogramConfig config = longDurationSpectrogramConfig;

            //Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            //dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            TimeSpan dataScale = config.IndexCalculationDuration;

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            string fileStem = config.FileName;
            string[] keys = { "ACI", "AVG", "BGN", "CVR", "ENT", "EVN" };

            Dictionary<string, double[,]> spectra = ReadCSVFiles(config.InputDirectoryInfo, fileStem, keys);

            // standard scales in seconds per pixel.
            double[] imageScales = {0.2, 0.6, 1, 2, 6, 12, 24, 60 };

            Image image = null;

            // remove "Towsey.Acoustic" (i.e. 16 letters) from end of the file names
            // fileStem = "TEST_TUITCE_20091215_220004.Towsey.Acoustic" becomes "TEST_TUITCE_20091215_220004";
            int nameLength = fileStem.Length - 16;
            fileStem = fileStem.Substring(0, nameLength);


            // TOP MOST ZOOMED OUT IMAGE 
            double scale = 60; // seconds per pixel.
            TimeSpan imageScale = TimeSpan.FromSeconds(scale);
            TimeSpan focalTime = TimeSpan.Zero;
            int superTileWidth = tileWidth * 12;
            image = ZoomFocusedSpectrograms.DrawIndexSpectrogramAtScale(config, spectrogramTilingConfig, focalTime, dataScale, imageScale, superTileWidth, spectra);
            if (image != null)
            {
                string opName = String.Format("{0}_scale-{1:0}supertile-0.png", fileStem, scale);
                image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
            }


            scale = 24;
            imageScale = TimeSpan.FromSeconds(scale);
            image = ZoomFocusedSpectrograms.DrawIndexSpectrogramAtScale(config, spectrogramTilingConfig, focalTime, dataScale, imageScale, superTileWidth, spectra);
            if (image != null)
            {
                string opName = String.Format("{0}_scale-{1:0}supertile-0.png", fileStem, scale);
                image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
            }



            for (int i = 5; i >= 0; i--)
            {
                imageScale = TimeSpan.FromSeconds(imageScales[i]);
                image = ZoomFocusedSpectrograms.DrawIndexSpectrogramAtScale(config, spectrogramTilingConfig, focalTime, dataScale, imageScale, superTileWidth, spectra);
                if (image != null)
                {
                    string opName = String.Format("{0}_scale-{1:1}supertile-0.png", fileStem, scale);
                    image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
                }
            }




            // ####################### DERIVE ZOOMED IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES
            //double frameDurationInSeconds = config.FrameWidth / (double)config.SampleRate;
            double frameStepInSeconds     = config.FrameStep / (double)config.SampleRate;
            TimeSpan frameScale = TimeSpan.FromTicks((long)Math.Round(frameStepInSeconds * 10000000));
            //int[] compressionFactor = { 1, 2, 4, 11, 22 };
            int[] compressionFactor = { 1, 2, 5 };
            int maxCompression = compressionFactor[compressionFactor.Length - 1];
            TimeSpan maxImageDuration = TimeSpan.FromTicks(maxCompression * superTileWidth * frameScale.Ticks);

            TimeSpan halfMaxImageDuration = TimeSpan.FromMilliseconds(maxImageDuration.TotalMilliseconds / 2);
            TimeSpan startTimeOfMaxImage = focalTime - halfMaxImageDuration;
            TimeSpan startTimeOfData = TimeSpan.FromMinutes(Math.Floor(startTimeOfMaxImage.TotalMinutes));

            // get the data // #### TODO  #################################################################################
            //TimeSpan imageScale1 = TimeSpan.FromMilliseconds(80);
            //double[,] cvrMatrix = ExpandMatrixOfIndices(spectra["CVR"], focalTime, dataScale, imageScale1, imageWidth);
            //double coverThreshold = 0.8;


            //List<double[]> frameData = ReadFrameData(config, startTimeOfMaxImage, maxImageDuration, config.InputDirectoryInfo, fileStem);
            //// make the images
            //int factor = 5;
            //image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, superTileWidth, frameData);
            //if (image != null)
            //{
            //    string opName = String.Format("{0}_factor-{1:1}supertile-0.png", fileStem, factor);
            //    image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
            //}

            //factor = 2;
            //image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, superTileWidth, frameData);
            //if (image != null)
            //{
            //    string opName = String.Format("{0}_factor-{1:1}supertile-0.png", fileStem, factor);
            //    image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
            //}

            //factor = 1;
            //image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, superTileWidth, frameData);
            //if (image != null)
            //{
            //    string opName = String.Format("{0}_factor-{1:1}supertile-0.png", fileStem, factor);
            //    image.Save(Path.Combine(config.OutputDirectoryInfo.FullName, opName));
            //}

        }




        //public static Image DrawFrameSpectrogramAtScale(LdSpectrogramConfig config, FileInfo indicesConfigPath, TimeSpan startTimeOfData,
        //                            TimeSpan focalTime, TimeSpan frameScale, int compressionFactor, int imageWidth, List<double[]> frameData/*, double[,] indexData*/)
        //{
        //    if ((frameData == null) || (frameData.Count == 0))
        //    {
        //        LoggedConsole.WriteLine("WARNING: NO SPECTRAL SPECTROGRAM DATA SUPPLIED");
        //        return null;
        //    }

        //    //Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
        //    //dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

        //    TimeSpan sourceMinuteOffset = config.MinuteOffset;   // default = zero minute of day i.e. midnight
        //    TimeSpan imageScale = TimeSpan.FromTicks(frameScale.Ticks * compressionFactor);
        //    TimeSpan imageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
        //    TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
        //    TimeSpan startTime = focalTime - halfImageDuration - startTimeOfData;
        //    //if (startTime < TimeSpan.Zero)
        //    //{
        //    //    offsetTime = TimeSpan.Zero - startTime;
        //    //    startTime = TimeSpan.Zero;
        //    //}

        //    //TimeSpan endTime = focalTime + halfImageDuration;
        //    //if (endTime > dataDuration) endTime = dataDuration;
        //    //TimeSpan spectrogramDuration = endTime - startTime;
        //    //int spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

        //    int startIndex = (int)(startTime.Ticks / frameScale.Ticks);
        //    int requiredFrameCount = imageWidth * compressionFactor;
        //    List<double[]> frameSelection = frameData.GetRange(startIndex, requiredFrameCount);
        //    double[,] spectralSelection = MatrixTools.ConvertList2Matrix(frameSelection); 

        //    // compress spectrograms to correct scale
        //    if (compressionFactor > 1)
        //        spectralSelection = CompressFrameSpectrograms(spectralSelection, compressionFactor);

        //    spectralSelection = MatrixTools.MatrixRotate90Anticlockwise(spectralSelection);

        //    // SOME EXPERIMENTS IN NORMALISATION
        //    //double min; double max;
        //    //DataTools.MinMax(spectralSelection, out min, out max);
        //    //double range = max - min;
        //    // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
        //    //double fractionalStretching = 0.2;
        //    //min = min + (range * fractionalStretching);
        //    //max = max - (range * fractionalStretching);
        //    //range = max - min;

        //    // this is a normalisastion hack to darken the frame derived spectrograms
        //    double min = -100;
        //    double max = -40;
        //    spectralSelection = MatrixTools.boundMatrix(spectralSelection, min, max);
        //    spectralSelection = DataTools.normalise(spectralSelection);
        //    //var cch = new CubeHelix(ColourCubeHelix.DEFAULT);
        //    var cch = CubeHelix.GetCubeHelix();
        //    Image spectrogramImage = cch.DrawMatrixWithoutNormalisation(spectralSelection);

        //    Graphics g2 = Graphics.FromImage(spectrogramImage);


        //    // draw focus time
        //    Pen pen = new Pen(Color.Red);
        //    int x1 = (int)(halfImageDuration.Ticks / imageScale.Ticks);
        //    g2.DrawLine(pen, x1, 0, x1, spectrogramImage.Height);

        //    int nyquist = 22050 / 2;
        //    int herzInterval = 1000;
        //    string title = string.Format("ZOOM SCALE={0}ms/pixel   Image duration={1} ", //   (colour:R-G-B={2})
        //                                                               imageScale.TotalMilliseconds, imageDuration);
        //    Image titleBar = DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);
        //    TimeSpan startTimeOfImage = focalTime - halfImageDuration;
        //    spectrogramImage = FrameZoomSpectrogram(spectrogramImage, titleBar, startTimeOfImage, imageScale, config.XAxisTicInterval, nyquist, herzInterval);


        //    // MAY WANT THESE CLIPPING TRACKS AT SOME POINT
        //    // read high amplitude and clipping info into an image
        //    //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
        //    //string indicesFile = Path.Combine(config.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
        //    //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");
        //    //Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
        //    //if (null != imageX) imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));


        //    // create the base image
        //    Image image = new Bitmap(imageWidth, spectrogramImage.Height);
        //    Graphics g1 = Graphics.FromImage(image);
        //    g1.Clear(Color.DarkGray);

        //    //int Xoffset = (int)(startTime.Ticks / imageScale.Ticks);
        //    int Xoffset = (imageWidth / 2) - x1;
        //    g1.DrawImage(spectrogramImage, Xoffset, 0);

        //    return image;
        //}

        public static double[,] ExpandMatrixOfIndices(double[,] matrix, TimeSpan focalTime, TimeSpan dataScale, TimeSpan opScale, int opColumns)
        {
            double scalingFactor = Math.Round(dataScale.TotalMilliseconds / opScale.TotalMilliseconds);
            if(scalingFactor <= 1.0) return null;
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            var newMatrix = new double[rowCount, opColumns];
            //double[] tempArray = new double[scalingFactor];

            return newMatrix;
        }


        public static Dictionary<string, double[,]> ReadCSVFiles(DirectoryInfo ipdir, string fileName, string[] keys)
        {
            string warning = null;

            Dictionary<string, double[,]> spectrogramMatrices = new Dictionary<string, double[,]>();
            for (int i = 0; i < keys.Length; i++)
            {
                string path = Path.Combine(ipdir.FullName, fileName + "." + keys[i] + ".csv");
                if (File.Exists(path))
                {
                    int freqBinCount;
                    double[,] matrix = LDSpectrogramRGB.ReadSpectrogram(path, out freqBinCount);
                    matrix = MatrixTools.MatrixRotate90Anticlockwise(matrix);
                    spectrogramMatrices.Add(keys[i], matrix);
                    //this.FrameWidth = freqBinCount * 2;
                }
                else
                {
                    if (warning == null)
                    {
                        warning = "\nWARNING: from method LDSpectrogramRGB.ReadCSVFiles()";
                    }

                    warning += "\n      {0} File does not exist: {1}".Format2(keys[i], path);
                }
            }

            if (warning != null)
            {
                LoggedConsole.WriteLine(warning);
            }

            if (spectrogramMatrices.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: from method LDSpectrogramRGB.ReadCSVFiles()");
                LoggedConsole.WriteLine("         NO FILES were read from this directory: " + ipdir);
            }

            return spectrogramMatrices;
        }

        public static Dictionary<string, double[,]> CompressIndexSpectrograms(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan defaultTimeScale)
        {
            int scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / defaultTimeScale.TotalMilliseconds);
            var compressedSpectra = new Dictionary<string, double[,]>();
            int step = scalingFactor - 1;
            foreach (string key in spectra.Keys)
            {
                double[,] matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                int colCount = matrix.GetLength(1);
                int compressedLength = (colCount / scalingFactor);
                var newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[scalingFactor];

                if ((key == "ENT") && (scalingFactor > 1))
                {
                    //matrix = spectra["AVG"];
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c < colCount - scalingFactor; c += step)
                        {
                            colIndex = c / scalingFactor;
                            for (int i = 0; i < scalingFactor; i++) tempArray[i] = matrix[r, c + i];
                            double entropy = DataTools.Entropy_normalised(tempArray);
                            if (Double.IsNaN(entropy)) entropy = 1.0;
                            newMatrix[r, colIndex] = 1 - entropy;
                        }
                    }
                }
                else // average all other spectral indices
                {
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c < colCount - scalingFactor; c += step)
                        {
                            colIndex = c / scalingFactor;
                            for (int i = 0; i < scalingFactor; i++) tempArray[i] = matrix[r, c + i];
                            newMatrix[r, colIndex] = tempArray.Average();
                        }
                    }
                }
                compressedSpectra[key] = newMatrix;
            }
            return compressedSpectra;
        }


        /// <summary>
        /// This method assumes that the matrix spectrograms are oriented so that the rows = spectra
        /// and the columns = freq bins, i.e. rotated 90 degrees from normal orientation.
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="compressionFactor"></param>
        /// <returns></returns>
        public static double[,] CompressFrameSpectrograms(double[,] matrix, int compressionFactor)
        {
            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            int compressedLength = (rowCount / compressionFactor);
            var newMatrix = new double[compressedLength, colCount];
            double[] tempArray = new double[compressionFactor];
            int step = compressionFactor - 1;
            for (int c = 0; c < colCount; c++)
            {
                int rowIndex = 0;
                for (int r = 0; r < rowCount - compressionFactor; r += step)
                {
                    rowIndex = r / compressionFactor;
                    for (int i = 0; i < compressionFactor; i++) 
                        tempArray[i] = matrix[r + i, c];
                    newMatrix[rowIndex, c] = tempArray.Average();
                }
            }
            return newMatrix;
        }


        public static List<double[]> ReadFrameData(LdSpectrogramConfig config, TimeSpan starttime, TimeSpan maxDuration, DirectoryInfo dataDir, string fileStem)
        {
            TimeSpan endtime = starttime + maxDuration;
            int startMinute = (int)Math.Floor(starttime.TotalMinutes); 
            int endMinute   = (int)Math.Ceiling(endtime.TotalMinutes);

            string name = fileStem + "_" + startMinute + "min.csv";
            string csvPath = Path.Combine(dataDir.FullName, name);
            bool skipHeader = true;
            bool skipFirstColumn = true;

            List<double[]> frameData = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
            for (int i = startMinute+1; i <= endMinute; i++)
            {
                name = fileStem + "_" + i + "min.csv";
                csvPath = Path.Combine(dataDir.FullName, name);

                List<double[]> data = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
                frameData.AddRange(data);
            }
            return frameData;
        }




    }


    public class SpectrogramScalingConfig
    {
        public double IndexCalculationDuration { get; set; } //: 0.2,
        public double FrameDurationInSeconds { get; set; } //"FrameDurationInSeconds": 0.02,
        public int TileWidth { get; set; }                  //"TileWidth": 300,


        //"SCALE 1": {
        //  "SecondsPerPixel": 60.0,
        //  "FramesPerPixel": 2584,
        //},
        //"SCALE 2": {
        //  "SecondsPerPixel": 24.0,
        //  "FramesPerPixel": 1033.6,
        //},
        //"SCALE 3": {
        //  "SecondsPerPixel": 12.0,
        //  "FramesPerPixel": 516.8,
        //},
        //"SCALE 4": {
        //  "SecondsPerPixel": 6.0,
        //  "FramesPerPixel": 258.4,
        //},
        //"SCALE 5": {
        //  "SecondsPerPixel": 2.0,
        //  "FramesPerPixel": 86.13,
        //},
        //"SCALE 6": {
        //  "SecondsPerPixel": 1.0,
        //  "FramesPerPixel": 43.066,
        //},
        //"SCALE 7": {
        //  "SecondsPerPixel": 0.60,
        //  "FramesPerPixel": 25.84,
        //},
        //"SCALE 8": {
        //  "Minimum": 0.20,
        //  "Maximum": 8.61,
        //},
        //"SCALE 9": {
        //  "SecondsPerPixel": 0.10,
        //  "FramesPerPixel": 5.0,
        //},
        //"SCALE 10": {
        //  "SecondsPerPixel": 0.04,
        //  "FramesPerPixel": 2.0,
        //},
        //},
        //"SCALE 11": {
        //  "SecondsPerPixel": 0.02,
        //  "FramesPerPixel": 1.0,
        //},


    }
}
