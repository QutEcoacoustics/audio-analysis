namespace Acoustics.Tools.Audio
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    using Acoustics.Shared;
    using Acoustics.Tools.Wav;

    using MathNet.Numerics.Transformations;

    /// <summary>
    /// Signal to Image used by web site.
    /// </summary>
    public class CustomSpectrogramUtility : AbstractSpectrogramUtility, ISpectrogramUtility
    {
        private const double WaveFormDbMin = -20.0;

        /// <summary>
        /// Window overlap of 0, framesize of 512, samplerate of 22050.
        /// 1 pixel for every 23.22 ms.
        /// </summary>
        private const double WindowOverlap = 0;
        private const int FrameWindowSize = 512;
        private const int SampleRate = 22050;

        private const int SmoothingWindow = 7;

        /// <summary>
        /// Hamming window function.
        /// </summary>
        private static readonly Func<int, int, double> Hamming = (n1, n2) =>
            {
                double x = 2.0 * Math.PI * n1 / (n2 - 1);

                ////return 0.53836 - 0.46164 * Math.Cos(x);
                // MATLAB code uses these value and says it is better!
                return 0.54 - (0.46 * Math.Cos(x));
            };

        private readonly IAudioUtility audioUtility;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomSpectrogramUtility"/> class.
        /// </summary>
        /// <param name="audioUtility">
        /// The audio utility.
        /// </param>
        public CustomSpectrogramUtility(IAudioUtility audioUtility)
        {
            this.audioUtility = audioUtility;
        }

        /// <summary>
        /// Create a spectrogram from a segment of the <paramref name="source"/> audio file.
        /// <paramref name="output"/> image file will be created.
        /// </summary>
        /// <param name="source">
        /// The source audio file.
        /// </param>
        /// <param name="sourceMimeType">
        /// The source Mime Type.
        /// </param>
        /// <param name="output">
        /// The output image file. Ensure the file does not exist.
        /// </param>
        /// <param name="outputMimeType">
        /// The output Mime Type.
        /// </param>
        /// <param name="request">
        /// The spectrogram request.
        /// </param>
        public void Create(FileInfo source, string sourceMimeType, FileInfo output, string outputMimeType, SpectrogramRequest request)
        {
            this.ValidateMimeTypeExtension(source, sourceMimeType, output, outputMimeType);

            var tempFile = TempFileHelper.NewTempFileWithExt(MediaTypes.ExtWav);

            var audioUtilRequest = new AudioUtilityRequest
            {
                MixDownToMono = true,
                OffsetStart = request.Start,
                OffsetEnd = request.End,
                SampleRate = 22050
            };

            this.audioUtility.Modify(source, sourceMimeType, tempFile, MediaTypes.MediaTypeWav, audioUtilRequest);

            Bitmap sourceImage;

            if (this.Log.IsDebugEnabled)
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                sourceImage = Spectrogram(File.ReadAllBytes(tempFile.FullName));

                stopwatch.Stop();

                this.Log.DebugFormat(
                    "Generated spectrogram for {0}. Took {1} ({2}ms).",
                    source.Name,
                    stopwatch.Elapsed.Humanise(),
                    stopwatch.Elapsed.TotalMilliseconds);

                this.Log.Debug("Source " + this.BuildFileDebuggingOutput(source));
            }
            else
            {
                sourceImage = Spectrogram(File.ReadAllBytes(tempFile.FullName));
            }

            // modify image to match request
            using (sourceImage)
            {
                // remove 1px from bottom (DC value)
                var sourceRectangle = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height - 1);

                using (var requestedImage = new Bitmap(
                        request.IsCalculatedWidthAvailable ? request.CalculatedWidth : sourceRectangle.Width,
                        request.Height.HasValue ? request.Height.Value : sourceRectangle.Height))
                using (var graphics = Graphics.FromImage(requestedImage))
                {
                    var destRectangle = new Rectangle(0, 0, requestedImage.Width, requestedImage.Height);
                    graphics.DrawImage(sourceImage, destRectangle, sourceRectangle, GraphicsUnit.Pixel);

                    var format = MediaTypes.GetImageFormat(MediaTypes.GetExtension(outputMimeType));

                    if (this.Log.IsDebugEnabled)
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        requestedImage.Save(output.FullName, format);

                        stopwatch.Stop();

                        this.Log.DebugFormat(
                            "Saved spectrogram for {0} to {1}. Took {2} ({3}ms).",
                            source.Name,
                            output.Name,
                            stopwatch.Elapsed.Humanise(),
                            stopwatch.Elapsed.TotalMilliseconds);

                        this.Log.Debug("Output " + this.BuildFileDebuggingOutput(output));
                    }
                    else
                    {
                        requestedImage.Save(output.FullName, format);
                    }
                }
            }

            tempFile.Delete();
        }

        /// <summary>
        /// Generate a Spectrogram.
        /// </summary>
        /// <param name="bytes">
        /// The bytes.
        /// </param>
        /// <returns>
        /// Spectrogram image.
        /// </returns>
        private static Bitmap Spectrogram(byte[] bytes)
        {
            IWavReader wavReader = new WavStreamReader(bytes);

            return new Bitmap(GetSpectrogram(wavReader, 1));
        }

        /// <summary>
        /// Get a spectrogram. 
        /// Channel must be > 0.
        /// </summary>
        /// <param name="reader">Wav Reader.</param>
        /// <param name="channel">Channel number.</param>
        /// <returns>Spectrogram image.</returns>
        private static Image GetSpectrogram(IWavReader reader, int channel)
        {
            /*
             * 80 pixels per second is too quick for Silverlight.
             * we want to use 40 pixels per second (half - window size of 0)
             * we want 40 pixels per second (about 23 millisec per pixel)
             * window overlap of 0.5
             * window size of 512? (might be too wide).
             */

            // to get spectrogram of required size:
            // 'subsample' (skip samples) based on sample rate (aiming for 22050 hz)
            // skip samples based on the window overlap (take 1 sample of every 1/windowOverlap).

            // reset stream position to data chunk
            var dataPosition = reader.Chunks.Where(c => c.Name == "data").First().Position;
            reader.SampleStream.Position = dataPosition;

            // get samples for all channel
            var samplesByChannel = reader.SampleStream.SplitChannels(reader.AudioInfo, null);

            // get samples for specified channel
            var channelSamples = samplesByChannel[channel - 1];

            // subsample to desired samplerate
            channelSamples = SubSample(channelSamples, reader.AudioInfo.SampleRate, SampleRate);

            int samples = channelSamples.Length;

            // FRAME WINDOWING
            int[,] frameIDs = FrameStartEnds(samples, FrameWindowSize, WindowOverlap);
            var frameCount = frameIDs.GetLength(0);

            // window or frame width
            int windowSize = frameIDs[0, 1] + 1;

            // get window weights
            double[] windowWeights = CalcuateWindowWeights(windowSize, Hamming);

            // get window power
            double windowPower = CalculateWindowPower(windowSize, windowWeights);

            // get coefficient count
            // f[0]=DC;  f[256]=Nyquist  
            int coeffCount = (windowSize / 2) + 1;

            // get amplitude sonogram.
            double[,] amplitudeSonogram = CalculateAmplitudeSonogram(
                channelSamples,
                frameIDs,
                windowSize,
                frameCount,
                coeffCount,
                windowWeights);

            // get decibel spectra
            double[,] dbSonogram = DecibelSpectra(
                amplitudeSonogram,
                windowPower,
                SampleRate,
                reader.AudioInfo.Epsilon);

            /*
             *  Noise reduce.
             */

            // modal noise calculation
            // calculate noise profile, smooth and return for later use
            double[] modalNoise = CalculateModalNoise(dbSonogram);

            // filter moving average (smooth)
            double[] filterMovingAverageModalNoise = FilterMovingAverage(modalNoise, SmoothingWindow);

            /*
             * Done. Spectrogram is (finally) ready!
             */

            Image image = GetImage(dbSonogram);
            return image;
        }

        /// <summary>
        /// Subsamples audio.
        /// </summary>
        /// <param name="samples">
        /// The samples.
        /// </param>
        /// <param name="currentSampleRate">
        /// The current Sample Rate.
        /// </param>
        /// <param name="targetSampleRate">
        /// The target Sample Rate.
        /// </param>
        /// <returns>
        /// The sub sample.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// <c>InvalidOperationException</c>.
        /// </exception>
        private static double[] SubSample(double[] samples, int currentSampleRate, int targetSampleRate)
        {
            double modifyInterval = (double)currentSampleRate / (double)targetSampleRate;

            if (modifyInterval < 1)
            {
                throw new InvalidOperationException(
                    "Cannot up sample. Current sample rate: " + currentSampleRate + " target sample rate: " +
                    targetSampleRate);
            }

            if (modifyInterval != Math.Floor(modifyInterval))
            {
                throw new InvalidOperationException(
                    "Cannot sub sample to non-integer amount. Interval: " + modifyInterval + " Current sample rate: " +
                    currentSampleRate + " target sample rate: " + targetSampleRate);
            }

            if (modifyInterval == 1)
            {
                // do not change anything!
                return samples;
            }

            int interval = (int)modifyInterval;
            int l = samples.Length;

            // the new length
            int newL = l / interval;
            var newSamples = new double[newL];

            for (int i = 0; i < newL; i++)
            {
                newSamples[i] = samples[i * interval];
            }

            return newSamples;
        }

        /// <summary>
        /// Returns the start and end index of all frames in a long audio signal.
        /// </summary>
        /// <param name="dataLength">
        /// The data Length.
        /// </param>
        /// <param name="windowSize">
        /// The window Size.
        /// </param>
        /// <param name="windowOverlap">
        /// The window Overlap.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Signal must produce at least two frames!
        /// </exception>
        /// <returns>
        /// The frame start ends.
        /// </returns>
        private static int[,] FrameStartEnds(int dataLength, int windowSize, double windowOverlap)
        {
            int step = (int)((double)windowSize * (1.0 - windowOverlap));

            if (step < 1)
            {
                throw new ArgumentException("Frame Step must be at least 1");
            }

            if (step > windowSize)
            {
                throw new ArgumentException("Frame Step must be <=" + windowSize);
            }

            int overlap = windowSize - step;

            // this truncates residual samples
            int framecount = (dataLength - overlap) / step;

            if (framecount < 2)
            {
                throw new ArgumentException("Signal must produce at least two frames!");
            }

            int offset = 0;

            // col 0 =start; col 1 =end
            var frames = new int[framecount, 2];

            for (int i = 0; i < framecount; i++)
            {
                // start of frame
                frames[i, 0] = offset;

                // end of frame
                frames[i, 1] = offset + windowSize - 1;
                offset += step;
            }

            return frames;
        }

        /// <summary>
        /// Calcuate window weights.
        /// </summary>
        /// <param name="windowSize">
        /// The window Size.
        /// </param>
        /// <param name="windowFunction">
        /// The window Function.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="windowFunction"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// WindowSize must be a power of 2.
        /// </exception>
        /// <returns>
        /// The window weights.
        /// </returns>
        private static double[] CalcuateWindowWeights(int windowSize, Func<int, int, double> windowFunction)
        {
            if (windowFunction == null)
            {
                throw new ArgumentNullException("windowFunction");
            }

            if (!IsPowerOf2(windowSize))
            {
                throw new ArgumentException("WindowSize must be a power of 2.");
            }

            // set up the FFT window
            var windowWeights = new double[windowSize];

            for (int i = 0; i < windowSize; i++)
            {
                windowWeights[i] = windowFunction(i, windowSize);
            }

            return windowWeights;
        }

        /// <summary>
        /// Is <paramref name="number"/> a power of 2.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// True if <paramref name="number"/> is a power of 2.
        /// </returns>
        private static bool IsPowerOf2(int number)
        {
            while (number > 1)
            {
                if (number == 2)
                {
                    return true;
                }

                number >>= 1;
            }

            return false;
        }

        private static double CalculateWindowPower(int windowSize, double[] windowWeights)
        {
            // calculate power of the FFT window
            double power = 0.0;
            for (int i = 0; i < windowSize; i++)
            {
                power += windowWeights[i] * windowWeights[i];
            }

            return power;
        }

        private static double[,] CalculateAmplitudeSonogram(double[] audioSignal, int[,] frames, int windowSize, int frameCount, int coeffCount, double[] windowWeights)
        {
            // init amplitude sonogram
            var amplitudeSonogram = new double[frameCount, coeffCount];

            var window = new double[windowSize];

            var rft = new RealFourierTransformation(TransformationConvention.Matlab);

            // foreach frame or time step (all frames)
            for (int i = 0; i < frameCount; i++)
            {
                // set up the window
                for (int j = 0; j < windowSize; j++)
                {
                    window[j] = audioSignal[frames[i, 0] + j];
                }

                // returns fft amplitude spectrum
                double[] f1 = InvokeDotNetFft(window, windowSize, coeffCount, windowWeights, rft);

                ////double[] f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                ////double[] f1 = fft.Invoke(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum

                // foreach freq bin
                for (int j = 0; j < coeffCount; j++)
                {
                    // transfer amplitude
                    amplitudeSonogram[i, j] = f1[j];
                }
            }

            return amplitudeSonogram;
        }

        /// <summary>
        /// This .NET FFT library was downloaded from  http://www.mathdotnet.com/Iridium.aspx.
        /// The documentation and various examples of code are available at http://www.mathdotnet.com/doc/IridiumFFT.ashx.
        /// </summary>
        /// <param name="data">
        /// signal samples.
        /// </param>
        /// <param name="windowSize">
        /// The window Size.
        /// </param>
        /// <param name="coeffCount">
        /// The coeff Count.
        /// </param>
        /// <param name="windowWeights">
        /// The window Weights.
        /// </param>
        /// <param name="rft">
        /// The RealFourierTransformation.
        /// </param>
        /// <returns>
        /// Transformed samples.
        /// </returns>
        private static double[] InvokeDotNetFft(double[] data, int windowSize, int coeffCount, double[] windowWeights, RealFourierTransformation rft)
        {
            if (windowSize != data.Length)
            {
                return null;
            }

            ////int half = WindowSize >> 1; //original dot net code returns N/2 coefficients.
            int half = coeffCount;

            // apply the window
            if (windowWeights != null)
            {
                // apply the window
                for (int i = 0; i < windowSize; i++)
                {
                    // window
                    data[i] = windowWeights[i] * data[i];
                }
            }

            double[] freqReal, freqImag;
            rft.TransformForward(data, out freqReal, out freqImag);

            var amplitude = new double[half];

            for (int i = 0; i < half; i++)
            {
                amplitude[i] = Math.Sqrt((freqReal[i] * freqReal[i]) + (freqImag[i] * freqImag[i]));
            }

            return amplitude;
        }

        /// <summary>
        /// Converts spectral amplitudes directly to dB, normalising for window power and sample rate.
        /// NOTE 1: The window contributes power to the signal which must subsequently be removed from the spectral power.
        /// NOTE 2: Spectral power must be normaliesd for sample rate. Effectively calculate freq power per sample.
        /// NOTE 3: The power in all freq bins except f=0 must be doubled because the power spectrum is an even function about f=0;
        ///         This is due to the fact that the spectrum actually consists of 512 + 1 values, the centre value being for f=0.
        /// NOTE 4: The decibels value is a ratio. Here the ratio is implied.
        ///         dB = 10*log(amplitude ^2) but in this method adjust power to account for power of Hamming window and SR.
        /// NOTE 5: THIS METHOD ASSUMES THAT THE LAST BIN IS THE NYQUIST FREQ BIN.
        /// </summary>
        /// <param name="amplitudeM">the amplitude spectra.</param>
        /// <param name="windowPower">value for window power normalisation.</param>
        /// <param name="sampleRate">to normalise for the sampling rate.</param>
        /// <param name="epsilon">small value to avoid log of zero.</param>
        /// <returns>Decibel Spectra.</returns>
        private static double[,] DecibelSpectra(double[,] amplitudeM, double windowPower, int sampleRate, double epsilon)
        {
            int frameCount = amplitudeM.GetLength(0);
            int binCount = amplitudeM.GetLength(1);

            double[,] spectra = new double[frameCount, binCount];

            //calculate power of the DC value - first column of matrix
            for (int i = 0; i < frameCount; i++)//foreach time step or frame
            {
                if (amplitudeM[i, 0] < epsilon)
                    spectra[i, 0] = 10 * Math.Log10(epsilon * epsilon / windowPower / sampleRate);
                else
                    spectra[i, 0] = 10 * Math.Log10(amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower / sampleRate);
                //spectra[i, 0] = amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower; //calculates power
            }


            //calculate power in frequency bins - must multiply by 2 to accomodate two spectral components, ie positive and neg freq.
            for (int j = 1; j < binCount - 1; j++)
            {
                for (int i = 0; i < frameCount; i++)//foreach time step or frame
                {
                    if (amplitudeM[i, j] < epsilon)
                        spectra[i, j] = 10 * Math.Log10(epsilon * epsilon * 2 / windowPower / sampleRate);
                    else
                        spectra[i, j] = 10 * Math.Log10(amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower / sampleRate);
                    //spectra[i, j] = amplitudeM[i, j] * amplitudeM[i, j] * 2 / windowPower; //calculates power
                }//end of all frames
            } //end of all freq bins


            //calculate power of the Nyquist freq bin - last column of matrix
            for (int i = 0; i < frameCount; i++)//foreach time step or frame
            {
                //calculate power of the DC value
                if (amplitudeM[i, binCount - 1] < epsilon)
                    spectra[i, binCount - 1] = 10 * Math.Log10(epsilon * epsilon / windowPower / sampleRate);
                else
                    spectra[i, binCount - 1] = 10 * Math.Log10(amplitudeM[i, binCount - 1] * amplitudeM[i, binCount - 1] / windowPower / sampleRate);
                //spectra[i, 0] = amplitudeM[i, 0] * amplitudeM[i, 0] / windowPower; //calculates power
            }


            return spectra;
        }

        /// <summary>
        /// Calculates the modal noise value for each freq bin.
        /// Does so using a series of overlapped matrices.
        /// TODO!!!! COULD SIMPLY THIS METHOD. JUST CALCULATE MODE FOR EACH FREQ BIN WITHOUT OVERLAP ....
        /// .... AND THEN APPLY MORE SEVERE SMOOTHING TO THE MODAL NOISE PROFILE IN PREVIOUS METHOD.
        /// 
        /// COMPARE THIS METHOD WITH SNR.SubtractModalNoise().
        /// </summary>
        /// <param name="matrix">Audio sample matrix.</param>
        /// <returns>Modal noise values.</returns>
        private static double[] CalculateModalNoise(double[,] matrix)
        {
            //set parameters for noise histograms based on overlapping bands.
            //*******************************************************************************************************************
            int bandWidth = 3;  // should be an odd number
            int binCount = 64;  // number of pixel intensity bins
            double upperLimitForMode = 0.666; // sets upper limit to modal noise bin. Higher values = more severe noise removal.
            int binLimit = (int)(binCount * upperLimitForMode);
            //*******************************************************************************************************************


            double minIntensity; // min value in matrix
            double maxIntensity; // max value in matrix
            MinMax(matrix, out minIntensity, out maxIntensity);
            double binWidth = (maxIntensity - minIntensity) / binCount;  // width of an intensity bin
            // LoggedConsole.WriteLine("minIntensity=" + minIntensity + "  maxIntensity=" + maxIntensity + "  binWidth=" + binWidth);

            int rowCount = matrix.GetLength(0);
            int colCount = matrix.GetLength(1);
            if (bandWidth > colCount) bandWidth = colCount - 1;
            int halfWidth = bandWidth / 2;

            // init matrix from which histogram derived
            double[,] submatrix = Submatrix(matrix, 0, 0, rowCount - 1, bandWidth);
            double[] modalNoise = new double[colCount];

            for (int col = 0; col < colCount; col++) // for all cols i.e. freq bins
            {
                // construct new submatrix to calculate modal noise
                int start = col - halfWidth;   //extend range of submatrix below col for smoother changes
                if (start < 0) start = 0;
                int stop = col + halfWidth;
                if (stop >= colCount) stop = colCount - 1;
                submatrix = Submatrix(matrix, 0, start, rowCount - 1, stop);
                int[] histo = Histo(submatrix, binCount, minIntensity, maxIntensity, binWidth);
                //DataTools.writeBarGraph(histo);
                double[] smoothHisto = FilterMovingAverage(histo, 7);
                int maxindex; //mode
                GetMaxIndex(smoothHisto, out maxindex); //this is mode of histogram
                if (maxindex > binLimit) maxindex = binLimit;
                modalNoise[col] = minIntensity + (maxindex * binWidth);
                //LoggedConsole.WriteLine("  modal index=" + maxindex + "  modalIntensity=" + modalIntensity.ToString("F3"));
            }//end for all cols
            return modalNoise;
        }

        /// <summary>
        /// returns the min and max values in a matrix of doubles.
        /// </summary>
        /// <param name="data">
        /// The audio data.
        /// </param>
        /// <param name="min">
        /// The min value.
        /// </param>
        /// <param name="max">
        /// The max value.
        /// </param>
        private static void MinMax(double[,] data, out double min, out double max)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            min = data[0, 0];
            max = data[0, 0];

            for (int i = 1; i < rows; i++)
            {
                for (int j = 1; j < cols; j++)
                {
                    if (data[i, j] < min)
                    {
                        min = data[i, j];
                    }
                    else if (data[i, j] > max)
                    {
                        max = data[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Returns the submatrix of passed matrix.
        /// Assume that r1 less than r2, c1 less than c2. 
        /// Row, column indices start at 0.
        /// </summary>
        /// <param name="M"></param>
        /// <param name="r1"></param>
        /// <param name="c1"></param>
        /// <param name="r2"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        private static double[,] Submatrix(double[,] M, int r1, int c1, int r2, int c2)
        {
            int smRows = r2 - r1 + 1;
            int smCols = c2 - c1 + 1;

            double[,] sm = new double[smRows, smCols];

            for (int i = 0; i < smRows; i++)
            {
                for (int j = 0; j < smCols; j++)
                {
                    sm[i, j] = M[r1 + i, c1 + j];
                }
            }
            return sm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="binCount"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="binWidth"></param>
        /// <returns></returns>
        private static int[] Histo(double[,] data, int binCount, double min, double max, double binWidth)
        {
            int rows = data.GetLength(0);
            int cols = data.GetLength(1);
            int[] histo = new int[binCount];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    int bin = (int)((data[i, j] - min) / binWidth);
                    if (bin >= binCount) bin = binCount - 1;
                    if (bin < 0) bin = 0;
                    histo[bin]++;
                }

            return histo;
        }

        /// <summary>
        /// Filter Moving Average.
        /// </summary>
        /// <param name="signal">Audio signal.</param>
        /// <param name="width">Given width.</param>
        /// <returns>Filtered moving average.</returns>
        private static double[] FilterMovingAverage(double[] signal, int width)
        {
            if (width <= 1) return signal;    // no filter required
            int length = signal.Length;
            if (length <= 3) return signal;   // not worth the effort!

            var fs = new double[length]; // filtered signal
            int edge = width / 2;            // half window width.
            ////int    odd  = width%2;          // odd or even filter window.
            double sum = 0.0;

            // filter leading edge
            for (int i = 0; i < edge; i++)
            {
                sum = 0.0;
                for (int j = 0; j <= (i + edge); j++) { sum += signal[j]; }
                fs[i] = sum / (double)(i + edge + 1);
            }

            for (int i = edge; i < length - edge; i++)
            {
                sum = 0.0;
                for (int j = 0; j < width; j++) { sum += signal[i - edge + j]; }
                //sum = signal[i-1]+signal[i]+signal[i+1];
                fs[i] = sum / (double)width;
            }

            // filter trailing edge
            for (int i = length - edge; i < length; i++)
            {
                sum = 0.0;
                for (int j = i; j < length; j++) { sum += signal[j]; }
                fs[i] = sum / (double)(length - i);
            }
            return fs;
        }

        /// <summary>
        /// wrapper so one can call moving average filter with array of int.
        /// </summary>
        /// <param name="signal">Audio signal.</param>
        /// <param name="width">Given Width.</param>
        /// <returns>filtered signal.</returns>
        private static double[] FilterMovingAverage(int[] signal, int width)
        {
            int length = signal.Length;
            var dbSignal = new double[length];
            for (int i = 0; i < length; i++)
            {
                // clone
                dbSignal[i] = (double)signal[i];
            }

            return FilterMovingAverage(dbSignal, width);
        }

        /// <summary>
        /// returns the index of max value in an array of doubles. 
        /// array index starts at zero.
        /// </summary>
        /// <param name="data">audio data.</param>
        /// <param name="indexMax">maximum index.</param>
        private static void GetMaxIndex(double[] data, out int indexMax)
        {
            indexMax = 0;
            double max = data[0];
            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                    indexMax = i;
                }
            }
        }

        /// <summary>
        /// Get image.
        /// </summary>
        /// <param name="data">
        /// transformed data.
        /// </param>
        /// <returns>
        /// the image.
        /// </returns>
        private static Image GetImage(double[,] data)
        {
            // Number of spectra in sonogram
            int width = data.GetLength(0);
            int fftBins = data.GetLength(1);

            // set up min, max, range for normalising of dB values
            double min;
            double max;
            MinMax(data, out min, out max);

            var bmp = GetImage(data, fftBins, width);

            return bmp;
        }

        /// <summary>
        /// Get spectrogram image.
        /// </summary>
        /// <param name="audioData">Audio data.</param>
        /// <param name="height">Spectrogram height.</param>
        /// <param name="width">Spectrogram width.</param>
        /// <returns>Spectrogram image.</returns>
        private static Bitmap GetImage(double[,] audioData, int height, int width)
        {
            var managedImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            AForge.Imaging.UnmanagedImage image = AForge.Imaging.UnmanagedImage.FromManagedImage(managedImage);

            int pixelSize = Image.GetPixelFormatSize(image.PixelFormat) / 8;

            // image dimension
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            int stride = image.Stride;

            const int StartX = 0;
            int stopX = imageWidth - 1;

            // spectrogram is drawn from the bottom
            const int StartY = 0;
            int stopY = imageHeight - 1;

            // min, max, range
            double min;
            double max;
            MinMax(audioData, out min, out max);
            double range = max - min;

            int offset = stride - (stopX - StartX + 1) * pixelSize;

            int heightOffset = imageHeight;

            unsafe
            {
                // do the job
                byte* ptr = (byte*)image.ImageData.ToPointer() + (StartY * stride) + (StartX * pixelSize);

                // height
                for (int y = StartY; y <= stopY; y++)
                {
                    // width
                    for (int x = StartX; x <= stopX; x++, ptr += pixelSize)
                    {
                        // required to render spectrogram correct way up
                        int spectrogramY = heightOffset - 1;

                        // normalise and bound the value - use min bound, max and 255 image intensity range
                        // this is the amplitude
                        double value = (audioData[x, spectrogramY] - min) / (double)range;
                        double colour = 255.0 - Math.Floor(255.0 * value);

                        colour = Math.Min(colour, 255);
                        colour = Math.Max(colour, 0);

                        byte paintColour = Convert.ToByte(colour);

                        // set colour
                        ptr[AForge.Imaging.RGB.R] = paintColour;
                        ptr[AForge.Imaging.RGB.G] = paintColour;
                        ptr[AForge.Imaging.RGB.B] = paintColour;
                    }

                    ptr += offset;

                    heightOffset--;
                }
            }

            return image.ToManagedImage();
        }
    }
}