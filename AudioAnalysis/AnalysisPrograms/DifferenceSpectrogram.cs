using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


using Acoustics.Shared;
using AudioAnalysisTools;
using PowerArgs;
using TowseyLib;

namespace AnalysisPrograms
{
    using AnalysisPrograms.Production;

    public static class DifferenceSpectrogram
    {
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments
        {
            [ArgDescription("Path to configuration file in YAML format")]
            [Production.ArgExistingFile]
            [ArgRequired]
            public FileInfo Config { get; set; }

            //[ArgDescription("The directory containing the input files.")]
            //[Production.ArgExistingDirectory]
            //[ArgPosition(1)]
            //[ArgRequired]
            //public DirectoryInfo InputDirectory { get; set; }

            //[ArgDescription("The directory to place output files.")]
            //[ArgPosition(2)]
            //[ArgRequired]
            //public DirectoryInfo OutputDirectory { get; set; }

            //[ArgDescription("The first input csv file containing acosutic index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(3)]
            //[ArgRequired]
            //public FileInfo IndexFile1 { get; set; }

            //[ArgDescription("The third input csv file containing standard deviations of index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(4)]
            //[ArgRequired]
            //public FileInfo StdDevFile1 { get; set; }

            //[ArgDescription("The fourth input csv file containing acosutic index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(5)]
            //[ArgRequired]
            //public FileInfo IndexFile2 { get; set; }

            //[ArgDescription("The fifth input csv file containing standard deviations of index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(6)]
            //[ArgRequired]
            //public FileInfo StdDevFile2 { get; set; }

            public static string Description()
            {
                return "Long duration difference spectrograms";
            }

            public static string AdditionalNotes()
            {
                // add long explantory notes here if you need to
                return "";
            }
        }



        public static Arguments Dev()
        {
            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart.ToString());

            string ipdir = @"C:\SensorNetworks\Output\SERF\2013MonthlyAveraged"; // SERF
            string ipFileName1 = "April.monthAv";
            string ipFileName2 = "June.monthAv";
            string ipSdFileName1 = "April.monthSd";
            string ipSdFileName2 = "June.monthSd";

            //string ipdir = @"C:\SensorNetworks\Output\TestSpectrograms";
            //string ipFileName = @"Test24hSpectrogram";


            // OUTPUT FILES
            string opdir = @"C:\SensorNetworks\Output\DifferenceSpectrograms\2014March20";


            // WRITE THE YAML CONFIG FILE
            string configPath = Path.Combine(opdir, "differenceSpectrogramConfig.yml");
            var cfgFile = new FileInfo(configPath);
            Yaml.Serialise(cfgFile, new
            { 
                //paths to required directories and files
                InputDirectory = ipdir,
                IndexFile1 = ipFileName1, 
                StdDevFile1 = ipSdFileName1,

                IndexFile2 = ipFileName2,
                StdDevFile2 = ipSdFileName2,
                OutputDirectory = opdir,

                //these parameters manipulate the colour map and appearance of the false-colour spectrogram
                ColorMap = SpectrogramConstants.RGBMap_ACI_TEN_CVR,                   // CHANGE RGB mapping here.
                BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF, // must be value <=1.0
                ColourGain = SpectrogramConstants.COLOUR_GAIN,                        // determines colour saturation of the difference spectrogram

                // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
                SampleRate = SpectrogramConstants.SAMPLE_RATE,       // default value - after resampling
                FrameWidth = SpectrogramConstants.FRAME_WIDTH,       // frame width from which spectrogram was derived. Assume no frame overlap.
                MinuteOffset = SpectrogramConstants.MINUTE_OFFSET,   // default is recording starts at zero minute of day i.e. midnight
                X_Scale = SpectrogramConstants.X_AXIS_SCALE,         // default is one minute spectra and hourly time lines
            });


            //SET UP THE ARGUMENTS CLASS containing path to the YAML config file
            var arguments = new Arguments
            {
                Config = configPath.ToFileInfo(),
            };
            return arguments;
        }

        public static void Execute(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            // load YAML configuration
            dynamic configuration = Yaml.Deserialise(arguments.Config);
            /*
             * Warning! The `configuration` variable is dynamic.
             * Do not use it outside of this method. Extract all params below.
             */

            LDSpectrogramDistance.DrawDistanceSpectrogram(configuration);

            LDSpectrogramDifference.DrawDifferenceSpectrogram(configuration);

            LDSpectrogramTStatistic.DrawTStatisticThresholdedDifferenceSpectrograms(configuration);

        }
    }
}
