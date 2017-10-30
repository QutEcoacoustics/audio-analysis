// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Audio2Sonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Defines the Audio2Sonogram type.
//   ACTIVITY CODE: audio2sonogram
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace AnalysisPrograms
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Acoustics.Shared;
    using Acoustics.Shared.Csv;
    using Acoustics.Tools.Wav;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using Production;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;

    using log4net;
    using MathNet.Numerics.NumberTheory;
    using PowerArgs;
    using TowseyLibrary;

    public class Audio2Sonogram
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // use the following paths for the command line for the <audio2sonogram> task.
        // audio2sonogram "C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav" "C:\SensorNetworks\Software\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.cfg"  C:\SensorNetworks\Output\Sonograms\BAC1_20071008-081607.png 0   0  true
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
                return "Generates multiple spectrogram images and ascilllations info";
            }

            public static string AdditionalNotes()
            {
                return string.Empty;
            }
        }

        [Obsolete("See https://github.com/QutBioacoustics/audio-analysis/issues/134")]
        private static Arguments Dev()
        {
            return new Arguments
            {
                //MARINE
                //Source = @"C:\SensorNetworks\WavFiles\MarineRecordings\20130318_171500.wav".ToFileInfo(),
                //Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.SonogramMarine.yml".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\MarineSonograms\".ToDirectoryInfo(),

                // LEWINs RAIL
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-062040.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC1_20071008-081607.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\LewinsRail\BAC2_20071008-085040.wav".ToFileInfo(),
                //Output = @"C:\SensorNetworks\Output\LewinsRail".ToDirectoryInfo(),
                Source = @"G:\SensorNetworks\WavFiles\LewinsRail\FromLizZnidersic\Lewinsrail_TasmanIs_Tractor_SM304253_0151119_0640_1min.wav".ToFileInfo(),
                Output = @"C:\SensorNetworks\Output\LewinsRail\LewinsRail_ThreeCallTypes".ToDirectoryInfo(),

                //CANETOAD
                //Source = @"Y:\Canetoad\FromPaulRoe\canetoad_CubberlaCreek_100529_16bitPCM.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\Frogs\JCU\Litoria fellax1.mp3".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\Frogs\MiscillaneousDataSet\CaneToads_rural1_20_MONO.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\TestRecordings\NW_NW273_20101013-051200-0514-1515-Brown Cuckoo-dove1.wav".ToFileInfo(),

                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\Kanowski_651_233394_20120831_072112_4.0__.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\Melaleuca_Middle_183_192469_20101123_013009_4.0__.wav".ToFileInfo(),
                //Source = @"C:\SensorNetworks\WavFiles\ConvDNNData\SE_399_188293_20101014_132950_4.0__.wav".ToFileInfo(),

                Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo(),
                //Config = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Mangalam.Sonogram.yml".ToFileInfo(),
            };

            throw new NoDeveloperMethodException();
        }

        public static void Main(Arguments arguments)
        {
            if (arguments == null)
            {
                arguments = Dev();
            }

            if (!arguments.Output.Exists)
            {
                arguments.Output.Create();
            }

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
                endOffset = TimeSpan.FromMinutes(arguments.EndOffset.Value);
            }

            const string title = "# MAKE FOUR SONOGRAMS FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + arguments.Source.Name);

            // 1. set up the necessary files
            FileInfo sourceRecording = arguments.Source;
            FileInfo configFile = arguments.Config;
            DirectoryInfo output = arguments.Output;

            // 2. get the config dictionary
            var configDict = GetConfigDictionary(configFile, false);
            configDict[ConfigKeys.Recording.Key_RecordingCallName] = arguments.Source.FullName;
            configDict[ConfigKeys.Recording.Key_RecordingFileName] = arguments.Source.Name;

            // 3: GET TEMPORARY RECORDING
            int resampleRate = Convert.ToInt32(configDict[AnalysisKeys.ResampleRate]);
            var tempAudioSegment = AudioRecording.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

            // 4: GET 4 sonogram images
            string sourceName = configDict[ConfigKeys.Recording.Key_RecordingFileName];
            sourceName = Path.GetFileNameWithoutExtension(sourceName);
            var soxFile = new FileInfo(Path.Combine(output.FullName, sourceName + "SOX.png"));
            var result = GenerateFourSpectrogramImages(tempAudioSegment, soxFile, configDict, dataOnly: false, makeSoxSonogram: false);
            var outputImageFile = new FileInfo(Path.Combine(output.FullName, sourceName + ".FourSpectrograms.png"));
            result.CompositeImage.Save(outputImageFile.FullName, ImageFormat.Png);

            LoggedConsole.WriteLine("\n##### FINISHED FILE ###################################################\n");
        }

        private static Dictionary<string, string> GetConfigDictionary(FileInfo configFile, bool writeParameters)
        {
            dynamic configuration = Yaml.Deserialise(configFile);

            // var configDict = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            var configDict = new Dictionary<string, string>(dictionary: (Dictionary<string, string>)configuration)
            {
                // below three lines are examples of retrieving info from dynamic config
                // string analysisIdentifier = configuration[AnalysisKeys.AnalysisName];
                // bool saveIntermediateWavFiles = (bool?)configuration[AnalysisKeys.SaveIntermediateWavFiles] ?? false;
                // scoreThreshold = (double?)configuration[AnalysisKeys.EventThreshold] ?? scoreThreshold;

                // Resample rate must be 2 X the desired Nyquist.
                // WARNING: Default used to be the SR of the recording. NOW DEFAULT = 22050.
                [AnalysisKeys.ResampleRate] = (string)configuration[AnalysisKeys.ResampleRate] ?? "22050",

                [AnalysisKeys.AddAxes] = ((bool?)configuration[AnalysisKeys.AddAxes] ?? true).ToString(),
                [AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? true
            };

            // # REDUCTION FACTORS for freq and time dimensions
            // #TimeReductionFactor: 1
            // #FreqReductionFactor: 1

            bool makeSoxSonogram = (bool?)configuration[AnalysisKeys.MakeSoxSonogram] ?? false;
            configDict[AnalysisKeys.SonogramTitle] = (string)configuration[AnalysisKeys.SonogramTitle] ?? "Sonogram";
            configDict[AnalysisKeys.SonogramComment] = (string)configuration[AnalysisKeys.SonogramComment] ?? "Sonogram produced using SOX";
            configDict[AnalysisKeys.SonogramColored] = (string)configuration[AnalysisKeys.SonogramColored] ?? "false";
            configDict[AnalysisKeys.SonogramQuantisation] = (string)configuration[AnalysisKeys.SonogramQuantisation] ?? "128";
            configDict[AnalysisKeys.AddTimeScale] = (string)configuration[AnalysisKeys.AddTimeScale] ?? "true";
            configDict[AnalysisKeys.AddAxes] = (string)configuration[AnalysisKeys.AddAxes] ?? "true";
            configDict[AnalysisKeys.AddSegmentationTrack] = (string)configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";

            if (!writeParameters)
            {
                return configDict;
            }

            // print out the sonogram parameters
            LoggedConsole.WriteLine("\nPARAMETERS");
            foreach (KeyValuePair<string, string> kvp in configDict)
            {
                LoggedConsole.WriteLine("{0}  =  {1}", kvp.Key, kvp.Value);
            }

            return configDict;
        }

        public static AudioToSonogramResult GenerateFourSpectrogramImages(
            FileInfo sourceRecording,
            FileInfo path2SoxSpectrogram,
            Dictionary<string, string> configDict,
            bool dataOnly = false,
            bool makeSoxSonogram = false)
        {
            var result = new AudioToSonogramResult();

            if (dataOnly && makeSoxSonogram)
            {
                throw new ArgumentException("Can't produce data only for a SoX sonogram");
            }

            if (makeSoxSonogram)
            {
                SpectrogramTools.MakeSonogramWithSox(sourceRecording, configDict, path2SoxSpectrogram);
                result.Path2SoxImage = path2SoxSpectrogram;
            }
            else if (dataOnly)
            {
                var recordingSegment = new AudioRecording(sourceRecording.FullName);
                var sonoConfig = new SonogramConfig(configDict); // default values config

                // disable noise removal
                sonoConfig.NoiseReductionType = NoiseReductionType.None;
                Log.Warn("Noise removal disabled!");

                var sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
                result.DecibelSpectrogram = sonogram;
            }
            else
            {
                // init the image stack
                var list = new List<Image>();

                // IMAGE 1) draw amplitude spectrogram
                var recordingSegment = new AudioRecording(sourceRecording.FullName);
                var sonoConfig = new SonogramConfig(configDict); // default values config

                // disable noise removal for first two spectrograms
                var disabledNoiseReductionType = sonoConfig.NoiseReductionType;
                sonoConfig.NoiseReductionType = NoiseReductionType.None;

                BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);

                // remove the DC bin if it has not already been removed.
                // Assume test of divisible by 2 is good enough.
                int binCount = sonogram.Data.GetLength(1);
                if (!binCount.IsEven())
                {
                    sonogram.Data = MatrixTools.Submatrix(sonogram.Data, 0, 1, sonogram.FrameCount - 1, binCount - 1);
                }

                //save spectrogram data at this point - prior to noise reduction
                var spectrogramDataBeforeNoiseReduction = sonogram.Data;

                const double neighbourhoodSeconds = 0.25;
                int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
                const double lcnContrastLevel = 0.001;
                LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
                LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
                const int lowPercentile = 20;
                sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);
                sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);
                //sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);

                var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");
                list.Add(image);

                //string path2 = @"C:\SensorNetworks\Output\Sonograms\dataInput2.png";
                //Histogram.DrawDistributionsAndSaveImage(sonogram.Data, path2);

                // double[,] matrix = sonogram.Data;
                double[,] matrix = ImageTools.WienerFilter(sonogram.Data, 3);
                double ridgeThreshold = 0.25;
                byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
                hits = RidgeDetection.JoinDisconnectedRidgesInMatrix(hits, matrix, ridgeThreshold);
                image = SpectrogramTools.CreateFalseColourAmplitudeSpectrogram(spectrogramDataBeforeNoiseReduction, null, hits);
                image = sonogram.GetImageAnnotatedWithLinearHerzScale(image, "AMPLITUDE SPECTROGRAM + LCN + ridge detection");
                list.Add(image);

                Image envelopeImage = Image_Track.DrawWaveEnvelopeTrack(recordingSegment, image.Width);
                list.Add(envelopeImage);

                // IMAGE 2) now draw the standard decibel spectrogram
                sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
                result.DecibelSpectrogram = (SpectrogramStandard)sonogram;
                image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM");
                list.Add(image);

                Image segmentationImage = Image_Track.DrawSegmentationTrack(
                    sonogram,
                    EndpointDetectionConfiguration.K1Threshold,
                    EndpointDetectionConfiguration.K2Threshold,
                    image.Width);
                list.Add(segmentationImage);

                // keep the sonogram data for later use
                double[,] dbSpectrogramData = (double[,])sonogram.Data.Clone();

                // 3) now draw the noise reduced decibel spectrogram
                // #NOISE REDUCTION PARAMETERS - restore noise reduction ##################################################################
                sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                sonoConfig.NoiseReductionParameter = double.Parse(configDict[AnalysisKeys.NoiseBgThreshold] ?? "2.0");

                // #NOISE REDUCTION PARAMETERS - MARINE HACK ##################################################################
                //sonoConfig.NoiseReductionType = NoiseReductionType.FIXED_DYNAMIC_RANGE;
                //sonoConfig.NoiseReductionParameter = 80.0;

                sonogram = new SpectrogramStandard(sonoConfig, recordingSegment.WavReader);
                image = sonogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM + Lamel noise subtraction");
                list.Add(image);

                // keep the sonogram data for later use
                double[,] nrSpectrogramData = sonogram.Data;

                // 4) A FALSE-COLOUR VERSION OF SPECTROGRAM
                // ########################### SOBEL ridge detection
                ridgeThreshold = 3.5;
                matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
                hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

                // ########################### EIGEN ridge detection
                //double ridgeThreshold = 6.0;
                //double dominanceThreshold = 0.7;
                //var rotatedData = MatrixTools.MatrixRotate90Anticlockwise(dbSpectrogramData);
                //byte[,] hits = RidgeDetection.StructureTensorRidgeDetection(rotatedData, ridgeThreshold, dominanceThreshold);
                //hits = MatrixTools.MatrixRotate90Clockwise(hits);
                // ########################### EIGEN ridge detection

                image = SpectrogramTools.CreateFalseColourDecibelSpectrogram(dbSpectrogramData, nrSpectrogramData, hits);
                image = sonogram.GetImageAnnotatedWithLinearHerzScale(image, "DECIBEL SPECTROGRAM - Colour annotated");

                list.Add(image);

                // 5) TODO: ONE OF THESE YEARS FIX UP THE CEPTRAL SONOGRAM
                ////SpectrogramCepstral cepgram = new SpectrogramCepstral((AmplitudeSonogram)amplitudeSpg);
                ////var mti3 = SpectrogramTools.Sonogram2MultiTrackImage(sonogram, configDict);
                ////var image3 = mti3.GetImage();
                ////image3.Save(fiImage.FullName + "3", ImageFormat.Png);

                // 6) COMBINE THE SPECTROGRAM IMAGES
                result.CompositeImage = ImageTools.CombineImagesVertically(list);
            }

            return result;
        }

        // ########################################  AUDIO2SONOGRAM TEST METHOD BELOW HERE ######################################################

        public static void TESTMETHOD_DrawFourSpectrograms()
        {
            {
                var sourceRecording = @"C:\SensorNetworks\SoftwareTests\TestRecordings\BAC2_20071008-085040.wav".ToFileInfo();
                var output = @"C:\SensorNetworks\SoftwareTests\TestFourSonograms".ToDirectoryInfo();
                var configFile = @"C:\Work\GitHub\audio-analysis\AudioAnalysis\AnalysisConfigFiles\Towsey.Sonogram.yml".ToFileInfo();
                var expectedResultsDir = new DirectoryInfo(Path.Combine(output.FullName, "ExpectedTestResults"));
                if (!expectedResultsDir.Exists)
                {
                    expectedResultsDir.Create();
                }

                // 1. get the config dictionary
                var configDict = GetConfigDictionary(configFile, true);
                configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
                configDict[ConfigKeys.Recording.Key_RecordingFileName] = sourceRecording.Name;

                // 2. Create temp copy of recording
                int resampleRate = Convert.ToInt32(configDict[AnalysisKeys.ResampleRate]);
                var tempAudioSegment = AudioRecording.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

                // 3. GET composite image of 4 sonograms
                var sourceName = Path.GetFileNameWithoutExtension(sourceRecording.Name);
                var soxImage = new FileInfo(Path.Combine(output.FullName, sourceName + ".SOX.png"));
                var result = GenerateFourSpectrogramImages(tempAudioSegment, soxImage, configDict, dataOnly: false, makeSoxSonogram: false);
                var outputImage = new FileInfo(Path.Combine(output.FullName, sourceName + ".FourSpectrograms.png"));
                result.CompositeImage.Save(outputImage.FullName, ImageFormat.Png);

                // construct output file names
                var fileName = sourceName + ".FourSpectrogramsImageInfo";
                var pathName = Path.Combine(output.FullName, fileName);
                var csvFile1 = new FileInfo(pathName + ".json");

                // Do my version of UNIT TESTING - This is the File Equality Test.
                // First construct a test result file containing image info
                var sb = new StringBuilder("Width,Height\n");
                sb.AppendLine($"{result.CompositeImage.Width},{result.CompositeImage.Height}");
                // Acoustics.Shared.Csv.Csv.WriteToCsv(csvFile1, sb);
                FileTools.WriteTextFile(csvFile1.FullName, sb.ToString());

                // Now do the test
                var expectedTestFile1 = new FileInfo(Path.Combine(expectedResultsDir.FullName, "FourSpectrogramsTest.EXPECTED.json"));
                TestTools.FileEqualityTest("Matrix Equality", csvFile1, expectedTestFile1);
                Console.WriteLine("\n\n");
            }
        }
    }

    /// <summary>
    /// In line class used to return results from the static method Audio2Sonogram.GenerateFourSpectrogramImages();
    /// </summary>
    public class AudioToSonogramResult
    {
        public SpectrogramStandard DecibelSpectrogram { get; set; }

        // path to spectrogram image
        public FileInfo Path2SoxImage { get; set; }

        // Four spectrogram image
        public Image CompositeImage { get; set; }

        public FileInfo FreqOscillationImage { get; set; }

        public FileInfo FreqOscillationData { get; set; }
    }

    /// <summary>
    /// This analyzer simply generates short (i.e. one minute) spectrograms and outputs them to CSV files.
    /// It does not accumulate data or other indices over a long recording.
    /// </summary>
    public class SpectrogramAnalyzer : IAnalyser2
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SpectrogramAnalyzer()
        {
            this.DisplayName = "Spectrogram Analyzer";
            this.Identifier = "Towsey.SpectrogramGenerator";
            this.DefaultSettings = new AnalysisSettings()
            {
                AnalysisMaxSegmentDuration = TimeSpan.FromMinutes(1),
                AnalysisMinSegmentDuration = TimeSpan.FromSeconds(20),
                SegmentMediaType = MediaTypes.MediaTypeWav,
                SegmentOverlapDuration = TimeSpan.Zero,
            };
        }

        public string DisplayName { get; private set; }

        public string Identifier { get; private set; }

        public string Description => "This analyzer simply generates short (i.e. one minute) spectrograms and outputs them to CSV files. It does not accumulate data or other indices over a long recording.";

        public AnalysisSettings DefaultSettings { get; private set; }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;

            var analysisResult = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            dynamic configuration = Yaml.Deserialise(analysisSettings.ConfigFile);

            bool saveCsv = (bool?)configuration[AnalysisKeys.SaveIntermediateCsvFiles] ?? false;

            if ((bool?)configuration[AnalysisKeys.MakeSoxSonogram] == true)
            {
                Log.Warn("SoX spectrogram generation config variable found (and set to true) but is ignored when running as an IAnalyzer");
            }

            // generate spectrogram
            var configurationDictionary = new Dictionary<string, string>((Dictionary<string, string>)configuration);
            configurationDictionary[ConfigKeys.Recording.Key_RecordingCallName] = audioFile.FullName;
            configurationDictionary[ConfigKeys.Recording.Key_RecordingFileName] = audioFile.Name;
            var spectrogramResult = Audio2Sonogram.GenerateFourSpectrogramImages(
                audioFile,
                null, // path2SoxFile
                configurationDictionary,
                dataOnly: analysisSettings.AnalysisDataSaveBehavior,
                makeSoxSonogram: false);

            // this analysis produces no results!
            // but we still print images (that is the point)
            if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResult.Events.Length))
            {
                Debug.Assert(segmentSettings.SegmentImageFile.Exists);
                spectrogramResult.CompositeImage.Save(segmentSettings.SegmentImageFile.FullName, ImageFormat.Png);
            }

            if (saveCsv)
            {
                var basename = Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name);
                var spectrogramCsvFile = outputDirectory.CombineFile(basename + ".Spectrogram.csv");
                Csv.WriteMatrixToCsv(spectrogramCsvFile, spectrogramResult.DecibelSpectrogram.Data, TwoDimensionalArray.RowMajor);
            }

            return analysisResult;
        }

        public void WriteEventsFile(FileInfo destination, IEnumerable<EventBase> results)
        {
            throw new NotImplementedException();
        }

        public void WriteSummaryIndicesFile(FileInfo destination, IEnumerable<SummaryIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public List<FileInfo> WriteSpectrumIndicesFiles(DirectoryInfo destination, string fileNameBase, IEnumerable<SpectralIndexBase> results)
        {
            throw new NotImplementedException();
        }

        public SummaryIndexBase[] ConvertEventsToSummaryIndices(IEnumerable<EventBase> events, TimeSpan unitTime, TimeSpan duration, double scoreThreshold)
        {
            throw new NotImplementedException();
        }

        public void SummariseResults(
            AnalysisSettings settings,
            FileSegment inputFileSegment,
            EventBase[] events,
            SummaryIndexBase[] indices,
            SpectralIndexBase[] spectralIndices,
            AnalysisResult2[] results)
        {
            // no-op
        }
    }
}