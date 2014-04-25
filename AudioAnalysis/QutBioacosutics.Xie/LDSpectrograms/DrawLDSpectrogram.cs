using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using AudioAnalysisTools;
using TowseyLibrary;
using AcousticIndicesJie;


namespace QutBioacosutics.Xie.LDSpectrograms
{
    public static class DrawLDSpectrogram
    {

        public static void GetJiesLDSpectrogramConfig(string fileName, DirectoryInfo ipDir, DirectoryInfo opDir)
        {
            int startMinute = 19 * 60; // 7pm frogs only call at night!!
            LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            //spgConfig.ColourMap = "TRC-OSC-HAR";
            spgConfig.ColourMap1 = "OSC-HAR-TRC";
            //spgConfig.ColourMap2 = "OSC-HAR-TRC";
            spgConfig.MinuteOffset = startMinute;
            spgConfig.FrameWidth = 256;
            //spgConfig.SampleRate = 17640;
            spgConfig.SampleRate = 22050;
            FileInfo path = new FileInfo(Path.Combine(opDir.FullName, "JiesLDSpectrogramConfig.yml"));
            spgConfig.WritConfigToYAML(path);
        }



        /// <summary>
        ///  This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        ///  IT CAN BE COPIED AND APPROPRIATELY MODIFIED BY ANY USER FOR THEIR OWN PURPOSE. 
        /// </summary>
        /// <param name="configuration"></param>
        public static void DrawFalseColourSpectrograms(LDSpectrogramConfig configuration)
        {
            string ipdir = configuration.InputDirectory.FullName;
            DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            string fileStem = configuration.FileName;
            string opdir = configuration.OutputDirectory.FullName;
            DirectoryInfo opDir = new DirectoryInfo(opdir);

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map = configuration.ColourMap1;
            string colorMap = map != null ? map : SpectrogramConstantsJie.RGBMap_ACI_ENT_CVR;   // assigns indices to RGB

            double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstantsJie.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            int minuteOffset = (int?)configuration.MinuteOffset ?? SpectrogramConstantsJie.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            int xScale = (int?)configuration.X_interval ?? SpectrogramConstantsJie.X_AXIS_SCALE; // default is one minute spectra i.e. 60 per hour
            int sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstantsJie.SAMPLE_RATE;
            int frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstantsJie.FRAME_WIDTH;


            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            var sip = InitialiseJiesIndexProperties.GetDictionaryOfSpectralIndexProperties();
            cs1.SetSpectralIndexProperties(sip); // set the relevant dictionary of index properties
            cs1.ReadCSVFiles(ipDir, fileStem); // reads all known files spectral indices
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                Console.WriteLine("No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return;
            }

            cs1.DrawGreyScaleSpectrograms(opDir, fileStem);

            cs1.CalculateStatisticsForAllIndices();
            List<string> lines = cs1.WriteStatisticsForAllIndices();
            FileTools.WriteTextFile(Path.Combine(opDir.FullName, fileStem + ".IndexStatistics.txt"), lines);

            cs1.DrawIndexDistributionsAndSave(Path.Combine(opDir.FullName, fileStem + ".IndexDistributions.png"));

            colorMap = SpectrogramConstantsJie.RGBMap_BGN_AVG_CVR;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image1.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));

            //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
            colorMap = SpectrogramConstantsJie.RGBMap_ACI_ENT_EVN;
            Image image2 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image2.Width);
            image2 = LDSpectrogramRGB.FrameSpectrogram(image2, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image2.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));
            Image[] array = new Image[2];
            array[0] = image1;
            array[1] = image2;
            Image image3 = ImageTools.CombineImagesVertically(array);
            image3.Save(Path.Combine(opDir.FullName, fileStem + ".2MAPS.png"));
        }




    }
}
