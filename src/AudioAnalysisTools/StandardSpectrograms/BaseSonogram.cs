// <copyright file="BaseSonogram.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.StandardSpectrograms
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Tools.Wav;

    using AudioAnalysisTools.DSP;
    using AudioAnalysisTools.LongDurationSpectrograms;
    using AudioAnalysisTools.WavTools;

    using TowseyLibrary;

    /// <summary>
    /// Sonogram type.
    /// </summary>
    public enum SonogramType
    {
        /// <summary>
        /// Ampltude Sonogram.
        /// </summary>
        Amplitude,

        /// <summary>
        /// Spectral Sonogram.
        /// </summary>
        Spectral,

        /// <summary>
        /// Cepstral Sonogram.
        /// </summary>
        Cepstral,

        /// <summary>
        /// Acoustic Vectors Sonogram.
        /// </summary>
        AcousticVectors,

        /// <summary>
        /// Sobel Edge Sonogram.
        /// </summary>
        SobelEdge,
    }

    /// <summary>
    /// Base Sonogram.
    /// </summary>
    public abstract partial class BaseSonogram : IDisposable
    {
        public SonogramConfig Configuration { get; set; }

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
        /// Gets or sets instance of class SNR that stores info about signal energy and dB per frame
        /// </summary>
        public SNR SnrData { get; set; }

        /// <summary>
        /// Gets or sets decibels per signal frame
        /// </summary>
        public double[] DecibelsPerFrame { get; set; }

        public double[] DecibelsNormalised { get; set; }

        /// <summary>
        /// Gets or sets decibel reference with which to NormaliseMatrixValues the dB values for MFCCs
        /// </summary>
        public double DecibelReference { get; protected set; }

        /// <summary>
        /// Gets or sets integer coded signal state ie  0=non-vocalisation, 1=vocalisation, etc.
        /// </summary>
        public int[] SigState { get; protected set; }

        /// <summary>
        /// Gets or sets the spectrogram data matrix of doubles
        /// </summary>
        public double[,] Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// BASE CONSTRUCTOR: Use this when want to extract time segment of existing sonogram
        /// </summary>
        /// <param name="config">config file to use</param>
        public BaseSonogram(SonogramConfig config)
        {
            this.Configuration = config;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// BASE CONSTRUCTOR
        /// This constructor contains all steps required to prepare the amplitude spectrogram.
        /// The third boolean parameter is simply a place-filler to ensure a different Constructor signature.
        /// from the principle Constructor which follows.
        /// </summary>
        /// <param name="config">config file to use</param>
        /// <param name="wav">wav</param>
        /// <param name="dummy">filler boolean. Calculate in method</param>
        public BaseSonogram(SonogramConfig config, WavReader wav, bool dummy)
            : this(config)
        {
            // As of 28 March 2017 drop capability to get sub-band of spectrogram because was not being used.
            // can be recovered later if desired.
            //bool doExtractSubband = this.SubBandMinHz > 0 || this.SubBandMaxHz < this.NyquistFrequency;

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
                config.WindowSize,
                config.WindowOverlap,
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

            // ENERGY PER FRAME and NORMALISED dB PER FRAME AND SNR
            // currently DoSnr = true by default
            if (config.DoSnr)
            {
                // If the FractionOfHighEnergyFrames PRIOR to noise removal exceeds SNR.FractionalBoundForMode,
                // then Lamel's noise removal algorithm may not work well.
                if (fftData.FractionOfHighEnergyFrames > SNR.FractionalBoundForMode)
                {
                    Log.WriteIfVerbose("\nWARNING ##############");
                    Log.WriteIfVerbose(
                        "\t############### BaseSonogram(): This is a high energy recording. Percent of high energy frames = {0:f0} > {1:f0}%",
                        fftData.FractionOfHighEnergyFrames * 100,
                        SNR.FractionalBoundForMode * 100);
                    Log.WriteIfVerbose("\t############### Noise reduction algorithm may not work well in this instance!\n");
                }

                //AUDIO SEGMENTATION/END POINT DETECTION - based on Lamel et al
                // Setting segmentation/endpoint detection parameters is broken as of September 2014.
                // The next line is a hack replacement
                EndpointDetectionConfiguration.SetDefaultSegmentationConfig();
                this.SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsPerFrame, this.FrameStep);
            }

            /* AS OF 30 MARCH 2017, NO LONGER IMPLEMENT SUB-BAND THINGS, because not being used for years.
            // EXTRACT REQUIRED FREQUENCY BAND
            if (doExtractSubband)
            {
                this.Data = SpectrogramTools.ExtractFreqSubband(
                    this.Data,
                    this.subBandMinHz,
                    this.subBandMaxHz,
                    this.Configuration.DoMelScale,
                    this.Configuration.FreqBinCount,
                    this.FBinWidth);
                this.CalculateSubbandSNR(this.Data);
            }
            */
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSonogram"/> class.
        /// This BASE CONSTRUCTOR is the one most used - it automatically makes the Amplitude spectrum and
        /// then, using a call to Make(), it converts the Amplitude matrix to a Spectrogram whose values are decibels.
        /// </summary>
        /// <param name="config">All parameters required to make spectrogram.</param>
        /// <param name="wav">The recording whose spectrogram is to be made.</param>
        public BaseSonogram(SonogramConfig config, WavReader wav)
            : this(config, wav, false)
        {
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
            //this.Make(this.Data); // Do we need this line???
        }

        public abstract void Make(double[,] amplitudeM);

        /* AS OF 30 MARCH 2017, NO LONGER IMPLEMENT SUB-BAND THINGS, because not being used for years.
        public void CalculateSubbandSNR(double[,] subband)
        {
            this.SnrSubband = new SNR(subband); //subband is the amplitude values

            //RECALCULATE DecibelsNormalised and dB REFERENCE LEVEL - need for MFCCs
            this.DecibelsInSubband = SnrSubband.Decibels;
            this.DecibelReference = SnrSubband.MaxReferenceDecibelsWrtNoise;
            this.DecibelsNormalised = SnrSubband.NormaliseDecibelArray_ZeroOne(this.DecibelReference);

            //RECALCULATE ENDPOINTS OF VOCALISATIONS
            SigState = EndpointDetectionConfiguration.DetermineVocalisationEndpoints(this.DecibelsInSubband, this.FrameStep);
        }
        */

        public void SetTimeScale(TimeSpan duration)
        {
            this.Duration = duration;
        }

        /// <summary>
        /// This method assumes that the spectrogram has linear Herz scale
        /// </summary>
        /// <param name="title">title to be added to spectrogram</param>
        public Image GetImageFullyAnnotated(string title)
        {
            var image = this.GetImage();
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image = this.GetImageAnnotatedWithLinearHerzScale(image, title);
            return image;
        }

        public Image GetImageFullyAnnotated(Image image, string title, int[,] gridLineLocations)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)image, gridLineLocations, includeLabels: true);

            var titleBar = LDSpectrogramRGB.DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            var timeBmp = ImageTrack.DrawTimeTrack(this.Duration, image.Width);
            //var list = new List<Image> { titleBar, timeBmp, image, timeBmp };
            var list = new List<Image> { titleBar, image, timeBmp };
            var compositeImage = ImageTools.CombineImagesVertically(list);
            return compositeImage;
        }

        public Image GetImageAnnotatedWithLinearHerzScale(Image image, string title)
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
            var compositeImage = this.GetImageFullyAnnotated(image, title, freqScale.GridLineLocations);
            //image = BaseSonogram.GetImageAnnotatedWithLinearHertzScale(image, sampleRate, frameStep, "DECIBEL SPECTROGRAM");
            return compositeImage;
        }

        public Image GetImage()
        {
            return this.GetImage(false, false, false);
        }

        public Image GetImage(bool doHighlightSubband, bool add1KHzLines, bool doMelScale)
        {
            // doHighlightSubband function still working but have removed min/max bounds from user control.
            // doHighlightSubband = true;
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
                    gridLineLocations = FrequencyScale.GetMelGridLineLocations(kHzInterval, this.NyquistFrequency, image.Height);
                }
                else
                {
                    gridLineLocations = FrequencyScale.GetLinearGridLineLocations(this.NyquistFrequency, kHzInterval, image.Height);
                }

                FrequencyScale.DrawFrequencyLinesOnImage((Bitmap)image, gridLineLocations, includeLabels: true);
            }

            return image;
        }

        public Image GetImage_ReducedSonogramWithWidth(int width, bool drawGridLines)
        {
            var data = this.Data; //sonogram intensity values
            int frameCount = data.GetLength(0); // Number of spectra in sonogram

            int factor = frameCount / width;

            if (factor <= 1)
            {
                return this.GetImage();
            }

            return this.GetImage_ReducedSonogram(factor, drawGridLines);
        }

        public Image GetImage_ReducedSonogram(int factor, bool drawGridLines)
        {
            //  double[] logEnergy = this.LogEnPerFrame;
            var data = this.Data; //sonogram intensity values
            int frameCount = data.GetLength(0); // Number of spectra in sonogram
            int imageHeight = data.GetLength(1); // image ht = sonogram ht. Later include grid and score scales
            int imageWidth = frameCount / factor;
            int subSample = frameCount / imageWidth;

            //set up min, max, range for normalising of dB values
            DataTools.MinMax(data, out double min, out double max);
            double range = max - min;

            var grayScale = ImageTools.GrayScale();

            //set up the 1000kHz scale
            int herzInterval = 1000;
            int[] vScale = FrequencyScale.CreateLinearYaxis(herzInterval, this.NyquistFrequency, imageHeight); //calculate location of 1000Hz grid lines
            var bmp = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
            for (int w = 0; w < imageWidth; w++)
            {
                int start = w * subSample;
                int end = ((w + 1) * subSample) - 1;
                double maxE = -double.MaxValue;
                int maxId = 0;
                for (int x = start; x < end; x++)
                {
                    // NOTE!@#$%^ This was changed from LogEnergy on 30th March 2009.
                    if (maxE < this.DecibelsPerFrame[x])
                    {
                        maxE = this.DecibelsPerFrame[x];
                        maxId = x;
                    }
                }

                // have found the frame with max energy. Now draw its spectrum
                // over all freq bins
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    // NormaliseMatrixValues and bound the value - use min bound, max and 255 image intensity range
                    double value = (data[maxId, y] - min) / range;
                    int c = 255 - (int)Math.Floor(255.0 * value); //original version
                    if (c < 0)
                    {
                        c = 0;
                    }
                    else if (c >= 256)
                    {
                        c = 255;
                    }

                    var col = grayScale[c];
                    bmp.SetPixel(w, imageHeight - y - 1, col);
                } //end over all freq bins

                //set up grid color

                if (drawGridLines)
                {
                    var gridCol = Color.Black;
                    if (w % 2 == 0)
                    {
                        gridCol = Color.Black;
                    }

                    //over all Y-axis pixels
                    for (int p = 0; p < vScale.Length; p++)
                    {
                        if (vScale[p] == 0)
                        {
                            continue;
                        }

                        int y = imageHeight - p;
                        bmp.SetPixel(w, y, gridCol);
                    }
                }
            }

            return bmp;
        }

        public void Dispose()
        {
            this.Configuration = null;
            this.SnrData = null;
            this.DecibelsPerFrame = null;
            this.DecibelsNormalised = null;
            this.Data = null;
            this.SigState = null;
        }

        //##################################################################################################################################
        //########### STATIC METHODS #######################################################################################################
        //##################################################################################################################################

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
        /// <param name="matrix">matrix of sonogram values</param>
        public static Tuple<double[,], double, double> Data2ImageData(double[,] matrix)
        {
            int width = matrix.GetLength(0);   // Number of spectra in sonogram
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

        public static Image GetSonogramImage(double[,] data, int nyquistFreq, int maxFrequency, bool doMelScale, int binHeight, bool doHighlightSubband, int subBandMinHz, int subBandMaxHz)
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

            var bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
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

                        var col = doHighlightSubband && IsInBand(y, minHighlightBin, maxHighlightBin) ? Color.FromArgb(c, g, c) : grayScale[c];
                        bmp.SetPixel(x, yOffset - 1, col);
                    }//for all pixels in line

                    yOffset--;
                } //end repeats over one track
            }//end over all freq bins

            return bmp;
        }

        /// <summary>
        /// Returns an image of the data matrix.
        /// Normalises the values from min->max according to passed rank values.
        /// Therefore pixels in the normalised grey-scale image will range from 0 to 255.
        /// </summary>
        public static Image GetSonogramImage(double[,] data, int minPercentile, int maxPercentile)
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

            var bmp = new Bitmap(width, imageHeight, PixelFormat.Format24bppRgb);
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

                        bmp.SetPixel(x, yOffset - 1, grayScale[c]);
                    }//for all pixels in line

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
        public static Image GetImageAnnotatedWithLinearHertzScale(Image image, int sampleRate, int frameStep, string title)
        {
            var titleBar = DrawTitleBarOfGrayScaleSpectrogram(title, image.Width);
            var startTime = TimeSpan.Zero;
            var xAxisTicInterval = TimeSpan.FromSeconds(1);
            TimeSpan xAxisPixelDuration = TimeSpan.FromSeconds(frameStep / (double)sampleRate);
            var labelInterval = TimeSpan.FromSeconds(5);
            var nyquist = sampleRate / 2;
            int hertzInterval = 1000;
            if (image.Height < 512)
            {
                hertzInterval = 2000;
            }

            image = FrameSonogram(image, titleBar, startTime, xAxisTicInterval, xAxisPixelDuration, labelInterval, nyquist, hertzInterval);
            return image;
        }

        /// <summary>
        /// This method assumes that the height of the passed sonogram image is half of the original frame size.
        /// This assumption allows the frequency scale grid lines to be placed at the correct intervals.
        /// </summary>
        public static Image FrameSonogram(
            Image sonogramImage,
            Image titleBar,
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
            SpectrogramTools.DrawGridLinesOnImage((Bitmap)sonogramImage, minuteOffset, fullDuration, xAxisTicInterval, freqScale);

            int imageWidth = sonogramImage.Width;
            var timeBmp = ImageTrack.DrawShortTimeTrack(minuteOffset, xAxisPixelDuration, xAxisTicInterval, labelInterval, imageWidth, "Seconds");
            Image[] imageArray = { titleBar, sonogramImage, timeBmp };
            return ImageTools.CombineImagesVertically(imageArray);
        }

        public static Image DrawTitleBarOfGrayScaleSpectrogram(string title, int width)
        {
            var bmp = new Bitmap(width, SpectrogramConstants.HEIGHT_OF_TITLE_BAR);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.Black);

            // var stringFont = new Font("Tahoma", 9);
            var stringFont = new Font("Arial", 9);

            //string text = title;
            int x = 4;
            g.DrawString(title, stringFont, Brushes.Wheat, new PointF(x, 3));

            var stringSize = g.MeasureString(title, stringFont);
            x += stringSize.ToSize().Width + 70;
            string text = Meta.OrganizationTag;
            stringSize = g.MeasureString(text, stringFont);
            int x2 = width - stringSize.ToSize().Width - 2;
            if (x2 > x)
            {
                g.DrawString(text, stringFont, Brushes.Wheat, new PointF(x2, 3));
            }

            // g.DrawLine(pen, duration + 1, 0, trackWidth, 0);
            g.DrawLine(new Pen(Color.Gray), 0, 0, width, 0); //draw upper boundary
            return bmp;
        }
    }
}