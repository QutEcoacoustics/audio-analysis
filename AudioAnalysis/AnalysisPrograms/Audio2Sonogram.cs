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
using AudioAnalysisTools.WavTools;
using TowseyLibrary;


namespace AnalysisPrograms
{
    using System.Diagnostics;

    using Acoustics.Shared.Extensions;

    using AnalysisPrograms.Production;

    using PowerArgs;
    using AudioAnalysisTools.StandardSpectrograms;

    public class Audio2Sonogram
    {
        //use the following paths for the command line for the <Audio2Sonogram> task. 
        // audio2sonogram "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true


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

            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png".ToFileInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),
                StartOffset = 0,
                EndOffset = 0,
                Verbose = true
            };

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

            // set default offsets - only use defaults if not provided in argments list
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
            dynamic configuration = Yaml.Deserialise(fiConfig);

            //below three lines are examples of retrieving info from dynamic config
            //string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            //bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            //scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            //3 transfer conogram parameters to a dictionary to be passed around
            var configDict = new Dictionary<string, string>();
            configDict["FrameLength"] = configuration[AnalysisKeys.FrameLength] ?? 512;
            // #Frame Overlap as fraction: default=0.0 
            configDict["FrameOverlap"] = configuration[AnalysisKeys.FrameOverlap] ?? 0.0;
            // #Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            configDict["ResampleRate"] = configuration[AnalysisKeys.ResampleRate] ?? 17640;
            // #MinHz: 500
            // #MaxHz: 3500
            // #NOISE REDUCTION PARAMETERS
            configDict["DoNoiseReduction"] = configuration["DoNoiseReduction"] ?? true;
            configDict["BgNoiseThreshold"] = configuration["BgNoiseThreshold"] ?? 3.0;

            configDict["ADD_AXES"] = configuration["ADD_AXES"] ?? true;
            configDict["AddSegmentationTrack"] = configuration["AddSegmentationTrack"] ?? true;

            // # REDUCTION FACTORS for freq and time dimensions
            // #TimeReductionFactor: 1          
            // #FreqReductionFactor: 1

            configDict["MakeSoxSonogram"] = (string)configuration["MakeSoxSonogram"] ?? "false";
            configDict["SonogramTitle"]   = (string)configuration["SonogramTitle"] ?? "Sonogram";
            configDict["SonogramComment"] = (string)configuration["SonogramComment"] ?? "Sonogram produced using SOX";
            configDict["SonogramColored"] = (string)configuration["SonogramColored"] ?? "false";
            configDict["SonogramQuantisation"] = (string)configuration["SonogramQuantisation"] ?? "128";


            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes]           ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";


            // print out the sonogram parameters
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
            if ((configDict.ContainsKey(AnalysisKeys.MakeSoxSonogram)) && (ConfigDictionary.GetBoolean(AnalysisKeys.MakeSoxSonogram, configDict)))
            {
                SpectrogramTools.MakeSonogramWithSox(fiOutputSegment, configDict, fiImage);
            }
            else
            {
                BaseSonogram sonogram = SpectrogramTools.Audio2Sonogram(fiOutputSegment, configDict);
                var mti = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
                var image = mti.GetImage();

                // TODO: remove eventually
                Debug.Assert(image != null, "The image should not be null - there is no reason it can be");
                if (fiImage.Exists) fiImage.Delete();
                image.Save(fiImage.FullName, ImageFormat.Png);


                configDict.Add("MakeAmplitudeSpectrogram", "true");
                BaseSonogram amplitudeSpg = SpectrogramTools.Audio2Sonogram(fiOutputSegment, configDict);
                var mti2 = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
                var image2 = mti2.GetImage();
                image2.Save(fiImage.FullName+"2", ImageFormat.Png);

                SpectrogramCepstral cepgram = new SpectrogramCepstral((AmplitudeSonogram)amplitudeSpg);
                var mti3 = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
                var image3 = mti3.GetImage();
                image3.Save(fiImage.FullName + "3", ImageFormat.Png);
                    
            }
            //###### get sonogram image ##############################################################################################

            if (verbose)
            {
                LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
            }
        }
    } //class Audio2Sonogram
}


