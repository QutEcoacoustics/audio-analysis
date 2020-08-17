// <copyright file="DSP_Frames.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using AudioAnalysisTools.WavTools;
    using TowseyLibrary;

    /// <summary>
    /// Digital signal processing methods.
    /// </summary>
    public static class DSP_Frames
    {
        public const double Pi = Math.PI;

        public class EnvelopeAndFft
        {
            public int SampleRate { get; set; }

            public double Epsilon { get; set; }

            public TimeSpan Duration { get; set; }

            public double MinSignalValue { get; set; }

            public double MaxSignalValue { get; set; }

            /// <summary>
            /// Gets or sets the fraction of high energy signal frames PRIOR to noise removal.
            /// This value is used only when doing noise removal. If the value exceeds SNR.FractionalBoundForMode,
            /// then Lamel's noise removal algorithm may not work well.
            /// </summary>
            public double FractionOfHighEnergyFrames { get; set; }

            public int FrameCount { get; set; }

            public double[] Envelope { get; set; }

            public double[] MinFrameValues { get; set; }

            public double[] MaxFrameValues { get; set; }

            public double[] FrameEnergy { get; set; }

            public double[] FrameDecibels { get; set; }

            public double[,] AmplitudeSpectrogram { get; set; }

            public double WindowPower { get; set; }

            public double[] Average { get; set; }

            public int NyquistFreq { get; set; }

            public double FreqBinWidth { get; set; }

            public int NyquistBin { get; set; }

            public int HighAmplitudeCount { get; set; }

            public int ClipCount { get; set; }
        }

        public static int FrameStep(int windowSize, double windowOverlap)
        {
            int step = (int)(windowSize * (1 - windowOverlap));
            return step;
        }

        public static int[,] FrameStartEnds(int dataLength, int frameSize, double windowOverlap)
        {
            int frameStep = (int)(frameSize * (1 - windowOverlap));
            return FrameStartEnds(dataLength, frameSize, frameStep);
        }

        /// <summary>
        /// Returns the start and end index of all frames in a long audio signal.
        /// </summary>
        public static int[,] FrameStartEnds(int dataLength, int frameSize, int frameStep)
        {
            if (frameStep < 1)
            {
                throw new ArgumentException("Frame Step must be at least 1");
            }

            if (frameStep > frameSize)
            {
                throw new ArgumentException("Frame Step must be <=" + frameSize);
            }

            int overlap = frameSize - frameStep;
            int frameCount = (dataLength - overlap) / frameStep; // this truncates residual samples
            if (frameCount < 2)
            {
                throw new ArgumentException("Signal must produce at least two frames!");
            }

            // In the matrix "frames", col 0 = start; col 1 = end
            int offset = 0;
            int[,] frames = new int[frameCount, 2];
            for (int i = 0; i < frameCount; i++)
            {
                frames[i, 0] = offset; // start of frame
                frames[i, 1] = offset + frameSize - 1; // end of frame
                offset += frameStep;
            }

            return frames;
        }

        /// <summary>
        /// Returns the signal broken into frames.
        /// This method is not called because the only reason to break a signal into frames is to do fft on the frames.
        /// </summary>
        public static double[,] Frames(double[] data, int[,] startEnds)
        {
            // window size = location of first frame end + 1
            int windowSize = startEnds[0, 1] + 1;
            int frameCount = startEnds.GetLength(0);
            double[,] frames = new double[frameCount, windowSize];

            for (int i = 0; i < frameCount; i++)
            {
                for (int j = 0; j < windowSize; j++)
                {
                    frames[i, j] = data[startEnds[i, 0] + j];
                }
            }

            return frames;
        }

        /// <summary>
        /// Calling this method will set default FFT window if windowName is null.
        /// Otherwise sets the FFT window specified in the config file.
        /// </summary>
        public static EnvelopeAndFft ExtractEnvelopeAndFfts(AudioRecording recording, int frameSize, double overlap, string windowName = null)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            return ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep, windowName);
        }

        /// <summary>
        /// Calling this method sets the default FFT window, currently HANNING - see FFT.cs line 22.
        /// </summary>
        public static EnvelopeAndFft ExtractEnvelopeAndFfts(AudioRecording recording, int frameSize, int frameStep)
        {
            return ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep, FFT.DefaultFftWindow);
        }

        /// <summary>
        /// Calling this method sets the default FFT window, currently HANNING - see FFT.cs line 22.
        /// </summary>
        public static EnvelopeAndFft ExtractEnvelopeAndAmplSpectrogram(double[] signal, int sampleRate, double epsilon, int frameSize, double overlap)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            return ExtractEnvelopeAndAmplSpectrogram(signal, sampleRate, epsilon, frameSize, frameStep, FFT.DefaultFftWindow);
        }

        /// <summary>
        /// Returns the following 18 values encapsulated in class EnvelopeAndFft
        /// 1) the minimum and maximum signal values
        /// 2) the average of absolute amplitudes for each frame
        /// 3) the minimum value in each frame
        /// 3) the maximum value in each frame.
        /// 3) the signal envelope as vector. i.e. the maximum of absolute amplitudes for each frame.
        /// 4) vector of frame energies
        /// 5) the high amplitude and clipping counts
        /// 6) the signal amplitude spectrogram
        /// 7) the power of the FFT Window, i.e. sum of squared window values.
        /// 8) the nyquist
        /// 9) the width of freq bin in Hz
        /// 10) the Nyquist bin ID
        /// AND OTHERS
        /// The returned info is used by Sonogram classes to draw sonograms and by Spectral Indices classes to calculate Spectral indices.
        /// Less than half the info is used to draw sonograms but it is difficult to disentangle calculation of all the info without
        /// reverting back to the old days when we used two classes and making sure they remain in synch.
        /// </summary>
        public static EnvelopeAndFft ExtractEnvelopeAndAmplSpectrogram(
            double[] signal,
            int sampleRate,
            double epsilon,
            int frameSize,
            int frameStep,
            string windowName = null)
        {
            // SIGNAL PRE-EMPHASIS helps with speech signals
            // Do not use this for environmental audio
            //if (config.DoPreemphasis)
            //{
            //    signal = DSP_Filters.PreEmphasis(signal, 0.96);
            //}

            int[,] frameIDs = FrameStartEnds(signal.Length, frameSize, frameStep);
            if (frameIDs == null)
            {
                throw new NullReferenceException("Thrown in EnvelopeAndFft.ExtractEnvelopeAndAmplSpectrogram(): int matrix, frameIDs, cannot be null.");
            }

            int frameCount = frameIDs.GetLength(0);

            // set up the FFT parameters
            if (windowName == null)
            {
                windowName = FFT.DefaultFftWindow;
            }

            FFT.WindowFunc w = FFT.GetWindowFunction(windowName);
            var fft = new FFT(frameSize, w); // init class which calculates the Matlab compatible .NET FFT
            double[,] spectrogram = new double[frameCount, fft.CoeffCount]; // init amplitude sonogram
            double minSignalValue = double.MaxValue;
            double maxSignalValue = double.MinValue;

            double[] average = new double[frameCount];
            double[] minValues = new double[frameCount];
            double[] maxValues = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] frameEnergy = new double[frameCount];
            double[] frameDecibels = new double[frameCount];

            // for all frames
            for (int i = 0; i < frameCount; i++)
            {
                int start = i * frameStep;
                int end = start + frameSize;

                // get average and envelope for current frame
                double frameMin = signal[start];
                double frameMax = signal[start];
                double frameSum = signal[start];
                double total = Math.Abs(signal[start]);
                double maxAbsValue = total;
                double energy = 0;

                // for all values in frame
                for (int x = start + 1; x < end; x++)
                {
                    if (signal[x] > maxSignalValue)
                    {
                        maxSignalValue = signal[x];
                    }

                    if (signal[x] < minSignalValue)
                    {
                        minSignalValue = signal[x];
                    }

                    frameSum += signal[x];

                    // Get frame min and max
                    if (signal[x] < frameMin)
                    {
                        frameMin = signal[x];
                    }

                    if (signal[x] > frameMax)
                    {
                        frameMax = signal[x];
                    }

                    energy += signal[x] * signal[x];

                    // Get absolute signal average in current frame
                    double absValue = Math.Abs(signal[x]);
                    total += absValue;

                    // Get the maximum absolute signal value in current frame
                    if (absValue > maxAbsValue)
                    {
                        maxAbsValue = absValue;
                    }
                } // end frame

                double frameDc = frameSum / frameSize;
                minValues[i] = frameMin;
                maxValues[i] = frameMax;
                average[i] = total / frameSize;
                envelope[i] = maxAbsValue;
                frameEnergy[i] = energy / frameSize;
                frameDecibels[i] = 10 * Math.Log10(frameEnergy[i]);

                // remove DC value from signal values
                double[] signalMinusAv = new double[frameSize];
                for (int j = 0; j < frameSize; j++)
                {
                    signalMinusAv[j] = signal[start + j] - frameDc;
                }

                // generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist
                var f1 = fft.InvokeDotNetFFT(signalMinusAv);

                // Smooth spectrum to reduce variance
                int smoothingWindow = 3;
                f1 = DataTools.filterMovingAverage(f1, smoothingWindow);

                // transfer amplitude spectrum to spectrogram matrix
                for (int j = 0; j < fft.CoeffCount; j++)
                {
                    spectrogram[i, j] = f1[j];
                }
            } // end frames

            // Remove the DC column (ie column zero) from amplitude spectrogram.
            double[,] amplitudeSpectrogram = MatrixTools.Submatrix(spectrogram, 0, 1, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

            // check the envelope for clipping. Accept a clip if two consecutive frames have max value = 1,0
            Clipping.GetClippingCount(signal, envelope, frameStep, epsilon, out int highAmplitudeCount, out int clipCount);

            // get SNR data
            var snrData = new SNR(signal, frameIDs);

            return new EnvelopeAndFft
            {
                // The following data is required when constructing sonograms
                Duration = TimeSpan.FromSeconds((double)signal.Length / sampleRate),
                Epsilon = epsilon,
                SampleRate = sampleRate,
                FrameCount = frameCount,
                FractionOfHighEnergyFrames = snrData.FractionOfHighEnergyFrames,
                WindowPower = fft.WindowPower,
                AmplitudeSpectrogram = amplitudeSpectrogram,

                // The below 11 variables are only used when calculating spectral and summary indices
                // energy level information
                ClipCount = clipCount,
                HighAmplitudeCount = highAmplitudeCount,
                MinSignalValue = minSignalValue,
                MaxSignalValue = maxSignalValue,

                // envelope info
                Average = average,
                MinFrameValues = minValues,
                MaxFrameValues = maxValues,
                Envelope = envelope,
                FrameEnergy = frameEnergy,
                FrameDecibels = frameDecibels,

                // freq scale info
                NyquistFreq = sampleRate / 2,
                NyquistBin = amplitudeSpectrogram.GetLength(1) - 1,
                FreqBinWidth = sampleRate / (double)amplitudeSpectrogram.GetLength(1) / 2,
            };
        }

        public static Tuple<double[], double[], double[], double[], double[]> ExtractEnvelopeAndZeroCrossings(double[] signal, int sr, int windowSize, double overlap)
        {
            int length = signal.Length;
            int frameOffset = (int)(windowSize * (1 - overlap));
            int frameCount = (length - windowSize + frameOffset) / frameOffset;
            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];

            // count of zero crossings
            double[] zeroCrossings = new double[frameCount];

            // sample count between zero crossings
            double[] zcPeriod = new double[frameCount];
            double[] sdPeriod = new double[frameCount];      // standard deviation of sample count between zc.
            for (int i = 0; i < frameCount; i++)
            {
                List<int> periodList = new List<int>();
                int start = i * frameOffset;
                int end = start + windowSize;

                //get average and envelope
                double maxValue = -double.MaxValue;
                double total = signal[start];
                for (int x = start + 1; x < end; x++)
                {
                    total += signal[x]; // go through current frame to get signal average/DC
                    double absValue = Math.Abs(signal[x]);
                    if (absValue > maxValue)
                    {
                        maxValue = absValue;
                    }
                }

                average[i] = total / windowSize;
                envelope[i] = maxValue;

                //remove the average from signal
                double[] signalMinusAv = new double[windowSize];
                for (int j = 0; j < windowSize; j++)
                {
                    signalMinusAv[j] = signal[start + j] - average[i];
                }

                //get zero crossings and periods
                int zeroCrossingCount = 0;
                int prevLocation = 0;
                double prevValue = signalMinusAv[0];

                // go through current frame
                for (int j = 1; j < windowSize; j++)
                {
                    //double absValue = Math.Abs(signalMinusAv[j]);

                    // if zero crossing
                    if (signalMinusAv[j] * prevValue < 0.0)
                    {
                        if (zeroCrossingCount > 0)
                        {
                            periodList.Add(j - prevLocation); // do not want to accumulate counts prior to first ZC.
                        }

                        zeroCrossingCount++; // count zero crossings
                        prevLocation = j;
                        prevValue = signalMinusAv[j];
                    }
                } // end current frame

                zeroCrossings[i] = zeroCrossingCount;
                int[] periods = periodList.ToArray();
                NormalDist.AverageAndSD(periods, out var av, out var sd);
                zcPeriod[i] = av;
                sdPeriod[i] = sd;
            }

            return Tuple.Create(average, envelope, zeroCrossings, zcPeriod, sdPeriod);
        }

        public static int[] ConvertZeroCrossings2Hz(double[] zeroCrossings, int frameWidth, int sampleRate)
        {
            int length = zeroCrossings.Length;
            var freq = new int[length];
            for (int i = 0; i < length; i++)
            {
                freq[i] = (int)(zeroCrossings[i] * sampleRate / 2 / frameWidth);
            }

            return freq;
        }

        public static double[] ConvertSamples2Milliseconds(double[] sampleCounts, int sampleRate)
        {
            var tValues = new double[sampleCounts.Length];
            for (int i = 0; i < sampleCounts.Length; i++)
            {
                tValues[i] = sampleCounts[i] * 1000 / sampleRate;
            }

            return tValues;
        }

        /// <summary>
        /// returns the min and max values in each frame. Signal values range from -1 to +1.
        /// </summary>
        public static void SignalEnvelope(double[,] frames, out double[] minAmp, out double[] maxAmp)
        {
            int frameCount = frames.GetLength(0);
            int n = frames.GetLength(1);
            minAmp = new double[frameCount];
            maxAmp = new double[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                double min = double.MaxValue;
                double max = -double.MaxValue;

                // foreach sample in frame
                for (int j = 0; j < n; j++)
                {
                    if (min > frames[i, j])
                    {
                        min = frames[i, j];
                    }
                    else
                    if (max < frames[i, j])
                    {
                        max = frames[i, j];
                    }
                }

                minAmp[i] = min;
                maxAmp[i] = max;
            }
        }

        /// <summary>
        /// counts the zero crossings in each frame
        /// This info is used for determining the begin and end points for vocalizations.
        /// </summary>
        public static int[] ZeroCrossings(double[,] frames)
        {
            int frameCount = frames.GetLength(0);
            int n = frames.GetLength(1);
            int[] zc = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                int count = 0;
                for (int j = 1; j < n; j++)
                {
                    count += Math.Abs(Math.Sign(frames[i, j]) - Math.Sign(frames[i, j - 1]));
                }

                zc[i] = count / 2;
            }

            return zc;
        }

        public static void DisplaySignal(double[] sig)
        {
            double[] newSig = DataTools.normalise(sig);

            foreach (double value in newSig)
            {
                int count = (int)(value * 50);
                for (int i = 0; i < count; i++)
                {
                    LoggedConsole.Write("=");
                }

                LoggedConsole.WriteLine("=");
            }
        }

        public static void DisplaySignal(double[] sig, bool showIndex)
        {
            double[] newSig = DataTools.normalise(sig);

            for (int n = 0; n < sig.Length; n++)
            {
                if (showIndex)
                {
                    LoggedConsole.Write(n.ToString("D3") + "|");
                }

                int count = (int)(newSig[n] * 50);
                for (int i = 0; i < count; i++)
                {
                    LoggedConsole.Write("=");
                }

                LoggedConsole.WriteLine("=");
            }
        }
    }
}