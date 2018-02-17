// <copyright file="DifferenceSpectrogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;

    using AudioAnalysisTools.LongDurationSpectrograms;
    using McMaster.Extensions.CommandLineUtils;
    using Production;
    using Production.Arguments;
    using Production.Validation;
    using TowseyLibrary;

    public static class DifferenceSpectrogram
    {
        public const string CommandName = "DrawDifferenceSpectrogram";

        [Command(
            CommandName,
            Description = "[INOPERABLE] Produces a false-colour spectrogram that show only the differences between two spectrograms.")]
        public class Arguments : SubCommandBase
        {
            [Option(Description = "Path to configuration file in YAML format")]
            [ExistingFile]
            [Required]
            [LegalFilePath]
            public string Config { get; set; }

            //[ArgDescription("The directory containing the input files.")]
            //[Production.ArgExistingDirectory]
            //[ArgPosition(1)]
            //[ArgRequired]
            //public string InputDirectory { get; set; }

            //[ArgDescription("The directory to place output files.")]
            //[ArgPosition(2)]
            //[ArgRequired]
            //public string OutputDirectory { get; set; }

            //[ArgDescription("The first input csv file containing acosutic index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(3)]
            //[ArgRequired]
            //public string IndexFile1 { get; set; }

            //[ArgDescription("The third input csv file containing standard deviations of index data 1.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(4)]
            //[ArgRequired]
            //public string StdDevFile1 { get; set; }

            //[ArgDescription("The fourth input csv file containing acosutic index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(5)]
            //[ArgRequired]
            //public string IndexFile2 { get; set; }

            //[ArgDescription("The fifth input csv file containing standard deviations of index data 2.")]
            //[Production.ArgExistingFile]
            //[ArgPosition(6)]
            //[ArgRequired]
            //public string StdDevFile2 { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                throw new NotImplementedException("The arguments for this class need to be fixed");
                DifferenceSpectrogram.Execute(this);
                return this.Ok();
            }
        }

        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        public static Arguments Dev()
        {
            //SET VERBOSITY
            DateTime tStart = DateTime.Now;
            Log.Verbosity = 1;
            Log.WriteLine("# Start Time = " + tStart);

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
            Yaml.Serialize(cfgFile, new
            {
                //paths to required directories and files
                InputDirectory = ipdir,
                IndexFile1 = ipFileName1,
                StdDevFile1 = ipSdFileName1,
                IndexFile2 = ipFileName2,
                StdDevFile2 = ipSdFileName2,
                OutputDirectory = opdir,

                //these parameters manipulate the colour map and appearance of the false-colour spectrogram
                ColorMap = SpectrogramConstants.RGBMap_ACI_ENT_CVR, // CHANGE RGB mapping here.
                BackgroundFilterCoeff = SpectrogramConstants.BACKGROUND_FILTER_COEFF, // must be value <=1.0
                ColourGain = 2.0, // determines colour saturation of the difference spectrogram

                // These parameters describe the frequency and times scales for drawing X and Y axes on the spectrograms
                SampleRate = SpectrogramConstants.SAMPLE_RATE,       // default value - after resampling
                FrameWidth = SpectrogramConstants.FRAME_LENGTH,       // frame width from which spectrogram was derived. Assume no frame overlap.
                MinuteOffset = SpectrogramConstants.MINUTE_OFFSET,   // default is recording starts at zero minute of day i.e. midnight
                X_Scale = SpectrogramConstants.X_AXIS_TIC_INTERVAL,         // default is one minute spectra and hourly time lines
            });

            //SET UP THE ARGUMENTS CLASS containing path to the YAML config file
            var arguments = new Arguments
            {
                Config = configPath,
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
            Config configuration = ConfigFile.Deserialize(arguments.Config);

            LDSpectrogramDistance.DrawDistanceSpectrogram(configuration);

            LdSpectrogramDifference.DrawDifferenceSpectrogram(configuration);

            LdSpectrogramTStatistic.DrawTStatisticThresholdedDifferenceSpectrograms(configuration);
        }
    }
}
