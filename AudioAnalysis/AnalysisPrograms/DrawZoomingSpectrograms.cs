// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawZoomingSpectrograms.cs" company="QutEcoacoustics">
//   All code in this file and all associated files are the copyright of the QUT Ecoacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//
// Action code for this analysis = ZoomingSpectrograms
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.IO;

    using AudioAnalysisTools.LongDurationSpectrograms;

    using PowerArgs;


    /// <summary>
    /// First argument on command line to call this action is "ZoomingSpectrograms"
    /// </summary>
    public static class DrawZoomingSpectrograms
    {

        public class Arguments
        {
            [ArgDescription("User specified file containing a list of indices and their properties.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            public FileInfo IndexPropertiesConfig { get; set; }


            [ArgDescription("Config file specifing directory containing indices.csv files and other parameters.")]
            [Production.ArgExistingFile(Extension = ".yml")]
            //[ArgPosition(1)]
            public FileInfo SpectrogramConfigPath { get; set; }
        }

        /// <summary>
        /// To get to this DEV method, the FIRST AND ONLY command line argument must be "colourspectrogram"
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

            // zoomable spectrograms
            string ipFileName = "TEST_TUITCE_20091215_220004_Towsey.Acoustic"; //exclude the analysis type from file name i.e. "Indices"
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic";
            //string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.OneSecondIndices";
            string ipdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\Towsey.Acoustic.200msIndices";
            string opdir = @"C:\SensorNetworks\Output\FalseColourSpectrograms\SpectrogramZoom\ZoomImages";

            DirectoryInfo ipDir = new DirectoryInfo(ipdir);
            DirectoryInfo opDir = new DirectoryInfo(opdir);

            //Write the default Yaml Config file for producing long duration spectrograms and place in the op directory
            var config = new LdSpectrogramConfig(ipFileName, ipDir, opDir); // default values have been set
            // need to set the data scale. THis info not available at present
            config.FrameStep = 441;
            config.IndexCalculationDuration = TimeSpan.FromSeconds(0.2);
            //config.XAxisTicInterval = TimeSpan.FromSeconds(60.0);


            FileInfo fiSpectrogramConfig = new FileInfo(Path.Combine(opDir.FullName, "LDSpectrogramConfig.yml"));
            config.WriteConfigToYaml(fiSpectrogramConfig);

            // USE THE Towsey.Acoustic file that contains parameters for the analysis.
            //FileInfo fiSpectrogramConfig = new FileInfo(Path.Combine(opDir.FullName, "Towsey.Acoustic.yml"));

            return new Arguments
            {
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
                    LoggedConsole.WriteLine("# DRAW ZOOMING SPECTROGRAMS DERIVED FROM CSV FILES OF SPECTRAL INDICES OBTAINED FROM AN AUDIO RECORDING");
                    LoggedConsole.WriteLine(date);
                    LoggedConsole.WriteLine("# Spectrogram Config      file: " + arguments.SpectrogramConfigPath);
                    LoggedConsole.WriteLine("# Index Properties Config file: " + arguments.IndexPropertiesConfig);
                    LoggedConsole.WriteLine("");
                }

            }

            string fileStem = "TEST_TUITCE_20091215_220004";
            var config = LdSpectrogramConfig.ReadYamlToConfig(arguments.SpectrogramConfigPath);
            TimeSpan focalTime = TimeSpan.FromMinutes(16);
            int imageWidth = 1500;

            ZoomingSpectrograms.DrawSpectrogramsFromSpectralIndices(config, arguments.IndexPropertiesConfig, focalTime, imageWidth);
        }




    }
}
