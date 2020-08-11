// <copyright file="SpectrogramGenerator.Core.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AnalysisPrograms.SpectrogramGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Acoustics.Shared.ColorScales;
    using Acoustics.Shared.ImageSharp;
    using AudioAnalysisTools;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.StandardSpectrograms;
    using AudioAnalysisTools.WavTools;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;
    using static SpectrogramImageType;

    public partial class SpectrogramGenerator
    {
        internal static readonly IDictionary<SpectrogramImageType, Color> ImageTags;

        static SpectrogramGenerator()
        {
            var values = (SpectrogramImageType[])Enum.GetValues(typeof(SpectrogramImageType));
            ImageTags = values
                .Zip(ColorBrewer.Qualitative.Set1.ForClassCount(values.Length))
                .ToImmutableDictionary(x => x.First, x => x.Second);
        }

        /// <summary>
        /// Calculates the following spectrograms as per settings in the Images array in the config file: Towsey.SpectrogramGenerator.yml:
        /// Waveform.
        /// DecibelSpectrogram.
        /// DecibelSpectrogramNoiseReduced.
        /// CepstralSpectrogram.
        /// DifferenceSpectrogram.
        /// AmplitudeSpectrogramLocalContrastNormalization.
        /// Experimental.
        /// Comment the config.yml file with a hash, those spectrograms that are not required.
        /// </summary>
        /// <param name="sourceRecording">The name of the original recording.</param>
        /// <param name="config">Contains parameter info to make spectrograms.</param>
        /// <param name="sourceRecordingName">.Name of source recording. Required only spectrogram labels.</param>
        public static AudioToSonogramResult GenerateSpectrogramImages(
            FileInfo sourceRecording,
            SpectrogramGeneratorConfig config,
            string sourceRecordingName)
        {
            //int signalLength = recordingSegment.WavReader.GetChannel(0).Length;
            var recordingSegment = new AudioRecording(sourceRecording.FullName);
            int sampleRate = recordingSegment.WavReader.SampleRate;
            var result = new AudioToSonogramResult();

            var requestedImageTypes = config.Images ?? new[] { SpectrogramImageType.DecibelSpectrogram };
            var @do = requestedImageTypes.ToHashSet();

            int frameSize = config.GetIntOrNull("FrameLength") ?? 512;
            int frameStep = config.GetIntOrNull("FrameStep") ?? 441;

            // must calculate this because used later on.
            double frameOverlap = (frameSize - frameStep) / (double)frameSize;

            // Default noiseReductionType = Standard
            var bgNoiseThreshold = config.BgNoiseThreshold;

            // threshold for drawing the difference spectrogram
            var differenceThreshold = config.DifferenceThreshold;

            // EXTRACT ENVELOPE and SPECTROGRAM FROM RECORDING SEGMENT
            var dspOutput1 = DSP_Frames.ExtractEnvelopeAndFfts(recordingSegment, frameSize, frameStep);

            // This constructor initializes default values for Melscale and Mfcc spectrograms and other parameters.
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

            var images = new Dictionary<SpectrogramImageType, Image<Rgb24>>(requestedImageTypes.Length);

            // IMAGE 1) draw the WAVEFORM
            if (@do.Contains(Waveform))
            {
                var minValues = dspOutput1.MinFrameValues;
                var maxValues = dspOutput1.MaxFrameValues;
                int height = config.WaveformHeight;
                var waveformImage = GetWaveformImage(minValues, maxValues, height);

                // add in the title bar and time scales.
                string title =
                    $"WAVEFORM - {sourceRecordingName} (min value={dspOutput1.MinSignalValue:f3}, max value={dspOutput1.MaxSignalValue:f3})";
                var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(
                    title,
                    waveformImage.Width,
                    ImageTags[Waveform]);
                var startTime = TimeSpan.Zero;
                var xAxisTicInterval = TimeSpan.FromSeconds(1);
                TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(frameStep / (double)sampleRate);
                var labelInterval = TimeSpan.FromSeconds(5);
                waveformImage = BaseSonogram.FrameSonogram(
                    waveformImage,
                    titleBar,
                    startTime,
                    xAxisTicInterval,
                    xAxisPixelDuration,
                    labelInterval);
                images.Add(Waveform, waveformImage);
            }

            // Draw various decibel spectrograms
            var decibelTypes = new[] { SpectrogramImageType.DecibelSpectrogram, DecibelSpectrogramNoiseReduced, DifferenceSpectrogram, Experimental };
            if (@do.Overlaps(decibelTypes))
            {
                // disable noise removal for first two spectrograms
                var disabledNoiseReductionType = sonoConfig.NoiseReductionType;
                sonoConfig.NoiseReductionType = NoiseReductionType.None;

                //Get the decibel spectrogram
                var decibelSpectrogram = new SpectrogramStandard(sonoConfig, dspOutput1.AmplitudeSpectrogram);
                result.DecibelSpectrogram = decibelSpectrogram;
                double[,] dbSpectrogramData = (double[,])decibelSpectrogram.Data.Clone();

                // IMAGE 2) Display the DecibelSpectrogram
                if (@do.Contains(SpectrogramImageType.DecibelSpectrogram))
                {
                    images.Add(
                        SpectrogramImageType.DecibelSpectrogram,
                        decibelSpectrogram.GetImageFullyAnnotated(
                            $"DECIBEL SPECTROGRAM ({sourceRecordingName})",
                            ImageTags[SpectrogramImageType.DecibelSpectrogram]));
                }

                if (@do.Overlaps(new[] { DecibelSpectrogramNoiseReduced, Experimental, CepstralSpectrogram }))
                {
                    sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                    sonoConfig.NoiseReductionParameter = bgNoiseThreshold;
                    double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(decibelSpectrogram.Data);
                    decibelSpectrogram.Data =
                        SNR.TruncateBgNoiseFromSpectrogram(decibelSpectrogram.Data, spectralDecibelBgn);
                    decibelSpectrogram.Data =
                        SNR.RemoveNeighbourhoodBackgroundNoise(decibelSpectrogram.Data, nhThreshold: bgNoiseThreshold);

                    // IMAGE 3) DecibelSpectrogram - noise reduced
                    if (@do.Contains(DecibelSpectrogramNoiseReduced))
                    {
                        images.Add(
                            DecibelSpectrogramNoiseReduced,
                            decibelSpectrogram.GetImageFullyAnnotated(
                                $"DECIBEL SPECTROGRAM + Lamel noise subtraction. ({sourceRecordingName})",
                                ImageTags[DecibelSpectrogramNoiseReduced]));
                    }

                    // IMAGE 4) EXPERIMENTAL Spectrogram
                    if (@do.Contains(Experimental))
                    {
                        sonoConfig.NoiseReductionType = disabledNoiseReductionType;
                        images.Add(
                            Experimental,
                            GetDecibelSpectrogram_Ridges(
                                dbSpectrogramData,
                                decibelSpectrogram,
                                sourceRecordingName));
                    }
                }

                // IMAGE 5) draw difference spectrogram. This is derived from the original decibel spectrogram
                if (@do.Contains(DifferenceSpectrogram))
                {
                    //var differenceThreshold = configInfo.GetDoubleOrNull("DifferenceThreshold") ?? 3.0;
                    var differenceImage = GetDifferenceSpectrogram(dbSpectrogramData, differenceThreshold);
                    differenceImage = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(
                        differenceImage,
                        sampleRate,
                        frameStep,
                        $"DECIBEL DIFFERENCE SPECTROGRAM ({sourceRecordingName})",
                        ImageTags[DifferenceSpectrogram]);
                    images.Add(DifferenceSpectrogram, differenceImage);
                }
            }

            // IMAGE 6) Mel-frequency Spectrogram
            // The default spectrogram has 64 frequency bands.
            if (@do.Contains(MelScaleSpectrogram))
            {
                images.Add(
                    MelScaleSpectrogram,
                    GetMelScaleSpectrogram(sonoConfig, recordingSegment, sourceRecordingName));
            }

            // IMAGE 7) Cepstral Spectrogram
            if (@do.Contains(CepstralSpectrogram))
            {
                images.Add(
                    CepstralSpectrogram,
                    GetCepstralSpectrogram(sonoConfig, recordingSegment, sourceRecordingName));
            }

            // IMAGE 8) Octave-frequency scale Spectrogram
            if (@do.Contains(OctaveScaleSpectrogram))
            {
                //Create new config because calling the octave spectrogram changes it.
                var octaveConfig = new SonogramConfig()
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

                // Here are some of the possible octave frequency scales.
                //var freqScale = new FrequencyScale(FreqScaleType.LinearOctaveStandard);
                //var freqScale = new FrequencyScale(FreqScaleType.OctaveDataReduction);
                //var freqScale = new FrequencyScale(FreqScaleType.Linear62OctaveTones31Nyquist11025);
                //var freqScale = new FrequencyScale(FreqScaleType.Linear125OctaveTones32Nyquist11025);

                images.Add(
                    OctaveScaleSpectrogram,
                    GetOctaveScaleSpectrogram(octaveConfig, FreqScaleType.Linear62OctaveTones31Nyquist11025, recordingSegment, sourceRecordingName));
            }

            // IMAGE 9) AmplitudeSpectrogram_LocalContrastNormalization
            if (@do.Contains(AmplitudeSpectrogramLocalContrastNormalization))
            {
                var neighborhoodSeconds = config.NeighborhoodSeconds;
                var lcnContrastParameter = config.LcnContrastLevel;
                images.Add(
                    AmplitudeSpectrogramLocalContrastNormalization,
                    GetLcnSpectrogram(
                        sonoConfig,
                        recordingSegment,
                        sourceRecordingName,
                        neighborhoodSeconds,
                        lcnContrastParameter));
            }

            // now pick and combine images in order user specified
            var sortedImages = requestedImageTypes.Select(x => images[x]);

            // COMBINE THE SPECTROGRAM IMAGES
            result.CompositeImage = ImageTools.CombineImagesVertically(sortedImages.ToArray());
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
                canvas.DrawTextSafe("+1.0", stringFont, brush, new PointF((imageWidth / 2) + 2, 10.0f));
                canvas.DrawTextSafe("-1.0", stringFont, brush, new PointF((imageWidth / 2) + 2, imageHeight - 20.0f));
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
        public static Image<Rgb24> GetDecibelSpectrogram_Ridges(
            double[,] dbSpectrogramData,
            SpectrogramStandard nrSpectrogram,
            string sourceRecordingName)
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
            image = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(
                image,
                sampleRate,
                frameStep,
                $"AN EXPERIMENTAL DECIBEL SPECTROGRAM with ridges ({sourceRecordingName})",
                ImageTags[Experimental]);

            //var image = decibelSpectrogram.GetImageFullyAnnotated("DECIBEL SPECTROGRAM - with ridges");
            return image;
        }

        public static Image<Rgb24> GetMelScaleSpectrogram(
            SonogramConfig sonoConfig,
            AudioRecording recording,
            string sourceRecordingName)
        {
            // TODO at present noise reduction type must be set = Standard.
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = 3.0;
            var melFreqGram = new SpectrogramMelScale(sonoConfig, recording.WavReader);
            var image = melFreqGram.GetImage();
            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(
                    "MEL-FREQUENCY SPECTROGRAM " + sourceRecordingName,
                    image.Width,
                    ImageTags[CepstralSpectrogram]);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(sonoConfig.WindowStep / (double)sonoConfig.SampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            return image;
        }

        public static Image<Rgb24> GetCepstralSpectrogram(
            SonogramConfig sonoConfig,
            AudioRecording recording,
            string sourceRecordingName)
        {
            // TODO at present noise reduction type must be set = Standard.
            sonoConfig.NoiseReductionType = NoiseReductionType.Standard;
            sonoConfig.NoiseReductionParameter = 3.0;
            var cepgram = new SpectrogramCepstral(sonoConfig, recording.WavReader);
            var image = cepgram.GetImage();
            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(
                    "CEPSTRO-GRAM " + sourceRecordingName,
                    image.Width,
                    ImageTags[CepstralSpectrogram]);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(sonoConfig.WindowStep / (double)sonoConfig.SampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            return image;
        }

        public static Image<Rgb24> GetOctaveScaleSpectrogram(
        SonogramConfig sgConfig,
        FreqScaleType fst,
        AudioRecording recording,
        string sourceRecordingName)
        {
            var freqScale = new FrequencyScale(fst)
            {
                WindowStep = 512,
            };

            // ensure that the freq scale and the config are consistent.
            sgConfig.WindowSize = freqScale.WindowSize;
            freqScale.WindowStep = sgConfig.WindowStep;
            sgConfig.WindowOverlap = SonogramConfig.CalculateFrameOverlap(freqScale.WindowSize, freqScale.WindowStep);

            // TODO at present noise reduction type must be set = Standard.
            sgConfig.NoiseReductionType = NoiseReductionType.Standard;
            sgConfig.NoiseReductionParameter = 3.0;

            var octaveScaleGram = new SpectrogramOctaveScale(sgConfig, freqScale, recording.WavReader);
            var image = octaveScaleGram.GetImage();
            var titleBar = BaseSonogram.DrawTitleBarOfGrayScaleSpectrogram(
                    "OCTAVE-SCALE SPECTROGRAM " + sourceRecordingName,
                    image.Width,
                    ImageTags[CepstralSpectrogram]);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(sgConfig.WindowStep / (double)sgConfig.SampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            image = BaseSonogram.FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval);
            return image;
        }

        public static Image<Rgb24> GetLcnSpectrogram(
            SonogramConfig sonoConfig,
            AudioRecording recordingSegment,
            string sourceRecordingName,
            double neighbourhoodSeconds,
            double lcnContrastLevel)
        {
            BaseSonogram spectrogram = new AmplitudeSonogram(sonoConfig, recordingSegment.WavReader);
            int neighbourhoodFrames = (int)(spectrogram.FramesPerSecond * neighbourhoodSeconds);
            LoggedConsole.WriteLine("LCN: FramesPerSecond (Prior to LCN) = {0}", spectrogram.FramesPerSecond);
            LoggedConsole.WriteLine("LCN: Neighbourhood of {0} seconds = {1} frames", neighbourhoodSeconds, neighbourhoodFrames);

            // subtract the lowest 20% of frames. This is first step in LCN noise removal. Sets the baseline.
            const int lowPercentile = 20;
            spectrogram.Data =
                NoiseRemoval_Briggs.NoiseReduction_byLowestPercentileSubtraction(spectrogram.Data, lowPercentile);
            spectrogram.Data =
                NoiseRemoval_Briggs.NoiseReduction_byLCNDivision(spectrogram.Data, neighbourhoodFrames, lcnContrastLevel);

            // Finally background noise removal. This step is optional.
            double[] spectralDecibelBgn = NoiseProfile.CalculateBackgroundNoise(spectrogram.Data);
            spectrogram.Data = SNR.TruncateBgNoiseFromSpectrogram(spectrogram.Data, spectralDecibelBgn);

            var image = spectrogram.GetImageFullyAnnotated(
                "AMPLITUDE SPECTROGRAM with freq bin Local Contrast Normalization - " + sourceRecordingName,
                ImageTags[AmplitudeSpectrogramLocalContrastNormalization]);
            return image;
        }
    }
}