// --------------------------------------------------------------------------------------------------------------------
// <copyright company="QutBioacoustics" file="ZoomTiledSpectrograms.cs">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.TileImage;

    using log4net;

    using TowseyLibrary;

    public static class ZoomTiledSpectrograms
    {
        #region Static Fields

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// THis method is a way of getting the acoustic index data at 0.2 second resolution to have some influence on the
        ///     frame spectrograms at 0.02s resolution.
        ///     We cannot assume that the two matrices will have the same number of columns i.e. same temporal duration.
        ///     The frame data has been padded to one minute duration. But the last index matrix will probably NOT be the full one
        ///     minute duration.
        ///     Therefore assume that indexData matrix will be shorter and take its column count.
        /// </summary>
        /// <param name="frameData">
        /// </param>
        /// <param name="indexData">
        /// </param>
        /// <param name="frameWt">
        /// The frame Wt.
        /// </param>
        /// <param name="indexWt">
        /// The index Wt.
        /// </param>
        public static void CombineFrameDataWithIndexData(
            double[,] frameData, 
            double[,] indexData, 
            double frameWt, 
            double indexWt)
        {
            int rowCount = frameData.GetLength(0); // number of rows should be same in both matrices
            int colCount = indexData.GetLength(1); // number of column will possibly be fewer in the indexData matrix.
            for (int c = 0; c < colCount; c++)
            {
                for (int r = 0; r < rowCount; r++)
                {
                    frameData[r, c] = (indexWt * indexData[r, c]) + (frameWt * Math.Sqrt(frameData[r, c]));
                }
            }

            // end all rows
        }

        public static Image DrawFrameSpectrogramAtScale(
            LdSpectrogramConfig config, 
            SuperTilingConfig tilingConfig, 
            TimeSpan startTimeOfData, 
            TimeSpan frameScale, 
            double[,] frameData, 
            double[,] indexDataNormalised,
            IndexGenerationData indexGeneration)
        {
            // ################### RESEARCH QUESTION:
            // I tried different means of NORMALISATION
            // double min; double max;
            // DataTools.MinMax(spectralSelection, out min, out max);
            // double range = max - min;
            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            // double fractionalStretching = 0.2;
            // min = min + (range * fractionalStretching);
            // max = max - (range * fractionalStretching);
            // range = max - min;

            // ULTIMATELY THE BEST APPROACH APPEARED TO BE FIXED NORMALISATION BOUNDS
            double min = tilingConfig.LowerNormalisationBoundForDecibelSpectrograms;
            double max = tilingConfig.UpperNormalisationBoundForDecibelSpectrograms;
            frameData = MatrixTools.boundMatrix(frameData, min, max);
            frameData = DataTools.normalise(frameData);

            // at this point moderate the frame data using the index data
            int msScale = frameScale.Milliseconds;
            double frameWt = 0.9;
            double indexWt = 0.1;

            // adjust combination weights to the scale, so as to decrease influence of index data.
            if (msScale == 100)
            {
                frameWt = 0.5;
                indexWt = 0.5;
            }
            else if (msScale == 40)
            {
                frameWt = 0.7;
                indexWt = 0.3;
            }

            CombineFrameDataWithIndexData(frameData, indexDataNormalised, frameWt, indexWt);

            CubeHelix cch = CubeHelix.GetCubeHelix();
            Image spectrogramImage = cch.DrawMatrixWithoutNormalisation(frameData);

            int nyquist = indexGeneration.SampleRate / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}ms/pixel ", frameScale.TotalMilliseconds);
            Image titleBar = ZoomFocusedSpectrograms.DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);
            spectrogramImage = ZoomFocusedSpectrograms.FrameZoomSpectrogram(
                spectrogramImage, 
                titleBar, 
                startTimeOfData, 
                frameScale, 
                config.XAxisTicInterval, 
                nyquist, 
                herzInterval);
            return spectrogramImage;
        }

        public static Image DrawIndexSpectrogramAtScale(LdSpectrogramConfig config, Dictionary<string, IndexProperties> dictIP, TimeSpan startTime, TimeSpan dataScale, TimeSpan imageScale, int imageWidth, Dictionary<string, double[,]> spectra, IndexGenerationData indexGeneration, string basename)
        {
            double scalingFactor = Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);

            TimeSpan sourceMinuteOffset = indexGeneration.MinuteOffset;

            // calculate data duration from column count of abitrary matrix
            double[,] matrix = spectra["ACI"]; // assume this key will always be present!!
            int columnCount = matrix.GetLength(1);
            TimeSpan dataDuration = TimeSpan.FromSeconds(columnCount * dataScale.TotalSeconds);

            TimeSpan offsetTime = TimeSpan.Zero;
            TimeSpan ImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * imageScale.Ticks / 2);
            if (startTime < TimeSpan.Zero)
            {
                offsetTime = TimeSpan.Zero - startTime;
                startTime = TimeSpan.Zero;
            }

            TimeSpan endTime = startTime + ImageDuration;
            if (endTime > dataDuration)
            {
                endTime = dataDuration;
            }

            TimeSpan spectrogramDuration = endTime - startTime;
            var spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

            var startIndex = (int)(startTime.Ticks / dataScale.Ticks);
            var endIndex = (int)(endTime.Ticks / dataScale.Ticks);
            if (endIndex >= columnCount)
            {
                endIndex = columnCount - 1;
            }

            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount - 1, endIndex);
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
            {
                spectralSelection = ZoomFocusedSpectrograms.CompressIndexSpectrograms(
                    spectralSelection, 
                    imageScale, 
                    dataScale);
            }

            // These parameters define the colour maps and appearance of the false-colour spectrogram
            string colorMap1 = "ACI-ENT-EVN";
            string colorMap2 = "BGN-POW-CVR";

            double backgroundFilterCoeff = indexGeneration.BackgroundFilterCoeff;

            // double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation
            var cs1 = new LDSpectrogramRGB(config, indexGeneration, colorMap1);
            cs1.FileName = basename;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties
            cs1.LoadSpectrogramDictionary(spectralSelection);

            var imageScaleInMsPerPixel = (int)imageScale.TotalMilliseconds;
            double blendWt1 = 0.0;
            double blendWt2 = 1.0;

            if (imageScaleInMsPerPixel > 15000)
            {
                blendWt1 = 1.0;
                blendWt2 = 0.0;
            }
            else if (imageScaleInMsPerPixel > 5000)
            {
                blendWt1 = 0.9;
                blendWt2 = 0.1;
            }
            else if (imageScaleInMsPerPixel >= 2000)
            {
                blendWt1 = 0.7;
                blendWt2 = 0.3;
            }
            else if (imageScaleInMsPerPixel >= 1000)
            {
                blendWt1 = 0.5;
                blendWt2 = 0.5;
            }
            else if (imageScaleInMsPerPixel > 500)
            {
                blendWt1 = 0.3;
                blendWt2 = 0.7;
            }

            Image LDSpectrogram = cs1.DrawBlendedFalseColourSpectrogram(
                "NEGATIVE", 
                colorMap1, 
                colorMap2, 
                blendWt1, 
                blendWt2);
            Graphics g2 = Graphics.FromImage(LDSpectrogram);

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}s/pixel", imageScale.TotalSeconds);

            // string title = string.Format("ZOOM SCALE={0}s/pixel   Image duration={1} ",  //  (colour:R-G-B={2})
            // imageScale.TotalSeconds, spectrogramDuration);
            Image titleBar = ZoomFocusedSpectrograms.DrawTitleBarOfZoomSpectrogram(title, LDSpectrogram.Width);
            LDSpectrogram = ZoomFocusedSpectrograms.FrameZoomSpectrogram(
                LDSpectrogram, 
                titleBar, 
                startTime, 
                imageScale, 
                config.XAxisTicInterval, 
                nyquist, 
                herzInterval);

            // create the base image
            Image image = new Bitmap(LDSpectrogram.Width, LDSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            var Xoffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(LDSpectrogram, Xoffset, 0);

            return image;
        }

        public static void DrawSuperTiles(
            DirectoryInfo inputDirectory, 
            DirectoryInfo outputDirectory, 
            ZoomCommonArguments common)
        {
            const bool SaveSuperTiles = false;

            Log.Info("Begin Draw Super Tiles");

            var zoomConfig = common.SuperTilingConfig;
            LdSpectrogramConfig ldsConfig = common.SuperTilingConfig.LdSpectrogramConfig;
            var distributions = IndexDistributions.Deserialize(common.IndexDistributionsFile);
            var indexGeneration = Json.Deserialise<IndexGenerationData>(common.IndexGenerationDataFile);
            var indexProperties = common.IndexProperties;

            string fileStem = common.OriginalBasename;
            string analysisType = "Towsey.Acoustic";
            TimeSpan dataScale = indexGeneration.IndexCalculationDuration;


            // scales for false color images in seconds per pixel.
            double[] imageScales = { 60, 24, 12, 6, 2, 1, 0.6, 0.2 };

            // default scales for standard spectrograms in seconds per pixel.
            double[] imageScales2 = { 0.1, 0.04, 0.02 };

            if (zoomConfig != null)
            {
                imageScales = zoomConfig.SpectralIndexScale;
            }

            if (zoomConfig != null)
            {
                imageScales2 = zoomConfig.SpectralFrameScale;
            }

            IEnumerable<double> allImageScales = imageScales.Concat(imageScales2);

            var namingPattern = new PanoJsTilingProfile();

            // create a new tiler
            // pass it scales for x and y-axis
            // also pass it unit scale relations (between unit scale and unit height/width) to use as a reference point
            var tiler = new Tiler(
                outputDirectory, 
                namingPattern, 
                new SortedSet<double>(allImageScales), 
                60.0, 
                1440, 
                new SortedSet<double>(allImageScales.Select(x => 1.0)), 
                1.0, 
                300);

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            indexProperties = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(indexProperties);
            string[] keys = indexProperties.Keys.ToArray();

            Stopwatch timer = Stopwatch.StartNew();
            Dictionary<string, double[,]> spectra = ZoomFocusedSpectrograms.ReadCSVFiles(
                inputDirectory, 
                fileStem + "_" + analysisType, 
                keys);
            timer.Stop();
            Log.Info("Time to read spectral index files = " + timer.Elapsed.TotalSeconds + " seconds");

            // remove "Towsey.Acoustic" (i.e. 16 letters) from end of the file names
            // fileStem = "TEST_TUITCE_20091215_220004.Towsey.Acoustic" becomes "TEST_TUITCE_20091215_220004";
            int nameLength = fileStem.Length - 16;
            fileStem = fileStem.Substring(0, nameLength);

            // TOP MOST ZOOMED-OUT IMAGES 
            Log.Info("START DRAWING ZOOMED-OUT INDEX SPECTROGRAMS");
            foreach (double scale in imageScales)
            {
                Log.Info("Starting scale: " + scale);
                TimeSpan imageScale = TimeSpan.FromSeconds(scale);
                TimeOffsetSingleLayerSuperTile[] superTiles = DrawSuperTilesFromIndexSpectrograms(
                    ldsConfig, 
                    indexProperties, 
                    zoomConfig, 
                    imageScale, 
                    spectra,
                    indexGeneration, 
                    fileStem);

                // below saving of images is for debugging.
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (SaveSuperTiles)
                {
                    string outputName;
                    Image[] images = superTiles.Select(x => x.Image).ToArray();
                    if ((images != null) && (images.Length > 0))
                    {
                        for (int i = 0; i < images.Length; i++)
                        {
                            outputName = string.Format("{0}_scale-{1:f1}_supertile-{2}.png", fileStem, scale, i);
                            if (images[i] != null)
                            {
                                images[i].Save(Path.Combine(outputDirectory.FullName, outputName));
                            }
                        }
                    }

                    Image combo = ImageTools.CombineImagesInLine(images);
                    outputName = string.Format("{0}_scale-{1:f1}_Combo.png", fileStem, scale);
                    if (combo != null)
                    {
                        combo.Save(Path.Combine(outputDirectory.FullName, outputName));
                    }
                }
                else
                {
                    // tile images as we go
                    Log.Debug("Writing index tiles for " + scale);
                    tiler.TileMany(superTiles);
                    Log.Debug("Completed writing index tiles for " + scale);
                }
            }

            // ####################### DRAW ZOOMED-IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES
            Log.Info("START DRAWING ZOOMED-IN FRAME SPECTROGRAMS");

            // Prepare index matrix. This will be used to enhance the frame spectrogram
            /* 
             * RESEARCH QUESTION:   Which is the better index to use? CVR or POW. 
             *                      Decide on CVR. It give better contrast.
             *                      Later decide on POW. It gives a less cluttered spgram.
             */
            var powMatrix = new TemporalMatrix("columns", spectra["POW"], indexGeneration.IndexCalculationDuration);

            // double min = tilingConfig.lowerNormalisationBoundForDecibelSpectrograms;
            // double max = tilingConfig.upperNormalisationBoundForDecibelSpectrograms;
            // bounds for normaliseing the POW spectral index values
            const double Min = 0.0; // dB tilingConfig.lowerNormalisationBoundForDecibelSpectrograms;
            const double Max = 70.0;

            TimeSpan dataDuration = powMatrix.DataDuration();
            var segmentDurationInSeconds = (int)zoomConfig.SegmentDuration.TotalSeconds;

            var minuteCount = (int)Math.Ceiling(dataDuration.TotalMinutes);
            for (int minute = 0; minute < minuteCount; minute++)
            {
                Log.Info("Starting minute: " + minute);
                double[,] powIndexMatrix = powMatrix.GetDataBlock(
                    TimeSpan.FromMinutes(minute), 
                    TimeSpan.FromSeconds(segmentDurationInSeconds));
                powIndexMatrix = MatrixTools.boundMatrix(powIndexMatrix, Min, Max);
                powIndexMatrix = DataTools.normalise(powIndexMatrix);

                TimeOffsetSingleLayerSuperTile[] superTilingResults = DrawSuperTilesFromSingleFrameSpectrogram(
                    inputDirectory, 
                    ldsConfig, 
                    indexProperties, 
                    zoomConfig, 
                    minute, 
                    imageScales2, 
                    powIndexMatrix, 
                    fileStem,
                    indexGeneration);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (SaveSuperTiles)
                {
                    // below saving of images is for debugging.
                    foreach (TimeOffsetSingleLayerSuperTile superTile in superTilingResults)
                    {
                        string outputName = string.Format(
                            "{0}_scale-{2:f2}_supertile-minute-{1}.png",
                            fileStem,
                            minute,
                            superTile.Scale.TotalSeconds);
                        superTile.Image.Save(Path.Combine(outputDirectory.FullName, outputName));
                    }
                }
                else
                {
                    // finaly tile the output
                    Log.Debug("Begin tile production for minute: " + minute);
                    tiler.TileMany(superTilingResults);
                    Log.Debug("Begin tile production for minute: " + minute);
                }
            }

            Log.Info("Tiling complete");
        }

        public static TimeOffsetSingleLayerSuperTile[] DrawSuperTilesFromIndexSpectrograms(
            LdSpectrogramConfig analysisConfig, Dictionary<string, IndexProperties> dictIP, SuperTilingConfig tilingConfig, 
            TimeSpan imageScale, Dictionary<string, double[,]> spectra, IndexGenerationData indexGeneration, string basename)
        {
            if ((spectra == null) || (spectra.Count == 0))
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            // check that scalingFactor >= 1.0
            double scalingFactor = tilingConfig.ScalingFactorSpectralIndex(imageScale.TotalSeconds, indexGeneration.IndexCalculationDuration.TotalSeconds);
            if (scalingFactor < 1.0)
            {
                LoggedConsole.WriteLine("WARNING: Scaling Factor < 1.0");
                return null;
            }

            // calculate source data duration from column count of abitrary matrix
            TimeSpan dataScale = indexGeneration.IndexCalculationDuration;
            double[,] matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan sourceDataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            int tileWidth = tilingConfig.TileWidth;
            int superTileWidth = tilingConfig.SuperTileWidthDefault();
            var superTileCount =
                (int)Math.Ceiling(tilingConfig.SuperTileCount(sourceDataDuration, imageScale.TotalSeconds));

            TimeSpan superTileDuration = TimeSpan.FromTicks(superTileWidth * imageScale.Ticks);

            // initialise the image array to return
            var imageArray = new TimeOffsetSingleLayerSuperTile[superTileCount];
            TimeSpan startTime = indexGeneration.MinuteOffset; // default = zero minute of day i.e. midnight

            // start the loop
            for (int t = 0; t < superTileCount; t++)
            {
                Image image = DrawIndexSpectrogramAtScale(
                    analysisConfig, 
                    dictIP, 
                    startTime, 
                    dataScale, 
                    imageScale, 
                    superTileWidth, 
                    spectra,
                    indexGeneration,
                    basename);
                imageArray[t] = new TimeOffsetSingleLayerSuperTile
                                    {
                                        TimeOffset = startTime,
                                        Scale = imageScale, 
                                        SpectrogramType = SpectrogramType.Index, 
                                        Image = image
                                    };

                startTime += superTileDuration;
                if (startTime > sourceDataDuration)
                {
                    break;
                }
            }

            return imageArray;
        }

        /// <summary>
        /// Assume that we are processing data for one minute only.
        ///     From this one minute of data, we produce images at three scales.
        ///     A one minute recording framed at 20ms should yield 3000 frames.
        ///     But to achieve this where sr= 22050 and frameSize=512, we need an overlap of 71 samples.
        ///     Consequently only 2999 frames returned per minute.
        ///     Therefore have to pad end to get 3000 frames.
        /// </summary>
        /// <param name="dataDir">
        ///     The data Dir.
        /// </param>
        /// <param name="analysisConfig">
        /// </param>
        /// <param name="indexProperties"></param>
        /// <param name="tilingConfig">
        /// </param>
        /// <param name="minute">
        /// </param>
        /// <param name="imageScales">
        /// </param>
        /// <param name="indexDataNormalised">
        ///     The index Data_normalised.
        /// </param>
        /// <param name="basename"></param>
        /// <returns>
        /// The <see cref="SuperTile[]"/>.
        /// </returns>
        public static TimeOffsetSingleLayerSuperTile[] DrawSuperTilesFromSingleFrameSpectrogram(
            DirectoryInfo dataDir, LdSpectrogramConfig analysisConfig, Dictionary<string, IndexProperties> indexProperties, 
            SuperTilingConfig tilingConfig, int minute, double[] imageScales, double[,] indexDataNormalised, 
            string basename, IndexGenerationData indexGeneration)
        {
            string fileStem = basename;

            // string analysisType = analysisConfig.AnalysisType;
            TimeSpan indexScale = indexGeneration.IndexCalculationDuration;
            TimeSpan frameScale = TimeSpan.FromSeconds(tilingConfig.SpectralFrameDuration);
            var expectedDataDurationInSeconds = (int)tilingConfig.SegmentDuration.TotalSeconds;
            var expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / tilingConfig.SpectralFrameDuration);

            string fileName = fileStem + "_" + minute + "min.csv";
            string csvPath = Path.Combine(dataDir.FullName, fileName);
            bool skipHeader = true;
            bool skipFirstColumn = true;

            // read spectrogram into a list of frames 
            List<double[]> frameList = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
            if (frameList == null)
            {
                LoggedConsole.WriteErrorLine(
                    "WARNING: METHOD DrawSuperTilesFromSingleFrameSpectrogram(): NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            PadEndOfListOfFrames(frameList, expectedFrameCount);
            TrimEndOfListOfFrames(frameList, expectedFrameCount);

            //// frame count will be one less than expected for the recording segment because of frame overlap
            //// Therefore pad the end of the list of frames with the last frame.
            // int frameDiscrepancy = expectedFrameCount - frameList.Count;
            // if (frameDiscrepancy > 0)
            // {
            // double[] frame = frameList[frameList.Count - 1];
            // for (int d = 0; d < frameDiscrepancy; d++)
            // {
            // frameList.Add(frame);
            // }
            // }
            var frameData = new TemporalMatrix("rows", MatrixTools.ConvertList2Matrix(frameList), frameScale);
            var indexData = new TemporalMatrix("columns", indexDataNormalised, indexScale);
            frameData.SwapTemporalDimension(); // so the two data matrices have the same temporal dimension

            TimeSpan startTime = indexGeneration.MinuteOffset; // default = zero minute of day i.e. midnight
            TimeSpan startTimeOfData = startTime + TimeSpan.FromMinutes(minute);

            var str = new TimeOffsetSingleLayerSuperTile[imageScales.Length];

            // make the images
            for (int scale = 0; scale < imageScales.Length; scale++)
            {
                TimeSpan imageScale = TimeSpan.FromSeconds(imageScales[scale]);
                var compressionFactor =
                    (int)Math.Round(imageScale.TotalMilliseconds / frameData.DataScale.TotalMilliseconds);
                double columnDuration = imageScale.TotalSeconds;

                // int expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / columnDuration);

                // ############## RESEARCH CHOICE HERE >>>>  compress spectrograms to correct scale using either max or average
                // Average appears to offer better contrast.
                // double[,] data = frameData.CompressMatrixInTemporalDirectionByTakingMax(imageScale);
                double[,] data = frameData.CompressMatrixInTemporalDirectionByTakingAverage(imageScale);

                // preprocess the index data ready for combining with the frame data
                double[,] indexMatrixExpanded = indexData.ExpandSubmatrixInTemporalDirection(
                    TimeSpan.Zero, 
                    tilingConfig.SegmentDuration, 
                    imageScale);

                // double min = 0;
                // double max = 70;
                // double max = tilingConfig.upperNormalisationBoundForDecibelSpectrograms;
                // indexMatrixExpanded = MatrixTools.boundMatrix(indexMatrixExpanded, min, max);
                // indexMatrixExpanded = DataTools.normalise(indexMatrixExpanded);
                Image spectrogramImage = DrawFrameSpectrogramAtScale(
                    analysisConfig, 
                    tilingConfig, 
                    startTimeOfData, 
                    imageScale, 
                    data, 
                    indexMatrixExpanded,
                    indexGeneration);

                str[scale] = new TimeOffsetSingleLayerSuperTile
                                 {
                                     TimeOffset = startTimeOfData,
                                     Scale = imageScale, 
                                     SpectrogramType = SpectrogramType.Frame, 
                                     Image = spectrogramImage
                                 };
            }

            return str;
        }

        /// <summary>
        /// THis method pads the end of a list of frames read from a csv file.
        ///     The frame count will be one less than expected for the recording segment because of frame overlap
        ///     Therefore pad the end of the list of frames with the last frame.
        /// </summary>
        /// <param name="frameList">
        /// </param>
        /// <param name="expectedFrameCount">
        /// </param>
        public static void PadEndOfListOfFrames(List<double[]> frameList, int expectedFrameCount)
        {
            int frameDiscrepancy = expectedFrameCount - frameList.Count;
            if (frameDiscrepancy > 0)
            {
                double[] frame = frameList[frameList.Count - 1];
                for (int d = 0; d < frameDiscrepancy; d++)
                {
                    frameList.Add(frame);
                }
            }
        }

        /// <summary>
        /// THis method trims the end of a list of frames read from a csv file.
        ///     Sometimes inaccuracies in cutting audio produced frame counts that are too long.
        ///     Therefore too many columns are rendered. Simply remove the end frames and issue a warning.
        /// TODO: a better solution would be to interpolate the extra frames... but too hard at the moment.
        /// </summary>
        /// <param name="frameList">
        /// </param>
        /// <param name="expectedFrameCount">
        /// </param>
        public static void TrimEndOfListOfFrames(List<double[]> frameList, int expectedFrameCount)
        {
            int frameDiscrepancy = frameList.Count - expectedFrameCount;
            if (frameDiscrepancy > 0)
            {
                frameList.RemoveRange(frameList.Count - frameDiscrepancy, frameDiscrepancy);
                Log.Warn(frameDiscrepancy + " frames were timmed from a frame spectrogram");
            }
        }

        #endregion
    }
}