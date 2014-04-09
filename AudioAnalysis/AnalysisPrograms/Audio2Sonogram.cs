using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acoustics.Tools;
using Acoustics.Tools.Audio;
using Acoustics.Shared;
using AnalysisBase;
using AnalysisRunner;
using AudioAnalysisTools;
using TowseyLibrary;


namespace AnalysisPrograms
{
    using System.Diagnostics;

    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;

    public class Audio2Sonogram
    {
        //use the following paths for the command line for the <Audio2Sonogram> task. 

        // audio2sonogram "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true

        //public const int DEFAULT_SAMPLE_RATE = 22050;

        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceAndConfigArguments
        {
            [ArgDescription("A file path to write output to")]
            [ArgNotExistingFile]
            [ArgRequired]
            public FileInfo Output { get; set; }

            public bool Verbose { get; set; }

            [ArgDescription("The start offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }


            public static string Description()
            {
                return "Does cool stuff";
            }

            public static string AdditionalNotes()
            {
                return "StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.StartOffset and EndOffset are both required when either is included.";
            }
        }

        private static Arguments Dev()
        {
            throw new NoDeveloperMethodException();
        }

        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            arguments.Output.CreateParentDirectories();

            if (arguments.StartOffset.HasValue ^ arguments.EndOffset.HasValue)
            {
                throw new InvalidStartOrEndException("If StartOffset or EndOffset is specifified, then both must be specified");
            }
            var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;


            /*// checks validity of the first 3 path arguments
            CheckArguments(args); 

            string recordingPath = args[0];
            string configPath    = args[1];
            string outputPath    = args[2];*/

            TimeSpan startOffsetMins = TimeSpan.Zero;
            TimeSpan endOffsetMins   = TimeSpan.Zero;

            if (offsetsProvided)
            {
                startOffsetMins = TimeSpan.FromMinutes(arguments.StartOffset.Value);
                endOffsetMins   = TimeSpan.FromMinutes(arguments.EndOffset.Value);
            }

            bool verbose = arguments.Verbose;
 
            if (verbose)
            {
                string title = "# MAKE A SONOGRAM FROM AUDIO RECORDING";
                string date  = "# DATE AND TIME: " + DateTime.Now;
                LoggedConsole.WriteLine(title);
                LoggedConsole.WriteLine(date);
                LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);
                LoggedConsole.WriteLine("# Output image file: " + arguments.Output);
            }
            
            

            //1. set up the necessary files
            DirectoryInfo diSource =  arguments.Source.Directory;
            FileInfo fiSourceRecording = arguments.Source;
            FileInfo fiConfig = arguments.Config;
            FileInfo fiImage  = arguments.Output;

            //2. get the config dictionary
            var configuration = new ConfigDictionary(fiConfig.FullName);
            Dictionary<string, string> configDict = configuration.GetTable();

            if (verbose)
            {
                LoggedConsole.WriteLine("\nPARAMETERS");
                foreach (KeyValuePair<string, string> kvp in configDict)
                {
                    LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
                }
            }

            //3: GET RECORDING
            FileInfo fiOutputSegment = fiSourceRecording;
            if (!((startOffsetMins == TimeSpan.Zero) && (endOffsetMins == TimeSpan.Zero)))
            {
                TimeSpan buffer = new TimeSpan(0, 0, 0);
                fiOutputSegment = new FileInfo(Path.Combine(arguments.Output.DirectoryName, "tempWavFile.wav"));
                AudioRecording.ExtractSegment(fiSourceRecording, startOffsetMins, endOffsetMins, buffer, configDict, fiOutputSegment);
            }

            //###### get sonogram image ##############################################################################################
            if ((configDict.ContainsKey(AnalysisKeys.MAKE_SOX_SONOGRAM)) && (ConfigDictionary.GetBoolean(AnalysisKeys.MAKE_SOX_SONOGRAM, configDict)))
            {
                SpectrogramTools.MakeSonogramWithSox(fiOutputSegment, configDict, fiImage);
            }
            else
            {
                using (Image image = SpectrogramTools.Audio2SonogramImage(fiOutputSegment, configDict))
                {
                    // TODO: remove eventually
                    Debug.Assert(image != null, "The image should not be null - there is no reason it can be");
                    if (fiImage.Exists) fiImage.Delete();
                    image.Save(fiImage.FullName, ImageFormat.Png);
                }
            }
            //###### get sonogram image ##############################################################################################

            if (verbose)
            {
                LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
            }
        }
    } //class Audio2Sonogram
}


