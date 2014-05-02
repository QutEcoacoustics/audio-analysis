using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using AudioAnalysisTools;
using TowseyLibrary;
using AcousticIndicesJie;
using AudioAnalysisTools.LongDurationSpectrograms;
using AudioAnalysisTools.Indices;


namespace QutBioacosutics.Xie.LDSpectrograms
{
    public static class DrawLDSpectrogram
    {
        public class Arguments
        {
            // User specified file containing a list of indices and their properties.
            public FileInfo IndexPropertiesConfig { get; set; }

            //Config file specifing directory containing indices.csv files and other parameters.
            public FileInfo SpectrogramConfigPath { get; set; }
        }


        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES
            //2010 Oct 13th
            //string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct13_SpectralIndices";

            //2010 Oct 14th
            //string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct14_SpectralIndices";

            //2010 Oct 15th
            //string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct15_SpectralIndices";

            //2010 Oct 16th
            //string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_30April2014\SERF_SE_2010Oct16_SpectralIndices";

            //2010 Oct 17th
            string ipFileName = "020313";
            string ipdir = @"C:\JCU\Campus\MyResults_Campus_020313"; // this is where ALL you indices csv files are located
            string opdir = @"C:\JCU\Campus\MyResults_Campus_020313";


            DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            DirectoryInfo opDir = new DirectoryInfo(opdir);

            //Write the default Yaml Config file for producing long duration spectrograms and place in the op directory
            var config = new LDSpectrogramConfig(ipFileName, ipDir, opDir); // default values have been set

            //config.ColourMap = "TRK-OSC-HAR";
            config.ColourMap1 = "ACI-ENT-CVR";
            config.ColourMap2 = "OSC-ENG-TRK";
            config.MinuteOffset = 19 * 60;
            config.FrameWidth = 256;
            //config.SampleRate = 17640;
            config.SampleRate = 22050;
            FileInfo outPath = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            config.WritConfigToYAML(outPath);


            FileInfo fiSpectrogramConfig = new FileInfo(Path.Combine(opDir.FullName, "FrogSpectrogramConfig.yml"));
            config.WritConfigToYAML(fiSpectrogramConfig);


            return new Arguments
            {
                // use the default set of index properties in the AnalysisConfig directory.
                IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfigForFrogs.yml".ToFileInfo(),
                SpectrogramConfigPath = fiSpectrogramConfig
            };
            throw new Exception();
        } //Dev()

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
                bool verbose = true; // assume verbose if in dev mode
                if (verbose)
                {
                    string date = "# DATE AND TIME: " + DateTime.Now;
                    LoggedConsole.WriteLine("# DRAW LONG DURATION SPECTROGRAMS DERIVED FROM CSV FILES OF SPECTRAL INDICES OBTAINED FROM AN AUDIO RECORDING");
                    LoggedConsole.WriteLine(date);
                    LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
                    LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
                    LoggedConsole.WriteLine("");
                }

            } // if (arguments == null)

            DrawLDSpectrogram.DrawSpectrogramsFromSpectralIndices(arguments.SpectrogramConfigPath, arguments.IndexPropertiesConfig);
        }



        /// <summary>
        ///  This IS THE MAJOR STATIC METHOD FOR CREATING LD SPECTROGRAMS 
        /// </summary>
        /// <param name="configuration"></param>
        public static void DrawSpectrogramsFromSpectralIndices(FileInfo spectrogramConfigPath, FileInfo indicesConfigPath)
        {
            //LDSpectrogramConfig configuration = Yaml.Deserialise<LDSpectrogramConfig>(configPath);
            LDSpectrogramConfig configuration = LDSpectrogramConfig.ReadYAMLToConfig(spectrogramConfigPath);

            Dictionary<string, IndexProperties> dictIP = IndexProperties.GetIndexProperties(indicesConfigPath);
            dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties(dictIP);
            //var dictIP = InitialiseIndexProperties.GetDictionaryOfSpectralIndexProperties();

            string fileStem = configuration.FileName;
            DirectoryInfo opDir = configuration.OutputDirectory;

            // These parameters manipulate the colour map and appearance of the false-colour spectrogram
            string map1 = configuration.ColourMap1;
            string colorMap1 = map1 != null ? map1 : SpectrogramConstants.RGBMap_BGN_AVG_CVR;   // assigns indices to RGB
            string map2 = configuration.ColourMap2;
            string colorMap2 = map2 != null ? map2 : SpectrogramConstants.RGBMap_ACI_ENT_CVR;   // assigns indices to RGB

            double backgroundFilterCoeff = (double?)configuration.BackgroundFilterCoeff ?? SpectrogramConstants.BACKGROUND_FILTER_COEFF;
            //double  colourGain = (double?)configuration.ColourGain ?? SpectrogramConstants.COLOUR_GAIN;  // determines colour saturation

            // These parameters describe the frequency and time scales for drawing the X and Y axes on the spectrograms
            int minuteOffset = (int?)configuration.MinuteOffset ?? SpectrogramConstants.MINUTE_OFFSET;   // default = zero minute of day i.e. midnight
            int xScale = (int?)configuration.X_interval ?? SpectrogramConstants.X_AXIS_SCALE; // default is one minute spectra i.e. 60 per hour
            int sampleRate = (int?)configuration.SampleRate ?? SpectrogramConstants.SAMPLE_RATE;
            int frameWidth = (int?)configuration.FrameWidth ?? SpectrogramConstants.FRAME_WIDTH;


            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap1);
            cs1.FileName = fileStem;
            cs1.BackgroundFilter = backgroundFilterCoeff;
            cs1.SetSpectralIndexProperties(dictIP); // set the relevant dictionary of index properties
            cs1.ReadCSVFiles(configuration.InputDirectory, fileStem); // reads all known files spectral indices
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

            string colorMap = colorMap1;
            Image image1 = cs1.DrawFalseColourSpectrogram("NEGATIVE", colorMap);
            string title = String.Format("FALSE-COLOUR SPECTROGRAM: {0}      (scale:hours x kHz)       (colour: R-G-B={1})", fileStem, colorMap);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            image1 = LDSpectrogramRGB.FrameSpectrogram(image1, titleBar, minuteOffset, cs1.X_interval, cs1.Y_interval);
            image1.Save(Path.Combine(opDir.FullName, fileStem + "." + colorMap + ".png"));

            //colorMap = SpectrogramConstants.RGBMap_ACI_ENT_SPT; //this has also been good
            colorMap = colorMap2;
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




        public static void GetJiesLDSpectrogramConfig(string fileName, DirectoryInfo ipDir, DirectoryInfo opDir)
        {
            int startMinute = 19 * 60; // 7pm frogs only call at night!!
            LDSpectrogramConfig spgConfig = new LDSpectrogramConfig(fileName, ipDir, opDir);
            //spgConfig.ColourMap = "TRK-OSC-HAR";
            spgConfig.ColourMap1 = "OSC-HAR-TRK";
            //spgConfig.ColourMap2 = "OSC-HAR-TRK";
            spgConfig.MinuteOffset = startMinute;
            spgConfig.FrameWidth = 256;
            //spgConfig.SampleRate = 17640;
            spgConfig.SampleRate = 22050;
            FileInfo path = new FileInfo(Path.Combine(opDir.FullName, "JiesLDSpectrogramConfig.yml"));
            spgConfig.WritConfigToYAML(path);
        }



    }
}
