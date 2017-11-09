// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZoomTiledSpectrograms.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace AudioAnalysisTools.LongDurationSpectrograms.Zooming
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Contracts;
    using Indices;
    using log4net;
    using TileImage;
    using TowseyLibrary;
    using Zio;
    using SpectrogramType = LongDurationSpectrograms.SpectrogramType;

    public static class ZoomTiledSpectrograms
    {
        private const double XNominalUnitScale = 60.0;
        private static readonly ILog Log = LogManager.GetLogger(nameof(ZoomTiledSpectrograms));

        /// <summary>
        /// THIS IS ENTRY METHOD FOR TILING SPECTROGRAMS.
        /// </summary>
        public static void DrawTiles(
            AnalysisIoInputDirectory io,
            ZoomParameters common,
            string analysisTag)
        {
            Log.Info("Begin Draw Super Tiles");
            Contract.Requires(common != null, "common can not be null");
            Contract.Requires(common.SpectrogramZoomingConfig != null, "SpectrogramZoomingConfig can not be null");

            var zoomConfig = common.SpectrogramZoomingConfig;
            LdSpectrogramConfig ldsConfig = common.SpectrogramZoomingConfig.LdSpectrogramConfig;
            var distributions = common.IndexDistributions;
            var indexGenerationData = common.IndexGenerationData;
            var indexProperties = common.IndexProperties;

            string fileStem = common.OriginalBasename;

            // scales for false color index spectrograms images in seconds per pixel.
            double[] indexScales = zoomConfig.SpectralIndexScale;

            // default scales for standard (FFT) spectrograms in seconds per pixel.
            double[] standardScales = zoomConfig.SpectralFrameScale;

            ValidateScales(indexScales, indexGenerationData.IndexCalculationDuration.TotalSeconds);

            var shouldRenderStandardScale = !standardScales.IsNullOrEmpty();
            if (shouldRenderStandardScale)
            {
                Log.Warn("Standard spectrograms will not be rendered");
            }

            var allImageScales = indexScales.Concat(standardScales).ToArray();

            Log.Info("Tiling at scales: " + allImageScales.ToCommaSeparatedList());

            // determine what naming format to use for tiles
            var namingPattern = GetTilingProfile(common, zoomConfig, indexGenerationData);

            // pad out image so it produces a whole number of tiles
            // this solves the asymmetric right padding of short audio files
            // var paddedWidth = (int)(Math.Ceiling(zoomConfig.TileWidth / xNominalUnitScale) * xNominalUnitScale);

            // create a new tiler
            // pass it scales for x and y-axis
            // also pass it unit scale relations (between unit scale and unit height/width) to use as a reference point
            var tiler = new Tiler(
                io.OutputBase,
                namingPattern,
                xScales: new SortedSet<double>(allImageScales),
                xUnitScale: XNominalUnitScale,
                unitWidth: 1440,
                yScales: new SortedSet<double>(allImageScales.Select(x => 1.0)),
                yUnitScale: 1.0,
                unitHeight: namingPattern.TileHeight);

            // TODO: does this load all spectra? even ones we don't need...
            var (spectra, filteredIndexProperties) = LoadSpectra(io, analysisTag, fileStem, indexProperties);

            // false color index tiles
            GenerateIndexSpectrogramTiles(
                indexScales,
                ldsConfig,
                filteredIndexProperties,
                zoomConfig,
                spectra,
                indexGenerationData,
                fileStem,
                namingPattern,
                tiler);

            // standard fft frame spectrograms
            if (shouldRenderStandardScale)
            {
                GenerateStandardSpectrogramTiles(
                    spectra,
                    indexGenerationData,
                    ldsConfig,
                    filteredIndexProperties,
                    zoomConfig,
                    standardScales,
                    fileStem,
                    namingPattern,
                    tiler);
            }

            Log.Success("Tiling complete");
        }

        private static void ValidateScales(double[] indexScales, double dataScale)
        {
            if (indexScales.IsNullOrEmpty())
            {
                throw new InvalidScaleException(
                    $"{nameof(SpectrogramZoomingConfig.SpectralIndexScale)} is null or empty "
                    + " we need at least some scales to render zooming spectrograms");
            }

            foreach (var scale in indexScales)
            {
                var rawScalingFactor = scale / dataScale;
                int scalingFactor = (int)Math.Round(rawScalingFactor);

                Contract.Requires<InvalidScaleException>(
                    Math.Abs(scalingFactor - rawScalingFactor) < 0.0000001,
                    $"Index scales must be integer factors of the data scale `{dataScale}` - provided scale `{scale}` was not valid");

                if (scalingFactor < 1)
                {
                    throw new InvalidScaleException(
                        "Index scale ratio to index calculation duration must be >=1 but was instead "
                        + scalingFactor.ToString());
                }
            }
        }

        private static void GenerateStandardSpectrogramTiles(
            Dictionary<string, double[,]> spectra,
            IndexGenerationData indexGeneration,
            LdSpectrogramConfig ldsConfig,
            Dictionary<string, IndexProperties> filteredIndexProperties,
            SpectrogramZoomingConfig zoomConfig,
            double[] standardScales,
            string fileStem,
            TilingProfile namingPattern,
            Tiler tiler)
        {
            Log.Info("START DRAWING ZOOMED-IN FRAME SPECTROGRAMS");

            TimeSpan dataDuration =
                TimeSpan.FromTicks(spectra["POW"].GetLength(1) * indexGeneration.IndexCalculationDuration.Ticks);
            TimeSpan duration = indexGeneration.RecordingDuration;

            Contract.Requires(
                (dataDuration - duration).Absolute() < indexGeneration.IndexCalculationDuration,
                "The expected amount of data was not supplied");

            var minuteCount = (int)Math.Ceiling(dataDuration.TotalMinutes);

            // window the standard spectrogram generation so that we can provide adjacent supertiles to the
            // tiler, so that bordering / overlapping tiles (for cases where tile size != multiple of supertile size)
            // don't render partial tiles (i.e. bad/partial rendering of image)

            // this is the function generator
            // use of Lazy means results will only be evaluated once
            // and only when needed. This is useful for sliding window.
            Lazy<TimeOffsetSingleLayerSuperTile[]> GenerateStandardSpectrogramGenerator(int minuteToLoad)
            {
                return new Lazy<TimeOffsetSingleLayerSuperTile[]>(
                    () =>
                        {
                            Log.Info("Starting generation for minute: " + minuteToLoad);

                            var superTilingResults = DrawSuperTilesFromSingleFrameSpectrogram(
                                null /*inputDirectory*/, //TODO:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                ldsConfig,
                                filteredIndexProperties,
                                zoomConfig,
                                minuteToLoad,
                                standardScales,
                                fileStem,
                                indexGeneration,
                                namingPattern.ChromeOption);

                            return superTilingResults;
                        });
            }

            Lazy<TimeOffsetSingleLayerSuperTile[]> previous = null;
            Lazy<TimeOffsetSingleLayerSuperTile[]> current = null;
            Lazy<TimeOffsetSingleLayerSuperTile[]> next = null;
            for (int minute = 0; minute < minuteCount; minute++)
            {
                Log.Trace("Starting loop for minute" + minute);

                // shift each value back
                previous = current;
                current = next ?? GenerateStandardSpectrogramGenerator(minute);

                next = minute + 1 < minuteCount ? GenerateStandardSpectrogramGenerator(minute + 1) : null;

                // for each scale level of the results
                for (int i = 0; i < current.Value.Length; i++)
                {
                    // finally tile the output
                    Log.Debug("Begin tile production for minute: " + minute);
                    tiler.Tile(previous?.Value[i], current.Value[i], next?.Value[i]);
                    Log.Debug("Begin tile production for minute: " + minute);
                }
            }
        }

        /// <summary>
        /// Derives false colour varying-resolution multi-index spectrograms from provided spectral indices.
        /// </summary>
        private static void GenerateIndexSpectrogramTiles(
            double[] indexScales,
            LdSpectrogramConfig ldsConfig,
            Dictionary<string, IndexProperties> filteredIndexProperties,
            SpectrogramZoomingConfig zoomConfig,
            Dictionary<string, double[,]> spectra,
            IndexGenerationData indexGenerationData,
            string fileStem,
            TilingProfile namingPattern,
            Tiler tiler)
        {
            // TOP MOST ZOOMED-OUT IMAGES
            Log.Info("START DRAWING ZOOMED-OUT INDEX SPECTROGRAMS");

            foreach (double scale in indexScales)
            {
                Log.Info("Starting scale: " + scale);

                if (indexGenerationData.RecordingDuration.TotalSeconds < scale)
                {
                    Log.Warn($"Skipping scale step {scale} because recording duration ({indexGenerationData.RecordingDuration}) is less than 1px of scale");
                    continue;
                }

                // TODO: eventual optimisation, remove concept of super tiles
                // TODO: optimisation, cache aggregation layers (currently every layer is raggregated from base level)
                TimeSpan imageScale = TimeSpan.FromSeconds(scale);
                var superTiles = DrawSuperTilesAtScaleFromIndexSpectrograms(
                    ldsConfig,
                    filteredIndexProperties,
                    zoomConfig,
                    imageScale,
                    spectra,
                    indexGenerationData,
                    fileStem,
                    namingPattern.ChromeOption);

                // tile images as we go
                Log.Debug("Writing index tiles for " + scale);
                tiler.TileMany(superTiles);
                Log.Debug("Completed writing index tiles for " + scale);
            }
        }

        private static TilingProfile GetTilingProfile(
            ZoomParameters common,
            SpectrogramZoomingConfig zoomConfig,
            IndexGenerationData indexGeneration)
        {
            TilingProfile namingPattern;
            switch (zoomConfig.TilingProfile)
            {
                case nameof(PanoJsTilingProfile):
                    namingPattern = new PanoJsTilingProfile();

                    if (zoomConfig.TileWidth != namingPattern.TileWidth)
                    {
                        throw new ConfigFileException(
                            "TileWidth must match the default PanoJS TileWidth of " + namingPattern.TileWidth);
                    }

                    break;
                case nameof(AbsoluteDateTilingProfile):
                    // Zooming spectrograms use multiple color profiles at different levels
                    // therefore unable to set a useful tag (like ACI-ENT-EVN).
                    if (indexGeneration.RecordingStartDate != null)
                    {
                        var tilingStartDate = GetPreviousTileBoundary(
                            zoomConfig.TileWidth,
                            XNominalUnitScale,
                            (DateTimeOffset)indexGeneration.RecordingStartDate);

                        // if we're not writing tiles to disk, omit the basename because whatever the container format is
                        // it will have the base name attached
                        namingPattern = new AbsoluteDateTilingProfile(
                            common.OmitBasename ? string.Empty : common.OriginalBasename,
                            "BLENDED.Tile",
                            tilingStartDate,
                            indexGeneration.FrameLength / 2,
                            zoomConfig.TileWidth);
                    }
                    else
                    {
                        throw new ArgumentNullException(
                            nameof(zoomConfig.TilingProfile),
                            "`RecordingStateDate` from the `IndexGenerationData.json` cannot be null when `AbsoluteDateTilingProfile` specified");
                    }

                    break;
                default:
                    throw new ConfigFileException(
                        $"The {nameof(zoomConfig.TilingProfile)} configuration property was set to an unsupported value - no profile known by that name");
            }

            Log.Info(
                $"Tiling using {namingPattern.GetType().Name}, Tile Width: {namingPattern.TileWidth}, Height: {namingPattern.TileHeight}");
            return namingPattern;
        }

        private static (Dictionary<string, double[,]>, Dictionary<string, IndexProperties>) LoadSpectra(
            AnalysisIoInputDirectory io,
            string analysisTag,
            string fileStem,
            Dictionary<string, IndexProperties> indexProperties)
        {
            indexProperties = InitialiseIndexProperties.FilterIndexPropertiesForSpectralOnly(indexProperties);
            string[] keys = indexProperties.Keys.ToArray();

            Dictionary<string, double[,]> spectra = IndexMatrices.ReadSpectralIndices(
                io.InputBase,
                fileStem,
                analysisTag,
                keys);

            return (spectra, indexProperties);
        }

        public static DateTimeOffset GetPreviousTileBoundary(int tileWidth, double scale, DateTimeOffset recordingStartDate)
        {
            // if recording does not start on an absolutely aligned hour of the day
            // align it, then adjust where the tiling starts from, and calculate the offset for the super tile (the gap)
            var timeOfDay = recordingStartDate.TimeOfDay;
            var previousAbsoluteHour =
                TimeSpan.FromSeconds(
                    Math.Floor(timeOfDay.TotalSeconds / (scale * tileWidth))
                    * (scale * tileWidth));
            var gap = timeOfDay - previousAbsoluteHour;
            var tilingStartDate = recordingStartDate - gap;
            return tilingStartDate;
        }

        /// <summary>
        /// THis method is a way of getting the acoustic index data at 0.2 second resolution to have some influence on the
        ///     frame spectrograms at 0.02s resolution.
        ///     We cannot assume that the two matrices will have the same number of columns i.e. same temporal duration.
        ///     The frame data has been padded to one minute duration. But the last index matrix will probably NOT be the full one
        ///     minute duration.
        ///     Therefore assume that indexData matrix will be shorter and take its column count.
        /// </summary>
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
            SpectrogramZoomingConfig zoomingConfig,
            TimeSpan startTimeOfData,
            TimeSpan frameScale,
            double[,] frameData,
            IndexGenerationData indexGeneration,
            ImageChrome chromeOption)
        {
            // TODO:  the following normalisation bounds could be passed instead of using hard coded.
            double min = zoomingConfig.LowerNormalizationBoundForDecibelSpectrograms;
            double max = zoomingConfig.UpperNormalizationBoundForDecibelSpectrograms;

            //need to correctly orient the matrix for this method
            frameData = MatrixTools.MatrixRotate90Clockwise(frameData);

            // Get an unchromed image
            Image spectrogramImage = ZoomFocusedSpectrograms.DrawStandardSpectrogramInFalseColour(frameData);

            if (chromeOption == ImageChrome.Without)
            {
                return spectrogramImage;
            }

            int nyquist = indexGeneration.SampleRateResampled / 2;
            int herzInterval = 1000;
            string title = $"ZOOM SCALE={frameScale.TotalMilliseconds}ms/pixel ";
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

        /// <summary>
        /// Draws FC index spectrograms for an entire row
        /// </summary>
        public static TimeOffsetSingleLayerSuperTile[] DrawSuperTilesAtScaleFromIndexSpectrograms(
            LdSpectrogramConfig analysisConfig,
            Dictionary<string, IndexProperties> indexProperties,
            SpectrogramZoomingConfig zoomingConfig,
            TimeSpan imageScale,
            Dictionary<string, double[,]> spectra,
            IndexGenerationData indexGeneration,
            string basename,
            ImageChrome chromeOption)
        {
            Contract.Requires(!spectra.IsNullOrEmpty(), "ERROR: NO SPECTRAL DATA SUPPLIED");

            // calculate source data duration from column count of arbitrary matrix
            TimeSpan dataScale = indexGeneration.IndexCalculationDuration;
            double[,] matrix = spectra.First().Value;
            TimeSpan sourceDataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            int tileWidth = zoomingConfig.TileWidth;
            int superTileWidth = zoomingConfig.SuperTileWidthDefault();
            var superTileCount =
                (int)Math.Ceiling(zoomingConfig.SuperTileCount(sourceDataDuration, imageScale.TotalSeconds));

            TimeSpan superTileDuration = TimeSpan.FromTicks(superTileWidth * imageScale.Ticks);

            // initialize the image array to return
            var imageArray = new TimeOffsetSingleLayerSuperTile[superTileCount];
            TimeSpan startTime = indexGeneration.MinuteOffset; // default = zero minute of day i.e. midnight

            // start the loop
            for (int t = 0; t < superTileCount; t++)
            {
                Image image = DrawOneScaledIndexSpectrogramTile(
                    analysisConfig,
                    indexGeneration,
                    indexProperties,
                    startTime,
                    dataScale,
                    imageScale,
                    superTileWidth,
                    spectra,
                    basename,
                    chromeOption);

                imageArray[t] = new TimeOffsetSingleLayerSuperTile
                                    {
                                        TimeOffset = startTime,
                                        Scale = imageScale,
                                        SpectrogramType = SpectrogramType.Index,
                                        Image = image,
                                    };

                startTime += superTileDuration;
                if (startTime > sourceDataDuration)
                {
                    break;
                }
            }

            return imageArray;
        }

        public static Image DrawOneScaledIndexSpectrogramTile(
            LdSpectrogramConfig config,
            IndexGenerationData indexGenerationData,
            Dictionary<string, IndexProperties> indexProperties,
            TimeSpan startTime,
            TimeSpan dataScale,
            TimeSpan imageScale,
            int superTileImageWidth,
            Dictionary<string, double[,]> spectra,
            string basename,
            ImageChrome chromeOption)
        {
            Contract.Requires(!spectra.IsNullOrEmpty());

            // calculate start time by combining DatetimeOffset with minute offset.
            TimeSpan sourceMinuteOffset = indexGenerationData.MinuteOffset;
            if (indexGenerationData.RecordingStartDate.HasValue)
            {
                DateTimeOffset dto = (DateTimeOffset)indexGenerationData.RecordingStartDate;
                sourceMinuteOffset = dto.TimeOfDay + sourceMinuteOffset;
            }

            // calculate data duration from column count of abitrary matrix
            var matrix = spectra.First().Value;
            int columnCount = matrix.GetLength(1);
            TimeSpan dataDuration = TimeSpan.FromSeconds(columnCount * dataScale.TotalSeconds);

            var recordingStartTime = TimeTools.DateTimePlusTimeSpan(indexGenerationData.RecordingStartDate, indexGenerationData.MinuteOffset);

            TimeSpan offsetTime = TimeSpan.Zero;
            TimeSpan imageDuration = TimeSpan.FromTicks(superTileImageWidth * imageScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(superTileImageWidth * imageScale.Ticks / 2);
            if (startTime < TimeSpan.Zero)
            {
                offsetTime = TimeSpan.Zero - startTime;
                startTime = TimeSpan.Zero;
            }

            TimeSpan endTime = startTime + imageDuration;
            if (endTime > dataDuration)
            {
                endTime = dataDuration;
            }

            // get the plain unchromed spectrogram
            Image ldSpectrogram = ZoomCommon.DrawIndexSpectrogramCommon(
                config,
                indexGenerationData,
                indexProperties,
                startTime,
                endTime,
                dataScale,
                imageScale,
                superTileImageWidth,
                spectra,
                basename);

            if (chromeOption == ImageChrome.Without)
            {
                return ldSpectrogram;
            }

            Graphics g2 = Graphics.FromImage(ldSpectrogram);

            int nyquist = 22050 / 2;
            if (indexGenerationData.SampleRateResampled > 0)
            {
                nyquist = indexGenerationData.SampleRateResampled / 2;
            }

            int hertzInterval = 1000;
            if (config != null)
            {
                hertzInterval = config.YAxisTicInterval;
            }

            string title = $"ZOOM SCALE={imageScale.TotalSeconds}s/pixel";

            Image titleBar = ZoomFocusedSpectrograms.DrawTitleBarOfZoomSpectrogram(title, ldSpectrogram.Width);
            startTime += recordingStartTime;
            ldSpectrogram = ZoomFocusedSpectrograms.FrameZoomSpectrogram(
                ldSpectrogram,
                titleBar,
                startTime,
                imageScale,
                config.XAxisTicInterval,
                nyquist,
                hertzInterval);

            // create the base image
            Image image = new Bitmap(ldSpectrogram.Width, ldSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            var Xoffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(ldSpectrogram, Xoffset, 0);

            return image;
        }

        /// <summary>
        /// Assume that we are processing data for one minute only.
        ///     From this one minute of data, we produce images at three scales.
        ///     A one minute recording framed at 20ms should yield 3000 frames.
        ///     But to achieve this where sr= 22050 and frameSize=512, we need an overlap of 71 samples.
        ///     Consequently only 2999 frames returned per minute.
        ///     Therefore have to pad end to get 3000 frames.
        /// </summary>
        public static TimeOffsetSingleLayerSuperTile[] DrawSuperTilesFromSingleFrameSpectrogram(
            DirectoryInfo dataDir,
            LdSpectrogramConfig analysisConfig,
            Dictionary<string, IndexProperties> indexProperties,
            SpectrogramZoomingConfig zoomingConfig,
            int minute,
            double[] imageScales,
            string basename,
            IndexGenerationData indexGeneration,
            ImageChrome chromeOption)
        {
            string fileStem = basename;

            // string analysisType = analysisConfig.AnalysisType;
            TimeSpan indexScale = indexGeneration.IndexCalculationDuration;
            TimeSpan frameScale = TimeSpan.FromSeconds(zoomingConfig.SpectralFrameDuration);

            var expectedDataDurationInSeconds = (int)indexGeneration.MaximumSegmentDuration.Value.TotalSeconds;
            var expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / zoomingConfig.SpectralFrameDuration);

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

                Image spectrogramImage = DrawFrameSpectrogramAtScale(
                    analysisConfig,
                    zoomingConfig,
                    startTimeOfData,
                    imageScale,
                    data,
                    indexGeneration,
                    chromeOption);

                str[scale] = new TimeOffsetSingleLayerSuperTile
                                 {
                                     TimeOffset = startTimeOfData,
                                     Scale = imageScale,
                                     SpectrogramType = SpectrogramType.Frame,
                                     Image = spectrogramImage,
                                 };
            }

            return str;
        }

        /// <summary>
        /// THis method pads the end of a list of frames read from a csv file.
        ///     The frame count will be one less than expected for the recording segment because of frame overlap
        ///     Therefore pad the end of the list of frames with the last frame.
        /// </summary>
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
        public static void TrimEndOfListOfFrames(List<double[]> frameList, int expectedFrameCount)
        {
            int frameDiscrepancy = frameList.Count - expectedFrameCount;
            if (frameDiscrepancy > 0)
            {
                frameList.RemoveRange(frameList.Count - frameDiscrepancy, frameDiscrepancy);
                Log.Warn(frameDiscrepancy + " frames were trimmed from a frame spectrogram");
            }
        }
    }
}