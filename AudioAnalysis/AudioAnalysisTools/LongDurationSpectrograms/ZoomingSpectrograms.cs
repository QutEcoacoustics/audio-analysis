﻿using System;
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


namespace AudioAnalysisTools.LongDurationSpectrograms
{
    public static class ZoomingSpectrograms
    {


        public static void DrawSpectrogramsFromSpectralIndices(LdSpectrogramConfig longDurationSpectrogramConfig, FileInfo indicesConfigPath)
        {
            LdSpectrogramConfig config = longDurationSpectrogramConfig;

            //Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            //dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            // THREE BASIC PARAMETERS
            TimeSpan focalTime = TimeSpan.FromMinutes(16);
            int imageWidth = 1600;
            TimeSpan dataScale = config.IndexCalculationDuration;

            string fileStem = config.FileName;
            //string[] keys = config.ColourMap2.Split('-');
            string[] keys = {"ACI", "AVG", "ENT", "EVN"};

            Dictionary<string, double[,]> spectra = ReadCSVFiles(config.InputDirectoryInfo, fileStem, keys);

            // standard scales in seconds per pixel.
            double[] scales = {0.25, 0.5, 1, 2, 5, 10, 20, 60 };
            //double[] scales = { 0.25, 0.5, 1, 2, 4, 8, 16, 30, 60 };

            var imageList = new List<Image>();

            for (int i = 7; i >= 0; i--)
            {
                TimeSpan imageScale = TimeSpan.FromSeconds(scales[i]);
                //double scale = scales[i];
                //TimeSpan imageScale = TimeSpan.FromSeconds(dataScale.TotalSeconds * scale);
                Image image = DrawIndexSpectrogramAtScale(config, indicesConfigPath, focalTime, dataScale, imageScale, imageWidth, spectra);
                if (image != null) imageList.Add(image);
            }


            // derive spectrograms from standard spectral frames
            double frameDurationInSeconds = config.FrameWidth / (double)config.SampleRate;
            TimeSpan frameScale = TimeSpan.FromTicks((long)Math.Round(frameDurationInSeconds * 10000000));
            int[] compressionFactor = { 1, 2, 5, 11, 22 };
            int maxCompression = compressionFactor[compressionFactor.Length - 1];
            TimeSpan maxImageDuration = TimeSpan.FromTicks(maxCompression * imageWidth * frameScale.Ticks);
            fileStem =  "TEST_TUITCE_20091215_220004";

            TimeSpan halfMaxImageDuration = TimeSpan.FromMilliseconds(maxImageDuration.TotalMilliseconds / 2);
            TimeSpan startTimeOfMaxImage = focalTime - halfMaxImageDuration;
            TimeSpan startTimeOfData = TimeSpan.FromMinutes(Math.Floor(startTimeOfMaxImage.TotalMinutes)); 

            List<double[]> frameData = ReadFrameData(config, startTimeOfMaxImage, maxImageDuration, config.InputDirectoryInfo, fileStem);

            for (int i = 4; i >= 0; i--)
            {
                int factor = compressionFactor[i];
                Image image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, imageWidth, frameData);
                if (image != null) imageList.Add(image);
            }

            Image combinedImage = ImageTools.CombineImagesVertically(imageList);
            combinedImage.Save(Path.Combine(config.OutputDirectoryInfo.FullName, "ZOOM.png"));
        }




        public static Image DrawIndexSpectrogramAtScale(LdSpectrogramConfig config, FileInfo indicesConfigPath,
                                    TimeSpan focalTime, TimeSpan dataScale, TimeSpan imageScale, int imageWidth, Dictionary<string, double[,]> spectra)
        {
            if (spectra == null)
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            // check that scalingFactor >= 1.0
            double scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            if (scalingFactor < 1.0)
            {
                LoggedConsole.WriteLine("WARNING: Scaling Factor < 1.0");
                return null;
            }



            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            TimeSpan sourceMinuteOffset = config.MinuteOffset;

            // calculate data duration from column count of abitrary matrix
            var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            TimeSpan offsetTime = TimeSpan.Zero;
            TimeSpan ImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            TimeSpan startTime = focalTime - halfImageDuration;
            if (startTime < TimeSpan.Zero)
            {
                offsetTime = TimeSpan.Zero - startTime;
                startTime  = TimeSpan.Zero;
            }
            TimeSpan endTime = focalTime + halfImageDuration;
            if (endTime > dataDuration) endTime = dataDuration; 
            TimeSpan spectrogramDuration = endTime - startTime;
            int spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

            int startIndex = (int)(startTime.Ticks / dataScale.Ticks);
            int endIndex   = (int)(endTime.Ticks   / dataScale.Ticks);
            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount-1, endIndex-1);
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
                spectralSelection = CompressIndexSpectrograms(spectralSelection, imageScale, dataScale);

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = config.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
            string colorMap2 = config.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB
            string colorMap = colorMap2;

            double backgroundFilterCoeff = (double?)config.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            var cs1 = new LDSpectrogramRGB(config, colorMap);
            cs1.FileName = config.FileName;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

            // TODO: not sure if this works
            //Logger.Info("Spectra loaded from memory");
            cs1.LoadSpectrogramDictionary(spectralSelection);


            Image LDSpectrogram = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            //Image LDSpectrogram = new Bitmap(spectrogramWidth, 256);
            Graphics g2 = Graphics.FromImage(LDSpectrogram);




            // draw focus time
            Pen pen = new Pen(Color.Red);
            TimeSpan focalOffset = focalTime - startTime;
            int x1 = (int)(focalOffset.Ticks / imageScale.Ticks);
            g2.DrawLine(pen, x1, 0, x1, LDSpectrogram.Height);

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}s/pixel   Image duration={1} ",  //  (colour:R-G-B={2})
                                                                       imageScale.TotalSeconds, spectrogramDuration);
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, LDSpectrogram.Width);
            LDSpectrogram = FrameZoomSpectrogram(LDSpectrogram, titleBar, startTime, imageScale, config.XAxisTicInterval, nyquist, herzInterval);



            // read high amplitude and clipping info into an image
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
            //string indicesFile = Path.Combine(config.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");

            //Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
            //if (null != imageX)
            //    imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));

            // create the base image
            Image image = new Bitmap(imageWidth, LDSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            int Xoffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(LDSpectrogram, Xoffset, 0);

            return image;
        }



        public static Image DrawFrameSpectrogramAtScale(LdSpectrogramConfig config, FileInfo indicesConfigPath, TimeSpan startTimeOfData,
                                    TimeSpan focalTime, TimeSpan frameScale, int compressionFactor, int imageWidth, List<double[]> frameData)
        {
            if ((frameData == null) || (frameData.Count == 0))
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL SPECTROGRAM DATA SUPPLIED");
                return null;
            }

            //Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            //dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            TimeSpan sourceMinuteOffset = config.MinuteOffset;   // default = zero minute of day i.e. midnight

            // calculate data duration from column count of abitrary matrix
            //TimeSpan dataDuration = TimeSpan.FromTicks((long)(matrix.GetLength(0) * frameScale.Ticks));

            TimeSpan imageScale = TimeSpan.FromTicks(frameScale.Ticks * compressionFactor);

            TimeSpan imageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            TimeSpan startTime = focalTime - halfImageDuration - startTimeOfData;
            //if (startTime < TimeSpan.Zero)
            //{
            //    offsetTime = TimeSpan.Zero - startTime;
            //    startTime = TimeSpan.Zero;
            //}

            //TimeSpan endTime = focalTime + halfImageDuration;
            //if (endTime > dataDuration) endTime = dataDuration;
            //TimeSpan spectrogramDuration = endTime - startTime;
            //int spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

            int startIndex = (int)(startTime.Ticks / frameScale.Ticks);
            int requiredFrameCount = imageWidth * compressionFactor;
            List<double[]> frameSelection = frameData.GetRange(startIndex, requiredFrameCount);
            double[,] spectralSelection = MatrixTools.ConvertList2Matrix(frameSelection); 

            // compress spectrograms to correct scale
            if (compressionFactor > 1)
                spectralSelection = CompressFrameSpectrograms(spectralSelection, compressionFactor);

            spectralSelection = MatrixTools.MatrixRotate90Anticlockwise(spectralSelection);

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            //string colorMap1 = config.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
            string colorMap2 = config.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB
            string colorMap = colorMap2;

            //double backgroundFilterCoeff = (double?)config.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            //var cs1 = new LDSpectrogramRGB(config, colorMap);
            //cs1.FileName = config.FileName;
            //cs1.BackgroundFilter = backgroundFilterCoeff;
            //cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

            Image spectrogramImage = ImageTools.DrawReversedMatrix(spectralSelection);
            Graphics g2 = Graphics.FromImage(spectrogramImage);


            // draw focus time
            Pen pen = new Pen(Color.Red);
            //TimeSpan startTime = focalTime - halfImageDuration - startTimeOfData;
            //TimeSpan focalOffset = focalTime - startTime;
            int x1 = (int)(halfImageDuration.Ticks / imageScale.Ticks);
            g2.DrawLine(pen, x1, 0, x1, spectrogramImage.Height);

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}ms/pixel   Image duration={1} ", //   (colour:R-G-B={2})
                                                                       imageScale.TotalMilliseconds, imageDuration);
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);
            TimeSpan startTimeOfImage = focalTime - halfImageDuration;
            spectrogramImage = FrameZoomSpectrogram(spectrogramImage, titleBar, startTimeOfImage, imageScale, config.XAxisTicInterval, nyquist, herzInterval);



            // read high amplitude and clipping info into an image
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
            //string indicesFile = Path.Combine(config.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");

            //Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
            //if (null != imageX)
            //    imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));

            // create the base image
            Image image = new Bitmap(imageWidth, spectrogramImage.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            //int Xoffset = (int)(startTime.Ticks / imageScale.Ticks);
            int Xoffset = (imageWidth / 2) - x1;
            g1.DrawImage(spectrogramImage, Xoffset, 0);

            return image;
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



        public static Dictionary<string, double[,]> CompressFrameSpectrogram(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan defaultTimeScale)
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

                // take the MAX
                for (int r = 0; r < rowCount; r++)
                {
                    int colIndex = 0;
                    for (int c = 0; c < colCount - scalingFactor; c += step)
                    {
                        colIndex = c / scalingFactor;
                        for (int i = 0; i < scalingFactor; i++) tempArray[i] = matrix[r, c + i];
                        newMatrix[r, colIndex] = tempArray.Max();
                    }
                }
                compressedSpectra[key] = newMatrix;
            }
            return compressedSpectra;
        }

        public static Image DrawTitleBarOfZoomSpectrogram(string title, int width)
        {
            //Image colourChart = LDSpectrogramRGB.DrawColourScale(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 2);

            Bitmap bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Pen pen = new Pen(Color.White);
            Font stringFont = new Font("Arial", 9);
            //Font stringFont = new Font("Tahoma", 9);
            SizeF stringSize = new SizeF();

            int X = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(X, 3));

            stringSize = g.MeasureString(title, stringFont);
            X += (stringSize.ToSize().Width + 70);
            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));

            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1);
            //g.DrawImage(colourChart, X, 1);

            string text = string.Format("(c) QUT.EDU.AU");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.White), 0, 0, width, 0);//draw upper boundary
            g.DrawLine(new Pen(Color.White), 0, 1, width, 1);//draw upper boundary
            return bmp;
        }

        public static Image FrameZoomSpectrogram(Image bmp1, Image titleBar, TimeSpan startOffset, TimeSpan xAxisPixelDuration, TimeSpan xAxisTicInterval, int nyquist, int herzInterval)
        {
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            AudioAnalysisTools.StandardSpectrograms.SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, startOffset, fullDuration, xAxisTicInterval, nyquist, herzInterval);

            int trackHeight = 20;
            int imageHt = bmp1.Height + trackHeight + trackHeight;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, startOffset, bmp1.Width, trackHeight);

            Bitmap compositeBmp = new Bitmap(bmp1.Width, imageHt); //get canvas for entire image
            Graphics gr = Graphics.FromImage(compositeBmp);
            gr.Clear(Color.Black);
            int offset = 0;
            gr.DrawImage(titleBar, 0, offset); //draw in the top time scale
            offset += titleBar.Height;
            gr.DrawImage(bmp1, 0, offset); //draw
            offset += bmp1.Height;
            gr.DrawImage(timeBmp, 0, offset); //draw
            return compositeBmp;
        }



    }
}
