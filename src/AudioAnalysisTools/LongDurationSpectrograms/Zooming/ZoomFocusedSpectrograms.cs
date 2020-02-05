// <copyright file="ZoomFocusedSpectrograms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using DSP;
    using Indices;
    using StandardSpectrograms;
    using TowseyLibrary;
    using Zooming;
    using Zio;
    using Zio.FileSystems;

    public static class ZoomFocusedSpectrograms
    {
        public static void DrawStackOfZoomedSpectrograms(
            DirectoryInfo inputDirectory,
            DirectoryInfo outputDirectory,
            AnalysisIoInputDirectory io,
            ZoomParameters common,
            string analysisTag,
            TimeSpan focalTime,
            int imageWidth)
        {
            var zoomConfig = common.SpectrogramZoomingConfig;
            LdSpectrogramConfig ldsConfig = common.SpectrogramZoomingConfig.LdSpectrogramConfig;

            //var distributions = common.IndexDistributions;
            string fileStem = common.OriginalBasename;
            var indexGeneration = common.IndexGenerationData;
            TimeSpan dataScale = indexGeneration.IndexCalculationDuration;

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            //var indexGenerationData = common.IndexGenerationData;
            var indexProperties = zoomConfig.IndexProperties;
            var (spectra, filteredIndexProperties) = ZoomCommon.LoadSpectra(io, analysisTag, fileStem, zoomConfig.LdSpectrogramConfig, indexProperties);

            Stopwatch sw = Stopwatch.StartNew();

            // Set the default time-scales in seconds per pixel.
            // These were changed on 3rd April 2019 to better match those in the current zooming config file.
            double[] imageScales = { 60, 30, 15, 7.5, 3.2, 1.6, 0.8, 0.4, 0.2 };
            if (zoomConfig.SpectralIndexScale != null)
            {
                imageScales = zoomConfig.SpectralIndexScale;
            }

            sw = Stopwatch.StartNew();
            int scaleCount = imageScales.Length;
            var imageList = new List<Image>();
            for (int i = 0; i < scaleCount; i++)
            {
                var imageScale = TimeSpan.FromSeconds(imageScales[i]);
                var image = DrawIndexSpectrogramAtScale(ldsConfig, indexGeneration, filteredIndexProperties, focalTime, dataScale, imageScale, imageWidth, spectra, fileStem);
                if (image != null)
                {
                    imageList.Add(image);
                    string name = $"{fileStem}_FocalZoom_min{focalTime.TotalMinutes:f1}_scale{imageScales[i]}.png";
                    image.Save(Path.Combine(outputDirectory.FullName, name));
                }
            }

            sw.Stop();
            LoggedConsole.WriteLine("Finished spectrograms derived from spectral indices. Elapsed time = " + sw.Elapsed.TotalSeconds + " seconds");

            // NOTE: The following code is deprecated. It was originally developed to provide some intermediate steps between the hi-resolution false-colour spectrograms
            // and the standard grey scale spectrograms.
            // ####################### DERIVE ZOOMED IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES

            /*
            int[] compressionFactor = { 8, 4, 2, 1 };
            int compressionCount = compressionFactor.Length;
            sw = Stopwatch.StartNew();
            double frameStepInSeconds = indexGeneration.FrameStep / (double)indexGeneration.SampleRateResampled;
            TimeSpan frameScale = TimeSpan.FromTicks((long)Math.Round(frameStepInSeconds * 10000000));
            if (zoomConfig.SpectralFrameScale != null)
            {
                imageScales = zoomConfig.SpectralFrameScale;

                // TODO: CONVERT IMAGE scales into Compression factors.
                compressionCount = imageScales.Length;
                compressionFactor = new int[compressionCount];
                compressionFactor[compressionCount - 1] = 1;
                double denom = imageScales[compressionCount - 1];

                for (int i = 0; i < compressionCount - 1; i++)
                {
                    compressionFactor[i] = (int)Math.Round(imageScales[i] / denom);
                }
            }

            int maxCompression = compressionFactor[0];
            TimeSpan maxImageDuration = TimeSpan.FromTicks(maxCompression * imageWidth * frameScale.Ticks);

            TimeSpan halfMaxImageDuration = TimeSpan.FromMilliseconds(maxImageDuration.TotalMilliseconds / 2);
            TimeSpan startTimeOfMaxImage = TimeSpan.Zero;
            if (focalTime != TimeSpan.Zero)
            {
                startTimeOfMaxImage = focalTime - halfMaxImageDuration;
            }

            TimeSpan startTimeOfData = TimeSpan.FromMinutes(Math.Floor(startTimeOfMaxImage.TotalMinutes));

            List<double[]> frameData = ReadFrameData(inputDirectory, fileStem, startTimeOfMaxImage, maxImageDuration, zoomConfig, indexGeneration.MaximumSegmentDuration.Value);

            // get the index data to add into the
            // TimeSpan imageScale1 = TimeSpan.FromSeconds(0.1);
            double[,] indexData = spectra["PMN"];

            // make the images
            for (int i = 0; i < compressionCount; i++)
            {
                int factor = compressionFactor[i];
                var image = DrawFrameSpectrogramAtScale(ldsConfig, indexGeneration, startTimeOfData, factor, frameData, indexData, focalTime, frameScale, imageWidth);
                if (image != null)
                {
                    imageList.Add(image);
                }
            }

            sw.Stop();
            LoggedConsole.WriteLine("Finished spectrograms derived from standard frames. Elapsed time = " + sw.Elapsed.TotalSeconds + " seconds");
            */

            // combine the images into a stack
            Image combinedImage = ImageTools.CombineImagesVertically(imageList);
            string fileName = $"{fileStem}_FocalZOOM_min{focalTime.TotalMinutes:f1}.png";
            combinedImage.Save(Path.Combine(outputDirectory.FullName, fileName));
        }

        /// <summary>
        /// This method can add in absolute time if you want.
        /// Currently commented out - see below.
        /// </summary>
        public static Image DrawIndexSpectrogramAtScale(
            LdSpectrogramConfig config,
            IndexGenerationData indexGenerationData,
            Dictionary<string, IndexProperties> indexProperties,
            TimeSpan focalTime,
            TimeSpan dataScale,
            TimeSpan imageScale,
            int imageWidth,
            Dictionary<string, double[,]> spectra,
            string basename)
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

            Dictionary<string, IndexProperties> dictIp = indexProperties;
            dictIp = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(dictIp);

            // calculate start time by combining DatetimeOffset with minute offset.
            TimeSpan sourceMinuteOffset = indexGenerationData.AnalysisStartOffset;
            if (indexGenerationData.RecordingStartDate.HasValue)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                sourceMinuteOffset = dto.TimeOfDay + sourceMinuteOffset;
            }

            // calculate data duration from column count of abitrary matrix
            var kvp = spectra.First();
            var matrix = kvp.Value;

            //var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            TimeSpan recordingStartTime = TimeSpan.Zero; // default = zero minute of day i.e. midnight
            recordingStartTime = indexGenerationData.RecordingStartDate.Value.TimeOfDay.Add(indexGenerationData.AnalysisStartOffset);

            TimeSpan offsetTime = TimeSpan.Zero;
            TimeSpan imageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            TimeSpan startTime = TimeSpan.Zero;
            if (focalTime != TimeSpan.Zero)
            {
                startTime = focalTime - halfImageDuration;
            }

            if (startTime < TimeSpan.Zero)
            {
                offsetTime = TimeSpan.Zero - startTime;
                startTime = TimeSpan.Zero;
            }

            TimeSpan endTime = imageDuration;
            if (focalTime != TimeSpan.Zero)
            {
                endTime = focalTime + halfImageDuration;
            }

            if (endTime > dataDuration)
            {
                endTime = dataDuration;
            }

            TimeSpan spectrogramDuration = endTime - startTime;
            int spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

            // get the plain unchromed spectrogram
            Image ldfcSpectrogram = ZoomCommon.DrawIndexSpectrogramCommon(
                config,
                indexGenerationData,
                indexProperties,
                startTime,
                endTime,
                dataScale,
                imageScale,
                imageWidth,
                spectra,
                basename);

            if (ldfcSpectrogram == null)
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTROGRAM AT SCALE " + imageScale);
                return null;
            }

            // now chrome spectrogram
            Graphics g2 = Graphics.FromImage(ldfcSpectrogram);

            // draw red line at focus time
            if (focalTime != TimeSpan.Zero)
            {
                Pen pen = new Pen(Color.Red);
                TimeSpan focalOffset = focalTime - startTime;
                int x1 = (int)(focalOffset.Ticks / imageScale.Ticks);
                g2.DrawLine(pen, x1, 0, x1, ldfcSpectrogram.Height);
            }

            // draw the title bar
            int nyquist = 22050 / 2; // default
            if (indexGenerationData.SampleRateResampled > 0)
            {
                nyquist = indexGenerationData.SampleRateResampled / 2;
            }

            int herzInterval = 1000;
            if (config != null)
            {
                herzInterval = config.YAxisTicInterval;
            }

            string title = $"SCALE={imageScale.TotalSeconds}s/px.  Duration={spectrogramDuration} ";

            //add chrome
            // NEXT LINE USED ONLY IF WANT ABSOLUTE TIME
            //startTime += recordingStartTime;
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, ldfcSpectrogram.Width);
            ldfcSpectrogram = FrameZoomSpectrogram(
                ldfcSpectrogram,
                titleBar,
                startTime,
                imageScale,
                config.XAxisTicInterval,
                nyquist,
                herzInterval);

            // create the base canvas image on which to centre the focal image
            Image image = new Bitmap(imageWidth, ldfcSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            int xOffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(ldfcSpectrogram, xOffset, 0);
            return image;
        }

        /// <summary>
        /// This method can add in the absolute recording start time. However currently disabled.
        /// </summary>
        /// <param name="config">v</param>
        /// <param name="indexGenerationData">indexGenerationData</param>
        /// <param name="startTimeOfData">startTimeOfData</param>
        /// <param name="compressionFactor">compressionFactor</param>
        /// <param name="frameData">frameData</param>
        /// <param name="indexData">indexData</param>
        /// <param name="focalTime">focalTime</param>
        /// <param name="frameScale">frameScale</param>
        /// <param name="imageWidth">imageWidth</param>
        public static Image DrawFrameSpectrogramAtScale(
            LdSpectrogramConfig config,
            IndexGenerationData indexGenerationData,
            TimeSpan startTimeOfData,
            int compressionFactor,
            List<double[]> frameData,
            double[,] indexData,
            TimeSpan focalTime,
            TimeSpan frameScale,
            int imageWidth)
        {
            if (frameData == null || frameData.Count == 0)
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL SPECTROGRAM DATA SUPPLIED");
                return null;
            }

            // var recordingStartTime = TimeSpan.Zero; // default = zero minute of day i.e. midnight
            // var recordingStartTime = TimeTools.DateTimePlusTimeSpan(indexGenerationData.RecordingStartDate, indexGenerationData.AnalysisStartOffset);

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
            {
                spectralSelection = TemporalMatrix.CompressFrameSpectrograms(spectralSelection, compressionFactor);
            }

            Image spectrogramImage = DrawStandardSpectrogramInFalseColour(spectralSelection);

            Graphics g2 = Graphics.FromImage(spectrogramImage);

            int x1 = (int)(halfImageDuration.Ticks / imageScale.Ticks);

            // draw focus time on image
            if (focalTime != TimeSpan.Zero)
            {
                Pen pen = new Pen(Color.Red);
                g2.DrawLine(pen, x1, 0, x1, spectrogramImage.Height);
            }

            int nyquist = 22050 / 2; // default
            if (indexGenerationData.SampleRateResampled > 0)
            {
                nyquist = indexGenerationData.SampleRateResampled / 2;
            }

            int herzInterval = config.YAxisTicInterval;
            string title = $"ZOOM SCALE={imageScale.TotalMilliseconds}ms/pixel   Image duration={imageDuration} ";
            Image titleBar = DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);

            // add the recording start time ONLY IF WANT ABSOLUTE TIME SCALE - obtained from info in file name
            // startTime += recordingStartTime;
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

            //int xOffset = (int)(startTime.Ticks / imageScale.Ticks);
            int xOffset = (imageWidth / 2) - x1;
            g1.DrawImage(spectrogramImage, xOffset, 0);

            return image;
        }

        public static List<double[]> ReadFrameData(DirectoryInfo dataDir, string fileStem, TimeSpan startTime, TimeSpan maxDuration, SpectrogramZoomingConfig zoomingConfig, TimeSpan indexGenerationSegmentDuration)
        {
            TimeSpan endTime = startTime + maxDuration;
            int startMinute = (int)Math.Floor(startTime.TotalMinutes);
            int endMinute = (int)Math.Ceiling(endTime.TotalMinutes);

            int expectedDataDurationInSeconds = (int)indexGenerationSegmentDuration.TotalSeconds;
            int expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / zoomingConfig.SpectralFrameDuration);

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
                int compressedLength = colCount / scalingFactor;
                var newMatrix = new double[rowCount, compressedLength];
                double[] tempArray = new double[scalingFactor];

                // take the MAX
                for (int r = 0; r < rowCount; r++)
                {
                    for (int c = 0; c < colCount - scalingFactor; c += step)
                    {
                        var colIndex = c / scalingFactor;
                        for (int i = 0; i < scalingFactor; i++)
                        {
                            tempArray[i] = matrix[r, c + i];
                        }

                        newMatrix[r, colIndex] = tempArray.Max();
                    }
                }

                compressedSpectra[key] = newMatrix;
            }

            return compressedSpectra;
        }

        private static Dictionary<string, string> GetConfigurationForConvCNN(FileInfo configFile)
        {
            Config configuration = ConfigFile.Deserialize(configFile);

            var configDict = new Dictionary<string, string>(configuration.ToDictionary())
            {
                [AnalysisKeys.AddAxes] = (configuration.GetBoolOrNull(AnalysisKeys.AddAxes) ?? true).ToString(),
                [AnalysisKeys.AddSegmentationTrack] = (configuration.GetBoolOrNull(AnalysisKeys.AddSegmentationTrack) ?? true).ToString(),
                [AnalysisKeys.AddTimeScale] = configuration[AnalysisKeys.AddTimeScale] ?? "true",

                [AnalysisKeys.AddAxes] = configuration[AnalysisKeys.AddAxes] ?? "true",

                [AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? "true",
            };
            return configDict;
        }

        /// <summary>
        /// A FALSE-COLOUR VERSION OF DECIBEL SPECTROGRAM
        ///         Taken and adapted from Spectrogram Image 5 in the method of CLASS Audio2InputForConvCNN.cs:
        /// </summary>
        /// <param name="dbSpectrogramData">the sonogram data (NOT noise reduced) </param>
        public static Image DrawStandardSpectrogramInFalseColour(double[,] dbSpectrogramData)
        {
            // Do NOISE REDUCTION
            double noiseReductionParameter = 2.0;
            var tuple = SNR.NoiseReduce(dbSpectrogramData, NoiseReductionType.Standard, noiseReductionParameter);
            double[,] nrSpectrogramData = tuple.Item1;   // store data matrix

            double ridgeThreshold = 2.5;
            double[,] matrix = dbSpectrogramData;

            byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

            // ################### RESEARCH QUESTION:
            // I tried different EXPERIMENTS IN NORMALISATION
            //double min; double max;
            //DataTools.MinMax(spectralSelection, out min, out max);
            //double range = max - min;
            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            //double fractionalStretching = 0.2;
            //min = min + (range * fractionalStretching);
            //max = max - (range * fractionalStretching);
            //range = max - min;
            // ULTIMATELY THE BEST APPROACH APPEARED TO BE FIXED NORMALISATION BOUNDS

            double truncateMin = -95.0;
            double truncateMax = -30.0;
            double filterCoefficient = 0.75;
            double[,] dbSpectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(dbSpectrogramData, truncateMin, truncateMax, filterCoefficient);

            truncateMin = 0;
            truncateMax = 50;

            // nr = noise reduced
            double[,] nrSpectrogramNorm = SpectrogramTools.NormaliseSpectrogramMatrix(nrSpectrogramData, truncateMin, truncateMax, filterCoefficient);

            nrSpectrogramNorm = MatrixTools.BoundMatrix(nrSpectrogramNorm, 0.0, 0.9);
            nrSpectrogramNorm = MatrixTools.SquareRootOfValues(nrSpectrogramNorm);
            nrSpectrogramNorm = DataTools.normalise(nrSpectrogramNorm);

            // create image from normalised data
            var image = SpectrogramTools.CreateFalseColourDecibelSpectrogramForZooming(dbSpectrogramNorm, nrSpectrogramNorm, hits);

            return image;
        }

        public static Image DrawTitleBarOfZoomSpectrogram(string title, int width)
        {
            //Image colourChart = LDSpectrogramRGB.DrawColourScale(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR - 2);
            int height = SpectrogramConstants.HEIGHT_OF_TITLE_BAR;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);
            Font stringFont = new Font("Arial", 9);

            int x1 = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(x1, 3));

            SizeF stringSize = g.MeasureString(title, stringFont);
            x1 += stringSize.ToSize().Width + 70;

            //g.DrawString(text, stringFont, Brushes.Wheat, new PointF(X, 3));
            //stringSize = g.MeasureString(text, stringFont);
            //X += (stringSize.ToSize().Width + 1);
            //g.DrawImage(colourChart, X, 1);

            string text = Meta.OrganizationTag;
            stringSize = g.MeasureString(text, stringFont);
            int x2 = width - stringSize.ToSize().Width - 2;
            if (x2 > x1)
            {
                g.DrawString(text, stringFont, Brushes.Wheat, new PointF(x2, 3));
            }

            g.DrawLine(new Pen(Color.LightGray), 0, 0, width, 0); //draw upper boundary
            g.DrawLine(new Pen(Color.LightGray), 0, 1, width, 1); //draw upper boundary
            g.DrawLine(new Pen(Color.LightGray), 0, height - 1, width, height - 1); //draw lower boundary
            g.DrawLine(new Pen(Color.Red), 0, 2, 0, height - 1);                    //draw start boundary
            g.DrawLine(new Pen(Color.Pink), width - 1, 2, width - 1, height - 1);   //draw end boundary
            return bmp;
        }

        public static Image FrameZoomSpectrogram(Image bmp1, Image titleBar, TimeSpan startOffset, TimeSpan xAxisPixelDuration, TimeSpan xAxisTicInterval, int nyquist, int herzInterval)
        {
            TimeSpan fullDuration = TimeSpan.FromTicks(xAxisPixelDuration.Ticks * bmp1.Width);

            // init frequency scale
            int frameSize = bmp1.Height * 2; // THIS MIGHT BECOME A BUG ONE DAY!!!!!
            var freqScale = new FrequencyScale(nyquist, frameSize, herzInterval);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)bmp1, startOffset, fullDuration, xAxisTicInterval, freqScale);
            int trackHeight = 20;

            // put start offset into a datetime object.
            var dto = default(DateTimeOffset);
            dto = dto + startOffset;

            Bitmap timeBmp = ImageTrack.DrawTimeTrack(fullDuration, dto, bmp1.Width, trackHeight);
            int imageHt = bmp1.Height + titleBar.Height + trackHeight + 1;

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
