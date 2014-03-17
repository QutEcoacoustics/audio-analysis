using Acoustics.Shared;
using AudioAnalysisTools;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            [ArgDescription("The path to the config file")]
            [Production.ArgExistingFile]
            [ArgRequired]
            public FileInfo Config { get; set; }

            [ArgDescription("The directory containing the input files.")]
            [Production.ArgExistingDirectory]
            [ArgPosition(1)]
            [ArgRequired]
            public DirectoryInfo InputDirectory { get; set; }

            [ArgDescription("The directory to place output files.")]
            [ArgPosition(2)]
            [ArgRequired]
            public DirectoryInfo OutputDirectory { get; set; }

            [ArgDescription("The first input csv file containing acosutic index data 1.")]
            [Production.ArgExistingFile]
            [ArgPosition(3)]
            [ArgRequired]
            public FileInfo IndexFile1 { get; set; }

            [ArgDescription("The third input csv file containing standard deviations of index data 1.")]
            [Production.ArgExistingFile]
            [ArgPosition(4)]
            [ArgRequired]
            public FileInfo StdDevFile1 { get; set; }

            [ArgDescription("The fourth input csv file containing acosutic index data 2.")]
            [Production.ArgExistingFile]
            [ArgPosition(5)]
            [ArgRequired]
            public FileInfo IndexFile2 { get; set; }

            [ArgDescription("The fifth input csv file containing standard deviations of index data 2.")]
            [Production.ArgExistingFile]
            [ArgPosition(6)]
            [ArgRequired]
            public FileInfo StdDevFile2 { get; set; }
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
            string opdir = @"C:\SensorNetworks\Output\DifferenceSpectrograms\2014March17";


            // WRITE THE YAML CONFIG FILE
            string configPath = Path.Combine(opdir, "differenceSpectrogramConfig.yml");
            var cfgFile = new FileInfo(configPath);
            Yaml.Serialise(cfgFile, new
            { 
                InputDirectory = ipdir,
                IndexFile1 = ipFileName1, 
                StdDevFile1 = ipSdFileName1,
                IndexFile2 = ipFileName2,
                StdDevFile2 = ipSdFileName2,
                OutputDirectory = opdir
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
            string inputDirectory = configuration.InputDirectory;
            string indexFile1 = configuration.IndexFile1;
            string stdDevFile1 =  configuration.StdDevFile1;
            string indexFile2 =  configuration.IndexFile2;
            string stdDevFile2 =  configuration.StdDevFile2;
            string outputDirectory =  configuration.OutputDirectory;

            // Load arguments class with additional info in the YAML config file
            /* Ant: FYI the following is a bad duplication of code
             * Either have the options in the config file or the arguments, not both...
             */
            arguments.InputDirectory = new DirectoryInfo(inputDirectory);
            arguments.IndexFile1 = new FileInfo(indexFile1);
            arguments.StdDevFile1 = new FileInfo(stdDevFile1);
            arguments.IndexFile2 = new FileInfo(indexFile2);
            arguments.StdDevFile2 = new FileInfo(stdDevFile2);
            arguments.OutputDirectory = new DirectoryInfo(outputDirectory);

            LDSpectrogramDistance.DrawDistanceSpectrogram(arguments.InputDirectory,
                                     arguments.IndexFile1, arguments.IndexFile2, arguments.OutputDirectory);

            LDSpectrogramDifference.DrawDifferenceSpectrogram(arguments.InputDirectory,
                                    arguments.IndexFile1, arguments.IndexFile2, arguments.OutputDirectory);

            LDSpectrogramDifference.DrawTStatisticThresholdedDifferenceSpectrograms(arguments.InputDirectory,
                                    arguments.IndexFile1, arguments.StdDevFile1, arguments.IndexFile2, arguments.StdDevFile2,
                                    arguments.OutputDirectory);

        }
    }
}
