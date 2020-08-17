// <copyright file="BaseSonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Acoustics.Shared;
    using Acoustics.Shared.ImageSharp;
    using Acoustics.Tools.Wav;
    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.WavTools;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;
    using TowseyLibrary;

    /// <summary>
    /// Base Sonogram.
    /// </summary>
    public abstract partial class BaseSonogram
    {
        /// <summary>
        /// Gets or sets the config information.
        /// The Configuration object should contain all the parameters required to construct an amplitude spectrogram given a recording.
        /// </summary>
        public SonogramConfig Configuration { get; set; }

        /// <summary>
        /// Gets or sets the frequency scale information.
        /// The FreqScale object should contain all the parameters required to convert the linear frquency scale of the amplitude spectrogram
        /// into any reduced or non-linear frequency scale required.
        /// </summary>
        public FrequencyScale FreqScale { get; set; }

        public double MaxAmplitude { get; set; }

        public int SampleRate { get; set; }

        public TimeSpan Duration { get; protected set; }

        // the following values are dependent on sampling rate.
        public int NyquistFrequency => this.SampleRate / 2;

        // Duration of full frame or window in seconds
        public double FrameDuration => this.Configuration.WindowSize / (double)this.SampleRate;

        // Duration of non-overlapped part of window/frame in seconds
        public double FrameStep => this.Configuration.GetFrameOffset(this.SampleRate);

        public double FBinWidth => this.NyquistFrequency / (double)this.Configuration.FreqBinCount;

        public double FramesPerSecond => 1 / this.FrameStep;

        public int FrameCount { get; protected set; } //Temporarily set to (int)(Duration.TotalSeconds/FrameOffset) then reset later

        /// <summary>
        /// Gets or sets instance of class SNR that stores info about signal energy and dB per frame.
        /// </summary>
        public SNR SnrData { get; set; }

        /// <summary>
        /// Gets or sets decibels per signal frame.
        /// </summary>
        public double[] DecibelsPerFrame { get; set; }

        public double[] DecibelsNormalised { get; set; }

        /// <summary>
        /// Gets or sets the Noise profile in decibels.
        /// </summary>
        public double[] ModalNoiseProfile { get; set; }

        /// <summary>
        /// Gets or sets decibel reference with which to NormaliseMatrixValues the dB values for MFCCs.
        /// </summary>
        public double DecibelReference { get; protected set; }

        /// <summary>
        /// Gets or sets integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
        /// </summary>
        public int[] SigState { get; protected set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles.
        /// </summary>
        public double[,] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// BASE CONSTRUCTOR: Use this when want to extract time segment of existing sonogram.
        /// </summary>
        /// <param name="config">config file to use.</param>
        public BaseSonogram(SonogramConfig config)
        {
            this.Configuration = config;
            this.Duration = config.Duration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// BASE CONSTRUCTOR.
        /// </summary>
        /// <param name="config">config file to use.</param>
        /// <param name="wav">wav.</param>
        public BaseSonogram(SonogramConfig config, WavReader wav)
            : this(config)
        {
            this.InitialiseSpectrogram(wav);

            // this Make() call makes the desired spectrogram.
            this.Make(this.Data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// BASE CONSTRUCTOR.
        /// </summary>
        /// <param name="config">config file to use.</param>
        /// <param name="wav">wav.</param>
        public BaseSonogram(SonogramConfig config, FrequencyScale freqScale, WavReader wav)
            : this(config)
        {
            // check that the frameWidths are consistent.
            if (config.WindowSize != freqScale.WindowSize)
            {
                throw new Exception("BaseSonogram: CONSTRUCTOR ERROR: Inconsistency in Frequency Scale conversion data.");
            }

            this.FreqScale = freqScale;
            this.InitialiseSpectrogram(wav);
            this.Make(this.Data);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// Use this BASE CONSTRUCTOR when already have the amplitude spectrogram in matrix.
        /// Init normalised signal energy array but do nothing with it. This has to be done from outside.
        /// </summary>
        /// <param name="config">the spectrogram config.</param>
        /// <param name="amplitudeSpectrogramData">data of an amplitude Spectrogram.</param>
        public BaseSonogram(SonogramConfig config, double[,] amplitudeSpectrogramData)
        {
            this.Configuration = config;
            this.FrameCount = amplitudeSpectrogramData.GetLength(0);
            this.SampleRate = this.Configuration.SampleRate;

            //init normalised signal energy array but do nothing with it. This has to be done from outside
            this.DecibelsNormalised = new double[this.FrameCount];
            this.Data = amplitudeSpectrogramData;
        }

        public abstract void Make(double[,] amplitudeM);

        /// <summary>
        /// This method creates the amplitude spectrogram.
        /// </summary>
        private void InitialiseSpectrogram(WavReader wav)
        {
            this.Duration = wav.Time;
            double minDuration = 0.2;
            if (this.Duration.TotalSeconds < minDuration)
            {
                LoggedConsole.WriteLine("Signal must at least {0} seconds long to produce a sonogram!", minDuration);
                return;
            }

            //set config params to the current recording
            this.SampleRate = wav.SampleRate;
            this.Configuration.Duration = wav.Time;
            this.Configuration.SampleRate = wav.SampleRate; //also set the Nyquist
            this.MaxAmplitude = wav.CalculateMaximumAmplitude();

            var recording = new AudioRecording(wav);
            var fftData = DSP_Frames.ExtractEnvelopeAndFfts(
                recording,
                this.Configuration.WindowSize,
                this.Configuration.WindowOverlap,
                this.Configuration.WindowFunction);

            // now recover required data
            //epsilon is a signal dependent minimum amplitude value to prevent possible subsequent log of zero value.
            this.Configuration.epsilon = fftData.Epsilon;
            this.Configuration.WindowPower = fftData.WindowPower;
            this.FrameCount = fftData.FrameCount;
            this.DecibelsPerFrame = fftData.FrameDecibels;

            //init normalised signal energy array but do nothing with it. This has to be done from outside
            this.DecibelsNormalised = new double[this.FrameCount];

            this.Data = fftData.AmplitudeSpectrogram;

            // currently DoSnr = true by default
            if (this.Configuration.DoSnr)
            {
                this.CalculateSnrData(fftData.FractionOfHighEnergyFrames);
            }
        }

        /// <summary>
        /// Calculates SNR, ENERGY PER FRAME and NORMALISED dB PER FRAME.
        /// </summary>
        private void CalculateSnrData(double highEnergyFraction)
        {
            // If the FractionOfHighEnergyFrames PRIOR to noise removal exceeds SNR.FractionalBoundForMode,
            // then Lamel's noise removal algorithm may not work well.
            if (highEnergyFraction > SNR.FractionalBoundForMode)
            {
                Log.WriteIfVerbose("\nWARNING ##############");
                Log.WriteIfVerbose(
                    "\t############### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
                    highEnergyFraction * 100,
                    SNR.FractionalBoundForMode * 100);
                Log.WriteIfVerbose("\t############### Noise reduction algorithm may not work well in this instance!\n");
            }

            //AUDIO SEGMENTATION/END POINT DETECTION - based on Lamel et al
            // Setting segmentation/endpoint detection parameters is broken as of September 2014.
            // The next line sets default parameters.
            EndpointDetectionConfiguration.SetDefaultSegmentationConfig();
            this.SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsPerFrame, this.FrameStep);
        }

        /// <summary>
        /// This method assumes that the spectrogram has linear Herz scale.
        /// </summary>
        /// <param name="title">title to be added to spectrogram.</param>
        public Image<Rgb24> GetImageFullyAnnotated(string title, Color? tag = null)
        {
            var image = this.GetImage();
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image = this.GetImageAnnotatedWithLinearHerzScale(image, title, tag);
            return image;
        }

        /// <summary>
        /// This method fully annotates a short-time scale spectrogram.
        /// The grid-lines are drawn according to indices in gridLineLocations.
        /// Therefore the method will accept spectrograms with octave or any frequency scale.
        /// The time scale is calculated from recording duration and width of image.
        /// </summary>
        /// <param name="image">The raw spectrogram image.</param>
        /// <param name="title">To go on the title bar.</param>
        /// <param name="gridLineLocations">A matrix of values.</param>
        /// <param name="tag">Used to identify images??.</param>
        /// <returns>The annotated spectrogram.</returns>
        public Image<Rgb24> GetImageFullyAnnotated(Image<Rgb24> image, string title, int[,] gridLineLocations, Color? tag = null)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            FrequencyScale.DrawFrequencyLinesOnImage(image, gridLineLocations, includeLabels: true);

            // collect all the images and combine.
            var titleBar = DrawTitleBarOfGrayScaleSpectrogram(title, image.Width, tag);
            var timeBmp = ImageTrack.DrawTimeTrack(this.Duration, image.Width);
            var list = new List<Image<Rgb24>> { titleBar, timeBmp, image, timeBmp };
            var compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }

        public Image<Rgb24> GetImageAnnotatedWithLinearHerzScale(Image<Rgb24> image, string title, Color? tag = null)
        {
            // init with linear frequency scale and draw freq grid lines on image
            // image height will be half frame size.
            int frameSize = image.Height * 2;
            int hertzInterval = 1000;
            if (image.Height < 200)
            {
                hertzInterval = 2000;
            }

            var freqScale = new FrequencyScale(this.NyquistFrequency, frameSize, hertzInterval);
            var compositeImage = this.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations, tag);

            //following line for debug.
            //image = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(image, sampleRate, frameStep, "DECIBEL SPECTROGRAM");
            return compositeImage;
        }

        public Image<Rgb24> GetImage()
        {
            return this.GetImage(false, false, false);
        }

        public Image<Rgb24> GetImage(bool doHighlightSubband, bool add1KHzLines, bool doMelScale)
        {
            int subBandMinHz = 1000;
            int subBandMaxHz = 9000;

            int maxFrequency = this.NyquistFrequency;
            var image = GetSonogramImage(this.Data, this.NyquistFrequency, maxFrequency, this.Configuration.DoMelScale, 1, doHighlightSubband, subBandMinHz, subBandMaxHz);

            if (add1KHzLines)
            {
                const int kHzInterval = 1000;

                //int bandCount = (int)Math.Floor(binCount / kHzBinWidth);
                int[,] gridLineLocations;

                if (doMelScale)
                {
                    gridLineLocations = SpectrogramMelScale.GetMelGridLineLocations(kHzInterval, this.NyquistFrequency, image.Height);
                }
                else
                {
                    gridLineLocations = FrequencyScale.GetLinearGridLineLocations(this.NyquistFrequency, kHzInterval, image.Height);
                }

                FrequencyScale.DrawFrequencyLinesOnImage((Image<Rgb24>)image, gridLineLocations, includeLabels: true);
            }

            return image;
        }

        private static bool IsInBand(int y, int? minFreq, int? maxFreq)
        {
            if (minFreq == null && maxFreq == null)
            {
                return false;
            }

            return (minFreq == null || y > minFreq) && (maxFreq == null || y < maxFreq);
        }

        /// <summary>
        /// converts the dB data in sonogram.Data to grey scale image of spectrogram.
        /// </summary>
        /// <param name="matrix">matrix of sonogram values.</param>
        public static Tuple<double[,], double, double> Data2ImageData(double[,] matrix)
        {
            // Number of spectra in sonogram
            int width = matrix.GetLength(0);
            int fftBins = matrix.GetLength(1);
            DataTools.MinMax(matrix, out double min, out double max);
            double range = max - min; //for normalization

            var mt = new double[fftBins, width];
            for (int f = 0; f < fftBins; f++)
            {
                for (int t = 0; t < width; t++)
                {
                    // Normalize MatrixValues and bound the value - use 0-255 image intensity range
                    double value = (matrix[t, f] - min) / range;
                    int c = 255 - (int)Math.Floor(255.0 * value); //original version
                    if (c < 0)
                    {
                        c = 0;
                    }
                    else if (c >= 256)
                    {
                        c = 255;
                    }

                    mt[fftBins - 1 - f, t] = c;
                }
            }

            return Tuple.Create(mt, min, max);
        }

        public static Image<Rgb24> GetSonogramImage(double[,] data, int nyquistFreq, int maxFrequency, bool doMelScale, int binHeight, bool doHighlightSubband, int subBandMinHz, int subBandMaxHz)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int fftBins = data.GetLength(1);
            int maxBin = (int)Math.Floor(fftBins * maxFrequency / (double)nyquistFreq);

            int imageHeight = maxBin * binHeight; // image ht = sonogram ht. Later include grid and score scales

            //set up min, max, range for normalising of dB values
            DataTools.MinMax(data, out double min, out double max);
            double range = max - min;

            // readjust min and max to create the effect of contrast stretching. It enhances the spectrogram a bit
            double fractionalStretching = 0.01;
            min = min + (range * fractionalStretching);
            max = max - (range * fractionalStretching);
            range = max - min;

            //int? minHighlightFreq = this.subBand_MinHz;
            //int? maxHighlightFreq = this.subBand_MaxHz;
            //int minHighlightBin = (minHighlightFreq == null) ? 0 : (int)Math.Round((double)minHighlightFreq / (double)NyquistFrequency * fftBins);
            //int maxHighlightBin = (maxHighlightFreq == null) ? 0 : (int)Math.Round((double)maxHighlightFreq / (double)NyquistFrequency * fftBins);

            //calculate top and bottom of sub-band
            int minHighlightBin = (int)Math.Round(subBandMinHz / (double)nyquistFreq * fftBins);
            int maxHighlightBin = (int)Math.Round(subBandMaxHz / (double)nyquistFreq * fftBins);
            if (doMelScale)
            {
                double maxMel = MFCCStuff.Mel(nyquistFreq);
                int melRange = (int)(maxMel - 0 + 1);
                double pixelPerMel = imageHeight / (double)melRange;
                double minBandMel = MFCCStuff.Mel(subBandMinHz);
                double maxBandMel = MFCCStuff.Mel(subBandMaxHz);
                minHighlightBin = (int)Math.Round(minBandMel * pixelPerMel);
                maxHighlightBin = (int)Math.Round(maxBandMel * pixelPerMel);
            }

            Color[] grayScale = ImageTools.GrayScale();

            var bmp = new Image<Rgb24>(width, imageHeight);
            int yOffset = imageHeight;

            // for all freq bins
            for (int y = 0; y < maxBin; y++)
            {
                //repeat this bin if ceptral image
                for (int r = 0; r < binHeight; r++)
                {
                    // for all pixels in line
                    for (int x = 0; x < width; x++)
                    {
                        // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                        double value = (data[x, y] - min) / range;
                        int c = 255 - (int)Math.Floor(255.0 * value); //original version
                        if (c < 0)
                        {
                            c = 0;
                        }
                        else if (c >= 256)
                        {
                            c = 255;
                        }

                        int g = c + 40; // green tinge used in the template scan band
                        if (g >= 256)
                        {
                            g = 255;
                        }

                        var col = doHighlightSubband && IsInBand(y, minHighlightBin, maxHighlightBin) ? Color.FromRgb((byte)c, (byte)g, (byte)c) : grayScale[c];
                        bmp[x, yOffset - 1] = col;
                    }

                    yOffset--;
                } //end repeats over one track
            }

            return bmp;
        }

        /// <summary>
        /// Returns an image of the data matrix.
        /// Normalises the values from min->max according to passed rank values.
        /// Therefore pixels in the normalised grey-scale image will range from 0 to 255.
        /// </summary>
        public static Image<Rgb24> GetSonogramImage(double[,] data, int minPercentile, int maxPercentile)
        {
            int width = data.GetLength(0); // Number of spectra in sonogram
            int binCount = data.GetLength(1);
            int binHeight = 1;
            int imageHeight = binCount * binHeight; // image ht = sonogram ht

            double[] lowAverage = GetAvSpectrum_LowestPercentile(data, minPercentile);
            double[] hihAverage = GetAvSpectrum_HighestPercentile(data, maxPercentile);
            double min = lowAverage.Min();
            double max = hihAverage.Max();

            double range = max - min;
            Color[] grayScale = ImageTools.GrayScale();

            var bmp = new Image<Rgb24>(width, imageHeight);
            int yOffset = imageHeight;
            for (int y = 0; y < binCount; y++)
            {
                // repeat this bin if pixel rows per bin>1
                for (int r = 0; r < binHeight; r++)
                {
                    // for pixels in the line
                    for (int x = 0; x < width; x++)
                    {
                        // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                        double value = (data[x, y] - min) / range;
                        int c = 255 - (int)Math.Floor(255.0 * value); //original version
                        if (c < 0)
                        {
                            c = 0;
                        }
                        else
                        if (c >= 256)
                        {
                            c = 255;
                        }

                        bmp[x, yOffset - 1] = grayScale[c];
                    }

                    yOffset--;
                } //end repeats over one track
            }

            return bmp;
        }

        public static double[] GetAvSpectrum_LowestPercentile(double[,] matrix, int lowPercentile)
        {
            var energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArrayInAscendingOrder(energyLevels);
            int[] order = sorted.Item1;

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(lowPercentile * matrix.GetLength(0) / 100D);
            double[] avSpectrum = new double[colCount];

            // sum the lowest percentile frames
            for (int i = 0; i < cutoff; i++)
            {
                double[] row = DataTools.GetRow(matrix, order[i]);
                for (int c = 0; c < colCount; c++)
                {
                    avSpectrum[c] += row[c];
                }
            }

            // get average of the lowest percentile frames
            for (int c = 0; c < colCount; c++)
            {
                avSpectrum[c] /= cutoff;
            }

            return avSpectrum;
        }

        public static double[] GetAvSpectrum_HighestPercentile(double[,] matrix, int highPercentile)
        {
            double[] energyLevels = MatrixTools.GetRowAverages(matrix);
            var sorted = DataTools.SortArray(energyLevels); // sorts array in descending order
            int[] order = sorted.Item1;

            int colCount = matrix.GetLength(1);
            int cutoff = (int)(highPercentile * matrix.GetLength(0) / 100D);
            double[] avSpectrum = new double[colCount];

            // sum the lowest percentile frames
            for (int i = 0; i < cutoff; i++)
            {
                double[] row = DataTools.GetRow(matrix, order[i]);
                for (int c = 0; c < colCount; c++)
                {
                    avSpectrum[c] += row[c];
                }
            }

            // get average of the lowest percentile frames
            for (int c = 0; c < colCount; c++)
            {
                avSpectrum[c] /= cutoff;
            }

            return avSpectrum;
        }

        /// <summary>
        /// Draws Frame around image of spectrogram.
        /// </summary>
        /// <param name="image">Image of Spectrogram.</param>
        /// <param name="sampleRate">sample rate of recording. Necessary for both time scale and Hertz scale.</param>
        /// <param name="frameStep">frame step allows correct time scale to be drawn.</param>
        /// <param name="title">Descriptive title of the spectrogram.</param>
        /// <returns>The framed spectrogram image.</returns>
        public static Image<Rgb24> GetImageAnnotatedWithLinearHertzScale(Image<Rgb24> image, int sampleRate, int frameStep, string title, Color? tag = null)
        {
            var titleBar = DrawTitleBarOfGrayScaleSpectrogram(title, image.Width, tag);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(frameStep / (double)sampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            var nyquist = sampleRate / 2;
            int hertzInterval = 1000;
            if (image.Height < 200)
            {
                hertzInterval = 2000;
            }

            image = FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval, nyquist, hertzInterval);
            return image;
        }

        /// <summary>
        /// This method draws only top and bottom time scales and adds the title bar.
        /// It does NOT include the frequency grid lines.
        /// </summary>
        public static Image<Rgb24> FrameSonogram(
            Image<Rgb24> sonogramImage,
            Image<Rgb24> titleBar,
            TimeSpan minuteOffset,
            TimeSpan xAxisTicInterval,
            TimeSpan xAxisPixelDuration,
            TimeSpan labelInterval)
        {
            int imageWidth = sonogramImage.Width;
            var timeBmp = ImageTrack.DrawShortTimeTrack(minuteOffset, xAxisPixelDuration, xAxisTicInterval, labelInterval, imageWidth, "Seconds");
            return ImageTools.CombineImagesVertically(titleBar, timeBmp, sonogramImage, timeBmp);
        }

        /// <summary>
        /// This method assumes that the height of the passed sonogram image is half of the original frame size.
        /// This assumption allows the frequency scale grid lines to be placed at the correct intervals.
        /// </summary>
        public static Image<Rgb24> FrameSonogram(
            Image<Rgb24> sonogramImage,
            Image<Rgb24> titleBar,
            TimeSpan minuteOffset,
            TimeSpan xAxisTicInterval,
            TimeSpan xAxisPixelDuration,
            TimeSpan labelInterval,
            int nyquist,
            int hertzInterval)
        {
            double secondsDuration = xAxisPixelDuration.TotalSeconds * sonogramImage.Width;
            var fullDuration = TimeSpan.FromSeconds(secondsDuration);

            // init frequency scale
            int frameSize = sonogramImage.Height * 2;
            var freqScale = new FrequencyScale(nyquist, frameSize, hertzInterval);
            SpectrogramTools.DrawGridLinesOnImage((Image<Rgb24>)sonogramImage, minuteOffset, fullDuration, xAxisTicInterval, freqScale);

            int imageWidth = sonogramImage.Width;
            var timeBmp = ImageTrack.DrawShortTimeTrack(minuteOffset, xAxisPixelDuration, xAxisTicInterval, labelInterval, imageWidth, "Seconds");
            return ImageTools.CombineImagesVertically(titleBar, timeBmp, sonogramImage, timeBmp);
        }

        public static Image<Rgb24> DrawTitleBarOfGrayScaleSpectrogram(string title, int width, Color? tag = null)
        {
            var bmp = Drawing.NewImage(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR, Color.Black);
            bmp.Mutate(g =>
            {
                // var stringFont = Drawing.Tahoma9;
                var stringFont = Drawing.Arial9;

                int xBuffer = 4;
                g.DrawTextSafe(title, stringFont, Color.Wheat, new PointF(xBuffer, 3));

                var stringSize = g.MeasureString(title, stringFont);
                xBuffer += stringSize.ToSize().Width + 70;
                string text = Meta.OrganizationTag;
                stringSize = g.MeasureString(text, stringFont);
                int x2 = width - stringSize.ToSize().Width - 2;
                if (x2 > xBuffer)
                {
                    g.DrawTextSafe(text, stringFont, Color.Wheat, new PointF(x2, 3));
                }

                // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
                g.NoAA().DrawLine(new Pen(Color.Gray, 1), 0, 0, width, 0); //draw upper boundary

                if (tag.NotNull())
                {
                    g.NoAA().DrawLines(
                        tag.Value,
                        1f,
                        new PointF(0, 0),
                        new PointF(0, SpectrogramConstants.HEIGHT_OF_TITLE_BAR));
                }
            });

            return bmp;
        }

        /*
        // mark of time scale according to scale.
        public static Image<Rgb24> DrawTimeTrack(TimeSpan offsetMinute, TimeSpan xAxisPixelDuration, TimeSpan xAxisTicInterval, TimeSpan labelInterval, int trackWidth, int trackHeight, string title)
        {
            var bmp = new Image<Rgb24>(trackWidth, trackHeight);
            bmp.Mutate(g =>
            {
                g.Clear(Color.Black);

                double elapsedTime = offsetMinute.TotalSeconds;
                double pixelDuration = xAxisPixelDuration.TotalSeconds;
                int labelSecondsInterval = (int)labelInterval.TotalSeconds;
                var whitePen = new Pen(Color.White, 1);
                var stringFont = Drawing.Arial8;

                // for columns, draw in second lines
                double xInterval = (int)(xAxisTicInterval.TotalMilliseconds / xAxisPixelDuration.TotalMilliseconds);

                // for pixels in the line
                for (int x = 1; x < trackWidth; x++)
                {
                    elapsedTime += pixelDuration;
                    if (x % xInterval <= pixelDuration)
                    {
                        g.DrawLine(whitePen, x, 0, x, trackHeight);
                        int totalSeconds = (int)Math.Round(elapsedTime);
                        if (totalSeconds % labelSecondsInterval == 0)
                        {
                            int minutes = totalSeconds / 60;
                            int seconds = totalSeconds % 60;
                            string time = $"{minutes}m{seconds}s";
                            g.DrawTextSafe(time, stringFont, Color.White, new PointF(x + 1, 2)); //draw time
                        }
                    }
                }

                g.DrawLine(whitePen, 0, 0, trackWidth, 0); //draw upper boundary
                g.DrawLine(whitePen, 0, trackHeight - 1, trackWidth, trackHeight - 1); //draw lower boundary
                g.DrawLine(whitePen, trackWidth, 0, trackWidth, trackHeight - 1); //draw right end boundary
                g.DrawTextSafe(title, stringFont, Color.White, new PointF(4, 3));
            });

            return bmp;
        }
        */
    }
}