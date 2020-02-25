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
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Acoustics.Shared;
    using Acoustics.Shared.ConfigFile;
    using Acoustics.Shared.ImageSharp;
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
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using Path = System.IO.Path;

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
            int resampleRate = configInfo.GetIntOrNull("ResampleRate") ?? 22050;
            var tempAudioSegment = AudioRecording.CreateTemporaryAudioFile(sourceRecording, output, resampleRate);

            // 4: GENERATE SPECTROGRAM images
            //string sourceName = sourceRecording.FullName;
            string sourceName = Path.GetFileNameWithoutExtension(sourceRecording.FullName);
            var result = GenerateSpectrogramImages(tempAudioSegment, configInfo, sourceName);

            // 5: Save the image
            var outputImageFile = new FileInfo(Path.Combine(output.FullName, sourceName + ".Spectrograms.png"));
            result.CompositeImage.Save(outputImageFile.FullName);
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
        /// <param name="sourceRecording">The name of the original recording.</param>
        /// <param name="configInfo">Contains parameter info to make spectrograms.</param>
        /// <param name="sourceRecordingName">.Name of source recording. Required only spectrogram labels.</param>
        public static AudioToSonogramResult GenerateSpectrogramImages(
            FileInfo sourceRecording,
            AnalyzerConfig configInfo,
            string sourceRecordingName)
        {
            //int signalLength = recordingSegment.WavReader.GetChannel(0).Length;
            var recordingSegment = new AudioRecording(sourceRecording.FullName);
            int sampleRate = recordingSegment.WavReader.SampleRate;
            var result = new AudioToSonogramResult();

            bool doWaveForm = configInfo.GetBoolOrNull("Waveform") ?? false;
            bool doDecibelSpectrogram = configInfo.GetBoolOrNull("DecibelSpectrogram") ?? false;
            bool doNoiseReducedSpectrogram = configInfo.GetBoolOrNull("DecibelSpectrogram_NoiseReduced") ?? true;
            bool doDifferenceSpectrogram = configInfo.GetBoolOrNull("DifferenceSpectrogram") ?? false;
            bool doLcnSpectrogram = configInfo.GetBoolOrNull("AmplitudeSpectrogram_LocalContrastNormalization") ?? false;
            bool doCepstralSpectrogram = configInfo.GetBoolOrNull("CepstralSpectrogram") ?? false;
            bool doExperimentalSpectrogram = configInfo.GetBoolOrNull("Experimental") ?? false;

            int frameSize = configInfo.GetIntOrNull("FrameLength") ?? 512;
            int frameStep = configInfo.GetIntOrNull("FrameStep") ?? 0;

            // must calculate this because used later on.
            double frameOverlap = (frameSize - frameStep) / (double)frameSize;

            // Default noiseReductionType = Standard
            var bgNoiseThreshold = configInfo.GetDoubleOrNull("BgNoiseThreshold") ?? 3.0;

            // threshold for drawing the difference spectrogram
            var differenceThreshold = configInfo.GetDoubleOrNull("DifferenceThreshold") ?? 3.0;

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
                NoiseReductionType = NoiseReductionType.Standard,
                NoiseReductionParameter = bgNoiseThreshold,
            };

            // IMAGE 1) draw the WAVEFORM
            Image<Rgb24> waveformImage = null;
            if (doWaveForm)
            {
                var minValues = dspOutput1.MinFrameValues;
                var maxValues = dspOutput1.MaxFrameValues;
                int height = configInfo.GetIntOrNull("WaveformHeight") ?? 180;
                waveformImage = GetWaveformImage(minValues, maxValues, height);

                // add in the title bar and time scales.
                string title = $"WAVEFORM - {sourceRecordingName} (min value={dspOutput1.MinSignalValue:f3}, max value={dspOutput1.MaxSignalValue:f3})";
                var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(title, waveformImage.Width);
                var startTime = TimeSpan.Zero;
                var xAxisTicInterval = TimeSpan.FromSeconds(1);
                TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(frameStep / (double)sampleRate);
                var labelInterval = TimeSpan.FromSeconds(5);
                waveformImage = BaseSonogram.FrameSonogram(waveformImage, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            }

            // Draw various decibel spectrograms
            Image<Rgb24> decibelImage = null;
            Image<Rgb24> noiseReducedImage = null;
            Image<Rgb24> differenceImage = null;
            Image<Rgb24> experimentalImage = null;
            Image<Rgb24> ceptralImage = null;
            Image<Rgb24> lcnImage = null;

            if (doDecibelSpectrogram || doNoiseReducedSpectrogram || doDifferenceSpectrogram || doExperimentalSpectrogram)
            {
                // disable noise removal for first two spectrograms
                var disabledNoiseReductionType = sonoConfig.NoiseReductionType;
                sonoConfig.NoiseReductionType = NoiseReductionType.None;

                //Get the decibel spectrogram
                var decibelSpectrogram = new SpectrogramStandard(sonoConfig, dspOutput1.AmplitudeSpectrogram);
                result.DecibelSpectrogram = decibelSpectrogram;
                double[,] dbSpectrogramData = (double[,])decibelSpectrogram.Data.Clone();

                // IMAGE 2) Display the DecibelSpectrogram
                if (doDecibelSpectrogram)
                {
                    decibelImage = decibelSpectrogram.GetImageFullyAnnotated($"DECIBEL SPECTROGRAM ({sourceRecordingName})");
                }

                if (doNoiseReducedSpectrogram || doExperimentalSpectrogram || doCepstralSpectrogram)
                {
                    sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                    sonoConfig.NoiseReductionParameter = bgNoiseThreshold;
                    double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram.Data);
                    decibelSpectrogram.Data = SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram.Data, spectralDecibelBgn);
                    decibelSpectrogram.Data = SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram.Data, nhThreshold: bgNoiseThreshold);

                    // IMAGE 3) DecibelSpectrogram - noise reduced
                    if (doNoiseReducedSpectrogram)
                    {
                        noiseReducedImage = decibelSpectrogram.GetImageFullyAnnotated($"DECIBEL SPECTROGRAM + Lamel noise subtraction. ({sourceRecordingName})");
                    }

                    // IMAGE 4) EXPERIMENTAL Spectrogram
                    if (doExperimentalSpectrogram)
                    {
                        sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                        experimentalImage = GetDecibelSpectrogram_Ridges(dbSpectrogramData, decibelSpectrogram, sourceRecordingName);
                    }
                }

                // IMAGE 5) draw difference spectrogram. This is derived from the original decibel spectrogram
                if (doDifferenceSpectrogram)
                {
                    //var differenceThreshold = configInfo.GetDoubleOrNull("DifferenceThreshold") ?? 3.0;
                    differenceImage = GetDifferenceSpectrogram(dbSpectrogramData, differenceThreshold);
                    differenceImage = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(differenceImage, sampleRate, frameStep, $"DECIBEL DIFFERENCE SPECTROGRAM ({sourceRecordingName})");
                }
            }

            // IMAGE 6) Cepstral Spectrogram
            if (doCepstralSpectrogram)
            {
                ceptralImage = GetCepstralSpectrogram(sonoConfig, recordingSegment, sourceRecordingName);
            }

            // IMAGE 7) AmplitudeSpectrogram_LocalContrastNormalization
            if (doLcnSpectrogram)
            {
                var neighbourhoodSeconds = configInfo.GetDoubleOrNull("NeighbourhoodSeconds") ?? 0.5;
                var lcnContrastParameter = configInfo.GetDoubleOrNull("LcnContrastLevel") ?? 0.2;
                lcnImage = GetLcnSpectrogram(sonoConfig, recordingSegment, sourceRecordingName, neighbourhoodSeconds, lcnContrastParameter);
            }

            // COMBINE THE SPECTROGRAM IMAGES
            var list = new List<Image<Rgb24>>();
            if (waveformImage != null) { list.Add(waveformImage); }
            if (decibelImage != null) { list.Add(decibelImage); }
            if (noiseReducedImage != null) { list.Add(noiseReducedImage); }
            if (experimentalImage != null) { list.Add(experimentalImage); }
            if (differenceImage != null) { list.Add(differenceImage); }
            if (ceptralImage != null) { list.Add(ceptralImage); }
            if (lcnImage != null) { list.Add(lcnImage); }
            result.CompositeImage = ImageTools.CombineImagesVertically(list);
            //result.CompositeImage = ImageTools.CombineImagesVertically(waveformImage, decibelImage, noiseReducedImage, experimentalImage, differenceImage, ceptralImage, lcnImage);
            return result;
        }

        public static Image<Rgb24> GetWaveformImage(double[] minValues, double[] maxValues, int imageHeight)
        {
            var range = imageHeight / 2;
            var imageWidth = minValues.Length;
            var image = Drawing.NewImage(imageWidth, imageHeight, Color.Black);
            var pen = Color.Lime.ToPen();

            image.Mutate(canvas =>
            {
                for (var i = 0; i < imageWidth; i++)
                {
                    var y1 = range - (int)Math.Ceiling(minValues[i] * range);
                    var y2 = range - (int)Math.Ceiling(maxValues[i] * range);
                    canvas.DrawLine(pen, i, y1, i, y2);
                }

                // draw axis labels
                var pen2 = Color.White.ToPen();
                var pen3 = Color.Black.ToPen();
                canvas.DrawLine(pen3, 0, range, imageWidth, range);
                canvas.DrawLine(pen2, imageWidth / 2, 0, imageWidth / 2, imageHeight);
                var stringFont = Drawing.Arial9;
                var brush = Color.LightGray;
                canvas.DrawText("+1.0", stringFont, brush, new PointF((imageWidth / 2) + 2, 10.0f));
                canvas.DrawText("-1.0", stringFont, brush, new PointF((imageWidth / 2) + 2, imageHeight - 20.0f));
            });

            return image;
        }

        public static Image<Rgb24> GetDifferenceSpectrogram(double[,] spectrogramData, double threshold)
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
                    var dpd = spectrogramData[r, c] - spectrogramData[r - 1, c - 1];

                    //var dy2 = spectrogramData[r, c] - spectrogramData[r, c + 1];
                    //var dnd = spectrogramData[r, c] - spectrogramData[r - 1, c + 1];
                    dM[r, c] = dx;
                    if (dy > dx)
                    {
                        dM[r, c] = dy;
                    }

                    if (dpd > dy)
                    {
                        dM[r, c] = dpd;
                    }

                    //if (dnd > dpd)
                    //{
                    //    dM[r, c] = dnd;
                    //}

                    //if (dy2 > dpd)
                    //{
                    //    dM[r, c] = dy2;
                    //}

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
        /// AN EXPERIMENTAL SPECTROGRAM - A FALSE-COLOR VERSION OF A standard scale SPECTROGRAM.
        /// </summary>
        /// <param name="dbSpectrogramData">The original data for decibel spectrogram.</param>
        /// <param name="nrSpectrogram">The noise-reduced spectrogram.</param>
        /// <param name="sourceRecordingName">Name of the source file. Required only to add label to spectrogram.</param>
        /// <returns>Image of spectrogram.</returns>
        public static Image<Rgb24> GetDecibelSpectrogram_Ridges(double[,] dbSpectrogramData,  SpectrogramStandard nrSpectrogram, string sourceRecordingName)
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
            image = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(image, sampleRate, frameStep, $"AN EXPERIMENTAL DECIBEL SPECTROGRAM with ridges ({sourceRecordingName})");
            //var image = decibelSpectrogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM - with ridges");
            return image;
        }

        public static Image<Rgb24> GetCepstralSpectrogram(SonogramConfig sonoConfig, AudioRecording recording, string sourceRecordingName)
        {
            // TODO at present noise reduction type must be set = Standard.
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = 3.0;
            var cepgram = new SpectrogramCepstral(sonoConfig, recording.WavReader);
            var image = cepgram.GetImage();
            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram("CEPSTRO-GRAM " + sourceRecordingName, image.Width);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(sonoConfig.WindowStep / (double)sonoConfig.SampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            return image;
        }

        public static Image<Rgb24> GetLcnSpectrogram(SonogramConfig sonoConfig, AudioRecording recordingSegment, string sourceRecordingName, double neighbourhoodSeconds, double lcnContrastLevel)
        {
            BaseSonogram sonogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            int neighbourhoodFrames = (int)(sonogram.FramesPerSecond * neighbourhoodSeconds);
            LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", sonogram.FramesPerSecond);
            LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);

            // subtract the lowest 20% of frames. This is first step in LCN noise removal. Sets the baseline.
            const int lowPercentile = 20;
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);
            sonogram.Data = NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(sonogram.Data, neighbourhoodFrames, lcnContrastLevel);

            //Matrix normalisation
            //MatrixTools.PercentileCutoffs(sonogram.Data, 10.0, 90, out double minCut, out double maxCut);
            //NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(sonogram.Data, lowPercentile);

            var image = sonogram.GetImageFullyAnnotated("AMPLITUDE SPECTROGRAM with freq bin Local Contrast Normalization - " + sourceRecordingName);
            return image;
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
            var sourceRecordingName = recording.BaseName;
            var configuration = analysisSettings.Configuration as AnalyzerConfig;

//            if (analysisSettings.Configuration.GetBool(AnalysisKeys.MakeSoxSonogram))
//            {
//                Log.Warn("SoX spectrogram generation config variable found (and set to true) but is ignored when running as an IAnalyzer");
//            }

            var analysisResult = new AnalysisResult2(analysisSettings, segmentSettings, recording.Duration);

            var spectrogramResult = Audio2Sonogram.GenerateSpectrogramImages(audioFile, configuration, sourceRecordingName);

            // this analysis produces no results! But we still print images (that is the point)
            // if (analysisSettings.AnalysisImageSaveBehavior.ShouldSave(analysisResult.Events.Length))
            // {
            //     Debug.Assert(condition: segmentSettings.SegmentImageFile.Exists, "Warning: Image file must exist.");
            spectrogramResult.CompositeImage.Save(segmentSettings.SegmentImageFile.FullName);
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