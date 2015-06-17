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

/*
    Action code for this analysis = ZoomingSpectrograms
    Activity Codes for other tasks to do with spectrograms and audio files:

    audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
    audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
    indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
    colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
    zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
    differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms

    audiofilecheck - Writes information about audio files to a csv file.
    snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
    audiocutter - Cuts audio into segments of desired length and format
    createfoursonograms 
*/
namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System.Diagnostics;

    public static class ZoomFocusedSpectrograms
    {

        public static void DrawStackOfZoomedSpectrograms(DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, 
                                                         ZoomCommonArguments common,
                                                         TimeSpan focalTime, int imageWidth)
        {
            var zoomConfig = common.SuperTilingConfig;
            LdSpectrogramConfig ldsConfig = common.SuperTilingConfig.LdSpectrogramConfig;
            var distributions = IndexDistributions.Deserialize(common.IndexDistributionsFile);
            var indexGeneration = Json.Deserialise<IndexGenerationData>(common.IndexGenerationDataFile);
            var indexProperties = common.IndexProperties;

            string fileStem     = common.OriginalBasename;
            string analysisType = "Towsey.Acoustic";
            TimeSpan dataScale  = indexGeneration.IndexCalculationDuration;

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            var sw = Stopwatch.StartNew();
            string[] keys = { "ACI", "POW", "BGN", "CVR", "DIF", "ENT", "EVN", "SUM", "SPT" };
            Dictionary<string, double[,]> spectra = ZoomFocusedSpectrograms.ReadCSVFiles(inputDirectory, fileStem + "_" + analysisType, keys);
            sw.Stop();
            LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds + " seconds");


            // standard scales in seconds per pixel.
            double[] imageScales = {0.2, 0.6, 1, 2, 6, 12, 24, 60 };

            var imageList = new List<Image>();
            Image image = null;
            for (int i = 7; i >= 0; i--)
            {
                TimeSpan imageScale = TimeSpan.FromSeconds(imageScales[i]);
                image = DrawIndexSpectrogramAtScale(ldsConfig, indexProperties, focalTime, dataScale, imageScale, imageWidth, spectra, indexGeneration, fileStem);
                if (image != null) imageList.Add(image);
            }

            // ####################### DERIVE ZOOMED IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES
            double frameStepInSeconds     = indexGeneration.FrameStep / (double)indexGeneration.SampleRateResampled;
            TimeSpan frameScale = TimeSpan.FromTicks((long)Math.Round(frameStepInSeconds * 10000000));
            int[] compressionFactor = { 1, 2, 5 };
            int maxCompression = compressionFactor[compressionFactor.Length - 1];
            TimeSpan maxImageDuration = TimeSpan.FromTicks(maxCompression * imageWidth * frameScale.Ticks);

            TimeSpan halfMaxImageDuration = TimeSpan.FromMilliseconds(maxImageDuration.TotalMilliseconds / 2);
            TimeSpan startTimeOfMaxImage = TimeSpan.Zero;
            if (focalTime != TimeSpan.Zero) 
                startTimeOfMaxImage = focalTime - halfMaxImageDuration;
            TimeSpan startTimeOfData = TimeSpan.FromMinutes(Math.Floor(startTimeOfMaxImage.TotalMinutes));

            List<double[]> frameData = ReadFrameData(inputDirectory, fileStem, startTimeOfMaxImage, maxImageDuration, zoomConfig);

            // get the index data to add into the  
            TimeSpan imageScale1 = TimeSpan.FromSeconds(0.1);
            double[,] indexData = spectra["POW"];
            
            // make the images
            for (int i = 2; i >= 0; i--)
            {
                int factor = compressionFactor[i];
                image = ZoomFocusedSpectrograms.DrawFrameSpectrogramAtScale(ldsConfig, startTimeOfData, frameScale, factor, frameData, indexData, focalTime, imageWidth, indexGeneration.MinuteOffset);
                if (image != null) imageList.Add(image);
            }

            // combine the images into a stack  
            Image combinedImage = ImageTools.CombineImagesVertically(imageList);
            string fileName = String.Format("{0}_FocalZOOM_min{1:f1}.png", fileStem, focalTime.TotalMinutes);
            combinedImage.Save(Path.Combine(outputDirectory.FullName, fileName));
        }




        public static Image DrawIndexSpectrogramAtScale(
            LdSpectrogramConfig spectrogramDrawingConfig, Dictionary<string, IndexProperties> indexProperties, TimeSpan focalTime, 
            TimeSpan dataScale, TimeSpan imageScale, int imageWidth, Dictionary<string, double[,]> spectra, 
            IndexGenerationData indexGeneration, string basename)
        {
            if (spectra == null)
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            // check that scalingFactor >= 1.0
            double scalingFactor = Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
            if (scalingFactor < 1.0)
            {
                LoggedConsole.WriteLine("WARNING: Scaling Factor < 1.0");
                return null;
            }


            Dictionary<string, IndexProperties> dictIP = indexProperties;
            dictIP = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIP);

            TimeSpan sourceMinuteOffset = indexGeneration.MinuteOffset;

            // calculate data duration from column count of abitrary matrix
            var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            TimeSpan offsetTime = TimeSpan.Zero;
            TimeSpan ImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            TimeSpan startTime = TimeSpan.Zero;
            if (focalTime != TimeSpan.Zero) startTime = focalTime - halfImageDuration;
            if (startTime < TimeSpan.Zero)
            {
                offsetTime = TimeSpan.Zero - startTime;
                startTime  = TimeSpan.Zero;
            }
            TimeSpan endTime = ImageDuration;
            if (focalTime != TimeSpan.Zero) endTime = focalTime + halfImageDuration;
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

            // These parameters define the colour maps and appearance of the false-colour spectrogram
            // TODO: change these to the values from spectrogramDrawingConfig
            string colorMap1 = "ACI-ENT-EVN";
            string colorMap2 = "BGN-POW-CVR";

            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            var cs1 = new LDSpectrogramRGB(spectrogramDrawingConfig, indexGeneration, colorMap1);
            cs1.FileName = basename;
            cs1.BackgroundFilter = indexGeneration.BackgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

            // TODO: not sure if this works
            //Logger.Info("Spectra loaded from memory");
            cs1.LoadSpectrogramDictionary(spectralSelection);


            int imageScaleInMsPerPixel = (int)imageScale.TotalMilliseconds;
            double blendWt1 = 0.1;
            double blendWt2 = 0.9;

            if (imageScaleInMsPerPixel > 15000)
            {
                blendWt1 = 1.0;
                blendWt2 = 0.0;
            }
            else
            if (imageScaleInMsPerPixel > 5000)
            {
                blendWt1 = 0.9;
                blendWt2 = 0.1;
            }
            else
            if (imageScaleInMsPerPixel > 1000)
            {
                blendWt1 = 0.7;
                blendWt2 = 0.3;
            } else
            if (imageScaleInMsPerPixel > 500)
            {
                blendWt1 = 0.3;
                blendWt2 = 0.7;
            } 

            Image LDSpectrogram = cs1.DrawBlendedFalseColourSpectrogram("NEGATIVE", colorMap1, colorMap2, blendWt1, blendWt2);
            Graphics g2 = Graphics.FromImage(LDSpectrogram);

            // draw focus time on image
            if (focalTime != TimeSpan.Zero)
            {
                Pen pen = new Pen(Color.Red);
                TimeSpan focalOffset = focalTime - startTime;
                int x1 = (int)(focalOffset.Ticks / imageScale.Ticks);
                g2.DrawLine(pen, x1, 0, x1, LDSpectrogram.Height);
            }

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}s/pixel   Image duration={1} ",  //  (colour:R-G-B={2})
                                                                       imageScale.TotalSeconds, spectrogramDuration);
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, LDSpectrogram.Width);
            LDSpectrogram = FrameZoomSpectrogram(LDSpectrogram, titleBar, startTime, imageScale, spectrogramDrawingConfig.XAxisTicInterval, nyquist, herzInterval);

            // create the base image
            Image image = new Bitmap(imageWidth, LDSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            int Xoffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(LDSpectrogram, Xoffset, 0);

            return image;
        }




        public static Image DrawFrameSpectrogramAtScale(LdSpectrogramConfig config, TimeSpan startTimeOfData, TimeSpan frameScale, int compressionFactor, List<double[]> frameData, double[,] indexData, TimeSpan focalTime, int imageWidth, TimeSpan minuteOffset /*FileInfo SpectrogramTilingConfig,*/)
        {
            if ((frameData == null) || (frameData.Count == 0))
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL SPECTROGRAM DATA SUPPLIED");
                return null;
            }

            TimeSpan sourceMinuteOffset = minuteOffset;   // default = zero minute of day i.e. midnight
            TimeSpan imageScale = TimeSpan.FromTicks(frameScale.Ticks * compressionFactor);
            TimeSpan imageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            TimeSpan startTime = focalTime - halfImageDuration;
            if (startTime < TimeSpan.Zero)
            {
                startTime = TimeSpan.Zero;
            }

            int startIndex = (int)((startTime.Ticks - startTimeOfData.Ticks) / frameScale.Ticks);
            int requiredFrameCount = imageWidth * compressionFactor;
            List<double[]> frameSelection = frameData.GetRange(startIndex, requiredFrameCount);
            double[,] spectralSelection = MatrixTools.ConvertList2Matrix(frameSelection); 

            // compress spectrograms to correct scale
            if (compressionFactor > 1)
                spectralSelection = TemporalMatrix.CompressFrameSpectrograms(spectralSelection, compressionFactor);

            spectralSelection = MatrixTools.MatrixRotate90Anticlockwise(spectralSelection);

            // SOME EXPERIMENTS IN NORMALISATION
            //double min; double max;
            //DataTools.MinMax(spectralSelection, out min, out max);
            //double range = max - min;
            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            //double fractionalStretching = 0.2;
            //min = min + (range * fractionalStretching);
            //max = max - (range * fractionalStretching);
            //range = max - min;

            // this is a normalisastion hack to darken the frame derived spectrograms
            double min = -100;
            double max = -30;
            spectralSelection = MatrixTools.boundMatrix(spectralSelection, min, max);
            spectralSelection = DataTools.normalise(spectralSelection);
            spectralSelection = MatrixTools.SquareRootOfValues(spectralSelection);
            var cch = CubeHelix.GetCubeHelix();
            Image spectrogramImage = cch.DrawMatrixWithoutNormalisation(spectralSelection);

            Graphics g2 = Graphics.FromImage(spectrogramImage);

            int x1 = (int)(halfImageDuration.Ticks / imageScale.Ticks);

            // draw focus time on image
            if (focalTime != TimeSpan.Zero)
            {
                Pen pen = new Pen(Color.Red);
                g2.DrawLine(pen, x1, 0, x1, spectrogramImage.Height);
            }

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}ms/pixel   Image duration={1} ", //   (colour:R-G-B={2})
                                          imageScale.TotalMilliseconds, imageDuration);
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);
            spectrogramImage = FrameZoomSpectrogram(spectrogramImage, titleBar, startTime, imageScale, config.XAxisTicInterval, nyquist, herzInterval);


            // MAY WANT THESE CLIPPING TRACKS AT SOME POINT
            // read high amplitude and clipping info into an image
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
            //string indicesFile = Path.Combine(config.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");
            //Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
            //if (null != imageX) imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));


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
                DateTime now1 = DateTime.Now;

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

                DateTime now2 = DateTime.Now;
                TimeSpan et = now2 - now1;
                LoggedConsole.WriteLine("Time to read spectral index file <" + keys[i] + "> = " + et.TotalSeconds + " seconds");
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

        /// <summary>
        /// compresses the spectral index data in the temporal direction by a factor dervied from the data scale and required image scale.
        /// In all cases, the compression is done by taking the average
        /// </summary>
        /// <param name="spectra"></param>
        /// <param name="imageScale"></param>
        /// <param name="dataScale"></param>
        /// <returns></returns>
        public static Dictionary<string, double[,]> CompressIndexSpectrograms(Dictionary<string, double[,]> spectra, TimeSpan imageScale, TimeSpan dataScale)
        {
            int scalingFactor = (int)Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);
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

                // the ENTROPY matrix requires separate calculation
                if ((key == "ENT") && (scalingFactor > 1))
                {
                    matrix = spectra["SUM"];
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c <= colCount - scalingFactor; c += step)
                        {
                            colIndex = c / scalingFactor;
                            for (int i = 0; i < scalingFactor; i++)
                            {
                                // square the amplitude to give energy
                                tempArray[i] = matrix[r, c + i] * matrix[r, c + i];
                            }
                            double entropy = DataTools.Entropy_normalised(tempArray);
                            if (Double.IsNaN(entropy)) entropy = 1.0;
                            newMatrix[r, colIndex] = 1 - entropy;
                        }
                    }
                }
                else 
                // THE ACI matrix requires separate calculation
                if ((key == "ACI") && (scalingFactor > 1))
                {
                    double[] DIFArray = new double[scalingFactor];
                    double[] SUMArray = new double[scalingFactor];
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c <= colCount - scalingFactor; c += step)
                        {
                            colIndex = c / scalingFactor;
                            for (int i = 0; i < scalingFactor; i++)
                            {
                                DIFArray[i] = spectra["DIF"][r, c + i];
                                SUMArray[i] = spectra["SUM"][r, c + i];
                            }
                            newMatrix[r, colIndex] = DIFArray.Sum() / SUMArray.Sum();
                        }
                    }
                }
                else // average all other spectral indices
                {
                    matrix = spectra[key];
                    for (int r = 0; r < rowCount; r++)
                    {
                        int colIndex = 0;
                        for (int c = 0; c <= colCount - scalingFactor; c += step)
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


        public static List<double[]> ReadFrameData(DirectoryInfo dataDir, string fileStem, TimeSpan starttime, TimeSpan maxDuration, SuperTilingConfig tilingConfig)
        {
            TimeSpan endtime = starttime + maxDuration;
            int startMinute = (int)Math.Floor(starttime.TotalMinutes); 
            int endMinute   = (int)Math.Ceiling(endtime.TotalMinutes);

            int expectedDataDurationInSeconds = (int)tilingConfig.SegmentDuration.TotalSeconds;
            int expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / tilingConfig.SpectralFrameDuration);

            string name = fileStem + "_" + startMinute + "min.csv";
            string csvPath = Path.Combine(dataDir.FullName, name);
            bool skipHeader = true;
            bool skipFirstColumn = true;

            List<double[]> frameData = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
            ZoomTiledSpectrograms.PadEndOfListOfFrames(frameData, expectedFrameCount);
            for (int i = startMinute + 1; i < endMinute; i++)
            {
                name = fileStem + "_" + i + "min.csv";
                csvPath = Path.Combine(dataDir.FullName, name);

                List<double[]> data = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
                ZoomTiledSpectrograms.PadEndOfListOfFrames(data, expectedFrameCount);

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
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            Bitmap bmp = new Bitmap(width, height);
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

            string text = string.Format("(c) qut.edu.au");
            stringSize = g.MeasureString(text, stringFont);
            int X2 = width - stringSize.ToSize().Width - 2;
            if (X2 > X) g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X2, 3));

            g.DrawLine(new Pen(Color.LightGray), 0, 0, width, 0);//draw upper boundary
            g.DrawLine(new Pen(Color.LightGray), 0, 1, width, 1);//draw upper boundary
            g.DrawLine(new Pen(Color.LightGray), 0, height - 1, width, height - 1); //draw lower boundary
            g.DrawLine(new Pen(Color.Red), 0, 2, 0, height - 1);                    //draw start boundary
            g.DrawLine(new Pen(Color.Pink), width - 1, 2, width - 1, height - 1);   //draw end boundary
            return bmp;
        }

        public static Image FrameZoomSpectrogram(Image bmp1, Image titleBar, TimeSpan startOffset, TimeSpan xAxisPixelDuration, TimeSpan xAxisTicInterval, int nyquist, int herzInterval)
        {
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            AudioAnalysisTools.StandardSpectrograms.SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, startOffset, fullDuration, xAxisTicInterval, nyquist, herzInterval);
            int trackHeight = 20;
            Bitmap timeBmp = Image_Track.DrawTimeTrack(fullDuration, startOffset, bmp1.Width, trackHeight);
            int imageHt = bmp1.Height + titleBar.Height + trackHeight;

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
