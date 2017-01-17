// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OscillationsGeneric.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the oscillationsGeneric activity.
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
    //using Acoustics.Shared.Extensions;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;

    using AnalysisBase;
    using AnalysisPrograms.Production;
    //using AnalysisRunner;

    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using PowerArgs;
    using TowseyLibrary;

    /// <summary>
    /// ACTIVITY NAME = oscillationsGeneric
    /// does a general search for oscillation in an audio file.
    /// </summary>
    public static class OscillationsGeneric
    {
        // ####################################################################

        // SET THE 3 PARAMETERS HERE FOR DETECTION OF OSCILLATION

        // Sensitivity: 0=detects fewest oscillations; 1=detects most oscillations
        public static double Sensitivity = 0.2;
        // Window width when sampling along freq bins to find oscillations.
        // This is Not the wave form window.
        // 64 is better where many birds and fast changing acoustic activity
        //int sampleLength = 64;
        // 128 is better where slow moving changes to acoustic activity
        public static int SampleLength = 128;

        // use this if want only dominant oscillations
        public static string AlgorithmName = "Autocorr-SVD-FFT";
        // use this if want more detailed output
        //public static string algorithmName = "Autocorr-FFT";
        // tried but not working
        //string algorithmName = "CwtWavelets";
        // ####################################################################





        // use the following paths for the command line for the <audio2sonogram> task. 
        // oscillationsGeneric "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true
        [CustomDetailedDescription]
        [CustomDescription]
        public class Arguments : SourceConfigOutputDirArguments
        {
            [ArgDescription("The start offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [ArgDescription("The end offset (in minutes) of the source audio file to operate on")]
            [ArgRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }


            public static string Description()
            {
                return "Does a generic search for oscillations in the passed audio file.";
            }

            public static string AdditionalNotes()
            {
                return "StartOffset and EndOffset are both required when either is included.";
            }
        }

        private static Arguments Dev()
        {

            return new Arguments
            {
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),
                //Source = @"Y:\Jie Frogs\Recording_1.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100530_1.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.OscillationsGeneric.yml".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\Sonograms".ToDirectoryInfo(),
            };

            throw new NoDeveloperMethodException();
        }


        public static void Main(Arguments arguments)
        {

            if (arguments == null)
            {
                arguments = Dev();
            }

            arguments.Output.Create();

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

            const string Title = "# MAKE A SONOGRAM FROM AUDIO RECORDING and do OscillationsGeneric activity.";
            string date  = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);


            // 1. set up the necessary files
            FileInfo sourceRecording = arguments.Source;
            FileInfo configFile = arguments.Config;
            DirectoryInfo opDir = arguments.Output;

            string sourceName = Path.GetFileNameWithoutExtension(sourceRecording.FullName);

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

            // ####################################################################

            // print out the sonogram parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }
            LoggedConsole.WriteLine("Sample Length for detecting oscillations = {0}", SampleLength);

            // 3: GET RECORDING
            FileInfo tempAudioSegment = new FileInfo(Path.Combine(opDir.FullName, "tempWavFile.wav"));
            // delete the temp audio file if it already exists.
            if (File.Exists(tempAudioSegment.FullName))
            {
                File.Delete(tempAudioSegment.FullName);
            }
            // This line creates a temporary version of the source file downsampled as per entry in the config file
            MasterAudioUtility.SegmentToWav(sourceRecording, tempAudioSegment, new AudioUtilityRequest() { TargetSampleRate = resampleRate });


            // 1) get amplitude spectrogram
            AudioRecording recordingSegment = new AudioRecording(tempAudioSegment.FullName);
            SonogramConfig sonoConfig = new SonogramConfig(configDict); // default values config
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            Console.WriteLine("FramesPerSecond = {0}", sonogram.FramesPerSecond);

            // remove the DC bin
            sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, sonogram.Configuration.FreqBinCount);

            // ###############################################################
            // DO LocalContrastNormalisation
            //int fieldSize = 9;
            //sonogram.Data = LocalContrastNormalisation.ComputeLCN(sonogram.Data, fieldSize);
            // LocalContrastNormalisation over frequency bins is better and faster.
            int neighbourhood = 15;
            double contrastLevel = 0.5;
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhood, contrastLevel);

            // ###############################################################
            // lowering the sensitivity threshold increases the number of hits.
            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SensitivityThreshold)) 
                Oscillations2014.DefaultSensitivityThreshold = double.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);

            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
                Oscillations2014.DefaultSampleLength = Int32.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);

            var list1 = new List<Image>();
            //var result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, 64, "Autocorr-FFT");
            //list1.Add(result.FreqOscillationImage);
            var result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-SVD-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-WPD");
            list1.Add(result.FreqOscillationImage);
            Image compositeOscImage1 = ImageTools.CombineImagesInLine(list1.ToArray());
            // ###############################################################

            // init the sonogram image stack
            var sonogramList = new List<Image>();
            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM");
            sonogramList.Add(image);
            //string testPath = @"C:\SensorNetworks\Output\Sonograms\amplitudeSonogram.png";
            //image.Save(testPath, ImageFormat.Png);

            Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
            sonogramList.Add(envelopeImage);

            // 2) now draw the standard decibel spectrogram
            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            // ###############################################################
            list1 = new List<Image>();
            //result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, 64, "Autocorr-FFT");
            //list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-SVD-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-WPD");
            list1.Add(result.FreqOscillationImage);
            Image compositeOscImage2 = ImageTools.CombineImagesInLine(list1.ToArray());
            // ###############################################################
            //image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM");
            //list.Add(image);


            // combine eight images
            list1 = new List<Image>();
            list1.Add(compositeOscImage1);
            list1.Add(compositeOscImage2);
            Image compositeOscImage3 = ImageTools.CombineImagesVertically(list1.ToArray());
            string imagePath3 = Path.Combine(opDir.FullName, sourceName + "_freqOscilMatrix.png");
            compositeOscImage3.Save(imagePath3, ImageFormat.Png);


            Image segmentationImage = Image_Track.DrawSegmentationTrack(
                sonogram,
                EndpointDetectionConfiguration.K1Threshold,
                EndpointDetectionConfiguration.K2Threshold,
                image.Width);
            sonogramList.Add(segmentationImage);

            // 3) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = configuration["BgNoiseThreshold"] ?? 3.0; 

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            image = sonogram.GetImageFullyAnnotated("NOISE-REDUCED DECIBEL  SPECTROGRAM");
            sonogramList.Add(image);
            // ###############################################################
            // deriving osscilation graph from this noise reduced spectrogram did not work well
            //Oscillations2014.SaveFreqVsOscillationsDataAndImage(sonogram, sampleLength, algorithmName, opDir);
            // ###############################################################

            Image compositeSonogram = ImageTools.CombineImagesVertically(sonogramList);
            string imagePath2 = Path.Combine(opDir.FullName, sourceName +".png");
            compositeSonogram.Save(imagePath2, ImageFormat.Png);

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");

        }



        /// <summary>
        /// Puts title bar, X & Y axes and gridlines on the passed sonogram.
        /// </summary>
        /// <param name="sonogram"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        private static Image AnnotateSonogram(BaseSonogram sonogram, string title)
        {
            var image = sonogram.GetImageFullyAnnotated(title);
            return image;
        }
    }
}


