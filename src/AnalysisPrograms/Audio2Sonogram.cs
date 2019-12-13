// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Audio2Sonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// <summary>
//   Produces various kinds of standard scale spectrograms.
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
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.Csv;
    using AnalysisBase;
    using AnalysisBase.ResultBases;
    using AnalysisPrograms.Production.Arguments;
    using AnalysisPrograms.Production.Validation;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using log4net;
    using McMaster.Extensions.CommandLineUtils;
    using TowseyLibrary;

    /// <summary>
    /// Produces standard greyscale spectrograms of various types from a wav audio file.
    /// </summary>
    public class Audio2Sonogram
    {
        public const string CommandName = "Audio2Sonogram";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Command(
            Name = CommandName,
            Description = "[BETA] Generates multiple standard-scale spectrograms")]
        public class Arguments : SourceConfigOutputDirArguments
        {
            [Option(Description = "The start offset to start analyzing from (in seconds)")]
            [InRange(min: 0)]
            public double? StartOffset { get; set; }

            [Option(Description = "The end offset to stop analyzing (in seconds)")]
            [InRange(min: 0)]
            public double? EndOffset { get; set; }

            public override Task<int> Execute(CommandLineApplication app)
            {
                Main(this);
                return this.Ok();
            }
        }

        public static void Main(Arguments arguments)
        {
            // 1. set up the necessary files
            var sourceRecording = arguments.Source;
            var configInfo = ConfigFile.Deserialize<AnalyzerConfig>(arguments.Config.ToFileInfo());
            DirectoryInfo output = arguments.Output;
            if (!output.Exists)
            {
                output.Create();
            }

            //if (arguments.StartOffset.HasValue ^ arguments.EndOffset.HasValue)
            //{
            //    throw new InvalidStartOrEndException("If StartOffset or EndOffset is specified, then both must be specified");
            //}
            // set default offsets - only use defaults if not provided in arguments list
            // var offsetsProvided = arguments.StartOffset.HasValue && arguments.EndOffset.HasValue;
            //TimeSpan? startOffset;
            //TimeSpan? endOffset;
            //if (offsetsProvided)
            //{
            //    startOffset = TimeSpan.FromSeconds(arguments.StartOffset.Value);
            //    endOffset = TimeSpan.FromSeconds(arguments.EndOffset.Value);
            //}

            const string title = "# MAKE MULTIPLE SONOGRAMS FROM AUDIO RECORDING";
            string date = "# DATE AND TIME: " + DateTime.Now;
            LoggedConsole.WriteLine(title);
            LoggedConsole.WriteLine(date);
            LoggedConsole.WriteLine("# Input  audio file: " + sourceRecording.Name);

            // 3: CREATE A TEMPORARY RECORDING
            int resampleRate = Convert.ToInt32(configInfo.GetInt("ResampleRate"));
            var tempAudioSegment = AudioRecording.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

            // 4: GENERATE SPECTROGRAM images
            //string sourceName = sourceRecording.FullName;
            string sourceName = Path.GetFileNameWithoutExtension(sourceRecording.FullName);
            var result = GenerateSpectrogramImages(tempAudioSegment, configInfo);

            // 5: Save the image
            var outputImageFile = new FileInfo(Path.Combine(output.FullName, sourceName + ".Spectrograms.png"));
            result.CompositeImage.Save(outputImageFile.FullName, ImageFormat.Png);
        }

        /// <summary>
        /// Calculates the following spectrograms as per content of config.yml file:
        /// Waveform: true.
        /// DifferenceSpectrogram: true.
        /// DecibelSpectrogram: true.
        /// DecibelSpectrogram_NoiseReduced: true.
        /// DecibelSpectrogram_Ridges: true.
        /// AmplitudeSpectrogram_LocalContrastNormalization: true.
        /// SoxSpectrogram: false.
        /// Experimental: true.
        /// </summary>
        public static AudioToSonogramResult GenerateSpectrogramImages(
            FileInfo sourceRecording,
            AnalyzerConfig configInfo)
        {
            //int signalLength = recordingSegment.WavReader.GetChannel(0).Length;
            var recordingSegment = new AudioRecording(sourceRecording.FullName);
            //configDict[ConfigKeys.Recording.Key_RecordingCallName] = sourceRecording.FullName;
            string recordingName = sourceRecording.Name;
            int sampleRate = recordingSegment.WavReader.SampleRate;
            var result = new AudioToSonogramResult();

            // init the image stack
            var list = new List<Image>();

            bool doWaveForm = configInfo.GetBool("Waveform");
            bool doDecibelSpectrogram = configInfo.GetBool("DecibelSpectrogram");
            bool doNoiseReducedSpectrogram = configInfo.GetBool("DecibelSpectrogram_NoiseReduced");
            bool doRidgeSpectrogram = configInfo.GetBool("DecibelSpectrogram_Ridges");
            bool doDifferenceSpectrogram = configInfo.GetBool("DifferenceSpectrogram");
            bool doLcnSpectrogram = configInfo.GetBool("AmplitudeSpectrogram_LocalContrastNormalization");
            bool doCepstralSpectrogram = configInfo.GetBool("CepstralSpectrogram");

            //Don't do SOX spectrogram.
            //bool doSoxSpectrogram = configInfo.GetBool("SoxSpectrogram");
            bool doSoxSpectrogram = false;
            bool doExperimentalSpectrogram = configInfo.GetBool("Experimental");

            int frameSize = configInfo.GetInt("FrameLength");
            int frameStep = configInfo.GetInt("FrameStep");

            // must calculate this because used later on.
            double frameOverlap = (frameSize - frameStep) / (double)frameSize;

            // EXTRACT ENVELOPE and SPECTROGRAM FROM RECORDING SEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recordingSegment, frameSize, frameStep);

            var sonoConfig = new SonogramConfig()
            {
                epsilon = recordingSegment.Epsilon,
                SampleRate = sampleRate,
                WindowSize = frameSize,
                WindowStep = frameStep,
                WindowOverlap = frameOverlap,
                WindowPower = dspOutput1.WindowPower,
                Duration = recordingSegment.Duration,
            };

            // IMAGE 1) draw the WAVEFORM
            if (doWaveForm)
            {
                var minValues = dspOutput1.MinFrameValues;
                var maxValues = dspOutput1.MaxFrameValues;
                var waveformImage = GetWaveformImage(minValues, maxValues);

                // add in the title bar and time scales.
                string title = $"WAVEFORM (min value={dspOutput1.MinSignalValue:f3}, max value={dspOutput1.MaxSignalValue:f3})";
                var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, waveformImage.Width);
                var startTime = TimeSpan.Zero;
                var xAxisTicInterval = TimeSpan.FromSeconds(1);
                TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(frameStep / (double)sampleRate);
                var labelInterval = TimeSpan.FromSeconds(5);
                waveformImage = BaseSonogram.FrameSonogram(waveformImage, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
                list.Add(waveformImage);
            }

            // Draw various decibel spectrograms
            if (doDecibelSpectrogram || doNoiseReducedSpectrogram || doRidgeSpectrogram || doDifferenceSpectrogram)
            {
                // disable noise removal for first spectrogram
                var disabledNoiseReductionType = sonoConfig.NoiseReductionType;
                sonoConfig.NoiseReductionType = NoiseReductionType.None;

                //Get the decibel spectrogram
                var decibelSpectrogram = new SpectrogramStandard(sonoConfig, dspOutput1.AmplitudeSpectrogram);
                result.DecibelSpectrogram = decibelSpectrogram;
                double[,] dbSpectrogramData = (double[,])decibelSpectrogram.Data.Clone();

                // IMAGE 2) DecibelSpectrogram
                if (doDecibelSpectrogram)
                {
                    var image3 = decibelSpectrogram.GetImageFullyAnnotated($"DECIBEL SPECTROGRAM ({recordingName})");
                    list.Add(image3);
                }

                if (doNoiseReducedSpectrogram || doRidgeSpectrogram)
                {
                    sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                    sonoConfig.NoiseReductionParameter = configInfo.GetDouble("BgNoiseThreshold");
                    double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram.Data);
                    decibelSpectrogram.Data = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram.Data, spectralDecibelBgn);
                    decibelSpectrogram.Data = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram.Data, nhThreshold: 2.0);

                    // IMAGE 3) DecibelSpectrogram - noise reduced
                    if (doNoiseReducedSpectrogram)
                    {
                        var image4 = decibelSpectrogram.GetImageFullyAnnotated($"DECIBEL SPECTROGRAM + Lamel noise subtraction");
                        list.Add(image4);
                    }

                    // IMAGE 4) DecibelSpectrogram - annotated
                    if (doRidgeSpectrogram)
                    {
                        sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                        var image5 = GetDecibelSpectrogram_Ridges(dbSpectrogramData, decibelSpectrogram);
                        list.Add(image5);
                    }

                    // IMAGE 5) draw difference spectrogram
                    if (doDifferenceSpectrogram)
                    {
                        double threshold = 3.0;
                        var image6 = GetDifferenceSpectrogram(dbSpectrogramData, threshold);
                        image6 = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(image6, sampleRate, frameStep, "DECIBEL DIFFERENCE SPECTROGRAM");
                        list.Add(image6);
                    }
                }
            }

            // IMAGE 6) Cepstral Spectrogram
            if (doCepstralSpectrogram)
            {
                var image6 = GetCepstralSpectrogram(sonoConfig, recordingSegment);
                list.Add(image6);
            }

            // 7) SOX SPECTROGRAM
            if (doSoxSpectrogram)
            {
                Log.Warn("SoX spectrogram set to true but is ignored when running as an IAnalyzer");

                // The following parameters were once used to implement a sox spectrogram.
                //bool makeSoxSonogram = configuration.GetBoolOrNull(AnalysisKeys.MakeSoxSonogram) ?? false;
                //configDict[AnalysisKeys.SonogramTitle] = configuration[AnalysisKeys.SonogramTitle] ?? "Sonogram";
                //configDict[AnalysisKeys.SonogramComment] = configuration[AnalysisKeys.SonogramComment] ?? "Sonogram produced using SOX";
                //configDict[AnalysisKeys.SonogramColored] = configuration[AnalysisKeys.SonogramColored] ?? "false";
                //configDict[AnalysisKeys.SonogramQuantisation] = configuration[AnalysisKeys.SonogramQuantisation] ?? "128";
                //configDict[AnalysisKeys.AddTimeScale] = configuration[AnalysisKeys.AddTimeScale] ?? "true";
                //configDict[AnalysisKeys.AddAxes] = configuration[AnalysisKeys.AddAxes] ?? "true";
                //configDict[AnalysisKeys.AddSegmentationTrack] = configuration[AnalysisKeys.AddSegmentationTrack] ?? "true";
                //    var soxFile = new FileInfo(Path.Combine(output.FullName, sourceName + "SOX.png"));
                //    SpectrogramTools.MakeSonogramWithSox(sourceRecording, configDict, path2SoxSpectrogram);
                // list.Add(image7);
            }

            // 8) AmplitudeSpectrogram_LocalContrastNormalization
            if (doLcnSpectrogram)
            {
                var image8 = GetLcnSpectrogram(sonoConfig, recordingSegment);
                list.Add(image8);
            }

            // 9) EXPERIMENTAL
            if (doExperimentalSpectrogram)
            {
                var image8 = GetExperimentalSpectrogram(sonoConfig, recordingSegment);
                list.Add(image8);
            }

            // COMBINE THE SPECTROGRAM IMAGES
            result.CompositeImage = ImageTools.CombineImagesVertically(list);
            return result;
        }

        public static Image GetWaveformImage(double[] minValues, double[] maxValues)
        {
            var imageHeight = 180;
            var range = imageHeight / 2;
            var imageWidth = minValues.Length;
            var image = new Bitmap(imageWidth, imageHeight);
            var canvas = Graphics.FromImage(image);
            canvas.Clear(Color.Black);
            var pen = new Pen(Color.Lime);

            for (var i = 0; i < imageWidth; i++)
            {
                var y1 = range - (int)Math.Ceiling(minValues[i] * range);
                var y2 = range - (int)Math.Ceiling(maxValues[i] * range);
                canvas.DrawLine(pen, i, y1, i, y2);
            }

            // draw axis labels
            var pen2 = new Pen(Color.White);
            var pen3 = new Pen(Color.Black);
            canvas.DrawLine(pen3, 0, range, imageWidth, range);
            canvas.DrawLine(pen2, imageWidth / 2, 0, imageWidth / 2, imageHeight);
            var stringFont = new Font("Arial", 9);
            var brush = new SolidBrush(Color.LightGray);
            canvas.DrawString("+1.0", stringFont, brush, (imageWidth / 2) + 2, 10.0F);
            canvas.DrawString("-1.0", stringFont, brush, (imageWidth / 2) + 2, imageHeight - 20.0F);

            return image;
        }

        public static Image GetDifferenceSpectrogram(double[,] spectrogramData, double threshold)
        {
            var rowCount = spectrogramData.GetLength(0);
            var colCount = spectrogramData.GetLength(1);

            // set up new difference matrix
            var dM = new double[rowCount, colCount];
            for (var r = 1; r < rowCount; r++)
            {
                for (var c = 1; c < colCount; c++)
                {
                    var dx = spectrogramData[r, c] - spectrogramData[r - 1, c];
                    var dy = spectrogramData[r, c] - spectrogramData[r, c - 1];
                    var dd = spectrogramData[r, c] - spectrogramData[r - 1, c - 1];
                    dM[r, c] = dx;
                    if (dy > dx)
                    {
                        dM[r, c] = dy;
                    }

                    if (dd > dy)
                    {
                        dM[r, c] = dd;
                    }

                    if (dM[r, c] < threshold)
                    {
                        dM[r, c] = 0.0;
                    }
                }
            }

            var image = ImageTools.DrawMatrixInGrayScale(dM, 1, 1, false);
            image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }

        /// <summary>
        /// A FALSE-COLOR VERSION OF A standard scale SPECTROGRAM.
        /// </summary>
        /// <param name="dbSpectrogramData">The original data for decibel spectrogram.</param>
        /// <param name="nrSpectrogram">The noise-reduced spectrogram.</param>
        /// <returns>Image of spectrogram.</returns>
        public static Image GetDecibelSpectrogram_Ridges(double[,] dbSpectrogramData,  SpectrogramStandard nrSpectrogram)
        {
            // ########################### SOBEL ridge detection
            var ridgeThreshold = 3.5;
            var matrix = ImageTools.WienerFilter(dbSpectrogramData, 3);
            var hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);

            // ########################### EIGEN ridge detection
            //double ridgeThreshold = 6.0;
            //double dominanceThreshold = 0.7;
            //var rotatedData = MatrixTools.MatrixRotate90Anticlockwise(dbSpectrogramData);
            //byte[,] hits = RidgeDetection.StructureTensorRidgeDetection(rotatedData, ridgeThreshold, dominanceThreshold);
            //hits = MatrixTools.MatrixRotate90Clockwise(hits);
            // ########################### EIGEN ridge detection

            var frameStep = nrSpectrogram.Configuration.WindowStep;
            var sampleRate = nrSpectrogram.SampleRate;
            var image = SpectrogramTools.CreateFalseColourDecibelSpectrogram(dbSpectrogramData, nrSpectrogram.Data, hits);
            image = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(image, sampleRate, frameStep, "DECIBEL SPECTROGRAM - with ridges");
            //var image = decibelSpectrogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM - with ridges");
            return image;
        }

        public static Image GetCepstralSpectrogram(SonogramConfig sonoConfig, AudioRecording recording)
        {
            // TODO at present noise reduction type must be set = Standard.
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = 3.0;
            var cepgram = new SpectrogramCepstral(sonoConfig, recording.WavReader);
            var image = cepgram.GetImage();
            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram("CEPSTRO-GRAM", image.Width);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(sonoConfig.WindowStep / (double)sonoConfig.SampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            return image;
        }

        public static Image GetLcnSpectrogram(SonogramConfig sonoConfig, AudioRecording recordingSegment)
        {
            const double neighbourhoodSeconds = 0.25;

            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
            const double lcnContrastLevel = 0.001;
            LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
            const int lowPercentile = 20;
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);
            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalization)");
            return image;
        }

        public static Image GetExperimentalSpectrogram(SonogramConfig sonoConfig, AudioRecording recordingSegment)
        {
            //const double neighbourhoodSeconds = 0.25;
            //const double lcnContrastLevel = 0.001;

            //BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            //int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
            //LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            //LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);
            //const int lowPercentile = 20;
            //sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);
            //sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);

            //sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);
            //var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM + Bin LCN (Local Contrast Normalisation)");

            //double[,] matrix = ImageTools.WienerFilter(sonogram.Data, 3);
            //double ridgeThreshold = 0.25;
            //byte[,] hits = RidgeDetection.Sobel5X5RidgeDetectionExperiment(matrix, ridgeThreshold);
            //hits = RidgeDetection.JoinDisconnectedRidgesInMatrix(hits, matrix, ridgeThreshold);
            //image = SpectrogramTools.CreateFalseColourAmplitudeSpectrogram(spectrogramDataBeforeNoiseReduction, null, hits);
            //image = sonogram.GetImageAnnotatedWithLinearHerzScale(image, "AMPLITUDE SPECTROGRAM + LCN + ridge detection");
            return null;
        }
    }

    /// <summary>
    /// In line class used to return results from the static method Audio2Sonogram.GenerateFourSpectrogramImages().
    /// </summary>
    public class AudioToSonogramResult
    {
        public SpectrogramStandard DecibelSpectrogram { get; set; }

        // path to spectrogram image
        public FileInfo Path2SoxImage { get; set; }

        // Four spectrogram image
        public Image CompositeImage { get; set; }
    }

    /// <summary>
    /// This analyzer simply generates short (i.e. one minute) spectrograms and outputs them to CSV files.
    /// It does not accumulate data or other indices over a long recording.
    /// </summary>
    public class SpectrogramAnalyzer : IAnalyser2
    {
        //private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        public AnalyzerConfig ParseConfig(FileInfo file)
        {
            return ConfigFile.Deserialize<AnalyzerConfig>(file);
        }

        public void BeforeAnalyze(AnalysisSettings analysisSettings)
        {
            // noop
        }

        public AnalysisResult2 Analyze<T>(AnalysisSettings analysisSettings, SegmentSettings<T> segmentSettings)
        {
            var audioFile = segmentSettings.SegmentAudioFile;
            var recording = new AudioRecording(audioFile.FullName);
            var outputDirectory = segmentSettings.SegmentOutputDirectory;
            bool saveCsv = analysisSettings.AnalysisDataSaveBehavior;
            var analysisResult = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);
            var configInfo = ConfigFile.Deserialize<AnalyzerConfig>(analysisSettings.ConfigFile);
            var spectrogramResult = Audio2Sonogram.GenerateSpectrogramImages(audioFile, configInfo);

            // this analysis produces no results! But we still print images (that is the point)
            // if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResult.Events.Length))
            // {
            //     Debug.Assert(condition: segmentSettings.SegmentImageFile.Exists, "Warning: Image file must exist.");
            spectrogramResult.CompositeImage.Save(segmentSettings.SegmentImageFile.FullName, ImageFormat.Png);
            // }

            //if (saveCsv)
            //{
            //    var basename = Path.GetFileNameWithoutExtension(segmentSettings.SegmentAudioFile.Name);
            //    var spectrogramCsvFile = outputDirectory.CombineFile(basename + ".Spectrogram.csv");
            //    Csv.WriteMatrixToCsv(spectrogramCsvFile, spectrogramResult.DecibelSpectrogram.Data, TwoDimensionalArray.None);
            //}

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