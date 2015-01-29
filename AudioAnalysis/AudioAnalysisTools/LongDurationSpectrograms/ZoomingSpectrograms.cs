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


namespace AudioAnalysisTools.LongDurationSpectrograms
{
    public static class ZoomingSpectrograms
    {


        public static void DrawSpectrogramsFromSpectralIndices(LdSpectrogramConfig longDurationSpectrogramConfig, FileInfo indicesConfigPath, Dictionary<string, double[,]> spectra = null)
        {
            LdSpectrogramConfig config = longDurationSpectrogramConfig;

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);

            // THREE BASIC PARAMETERS
            TimeSpan focalTime = TimeSpan.FromMinutes(16);
            int imageWidth = 1600;
            TimeSpan xScale = config.IndexCalculationDuration;
            TimeSpan ImageDuration = TimeSpan.FromTicks(imageWidth * xScale.Ticks);
            TimeSpan halfImageDuration = TimeSpan.FromTicks(imageWidth * xScale.Ticks / 2);
            TimeSpan startTime = focalTime - halfImageDuration;
            TimeSpan endTime   = focalTime + halfImageDuration;
            Image imageDummy = new Bitmap(imageWidth, 256);
            Graphics g = Graphics.FromImage(imageDummy);
            Pen pen = new Pen(Color.Red);
            int x1 = imageWidth / 2;
            g.DrawLine(pen, x1, 0, x1, imageDummy.Height);
            int nyquist = 22050 / 2;
            int herzInterval = 1000;
            string title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", "DUMMY", SpectrogramConstants.RGBMap_ACI_ENT_EVN);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, imageDummy.Width);
            imageDummy = LDSpectrogramRGB.FrameLDSpectrogram(imageDummy, titleBar, startTime, xScale, config.XAxisTicInterval, nyquist, herzInterval);
            imageDummy.Save(Path.Combine(config.OutputDirectoryInfo.FullName, "ZOOM.png"));


            string fileStem = config.FileName;
            DirectoryInfo outputDirectory = config.OutputDirectoryInfo;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string colorMap1 = config.ColourMap1 ?? SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
            string colorMap2 = config.ColourMap2 ?? SpectrogramConstants.RGBMap_ACI_ENT_EVN;   // assigns indices to RGB

            double backgroundFilterCoeff = (double?)config.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            var cs1 = new LDSpectrogramRGB(config, colorMap1);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties

            if (spectra == null)
            {
                // reads all known files spectral indices
                //Logger.Info("Reading spectra files from disk");
                cs1.ReadCSVFiles(config.InputDirectoryInfo, fileStem);
            }
            else
            {
                // TODO: not sure if this works
                //Logger.Info("Spectra loaded from memory");
                cs1.LoadSpectrogramDictionary(spectra);
            }

            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }

            //cs1.DrawGreyScaleSpectrograms(outputDirectory, fileStem);

            //cs1.CalculateStatisticsForAllIndices();
            //Json.Serialise(Path.Combine(outputDirectory.FullName, fileStem + ".IndexStatistics.json").ToFileInfo(), cs1.indexStats);


            //cs1.DrawIndexDistributionsAndSave(Path.Combine(outputDirectory.FullName, fileStem + ".IndexDistributions.png"));

            //string colorMap = colorMap1;
            //Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            //string title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            //Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);

            TimeSpan minuteOffset = config.MinuteOffset;   // default = zero minute of day i.e. midnight
            //int nyquist = cs1.SampleRate / 2;
            //int herzInterval = 1000;
            //image1 = LDSpectrogramRGB.FrameLDSpectrogram(image1, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);

            //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
            //colorMap = colorMap2;
            //Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            //title = string.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            //titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            //image2 = LDSpectrogramRGB.FrameLDSpectrogram(image2, titleBar, minuteOffset, cs1.IndexCalculationDuration, cs1.XTicInterval, nyquist, herzInterval);
            //image2.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap + ".png"));

            // read high amplitude and clipping info into an image
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + ".csv");
            //string indicesFile = Path.Combine(config.InputDirectoryInfo.FullName, fileStem + ".Indices.csv");
            //string indicesFile = Path.Combine(configuration.InputDirectoryInfo.FullName, fileStem + "_" + configuration.AnalysisType + ".csv");

            //Image imageX = DrawSummaryIndices.DrawHighAmplitudeClippingTrack(indicesFile.ToFileInfo());
            //if (null != imageX)
            //    imageX.Save(Path.Combine(outputDirectory.FullName, fileStem + ".ClipHiAmpl.png"));

            //var imageList = new List<Image>();
            //imageList.Add(image1);
            //imageList.Add(imageX);
            //imageList.Add(image2);
            //Image image3 = ImageTools.CombineImagesVertically(imageList);
            //image3.Save(Path.Combine(outputDirectory.FullName, fileStem + ".2MAPS.png"));

            //Image ribbon;
            //// ribbon = cs1.GetSummaryIndexRibbon(colorMap1);
            //ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap1);
            //ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap1 + ".SummaryRibbon.png"));
            //// ribbon = cs1.GetSummaryIndexRibbon(colorMap2);
            //ribbon = cs1.GetSummaryIndexRibbonWeighted(colorMap2);
            //ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap2 + ".SummaryRibbon.png"));

            //ribbon = cs1.GetSpectrogramRibbon(colorMap1, 32);
            //ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap1 + ".SpectralRibbon.png"));
            //ribbon = cs1.GetSpectrogramRibbon(colorMap2, 32);
            //ribbon.Save(Path.Combine(outputDirectory.FullName, fileStem + "." + colorMap2 + ".SpectralRibbon.png"));
        }





    }
}
