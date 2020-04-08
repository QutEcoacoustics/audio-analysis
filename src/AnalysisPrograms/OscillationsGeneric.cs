// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OscillationsGeneric.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the oscillationsGeneric activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Tools;
    using Acoustics.Tools.Audio;
    using AnalysisPrograms.Production;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using McMaster.Extensions.CommandLineUtils;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using TowseyLibrary;
    using Path = System.IO.Path;

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

        public const string CommandName = "OscillationsGeneric";

        [Command(
            CommandName,
            Description = "[BETA] Does a generic search for oscillations in the passed audio file. Short recordings only.")]
        public class Arguments : SourceConfigOutputDirArguments
        {
            [Option(Description = "The start offset (in seconds) of the source audio file to operate on")]
            [InRange(0, double.MaxValue)]
            public double? StartOffset { get; set; }

            [Option(Description = "The end offset (in minutes) of the source audio file to operate on")]
            [InRange(0, double.MaxValue)]
            public double? EndOffset { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                OscillationsGeneric.Main(this);
                return this.Ok();
            }
        }

        public static void Main(Arguments arguments)
        {
            // 1. set up the necessary files
            FileInfo sourceRecording = arguments.Source;
            FileInfo configFile = arguments.Config.ToFileInfo();
            DirectoryInfo opDir = arguments.Output;

            opDir.Create();

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
                startOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
                endOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            }

            const string Title = "# MAKE A SONOGRAM FROM AUDIO RECORDING and do OscillationsGeneric activity.";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(Title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + sourceRecording.Name);

            string sourceName = Path.GetFileNameWithoutExtension(sourceRecording.FullName);

            // 2. get the config dictionary
            Config configuration = ConfigFile.Deserialize(configFile);

            // below three lines are examples of retrieving info from Config config
            // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
            // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
            // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

            // Resample rate must be 2 X the desired Nyquist. Default is that of recording.
            var resampleRate = configuration.GetIntOrNull(AnalysisKeys.ResampleRate) ?? AppConfigHelper.DefaultTargetSampleRate;

            var configDict = new Dictionary<string, string>(configuration.ToDictionary());

            // #NOISE REDUCTION PARAMETERS
            //string noisereduce = configDict[ConfigKeys.Mfcc.Key_NoiseReductionType];
            configDict[AnalysisKeys.NoiseDoReduction] = "false";
            configDict[AnalysisKeys.NoiseReductionType] = "NONE";

            configDict[AnalysisKeys.AddAxes] = configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";

            configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

            configDict[AnalysisKeys.AddTimeScale] = configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";

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
            {
                Oscillations2014.DefaultSensitivityThreshold = double.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);
            }

            if (configDict.ContainsKey(AnalysisKeys.OscilDetection2014SampleLength))
            {
                Oscillations2014.DefaultSampleLength = int.Parse(configDict[AnalysisKeys.OscilDetection2014SensitivityThreshold]);
            }

            var list1 = new List<Image<Rgb24>>();

            //var result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, 64, "Autocorr-FFT");
            //list1.Add(result.FreqOscillationImage);
            var result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-SVD-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-WPD");
            list1.Add(result.FreqOscillationImage);
            var compositeOscImage1 = ImageTools.CombineImagesInLine(list1.ToArray());

            // ###############################################################

            // init the sonogram image stack
            var sonogramList = new List<Image<Rgb24>>();
            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM");
            sonogramList.Add(image);

            //string testPath = @"C:\SensorNetworks\Output\Sonograms\amplitudeSonogram.png";
            //image.Save(testPath);

            var envelopeImage = ImageTrack.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
            sonogramList.Add(envelopeImage);

            // 2) now draw the standard decibel spectrogram
            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);

            // ###############################################################
            list1 = new List<Image<Rgb24>>();

            //result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, 64, "Autocorr-FFT");
            //list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-SVD-FFT");
            list1.Add(result.FreqOscillationImage);
            result = Oscillations2014.GetFreqVsOscillationsDataAndImage(sonogram, "Autocorr-WPD");
            list1.Add(result.FreqOscillationImage);
            var compositeOscImage2 = ImageTools.CombineImagesInLine(list1.ToArray());

            // ###############################################################
            //image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM");
            //list.Add(image);

            // combine eight images
            list1 = new List<Image<Rgb24>>();
            list1.Add(compositeOscImage1);
            list1.Add(compositeOscImage2);
            var compositeOscImage3 = ImageTools.CombineImagesVertically(list1.ToArray());
            string imagePath3 = Path.Combine(opDir.FullName, sourceName + "_freqOscilMatrix.png");
            compositeOscImage3.Save(imagePath3);

            var segmentationImage = ImageTrack.DrawSegmentationTrack(
                sonogram,
                EndpointDetectionConfiguration.K1Threshold,
                EndpointDetectionConfiguration.K2Threshold,
                image.Width);
            sonogramList.Add(segmentationImage);

            // 3) now draw the noise reduced decibel spectrogram
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = configuration.GetDoubleOrNull(AnalysisKeys.NoiseBgThreshold) ?? 3.0;

            sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
            image = sonogram.GetImageFullyAnnotated("NOISE-REDUCED DECIBEL  SPECTROGRAM");
            sonogramList.Add(image);

            // ###############################################################
            // deriving osscilation graph from this noise reduced spectrogram did not work well
            //Oscillations2014.SaveFreqVsOscillationsDataAndImage(sonogram, sampleLength, algorithmName, opDir);
            // ###############################################################

            var compositeSonogram = ImageTools.CombineImagesVertically(sonogramList);
            string imagePath2 = Path.Combine(opDir.FullName, sourceName + ".png");
            compositeSonogram.Save(imagePath2);

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        }

        /// <summary>
        /// Puts title bar, X &amp; Y axes and gridlines on the passed sonogram.
        /// </summary>
        private static Image AnnotateSonogram(BaseSonogram sonogram, string title)
        {
            var image = sonogram.GetImageFullyAnnotated(title);
            return image;
        }
    }
}