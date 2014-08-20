// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaveletPacketDecomp.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the WaveletPacketDecomp activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Acoustics.Shared;
    using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;

    using AnalysisPrograms.Production;

    using AnalysisRunner;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using PowerArgs;

    using TowseyLibrary;

    /// <summary>
    /// ACTIVITY NAME = WaveletPacketDecomp
    /// does wavelet packet decomposition on an audio file.
    /// </summary>
    public class WaveletPacketDecomp
    {
        // use the following paths for the command line for the <audio2sonogram> task. 
        // WaveletPacketDecomp "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true
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
                return "Does Wavelet Packet Decomposition on the passed audio file.";
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
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-062040.png".ToFileInfo(),
                // Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                // Output = @"C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms\BAC2_20071008-085040.png".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\Sonograms\CaneToads_rural1_20_MONO.png".ToFileInfo(),
                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.WPD.yml".ToFileInfo(),
                // StartOffset = 0,
                // ################################ THERE IS AMBIGUITY IN NEXT ARGUMENT THAT COULD ACTUALLY BE A BUG - SEE ANTHONY
                // EndOffset = 0,
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
                throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
            }

            var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;

            // set default offsets - only use defaults if not provided in argments list
            TimeSpan? startOffset = null;
            TimeSpan? endOffset = null;
            if (offsetsProvided)
            {
                startOffset = TimeSpan.FromMinutes(arguments.StartOffset.Value);
                endOffset   = TimeSpan.FromMinutes(arguments.EndOffset.Value);
            }

            bool verbose = arguments.Verbose;
 

            const string Title = "# MAKE A SONOGRAM FROM AUDIO RECORDING";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);


            // 1. set up the necessary files
            FileInfo sourceRecording = arguments.Source;
            FileInfo configFile = arguments.Config;
            FileInfo outputImage  = arguments.Output;

            // 2. get the config dictionary
            dynamic configuration = Yaml.Deserialise(configFile);

            // below three lines are examples of retrieving info from dynamic config
            // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            // Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            var resampleRate = (int?)configuration[AnalysisKeys.ResampleRate] ?? AppConfigHelper.DefaultTargetSampleRate;


            var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            // #NOISE REDUCTION PARAMETERS
            //string noisereduce = configDict[ConfigKeys.Mfcc.Key_NoiseReductionType];
            configDict[AnalysisKeys.NoiseDoReduction]   = "false";
            configDict[AnalysisKeys.NoiseReductionType] = "NONE";

            configDict[AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString();
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true;

            configDict[ConfigKeys.Recording.Key_RecordingCallName] = arguments.Source.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = arguments.Source.Name;

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

            // 3: GET RECORDING
            FileInfo outputSegment = sourceRecording;
            outputSegment = new FileInfo(Path.Combine(arguments.Output.DirectoryName, "tempWavFile.wav"));
            
            // This line creates a downsampled version of the source file
            MasterAudioUtility.SegmentToWav(sourceRecording, outputSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });

            // init the image stack
            var list = new List<Image>();

            // 1) draw amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(outputSegment.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config

            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            // ###############################################################
            // DO LocalContrastNormalisation
            int fieldSize = 9;
            sonogram.Data = LocalContrastNormalisation.ComputeLCN(sonogram.Data, fieldSize);
            double fractionalStretching = 0.05;
            sonogram.Data = ImageTools.ContrastStretching(sonogram.Data, fractionalStretching);

            // ###############################################################
            int levelNumber = 7;
            int wpdWindow = (int)Math.Pow(2, levelNumber);

            Console.WriteLine("FramesPerSecond = {0}", sonogram.FramesPerSecond);
            double secondsPerWPDwindow = wpdWindow / sonogram.FramesPerSecond;
            Console.WriteLine("secondsPerWPDwindow = {0}", secondsPerWPDwindow);

            double[,] freqOscilMatrix = Wavelets.GetFrequencyByOscillationsMatrix(sonogram.Data, levelNumber, sonogram.FramesPerSecond);


            // ###############################################################

            var image = sonogram.GetImage(false, false);

            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);

            // initialise parameters for drawing gridlines on images
            var minuteOffset = TimeSpan.Zero;
            int nyquist = sonogram.NyquistFrequency;
            var xInterval = TimeSpan.FromSeconds(10);
            TimeSpan xAxisPixelDuration = TimeSpan.FromTicks((long)(sonogram.Duration.Ticks / (double)image.Width));
            const int HertzInterval = 1000;
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            // add title bar and time scale
            string title = "AMPLITUDE SPECTROGRAM";
            var xAxisTicInterval = TimeSpan.FromSeconds(1.0);
            Image titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            Bitmap timeBmp = Image_Track.DrawTimeTrack(sonogram.Duration, image.Width);

            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);
            list.Add(envelopeImage);

            // 2) now draw the standard decibel spectrogram
            //title = "DECIBEL SPECTROGRAM";
            //sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            //image = sonogram.GetImage(false, false);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            //titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            //Image segmentationImage = Image_Track.DrawSegmentationTrack(
            //    sonogram,
            //    EndpointDetectionConfiguration.K1Threshold,
            //    EndpointDetectionConfiguration.K2Threshold,
            //    image.Width);

            //list.Add(titleBar);
            //list.Add(timeBmp);
            //list.Add(image);
            //list.Add(timeBmp);
            //list.Add(segmentationImage);

            // keep the sonogram data for later use
            double[,] dbSpectrogramData = sonogram.Data;

            // 3) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.STANDARD;
            sonoConfig.NoiseReductionParameter = configuration["BgNoiseThreshold"] ?? 3.0; 

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            image = sonogram.GetImage(false, false);
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            // keep the sonogram data for later use
            double[,] nrSpectrogramData = sonogram.Data;

            // add title bar and time scale
            title = "NOISE-REDUCED DECIBEL SPECTROGRAM";
            titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);

            list.Add(titleBar);
            list.Add(timeBmp);
            list.Add(image);
            list.Add(timeBmp);

            // 4) A FALSE-COLOUR VERSION OF SPECTROGRAM
            //title = "FALSE-COLOUR SPECTROGRAM";
            //image = SpectrogramTools.CreateFalseColourSpectrogram(dbSpectrogramData, nrSpectrogramData);
            //SpectrogramTools.DrawGridLinesOnImage((Bitmap)image, minuteOffset, xInterval, xAxisPixelDuration, nyquist, HertzInterval);

            //// add title bar and time scale
            //titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            //list.Add(titleBar);
            //list.Add(timeBmp);
            //list.Add(image);
            //list.Add(timeBmp);

            Image compositeImage = ImageTools.CombineImagesVertically(list);
            compositeImage.Save(outputImage.FullName, ImageFormat.Png);


            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");

        }
    }
}


