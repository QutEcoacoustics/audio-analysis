// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawLongDurationSpectrograms.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the DrawLongDurationSpectrograms type.
//
// Action code for this analysis = ColourSpectrogram
/// Activity Codes for other tasks to do with spectrograms and audio files:
/// 
/// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
/// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
/// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
/// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
/// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
/// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
///
/// audiofilecheck - Writes information about audio files to a csv file.
/// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
/// audiocutter - Cuts audio into segments of desired length and format
/// createfoursonograms 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.IO;

    using Acoustics.Shared;

    using AnalysisPrograms.Production;

    using AudioAnalysisTools.Indices;
    using AudioAnalysisTools.LongDurationSpectrograms;

    using PowerArgs;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Drawing;
    using TowseyLibrary;
    using AudioAnalysisTools;


    /// <summary>
    /// First argument on command line to call this action is "ColourSpectrogram"
    /// </summary>
    public static class DrawLongDurationSpectrograms
    {

        public class Arguments
        {
            [ArgDescription("Directory where the input data is located.")]
            public DirectoryInfo InputDataDirectory { get; set; }

            [ArgDescription("Directory where the output is to go.")]
            public DirectoryInfo OutputDirectory { get; set; }

            [ArgDescription("User specified file containing a list of indices and their properties.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            public FileInfo IndexPropertiesConfig { get; set; }

            [ArgDescription("Config file specifying directory containing indices.csv files and other parameters.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            public FileInfo SpectrogramConfigPath { get; set; }
        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "colourspectrogram"
        /// Activity Codes for other tasks to do with spectrograms and audio files:
        /// 
        /// audio2csv - Calls AnalyseLongRecording.Execute(): Outputs acoustic indices and LD false-colour spectrograms.
        /// audio2sonogram - Calls AnalysisPrograms.Audio2Sonogram.Main(): Produces a sonogram from an audio file - EITHER custom OR via SOX.Generates multiple spectrogram images and oscilllations info
        /// indicescsv2image - Calls DrawSummaryIndexTracks.Main(): Input csv file of summary indices. Outputs a tracks image.
        /// colourspectrogram - Calls DrawLongDurationSpectrograms.Execute():  Produces LD spectrograms from matrices of indices.
        /// zoomingspectrograms - Calls DrawZoomingSpectrograms.Execute():  Produces LD spectrograms on different time scales.
        /// differencespectrogram - Calls DifferenceSpectrogram.Execute():  Produces Long duration difference spectrograms
        ///
        /// audiofilecheck - Writes information about audio files to a csv file.
        /// snr - Calls SnrAnalysis.Execute():  Calculates signal to noise ratio.
        /// audiocutter - Cuts audio into segments of desired length and format
        /// createfoursonograms 
        /// </summary>
        /// <param name="arguments"></param>
        public static Arguments Dev()
        {
            // INPUT and OUTPUT DIRECTORIES
            //2010 Oct 13th
            //string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014May06-100720 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            //string ipFileName = "7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\7a667c05-825e-4870-bc4b-9cec98024f5a_101013-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct13_SpectralIndices";

            //2010 Oct 14th
            //string ipFileName = "b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\b562c8cd-86ba-479e-b499-423f5d68a847_101014-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct14_SpectralIndices";

            //2010 Oct 15th
            //string ipFileName = "d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\d9eb5507-3a52-4069-a6b3-d8ce0a084f17_101015-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct15_SpectralIndices";

            //2010 Oct 16th
            //string ipFileName = "418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\418b1c47-d001-4e6e-9dbe-5fe8c728a35d_101016-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct16_SpectralIndices";

            //2010 Oct 17th
            //string ipFileName = "0f2720f2-0caa-460a-8410-df24b9318814_101017-0000";
            //string ipdir = @"C:\SensorNetworks\Output\SERF\2014Apr24-020709 - Indices, OCT 2010, SERF\SERF\TaggedRecordings\SE\0f2720f2-0caa-460a-8410-df24b9318814_101017-0000.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\Test_04May2014\SERF_SE_2010Oct17_SpectralIndices";

            // exclude the analysis type from file name i.e. "Indices"
            //string ipFileName = "BYR4_20131029_Towsey.Acoustic";
            //string ipdir = @"Y:\Results\2014Nov28-083415 - False Color, Mt Byron PRA, For Jason\to upload\Mt Byron\PRA\report\joined\BYR4_20131029.mp3\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\Test\RibbonTest";

            // false-colour spectrograms
            //string ipFileName = "TEST_Farmstay_ECLIPSE3_20121114-060001+1000"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            string ipdir = @"C:\SensorNetworks\Output\Test\Test2\Towsey.Acoustic";
            string opdir = @"C:\SensorNetworks\Output\Test\Test2";

            // false-colour spectrograms
            //string ipFileName = "Farmstay_ECLIPSE3_20121114_060001TEST"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.60sppx.EclipseFarmstay";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string ipFileName = "Farmstay_ECLIPSE3_20121114-060001+1000_TEST"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.60sppx.EclipseFarmstay";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";

            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Farmstay_ECLIPSE3_20121114_060001TEST\Indices\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\Farmstay_ECLIPSE3_20121114_060001TEST\Spectrograms";

            // zoomable spectrograms
            //string ipFileName = "TEST_TUITCE_20091215_220004"; //exclude the analysis type from file name i.e. "Towsey.Acoustic.Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";


            DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            DirectoryInfo opDir = new DirectoryInfo(opdir);

            //FileInfo fiSpectrogramConfig = null;
            FileInfo fiSpectrogramConfig = new FileInfo(@"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\SpectrogramFalseColourConfig.yml");

            return new Arguments
            {
                InputDataDirectory = ipDir,
                OutputDirectory = opDir,
                // use the default set of index properties in the AnalysisConfig directory.
                IndexPropertiesConfig = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\IndexPropertiesConfig.yml".ToFileInfo(),
                SpectrogramConfigPath = fiSpectrogramConfig
            };
            throw new NoDeveloperMethodException();
    }

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
                    LoggedConsole.WriteLine();
                }
            }


            FileInfo indexGenerationDataFile;
            FileInfo indexDistributionsFile;
            ZoomCommonArguments.CheckForNeededFiles(arguments.InputDataDirectory, out indexGenerationDataFile, out indexDistributionsFile);
            var indexGenerationData = Json.Deserialise<IndexGenerationData>(indexGenerationDataFile);
            var indexDistributionsData = IndexDistributions.Deserialize(indexDistributionsFile);

            // this config can be found in IndexGenerationData. If config argument not specified, simply take it from icd file
            LdSpectrogramConfig config;
            if (arguments.SpectrogramConfigPath == null)
            {
                config = indexGenerationData.LongDurationSpectrogramConfig;
            }
            else
            {
                config = LdSpectrogramConfig.ReadYamlToConfig(arguments.SpectrogramConfigPath);
                //config = Yaml.Deserialise<SuperTilingConfig>(arguments.SpectrogramConfigPath).LdSpectrogramConfig;
            }

            string originalBaseName;
            string[] otherSegments;
            string analysisTag;
            FilenameHelpers.ParseAnalysisFileName(indexGenerationDataFile, out originalBaseName, out analysisTag, out otherSegments);

            //config.IndexCalculationDuration = TimeSpan.FromSeconds(1.0);
            //config.XAxisTicInterval = TimeSpan.FromSeconds(60.0);
            //config.IndexCalculationDuration = TimeSpan.FromSeconds(60.0);
            //config.XAxisTicInterval = TimeSpan.FromSeconds(3600.0);
            LDSpectrogramRGB.DrawSpectrogramsFromSpectralIndices(
                inputDirectory: arguments.InputDataDirectory,
                outputDirectory: arguments.OutputDirectory,
                ldSpectrogramConfig: config,
                indexPropertiesConfigPath: arguments.IndexPropertiesConfig,
                indexGenerationData: indexGenerationData,
                basename: originalBaseName,
                analysisType: Acoustic.TowseyAcoustic,
                indexSpectrograms: null,
                indexDistributions: indexDistributionsData,
                imageChrome: false.ToImageChrome());
        } // Execute()



        public static int DrawAggregatedSpectrograms(Arguments arguments, string fileStem)
        {
            int sampleRate = 22050;
            int frameWidth = 512;
            double spectrogramScale = 0.1;
            string analysisType = "Towsey.Acoustic";
            string[] keys = { "ACI", "POW", "BGN", "CVR", "ENT", "EVN", "RHZ", "RVT", "RPS", "RNG", "SPT" };

            LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
            LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
            DirectoryInfo inputDirectory  = arguments.InputDataDirectory;
            DirectoryInfo outputDirectory = arguments.OutputDirectory;
            TimeSpan dataScale = TimeSpan.FromSeconds(spectrogramScale);

            Dictionary<string, IndexProperties> indexProperties = IndexProperties.GetIndexProperties(arguments.IndexPropertiesConfig);



            var sw = Stopwatch.StartNew();
            //C:\SensorNetworks\Output\BIRD50\Training\ID0001\Towsey.Acoustic\ID0001__Towsey.Acoustic.ACI
            Dictionary<string, double[,]> spectra = IndexMatrices.ReadCSVFiles(inputDirectory, fileStem + "__" + analysisType, keys);

            var minuteOffset = TimeSpan.Zero;
            var xScale       = dataScale;
            string colorMap1 = null;

            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap1);

            cs1.FileName = fileStem;
            cs1.BackgroundFilter = 0.75;
            cs1.IndexCalculationDuration = dataScale;
            cs1.SetSpectralIndexProperties(indexProperties); // set the relevant dictionary of index properties

            cs1.spectrogramMatrices = spectra;
            //cs1.ReadCSVFiles(configuration.InputDirectoryInfo, fileStem); // reads all known files spectral indices
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("WARNING:  "+fileStem +":   No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return 0;
            }


            sw.Stop();
            LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds + " seconds");
            List<Image> list = new List<Image>();
            //Font stringFont = new Font("Tahoma", 9);
            Font stringFont = new Font("Arial", 14);
            int pixelWidth = 0;

            foreach (string key in keys)
            {
                Image image = cs1.DrawGreyscaleSpectrogramOfIndex(key);
                pixelWidth = image.Width;

                int width = 70;
                int height = image.Height;
                Image label = new Bitmap(width, height);
                Graphics g1 = Graphics.FromImage(label);
                g1.Clear(Color.Gray);
                g1.DrawString(key, stringFont, Brushes.Black, new PointF(4, 30));
                g1.DrawLine(new Pen(Color.Black), 0, 0, width, 0);//draw upper boundary
                g1.DrawLine(new Pen(Color.Black), 0, 1, width, 1);//draw upper boundary

                Image[] imagearray = { label, image };
                Image labelledImage = ImageTools.CombineImagesInLine(imagearray);
                list.Add(labelledImage);
            } //foreach key

            Image combinedImage = ImageTools.CombineImagesVertically(list.ToArray());
            string fileName = Path.Combine(outputDirectory.FullName, fileStem + ".CombinedGreyScale.png");
            combinedImage.Save(fileName);



            string colourMode = "NEGATIVE";
            string colourMap  = "BGN-POW-EVN";
            bool   withChrome = true;
            Image image1 = cs1.DrawFalseColourSpectrogram(colourMode, colourMap, withChrome);
            TimeSpan fullDuration = TimeSpan.FromSeconds(image1.Width * spectrogramScale);

            string title = fileStem;
            Image titleImage = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            int trackHeight = 20;
            Bitmap timeScale = Image_Track.DrawTimeRelativeTrack(fullDuration, image1.Width, trackHeight);

            colourMap = "RHZ-RVT-SPT";
            Image image2 = cs1.DrawFalseColourSpectrogram(colourMode, colourMap, withChrome);
            list = new List<Image>();
            list.Add(titleImage); 
            list.Add(image1);
            list.Add(timeScale); 
            list.Add(image2);

            combinedImage = ImageTools.CombineImagesVertically(list.ToArray());
            fileName = Path.Combine(outputDirectory.FullName, fileStem + ".TwoMaps.png");
            combinedImage.Save(fileName);


            return (int)(Math.Round(pixelWidth * spectrogramScale));
        } // method DrawAggregatedSpectrograms()

        public static int DrawRidgeSpectrograms(Arguments arguments, string fileStem)
        {
            LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
            LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
            DirectoryInfo inputDirectory = arguments.InputDataDirectory;
            DirectoryInfo outputDirectory = arguments.OutputDirectory;
            string analysisType = "Towsey.Acoustic";
            double spectrogramScale = 0.1;
            TimeSpan dataScale = TimeSpan.FromSeconds(spectrogramScale);

            Dictionary<string, IndexProperties> indexProperties = IndexProperties.GetIndexProperties(arguments.IndexPropertiesConfig);



            var sw = Stopwatch.StartNew();
            string[] keys = { "SPT", "RVT", "RHZ", "RPS", "RNG" };
            //C:\SensorNetworks\Output\BIRD50\Training\ID0001\Towsey.Acoustic\ID0001__Towsey.Acoustic.ACI

            // read the csv files of the indices in keys array
            Dictionary<string, double[,]> spectra = IndexMatrices.ReadCSVFiles(inputDirectory, fileStem + "__" + analysisType, keys);

            var minuteOffset = TimeSpan.Zero;
            var xScale = dataScale;
            string colorMap1 = null;
            int sampleRate = 22050;
            int frameWidth = 512;

            var cs1 = new LDSpectrogramRGB(minuteOffset, xScale, sampleRate, frameWidth, colorMap1);

            cs1.FileName = fileStem;
            cs1.BackgroundFilter = 0.75;
            cs1.IndexCalculationDuration = dataScale;
            cs1.SetSpectralIndexProperties(indexProperties); // set the relevant dictionary of index properties

            cs1.spectrogramMatrices = spectra;
            if (cs1.GetCountOfSpectrogramMatrices() == 0)
            {
                LoggedConsole.WriteLine("WARNING:  " + fileStem + ":   No spectrogram matrices in the dictionary. Spectrogram files do not exist?");
                return 0;
            }
            else if (cs1.GetCountOfSpectrogramMatrices() != keys.Length)
            {
                LoggedConsole.WriteLine("WARNING:  " + fileStem + ":   Missing indices in the dictionary. Some files do not exist?");
                return 0;
            }



            sw.Stop();
            LoggedConsole.WriteLine("Time to read spectral index files = " + sw.Elapsed.TotalSeconds + " seconds");
            //Font stringFont = new Font("Tahoma", 9);
            Font stringFont = new Font("Arial", 14);
            int pixelWidth = 0;

            // constants for labels 
            Brush[] brush = { Brushes.Blue, Brushes.LightGreen, Brushes.Red, Brushes.Orange, Brushes.Purple };
            Color[] color = { Color.Blue, Color.LightGreen, Color.Red, Color.Orange, Color.Purple }; 
            int labelWidth = 70;
            int labelYvalue = 0;
            int labelIndex = 0;
            Image label = null;
            Graphics g1 = null;
            Bitmap ridges = null;
            Graphics g2 = null;

            foreach (string key in keys)
            {
                Bitmap greyScaleImage = (Bitmap)cs1.DrawGreyscaleSpectrogramOfIndex(key);
                pixelWidth = greyScaleImage.Width;

                int height = greyScaleImage.Height;
                if (label == null)
                {
                    label = new Bitmap(labelWidth, height);
                    g1 = Graphics.FromImage(label);
                    g1.Clear(Color.Gray);

                    ridges = new Bitmap(pixelWidth, height);
                    g2 = Graphics.FromImage(ridges);
                    g2.Clear(Color.White);
                }
                labelYvalue += 30;
                g1.DrawString(key, stringFont, brush[labelIndex], new PointF(4, labelYvalue));
                //g1.DrawLine(new Pen(Color.Black), 0, 0, width, 0);//draw upper boundary
                //g1.DrawLine(new Pen(Color.Black), 0, 1, width, 1);//draw upper boundary




                // transfer greyscale image to colour image
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < pixelWidth; x++)
                    {
                        Color col = greyScaleImage.GetPixel(x, y);
                        if (col.G < 150)
                            ridges.SetPixel(x, y, color[labelIndex]);
                    }
                }

                labelIndex += 1;

            } //foreach key

            Image[] imagearray = { label, ridges };
            Image labelledImage = ImageTools.CombineImagesInLine(imagearray);
            string fileName = Path.Combine(outputDirectory.FullName, fileStem + ".Ridges.png");
            labelledImage.Save(fileName);



            //string colourMode = "NEGATIVE";
            //string colourMap = "BGN-POW-EVN";
            //bool withChrome = true;
            //Image image1 = cs1.DrawFalseColourSpectrogram(colourMode, colourMap, withChrome);
            //TimeSpan fullDuration = TimeSpan.FromSeconds(image1.Width * spectrogramScale);

            //string title = fileStem;
            //Image titleImage = LDSpectrogramRGB.DrawTitleBarOfFalseColourSpectrogram(title, image1.Width);
            //int trackHeight = 20;
            //Bitmap timeScale = Image_Track.DrawTimeRelativeTrack(fullDuration, image1.Width, trackHeight);

            return (int)(Math.Round(pixelWidth * spectrogramScale));
        } // method DrawRidgeSpectrograms()


    } // class DrawLongDurationSpectrograms
}
