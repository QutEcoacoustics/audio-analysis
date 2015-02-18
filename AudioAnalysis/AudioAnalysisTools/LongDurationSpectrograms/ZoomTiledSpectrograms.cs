//
// Action code for this analysis = ZoomingSpectrograms
namespace AudioAnalysisTools.LongDurationSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Acoustics.Shared;
    using Acoustics.Shared.Csv;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.TileImage;

    using log4net;
    using log4net.Repository.Hierarchy;

    using TowseyLibrary;

    public static class ZoomTiledSpectrograms
    {

        public static void DrawSuperTiles(DirectoryInfo inputDirectory, DirectoryInfo outputDirectory, FileInfo analysisConfigFile, FileInfo tilingConfigFile, FileInfo indexPropertiesConfigFile)
        {
            const bool saveSuperTiles = true;

            var analysisConfig = LdSpectrogramConfig.ReadYamlToConfig(analysisConfigFile);
            var tilingConfig   = Json.Deserialise<SuperTilingConfig>(tilingConfigFile);

            string fileStem    = analysisConfig.FileName;
            var namingPattern = new PanoJsTilingProfile();
            var tiler = new Tiler(outputDirectory, namingPattern, new SortedSet<decimal>(), 60.0m, 1440, 300);
            

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            DateTime now1 = DateTime.Now;
            string[] keys = { "ACI", "AVG", "BGN", "CVR", "ENT", "EVN" };
            Dictionary<string, double[,]> spectra = ZoomFocusedSpectrograms.ReadCSVFiles(analysisConfig.InputDirectoryInfo, fileStem, keys);
            DateTime now2 = DateTime.Now;
            TimeSpan et = now2 - now1;
            LoggedConsole.WriteLine("Time to read spectral index files = " + et.TotalSeconds + " seconds");

            // scales in seconds per pixel.
            double[] imageScales = {60, 24, 12, 6, 2, 1, 0.6, 0.2 };
            if (tilingConfig != null)
            {
                imageScales = tilingConfig.SpectralIndexScale;
            }

            // remove "Towsey.Acoustic" (i.e. 16 letters) from end of the file names
            // fileStem = "TEST_TUITCE_20091215_220004.Towsey.Acoustic" becomes "TEST_TUITCE_20091215_220004";
            int nameLength = fileStem.Length - 16;
            fileStem = fileStem.Substring(0, nameLength);


            // TOP MOST ZOOMED OUT IMAGES 
            foreach (double scale in imageScales)
            {
                TimeSpan imageScale = TimeSpan.FromSeconds(scale);
                var superTiles = DrawSuperTilesFromIndexSpectrograms(analysisConfig, indexPropertiesConfigFile, tilingConfig, imageScale, spectra);

                // below saving of images is for debugging.
                if (saveSuperTiles)
                {
                    string outputName;
                    Image[] images = superTiles.Select( x => x.Image).ToArray();
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

                // tile images as we go
                LoggedConsole.WriteLine("Writing index tiles for " + scale);
                tiler.TileMany(superTiles);
            }



            // ####################### DRAW ZOOMED IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES
            LoggedConsole.WriteLine("START DRAWING ZOOMED-IN FRAME SPECTROGRAMS");

            // default scales in seconds per pixel.
            double[] imageScales2 = { 0.1, 0.04, 0.02 };
            if (tilingConfig != null)
                imageScales2 = tilingConfig.SpectralFrameScale;

            // Prepare index matrix. This will be used to enhance the frame spectrogram
            /* 
             * RESEARCH QUESTION:   Which is the better index to use? CVR or AVG. 
             *                      Decide on CVR. It give better contrast and a less cluttered spgram.
             */
            TemporalMatrix cvrMatrix = new TemporalMatrix("columns", spectra["CVR"], analysisConfig.IndexCalculationDuration);
            TimeSpan dataDuration = cvrMatrix.DataDuration(); 
            int segmentDurationInSeconds = (int)tilingConfig.SegmentDuration.TotalSeconds;

            int minuteCount = (int)Math.Ceiling(dataDuration.TotalMinutes);
            for (int min = 0; min < minuteCount; min++)
            {
                double[,] indexMatrix = cvrMatrix.GetDataBlock(TimeSpan.FromMinutes(min), TimeSpan.FromSeconds(segmentDurationInSeconds));

                var superTilingResults = DrawSuperTilesFromSingleFrameSpectrogram(
                    analysisConfig,
                    indexPropertiesConfigFile,
                    tilingConfig,
                    min,
                    imageScales2,
                    indexMatrix);

                if (saveSuperTiles)
                { 
                    // below saving of images is for debugging.
                    foreach (var superTile in superTilingResults)
                    {
                        string outputName = string.Format("{0}_scale-{2:f2}_supertile-minute-{1}.png", fileStem, min, superTile.Scale.TotalSeconds);
                        superTile.Image.Save(Path.Combine(outputDirectory.FullName, outputName));
                    }
                }

                // finaly tile the output
                LoggedConsole.WriteLine("Begin tile production");
                tiler.TileMany(superTilingResults);
            }

            LoggedConsole.WriteLine("Tiling complete");
        }




        public static SuperTile[] DrawSuperTilesFromIndexSpectrograms(LdSpectrogramConfig analysisConfig, FileInfo indicesConfigFile, SuperTilingConfig tilingConfig, 
                                    TimeSpan imageScale, Dictionary<string, double[,]> spectra)
        {
            if (spectra == null)
            {
                LoggedConsole.WriteLine("WARNING: NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            // check that scalingFactor >= 1.0
            double scalingFactor = tilingConfig.ScalingFactor_SpectralIndex(imageScale.TotalSeconds);
            if (scalingFactor < 1.0)
            {
                LoggedConsole.WriteLine("WARNING: Scaling Factor < 1.0");
                return null;
            }

            // calculate source data duration from column count of abitrary matrix
            TimeSpan dataScale = analysisConfig.IndexCalculationDuration;
            var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan sourceDataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigFile);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            int tileWidth = tilingConfig.TileWidth;
            int superTileWidth = tilingConfig.SuperTileWidthDefault();
            int superTileCount = (int)Math.Ceiling(tilingConfig.SuperTileCount(sourceDataDuration, imageScale.TotalSeconds));

            TimeSpan superTileDuration = TimeSpan.FromTicks(superTileWidth * imageScale.Ticks);

            // initialise the image array to return
            var imageArray = new SuperTile[superTileCount];
            TimeSpan startTime = analysisConfig.MinuteOffset;   // default = zero minute of day i.e. midnight

            // start the loop
            for (int t = 0; t < superTileCount; t++)
            {
                Image image = ZoomTiledSpectrograms.DrawIndexSpectrogramAtScale(analysisConfig, indicesConfigFile, startTime, dataScale, imageScale, superTileWidth, spectra);
                imageArray[t] = new SuperTile()
                                    {
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


        public static Image DrawIndexSpectrogramAtScale(LdSpectrogramConfig config, FileInfo indicesConfigPath,
                                    TimeSpan startTime, TimeSpan dataScale, TimeSpan imageScale, int imageWidth, Dictionary<string, double[,]> spectra)
        {
            double scalingFactor = Math.Round(imageScale.TotalMilliseconds / dataScale.TotalMilliseconds);

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            TimeSpan sourceMinuteOffset = config.MinuteOffset;

            // calculate data duration from column count of abitrary matrix
            var matrix = spectra["ACI"]; // assume this key will always be present!!
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
            if (endTime > dataDuration) endTime = dataDuration;
            TimeSpan spectrogramDuration = endTime - startTime;
            int spectrogramWidth = (int)(spectrogramDuration.Ticks / imageScale.Ticks);

            int startIndex = (int)(startTime.Ticks / dataScale.Ticks);
            int endIndex = (int)(endTime.Ticks / dataScale.Ticks);
            if (endIndex >= columnCount) endIndex = columnCount - 1;
            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount - 1, endIndex);
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
                spectralSelection = ZoomFocusedSpectrograms.CompressIndexSpectrograms(spectralSelection, imageScale, dataScale);

            // These parameters define the colour maps and appearance of the false-colour spectrogram
            string colorMap1 = "ACI-ENT-EVN";
            string colorMap2 = "BGN-AVG-CVR";

            double backgroundFilterCoeff = (double?)config.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            var cs1 = new LDSpectrogramRGB(config, colorMap1);
            cs1.FileName = config.FileName;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties
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
                }
                else
                    if (imageScaleInMsPerPixel > 500)
                    {
                        blendWt1 = 0.3;
                        blendWt2 = 0.7;
                    }

            Image LDSpectrogram = cs1.DrawBlendedFalseColourSpectrogram("NEGATIVE", colorMap1, colorMap2, blendWt1, blendWt2);
            Graphics g2 = Graphics.FromImage(LDSpectrogram);

            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}s/pixel", imageScale.TotalSeconds);
            //string title = string.Format("ZOOM SCALE={0}s/pixel   Image duration={1} ",  //  (colour:R-G-B={2})
            //                                                           imageScale.TotalSeconds, spectrogramDuration);
            Image titleBar = ZoomFocusedSpectrograms.DrawTitleBarOfZoomSpectrogram(title, LDSpectrogram.Width);
            LDSpectrogram = ZoomFocusedSpectrograms.FrameZoomSpectrogram(LDSpectrogram, titleBar, startTime, imageScale, config.XAxisTicInterval, nyquist, herzInterval);

            // create the base image
            Image image = new Bitmap(imageWidth, LDSpectrogram.Height);
            Graphics g1 = Graphics.FromImage(image);
            g1.Clear(Color.DarkGray);

            int Xoffset = (int)(offsetTime.Ticks / imageScale.Ticks);
            g1.DrawImage(LDSpectrogram, Xoffset, 0);

            return image;
        }


        /// <summary>
        /// Assume that we are processing data for one minute only.
        /// From this one minute of data, we produce images at three scales.
        /// A one minute recording framed at 20ms should yield 3000 frames.
        /// But to achieve this where sr= 22050 and frameSize=512, we need an overlap of 71 samples.
        /// Consequently only 2999 frames returned per minute.
        /// Therefore have to pad end to get 3000 frames.
        /// </summary>
        /// <param name="analysisConfig"></param>
        /// <param name="indicesConfigFile"></param>
        /// <param name="tilingConfig"></param>
        /// <param name="minute"></param>
        /// <param name="imageScales"></param>
        /// <returns></returns>
        public static SuperTile[] DrawSuperTilesFromSingleFrameSpectrogram(LdSpectrogramConfig analysisConfig, FileInfo indexPropertiesConfigFile,
                                              SuperTilingConfig tilingConfig, int minute, double[] imageScales, double[,] indexMatrix)
        {
            //string analysisType   = analysisConfig.AnalysisType;
            string analysisType = "Towsey.Acoustic";
            string fileStem       = analysisConfig.FileName;
            DirectoryInfo dataDir = analysisConfig.InputDirectoryInfo;
            TimeSpan indexScale   = analysisConfig.IndexCalculationDuration;
            TimeSpan frameScale   = TimeSpan.FromSeconds(tilingConfig.SpectralFrameDuration);
            int expectedDataDurationInSeconds = (int)tilingConfig.SegmentDuration.TotalSeconds;
            int expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / tilingConfig.SpectralFrameDuration);

            // remove "Towsey.Acoustic" (i.e. 16 letters) from end of the file names
            // fileStem = "TEST_TUITCE_20091215_220004.Towsey.Acoustic" becomes "TEST_TUITCE_20091215_220004";
            int nameLength = fileStem.Length - analysisType.Length - 1;
            fileStem = fileStem.Substring(0, nameLength);
            string fileName = fileStem + "_" + minute + "min.csv";
            string csvPath = Path.Combine(dataDir.FullName, fileName);
            bool skipHeader = true;
            bool skipFirstColumn = true;

            List<double[]> frameList = CsvTools.ReadCSVFileOfDoubles(csvPath, skipHeader, skipFirstColumn);
            if (frameList == null)
            {
                LoggedConsole.WriteErrorLine("WARNING: METHOD DrawSuperTilesFromSingleFrameSpectrogram(): NO SPECTRAL DATA SUPPLIED");
                return null;
            }

            // frame count will be one less than expected for the recording segment because of frame overlap
            // Therefore pad the end of the list of frames with the last frame.
            int frameDiscrepancy = expectedFrameCount - frameList.Count;
            if (frameDiscrepancy > 0)
            {
                double[] frame = frameList[frameList.Count - 1];
                for (int d = 0; d < frameDiscrepancy; d++)
                {
                    frameList.Add(frame);
                }
            }


            TemporalMatrix frameData = new TemporalMatrix("rows", MatrixTools.ConvertList2Matrix(frameList), frameScale);
            TemporalMatrix indexData = new TemporalMatrix("columns", indexMatrix, indexScale);
            frameData.SwapTemporalDimension(); // so two data sets are equivalent.

            TimeSpan startTime = analysisConfig.MinuteOffset;   // default = zero minute of day i.e. midnight
            TimeSpan startTimeOfData = startTime + TimeSpan.FromMinutes(minute);
            
            var str = new SuperTile[3];
            // make the images
            for (int scale = 0; scale < imageScales.Length; scale++)
            {

                TimeSpan imageScale = TimeSpan.FromSeconds(imageScales[scale]);
                int compressionFactor = (int)Math.Round(imageScale.TotalMilliseconds / frameData.DataScale.TotalMilliseconds);
                double columnDuration = imageScale.TotalSeconds;
                //int expectedFrameCount = (int)Math.Round(expectedDataDurationInSeconds / columnDuration);

                // ############## RESEARCH CHOICE HERE >>>>  compress spectrograms to correct scale using either max or average
                // AVG appears to offer better contrast.
                //double[,] data = frameData.CompressMatrixInTemporalDirectionByTakingMax(imageScale);
                double[,] data = frameData.CompressMatrixInTemporalDirectionByTakingAverage(imageScale);
                double[,] indexMatrixExpanded = indexData.ExpandSubmatrixInTemporalDirection(TimeSpan.Zero, tilingConfig.SegmentDuration, imageScale);

                Image spectrogramImage = DrawFrameSpectrogramAtScale(analysisConfig, tilingConfig, startTimeOfData, imageScale, data, indexMatrixExpanded);

                str[scale] = new SuperTile()
                             {
                                 Scale = imageScale,
                                 SpectrogramType = SpectrogramType.Frame,
                                 Image = spectrogramImage
                             };
//                if (spectrogramImage != null)
//                {
//                    if (compressionFactor == 1) str.SuperTileScale1 = spectrogramImage;
//                    else
//                    if (compressionFactor == 2) str.SuperTileScale2 = spectrogramImage;
//                    else
//                    if (compressionFactor == 5) str.SuperTileScale5 = spectrogramImage;
//                }
            }

            return str;
        }



        public static Image DrawFrameSpectrogramAtScale(LdSpectrogramConfig config, SuperTilingConfig tilingConfig, TimeSpan startTimeOfData,
                                    TimeSpan frameScale, double[,] frameData, double[,] indexData)
        {
            // ################### RESEARCH QUESTION:
            // I tried different means of NORMALISATION
            //double min; double max;
            //DataTools.MinMax(spectralSelection, out min, out max);
            //double range = max - min;
            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            //double fractionalStretching = 0.2;
            //min = min + (range * fractionalStretching);
            //max = max - (range * fractionalStretching);
            //range = max - min;

            // ULTIMATELY THE BEST APPROACH APPEARED TO BE FIXED NORMALISATION BOUNDS
            double min = tilingConfig.lowerNormalisationBoundForDecibelSpectrograms;
            double max = tilingConfig.upperNormalisationBoundForDecibelSpectrograms;
            frameData = MatrixTools.boundMatrix(frameData, min, max);
            frameData = DataTools.normalise(frameData);

            // at this point moderate the frame data using the index data
            double threshold = 0.5;
            ModerateFrameDataUsingIndexData(frameData, indexData, threshold);

            var cch = CubeHelix.GetCubeHelix();
            Image spectrogramImage = cch.DrawMatrixWithoutNormalisation(frameData);

            int nyquist = config.SampleRate / 2;            
            int herzInterval = 1000;
            string title = string.Format("ZOOM SCALE={0}ms/pixel ", frameScale.TotalMilliseconds);
            Image titleBar = ZoomFocusedSpectrograms.DrawTitleBarOfZoomSpectrogram(title, spectrogramImage.Width);
            spectrogramImage = ZoomFocusedSpectrograms.FrameZoomSpectrogram(spectrogramImage, titleBar, startTimeOfData, frameScale, 
                                                                            config.XAxisTicInterval, nyquist, herzInterval);
            return spectrogramImage;
        }

        /// <summary>
        /// THis method is a way of getting the acoustic index data at 0.2 second resolution to have some influence on the frame spectrograms at 0.02s resolution.
        /// We cannot assume that the two matrices will have the same number of columns i.e. same temporal duration.
        /// The frame data has been padded to one minute duration. But the last index matrix will unlikely  be the full one minute duration.
        /// Therefore assume that indexData matrix will be shorter and take its column count.
        /// </summary>
        /// <param name="frameData"></param>
        /// <param name="indexData"></param>
        /// <param name="threshold"></param>
        public static void ModerateFrameDataUsingIndexData(double[,] frameData, double[,] indexData, double threshold)
        {
            int rowCount = frameData.GetLength(0); //number of rows should be same in both matrices
            int colCount = indexData.GetLength(1); //number of column will possibly be fewer in the indexData matrix.
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if(indexData[r, c] < threshold)
                        frameData[r, c] *= threshold;
                        //frameData[r, c] *= indexData[r, c];
                }//end all columns
            }//end all rows

        }

    }



    public class SuperTilingConfig
    {
        public TimeSpan SegmentDuration = TimeSpan.FromSeconds(60);
        public string UnitsOfTime { get; set; } // should be "seconds"
        public string ScaleUnits { get; set; }  // should be "SecondsPerPixel"
        public double IndexCalculationDuration { get; set; } //: 0.2,
        public double SpectralFrameDuration { get; set; } //"SpectralFrameDuration": 0.02,
        public int TileWidth { get; set; }                  //"TileWidth": 300,
        public int MaxTilesPerSuperTile { get; set; }       // should be about 12
        public double[] SpectralIndexScale { get; set; }
        public double[] SpectralFrameScale { get; set; }
        public double lowerNormalisationBoundForDecibelSpectrograms = -100;
        public double LowerNormalisationBoundForDecibelSpectrograms { get; set; }
        public double upperNormalisationBoundForDecibelSpectrograms = -20;
        public double UpperNormalisationBoundForDecibelSpectrograms { get; set; }


        public int ScalingFactor_SpectralIndex(double scaleValue_secondsPerPixel)
        {
            int scaleFactor = (int)Math.Round(scaleValue_secondsPerPixel / IndexCalculationDuration);
            return scaleFactor;
        }

        public int ScalingFactor_SpectralFrame(double scaleValue_secondsPerPixel)
        {
            int scaleFactor = (int)Math.Round(scaleValue_secondsPerPixel / SpectralFrameDuration);
            return scaleFactor;
        }

        public TimeSpan TimePerTile(double scaleValue_secondsPerPixel)
        {
            return TimeSpan.FromSeconds(TileWidth * scaleValue_secondsPerPixel);
        }

        /// <summary>
        /// returns fractional tile count generated by a recording at any one scale
        /// </summary>
        /// <param name="recordingDuration"></param>
        /// <param name="scaleValue_secondsPerPixel"></param>
        /// <returns></returns>
        public double TileCount(TimeSpan recordingDuration, double scaleValue_secondsPerPixel)
        {
            TimeSpan tileDuration = TimeSpan.FromSeconds(TileWidth * scaleValue_secondsPerPixel);
            double count = recordingDuration.TotalMilliseconds / tileDuration.TotalMilliseconds;
            return count;
        }

        public TimeSpan TimePerSuperTile(double scaleValue_secondsPerPixel)
        {
            return TimeSpan.FromSeconds(TileWidth * scaleValue_secondsPerPixel * MaxTilesPerSuperTile);
        }

        public int SuperTileWidthDefault()
        {
            return (TileWidth * MaxTilesPerSuperTile);
        }

        public double SuperTileCount(TimeSpan recordingDuration, double scaleValue_secondsPerPixel)
        {
            TimeSpan supertileDuration = TimeSpan.FromSeconds(TileWidth * scaleValue_secondsPerPixel * MaxTilesPerSuperTile);
            double count = recordingDuration.TotalMilliseconds / supertileDuration.TotalMilliseconds;
            return count;
        }

    }

    public enum SpectrogramType
    {
        Frame,
        Index
    }

    public class SuperTile
    {
        public TimeSpan Scale { get; set; }

        public SpectrogramType SpectrogramType { get; set; }

        public Image Image { get; set; }
    }

//    public class SuperTilingResults
//    {
//        //public string UnitsOfTime { get; set; } // should be "seconds"
//        //public string ScaleUnits { get; set; }  // should be "SecondsPerPixel"
//        //public double IndexCalculationDuration { get; set; } //: 0.2,
//        //public double SpectralFrameDuration { get; set; } //"SpectralFrameDuration": 0.02,
//        //public int TileWidth { get; set; }                  //"TileWidth": 300,
//        public Image[] SupertilesFromSpectralIndices { get; set; }
//
//        public Image[] SuperTilesFromFrameData { get; set; }
//        public Image SuperTileScale1 { get; set; }
//        public Image SuperTileScale2 { get; set; }
//        public Image SuperTileScale5 { get; set; }
//
//    }


}
