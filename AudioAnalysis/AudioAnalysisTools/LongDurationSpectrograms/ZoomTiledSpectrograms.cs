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

        public static void DrawSuperTiles(FileInfo analysisConfigFile, FileInfo tilingConfigFile, FileInfo indexPropertiesConfigFile)
        {

            var analysisConfig = LdSpectrogramConfig.ReadYamlToConfig(analysisConfigFile);
            var tilingConfig   = Json.Deserialise<SpectrogramScalingConfig>(tilingConfigFile);

            string fileStem    = analysisConfig.FileName;
            string opDir       = analysisConfig.OutputDirectoryInfo.FullName;

            // ####################### DERIVE ZOOMED OUT SPECTROGRAMS FROM SPECTRAL INDICES
            string[] keys = { "ACI", "AVG", "BGN", "CVR", "ENT", "EVN" };
            Dictionary<string, double[,]> spectra = ZoomFocusedSpectrograms.ReadCSVFiles(analysisConfig.InputDirectoryInfo, fileStem, keys);

            // scales in seconds per pixel.
            double[] imageScales = {60, 24, 12, 6, 2, 1, 0.6, 0.2 };
            if (tilingConfig != null)
                imageScales = tilingConfig.SpectralIndexScale;

            // remove "Towsey.Acoustic" (i.e. 16 letters) from end of the file names
            // fileStem = "TEST_TUITCE_20091215_220004.Towsey.Acoustic" becomes "TEST_TUITCE_20091215_220004";
            int nameLength = fileStem.Length - 16;
            fileStem = fileStem.Substring(0, nameLength);


            // TOP MOST ZOOMED OUT IMAGE 
            foreach (double scale in imageScales)
            {
                TimeSpan imageScale = TimeSpan.FromSeconds(scale);

                Image[] images = ZoomTiledSpectrograms.DrawSuperTilesFromIndexSpectrograms(analysisConfig, indexPropertiesConfigFile, tilingConfig, imageScale, spectra);
                if ((images != null) && (images.Length > 0))
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        string opName = String.Format("{0}_scale-{1:f1}_supertile-{2}.png", fileStem, scale, i);
                        if (images[i] != null) images[i].Save(Path.Combine(opDir, opName));
                    }
                }
            }


            // ####################### DERIVE ZOOMED IN SPECTROGRAMS FROM STANDARD SPECTRAL FRAMES
            //double frameDurationInSeconds = config.FrameWidth / (double)config.SampleRate;
            //double frameStepInSeconds     = config.FrameStep / (double)config.SampleRate;
            //TimeSpan frameScale = TimeSpan.FromTicks((long)Math.Round(frameStepInSeconds * 10000000));
            //int[] compressionFactor = { 5, 2, 1 };
            //int maxCompression = compressionFactor[compressionFactor.Length - 1];
            //TimeSpan maxImageDuration = TimeSpan.FromTicks(maxCompression * superTileWidth * frameScale.Ticks);

            //TimeSpan halfMaxImageDuration = TimeSpan.FromMilliseconds(maxImageDuration.TotalMilliseconds / 2);
            //TimeSpan startTimeOfMaxImage = focalTime - halfMaxImageDuration;
            //TimeSpan startTimeOfData = TimeSpan.FromMinutes(Math.Floor(startTimeOfMaxImage.TotalMinutes));

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
            //    image.Save(Path.Combine(opDir, opName));
            //}

            //factor = 2;
            //image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, superTileWidth, frameData);
            //if (image != null)
            //{
            //    string opName = String.Format("{0}_factor-{1:1}supertile-0.png", fileStem, factor);
            //    image.Save(Path.Combine(opDir, opName));
            //}

            //factor = 1;
            //image = DrawFrameSpectrogramAtScale(config, indicesConfigPath, startTimeOfData, focalTime, frameScale, factor, superTileWidth, frameData);
            //if (image != null)
            //{
            //    string opName = String.Format("{0}_factor-{1:1}supertile-0.png", fileStem, factor);
            //    image.Save(Path.Combine(opDir, opName));
            //}

        }




        public static Image[] DrawSuperTilesFromIndexSpectrograms(LdSpectrogramConfig analysisConfig, FileInfo indicesConfigFile, SpectrogramScalingConfig tilingConfig, 
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

            // calculate data duration from column count of abitrary matrix
            TimeSpan dataScale = analysisConfig.IndexCalculationDuration;
            var matrix = spectra["ACI"]; // assume this key will always be present!!
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigFile);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            int tileWidth = tilingConfig.TileWidth;
            int superTileWidth = tilingConfig.SuperTileWidthDefault();
            int superTileCount = (int)Math.Ceiling(tilingConfig.SuperTileCount(dataDuration, imageScale.TotalSeconds));

            TimeSpan superTileDuration = TimeSpan.FromTicks(superTileWidth * imageScale.Ticks);

            // initialise the image array to return
            var imageArray = new Image[superTileCount];
            TimeSpan startTime = analysisConfig.MinuteOffset;   // default = zero minute of day i.e. midnight

            // start the loop
            for (int t = 0; t < superTileCount; t++)
            {
                Image image = ZoomTiledSpectrograms.DrawIndexSpectrogramAtScale(analysisConfig, indicesConfigFile, startTime, dataScale, imageScale, superTileWidth, spectra);
                imageArray[t] = image;
                startTime += superTileDuration;
                if (startTime > dataDuration) break;
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
            TimeSpan dataDuration = TimeSpan.FromSeconds(matrix.GetLength(1) * dataScale.TotalSeconds);

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
            var spectralSelection = new Dictionary<string, double[,]>();
            foreach (string key in spectra.Keys)
            {
                matrix = spectra[key];
                int rowCount = matrix.GetLength(0);
                spectralSelection[key] = MatrixTools.Submatrix(matrix, 0, startIndex, rowCount - 1, endIndex - 1);
            }

            // compress spectrograms to correct scale
            if (scalingFactor > 1)
                spectralSelection = ZoomFocusedSpectrograms.CompressIndexSpectrograms(spectralSelection, imageScale, dataScale);

            // var mergedSpectra = CombineSpectrogramsForScale(spectralSelection, imageScale, dataScale);

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
            string title = string.Format("ZOOM SCALE={0}s/pixel   Image duration={1} ",  //  (colour:R-G-B={2})
                                                                       imageScale.TotalSeconds, spectrogramDuration);
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


    }




    public class SpectrogramScalingConfig
    {
        public string UnitsOfTime { get; set; } // should be "seconds"
        public string ScaleUnits { get; set; }  // should be "SecondsPerPixel"
        public double IndexCalculationDuration { get; set; } //: 0.2,
        public double SpectralFrameDuration { get; set; } //"SpectralFrameDuration": 0.02,
        public int TileWidth { get; set; }                  //"TileWidth": 300,
        public int MaxTilesPerSuperTile { get; set; }       // should be about 12
        public double[] SpectralIndexScale { get; set; }
        public double[] SpectralFrameScale { get; set; }

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
}
