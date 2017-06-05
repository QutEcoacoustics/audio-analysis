// <copyright file="DSP_Frames.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.DSP
{
    using System;
    using System.Collections.Generic;
    using WavTools;
    using TowseyLibrary;

    // using MathNet.Numerics;using MathNet.Numerics.Transformations;

    /// <summary>
    /// digital signal processing methods
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

            public double FractionOfHighEnergyFrames { get; set; }

            public int FrameCount { get; set; }

            public double[] Envelope { get; set; }

            public double[] FrameEnergy { get; set; }

            public double[] FrameDecibels { get; set; }

            public double[,] AmplitudeSpectrogram { get; set; }

            public double WindowPower { get; set; }

            public double[] Average { get; set; }

            public int NyquistFreq { get; set; }

            public double FreqBinWidth { get; set; }

            public int NyquistBin { get; set; }

            public int MaxAmplitudeCount { get; set; }

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
        /// Returns the start and end index of all frames in a long audio signal
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
            int framecount = (dataLength - overlap) / frameStep; // this truncates residual samples
            if (framecount < 2)
            {
                throw new ArgumentException("Signal must produce at least two frames!");
            }

            // In the matrix "frames", col 0 = start; col 1 = end
            int offset = 0;
            int[,] frames = new int[framecount, 2];
            for (int i = 0; i < framecount; i++)
            {
                frames[i, 0] = offset; // start of frame
                frames[i, 1] = offset + frameSize - 1; // end of frame
                offset += frameStep;
            }

            return frames;
        }

        /// <summary>
        /// Returns the signal broken into frames
        /// This method is not called because the only reason to break a signal into frames is to do fft on the frames.
        /// </summary>
        public static double[,] Frames(double[] data, int[,] startEnds)
        {
            // window size = location of first frame end + 1
            int windowSize = startEnds[0, 1] + 1;
            int framecount = startEnds.GetLength(0);
            double[,] frames = new double[framecount, windowSize];

            for (int i = 0; i < framecount; i++)
            {
                for (int j = 0; j < windowSize; j++)
                {
                    frames[i, j] = data[startEnds[i, 0] + j];
                }
            }

            return frames;
        }

        public static EnvelopeAndFft ExtractEnvelopeAndFfts(AudioRecording recording, int frameSize, double overlap, string windowName = null)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            return ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep, windowName);
        }

        public static EnvelopeAndFft ExtractEnvelopeAndFfts(AudioRecording recording, int frameSize, int frameStep)
        {
            string windowName = FFT.Key_HammingWindow;
            return ExtractEnvelopeAndAmplSpectrogram(recording.WavReader.Samples, recording.SampleRate, recording.Epsilon, frameSize, frameStep, windowName);
        }

        public static EnvelopeAndFft ExtractEnvelopeAndAmplSpectrogram(double[] signal, int sampleRate, double epsilon, int frameSize, double overlap)
        {
            int frameStep = (int)(frameSize * (1 - overlap));
            string windowName = FFT.Key_HammingWindow;
            return ExtractEnvelopeAndAmplSpectrogram(signal, sampleRate, epsilon, frameSize, frameStep, windowName);
        }

        /// <summary>
        /// Returns the following 18 values encapsulated in class EnvelopeAndFft
        /// 1) the minimum and maximum signal values
        /// 2) the average of absolute amplitudes for each frame
        /// 3) the signal envelope as vector. i.e. the maximum of absolute amplitudes for each frame.
        /// 4) vector of frame energies
        /// 5) the high amplitdue and clipping counts
        /// 6) the signal amplitude spectrogram
        /// 7) the power of the FFT Window, i.e. sum of squared window values.
        /// 8) the nyquist
        /// 9) the width of freq bin in Hz
        /// 10) the byquist bin ID
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
            // Do not use this for enviromental audio
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

            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] frameEnergy = new double[frameCount];
            double[] frameDecibels = new double[frameCount];

            // get SNR data
            var snrdata = new SNR(signal, frameIDs);
            var fractionOfHighEnergyFrames = snrdata.FractionHighEnergyFrames(EndpointDetectionConfiguration.K2Threshold);

            // double[] decibelsNormalised = snrdata.NormaliseDecibelArray_ZeroOne(decibelReference);
            // double decibelReference = snrdata.MaxReference_dBWrtNoise;  // Used to NormaliseMatrixValues the dB values for feature extraction

            // set up the FFT parameters
            if (windowName == null)
            {
                windowName = FFT.Key_HammingWindow;
            }

            FFT.WindowFunc w = FFT.GetWindowFunction(windowName);
            var fft = new FFT(frameSize, w, true); // init class which calculates the MATLAB compatible .NET FFT
            double[,] spectrogram = new double[frameCount, fft.CoeffCount]; // init amplitude sonogram
            double minSignalValue = double.MaxValue;
            double maxSignalValue = double.MinValue;

            // for all frames
            for (int i = 0; i < frameCount; i++)
            {
                int start = i * frameStep;
                int end = start + frameSize;

                // get average and envelope
                double frameSum = signal[start];
                double total = Math.Abs(signal[start]);
                double maxValue = total;
                double energy = 0;
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

                    // Get absolute signal average in current frame
                    double absValue = Math.Abs(signal[x]);
                    total += absValue;
                    if (absValue > maxValue)
                    {
                        maxValue = absValue;
                    }

                    energy += signal[x] * signal[x];
                }

                double frameDc = frameSum / frameSize;
                average[i] = total / frameSize;
                envelope[i] = maxValue;
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

                // Previous alternative call to do the FFT and return amplitude spectrum
                //f1 = fft.Invoke(window);

                // Smooth spectrum to reduce variance
                // In the early days (pre-2010), we used to smooth the spectra to reduce sonogram variance. This is statistically correct thing to do.
                // Later, we stopped this for standard sonograms but kept it for calculating acoustic indices.
                // As of 28 March 2017, we are merging the two codes and keeping spectrum smoothing.
                // Will need to check the effect on spectrograms.
                int smoothingWindow = 3;
                f1 = DataTools.filterMovingAverage(f1, smoothingWindow);

                // transfer amplitude spectrum to spectrogram matrix
                for (int j = 0; j < fft.CoeffCount; j++)
                {
                    spectrogram[i, j] = f1[j];
                }
            } // end frames

            // Remove the DC column ie column zero from amplitude spectrogram.
            double[,] amplSpectrogram = MatrixTools.Submatrix(spectrogram, 0, 1, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

            // check the envelope for clipping. Accept a clip if two consecutive frames have max value = 1,0
            Clipping.GetClippingCount(signal, envelope, frameStep, epsilon, out int maxAmplitudeCount, out int clipCount);

            return new EnvelopeAndFft
            {
                // The following data is required when constructing sonograms
                Duration = TimeSpan.FromSeconds((double)signal.Length / sampleRate),
                Epsilon = epsilon,
                SampleRate = sampleRate,
                FrameCount = frameCount,
                FractionOfHighEnergyFrames = fractionOfHighEnergyFrames,
                WindowPower = fft.WindowPower,
                AmplitudeSpectrogram = amplSpectrogram,

                // The below 11 variables are only used when calculating spectral and summary indices
                // energy level information
                ClipCount = clipCount,
                MaxAmplitudeCount = maxAmplitudeCount,
                MinSignalValue = minSignalValue,
                MaxSignalValue = maxSignalValue,

                // envelope info
                Average = average,
                Envelope = envelope,
                FrameEnergy = frameEnergy,
                FrameDecibels = frameDecibels,

                // freq scale info
                NyquistFreq = sampleRate / 2,
                NyquistBin = amplSpectrogram.GetLength(1) - 1,
                FreqBinWidth = sampleRate / (double)amplSpectrogram.GetLength(1) / 2,
            };
        }

        /*
         * BELOW ARE THE TWO CLASSES ONCE USED TO MAKE SPECTROGRAMS for Sonograms and for Spectral INdices. Have merged to codes in method above.
         * In fact they only differed in smoothing of the spectra. See note in above method.
        public static double[,] MakeAmplitudeSpectrogram(double[] signal, int[,] frames, FFT.WindowFunc w, out double power)
        {
            // cycle through the frames
            for (int i = 0; i < frameCount; i++)
            {
                int start = i * frameStep;
                int end = start + frameSize;

                // get average and envelope
                double frameDc = signal[start];
                double total = Math.Abs(signal[start]);
                double maxValue = total;
                double energy = 0;
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

                    frameDc += signal[x];

                    // Get absolute signal average in current frame
                    double absValue = Math.Abs(signal[x]);
                    total += absValue;
                    if (absValue > maxValue)
                    {
                        maxValue = absValue;
                    }

                    energy += (signal[x] * signal[x]);
                }

                frameDc /= frameSize;
                average[i] = total / frameSize;
                envelope[i] = maxValue;
                frameEnergy[i] = energy / frameSize;

                // remove DC value from signal values
                double[] signalMinusAv = new double[frameSize];
                for (int j = 0; j < frameSize; j++)
                {
                    signalMinusAv[j] = signal[start + j] - frameDc;
                }

                // generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist
                var f1 = fft.InvokeDotNetFFT(signalMinusAv);

                ////f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                ////f1 = fft.Invoke(DataTools.GetRow(frames, i));          //returns fft amplitude spectrum

                // smooth spectrum to reduce variance
                f1 = DataTools.filterMovingAverage(f1, 3);

                // transfer amplitude spectrum to spectrogram matrix
                for (int j = 0; j < fft.CoeffCount; j++)
                {
                    spectrogram[i, j] = f1[j];
                }
            } // end frames
        }

        public static double[,] MakeAmplitudeSpectrogram(double[] signal, int[,] frames, FFT.WindowFunc w, out double power)
        {
            int frameCount = frames.GetLength(0);
            int frameSize = frames[0, 1] + 1;

            // init FFT class which calculates the MATLAB compatible .NET FFT
            var fft = new FFT(frameSize, w, true);
            power = fft.WindowPower; //store for later use when calculating dB
            double[,] amplitudeSonogram = new double[frameCount, fft.CoeffCount]; //init amplitude sonogram
            var window = new double[frameSize];

            // foreach frame or time step
            for (int i = 0; i < frameCount; i++)
            {
                //set up the window containing signal
                for (int j = 0; j < frameSize; j++)
                {
                    window[j] = signal[frames[i, 0] + j];
                }

                var f1 = fft.InvokeDotNetFFT(window);

                // Previous alternative call to do the FFT and return amplitude spectrum
                //f1 = fft.Invoke(window);

                // In the early days, we used to smooth the spectra to reduce sonogram variance. This is theoretically correct.
                // Stopped this for standard sonograms but kept it for calculating indices.
                // We are merging the two codes on 28March2017 cnad keeping the smoothing.
                // Will need to check the effect on spectrograms.
                int smoothingWindow = 3;
                f1 = DataTools.filterMovingAverage(f1, smoothingWindow);

                // transfer amplitude spectrum to a matrix
                for (int j = 0; j < fft.CoeffCount; j++)
                {
                    amplitudeSonogram[i, j] = f1[j];
                }
            } //end of all frames

            return amplitudeSonogram;
        }

        /// <summary>
        /// Does same as the method above but returns values for octave scale spectrograms.
        /// </summary>
        public static EnvelopeAndFft ExtractEnvelopeAndFftForOctaveScale(double[] signal, int sampleRate, double epsilon, int frameSize, int frameStep)
        {
            int[,] frameIDs = DSP_Frames.FrameStartEnds(signal.Length, frameSize, frameStep);
            if (frameIDs == null) return null;
            int frameCount = frameIDs.GetLength(0);

            double[] average = new double[frameCount];
            double[] envelope = new double[frameCount];
            double[] frameEnergy = new double[frameCount];

            // set up the FFT parameters
            FFT.WindowFunc w = FFT.GetWindowFunction(FFT.Key_HammingWindow);
            var fft = new FFT(frameSize, w, true); // init class which calculates the MATLAB compatible .NET FFT
            double[,] spectrogram = new double[frameCount, fft.CoeffCount]; // init amplitude sonogram
            double minSignalValue = double.MaxValue;
            double maxSignalValue = double.MinValue;

            // cycle through the frames
            for (int i = 0; i < frameCount; i++)
            {
                int start = i * frameStep;
                int end = start + frameSize;

                // get average and envelope
                double frameDC = signal[start];
                double total = Math.Abs(signal[start]);
                double maxValue = total;
                double energy = 0;
                for (int x = start + 1; x < end; x++)
                {
                    if (signal[x] > maxSignalValue) maxSignalValue = signal[x];
                    if (signal[x] < minSignalValue) minSignalValue = signal[x];
                    frameDC += signal[x];
                    double absValue = Math.Abs(signal[x]);
                    total += absValue; // go through current frame to get signal (absolute) average
                    if (absValue > maxValue) maxValue = absValue;
                    energy += (signal[x] * signal[x]);
                }
                frameDC /= frameSize;
                average[i] = total / frameSize;
                envelope[i] = maxValue;
                frameEnergy[i] = energy / frameSize;

                // remove DC value from signal values
                double[] signalMinusAv = new double[frameSize];
                for (int j = 0; j < frameSize; j++)
                    signalMinusAv[j] = signal[start + j] - frameDC;

                // generate the spectra of FFT AMPLITUDES - NOTE: f[0]=DC;  f[64]=Nyquist
                var f1 = fft.InvokeDotNetFFT(signalMinusAv); // the fft
                ////f1 = fft.InvokeDotNetFFT(DataTools.GetRow(frames, i)); //returns fft amplitude spectrum
                ////f1 = fft.Invoke(DataTools.GetRow(frames, i));          //returns fft amplitude spectrum

                f1 = DataTools.filterMovingAverage(f1, 3); //smooth spectrum to reduce variance
                for (int j = 0; j < fft.CoeffCount; j++)   //foreach freq bin
                    spectrogram[i, j] = f1[j];             //transfer amplitude

            } // end frames

            // check the envelope for clipping. Accept a clip if two consecutive frames have max value = 1,0
            int maxAmplitudeCount, clipCount;
            Clipping.GetClippingCount(signal, envelope, frameStep, epsilon, out maxAmplitudeCount, out clipCount);

            // Remove the DC column ie column zero from amplitude spectrogram.
            double[,] amplSpectrogram = MatrixTools.Submatrix(spectrogram, 0, 1, spectrogram.GetLength(0) - 1, spectrogram.GetLength(1) - 1);

            int nyquistFreq = sampleRate / 2;
            double binWidth = nyquistFreq / (double)amplSpectrogram.GetLength(1);
            int nyquistBin = amplSpectrogram.GetLength(1) - 1;

            return new EnvelopeAndFft
            {
                MinSignalValue = minSignalValue,
                MaxSignalValue = maxSignalValue,
                Average = average,
                Envelope = envelope,
                FrameEnergy = frameEnergy,
                MaxAmplitudeCount = maxAmplitudeCount,
                ClipCount = clipCount,
                AmplitudeSpectrogram = amplSpectrogram,
                WindowPower = fft.WindowPower,
                NyquistFreq = nyquistFreq,
                FreqBinWidth = binWidth,
                NyquistBin = nyquistBin
            };
        }
        */

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
                double av;
                double sd;
                NormalDist.AverageAndSD(periods, out av, out sd);
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
        /// This info is used for determing the begin and end points for vocalisations.
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
    }//end class DSP
}
